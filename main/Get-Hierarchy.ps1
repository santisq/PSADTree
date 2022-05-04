function Get-Hierarchy {
<#
    .SYNOPSIS
    Gets group membership or parentship and draws it's hierarchy.
    
    .PARAMETER Name
    Objects of class User or Group.

    .OUTPUTS
    Object[] // System.Array. Returns with properties:
        - InputParameter: The input user or group.
        - Index: The object on each level of recursion.
        - Recursion: The level of recursion or nesting.
        - Class: The objectClass.
        - SubClass: The subClass of the group (DistributionList or SecurityGroup) or user (EID, AppID, etc).
        - Hierarchy: The hierarchy map of the input paremeter.

    .EXAMPLE
    C:\PS> Get-Hierarchy ExampleGroup

    .EXAMPLE
    C:\PS> gh ExampleGroup -RecursionProperty MemberOf
    
    .EXAMPLE
    C:\PS> Get-ADUser ExampleUser | Get-Hierarchy -RecursionProperty MemberOf

    .EXAMPLE           
    C:\PS> Get-ADGroup ExampleGroup | Get-Hierarchy -RecursionProperty MemberOf
    
    .EXAMPLE           
    C:\PS> Get-ADGroup ExampleGroup | gh
    
    .EXAMPLE
    C:\PS> Get-ADGroup -Filter {Name -like 'ExampleGroups*'} | Get-Hierarchy
    
    .EXAMPLE           
    C:\PS> 'Group1,Group2,Group3'.split(',') | Get-Hierarchy -RecursionProperty MemberOf

    .NOTES
    Author: Santiago Squarzon.
#>

[cmdletbinding()]
[alias('gh')]
param(
    [parameter(Mandatory,ValueFromPipeline)]
    [string]$Name,
    [validateset('MemberOf','Member')]
    [string]$RecursionProperty = 'Member'
)

    begin
    {
        # Requires -Modules ActiveDirectory
        $ErrorActionPreference = 'Stop'
        $culture = (Get-Culture).TextInfo
        Get-ChildItem "$PSScriptRoot\Dependencies\*.ps1" | Import-Module
    }

    process
    {
        $script:Index = New-Object System.Collections.ArrayList
        RecHierarchy -Name $Name -RecursionProperty $RecursionProperty
        Draw-Hierarchy -Array $Index
        Remove-Variable Index -Scope Global -Force -ErrorAction SilentlyContinue
    }
}
