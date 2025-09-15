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
        // if the first group is cached
        if (Cache.TryGet(group.DistinguishedName, out TreeGroup? _))
        {
            // link the children and build from cached path,
            // no need to query AD at all!
            treeGroup.LinkCachedChildren(Cache);
        }

        return treeGroup;
    }

    private TreeObjectBase[] Traverse(GroupPrincipal groupPrincipal)
    {
        int depth;
        string source = groupPrincipal.DistinguishedName;
        PushToStack(groupPrincipal, GetFirstTreeGroup(groupPrincipal));

        while (Stack.Count > 0)
        {
            (GroupPrincipal? current, TreeGroup treeGroup) = Stack.Pop();
            depth = treeGroup.Depth + 1;
            Index.Add(treeGroup);

            // if this group is already processed
            if (!Cache.TryAdd(treeGroup))
            {
                HandleCachedGroup(treeGroup, depth);
                current?.Dispose();
                continue;
            }

            // else, group isn't cached so query AD
            BuildMembersFromAD(treeGroup, current, source, depth);
            Index.CommitStaged();
            current?.Dispose();
        }

        return Index.GetTree();
    }

    private void HandleCachedGroup(TreeGroup treeGroup, int depth)
    {
        // if it's a circular reference, nothing to do here
        if (treeGroup.SetIfCircularNested())
        {
            return;
        }

        // else, if we want to show all nodes OR this node was not yet visited
        if (ShowAll || IsNotVisited(treeGroup))
        {
            // reconstruct the output without querying AD again
            BuildMembersFromCache(treeGroup, depth);
            return;
        }

        // else, just skip this reference and go next
        treeGroup.SetProcessed();
    }

    private void BuildMembersFromAD(
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
                if (member is { DistinguishedName: null } ||
                    member.StructuralObjectClass != "group" && Group.IsPresent ||
                    ShouldExclude(member))
                {
                    disposable = member;
                    continue;
                }

                ProcessPrincipal(
                    principal: member,
                    parent: parent,
                    source: source,
                    depth: depth);
            }
            finally
            {
                disposable?.Dispose();
            }
        }
    }

    private void BuildMembersFromCache(TreeGroup parent, int depth)
    {
        foreach (TreeObjectBase member in parent.Children)
        {
            if (member is TreeGroup treeGroup)
            {
                PushToStack(null, (TreeGroup)treeGroup.Clone(parent, depth));
                continue;
            }

            if (depth <= Depth)
            {
                Index.Add(member.Clone(parent, depth));
            }
        }
    }

    private void ProcessPrincipal(
        Principal principal,
        TreeGroup parent,
        string source,
        int depth)
    {
        TreeObjectBase treeObject = principal switch
        {
            UserPrincipal user => AddTreeObject(new TreeUser(source, parent, user, depth)),
            ComputerPrincipal computer => AddTreeObject(new TreeComputer(source, parent, computer, depth)),
            GroupPrincipal group => ProcessGroup(parent, group, source, depth),
            _ => throw new ArgumentOutOfRangeException(nameof(principal)),
        };

        parent.AddChild(treeObject);

        TreeObjectBase AddTreeObject(TreeObjectBase obj)
        {
            if (depth <= Depth)
            {
                Index.Stage(obj);
            }

            return obj;
        }
    }
}
