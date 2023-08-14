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
        [parameter(Mandatory, ValueFromPipeline)]
        [string]$Name,
        [string]$Server,
        [validateset('MemberOf', 'Member')]
        [string]$RecursionProperty = 'Member'
    )

    begin {
        function GetObject {
            param(
                [parameter(Mandatory)]
                [string]$Name,
                [string]$Server
            )

            try {
                if ($PSBoundParameters.ContainsKey('Server')) {
                    $Entry = [adsi] "LDAP://$Server"
                }
            }
            catch {
                $PSCmdlet.ThrowTerminatingError($_)
            }

            $Searcher = [adsisearcher]::new(
                $Entry,
                "(|(name=$name)(samAccountName=$Name)(distinguishedName=$Name))")

            $Object = $Searcher.FindOne()

            if (-not $Object) {
                $PSCmdlet.ThrowTerminatingError(
                    [System.Management.Automation.ErrorRecord]::new(
                        [System.ArgumentException] ("Cannot find an object: '{0}' under: '{1}'." -f $Name, $Searcher.SearchRoot.distinguishedName.ToString()),
                        'ObjectNotFound',
                        [System.Management.Automation.ErrorCategory]::ObjectNotFound,
                        $Name))
            }

            $Object
        }

        function RecHierarchy {
            param(
                [parameter(mandatory)]
                [String]$DistinguishedName,
                [Int]$Recursion = 0,
                [validateset('MemberOf', 'Member')]
                [string]$RecursionProperty = 'Member'
            )

            $ErrorMessage = {
                "`nGroup Name: {0}`nMember: {1}`nError Message: $_`n" -f $Group.Name, $Member.Split(',')[0].Replace('CN=', '')
            }

            $queryObjectSplat = @{
                DistinguishedName = $DistinguishedName
                RecursionProperty = $RecursionProperty
            }

            $thisObject = QueryObject @queryObjectSplat

            $Hierarchy = & {
                foreach ($object in $thisObject.Property) {
                    try {
                        QueryObject -DistinguishedName $object
                    }
                    catch {
                        Write-Warning (& $errorMessage)
                    }
                }
            } | Sort-Object -Descending ObjectClass

            $thisInput = if ($Index[0].Index) {
                $Index[0].Index
            }
            else {
                $thisObject.Name
            }

            $Index.Add(
                [pscustomobject]@{
                    InputParameter = $thisInput
                    Index          = $thisObject.Name
                    Class          = $thisObject.ObjectClass
                    Recursion      = $Recursion
                    Domain         = $thisObject.Domain
                    Hierarchy      = [Tree]::Indent($thisObject.Name, $Recursion)
                })

            $Recursion++

            foreach ($object in $Hierarchy) {
                $class = $object.ObjectClass

                if (($object.Name -in $Index.Index -and ($object.Domain -in $Index.Domain))) {
                    [int]$i = $Recursion
                    do {
                        $i--
                        $z = $index.where({ $_.Recursion -eq $i }).Index | Select-Object -Last 1
                        if ($object.Name -eq $z) {
                            $layer = $true
                        }
                    } until($i -eq 0 -or $layer -eq $true)

                    if ($layer) {
                        $string = switch ($object.ObjectClass) {
                            default {
                                $object.Name
                            }
                            'Group' {
                                '{0} <=> Circular Nested Group' -f $object.Name
                            }
                        }
                    }
                    else {
                        $string = switch ($object.ObjectClass) {
                            default {
                                $object.Name
                            }
                            'Group' {
                                '{0} <=> Skipping // Processed' -f $object.Name
                            }
                        }
                    }

                    $Index.Add(
                        [pscustomobject]@{
                            InputParameter = $thisInput
                            Index          = $object.Name
                            Class          = $class
                            Recursion      = $Recursion
                            Domain         = $object.Domain
                            Hierarchy      = [Tree]::Indent($string, $Recursion)
                        })
                }
                else {
                    $recHierarchySplat = @{
                        DistinguishedName = $object.DistinguishedName
                        Recursion         = $Recursion
                        RecursionProperty = $RecursionProperty
                    }

                    RecHierarchy @recHierarchySplat
                }
            }
        }

        function QueryObject {
            [cmdletbinding()]
            param(
                [string]$DistinguishedName,
                [validateset('MemberOf', 'Member')]
                [string]$RecursionProperty
            )

            $Object = [adsi] "LDAP://$DistinguishedName"

            $Properties = [ordered]@{
                Name              = $Object.name.ToString()
                UserPrincipalName = $Object.userPrincipalName.ToString()
                DistinguishedName = $Object.distinguishedName.ToString()
                Domain            = ($Object.distinguishedName.ToString() -isplit ",DC=")[1].ToUpper()
                ObjectClass       = $Object.SchemaClassName.ToString()
            }

            if ($RecursionProperty) {
                $Properties['Property'] = $Object.$RecursionProperty
            }

            return [pscustomobject] $Properties
        }

        class Tree {
            hidden static [regex] $s_re = [regex]::new(
                '└|\S',
                [System.Text.RegularExpressions.RegexOptions]::Compiled)

            static [string] Indent([string] $inputString, [int] $indentation) {
                if ($indentation -eq 0) {
                    return $inputString
                }

                return [string]::new(' ', (4 * $indentation) - 4) + '└── ' + $inputString
            }

            static [object[]] ConvertToTree([object[]] $inputObject) {
                for ($i = 0; $i -lt $inputObject.Length; $i++) {
                    $index = $inputObject[$i].Hierarchy.IndexOf('└')

                    if ($index -lt 0) {
                        continue
                    }

                    $z = $i - 1

                    while (-not [Tree]::s_re.IsMatch($inputObject[$z].Hierarchy[$index].ToString())) {
                        $replace = $inputObject[$z].Hierarchy.ToCharArray()
                        $replace[$index] = '│'
                        $inputObject[$z].Hierarchy = [string]::new($replace)
                        $z--
                    }

                    if ($inputObject[$z].Hierarchy[$index] -eq '└') {
                        $replace = $inputObject[$z].Hierarchy.ToCharArray()
                        $replace[$index] = '├'
                        $inputObject[$z].Hierarchy = [string]::new($replace)
                    }
                }

                return $inputObject
            }
        }
    }

    process {
        try {
            $getObjectSplat = @{
                Name = $Name
            }

            if ($PSBoundParameters.ContainsKey('Server')) {
                $getObjectSplat['Server'] = $Server
            }
            $Index = [System.Collections.Generic.List[object]]::new()
            $Object = GetObject @getObjectSplat

            $recHierarchySplat = @{
                DistinguishedName = $Object.Properties.distinguishedname
                RecursionProperty = $RecursionProperty
            }

            RecHierarchy @recHierarchySplat
            [Tree]::ConvertToTree($Index.ToArray())
        }
        catch {
            $PSCmdlet.ThrowTerminatingError($_)
        }
    }
}
