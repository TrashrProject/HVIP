@echo off
setlocal EnableExtensions EnableDelayedExpansion
chcp 65001 >nul
title ParadiseRP - Emulateur

set "RUNTIME_DIR=%~dp0runtime\WavePlus"
set "JAVA_EXE=C:\Program Files\Android\openjdk\jdk-21.0.8\bin\java.exe"
set "EMULATOR_JAR=%RUNTIME_DIR%\WaveRP-Arcturus.jar"
set "PID_FILE=%RUNTIME_DIR%\emulator.pid"
set "STDOUT_LOG=%RUNTIME_DIR%\emulator.stdout.log"
set "STDERR_LOG=%RUNTIME_DIR%\emulator.stderr.log"

if not exist "%JAVA_EXE%" (
    echo [ERREUR] Java 21 est introuvable :
    echo %JAVA_EXE%
    pause
    exit /b 1
)

if not exist "%EMULATOR_JAR%" (
    echo [ERREUR] Le fichier WaveRP-Arcturus.jar est introuvable :
    echo %EMULATOR_JAR%
    pause
    exit /b 1
)

:menu
call :detect_emulator
cls
echo ==================================================
echo             PARADISERP - EMULATEUR
echo ==================================================
if defined EMU_PID (
    echo  Statut : ALLUME ^(PID !EMU_PID!^)
) else (
    echo  Statut : ETEINT
)
echo.
echo  [1] Demarrer
echo  [2] Redemarrer
echo  [3] Arreter
echo  [4] Ouvrir les logs
echo  [0] Fermer ce menu
echo.
choice /C 12340 /N /M "Votre choix : "

if errorlevel 5 exit /b 0
if errorlevel 4 goto open_logs
if errorlevel 3 goto stop_emulator
if errorlevel 2 goto restart_emulator
if errorlevel 1 goto start_emulator
goto menu

:start_emulator
call :detect_emulator
if defined EMU_PID (
    echo.
    echo L'emulateur est deja allume ^(PID !EMU_PID!^).
    timeout /t 2 /nobreak >nul
    goto menu
)

echo.
echo Demarrage de WaveRP en arriere-plan...
powershell.exe -NoLogo -NoProfile -NonInteractive -Command "$p=Start-Process -FilePath '%JAVA_EXE%' -ArgumentList '-jar','WaveRP-Arcturus.jar' -WorkingDirectory '%RUNTIME_DIR%' -RedirectStandardOutput '%STDOUT_LOG%' -RedirectStandardError '%STDERR_LOG%' -WindowStyle Hidden -PassThru; Set-Content -LiteralPath '%PID_FILE%' -Value $p.Id -Encoding ascii"
if errorlevel 1 (
    echo [ERREUR] Le lancement a echoue. Consultez emulator.stderr.log.
    pause
    goto menu
)
echo Commande envoyee. WaveRP termine son chargement en arriere-plan.
timeout /t 1 /nobreak >nul
goto menu

:restart_emulator
call :detect_emulator
if defined EMU_PID (
    echo.
    echo Arret du processus !EMU_PID!...
    taskkill /PID !EMU_PID! /T /F >nul 2>&1
    call :wait_for_stop
)
if exist "%PID_FILE%" del /Q "%PID_FILE%" >nul 2>&1
goto start_emulator

:stop_emulator
call :detect_emulator
if not defined EMU_PID (
    echo.
    echo L'emulateur est deja eteint.
    timeout /t 2 /nobreak >nul
    goto menu
)
echo.
echo Arret du processus !EMU_PID!...
taskkill /PID !EMU_PID! /T /F >nul 2>&1
call :wait_for_stop
if exist "%PID_FILE%" del /Q "%PID_FILE%" >nul 2>&1
echo Emulateur arrete.
timeout /t 1 /nobreak >nul
goto menu

:open_logs
start "" notepad.exe "%STDOUT_LOG%"
goto menu

:detect_emulator
set "EMU_PID="
if exist "%PID_FILE%" set /p EMU_PID=<"%PID_FILE%"
if defined EMU_PID (
    tasklist /FI "PID eq !EMU_PID!" /NH 2>nul | findstr /C:"!EMU_PID!" >nul
    if errorlevel 1 set "EMU_PID="
)
if not defined EMU_PID (
    for /f "tokens=5" %%P in ('netstat -ano ^| findstr /R /C:":30000 .*LISTENING"') do if not defined EMU_PID set "EMU_PID=%%P"
)
if defined EMU_PID >"%PID_FILE%" echo !EMU_PID!
exit /b 0

:wait_for_stop
for /L %%I in (1,1,10) do (
    tasklist /FI "PID eq !EMU_PID!" /NH 2>nul | findstr /C:"!EMU_PID!" >nul
    if errorlevel 1 exit /b 0
    ping 127.0.0.1 -n 2 >nul
)
exit /b 0
