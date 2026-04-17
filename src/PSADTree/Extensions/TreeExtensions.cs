using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Management.Automation;
#if NETCOREAPP
using System.Runtime.CompilerServices;
#else
using System.Text;
#endif
using System.Text.RegularExpressions;
using PSADTree.Style;

namespace PSADTree.Extensions;

internal static partial class TreeExtensions
{
#if NETCOREAPP
    [GeneratedRegex("(?<=,)DC=.+$", RegexOptions.Compiled)]
    private static partial Regex GetDefaultNamingContextRegex();

    private static readonly Regex s_reDefaultNamingContext = GetDefaultNamingContextRegex();
#else
    private static readonly Regex s_reDefaultNamingContext = new(
        "(?<=,)DC=.+$",
        RegexOptions.Compiled);
#endif

    extension(string input)
    {
        internal string GetDefaultNamingContext() => s_reDefaultNamingContext.Match(input).Value;

#if !NETCOREAPP
        [ThreadStatic]
        private static StringBuilder? s_sb;
#endif
        internal string Indent(int indentation)
        {
            string corner = TreeStyle.Instance.RenderingSet.Corner;
            int repeatCount = (4 * indentation) - 4;
            int capacity = repeatCount + 4 + input.Length;

#if NETCOREAPP
            return string.Create(
                capacity, (repeatCount, corner, input),
                static (buffer, state) =>
            {
                int count = state.repeatCount;
                buffer[..count].Fill(' ');
                state.corner.AsSpan().CopyTo(buffer[count..]);
                state.input.AsSpan().CopyTo(buffer[(count + 4)..]);
            });
#else
            s_sb ??= new StringBuilder(64);
            s_sb.Clear().EnsureCapacity(capacity);

            return s_sb
                .Append(' ', repeatCount)
                .Append(corner)
                .Append(input)
                .ToString();
#endif
        }

#if NETCOREAPP
        [SkipLocalsInit]
        private string ReplaceAt(int index, char newChar)
            => string.Create(
                input.Length, (input, index, newChar),
                static (buffer, state) =>
            {
                state.input.AsSpan().CopyTo(buffer);
                buffer[state.index] = state.newChar;
            });
#else
        private unsafe string ReplaceAt(int index, char newChar)
        {
            if (input.Length > 0x200)
            {
                char[] chars = input.ToCharArray();
                chars[index] = newChar;
                return new string(chars);
            }

            char* pChars = stackalloc char[0x200];
            fixed (char* source = input)
            {
                Buffer.MemoryCopy(
                    source,
                    pChars,
                    0x200 * sizeof(char),
                    input.Length * sizeof(char));
            }

            pChars[index] = newChar;
            return new string(pChars, 0, input.Length);
        }
#endif
    }

    extension(TreeObjectBase[] tree)
    {
        internal TreeObjectBase[] Format()
        {
            int index;
            RenderingSet set = TreeStyle.Instance.RenderingSet;
            for (int i = 0; i < tree.Length; i++)
            {
                TreeObjectBase current = tree[i];

                if ((index = current.Hierarchy.IndexOf(set.UpRight)) == -1)
                {
                    continue;
                }

                for (int z = i - 1; z >= 0; z--)
                {
                    current = tree[z];
                    string hierarchy = current.Hierarchy;

                    if (char.IsWhiteSpace(hierarchy[index]))
                    {
                        current.Hierarchy = hierarchy.ReplaceAt(index, set.Vertical);
                        continue;
                    }

                    if (hierarchy[index] == set.UpRight)
                    {
                        current.Hierarchy = hierarchy.ReplaceAt(index, set.VerticalRight);
                    }

                    break;
                }
            }

            return tree;
        }
    }

    extension<TPrincipal>(TPrincipal principal) where TPrincipal : Principal
    {
        internal IEnumerable<Principal> ToSafeSortedEnumerable(
            Func<TPrincipal, PrincipalSearchResult<Principal>> selector,
            PSCmdlet cmdlet)
        {
            List<Principal> principals = [];
            using PrincipalSearchResult<Principal> search = selector(principal);
            using IEnumerator<Principal> enumerator = search.GetEnumerator();

            while (true)
            {
                try
                {
                    if (!enumerator.MoveNext()) break;

                    principals.Add(enumerator.Current);
                }
                catch (Exception _) when (_ is PipelineStoppedException or FlowControlException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    cmdlet.WriteError(exception.ToEnumerationFailure(principal));
                }
            }

            return principals
                .OrderBy(static e => e.StructuralObjectClass == "group")
                .ThenBy(static e => e, PSADTreeComparer.Value);
        }
    }
}
