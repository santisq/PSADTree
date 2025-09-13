using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PSADTree;

internal sealed class TreeCache
{
    private readonly Dictionary<string, TreeGroup> _cache = [];

    internal TreeGroup this[string distinguishedName] => _cache[distinguishedName];

    internal void Add(TreeGroup group) => _cache.Add(group.DistinguishedName, group);

    internal bool TryAdd(TreeGroup group)
    {
        if (_cache.ContainsKey(group.DistinguishedName))
        {
            return false;
        }

        _cache.Add(group.DistinguishedName, group);
        return true;
    }

    internal bool TryGet(string distinguishedName, [NotNullWhen(true)] out TreeGroup? group)
        => _cache.TryGetValue(distinguishedName, out group);
}
