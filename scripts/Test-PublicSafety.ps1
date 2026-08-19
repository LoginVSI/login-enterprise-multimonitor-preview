[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))

# Keep this list short and high-confidence. Add narrowly scoped patterns with a clear public-safety reason.
$identityTerms = @(
    ('NVI' + 'DIA'),
    ('Perf' + 'Labs'),
    ('Poo' + 'rna')
)
$rules = @(
    @{ Name = 'Restricted identity term'; Pattern = '(?i)\b(' + (($identityTerms | ForEach-Object { [regex]::Escape($_) }) -join '|') + ')\b' },
    @{ Name = 'Machine-specific user profile path'; Pattern = '(?i)\bC:\\Users\\[^\\\s"'']+' },
    @{ Name = 'Credential-like assignment'; Pattern = '(?i)\b(api[_-]?key|access[_-]?token|client[_-]?secret|password)\s*[:=]\s*["''][^"'']{8,}["'']' },
    @{ Name = 'GitHub token-like value'; Pattern = '\bgh[pousr]_[A-Za-z0-9]{20,}\b' },
    @{ Name = 'AWS access-key-like value'; Pattern = '\bAKIA[0-9A-Z]{16}\b' },
    @{ Name = 'Bearer token-like value'; Pattern = '(?i)\bBearer\s+[A-Za-z0-9._~-]{20,}\b' },
    @{ Name = 'Basic authorization-like value'; Pattern = '(?i)\bBasic\s+[A-Za-z0-9+/]{16,}={0,2}\b' },
    @{ Name = 'Private-key header'; Pattern = '-----BEGIN (RSA |EC |OPENSSH |DSA )?PRIVATE KEY-----' }
)

$sourceExtensions = @(
    '.md', '.txt', '.cs', '.csx', '.ps1', '.psm1', '.psd1', '.json', '.yml', '.yaml',
    '.xml', '.config', '.props', '.targets', '.sln', '.csproj', '.gitignore'
)

$gitOutput = @(& git -C $repoRoot ls-files --cached --others --exclude-standard 2>$null)
if ($LASTEXITCODE -ne 0) {
    Write-Error "Unable to enumerate repository files with Git: $($gitOutput -join [Environment]::NewLine)"
    exit 2
}

$findings = New-Object System.Collections.Generic.List[object]
foreach ($relativePath in @($gitOutput | Sort-Object -Unique)) {
    $relativePathText = [string]$relativePath
    if ([string]::IsNullOrWhiteSpace($relativePathText)) { continue }
    $normalized = $relativePathText.Replace('\', '/')
    if ($normalized -match '(^|/)(\.git|artifacts|bin|obj)(/|$)') { continue }

    # Supplied reference material remains verbatim evidence. The separate hash
    # checks protect the immutable subsets; the email rule governs new public
    # implementation/docs outside reference/ without echoing candidate values.
    $isPreservedEvidence = $normalized.StartsWith('reference/', [System.StringComparison]::OrdinalIgnoreCase)

    $extension = [System.IO.Path]::GetExtension($normalized).ToLowerInvariant()
    if ($normalized -eq '.gitignore') { $extension = '.gitignore' }
    if ($sourceExtensions -notcontains $extension) { continue }

    $fullPath = Join-Path $repoRoot $relativePathText
    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) { continue }

    $lineNumber = 0
    foreach ($line in (Get-Content -LiteralPath $fullPath)) {
        $lineNumber++
        foreach ($rule in $rules) {
            if ($line -match $rule.Pattern) {
                $findings.Add([pscustomobject]@{
                    Rule = $rule.Name
                    Path = $normalized
                    Line = $lineNumber
                })
            }
        }

        if (-not $isPreservedEvidence) {
            foreach ($emailMatch in [regex]::Matches($line, '(?i)\b[A-Z0-9._%+-]+@([A-Z0-9.-]+\.[A-Z]{2,})\b')) {
                if ($emailMatch.Groups[1].Value -ne 'example.invalid') {
                    $findings.Add([pscustomobject]@{
                        Rule = 'Email address outside preserved evidence'
                        Path = $normalized
                        Line = $lineNumber
                    })
                }
            }
        }
    }
}

if ($findings.Count -gt 0) {
    Write-Host 'High-confidence public-safety finding(s):' -ForegroundColor Red
    $findings | Sort-Object Path, Line, Rule | Format-Table -AutoSize
    Write-Host 'Review and remove sensitive content before publication.' -ForegroundColor Red
    exit 1
}

Write-Host 'Public-safety scan passed with no high-confidence findings.' -ForegroundColor Green
Write-Host 'This supplements human review and repository security tooling; it is not exhaustive.'
exit 0
