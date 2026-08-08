[CmdletBinding(DefaultParameterSetName = 'Verify')]
param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Generate')]
    [switch]$Generate,

    [Parameter(Mandatory = $true, ParameterSetName = 'Verify')]
    [switch]$Verify
)

$ErrorActionPreference = 'Stop'
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$referenceRoot = Join-Path $repoRoot 'reference\original-workloads'
$manifestPath = Join-Path $referenceRoot 'SHA256SUMS.txt'
$excludedNames = @('README.md', 'SHA256SUMS.txt')

function Get-ReferenceFiles {
    if (-not (Test-Path -LiteralPath $referenceRoot -PathType Container)) {
        return @()
    }

    return @(Get-ChildItem -LiteralPath $referenceRoot -File -Recurse |
        Where-Object { $excludedNames -notcontains $_.Name } |
        Sort-Object { $_.FullName.Substring($referenceRoot.Length).Replace('\', '/').ToLowerInvariant() })
}

function Get-RelativeReferencePath([System.IO.FileInfo]$File) {
    return $File.FullName.Substring($referenceRoot.Length).TrimStart('\', '/').Replace('\', '/')
}

if ($Generate) {
    Write-Warning 'Replacing the immutable reference hash manifest. Do this only after the complete baseline set has been reviewed.'
    $files = Get-ReferenceFiles
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add('# SHA-256 manifest for immutable files in reference/original-workloads/.')
    $lines.Add('# Generated intentionally with scripts/Verify-ReferenceHashes.ps1 -Generate.')

    foreach ($file in $files) {
        $relativePath = Get-RelativeReferencePath $file
        $hash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        $lines.Add(('{0}  {1}' -f $hash, $relativePath))
    }

    if ($files.Count -eq 0) {
        $lines.Add('# No baseline workload files were present when this manifest was generated.')
        Write-Warning 'No baseline workload files were found; an empty manifest was written.'
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
foreach ($file in (Get-ReferenceFiles)) {
    $relativePath = Get-RelativeReferencePath $file
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
    Write-Host 'Reference verification failed.' -ForegroundColor Red
    exit 1
}

if ($expected.Count -eq 0) {
    Write-Warning 'Reference verification succeeded, but the manifest contains no baseline files.'
} else {
    Write-Host ("Reference verification succeeded for {0} file(s)." -f $expected.Count) -ForegroundColor Green
}
exit 0
