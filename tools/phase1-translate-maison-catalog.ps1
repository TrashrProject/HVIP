[CmdletBinding()]
param(
    [string]$Mysql = "C:\xampp\mysql\bin\mysql.exe",
    [string]$Database = "waveplus",
    [switch]$Apply
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$pageId = 9967202
$repoRoot = Split-Path -Parent $PSScriptRoot
$sqlPath = Join-Path $repoRoot 'migrations\20260903_catalog_phase1_maison_fr.sql'
$reportPath = Join-Path $PSScriptRoot 'catalog-phase1-maison-fr.csv'

if (-not (Test-Path -LiteralPath $Mysql)) { throw "mysql.exe introuvable : $Mysql" }

function Escape-Sql([AllowNull()][string]$s) {
    if ($null -eq $s) { return '' }
    return $s.Replace("'","''").Replace("`r",' ').Replace("`n",' ')
}

function Normalize([string]$s) {
    if ([string]::IsNullOrWhiteSpace($s)) { return '' }
    return (($s -replace '[_.-]+',' ') -replace '\s+',' ').Trim()
}

$translations = [ordered]@{
    'armchair'='Fauteuil'; 'chair'='Chaise'; 'sofa'='Canape'; 'couch'='Canape'; 'stool'='Tabouret'; 'bench'='Banc';
    'table'='Table'; 'desk'='Bureau'; 'bed'='Lit'; 'pillow'='Oreiller'; 'cushion'='Coussin'; 'cabinet'='Meuble';
    'wardrobe'='Armoire'; 'shelf'='Etagere'; 'bookcase'='Bibliotheque'; 'lamp'='Lampe'; 'light'='Lumiere';
    'rug'='Tapis'; 'carpet'='Tapis'; 'mirror'='Miroir'; 'painting'='Tableau'; 'frame'='Cadre'; 'vase'='Vase';
    'kitchen'='Cuisine'; 'fridge'='Refrigerateur'; 'refrigerator'='Refrigerateur'; 'oven'='Four'; 'sink'='Evier';
    'toilet'='Toilettes'; 'shower'='Douche'; 'bath'='Baignoire'; 'door'='Porte'; 'window'='Fenetre';
    'wall'='Mur'; 'floor'='Sol'; 'stairs'='Escalier'; 'stair'='Escalier'; 'fireplace'='Cheminee';
    'candle'='Bougie'; 'clock'='Horloge'; 'plant'='Plante'; 'flower'='Fleur'; 'basket'='Panier';
    'curtain'='Rideau'; 'drawer'='Tiroir'; 'dresser'='Commode'; 'cupboard'='Placard'; 'bookshelf'='Bibliotheque';
    'mat'='Tapis'; 'blanket'='Couverture'; 'bowl'='Bol'; 'plate'='Assiette'; 'glass'='Verre'; 'cup'='Tasse';
    'coffee'='Cafe'; 'tea'='The'; 'dining'='Salle a manger'; 'living room'='Salon'; 'bedroom'='Chambre';
    'bathroom'='Salle de bain'; 'home'='Maison'; 'house'='Maison'; 'decor'='Decoration'; 'decoration'='Decoration';
    'small'='Petit'; 'large'='Grand'; 'round'='Rond'; 'wooden'='En bois'; 'wood'='Bois'; 'white'='Blanc';
    'black'='Noir'; 'red'='Rouge'; 'blue'='Bleu'; 'green'='Vert'; 'yellow'='Jaune'; 'pink'='Rose';
    'purple'='Violet'; 'orange'='Orange'; 'brown'='Marron'; 'grey'='Gris'; 'gray'='Gris'; 'gold'='Dore'; 'silver'='Argente'
}

function Translate-Name([string]$name) {
    $result = Normalize $name
    if ([string]::IsNullOrWhiteSpace($result)) { return $result }
    foreach ($kv in $translations.GetEnumerator()) {
        $pattern = '(?i)(?<![A-Za-z0-9])' + [regex]::Escape($kv.Key) + '(?![A-Za-z0-9])'
        $result = [regex]::Replace($result,$pattern,$kv.Value)
    }
    $result = (($result -replace '\s+',' ').Trim())
    if ($result.Length -gt 96) { $result = $result.Substring(0,96).Trim() }
    return $result
}

$sql = @"
SELECT ci.id,ci.catalog_name,f.item_name,f.public_name
FROM catalog_items ci
LEFT JOIN furniture f ON f.id = CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(ci.item_ids, ',', 1), ':', 1) AS UNSIGNED)
WHERE ci.page_id=$pageId
ORDER BY ci.id;
"@

