---
Module Name: PSADTree
Module Guid: e49013dc-4106-4a95-aebc-b2669cbadeab
Download Help Link: https://github.com/santisq/PSADTree
Help Version: 1.0.0.0
Locale: en-US
---

# about_PSADTree

## SHORT DESCRIPTION

Displays Active Directory group membership hierarchies in a tree-like format (similar to the Windows `tree` command).

## LONG DESCRIPTION

**PSADTree** is a lightweight PowerShell module that provides two cmdlets to visualize Active Directory group structures:

- [`Get-ADTreeGroupMember`](Get-ADTreeGroupMember.md) – Shows the members (users, groups, computers, etc.) of a group, including nested membership, in a tree view.  
  Helps identify nested groups and detect circular references.

- [`Get-ADTreePrincipalGroupMembership`](Get-ADTreePrincipalGroupMembership.md) – Shows all groups that a principal (user, computer, group, or service account) belongs to, including nested paths, in a tree view.  
  Useful for auditing effective permissions and spotting circular nesting.

Both cmdlets support:

- Depth limiting and full recursion
- Additional property retrieval (`-Properties`)
- Caching to avoid redundant AD queries
- Exclusion patterns
- Cross-domain support (`-Server`)

They are particularly useful for troubleshooting complex group nesting, security reviews, and understanding effective group membership without manually traversing AD.

## REQUIREMENTS

- PowerShell 7.4+ or later (or Windows PowerShell 5.1)
- Appropriate permissions to read AD objects

## EXAMPLES

```powershell
# View nested members of a group (default depth 3)
Get-ADTreeGroupMember "Domain Admins"

# View all groups a user belongs to (recursive)
Get-ADTreePrincipalGroupMembership john.doe -Recursive

# Get full membership tree with extra properties
Get-ADTreePrincipalGroupMembership john.doe -Properties Department, Title, PasswordLastSet -Recursive
```
