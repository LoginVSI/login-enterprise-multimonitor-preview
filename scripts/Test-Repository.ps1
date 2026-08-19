[CmdletBinding()]
param([switch]$Fast)

$ErrorActionPreference = 'Stop'
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))

function Invoke-Native {
    param([string]$FilePath, [string[]]$Arguments)
    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$FilePath failed with exit code $LASTEXITCODE." }
}

function Invoke-RepositoryScript {
    param([string]$Path, [string[]]$Arguments = @())
    $global:LASTEXITCODE = 0
    & $Path @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$Path failed with exit code $LASTEXITCODE." }
}

Push-Location $repoRoot
try {
    Write-Host '=== Repository whitespace/integrity ==='
    Invoke-Native 'git' @('diff', '--check')
    Invoke-Native 'git' @('diff', '--cached', '--check')
    Invoke-Native 'git' @('show', '--check', '--format=', 'HEAD')

    Write-Host '=== Preserved references ==='
    $global:LASTEXITCODE = 0
    & (Join-Path $PSScriptRoot 'Verify-ReferenceHashes.ps1') -Verify
    if ($LASTEXITCODE -ne 0) { throw 'Reference hash verification failed.' }
    $global:LASTEXITCODE = 0
    & (Join-Path $PSScriptRoot 'Verify-PreservedEvidenceHashes.ps1') -Verify
    if ($LASTEXITCODE -ne 0) { throw 'Preserved-evidence hash verification failed.' }

    Write-Host '=== Public safety ==='
    Invoke-RepositoryScript -Path (Join-Path $PSScriptRoot 'Test-PublicSafety.ps1')

    Write-Host '=== Static contracts ==='
    & (Join-Path $PSScriptRoot 'Test-DllContract.ps1')
    & (Join-Path $PSScriptRoot 'Test-WorkloadContracts.ps1')

    if ($Fast) {
        Write-Host 'Fast mode skipped restore/build/unit execution.' -ForegroundColor Yellow
    }
    else {
        Write-Host '=== Restore, build, unit/pure-logic/source-contract tests ==='
        Invoke-RepositoryScript -Path (Join-Path $repoRoot 'build.ps1')
    }

    Write-Host 'Repository validation completed successfully.' -ForegroundColor Green
}
finally {
    Pop-Location
}
