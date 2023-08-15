using System.Diagnostics;
using System.Management.Automation;

namespace PSADTree;

[Cmdlet(VerbsCommon.Get, "PSADHierarchy")]
public sealed class GetPSADHierarchyCommand : PSCmdlet
{
    [Parameter(Mandatory = true, ValueFromPipeline = true)]
    public Identity? Identity { get; set; }

    protected override void ProcessRecord()
    {
        Dbg.Assert(Identity is not null);
        WriteObject(Identity.GetEntry());
    }
}
