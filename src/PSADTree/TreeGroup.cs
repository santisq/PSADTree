using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.AccountManagement;

namespace PSADTree;

public sealed class TreeGroup : TreeObjectBase
{
    private List<TreeObjectBase>? _members;

    public ReadOnlyCollection<TreeObjectBase> Members => new(_members ??= new());

    public bool IsCircular { get; private set; }

    private TreeGroup(
        TreeGroup group,
        TreeGroup parent,
        int depth)
        : base(group, parent, depth)
    {
        _members = group._members;
    }

    internal TreeGroup(
        string source,
        GroupPrincipal group)
        : base(source, group)
    { }

    internal TreeGroup(
        string source,
        TreeGroup parent,
        GroupPrincipal group,
        int depth)
        : base(source, parent, group, depth)
    { }

    // internal void AddParent(TreeGroup parent) =>
    //     Parent = parent;

    internal void SetCircularNested() => IsCircular = true;

    internal void AddMember(TreeObjectBase member)
    {
        _members ??= new();
        _members.Add(member);
    }

    internal void Hook(TreeGroup group)
    {
        _members = group._members;
        // Parent = group.Parent;
        // _parent ??= group._parent;
    }

    internal override TreeObjectBase Clone(TreeGroup parent, int depth) =>
        new TreeGroup(this, parent, depth);
}
