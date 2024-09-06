using System;
using System.DirectoryServices.AccountManagement;
using System.Management.Automation;

namespace PSADTree;

internal static class Exceptions
{
    internal static ErrorRecord IdentityNotFound(string? identity) =>
        new(
            new NoMatchingPrincipalException($"Cannot find an object with identity: '{identity}'."),
            "IdentityNotFound",
            ErrorCategory.ObjectNotFound,
            identity);

    internal static ErrorRecord CredentialRequiresServer() =>
        new(
            new ArgumentException("Server parameter is required when Credential parameter is used."),
            "CredentialRequiresServer",
            ErrorCategory.InvalidOperation,
            null);

    internal static ErrorRecord AmbiguousIdentity(this Exception exception, string? identity) =>
        new(exception, "AmbiguousIdentity", ErrorCategory.InvalidResult, identity);

    internal static ErrorRecord Unspecified(this Exception exception, string? identity) =>
        new(exception, "Unspecified", ErrorCategory.NotSpecified, identity);

    internal static ErrorRecord EnumerationFailure(this Exception exception, GroupPrincipal? groupPrincipal) =>
        new(exception, "EnumerationFailure", ErrorCategory.NotSpecified, groupPrincipal);

    internal static ErrorRecord SetPrincipalContext(this Exception exception) =>
        new(exception, "SetPrincipalContext", ErrorCategory.ConnectionError, null);
}
