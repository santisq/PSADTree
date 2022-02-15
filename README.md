# Get-Hierarchy

### DESCRIPTION
Gets Group's and User's membership or Group parentship and draws it's hierarchy.
Helps identifying <b>Circular Nested Groups</b>.
    
### PARAMETER

| Parameter Name | Description |
| --- | --- |
| `-Name <string>` | __Name, SamAccountName or DistinguishedName__ of an AD Object of the class __User__ or __Group__ |
| `[-RecursionProperty <string>]` | Set AD attribute for recursion. __Valid Values__: `Member` / `MemberOf`. __Default Value__: `Member`  |
| `[<CommonParameters>]` | See [`about_CommonParameters`](https://go.microsoft.com/fwlink/?LinkID=113216) |

### OUTPUTS
`<Object[]>`

- `InputParameter` The input user or group </li>
- `Index` The object on each level of recursion </li>
- `Recursion` The level of recursion or nesting </li>
- `Class` The objectClass </li>
- `Hierarchy` The hierarchy map of the input paremeter </li>

### REQUIREMENTS

- PowerShell v5.1
- [ActiveDirectory Module](https://docs.microsoft.com/en-us/powershell/module/activedirectory/?view=windowsserver2022-ps)</li>


### USAGE EXAMPLES

```
PS C:\> Get-Hierarchy ExampleGroup
PS C:\> gh ExampleGroup -RecursionProperty MemberOf
PS C:\> Get-ADGroup ExampleGroup | Get-Hierarchy -RecursionProperty MemberOf
PS C:\> Get-ADGroup ExampleGroup | gh
PS C:\> Get-ADGroup -Filter "Name -like 'ExampleGroups*'" | Get-Hierarchy
PS C:\> 'Group1,Group2,Group3'.split(',') | Get-Hierarchy -RecursionProperty MemberOf
```


![Alt text](/Examples/1.png?raw=true)
![Alt text](/Examples/2.png?raw=true)
![Alt text](/Examples/3.png?raw=true)
