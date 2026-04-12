using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.AccountManagement;
using PSADTree.Style;

namespace PSADTree;

public sealed class TreeGroup : TreeObjectBase
{
    private List<TreeObjectBase> _children;

    public ReadOnlyCollection<TreeObjectBase> Children => new(_children);

    public bool IsCircular { get; private set; }

    private TreeGroup(
        TreeGroup group,
        TreeGroup parent,
        string source,
        int depth)
        : base(group, parent, source, depth)
    {
        _children = group._children;
        IsCircular = group.IsCircular;
    }

    internal TreeGroup(string source, GroupPrincipal group, string[] properties)
        : base(source, group, properties)
    {
        _children = [];
    }

    internal TreeGroup(
        string source,
        TreeGroup? parent,
        GroupPrincipal group,
        string[] properties,
        int depth)
        : base(source, parent, group, properties, depth)
    {
        _children = [];
    }

    private bool IsCircularNested()
    {
        if (Parent is null)
        {
            return false;
        }

        for (TreeGroup? parent = Parent; parent is not null; parent = parent.Parent)
        {
            if (DistinguishedName == parent.DistinguishedName)
            {
                return true;
            }
        }

        return false;
    }

    internal bool SetIfCircularNested()
    {
        if (IsCircular = IsCircularNested())
        {
            Hierarchy = $"{Hierarchy} {TreeStyle.Instance.Group.GetColoredCircular()}";
        }

        return IsCircular;
    }

    internal void SetProcessed()
    {
        Hierarchy = $"{Hierarchy} {TreeStyle.Instance.Group.GetColoredProcessed()}";
    }

    internal void LinkCachedChildren(TreeCache cache)
        => _children = cache[DistinguishedName]._children;

    internal void AddChild(TreeObjectBase child) => _children.Add(child);

    internal override TreeObjectBase Clone(TreeGroup parent, string source, int depth)
        => new TreeGroup(this, parent, source, depth);
}
