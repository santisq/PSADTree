using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.AccountManagement;

namespace PSADTree;

public sealed class TreeGroup : TreeObjectBase
{
    private const string Circular = $" ↔ {VTBrightRed}Circular Reference{VTReset}";

    private const string Processed = $" ↔ {VTBrightYellow}Processed Group{VTReset}";

    private const string VTBrightRed = "\x1B[91m";

    private const string VTBrightYellow = "\x1B[93m";

    private const string VTReset = "\x1B[0m";

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

    internal TreeGroup(string source, GroupPrincipal group)
        : base(source, group)
    {
        _children = [];
    }

    internal TreeGroup(
        string source,
        TreeGroup? parent,
        GroupPrincipal group,
        int depth)
        : base(source, parent, group, depth)
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
            Hierarchy = $"{Hierarchy}{Circular}";
        }

        return IsCircular;
    }

    internal void SetProcessed() => Hierarchy = $"{Hierarchy}{Processed}";

    internal void LinkCachedChildren(TreeCache cache)
    {
        TreeGroup cached = cache[DistinguishedName];
        _children = cached._children;
    }

    internal void AddChild(TreeObjectBase child) => _children.Add(child);

    internal override TreeObjectBase Clone(TreeGroup parent, string source, int depth)
        => new TreeGroup(this, parent, source, depth);
}
