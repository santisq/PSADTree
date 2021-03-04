function shouldProcess{
[cmdletbinding()]
param(
    [string]$Name,
    [validateset('MemberOf','Member')]
    [string]$RecursionProperty
)

$filter="(|(distinguishedname=$Name)(samaccountname=$Name)(name=$Name))"
switch($Name)
{
    'Administrators'{
        if($RecursionProperty){
            $object=Get-ADGroup -LDAPFilter $filter -Properties $RecursionProperty
        }else{
            $object=Get-ADGroup -LDAPFilter $filter
        }
    }
    Default{
        if($RecursionProperty){
            $object=Get-ADObject -LDAPFilter $filter -Properties $RecursionProperty
        }else{
            $object=Get-ADObject -LDAPFilter $filter
        }
    }
}

if(!$object){
    $eMessage="Cannot find an object with identity: '{0}' under: '{1}'." -f $Name,$((Get-ADRootDSE).defaultNamingContext)
    throw $eMessage
}
if($object.count -gt 1){
    throw "Multiple objects with Name '$Name' were found. Use DistinguishedName for unique output."
}

$culture=(Get-Culture).TextInfo

if($RecursionProperty){
    [PScustomObject]@{
        Name=$object.Name
        UserPrincipalName=$props.UserPrincipalName
        ObjectClass=$culture.ToTitleCase($object.ObjectClass)
        Property=$object.$RecursionProperty
    }
}else{
    [PScustomObject]@{
        Name=$object.Name
        UserPrincipalName=$props.UserPrincipalName
        ObjectClass=$culture.ToTitleCase($object.ObjectClass)
    }
}

}