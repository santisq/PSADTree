using System.DirectoryServices.AccountManagement;
using System.Text;
using System.Text.RegularExpressions;

namespace PSADTree;

internal static partial class TreeExtensions
{
    [GeneratedRegex("└|\\S", RegexOptions.Compiled)]
    private static partial Regex Init();

    private static readonly Regex s_re = Init();

    private static readonly StringBuilder s_sb = new();

    internal static string Indent(this string inputString, int indentation)
    {
        s_sb.Clear();

        return s_sb.Append(' ', (4 * indentation) - 4)
            .Append("└── ")
            .Append(inputString)
            .ToString();
    }

    internal static TreeObject ToTreeObject(
        this Principal principal,
        string source,
        int depth) =>
        new(source, principal, depth);

    internal static TreeObject ToTreeObject(
        this Principal principal,
        string source) =>
        new(source, principal);

    internal static TreeObject[] ConvertToTree(
        this TreeObject[] inputObject)
    {
        for (int i = 0; i < inputObject.Length; i++)
        {
            int index = inputObject[i].Hierarchy.IndexOf('└');

            if (index < 0)
            {
                continue;
            }

            int z = i - 1;

            while (!s_re.IsMatch(inputObject[z].Hierarchy[index].ToString()))
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
