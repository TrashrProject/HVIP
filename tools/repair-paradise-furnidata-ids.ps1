[CmdletBinding()]
param(
    [string]$RepositoryRoot = "",
    [string]$Mysql = "C:\xampp\mysql\bin\mysql.exe",
    [string]$Database = "waveplus",
    [string]$HabboRpRoot = "C:\xampp\htdocs\HabboRPbr"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) { $RepositoryRoot = Split-Path -Parent $PSScriptRoot }

$fdPath    = Join-Path $RepositoryRoot 'swf_pz\V5-0-2\gamedata\json\FurnitureData.json'
$furnDir   = Join-Path $RepositoryRoot 'swf_pz\V5-0-2\furniture'
$iconDir   = Join-Path $RepositoryRoot 'swf_pz\V5-0-2\dcr\hof_furni\icon'
$sourceFd  = Join-Path $HabboRpRoot 'pack\cdn-react\gamedata\FurnitureData.json'
$sourceFur = Join-Path $HabboRpRoot 'pack\cdn-react\bundled\furniture'
$sourceIco = Join-Path $HabboRpRoot 'pack\cdn-react\icons'
$report    = Join-Path $PSScriptRoot 'repair-paradise-furnidata-ids.csv'

foreach ($p in @($Mysql,$fdPath,$furnDir,$iconDir)) {
    if (-not (Test-Path -LiteralPath $p)) { throw "Ressource requise absente : $p" }
}

$fd = Get-Content -LiteralPath $fdPath -Raw | ConvertFrom-Json
$allFd = @($fd.roomitemtypes.furnitype) + @($fd.wallitemtypes.furnitype)
$fdById = @{}
$fdByClass = @{}
foreach ($e in $allFd) {
    if ($null -ne $e.id) { $fdById[[long]$e.id] = $e }
    if ($null -ne $e.classname -and -not $fdByClass.ContainsKey([string]$e.classname)) { $fdByClass[[string]$e.classname] = $e }
}

$sourceByClass = @{}
if (Test-Path -LiteralPath $sourceFd) {
    $sfd = Get-Content -LiteralPath $sourceFd -Raw | ConvertFrom-Json
    foreach ($e in @($sfd.roomitemtypes.furnitype)+@($sfd.wallitemtypes.furnitype)) {
        if ($null -ne $e.classname -and -not $sourceByClass.ContainsKey([string]$e.classname)) { $sourceByClass[[string]$e.classname] = $e }
    }
}

$furnRows = & $Mysql -u root -N -B --raw $Database -e "SELECT id,item_name,type FROM furniture WHERE type IN ('s','i') AND item_name<>'';"
if ($LASTEXITCODE -ne 0) { throw 'Lecture furniture impossible.' }
$catRows = & $Mysql -u root -N -B --raw $Database -e "SELECT id,item_ids FROM catalog_items WHERE item_ids REGEXP '^[0-9]+';"
if ($LASTEXITCODE -ne 0) { throw 'Lecture catalog_items impossible.' }

$furnById = @{}
foreach ($line in $furnRows) {
    $p=$line -split "`t",-1
    if ($p.Count -ge 3) { $furnById[[long]$p[0]]=[pscustomobject]@{Id=[long]$p[0];ClassName=$p[1];Type=$p[2]} }
}

$usedIds=[Collections.Generic.HashSet[long]]::new()
foreach($line in $catRows){
    $p=$line -split "`t",-1
    if($p.Count -lt 2){continue}
    $first=(($p[1]-split '[,:;]')[0]-replace '[^0-9]','')
    if(-not [string]::IsNullOrWhiteSpace($first)){[void]$usedIds.Add([long]$first)}
}

$remapped=0; $addedFd=0; $copiedNitro=0; $copiedIcon=0
$rows=[Collections.Generic.List[object]]::new()

foreach($fid in $usedIds){
    if(-not $furnById.ContainsKey($fid)){continue}
    $f=$furnById[$fid]
    $class=[string]$f.ClassName
    $asset=($class -split '\*')[0]
    $fdEntry=$null

    if($fdById.ContainsKey($fid)){
        $fdEntry=$fdById[$fid]
    } elseif($fdByClass.ContainsKey($class)) {
        $fdEntry=$fdByClass[$class]
        $fdEntry.id=$fid
        $fdById[$fid]=$fdEntry
        $remapped++
    } elseif($sourceByClass.ContainsKey($class)) {
        $src=$sourceByClass[$class] | ConvertTo-Json -Depth 30 | ConvertFrom-Json
        $src.id=$fid
        if($f.Type -eq 'i'){$fd.wallitemtypes.furnitype=@($fd.wallitemtypes.furnitype)+@($src)}else{$fd.roomitemtypes.furnitype=@($fd.roomitemtypes.furnitype)+@($src)}
        $fdById[$fid]=$src; $fdByClass[$class]=$src; $fdEntry=$src; $addedFd++
    }

    $nitro=Join-Path $furnDir ($asset+'.nitro')
    $icon=Join-Path $iconDir ($asset+'_icon.png')
    if(-not(Test-Path -LiteralPath $nitro) -and (Test-Path -LiteralPath (Join-Path $sourceFur ($asset+'.nitro')))){
        Copy-Item -LiteralPath (Join-Path $sourceFur ($asset+'.nitro')) -Destination $nitro -Force; $copiedNitro++
    }
    if(-not(Test-Path -LiteralPath $icon) -and (Test-Path -LiteralPath (Join-Path $sourceIco ($asset+'_icon.png')))){
        Copy-Item -LiteralPath (Join-Path $sourceIco ($asset+'_icon.png')) -Destination $icon -Force; $copiedIcon++
    }

    $rows.Add([pscustomobject]@{
        FurnitureId=$fid; ClassName=$class;
        FurnitureData=($null -ne $fdEntry);
        Nitro=(Test-Path -LiteralPath $nitro);
        Icon=(Test-Path -LiteralPath $icon)
    })
}

[IO.File]::WriteAllText($fdPath,($fd|ConvertTo-Json -Depth 30 -Compress),[Text.UTF8Encoding]::new($false))
$rows | Export-Csv -LiteralPath $report -NoTypeInformation -Encoding UTF8

$missingFd=@($rows|Where-Object{-not $_.FurnitureData}).Count
$missingNitro=@($rows|Where-Object{-not $_.Nitro}).Count
$missingIcon=@($rows|Where-Object{-not $_.Icon}).Count
$valid=@($rows|Where-Object{$_.FurnitureData -and $_.Nitro -and $_.Icon}).Count

Write-Host '=== REPARATION CATALOGUE PARADISERP ===' -ForegroundColor Cyan
Write-Host "Offres uniques controlees : $($rows.Count)"
Write-Host "FurnitureData IDs remappes par classname : $remapped" -ForegroundColor Green
Write-Host "FurnitureData ajoutes depuis HabboRPbr : $addedFd" -ForegroundColor Green
Write-Host ".nitro recopies : $copiedNitro" -ForegroundColor Green
Write-Host "Icones recopiees : $copiedIcon" -ForegroundColor Green
Write-Host "Valides apres reparation : $valid" -ForegroundColor Cyan
Write-Host "FurnitureData encore manquants : $missingFd"
Write-Host ".nitro encore manquants : $missingNitro"
Write-Host "Icones encore manquantes : $missingIcon"
Write-Host "Rapport : $report"
