using System.DirectoryServices.AccountManagement;

namespace PSADTree.Style;

public sealed class ComputerStyle
{
    internal ComputerStyle()
    { }

    internal string GetColoredName(TreeObjectBase computer)
    {
        return computer.SamAccountName;
    }

    internal string GetColoredName(ComputerPrincipal computer)
    {
        return computer.SamAccountName;
    }
}
