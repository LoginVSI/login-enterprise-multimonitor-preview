[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = [System.IO.Path]::GetFullPath($PSScriptRoot).TrimEnd('\')
$libraryProject = Join-Path $repoRoot 'src\LoginVSI.MultiMonitor\LoginVSI.MultiMonitor.csproj'
$testProject = Join-Path $repoRoot 'tests\LoginVSI.MultiMonitor.Tests\LoginVSI.MultiMonitor.Tests.csproj'
$distributionDirectory = Join-Path $repoRoot 'dist'
$distributionDll = Join-Path $distributionDirectory 'LoginVSI.MultiMonitor.dll'
$distributionChecksums = Join-Path $distributionDirectory 'SHA256SUMS.txt'

function Invoke-DotNet {
    param([Parameter(Mandatory = $true)][string[]]$Arguments)

    Write-Host ('dotnet ' + ($Arguments -join ' '))
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0)
    {
        throw "dotnet failed with exit code $LASTEXITCODE."
    }
}

function Remove-RepositoryBuildTree {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path))
    {
        return
    }

    $resolved = [System.IO.Path]::GetFullPath($Path)
    $repoPrefix = $repoRoot + [System.IO.Path]::DirectorySeparatorChar
    if (-not $resolved.StartsWith($repoPrefix, [System.StringComparison]::OrdinalIgnoreCase))
    {
        throw "Refusing to remove a build directory outside the repository: $resolved"
    }

    Remove-Item -LiteralPath $resolved -Recurse -Force
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue))
{
    throw 'The dotnet SDK is required but was not found on PATH.'
}

Remove-RepositoryBuildTree (Join-Path $repoRoot 'src\LoginVSI.MultiMonitor\bin')
Remove-RepositoryBuildTree (Join-Path $repoRoot 'src\LoginVSI.MultiMonitor\obj')
Remove-RepositoryBuildTree (Join-Path $repoRoot 'tests\LoginVSI.MultiMonitor.Tests\bin')
Remove-RepositoryBuildTree (Join-Path $repoRoot 'tests\LoginVSI.MultiMonitor.Tests\obj')

Invoke-DotNet @('restore', $testProject)
Invoke-DotNet @('build', $testProject, '--configuration', 'Release', '--no-restore')
Invoke-DotNet @('run', '--project', $testProject, '--configuration', 'Release', '--no-build')

$builtDll = Join-Path $repoRoot 'src\LoginVSI.MultiMonitor\bin\Release\netstandard2.0\LoginVSI.MultiMonitor.dll'
if (-not (Test-Path -LiteralPath $builtDll -PathType Leaf))
{
    throw "Expected library output was not found: $builtDll"
}

New-Item -ItemType Directory -Path $distributionDirectory -Force | Out-Null
Copy-Item -LiteralPath $builtDll -Destination $distributionDll -Force
$distributionHash = (Get-FileHash -LiteralPath $distributionDll -Algorithm SHA256).Hash.ToLowerInvariant()
[System.IO.File]::WriteAllText($distributionChecksums, $distributionHash + '  LoginVSI.MultiMonitor.dll' + [Environment]::NewLine, (New-Object System.Text.UTF8Encoding($false)))
Write-Host "Build, tests, and distribution copy completed: $distributionDll" -ForegroundColor Green
