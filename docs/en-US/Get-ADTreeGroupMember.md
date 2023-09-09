---
external help file: PSADTree.dll-Help.xml
Module Name: PSADTree
online version:
schema: 2.0.0
---

# Get-ADTreeGroupMember

## SYNOPSIS

`tree` like cmdlet for Active Directory group members.

## SYNTAX

### Depth (Default)

```powershell
Get-ADTreeGroupMember [-Group] [-Identity] <String> [-Server <String>] [-Depth <UInt32>] [-ShowAll]
 [<CommonParameters>]
```

### Recursive

```powershell
Get-ADTreeGroupMember [-Group] [-Identity] <String> [-Server <String>] [-Recursive] [-ShowAll]
 [<CommonParameters>]
```

## DESCRIPTION

The `Get-ADTreeGroupMember` cmdlet gets the Active Directory members of a specified group and displays them in a tree like structure. The members of a group can be users, groups, computers and service accounts. This cmdlet also helps identifying Circular Nested Groups.

## EXAMPLES

### Example 1: Get the members of a group

```powershell
PS ..\PSADTree\> Get-ADTreeGroupMember TestGroup001
```

By default, this cmdlet uses `-Depth` with a default value of `3`.

### Example 2: Get the members of a group recursively

```powershell
PS ..\PSADTree\> Get-ADTreeGroupMember TestGroup001 -Recursive
```

### Example 3: Get the members of all groups under an Organizational Unit

```powershell
PS ..\PSADTree\> Get-ADGroup -Filter * -SearchBase 'OU=myOU,DC=myDomain,DC=com' |
    Get-ADTreeGroupMember
```

You can pipe strings containing an identity to this cmdlet. [__`ADGroup`__](https://learn.microsoft.com/en-us/dotnet/api/microsoft.activedirectory.management.adgroup?view=activedirectory-management-10.0) instances piped to this cmdlet are also supported.

### Example 4: Find any Circular Nested Groups from previous example

```powershell
PS ..\PSADTree\> Get-ADComputer -Filter * -SearchBase 'OU=myOU,DC=myDomain,DC=com' |
    Get-ADTreeGroupMember -Recursive -Group |
    Where-Object IsCircular
```

The `-Group` switch limits the members tree view to nested groups only.

### Example 5: Get group members in a different Domain

```powershell
PS ..\PSADTree\> Get-ADTreeGroupMember TestGroup001 -Server otherDomain
```

### Example 6: Get group members including processed groups

```powershell
PS ..\PSADTree\> Get-ADTreeGroupMember TestGroup001 -ShowAll
```

By default, previously processed groups will be marked as _"Processed Group"_ and their hierarchy will not be displayed.  
The `-ShowAll` switch indicates that the cmdlet should display the hierarchy of all previously processed groups.

__NOTE:__ The use of this switch should not infer in a great performance cost, for more details see the parameter details.

## PARAMETERS

### -Depth

Determines the number of nested groups and their members included in the recursion.  
By default, only 3 levels of recursion are included.

```yaml
Type: UInt32
Parameter Sets: Depth
Aliases:

Required: False
Position: Named
Default value: 3
Accept pipeline input: False
Accept wildcard characters: False
```

### -Group

The `-Group` switch indicates that the cmdlet should display nested group members only. Essentially, a built-in filter where [`ObjectClass`](https://learn.microsoft.com/en-us/windows/win32/adschema/a-objectclass) is `group`.

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

### -Identity

Specifies an Active Directory group by providing one of the following property values:

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

Specifies that the cmdlet should get all group members of the specified group.

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
This switch forces the cmdlet to display the full hierarchy including previously processed groups.

> __NOTE:__ This cmdlet uses a caching mechanism to ensure that Active Directory Groups are only queried once per Identity.  
This caching mechanism is also used to reconstruct the pre-processed group's hierarchy when the `-ShowAll` switch is used, thus not incurring a performance cost.  
The intent behind this switch is to not clutter the cmdlet's output by default.

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

You can pipe strings containing an identity to this cmdlet. [__`ADGroup`__](https://learn.microsoft.com/en-us/dotnet/api/microsoft.activedirectory.management.adgroup?view=activedirectory-management-10.0) instances piped to this cmdlet are also supported.

## OUTPUTS

### PSADTree.TreeGroup

### PSADTree.TreeUser

### PSADTree.TreeComputer

## NOTES

## RELATED LINKS
