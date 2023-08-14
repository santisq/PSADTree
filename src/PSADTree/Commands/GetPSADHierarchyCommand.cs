using System.Diagnostics;
using System.Management.Automation;

namespace PSADTree;

[Cmdlet(VerbsCommon.Get, "PSADHierarchy")]
public sealed class GetPSADHierarchyCommand : PSCmdlet
{
    [Parameter(Mandatory = true, ValueFromPipeline = true)]
    public PSADIdentity? Identity { get; set; }

    protected override void ProcessRecord()
    {
        Debug.Assert(
            Identity is not null,
            "Mandatory Parameters can't be null.");

        WriteObject(Identity.GetEntry());
    }
}
