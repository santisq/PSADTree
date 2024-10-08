using System;
using System.DirectoryServices.AccountManagement;
using System.Management.Automation;

namespace PSADTree.Commands;

[Cmdlet(
    VerbsCommon.Get, "ADTreePrincipalGroupMembership",
    DefaultParameterSetName = DepthParameterSet)]
[Alias("treeprincipalmembership")]
[OutputType(
    typeof(TreeGroup),
    typeof(TreeUser),
    typeof(TreeComputer))]
public sealed class GetADTreePrincipalGroupMembershipCommand : PSADTreeCmdletBase
{
    protected override void ProcessRecord()
    {
        Dbg.Assert(Identity is not null);
        Dbg.Assert(_context is not null);
        _truncatedOutput = false;
        Principal? principal;
        Clear();

        try
        {
            principal = Principal.FindByIdentity(_context, Identity);
        }
        catch (Exception _) when (_ is PipelineStoppedException or FlowControlException)
        {
            throw;
        }
        catch (MultipleMatchesException exception)
        {
            WriteError(exception.AmbiguousIdentity(Identity));
            return;
        }
        catch (Exception exception)
        {
            WriteError(exception.Unspecified(Identity));
            return;
        }

        if (principal is null)
        {
            WriteError(Exceptions.IdentityNotFound(Identity));
            return;
        }

        string source = principal.DistinguishedName;
        switch (principal)
        {
            case UserPrincipal user:
                _index.Add(new TreeUser(source, user));
                break;

            case ComputerPrincipal computer:
                _index.Add(new TreeComputer(source, computer));
                break;

            case GroupPrincipal group:
                TreeGroup treeGroup = new(source, group);
                _index.Add(treeGroup);
                _cache.Add(treeGroup);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(principal));
        }

        try
        {
            using PrincipalSearchResult<Principal> search = principal.GetGroups(_context);
            foreach (Principal parent in search.GetSortedEnumerable(_comparer))
            {
                if (ShouldExclude(parent, _exclusionPatterns))
                {
                    continue;
                }

                GroupPrincipal groupPrincipal = (GroupPrincipal)parent;
                TreeGroup treeGroup = new(source, null, groupPrincipal, 1);
                Push(groupPrincipal, treeGroup);
            }
        }
        catch (Exception _) when (_ is PipelineStoppedException or FlowControlException)
        {
            throw;
        }
        catch (Exception exception)
        {
            WriteError(exception.EnumerationFailure(null));
        }
        finally
        {
            principal?.Dispose();
        }

        TreeObjectBase[] result = Traverse(source);
        DisplayWarningIfTruncatedOutput();
        WriteObject(sendToPipeline: result, enumerateCollection: true);
    }

    private TreeObjectBase[] Traverse(string source)
    {
        int depth;
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
                        EnumerateMembership(treeGroup, depth);
                        continue;
                    }

                    // else, just skip this reference and go next
                    treeGroup.SetProcessed();
                    continue;
                }

                using PrincipalSearchResult<Principal>? search = current?.GetGroups(_context);

                if (search is not null)
                {
                    EnumerateMembership(treeGroup, search, source, depth);
                }

                _index.Add(treeGroup);
                current?.Dispose();
            }
            catch (Exception _) when (_ is PipelineStoppedException or FlowControlException)
            {
                throw;
            }
            catch (Exception exception)
            {
                WriteError(exception.EnumerationFailure(current));
            }
        }

        return _index.GetTree();
    }

    private void EnumerateMembership(
        TreeGroup parent,
        PrincipalSearchResult<Principal> searchResult,
        string source,
        int depth)
    {
        foreach (Principal group in searchResult.GetSortedEnumerable(_comparer))
        {
            if (ShouldExclude(group, _exclusionPatterns))
            {
                continue;
            }

            TreeGroup treeGroup = ProcessGroup((GroupPrincipal)group);
            if (ShowAll.IsPresent)
            {
                parent.AddChild(treeGroup);
            }
        }

        TreeGroup ProcessGroup(GroupPrincipal group)
        {
            if (_cache.TryGet(group.DistinguishedName, out TreeGroup? treeGroup))
            {
                Push(group, (TreeGroup)treeGroup.Clone(parent, depth));
                return treeGroup;
            }

            treeGroup = new(source, parent, group, depth);
            Push(group, treeGroup);
            return treeGroup;
        }
    }

    private void EnumerateMembership(TreeGroup parent, int depth)
    {
        if (depth > Depth)
        {
            return;
        }

        foreach (TreeObjectBase child in parent.Childs)
        {
            TreeGroup group = (TreeGroup)child;
            Push(null, (TreeGroup)group.Clone(parent, depth));
        }
    }
}
