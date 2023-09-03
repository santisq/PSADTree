---
external help file: PSADTree.dll-Help.xml
Module Name: PSADTree
online version:
schema: 2.0.0
---

# Get-ADTreePrincipalGroupMembership

## SYNOPSIS

`tree` like cmdlet for Active Directory Principal group membership.

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

The `Get-ADTreePrincipalGroupMembership` cmdlet displays an Active Directory Principal group membership in a tree structure.


## EXAMPLES

### Example 1

```powershell
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

## PARAMETERS

### -Depth

{{ Fill Depth Description }}

```yaml
Type: Int32
Parameter Sets: Depth
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Identity

{{ Fill Identity Description }}

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

{{ Fill Recursive Description }}

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

{{ Fill Server Description }}

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

{{ Fill ShowAll Description }}

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

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String

## OUTPUTS

### PSADTree.TreeGroup

### PSADTree.TreeUser

### PSADTree.TreeComputer

## NOTES

## RELATED LINKS
