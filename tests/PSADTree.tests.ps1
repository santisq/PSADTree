# I know, this is a shame but setting up a Domain for tests is too much trouble :D
$ErrorActionPreference = 'Stop'

Describe 'PSADTreeModule' {
    It 'Should not throw on import' {
        $moduleName = (Get-Item ([IO.Path]::Combine($PSScriptRoot, '..', 'module', '*.psd1'))).BaseName
        $manifestPath = [IO.Path]::Combine($PSScriptRoot, '..', 'output', $moduleName)

        { Import-Module $manifestPath } | Should -Not -Throw
    }
}
