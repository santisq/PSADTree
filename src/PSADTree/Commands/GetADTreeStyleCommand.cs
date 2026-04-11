using System.Management.Automation;
using PSADTree.Style;

namespace PSADTree.Commands;

[Cmdlet(VerbsCommon.Get, "ADTreeStyle")]
[OutputType(typeof(TreeStyle))]
[Alias("treestyle")]
public sealed class GetADTreeStyleCommand : PSCmdlet
{
    protected override void BeginProcessing() => WriteObject(TreeStyle.Instance);
}
