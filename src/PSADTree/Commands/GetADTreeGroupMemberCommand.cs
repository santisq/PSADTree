using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Management.Automation;

namespace PSADTree.Commands;

[Cmdlet(
    VerbsCommon.Get, "ADTreeGroupMember",
    DefaultParameterSetName = DepthParameterSet)]
[Alias("treegroupmember")]
[OutputType(
    typeof(TreeGroup),
    typeof(TreeUser),
    typeof(TreeComputer))]
public sealed class GetADTreeGroupMemberCommand : PSADTreeCmdletBase
{
    [Parameter]
    public SwitchParameter Group { get; set; }

    protected override void ProcessRecord()
    {
        Dbg.Assert(Identity is not null);
        Dbg.Assert(Context is not null);
        base.ProcessRecord();

        try
        {
            using GroupPrincipal? group = GroupPrincipal.FindByIdentity(Context, Identity);
            if (group is null)
            {
                WriteError(Identity.ToIdentityNotFound());
                return;
            }

            TreeObjectBase[] result = Traverse(group);
            DisplayWarningIfTruncatedOutput();
            WriteObject(sendToPipeline: result, enumerateCollection: true);
        }
        catch (Exception _) when (_ is PipelineStoppedException or FlowControlException)
        {
            throw;
        }
        catch (MultipleMatchesException exception)
        {
            WriteError(exception.ToAmbiguousIdentity(Identity));
        }
        catch (Exception exception)
        {
            WriteError(exception.ToUnspecified(Identity));
        }
    }

    private TreeGroup GetFirstTreeGroup(GroupPrincipal group)
    {
        TreeGroup treeGroup = new(group.DistinguishedName, group);
        if (Cache.TryGet(group.DistinguishedName, out TreeGroup? _))
        {
            treeGroup.LinkCachedChildren(Cache);
        }

        return treeGroup;
    }

    private TreeObjectBase[] Traverse(GroupPrincipal groupPrincipal)
    {
        int depth;
        string source = groupPrincipal.DistinguishedName;
        Push(groupPrincipal, GetFirstTreeGroup(groupPrincipal));

        while (Stack.Count > 0)
        {
            (GroupPrincipal? current, TreeGroup treeGroup) = Stack.Pop();
            depth = treeGroup.Depth + 1;
            Index.Add(treeGroup);

            // if this node has been already processed
            if (!Cache.TryAdd(treeGroup))
            {
                // if it's a circular reference, go next
                if (treeGroup.SetIfCircularNested())
                {
                    continue;
                }

                // else, if we want to show all nodes OR this node was not yet visited
                if (ShowAll || IsNotVisited(treeGroup))
                {
                    // reconstruct the output without querying AD again
                    EnumerateMembers(treeGroup, depth);
                    continue;
                }

                // else, just skip this reference and go next
                treeGroup.SetProcessed();
                continue;
            }

            // else, group isn't cached query AD
            EnumerateMembers(treeGroup, current, source, depth);
            Index.CommitStaged();
            current?.Dispose();
        }

        return Index.GetTree();
    }

    private void EnumerateMembers(
        TreeGroup parent,
        GroupPrincipal? group,
        string source,
        int depth)
    {
        if (group is null)
        {
            return;
        }

        IEnumerable<Principal> members = group.ToSafeSortedEnumerable(
            selector: group => group.GetMembers(),
            cmdlet: this,
            comparer: Comparer);

        foreach (Principal member in members)
        {
            IDisposable? disposable = null;
            try
            {
                if (member is { DistinguishedName: null })
                {
                    disposable = member;
                    continue;
                }

                if (member.StructuralObjectClass != "group")
                {
                    disposable = member;
                    if (Group.IsPresent)
                    {
                        continue;
                    }
                }

                if (ShouldExclude(member))
                {
                    continue;
                }

                TreeObjectBase treeObject = ProcessPrincipal(
                    principal: member,
                    parent: parent,
                    source: source,
                    depth: depth);

                parent.AddChild(treeObject);
            }
            finally
            {
                disposable?.Dispose();
            }
        }
    }

    private void EnumerateMembers(TreeGroup parent, int depth)
    {
        foreach (TreeObjectBase member in parent.Children)
        {
            if (member is TreeGroup treeGroup)
            {
                Push(null, (TreeGroup)treeGroup.Clone(parent, depth));
                continue;
            }

            if (depth <= Depth)
            {
                Index.Add(member.Clone(parent, depth));
            }
        }
    }

    private TreeObjectBase ProcessPrincipal(
        Principal principal,
        TreeGroup parent,
        string source,
        int depth)
    {
        return principal switch
        {
            UserPrincipal user => AddTreeObject(new TreeUser(source, parent, user, depth)),
            ComputerPrincipal computer => AddTreeObject(new TreeComputer(source, parent, computer, depth)),
            GroupPrincipal group => ProcessGroup(parent, group, source, depth),
            _ => throw new ArgumentOutOfRangeException(nameof(principal)),
        };

        TreeObjectBase AddTreeObject(TreeObjectBase obj)
        {
            if (depth <= Depth)
            {
                Index.Stage(obj);
            }

            return obj;
        }

        TreeObjectBase ProcessGroup(
            TreeGroup parent,
            GroupPrincipal group,
            string source,
            int depth)
        {
            if (Cache.TryGet(group.DistinguishedName, out TreeGroup? treeGroup))
            {
                TreeGroup cloned = (TreeGroup)treeGroup.Clone(parent, depth);
                cloned.LinkCachedChildren(Cache);
                Push(null, cloned);
                group.Dispose();
                return treeGroup;
            }

            treeGroup = new TreeGroup(source, parent, group, depth);
            Push(group, treeGroup);
            return treeGroup;
        }
    }
}
