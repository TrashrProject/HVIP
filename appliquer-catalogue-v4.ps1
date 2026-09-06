[CmdletBinding()]
param(
    [string]$RepositoryPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepositoryPath)) {
    $RepositoryPath = Split-Path -Parent $MyInvocation.MyCommand.Path
}

$RepositoryRoot = (Resolve-Path -LiteralPath $RepositoryPath).Path
$RuntimeDirectory = Join-Path $RepositoryRoot "runtime\WavePlus"
$RuntimeConfig = Join-Path $RuntimeDirectory "config.ini"

function Get-IniValue {
    param([string]$Path, [string]$Key, [string]$Default = "")
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $Default }
    $Line = Get-Content -LiteralPath $Path | Where-Object { $_ -match "^\s*$([regex]::Escape($Key))\s*=" } | Select-Object -Last 1
    if (-not $Line) { return $Default }
    return (($Line -split '=', 2)[1]).Trim()
}

if (-not (Test-Path -LiteralPath $RuntimeConfig -PathType Leaf)) {
    throw "Configuration WavePlus introuvable : $RuntimeConfig"
}

$DatabaseHost = Get-IniValue $RuntimeConfig 'db.hostname' '127.0.0.1'
$DatabasePort = Get-IniValue $RuntimeConfig 'db.port' '3306'
$DatabaseName = Get-IniValue $RuntimeConfig 'db.database'
if ([string]::IsNullOrWhiteSpace($DatabaseName)) { $DatabaseName = Get-IniValue $RuntimeConfig 'db.name' }
$DatabaseUser = Get-IniValue $RuntimeConfig 'db.username' 'root'
$DatabasePassword = Get-IniValue $RuntimeConfig 'db.password' ''

if ([string]::IsNullOrWhiteSpace($DatabaseName)) {
    throw "Nom de base absent de $RuntimeConfig"
}

$Mysql = @(
    'C:\xampp\mysql\bin\mysql.exe',
    'C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe'
) | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1

if (-not $Mysql) {
    $MysqlCommand = Get-Command mysql.exe -ErrorAction SilentlyContinue
    if ($MysqlCommand) { $Mysql = $MysqlCommand.Source }
}
if (-not $Mysql) { throw 'mysql.exe introuvable.' }

