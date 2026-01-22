using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using PSADTree.Extensions;

namespace PSADTree;

public abstract class TreeObjectBase
{
    public int Depth { get; }

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

    public ReadOnlyDictionary<string, object?>? AdditionalProperties { get; }

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

    protected TreeObjectBase(string source, Principal principal, string[]? properties)
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
        AdditionalProperties = GetAdditionalProperties(principal.GetDirectoryEntry(), properties);
    }

    protected TreeObjectBase(
        string source,
        TreeGroup? parent,
        Principal principal,
        string[]? properties,
        int depth)
        : this(source, principal, properties)
    {
        Depth = depth;
        Hierarchy = principal.SamAccountName.Indent(depth);
        Parent = parent;
    }

    private ReadOnlyDictionary<string, object?>? GetAdditionalProperties(
        DirectoryEntry entry,
        string[]? Properties)
    {
        if (Properties is null or { Length: 0 })
        {
            return null;
        }

        Dictionary<string, object?> additionalProperties = [];

        foreach (string property in Properties)
        {
            if (property.Equals("nTSecurityDescriptor", StringComparison.OrdinalIgnoreCase))
            {
                additionalProperties[property] = GetAcl(entry);
                continue;
            }

            if (entry.Properties.Contains(property))
            {
                additionalProperties[property] = entry.Properties[property][0];
            }

        }

        return new ReadOnlyDictionary<string, object?>(additionalProperties);
    }

    private static ActiveDirectorySecurity? GetAcl(DirectoryEntry entry)
    {
        using DirectorySearcher searcher = new(entry, null, ["nTSecurityDescriptor"])
        {
            SecurityMasks = SecurityMasks.Group | SecurityMasks.Owner | SecurityMasks.Dacl
        };

        SearchResult? result = searcher.FindOne();

        if (result is null)
        {
            return null;
        }

        if (!result.TryGetProperty("nTSecurityDescriptor", out byte[]? descriptor))
        {
            return null;
        }

        ActiveDirectorySecurity acl = new();
        acl.SetSecurityDescriptorBinaryForm(descriptor);
        return acl;
    }

    public override string ToString() => DistinguishedName;

    internal abstract TreeObjectBase Clone(TreeGroup parent, string source, int depth);
}
