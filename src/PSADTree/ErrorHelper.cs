using System;
using System.DirectoryServices.AccountManagement;
using System.Management.Automation;

namespace PSADTree;

internal static class ErrorHelper
{
    internal static ErrorRecord IdentityNotFound(string? identity) =>
        new(
            new NoMatchingPrincipalException($"Cannot find an object with identity: '{identity}'."),
            "IdentityNotFound",
            ErrorCategory.ObjectNotFound,
            identity);

    internal static ErrorRecord AmbiguousIdentity(string? identity, Exception exception) =>
        new(exception, "AmbiguousIdentity", ErrorCategory.InvalidResult, identity);

    internal static ErrorRecord Unspecified(string? identity, Exception exception) =>
        new(exception, "Unspecified", ErrorCategory.NotSpecified, identity);

    internal static ErrorRecord EnumerationFailure(
        GroupPrincipal? groupPrincipal,
        Exception exception) =>
        new(exception, "EnumerationFailure", ErrorCategory.NotSpecified, groupPrincipal);
}
