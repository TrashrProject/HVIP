[CmdletBinding()]
param(
    [string]$RepositoryRoot = "",
    [string]$HabboRpRoot = "C:\xampp\htdocs\HabboRPbr",
    [string]$Mysql = "C:\xampp\mysql\bin\mysql.exe",
    [string]$Database = "waveplus",
    [int]$Limit = 2000
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = Split-Path -Parent $PSScriptRoot
}

$builder = Join-Path $PSScriptRoot "build-paradise-catalog-v4.ps1"
$rebalancer = Join-Path $PSScriptRoot "rebalance-paradise-catalog-v4.ps1"

& $builder -RepositoryRoot $RepositoryRoot -HabboRpRoot $HabboRpRoot -Mysql $Mysql -Database $Database -Limit $Limit
if ($LASTEXITCODE -ne 0) { throw "La generation du catalogue v4 a echoue." }

& $rebalancer -RepositoryRoot $RepositoryRoot
if ($LASTEXITCODE -ne 0) { throw "Le reequilibrage final du catalogue a echoue." }

Write-Host "Catalogue ParadiseRP final pret." -ForegroundColor Green
