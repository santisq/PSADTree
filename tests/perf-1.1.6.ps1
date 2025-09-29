$func = 'https://gist.githubusercontent.com/santisq/bd3d1d47c89f030be1b4e57b92baaddd/raw/aa78870a9674e9e4769b05e333586bf405c1362c/Measure-Expression.ps1'
Invoke-RestMethod $func | Invoke-Expression

$modulePath = Convert-Path $PSScriptRoot\..\output\PSADTree

Measure-Expression @{
    'v1.1.6-pwsh-7'   = { pwsh -File $PSScriptRoot\perfCode.ps1 $modulePath }
    'v1.1.6-pwsh-5.1' = { powershell -File $PSScriptRoot\perfCode.ps1 $modulePath }
    'v1.1.5-pwsh-7'   = { pwsh -File $PSScriptRoot\perfCode.ps1 PSADTree }
    'v1.1.5-pwsh-5.1' = { powershell -File $PSScriptRoot\perfCode.ps1 PSADTree }
}
