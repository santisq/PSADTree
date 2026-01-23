using System.DirectoryServices.AccountManagement;

namespace PSADTree;

public sealed class TreeComputer : TreeObjectBase
{
    private TreeComputer(
        TreeComputer computer,
        TreeGroup parent,
        string source,
        int depth)
        : base(computer, parent, source, depth)
    { }

    internal TreeComputer(
        string source,
        TreeGroup? parent,
        ComputerPrincipal computer,
        string[] properties,
        int depth)
        : base(source, parent, computer, properties, depth)
    { }

    internal TreeComputer(string source, ComputerPrincipal computer, string[] properties)
        : base(source, computer, properties)
    { }

    internal override TreeObjectBase Clone(TreeGroup parent, string source, int depth)
        => new TreeComputer(this, parent, source, depth);
}
