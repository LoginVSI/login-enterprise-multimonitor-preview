[CmdletBinding(DefaultParameterSetName = 'Verify')]
param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Generate')]
    [switch]$Generate,

    [Parameter(Mandatory = $true, ParameterSetName = 'Verify')]
    [switch]$Verify
)

$ErrorActionPreference = 'Stop'
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$pocRoot = Join-Path $repoRoot 'reference\proven-pocs'
$scenarioPath = Join-Path $repoRoot 'reference\test-scenario\workload-sequence.txt'
$manifestPath = Join-Path $repoRoot 'reference\PRESERVED-EVIDENCE-SHA256SUMS.txt'

function Get-PreservedEvidenceFiles {
    $files = New-Object System.Collections.Generic.List[System.IO.FileInfo]

    if (Test-Path -LiteralPath $pocRoot -PathType Container) {
        foreach ($file in (Get-ChildItem -LiteralPath $pocRoot -File -Recurse |
            Where-Object { $_.Name -ne 'README.md' })) {
            $files.Add($file)
        }
    }

    if (Test-Path -LiteralPath $scenarioPath -PathType Leaf) {
        $files.Add((Get-Item -LiteralPath $scenarioPath))
    }

    return @($files | Sort-Object { (Get-RepoRelativePath $_).ToLowerInvariant() })
}

function Get-RepoRelativePath([System.IO.FileInfo]$File) {
    return $File.FullName.Substring($repoRoot.Length).TrimStart('\', '/').Replace('\', '/')
}

if ($Generate) {
    Write-Warning 'Replacing the preserved-evidence hash manifest. Do this only after the POCs and authoritative scenario transcription have been reviewed.'
    $files = Get-PreservedEvidenceFiles
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add('# SHA-256 manifest for preserved POC source and the authoritative scenario transcription.')
    $lines.Add('# Generated intentionally with scripts/Verify-PreservedEvidenceHashes.ps1 -Generate.')
    $lines.Add('# The supporting scenario PNG is intentionally excluded.')

    foreach ($file in $files) {
        $relativePath = Get-RepoRelativePath $file
        $hash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        $lines.Add(('{0}  {1}' -f $hash, $relativePath))
    }

    if ($files.Count -eq 0) {
        $lines.Add('# No preserved evidence files were present when this manifest was generated.')
        Write-Warning 'No preserved evidence files were found; an empty manifest was written.'
    }

    [System.IO.File]::WriteAllLines($manifestPath, $lines, (New-Object System.Text.UTF8Encoding($false)))
    Write-Host ('Generated {0} hash entries in {1}' -f $files.Count, $manifestPath)
    exit 0
}

if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    Write-Error "Manifest not found: $manifestPath"
    exit 2
}

$expected = @{}
$parseErrors = New-Object System.Collections.Generic.List[string]
$lineNumber = 0
foreach ($line in (Get-Content -LiteralPath $manifestPath)) {
    $lineNumber++
    if ([string]::IsNullOrWhiteSpace($line) -or $line.TrimStart().StartsWith('#')) { continue }
    if ($line -notmatch '^([0-9A-Fa-f]{64})\s{2,}(.+)$') {
        $parseErrors.Add("Invalid manifest line $lineNumber")
        continue
    }

    $relativePath = $Matches[2].Replace('\', '/').TrimStart('/')
    if ($expected.ContainsKey($relativePath)) {
        $parseErrors.Add("Duplicate manifest path: $relativePath")
        continue
    }
    $expected[$relativePath] = $Matches[1].ToLowerInvariant()
}

$actual = @{}
foreach ($file in (Get-PreservedEvidenceFiles)) {
    $relativePath = Get-RepoRelativePath $file
    $actual[$relativePath] = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
}

$modified = @($expected.Keys | Where-Object { $actual.ContainsKey($_) -and $actual[$_] -ne $expected[$_] } | Sort-Object)
$missing = @($expected.Keys | Where-Object { -not $actual.ContainsKey($_) } | Sort-Object)
$unexpected = @($actual.Keys | Where-Object { -not $expected.ContainsKey($_) } | Sort-Object)

foreach ($message in $parseErrors) { Write-Host "ERROR: $message" -ForegroundColor Red }
foreach ($path in $modified) { Write-Host "MODIFIED: $path" -ForegroundColor Red }
foreach ($path in $missing) { Write-Host "MISSING: $path" -ForegroundColor Red }
foreach ($path in $unexpected) { Write-Host "UNEXPECTED: $path" -ForegroundColor Red }

if ($parseErrors.Count -gt 0 -or $modified.Count -gt 0 -or $missing.Count -gt 0 -or $unexpected.Count -gt 0) {
    Write-Host 'Preserved-evidence verification failed.' -ForegroundColor Red
    exit 1
}

if ($expected.Count -eq 0) {
    Write-Warning 'Preserved-evidence verification succeeded, but the manifest contains no files.'
} else {
    Write-Host ("Preserved-evidence verification succeeded for {0} file(s)." -f $expected.Count) -ForegroundColor Green
}
exit 0
