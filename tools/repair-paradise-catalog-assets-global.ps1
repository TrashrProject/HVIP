[CmdletBinding()]
param(
    [string]$RepositoryRoot = "C:\HVIP",
    [string]$Mysql = "C:\xampp\mysql\bin\mysql.exe",
    [string]$Database = "waveplus"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$targetFd   = Join-Path $RepositoryRoot 'swf_pz\V5-0-2\gamedata\json\FurnitureData.json'
$targetFurn = Join-Path $RepositoryRoot 'swf_pz\V5-0-2\furniture'
$targetIcon = Join-Path $RepositoryRoot 'swf_pz\V5-0-2\dcr\hof_furni\icon'
$reportPath = Join-Path $RepositoryRoot 'tools\repair-paradise-catalog-assets-global.csv'

foreach ($p in @($Mysql,$targetFd,$targetFurn,$targetIcon)) {
    if (-not (Test-Path -LiteralPath $p)) { throw "Ressource requise absente : $p" }
}

function Get-FdEntries([string]$Path) {
    try {
        $json = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
        return @($json.roomitemtypes.furnitype) + @($json.wallitemtypes.furnitype)
    } catch { return @() }
}

$fd = Get-Content -LiteralPath $targetFd -Raw | ConvertFrom-Json
$targetEntries = @($fd.roomitemtypes.furnitype)+@($fd.wallitemtypes.furnitype)
$fdById=@{}; $fdByClass=@{}
foreach($e in $targetEntries){
    if($null -ne $e.id){$fdById[[long]$e.id]=$e}
    if($null -ne $e.classname -and -not $fdByClass.ContainsKey([string]$e.classname)){$fdByClass[[string]$e.classname]=$e}
}

# Indexe toutes les autres FurnitureData du repo pour recuperer des definitions absentes.
$altFdByClass=@{}
$fdFiles = @(Get-ChildItem -LiteralPath $RepositoryRoot -File -Recurse -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '^(FurnitureData|furnidata).*\.json$' })
foreach($f in $fdFiles){
    if($f.FullName -eq $targetFd){continue}
    foreach($e in Get-FdEntries $f.FullName){
        if($null -ne $e.classname){
            $cn=[string]$e.classname
            if(-not $altFdByClass.ContainsKey($cn)){$altFdByClass[$cn]=$e}
        }
    }
}

# Indexe tous les .nitro et *_icon.png disponibles dans le repo, hors destination canonique.
$nitroIndex=@{}
$iconIndex=@{}
Get-ChildItem -LiteralPath $RepositoryRoot -Filter *.nitro -File -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
    $base=$_.BaseName
    if(-not $nitroIndex.ContainsKey($base) -or $_.FullName -like '*\swf_pz\V5-0-2\furniture\*'){$nitroIndex[$base]=$_.FullName}
}
Get-ChildItem -LiteralPath $RepositoryRoot -Filter *_icon.png -File -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
    $base=$_.Name -replace '_icon\.png$',''
    if(-not $iconIndex.ContainsKey($base) -or $_.FullName -like '*\swf_pz\V5-0-2\dcr\hof_furni\icon\*'){$iconIndex[$base]=$_.FullName}
}

$furnSql = "SELECT id,item_name,public_name,type FROM furniture WHERE type IN ('s','i') AND item_name<>'';"
$catalogSql = "SELECT DISTINCT item_ids FROM catalog_items WHERE item_ids REGEXP '^[0-9]+';"
$furnRows = & $Mysql -u root -N -B --raw $Database -e $furnSql
if($LASTEXITCODE -ne 0){throw 'Lecture furniture impossible.'}
$catRows = & $Mysql -u root -N -B --raw $Database -e $catalogSql
if($LASTEXITCODE -ne 0){throw 'Lecture catalog_items impossible.'}

$furnById=@{}
foreach($line in $furnRows){
    $p=$line -split "`t",-1
    if($p.Count -ge 4){$furnById[[long]$p[0]]=[pscustomobject]@{Id=[long]$p[0];ClassName=$p[1];PublicName=$p[2];Type=$p[3]}}
}

