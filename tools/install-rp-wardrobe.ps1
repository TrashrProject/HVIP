param([string]$Root='C:\HVIP')
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

function Need([string]$p){ if(-not(Test-Path $p)){ throw "Introuvable: $p" } }
$builder=Join-Path $Root 'tools\build-rp-outfits.js'
$figureData=Join-Path $Root 'swf_pz\V5-0-2\gamedata\json\FigureData.json'
$figureMap=Join-Path $Root 'swf_pz\V5-0-2\gamedata\json\FigureMap.json'
$safeJs=Join-Path $Root 'WebPixel\nitro-last\rp-wardrobe-safe.js'
$safeCss=Join-Path $Root 'WebPixel\nitro-last\rp-wardrobe-safe.css'
$endpoint=Join-Path $Root 'WebPixel\rp-outfit-apply.php'
foreach($p in @($builder,$figureData,$figureMap,$safeJs,$safeCss,$endpoint)){ Need $p }
if(-not(Get-Command node -ErrorAction SilentlyContinue)){throw 'Node.js introuvable.'}

Write-Host '=== ParadiseRP - Tenues RP SAFE ===' -ForegroundColor Cyan
Write-Host '[1/2] Analyse des vêtements déjà présents...' -ForegroundColor Cyan
node $builder $Root
if($LASTEXITCODE-ne 0){throw "Generation des tenues RP échouée: $LASTEXITCODE"}

Write-Host '[2/2] Vérification du catalogue RP...' -ForegroundColor Cyan
$out=Join-Path $Root 'WebPixel\nitro-last\rp-outfits.json'
Need $out
$data=Get-Content $out -Raw | ConvertFrom-Json
Write-Host "Presets RP disponibles : $($data.total)" -ForegroundColor Green
$data.categories | ForEach-Object { Write-Host (" - {0}: {1}" -f $_.label,$_.count) -ForegroundColor Green }
Write-Host ''
Write-Host 'Aucun patch du bundle Nitro n est effectué.' -ForegroundColor Green
Write-Host 'L onglet Tenues RP est chargé par un script externe isolé.' -ForegroundColor Green
Write-Host 'Ferme complètement /play, rouvre-le puis Ctrl+F5.' -ForegroundColor Yellow
