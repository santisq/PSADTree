using System.DirectoryServices.AccountManagement;

namespace PSADTree.Style;

public sealed class GroupStyle
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

    internal GroupStyle()
    { }

    internal string GetColoredName(TreeObjectBase group)
    {
        return group.SamAccountName;
    }

    internal string GetColoredName(GroupPrincipal group)
    {
        return group.SamAccountName;
    }

    internal string GetColoredProcessed()
    {
        TreeStyle treeStyle = TreeStyle.Instance;
        if (treeStyle.OutputRendering == OutputRendering.PlainText)
        {
            return $"{treeStyle.RenderingSet.Arrows} Circular Reference";
        }

        return $"{treeStyle.RenderingSet.Arrows} {Circular}Circular Reference{treeStyle.Reset}";
    }

    internal string GetColoredCircular()
    {
        TreeStyle treeStyle = TreeStyle.Instance;
        if (treeStyle.OutputRendering == OutputRendering.PlainText)
        {
            return $"{treeStyle.RenderingSet.Arrows} Processed Group";
        }

        return $"{treeStyle.RenderingSet.Arrows} {Processed}Processed Group{treeStyle.Reset}";
    }
}
