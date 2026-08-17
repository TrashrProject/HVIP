$ErrorActionPreference = 'Stop'

$repoRoot = 'C:\HVIP'
$emuRoot = Join-Path $repoRoot 'RDP EMU header change'
$csproj = Join-Path $emuRoot 'Plus Emulator.csproj'
$libDir = Join-Path $emuRoot 'lib'

if (-not (Test-Path $csproj)) { throw "Projet introuvable: $csproj" }
New-Item -ItemType Directory -Force -Path $libDir | Out-Null

Write-Host '=== ParadiseRP - Correction références émulateur ===' -ForegroundColor Cyan

function Find-Binary([string]$name) {
    $preferred = Get-ChildItem -Path $emuRoot -Filter $name -Recurse -File -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\packages\\' -and $_.FullName -notmatch '\\lib\\' } |
        Sort-Object @{ Expression = {
            if ($_.FullName -match '\\bin\\x64\\ClientNormal\\') { 0 }
            elseif ($_.FullName -match '\\bin\\x64\\PlusEmulator_ShockEdition\\') { 1 }
            elseif ($_.FullName -match '\\bin\\') { 2 }
            else { 3 }
        }}, FullName |
        Select-Object -First 1
    return $preferred
}

# log4net existe déjà dans le package NuGet du projet.
$log4net = Join-Path $emuRoot 'packages\log4net.2.0.3\lib\net40-full\log4net.dll'
if (-not (Test-Path $log4net)) { throw "log4net introuvable: $log4net" }

$required = @('AStar.dll','HabboEncryption.dll','MySql.Data.dll')
foreach ($dll in $required) {
    $src = Find-Binary $dll
    if (-not $src) { throw "$dll introuvable sous $emuRoot" }
    $dst = Join-Path $libDir $dll
    Copy-Item $src.FullName $dst -Force
    Write-Host "Copié: $dll <- $($src.FullName)" -ForegroundColor Green
}

$xml = Get-Content -LiteralPath $csproj -Raw
$before = $xml

$replacements = @{
    '..\Torontov1\bin\Debug\AStar.dll' = 'lib\AStar.dll'
    '..\Torontov1\bin\Debug\HabboEncryption.dll' = 'lib\HabboEncryption.dll'
    '..\Torontov1\bin\Debug\log4net.dll' = 'packages\log4net.2.0.3\lib\net40-full\log4net.dll'
    '..\Torontov1\bin\Debug\MySql.Data.dll' = 'lib\MySql.Data.dll'
}

foreach ($old in $replacements.Keys) {
    $xml = $xml.Replace($old, $replacements[$old])
}

if ($xml -eq $before) {
    Write-Host 'Les anciennes références Torontov1 étaient déjà corrigées.' -ForegroundColor Yellow
} else {
    Set-Content -LiteralPath $csproj -Value $xml -Encoding UTF8
    Write-Host 'Plus Emulator.csproj corrigé.' -ForegroundColor Green
}

Write-Host ''
Write-Host 'Références encore vers Torontov1 :' -ForegroundColor Yellow
$remaining = Select-String -LiteralPath $csproj -Pattern 'Torontov1' -SimpleMatch
if ($remaining) { $remaining | ForEach-Object { Write-Host $_.Line.Trim() -ForegroundColor Red } }
else { Write-Host 'Aucune.' -ForegroundColor Green }

$msbuildCandidates = @(
    'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe',
    'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe',
    'C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe',
    'C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\amd64\MSBuild.exe',
    'C:\Program Files\dotnet\sdk\10.0.400\MSBuild.exe'
)
$msbuild = $msbuildCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $msbuild) { throw 'MSBuild.exe introuvable.' }

Write-Host ''
Write-Host "Compilation avec: $msbuild" -ForegroundColor Cyan
Push-Location $emuRoot
try {
    & $msbuild 'Plus Emulator.sln' /p:Configuration=Debug /p:Platform=x86 /m
    if ($LASTEXITCODE -ne 0) { throw "Compilation échouée (code $LASTEXITCODE)" }
} finally {
    Pop-Location
}

Write-Host ''
Write-Host '=== Compilation réussie ===' -ForegroundColor Green
