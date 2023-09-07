---
external help file: PSADTree.dll-Help.xml
Module Name: PSADTree
online version:
schema: 2.0.0
---

# Get-ADTreePrincipalGroupMembership

## SYNOPSIS

`tree` like cmdlet for Active Directory Principals Group Membership.

## SYNTAX

### Depth (Default)

```powershell
Get-ADTreePrincipalGroupMembership [-Identity] <String> [-Server <String>] [-Depth <Int32>] [-ShowAll]
 [<CommonParameters>]
```

### Recursive

```powershell
Get-ADTreePrincipalGroupMembership [-Identity] <String> [-Server <String>] [-Recursive] [-ShowAll]
 [<CommonParameters>]
```

## DESCRIPTION

The `Get-ADTreePrincipalGroupMembership` cmdlet gets the Active Directory groups that have a specified user, computer, group, or service account as a member and displays them in a tree like structure. This cmdlet also helps identifying Circular Nested Groups.

## EXAMPLES

### Example 1: Get group memberships for a user

```powershell
PS ..\PSADTree\> Get-ADTreePrincipalGroupMembership john.doe
```

By default, this cmdlet uses `-Depth` with a default value of `3`.

### Example 2: Get the recursive group memberships for a user

```powershell
PS ..\PSADTree\> Get-ADTreePrincipalGroupMembership john.doe -Recursive
```

### Example 3: Get group memberships for all computers under an Organizational Unit

```powershell
PS ..\PSADTree\> Get-ADComputer -Filter * -SearchBase 'OU=myOU,DC=myDomain,DC=com' |
    Get-ADTreePrincipalGroupMembership
```

You can pipe strings containing an identity to this cmdlet. [`ADObject`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.activedirectory.management.adobject?view=activedirectory-management-10.0) instances piped to this cmdlet are also supported.

### Example 4: Find any Circular Nested Groups from previous example

```powershell
PS ..\PSADTree\> Get-ADComputer -Filter * -SearchBase 'OU=myOU,DC=myDomain,DC=com' |
    Get-ADTreePrincipalGroupMembership -Recursive |
    Where-Object IsCircular
```

### Example 5: Get group memberships for a user in a different Domain

```powershell
PS ..\PSADTree\> Get-ADTreePrincipalGroupMembership john.doe -Server otherDomain
```

### Example 6: Get group memberships for a user, including processed groups

```powershell
PS ..\PSADTree\> Get-ADTreePrincipalGroupMembership john.doe -ShowAll
```

By default, previously processed groups will be marked as _"Processed Group"_ and their hierarchy will not be displayed.
The `-ShowAll` switch determines that, the hierarchy of previously processed groups should be displayed for the specified principal.

__NOTE:__ The use of this switch should not infer in a great performance cost, for more details see the parameter details.

## PARAMETERS

### -Depth

Determines the number of group membership levels that are included in the recursion.
By default, only 3 levels of recursion are included in the default view.

```yaml
Type: Int32
Parameter Sets: Depth
Aliases:

Required: False
Position: Named
Default value: 3
Accept pipeline input: False
Accept wildcard characters: False
```

### -Identity

Specifies an Active Directory principal by providing one of the following property values:

- A DistinguishedName
- A GUID
- A SID (Security Identifier)
- A sAMAccountName
- A UserPrincipalName

See [`IdentityType` Enum](https://learn.microsoft.com/en-us/dotnet/api/system.directoryservices.accountmanagement.identitytype?view=dotnet-plat-ext-7.0) for more information.

```yaml
Type: String
Parameter Sets: (All)
Aliases: DistinguishedName

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -Recursive

Specifies that the cmdlet should get all group membership of a principal.

```yaml
Type: SwitchParameter
Parameter Sets: Recursive
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Server

Specifies the AD DS instance to connect to by providing one of the following values for a corresponding domain name or directory server.

Domain name values:

- Fully qualified domain name
- NetBIOS name

Directory server values:

- Fully qualified directory server name
- NetBIOS name
- Fully qualified directory server name and port

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ShowAll

By default, previously processed groups will be marked as _"Processed Group"_ and their hierarchy will not be displayed.
This switch determines that this cmdlet should display the hierarchy of previously processed groups for the specified principal.

> __NOTE:__ This cmdlet uses a caching mechanism to ensure that Active Directory Groups are queried only once per Identity.
This chaching mechanism is also used to reconstruct the pre-processed group's hierarchy when the `-ShowAll` switch is used, thus not infering in a great performance cost.
The intent behind this switch is as to not clotter the cmdlet's output by default.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String

You can pipe strings containing an identity to this cmdlet. [`ADObject`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.activedirectory.management.adobject?view=activedirectory-management-10.0) instances piped to this cmdlet are also supported.

## OUTPUTS

### PSADTree.TreeGroup

### PSADTree.TreeUser

### PSADTree.TreeComputer

## NOTES

## RELATED LINKS
