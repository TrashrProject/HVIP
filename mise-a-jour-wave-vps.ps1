[CmdletBinding()]
param(
    [string]$RepositoryPath = "",
    [string]$ServiceName = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Configuration Wave / ParadiseRP.
$Branch = "quality/paradise-core-v1-fixes"
$JavaHome = "C:\Program Files\Android\openjdk\jdk-21.0.8"
$MavenRelativePath = "WavePlus\_tools\apache-maven-3.9.11\bin\mvn.cmd"
$MavenDownloadUrl = "https://archive.apache.org/dist/maven/maven-3/3.9.11/binaries/apache-maven-3.9.11-bin.zip"
$MavenSha512 = "03e2d65d4483a3396980629f260e25cac0d8b6f7f2791e4dc20bc83f9514db8d0f05b0479e699a5f34679250c49c8e52e961262ded468a20de0be254d8207076"
$BuiltJarRelativePath = "WavePlus\target\RPHabbo-3.5.4-jar-with-dependencies.jar"
$RuntimeJarRelativePath = "runtime\WavePlus\WaveRP-Arcturus.jar"
$BuiltPluginRelativePath = "WaveRP-Plugin\target\Roleplay-1.0.45-jar-with-dependencies.jar"
$RuntimePluginRelativePath = "runtime\WavePlus\plugins\WaveRP-Roleplay.jar"
$CommonMigrationRelativePaths = @(
    "migrations\20260902_paradise_weapon_skins.sql",
    "migrations\20260903_paradise_complete_inventory.sql"
)
$ModernCatalogMigrationRelativePaths = @(
    "migrations\20260903_paradise_catalogue_rp_v3.sql",
    "migrations\20260903_paradise_catalogue_extension_v4.sql",
    "migrations\20260903_paradise_catalogue_mass_habborpbr.sql",
    "migrations\20260904_paradise_catalogue_reorganize_v2.sql",
    "migrations\20260904_paradise_island_builder_kit.sql",
    "migrations\20260904_paradise_island_visibility_fix.sql",
    "migrations\20260904_paradise_black_cubes_force_visible.sql",
    "migrations\20260905_paradise_pure_black_block.sql",
    "migrations\20260906_paradise_catalogue_image_cleanup.sql"
)
$LegacyCatalogMigrationRelativePaths = @(
    "migrations\20260904_paradise_catalogue_mass_habborpbr_legacy.sql",
    "migrations\20260904_paradise_catalogue_reorganize_v2_legacy.sql",
    "migrations\20260904_paradise_island_builder_kit_legacy.sql",
    "migrations\20260904_paradise_island_visibility_fix_legacy.sql",
    "migrations\20260904_add_black_block_catalog.sql",
    "migrations\20260904_paradise_black_cubes_force_visible_legacy.sql",
    "migrations\20260905_paradise_pure_black_block_legacy.sql",
    "migrations\20260906_paradise_catalogue_image_cleanup_legacy.sql"
)
$Ports = @(30000, 30001, 2096)
$Timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$StoppedService = $false
$StoppedProcess = $false

if ([string]::IsNullOrWhiteSpace($RepositoryPath)) {
    $RepositoryPath = Split-Path -Parent $MyInvocation.MyCommand.Path
}

function Invoke-CheckedCommand {
    param(
        [Parameter(Mandatory = $true)][string]$Command,
        [Parameter(ValueFromRemainingArguments = $true)][string[]]$CommandArguments
    )

    & $Command @CommandArguments
    if ($LASTEXITCODE -ne 0) {
        throw "Commande echouee (code $LASTEXITCODE) : $Command $($CommandArguments -join ' ')"
    }
}

function Get-IniValue {
    param([string]$Path, [string]$Key, [string]$Default = "")
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $Default }
    $Line = Get-Content -LiteralPath $Path | Where-Object { $_ -match "^\s*$([regex]::Escape($Key))\s*=" } | Select-Object -Last 1
    if (-not $Line) { return $Default }
    return (($Line -split '=', 2)[1]).Trim()
}

