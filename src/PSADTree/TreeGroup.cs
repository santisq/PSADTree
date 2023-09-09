using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.AccountManagement;

namespace PSADTree;

public sealed class TreeGroup : TreeObjectBase
{
    private const string _isCircular = " ↔ Circular Reference";

    private const string _isProcessed = " ↔ Processed Group";

    private const string _vtBrightRed = "\x1B[91m";

    private const string _vtReset = "\x1B[0m";

    private List<TreeObjectBase>? _childs;

    public ReadOnlyCollection<TreeObjectBase> Childs => new(_childs ??= new());

    public bool IsCircular { get; private set; }

    private TreeGroup(
        TreeGroup group,
        TreeGroup parent,
        int depth)
        : base(group, parent, depth)
    {
        _childs = group._childs;
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
        Hierarchy = string.Concat(
            Hierarchy.Insert(
                Hierarchy.IndexOf("─ ") + 2,
                _vtBrightRed),
            _isCircular,
            _vtReset);
    }

    internal void SetProcessed() =>
        Hierarchy = string.Concat(Hierarchy, _isProcessed);

    internal void Hook(TreeCache cache) =>
        _childs ??= cache[DistinguishedName]._childs;

    internal void AddChild(TreeObjectBase child)
    {
        _childs ??= new();
        _childs.Add(child);
    }

    internal override TreeObjectBase Clone(TreeGroup parent, int depth) =>
        new TreeGroup(this, parent, depth);
}
