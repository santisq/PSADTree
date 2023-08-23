using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PSADTree;

internal sealed class TreeCache
{
    private readonly Dictionary<string, TreeGroup> _cache;

    internal TreeGroup this[string distinguishedName] =>
        _cache[distinguishedName];

    internal TreeCache() => _cache = new();

    internal void Add(string distinguishedName, TreeGroup principal) =>
        _cache.Add(distinguishedName, principal);

    internal bool TryAdd(TreeGroup group)
    {
        if (_cache.ContainsKey(group.DistinguishedName))
        {
            return false;
        }

        _cache.Add(group.DistinguishedName, group);
        return true;
    }

    internal bool TryGet(
        string distinguishedName,
        [NotNullWhen(true)] out TreeGroup? principal) =>
        _cache.TryGetValue(distinguishedName, out principal);

    internal static bool IsCircular(TreeGroup node)
    {
        if (node.Parent is null)
        {
            return false;
        }

        TreeGroup? current = node.Parent;
        while (current is not null)
        {
            if (node.DistinguishedName == current.DistinguishedName)
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }

    internal void Clear() => _cache.Clear();
}
