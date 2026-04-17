using System.ComponentModel;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace PSADTree.Internal;

#pragma warning disable IDE1006

[EditorBrowsable(EditorBrowsableState.Never)]
public static partial class _FormattingInternals
{
#if NET8_0_OR_GREATER
    [GeneratedRegex(@"^DC=|(?<!\\),.+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex GetDomainRegex();
    private static readonly Regex s_getDomain = GetDomainRegex();
#else
    private static readonly Regex s_getDomain = new(
        @"^DC=|(?<!\\),.+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
#endif

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string GetSource(TreeObjectBase treeObject) => treeObject.Source;

    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string GetDomain(TreeObjectBase treeObject) => s_getDomain.Replace(treeObject.Domain, string.Empty);
}
