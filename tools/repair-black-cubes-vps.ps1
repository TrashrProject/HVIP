[CmdletBinding()]
param(
    [string]$RepositoryPath = 'C:\HVIP',
    [string]$ConfigPath = '',
    [string]$Mysql = 'C:\xampp\mysql\bin\mysql.exe',
    [switch]$CheckOnly
)

# Standalone WavePlus repair: no git operations, build, or process restart.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$RepositoryPath = (Resolve-Path -LiteralPath $RepositoryPath).Path
if (-not $ConfigPath) {
    # Emulator.main loads config.ini relative to the launcher's runtime directory.
    $ConfigPath = Join-Path $RepositoryPath 'runtime\WavePlus\config.ini'
}
if (-not (Test-Path -LiteralPath $ConfigPath -PathType Leaf)) {
    throw "Configuration runtime absente : $ConfigPath. Preciser le fichier utilise par l'emulateur avec -ConfigPath."
}
if (-not (Test-Path -LiteralPath $Mysql -PathType Leaf)) { throw "mysql.exe absent : $Mysql" }
$configLines = Get-Content -LiteralPath $ConfigPath
function Get-Setting([string]$Key, [string]$Default = '') {
    $line = $configLines | Where-Object { $_ -match ('^\s*' + [regex]::Escape($Key) + '\s*=') } | Select-Object -Last 1
    if ($null -eq $line) { return $Default }
    return ($line -split '=', 2)[1].Trim()
}
# WavePlus/config.ini.example and DatabasePool use db.database.
# Keep db.name only as a fallback for configurations from older packs.
$database = Get-Setting 'db.database'
if ([string]::IsNullOrWhiteSpace($database)) { $database = Get-Setting 'db.name' }
if ([string]::IsNullOrWhiteSpace($database)) { throw "db.database absent du fichier $ConfigPath (db.name accepte en compatibilite)." }
# Build each argument separately: Windows PowerShell must not concatenate the array.
$dbArgs = @("--host=$(Get-Setting 'db.hostname' '127.0.0.1')", "--port=$(Get-Setting 'db.port' '3306')", "--user=$(Get-Setting 'db.username' 'root')")
$previousPassword = $env:MYSQL_PWD
function Invoke-Sql([string]$Sql) {
    $result = & $Mysql @dbArgs "--database=$database" --default-character-set=utf8mb4 --batch --skip-column-names "--execute=$Sql"
    if ($LASTEXITCODE -ne 0) { throw 'Requete SQL echouee. Aucun redemarrage automatique.' }
    return $result
}
try {
    $env:MYSQL_PWD = Get-Setting 'db.password'
    Write-Host "=== Reparation ciblee cubes noirs / $database ===" -ForegroundColor Cyan
    $schema = Invoke-Sql "SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE() AND ((table_name='catalog_items' AND column_name='item_ids') OR (table_name='items_base' AND column_name='sprite_id'))"
    if ($schema -ne '2') { throw 'Ce correctif exige WavePlus : catalog_items.item_ids et items_base.sprite_id.' }
    $collisions = Invoke-Sql "SELECT COUNT(*) FROM items_base WHERE (id=5480 AND item_name NOT IN ('bc_block_1*13','bc_block_1*14')) OR (id=5466 AND item_name <> 'bc_block_0*13')"
    if ($collisions -ne '0') { throw 'Un ID de cube appartient a un autre mobilier. Arret pour conserver ce mobilier.' }
    $page = Invoke-Sql "SELECT COUNT(*) FROM catalog_pages WHERE id=9967201 AND visible='1' AND enabled='1'"
    if ($page -ne '1') { throw 'La page 9967201 (Construction - Blocs couleurs) est absente ou masquee.' }
    Invoke-Sql "SELECT id,item_name,sprite_id FROM items_base WHERE id IN (5480,5466)" | Write-Host
    if ($CheckOnly) { Write-Host 'Audit termine : aucune modification appliquee.'; return }

    $dump = Join-Path (Split-Path -Parent $Mysql) 'mysqldump.exe'
    if (-not (Test-Path -LiteralPath $dump -PathType Leaf)) { throw 'mysqldump.exe absent : sauvegarde impossible.' }
    $backup = Join-Path $RepositoryPath ('backups\black-cubes-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $backup | Out-Null
    $filters = @{
        items_base = 'id IN (5480,5466)'
        catalog_items = "page_id IN (9967201,9967601,9967602) AND item_ids IN ('5480','5466')"
    }
    foreach ($table in @('items_base','catalog_items')) {
        & $dump @dbArgs --default-character-set=utf8mb4 --no-tablespaces --skip-lock-tables --no-create-info --skip-add-locks --complete-insert "--where=$($filters[$table])" "--result-file=$(Join-Path $backup ($table + '.sql'))" $database $table
        if ($LASTEXITCODE -ne 0) { throw "Sauvegarde $table echouee. Reparation annulee." }
    }
    Write-Host "Sauvegarde des lignes concernees : $backup"

    $sql = @'
START TRANSACTION;
INSERT INTO items_base
 (id,sprite_id,public_name,item_name,type,width,length,stack_height,
 allow_stack,allow_sit,allow_lay,allow_walk,allow_gift,allow_trade,
 allow_recycle,allow_marketplace_sell,allow_inventory_stack,
 interaction_type,interaction_modes_count,vending_ids,multiheight,
 customparams,effect_id_male,effect_id_female,clothing_on_walk)
VALUES
 (5480,5480,'Grand Cube noir','bc_block_1*13','s',1,1,1,
 '1','0','0','1','1','1','0','1','1','default',5,'0','0','',0,0,''),
 (5466,5466,'Petit Cube noir','bc_block_0*13','s',1,1,1,
 '1','0','0','1','1','1','0','1','1','default',5,'0','1.0,0.75,0.5,0.25,0.1','',0,0,'')
ON DUPLICATE KEY UPDATE item_name=VALUES(item_name),sprite_id=VALUES(sprite_id),public_name=VALUES(public_name);

INSERT INTO catalog_items
 (item_ids,page_id,catalog_name,cost_credits,cost_points,points_type,amount,
 limited_stack,limited_sells,order_number,offer_id,song_id,extradata,have_offer,club_only)
SELECT CAST(b.id AS CHAR),p.id,b.public_name,3,0,0,1,0,0,
 CASE WHEN b.id=5480 THEN 0 ELSE 1 END,-1,0,'','1','0'
FROM items_base b JOIN catalog_pages p
 ON p.id=9967201 OR (b.id=5480 AND p.id=9967601) OR (b.id=5466 AND p.id=9967602)
WHERE b.id IN (5480,5466) AND p.visible='1' AND p.enabled='1'
 AND NOT EXISTS (SELECT 1 FROM catalog_items c WHERE c.page_id=p.id AND c.item_ids=CAST(b.id AS CHAR));

UPDATE catalog_items c JOIN items_base b ON c.item_ids=CAST(b.id AS CHAR)
SET c.catalog_name=b.public_name,c.order_number=CASE WHEN b.id=5480 THEN 0 ELSE 1 END,
 c.club_only='0',c.have_offer='1'
WHERE b.id IN (5480,5466) AND c.page_id IN (9967201,9967601,9967602);
COMMIT;
'@
    Invoke-Sql $sql | Out-Null
    $count = Invoke-Sql "SELECT COUNT(DISTINCT b.id) FROM items_base b JOIN catalog_items c ON c.item_ids=CAST(b.id AS CHAR) JOIN catalog_pages p ON p.id=c.page_id WHERE c.page_id=9967201 AND p.visible='1' AND p.enabled='1' AND b.sprite_id=b.id AND ((b.id=5480 AND b.item_name='bc_block_1*13') OR (b.id=5466 AND b.item_name='bc_block_0*13'))"
    if ($count -ne '2') { throw "Verification echouee : $count/2 definitions avec offres. Sauvegarde : $backup" }
    Write-Host 'OK : 2/2 cubes noirs avec definitions items_base et offres catalogue.' -ForegroundColor Green
    Invoke-Sql "SELECT c.catalog_name,p.caption,b.id,b.sprite_id FROM items_base b JOIN catalog_items c ON c.item_ids=CAST(b.id AS CHAR) JOIN catalog_pages p ON p.id=c.page_id WHERE b.id IN (5480,5466) AND p.id IN (9967201,9967601,9967602) ORDER BY p.id,c.order_number" | Write-Host
    Write-Host 'Redemarre maintenant le runtime WaveRP existant pour recharger les definitions et le catalogue, puis reconnecte le client.' -ForegroundColor Yellow
} finally {
    $env:MYSQL_PWD = $previousPassword
}
