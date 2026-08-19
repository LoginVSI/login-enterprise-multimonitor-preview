[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$projectPath = Join-Path $repoRoot 'src\LoginVSI.MultiMonitor\LoginVSI.MultiMonitor.csproj'
[xml]$project = Get-Content -LiteralPath $projectPath -Raw

$targetFramework = [string]$project.Project.PropertyGroup.TargetFramework
if ($targetFramework -ne 'netstandard2.0') { throw "Unexpected reusable DLL target framework: $targetFramework" }

$packageReferences = @($project.SelectNodes('//PackageReference'))
$assemblyReferences = @($project.SelectNodes('//Reference'))
if ($packageReferences.Count -ne 0 -or $assemblyReferences.Count -ne 0) {
    throw 'The reusable DLL project must remain dependency-free.'
}

$sourceHits = @(Get-ChildItem (Join-Path $repoRoot 'src\LoginVSI.MultiMonitor') -Filter '*.cs' | Select-String -Pattern 'LoginPI\.Engine')
if ($sourceHits.Count -ne 0) { throw 'Reusable DLL source contains a LoginPI.Engine dependency.' }

Write-Host 'DLL contract passed: netstandard2.0, no package/assembly references, no LoginPI.Engine source dependency.' -ForegroundColor Green
