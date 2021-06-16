function RecHierarchy {
param(
    [parameter(mandatory)]
    [String]$Name,
    [Int]$Recursion = 0,
    [validateset('MemberOf','Member')]
    [string]$RecursionProperty = 'Member'
)

    $ErrorMessage = 
    {
        "`nGroup Name: {0}`nMember: {1}`nError Message: $_`n" -f $Group.Name, $Member.Split(',')[0].Replace('CN=','')
    }

    $thisObject = ShouldProcess -Name $Name -RecursionProperty $RecursionProperty

    $Hierarchy = $(
    foreach($object in $thisObject.Property)
    {
        try
        {
            shouldProcess -Name $object
        }
        catch
        {
            Write-Warning (& $errorMessage)
        }
    }) | Sort-Object -Descending ObjectClass

    $thisInput = if($Index[0].Index)
    {
        $Index[0].Index
    }
    else
    {
        $thisObject.Name
    }

    $script:Index.Add(
    [pscustomobject]@{
        InputParameter = $thisInput
        Index = $thisObject.Name
        Class = $txtInfo.ToTitleCase($thisObject.ObjectClass)
        Recursion = $Recursion
        Hierarchy = Indent -String $thisObject.Name -Indent $Recursion
    }) > $null
    
    $Recursion++

    foreach($object in $Hierarchy)
    {
        $class = $txtInfo.ToTitleCase($object.ObjectClass)

        if($object.Name -in $Index.Index)
        {
            [int]$i = $Recursion
            do
            {
                $i--
                $z = $index.where({$_.Recursion -eq $i}).Index | Select-Object -Last 1
                if($object.Name -eq $z)
                {
                    $layer = $true
                }
            }until($i -eq 0 -or $layer -eq $true)
                    
            if($layer)
            {    
                $string = switch($object.ObjectClass)
                {
                    'User'
                    { 
                        $object.Name 
                    }
                    'Group'
                    {
                        "{0} <=> Circular Nested Group" -f $object.Name
                    }
                }
            }
            else
            {
                $string = switch($object.ObjectClass)
                {
                    'User'
                    {
                        $object.Name
                    }
                    'Group'
                    {
                        "{0} <=> Skipping // Processed" -f $object.Name
                    }
                }
            }

            $script:Index.Add(
            [pscustomobject]@{
                InputParameter = $thisInput
                Index = $object.Name
                Class = $class
                Recursion = $Recursion
                Hierarchy = Indent -String $string -Indent $Recursion
            }) > $null
        }
        else
        {
            RecHierarchy -Name $object.Name -Recursion $Recursion -RecursionProperty $RecursionProperty
        }
    }
}
