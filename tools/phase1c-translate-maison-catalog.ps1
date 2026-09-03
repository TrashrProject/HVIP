[CmdletBinding()]
param(
    [string]$Mysql = 'C:\xampp\mysql\bin\mysql.exe',
    [string]$Database = 'waveplus',
    [switch]$Apply
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)
$OutputEncoding = [Text.UTF8Encoding]::new($false)

$pageId = 9967202
$repoRoot = Split-Path -Parent $PSScriptRoot
$report = Join-Path $PSScriptRoot 'catalog-phase1c-maison-fr.csv'
$sqlPath = Join-Path $repoRoot 'migrations\20260903_catalog_phase1c_maison_fr.sql'

function SqlEscape([string]$s) {
    if ($null -eq $s) { return '' }
    return $s.Replace("'","''").Replace("`r",' ').Replace("`n",' ')
}

function Clean-TechnicalName([string]$s) {
    if ([string]::IsNullOrWhiteSpace($s)) { return '' }
    $r = $s.Trim()
    $r = $r -replace '(?i)_name$',''
    $r = $r -replace '_',' '
    $r = $r -replace '\s+',' '
    return $r.Trim()
}

function Is-TechnicalName([string]$s) {
    if ([string]::IsNullOrWhiteSpace($s)) { return $true }
    return ($s -match '(?i)_name$|^[a-z0-9]{2,12}[_-][a-z0-9_\-]+$|^[a-z0-9]{2,10}\d{1,4}[_-]')
}

$exact = @{
    'AFTV Circle Rug'='Tapis rond AFTV'
    'Comfy Couch'='Canapé confortable'
    'On Track Wheel Shelf'='Étagère à roue On Track'
    'Study Rug'="Tapis d'étude"
    'Cabinet of Curiosities'='Cabinet de curiosités'
    'Artist Stool'="Tabouret d'artiste"
    'Deluxe Artist Chair'="Chaise d'artiste Deluxe"
    'Artist Desk'="Bureau d'artiste"
    'Gold Sequin Pillow'='Oreiller à sequins dorés'
    'Sequin Pillow'='Oreiller à sequins'
    'Geometric Rug'='Tapis géométrique'
    'Contemporary Sofa'='Canapé contemporain'
}

$words = [ordered]@{
    'armchair'='fauteuil'; 'chair'='chaise'; 'couch'='canapé'; 'sofa'='canapé'; 'stool'='tabouret';
    'bench'='banc'; 'desk'='bureau'; 'bed'='lit'; 'pillow'='oreiller'; 'pillows'='oreillers';
    'cushion'='coussin'; 'cushions'='coussins'; 'wardrobe'='armoire'; 'shelf'='étagère'; 'bookcase'='bibliothèque';
    'lamp'='lampe'; 'light'='lumière'; 'rug'='tapis'; 'carpet'='tapis'; 'mirror'='miroir'; 'painting'='tableau';
    'frame'='cadre'; 'vase'='vase'; 'kitchen'='cuisine'; 'fridge'='réfrigérateur'; 'oven'='four'; 'sink'='évier';
    'toilet'='toilettes'; 'shower'='douche'; 'bath'='baignoire'; 'door'='porte'; 'window'='fenêtre';
    'table'='table'; 'cabinet'='meuble'; 'round'='rond'; 'square'='carré'; 'small'='petit'; 'large'='grand';
    'modern'='moderne'; 'classic'='classique'; 'contemporary'='contemporain'; 'geometric'='géométrique';
    'gold'='doré'; 'silver'='argenté'; 'wooden'='en bois'; 'wood'='bois'; 'artist'="d'artiste";
    'deluxe'='Deluxe'; 'study'="d'étude"; 'wheel'='roue'; 'sequin'='sequins'; 'circle'='rond'
}

