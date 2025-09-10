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
        Dbg.Assert(_context is not null);
        _truncatedOutput = false;

        try
        {
            using GroupPrincipal? group = GroupPrincipal.FindByIdentity(_context, Identity);
            if (group is null)
            {
                WriteError(Identity.ToIdentityNotFound());
                return;
            }

            TreeObjectBase[] result = Traverse(
                groupPrincipal: group,
                source: group.DistinguishedName);

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

    private TreeObjectBase[] Traverse(
        GroupPrincipal groupPrincipal,
        string source)
    {
        int depth;
        Clear();
        Push(groupPrincipal, new TreeGroup(source, groupPrincipal));

        while (_stack.Count > 0)
        {
            (GroupPrincipal? current, TreeGroup treeGroup) = _stack.Pop();

            try
            {
                depth = treeGroup.Depth + 1;

                // if this node has been already processed
                if (!_cache.TryAdd(treeGroup))
                {
                    current?.Dispose();
                    treeGroup.Hook(_cache);
                    _index.Add(treeGroup);

                    // if it's a circular reference, go next
                    if (TreeCache.IsCircular(treeGroup))
                    {
                        treeGroup.SetCircularNested();
                        continue;
                    }

                    // else, if we want to show all nodes
                    if (ShowAll.IsPresent)
                    {
                        // reconstruct the output without querying AD again
                        EnumerateMembers(treeGroup, depth);
                        continue;
                    }

                    // else, just skip this reference and go next
                    treeGroup.SetProcessed();
                    continue;
                }

                if (current is not null)
                {
                    EnumerateMembers(treeGroup, current, source, depth);
                }

                _index.Add(treeGroup);
                _index.TryAddPrincipals();
                current?.Dispose();
            }
            catch (Exception _) when (_ is PipelineStoppedException or FlowControlException)
            {
                throw;
            }
            catch (Exception exception)
            {
                WriteError(exception.ToEnumerationFailure(current));
            }
        }

        return _index.GetTree();
    }

    private void EnumerateMembers(
        TreeGroup parent,
        GroupPrincipal group,
        string source,
        int depth)
    {
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

                if (ShouldExclude(member, _exclusionPatterns))
                {
                    continue;
                }

                TreeObjectBase treeObject = ProcessPrincipal(
                    principal: member,
                    parent: parent,
                    source: source,
                    depth: depth);

                if (ShowAll.IsPresent)
                {
                    parent.AddChild(treeObject);
                }
            }
            finally
            {
                disposable?.Dispose();
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
            GroupPrincipal group => HandleGroup(parent, group, source, depth),
            _ => throw new ArgumentOutOfRangeException(nameof(principal)),
        };

        TreeObjectBase AddTreeObject(TreeObjectBase obj)
        {
            if (depth <= Depth)
            {
                _index.AddPrincipal(obj);
            }

            return obj;
        }

        TreeObjectBase HandleGroup(
            TreeGroup parent,
            GroupPrincipal group,
            string source,
            int depth)
        {
            if (_cache.TryGet(group.DistinguishedName, out TreeGroup? treeGroup))
            {
                Push(group, (TreeGroup)treeGroup.Clone(parent, depth));
                return treeGroup;
            }

            treeGroup = new TreeGroup(source, parent, group, depth);
            Push(group, treeGroup);
            return treeGroup;
        }
    }

    private void EnumerateMembers(TreeGroup parent, int depth)
    {
        foreach (TreeObjectBase member in parent.Childs)
        {
            if (member is TreeGroup treeGroup)
            {
                Push(null, (TreeGroup)treeGroup.Clone(parent, depth));
                continue;
            }

            if (depth <= Depth)
            {
                _index.Add(member.Clone(parent, depth));
            }
        }
    }
}
