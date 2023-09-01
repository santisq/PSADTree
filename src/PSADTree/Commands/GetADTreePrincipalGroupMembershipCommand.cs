using System;
using System.DirectoryServices.AccountManagement;
using System.Management.Automation;

namespace PSADTree;

[Cmdlet(VerbsCommon.Get, "ADTreePrincipalGroupMembership")]
[Alias("treeprincipalmembership")]
[OutputType(
    typeof(TreeGroup),
    typeof(TreeUser),
    typeof(TreeComputer))]
public sealed class GetADTreePrincipalGroupMembershipCommand : PSADTreeCmdletBase
{
    protected override void ProcessRecord()
    {
        Dbg.Assert(Identity is not null);
        Dbg.Assert(_context is not null);

        try
        {
            using Principal? principal = Principal.FindByIdentity(_context, Identity);
            if (principal is null)
            {
                WriteError(ErrorHelper.IdentityNotFound(Identity));
                return;
            }

            // WriteObject(
            //     sendToPipeline: Traverse(
            //         groupPrincipal: group,
            //         source: group.DistinguishedName),
            //     enumerateCollection: true);
        }
        catch (Exception e) when (e is PipelineStoppedException or FlowControlException)
        {
            throw;
        }
        catch (MultipleMatchesException e)
        {
            WriteError(ErrorHelper.AmbiguousIdentity(Identity, e));
        }
        catch (Exception e)
        {
            WriteError(ErrorHelper.Unspecified(Identity, e));
        }
    }

    private object Traverse(GroupPrincipal groupPrincipal, string source)
    {
        throw new NotImplementedException();
    }
}