try {
    $env:MYSQL_PWD = $DatabasePassword
    $CommonArgs = @(
        "--host=$DatabaseHost",
        "--port=$DatabasePort",
        "--user=$DatabaseUser",
        "--database=$DatabaseName",
        '--batch',
        '--skip-column-names'
    )

    $SchemaSql = "SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='catalog_items' AND column_name='item_ids'"
    $LegacyCatalog = (((& $Mysql @CommonArgs "--execute=$SchemaSql") -join '').Trim() -eq '1')
    if ($LASTEXITCODE -ne 0) { throw 'Detection du schema catalogue impossible.' }

    $MigrationRelativePath = if ($LegacyCatalog) {
        'migrations\20260906_paradise_catalogue_taxonomy_v4_global_legacy.sql'
    } else {
        'migrations\20260906_paradise_catalogue_taxonomy_v4_global.sql'
    }
    $AllItemsMigrationRelativePath = if ($LegacyCatalog) {
        'migrations\20260906_paradise_catalogue_taxonomy_v4_all_items_legacy.sql'
    } else {
        'migrations\20260906_paradise_catalogue_taxonomy_v4_all_items.sql'
    }
    $BlockFamiliesMigrationRelativePath = if ($LegacyCatalog) {
        'migrations\20260906_paradise_catalogue_blocks_families_v5_legacy.sql'
    } else {
        'migrations\20260906_paradise_catalogue_blocks_families_v5.sql'
    }

    $Migration = Join-Path $RepositoryRoot $MigrationRelativePath
    $AllItemsMigration = Join-Path $RepositoryRoot $AllItemsMigrationRelativePath
    $BlockFamiliesMigration = Join-Path $RepositoryRoot $BlockFamiliesMigrationRelativePath

    foreach ($RequiredMigration in @($Migration, $AllItemsMigration, $BlockFamiliesMigration)) {
        if (-not (Test-Path -LiteralPath $RequiredMigration -PathType Leaf)) {
            throw "Migration catalogue introuvable : $RequiredMigration"
        }
    }

    Write-Host ("Schema detecte : " + $(if ($LegacyCatalog) { 'legacy' } else { 'moderne' })) -ForegroundColor Cyan
    Write-Host "Application du catalogue global + separation fine des familles..." -ForegroundColor Cyan

    foreach ($MigrationToApply in @($Migration, $AllItemsMigration, $BlockFamiliesMigration)) {
        $MigrationProcess = Start-Process -FilePath $Mysql -ArgumentList @(
            "--host=$DatabaseHost",
            "--port=$DatabasePort",
            "--user=$DatabaseUser",
            "--database=$DatabaseName",
            '--default-character-set=utf8mb4'
        ) -RedirectStandardInput $MigrationToApply -NoNewWindow -Wait -PassThru
        if ($MigrationProcess.ExitCode -ne 0) {
            throw "Migration catalogue echouee : $(Split-Path -Leaf $MigrationToApply)"
        }
        Write-Host "Migration appliquee : $(Split-Path -Leaf $MigrationToApply)" -ForegroundColor Green
    }

    # Validation forte : absolument toutes les offres doivent maintenant etre
    # sous Catalogue ParadiseRP complet, y compris les anciennes pages staff/cachees.
    $AllItemsValidationSql = @"
SELECT CONCAT(
  (SELECT COUNT(*) FROM catalog_items), '|',
  (SELECT COUNT(*) FROM catalog_items WHERE page_id BETWEEN 9967201 AND 9967224 OR page_id BETWEEN 9968100 AND 9968199), '|',
  (SELECT COUNT(*) FROM catalog_items WHERE NOT (page_id BETWEEN 9967201 AND 9967224 OR page_id BETWEEN 9968100 AND 9968199))
)
"@
    $AllItemsCounts = ((& $Mysql @CommonArgs "--execute=$AllItemsValidationSql") -join '').Trim()
    if ($LASTEXITCODE -ne 0) { throw 'Validation totale du catalogue impossible.' }
    $CountParts = @($AllItemsCounts -split '\|')
    if ($CountParts.Count -ne 3 -or $CountParts[0] -ne $CountParts[1] -or $CountParts[2] -ne '0') {
        throw "Catalogue V4 incomplet : total|dans Paradise|hors Paradise = $AllItemsCounts"
    }
    Write-Host "Verification totale : $($CountParts[0]) offres sur $($CountParts[0]) sont dans Catalogue ParadiseRP complet, 0 hors catalogue." -ForegroundColor Green

    $BlockFamilyValidationSql = @"
SELECT CONCAT(
 (SELECT COUNT(*) FROM catalog_items WHERE page_id=9967201), '|',
 (SELECT COUNT(*) FROM catalog_items WHERE page_id BETWEEN 9968140 AND 9968152), '|',
 (SELECT COUNT(*) FROM catalog_pages WHERE parent_id=9967201 AND id BETWEEN 9968140 AND 9968152 AND visible='1' AND enabled='1')
)
"@
    $BlockFamilyCounts = ((& $Mysql @CommonArgs "--execute=$BlockFamilyValidationSql") -join '').Trim()
    if ($LASTEXITCODE -ne 0) { throw 'Validation des familles de blocs impossible.' }
    $BlockFamilyParts = @($BlockFamilyCounts -split '\|')
    if ($BlockFamilyParts.Count -ne 3 -or $BlockFamilyParts[0] -ne '0' -or $BlockFamilyParts[2] -ne '13') {
        throw "Separation des blocs incomplete : restants_dans_parent|classes|pages = $BlockFamilyCounts (attendu 0|N|13)."
    }
    Write-Host "Verification blocs : 0 offre melangee dans le parent, $($BlockFamilyParts[1]) offres reparties dans 13 familles." -ForegroundColor Green

    $ValidationMigration = Join-Path $RepositoryRoot 'migrations\20260906_paradise_catalogue_taxonomy_v4_validation.sql'
    if (Test-Path -LiteralPath $ValidationMigration -PathType Leaf) {
        $ValidationProcess = Start-Process -FilePath $Mysql -ArgumentList @(
            "--host=$DatabaseHost",
            "--port=$DatabasePort",
            "--user=$DatabaseUser",
            "--database=$DatabaseName",
            '--default-character-set=utf8mb4'
        ) -RedirectStandardInput $ValidationMigration -NoNewWindow -Wait -PassThru
        if ($ValidationProcess.ExitCode -ne 0) {
            throw 'Migration de validation V4 echouee.'
        }
    }

    $MetricsSql = "SELECT metric,metric_value FROM paradise_catalog_v4_validation ORDER BY metric"
    $Metrics = & $Mysql @CommonArgs "--execute=$MetricsSql"
    if ($LASTEXITCODE -ne 0) { throw 'Lecture des metriques V4 impossible.' }

    Write-Host ''
    Write-Host '=== Catalogue ParadiseRP V5 FIN ===' -ForegroundColor Green
    foreach ($Metric in $Metrics) {
        Write-Host $Metric
    }

    $TopPagesSql = @"
SELECT cp.id, cp.caption, COUNT(ci.id) AS offers
FROM catalog_pages cp
LEFT JOIN catalog_items ci ON ci.page_id=cp.id
WHERE (cp.id BETWEEN 9967201 AND 9967224 OR cp.id BETWEEN 9968100 AND 9968199)
  AND cp.visible='1' AND cp.enabled='1'
GROUP BY cp.id, cp.caption
ORDER BY offers DESC
LIMIT 25
"@
    Write-Host ''
    Write-Host 'Pages les plus chargees :' -ForegroundColor Cyan
    & $Mysql @CommonArgs "--execute=$TopPagesSql"
    if ($LASTEXITCODE -ne 0) { throw 'Lecture des pages catalogue impossible.' }

    Write-Host ''
    Write-Host 'Catalogue applique : tout est fusionne, puis les blocs sont separes par famille exacte.' -ForegroundColor Green
}
finally {
    $env:MYSQL_PWD = $null
}