function Translate-Name([string]$name) {
    $r = Clean-TechnicalName $name
    if ([string]::IsNullOrWhiteSpace($r)) { return $r }
    if ($exact.ContainsKey($r)) { return $exact[$r] }

    foreach ($kv in $words.GetEnumerator()) {
        $pattern = '(?i)(?<![A-Za-z0-9])'+[regex]::Escape($kv.Key)+'(?![A-Za-z0-9])'
        $r = [regex]::Replace($r,$pattern,$kv.Value)
    }

    $r = $r -replace '(?i)\bname\b',''
    $r = $r -replace '\s+',' '
    $r = $r.Trim(' ','-','_')
    if ($r.Length -gt 96) { $r = $r.Substring(0,96).Trim() }
    return $r
}

$query = @"
SELECT ci.id,ci.catalog_name,f.item_name,f.public_name
FROM catalog_items ci
LEFT JOIN furniture f ON f.id=CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(ci.item_ids,',',1),':',1) AS UNSIGNED)
WHERE ci.page_id=$pageId
ORDER BY ci.id;
"@
$lines = & $Mysql -u root -N -B --raw --default-character-set=utf8mb4 $Database -e $query
if ($LASTEXITCODE -ne 0) { throw 'Lecture BDD impossible.' }

$rows = [Collections.Generic.List[object]]::new()
foreach ($line in $lines) {
    $p = $line -split "`t",-1
    if ($p.Count -lt 4) { continue }
    $id=[long]$p[0]; $old=[string]$p[1]; $item=[string]$p[2]; $public=[string]$p[3]

    if (-not (Is-TechnicalName $public)) { $base=$public }
    elseif (-not (Is-TechnicalName $old)) { $base=$old }
    elseif (-not (Is-TechnicalName $item)) { $base=$item }
    elseif (-not [string]::IsNullOrWhiteSpace($public)) { $base=$public }
    elseif (-not [string]::IsNullOrWhiteSpace($old)) { $base=$old }
    else { $base=$item }

    $new=Translate-Name $base
    $changed=($new -ne $old) -and -not [string]::IsNullOrWhiteSpace($new)
    $rows.Add([pscustomobject]@{Id=$id;OldName=$old;BaseName=$base;NewName=$new;Changed=$changed;ItemName=$item;PublicName=$public})
}

$rows | Export-Csv -LiteralPath $report -NoTypeInformation -Encoding UTF8
$changedRows=@($rows|Where-Object Changed)

$sb=[Text.StringBuilder]::new()
[void]$sb.AppendLine('SET NAMES utf8mb4;')
[void]$sb.AppendLine('START TRANSACTION;')
foreach($r in $changedRows){
    [void]$sb.AppendLine("UPDATE catalog_items SET catalog_name=CONVERT(_utf8mb4'$(SqlEscape $r.NewName)' USING utf8mb4) WHERE id=$($r.Id) AND page_id=$pageId;")
}
[void]$sb.AppendLine('COMMIT;')
[IO.File]::WriteAllText($sqlPath,$sb.ToString(),[Text.UTF8Encoding]::new($false))

Write-Host '=== PHASE 1C - MAISON ET DECORATION ===' -ForegroundColor Cyan
Write-Host "Page cible : $pageId"
Write-Host "Offres analysees : $($rows.Count)"
Write-Host "Noms qui seraient modifies : $($changedRows.Count)"
Write-Host 'Aucun page_id n est modifie.' -ForegroundColor Green
Write-Host 'Aucune page n est masquee ou supprimee.' -ForegroundColor Green
Write-Host "Rapport : $report"
Write-Host "Migration : $sqlPath"
Write-Host ''
Write-Host 'Exemples de changements :' -ForegroundColor Cyan
$changedRows | Select-Object -First 40 | ForEach-Object { Write-Host (" - {0} -> {1}" -f $_.OldName,$_.NewName) }

if($Apply){
    & "$env:ComSpec" /c ('"'+$Mysql+'" --default-character-set=utf8mb4 -u root '+$Database+' < "'+$sqlPath+'"')
    if($LASTEXITCODE -ne 0){ throw "Application SQL echouee (code $LASTEXITCODE)." }
    Write-Host 'Phase 1C appliquee.' -ForegroundColor Green
}
