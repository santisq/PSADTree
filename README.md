<h1 align="center">PSADTree</h1>

<div align="center">
    <sub>Tree like cmdlets for Active Directory Principals!</sub>
    <br /><br />

[![build](https://github.com/santisq/PSADTree/actions/workflows/ci.yml/badge.svg)](https://github.com/santisq/PSADTree/actions/workflows/ci.yml)
[![PowerShell Gallery](https://img.shields.io/powershellgallery/v/PSADTree?label=gallery)](https://www.powershellgallery.com/packages/PSADTree)
[![LICENSE](https://img.shields.io/github/license/santisq/PSADTree)](https://github.com/santisq/PSADTree/blob/main/LICENSE)

</div>

PSADTree is a PowerShell Module with cmdlets that emulate the [`tree` command](https://learn.microsoft.com/en-us/windows-server/administration/windows-commands/tree) for Active Directory Principals.  
This Module currently includes two cmdlets:

- [Get-ADTreeGroupMember](Get-ADTreeGroupMember.md) for AD Group Members.
- [Get-ADTreePrincipalGroupMembership](Get-ADTreePrincipalGroupMembership.md) for AD Principal Group Membership.

__Both cmdlets help with discovery of Circular Nested Groups.__

## Documentation

Check out [__the docs__](./docs/en-US/PSADTree.md) for information about how to use this Module.

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

This Module uses the [`System.DirectoryServices.AccountManagement` Namespace](https://learn.microsoft.com/en-us/dotnet/api/system.directoryservices.accountmanagement?view=dotnet-plat-ext-7.0) to query Active Directory, its System Requirement is __Windows OS__ and is compatible with __Windows PowerShell v5.1__ or [__PowerShell 7+__](https://github.com/PowerShell/PowerShell).

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

### Get group members recursively, include only groups and display all processed groups

The `-Recursive` switch indicates that the cmdlet should traverse all the group hierarchy.  
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
Get-ADTreePrincipalGroupMembership TestUser002 -Recursive -ShowAll

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

Contributions are more than welcome, if you wish to contribute, fork this repository and submit a pull request with the changes.
