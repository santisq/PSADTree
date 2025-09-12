using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Management.Automation;

namespace PSADTree;

public abstract class PSADTreeCmdletBase : PSCmdlet, IDisposable
{
    protected const string DepthParameterSet = "Depth";

    protected const string RecursiveParameterSet = "Recursive";

    protected PrincipalContext? _context;

    private bool _disposed;

    protected bool TruncatedOutput { get; set; }

    protected Stack<(GroupPrincipal? group, TreeGroup treeGroup)> Stack { get; } = new();

    internal TreeCache Cache { get; } = new();

    internal TreeIndex Index { get; } = new();

    internal PSADTreeComparer Comparer { get; } = new();

    protected WildcardPattern[]? ExclusionPatterns { get; set; }

    private const WildcardOptions WildcardPatternOptions = WildcardOptions.Compiled
        | WildcardOptions.CultureInvariant
        | WildcardOptions.IgnoreCase;

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
                ExclusionPatterns = [.. Exclude.Select(e => new WildcardPattern(e, WildcardPatternOptions))];
            }

            if (Credential is null)
            {
                _context = new PrincipalContext(ContextType.Domain, Server);
                return;
            }

            _context = new PrincipalContext(
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

    protected void Push(GroupPrincipal? groupPrincipal, TreeGroup treeGroup)
    {
        if (treeGroup.Depth > Depth)
        {
            return;
        }

        if (treeGroup.Depth == Depth)
        {
            TruncatedOutput = true;
        }

        Stack.Push((groupPrincipal, treeGroup));
    }

    protected void DisplayWarningIfTruncatedOutput()
    {
        if (TruncatedOutput)
        {
            WriteWarning($"Result is truncated as enumeration has exceeded the set depth of {Depth}.");
        }
    }

    private static bool MatchAny(
        Principal principal,
        WildcardPattern[] patterns)
    {
        foreach (WildcardPattern pattern in patterns)
        {
            if (pattern.IsMatch(principal.SamAccountName))
            {
                return true;
            }
        }

        return false;
    }

    protected static bool ShouldExclude(
        Principal principal,
        WildcardPattern[]? patterns)
    {
        if (patterns is null)
        {
            return false;
        }

        return MatchAny(principal, patterns);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _context?.Dispose();
            _disposed = true;
        }
    }

    // protected void Clear()
    // {
    //     Index.Clear();
    //     Cache.Clear();
    // }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
