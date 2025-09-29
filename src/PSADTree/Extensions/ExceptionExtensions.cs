using System;
using System.DirectoryServices.AccountManagement;
using System.Management.Automation;

namespace PSADTree.Extensions;

internal static class ExceptionExtensions
{
    internal static ErrorRecord ToIdentityNotFound(this string? identity) =>
        new(
            new NoMatchingPrincipalException($"Cannot find an object with identity: '{identity}'."),
            "IdentityNotFound",
            ErrorCategory.ObjectNotFound,
            identity);

    internal static ErrorRecord ToAmbiguousIdentity(this Exception exception, string? identity) =>
        new(exception, "AmbiguousIdentity", ErrorCategory.InvalidResult, identity);

    internal static ErrorRecord ToUnspecified(this Exception exception, string? identity) =>
        new(exception, "Unspecified", ErrorCategory.NotSpecified, identity);

    internal static ErrorRecord ToEnumerationFailure(this Exception exception, Principal? principal) =>
        new(exception, "EnumerationFailure", ErrorCategory.NotSpecified, principal);

    internal static ErrorRecord ToSetPrincipalContext(this Exception exception) =>
        new(exception, "SetPrincipalContext", ErrorCategory.ConnectionError, null);
}
