[CmdletBinding()]
param(
    [string]$SearchRoot = "C:\xampp\htdocs",
    [string]$CurrentFurnitureData = "C:\HVIP\swf_pz\V5-0-2\gamedata\json\FurnitureData.json",
    [int]$Target = 50000
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-FurniEntries([string]$Path) {
    try {
        $json = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
        return @($json.roomitemtypes.furnitype) + @($json.wallitemtypes.furnitype)
    }
    catch {
        Write-Warning "FurnitureData illisible : $Path"
        return @()
    }
}

$currentNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
if (Test-Path -LiteralPath $CurrentFurnitureData) {
    foreach ($entry in Get-FurniEntries $CurrentFurnitureData) {
        if ($null -ne $entry.classname) { [void]$currentNames.Add([string]$entry.classname) }
    }
}

$fdFiles = @(Get-ChildItem -LiteralPath $SearchRoot -Filter FurnitureData.json -File -Recurse -ErrorAction SilentlyContinue)
$results = [Collections.Generic.List[object]]::new()
$globalComplete = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$globalNew = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)

foreach ($fd in $fdFiles) {
    $entries = @(Get-FurniEntries $fd.FullName)
    if ($entries.Count -eq 0) { continue }

    $base = Split-Path -Parent (Split-Path -Parent $fd.DirectoryName)
    $candidateFurnitureDirs = @(
        (Join-Path $base 'bundled\furniture'),
        (Join-Path $base 'furniture'),
        (Join-Path (Split-Path -Parent $fd.DirectoryName) 'furniture'),
        (Join-Path $fd.DirectoryName '..\..\furniture')
    ) | ForEach-Object { try { (Resolve-Path -LiteralPath $_ -ErrorAction Stop).Path } catch { $null } } | Where-Object { $_ } | Select-Object -Unique

    $candidateIconDirs = @(
        (Join-Path $base 'icons'),
        (Join-Path $base 'dcr\hof_furni\icon'),
        (Join-Path (Split-Path -Parent $fd.DirectoryName) 'dcr\hof_furni\icon'),
        (Join-Path $fd.DirectoryName '..\..\dcr\hof_furni\icon')
    ) | ForEach-Object { try { (Resolve-Path -LiteralPath $_ -ErrorAction Stop).Path } catch { $null } } | Where-Object { $_ } | Select-Object -Unique

    $complete = 0
    $newComplete = 0
    foreach ($entry in $entries) {
        $className = [string]$entry.classname
        if ([string]::IsNullOrWhiteSpace($className)) { continue }
        $assetName = $className.Split('*')[0]
        if ($assetName -notmatch '^[A-Za-z0-9_.-]+$') { continue }

        $hasAsset = $false
        foreach ($dir in $candidateFurnitureDirs) {
            if (Test-Path -LiteralPath (Join-Path $dir ($assetName + '.nitro'))) { $hasAsset = $true; break }
        }
        if (-not $hasAsset) { continue }

        $hasIcon = $false
        foreach ($dir in $candidateIconDirs) {
            if (Test-Path -LiteralPath (Join-Path $dir ($assetName + '_icon.png'))) { $hasIcon = $true; break }
        }
        if (-not $hasIcon) { continue }

        $complete++
        [void]$globalComplete.Add($className)
        if (-not $currentNames.Contains($className)) {
            $newComplete++
            [void]$globalNew.Add($className)
        }
    }

    if ($complete -gt 0) {
        $results.Add([pscustomobject]@{
            FurnitureData = $fd.FullName
            Entries = $entries.Count
            Complete = $complete
            NewComplete = $newComplete
            FurnitureDirs = ($candidateFurnitureDirs -join '; ')
            IconDirs = ($candidateIconDirs -join '; ')
        })
    }
}

Write-Host "=== AUDIT SOURCES MOBIS PARADISERP ===" -ForegroundColor Cyan
Write-Host "Mobis actuellement connus dans FurnitureData : $($currentNames.Count)" -ForegroundColor White
Write-Host "Sources FurnitureData trouvees : $($fdFiles.Count)" -ForegroundColor White
Write-Host "Mobis complets uniques detectes (asset + icone) : $($globalComplete.Count)" -ForegroundColor Green
Write-Host "Mobis complets uniques encore absents du catalogue courant : $($globalNew.Count)" -ForegroundColor Green
Write-Host "Objectif total : $Target" -ForegroundColor Cyan
$projected = $currentNames.Count + $globalNew.Count
Write-Host "Maximum theorique avec les sources locales detectees : $projected" -ForegroundColor Cyan

if ($projected -ge $Target) {
    Write-Host "OBJECTIF 50K ATTEIGNABLE avec les sources locales." -ForegroundColor Green
} else {
    Write-Warning "Il manque encore $($Target - $projected) mobis complets uniques pour atteindre $Target."
}

Write-Host "`n=== DETAIL PAR SOURCE ===" -ForegroundColor Cyan
$results | Sort-Object NewComplete -Descending | Format-Table -AutoSize FurnitureData,Entries,Complete,NewComplete

$csv = Join-Path $PSScriptRoot 'audit-furniture-sources-50k.csv'
$results | Sort-Object NewComplete -Descending | Export-Csv -LiteralPath $csv -NoTypeInformation -Encoding UTF8
Write-Host "`nRapport CSV : $csv" -ForegroundColor Green
