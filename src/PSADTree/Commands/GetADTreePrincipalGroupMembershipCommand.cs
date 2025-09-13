using System;
using System.Collections.Generic;
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
        Dbg.Assert(Context is not null);
        Principal? principal;

        try
        {
            principal = Principal.FindByIdentity(Context, Identity);
        }
        catch (Exception _) when (_ is PipelineStoppedException or FlowControlException)
        {
            throw;
        }
        catch (MultipleMatchesException exception)
        {
            WriteError(exception.ToAmbiguousIdentity(Identity));
            return;
        }
        catch (Exception exception)
        {
            WriteError(exception.ToUnspecified(Identity));
            return;
        }

        if (principal is null)
        {
            WriteError(Identity.ToIdentityNotFound());
            return;
        }

        string source = principal.DistinguishedName;
        switch (principal)
        {
            case UserPrincipal user:
                Index.Add(new TreeUser(source, user));
                break;

            case ComputerPrincipal computer:
                Index.Add(new TreeComputer(source, computer));
                break;

            case GroupPrincipal group:
                TreeGroup treeGroup = new(source, group);
                Index.Add(treeGroup);
                Cache.TryAdd(treeGroup);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(principal));
        }

        try
        {
            IEnumerable<Principal> groups = principal.ToSafeSortedEnumerable(
                selector: principal => principal.GetGroups(Context),
                cmdlet: this,
                comparer: Comparer);

            foreach (Principal parent in groups)
            {
                if (ShouldExclude(parent, ExclusionPatterns))
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
            WriteError(exception.ToEnumerationFailure(null));
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
        HashSet<string> visited = [];

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

                // else, if we want to show all nodes and this node was not yet visited
                if (ShowAll && !visited.Add(treeGroup.DistinguishedName))
                {
                    // reconstruct the output without querying AD again
                    EnumerateMembership(treeGroup, depth);
                    continue;
                }

                // else, just skip this reference and go next
                treeGroup.SetProcessed();
                continue;
            }

            // else, get membership from AD Query
            EnumerateMembership(current, treeGroup, source, depth);
            current?.Dispose();
        }

        return Index.GetTree();
    }

    private void EnumerateMembership(
        Principal? principal,
        TreeGroup parent,
        string source,
        int depth)
    {
        if (principal is null)
        {
            return;
        }

        IEnumerable<Principal> groups = principal.ToSafeSortedEnumerable(
            selector: principal => principal.GetGroups(Context),
            cmdlet: this,
            comparer: Comparer);

        foreach (Principal group in groups)
        {
            if (ShouldExclude(group, ExclusionPatterns))
            {
                continue;
            }

            TreeGroup treeGroup = ProcessGroup((GroupPrincipal)group);
            parent.AddChild(treeGroup);
        }

        TreeGroup ProcessGroup(GroupPrincipal group)
        {
            if (Cache.TryGet(group.DistinguishedName, out TreeGroup? treeGroup))
            {
                TreeGroup cloned = (TreeGroup)treeGroup.Clone(parent, depth);
                cloned.LinkCachedChildren(Cache);
                Push(group, cloned);
                group.Dispose();
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

        foreach (TreeObjectBase child in parent.Children)
        {
            TreeGroup group = (TreeGroup)child;
            Push(null, (TreeGroup)group.Clone(parent, depth));
        }
    }
}
