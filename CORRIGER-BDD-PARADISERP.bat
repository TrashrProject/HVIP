@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0deploy-vps\Apply-DatabaseMigrations.ps1"
pause
