using System.DirectoryServices.AccountManagement;

namespace PSADTree.Style;

public sealed class UserStyle
{
    internal UserStyle()
    { }

    internal string GetColoredName(TreeObjectBase user)
    {
        return user.SamAccountName;
    }

    internal string GetColoredName(UserPrincipal user)
    {
        return user.SamAccountName;
    }

}
