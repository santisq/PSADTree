using System.ComponentModel;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace PSADTree.Internal;

#pragma warning disable IDE1006

[EditorBrowsable(EditorBrowsableState.Never)]
public static class _FormattingInternals
{
    private static readonly Regex s_getDomain = new(
        @"^DC=|(?<!\\),.+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string GetSource(TreeObjectBase treeObject) => treeObject.Source;

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string GetDomain(TreeObjectBase treeObject) => s_getDomain.Replace(treeObject.Domain, string.Empty);
}
