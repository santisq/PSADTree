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
        string source = principal.DistinguishedName;
        switch (principal)
        {
            case UserPrincipal user:
                HandleOther(new TreeUser(source, user), principal);
                break;

            case ComputerPrincipal computer:
                HandleOther(new TreeComputer(source, computer), principal);
                break;

            case GroupPrincipal group:
                HandleGroup(new TreeGroup(source, group), group);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(principal));
        }

        void HandleGroup(TreeGroup treeGroup, GroupPrincipal groupPrincipal)
        {
            if (!ShouldExclude(groupPrincipal))
            {
                PushToStack(groupPrincipal, treeGroup);
            }
        }

        void HandleOther(TreeObjectBase treeObject, Principal principal)
        {
            Builder.Add(treeObject);

            IEnumerable<Principal> principalMembership = principal.ToSafeSortedEnumerable(
                selector: principal => principal.GetGroups(Context),
                cmdlet: this,
                comparer: Comparer);

            foreach (Principal parent in principalMembership)
            {
                if (!ShouldExclude(parent))
                {
                    GroupPrincipal groupPrincipal = (GroupPrincipal)parent;
                    TreeGroup treeGroup = new(source, null, groupPrincipal, 1);
                    PushToStack(groupPrincipal, treeGroup);
                }
            }
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

    protected override void BuildFromCache(TreeGroup parent, string source, int depth)
    {
        if (depth > Depth)
        {
            return;
        }

        foreach (TreeObjectBase child in parent.Children)
        {
            TreeGroup group = (TreeGroup)child;
            PushToStack(null, (TreeGroup)group.Clone(parent, source, depth));
        }
    }
}
