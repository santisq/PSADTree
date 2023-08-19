using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PSADTree;

internal sealed class TreeCache
{
    private readonly Dictionary<string, TreeObject> _cache;

    private readonly Queue<TreeObject> _queue;

    internal TreeObject this[string distinguishedName] =>
        _cache[distinguishedName];

    internal TreeCache()
    {
        _cache = new();
        _queue = new();
    }

    internal void Add(string distinguishedName, TreeObject principal) =>
        _cache.Add(distinguishedName, principal);

    internal bool TryAdd(TreeObject treeObject)
    {
        if (_cache.ContainsKey(treeObject.DistinguishedName))
        {
            return false;
        }

        _cache.Add(treeObject.DistinguishedName, treeObject);
        return true;
    }

    internal bool TryGet(
        string distinguishedName,
        [NotNullWhen(true)] out TreeObject? principal) =>
        _cache.TryGetValue(distinguishedName, out principal);

    internal bool IsCircular(TreeObject node)
    {
        foreach (TreeObject parent in node.Parent)
        {
            _queue.Enqueue(parent);
        }

        while (_queue.Count > 0)
        {
            TreeObject current = _queue.Dequeue();
            if (node.DistinguishedName == current.DistinguishedName)
            {
                return true;
            }

            foreach (TreeObject parent in current.Parent)
            {
                _queue.Enqueue(_cache[parent.DistinguishedName]);
            }
        }

        return false;
    }

    internal void Clear() => _cache.Clear();
}
