[CmdletBinding()]
param(
    [string]$RepositoryRoot = "",
    [string]$HabboRpRoot = "C:\xampp\htdocs\HabboRPbr",
    [string]$Mysql = "C:\xampp\mysql\bin\mysql.exe",
    [string]$Database = "waveplus"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = Split-Path -Parent $PSScriptRoot
}

$targetFurnitureData = Join-Path $RepositoryRoot "swf_pz\V5-0-2\gamedata\json\FurnitureData.json"
$sourceFurnitureData = Join-Path $HabboRpRoot "pack\cdn-react\gamedata\FurnitureData.json"
$sourceFurnitureDir = Join-Path $HabboRpRoot "pack\cdn-react\bundled\furniture"
$sourceIconDir = Join-Path $HabboRpRoot "pack\cdn-react\icons"

foreach ($path in @($Mysql,$targetFurnitureData,$sourceFurnitureData,$sourceFurnitureDir,$sourceIconDir)) {
    if (-not (Test-Path -LiteralPath $path)) { throw "Ressource requise absente : $path" }
}

function Get-Category([string]$Text) {
    $n = $Text.ToLowerInvariant()
    if ($n -match 'xmas|christmas|noel|kerst|easter|pasen|hween|halloween|valentine|newyear|carnav|circus|festive|pumpkin|snow|winter') { return 'Saisons et evenements' }
    if ($n -match 'hospital|clinic|medic|doctor|nurse|ambulance|pharmacy|surgery|dentist|firstaid|stretcher|wheelchair') { return 'Medical et sante' }
    if ($n -match 'police|prison|security|court|swat|jail|guard|army|military') { return 'Securite et armee' }
    if ($n -match '(^|_)car(_|$)|vehicle|garage|taxi|(^|_)bus(_|$)|busstop|bike|bicycle|moto|scooter|train|metro|airport|plane|boat|ship|tram|rail|parking|petrol|traffic') { return 'Transports' }
    if ($n -match 'shop|store|market|restaurant|cafe|coffee|food|bar_|mall|vending|bakery|diner|kiosk|boutique|cashier|checkout|register|counter|menu|pizza|burger|salon') { return 'Commerces et restauration' }
    if ($n -match 'office|bank|school|city|urban|hotel|apartment|government|station|reception|lobby|library|museum|streetlight|elevator|locker|mail|queue|barrier|sign_') { return 'Ville et services' }
    if ($n -match 'computer|laptop|phone|screen|monitor|server|tech|robot|camera|television|speaker|console|radio|tablet|keyboard|machine|device') { return 'Technologie et bureau' }
    if ($n -match 'tree|plant|flower|fleur|sakura|bush|grass|rock|garden|forest|farm|beach|water|pool|nature|outdoor|park|pond|river|mountain|sand|soil') { return 'Nature et exterieurs' }
    if ($n -match 'game|gaming|music|disco|cinema|stage|gym|football|trophy|dance|theatre|basket|tennis|skate|ball|piano|guitar|drum|billiard|pooltable|foosball|dj_|arcade|chess|domino|mahjong|sport') { return 'Jeux loisirs et musique' }
    if ($n -match 'sofa|chair|seat|stool|bench|table|cabinet|shelf|bed|nightstand|bath|toilet|shower|kitchen|lamp|rug|carpet|home|wardrobe|dresser|desk|couch|fridge|oven|sink|mirror|blanket|painting|candle|bowl|cushion|pillow|frame|vase') { return 'Maison et decoration' }
    if ($n -match 'wall|floor|tile|roof|door|window|gate|fence|stair|column|pillar|block|build|construction|road|street|bridge|brick|concrete|beam|ladder|plank') { return 'Construction et architecture' }
    return 'Collections diverses'
}

$target = Get-Content -LiteralPath $targetFurnitureData -Raw | ConvertFrom-Json
$source = Get-Content -LiteralPath $sourceFurnitureData -Raw | ConvertFrom-Json

$targetNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($entry in @($target.roomitemtypes.furnitype)+@($target.wallitemtypes.furnitype)) {
    if ($null -ne $entry.classname) { [void]$targetNames.Add([string]$entry.classname) }
}

