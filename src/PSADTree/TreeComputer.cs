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
        int depth)
        : base(source, parent, computer, depth)
    { }

    internal TreeComputer(string source, ComputerPrincipal computer)
        : base(source, computer)
    { }

    internal override TreeObjectBase Clone(TreeGroup parent, string source, int depth)
        => new TreeComputer(this, parent, source, depth);
}
