using System;
using System.DirectoryServices.AccountManagement;
using System.Management.Automation;

namespace PSADTree;

[Cmdlet(VerbsCommon.Get, "PSADHierarchy")]
public sealed class GetPSADHierarchyCommand : PSCmdlet, IDisposable
{
    private PrincipalContext? _context;

    [Parameter(
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
                e, "SetContext", ErrorCategory.ConnectionError, null));
        }
    }

    protected override void ProcessRecord()
    {
        Dbg.Assert(Identity is not null);
        Dbg.Assert(_context is not null);

        using Principal principal = Principal.FindByIdentity(_context, Identity);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
