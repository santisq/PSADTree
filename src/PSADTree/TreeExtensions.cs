using System.Text;
using System.Text.RegularExpressions;

namespace PSADTree;

internal static class TreeExtensions
{
    private static readonly Regex s_reBoxOrNotSpace = new(
        @"└|\S",
        RegexOptions.Compiled);

    private static readonly Regex s_reDefaultNamingContext = new(
        "(?<=,)DC=.+$",
        RegexOptions.Compiled);

    private static readonly StringBuilder s_sb = new();

    internal static string Indent(this string inputString, int indentation)
    {
        s_sb.Clear();

        return s_sb.Append(' ', (4 * indentation) - 4)
            .Append("└── ")
            .Append(inputString)
            .ToString();
    }

    internal static string GetDefaultNamingContext(this string distinguishedName) =>
        s_reDefaultNamingContext.Match(distinguishedName).Value;

    internal static TreeObjectBase[] ConvertToTree(
        this TreeObjectBase[] inputObject)
    {
        for (int i = 0; i < inputObject.Length; i++)
        {
            int index = inputObject[i].Hierarchy.IndexOf('└');

            if (index < 0)
            {
                continue;
            }

            int z = i - 1;

            while (!s_reBoxOrNotSpace.IsMatch(inputObject[z].Hierarchy[index].ToString()))
            {
                char[] replace = inputObject[z].Hierarchy.ToCharArray();
                replace[index] = '│';
                inputObject[z].Hierarchy = new string(replace);
                z--;
            }

            if (inputObject[z].Hierarchy[index] == '└')
            {
                char[] replace = inputObject[z].Hierarchy.ToCharArray();
                replace[index] = '├';
                inputObject[z].Hierarchy = new string(replace);
            }
        }

        return inputObject;
    }
}
