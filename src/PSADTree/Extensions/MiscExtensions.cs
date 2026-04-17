using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Linq;
using PSADTree.Internal;

namespace PSADTree.Extensions;

internal static class MiscExtensions
{
    extension(Principal principal)
    {
        internal DirectoryEntry GetDirectoryEntry() => (DirectoryEntry)principal.GetUnderlyingObject();

        internal ReadOnlyDictionary<string, object?>? GetAdditionalProperties(string[] properties)
        {
            if (properties.Length == 0)
                return null;

            DirectoryEntry entry = principal.GetDirectoryEntry();

            if (properties.Any(e => e == "*"))
                return GetAllAttributes(entry);

            Dictionary<string, object?> additionalProperties = new(
                capacity: properties.Length,
                StringComparer.OrdinalIgnoreCase);

            foreach (string property in properties)
            {
                // already processed
                if (additionalProperties.ContainsKey(property))
                    continue;

                if (!LdapMap.TryGetValue(property, out string? ldapDn))
                {
                    ldapDn = property;
                }

                if (IsSecurityDescriptor(property))
                {
                    additionalProperties[property] = entry.GetSecurityDescriptorAsPSObject();
                    continue;
                }

                object? value = entry.Properties[ldapDn]?.Value;

                if (value is null) continue;
                if (IsIAdsLargeInteger(value, out long? fileTime))
                {
                    additionalProperties[property] = fileTime;
                    continue;
                }

                additionalProperties[property] = value;
            }

            return additionalProperties is { Count: 0 } ? null : new(additionalProperties);
        }
    }

    extension(AuthenticablePrincipal principal)
    {
        internal UserAccountControl? GetUserAccountControl()
        {
            DirectoryEntry entry = principal.GetDirectoryEntry();
            object? uac = entry.Properties["userAccountControl"]?.Value;
            if (uac is null) return null;
            return (UserAccountControl)Convert.ToUInt32(uac, CultureInfo.InvariantCulture);
        }
    }

    extension(UserAccountControl uac)
    {
        internal bool IsEnabled() => !uac.HasFlag(UserAccountControl.ACCOUNTDISABLE);
    }

    private static ReadOnlyDictionary<string, object?> GetAllAttributes(DirectoryEntry entry)
    {
        Dictionary<string, object?> additionalProperties = new(
            capacity: entry.Properties.Count,
            StringComparer.OrdinalIgnoreCase);

        foreach (string property in entry.Properties.PropertyNames)
        {
            if (IsSecurityDescriptor(property))
            {
                additionalProperties[property] = entry.GetSecurityDescriptorAsPSObject();
                continue;
            }

            object? value = entry.Properties[property]?.Value;

            if (value is null) continue;

            if (IsIAdsLargeInteger(value, out long? fileTime))
            {
                additionalProperties[property] = fileTime;
                continue;
            }

            additionalProperties[property] = value;
        }

        return new(additionalProperties);
    }

    private static bool IsIAdsLargeInteger(
        object value,
        [NotNullWhen(true)] out long? fileTime)
    {
        fileTime = default;
        if (value is not IAdsLargeInteger largeInt) return false;

        fileTime = (largeInt.HighPart << 32) + largeInt.LowPart;
        return true;
    }

    private static bool IsSecurityDescriptor(string ldapDn)
        => ldapDn.Equals("nTSecurityDescriptor", StringComparison.OrdinalIgnoreCase);
}
