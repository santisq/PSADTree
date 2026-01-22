using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using PSADTree.Extensions;

namespace PSADTree;

public sealed class TreeUser : TreeObjectBase
{
    public UserAccountControl UserAccountControl { get; private set; }

    public bool Enabled { get; private set; }

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
        string[]? properties,
        int depth)
        : base(source, parent, user, properties, depth)
    { }

    internal TreeUser(string source, UserPrincipal user, string[]? properties)
        : base(source, user, properties)
    { }

    internal void SetUserAccountControl(DirectoryEntry entry)
    {
        UserAccountControl = entry.GetProperty<UserAccountControl>("userAccountControl");
        Enabled = !UserAccountControl.HasFlag(UserAccountControl.ACCOUNTDISABLE);
    }

    internal override TreeObjectBase Clone(TreeGroup parent, string source, int depth)
        => new TreeUser(this, parent, source, depth);
}
