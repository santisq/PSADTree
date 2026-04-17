using System;
using System.DirectoryServices.AccountManagement;
using System.Management.Automation;

namespace PSADTree.Extensions;

internal static class ExceptionExtensions
{
    extension(string? identity)
    {
        internal ErrorRecord ToIdentityNotFound() =>
            new(
                new NoMatchingPrincipalException($"Cannot find an object with identity: '{identity}'."),
                "IdentityNotFound",
                ErrorCategory.ObjectNotFound,
                identity);
    }

    extension(Exception exception)
    {
        internal ErrorRecord ToAmbiguousIdentity(string? identity) =>
            new(exception, "AmbiguousIdentity", ErrorCategory.InvalidResult, identity);

        internal ErrorRecord ToUnspecified(string? identity) =>
            new(exception, "Unspecified", ErrorCategory.NotSpecified, identity);

        internal ErrorRecord ToEnumerationFailure(Principal? principal) =>
            new(exception, "EnumerationFailure", ErrorCategory.NotSpecified, principal);

        internal ErrorRecord ToSetPrincipalContext() =>
            new(exception, "SetPrincipalContext", ErrorCategory.ConnectionError, null);
    }

    extension(string vt)
    {
        internal void ThrowInvalidSequence() => throw new ArgumentException(
            $"The specified string contains printable content when it should only contain ANSI escape sequences: '{vt}'.");
    }
}
