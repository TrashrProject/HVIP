param([string]$Root='C:\HVIP')
$ErrorActionPreference='Stop'; Set-StrictMode -Version Latest
function Require-Command([string]$Name){ if(-not(Get-Command $Name -ErrorAction SilentlyContinue)){ throw "Commande introuvable: $Name" } }
Require-Command git; Require-Command node
$assetRoot=Join-Path $Root 'swf_pz\V5-0-2'; $figureDir=Join-Path $assetRoot 'figure'; $gameDataDir=Join-Path $assetRoot 'gamedata\json'
$currentFD=Join-Path $gameDataDir 'FigureData.json'; $currentFM=Join-Path $gameDataDir 'FigureMap.json'
$mergeScript=Join-Path $Root 'tools\merge-wardrobe.js'; $repairScript=Join-Path $Root 'tools\repair-wardrobe-json.js'; $scanScript=Join-Path $Root 'tools\scan-missing-clothes.js'
$toolsDir=Join-Path $Root '_tools'; $sourceDir=Join-Path $toolsDir 'objectretros-clothes'; $timestamp=Get-Date -Format 'yyyyMMdd-HHmmss'; $backupDir=Join-Path $Root "_backups\clothes-objectretros\$timestamp"
foreach($path in @($figureDir,$currentFD,$currentFM,$mergeScript,$repairScript)){ if(-not(Test-Path $path)){ throw "Introuvable: $path" } }
New-Item -ItemType Directory -Force -Path $toolsDir,$backupDir|Out-Null; Copy-Item $currentFD (Join-Path $backupDir 'FigureData.json') -Force; Copy-Item $currentFM (Join-Path $backupDir 'FigureMap.json') -Force
Write-Host "[1/7] Backup ParadiseRP -> $backupDir" -ForegroundColor Cyan
if(-not(Test-Path(Join-Path $sourceDir '.git'))){
 Write-Host '[2/7] Telechargement du pack public ObjectRetros/xlRaiko...' -ForegroundColor Cyan
 git clone --depth 1 --filter=blob:none --sparse https://github.com/ObjectRetros/retro-hotel-files.git $sourceDir; if($LASTEXITCODE-ne 0){throw "git clone a echoue: $LASTEXITCODE"}
 git -C $sourceDir sparse-checkout set 'nitro/nitro-assets/bundled/figure' 'nitro/nitro-assets/gamedata'; if($LASTEXITCODE-ne 0){throw "sparse-checkout a echoue: $LASTEXITCODE"}
}else{
 Write-Host '[2/7] Mise a jour du pack ObjectRetros/xlRaiko...' -ForegroundColor Cyan
 git -C $sourceDir fetch --depth 1 origin main; if($LASTEXITCODE-ne 0){throw "git fetch a echoue: $LASTEXITCODE"}; git -C $sourceDir reset --hard origin/main; if($LASTEXITCODE-ne 0){throw "git reset a echoue: $LASTEXITCODE"}
}
$sourceFigure=Join-Path $sourceDir 'nitro\nitro-assets\bundled\figure'; $sourceFD=Join-Path $sourceDir 'nitro\nitro-assets\gamedata\FigureData.json'; $sourceFM=Join-Path $sourceDir 'nitro\nitro-assets\gamedata\FigureMap.json'
foreach($path in @($sourceFigure,$sourceFD,$sourceFM)){if(-not(Test-Path $path)){throw "Pack incomplet: $path"}}
Write-Host '[3/7] Copie des .nitro customs manquants...' -ForegroundColor Cyan
$added=0;$skipped=0; Get-ChildItem $sourceFigure -Filter '*.nitro' -File|ForEach-Object{ $target=Join-Path $figureDir $_.Name; if(Test-Path $target){$skipped++}else{Copy-Item $_.FullName $target -Force;$added++} }
Write-Host "[4/7] Assets ajoutes: $added | deja presents: $skipped" -ForegroundColor Cyan
$tmpFD=Join-Path $backupDir 'FigureData.merged.json';$tmpFM=Join-Path $backupDir 'FigureMap.merged.json'
node $mergeScript $sourceFD $sourceFM $currentFD $currentFM $tmpFD $tmpFM; if($LASTEXITCODE-ne 0){throw "Fusion echouee: $LASTEXITCODE"}; Copy-Item $tmpFD $currentFD -Force; Copy-Item $tmpFM $currentFM -Force
Write-Host '[5/7] FigureData/FigureMap fusionnes, ParadiseRP prioritaire.' -ForegroundColor Cyan
node $repairScript $currentFD $currentFM; if($LASTEXITCODE-ne 0){throw "Reparation echouee: $LASTEXITCODE"}
Write-Host '[6/7] Scan des bibliotheques manquantes...' -ForegroundColor Cyan; if(Test-Path $scanScript){node $scanScript $currentFM $figureDir}
Write-Host '[7/7] Import termine.' -ForegroundColor Green; Write-Host "Nouveaux .nitro ajoutes : $added" -ForegroundColor Green; Write-Host "Existants preserves : $skipped" -ForegroundColor Green; Write-Host "Backup : $backupDir" -ForegroundColor Yellow; Write-Host 'Ferme completement le client, rouvre /play puis Ctrl+F5.' -ForegroundColor Yellow
