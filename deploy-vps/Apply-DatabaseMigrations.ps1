$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$sql=Join-Path $root 'WavePlus\database\production-migrations.sql'
$weaponSkinsSql=Join-Path $root 'migrations\20260902_paradise_weapon_skins.sql'
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
 foreach($migrationFile in @($sql,$weaponSkinsSql)){
  if(!(Test-Path -LiteralPath $migrationFile)){continue}
  $mig=Start-Process $mysql -ArgumentList @("--host=$h","--port=$p","--user=$u","--database=$n",'--default-character-set=utf8mb4') -RedirectStandardInput $migrationFile -NoNewWindow -Wait -PassThru
  if($mig.ExitCode){throw "Migration SQL echouee : $migrationFile"}
 }
 $cmsConfigPath=Join-Path $root 'WebPixel\app\Controller\Config.class.php'
 if(Test-Path $cmsConfigPath){
  $cms=[IO.File]::ReadAllText($cmsConfigPath)
  $phpValues=@{'DBHOST'=$h;'DBName'=$n;'DBUser'=$u;'DBPass'=$pw}
  foreach($property in $phpValues.Keys){
   $escaped=[string]$phpValues[$property]
   $escaped=$escaped.Replace('\','\\').Replace("'","\'").Replace("`r",'').Replace("`n",'')
   $replacement="    protected static `$$property = '$escaped';"
   $pattern="(?m)^\s*protected static \`$$property\s*=\s*.*?;\s*$"
   $cms=[regex]::Replace($cms,$pattern,[System.Text.RegularExpressions.MatchEvaluator]{param($match)$replacement})
  }
  [IO.File]::WriteAllText($cmsConfigPath,$cms)
 }
 Write-Host 'Base et CMS configures sur waveplus. Tu peux te recreer un compte.' -ForegroundColor Green
}finally{$env:MYSQL_PWD=$null;if($ptr-ne[IntPtr]::Zero){[Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)}}
