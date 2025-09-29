using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Management.Automation;
using PSADTree.Extensions;

namespace PSADTree;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class PSADTreeCmdletBase : PSCmdlet, IDisposable
{
    private bool _disposed;

    private bool _truncatedOutput;

    private bool _canceled;

    private readonly HashSet<string> _visited = [];

    private WildcardPattern[]? _exclusionPatterns;

    private const WildcardOptions WildcardPatternOptions = WildcardOptions.Compiled
        | WildcardOptions.CultureInvariant
        | WildcardOptions.IgnoreCase;

    protected const string DepthParameterSet = "Depth";

    protected const string RecursiveParameterSet = "Recursive";

    protected PrincipalContext? Context { get; set; }

    protected Stack<(GroupPrincipal? group, TreeGroup treeObject)> Stack { get; } = new();

    internal TreeCache Cache { get; } = new();

    internal TreeBuilder Builder { get; } = new();

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

    protected override void ProcessRecord()
    {
        Builder.Clear();
        _visited.Clear();
        _truncatedOutput = false;

        try
        {
            using Principal? principal = GetFirstPrincipal();
            if (principal is null)
            {
                WriteError(Identity.ToIdentityNotFound());
                return;
            }

            HandleFirstPrincipal(principal);
            TreeObjectBase[] result = Traverse(principal.DistinguishedName);
            DisplayWarningIfTruncatedOutput();
            WriteObject(sendToPipeline: result, enumerateCollection: true);
        }
        catch (Exception _) when (_ is PipelineStoppedException or FlowControlException)
        {
            throw;
        }
        catch (MultipleMatchesException exception)
        {
            WriteError(exception.ToAmbiguousIdentity(Identity));
        }
        catch (Exception exception)
        {
            WriteError(exception.ToUnspecified(Identity));
        }
    }

    protected abstract Principal GetFirstPrincipal();

    protected abstract void HandleFirstPrincipal(Principal principal);

    private TreeObjectBase[] Traverse(string source)
    {
        int depth;
        while (Stack.Count > 0 && !_canceled)
        {
            (GroupPrincipal? current, TreeGroup treeGroup) = Stack.Pop();
            depth = treeGroup.Depth + 1;
            Builder.Add(treeGroup);

            // if this group is already cached
            if (!Cache.TryAdd(treeGroup))
            {
                current?.Dispose();
                treeGroup.LinkCachedChildren(Cache);
                // if it's a circular reference, nothing to do here
                if (treeGroup.SetIfCircularNested())
                {
                    continue;
                }

                // else, if we want to show all nodes OR this node was not yet visited
                if (ShowAll || _visited.Add(treeGroup.DistinguishedName))
                {
                    // reconstruct the output without querying AD again
                    BuildFromCache(treeGroup, source, depth);
                    continue;
                }

                // else, just skip this reference and go next
                treeGroup.SetProcessed();
                continue;
            }

            if (current is not null)
            {
                // else, group isn't cached so query AD
                BuildFromAD(treeGroup, current, source, depth);
                _visited.Add(treeGroup.DistinguishedName);
                Builder.CommitStaged();
                current.Dispose();
            }
        }

        return Builder.GetTree();
    }

    protected abstract void BuildFromAD(
        TreeGroup parent,
        GroupPrincipal groupPrincipal,
        string source,
        int depth);

    protected abstract void BuildFromCache(
        TreeGroup parent,
        string source,
        int depth);

    protected void PushToStack(
        TreeGroup treeGroup,
        GroupPrincipal? groupPrincipal = null)
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
            TreeGroup cloned = (TreeGroup)treeGroup.Clone(parent, source, depth);
            PushToStack(cloned, group);
            return treeGroup;
        }

        treeGroup = new TreeGroup(source, parent, group, depth);
        PushToStack(treeGroup, group);
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

    protected override void StopProcessing() => _canceled = true;

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
