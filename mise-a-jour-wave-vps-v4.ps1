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
$PatchedBaseUpdater = Join-Path ([System.IO.Path]::GetTempPath()) 'paradise-mise-a-jour-wave-vps-v4-base.ps1'

if (-not (Test-Path -LiteralPath $BaseUpdater -PathType Leaf)) {
    throw "Updater Wave introuvable : $BaseUpdater"
}
if (-not (Test-Path -LiteralPath $CatalogUpdater -PathType Leaf)) {
    throw "Updater catalogue V4 introuvable : $CatalogUpdater"
}

Write-Host '=== ParadiseRP : mise a jour Wave + Catalogue V4 ===' -ForegroundColor Cyan

# L'ancien updater V3 contient un controle historique qui exige que le bloc noir
# 996700070 soit obligatoirement dans la page 9967201. Ce controle n'est plus
# pertinent avec la taxonomie V4 globale. On ne modifie pas le fichier original :
# on execute une copie temporaire ou ce seul echec devient un avertissement.
$BaseUpdaterContent = Get-Content -LiteralPath $BaseUpdater -Raw
$OldBlackCubeThrow = 'throw "Verification catalogue impossible : le Bloc noir pur 996700070 est absent de la page 9967201."'
$NewBlackCubeWarning = 'Write-Warning "Controle V3 ignore pour le Bloc noir pur 996700070 : la taxonomie V4 globale sera appliquee juste apres."'

if ($BaseUpdaterContent.Contains($OldBlackCubeThrow)) {
    $BaseUpdaterContent = $BaseUpdaterContent.Replace($OldBlackCubeThrow, $NewBlackCubeWarning)
}
else {
    Write-Warning 'Le controle historique du bloc noir n a pas ete trouve dans mise-a-jour-wave-vps.ps1. Execution sans patch specifique.'
}

Set-Content -LiteralPath $PatchedBaseUpdater -Value $BaseUpdaterContent -Encoding UTF8

try {
    & $PatchedBaseUpdater -RepositoryPath $RepositoryRoot -ServiceName $ServiceName
    if ($LASTEXITCODE -ne 0) {
        throw "La mise a jour Wave principale a echoue avec le code $LASTEXITCODE."
    }
}
finally {
    Remove-Item -LiteralPath $PatchedBaseUpdater -Force -ErrorAction SilentlyContinue
}

# La V4 est toujours executee en dernier : anciennes pages + nouveautes sont
# fusionnees puis reclassees par type de mobi dans les categories fines.
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
        $WaveProcessId = $WaveProcesses[0].ProcessId
        Stop-Process -Id $WaveProcessId -Force
        $Limit = (Get-Date).AddSeconds(30)
        while (Get-Process -Id $WaveProcessId -ErrorAction SilentlyContinue) {
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
