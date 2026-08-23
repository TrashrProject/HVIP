$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$sql=Join-Path $root 'WavePlus\database\production-migrations.sql'
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
 $mig=Start-Process $mysql -ArgumentList @("--host=$h","--port=$p","--user=$u","--database=$n",'--default-character-set=utf8mb4') -RedirectStandardInput $sql -NoNewWindow -Wait -PassThru
 if($mig.ExitCode){throw 'Migration SQL echouee'}
 Write-Host 'Base corrigee. Tu peux relancer LANCER-PARADISERP.bat' -ForegroundColor Green
}finally{$env:MYSQL_PWD=$null;if($ptr-ne[IntPtr]::Zero){[Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)}}