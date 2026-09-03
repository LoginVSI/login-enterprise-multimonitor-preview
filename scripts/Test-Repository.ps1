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
    & (Join-Path $PSScriptRoot 'Test-Documentation.ps1')

    if ($Fast) {
        Write-Host 'Fast mode skipped restore/build/unit execution.' -ForegroundColor Yellow
    }
    else {
        Write-Host '=== Restore, build, unit/pure-logic/source-contract tests ==='
        Invoke-RepositoryScript -Path (Join-Path $repoRoot 'build.ps1')
        Write-Host '=== Post-build distributable contract ==='
        & (Join-Path $PSScriptRoot 'Test-DllContract.ps1')

        # build.ps1 always rewrites dist/. The DLL embeds its build path, so a build from another
        # checkout path or SDK legitimately differs from the committed distributable; surface that
        # instead of leaving a silently modified binary in the working tree.
        $distributionDrift = @(git -C $repoRoot status --porcelain -- dist)
        if ($distributionDrift.Count -gt 0) {
            Write-Warning 'build.ps1 rewrote dist/ and it no longer matches the committed distributable. Run "git checkout -- dist" unless you intend to publish the rebuilt DLL and checksum together.'
        }
    }

    Write-Host 'Repository validation completed successfully.' -ForegroundColor Green
}
finally {
    Pop-Location
}
