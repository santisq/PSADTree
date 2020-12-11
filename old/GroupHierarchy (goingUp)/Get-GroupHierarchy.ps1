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

function Get-Hierarchy{
param(
    [String]$Name,
    [Int]$Recursion=0
)

$filter="(&(|(distinguishedname=$Name)(samaccountname=$Name)(name=$Name)))"

$errorMessage={
"
Group Name: $($Group.Name)
Member: $(($Member -replace 'CN=').Split(',')[0])
Error Message: $_
`n"
}

$thisObject=Get-ADObject -LDAPFilter $filter -Properties memberof
$subClass=Get-SubClass -DistinguishedName $thisObject.DistinguishedName

$parents=$(
    ForEach($parent in $thisObject.memberof)
    {
        try{Get-SubClass $parent}
        catch{Write-Warning $(&$errorMessage)}
    }
)|Sort -Descending ObjectClass

$script:Index.Add(
    [pscustomobject]@{
        ChildObject=$(
            if($Index[0].Index){$Index[0].Index}
            else{$thisObject.Name}
            )
        Index=$thisObject.Name
        Class=$txtInfo.ToTitleCase($thisObject.ObjectClass)
        SubClass=$subClass.SubClass
        Recursion=$Recursion
        Hierarchy=Indent -String $thisObject.Name -Indent $Recursion
    }) > $null
    
$Recursion++

foreach($parent in $parents){

if($parent.Name -in $Index.Index){
    [int]$i=$Recursion
    do{
        $i--
        $z=($index.where({$_.Recursion -eq $i})).Index|select -last 1
        if($parent.Name -eq $z){$layer=$true}
    }until($i -eq 0 -or $layer -eq $true)
                    
    if($layer){
        $script:Index.Add(
            [pscustomobject]@{
                ChildObject=$(
                    if($Index[0].Index){$Index[0].Index}
                    else{$thisObject.Name}
                )
                Index=$parent.Name
                Class=$txtInfo.ToTitleCase($parent.ObjectClass)
                SubClass=$parent.SubClass
                Recursion=$Recursion
                Hierarchy=Indent -String "$($parent.Name) <=> Circular Nested Group" -Indent $Recursion
        }) > $null
    }
    else{
        $script:Index.Add(
            [pscustomobject]@{
                ChildObject=$(
                    if($Index[0].Index){$Index[0].Index}
                    else{$thisObject.Name}
                )
                Index=$parent.Name
                Class=$txtInfo.ToTitleCase($parent.ObjectClass)
                SubClass=$parent.SubClass
                Recursion=$Recursion
                Hierarchy=Indent -String "$($parent.Name) <=> Skipping // Processed" -Indent $Recursion
        }) > $null
    }
}
else{Get-Hierarchy -Name $parent.Name -Recursion $Recursion}
}
 
}

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

function Get-SubClass{
param(
    [string]$DistinguishedName
)
<#
try{
    $props=Get-ADuser $DistinguishedName -Properties a-userObjectCode,a-userObjectSubCode
}
catch{
    try{$props=Get-ADGroup $DistinguishedName}
    catch{$props=Get-ADObject $DistinguishedName}
}
#>

$object=Get-ADObject $DistinguishedName

$subClass=switch($object)
{
    {$_.ObjectClass -eq 'User'}{
        $props=Get-ADuser $object -Properties a-userObjectCode,a-userObjectSubCode
        switch($props)
        {
            {$_.'a-userObjectCode' -match '1|2|9'}{'Enterprise ID'}
            {$_.'a-userObjectCode' -eq '3'}{
                switch($props)
                {
                    {$_.'a-userObjectSubCode' -match '8|16'}{'Shared Mailbox'}
                    Default{'Application ID'}
                }
            }
            {$_.'a-userObjectCode' -eq '5'}{'Test ID'}
            Default{'Other'}
        }
    }

    {$_.ObjectClass -eq 'Group'}{
            $props=Get-ADGroup $object
            switch($props.GroupCategory)
            {
                'Distribution'{'Distribution List'}
                'Security'{'Security Group'}
                Default{$_}
            }
    }
    Default{$_.ObjectClass}
}

return [pscustomobject]@{
    Name=$props.Name
    ObjectClass=$props.ObjectClass
    SubClass=$subClass
    }
}

}

process{

$script:Index=New-Object System.Collections.ArrayList

Get-Hierarchy -Name $Name


Draw-Hierarchy -Array $Index
rv Index -Scope Global -Force 2>$null

}

}