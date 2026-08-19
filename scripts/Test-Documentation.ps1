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

$skillPath = Join-Path $repoRoot 'skills\login-enterprise-multimonitor\SKILL.md'
$skill = [System.IO.File]::ReadAllText($skillPath)
foreach ($required in @('Classify every supplied file', 'PlaceNext', 'PlaceLastUsed', 'mapping/delta record', 'scripts/Test-Repository.ps1', 'not runtime-proven')) {
    if (-not $skill.Contains($required)) { throw "Repository adaptation skill lacks required guidance: $required" }
}

Write-Host "Documentation contracts passed for $($markdownFiles.Count) Markdown files and $checkedLinks local links; adaptation skill guidance is aligned." -ForegroundColor Green
