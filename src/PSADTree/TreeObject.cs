using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;

namespace PSADTree;

public sealed class TreeObject
{
    private List<TreeObject>? _parent;

    private List<TreeObject>? _member;

    public TreeObject[] Parent =>
        _parent is not null
        ? _parent.ToArray()
        : Array.Empty<TreeObject>();

    public TreeObject[] Member =>
        _member is not null
        ? _member.ToArray()
        : Array.Empty<TreeObject>();

    internal string Source { get; }

    internal int Depth { get; }

    public string SamAccountName { get; }

    public string ObjectClass { get; }

    public string Hierarchy { get; internal set; }

    public string DistinguishedName { get; }

    public Guid? ObjectGuid { get; }

    public SecurityIdentifier ObjectSid { get; }

    private TreeObject(TreeObject treeObject, int depth)
    {
        Depth = treeObject.Depth;
        Source = treeObject.Source;
        SamAccountName = treeObject.SamAccountName;
        ObjectClass = treeObject.ObjectClass;
        DistinguishedName = treeObject.DistinguishedName;
        ObjectGuid = treeObject.ObjectGuid;
        ObjectSid = treeObject.ObjectSid;
        Hierarchy = treeObject.SamAccountName.Indent(depth);
        Hook(treeObject);
    }

    internal TreeObject(
        string source,
        Principal principal,
        int depth)
    {
        Depth = depth;
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

    internal void AddParent(TreeObject parent)
    {
        _parent ??= new();
        _parent.Add(parent);
    }

    internal void AddMember(TreeObject member)
    {
        _member ??= new();
        _member.Add(member);
    }

    public override string ToString() => DistinguishedName;

    internal TreeObject Copy(int depth) => new(this, depth);

    internal void Hook(TreeObject treeObject)
    {
        _member = treeObject._member;
        _parent = treeObject._parent;
    }
}
