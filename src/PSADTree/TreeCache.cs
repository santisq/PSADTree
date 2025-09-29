using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PSADTree;

internal sealed class TreeCache
{
    private readonly Dictionary<string, TreeGroup> _cache = [];

    internal TreeGroup this[string distinguishedName] => _cache[distinguishedName];

    internal bool TryAdd(TreeGroup group)
    {
#if NET6_0_OR_GREATER
        return _cache.TryAdd(group.DistinguishedName, group);
#else
        if (_cache.ContainsKey(group.DistinguishedName))
        {
            return false;
        }

        _cache.Add(group.DistinguishedName, group);
        return true;
#endif
    }

    internal bool TryGet(string distinguishedName, [NotNullWhen(true)] out TreeGroup? group)
        => _cache.TryGetValue(distinguishedName, out group);
}
