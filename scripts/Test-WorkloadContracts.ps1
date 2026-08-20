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
    '41-Place-Microsoft-Outlook-New.cs',
    '50-Place-Microsoft-Edge.cs'
)
foreach ($name in $officeApps) {
    $path = Join-Path $officeRoot $name
    Assert-True (Test-Path -LiteralPath $path -PathType Leaf) "Office Preview workload is missing: $name"
    $text = Get-Text $path
    Assert-True ((Get-MatchesCount $text 'placement\.PlaceNext\(') -eq 1) "Office Preview workload must allocate exactly once: $name"
    foreach ($required in @('NativeWindowHandle', 'state.txt', 'LoginVSI.MultiMonitor.dll', 'StateAdvanced')) {
        Assert-True $text.Contains($required) "Office Preview workload lacks '$required': $name"
    }
    Assert-True (-not [regex]::IsMatch($text, '(?i)[A-Z]:\\Program Files')) "Office Preview workload uses an absolute application path: $name"
}

$officePreflightContracts = @{
    '10-Place-Microsoft-Word.cs' = @('RequireNoExistingWordWindow', 'className: "Win32 Window:OpusApp"', 'processName: "WINWORD"', 'START(')
    '20-Place-Microsoft-Excel.cs' = @('RequireNoExistingExcelWindow', 'className: "*XLMAIN*"', 'processName: "EXCEL"', 'START(')
    '30-Place-Microsoft-PowerPoint.cs' = @('RequireNoExistingPowerPointWindow', 'className: "*PPTFrameClass*"', 'processName: "POWERPNT"', 'START(')
    '40-Place-Microsoft-Outlook.cs' = @('RequireNoExistingClassicOutlookWindow', 'className: "Win32 Window:rctrl_renwnd32"', 'processName: "OUTLOOK"', 'START(')
}
foreach ($name in $officePreflightContracts.Keys) {
    $text = Get-Text (Join-Path $officeRoot $name)
    foreach ($required in $officePreflightContracts[$name]) {
        Assert-True $text.Contains($required) "Office ownership preflight lacks '$required': $name"
    }
    Assert-True $text.Contains('FindWindows(') "Office ownership preflight does not inspect existing windows: $name"
}

$officeOutlook = Get-Text (Join-Path $officeRoot '40-Place-Microsoft-Outlook.cs')
Assert-True (-not $officeOutlook.Contains('Inbox*')) 'Generic Office Outlook example regressed to an English/folder-specific Inbox title.'
foreach ($required in @('// TARGET:outlook.exe', 'className: "Win32 Window:rctrl_renwnd32"', 'processName: "OUTLOOK"', 'Microsoft Outlook (Classic)')) {
    Assert-True $officeOutlook.Contains($required) "Classic Outlook Preview contract lacks '$required'."
}
$officeOutlookNew = Get-Text (Join-Path $officeRoot '41-Place-Microsoft-Outlook-New.cs')
foreach ($required in @('// TARGET:olk', 'START();', 'MainWindow', 'Microsoft Outlook (New)')) {
    Assert-True $officeOutlookNew.Contains($required) "New Outlook Preview contract lacks '$required'."
}
foreach ($forbidden in @('rctrl_renwnd32', 'processName: "OUTLOOK"', '// TARGET:outlook.exe', 'STOP(')) {
    Assert-True (-not $officeOutlookNew.Contains($forbidden)) "New Outlook Preview contains a Classic Outlook or conflicting lifecycle assumption: $forbidden"
}
$officeEdge = Get-Text (Join-Path $officeRoot '50-Place-Microsoft-Edge.cs')
foreach ($required in @('RequireNoExistingEdgeWindow', 'START(processName: "msedge"', 'IWindow edge = MainWindow', 'RequireUniqueEdgeWindow', 'edge.NativeWindowHandle != resolvedEdge.NativeWindowHandle', 'count != 1')) {
    Assert-True $officeEdge.Contains($required) "Office Edge durable START/MainWindow ownership contract lacks '$required'."
}
foreach ($forbidden in @('ShellExecute(', '--new-window about:blank', 'CaptureEdgeWindowHandles', 'FindUniqueNewEdgeWindow', 'HashSet<IntPtr>')) {
    Assert-True (-not $officeEdge.Contains($forbidden)) "Office Edge regressed to the transient raw launch path: $forbidden"
}
$officeReadme = Get-Text (Join-Path $officeRoot 'README.md')
foreach ($required in @('Choose one Outlook flavor', 'Microsoft Outlook (Classic)', 'Microsoft Outlook (New)', 'Do not run both Outlook variants', 'launch/find/place only')) {
    Assert-True $officeReadme.Contains($required) "Office README does not distinguish Outlook flavors: $required"
}