$lines = & $Mysql -u root -N -B --raw --default-character-set=utf8mb4 $Database -e $sql
if ($LASTEXITCODE -ne 0) { throw 'Lecture de la categorie Maison impossible.' }

$rows = [Collections.Generic.List[object]]::new()
foreach($line in $lines) {
    $p = $line -split "`t",-1
    if($p.Count -lt 4) { continue }
    $old = [string]$p[1]
    $base = $old
    if([string]::IsNullOrWhiteSpace($base) -or $base -match '^\s*(null|none|default)\s*$') { $base = [string]$p[3] }
    if([string]::IsNullOrWhiteSpace($base)) { $base = [string]$p[2] }
    $fr = Translate-Name $base
    $changed = ($fr -ne $old -and -not [string]::IsNullOrWhiteSpace($fr))
    $rows.Add([pscustomobject]@{
        CatalogItemId=[long]$p[0]
        OldName=$old
        SourceName=$base
        FrenchName=$fr
        Changed=$changed
    })
}

$changedRows = @($rows | Where-Object Changed)
$rows | Export-Csv -LiteralPath $reportPath -NoTypeInformation -Encoding UTF8

$sb = [Text.StringBuilder]::new()
[void]$sb.AppendLine('-- ParadiseRP - Phase 1 catalogue : Maison et decoration uniquement')
[void]$sb.AppendLine('SET NAMES utf8mb4;')
[void]$sb.AppendLine('START TRANSACTION;')
[void]$sb.AppendLine('CREATE TABLE IF NOT EXISTS catalog_items_phase1_backup LIKE catalog_items;')
[void]$sb.AppendLine("DELETE FROM catalog_items_phase1_backup WHERE page_id=$pageId;")
[void]$sb.AppendLine("INSERT INTO catalog_items_phase1_backup SELECT * FROM catalog_items WHERE page_id=$pageId;")
[void]$sb.AppendLine("UPDATE catalog_pages SET caption='Maison et decoration',caption_save='Maison et decoration' WHERE id=$pageId;")
foreach($r in $changedRows) {
    [void]$sb.AppendLine("UPDATE catalog_items SET catalog_name='$(Escape-Sql $r.FrenchName)' WHERE id=$($r.CatalogItemId) AND page_id=$pageId;")
}
[void]$sb.AppendLine('COMMIT;')
[IO.File]::WriteAllText($sqlPath,$sb.ToString(),[Text.UTF8Encoding]::new($false))

Write-Host '=== PHASE 1 - MAISON ET DECORATION ===' -ForegroundColor Cyan
Write-Host "Page cible : $pageId" -ForegroundColor White
Write-Host "Offres analysees : $($rows.Count)" -ForegroundColor White
Write-Host "Noms qui seraient modifies : $($changedRows.Count)" -ForegroundColor Yellow
Write-Host "Aucun page_id n'est modifie." -ForegroundColor Green
Write-Host "Aucune page n'est masquee/supprimee." -ForegroundColor Green
Write-Host "Rapport : $reportPath" -ForegroundColor Cyan
Write-Host "Migration : $sqlPath" -ForegroundColor Cyan
Write-Host ''
Write-Host 'Exemples de changements :' -ForegroundColor Cyan
$changedRows | Select-Object -First 25 | ForEach-Object { Write-Host (" - {0} -> {1}" -f $_.OldName,$_.FrenchName) }

if($Apply) {
    Get-Content -LiteralPath $sqlPath -Raw | & $Mysql -u root --default-character-set=utf8mb4 $Database
    if($LASTEXITCODE -ne 0) { throw "Application SQL echouee (code $LASTEXITCODE)." }
    Write-Host 'PHASE 1 appliquee avec succes.' -ForegroundColor Green
}
