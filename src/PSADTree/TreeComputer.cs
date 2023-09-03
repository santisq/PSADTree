using System.DirectoryServices.AccountManagement;

namespace PSADTree;

public sealed class TreeComputer : TreeObjectBase
{
    private TreeComputer(
        TreeComputer computer,
        TreeGroup parent,
        int depth)
        : base(computer, parent, depth)
    { }

    internal TreeComputer(
        string source,
        TreeGroup? parent,
        ComputerPrincipal computer,
        int depth)
        : base(source, parent, computer, depth)
    { }

    internal TreeComputer(
        string source,
        ComputerPrincipal computer)
        : base(source, computer)
    { }

    internal override TreeObjectBase Clone(TreeGroup parent, int depth) =>
        new TreeComputer(this, parent, depth);
}
