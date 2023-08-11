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
        [string]$Server,
        [validateset('MemberOf','Member')]
        [string]$RecursionProperty = 'Member'
    )

    begin {
        $txtInfo = (Get-Culture).TextInfo

        function GetObject {
            param(
                [parameter(mandatory)]
                [string]$Name,
                [string]$Server
            )

            try {
                if ($Server -and [System.DirectoryServices.DirectoryEntry]::Exists("LDAP://$Server")) {
                    $Entry = [System.DirectoryServices.DirectoryEntry]"LDAP://$Server"
                }
            } catch {
                throw $_.Exception.InnerException
            }

            $Searcher = [System.DirectoryServices.DirectorySearcher]::new($Entry , "(|(aNR==$Name)(distinguishedName=$Name))")
            $Object = $Searcher.FindOne()

            if (-not $Object) {
                throw ("Cannot find an object: '{0}' under: '{1}'." -f $Name, $Searcher.SearchRoot.distinguishedName.ToString())
            }

            return $Object
        }

        function RecHierarchy {
            param(
                [parameter(mandatory)]
                [String]$DistinguishedName,
                [Int]$Recursion = 0,
                [validateset('MemberOf','Member')]
                [string]$RecursionProperty = 'Member'
            )

            $ErrorMessage = {
                "`nGroup Name: {0}`nMember: {1}`nError Message: $_`n" -f $Group.Name, $Member.Split(',')[0].Replace('CN=','')
            }

            $thisObject = QueryObject -DistinguishedName $DistinguishedName -RecursionProperty $RecursionProperty

            $Hierarchy = $(
            foreach($object in $thisObject.Property) {
                try {
                    QueryObject -DistinguishedName $object
                }
                catch {
                    Write-Warning (& $errorMessage)
                }
            }) | Sort-Object -Descending ObjectClass

            $thisInput = if($Index[0].Index) {
                $Index[0].Index
            }
            else {
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

            foreach($object in $Hierarchy) {
                $class = $txtInfo.ToTitleCase($object.ObjectClass)

                if($object.Name -in $Index.Index) {
                    [int]$i = $Recursion
                    do {
                        $i--
                        $z = $index.where({$_.Recursion -eq $i}).Index | Select-Object -Last 1
                        if($object.Name -eq $z) {
                            $layer = $true
                        }
                    } until($i -eq 0 -or $layer -eq $true)

                    if($layer) {
                        $string = switch($object.ObjectClass) {
                            'User' {
                                $object.Name
                            }
                            'Group' {
                                "{0} <=> Circular Nested Group" -f $object.Name
                            }
                        }
                    }
                    else {
                        $string = switch($object.ObjectClass) {
                            'User' {
                                $object.Name
                            }
                            'Group' {
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
                else {
                    RecHierarchy -DistinguishedName $object.DistinguishedName -Recursion $Recursion -RecursionProperty $RecursionProperty
                }
            }
        }

        function QueryObject {
            [cmdletbinding()]
            param(
                [string]$DistinguishedName,
                [validateset('MemberOf','Member')]
                [string]$RecursionProperty
            )

            $Object = [System.DirectoryServices.DirectoryEntry]"LDAP://$DistinguishedName"

            $Properties = [ordered]@{
                Name = $Object.name.ToString()
                UserPrincipalName = $Object.userPrincipalName.ToString()
                DistinguishedName = $Object.distinguishedName.ToString()
                ObjectClass = $txtInfo.ToTitleCase($Object.SchemaClassName.ToString())
            }

            if($RecursionProperty) {
                $Properties["Property"] = $Object.$RecursionProperty.GetEnumerator()
            }

            return ([pscustomobject]$Properties)
        }

        function Draw-Hierarchy {
            param(
                [System.Collections.ArrayList]$Array
            )

            $Array.Reverse()

            for($i=0;$i -lt $Array.Count;$i++) {
                if($Array[$i+1] -and $Array[$i].Hierarchy.IndexOf('|_') -lt $Array[$i+1].Hierarchy.IndexOf('|_')) {
                    $z = $i+1
                    $ind = $Array[$i].Hierarchy.IndexOf('|_')
                    while($Array[$z].Hierarchy[$ind] -ne '|') {
                        $string = ($Array[$z].Hierarchy).ToCharArray()
                        $string[$ind] = '|'
                        $string = $string -join ''
                        $Array[$z].Hierarchy = $string
                        $z++
                        if($Array[$z].Hierarchy[$ind] -eq '|') {
                            break
                        }
                    }
                }
            }

            $Array.Reverse()
            return $Array
        }

        function Indent {
            param(
                [String]$String,
                [Int]$Indent
            )

            $x='_';$y='|';$z='    '

            switch($Indent) {
                {$_ -eq 0}{
                    return $String
                }
                {$_ -gt 0}{
                    return "$($z*$_)$y$x $string"
                }
            }
        }
    }

    process {
        $script:Index = New-Object System.Collections.ArrayList
        $Object = GetObject -Name $Name -Server $Server
        RecHierarchy -DistinguishedName $Object.Properties.distinguishedname -RecursionProperty $RecursionProperty
        Draw-Hierarchy -Array $Index
        Remove-Variable Index -Scope Global -Force -ErrorAction SilentlyContinue
    }
}