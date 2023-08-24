using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PSADTree;

internal static class Dbg
{
    [Conditional("DEBUG")]
    public static void Assert([DoesNotReturnIf(false)] bool condition) =>
        Debug.Assert(condition);
}
