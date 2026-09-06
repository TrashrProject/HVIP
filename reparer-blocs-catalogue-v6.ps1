[CmdletBinding()]
param(
    [string]$RepositoryPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepositoryPath)) {
    $RepositoryPath = Split-Path -Parent $MyInvocation.MyCommand.Path
}

$RepositoryRoot = (Resolve-Path -LiteralPath $RepositoryPath).Path
$RuntimeDirectory = Join-Path $RepositoryRoot 'runtime\WavePlus'
$RuntimeConfig = Join-Path $RuntimeDirectory 'config.ini'
$JavaExecutable = 'C:\Program Files\Android\openjdk\jdk-21.0.8\bin\java.exe'

function Get-IniValue {
    param([string]$Path, [string]$Key, [string]$Default = '')
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
if ([string]::IsNullOrWhiteSpace($DatabaseName)) { throw 'Nom de base de donnees introuvable.' }

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

    $LegacySql = "SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='catalog_items' AND column_name='item_ids'"
    $Legacy = (((& $Mysql @CommonArgs "--execute=$LegacySql") -join '').Trim() -eq '1')
    if ($LASTEXITCODE -ne 0) { throw 'Detection du schema catalogue impossible.' }

    $MigrationRelative = if ($Legacy) {
        'migrations\20260906_paradise_catalogue_blocks_final_v6_legacy.sql'
    } else {
        'migrations\20260906_paradise_catalogue_blocks_final_v6.sql'
    }
    $Migration = Join-Path $RepositoryRoot $MigrationRelative
    if (-not (Test-Path -LiteralPath $Migration -PathType Leaf)) {
        throw "Migration finale des blocs introuvable : $Migration"
    }

    Write-Host '=== Reparation finale des blocs ParadiseRP ===' -ForegroundColor Cyan
    Write-Host ("Schema : " + $(if ($Legacy) { 'legacy item_ids' } else { 'moderne item_id' })) -ForegroundColor Cyan

    $Process = Start-Process -FilePath $Mysql -ArgumentList @(
        "--host=$DatabaseHost",
        "--port=$DatabasePort",
        "--user=$DatabaseUser",
        "--database=$DatabaseName",
        '--default-character-set=utf8mb4'
    ) -RedirectStandardInput $Migration -NoNewWindow -Wait -PassThru
    if ($Process.ExitCode -ne 0) { throw "Migration blocs echouee : $MigrationRelative" }

    $IdSql = if ($Legacy) {
        "CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_ids, ',', 1), ':', 1) AS UNSIGNED)"
    } else {
        "CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_id, ',', 1), ':', 1) AS UNSIGNED)"
    }

    $ValidationSql = @"
SELECT CONCAT(
 (SELECT COUNT(*) FROM catalog_items WHERE $IdSql=5480 AND page_id=9968141), '|',
 (SELECT COUNT(*) FROM catalog_items WHERE $IdSql=5466 AND page_id=9968150), '|',
 (SELECT COUNT(*) FROM catalog_items WHERE $IdSql=996661582 AND page_id=9968150), '|',
 (SELECT COUNT(*) FROM catalog_items WHERE $IdSql=996700070 AND page_id=9968140)
)
"@
    $KnownCounts = ((& $Mysql @CommonArgs "--execute=$ValidationSql") -join '').Trim()
    if ($LASTEXITCODE -ne 0) { throw 'Validation des blocs critiques impossible.' }
    $Parts = @($KnownCounts -split '\|')
    if ($Parts.Count -ne 4 -or ($Parts | Where-Object { [int]$_ -lt 1 }).Count -gt 0) {
        throw "Blocs critiques mal classes : Large5480|Small5466|Small996661582|Noir996700070 = $KnownCounts"
    }

    $FamilySql = @"
SELECT cp.id,cp.caption,COUNT(ci.id)
FROM catalog_pages cp
LEFT JOIN catalog_items ci ON ci.page_id=cp.id
WHERE cp.id BETWEEN 9968140 AND 9968152
GROUP BY cp.id,cp.caption
ORDER BY cp.id
"@
    Write-Host 'Blocs critiques verifies :' -ForegroundColor Green
    Write-Host '  5480 -> Large Blocks' -ForegroundColor Green
    Write-Host '  5466 -> Small Blocks' -ForegroundColor Green
    Write-Host '  996661582 -> Small Blocks' -ForegroundColor Green
    Write-Host '  996700070 -> Blocs de couleur' -ForegroundColor Green
    Write-Host ''
    Write-Host 'Repartition actuelle des familles :' -ForegroundColor Cyan
    & $Mysql @CommonArgs "--execute=$FamilySql"
    if ($LASTEXITCODE -ne 0) { throw 'Lecture des familles de blocs impossible.' }
}
finally {
    $env:MYSQL_PWD = $null
}

# Le catalogue est charge en memoire par WaveRP : redemarrage cible apres reparation.
$WaveProcesses = @(
    Get-CimInstance Win32_Process |
        Where-Object {
            ($_.Name -eq 'java.exe' -or $_.Name -eq 'javaw.exe') -and
            $_.CommandLine -like '*WaveRP-Arcturus.jar*'
        }
)
if ($WaveProcesses.Count -gt 1) {
    throw 'Plusieurs processus WaveRP detectes. Arrete-les manuellement puis relance le script.'
}
if ($WaveProcesses.Count -eq 1) {
    $PidToStop = $WaveProcesses[0].ProcessId
    Stop-Process -Id $PidToStop -Force
    $Limit = (Get-Date).AddSeconds(30)
    while (Get-Process -Id $PidToStop -ErrorAction SilentlyContinue) {
        if ((Get-Date) -gt $Limit) { throw "WaveRP ne s'est pas arrete apres 30 secondes." }
        Start-Sleep -Seconds 1
    }
}

if (-not (Test-Path -LiteralPath $JavaExecutable -PathType Leaf)) {
    throw "Java 21 introuvable : $JavaExecutable"
}
$LogsDirectory = Join-Path $RuntimeDirectory 'logs'
New-Item -ItemType Directory -Path $LogsDirectory -Force | Out-Null
$OutLog = Join-Path $LogsDirectory 'console-blocks-v6.out.log'
$ErrLog = Join-Path $LogsDirectory 'console-blocks-v6.error.log'
$Wave = Start-Process -FilePath $JavaExecutable -ArgumentList '-jar WaveRP-Arcturus.jar' -WorkingDirectory $RuntimeDirectory -WindowStyle Hidden -RedirectStandardOutput $OutLog -RedirectStandardError $ErrLog -PassThru
Start-Sleep -Seconds 10
$Wave.Refresh()
if ($Wave.HasExited) {
    Get-Content -LiteralPath $ErrLog -Tail 80 -ErrorAction SilentlyContinue
    throw "WaveRP s'est arrete pendant le redemarrage."
}
Write-Host "WaveRP redemarre. PID $($Wave.Id)." -ForegroundColor Green
Write-Host 'Reparation blocs V6 terminee.' -ForegroundColor Green
