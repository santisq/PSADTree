using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;

namespace PSADTree;

internal sealed class TreeIndex
{
    private readonly List<TreeObject> _principals;

    private readonly List<TreeObject> _output;

    internal TreeIndex()
    {
        _principals = new();
        _output = new();
    }

    internal void AddPrincipal(Principal principal, string source, int depth) =>
        _principals.Add(principal.ToTreeObject(source, depth));

    internal void Add(TreeObject principal) => _output.Add(principal);

    internal void TryAddPrincipals()
    {
        if (_principals.Count > 0)
        {
            _output.AddRange(_principals.ToArray());
            _principals.Clear();
        }
    }

    internal TreeObject[] GetTree() => _output.ToArray().ConvertToTree();

    internal void Clear()
    {
        _output.Clear();
        _principals.Clear();
    }
}
