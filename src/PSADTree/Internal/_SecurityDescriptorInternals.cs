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
    private readonly static Type _target = typeof(NTAccount);
    private static readonly MethodInfo _getOwner;
    private static readonly MethodInfo _getGroup;
    private static readonly MethodInfo _getSddlForm;
    private static readonly MethodInfo _getAccessRules;
    private static readonly MethodInfo _getAccessToString;

    static _SecurityDescriptorInternals()
    {
        Type type = typeof(_SecurityDescriptorInternals);
        _getOwner = type.GetMethod(nameof(GetOwner))!;
        _getGroup = type.GetMethod(nameof(GetGroup))!;
        _getSddlForm = type.GetMethod(nameof(GetSddlForm))!;
        _getAccessRules = type.GetMethod(nameof(GetAccessRules))!;
        _getAccessToString = type.GetMethod(nameof(GetAccessToString))!;
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
        => PSObject.AsPSObject(entry.ObjectSecurity)
                .AddProperty("Path", entry.Path)
                .AddCodeProperty("Owner", _getOwner)
                .AddCodeProperty("Group", _getGroup)
                .AddCodeProperty("Sddl", _getSddlForm)
                .AddCodeProperty("Access", _getAccessRules)
                .AddCodeProperty("AccessToString", _getAccessToString);

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
