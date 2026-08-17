$ErrorActionPreference = 'Stop'

$Root = 'C:\HVIP'
$ServiceDir = Join-Path $Root 'services\nitro-imager'
$Assets = Join-Path $Root 'swf_pz\V5-0-2'
$Cache = Join-Path $Root 'cache\nitro-imager'
$RuntimeRoot = Join-Path $Root 'runtime'
$NodeVersion = '16.20.2'
$NodeFolderName = "node-v$NodeVersion-win-x64"
$NodeZip = Join-Path $RuntimeRoot "$NodeFolderName.zip"
$NodeDir = Join-Path $RuntimeRoot $NodeFolderName
$NodeExe = Join-Path $NodeDir 'node.exe'
$NpmCli = Join-Path $NodeDir 'node_modules\npm\bin\npm-cli.js'

Write-Host '=== ParadiseRP - Installation Nitro Imager ===' -ForegroundColor Cyan
Write-Host 'Runtime dédié : Node.js 16.20.2 (compatibilité canvas 2.8)' -ForegroundColor DarkCyan

$required = @(
    (Join-Path $Assets 'gamedata\json\HabboAvatarActions.json'),
    (Join-Path $Assets 'gamedata\json\FigureData.json'),
    (Join-Path $Assets 'gamedata\json\FigureMap.json')
)
foreach ($file in $required) {
    if (!(Test-Path $file)) { throw "Fichier requis introuvable : $file" }
}

if (!(Get-Command git -ErrorAction SilentlyContinue)) { throw 'Git est introuvable dans le PATH.' }

New-Item -ItemType Directory -Force -Path (Split-Path $ServiceDir), $Cache, $RuntimeRoot | Out-Null

# Nitro Imager utilise canvas 2.8.0. Node 24 force une compilation native qui échoue.
# On installe un Node 16 portable uniquement pour ce service, sans modifier Node global du VPS.
if (!(Test-Path $NodeExe) -or !(Test-Path $NpmCli)) {
    Write-Host '[0/4] Installation du runtime Node 16 portable...' -ForegroundColor Yellow
    if (Test-Path $NodeDir) { Remove-Item -Recurse -Force $NodeDir }
    if (Test-Path $NodeZip) { Remove-Item -Force $NodeZip }

    $nodeUrl = "https://nodejs.org/dist/v$NodeVersion/$NodeFolderName.zip"
    Write-Host "Téléchargement : $nodeUrl" -ForegroundColor DarkGray
    Invoke-WebRequest -UseBasicParsing -Uri $nodeUrl -OutFile $NodeZip
    Expand-Archive -Path $NodeZip -DestinationPath $RuntimeRoot -Force
    Remove-Item -Force $NodeZip
}

if (!(Test-Path $NodeExe)) { throw "Node portable introuvable : $NodeExe" }
if (!(Test-Path $NpmCli)) { throw "npm portable introuvable : $NpmCli" }

$runtimeVersion = (& $NodeExe --version).Trim()
Write-Host "Node utilisé par Nitro Imager : $runtimeVersion" -ForegroundColor Green

if (!(Test-Path (Join-Path $ServiceDir '.git'))) {
    if (Test-Path $ServiceDir) { Remove-Item -Recurse -Force $ServiceDir }
    git clone https://github.com/billsonnn/nitro-imager.git $ServiceDir
    if ($LASTEXITCODE -ne 0) { throw "git clone a échoué avec le code $LASTEXITCODE" }
} else {
    Push-Location $ServiceDir
    try {
        git fetch origin
        if ($LASTEXITCODE -ne 0) { throw "git fetch a échoué avec le code $LASTEXITCODE" }
        git reset --hard origin/main
        if ($LASTEXITCODE -ne 0) { throw "git reset a échoué avec le code $LASTEXITCODE" }
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
    # Supprime l'installation partielle faite précédemment avec Node 24.
    if (Test-Path 'node_modules') {
        Write-Host 'Nettoyage de node_modules incompatible...' -ForegroundColor DarkYellow
        Remove-Item -Recurse -Force 'node_modules'
    }

    Write-Host '[1/4] Installation des dépendances avec Node 16...' -ForegroundColor Yellow
    & $NodeExe $NpmCli install --no-audit --no-fund
    if ($LASTEXITCODE -ne 0) { throw "npm install a échoué avec le code $LASTEXITCODE" }

    Write-Host '[2/4] Compilation...' -ForegroundColor Yellow
    & $NodeExe $NpmCli run build
    if ($LASTEXITCODE -ne 0) { throw "npm run build a échoué avec le code $LASTEXITCODE" }
} finally {
    Pop-Location
}

$startCmd = Join-Path $Root 'tools\start-nitro-imager.cmd'
$cmdLines = @(
    '@echo off',
    'cd /d C:\HVIP\services\nitro-imager',
    ('"{0}" dist\index.js >> "C:\HVIP\cache\nitro-imager\service.log" 2>&1' -f $NodeExe)
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

# Vide l'ancien log pour que le diagnostic soit lisible.
$logFile = Join-Path $Cache 'service.log'
Set-Content -Path $logFile -Value '' -Encoding ASCII

Write-Host '[3/4] Démarrage du renderer local...' -ForegroundColor Yellow
Start-Process -FilePath $startCmd -WindowStyle Hidden
Start-Sleep -Seconds 5

Write-Host '[4/4] Test du rendu avatar...' -ForegroundColor Yellow
$ok = $false
try {
    $test = Invoke-WebRequest -UseBasicParsing -TimeoutSec 20 -Uri 'http://127.0.0.1:3030/?figure=hd-180-1.hr-100-40.ch-210-66.lg-270-82.sh-290-80&size=n&direction=2&head_direction=2'
    $contentType = [string]$test.Headers['Content-Type']
    $ok = $test.StatusCode -eq 200 -and $test.RawContentLength -gt 50 -and $contentType -match 'image'
} catch {
    Write-Host $_.Exception.Message -ForegroundColor DarkYellow
}

if (!$ok) {
    Write-Host ''
    Write-Host 'Le service ne répond pas encore correctement.' -ForegroundColor Red
    Write-Host 'Dernières lignes du log :' -ForegroundColor Yellow
    if (Test-Path $logFile) { Get-Content $logFile -Tail 35 }
    Write-Host ''
    Write-Host 'Log complet : C:\HVIP\cache\nitro-imager\service.log'
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
