param(
    [string]$Root = 'C:\HVIP'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Require-Command([string]$Name) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Commande introuvable: $Name"
    }
}

Require-Command git
Require-Command node
Require-Command npm

$assetRoot = Join-Path $Root 'swf_pz\V5-0-2'
$figureDir = Join-Path $assetRoot 'figure'
$gameDataDir = Join-Path $assetRoot 'gamedata\json'
$currentFD = Join-Path $gameDataDir 'FigureData.json'
$currentFM = Join-Path $gameDataDir 'FigureMap.json'
$toolsDir = Join-Path $Root '_tools'
$converterDir = Join-Path $toolsDir 'nitro-converter'
$mergeScript = Join-Path $Root 'tools\merge-wardrobe.js'
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$backupDir = Join-Path $Root "_backups\clothes\$timestamp"

foreach ($path in @($assetRoot, $figureDir, $gameDataDir, $mergeScript, $currentFD, $currentFM)) {
    if (-not (Test-Path $path)) { throw "Introuvable: $path" }
}

New-Item -ItemType Directory -Force -Path $toolsDir, $backupDir | Out-Null
Copy-Item $currentFD (Join-Path $backupDir 'FigureData.json') -Force
Copy-Item $currentFM (Join-Path $backupDir 'FigureMap.json') -Force

Write-Host "[1/6] Backup FigureData/FigureMap -> $backupDir" -ForegroundColor Cyan

if (-not (Test-Path (Join-Path $converterDir '.git'))) {
    Write-Host '[2/6] Installation de nitro-converter...' -ForegroundColor Cyan
    git clone https://github.com/billsonnn/nitro-converter.git $converterDir
} else {
    Write-Host '[2/6] Mise a jour de nitro-converter...' -ForegroundColor Cyan
    git -C $converterDir pull --ff-only
}

$config = @'
{
  "flash.client.url": "",
  "furnidata.load.url": "",
  "productdata.load.url": "",
  "figuredata.load.url": "https://www.habbo.com/gamedata/figuredata/1",
  "figuremap.load.url": "${flash.client.url}figuremap.xml",
  "effectmap.load.url": "${flash.client.url}effectmap.xml",
  "dynamic.download.pet.url": "${flash.client.url}%className%.swf",
  "dynamic.download.figure.url": "${flash.client.url}%className%.swf",
  "dynamic.download.effect.url": "${flash.client.url}%className%.swf",
  "flash.dynamic.download.url": "",
  "dynamic.download.furniture.url": "${flash.dynamic.download.url}%revision%/%className%.swf",
  "external.variables.url": "https://www.habbo.com/gamedata/external_variables/1",
  "external.texts.url": "${external.texts.txt}",
  "convert.figure": "1",
  "convert.effect": "0",
  "convert.furniture": "0",
  "convert.furniture.floor.only": "0",
  "convert.furniture.wall.only": "0",
  "convert.pet": "0"
}
'@
Set-Content -Path (Join-Path $converterDir 'configuration.json') -Value $config -Encoding UTF8

Write-Host '[3/6] Installation/build du convertisseur...' -ForegroundColor Cyan
Push-Location $converterDir
try {
    npm install --no-audit --no-fund
    npm run build

    $convertedFigure = Join-Path $converterDir 'assets\bundled\figure'
    if (Test-Path $convertedFigure) {
        Remove-Item $convertedFigure -Recurse -Force
    }

    Write-Host '[4/6] Telechargement + conversion de TOUS les vêtements officiels Habbo actuels...' -ForegroundColor Cyan
    npm start
}
finally {
    Pop-Location
}

$latestFD = Join-Path $converterDir 'assets\gamedata\FigureData.json'
$latestFM = Join-Path $converterDir 'assets\gamedata\FigureMap.json'
$convertedFigure = Join-Path $converterDir 'assets\bundled\figure'

foreach ($path in @($latestFD, $latestFM, $convertedFigure)) {
    if (-not (Test-Path $path)) { throw "Sortie du convertisseur introuvable: $path" }
}

$newNitros = Get-ChildItem $convertedFigure -Filter '*.nitro' -File
if ($newNitros.Count -eq 0) { throw 'Aucun vêtement .nitro converti.' }

Write-Host "[5/6] Copie de $($newNitros.Count) bibliothèques vêtements vers ParadiseRP..." -ForegroundColor Cyan
foreach ($file in $newNitros) {
    Copy-Item $file.FullName (Join-Path $figureDir $file.Name) -Force
}

$tmpFD = Join-Path $backupDir 'FigureData.merged.json'
$tmpFM = Join-Path $backupDir 'FigureMap.merged.json'
node $mergeScript $currentFD $currentFM $latestFD $latestFM $tmpFD $tmpFM
Copy-Item $tmpFD $currentFD -Force
Copy-Item $tmpFM $currentFM -Force

Write-Host '[6/6] Terminé.' -ForegroundColor Green
Write-Host "Vêtements .nitro copiés : $($newNitros.Count)" -ForegroundColor Green
Write-Host 'FigureData/FigureMap officiels fusionnés avec tes anciens vêtements.' -ForegroundColor Green
Write-Host 'Tous les sets fusionnés sont déverrouillés/selectables dans Mi ropa.' -ForegroundColor Green
Write-Host "Backup : $backupDir" -ForegroundColor Yellow
Write-Host ''
Write-Host 'Ensuite: redémarre uniquement le navigateur/client si nécessaire et fais Ctrl+F5.' -ForegroundColor Yellow
