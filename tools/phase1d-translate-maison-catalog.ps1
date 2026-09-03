[CmdletBinding()]
param(
    [string]$Mysql = 'C:\xampp\mysql\bin\mysql.exe',
    [string]$Database = 'waveplus',
    [switch]$Apply
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$pageId = 9967202
$repoRoot = Split-Path -Parent $PSScriptRoot
$report = Join-Path $PSScriptRoot 'catalog-phase1d-maison-fr.csv'
$sqlPath = Join-Path $repoRoot 'migrations\20260903_catalog_phase1d_maison_fr.sql'

# Build accented characters at runtime so Windows PowerShell 5.1 cannot misread
# this script when Git checked it out as UTF-8 without BOM.
$eAcute=[char]0x00E9; $eGrave=[char]0x00E8; $eCirc=[char]0x00EA; $aGrave=[char]0x00E0
$cCed=[char]0x00E7; $iCirc=[char]0x00EE; $oCirc=[char]0x00F4; $uGrave=[char]0x00F9
$EAcute=[char]0x00C9

function U([string]$s) {
    return $s.Replace('{e}',[string]$eAcute).Replace('{eg}',[string]$eGrave).Replace('{ec}',[string]$eCirc).Replace('{a}',[string]$aGrave).Replace('{c}',[string]$cCed).Replace('{i}',[string]$iCirc).Replace('{o}',[string]$oCirc).Replace('{u}',[string]$uGrave).Replace('{E}',[string]$EAcute)
}
function SqlEscape([string]$s) { if($null -eq $s){return ''}; return $s.Replace("'","''").Replace("`r",' ').Replace("`n",' ') }
function Clean([string]$s) {
    if([string]::IsNullOrWhiteSpace($s)){return ''}
    $r=$s -replace '(?i)_name$','' -replace '_',' ' -replace '\s+',' '
    return $r.Trim()
}

$exact=[ordered]@{
    'AFTV Circle Rug' = U 'Tapis rond AFTV'
    'Comfy Couch' = U 'Canap{e} confortable'
    'On Track Wheel Shelf' = U '{E}tag{eg}re {a} roue On Track'
    'Study Rug' = U "Tapis d'{e}tude"
    'Cabinet of Curiosities' = U 'Cabinet de curiosit{e}s'
    'Artist Stool' = U "Tabouret d'artiste"
    'Deluxe Artist Chair' = U "Chaise d'artiste Deluxe"
    'Artist Desk' = U "Bureau d'artiste"
    'Gold Sequin Pillow' = U 'Oreiller {a} sequins dor{e}s'
    'Sequin Pillow' = U 'Oreiller {a} sequins'
    'Geometric Rug' = U 'Tapis g{e}om{e}trique'
    'Contemporary Sofa' = U 'Canap{e} contemporain'
}
$words=[ordered]@{
    'armchair'=U 'fauteuil'; 'chair'=U 'chaise'; 'couch'=U 'canap{e}'; 'sofa'=U 'canap{e}'; 'stool'='tabouret';
    'bench'='banc'; 'table'='table'; 'desk'='bureau'; 'bed'='lit'; 'pillow'='oreiller'; 'pillows'='oreillers';
    'cushion'='coussin'; 'cushions'='coussins'; 'cabinet'='meuble'; 'wardrobe'='armoire'; 'shelf'=U '{e}tag{eg}re';
    'bookcase'=U 'biblioth{eg}que'; 'lamp'='lampe'; 'light'=U 'lumi{eg}re'; 'rug'='tapis'; 'carpet'='tapis';
    'mirror'='miroir'; 'painting'='tableau'; 'paintings'='tableaux'; 'frame'='cadre'; 'vase'='vase'; 'candle'='bougie';
    'candles'='bougies'; 'kitchen'='cuisine'; 'fridge'=U 'r{e}frig{e}rateur'; 'oven'='four'; 'sink'=U '{e}vier';
    'toilet'='toilettes'; 'shower'='douche'; 'bath'='baignoire'; 'door'='porte'; 'window'=U 'fen{ec}tre';
    'wall'='mur'; 'floor'='sol'; 'round'='rond'; 'square'=U 'carr{e}'; 'small'='petit'; 'large'='grand';
    'modern'='moderne'; 'classic'='classique'; 'gold'=U 'dor{e}'; 'silver'=U 'argent{e}'; 'wood'='bois';
    'wooden'='en bois'; 'geometric'=U 'g{e}om{e}trique'; 'contemporary'='contemporain'; 'comfy'='confortable';
    'wheel'='roue'; 'curiosities'=U 'curiosit{e}s'; 'sequin'='sequins'; 'circle'='rond'; 'deluxe'='Deluxe'
}

function LooksTechnical([string]$s) {
    return $s -match '(?i)(_name$|^[a-z0-9]{2,8}[_-]|[_-][a-z0-9]+[_-]|^[a-z]{2,6}\d+[_-])'
}
function Translate([string]$old,[string]$public,[string]$item) {
    $base=$old
    if((LooksTechnical $base) -and -not [string]::IsNullOrWhiteSpace($public) -and -not (LooksTechnical $public)){ $base=$public }
    if([string]::IsNullOrWhiteSpace($base) -or $base -match '(?i)^\s*(null|none|default)\s*$'){ $base=$public }
    if([string]::IsNullOrWhiteSpace($base)){ $base=$item }
    $r=Clean $base
    if($exact.Contains($r)){ return [string]$exact[$r] }
    foreach($kv in $words.GetEnumerator()){
        $pat='(?i)(?<![A-Za-z0-9])'+[regex]::Escape($kv.Key)+'(?![A-Za-z0-9])'
        $r=[regex]::Replace($r,$pat,[string]$kv.Value)
    }
    $r=$r -replace '(?i)\bname\b','' -replace '\s+',' '
    $r=$r.Trim(' ','-','_')
    if($r.Length -gt 96){$r=$r.Substring(0,96).Trim()}
    return $r
}

$query=@"
SELECT ci.id,ci.catalog_name,IFNULL(f.item_name,''),IFNULL(f.public_name,'')
FROM catalog_items ci
LEFT JOIN furniture f ON f.id=CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(ci.item_ids,',',1),':',1) AS UNSIGNED)
WHERE ci.page_id=$pageId
ORDER BY ci.id;
"@
$lines=& $Mysql -u root -N -B --raw --default-character-set=utf8mb4 $Database -e $query
if($LASTEXITCODE -ne 0){throw 'Lecture BDD impossible.'}
$rows=[Collections.Generic.List[object]]::new()
foreach($line in $lines){
    $p=$line -split "`t",-1; if($p.Count -lt 4){continue}
    $new=Translate ([string]$p[1]) ([string]$p[3]) ([string]$p[2])
    $rows.Add([pscustomobject]@{Id=[long]$p[0];OldName=[string]$p[1];NewName=$new;Changed=($new -ne [string]$p[1] -and -not [string]::IsNullOrWhiteSpace($new));ItemName=[string]$p[2];PublicName=[string]$p[3]})
}
$rows|Export-Csv -LiteralPath $report -NoTypeInformation -Encoding UTF8
$changed=@($rows|Where-Object Changed)
$sb=[Text.StringBuilder]::new(); [void]$sb.AppendLine('SET NAMES utf8mb4;'); [void]$sb.AppendLine('START TRANSACTION;')
foreach($r in $changed){[void]$sb.AppendLine("UPDATE catalog_items SET catalog_name='$(SqlEscape $r.NewName)' WHERE id=$($r.Id) AND page_id=$pageId;")}
[void]$sb.AppendLine('COMMIT;')
[IO.File]::WriteAllText($sqlPath,$sb.ToString(),[Text.UTF8Encoding]::new($false))

Write-Host '=== PHASE 1D - MAISON ET DECORATION ==='
Write-Host "Page cible : $pageId"
Write-Host "Offres analysees : $($rows.Count)"
Write-Host "Noms qui seraient modifies : $($changed.Count)"
Write-Host 'Aucun page_id n est modifie.'
Write-Host 'Aucune page n est masquee ou supprimee.'
Write-Host "Rapport : $report"
Write-Host "Migration : $sqlPath"
Write-Host ''
Write-Host 'Exemples de changements :'
$changed|Select-Object -First 40|ForEach-Object{Write-Host (" - {0} -> {1}" -f $_.OldName,$_.NewName)}

if($Apply){
    & "$env:ComSpec" /c ('"'+$Mysql+'" --default-character-set=utf8mb4 -u root '+$Database+' < "'+$sqlPath+'"')
    if($LASTEXITCODE -ne 0){throw "Application SQL echouee (code $LASTEXITCODE)."}
    Write-Host 'Phase 1D appliquee.'
}