$rows=[Collections.Generic.List[object]]::new()
$addedFd=0;$remappedFd=0;$copiedNitro=0;$copiedIcon=0
$seen=[Collections.Generic.HashSet[long]]::new()
foreach($itemIds in $catRows){
    $firstId=(($itemIds -split '[,:;]')[0] -replace '[^0-9]','')
    if([string]::IsNullOrWhiteSpace($firstId)){continue}
    $fid=[long]$firstId
    if(-not $seen.Add($fid)){continue}
    if(-not $furnById.ContainsKey($fid)){continue}
    $f=$furnById[$fid]
    $className=[string]$f.ClassName
    $assetName=($className -split '\*')[0]

    if(-not $fdById.ContainsKey($fid)){
        if($fdByClass.ContainsKey($className)){
            $entry=$fdByClass[$className]
            $entry.id=$fid
            $fdById[$fid]=$entry
            $remappedFd++
        } elseif($altFdByClass.ContainsKey($className)) {
            $src=$altFdByClass[$className] | ConvertTo-Json -Depth 30 | ConvertFrom-Json
            $src.id=$fid
            if($f.Type -eq 'i'){$fd.wallitemtypes.furnitype=@($fd.wallitemtypes.furnitype)+@($src)}else{$fd.roomitemtypes.furnitype=@($fd.roomitemtypes.furnitype)+@($src)}
            $fdById[$fid]=$src;$fdByClass[$className]=$src;$addedFd++
        }
    }

    $nitro=Join-Path $targetFurn ($assetName+'.nitro')
    if(-not (Test-Path -LiteralPath $nitro) -and $nitroIndex.ContainsKey($assetName)){
        Copy-Item -LiteralPath $nitroIndex[$assetName] -Destination $nitro -Force
        $copiedNitro++
    }
    $icon=Join-Path $targetIcon ($assetName+'_icon.png')
    if(-not (Test-Path -LiteralPath $icon) -and $iconIndex.ContainsKey($assetName)){
        Copy-Item -LiteralPath $iconIndex[$assetName] -Destination $icon -Force
        $copiedIcon++
    }

    $hasFd=$fdById.ContainsKey($fid)
    $hasNitro=Test-Path -LiteralPath $nitro
    $hasIcon=Test-Path -LiteralPath $icon
    $rows.Add([pscustomobject]@{FurnitureId=$fid;ClassName=$className;HasFurnitureData=$hasFd;HasNitro=$hasNitro;HasIcon=$hasIcon;Valid=($hasFd -and $hasNitro -and $hasIcon)})
}

[IO.File]::WriteAllText($targetFd,($fd|ConvertTo-Json -Depth 30 -Compress),[Text.UTF8Encoding]::new($false))
$rows | Export-Csv -LiteralPath $reportPath -NoTypeInformation -Encoding UTF8

$valid=@($rows|Where-Object Valid).Count
$missingFd=@($rows|Where-Object {-not $_.HasFurnitureData}).Count
$missingNitro=@($rows|Where-Object {-not $_.HasNitro}).Count
$missingIcon=@($rows|Where-Object {-not $_.HasIcon}).Count

Write-Host '=== REPARATION GLOBALE ASSETS CATALOGUE ===' -ForegroundColor Cyan
Write-Host "Offres uniques controlees : $($rows.Count)"
Write-Host "FurnitureData remappes : $remappedFd"
Write-Host "FurnitureData ajoutes depuis autres sources du repo : $addedFd"
Write-Host ".nitro recopies depuis autres emplacements : $copiedNitro"
Write-Host "Icones recopiees depuis autres emplacements : $copiedIcon"
Write-Host "Valides apres reparation globale : $valid" -ForegroundColor Green
Write-Host "FurnitureData encore manquants : $missingFd"
Write-Host ".nitro encore manquants : $missingNitro"
Write-Host "Icones encore manquantes : $missingIcon"
Write-Host "Rapport : $reportPath" -ForegroundColor Green
