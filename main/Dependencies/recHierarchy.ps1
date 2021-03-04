function recHierarchy{
param(
    [parameter(mandatory)]
    [String]$Name,
    [Int]$Recursion=0,
    [validateset('MemberOf','Member')]
    [string]$RecursionProperty='Member'
)

$errorMessage={
"
Group Name: $($Group.Name)
Member: $(($Member -replace 'CN=').Split(',')[0])
Error Message: $_
`n"
}

$thisObject=shouldProcess -Name $Name -RecursionProperty $RecursionProperty

$Hierarchy=$(
    ForEach($object in $thisObject.Property)
    {
        try{shouldProcess -Name $object}
        catch{Write-Warning $(&$errorMessage)}
    }
)|Sort -Descending ObjectClass

$script:Index.Add(
    [pscustomobject]@{
        InputParameter=$(
            if($Index[0].Index){$Index[0].Index}
            else{$thisObject.Name}
            )
        Index=$thisObject.Name
        Class=$txtInfo.ToTitleCase($thisObject.ObjectClass)
        Recursion=$Recursion
        Hierarchy=Indent -String $thisObject.Name -Indent $Recursion
    }) > $null
    
$Recursion++

foreach($object in $Hierarchy){

if($object.Name -in $Index.Index){
    [int]$i=$Recursion
    do{
        $i--
        $z=($index.where({$_.Recursion -eq $i})).Index|select -last 1
        if($object.Name -eq $z){$layer=$true}
    }until($i -eq 0 -or $layer -eq $true)
                    
    if($layer){
        
        $string=switch($object.ObjectClass)
        {
            'User'{$object.Name}
            'Group'{"{0} <=> Circular Nested Group" -f $object.Name}
        }

        $script:Index.Add(
            [pscustomobject]@{
                InputParameter=$(
                    if($Index[0].Index){$Index[0].Index}
                    else{$thisObject.Name}
                )
                Index=$object.Name
                Class=$txtInfo.ToTitleCase($object.ObjectClass)
                Recursion=$Recursion
                Hierarchy=Indent -String $string -Indent $Recursion
        }) > $null
    }
    else{
        
        $string=switch($object.ObjectClass)
        {
            'User'{$object.Name}
            'Group'{"{0} <=> Skipping // Processed" -f $object.Name}
        }

        $script:Index.Add(
            [pscustomobject]@{
                InputParameter=$(
                    if($Index[0].Index){$Index[0].Index}
                    else{$thisObject.Name}
                )
                Index=$object.Name
                Class=$txtInfo.ToTitleCase($object.ObjectClass)
                Recursion=$Recursion
                Hierarchy=Indent -String $string -Indent $Recursion
        }) > $null
    }
}
else{recHierarchy -Name $object.Name -Recursion $Recursion -RecursionProperty $RecursionProperty}
}
 
}