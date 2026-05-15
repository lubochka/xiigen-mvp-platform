$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$dotnet = if (Test-Path "$env:USERPROFILE\.dotnet\dotnet.exe") { "$env:USERPROFILE\.dotnet\dotnet.exe" } else { "dotnet" }

& $dotnet restore "$root\Xiigen.Platform.slnx" --locked-mode
& $dotnet build "$root\Xiigen.Platform.slnx" -c Release --no-restore
& $dotnet test "$root\Xiigen.Platform.slnx" -c Release --no-build

Write-Host "Xiigen platform pre-commit gate passed."
