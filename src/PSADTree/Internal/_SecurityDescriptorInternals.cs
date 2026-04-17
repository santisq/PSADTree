using System;
using System.ComponentModel;
using System.DirectoryServices;
using System.Globalization;
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
    private static readonly Type s_type = typeof(_SecurityDescriptorInternals);
    private static readonly Type s_target = typeof(NTAccount);
    private static readonly MethodInfo s_getOwner = GetMethod(s_type, nameof(GetOwner));
    private static readonly MethodInfo s_getGroup = GetMethod(s_type, nameof(GetGroup));
    private static readonly MethodInfo s_getSddlForm = GetMethod(s_type, nameof(GetSddlForm));
    private static readonly MethodInfo s_getAccessRules = GetMethod(s_type, nameof(GetAccessRules));
    private static readonly MethodInfo s_getAccessToString = GetMethod(s_type, nameof(GetAccessToString));

    private static MethodInfo GetMethod(Type type, string name) =>
        type.GetMethod(name)
            ?? throw new InvalidOperationException(
                $"Method '{name}' not found on type '{type.FullName}'.");

    private static ActiveDirectorySecurity GetBaseObject(PSObject target)
        => (ActiveDirectorySecurity)target.BaseObject;

    public static IdentityReference? GetOwner(PSObject target)
        => GetBaseObject(target).GetOwner(s_target);

    public static IdentityReference? GetGroup(PSObject target)
        => GetBaseObject(target).GetGroup(s_target);

    public static string GetSddlForm(PSObject target)
        => GetBaseObject(target).GetSecurityDescriptorSddlForm(AccessControlSections.All);

    public static AuthorizationRuleCollection GetAccessRules(PSObject target)
        => GetBaseObject(target).GetAccessRules(true, true, s_target);

    public static string GetAccessToString(PSObject target)
    {
        StringBuilder builder = new();
        foreach (ActiveDirectoryAccessRule rule in GetAccessRules(target))
        {
#if NET8_0_OR_GREATER
            builder.AppendLine(CultureInfo.InvariantCulture, $"{rule.IdentityReference} {rule.AccessControlType}");
#else
            builder.AppendLine($"{rule.IdentityReference} {rule.AccessControlType}");
#endif
        }

        return builder.ToString();
    }

    internal static PSObject GetSecurityDescriptorAsPSObject(this DirectoryEntry entry)
        => PSObject
            .AsPSObject(entry.ObjectSecurity)
            .AddProperty("Path", entry.Path)
            .AddCodeProperty("Owner", s_getOwner)
            .AddCodeProperty("Group", s_getGroup)
            .AddCodeProperty("Sddl", s_getSddlForm)
            .AddCodeProperty("Access", s_getAccessRules)
            .AddCodeProperty("AccessToString", s_getAccessToString);

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
