function Get-GroupHierarchy{
<#
    .SYNOPSIS
    Gets group membership and draws hierarchy.
    
    .PARAMETER Name
    Distribution List or Security Group.

    .OUTPUTS
    Object[] // System.Array. Returns with properties:
        - Parent: The input group parameter.
        - Index: The object on each level of recursion.
        - Recursion: The level of recursion or group nesting.
        - Class: The objectClass.
        - Hierarchy: The hierarchy map of the input group.

    .EXAMPLE
    C:\PS> Get-GroupHierarchy ExampleGroup

    .EXAMPLE
    C:\PS> ggh ExampleGroup

    .EXAMPLE           
    C:\PS> Get-ADGroup ExampleGroup | Get-GroupHierarchy
    
    .EXAMPLE           
    C:\PS> Get-ADGroup ExampleGroup | ggh
    
    .EXAMPLE
    C:\PS> Get-ADGroup -Filter {Name -like 'ExampleGroups*'} | Get-GroupHierarchy
    
    .EXAMPLE           
    C:\PS> 'Group1,Group2,Group3'.split(',') | Get-GroupHierarchy

    .NOTES
    Author: Santiago Squarzon.
#>

[cmdletbinding()]
[alias('ggh')]
param(
    [parameter(mandatory,valuefrompipeline)]
    [string]$Name
)

begin{

#requires -Modules ActiveDirectory

$txtInfo=(Get-Culture).TextInfo

function Indent{
param(
    [String]$String,
    [Int]$Indent
)

$x='_';$y='|';$z='    '

switch($Indent){
    {$_ -eq 0}{return $String}
    {$_ -gt 0}{return "$($z*$_)$y$x $string"}    
    }
}

function Draw-Hierarchy{
param(
    [System.Collections.ArrayList]$Array
)

$Array.Reverse()

for($i=0;$i -lt $Array.Count;$i++){

    if(
        $Array[$i+1] -and 
        $Array[$i].Hierarchy.IndexOf('|_') -lt $Array[$i+1].Hierarchy.IndexOf('|_')
    ){
    $z=$i+1
    $ind=$Array[$i].Hierarchy.IndexOf('|_')
        while($Array[$z].Hierarchy[$ind] -ne '|'){
            $string=($Array[$z].Hierarchy).ToCharArray()
            $string[$ind]='|'
            $string=$string -join ''
            $Array[$z].Hierarchy=$string
            $z++
            if($Array[$z].Hierarchy[$ind] -eq '|'){break}
            }
        }
    }

$Array.Reverse()
return $Array

}

function Get-Hierarchy{
param(
    [String]$GroupName,
    [Int]$Recursion=0
)

<#

Using Get-ADobject with LDAPFilter is much faster, almost 2 times faster.

switch -Regex($PSBoundParameters.Values)
{
    "^(?:(?<cn>CN=(?<name>[^,]*)),)?(?:(?<path>(?:(?:CN|OU)=[^,]+,?)+),)?(?<domain>(?:DC=[^,]+,?)+)$"
    {
        $Group=Get-ADObject $GroupName -Properties Member
    }
    Default
    {
        $Group=Get-ADObject -LDAPFilter "samAccountName=$GroupName" -Properties Member
    }
}
#>

$filter="(&(objectclass=group)(|(distinguishedname=$GroupName)(samaccountname=$GroupName)))"
$Group=Get-ADObject -LDAPFilter $filter -Properties member

$GroupMembers=$(
    ForEach($Member in $Group.Member)
    {
        Get-ADobject $Member|select Name,ObjectClass
    })|Sort -Descending ObjectClass

$script:Index.Add(
    [pscustomobject]@{
        Parent=$(
            if($Index[0].Index){$Index[0].Index}
            else{$Group.Name}
            )
        Index=$Group.Name
        Recursion=$Recursion
        Class=$txtInfo.ToTitleCase($Group.ObjectClass)
        Hierarchy=Indent -String $Group.Name -Indent $Recursion -Class Group
    }) > $null
    
$Recursion++

foreach($Member in $GroupMembers){
        
    switch($Member.ObjectClass){
        'Group'{        

            if($Member.Name -in $Index.Index)
            {
                [int]$i=$Recursion
                do
                {
                    $i--
                    $z=($Index|?{$_.Recursion -eq $i}).Index|select -last 1
                    if($Member.Name -eq $z){$layer=$true}
                } until($i -eq 0 -or $layer -eq $true)
                    
                if($layer)
                {
                    $script:Index.Add(
                        [pscustomobject]@{
                            Parent=$(
                                if($Index[0].Index){$Index[0].Index}
                                else{$Group.Name}
                                )
                            Index=$Member.Name
                            Recursion=$Recursion
                            Class=$txtInfo.ToTitleCase($Member.ObjectClass)
                            Hierarchy=Indent -String "$($Member.Name) <=> Circular Nested Group" -Indent $Recursion -Class Group
                        }) > $null
                }
                else
                {
                    $script:Index.Add(
                        [pscustomobject]@{
                            Parent=$(
                                if($Index[0].Index){$Index[0].Index}
                                else{$Group.Name}
                                )
                            Index=$Member.Name
                            Recursion=$Recursion
                            Class=$txtInfo.ToTitleCase($Member.ObjectClass)
                            Hierarchy=Indent -String "$($Member.Name) <=> Skipping // Processed" -Indent $Recursion -Class Group
                        }) > $null
                }
            }
            else{Get-Hierarchy -GroupName $Member.Name -Recursion $Recursion}
        }
        'User'{
            $script:Index.Add(
                [pscustomobject]@{
                    Parent=$(
                        if($Index[0].Index){$Index[0].Index}
                        else{$Group.Name}
                        )
                    Index=$Member.Name
                    Recursion=$Recursion
                    Class=$txtInfo.ToTitleCase($Member.ObjectClass)
                    Hierarchy=Indent -String $Member.Name -Indent $Recursion -Class User
                }) > $null
        }
    }

}

}

}

process{

$script:Index=New-Object System.Collections.ArrayList
Get-Hierarchy -GroupName $Name
Draw-Hierarchy -Array $Index
rv Index -Scope Global -Force 2>$null

}

}