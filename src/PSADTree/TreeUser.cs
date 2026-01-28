using System.DirectoryServices.AccountManagement;
using PSADTree.Extensions;

namespace PSADTree;

public sealed class TreeUser : TreeObjectBase
{
    public UserAccountControl? UserAccountControl { get; }

    public bool? Enabled { get; }

    private TreeUser(
        TreeUser user,
        TreeGroup parent,
        string source,
        int depth)
        : base(user, parent, source, depth)
    {
        UserAccountControl = user.UserAccountControl;
        Enabled = user.Enabled;
    }

    internal TreeUser(
        string source,
        TreeGroup? parent,
        UserPrincipal user,
        string[] properties,
        int depth)
        : base(source, parent, user, properties, depth)
    {
        UserAccountControl = user.GetUserAccountControl();
        Enabled = UserAccountControl?.IsEnabled();
    }

    internal TreeUser(string source, UserPrincipal user, string[] properties)
        : base(source, user, properties)
    {
        UserAccountControl = user.GetUserAccountControl();
        Enabled = UserAccountControl?.IsEnabled();
    }

    internal override TreeObjectBase Clone(TreeGroup parent, string source, int depth)
        => new TreeUser(this, parent, source, depth);
}
