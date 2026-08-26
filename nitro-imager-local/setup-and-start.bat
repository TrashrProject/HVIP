@echo off
setlocal EnableExtensions
cd /d "%~dp0"

rem ParadiseRP Nitro Imager uses canvas 2.8.0. That native dependency ships a
rem Windows x64 prebuilt binary for Node ABI v93 (Node 16), but not Node 24.
rem Keep the VPS global Node installation untouched and run this legacy,
rem loopback-only service with an isolated portable Node 16 runtime.
set "NODE_VERSION=16.20.2"
set "NODE_DIST=node-v%NODE_VERSION%-win-x64"
set "RUNTIME_DIR=%~dp0.runtime"
set "NODE_DIR=%RUNTIME_DIR%\%NODE_DIST%"
set "NODE_ZIP=%TEMP%\%NODE_DIST%.zip"
set "NODE_URL=https://nodejs.org/dist/v%NODE_VERSION%/%NODE_DIST%.zip"

if not exist "%NODE_DIR%\node.exe" (
  echo [RDP Imager] Installation du runtime Node %NODE_VERSION% isole...
  if not exist "%RUNTIME_DIR%" mkdir "%RUNTIME_DIR%"
  powershell -NoProfile -ExecutionPolicy Bypass -Command "$ErrorActionPreference='Stop'; Invoke-WebRequest -Uri '%NODE_URL%' -OutFile '%NODE_ZIP%'; Expand-Archive -Path '%NODE_ZIP%' -DestinationPath '%RUNTIME_DIR%' -Force; Remove-Item '%NODE_ZIP%' -Force" || goto :error
)

set "PATH=%NODE_DIR%;%PATH%"
for /f "delims=" %%V in ('"%NODE_DIR%\node.exe" -v') do set "ACTIVE_NODE=%%V"
echo [RDP Imager] Runtime: %ACTIVE_NODE% ^(isole - Node global non modifie^)

if not exist "nitro-imager\package.json" (
  echo [RDP Imager] Installation de nitro-imager...
  git clone --depth 1 https://github.com/billsonnn/nitro-imager.git nitro-imager || goto :error
)

cd /d "%~dp0nitro-imager"

echo [RDP Imager] Application du correctif de robustesse ParadiseRP...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0patch-nitro-imager.ps1" -Root "%~dp0nitro-imager" || goto :error

> .env echo API_HOST=127.0.0.1
>> .env echo API_PORT=3030
>> .env echo AVATAR_SAVE_PATH=C:/HVIP/nitro-imager-local/cache
rem Use the public HTTPS hostname for JSON gamedata. localhost is redirected to
rem HTTPS by the web stack, but its TLS certificate is only valid for
rem paradiserp.fr; Node correctly rejects https://localhost as a hostname
rem mismatch. The large .nitro assets remain local filesystem reads below.
>> .env echo AVATAR_ACTIONS_URL=https://paradiserp.fr/swf_pz/V5-0-2/gamedata/json/HabboAvatarActions.json
>> .env echo AVATAR_FIGUREDATA_URL=https://paradiserp.fr/swf_pz/V5-0-2/gamedata/json/FigureData.json
>> .env echo AVATAR_FIGUREMAP_URL=https://paradiserp.fr/swf_pz/V5-0-2/gamedata/json/FigureMap.json
>> .env echo AVATAR_EFFECTMAP_URL=https://paradiserp.fr/swf_pz/V5-0-2/gamedata/json/EffectMap.json
>> .env echo AVATAR_ASSET_URL=C:/xampp/htdocs/swf_pz/V5-0-2/figure/%%libname%%.nitro
>> .env echo AVATAR_ASSET_EFFECT_URL=C:/xampp/htdocs/swf_pz/V5-0-2/effect/%%libname%%.nitro

if not exist "..\cache" mkdir "..\cache"

rem A previous npm install under Node 24 leaves an unusable native canvas tree.
rem Verify the existing install with Node 16; if it cannot load canvas, rebuild
rem node_modules from scratch under the compatible ABI.
if exist "node_modules" (
  "%NODE_DIR%\node.exe" -e "require('canvas')" >nul 2>&1
  if errorlevel 1 (
    echo [RDP Imager] Nettoyage des dependances natives incompatibles...
    rmdir /s /q "node_modules"
  )
)

if not exist "node_modules" (
  echo [RDP Imager] npm install avec Node %NODE_VERSION%...
  call "%NODE_DIR%\npm.cmd" install || goto :error
)

"%NODE_DIR%\node.exe" -e "require('canvas')" >nul 2>&1 || (
  echo [RDP Imager] ERREUR: canvas ne se charge toujours pas avec Node %NODE_VERSION%.
  goto :error
)

echo [RDP Imager] Build...
call "%NODE_DIR%\npm.cmd" run build || goto :error

echo.
echo [RDP Imager] Assets locaux: C:\xampp\htdocs\swf_pz\V5-0-2
echo [RDP Imager] Gamedata HTTPS: https://paradiserp.fr/swf_pz/V5-0-2/gamedata/json
echo [RDP Imager] API avatar: http://127.0.0.1:3030/
echo [RDP Imager] Node global du VPS laisse intact.
echo.
"%NODE_DIR%\node.exe" ".\dist\index.js"
exit /b %ERRORLEVEL%

:error
echo.
echo [RDP Imager] ERREUR. Consulte les lignes ci-dessus; le Node global du VPS n'a pas ete modifie.
pause
exit /b 1
