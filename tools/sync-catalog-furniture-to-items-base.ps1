[CmdletBinding()]
param(
    [string]$Mysql = 'C:\xampp\mysql\bin\mysql.exe',
    [string]$MysqlDump = 'C:\xampp\mysql\bin\mysqldump.exe',
    [string]$Database = 'waveplus',
    [switch]$Apply
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$migration = Join-Path $repoRoot 'migrations\20260904_sync_catalog_furniture_to_items_base.sql'
$backupDir = Join-Path $repoRoot 'backups\catalogue'
$backup = Join-Path $backupDir 'items_base-before-catalog-sync.sql'

foreach ($p in @($Mysql, $MysqlDump)) {
    if (-not (Test-Path -LiteralPath $p)) { throw "Fichier requis absent : $p" }
}

function Invoke-Scalar([string]$sql) {
    $out = & $Mysql -u root -N -B --raw $Database -e $sql
    if ($LASTEXITCODE -ne 0) { throw "Requete SQL echouee : $sql" }
    return [long]($out | Select-Object -First 1)
}

$catalogMissingSql = @"
SELECT COUNT(*)
FROM catalog_items ci
LEFT JOIN items_base ib
  ON ib.id = CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(ci.item_ids, ',', 1), ':', 1) AS UNSIGNED)
WHERE ci.item_ids REGEXP '^[0-9]+'
  AND ib.id IS NULL;
"@

$syncableSql = @"
SELECT COUNT(DISTINCT f.id)
FROM catalog_items ci
JOIN furniture f
  ON f.id = CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(ci.item_ids, ',', 1), ':', 1) AS UNSIGNED)
LEFT JOIN items_base ib ON ib.id = f.id
WHERE ci.item_ids REGEXP '^[0-9]+'
  AND f.type IN ('s','i')
  AND ib.id IS NULL;
"@

$catalogMissing = Invoke-Scalar $catalogMissingSql
$syncable = Invoke-Scalar $syncableSql
$notSyncable = $catalogMissing - $syncable
$beforeCount = Invoke-Scalar 'SELECT COUNT(*) FROM items_base;'

$sql = @"
-- ParadiseRP - synchronisation prudente du catalogue vers items_base
-- Ne touche ni catalog_items, ni catalog_pages, ni furniture existant.
SET NAMES utf8mb4;
START TRANSACTION;

INSERT INTO items_base (
    id, sprite_id, public_name, item_name, type, width, length, stack_height,
    allow_stack, allow_sit, allow_lay, allow_walk, allow_gift, allow_trade,
    allow_recycle, allow_marketplace_sell, allow_inventory_stack,
    interaction_type, interaction_modes_count, vending_ids, multiheight,
    customparams, effect_id_male, effect_id_female, clothing_on_walk
)
SELECT DISTINCT
       f.id,
       COALESCE(f.sprite_id, 0),
       LEFT(COALESCE(NULLIF(f.public_name,''), f.item_name), 56),
       LEFT(f.item_name, 70),
       f.type,
       COALESCE(f.width, 1),
       COALESCE(f.length, 1),
       COALESCE(f.stack_height, 0),
       COALESCE(f.can_stack, 1),
       COALESCE(f.can_sit, 0),
       COALESCE(f.allow_lay, 0),
       COALESCE(f.is_walkable, 0),
       COALESCE(f.allow_gift, 1),
       COALESCE(f.allow_trade, 1),
       COALESCE(f.allow_recycle, 0),
       COALESCE(f.allow_marketplace_sell, 0),
       COALESCE(f.allow_inventory_stack, 1),
       LEFT(COALESCE(NULLIF(f.interaction_type,''), 'default'), 500),
       COALESCE(f.interaction_modes_count, 1),
       LEFT(COALESCE(f.vending_ids, '0'), 255),
       LEFT(COALESCE(f.height_adjustable, '0'), 50),
       '',
       COALESCE(f.effect_id, 0),
       COALESCE(f.effect_id, 0),
       IF(COALESCE(f.clothing_id,0) > 0, CAST(f.clothing_id AS CHAR), '')
FROM catalog_items ci
JOIN furniture f
  ON f.id = CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(ci.item_ids, ',', 1), ':', 1) AS UNSIGNED)
LEFT JOIN items_base ib ON ib.id = f.id
WHERE ci.item_ids REGEXP '^[0-9]+'
  AND f.type IN ('s','i')
  AND ib.id IS NULL;

COMMIT;
"@

[IO.File]::WriteAllText($migration, $sql, [Text.UTF8Encoding]::new($false))

Write-Host '=== SYNC CATALOGUE -> ITEMS_BASE ===' -ForegroundColor Cyan
Write-Host "items_base avant : $beforeCount"
Write-Host "Offres catalogue sans items_base : $catalogMissing"
Write-Host "Definitions uniques synchronisables depuis furniture : $syncable" -ForegroundColor Green
Write-Host "References catalogue non synchronisables automatiquement : $notSyncable" -ForegroundColor $(if($notSyncable -gt 0){'Yellow'}else{'Green'})
Write-Host "Migration : $migration"
Write-Host 'Aucun catalog_items/page_id/catalog_pages ne sera modifie.' -ForegroundColor Green

if (-not $Apply) {
    Write-Host 'Mode audit uniquement. Relancer avec -Apply seulement apres verification.' -ForegroundColor Yellow
    exit 0
}

New-Item -ItemType Directory -Force $backupDir | Out-Null
& $MysqlDump -u root $Database items_base --result-file=$backup
if ($LASTEXITCODE -ne 0) { throw 'Sauvegarde items_base impossible. Synchronisation annulee.' }
Write-Host "Sauvegarde : $backup" -ForegroundColor Cyan

& "$env:ComSpec" /c ('"'+$Mysql+'" -u root --default-character-set=utf8mb4 '+$Database+' < "'+$migration+'"')
if ($LASTEXITCODE -ne 0) { throw "Application SQL echouee (code $LASTEXITCODE)." }

$afterCount = Invoke-Scalar 'SELECT COUNT(*) FROM items_base;'
$remaining = Invoke-Scalar $catalogMissingSql
Write-Host "items_base apres : $afterCount" -ForegroundColor Green
Write-Host "Ajoutees : $($afterCount - $beforeCount)" -ForegroundColor Green
Write-Host "Offres catalogue encore sans items_base : $remaining" -ForegroundColor $(if($remaining -gt 0){'Yellow'}else{'Green'})
Write-Host 'Synchronisation terminee. Redemarrer WaveRP pour recharger ItemManager.' -ForegroundColor Green
