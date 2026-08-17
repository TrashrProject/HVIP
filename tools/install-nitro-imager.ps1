$ErrorActionPreference = 'Stop'

$Root = 'C:\HVIP'
$ServiceDir = Join-Path $Root 'services\nitro-imager'
$Assets = Join-Path $Root 'swf_pz\V5-0-2'
$Cache = Join-Path $Root 'cache\nitro-imager'

Write-Host '=== ParadiseRP - Installation Nitro Imager ===' -ForegroundColor Cyan

$required = @(
    (Join-Path $Assets 'gamedata\json\HabboAvatarActions.json'),
    (Join-Path $Assets 'gamedata\json\FigureData.json'),
    (Join-Path $Assets 'gamedata\json\FigureMap.json')
)
foreach ($file in $required) {
    if (!(Test-Path $file)) { throw "Fichier requis introuvable : $file" }
}

if (!(Get-Command git -ErrorAction SilentlyContinue)) { throw 'Git est introuvable dans le PATH.' }
if (!(Get-Command npm -ErrorAction SilentlyContinue)) { throw 'Node.js / npm est introuvable dans le PATH.' }
if (!(Get-Command node -ErrorAction SilentlyContinue)) { throw 'Node.js est introuvable dans le PATH.' }

New-Item -ItemType Directory -Force -Path (Split-Path $ServiceDir), $Cache | Out-Null

if (!(Test-Path (Join-Path $ServiceDir '.git'))) {
    if (Test-Path $ServiceDir) { Remove-Item -Recurse -Force $ServiceDir }
    git clone https://github.com/billsonnn/nitro-imager.git $ServiceDir
} else {
    Push-Location $ServiceDir
    try {
        git fetch origin
        git reset --hard origin/main
    } finally {
        Pop-Location
    }
}

$envText = @"
API_HOST=127.0.0.1
API_PORT=3030
AVATAR_SAVE_PATH=C:/HVIP/cache/nitro-imager
AVATAR_ACTIONS_URL=C:/HVIP/swf_pz/V5-0-2/gamedata/json/HabboAvatarActions.json
AVATAR_FIGUREDATA_URL=C:/HVIP/swf_pz/V5-0-2/gamedata/json/FigureData.json
AVATAR_FIGUREMAP_URL=C:/HVIP/swf_pz/V5-0-2/gamedata/json/FigureMap.json
AVATAR_EFFECTMAP_URL=C:/HVIP/swf_pz/V5-0-2/gamedata/json/EffectMap.json
AVATAR_ASSET_URL=C:/HVIP/swf_pz/V5-0-2/figure/%libname%.nitro
AVATAR_ASSET_EFFECT_URL=C:/HVIP/swf_pz/V5-0-2/effect/%libname%.nitro
"@
Set-Content -Path (Join-Path $ServiceDir '.env') -Value $envText -Encoding ASCII

Push-Location $ServiceDir
try {
    Write-Host '[1/3] Installation des dépendances...' -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -ne 0) { throw "npm install a échoué avec le code $LASTEXITCODE" }

    Write-Host '[2/3] Compilation...' -ForegroundColor Yellow
    npm run build
    if ($LASTEXITCODE -ne 0) { throw "npm run build a échoué avec le code $LASTEXITCODE" }
} finally {
    Pop-Location
}

$nodeExe = (Get-Command node).Source
$startCmd = Join-Path $Root 'tools\start-nitro-imager.cmd'
$cmdLines = @(
    '@echo off',
    'cd /d C:\HVIP\services\nitro-imager',
    ('"{0}" dist\index.js >> "C:\HVIP\cache\nitro-imager\service.log" 2>&1' -f $nodeExe)
)
Set-Content -Path $startCmd -Value ($cmdLines -join "`r`n") -Encoding ASCII

# Arrête uniquement le processus qui écoute déjà sur 3030.
try {
    $existing = Get-NetTCPConnection -LocalPort 3030 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($existing) {
        Stop-Process -Id $existing.OwningProcess -Force -ErrorAction SilentlyContinue
        Start-Sleep -Milliseconds 700
    }
} catch {}

Write-Host '[3/3] Démarrage du renderer local...' -ForegroundColor Yellow
Start-Process -FilePath $startCmd -WindowStyle Hidden
Start-Sleep -Seconds 4

$ok = $false
try {
    $test = Invoke-WebRequest -UseBasicParsing -TimeoutSec 12 -Uri 'http://127.0.0.1:3030/?figure=hd-180-1.hr-100-40.ch-210-66.lg-270-82.sh-290-80&size=n&direction=2&head_direction=2'
    $ok = $test.StatusCode -eq 200 -and $test.RawContentLength -gt 50
} catch {
    Write-Host $_.Exception.Message -ForegroundColor DarkYellow
}

if (!$ok) {
    Write-Host ''
    Write-Host 'Le service a été compilé mais le test HTTP a échoué.' -ForegroundColor Red
    Write-Host 'Regarde : C:\HVIP\cache\nitro-imager\service.log'
    exit 1
}

# Démarrage automatique à l'ouverture de session Windows.
try {
    $action = New-ScheduledTaskAction -Execute $startCmd
    $trigger = New-ScheduledTaskTrigger -AtLogOn
    Register-ScheduledTask -TaskName 'ParadiseRP Nitro Imager' -Action $action -Trigger $trigger -Description 'Renderer avatar Nitro local ParadiseRP' -Force | Out-Null
    Write-Host 'Démarrage automatique Windows : OK' -ForegroundColor Green
} catch {
    Write-Host 'Démarrage automatique non créé (non bloquant).' -ForegroundColor DarkYellow
}

Write-Host ''
Write-Host '=== Nitro Imager prêt sur 127.0.0.1:3030 ===' -ForegroundColor Green
Write-Host 'Les previews utilisent maintenant les mêmes .nitro que le client.' -ForegroundColor Green
