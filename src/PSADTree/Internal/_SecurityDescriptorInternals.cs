using System;
using System.ComponentModel;
using System.DirectoryServices;
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
    private readonly static Type s_target = typeof(NTAccount);
    private static readonly MethodInfo s_getOwner;
    private static readonly MethodInfo s_getGroup;
    private static readonly MethodInfo s_getSddlForm;
    private static readonly MethodInfo s_getAccessRules;
    private static readonly MethodInfo s_getAccessToString;

    static _SecurityDescriptorInternals()
    {
        Type type = typeof(_SecurityDescriptorInternals);
        s_getOwner = GetMethod(type, nameof(GetOwner));
        s_getGroup = GetMethod(type, nameof(GetGroup));
        s_getSddlForm = GetMethod(type, nameof(GetSddlForm));
        s_getAccessRules = GetMethod(type, nameof(GetAccessRules));
        s_getAccessToString = GetMethod(type, nameof(GetAccessToString));
    }

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
            builder.AppendLine($"{rule.IdentityReference} {rule.AccessControlType}");

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
