using System;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Management.Automation;

namespace PSADTree;

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
        Principal? principal;
        Clear();

        try
        {
            principal = Principal.FindByIdentity(_context, Identity);
        }
        catch (Exception e) when (e is PipelineStoppedException or FlowControlException)
        {
            throw;
        }
        catch (MultipleMatchesException e)
        {
            WriteError(ErrorHelper.AmbiguousIdentity(Identity, e));
            return;
        }
        catch (Exception e)
        {
            WriteError(ErrorHelper.Unspecified(Identity, e));
            return;
        }

        if (principal is null)
        {
            WriteError(ErrorHelper.IdentityNotFound(Identity));
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
            using PrincipalSearchResult<Principal> search = principal.GetGroups();
            foreach (GroupPrincipal parent in search.Cast<GroupPrincipal>())
            {
                TreeGroup treeGroup = new(source, null, parent, 1);
                Push(parent, treeGroup);
            }
        }
        catch (Exception e) when (e is PipelineStoppedException or FlowControlException)
        {
            throw;
        }
        catch (Exception e)
        {
            WriteError(ErrorHelper.EnumerationFailure(null, e));
        }
        finally
        {
            principal?.Dispose();
        }

        WriteObject(
            sendToPipeline: Traverse(source),
            enumerateCollection: true);
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

                using PrincipalSearchResult<Principal>? search = current?.GetGroups();

                if (search is not null)
                {
                    EnumerateMembership(treeGroup, search, source, depth);
                }

                _index.Add(treeGroup);
                current?.Dispose();
            }
            catch (Exception e) when (e is PipelineStoppedException or FlowControlException)
            {
                throw;
            }
            catch (Exception e)
            {
                WriteError(ErrorHelper.EnumerationFailure(current, e));
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
        foreach (GroupPrincipal group in searchResult.Cast<GroupPrincipal>())
        {
            TreeGroup treeGroup = ProcessGroup(group);
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
        if (!Recursive.IsPresent && depth > Depth)
        {
            return;
        }

        foreach (TreeGroup group in parent.Childs.Cast<TreeGroup>())
        {
            Push(null, (TreeGroup)group.Clone(parent, depth));
        }
    }
}
