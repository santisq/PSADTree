using System.Collections.Generic;

namespace PSADTree;

internal sealed class TreeIndex
{
    private readonly List<TreeObjectBase> _principals;

    private readonly List<TreeObjectBase> _output;

    internal TreeIndex()
    {
        _principals = new();
        _output = new();
    }

    internal void AddPrincipal(TreeObjectBase principal) =>
        _principals.Add(principal);

    internal void Add(TreeObjectBase principal) =>
        _output.Add(principal);

    internal void TryAddPrincipals()
    {
        if (_principals.Count > 0)
        {
            _output.AddRange(_principals.ToArray());
            _principals.Clear();
        }
    }

    internal TreeObjectBase[] GetTree() =>
        _output.ToArray().ConvertToTree();

    internal void Clear()
    {
        _output.Clear();
        _principals.Clear();
    }
}
