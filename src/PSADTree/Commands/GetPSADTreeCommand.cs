using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Management.Automation;

namespace PSADTree;

[Cmdlet(VerbsCommon.Get, "PSADTree")]
[Alias("psadtree")]
[OutputType(typeof(TreeObject))]
public sealed class GetPSADTreeCommand : PSCmdlet, IDisposable
{
    private PrincipalContext? _context;

    private readonly Stack<(int depth, GroupPrincipal group)> _stack = new();

    private readonly TreeCache _cache = new();

    private readonly TreeIndex _index = new();

    [Parameter(
        Position = 0,
        Mandatory = true,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    [Alias("DistinguishedName")]
    public string? Identity { get; set; }

    [Parameter]
    public string? Server { get; set; }

    protected override void BeginProcessing()
    {
        try
        {
            if (Server is null)
            {
                _context = new PrincipalContext(ContextType.Domain);
                return;
            }

            _context = new PrincipalContext(ContextType.Domain, Server);
        }
        catch (Exception e)
        {
            ThrowTerminatingError(new ErrorRecord(
                e, "SetPrincipalContext", ErrorCategory.ConnectionError, null));
        }
    }

    protected override void ProcessRecord()
    {
        Dbg.Assert(Identity is not null);
        Dbg.Assert(_context is not null);

        try
        {
            using Principal? principal = Principal.FindByIdentity(_context, Identity);
            if (principal is null)
            {
                WriteError(new ErrorRecord(
                    new NoMatchingPrincipalException($"Cannot find an object with identity: '{Identity}'."),
                    "IdentityNotFound",
                    ErrorCategory.ObjectNotFound,
                    Identity));

                return;
            }

            if (principal is GroupPrincipal group)
            {
                WriteObject(
                    sendToPipeline: Traverse(
                        groupPrincipal: group,
                        source: group.DistinguishedName),
                    enumerateCollection: true);
            }
        }
        catch (Exception e) when (e is PipelineStoppedException or FlowControlException)
        {
            throw;
        }
        catch (MultipleMatchesException e)
        {
            WriteError(new ErrorRecord(
                e,
                "AmbiguousIdentity",
                ErrorCategory.InvalidResult,
                Identity));
        }
        catch (Exception e)
        {
            WriteError(new ErrorRecord(
                e,
                "Unspecified",
                ErrorCategory.NotSpecified,
                Identity));
        }
    }

    private TreeObject[] Traverse(
        GroupPrincipal groupPrincipal,
        string source)
    {
        _stack.Push((depth: 0, groupPrincipal));
        _index.Clear();
        _cache.Clear();

        while (_stack.Count > 0)
        {
            (int depth, GroupPrincipal current) = _stack.Pop();

            TreeObject treeObject = current.ToTreeObject(source, depth);

            if (current is { DistinguishedName: null })
            {
                _index.Add(treeObject);
                continue;
            }

            if (!_cache.TryAdd(treeObject))
            {
                treeObject.Hierarchy += " <-> Possible Circular Reference (need to handle this later)";
                _index.Add(treeObject);
                current.Dispose();
                // handle possible circular reference here
                continue;
            }

            depth++;

            try
            {
                using PrincipalSearchResult<Principal> search = current.GetMembers();
                EnumerateMembers(search, source, depth);
                _index.Add(treeObject);
                _index.TryAddPrincipals();
                current.Dispose();
            }
            catch (Exception e) when (e is PipelineStoppedException or FlowControlException)
            {
                throw;
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(
                    e, "EnumerationError", ErrorCategory.NotSpecified, current));
            }
        }

        return _index.GetTree();
    }

    private void EnumerateMembers(
        PrincipalSearchResult<Principal> searchResult,
        string source,
        int depth)
    {
        foreach (Principal member in searchResult)
        {
            if (member is not GroupPrincipal group)
            {
                _index.AddPrincipal(member, source, depth);
                member.Dispose();
                continue;
            }

            _stack.Push((depth, group));
        }
    }

    public void Dispose() => _context?.Dispose();
}
