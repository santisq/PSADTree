using System;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
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
        if (!search.Properties.Contains(property))
        {
            return false;
        }

        return LanguagePrimitives.TryConvertTo(search.Properties[property][0], out value);
    }

    internal static DirectoryEntry GetDirectoryEntry(this Principal principal)
        => (DirectoryEntry)principal.GetUnderlyingObject();
}
