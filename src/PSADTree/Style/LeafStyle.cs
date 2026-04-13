using System.DirectoryServices.AccountManagement;

namespace PSADTree.Style;

public sealed class LeafStyle
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

    private TreeStyle TreeStyle { get => TreeStyle.Instance; }

    internal LeafStyle()
    { }

    internal string GetColoredName(TreeObjectBase treeObject)
    {
        if (TreeStyle.OutputRendering == OutputRendering.PlainText)
        {
            return treeObject.SamAccountName;
        }

        return $"{treeObject.SamAccountName}";
    }

    internal string GetColoredName(Principal principal)
    {
        if (TreeStyle.OutputRendering == OutputRendering.PlainText)
        {
            return principal.SamAccountName;
        }

        return $"{principal.SamAccountName}";
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
