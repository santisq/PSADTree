using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Management.Automation;

namespace PSADTree;

public abstract class PSADTreeCmdletBase : PSCmdlet, IDisposable
{
    private bool _disposed;

    private bool _truncatedOutput;

    private readonly HashSet<string> _visited = [];

    private WildcardPattern[]? _exclusionPatterns;

    private const WildcardOptions WildcardPatternOptions = WildcardOptions.Compiled
        | WildcardOptions.CultureInvariant
        | WildcardOptions.IgnoreCase;

    protected const string DepthParameterSet = "Depth";

    protected const string RecursiveParameterSet = "Recursive";

    protected PrincipalContext? Context { get; set; }

    protected Stack<(GroupPrincipal? group, TreeGroup treeGroup)> Stack { get; } = new();

    internal TreeCache Cache { get; } = new();

    internal TreeBuilder Index { get; } = new();

    internal PSADTreeComparer Comparer { get; } = new();

    [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
    [Alias("DistinguishedName")]
    public string? Identity { get; set; }

    [Parameter]
    public string Server { get; set; } = Environment.UserDomainName;

    [Parameter]
    [Credential]
    public PSCredential? Credential { get; set; }

    [Parameter(ParameterSetName = DepthParameterSet)]
    [ValidateRange(0, int.MaxValue)]
    public int Depth { get; set; } = 3;

    [Parameter(ParameterSetName = RecursiveParameterSet)]
    public SwitchParameter Recursive { get; set; }

    [Parameter]
    public SwitchParameter ShowAll { get; set; }

    [Parameter]
    [SupportsWildcards]
    public string[]? Exclude { get; set; }

    protected override void BeginProcessing()
    {
        try
        {
            if (Recursive.IsPresent)
            {
                Depth = int.MaxValue;
            }

            if (Exclude is not null)
            {
                _exclusionPatterns = [.. Exclude.Select(e => new WildcardPattern(e, WildcardPatternOptions))];
            }

            if (Credential is null)
            {
                Context = new PrincipalContext(ContextType.Domain, Server);
                return;
            }

            Context = new PrincipalContext(
                ContextType.Domain,
                Server,
                Credential.UserName,
                Credential.GetNetworkCredential().Password);
        }
        catch (Exception exception)
        {
            ThrowTerminatingError(exception.ToSetPrincipalContext());
        }
    }

    protected bool IsNotVisited(TreeGroup group) => _visited.Add(group.DistinguishedName);

    protected override void ProcessRecord()
    {
        Index.Clear();
        _visited.Clear();
        _truncatedOutput = false;
    }

    protected void PushToStack(GroupPrincipal? groupPrincipal, TreeGroup treeGroup)
    {
        if (treeGroup.Depth > Depth)
        {
            return;
        }

        if (treeGroup.Depth == Depth)
        {
            _truncatedOutput = true;
        }

        Stack.Push((groupPrincipal, treeGroup));
    }

    protected TreeGroup ProcessGroup(
            TreeGroup parent,
            GroupPrincipal group,
            string source,
            int depth)
    {
        if (Cache.TryGet(group.DistinguishedName, out TreeGroup? treeGroup))
        {
            TreeGroup cloned = (TreeGroup)treeGroup.Clone(parent, depth);
            cloned.LinkCachedChildren(Cache);
            PushToStack(group, cloned);
            return treeGroup;
        }

        treeGroup = new TreeGroup(source, parent, group, depth);
        PushToStack(group, treeGroup);
        return treeGroup;
    }

    protected void DisplayWarningIfTruncatedOutput()
    {
        if (_truncatedOutput)
        {
            WriteWarning($"Result is truncated as enumeration has exceeded the set depth of {Depth}.");
        }
    }

    protected bool ShouldExclude(Principal principal) =>
        _exclusionPatterns?.Any(pattern => pattern.IsMatch(principal.SamAccountName)) ?? false;

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            Context?.Dispose();
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
