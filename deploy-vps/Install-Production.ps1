$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$source=Join-Path $root 'WavePlus'
$sql=Join-Path $source 'waveplus.sql'
$migrations=Join-Path $source 'database\production-migrations.sql'
$weaponSkinsMigration=Join-Path $root 'migrations\20260902_paradise_weapon_skins.sql'
$out=Join-Path $root 'runtime\WavePlus'
Write-Host 'Installation WavePlus + base ParadiseRP' -ForegroundColor Cyan
$mysql=@('C:\xampp\mysql\bin\mysql.exe','C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe')|Where-Object{Test-Path $_}|Select-Object -First 1
if(!$mysql){$c=Get-Command mysql.exe -ErrorAction SilentlyContinue;if($c){$mysql=$c.Source}}
if(!$mysql){throw 'mysql.exe introuvable'}
$h=Read-Host 'Hote MySQL [127.0.0.1]';if(!$h){$h='127.0.0.1'}
$p=Read-Host 'Port [3306]';if(!$p){$p='3306'}
$n=Read-Host 'Base [waveplus]';if(!$n){$n='waveplus'}
$u=Read-Host 'Utilisateur [root]';if(!$u){$u='root'}
$s=Read-Host 'Mot de passe MySQL' -AsSecureString
$ptr=[Runtime.InteropServices.Marshal]::SecureStringToBSTR($s)
$pw=[Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr)
try{
 $env:MYSQL_PWD=$pw
 & $mysql --host=$h --port=$p --user=$u --execute="CREATE DATABASE IF NOT EXISTS ``$n`` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
 if($LASTEXITCODE){throw 'Creation base impossible'}
 $imp=Start-Process $mysql -ArgumentList @("--host=$h","--port=$p","--user=$u","--database=$n",'--default-character-set=utf8mb4') -RedirectStandardInput $sql -NoNewWindow -Wait -PassThru
 if($imp.ExitCode){throw 'Import SQL echoue'}
 if(Test-Path $migrations){
  $mig=Start-Process $mysql -ArgumentList @("--host=$h","--port=$p","--user=$u","--database=$n",'--default-character-set=utf8mb4') -RedirectStandardInput $migrations -NoNewWindow -Wait -PassThru
  if($mig.ExitCode){throw 'Migration SQL echouee'}
 }
 if(Test-Path $weaponSkinsMigration){
  $skinMig=Start-Process $mysql -ArgumentList @("--host=$h","--port=$p","--user=$u","--database=$n",'--default-character-set=utf8mb4') -RedirectStandardInput $weaponSkinsMigration -NoNewWindow -Wait -PassThru
  if($skinMig.ExitCode){throw 'Migration des skins echouee'}
 }
 $cp=Join-Path $source 'Config\config.ini';$cfg=[IO.File]::ReadAllText($cp)
 $rconPort=30001
 while(Get-NetTCPConnection -State Listen -LocalPort $rconPort -ErrorAction SilentlyContinue){$rconPort++}
 $v=@{'db.hostname'=$h;'db.port'=$p;'db.username'=$u;'db.password'=$pw.Replace("`r",'').Replace("`n",'');'rcon.tcp.port'=[string]$rconPort;'db.name'=$n}
 foreach($k in $v.Keys){$cfg=[regex]::Replace($cfg,"(?m)^$([regex]::Escape($k))=.*$","$k=$($v[$k])")}
 $cfg=[regex]::Replace($cfg,'(?m)^group\.badge\.url=.*$','group.badge.url=https://paradiserp.fr/swf_pz/V5-0-2/c_images/Badgeparts/%badge%.png')
 [IO.File]::WriteAllText($cp,$cfg)
 $runningWavePlus=Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.Path -and $_.Path.StartsWith($out,[StringComparison]::OrdinalIgnoreCase) }
 if($runningWavePlus){
  Write-Host 'Arret de WavePlus avant mise a jour...' -ForegroundColor Yellow
  $runningWavePlus | Stop-Process -Force
  Start-Sleep -Seconds 2
 }
 New-Item -ItemType Directory -Path $out -Force|Out-Null
 & dotnet publish (Join-Path $source 'Plus.csproj') -c Release -r win-x64 --self-contained true -o $out
 if($LASTEXITCODE){throw 'Compilation WavePlus echouee'}
 $runtimeConfigDir=Join-Path $out 'Config'
 New-Item -ItemType Directory -Path $runtimeConfigDir -Force|Out-Null
 [IO.File]::WriteAllText((Join-Path $runtimeConfigDir 'config.ini'),$cfg)
 $cmsConfigPath=Join-Path $root 'WebPixel\app\Controller\Config.class.php'
 if(Test-Path $cmsConfigPath){
  $cms=[IO.File]::ReadAllText($cmsConfigPath)
  $phpValues=@{
   'DBHOST'=$h
   'DBName'=$n
   'DBUser'=$u
   'DBPass'=$pw
  }
  foreach($property in $phpValues.Keys){
   $escaped=[string]$phpValues[$property]
   $escaped=$escaped.Replace('\','\\').Replace("'","\'").Replace("`r",'').Replace("`n",'')
   $replacement="    protected static `$$property = '$escaped';"
   $pattern="(?m)^\s*protected static \`$$property\s*=\s*.*?;\s*$"
   $cms=[regex]::Replace($cms,$pattern,[System.Text.RegularExpressions.MatchEvaluator]{param($match)$replacement})
  }
  [IO.File]::WriteAllText($cmsConfigPath,$cms)
 }
 Push-Location (Join-Path $root 'nitro-proxy');try{npm install --omit=dev;if($LASTEXITCODE){throw 'npm install echoue'}}finally{Pop-Location}
 Write-Host 'PRET. Lance LANCER-PARADISERP.bat' -ForegroundColor Green
}finally{$env:MYSQL_PWD=$null;if($ptr-ne[IntPtr]::Zero){[Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)}}
