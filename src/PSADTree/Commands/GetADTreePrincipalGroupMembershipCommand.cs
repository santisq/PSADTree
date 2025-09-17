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
    protected override Principal GetFirstPrincipal() => Principal.FindByIdentity(Context, Identity);

    protected override void HandleFirstPrincipal(Principal principal)
    {
        TreeGroup LinkIfCached(TreeGroup treeGroup)
        {
            // if the first group is cached
            if (Cache.TryGet(treeGroup.DistinguishedName, out TreeGroup? _))
            {
                // link the children and build from cached path,
                // no need to query AD at all!
                treeGroup.LinkCachedChildren(Cache);
            }

            return treeGroup;
        }

        string source = principal.DistinguishedName;
        TreeObjectBase treeObject = principal switch
        {
            UserPrincipal user => new TreeUser(source, user),
            ComputerPrincipal computer => new TreeComputer(source, computer),
            GroupPrincipal group => LinkIfCached(new TreeGroup(source, group)),
            _ => throw new ArgumentOutOfRangeException(nameof(principal))
        };

        Builder.Add(treeObject);

        IEnumerable<Principal> principalMembership = principal.ToSafeSortedEnumerable(
            selector: principal => principal.GetGroups(Context),
            cmdlet: this,
            comparer: Comparer);

        foreach (Principal parent in principalMembership)
        {
            if (ShouldExclude(parent))
            {
                continue;
            }

            GroupPrincipal groupPrincipal = (GroupPrincipal)parent;
            TreeGroup treeGroup = new(source, null, groupPrincipal, 1);
            PushToStack(groupPrincipal, treeGroup);
        }
    }

    protected override void BuildFromAD(
        TreeGroup parent,
        GroupPrincipal groupPrincipal,
        string source,
        int depth)
    {
        IEnumerable<Principal> principalMembership = groupPrincipal.ToSafeSortedEnumerable(
            selector: principal => principal.GetGroups(Context),
            cmdlet: this,
            comparer: Comparer);

        foreach (Principal group in principalMembership)
        {
            if (ShouldExclude(group))
            {
                continue;
            }

            TreeGroup treeGroup = ProcessGroup(parent, (GroupPrincipal)group, source, depth);
            parent.AddChild(treeGroup);
        }
    }

    protected override void BuildFromCache(TreeGroup parent, int depth)
    {
        if (depth > Depth)
        {
            return;
        }

        foreach (TreeObjectBase child in parent.Children)
        {
            TreeGroup group = (TreeGroup)child;
            PushToStack(null, (TreeGroup)group.Clone(parent, depth));
        }
    }
}
