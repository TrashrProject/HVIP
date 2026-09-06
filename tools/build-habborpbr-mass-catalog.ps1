[CmdletBinding()]
param(
    [string]$RepositoryRoot = "",
    [string]$HabboRpRoot = "C:\xampp\htdocs\HabboRPbr",
    [string]$Mysql = "C:\xampp\mysql\bin\mysql.exe",
    [string]$Database = "waveplus",
    [int]$Limit = 0
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) { $RepositoryRoot = Split-Path -Parent $PSScriptRoot }

$targetFurnitureData = Join-Path $RepositoryRoot "swf_pz\V5-0-2\gamedata\json\FurnitureData.json"
$targetFurnitureDir  = Join-Path $RepositoryRoot "swf_pz\V5-0-2\furniture"
$targetIconDir       = Join-Path $RepositoryRoot "swf_pz\V5-0-2\dcr\hof_furni\icon"
$sourceFurnitureData = Join-Path $HabboRpRoot "pack\cdn-react\gamedata\FurnitureData.json"
$sourceFurnitureDir  = Join-Path $HabboRpRoot "pack\cdn-react\bundled\furniture"
$sourceIconDir       = Join-Path $HabboRpRoot "pack\cdn-react\icons"
$migrationPath       = Join-Path $RepositoryRoot "migrations\20260903_paradise_catalogue_mass_habborpbr.sql"

foreach ($path in @($Mysql,$targetFurnitureData,$sourceFurnitureData,$sourceFurnitureDir,$sourceIconDir)) {
    if (-not (Test-Path -LiteralPath $path)) { throw "Ressource requise absente : $path" }
}

function Escape-Sql([AllowNull()][string]$Value) {
    if ($null -eq $Value) { return '' }
    return $Value.Replace('\\','\\\\').Replace("'","''").Replace("`r",' ').Replace("`n",' ')
}

