using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Management.Automation;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace PSADTree.Extensions;

internal static class MiscExtensions
{
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
            if (!LdapMap.TryGetValue(property, out string? ldapDn))
            {
                ldapDn = property;
            }

            if (IsSecurityDescriptor(property))
            {
                additionalProperties[property] = entry.GetAcl();
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

    private static PSObject GetAcl(this DirectoryEntry entry)
    {
        Type target = typeof(NTAccount);
        ActiveDirectorySecurity acl = entry.ObjectSecurity;
        AuthorizationRuleCollection rules = acl.GetAccessRules(true, true, target);
        return PSObject.AsPSObject(acl)
            .AddProperty("Path", entry.Path)
            .AddProperty("Owner", acl.GetOwner(target))
            .AddProperty("Group", acl.GetGroup(target))
            .AddProperty("Sddl", acl.GetSecurityDescriptorSddlForm(AccessControlSections.All))
            .AddProperty("Access", rules)
            .AddProperty("AccessToString", rules.GetAccessToString());
    }

    private static PSObject AddProperty(this PSObject pSObject, string name, object? value)
    {
        pSObject.Properties.Add(new PSNoteProperty(name, value));
        return pSObject;
    }

    private static string GetAccessToString(this AuthorizationRuleCollection rules)
    {
        StringBuilder builder = new();
        foreach (ActiveDirectoryAccessRule rule in rules)
            builder.AppendLine($"{rule.IdentityReference} {rule.AccessControlType}");

        return builder.ToString();
    }

    private static bool IsIAdsLargeInteger(
        object value,
        [NotNullWhen(true)] out long? fileTime)
    {
        fileTime = default;
        if (value is not IAdsLargeInteger largeInt)
            return false;

        fileTime = (largeInt.HighPart << 32) + largeInt.LowPart;
        return true;
    }

    private static bool IsSecurityDescriptor(string ldapDn)
        => ldapDn.Equals("nTSecurityDescriptor", StringComparison.OrdinalIgnoreCase);

    internal static UserAccountControl? GetUserAccountControl(this AuthenticablePrincipal principal)
    {
        DirectoryEntry entry = principal.GetDirectoryEntry();
        return (UserAccountControl?)entry.Properties["userAccountControl"]?.Value;
    }
}
