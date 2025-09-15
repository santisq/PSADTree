using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.AccountManagement;

namespace PSADTree;

public sealed class TreeGroup : TreeObjectBase
{
    private const string Circular = " ↔ Circular Reference";

    private const string Processed = " ↔ Processed Group";

    private const string VTBrightRed = "\x1B[91m";

    private const string VTReset = "\x1B[0m";

    private readonly bool _isCloned;

    private List<TreeObjectBase> _children;

    public ReadOnlyCollection<TreeObjectBase> Children => new(_children);

    public bool IsCircular { get; private set; }

    private TreeGroup(
        TreeGroup group,
        TreeGroup parent,
        int depth)
        : base(group, parent, depth)
    {
        _isCloned = true;
        _children = group._children;
        IsCircular = group.IsCircular;
    }

    internal TreeGroup(
        string source,
        GroupPrincipal group)
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
        // there is no need to check again if the object is cloned
        if (_isCloned)
        {
            return IsCircular;
        }

        if (Parent is null)
        {
            return false;
        }

        TreeGroup? current = Parent;
        while (current is not null)
        {
            if (DistinguishedName == current.DistinguishedName)
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }

    internal bool SetIfCircularNested()
    {
        if (IsCircular = IsCircularNested())
        {
            Hierarchy = $"{Hierarchy.Insert(Hierarchy.IndexOf("─ ") + 2, VTBrightRed)}{Circular}{VTReset}";
        }

        return IsCircular;
    }

    internal void SetProcessed() => Hierarchy = $"{Hierarchy}{Processed}";

    internal void LinkCachedChildren(TreeCache cache)
    {
        _children = cache[DistinguishedName]._children;
    }

    internal void AddChild(TreeObjectBase child) => _children.Add(child);

    internal override TreeObjectBase Clone(TreeGroup parent, int depth)
        => new TreeGroup(this, parent, depth);
}
