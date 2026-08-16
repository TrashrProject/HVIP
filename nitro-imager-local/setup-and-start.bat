@echo off
setlocal
cd /d %~dp0

if not exist "nitro-imager\package.json" (
  echo [RDP Imager] Installation de nitro-imager...
  git clone --depth 1 https://github.com/billsonnn/nitro-imager.git nitro-imager || goto :error
)

cd /d "%~dp0nitro-imager"

> .env echo API_HOST=127.0.0.1
>> .env echo API_PORT=3030
>> .env echo AVATAR_SAVE_PATH=C:/xampp/htdocs/nitro-imager-local/cache
>> .env echo AVATAR_ACTIONS_URL=http://localhost/swf_pz/V5-0-2/gamedata/json/HabboAvatarActions.json
>> .env echo AVATAR_FIGUREDATA_URL=http://localhost/swf_pz/V5-0-2/gamedata/json/FigureData.json
>> .env echo AVATAR_FIGUREMAP_URL=http://localhost/swf_pz/V5-0-2/gamedata/json/FigureMap.json
>> .env echo AVATAR_EFFECTMAP_URL=http://localhost/swf_pz/V5-0-2/gamedata/json/EffectMap.json
>> .env echo AVATAR_ASSET_URL=C:/xampp/htdocs/swf_pz/V5-0-2/figure/%%libname%%.nitro
>> .env echo AVATAR_ASSET_EFFECT_URL=C:/xampp/htdocs/swf_pz/V5-0-2/effect/%%libname%%.nitro

if not exist "..\cache" mkdir "..\cache"

if not exist "node_modules" (
  echo [RDP Imager] npm install...
  call npm install || goto :error
)

if not exist "dist" (
  echo [RDP Imager] build...
  call npm run build || goto :error
)

echo.
echo [RDP Imager] Assets locaux: C:\xampp\htdocs\swf_pz\V5-0-2
echo [RDP Imager] API avatar: http://127.0.0.1:3030/
echo.
call npm start
exit /b 0

:error
echo.
echo [RDP Imager] ERREUR. Verifie que Git et Node.js/npm sont installes.
pause
exit /b 1
