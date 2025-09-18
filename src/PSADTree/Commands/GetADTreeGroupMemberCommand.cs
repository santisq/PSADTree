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

    protected override Principal GetFirstPrincipal() => GroupPrincipal.FindByIdentity(Context, Identity);

    protected override void HandleFirstPrincipal(Principal principal)
    {
        if (principal is GroupPrincipal group && !ShouldExclude(principal))
        {
            PushToStack(group, new(group.DistinguishedName, group));
        }
    }

    protected override void BuildFromAD(
        TreeGroup parent,
        GroupPrincipal groupPrincipal,
        string source,
        int depth)
    {
        IEnumerable<Principal> members = groupPrincipal.ToSafeSortedEnumerable(
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

    protected override void BuildFromCache(TreeGroup parent, int depth)
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
                Builder.Add(member.Clone(parent, depth));
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
                Builder.Stage(obj);
            }

            return obj;
        }
    }
}