$query = @"
SELECT id,item_name,public_name,type
FROM furniture
WHERE type IN ('s','i') AND item_name<>'';
"@
$rawRows = & $Mysql -u root -N -B --raw $Database -e $query
if ($LASTEXITCODE -ne 0) { throw "Lecture MySQL impossible dans $Database." }

$dbByName = @{}
foreach ($line in $rawRows) {
    $parts = $line -split "`t", -1
    if ($parts.Count -ge 4 -and -not $dbByName.ContainsKey($parts[1])) { $dbByName[$parts[1]] = $parts }
}

$allSource = @($source.roomitemtypes.furnitype)+@($source.wallitemtypes.furnitype)
$stats = [ordered]@{
    'FurnitureData source' = $allSource.Count
    'Nitro dans le dossier source' = @(Get-ChildItem -LiteralPath $sourceFurnitureDir -Filter *.nitro -File).Count
    'Icones dans le dossier source' = @(Get-ChildItem -LiteralPath $sourceIconDir -Filter *_icon.png -File).Count
    'Deja presents dans ParadiseRP' = 0
    'Sans ligne serveur furniture' = 0
    'Sans fichier .nitro' = 0
    'Sans icone' = 0
    'Nouveaux complets exploitables' = 0
}

$usable = [Collections.Generic.List[object]]::new()
$seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)

foreach ($entry in $allSource) {
    $className = [string]$entry.classname
    if ([string]::IsNullOrWhiteSpace($className)) { continue }
    if (-not $seen.Add($className)) { continue }
    if ($targetNames.Contains($className)) { $stats['Deja presents dans ParadiseRP']++; continue }
    if (-not $dbByName.ContainsKey($className)) { $stats['Sans ligne serveur furniture']++; continue }

    $assetName = $className.Split('*')[0]
    $assetPath = Join-Path $sourceFurnitureDir ($assetName + '.nitro')
    $iconPath = Join-Path $sourceIconDir ($assetName + '_icon.png')
    if (-not (Test-Path -LiteralPath $assetPath)) { $stats['Sans fichier .nitro']++; continue }
    if (-not (Test-Path -LiteralPath $iconPath)) { $stats['Sans icone']++; continue }

    $db = $dbByName[$className]
    $name = if ($null -ne $entry.name) { [string]$entry.name } else { '' }
    $description = if ($null -ne $entry.description) { [string]$entry.description } else { '' }
    $search = "$className $($db[2]) $name $description"
    $usable.Add([pscustomobject]@{
        ClassName = $className
        AssetName = $assetName
        FurnitureId = [long]$db[0]
        PublicName = [string]$db[2]
        Category = Get-Category $search
        NitroBytes = (Get-Item -LiteralPath $assetPath).Length
    })
    $stats['Nouveaux complets exploitables']++
}

Write-Host "=== AUDIT MASSIF HABBORPBR -> PARADISERP ===" -ForegroundColor Cyan
foreach ($kv in $stats.GetEnumerator()) {
    Write-Host ("{0,-34}: {1}" -f $kv.Key,$kv.Value) -ForegroundColor White
}

Write-Host "`n=== REPARTITION DES NOUVEAUX EXPLOITABLES ===" -ForegroundColor Cyan
$usable | Group-Object Category | Sort-Object Count -Descending | ForEach-Object {
    Write-Host ("{0,-30}: {1,6}" -f $_.Name,$_.Count) -ForegroundColor Green
}

$projected = $targetNames.Count + $usable.Count
Write-Host "`nMobis connus actuellement : $($targetNames.Count)" -ForegroundColor Cyan
Write-Host "Nouveaux mobis complets ajoutables : $($usable.Count)" -ForegroundColor Cyan
Write-Host "Total theorique apres import : $projected" -ForegroundColor Green

$csv = Join-Path $PSScriptRoot 'audit-habborpbr-mass-catalog.csv'
$usable | Sort-Object Category,ClassName | Export-Csv -LiteralPath $csv -NoTypeInformation -Encoding UTF8
Write-Host "Rapport detaille : $csv" -ForegroundColor Green
