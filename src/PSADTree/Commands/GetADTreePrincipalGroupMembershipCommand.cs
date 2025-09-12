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
        Dbg.Assert(_context is not null);
        TruncatedOutput = false;
        Principal? principal;
        Index.Clear();

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
                Cache.Add(treeGroup);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(principal));
        }

        try
        {
            IEnumerable<Principal> groups = principal.ToSafeSortedEnumerable(
                selector: principal => principal.GetGroups(_context),
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
        while (Stack.Count > 0)
        {
            (GroupPrincipal? current, TreeGroup treeGroup) = Stack.Pop();

            try
            {
                depth = treeGroup.Depth + 1;

                // if this node has been already processed
                if (!Cache.TryAdd(treeGroup))
                {
                    current?.Dispose();
                    treeGroup.Hook(Cache);
                    Index.Add(treeGroup);

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

                if (current is not null)
                {
                    IEnumerable<Principal> groups = current.ToSafeSortedEnumerable(
                        selector: principal => principal.GetGroups(_context),
                        cmdlet: this,
                        comparer: Comparer);

                    EnumerateMembership(treeGroup, groups, source, depth);
                }

                Index.Add(treeGroup);
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

        return Index.GetTree();
    }

    private void EnumerateMembership(
        TreeGroup parent,
        IEnumerable<Principal> groups,
        string source,
        int depth)
    {
        foreach (Principal group in groups)
        {
            if (ShouldExclude(group, ExclusionPatterns))
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
            if (Cache.TryGet(group.DistinguishedName, out TreeGroup? treeGroup))
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

        foreach (TreeObjectBase child in parent.Children)
        {
            TreeGroup group = (TreeGroup)child;
            Push(null, (TreeGroup)group.Clone(parent, depth));
        }
    }
}
