[CmdletBinding()]
param(
    [string]$RepositoryPath = "",
    [string]$ServiceName = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepositoryPath)) {
    $RepositoryPath = Split-Path -Parent $MyInvocation.MyCommand.Path
}

$RepositoryRoot = (Resolve-Path -LiteralPath $RepositoryPath).Path
$BaseUpdater = Join-Path $RepositoryRoot 'mise-a-jour-wave-vps.ps1'
$CatalogUpdater = Join-Path $RepositoryRoot 'appliquer-catalogue-v4.ps1'
$RuntimeDirectory = Join-Path $RepositoryRoot 'runtime\WavePlus'
$JavaExecutable = 'C:\Program Files\Android\openjdk\jdk-21.0.8\bin\java.exe'

if (-not (Test-Path -LiteralPath $BaseUpdater -PathType Leaf)) {
    throw "Updater Wave introuvable : $BaseUpdater"
}
if (-not (Test-Path -LiteralPath $CatalogUpdater -PathType Leaf)) {
    throw "Updater catalogue V4 introuvable : $CatalogUpdater"
}

Write-Host '=== ParadiseRP : mise a jour Wave + Catalogue V4 ===' -ForegroundColor Cyan

# La mise a jour principale conserve toutes les verifications build/plugin/catalogue
# historiques. La V4 est appliquee ensuite afin d'etre toujours la derniere
# taxonomie executee, donc les migrations V2/V3 ne peuvent pas reprendre la main.
& $BaseUpdater -RepositoryPath $RepositoryRoot -ServiceName $ServiceName
if ($LASTEXITCODE -ne 0) {
    throw "La mise a jour Wave principale a echoue avec le code $LASTEXITCODE."
}

& $CatalogUpdater -RepositoryPath $RepositoryRoot
if ($LASTEXITCODE -ne 0) {
    throw "L'application du catalogue V4 a echoue avec le code $LASTEXITCODE."
}

# Le catalogue est charge en memoire par l'emulateur. On effectue donc un dernier
# redemarrage propre apres la taxonomie V4.
if (-not [string]::IsNullOrWhiteSpace($ServiceName)) {
    Write-Host "Redemarrage du service $ServiceName pour charger le catalogue V4..." -ForegroundColor Cyan
    Restart-Service -Name $ServiceName -Force
    (Get-Service -Name $ServiceName).WaitForStatus('Running', [TimeSpan]::FromSeconds(60))
}
else {
    $WaveProcesses = @(
        Get-CimInstance Win32_Process |
            Where-Object {
                ($_.Name -eq 'java.exe' -or $_.Name -eq 'javaw.exe') -and
                $_.CommandLine -like '*WaveRP-Arcturus.jar*'
            }
    )

    if ($WaveProcesses.Count -gt 1) {
        throw 'Plusieurs processus WaveRP sont actifs. Impossible de choisir lequel redemarrer.'
    }
    if ($WaveProcesses.Count -eq 1) {
        Stop-Process -Id $WaveProcesses[0].ProcessId -Force
        $Limit = (Get-Date).AddSeconds(30)
        while (Get-Process -Id $WaveProcesses[0].ProcessId -ErrorAction SilentlyContinue) {
            if ((Get-Date) -gt $Limit) {
                throw "WaveRP ne s'est pas arrete apres 30 secondes."
            }
            Start-Sleep -Seconds 1
        }
    }

    if (-not (Test-Path -LiteralPath $JavaExecutable -PathType Leaf)) {
        throw "Java 21 introuvable : $JavaExecutable"
    }

    $LogsDirectory = Join-Path $RuntimeDirectory 'logs'
    New-Item -ItemType Directory -Path $LogsDirectory -Force | Out-Null
    $OutputLog = Join-Path $LogsDirectory 'console-catalog-v4.out.log'
    $ErrorLog = Join-Path $LogsDirectory 'console-catalog-v4.error.log'

    $Process = Start-Process `
        -FilePath $JavaExecutable `
        -ArgumentList '-jar WaveRP-Arcturus.jar' `
        -WorkingDirectory $RuntimeDirectory `
        -WindowStyle Hidden `
        -RedirectStandardOutput $OutputLog `
        -RedirectStandardError $ErrorLog `
        -PassThru

    Start-Sleep -Seconds 10
    $Process.Refresh()
    if ($Process.HasExited) {
        Get-Content -LiteralPath $ErrorLog -Tail 80 -ErrorAction SilentlyContinue
        throw "WaveRP s'est arrete pendant le redemarrage V4."
    }
    Write-Host "WaveRP redemarre avec le catalogue V4, processus $($Process.Id)." -ForegroundColor Green
}

Write-Host ''
Write-Host 'ParadiseRP est a jour et le catalogue V4 global est actif.' -ForegroundColor Green
Write-Host 'Controle en jeu : ouvre le catalogue et verifie notamment Blocs, Eau/Puits, Fontaines, Piscines, Murs et Sols.' -ForegroundColor Yellow
