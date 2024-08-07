using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Management.Automation;

namespace PSADTree;

public abstract class PSADTreeCmdletBase : PSCmdlet, IDisposable
{
    protected const string DepthParameterSet = "Depth";

    protected const string RecursiveParameterSet = "Recursive";

    protected PrincipalContext? _context;

    private bool _disposed;

    protected readonly Stack<(GroupPrincipal? group, TreeGroup treeGroup)> _stack = new();

    internal readonly TreeCache _cache = new();

    internal readonly TreeIndex _index = new();

    [Parameter(
        Position = 0,
        Mandatory = true,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true)]
    [Alias("DistinguishedName")]
    public string? Identity { get; set; }

    [Parameter]
    public string? Server { get; set; }

    [Parameter(ParameterSetName = DepthParameterSet)]
    [ValidateRange(0, int.MaxValue)]
    public int Depth { get; set; } = 3;

    [Parameter(ParameterSetName = RecursiveParameterSet)]
    public SwitchParameter Recursive { get; set; }

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

    protected void Push(GroupPrincipal? groupPrincipal, TreeGroup treeGroup)
    {
        if (Recursive.IsPresent || treeGroup.Depth <= Depth)
        {
            _stack.Push((groupPrincipal, treeGroup));
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _context?.Dispose();
            _disposed = true;
        }
    }

    protected void Clear()
    {
        _index.Clear();
        _cache.Clear();
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
