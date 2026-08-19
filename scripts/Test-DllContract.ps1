[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$projectPath = Join-Path $repoRoot 'src\LoginVSI.MultiMonitor\LoginVSI.MultiMonitor.csproj'
[xml]$project = Get-Content -LiteralPath $projectPath -Raw

$targetFramework = [string]$project.Project.PropertyGroup.TargetFramework
if ($targetFramework -ne 'netstandard2.0') { throw "Unexpected reusable DLL target framework: $targetFramework" }

$packageReferences = @($project.SelectNodes('//PackageReference'))
$assemblyReferences = @($project.SelectNodes('//Reference'))
if ($packageReferences.Count -ne 0 -or $assemblyReferences.Count -ne 0) {
    throw 'The reusable DLL project must remain dependency-free.'
}

$sourceHits = @(Get-ChildItem (Join-Path $repoRoot 'src\LoginVSI.MultiMonitor') -Filter '*.cs' | Select-String -Pattern 'LoginPI\.Engine')
if ($sourceHits.Count -ne 0) { throw 'Reusable DLL source contains a LoginPI.Engine dependency.' }

$distributionDll = Join-Path $repoRoot 'dist\LoginVSI.MultiMonitor.dll'
$checksumPath = Join-Path $repoRoot 'dist\SHA256SUMS.txt'
if (-not (Test-Path -LiteralPath $distributionDll -PathType Leaf)) { throw 'The distributable DLL is missing.' }
if (-not (Test-Path -LiteralPath $checksumPath -PathType Leaf)) { throw 'dist/SHA256SUMS.txt is missing.' }
$checksumLine = @(Get-Content -LiteralPath $checksumPath | Where-Object { -not [string]::IsNullOrWhiteSpace($_) -and -not $_.TrimStart().StartsWith('#') })
$checksumMatch = if ($checksumLine.Count -eq 1) { [regex]::Match($checksumLine[0], '^([0-9a-fA-F]{64})\s{2}LoginVSI\.MultiMonitor\.dll$') } else { $null }
if ($null -eq $checksumMatch -or -not $checksumMatch.Success) { throw 'dist/SHA256SUMS.txt has an unexpected format.' }
$actualHash = (Get-FileHash -LiteralPath $distributionDll -Algorithm SHA256).Hash.ToLowerInvariant()
if ($actualHash -ne $checksumMatch.Groups[1].Value.ToLowerInvariant()) { throw 'The distributable DLL does not match dist/SHA256SUMS.txt.' }

Write-Host 'DLL contract passed: netstandard2.0, dependency-free, no LoginPI.Engine dependency, checksum verified.' -ForegroundColor Green
