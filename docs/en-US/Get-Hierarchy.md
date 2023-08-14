---
external help file:
Module Name:
online version:
schema: 2.0.0
---

# Get-Hierarchy

## SYNOPSIS

Gets group membership or parentship and draws it's hierarchy.

## SYNTAX

```
Get-Hierarchy [-Name] <String> [[-Server] <String>] [[-RecursionProperty] <String>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

{{ Fill in the Description }}

## EXAMPLES

### EXAMPLE 1

```
Get-Hierarchy ExampleGroup
```

### EXAMPLE 2

```
gh ExampleGroup -RecursionProperty MemberOf
```

### EXAMPLE 3

```
Get-ADUser ExampleUser | Get-Hierarchy -RecursionProperty MemberOf
```

### EXAMPLE 4

```
Get-ADGroup ExampleGroup | Get-Hierarchy -RecursionProperty MemberOf
```

### EXAMPLE 5

```
Get-ADGroup ExampleGroup | gh
```

### EXAMPLE 6

```
Get-ADGroup -Filter {Name -like 'ExampleGroups*'} | Get-Hierarchy
```

### EXAMPLE 7

```
'Group1,Group2,Group3'.split(',') | Get-Hierarchy -RecursionProperty MemberOf
```

## PARAMETERS

### -Name

Objects of class User or Group.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -ProgressAction

{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RecursionProperty

{{ Fill RecursionProperty Description }}

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 3
Default value: Member
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
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

### Object[] // System.Array. Returns with properties

### - InputParameter: The input user or group

### - Index: The object on each level of recursion

### - Recursion: The level of recursion or nesting

### - Class: The objectClass

### - SubClass: The subClass of the group (DistributionList or SecurityGroup) or user (EID, AppID, etc)

### - Hierarchy: The hierarchy map of the input paremeter

## NOTES

Author: Santiago Squarzon.

## RELATED LINKS
