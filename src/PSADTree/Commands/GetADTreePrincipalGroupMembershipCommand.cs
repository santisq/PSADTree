using System;
using System.DirectoryServices.AccountManagement;
using System.Linq;
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
        Principal? principal;
        Clear();

        try
        {
            principal = Principal.FindByIdentity(_context, Identity);
        }
        catch (Exception e) when (e is PipelineStoppedException or FlowControlException)
        {
            throw;
        }
        catch (MultipleMatchesException e)
        {
            WriteError(ErrorHelper.AmbiguousIdentity(Identity, e));
            return;
        }
        catch (Exception e)
        {
            WriteError(ErrorHelper.Unspecified(Identity, e));
            return;
        }

        if (principal is null)
        {
            WriteError(ErrorHelper.IdentityNotFound(Identity));
            return;
        }

        string source = principal.DistinguishedName;
        switch (principal)
        {
            case UserPrincipal user:
                _index.Add(new TreeUser(source, null, user, 0));
                break;

            case GroupPrincipal group:
                TreeGroup treeGroup = new(source, group);
                _index.Add(treeGroup);
                _cache.Add(treeGroup);
                break;

            case ComputerPrincipal computer:
                _index.Add(new TreeComputer(source, null, computer, 0));
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(principal));
        }

        try
        {
            using PrincipalSearchResult<Principal> search = principal.GetGroups();
            foreach (GroupPrincipal parent in search.Cast<GroupPrincipal>())
            {
                TreeGroup treeGroup = new(source, null, parent, 1);
                _cache.Add(treeGroup);
                Push(parent, treeGroup);
            }
        }
        catch (Exception e) when (e is PipelineStoppedException or FlowControlException)
        {
            throw;
        }
        catch (Exception e)
        {
            WriteError(ErrorHelper.EnumerationFailure(null, e));
        }
        finally
        {
            principal?.Dispose();
        }

        WriteObject(
            sendToPipeline: Traverse(source),
            enumerateCollection: true);
    }

    private object? Traverse(string source)
    {
        int depth = 1;
        return default;
    }
}
