---
external help file: PSADTree.dll-Help.xml
Module Name: PSADTree
online version: https://github.com/santisq/PSADTree/blob/main/docs/en-US/Get-ADTreeStyle.md
schema: 2.0.0
---

# Get-ADTreeStyle

## SYNOPSIS

Retrieves the `TreeStyle` instance used for output rendering.

## SYNTAX

```powershell
Get-ADTreeStyle
    [<CommonParameters>]
```

## DESCRIPTION

The `Get-ADTreeStyle` cmdlet provides access to the `TreeStyle` instance that controls the rendering and customization of output for the `Get-ADTreeGroupMember` and `Get-ADTreePrincipalGroupMembership` cmdlets.

For details, see [__about_TreeStyle__](./about_TreeStyle.md).

## EXAMPLES

### Example 1

```powershell
PS ..\PSADTree> $style = Get-ADTreeStyle
```

Stores the `TreeStyle` instance in the `$style` variable.

## PARAMETERS

### CommonParameters

This cmdlet supports the common parameters. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### TreeStyle
