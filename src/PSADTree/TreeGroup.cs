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

    private List<TreeObjectBase> _children = [];

    public ReadOnlyCollection<TreeObjectBase> Children => new(_children);

    public bool IsCircular { get; private set; }

    private TreeGroup(
        TreeGroup group,
        TreeGroup? parent,
        int depth)
        : base(group, parent, depth)
    {
        _children = group._children;
    }

    internal TreeGroup(
        string source,
        GroupPrincipal group)
        : base(source, group)
    { }

    internal TreeGroup(
        string source,
        TreeGroup? parent,
        GroupPrincipal group,
        int depth)
        : base(source, parent, group, depth)
    { }

    internal void SetCircularNested()
    {
        IsCircular = true;
        Hierarchy = $"{Hierarchy.Insert(Hierarchy.IndexOf("─ ") + 2, VTBrightRed)}{Circular}{VTReset}";
    }

    internal void SetProcessed() => Hierarchy = string.Concat(Hierarchy, Processed);

    internal void Hook(TreeCache cache) => _children = cache[DistinguishedName]._children;

    internal void AddChild(TreeObjectBase child) => _children.Add(child);

    internal override TreeObjectBase Clone(TreeGroup? parent = null, int depth = 0)
        => new TreeGroup(this, parent, depth);
}
