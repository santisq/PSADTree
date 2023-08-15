using System.Collections.Generic;

namespace PSADTree;

internal sealed class TreeCache
{
    private readonly Dictionary<string, TreeObject> _cache;

    internal TreeObject this[string distinguishedName]
    {
        get => _cache[distinguishedName];
    }

    internal TreeCache() => _cache = new();

    internal void Add(string distinguishedName, TreeObject principal) =>
        _cache.Add(distinguishedName, principal);

    internal bool TryGet(string distinguishedName, out TreeObject? principal) =>
        _cache.TryGetValue(distinguishedName, out principal);

    internal void Clear() => _cache.Clear();
}
