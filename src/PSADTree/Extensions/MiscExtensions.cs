using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Management.Automation;

namespace PSADTree.Extensions;

internal static class MiscExtensions
{
    internal static T GetProperty<T>(
        this DirectoryEntry entry,
        string property)
        => LanguagePrimitives.ConvertTo<T>(entry.Properties[property][0]);

    internal static bool TryGetProperty<T>(
        this SearchResult search,
        string property,
        [NotNullWhen(true)] out T? value)
    {
        value = default;
        ResultPropertyValueCollection? toConvert = search.Properties[property];
        return toConvert is not null and { Count: > 0 }
            && LanguagePrimitives.TryConvertTo(toConvert, out value);
    }

    internal static DirectoryEntry GetDirectoryEntry(this Principal principal)
        => (DirectoryEntry)principal.GetUnderlyingObject();

    internal static ReadOnlyDictionary<string, object?>? GetAdditionalProperties(
        this Principal principal,
        string[] properties)
    {
        DirectoryEntry entry = principal.GetDirectoryEntry();

        if (properties.Length == 0)
            return null;

        if (properties.Any(e => e == "*"))
            return entry.GetAllAttributes();

        Dictionary<string, object?> additionalProperties = new(
            capacity: properties.Length,
            StringComparer.OrdinalIgnoreCase);

        foreach (string property in properties)
        {
            if (!LdapMap.Instance.TryGetValue(property, out string? ldapDn))
            {
                ldapDn = property;
            }

            if (IsSecurityDescriptor(property))
            {
                additionalProperties[property] = GetAcl(entry);
                continue;
            }

            if (entry.Properties.Contains(ldapDn))
            {
                additionalProperties[property] = entry.Properties[ldapDn].Value;
            }
        }

        return additionalProperties is { Count: 0 } ? null : new(additionalProperties);
    }

    private static ReadOnlyDictionary<string, object?> GetAllAttributes(this DirectoryEntry entry)
    {
        Dictionary<string, object?> additionalProperties = new(
            capacity: entry.Properties.Count,
            StringComparer.OrdinalIgnoreCase);

        foreach (string property in entry.Properties.PropertyNames)
        {
            if (IsSecurityDescriptor(property))
            {
                additionalProperties[property] = entry.GetAcl();
                continue;
            }

            additionalProperties[property] = entry.Properties[property].Value;
        }

        return new(additionalProperties);
    }

    private static ActiveDirectorySecurity? GetAcl(this DirectoryEntry entry)
    {
        using DirectorySearcher searcher = new(entry, null, ["nTSecurityDescriptor"])
        {
            SecurityMasks = SecurityMasks.Group | SecurityMasks.Owner | SecurityMasks.Dacl
        };

        SearchResult? result = searcher.FindOne();

        if (result is null || !result.TryGetProperty("nTSecurityDescriptor", out byte[]? descriptor))
            return null;

        ActiveDirectorySecurity acl = new();
        acl.SetSecurityDescriptorBinaryForm(descriptor);
        return acl;
    }

    private static bool IsSecurityDescriptor(string ldapDn)
        => ldapDn.Equals("nTSecurityDescriptor", StringComparison.OrdinalIgnoreCase);
}
