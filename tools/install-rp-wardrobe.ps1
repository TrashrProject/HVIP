param([string]$Root='C:\HVIP')
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

function Need([string]$p){ if(-not(Test-Path $p)){ throw "Introuvable: $p" } }
$builder=Join-Path $Root 'tools\build-rp-outfits.js'
$patcher=Join-Path $Root 'tools\patch-rp-wardrobe.js'
$figureData=Join-Path $Root 'swf_pz\V5-0-2\gamedata\json\FigureData.json'
$figureMap=Join-Path $Root 'swf_pz\V5-0-2\gamedata\json\FigureMap.json'
$bundle=Join-Path $Root 'WebPixel\nitro-last\assets\index-9f9954ad.js'
foreach($p in @($builder,$patcher,$figureData,$figureMap,$bundle)){ Need $p }
if(-not(Get-Command node -ErrorAction SilentlyContinue)){throw 'Node.js introuvable.'}

Write-Host '=== ParadiseRP - Installation Tenues RP ===' -ForegroundColor Cyan
Write-Host '[1/3] Analyse des vêtements déjà présents...' -ForegroundColor Cyan
node $builder $Root
if($LASTEXITCODE-ne 0){throw "Generation des tenues RP échouée: $LASTEXITCODE"}

Write-Host '[2/3] Ajout de l onglet Tenues RP dans Mi Ropa...' -ForegroundColor Cyan
node $patcher $Root
if($LASTEXITCODE-ne 0){throw "Patch Mi Ropa échoué: $LASTEXITCODE"}

Write-Host '[3/3] Vérification...' -ForegroundColor Cyan
$out=Join-Path $Root 'WebPixel\nitro-last\rp-outfits.json'
Need $out
$data=Get-Content $out -Raw | ConvertFrom-Json
Write-Host "Presets RP disponibles : $($data.total)" -ForegroundColor Green
$data.categories | ForEach-Object { Write-Host (" - {0}: {1}" -f $_.label,$_.count) -ForegroundColor Green }
Write-Host ''
Write-Host 'Installation terminée. Ferme complètement /play, rouvre-le puis Ctrl+F5.' -ForegroundColor Yellow
Write-Host 'Dans Mi Ropa, un nouvel onglet Tenues RP est maintenant disponible.' -ForegroundColor Yellow
