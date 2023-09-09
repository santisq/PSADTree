using System.ComponentModel;
using System.Management.Automation;

namespace PSADTree.Internal;

#pragma warning disable IDE1006

[EditorBrowsable(EditorBrowsableState.Never)]
public static class _FormattingInternals
{
    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string GetSource(TreeObjectBase treeObject) =>
        treeObject.Source;
}
