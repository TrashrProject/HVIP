$repoRootEms = 'C:\HVIP'
$runtimeEms = 'C:\HVIP\runtime\WavePlus'
$coreBuildEms = 'C:\HVIP\WavePlus\target\RPHabbo-3.5.4-jar-with-dependencies.jar'
$pluginBuildEms = 'C:\HVIP\WaveRP-Plugin\target\Roleplay-1.0.45-jar-with-dependencies.jar'
$coreRuntimeEms = 'C:\HVIP\runtime\WavePlus\WaveRP-Arcturus.jar'
$pluginRuntimeEms = 'C:\HVIP\runtime\WavePlus\plugins\WaveRP-Roleplay.jar'
$javaExeEms = Join-Path $env:JAVA_HOME 'bin\java.exe'

if (!(Test-Path -LiteralPath $javaExeEms)) {
    throw "Java introuvable : $javaExeEms"
}

if (!(Test-Path -LiteralPath $coreBuildEms)) {
    throw "JAR WavePlus introuvable. Compile WavePlus avant le déploiement."
}

if (!(Test-Path -LiteralPath $pluginBuildEms)) {
    throw "JAR WaveRP-Plugin introuvable. Compile le plugin avant le déploiement."
}

$waveProcessesEms = @(
    Get-CimInstance Win32_Process |
    Where-Object {
        ($_.Name -eq 'java.exe' -or $_.Name -eq 'javaw.exe') -and
        $_.CommandLine -like '*WaveRP-Arcturus.jar*'
    }
)

if ($waveProcessesEms.Count -gt 1) {
    throw "Plusieurs processus WaveRP ont été trouvés. Arrête-les manuellement."
}

if ($waveProcessesEms.Count -eq 1) {
    $processIdEms = $waveProcessesEms[0].ProcessId
    Write-Host "Arrêt de WaveRP, processus $processIdEms..."
    Stop-Process -Id $processIdEms

    $limitEms = (Get-Date).AddSeconds(30)

    while (Get-Process -Id $processIdEms -ErrorAction SilentlyContinue) {
        if ((Get-Date) -gt $limitEms) {
            throw "WaveRP ne s'est pas arrêté après 30 secondes."
        }

        Start-Sleep -Seconds 1
    }
}

$backupEms = Join-Path $repoRootEms "backups\ems-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
New-Item -ItemType Directory -Path $backupEms | Out-Null

if (Test-Path -LiteralPath $coreRuntimeEms) {
    Copy-Item -LiteralPath $coreRuntimeEms -Destination $backupEms
}

if (Test-Path -LiteralPath $pluginRuntimeEms) {
    Copy-Item -LiteralPath $pluginRuntimeEms -Destination $backupEms
}

New-Item -ItemType Directory -Path (Split-Path $pluginRuntimeEms) -Force | Out-Null

Copy-Item -LiteralPath $coreBuildEms -Destination $coreRuntimeEms -Force
Copy-Item -LiteralPath $pluginBuildEms -Destination $pluginRuntimeEms -Force

$logsEms = Join-Path $runtimeEms 'logs'
New-Item -ItemType Directory -Path $logsEms -Force | Out-Null

$outputEms = Join-Path $logsEms 'console-ems.out.log'
$errorEms = Join-Path $logsEms 'console-ems.error.log'

$startedEms = Start-Process `
    -FilePath $javaExeEms `
    -ArgumentList '-jar WaveRP-Arcturus.jar' `
    -WorkingDirectory $runtimeEms `
    -WindowStyle Hidden `
    -RedirectStandardOutput $outputEms `
    -RedirectStandardError $errorEms `
    -PassThru

Start-Sleep -Seconds 10
$startedEms.Refresh()

if ($startedEms.HasExited) {
    Get-Content -LiteralPath $errorEms -Tail 50 -ErrorAction SilentlyContinue
    throw "WaveRP s'est arrêté pendant son démarrage."
}

Write-Host "WaveRP redémarré avec succès. Processus : $($startedEms.Id)"
Write-Host "Sauvegarde : $backupEms"

Get-NetTCPConnection -State Listen -ErrorAction SilentlyContinue |
    Where-Object LocalPort -in 30000, 30001, 2096 |
    Select-Object LocalAddress, LocalPort, OwningProcess