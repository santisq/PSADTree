function Measure-Expression {
    [CmdletBinding()]
    [Alias('measureme', 'time')]
    param(
        [Parameter(Mandatory, Position = 0)]
        [hashtable] $Tests,

        [Parameter()]
        [int32] $TestCount = 5,

        [Parameter()]
        [switch] $OutputAllTests,

        [Parameter()]
        [hashtable] $Parameters
    )

    end {
        $allTests = 1..$TestCount | ForEach-Object {
            foreach ($test in $Tests.GetEnumerator()) {
                $sb = if ($Parameters) {
                    { & $test.Value @Parameters }
                }
                else {
                    { & $test.Value }
                }

                $now = [datetime]::Now
                $null = $sb.Invoke()
                $span = [datetime]::Now - $now

                [pscustomobject]@{
                    TestRun  = $_
                    Test     = $test.Key
                    TimeSpan = $span
                }

                [GC]::Collect()
                [GC]::WaitForPendingFinalizers()
            }
        } | Sort-Object TotalMilliseconds

        $average = $allTests | Group-Object Test | ForEach-Object {
            $avg = [Linq.Enumerable]::Average([long[]] $_.Group.TimeSpan.Ticks)

            [pscustomobject]@{
                Test    = $_.Name
                Average = $avg
            }
        } | Sort-Object Average

        $average | Select-Object @(
            'Test'
            @{
                Name       = "Average ($TestCount invocations)"
                Expression = { [timespan]::new($_.Average).ToString('hh\:mm\:ss\.fff') }
            }
            @{
                Name       = 'RelativeSpeed'
                Expression = {
                    $relativespeed = $_.Average / $average[0].Average
                    [math]::Round($relativespeed, 2).ToString() + 'x'
                }
            }
        )

        if ($OutputAllTests.IsPresent) {
            $allTests
        }
    }
}

$modulePath = Convert-Path $PSScriptRoot\..\output\PSADTree

Measure-Expression -TestCount 5 @{
    'PSADTree v1.1.6-pwsh-7'   = { pwsh -File $PSScriptRoot\perfCode.ps1 $modulePath }
    'PSADTree v1.1.6-pwsh-5.1' = { powershell -File $PSScriptRoot\perfCode.ps1 $modulePath }
    'PSADTree v1.1.5-pwsh-7'   = { pwsh -File $PSScriptRoot\perfCode.ps1 PSADTree }
    'PSADTree v1.1.5-pwsh-5.1' = { powershell -File $PSScriptRoot\perfCode.ps1 PSADTree }
}
