#!/usr/bin/pwsh

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

function Fail([string]$message) {
    throw $message
}

try {
    Push-Location "$PSScriptRoot/src/IxMilia.Dwg.Generator"
    dotnet run -- "$PSScriptRoot/src/IxMilia.Dwg/Generated" || Fail "Error running generator."
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
finally {
    Pop-Location
}
