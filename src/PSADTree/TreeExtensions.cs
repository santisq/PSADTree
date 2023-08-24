using System.Text;
using System.Text.RegularExpressions;

namespace PSADTree;

internal static class TreeExtensions
{
    // private static readonly Regex s_reBoxOrNotSpace = new(
    //     @"└|\S",
    //     RegexOptions.Compiled);

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
        int index;
        TreeObjectBase current;
        for (int i = 0; i < inputObject.Length; i++)
        {
            current = inputObject[i];
            if ((index = current.Hierarchy.IndexOf('└')) == -1)
            {
                continue;
            }

            int z;
            char[] replace;
            for (z = i - 1; z >= 0; z--)
            {
                current = inputObject[z];
                if (!char.IsWhiteSpace(current.Hierarchy[index]))
                {
                    UpdateCorner(index, current);
                    break;
                }

                replace = current.Hierarchy.ToCharArray();
                replace[index] = '│';
                current.Hierarchy = new string(replace);
            }

            // while (!s_reBoxOrNotSpace.IsMatch(inputObject[z].Hierarchy[index].ToString()))
            // {
            //     char[] replace = inputObject[z].Hierarchy.ToCharArray();
            //     replace[index] = '│';
            //     inputObject[z].Hierarchy = new string(replace);
            //     z--;
            // }

            // if (inputObject[z].Hierarchy[index] == '└')
            // {
            //     char[] replace = inputObject[z].Hierarchy.ToCharArray();
            //     replace[index] = '├';
            //     inputObject[z].Hierarchy = new string(replace);
            // }
        }

        return inputObject;
    }

    private static void UpdateCorner(int index, TreeObjectBase current)
    {
        if (current.Hierarchy[index] == '└')
        {
            char[] replace = current.Hierarchy.ToCharArray();
            replace[index] = '├';
            current.Hierarchy = new string(replace);
        }
    }
}
