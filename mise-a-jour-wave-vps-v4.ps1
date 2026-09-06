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
$RuntimeSettingsRelativePath = 'WebPixel/app/runtime-settings.json'
$RuntimeSettingsPath = Join-Path $RepositoryRoot 'WebPixel\app\runtime-settings.json'
$RuntimeSettingsBackup = Join-Path ([System.IO.Path]::GetTempPath()) ('paradise-runtime-settings-' + [guid]::NewGuid().ToString('N') + '.json')
$RuntimeSettingsWasLocal = $false

if (-not (Test-Path -LiteralPath $BaseUpdater -PathType Leaf)) {
    throw "Updater Wave introuvable : $BaseUpdater"
}
if (-not (Test-Path -LiteralPath $CatalogUpdater -PathType Leaf)) {
    throw "Updater catalogue V4 introuvable : $CatalogUpdater"
}

Write-Host '=== ParadiseRP : mise a jour Wave + Catalogue V4 ===' -ForegroundColor Cyan

# runtime-settings.json est un fichier de configuration locale du VPS (maintenance,
# titre/message, etc.). S'il a ete modifie localement, on le sauvegarde, on remet
# temporairement la version Git pour permettre le pull, puis on restaure exactement
# le contenu local apres la mise a jour. Les autres modifications suivies restent
# volontairement bloquantes dans l'updater principal.
$Git = (Get-Command git.exe -ErrorAction Stop).Source
Push-Location $RepositoryRoot
try {
    $RuntimeSettingsStatus = (& $Git status --porcelain --untracked-files=no -- $RuntimeSettingsRelativePath) -join "`n"
    if ($LASTEXITCODE -ne 0) {
        throw 'Impossible de verifier runtime-settings.json avec git status.'
    }

    if (-not [string]::IsNullOrWhiteSpace($RuntimeSettingsStatus)) {
        if (-not (Test-Path -LiteralPath $RuntimeSettingsPath -PathType Leaf)) {
            throw "runtime-settings.json est marque modifie mais le fichier est introuvable : $RuntimeSettingsPath"
        }

        [System.IO.File]::WriteAllBytes($RuntimeSettingsBackup, [System.IO.File]::ReadAllBytes($RuntimeSettingsPath))
        & $Git checkout -- $RuntimeSettingsRelativePath
        if ($LASTEXITCODE -ne 0) {
            throw 'Impossible de remettre temporairement runtime-settings.json a la version Git.'
        }
        $RuntimeSettingsWasLocal = $true
        Write-Host 'Reglages WebPixel locaux sauvegardes temporairement pour le deploiement.' -ForegroundColor Yellow
    }
}
finally {
    Pop-Location
}

# L'ancien updater V3 contient plusieurs controles historiques qui ne correspondent
# plus au fonctionnement de la taxonomie V4 globale. On execute une copie temporaire
# corrigee, sans modifier le script principal historique sur le VPS.
$BaseUpdaterContent = Get-Content -LiteralPath $BaseUpdater -Raw
$OldBlackCubeThrow = 'throw "Verification catalogue impossible : le Bloc noir pur 996700070 est absent de la page 9967201."'
$NewBlackCubeWarning = 'Write-Warning "Controle V3 ignore pour le Bloc noir pur 996700070 : la taxonomie V4 globale sera appliquee juste apres."'

if ($BaseUpdaterContent.Contains($OldBlackCubeThrow)) {
    $BaseUpdaterContent = $BaseUpdaterContent.Replace($OldBlackCubeThrow, $NewBlackCubeWarning)
}
else {
    Write-Warning 'Le controle historique du bloc noir n a pas ete trouve dans mise-a-jour-wave-vps.ps1. Execution sans patch specifique.'
}

# Les migrations catalogue peuvent creer plusieurs offres qui pointent vers un meme
# mobi. Les controles des lots officiels doivent donc verifier la presence des IDs
# de furniture distincts, pas exiger exactement une ligne catalog_items par mobi.
$BaseUpdaterContent = $BaseUpdaterContent.Replace(
    '(SELECT COUNT(*) FROM catalog_items WHERE $CatalogFurnitureIdSql BETWEEN 997100000 AND 997101056)',
    '(SELECT COUNT(DISTINCT $CatalogFurnitureIdSql) FROM catalog_items WHERE $CatalogFurnitureIdSql BETWEEN 997100000 AND 997101056)'
)
$BaseUpdaterContent = $BaseUpdaterContent.Replace(
    '(SELECT COUNT(*) FROM catalog_items WHERE $CatalogFurnitureIdSql BETWEEN 997200000 AND 997202944)',
    '(SELECT COUNT(DISTINCT $CatalogFurnitureIdSql) FROM catalog_items WHERE $CatalogFurnitureIdSql BETWEEN 997200000 AND 997202944)'
)

Set-Content -LiteralPath $PatchedBaseUpdater -Value $BaseUpdaterContent -Encoding UTF8

try {
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
}
finally {
    if ($RuntimeSettingsWasLocal -and (Test-Path -LiteralPath $RuntimeSettingsBackup -PathType Leaf)) {
        [System.IO.File]::WriteAllBytes($RuntimeSettingsPath, [System.IO.File]::ReadAllBytes($RuntimeSettingsBackup))
        Remove-Item -LiteralPath $RuntimeSettingsBackup -Force -ErrorAction SilentlyContinue
        Write-Host 'Reglages WebPixel locaux restaures.' -ForegroundColor Green
    }
    elseif (Test-Path -LiteralPath $RuntimeSettingsBackup -PathType Leaf) {
        Remove-Item -LiteralPath $RuntimeSettingsBackup -Force -ErrorAction SilentlyContinue
    }
}
