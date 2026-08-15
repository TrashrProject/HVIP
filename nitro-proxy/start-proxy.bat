@echo off
cd /d "%~dp0"
where node >nul 2>nul
if errorlevel 1 (
  echo [ERROR] Node.js is not installed or not in PATH.
  echo Install Node.js LTS, then run this file again.
  pause
  exit /b 1
)
if not exist node_modules\ws (
  echo [NitroProxy] Installing dependency...
  call npm install
  if errorlevel 1 pause & exit /b 1
)
echo [NitroProxy] Starting...
node server.js
pause
