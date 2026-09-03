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
$report = Join-Path $PSScriptRoot 'catalog-phase1b-maison-fr.csv'
$sqlPath = Join-Path $repoRoot 'migrations\20260903_catalog_phase1b_maison_fr.sql'
$backupDir = Join-Path $repoRoot 'backups\catalogue'
New-Item -ItemType Directory -Force $backupDir | Out-Null
$backup = Join-Path $backupDir 'phase1b-maison-before.sql'

function SqlEscape([string]$s) {
    if ($null -eq $s) { return '' }
    return $s.Replace("'","''").Replace("`r",' ').Replace("`n",' ')
}

function Clean-TechnicalName([string]$s) {
    if ([string]::IsNullOrWhiteSpace($s)) { return '' }
    $r = $s -replace '(?i)_name$',''
    $r = $r -replace '_',' '
    $r = $r -replace '\s+',' '
    return $r.Trim()
}

$phraseMap = [ordered]@{
    'Cabinet of Curiosities'='Cabinet de curiosités'
    'Comfy Couch'='Canapé confortable'
    'Circle Rug'='Tapis rond'
    'Study Rug'="Tapis d'étude"
    'Artist Stool'="Tabouret d'artiste"
    'Deluxe Artist Chair'="Chaise d'artiste deluxe"
    'Artist Desk'="Bureau d'artiste"
    'Gold Sequin Pillow'='Oreiller à sequins dorés'
    'Sequin Pillow'='Oreiller à sequins'
    'Geometric Rug'='Tapis géométrique'
    'Contemporary Sofa'='Canapé contemporain'
    'Wheel Shelf'='Étagère à roue'
}

$wordMap = [ordered]@{
    'armchair'='fauteuil'; 'chair'='chaise'; 'couch'='canapé'; 'sofa'='canapé'; 'stool'='tabouret';
    'bench'='banc'; 'table'='table'; 'desk'='bureau'; 'bed'='lit'; 'pillow'='oreiller'; 'cushion'='coussin';
    'cabinet'='meuble'; 'wardrobe'='armoire'; 'shelf'='étagère'; 'bookcase'='bibliothèque'; 'lamp'='lampe';
    'light'='lumière'; 'rug'='tapis'; 'carpet'='tapis'; 'mirror'='miroir'; 'painting'='tableau'; 'frame'='cadre';
    'vase'='vase'; 'kitchen'='cuisine'; 'fridge'='réfrigérateur'; 'oven'='four'; 'sink'='évier'; 'toilet'='toilettes';
    'shower'='douche'; 'bath'='baignoire'; 'door'='porte'; 'window'='fenêtre'; 'wall'='mur'; 'floor'='sol';
    'stairs'='escalier'; 'stair'='escalier'; 'fence'='clôture'; 'gate'='portail'; 'pillar'='pilier'; 'column'='colonne';
    'brick'='brique'; 'concrete'='béton'; 'gold'='doré'; 'silver'='argenté'; 'wood'='bois'; 'wooden'='en bois';
    'round'='rond'; 'square'='carré'; 'small'='petit'; 'large'='grand'; 'modern'='moderne'; 'classic'='classique';
    'deluxe'='deluxe'; 'contemporary'='contemporain'; 'geometric'='géométrique'; 'study'="d'étude"; 'artist'="d'artiste";
    'comfy'='confortable'; 'wheel'='roue'; 'curiosities'='curiosités'; 'sequin'='sequins'; 'circle'='rond'
}

function Translate-Name([string]$name) {
    $r = Clean-TechnicalName $name
    if ([string]::IsNullOrWhiteSpace($r)) { return $r }

    foreach ($kv in $phraseMap.GetEnumerator()) {
        $r = [regex]::Replace($r,'(?i)'+[regex]::Escape($kv.Key),$kv.Value)
    }

    foreach ($kv in $wordMap.GetEnumerator()) {
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
$lines = & $Mysql -u root -N -B --raw $Database -e $query
if ($LASTEXITCODE -ne 0) { throw 'Lecture BDD impossible.' }

$rows = [Collections.Generic.List[object]]::new()
foreach ($line in $lines) {
    $p = $line -split "`t",-1
    if ($p.Count -lt 4) { continue }
    $id = [long]$p[0]
    $old = [string]$p[1]
    $itemName = [string]$p[2]
    $publicName = [string]$p[3]

    $base = $old
    if ([string]::IsNullOrWhiteSpace($base) -or $base -match '(?i)^\s*(null|none|default)\s*$') { $base = $publicName }
    if ([string]::IsNullOrWhiteSpace($base)) { $base = $itemName }
    $new = Translate-Name $base
    $changed = ($new -ne $old) -and -not [string]::IsNullOrWhiteSpace($new)

    $rows.Add([pscustomobject]@{Id=$id;OldName=$old;NewName=$new;Changed=$changed;ItemName=$itemName;PublicName=$publicName})
}

$rows | Export-Csv -LiteralPath $report -NoTypeInformation -Encoding UTF8
$changedRows = @($rows | Where-Object Changed)

$sb = [Text.StringBuilder]::new()
[void]$sb.AppendLine('SET NAMES utf8mb4;')
[void]$sb.AppendLine('START TRANSACTION;')
foreach ($r in $changedRows) {
    [void]$sb.AppendLine("UPDATE catalog_items SET catalog_name='$(SqlEscape $r.NewName)' WHERE id=$($r.Id) AND page_id=$pageId;")
}
[void]$sb.AppendLine('COMMIT;')
[IO.File]::WriteAllText($sqlPath,$sb.ToString(),[Text.UTF8Encoding]::new($false))

Write-Host '=== PHASE 1B - MAISON ET DECORATION ===' -ForegroundColor Cyan
Write-Host "Page cible : $pageId"
Write-Host "Offres analysees : $($rows.Count)"
Write-Host "Noms qui seraient modifies : $($changedRows.Count)"
Write-Host 'Aucun page_id n est modifie.' -ForegroundColor Green
Write-Host 'Aucune page n est masquee ou supprimee.' -ForegroundColor Green
Write-Host "Rapport : $report"
Write-Host "Migration : $sqlPath"
Write-Host ''
Write-Host 'Exemples de changements :' -ForegroundColor Cyan
$changedRows | Select-Object -First 30 | ForEach-Object { Write-Host (" - {0} -> {1}" -f $_.OldName,$_.NewName) }

if ($Apply) {
    & "$env:ComSpec" /c ('"'+$Mysql+'" -u root '+$Database+' < "'+$sqlPath+'"')
    if ($LASTEXITCODE -ne 0) { throw "Application SQL echouee (code $LASTEXITCODE)." }
    Write-Host 'Phase 1B appliquee.' -ForegroundColor Green
}
