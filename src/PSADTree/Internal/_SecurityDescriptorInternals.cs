using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace PSADTree.Internal;

#pragma warning disable IDE1006

[EditorBrowsable(EditorBrowsableState.Never)]
public static class _SecurityDescriptorInternals
{
    private readonly static Type _target = typeof(NTAccount);

    private readonly static ReadOnlyDictionary<string, MethodInfo> _propertyGetters;

    static _SecurityDescriptorInternals()
    {
        _propertyGetters = new(typeof(_SecurityDescriptorInternals)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .ToDictionary(prop => prop.Name, prop => prop));
    }

    private static ActiveDirectorySecurity GetBaseObject(PSObject target)
        => (ActiveDirectorySecurity)target.BaseObject;

    public static IdentityReference? GetOwner(PSObject target)
        => GetBaseObject(target).GetOwner(_target);

    public static IdentityReference? GetGroup(PSObject target)
        => GetBaseObject(target).GetGroup(_target);

    public static string GetSddlForm(PSObject target)
        => GetBaseObject(target).GetSecurityDescriptorSddlForm(AccessControlSections.All);

    public static AuthorizationRuleCollection GetAccessRules(PSObject target)
        => GetBaseObject(target).GetAccessRules(true, true, _target);

    public static string GetAccessToString(PSObject target)
    {
        StringBuilder builder = new();
        foreach (ActiveDirectoryAccessRule rule in GetAccessRules(target))
            builder.AppendLine($"{rule.IdentityReference} {rule.AccessControlType}");

        return builder.ToString();
    }

    internal static PSObject GetSecurityDescriptorAsPSObject(this DirectoryEntry entry)
    {
        return PSObject.AsPSObject(entry.ObjectSecurity)
            .AddProperty("Path", entry.Path)
            .AddCodeProperty("Owner", _propertyGetters["GetOwner"])
            .AddCodeProperty("Group", _propertyGetters["GetGroup"])
            .AddCodeProperty("Sddl", _propertyGetters["GetSddlForm"])
            .AddCodeProperty("Access", _propertyGetters["GetAccessRules"])
            .AddCodeProperty("AccessToString", _propertyGetters["GetAccessToString"]);
    }

    private static PSObject AddProperty(
        this PSObject psObject,
        string name,
        object? value)
    {
        psObject.Properties.Add(new PSNoteProperty(name, value), preValidated: true);
        return psObject;
    }

    private static PSObject AddCodeProperty(
        this PSObject psObject,
        string name,
        MethodInfo method)
    {
        psObject.Properties.Add(new PSCodeProperty(name, method), preValidated: true);
        return psObject;
    }
}
