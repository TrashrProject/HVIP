$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot;$runtime=Join-Path $root 'runtime';$wr=Join-Path $runtime 'WavePlus';$exe=Join-Path $wr 'Plus.exe';$proxy=Join-Path $root 'nitro-proxy\server.js';$logs=Join-Path $runtime 'logs'
New-Item -ItemType Directory -Path $logs -Force|Out-Null
if(!(Test-Path $exe)){throw 'Lance INSTALLER-PARADISERP.bat avant'}
if(!(Get-NetTCPConnection -State Listen -LocalPort 2096 -ErrorAction SilentlyContinue)){Start-Process $exe -WorkingDirectory $wr -WindowStyle Hidden -RedirectStandardOutput (Join-Path $logs 'waveplus.out.log') -RedirectStandardError (Join-Path $logs 'waveplus.err.log')}
if(!(Get-NetTCPConnection -State Listen -LocalPort 2097 -ErrorAction SilentlyContinue)){$env:NITRO_WS_HOST='127.0.0.1';$env:NITRO_WS_PORT='2097';$env:EMU_HOST='127.0.0.1';$env:EMU_PORT='2096';Start-Process node.exe -ArgumentList $proxy -WorkingDirectory (Split-Path $proxy) -WindowStyle Hidden -RedirectStandardOutput (Join-Path $logs 'proxy.out.log') -RedirectStandardError (Join-Path $logs 'proxy.err.log')}
Start-Sleep -Seconds 8
Get-NetTCPConnection -State Listen|Where-Object{$_.LocalPort-in 2096,2097}|Select-Object LocalAddress,LocalPort,OwningProcess