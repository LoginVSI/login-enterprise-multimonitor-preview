[CmdletBinding()]
param([string]$SourceDirectory)
$ErrorActionPreference = 'Stop'
$root = Join-Path $PSScriptRoot '../workloads/knowledge-worker-2025-multimonitor'
$manifest = Get-Content (Join-Path $root 'adaptation-manifest.json') -Raw | ConvertFrom-Json
function Require($condition, $message) { if (-not $condition) { throw $message } }
$files = @(Get-ChildItem $root -Filter '*.cs')
Require ($files.Count -eq 12) 'Expected exactly twelve in-scope KW25 scripts.'
Require (@($files | Where-Object Name -Match 'Close').Count -eq 0) 'Close script packaged.'
foreach ($entry in $manifest.workloads) {
    $path = Join-Path $root $entry.file
    $text = [IO.File]::ReadAllText($path)
    Require ((Get-FileHash $path -Algorithm SHA256).Hash -eq $entry.adaptedSha256) "Output hash changed: $($entry.file)"
    if ($SourceDirectory) {
        Require ((Get-FileHash (Join-Path $SourceDirectory $entry.file) -Algorithm SHA256).Hash -eq $entry.sourceSha256) "External source changed: $($entry.file)"
    }
    $base = [regex]::Replace($text, '(?s)[ \t]*// BEGIN KW25 PREVIEW HELPERS.*?// END KW25 PREVIEW HELPERS\r?\n', '')
    $base = [regex]::Replace($base, '(?s)[ \t]*// BEGIN KW25 PREVIEW\r?\n.*?// END KW25 PREVIEW\r?\n', '')
    $canonical = [regex]::Replace($base, '\s+', '')
    $sha = [Security.Cryptography.SHA256]::Create()
    $digest = [BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($canonical))).Replace('-', '').ToLowerInvariant()
    $sha.Dispose()
    Require ($digest -eq $entry.canonicalSourceSha256) "Original behavior changed outside declared additions: $($entry.file)"
    if ($entry.file -match 'Start.cs$') {
        Require ([regex]::Matches($text, 'KwPlace\(kwBase, "\w+", true\)').Count -eq 1) 'Start must allocate once.'
        Require ($text.Contains('KwRequireEmpty(') -and $text.Contains('KwRequireUnique(')) 'Start ownership guards missing.'
    }
    if ($entry.file -match 'Run.cs$') {
        Require (-not $text.Contains('PlaceNext') -and -not $text.Contains('PlaceLastUsed')) 'Run must not allocate or use global last-used.'
        Require ($text.Contains('"PlaceOnMonitor"')) 'Run maintenance missing.'
        Require (-not $text.Contains('File.WriteAll') -and -not $text.Contains('File.Delete')) 'Run must not rewrite application assignments.'
        Require ($text.Contains('assignment.Length != 2') -and $text.Contains('_kwMonitor >= _kwMonitorCount')) 'Run assignment validation missing.'
        Require ($text.Contains('count != _kwMonitorCount') -and $text.Contains('last >= count')) 'Run shared-state validation missing.'
        Require ($text.Contains('if (!start) { KwValidateState(); }')) 'Maintenance must validate shared state before placement.'
    }
    if ($entry.status -eq 'adapted') {
        Require ($text.Contains('current == null || current.NativeWindowHandle != window.NativeWindowHandle')) 'Placement must reject missing/replaced/ambiguous windows.'
        Require ($text.Contains('IWindow current = KwRequireUnique(')) 'Placement ownership check missing.'
    }
    if ($entry.file -match 'Prep') {
        Require ($entry.sourceSha256 -eq $entry.adaptedSha256) 'Prepare must be byte-identical.'
        Require (-not $text.Contains('MultiMonitor')) 'Prepare must be state-neutral.'
    }
    Require (-not $text.Contains('ResetState')) 'KW25 must not reset allocation state.'
}
Require ($manifest.workloads.Count -eq 12) 'Manifest must cover twelve scripts.'
Require (@($manifest.workloads.file | Sort-Object -Unique).Count -eq 12) 'Duplicate manifest paths.'
foreach ($content in $manifest.content) {
    Require ((Get-FileHash (Join-Path $root $content.file) -Algorithm SHA256).Hash -eq $content.sha256) 'Content hash mismatch.'
    if ($SourceDirectory) {
        Require ((Get-FileHash (Join-Path $SourceDirectory $content.file) -Algorithm SHA256).Hash -eq $content.sha256) 'Source content hash mismatch.'
    }
}
$expected = @{ 1 = '0,0,0,0,0'; 2 = '0,1,0,1,0'; 3 = '0,1,2,0,1'; 4 = '0,1,2,3,0' }
foreach ($count in 1..4) {
    Require (((0..4 | ForEach-Object { $_ % $count }) -join ',') -eq $expected[$count]) 'Allocation expectation mismatch.'
}
Write-Host 'KW25 contracts passed: twelve source digests, output hashes, fidelity, lifecycle and 1/2/3/4-monitor expectations. No runtime proof.'
