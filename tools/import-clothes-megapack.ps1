param([string]$Root='C:\HVIP')
$ErrorActionPreference='Stop'; Set-StrictMode -Version Latest
function Run-Step([string]$Label,[scriptblock]$Block){ Write-Host "`n=== $Label ===" -ForegroundColor Cyan; & $Block }
function Require([string]$Path){ if(-not(Test-Path $Path)){ throw "Introuvable: $Path" } }
$tools=Join-Path $Root 'tools'; $imports=Join-Path $Root '_imports\clothes'; $assetRoot=Join-Path $Root 'swf_pz\V5-0-2'; $figureDir=Join-Path $assetRoot 'figure'; $game=Join-Path $assetRoot 'gamedata\json'
$fd=Join-Path $game 'FigureData.json'; $fm=Join-Path $game 'FigureMap.json'; $merge=Join-Path $tools 'merge-wardrobe.js'; $repair=Join-Path $tools 'repair-wardrobe-json.js'; $scan=Join-Path $tools 'scan-missing-clothes.js'
foreach($p in @($figureDir,$fd,$fm,$merge,$repair,$scan)){Require $p}; New-Item -ItemType Directory -Force -Path $imports|Out-Null
Run-Step '1/5 - Sources publiques déjà intégrées' {
  $a=Join-Path $tools 'import-objectretros-clothes.ps1'; if(Test-Path $a){ powershell -ExecutionPolicy Bypass -File $a -Root $Root }
  $b=Join-Path $tools 'import-sphynxkitten-clothes.ps1'; if(Test-Path $b){ powershell -ExecutionPolicy Bypass -File $b -Root $Root }
}
Run-Step '2/5 - Import automatique des packs locaux' {
  $packs=Get-ChildItem $imports -Directory -ErrorAction SilentlyContinue
  foreach($pack in $packs){
    Write-Host "Pack: $($pack.Name)" -ForegroundColor Yellow
    $pfd=Get-ChildItem $pack.FullName -Recurse -File -Filter 'FigureData.json' -ErrorAction SilentlyContinue | Select-Object -First 1
    $pfm=Get-ChildItem $pack.FullName -Recurse -File -Filter 'FigureMap.json' -ErrorAction SilentlyContinue | Select-Object -First 1
    $nitros=Get-ChildItem $pack.FullName -Recurse -File -Filter '*.nitro' -ErrorAction SilentlyContinue
    if(-not $pfd -or -not $pfm){ Write-Host '  -> ignoré: FigureData/FigureMap manquants' -ForegroundColor DarkYellow; continue }
    $stamp=Get-Date -Format 'yyyyMMdd-HHmmss'; $backup=Join-Path $Root "_backups\clothes-megapack\$stamp-$($pack.Name)"; New-Item -ItemType Directory -Force -Path $backup|Out-Null; Copy-Item $fd (Join-Path $backup 'FigureData.json'); Copy-Item $fm (Join-Path $backup 'FigureMap.json')
    $added=0;$skipped=0; foreach($n in $nitros){$t=Join-Path $figureDir $n.Name;if(Test-Path $t){$skipped++}else{Copy-Item $n.FullName $t -Force;$added++}}
    $tmpfd=Join-Path $backup 'FigureData.merged.json'; $tmpfm=Join-Path $backup 'FigureMap.merged.json'
    node $merge $pfd.FullName $pfm.FullName $fd $fm $tmpfd $tmpfm; if($LASTEXITCODE-ne 0){throw "Fusion impossible pour $($pack.Name)"}; Copy-Item $tmpfd $fd -Force; Copy-Item $tmpfm $fm -Force
    node $repair $fd $fm; if($LASTEXITCODE-ne 0){throw "Réparation impossible pour $($pack.Name)"}
    Write-Host "  -> +$added .nitro | $skipped doublons" -ForegroundColor Green
  }
}
Run-Step '3/5 - Réparation finale FigureData/FigureMap' { node $repair $fd $fm; if($LASTEXITCODE-ne 0){throw 'Réparation finale échouée'} }
Run-Step '4/5 - Rapport catégories' {
  $names=(Get-ChildItem $figureDir -Filter '*.nitro' -File).BaseName
  $cats=[ordered]@{
    'football/sport'='football|soccer|jersey|shirt.*(psg|barca|madrid|city|united|milan|inter|juve)|kit|sport|fc_|fifa'
    'streetwear'='nike|adidas|supreme|hoodie|street|tech|tracksuit|jordan|trapstar|northface|tnf'
    'luxe'='gucci|louis|vuitton|dior|prada|versace|balenciaga|moncler|fendi|burberry|cartier'
    'anime'='anime|naruto|anbu|akatsuki|dragonball|goku|onepiece|luffy|bleach|demon|jujutsu|pokemon'
    'uniformes RP'='police|cop|sheriff|medic|doctor|nurse|fire|army|military|security|swat|guard|uniform'
    'accessoires'='acc_|necklace|chain|bag|backpack|glasses|mask|scarf|wing|cape|earring|watch'
    'coiffures'='hair_|hair|hairstyle|braid|dread|afro|ponytail|bob'
  }
  foreach($k in $cats.Keys){$count=($names|Where-Object{$_ -match $cats[$k]}).Count; Write-Host ("{0,-16}: {1}" -f $k,$count)}
}
Run-Step '5/5 - Scan des fichiers manquants' { node $scan $fm $figureDir; if($LASTEXITCODE-ne 0){Write-Host 'Des .nitro manquent encore; voir MissingFigureAssets.txt' -ForegroundColor Yellow} }
Write-Host "`nMEGAPACK terminé. Dossier prévu pour futurs packs: $imports" -ForegroundColor Green
Write-Host 'Ferme le client, rouvre /play, puis Ctrl+F5.' -ForegroundColor Yellow
