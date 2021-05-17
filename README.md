# Get-Hierarchy

### DESCRIPTION
Gets Group's and User's membership or Group parentship and draws it's hierarchy.
Helps identifying <b>Circular Nested Groups</b>.
    
### PARAMETER
`<Name>` // Objects of class User or Group.

### OUTPUTS
`<Object[]> // System.Array`

<ul>
    <li> InputParameter: The input user or group </li>
    <li> Index: The object on each level of recursion </li>
    <li> Recursion: The level of recursion or nesting </li>
    <li> Class: The objectClass </li>
    <li> Hierarchy: The hierarchy map of the input paremeter </li>
</ul>

### REQUIREMENTS
<ul>
    <li>PowerShell v5.1</li>
    <li>ActiveDirectory PS Module</li>
</ul>


### USAGE EXAMPLES

```
PS C:\> Get-Hierarchy ExampleGroup

PS C:\> gh ExampleGroup -RecursionProperty MemberOf

PS C:\> Get-ADGroup ExampleGroup | Get-Hierarchy -RecursionProperty MemberOf

PS C:\> Get-ADGroup ExampleGroup | gh

PS C:\> Get-ADGroup -Filter {Name -like 'ExampleGroups*'} | Get-Hierarchy
           
PS C:\> 'Group1,Group2,Group3'.split(',') | Get-Hierarchy -RecursionProperty MemberOf
```


![Alt text](/Examples/1.png?raw=true)
![Alt text](/Examples/2.png?raw=true)
![Alt text](/Examples/3.png?raw=true)
