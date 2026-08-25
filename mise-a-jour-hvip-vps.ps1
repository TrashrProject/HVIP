[CmdletBinding()]
param(
    [string]$RepositoryPath = "C:\HVIP",
    [string]$ServiceName = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Valeurs imposees par DEVELOPPEMENT.md.
$Branch = "quality/paradise-core-v1-fixes"
$JavaHome = "C:\Program Files\Android\openjdk\jdk-21.0.8"
$MavenRelativePath = "WavePlus\_tools\apache-maven-3.9.11\bin\mvn.cmd"
$MavenDownloadUrl = "https://archive.apache.org/dist/maven/maven-3/3.9.11/binaries/apache-maven-3.9.11-bin.zip"
$MavenSha512 = "03e2d65d4483a3396980629f260e25cac0d8b6f7f2791e4dc20bc83f9514db8d0f05b0479e699a5f34679250c49c8e52e961262ded468a20de0be254d8207076"
$BuiltJarRelativePath = "WavePlus\target\RPHabbo-3.5.4-jar-with-dependencies.jar"
$RuntimeJarRelativePath = "runtime\WavePlus\WaveRP-Arcturus.jar"
$Ports = @(30000, 30001, 2096)
$Timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$StoppedService = $false

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

try {
    Write-Host "=== Mise a jour HVIP ===" -ForegroundColor Cyan

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
    $RuntimeDirectory = Split-Path -Parent $RuntimeJar

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
        # Ne jamais pull automatiquement au-dessus de modifications locales suivies.
        $TrackedChanges = (& $Git status --porcelain --untracked-files=no) -join "`n"
        if ($LASTEXITCODE -ne 0) {
            throw "Impossible de lire git status."
        }
        if ($TrackedChanges) {
            Write-Host $TrackedChanges -ForegroundColor Yellow
            throw "Des modifications locales sont presentes. Mise a jour annulee."
        }

        # Commandes demandees dans DEVELOPPEMENT.md.
        Invoke-CheckedCommand $Git switch $Branch
        Invoke-CheckedCommand $Git pull

        $CurrentCommit = (& $Git rev-parse --short HEAD).Trim()
        if ($LASTEXITCODE -ne 0) {
            throw "Impossible d'identifier le commit courant."
        }

        $env:JAVA_HOME = $JavaHome
        Set-Location (Join-Path $RepositoryRoot "WavePlus")
        Invoke-CheckedCommand $Maven -DskipTests clean install
    }
    finally {
        Pop-Location
    }

    if (-not (Test-Path -LiteralPath $BuiltJar -PathType Leaf)) {
        throw "JAR compile introuvable : $BuiltJar"
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
        $Answer = Read-Host "Arrete WavePlus, puis saisis O pour deployer le JAR"
        if ($Answer -notin @("O", "o", "Oui", "oui")) {
            throw "Deploiement annule avant le remplacement du JAR."
        }
    }

    if (Test-Path -LiteralPath $RuntimeJar -PathType Leaf) {
        Copy-Item -LiteralPath $RuntimeJar -Destination (Join-Path $BackupDirectory "WaveRP-Arcturus.jar") -Force
    }
    Copy-Item -LiteralPath $BuiltJar -Destination $RuntimeJar -Force
    Write-Host "JAR deploye : $RuntimeJar" -ForegroundColor Green
    Write-Host "WebPixel\nitro a ete actualise par git pull." -ForegroundColor Green

    if ($ServiceName) {
        Start-Service -Name $ServiceName
        (Get-Service -Name $ServiceName).WaitForStatus("Running", [TimeSpan]::FromSeconds(60))
        $StoppedService = $false
        Start-Sleep -Seconds 5
    }
    else {
        Read-Host "Redemarre WavePlus, puis appuie sur Entree"
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
    Write-Error $_
    if ($ServiceName -and $StoppedService) {
        try {
            Start-Service -Name $ServiceName
        }
        catch {
            Write-Warning "Le service $ServiceName n'a pas pu etre redemarre."
        }
    }
    exit 1
}
