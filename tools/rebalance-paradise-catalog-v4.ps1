[CmdletBinding()]
param(
    [string]$RepositoryRoot = "",
    [string]$MigrationPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = Split-Path -Parent $PSScriptRoot
}
if ([string]::IsNullOrWhiteSpace($MigrationPath)) {
    $MigrationPath = Join-Path $RepositoryRoot "migrations\20260903_paradise_catalogue_extension_v4.sql"
}
if (-not (Test-Path -LiteralPath $MigrationPath -PathType Leaf)) {
    throw "Migration catalogue introuvable : $MigrationPath"
}

$content = Get-Content -LiteralPath $MigrationPath -Raw

$pages = @(
    @(9967100,9967000,'Extension 2000 meubles',1),
    @(9967102,9967100,'Construction et architecture',1),
    @(9967103,9967100,'Maison et decoration',2),
    @(9967104,9967100,'Ville services et transports',3),
    @(9967107,9967100,'Commerces et restauration',4),
    @(9967106,9967100,'Nature et exterieurs',5),
    @(9967111,9967100,'Loisirs jeux et musique',6),
    @(9967112,9967100,'Saisons et evenements',7),
    @(9967113,9967100,'Custom - Habbox',8),
    @(9967114,9967100,'Custom - Yvess',9),
    @(9967115,9967100,'Custom - Habblet',10),
    @(9967116,9967100,'Custom - CSTM',11),
    @(9967117,9967100,'Custom - Atlanta',12),
    @(9967118,9967100,'Custom A-H',13),
    @(9967119,9967100,'Custom I-P',14),
    @(9967120,9967100,'Custom Q-Z',15)
)

$pageRows = foreach ($p in $pages) {
    "($($p[0]),$($p[1]),'$($p[2])',1,'1','1',1,0,$($p[3]),'','default_3x3','','')"
}
$pageBlock = "INSERT INTO catalog_pages (id,parent_id,caption,icon_image,visible,enabled,min_rank,min_vip,order_num,page_link,page_layout,page_strings_1,page_strings_2) VALUES`r`n" +
    ($pageRows -join ",`r`n") +
    "`r`nON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption=VALUES(caption),visible='1',enabled='1',order_num=VALUES(order_num);"

$content = [regex]::Replace(
    $content,
    "INSERT INTO catalog_pages \(id,parent_id,caption,icon_image,visible,enabled,min_rank,min_vip,order_num,page_link,page_layout,page_strings_1,page_strings_2\) VALUES\r?\n.*?ON DUPLICATE KEY UPDATE parent_id=VALUES\(parent_id\),caption=VALUES\(caption\),visible='1',enabled='1',order_num=VALUES\(order_num\);",
    [System.Text.RegularExpressions.MatchEvaluator]{ param($m) $pageBlock },
    [System.Text.RegularExpressions.RegexOptions]::Singleline
)

$keepIds = ($pages | Where-Object { $_[0] -ne 9967100 } | ForEach-Object { $_[0] }) -join ','
$content = [regex]::Replace(
    $content,
    "DELETE FROM catalog_pages WHERE id BETWEEN 9967101 AND 9967199 AND id NOT IN \([^;]+\);",
    "DELETE FROM catalog_pages WHERE id BETWEEN 9967101 AND 9967199 AND id NOT IN ($keepIds);"
)

function Get-CustomPage([string]$Name) {
    $n = $Name.ToLowerInvariant()
    if ($n -match '^habbox_') { return 9967113 }
    if ($n -match '^yvess5_|^yvesss_') { return 9967114 }
    if ($n -match '^habblet_') { return 9967115 }
    if ($n -match '^cstm_') { return 9967116 }
    if ($n -match '^atlanta_') { return 9967117 }

    $first = if ($n.Length -gt 0) { $n.Substring(0,1) } else { '' }
    if ($first -match '[a-h0-9]') { return 9967118 }
    if ($first -match '[i-p]') { return 9967119 }
    return 9967120
}

$lines = $content -split "`r?`n"
$changed = 0
for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    if ($line -match "^\(9967113,'[^']*','([^']*)',") {
        $name = $Matches[1]
        $page = Get-CustomPage $name
        if ($page -ne 9967113) {
            $lines[$i] = $line -replace '^\(9967113,', "($page,"
            $changed++
        }
    }
}
$content = $lines -join "`r`n"

# Compter uniquement les lignes de l'INSERT catalog_items.
$catalogItemsMatch = [regex]::Match(
    $content,
    "INSERT INTO catalog_items \([^;]+?\) VALUES\r?\n(?<rows>.*?);\r?\nCOMMIT;",
    [System.Text.RegularExpressions.RegexOptions]::Singleline
)
if (-not $catalogItemsMatch.Success) {
    throw 'Bloc INSERT INTO catalog_items introuvable dans la migration.'
}
$catalogRowsText = $catalogItemsMatch.Groups['rows'].Value

[IO.File]::WriteAllText($MigrationPath, $content, [Text.UTF8Encoding]::new($false))

Write-Host "Catalogue custom reequilibre : $MigrationPath" -ForegroundColor Green
Write-Host "Offres custom deplacees : $changed" -ForegroundColor Green
Write-Host "Repartition SQL finale :" -ForegroundColor Cyan
$total = 0
foreach ($page in $pages | Where-Object { $_[0] -ne 9967100 }) {
    $id = [int]$page[0]
    $count = ([regex]::Matches($catalogRowsText, "(?m)^\($id,")).Count
    $total += $count
    $color = if ($count -eq 0) { 'Yellow' } else { 'Green' }
    Write-Host (" - {0}: {1} offres" -f $page[2], $count) -ForegroundColor $color
}
Write-Host "Total offres catalogue : $total" -ForegroundColor Cyan
if ($total -ne 2000) {
    throw "Le reequilibrage attend 2000 offres, mais en compte $total."
}
