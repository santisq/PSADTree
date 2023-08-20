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

    private readonly Stack<(GroupPrincipal? group, TreeObject treeObject)> _stack = new();

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

    [Parameter]
    public SwitchParameter ShowAll { get; set; }

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
        int depth;
        _index.Clear();
        _cache.Clear();
        _stack.Push((groupPrincipal, groupPrincipal.ToTreeObject(source)));

        while (_stack.Count > 0)
        {
            (GroupPrincipal? current, TreeObject treeObject) = _stack.Pop();

            if (current is { DistinguishedName: null })
            {
                _index.Add(treeObject);
                continue;
            }

            try
            {
                depth = treeObject.Depth + 1;

                // if this node has been already processed
                if (!_cache.TryAdd(treeObject))
                {
                    _index.Add(treeObject);
                    current?.Dispose();

                    // if it's a circular reference, go next
                    if (_cache.IsCircular(treeObject))
                    {
                        treeObject.Hierarchy += " <-> Circular Reference";
                        continue;
                    }

                    // else, if we want to show all nodes
                    if (ShowAll.IsPresent)
                    {
                        // reconstruct the output without querying AD again
                        treeObject.Hook(_cache[treeObject.DistinguishedName]);
                        EnumerateMembers(treeObject, depth);
                        continue;
                    }

                    // else, just skip this reference and go next
                    treeObject.Hierarchy += " <-> Processed Group";
                    continue;
                }

                using PrincipalSearchResult<Principal>? search = current?.GetMembers();

                if (search is not null)
                {
                    EnumerateMembers(treeObject, search, source, depth);
                    _index.Add(treeObject);
                    _index.TryAddPrincipals();
                    current?.Dispose();
                }
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
        TreeObject parent,
        PrincipalSearchResult<Principal> searchResult,
        string source,
        int depth)
    {
        TreeObject treeObject;
        foreach (Principal member in searchResult)
        {
            treeObject = member.ToTreeObject(source, depth);

            // we only need to add childs to the .Member property
            // if this switch is in use, otherwise it creates overhead
            if (ShowAll.IsPresent)
            {
                parent.AddMember(treeObject);
            }

            if (member is not GroupPrincipal group)
            {
                _index.AddPrincipal(treeObject);
                member.Dispose();
                continue;
            }

            treeObject.AddParent(parent);
            _stack.Push((group, treeObject));
        }
    }

    private void EnumerateMembers(TreeObject parent, int depth)
    {
        // this should be changed in the future,
        // possibly make TreeObject abstract or generic
        foreach (TreeObject member in parent.Member)
        {
            if (member.ObjectClass is not "group")
            {
                _index.Add(member.Clone(depth));
                continue;
            }

            _stack.Push((null, member.Clone(depth)));
        }
    }

    public void Dispose() => _context?.Dispose();
}
