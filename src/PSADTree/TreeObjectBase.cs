using System;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;

namespace PSADTree;

public abstract class TreeObjectBase
{
    internal int Depth { get; }

    internal string Source { get; }

    public TreeGroup? Parent { get; }

    public string Domain { get; }

    public string SamAccountName { get; }

    public string ObjectClass { get; }

    public string Hierarchy { get; internal set; }

    public string DistinguishedName { get; }

    public Guid? ObjectGuid { get; }

    public string UserPrincipalName { get; }

    public string Description { get; }

    public string DisplayName { get; }

    public SecurityIdentifier ObjectSid { get; }

    protected TreeObjectBase(
        TreeObjectBase treeObject,
        TreeGroup? parent,
        string source,
        int depth)
    {
        Depth = depth;
        Source = source;
        SamAccountName = treeObject.SamAccountName;
        Domain = treeObject.Domain;
        ObjectClass = treeObject.ObjectClass;
        DistinguishedName = treeObject.DistinguishedName;
        ObjectGuid = treeObject.ObjectGuid;
        ObjectSid = treeObject.ObjectSid;
        Hierarchy = treeObject.SamAccountName.Indent(depth);
        Parent = parent;
        UserPrincipalName = treeObject.UserPrincipalName;
        Description = treeObject.Description;
        DisplayName = treeObject.DisplayName;
    }

    protected TreeObjectBase(string source, Principal principal)
    {
        Source = source;
        SamAccountName = principal.SamAccountName;
        Domain = principal.DistinguishedName.GetDefaultNamingContext();
        ObjectClass = principal.StructuralObjectClass;
        DistinguishedName = principal.DistinguishedName;
        ObjectGuid = principal.Guid;
        ObjectSid = principal.Sid;
        Hierarchy = SamAccountName;
        UserPrincipalName = principal.UserPrincipalName;
        Description = principal.Description;
        DisplayName = principal.DisplayName;
    }

    protected TreeObjectBase(
        string source,
        TreeGroup? parent,
        Principal principal,
        int depth)
        : this(source, principal)
    {
        Depth = depth;
        Hierarchy = principal.SamAccountName.Indent(depth);
        Parent = parent;
    }

    public override string ToString() => DistinguishedName;

    internal abstract TreeObjectBase Clone(TreeGroup parent, string source, int depth);
}
