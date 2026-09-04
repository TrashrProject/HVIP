[CmdletBinding()]
param([string]$RepositoryRoot = "")

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) { $RepositoryRoot = Split-Path -Parent $PSScriptRoot }

$furniDataPath = Join-Path $RepositoryRoot "swf_pz\V5-0-2\gamedata\json\FurnitureData.json"
$iconDir = Join-Path $RepositoryRoot "swf_pz\V5-0-2\dcr\hof_furni\icon"
$data = Get-Content -LiteralPath $furniDataPath -Raw | ConvertFrom-Json
$entries = @($data.roomitemtypes.furnitype) + @($data.wallitemtypes.furnitype)
$created = 0
$unresolved = [Collections.Generic.List[string]]::new()

foreach ($entry in $entries) {
    $className = [string]$entry.classname
    if ($className -notmatch '^([^*]+)\*(.+)$') { continue }
    $base = $Matches[1]
    $parameter = $Matches[2]
    if ($base -notmatch '^[A-Za-z0-9_.-]+$' -or $parameter -notmatch '^[A-Za-z0-9_.-]+$') { continue }

    $source = Join-Path $iconDir ($base + '_icon.png')
    $destination = Join-Path $iconDir ($base + '_' + $parameter + '_icon.png')
    if (Test-Path -LiteralPath $destination) { continue }

    # Une ancienne source ne fournit pas l'aperçu du dragon p3x. Un dragon-lampe
    # local sert uniquement de miniature de catalogue; son asset Nitro reste exact.
    if (-not (Test-Path -LiteralPath $source) -and $base -eq 'dragon_lamp_p3x_001') {
        $source = Join-Path $iconDir '15rare_dragonlamp_icon.png'
    }

    if (Test-Path -LiteralPath $source) {
        Copy-Item -LiteralPath $source -Destination $destination -Force
        $created++
    } else {
        $unresolved.Add($className)
    }
}

Write-Host "Alias d'icones crees : $created" -ForegroundColor Green
if ($unresolved.Count -gt 0) {
    $unresolved | Sort-Object -Unique | ForEach-Object { Write-Warning "Icone introuvable : $_" }
    throw "$($unresolved.Count) variante(s) restent sans icone."
}
