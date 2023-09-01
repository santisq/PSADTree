using System;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;

namespace PSADTree;

public abstract class TreeObjectBase
{
    internal int Depth { get; set; }

    internal string Source { get; }

    public TreeGroup? Parent { get; }

    public string Domain { get; }

    public string SamAccountName { get; }

    public string ObjectClass { get; }

    public string Hierarchy { get; internal set; }

    public string DistinguishedName { get; }

    public Guid? ObjectGuid { get; }

    public SecurityIdentifier ObjectSid { get; }

    protected TreeObjectBase(
        TreeObjectBase treeObject,
        TreeGroup? parent,
        int depth)
    {
        Depth = depth;
        Source = treeObject.Source;
        SamAccountName = treeObject.SamAccountName;
        Domain = treeObject.Domain;
        ObjectClass = treeObject.ObjectClass;
        DistinguishedName = treeObject.DistinguishedName;
        ObjectGuid = treeObject.ObjectGuid;
        ObjectSid = treeObject.ObjectSid;
        Hierarchy = treeObject.SamAccountName.Indent(depth);
        Parent = parent;
    }

    protected TreeObjectBase(
        string source,
        Principal principal)
    {
        Source = source;
        SamAccountName = principal.SamAccountName;
        Domain = principal.DistinguishedName.GetDefaultNamingContext();
        ObjectClass = principal.StructuralObjectClass;
        DistinguishedName = principal.DistinguishedName;
        ObjectGuid = principal.Guid;
        ObjectSid = principal.Sid;
        Hierarchy = SamAccountName;
    }

    protected TreeObjectBase(
        string source,
        TreeGroup? parent,
        Principal principal,
        int depth)
    {
        Depth = depth;
        Source = source;
        Domain = principal.DistinguishedName.GetDefaultNamingContext();
        SamAccountName = principal.SamAccountName;
        ObjectClass = principal.StructuralObjectClass;
        DistinguishedName = principal.DistinguishedName;
        ObjectGuid = principal.Guid;
        ObjectSid = principal.Sid;
        Hierarchy = principal.SamAccountName.Indent(depth);
        Parent = parent;
    }

    public override string ToString() => DistinguishedName;

    internal abstract TreeObjectBase Clone(TreeGroup parent, int depth);
}