$adaptationSkill = Get-Text (Join-Path $repoRoot 'skills\login-enterprise-multimonitor\SKILL.md')
foreach ($required in @('Classic Outlook and New Outlook', 'Do not silently substitute', 'substantive workload adaptation', 'launch success does not prove interaction compatibility')) {
    Assert-True $adaptationSkill.Contains($required) "Repository skill lacks Outlook flavor guidance: $required"
}
$agenticGuide = Get-Text (Join-Path $repoRoot 'docs\agentic-workload-adaptation.md')
foreach ($required in @('Classic Outlook or New Outlook', 'Merely changing `TARGET` or an executable is not a valid conversion', 'substantive adaptation')) {
    Assert-True $agenticGuide.Contains($required) "Agentic adaptation guide lacks Outlook flavor guidance: $required"
}
$manualGuide = Get-Text (Join-Path $repoRoot 'docs\adapt-your-own-workload.md')
foreach ($required in @('Classic Outlook and New Outlook', 'Do not silently substitute', 'launch success does not prove interaction compatibility')) {
    Assert-True $manualGuide.Contains($required) "Manual adaptation guide lacks Outlook flavor guidance: $required"
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
    $originalTimerSequence = @([regex]::Matches($originalText, $timerPattern) | ForEach-Object { $_.Groups[1].Value + ':' + $_.Groups[2].Value })
    $adaptedTimerSequence = @([regex]::Matches($adaptedText, $timerPattern) | ForEach-Object { $_.Groups[1].Value + ':' + $_.Groups[2].Value })
    $adaptedOffset = 0
    foreach ($originalTimerCall in $originalTimerSequence) {
        while ($adaptedOffset -lt $adaptedTimerSequence.Count -and $adaptedTimerSequence[$adaptedOffset] -ne $originalTimerCall) { $adaptedOffset++ }
        Assert-True ($adaptedOffset -lt $adaptedTimerSequence.Count) "Original ordered timer call '$originalTimerCall' is missing/reordered: $($entry.file)"
        $adaptedOffset++
    }
    $originalTimers = @([regex]::Matches($originalText, $timerPattern) | ForEach-Object { $_.Groups[2].Value } | Sort-Object -Unique)
    $adaptedTimers = @([regex]::Matches($adaptedText, $timerPattern) | ForEach-Object { $_.Groups[2].Value } | Sort-Object -Unique)
    foreach ($timer in $adaptedTimers) {
        Assert-True ($originalTimers -contains $timer) "Adaptation introduced an undocumented timer name '$timer': $($entry.file)"
    }
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

$substitutions = @($manifest.publicSafetySubstitutions)
Assert-True ($substitutions.Count -eq 3) 'Knowledge Worker public-safety substitutions are not completely disclosed.'
foreach ($id in @('outlook-example-recipients', 'edge-customer-target', 'edge-local-media-path')) {
    Assert-True (@($substitutions | Where-Object { $_.id -eq $id }).Count -eq 1) "Missing or duplicate Knowledge Worker substitution disclosure: $id"
}
$adaptedOutlook = Get-Text (Join-Path $adaptedRoot '(KW) Microsoft Outlook.cs')
Assert-True $adaptedOutlook.Contains('@example.invalid') 'Adapted Outlook recipients are not reserved public-safe placeholders.'
Assert-True (-not $adaptedOutlook.Contains('@loginvsi.com')) 'Adapted Outlook workload contains supplied corporate recipients.'
$adaptedEdgeStart = Get-Text (Join-Path $adaptedRoot 'KW25 Edge Start-4KVideoHeavy.cs')
Assert-True $adaptedEdgeStart.Contains('"about:blank;"') 'Adapted Edge workload lost its disclosed public-safe target substitution.'
Assert-True (-not $adaptedEdgeStart.Contains('/customer-portal/')) 'Adapted Edge workload contains the supplied customer-oriented target.'
$edgeStartEntry = @($manifest.workloads | Where-Object { $_.file -eq 'KW25 Edge Start-4KVideoHeavy.cs' })[0]
Assert-True $edgeStartEntry.change.Contains('about:blank') 'Edge manifest entry does not disclose the meaningful URL substitution.'

$manifestNames = @($manifest.workloads | ForEach-Object { $_.file } | Sort-Object)
$originalNames = @($originalFiles | ForEach-Object { $_.Name } | Sort-Object)
Assert-True (($manifestNames -join "`n") -eq ($originalNames -join "`n")) 'Knowledge Worker manifest filenames differ from the preserved original set.'
Assert-True (-not (Test-Path -LiteralPath (Join-Path $workloadsRoot 'integrated'))) 'Obsolete workloads/integrated directory still exists.'

$stateSource = Get-Text (Join-Path $repoRoot 'src\LoginVSI.MultiMonitor\StateFileStore.cs')
Assert-True $stateSource.Contains('"MonitorCount="') 'State schema lost MonitorCount.'
Assert-True $stateSource.Contains('"LastUsedIndex="') 'State schema lost LastUsedIndex.'

foreach ($relativePath in @(
    'dist\LoginVSI.MultiMonitor.dll',
    'dist\SHA256SUMS.txt',
    'workloads\dll-backed\00-Prepare-MultiMonitor.cs',
    'workloads\office-preview\README.md',
    'workloads\knowledge-worker-multimonitor\README.md',
    'docs\test-lab-quickstart.md',
    '.github\workflows\repository-validation.yml',
    '.github\dependabot.yml',
    'SECURITY.md',
    'docs\getting-started.md',
    'docs\adapt-your-own-workload.md',
    'docs\agentic-workload-adaptation.md',
    'docs\troubleshooting.md',
    'scripts\New-TestLabBundle.ps1',
    'scripts\Test-Documentation.ps1'
)) {
    Assert-True (Test-Path -LiteralPath (Join-Path $repoRoot $relativePath) -PathType Leaf) "Required public/test-lab path is missing: $relativePath"
}

$workflow = Get-Text (Join-Path $repoRoot '.github\workflows\repository-validation.yml')
$actionReferences = @([regex]::Matches($workflow, '(?m)^\s*uses:\s*[^@\s]+@([^\s#]+)') | ForEach-Object { $_.Groups[1].Value })
Assert-True ($actionReferences.Count -gt 0) 'GitHub Actions workflow has no action references.'
foreach ($reference in $actionReferences) {
    Assert-True ($reference -match '^[0-9a-f]{40}$') "GitHub Action is not pinned to an immutable full commit SHA: $reference"
}

$trackedArtifacts = @(git -C $repoRoot ls-files -- 'artifacts/*' '*.log')
$unexpectedArtifacts = @($trackedArtifacts | Where-Object { $_ -ne 'artifacts/.gitkeep' })
Assert-True ($unexpectedArtifacts.Count -eq 0) ('Tracked raw artifact/log paths found: ' + ($unexpectedArtifacts -join ', '))

Write-Host "Workload/source contracts passed for $($allWorkloadFiles.Count) workload files and $($manifest.workloads.Count) preserved-workload adaptations." -ForegroundColor Green
