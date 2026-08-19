[CmdletBinding()]
param(
    [string]$OutputPath,
    [switch]$Zip
)

$ErrorActionPreference = 'Stop'
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $repoRoot 'artifacts\test-lab-bundle'
}

$resolvedOutput = [System.IO.Path]::GetFullPath($OutputPath)
$artifactsRoot = [System.IO.Path]::GetFullPath((Join-Path $repoRoot 'artifacts')).TrimEnd('\') + '\'
if (-not $resolvedOutput.StartsWith($artifactsRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Bundle output must remain under the ignored artifacts directory: $artifactsRoot"
}

& (Join-Path $PSScriptRoot 'Test-DllContract.ps1')

if (Test-Path -LiteralPath $resolvedOutput) {
    Remove-Item -LiteralPath $resolvedOutput -Recurse -Force
}

New-Item -ItemType Directory -Path $resolvedOutput -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $resolvedOutput 'dist') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $resolvedOutput 'workloads\dll-backed') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $resolvedOutput 'workloads') -Force | Out-Null

Copy-Item -LiteralPath (Join-Path $repoRoot 'dist\LoginVSI.MultiMonitor.dll') -Destination (Join-Path $resolvedOutput 'dist')
Copy-Item -LiteralPath (Join-Path $repoRoot 'dist\SHA256SUMS.txt') -Destination (Join-Path $resolvedOutput 'dist')
Copy-Item -LiteralPath (Join-Path $repoRoot 'workloads\dll-backed\00-Prepare-MultiMonitor.cs') -Destination (Join-Path $resolvedOutput 'workloads\dll-backed')
Copy-Item -LiteralPath (Join-Path $repoRoot 'workloads\office-preview') -Destination (Join-Path $resolvedOutput 'workloads') -Recurse
Copy-Item -LiteralPath (Join-Path $repoRoot 'workloads\knowledge-worker-multimonitor') -Destination (Join-Path $resolvedOutput 'workloads') -Recurse
Copy-Item -LiteralPath (Join-Path $repoRoot 'docs\test-lab-quickstart.md') -Destination (Join-Path $resolvedOutput 'README.md')

Write-Host "Created ignored test-lab bundle directory: $resolvedOutput" -ForegroundColor Green
if ($Zip) {
    $zipPath = $resolvedOutput + '.zip'
    if (Test-Path -LiteralPath $zipPath) { Remove-Item -LiteralPath $zipPath -Force }
    Compress-Archive -Path (Join-Path $resolvedOutput '*') -DestinationPath $zipPath -CompressionLevel Optimal
    Write-Host "Created ignored test-lab bundle archive: $zipPath" -ForegroundColor Green
}
