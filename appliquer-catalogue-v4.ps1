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
    $Migration = Join-Path $RepositoryRoot $MigrationRelativePath
    if (-not (Test-Path -LiteralPath $Migration -PathType Leaf)) {
        throw "Migration V4 introuvable : $Migration"
    }

    Write-Host ("Schema detecte : " + $(if ($LegacyCatalog) { 'legacy' } else { 'moderne' })) -ForegroundColor Cyan
    Write-Host "Application du catalogue V4 global..." -ForegroundColor Cyan

    $MigrationProcess = Start-Process -FilePath $Mysql -ArgumentList @(
        "--host=$DatabaseHost",
        "--port=$DatabasePort",
        "--user=$DatabaseUser",
        "--database=$DatabaseName",
        '--default-character-set=utf8mb4'
    ) -RedirectStandardInput $Migration -NoNewWindow -Wait -PassThru
    if ($MigrationProcess.ExitCode -ne 0) {
        throw "Migration V4 echouee : $MigrationRelativePath"
    }

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
    Write-Host '=== Catalogue ParadiseRP V4 ===' -ForegroundColor Green
    foreach ($Metric in $Metrics) {
        Write-Host $Metric
    }

    $TopPagesSql = @"
SELECT cp.id, cp.caption, COUNT(ci.id) AS offers
FROM catalog_pages cp
LEFT JOIN catalog_items ci ON ci.page_id=cp.id
WHERE cp.id BETWEEN 9967201 AND 9968199
  AND cp.visible='1' AND cp.enabled='1'
GROUP BY cp.id, cp.caption
ORDER BY offers DESC
LIMIT 15
"@
    Write-Host ''
    Write-Host 'Pages les plus chargees :' -ForegroundColor Cyan
    & $Mysql @CommonArgs "--execute=$TopPagesSql"
    if ($LASTEXITCODE -ne 0) { throw 'Lecture des pages catalogue impossible.' }

    Write-Host ''
    Write-Host 'Catalogue V4 applique. Redemarre WaveRP pour recharger le catalogue si necessaire.' -ForegroundColor Green
}
finally {
    $env:MYSQL_PWD = $null
}
