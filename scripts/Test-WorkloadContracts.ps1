[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))

function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { throw $Message }
}

function Get-Text {
    param([string]$Path)
    return [System.IO.File]::ReadAllText($Path)
}

function Get-MatchesCount {
    param([string]$Text, [string]$Pattern)
    return [regex]::Matches($Text, $Pattern).Count
}

$workloadsRoot = Join-Path $repoRoot 'workloads'
$allWorkloadFiles = @(Get-ChildItem -LiteralPath $workloadsRoot -Filter '*.cs' -Recurse)

foreach ($file in $allWorkloadFiles) {
    $text = Get-Text $file.FullName
    Assert-True (-not $text.Contains('classname:')) "Invalid lowercase classname named argument: $($file.FullName)"
    Assert-True (-not $text.Contains('processname:')) "Invalid lowercase processname named argument: $($file.FullName)"

    if ($text.Contains('LoginVSI.MultiMonitor.dll')) {
        Assert-True $text.Contains('"LoginPI", "MultiMonitor"') "DLL consumer does not use the target-local Preview directory: $($file.FullName)"
        if ($file.Name -ne '00-Prepare-MultiMonitor.cs') {
            Assert-True $text.Contains('Assembly.LoadFrom') "DLL consumer does not load the staged assembly: $($file.FullName)"
        }
    }

    Assert-True (-not [regex]::IsMatch($text, '(WriteAllText|WriteAllLines|AppendAllText)[\s\S]{0,200}(NativeWindowHandle|monitorHandle)', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)) "A workload appears to persist a native handle: $($file.FullName)"
}

$canonicalClose = Get-Text (Join-Path $workloadsRoot 'dll-backed\02-Close-Applications.cs')
foreach ($forbidden in @('PlaceNext', 'ResetState', 'state.txt', 'NativeWindowHandle', 'LoginVSI.MultiMonitor.dll')) {
    Assert-True (-not $canonicalClose.Contains($forbidden)) "Canonical Close contains forbidden state/allocation token: $forbidden"
}

$officeRoot = Join-Path $workloadsRoot 'office-preview'
$officeApps = @(
    '10-Place-Microsoft-Word.cs',
    '20-Place-Microsoft-Excel.cs',
    '30-Place-Microsoft-PowerPoint.cs',
    '40-Place-Microsoft-Outlook.cs',
    '50-Place-Microsoft-Edge.cs'
)
foreach ($name in $officeApps) {
    $path = Join-Path $officeRoot $name
    Assert-True (Test-Path -LiteralPath $path -PathType Leaf) "Office Preview workload is missing: $name"
    $text = Get-Text $path
    Assert-True ((Get-MatchesCount $text 'placement\.PlaceNext\(') -eq 1) "Office Preview workload must allocate exactly once: $name"
    foreach ($required in @('START(', 'MainWindow', 'NativeWindowHandle', 'state.txt', 'LoginVSI.MultiMonitor.dll', 'StateAdvanced')) {
        Assert-True $text.Contains($required) "Office Preview workload lacks '$required': $name"
    }
}

$resetText = Get-Text (Join-Path $officeRoot '01-Reset-Placement-State.cs')
Assert-True $resetText.Contains('"ResetState"') 'Office Preview reset workload does not invoke ResetState.'
Assert-True (-not $resetText.Contains('"PlaceNext"')) 'Office Preview reset workload must not allocate.'

$manifestPath = Join-Path $workloadsRoot 'knowledge-worker-multimonitor\adaptation-manifest.json'
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$originalRoot = Join-Path $repoRoot $manifest.sourceDirectory
$adaptedRoot = Join-Path $repoRoot $manifest.adaptedDirectory
$originalFiles = @(Get-ChildItem -LiteralPath $originalRoot -Filter '*.cs' | Sort-Object Name)
Assert-True ($manifest.workloads.Count -eq $originalFiles.Count) 'Knowledge Worker manifest does not cover every preserved original workload.'

