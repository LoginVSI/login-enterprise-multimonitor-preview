[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))

# Keep this list short and high-confidence. Add narrowly scoped patterns with a clear public-safety reason.
# Restricted identity terms are matched as whole words by the SHA-256 of their lowercase form, so this
# public script does not carry the names it screens for. To add one, hash the lowercase term and record
# the hash with the term length.
$restrictedTermHashes = @{
    '8d4321d936320802386311d254c4af52951abb880b47fb077edf3d89c150b289' = 6
    '65ce5d19b20696b1e744bd629bd5f59debb28f4f18b69c2a06cba0f200a84af0' = 8
    '5a8d31f7ddd32b1f7e5c3e711571af3ad120b9359bca2d41751db8e69075a7aa' = 6
}
$restrictedTermLengths = @($restrictedTermHashes.Values | Sort-Object -Unique)
$sha256 = [System.Security.Cryptography.SHA256]::Create()

function Test-RestrictedTerm {
    param([string]$Line)
    foreach ($token in [regex]::Matches($Line, '\w+')) {
        if ($restrictedTermLengths -notcontains $token.Length) { continue }
        $hash = ($sha256.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($token.Value.ToLowerInvariant())) | ForEach-Object { $_.ToString('x2') }) -join ''
        if ($restrictedTermHashes.ContainsKey($hash)) { return $true }
    }

    return $false
}

$rules = @(
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
        if (Test-RestrictedTerm $line) {
            $findings.Add([pscustomobject]@{
                Rule = 'Restricted identity term'
                Path = $normalized
                Line = $lineNumber
            })
        }

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
