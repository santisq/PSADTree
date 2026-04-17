<h1 align="center">PSADTree</h1>

<div align="center">
<sub>Tree-like cmdlets for Active Directory principals!</sub>
<br /><br />

[![build](https://github.com/santisq/PSADTree/actions/workflows/ci.yml/badge.svg)](https://github.com/santisq/PSADTree/actions/workflows/ci.yml)
[![PowerShell Gallery](https://img.shields.io/powershellgallery/v/PSADTree?label=gallery)](https://www.powershellgallery.com/packages/PSADTree)
[![LICENSE](https://img.shields.io/github/license/santisq/PSADTree)](https://github.com/santisq/PSADTree/blob/main/LICENSE)

</div>

PSADTree is a PowerShell module that brings intuitive `tree`-like visualization to Active Directory group structures. It helps administrators and security professionals quickly understand nested group memberships, identify effective permissions, and spot potential circular references at a glance.

## Cmdlets

- **`Get-ADTreeGroupMember`**  
Displays the members of an Active Directory group in a clear hierarchical tree view. It recursively shows nested groups, users, computers, and other principals, making it easy to visualize complex group nesting.

- **`Get-ADTreePrincipalGroupMembership`**  
Shows all groups that a given Active Directory principal (user, computer, group, etc.) belongs to, presented in a tree structure. This reverse view is especially useful for understanding effective membership and troubleshooting access issues.

- **`Get-ADTreeStyle`**  
Retrieves the singleton `TreeStyle` instance used to customize the colored, hierarchical output of `Get-ADTreeGroupMember` and `Get-ADTreePrincipalGroupMembership`.  
Allows you to change colors for groups, users, computers, other principals, and apply accents. You can also control ANSI output rendering.

## Documentation

- Learn how to use the cmdlets in the [official documentation](./docs/en-US/).

- To Customize output rendering, see [about_TreeStyle](./docs/en-US/about_TreeStyle.md).

## Installation

### Gallery

The module is available through the [PowerShell Gallery](https://www.powershellgallery.com/packages/PSADTree):

```powershell
Install-Module PSADTree -Scope CurrentUser
```

### Source

```powershell
git clone 'https://github.com/santisq/PSADTree.git'
Set-Location ./PSADTree
./build.ps1
```

## Requirements

- Windows operating system (uses Windows-specific Active Directory .NET APIs)
- PowerShell 5.1 (Windows PowerShell) or PowerShell 7.4+
- Read permissions on the Active Directory objects you want to query

## Usage

These are some examples of what the cmdlets from this Module allow you to do. For more examples check out the docs.

### Get the members of a group

```powershell
PS ..\PSADTree> Get-ADTreeGroupMember TestGroup007

   Source: CN=TestGroup007,OU=Operations,DC=ChildDomain,DC=ParentDomain,DC=myDomain,DC=xyz

Domain                              ObjectClass Hierarchy
------                              ----------- ---------
ChildDomain                               group TestGroup007
ChildDomain          msDS-ManagedServiceAccount ├── testMSA$
ChildDomain                                user ├── TestUser013
ChildDomain                                user ├── TestUser010
ChildDomain                                user ├── TestUser007
ChildDomain                               group ├── TestGroup001
ChildDomain                                user │   ├── TestUser015
ChildDomain                                user │   ├── TestUser013
ChildDomain                                user │   ├── TestUser010
ChildDomain                                user │   ├── TestUser007
ChildDomain                                user │   ├── TestUser002
ChildDomain                               group │   ├── TestGroup005
ParentDomain                              group │   │   ├── TestGroup001
ParentDomain                              group │   │   └── TestGroup002
ChildDomain                               group │   ├── TestGroup006
ChildDomain                            computer │   │   ├── TestComputer0000004$
ChildDomain                            computer │   │   ├── TestComputer0000003$
ChildDomain                            computer │   │   ├── TestComputer0000002$
ChildDomain                            computer │   │   └── TestComputer0000001$
ChildDomain                               group │   └── TestGroup007 ↔ Circular Reference
ChildDomain                               group ├── TestGroup005 ↔ Processed Group
ChildDomain                               group └── TestGroup006 ↔ Processed Group
```

### Control the grade of recursion with the `-Depth` parameter

The default value for `-Depth` is 3.

```powershell
PS ..\PSADTree> Get-ADTreeGroupMember TestGroup007 -Depth 2

   Source: CN=TestGroup007,OU=Operations,DC=ChildDomain,DC=ParentDomain,DC=myDomain,DC=xyz

Domain                        ObjectClass Hierarchy
------                        ----------- ---------
ChildDomain                         group TestGroup007
ChildDomain    msDS-ManagedServiceAccount ├── testMSA$
ChildDomain                          user ├── TestUser013
ChildDomain                          user ├── TestUser010
ChildDomain                          user ├── TestUser007
ChildDomain                         group ├── TestGroup001
ChildDomain                          user │   ├── TestUser015
ChildDomain                          user │   ├── TestUser013
ChildDomain                          user │   ├── TestUser010
ChildDomain                          user │   ├── TestUser007
ChildDomain                          user │   ├── TestUser002
ChildDomain                         group │   ├── TestGroup005
ChildDomain                         group │   ├── TestGroup006
ChildDomain                         group │   └── TestGroup007 ↔ Circular Reference
ChildDomain                         group ├── TestGroup005 ↔ Processed Group
ChildDomain                         group └── TestGroup006 ↔ Processed Group
```

### Retrieve and inspect additional properties

```powershell
PS ..\PSADTree> $tree = Get-ADTreeGroupMember TestGroup001 -Properties *
PS ..\PSADTree> $user = $tree | Where-Object ObjectClass -EQ user | Select-Object -First 1
PS ..\PSADTree> $user.AdditionalProperties

Key                        Value
---                        -----
objectClass                {top, person, organizationalPerson, user}
cn                         John Doe
sn                         Doe
c                          US
l                          Elizabethtown
st                         NC
title                      Accounting Specialist
postalCode                 28337
physicalDeliveryOfficeName Accounting Office
telephoneNumber            910-862-8720
givenName                  John
initials                   B
distinguishedName          CN=John Doe,OU=Accounting,OU=Mylab Users,DC=mylab,DC=local
instanceType               4
whenCreated                9/18/2025 4:53:58 PM
whenChanged                9/18/2025 4:53:58 PM
displayName                John Doe
uSNCreated                 19664
memberOf                   CN=TestGroup001,OU=Mylab Groups,DC=mylab,DC=local
uSNChanged                 19668
department                 Accounting
company                    Active Directory Pro
streetAddress              2628 Layman Avenue
nTSecurityDescriptor       System.DirectoryServices.ActiveDirectorySecurity
name                       John Doe
objectGUID                 {225, 241, 160, 222…}
userAccountControl         512
badPwdCount                0
codePage                   0
countryCode                0
badPasswordTime            0
lastLogoff                 0
lastLogon                  0
pwdLastSet                 0
primaryGroupID             513
objectSid                  {1, 5, 0, 0…}
accountExpires             9223372036854775807
logonCount                 0
sAMAccountName             john.doe
sAMAccountType             805306368
userPrincipalName          john.doe@mylab.com
objectCategory             CN=Person,CN=Schema,CN=Configuration,DC=mylab,DC=local
dSCorePropagationData      1/1/1601 12:00:00 AM
mail                       john.doe@mylab.com
```

>[!TIP]
>
> - `-Properties *` retrieves **all** available attributes from each object.
> - Use friendly names (e.g. `Country` → `c`, `City` → `l`, `PasswordLastSet` → `pwdLastSet`) or raw LDAP names — the key in `.AdditionalProperties` matches what you requested.
> - See the full list of supported friendly names in the [source code `LdapMap.cs`](https://github.com/santisq/PSADTree/tree/main/src/PSADTree/LdapMap.cs)

### Get group members recursively, include only groups and display all processed groups

The `-Recursive` switch indicates that the cmdlet should traverse traverse the entire group hierarchy.  
The `-Group` switch limits the members tree view to nested groups only.  
By default, previously processed groups will be marked as _"Processed Group"_ and their hierarchy will not be displayed.  
The `-ShowAll` switch indicates that the cmdlet should display the hierarchy of all previously processed groups.  

```powershell
PS ..\PSADTree> Get-ADTreeGroupMember TestGroup007 -Recursive -Group -ShowAll

   Source: CN=TestGroup007,OU=Operations,DC=ChildDomain,DC=ParentDomain,DC=myDomain,DC=xyz

Domain         ObjectClass Hierarchy
------         ----------- ---------
ChildDomain          group TestGroup007
ChildDomain          group ├── TestGroup001
ChildDomain          group │   ├── TestGroup005
ParentDomain         group │   │   ├── TestGroup001
ParentDomain         group │   │   │   └── TestGroup002
ParentDomain         group │   │   └── TestGroup002
ChildDomain          group │   ├── TestGroup006
ChildDomain          group │   └── TestGroup007 ↔ Circular Reference
ChildDomain          group ├── TestGroup005
ParentDomain         group │   ├── TestGroup001
ParentDomain         group │   │   └── TestGroup002
ParentDomain         group │   └── TestGroup002
ChildDomain          group └── TestGroup006
```

### Get group memberships for a user

```powershell
PS ..\PSADTree> Get-ADTreePrincipalGroupMembership TestUser002

   Source: CN=TestUser002,OU=Operations,DC=ChildDomain,DC=ParentDomain,DC=myDomain,DC=xyz

Domain         ObjectClass Hierarchy
------         ----------- ---------
ChildDomain           user TestUser002
ChildDomain          group ├── TestGroup003
ChildDomain          group │   └── TestGroup000
ChildDomain          group ├── TestGroup001
ChildDomain          group │   ├── TestGroup007
ChildDomain          group │   │   ├── TestGroup004
ChildDomain          group │   │   ├── TestGroup002
ChildDomain          group │   │   └── TestGroup001 ↔ Circular Reference
ChildDomain          group │   └── TestGroup000 ↔ Processed Group
ChildDomain          group ├── Terminal Server License Servers
ChildDomain          group └── Domain Users
ChildDomain          group     └── Users
```

### Control the grade of recursion with the `-Depth` parameter

Same as `Get-ADTreeGroupMember`, the default depth to display the principal memberships is 2.

```powershell
PS ..\PSADTree> Get-ADTreePrincipalGroupMembership TestUser002 -Depth 2

   Source: CN=TestUser002,OU=Operations,DC=ChildDomain,DC=ParentDomain,DC=myDomain,DC=xyz

Domain         ObjectClass Hierarchy
------         ----------- ---------
ChildDomain           user TestUser002
ChildDomain          group ├── TestGroup003
ChildDomain          group │   └── TestGroup000
ChildDomain          group ├── TestGroup001
ChildDomain          group │   ├── TestGroup007
ChildDomain          group │   └── TestGroup000 ↔ Processed Group
ChildDomain          group ├── Terminal Server License Servers
ChildDomain          group └── Domain Users
ChildDomain          group     └── Users
```

### Get the user principal membership recursively and display all processed groups

```powershell
PS ..\PSADTree> Get-ADTreePrincipalGroupMembership TestUser002 -Recursive -ShowAll

   Source: CN=TestUser002,OU=Operations,DC=ChildDomain,DC=ParentDomain,DC=myDomain,DC=xyz

Domain         ObjectClass Hierarchy
------         ----------- ---------
ChildDomain           user TestUser002
ChildDomain          group ├── TestGroup003
ChildDomain          group │   └── TestGroup000
ChildDomain          group ├── TestGroup001
ChildDomain          group │   ├── TestGroup007
ChildDomain          group │   │   ├── TestGroup004
ChildDomain          group │   │   ├── TestGroup002
ChildDomain          group │   │   │   └── TestGroup000
ChildDomain          group │   │   └── TestGroup001 ↔ Circular Reference
ChildDomain          group │   └── TestGroup000
ChildDomain          group ├── Terminal Server License Servers
ChildDomain          group └── Domain Users
ChildDomain          group     └── Users
```

## Contributing

Contributions are welcome, if you wish to contribute, fork this repository and submit a pull request with the changes.
