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

    [ThreadStatic]
    private static readonly Stack<(int depth, GroupPrincipal group)> _stack = new();

    [ThreadStatic]
    private static readonly TreeCache _cache = new();

    [ThreadStatic]
    private static readonly TreeIndex _index = new();

    [Parameter(
        Position = 0,
        Mandatory = true,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    [Alias("DistinguishedName")]
    public string? Identity { get; set; }

    protected override void BeginProcessing()
    {
        try
        {
            _context = new PrincipalContext(ContextType.Domain);
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

        using Principal principal = Principal.FindByIdentity(_context, Identity);

        if (principal is GroupPrincipal group)
        {
            WriteObject(
                sendToPipeline: Traverse(
                    groupPrincipal: group,
                    source: group.DistinguishedName),
                enumerateCollection: true);
        }
    }

    private TreeObject[] Traverse(
        GroupPrincipal groupPrincipal,
        string source)
    {
        _stack.Push((depth: 0, groupPrincipal));

        GroupPrincipal current;
        int depth;

        while (_stack.Count > 0)
        {
            (depth, current) = _stack.Pop();
            depth++;

            try
            {
                foreach (Principal member in current.GetMembers())
                {
                    if (member is not GroupPrincipal group)
                    {
                        _index.AddPrincipal(member, source, depth);
                        member.Dispose();
                        continue;
                    }

                    if (!_cache.TryGet(group.DistinguishedName, out TreeObject? treeObject))
                    {
                        _cache.Add(
                            group.DistinguishedName,
                            group.ToTreeObject(source, depth));

                        _stack.Push((depth, group));
                    }
                }

                _index.Add(_cache[current.DistinguishedName]);
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

    public void Dispose()
    {
        _context?.Dispose();
        _cache?.Clear();
    }
}