function Get-SemanticCategory([string]$Text) {
    $n = $Text.ToLowerInvariant()
    if ($n -match 'xmas|christmas|noel|kerst|easter|pasen|hween|halloween|valentine|newyear|carnav|circus|festive|pumpkin|snow|winter') { return 'Saisons et evenements' }
    if ($n -match 'hospital|clinic|medic|doctor|nurse|ambulance|pharmacy|surgery|dentist|firstaid|stretcher|wheelchair|xray|defib') { return 'Medical et sante' }
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

function Get-DiverseBucket([string]$Name) {
    $c = if ([string]::IsNullOrWhiteSpace($Name)) { '#' } else { $Name.Substring(0,1).ToUpperInvariant() }
    if ($c -match '[A-D0-9]') { return 'Collections A-D' }
    if ($c -match '[E-H]') { return 'Collections E-H' }
    if ($c -match '[I-L]') { return 'Collections I-L' }
    if ($c -match '[M-P]') { return 'Collections M-P' }
    if ($c -match '[Q-T]') { return 'Collections Q-T' }
    return 'Collections U-Z'
}

$pages = [ordered]@{
    'Construction et architecture' = 9967201
    'Maison et decoration'         = 9967202
    'Ville et services'            = 9967203
    'Commerces et restauration'    = 9967204
    'Nature et exterieurs'         = 9967205
    'Jeux loisirs et musique'      = 9967206
    'Saisons et evenements'        = 9967207
    'Medical et sante'             = 9967208
    'Securite et armee'            = 9967209
    'Transports'                   = 9967210
    'Technologie et bureau'        = 9967211
    'Collections A-D'              = 9967212
    'Collections E-H'              = 9967213
    'Collections I-L'              = 9967214
    'Collections M-P'              = 9967215
    'Collections Q-T'              = 9967216
    'Collections U-Z'              = 9967217
}
$rootPageId = 9967200

$query = @"
SELECT id,item_name,public_name,type,width,length,stack_height,can_stack,can_sit,
       COALESCE(is_walkable,0),COALESCE(sprite_id,0),allow_recycle,allow_trade,
       allow_marketplace_sell,allow_gift,allow_inventory_stack,interaction_type,
       behaviour_data,interaction_modes_count,vending_ids,height_adjustable,effect_id,
       wired_id,is_rare,clothing_id,extra_rot,COALESCE(allow_lay,0)
FROM furniture WHERE type IN ('s','i') AND item_name<>'';
"@
$rawRows = & $Mysql -u root -N -B --raw $Database -e $query
if ($LASTEXITCODE -ne 0) { throw "Lecture MySQL impossible dans $Database." }
$dbByName = @{}
foreach ($line in $rawRows) {
    $parts = $line -split "`t", -1
    if ($parts.Count -ge 27 -and -not $dbByName.ContainsKey($parts[1])) { $dbByName[$parts[1]] = $parts }
}

$target = Get-Content -LiteralPath $targetFurnitureData -Raw | ConvertFrom-Json
$source = Get-Content -LiteralPath $sourceFurnitureData -Raw | ConvertFrom-Json
$targetNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$targetIds = [Collections.Generic.HashSet[long]]::new()
foreach ($entry in @($target.roomitemtypes.furnitype)+@($target.wallitemtypes.furnitype)) {
    if ($null -ne $entry.classname) { [void]$targetNames.Add([string]$entry.classname) }
    if ($null -ne $entry.id) { [void]$targetIds.Add([long]$entry.id) }
}

$sourceEntries = [Collections.Generic.List[object]]::new()
foreach ($entry in @($source.roomitemtypes.furnitype)) { $sourceEntries.Add([pscustomobject]@{Kind='room';Entry=$entry}) }
foreach ($entry in @($source.wallitemtypes.furnitype)) { $sourceEntries.Add([pscustomobject]@{Kind='wall';Entry=$entry}) }

$usable = [Collections.Generic.List[object]]::new()
$seenNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$seenIds = [Collections.Generic.HashSet[long]]::new()
foreach ($item in $sourceEntries) {
    $className = [string]$item.Entry.classname
    if ([string]::IsNullOrWhiteSpace($className) -or $targetNames.Contains($className)) { continue }
    if (-not $dbByName.ContainsKey($className)) { continue }
    $db = $dbByName[$className]
    $fid = [long]$db[0]
    if ($targetIds.Contains($fid) -or -not $seenNames.Add($className) -or -not $seenIds.Add($fid)) { continue }
    $assetName = $className.Split('*')[0]
    if ($assetName -notmatch '^[A-Za-z0-9_.-]+$') { continue }
    $assetPath = Join-Path $sourceFurnitureDir ($assetName + '.nitro')
    $iconPath = Join-Path $sourceIconDir ($assetName + '_icon.png')
    if (-not (Test-Path -LiteralPath $assetPath) -or -not (Test-Path -LiteralPath $iconPath)) { continue }
    $entryName = if ($null -ne $item.Entry.name) { [string]$item.Entry.name } else { '' }
    $desc = if ($null -ne $item.Entry.description) { [string]$item.Entry.description } else { '' }
    $search = "$className $($db[2]) $entryName $desc"
    $category = Get-SemanticCategory $search
    if ($category -eq 'Collections diverses') { $category = Get-DiverseBucket $className }
    $usable.Add([pscustomobject]@{Kind=$item.Kind;Entry=$item.Entry;Db=$db;ClassName=$className;AssetName=$assetName;Category=$category})
}

$selected = @($usable | Sort-Object Category,ClassName)
if ($Limit -gt 0 -and $selected.Count -gt $Limit) { $selected = @($selected | Select-Object -First $Limit) }
if ($selected.Count -eq 0) { throw 'Aucun nouveau mobi complet a importer.' }

New-Item -ItemType Directory -Path $targetFurnitureDir -Force | Out-Null
New-Item -ItemType Directory -Path $targetIconDir -Force | Out-Null
foreach ($item in $selected) {
    Copy-Item -LiteralPath (Join-Path $sourceFurnitureDir ($item.AssetName+'.nitro')) -Destination (Join-Path $targetFurnitureDir ($item.AssetName+'.nitro')) -Force
    Copy-Item -LiteralPath (Join-Path $sourceIconDir ($item.AssetName+'_icon.png')) -Destination (Join-Path $targetIconDir ($item.AssetName+'_icon.png')) -Force
    # Le client doit utiliser exactement le meme identifiant que furniture.id.
    $item.Entry.id = [long]$item.Db[0]
    $item.Entry.offerid = [long]$item.Db[0]
}

$roomNew = @($selected | Where-Object Kind -eq 'room' | ForEach-Object Entry)
$wallNew = @($selected | Where-Object Kind -eq 'wall' | ForEach-Object Entry)
$target.roomitemtypes.furnitype = @($target.roomitemtypes.furnitype) + $roomNew
$target.wallitemtypes.furnitype = @($target.wallitemtypes.furnitype) + $wallNew
[IO.File]::WriteAllText($targetFurnitureData,($target | ConvertTo-Json -Depth 30 -Compress),[Text.UTF8Encoding]::new($false))

$sb = [Text.StringBuilder]::new()
[void]$sb.AppendLine('-- ParadiseRP import massif HabboRPbr - genere automatiquement')
[void]$sb.AppendLine('SET NAMES utf8mb4;')
[void]$sb.AppendLine('START TRANSACTION;')

$pageRows = [Collections.Generic.List[string]]::new()
$pageRows.Add("($rootPageId,9967000,'Catalogue massif HabboRPbr',1,'1','1',1,0,90,'','default_3x3','','')")
$order = 1
foreach ($kv in $pages.GetEnumerator()) {
    $cap = Escape-Sql $kv.Key
    $pageRows.Add("($($kv.Value),$rootPageId,'$cap',1,'1','1',1,0,$order,'','default_3x3','','')")
    $order++
}
[void]$sb.AppendLine('INSERT INTO catalog_pages (id,parent_id,caption,icon_image,visible,enabled,min_rank,min_vip,order_num,page_link,page_layout,page_strings_1,page_strings_2) VALUES')
[void]$sb.AppendLine(($pageRows -join ",`n") + "`nON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption=VALUES(caption),order_num=VALUES(order_num),visible='1',enabled='1';")
[void]$sb.AppendLine("DELETE FROM catalog_items WHERE page_id BETWEEN 9967200 AND 9967299;")

$furnitureRows = foreach ($item in $selected) {
    $d = $item.Db
    $values = @($d[0],"'$(Escape-Sql $d[1])'","'$(Escape-Sql $d[2])'","'$($d[3])'",$d[4],$d[5],$d[6],$d[7],$d[8],$d[9],$d[0],"'$($d[11])'","'$($d[12])'","'$($d[13])'","'$($d[14])'","'$($d[15])'","'$(Escape-Sql $d[16])'",$d[17],$d[18],"'$(Escape-Sql $d[19])'","'$(Escape-Sql $d[20])'",$d[21],$d[22],"'$($d[23])'",$d[24],"'$($d[25])'",$d[26])
    '(' + ($values -join ',') + ')'
}
$furnitureHeader = 'INSERT INTO furniture (id,item_name,public_name,type,width,length,stack_height,can_stack,can_sit,is_walkable,sprite_id,allow_recycle,allow_trade,allow_marketplace_sell,allow_gift,allow_inventory_stack,interaction_type,behaviour_data,interaction_modes_count,vending_ids,height_adjustable,effect_id,wired_id,is_rare,clothing_id,extra_rot,allow_lay) VALUES'
$furnitureUpdate = 'ON DUPLICATE KEY UPDATE item_name=VALUES(item_name),public_name=VALUES(public_name),type=VALUES(type),width=VALUES(width),length=VALUES(length),stack_height=VALUES(stack_height),can_stack=VALUES(can_stack),can_sit=VALUES(can_sit),is_walkable=VALUES(is_walkable),sprite_id=VALUES(sprite_id),interaction_type=VALUES(interaction_type),behaviour_data=VALUES(behaviour_data),interaction_modes_count=VALUES(interaction_modes_count),vending_ids=VALUES(vending_ids),allow_lay=VALUES(allow_lay);'
for ($offset=0; $offset -lt $furnitureRows.Count; $offset+=250) {
    $last = [Math]::Min($offset+249,$furnitureRows.Count-1)
    [void]$sb.AppendLine($furnitureHeader)
    [void]$sb.AppendLine((@($furnitureRows[$offset..$last]) -join ",`n") + "`n$furnitureUpdate")
}

$catalogHeader = 'INSERT INTO catalog_items (page_id,item_id,catalog_name,cost_credits,cost_pixels,cost_diamonds,amount,limited_sells,limited_stack,offer_active,extradata,badge,offer_id,points_type) VALUES'
$offer = 1950200000
$catalogRows = foreach ($item in $selected) {
    $d = $item.Db
    $pageId = [int]$pages[$item.Category]
    $name = if ([string]::IsNullOrWhiteSpace([string]$d[2])) { [string]$d[1] } else { [string]$d[2] }
    $price = [Math]::Min(30,[Math]::Max(3,2+([int]$d[4]*[int]$d[5])))
    $row = "($pageId,'$($d[0])','$(Escape-Sql $name)',$price,0,0,1,0,0,'1','','',$offer,0)"
    $offer++
    $row
}
for ($offset=0; $offset -lt $catalogRows.Count; $offset+=500) {
    $last = [Math]::Min($offset+499,$catalogRows.Count-1)
    [void]$sb.AppendLine($catalogHeader)
    [void]$sb.AppendLine((@($catalogRows[$offset..$last]) -join ",`n") + ';')
}
[void]$sb.AppendLine('COMMIT;')
[IO.File]::WriteAllText($migrationPath,$sb.ToString(),[Text.UTF8Encoding]::new($false))

Write-Host "=== IMPORT MASSIF HABBORPBR PREPARE ===" -ForegroundColor Cyan
Write-Host "Nouveaux mobis complets retenus : $($selected.Count)" -ForegroundColor Green
Write-Host "Migration : $migrationPath" -ForegroundColor Green
Write-Host "FurnitureData total apres fusion : $(@($target.roomitemtypes.furnitype).Count + @($target.wallitemtypes.furnitype).Count)" -ForegroundColor Green
Write-Host "Repartition :" -ForegroundColor Cyan
$selected | Group-Object Category | Sort-Object Count -Descending | ForEach-Object { Write-Host (" - {0}: {1}" -f $_.Name,$_.Count) -ForegroundColor Green }
