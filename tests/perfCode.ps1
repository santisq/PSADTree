param($pathOrName)

Import-Module $pathOrName

Write-Host "Running PSADTree v$((Get-Module PSADTree).Version)"

$objects = Get-ADObject -LDAPFilter '(|(objectClass=group)(objectClass=user))'
$objects = 0..15 | ForEach-Object { $objects }
$objects | Where-Object ObjectClass -EQ group | Get-ADTreeGroupMember -Recursive -ShowAll
$objects | Get-ADTreePrincipalGroupMembership -Recursive -ShowAll
