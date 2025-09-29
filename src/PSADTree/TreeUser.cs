using System.DirectoryServices.AccountManagement;

namespace PSADTree;

public sealed class TreeUser : TreeObjectBase
{
    private TreeUser(
        TreeUser user,
        TreeGroup parent,
        string source,
        int depth)
        : base(user, parent, source, depth)
    { }

    internal TreeUser(
        string source,
        TreeGroup? parent,
        UserPrincipal user,
        int depth)
        : base(source, parent, user, depth)
    { }

    internal TreeUser(string source, UserPrincipal user)
        : base(source, user)
    { }

    internal override TreeObjectBase Clone(TreeGroup parent, string source, int depth)
        => new TreeUser(this, parent, source, depth);
}
