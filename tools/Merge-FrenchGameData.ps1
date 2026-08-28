param(
    [string]$GameDataPath = "C:\HVIP\swf_pz\V5-0-2\gamedata\json",
    [string]$OfficialDataPath = "C:\HVIP\backups\before_game_french_20260824"
)

$ErrorActionPreference = 'Stop'
$utf8 = [System.Text.UTF8Encoding]::new($false)

function Read-Json([string]$Path) {
    return [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
}

function Write-Json([string]$Path, $Value) {
    $json = $Value | ConvertTo-Json -Depth 30 -Compress
    [System.IO.File]::WriteAllText($Path, $json, $utf8)
}

$furniturePath = Join-Path $GameDataPath 'FurnitureData.json'
$productPath = Join-Path $GameDataPath 'ProductData.json'
$backupPath = Join-Path $OfficialDataPath 'before-french-furniture-merge'
New-Item -ItemType Directory -Path $backupPath -Force | Out-Null
Copy-Item -LiteralPath $furniturePath -Destination $backupPath -Force
Copy-Item -LiteralPath $productPath -Destination $backupPath -Force

[xml]$officialFurniture = [System.IO.File]::ReadAllText(
    (Join-Path $OfficialDataPath 'official-fr-furnidata.json'),
    [System.Text.Encoding]::UTF8
)
$frenchFurniture = @{}
foreach ($node in @($officialFurniture.furnidata.roomitemtypes.furnitype) + @($officialFurniture.furnidata.wallitemtypes.furnitype)) {
    $frenchFurniture[[string]$node.classname] = $node
}

$furniture = Read-Json $furniturePath
$furnitureChanges = 0
foreach ($item in @($furniture.roomitemtypes.furnitype) + @($furniture.wallitemtypes.furnitype)) {
    $translation = $frenchFurniture[[string]$item.classname]
    if ($null -eq $translation) { continue }

    $name = [string]$translation.name
    $description = [string]$translation.description
    if ($name -and $item.name -ne $name) { $item.name = $name; $furnitureChanges++ }
    if ($description -and $item.description -ne $description) { $item.description = $description; $furnitureChanges++ }
}
Write-Json $furniturePath $furniture

[xml]$officialProducts = [System.IO.File]::ReadAllText(
    (Join-Path $OfficialDataPath 'official-fr-productdata.json'),
    [System.Text.Encoding]::UTF8
)
$frenchProducts = @{}
foreach ($node in @($officialProducts.productdata.product)) {
    $frenchProducts[[string]$node.code] = $node
}

$products = Read-Json $productPath
$productChanges = 0
foreach ($item in @($products.productdata.product)) {
    $translation = $frenchProducts[[string]$item.code]
    if ($null -eq $translation) { continue }

    $name = [string]$translation.name
    $description = [string]$translation.description
    if ($name -and $item.name -ne $name) { $item.name = $name; $productChanges++ }
    if ($description -and $item.description -ne $description) { $item.description = $description; $productChanges++ }
}
Write-Json $productPath $products

Write-Output "FURNITURE_FIELDS_CHANGED=$furnitureChanges"
Write-Output "PRODUCT_FIELDS_CHANGED=$productChanges"
