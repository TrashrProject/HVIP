[CmdletBinding()]
param(
    [string]$Migration = "C:\HVIP\migrations\20260903_paradise_catalogue_fr_clean.sql",
    [string]$Report = "C:\HVIP\tools\catalogue-fr-clean-report.csv"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $Migration)) { throw "Migration introuvable : $Migration" }
if (-not (Test-Path -LiteralPath $Report)) { throw "Rapport introuvable : $Report" }

function Escape-Sql([AllowNull()][string]$s) {
    if ($null -eq $s) { return '' }
    return $s.Replace('\\','\\\\').Replace("'","''").Replace("`r",' ').Replace("`n",' ')
}

$rows = @(Import-Csv -LiteralPath $Report)
if ($rows.Count -eq 0) { throw 'Rapport catalogue vide.' }

# Garde la partie de creation des pages, mais remplace tous les UPDATE CASE
# par une table temporaire UTF8MB4. Cela evite les conflits de collations
# entre catalog_items.catalog_name et les litteraux traduits en francais.
$sql = Get-Content -LiteralPath $Migration -Raw
$marker = '#PARADISE_FR_UPDATES_BEGIN'

# Les anciennes migrations n'ont pas encore de marqueur : on coupe juste avant
# le premier UPDATE catalog_items SET page_id = CASE id.
$cut = $sql.IndexOf('UPDATE catalog_items SET page_id = CASE id')
if ($cut -lt 0) { throw 'Bloc UPDATE catalog_items introuvable dans la migration.' }
$prefix = $sql.Substring(0,$cut)

$sb = [Text.StringBuilder]::new()
[void]$sb.Append($prefix)
[void]$sb.AppendLine("-- $marker")
[void]$sb.AppendLine('DROP TEMPORARY TABLE IF EXISTS paradise_catalog_fr_tmp;')
[void]$sb.AppendLine("CREATE TEMPORARY TABLE paradise_catalog_fr_tmp (id BIGINT NOT NULL PRIMARY KEY, page_id INT NOT NULL, catalog_name VARCHAR(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL) ENGINE=InnoDB;")

$chunkSize = 300
for ($offset=0; $offset -lt $rows.Count; $offset += $chunkSize) {
    $chunk = @($rows | Select-Object -Skip $offset -First $chunkSize)
    [void]$sb.AppendLine('INSERT INTO paradise_catalog_fr_tmp (id,page_id,catalog_name) VALUES')
    $values = foreach ($r in $chunk) {
        $valid = [System.Convert]::ToBoolean($r.Valid)
        $page = if ($valid) {
            switch ($r.Category) {
                'Maison et decoration' {9967401}
                'Construction et architecture' {9967402}
                'Ville et services' {9967403}
                'Commerces et restauration' {9967404}
                'Nature et exterieurs' {9967405}
                'Jeux sport et musique' {9967406}
                'Fetes et saisons' {9967407}
                'Hopital et sante' {9967408}
                'Police et securite' {9967409}
                'Armee et militaire' {9967410}
                'Transports et vehicules' {9967411}
                'Technologie et bureau' {9967412}
                'Animaux' {9967413}
                'Rares et prestige' {9967414}
                default {9967415}
            }
        } else { 9967499 }
        "($($r.CatalogItemId),$page,_utf8mb4'$(Escape-Sql $r.FrenchName)')"
    }
    [void]$sb.AppendLine(($values -join ",`n") + ';')
}

[void]$sb.AppendLine("UPDATE catalog_items ci JOIN paradise_catalog_fr_tmp t ON t.id=ci.id SET ci.page_id=t.page_id, ci.catalog_name=CONVERT(t.catalog_name USING utf8mb4);")
[void]$sb.AppendLine("UPDATE catalog_pages SET visible='0',enabled='0' WHERE id BETWEEN 9967100 AND 9967399;")
[void]$sb.AppendLine('DROP TEMPORARY TABLE paradise_catalog_fr_tmp;')
[void]$sb.AppendLine('COMMIT;')

[IO.File]::WriteAllText($Migration,$sb.ToString(),[Text.UTF8Encoding]::new($false))
Write-Host 'Migration catalogue FR corrigee pour les collations.' -ForegroundColor Green
Write-Host "Lignes catalogue preparees : $($rows.Count)" -ForegroundColor Cyan
Write-Host "Migration : $Migration" -ForegroundColor Cyan
