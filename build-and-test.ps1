#!/usr/bin/pwsh

[CmdletBinding(PositionalBinding=$false)]
param (
    [string]$configuration = "Debug",
    [switch]$noTest
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

try {
    # run code generator
    Push-Location "$PSScriptRoot/src/IxMilia.Dwg.Generator"
    dotnet restore
    dotnet build
    dotnet run -- "$PSScriptRoot/src/IxMilia.Dwg"
    Pop-Location

    # build
    $solution = "$PSScriptRoot/IxMilia.Dwg.sln"
    dotnet restore $solution
    dotnet build $solution -c $configuration

    # test
    if (-Not $noTest) {
        dotnet test --no-restore --no-build -c $configuration
    }
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
