using System.DirectoryServices.AccountManagement;
using PSADTree.Extensions;

namespace PSADTree;

public sealed class TreeComputer : TreeObjectBase
{
    public UserAccountControl? UserAccountControl { get; }

    public bool? Enabled { get; }

    private TreeComputer(
        TreeComputer computer,
        TreeGroup parent,
        string source,
        int depth)
        : base(computer, parent, source, depth)
    {
        UserAccountControl = computer.UserAccountControl;
        Enabled = computer.Enabled;
    }

    internal TreeComputer(
        string source,
        TreeGroup? parent,
        ComputerPrincipal computer,
        string[] properties,
        int depth)
        : base(source, parent, computer, properties, depth)
    {
        UserAccountControl = computer.GetUserAccountControl();
        Enabled = UserAccountControl?.IsEnabled();
    }

    internal TreeComputer(string source, ComputerPrincipal computer, string[] properties)
        : base(source, computer, properties)
    {
        UserAccountControl = computer.GetUserAccountControl();
        Enabled = UserAccountControl?.IsEnabled();
    }

    internal override TreeObjectBase Clone(TreeGroup parent, string source, int depth)
        => new TreeComputer(this, parent, source, depth);
}