foreach ($entry in $manifest.workloads) {
    $originalPath = Join-Path $originalRoot $entry.file
    $adaptedPath = Join-Path $adaptedRoot $entry.file
    Assert-True (Test-Path -LiteralPath $originalPath -PathType Leaf) "Manifest source is missing: $($entry.file)"
    Assert-True (Test-Path -LiteralPath $adaptedPath -PathType Leaf) "Adapted workload is missing: $($entry.file)"

    $originalText = Get-Text $originalPath
    $adaptedText = Get-Text $adaptedPath
    $originalTarget = ([regex]::Match($originalText, '(?m)^// TARGET:.*$')).Value.TrimEnd()
    $adaptedTarget = ([regex]::Match($adaptedText, '(?m)^// TARGET:.*$')).Value.TrimEnd()
    Assert-True ($originalTarget -eq $adaptedTarget) "TARGET drifted from preserved original: $($entry.file)"

    $originalClass = ([regex]::Match($originalText, '(?m)^public class\s+([A-Za-z0-9_]+)')).Groups[1].Value
    $adaptedClass = ([regex]::Match($adaptedText, '(?m)^public class\s+([A-Za-z0-9_]+)')).Groups[1].Value
    Assert-True ($originalClass -eq $adaptedClass) "Primary script class drifted from preserved original: $($entry.file)"

    $addedLines = ([System.IO.File]::ReadAllLines($adaptedPath).Count - [System.IO.File]::ReadAllLines($originalPath).Count)
    Assert-True ($addedLines -le [int]$entry.maxAddedLines) "Adaptation exceeds its documented line-delta budget: $($entry.file) delta=$addedLines"

    $timerPattern = '(StartTimer|StopTimer|CancelTimer)\("([^"]+)"'
    $originalTimers = @([regex]::Matches($originalText, $timerPattern) | ForEach-Object { $_.Groups[2].Value } | Sort-Object -Unique)
    foreach ($timer in $originalTimers) {
        Assert-True $adaptedText.Contains('"' + $timer + '"') "Original timer '$timer' is missing from adaptation: $($entry.file)"
    }

    $allocationCalls = Get-MatchesCount $adaptedText '(InvokePlacement\("PlaceNext"|GetMethod\("PlaceNext"|\.PlaceNext\()'
    if ([bool]$entry.allocates) {
        Assert-True ($allocationCalls -eq 1) "Allocating adaptation must contain exactly one PlaceNext invocation: $($entry.file) count=$allocationCalls"
        foreach ($required in @('LoginVSI.MultiMonitor.dll', 'Assembly.LoadFrom', 'NativeWindowHandle', 'StateAdvanced')) {
            Assert-True $adaptedText.Contains($required) "Allocating adaptation lacks '$required': $($entry.file)"
        }
    }
    else {
        Assert-True ($allocationCalls -eq 0) "Non-allocating adaptation invokes PlaceNext: $($entry.file)"
    }

    if ($entry.type -like 'close/*') {
        foreach ($forbidden in @('ResetState', 'PlaceLastUsed', 'PlaceOnMonitor', 'state.txt', 'NativeWindowHandle')) {
            Assert-True (-not $adaptedText.Contains($forbidden)) "Close adaptation contains forbidden placement/state token '$forbidden': $($entry.file)"
        }
    }
}

$manifestNames = @($manifest.workloads | ForEach-Object { $_.file } | Sort-Object)
$originalNames = @($originalFiles | ForEach-Object { $_.Name } | Sort-Object)
Assert-True (($manifestNames -join "`n") -eq ($originalNames -join "`n")) 'Knowledge Worker manifest filenames differ from the preserved original set.'
Assert-True (-not (Test-Path -LiteralPath (Join-Path $workloadsRoot 'integrated'))) 'Obsolete workloads/integrated directory still exists.'

$stateSource = Get-Text (Join-Path $repoRoot 'src\LoginVSI.MultiMonitor\StateFileStore.cs')
Assert-True $stateSource.Contains('"MonitorCount="') 'State schema lost MonitorCount.'
Assert-True $stateSource.Contains('"LastUsedIndex="') 'State schema lost LastUsedIndex.'

foreach ($relativePath in @(
    'dist\LoginVSI.MultiMonitor.dll',
    'workloads\dll-backed\00-Prepare-MultiMonitor.cs',
    'workloads\office-preview\README.md',
    'workloads\knowledge-worker-multimonitor\README.md',
    'docs\test-lab-quickstart.md',
    '.github\workflows\repository-validation.yml'
)) {
    Assert-True (Test-Path -LiteralPath (Join-Path $repoRoot $relativePath) -PathType Leaf) "Required public/test-lab path is missing: $relativePath"
}

$trackedArtifacts = @(git -C $repoRoot ls-files -- 'artifacts/*' '*.log')
$unexpectedArtifacts = @($trackedArtifacts | Where-Object { $_ -ne 'artifacts/.gitkeep' })
Assert-True ($unexpectedArtifacts.Count -eq 0) ('Tracked raw artifact/log paths found: ' + ($unexpectedArtifacts -join ', '))

Write-Host "Workload/source contracts passed for $($allWorkloadFiles.Count) workload files and $($manifest.workloads.Count) preserved-workload adaptations." -ForegroundColor Green