try {
    Write-Host "=== Mise a jour Wave / ParadiseRP ===" -ForegroundColor Cyan

    if (-not (Test-Path -LiteralPath $RepositoryPath -PathType Container)) {
        throw "Depot introuvable : $RepositoryPath"
    }

    $RepositoryRoot = (Resolve-Path -LiteralPath $RepositoryPath).Path
    if (-not (Test-Path -LiteralPath (Join-Path $RepositoryRoot "DEVELOPPEMENT.md") -PathType Leaf)) {
        throw "DEVELOPPEMENT.md est absent de $RepositoryRoot."
    }

    $Git = (Get-Command git.exe -ErrorAction Stop).Source
    $Maven = Join-Path $RepositoryRoot $MavenRelativePath
    $BuiltJar = Join-Path $RepositoryRoot $BuiltJarRelativePath
    $RuntimeJar = Join-Path $RepositoryRoot $RuntimeJarRelativePath
    $BuiltPlugin = Join-Path $RepositoryRoot $BuiltPluginRelativePath
    $RuntimePlugin = Join-Path $RepositoryRoot $RuntimePluginRelativePath
    $Migrations = @($CommonMigrationRelativePaths | ForEach-Object { Join-Path $RepositoryRoot $_ })
    $RuntimeDirectory = Split-Path -Parent $RuntimeJar
    $JavaExecutable = Join-Path $JavaHome "bin\java.exe"

    if (-not (Test-Path -LiteralPath $JavaHome -PathType Container)) {
        throw "Java 21 est absent : $JavaHome"
    }
    if (-not (Test-Path -LiteralPath $Maven -PathType Leaf)) {
        $InstalledMaven = Get-Command mvn.cmd -ErrorAction SilentlyContinue
        if ($InstalledMaven) {
            $Maven = $InstalledMaven.Source
            Write-Host "Maven deja installe detecte : $Maven" -ForegroundColor Green
        }
        else {
            Write-Host "Maven 3.9.11 est absent. Telechargement officiel Apache..." -ForegroundColor Cyan
            $MavenToolsDirectory = Join-Path $RepositoryRoot "WavePlus\_tools"
            $MavenArchive = Join-Path ([System.IO.Path]::GetTempPath()) "apache-maven-3.9.11-bin.zip"
            New-Item -ItemType Directory -Path $MavenToolsDirectory -Force | Out-Null

            [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
            Invoke-WebRequest -Uri $MavenDownloadUrl -OutFile $MavenArchive -UseBasicParsing

            $DownloadedHash = (Get-FileHash -LiteralPath $MavenArchive -Algorithm SHA512).Hash.ToLowerInvariant()
            if ($DownloadedHash -ne $MavenSha512) {
                Remove-Item -LiteralPath $MavenArchive -Force
                throw "Le controle SHA-512 de Maven a echoue. Archive refusee."
            }

            Expand-Archive -LiteralPath $MavenArchive -DestinationPath $MavenToolsDirectory -Force
            Remove-Item -LiteralPath $MavenArchive -Force

            if (-not (Test-Path -LiteralPath $Maven -PathType Leaf)) {
                throw "Maven a ete telecharge mais mvn.cmd reste introuvable : $Maven"
            }
            Write-Host "Maven 3.9.11 installe dans WavePlus\_tools." -ForegroundColor Green
        }
    }
    if (-not (Test-Path -LiteralPath $RuntimeDirectory -PathType Container)) {
        throw "Runtime WavePlus absent : $RuntimeDirectory"
    }

    Push-Location $RepositoryRoot
    try {
        $TrackedChanges = (& $Git status --porcelain --untracked-files=no) -join "`n"
        if ($LASTEXITCODE -ne 0) {
            throw "Impossible de lire git status."
        }
        if ($TrackedChanges) {
            Write-Host $TrackedChanges -ForegroundColor Yellow
            throw "Des modifications locales sont presentes. Mise a jour annulee."
        }

        Invoke-CheckedCommand $Git switch $Branch
        Invoke-CheckedCommand $Git pull
        Invoke-CheckedCommand $Git lfs pull

        $CurrentCommit = (& $Git rev-parse --short HEAD).Trim()
        if ($LASTEXITCODE -ne 0) {
            throw "Impossible d'identifier le commit courant."
        }

        $env:JAVA_HOME = $JavaHome
        Set-Location (Join-Path $RepositoryRoot "WavePlus")
        Invoke-CheckedCommand $Maven -DskipTests clean install
        Set-Location (Join-Path $RepositoryRoot "WaveRP-Plugin")
        Invoke-CheckedCommand $Maven -DskipTests clean package
    }
    finally {
        Pop-Location
    }

    if (-not (Test-Path -LiteralPath $BuiltJar -PathType Leaf)) {
        throw "JAR compile introuvable : $BuiltJar"
    }
    if (-not (Test-Path -LiteralPath $BuiltPlugin -PathType Leaf)) {
        throw "JAR du plugin compile introuvable : $BuiltPlugin"
    }

    if ($Migrations | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf }) {
        $RuntimeConfig = Join-Path $RuntimeDirectory "config.ini"
        if (-not (Test-Path -LiteralPath $RuntimeConfig -PathType Leaf)) {
            throw "Configuration runtime introuvable : $RuntimeConfig"
        }
        $DatabaseHost = Get-IniValue $RuntimeConfig 'db.hostname' '127.0.0.1'
        $DatabasePort = Get-IniValue $RuntimeConfig 'db.port' '3306'
        $DatabaseName = Get-IniValue $RuntimeConfig 'db.database'
        if ([string]::IsNullOrWhiteSpace($DatabaseName)) { $DatabaseName = Get-IniValue $RuntimeConfig 'db.name' }
        if ([string]::IsNullOrWhiteSpace($DatabaseName)) { throw "db.database absent du fichier $RuntimeConfig" }
        $DatabaseUser = Get-IniValue $RuntimeConfig 'db.username' 'root'
        $DatabasePassword = Get-IniValue $RuntimeConfig 'db.password' ''
        $Mysql = @('C:\xampp\mysql\bin\mysql.exe','C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe') |
            Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
        if (-not $Mysql) {
            $MysqlCommand = Get-Command mysql.exe -ErrorAction SilentlyContinue
            if ($MysqlCommand) { $Mysql = $MysqlCommand.Source }
        }
        if (-not $Mysql) { throw 'mysql.exe introuvable pour appliquer les migrations SQL.' }
        try {
            $env:MYSQL_PWD = $DatabasePassword
            $CatalogSchemaArgs = @(
                "--host=$DatabaseHost", "--port=$DatabasePort", "--user=$DatabaseUser",
                "--database=$DatabaseName", '--batch', '--skip-column-names',
                "--execute=SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='catalog_items' AND column_name='item_ids'"
            )
            $LegacyCatalog = ((& $Mysql @CatalogSchemaArgs) -join '').Trim() -eq '1'
            if ($LASTEXITCODE -ne 0) { throw 'Detection du schema catalogue impossible.' }
            $CatalogMigrations = if ($LegacyCatalog) { $LegacyCatalogMigrationRelativePaths } else { $ModernCatalogMigrationRelativePaths }
            $Migrations = @($CommonMigrationRelativePaths + $CatalogMigrations | ForEach-Object { Join-Path $RepositoryRoot $_ })
            Write-Host ("Schema catalogue detecte : " + $(if ($LegacyCatalog) { 'legacy item_ids/vip_only' } else { 'moderne item_id/min_vip' })) -ForegroundColor Cyan
            foreach ($Migration in $Migrations) {
                if (-not (Test-Path -LiteralPath $Migration -PathType Leaf)) { continue }
                $MigrationProcess = Start-Process -FilePath $Mysql -ArgumentList @(
                    "--host=$DatabaseHost", "--port=$DatabasePort", "--user=$DatabaseUser",
                    "--database=$DatabaseName", '--default-character-set=utf8mb4'
                ) -RedirectStandardInput $Migration -NoNewWindow -Wait -PassThru
                if ($MigrationProcess.ExitCode -ne 0) {
                    throw "La migration SQL a echoue : $Migration"
                }
                Write-Host "Migration appliquee : $(Split-Path -Leaf $Migration)" -ForegroundColor Green
            }

            $CatalogItemColumn = if ($LegacyCatalog) { 'item_ids' } else { 'item_id' }
            $BlackCubeValidationSql = "SELECT COUNT(DISTINCT CAST(SUBSTRING_INDEX(SUBSTRING_INDEX($CatalogItemColumn, ',', 1), ':', 1) AS UNSIGNED)) FROM catalog_items WHERE page_id=9967201 AND CAST(SUBSTRING_INDEX(SUBSTRING_INDEX($CatalogItemColumn, ',', 1), ':', 1) AS UNSIGNED)=996700070"
            $BlackCubeValidationArgs = @(
                "--host=$DatabaseHost", "--port=$DatabasePort", "--user=$DatabaseUser",
                "--database=$DatabaseName", '--batch', '--skip-column-names',
                "--execute=$BlackCubeValidationSql"
            )
            $VisibleBlackCubeCount = ((& $Mysql @BlackCubeValidationArgs) -join '').Trim()
            if ($LASTEXITCODE -ne 0 -or $VisibleBlackCubeCount -ne '1') {
                throw "Verification catalogue impossible : le Bloc noir pur 996700070 est absent de la page 9967201."
            }
            Write-Host 'Verification catalogue : Bloc noir pur visible dans Construction - Blocs couleurs.' -ForegroundColor Green
        }
        finally {
            $env:MYSQL_PWD = $null
        }
    }

    $BackupDirectory = Join-Path $RuntimeDirectory "deploy-backups\$Timestamp"
    New-Item -ItemType Directory -Path $BackupDirectory -Force | Out-Null

    if ($ServiceName) {
        $Service = Get-Service -Name $ServiceName -ErrorAction Stop
        if ($Service.Status -ne "Stopped") {
            Stop-Service -Name $ServiceName -Force
            $Service.WaitForStatus("Stopped", [TimeSpan]::FromSeconds(60))
            $StoppedService = $true
        }
    }
    else {
        $WaveProcesses = @(
            Get-CimInstance Win32_Process |
                Where-Object {
                    ($_.Name -eq "java.exe" -or $_.Name -eq "javaw.exe") -and
                    $_.CommandLine -like "*WaveRP-Arcturus.jar*"
                }
        )
        if ($WaveProcesses.Count -gt 1) {
            throw "Plusieurs processus WaveRP ont ete trouves. Arrete-les manuellement."
        }
        if ($WaveProcesses.Count -eq 1) {
            $WaveProcessId = $WaveProcesses[0].ProcessId
            Write-Host "Arret automatique de WaveRP, processus $WaveProcessId..." -ForegroundColor Cyan
            Stop-Process -Id $WaveProcessId
            $StopLimit = (Get-Date).AddSeconds(30)
            while (Get-Process -Id $WaveProcessId -ErrorAction SilentlyContinue) {
                if ((Get-Date) -gt $StopLimit) {
                    throw "WaveRP ne s'est pas arrete apres 30 secondes."
                }
                Start-Sleep -Seconds 1
            }
            $StoppedProcess = $true
        }
    }

    if (Test-Path -LiteralPath $RuntimeJar -PathType Leaf) {
        Copy-Item -LiteralPath $RuntimeJar -Destination (Join-Path $BackupDirectory "WaveRP-Arcturus.jar") -Force
    }
    if (Test-Path -LiteralPath $RuntimePlugin -PathType Leaf) {
        Copy-Item -LiteralPath $RuntimePlugin -Destination (Join-Path $BackupDirectory "WaveRP-Roleplay.jar") -Force
    }
    Copy-Item -LiteralPath $BuiltJar -Destination $RuntimeJar -Force
    New-Item -ItemType Directory -Path (Split-Path -Parent $RuntimePlugin) -Force | Out-Null
    Copy-Item -LiteralPath $BuiltPlugin -Destination $RuntimePlugin -Force
    Write-Host "JAR deploye : $RuntimeJar" -ForegroundColor Green
    Write-Host "Plugin deploye : $RuntimePlugin" -ForegroundColor Green
    Write-Host "WebPixel\nitro a ete actualise par git pull." -ForegroundColor Green

    if ($ServiceName) {
        Start-Service -Name $ServiceName
        (Get-Service -Name $ServiceName).WaitForStatus("Running", [TimeSpan]::FromSeconds(60))
        $StoppedService = $false
        Start-Sleep -Seconds 5
    }
    else {
        $LogsDirectory = Join-Path $RuntimeDirectory "logs"
        New-Item -ItemType Directory -Path $LogsDirectory -Force | Out-Null
        $OutputLog = Join-Path $LogsDirectory "console-update.out.log"
        $ErrorLog = Join-Path $LogsDirectory "console-update.error.log"

        Write-Host "Redemarrage automatique de WaveRP..." -ForegroundColor Cyan
        $StartedProcess = Start-Process `
            -FilePath $JavaExecutable `
            -ArgumentList "-jar WaveRP-Arcturus.jar" `
            -WorkingDirectory $RuntimeDirectory `
            -WindowStyle Hidden `
            -RedirectStandardOutput $OutputLog `
            -RedirectStandardError $ErrorLog `
            -PassThru
        Start-Sleep -Seconds 10
        $StartedProcess.Refresh()
        if ($StartedProcess.HasExited) {
            Get-Content -LiteralPath $ErrorLog -Tail 50 -ErrorAction SilentlyContinue
            throw "WaveRP s'est arrete pendant son redemarrage."
        }
        $StoppedProcess = $false
        Write-Host "WaveRP redemarre, processus $($StartedProcess.Id)." -ForegroundColor Green
    }

    foreach ($Port in $Ports) {
        $IsOpen = Test-NetConnection -ComputerName "127.0.0.1" -Port $Port -InformationLevel Quiet -WarningAction SilentlyContinue
        if ($IsOpen) {
            Write-Host "Port $Port : OK" -ForegroundColor Green
        }
        else {
            Write-Warning "Port $Port ferme. Verifie les logs WavePlus."
        }
    }

    $LatestLog = Get-ChildItem -LiteralPath $RuntimeDirectory -Filter "*.log" -File -Recurse -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    if ($LatestLog) {
        Get-Content -LiteralPath $LatestLog.FullName -Tail 40
    }

    Write-Host "Mise a jour terminee sur $Branch, commit $CurrentCommit." -ForegroundColor Green
    Write-Host "Teste maintenant :commands dans le jeu." -ForegroundColor Yellow
}
catch {
    Write-Error ("{0}`nLigne d'origine : {1}`n{2}" -f $_.Exception.Message, $_.InvocationInfo.PositionMessage, $_.ScriptStackTrace)
    if ($ServiceName -and $StoppedService) {
        try {
            Start-Service -Name $ServiceName
        }
        catch {
            Write-Warning "Le service $ServiceName n'a pas pu etre redemarre."
        }
    }
    elseif ($StoppedProcess) {
        Write-Warning "Tentative de redemarrage de WaveRP apres l'erreur."
        try {
            Start-Process -FilePath $JavaExecutable -ArgumentList "-jar WaveRP-Arcturus.jar" -WorkingDirectory $RuntimeDirectory -WindowStyle Hidden
        }
        catch {
            Write-Warning "WaveRP n'a pas pu etre redemarre automatiquement."
        }
    }
    exit 1
}
