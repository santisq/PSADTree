using System;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;

namespace PSADTree;

public sealed class TreeObject
{
    internal string Source { get; }

    internal int Depth { get; }

    public string SamAccountName { get; }

    public string ObjectClass { get; }

    public string DistinguishedName { get; }

    public Guid? ObjectGuid { get; }

    public SecurityIdentifier ObjectSid { get; }

    public string Hierarchy { get; internal set; }

    internal TreeObject(
        string source,
        Principal principal,
        int depth)
    {
        Source = source;
        SamAccountName = principal.SamAccountName;
        ObjectClass = principal.StructuralObjectClass;
        DistinguishedName = principal.DistinguishedName;
        ObjectGuid = principal.Guid;
        ObjectSid = principal.Sid;
        Hierarchy = SamAccountName.Indent(depth);
    }

    internal TreeObject(
        string source,
        Principal principal)
    {
        Source = source;
        SamAccountName = principal.SamAccountName;
        ObjectClass = principal.StructuralObjectClass;
        DistinguishedName = principal.DistinguishedName;
        ObjectGuid = principal.Guid;
        ObjectSid = principal.Sid;
        Hierarchy = SamAccountName;
    }

    public override string ToString() => DistinguishedName;
}
