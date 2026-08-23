$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$source=Join-Path $root 'WavePlus'
$sql=Join-Path $source 'waveplus.sql'
$migrations=Join-Path $source 'database\production-migrations.sql'
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
 $cp=Join-Path $source 'Config\config.ini';$cfg=[IO.File]::ReadAllText($cp)
 $v=@{'db.hostname'=$h;'db.port'=$p;'db.username'=$u;'db.password'=$pw.Replace("`r",'').Replace("`n",'');'db.name'=$n}
 foreach($k in $v.Keys){$cfg=[regex]::Replace($cfg,"(?m)^$([regex]::Escape($k))=.*$","$k=$($v[$k])")}
 $cfg=[regex]::Replace($cfg,'(?m)^group\.badge\.url=.*$','group.badge.url=https://paradiserp.fr/swf_pz/V5-0-2/c_images/Badgeparts/%badge%.png')
 [IO.File]::WriteAllText($cp,$cfg)
 New-Item -ItemType Directory -Path $out -Force|Out-Null
 & dotnet publish (Join-Path $source 'Plus.csproj') -c Release -r win-x64 --self-contained true -o $out
 if($LASTEXITCODE){throw 'Compilation WavePlus echouee'}
 $runtimeConfigDir=Join-Path $out 'Config'
 New-Item -ItemType Directory -Path $runtimeConfigDir -Force|Out-Null
 [IO.File]::WriteAllText((Join-Path $runtimeConfigDir 'config.ini'),$cfg)
 Push-Location (Join-Path $root 'nitro-proxy');try{npm install --omit=dev;if($LASTEXITCODE){throw 'npm install echoue'}}finally{Pop-Location}
 Write-Host 'PRET. Lance LANCER-PARADISERP.bat' -ForegroundColor Green
}finally{$env:MYSQL_PWD=$null;if($ptr-ne[IntPtr]::Zero){[Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)}}