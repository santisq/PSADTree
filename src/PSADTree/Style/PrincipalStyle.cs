using System.DirectoryServices.AccountManagement;

namespace PSADTree.Style;

public sealed class PrincipalStyle
{
    public string Circular
    {
        get;
        set => field = TreeStyle.ThrowIfInvalidSequence(value);
    } = "\x1B[91m";

    public string Processed
    {
        get;
        set => field = TreeStyle.ThrowIfInvalidSequence(value);
    } = "\x1B[93m";

    public string Computer
    {
        get;
        set => TreeStyle.ThrowIfInvalidSequence(value);
    } = string.Empty;

    public string Group
    {
        get;
        set => field = TreeStyle.ThrowIfInvalidSequence(value);
    } = string.Empty;

    public string User
    {
        get;
        set => field = TreeStyle.ThrowIfInvalidSequence(value);
    } = string.Empty;

    private TreeStyle TreeStyle { get => TreeStyle.Instance; }

    internal PrincipalStyle()
    { }

    internal string GetColoredName(TreeObjectBase treeObject)
    {
        if (TreeStyle.OutputRendering == OutputRendering.PlainText)
        {
            return treeObject.SamAccountName;
        }

        return treeObject switch
        {
            TreeComputer treeComputer => $"{Computer}{treeComputer.SamAccountName}{TreeStyle.Reset}",
            TreeGroup treeGroup => $"{Group}{treeGroup.SamAccountName}{TreeStyle.Reset}",
            TreeUser treeUser => $"{User}{treeUser.SamAccountName}{TreeStyle.Reset}",
            _ => treeObject.SamAccountName
        };
    }

    internal string GetColoredName(Principal principal)
    {
        if (TreeStyle.OutputRendering == OutputRendering.PlainText)
        {
            return principal.SamAccountName;
        }

        return principal switch
        {
            ComputerPrincipal computerPrincipal => $"{Computer}{computerPrincipal.SamAccountName}{TreeStyle.Reset}",
            GroupPrincipal groupPrincipal => $"{Group}{groupPrincipal.SamAccountName}{TreeStyle.Reset}",
            UserPrincipal userPrincipal => $"{User}{userPrincipal.SamAccountName}{TreeStyle.Reset}",
            _ => principal.SamAccountName
        };
    }

    internal string GetColoredCircular()
    {
        if (TreeStyle.OutputRendering == OutputRendering.PlainText)
        {
            return $"{TreeStyle.RenderingSet.Arrows} Circular Reference";
        }

        return $"{TreeStyle.RenderingSet.Arrows} {Circular}Circular Reference{TreeStyle.Reset}";
    }

    internal string GetColoredProcessed()
    {
        if (TreeStyle.OutputRendering == OutputRendering.PlainText)
        {
            return $"{TreeStyle.RenderingSet.Arrows} Processed Group";
        }

        return $"{TreeStyle.RenderingSet.Arrows} {Processed}Processed Group{TreeStyle.Reset}";
    }
}
