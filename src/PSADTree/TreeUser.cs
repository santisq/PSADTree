using System.DirectoryServices.AccountManagement;

namespace PSADTree;

public sealed class TreeUser : TreeObjectBase
{
    private TreeUser(TreeUser user, TreeGroup parent, int depth)
        : base(user, parent, depth)
    { }

    internal TreeUser(
        string source,
        TreeGroup? parent,
        UserPrincipal user,
        int depth)
        : base(source, parent, user, depth)
    { }

    internal override TreeObjectBase Clone(TreeGroup parent, int depth) =>
        new TreeUser(this, parent, depth);
}
