[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$markdownFiles = @(Get-ChildItem -LiteralPath $repoRoot -Filter '*.md' -File -Recurse | Where-Object {
    $_.FullName -notmatch '[\\/](\.git|artifacts|bin|obj)[\\/]'
})

$checkedLinks = 0
foreach ($file in $markdownFiles) {
    $text = [System.IO.File]::ReadAllText($file.FullName)
    foreach ($match in [regex]::Matches($text, '(?!!)(?:\[[^\]]+\])\(([^)]+)\)')) {
        $target = $match.Groups[1].Value.Trim().Trim('<', '>')
        if ([string]::IsNullOrWhiteSpace($target) -or $target.StartsWith('#') -or $target -match '^[a-zA-Z][a-zA-Z0-9+.-]*:') { continue }
        $target = $target.Split('#')[0]
        $target = [System.Uri]::UnescapeDataString($target)
        $fullTarget = [System.IO.Path]::GetFullPath((Join-Path $file.DirectoryName $target))
        if (-not (Test-Path -LiteralPath $fullTarget)) {
            $relativeFile = $file.FullName.Substring($repoRoot.Length).TrimStart('\', '/')
            throw "Broken local Markdown link in $relativeFile`: $target"
        }
        $checkedLinks++
    }
}

$criticalPaths = @(
    'dist\LoginVSI.MultiMonitor.dll',
    'dist\SHA256SUMS.txt',
    'workloads\dll-backed\00-Prepare-MultiMonitor.cs',
    'workloads\office-preview\01-Reset-Placement-State.cs',
    'docs\evidence-status.md',
    'docs\adapt-your-own-workload.md',
    'docs\agentic-workload-adaptation.md',
    'docs\product-handoff.md',
    'skills\login-enterprise-multimonitor\implementation-guidance.md',
    'skills\login-enterprise-multimonitor\validation-guidance.md',
    'skills\login-enterprise-multimonitor\product-context.md'
)
foreach ($relativePath in $criticalPaths) {
    if (-not (Test-Path -LiteralPath (Join-Path $repoRoot $relativePath))) {
        throw "Critical documented path is missing: $relativePath"
    }
}

$readme = [System.IO.File]::ReadAllText((Join-Path $repoRoot 'README.md'))
foreach ($required in @(
    'docs/test-lab-quickstart.md',
    'docs/adapt-your-own-workload.md',
    'docs/agentic-workload-adaptation.md',
    'docs/evidence-status.md',
    'docs/product-handoff.md'
)) {
    if (-not $readme.Contains($required)) { throw "README lacks required customer path: $required" }
}

$quickstart = [System.IO.File]::ReadAllText((Join-Path $repoRoot 'docs\test-lab-quickstart.md'))
foreach ($required in @('00-Prepare-MultiMonitor.cs', '01-Reset-Placement-State.cs', 'MonitorCount', 'StateAdvanced')) {
    if (-not $quickstart.Contains($required)) { throw "Quickstart lacks required setup/result guidance: $required" }
}

$agentGuide = [System.IO.File]::ReadAllText((Join-Path $repoRoot 'docs\agentic-workload-adaptation.md'))
foreach ($required in @(
    'Copy/paste prompt for a coding agent',
    'docs/adapt-your-own-workload.md',
    'skills/login-enterprise-multimonitor/SKILL.md',
    'workloads/source/MyWordWorkload.cs',
    'workloads/custom/MyWordWorkload-MultiMonitor.cs',
    'PlaceNext exactly once',
    'PlaceLastUsed or PlaceOnMonitor',
    '.\scripts\Test-Repository.ps1',
    'Do not claim runtime proof'
)) {
    if (-not $agentGuide.Contains($required)) { throw "Agentic adaptation prompt lacks required contract: $required" }
}

foreach ($relativePath in @('README.md', 'docs\getting-started.md', 'docs\test-lab-quickstart.md', 'docs\architecture.md', 'docs\product-handoff.md', 'skills\login-enterprise-multimonitor\SKILL.md', 'workloads\README.md', 'workloads\knowledge-worker-multimonitor\README.md')) {
    $content = [System.IO.File]::ReadAllText((Join-Path $repoRoot $relativePath))
    if ($content -match 'Knowledge Worker.{0,80}partner-lab (runtime validation )?pending') {
        throw "Obsolete blanket Knowledge Worker partner-lab pending claim remains in $relativePath"
    }
}

$skillPath = Join-Path $repoRoot 'skills\login-enterprise-multimonitor\SKILL.md'
$skill = [System.IO.File]::ReadAllText($skillPath)
foreach ($required in @('Classify every supplied file', 'PlaceNext', 'PlaceLastUsed', 'mapping/delta record', 'scripts/Test-Repository.ps1', 'not runtime-proven')) {
    if (-not $skill.Contains($required)) { throw "Repository adaptation skill lacks required guidance: $required" }
}

Write-Host "Documentation contracts passed for $($markdownFiles.Count) Markdown files, $checkedLinks local links, customer paths, evidence status, and adaptation guidance." -ForegroundColor Green
