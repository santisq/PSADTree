using System.Collections.Generic;

namespace PSADTree;

internal sealed class TreeBuilder
{
    private readonly List<TreeObjectBase> _principals = [];

    private readonly List<TreeObjectBase> _output = [];

    internal void AddPrincipal(TreeObjectBase principal) => _principals.Add(principal);

    internal void Add(TreeObjectBase principal) => _output.Add(principal);

    internal void TryAddPrincipals()
    {
        if (_principals.Count > 0)
        {
            _output.AddRange(_principals);
            _principals.Clear();
        }
    }

    internal TreeObjectBase[] GetTree() => _output.ToArray().Format();

    internal void Clear()
    {
        _output.Clear();
        _principals.Clear();
    }
}
