[CmdletBinding()]
param(
    [string]$RepositoryRoot = "",
    [string]$HabboRpRoot = "C:\xampp\htdocs\HabboRPbr"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = Split-Path -Parent $PSScriptRoot
}

$targetFurnitureData = Join-Path $RepositoryRoot "swf_pz\V5-0-2\gamedata\json\FurnitureData.json"
$targetFurnitureDir = Join-Path $RepositoryRoot "swf_pz\V5-0-2\furniture"
$targetIconDir = Join-Path $RepositoryRoot "swf_pz\V5-0-2\dcr\hof_furni\icon"
$sourceFurnitureData = Join-Path $HabboRpRoot "pack\cdn-react\gamedata\FurnitureData.json"
$sourceFurnitureDir = Join-Path $HabboRpRoot "pack\cdn-react\bundled\furniture"
$sourceIconDir = Join-Path $HabboRpRoot "pack\cdn-react\icons"
$migrationPath = Join-Path $RepositoryRoot "migrations\20260903_paradise_catalogue_rp_v3.sql"

foreach ($path in @($targetFurnitureData, $sourceFurnitureData, $sourceFurnitureDir, $sourceIconDir)) {
    if (-not (Test-Path -LiteralPath $path)) { throw "Ressource requise absente : $path" }
}

New-Item -ItemType Directory -Path $targetFurnitureDir -Force | Out-Null
New-Item -ItemType Directory -Path $targetIconDir -Force | Out-Null

$target = Get-Content -LiteralPath $targetFurnitureData -Raw | ConvertFrom-Json
$source = Get-Content -LiteralPath $sourceFurnitureData -Raw | ConvertFrom-Json
$prefixPattern = '^(gracie_24|betty_24|badrum_24)|^(Habblet|Hellsinore)_.*_24'

$selected = @()
foreach ($entry in @($source.roomitemtypes.furnitype)) {
    if ([string]$entry.classname -match $prefixPattern) {
        $selected += [pscustomobject]@{ Kind = 'room'; Entry = $entry }
    }
}
foreach ($entry in @($source.wallitemtypes.furnitype)) {
    if ([string]$entry.classname -match $prefixPattern) {
        $selected += [pscustomobject]@{ Kind = 'wall'; Entry = $entry }
    }
}
$selected = @($selected | Sort-Object { [string]$_.Entry.classname }, Kind -Unique)
if ($selected.Count -lt 200) { throw "Selection 2024 anormalement petite : $($selected.Count) meubles." }

function Get-FriendlyName([string]$className) {
    $collection = switch -Regex ($className) {
        '^gracie_24' { 'Gracie 2024'; break }
        '^betty_24' { 'Betty 2024'; break }
        '^badrum_24' { 'Badrum 2024'; break }
        '^Habblet_' { 'Habblet 2024'; break }
        '^Hellsinore_' { 'Hellsinore 2024'; break }
        default { 'Collection 2024' }
    }
    $label = $className -replace '^(gracie_24|betty_24|badrum_24)_?', ''
    $label = $label -replace '^(Habblet|Hellsinore)_', '' -replace '_24[0-9]*$', ''
    $label = $label -replace '_0?1$', '' -replace '_', ' '
    $translations = [ordered]@{
        'upper cabinet'='placard mural'; 'coffee table'='table basse'; 'coffee machine'='machine a cafe';
        'living chair'='fauteuil'; 'desk chair'='chaise de bureau'; 'room divider'='separation';
        'glass door'='porte vitree'; 'floor lamp'='lampadaire'; 'kitchen range'='cuisiniere';
        'range hood'='hotte'; 'cutting boards'='planches de cuisine'; 'picture frame'='cadre';
        'trash bin'='poubelle'; 'storage jar'='bocal'; 'flower rug'='tapis fleuri';
        'wall'='mur'; 'floor'='sol'; 'window'='fenetre'; 'door'='porte'; 'chair'='chaise';
        'bench'='banc'; 'stool'='tabouret'; 'sofa'='canape'; 'table'='table'; 'desk'='bureau';
        'bookshelf'='bibliotheque'; 'cabinet'='meuble'; 'drawer'='tiroir'; 'shelf'='etagere';
        'bed'='lit'; 'bath'='baignoire'; 'toilet'='toilettes'; 'shower'='douche'; 'sink'='lavabo';
        'lamp'='lampe'; 'rug'='tapis'; 'plant'='plante'; 'painting'='tableau'; 'mirror'='miroir';
        'fridge'='refrigerateur'; 'oven'='four'; 'stove'='cuisiniere'; 'pillow'='coussin';
        'counter'='comptoir'; 'block'='bloc'; 'tile'='dalle'; 'vase'='vase'; 'books'='livres'
    }
    foreach ($pair in $translations.GetEnumerator()) {
        $label = [regex]::Replace($label, "\b$([regex]::Escape($pair.Key))\b", $pair.Value, 'IgnoreCase')
    }
    $label = (Get-Culture).TextInfo.ToTitleCase($label.Trim().ToLowerInvariant())
    if (-not $label) { $label = 'Mobi' }
    return "$collection - $label"
}

function Escape-Sql([AllowNull()][string]$value) {
    if ($null -eq $value) { return '' }
    return $value.Replace('\', '\\').Replace("'", "''").Replace("`r", ' ').Replace("`n", ' ')
}

$nextId = 1924000001
$newEntries = @()
foreach ($item in $selected) {
    $entry = $item.Entry
    $className = [string]$entry.classname
    $assetName = $className.Split('*')[0]
    $assetPath = Join-Path $sourceFurnitureDir ($assetName + '.nitro')
    $iconPath = Join-Path $sourceIconDir ($assetName + '_icon.png')
    if (-not (Test-Path -LiteralPath $assetPath)) { throw "Asset Nitro absent : $assetName" }
    if (-not (Test-Path -LiteralPath $iconPath)) { throw "Icone absente : $assetName" }

    Copy-Item -LiteralPath $assetPath -Destination (Join-Path $targetFurnitureDir ($assetName + '.nitro')) -Force
    Copy-Item -LiteralPath $iconPath -Destination (Join-Path $targetIconDir ($assetName + '_icon.png')) -Force

    $entry.id = $nextId
    $entry.offerid = $nextId
    $entry.name = Get-FriendlyName $className
    $entry.description = 'Nouveaute ParadiseRP pour la construction et la decoration.'
    $newEntries += [pscustomobject]@{ Id = $nextId; Kind = $item.Kind; Entry = $entry; AssetName = $assetName }
    $nextId++
}

$newClassNames = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($item in $newEntries) { [void]$newClassNames.Add([string]$item.Entry.classname) }
$target.roomitemtypes.furnitype = @($target.roomitemtypes.furnitype | Where-Object { -not $newClassNames.Contains([string]$_.classname) }) + @($newEntries | Where-Object Kind -eq 'room' | ForEach-Object Entry)
$target.wallitemtypes.furnitype = @($target.wallitemtypes.furnitype | Where-Object { -not $newClassNames.Contains([string]$_.classname) }) + @($newEntries | Where-Object Kind -eq 'wall' | ForEach-Object Entry)

$json = $target | ConvertTo-Json -Depth 20 -Compress
[IO.File]::WriteAllText($targetFurnitureData, $json, [Text.UTF8Encoding]::new($false))

$pages = @(
    @(9967000,-1,'ParadiseRP - Construction',1,1),
    @(9967001,9967000,'Nouveautes 2024',1,1), @(9967002,9967000,'Gracie 2024',1,2),
    @(9967003,9967000,'Betty 2024',1,3), @(9967004,9967000,'Badrum 2024',1,4),
    @(9967005,9967000,'Habblet et Hellsinore 2024',1,5),
    @(9967010,9967000,'Fondations et blocs',1,10), @(9967011,9967000,'Murs et facades',1,11),
    @(9967012,9967000,'Sols et toitures',1,12), @(9967013,9967000,'Portes, fenetres et clotures',1,13),
    @(9967014,9967000,'Escaliers, piliers et structures',1,14),
    @(9967020,9967000,'Routes et signalisation',1,20), @(9967021,9967000,'Vehicules et garages',1,21),
    @(9967022,9967000,'Rail, metro et aeroport',1,22), @(9967023,9967000,'Port, bateaux et littoral',1,23),
    @(9967030,9967000,'Nature et relief',1,30), @(9967031,9967000,'Plages, eau et piscines',1,31),
    @(9967032,9967000,'Fermes et agriculture',1,32),
    @(9967040,9967000,'Salon et mobilier',1,40), @(9967041,9967000,'Cuisine et restauration',1,41),
    @(9967042,9967000,'Chambres et salles de bain',1,42), @(9967043,9967000,'Eclairage et ambiance',1,43),
    @(9967050,9967000,'Commerces et marches',1,50), @(9967051,9967000,'Cafes, bars et restaurants',1,51),
    @(9967052,9967000,'Bureaux, banques et services',1,52), @(9967053,9967000,'Hotels et loisirs',1,53),
    @(9967060,9967000,'Hopital et sante',1,60), @(9967061,9967000,'Police, justice et prison',1,61),
    @(9967062,9967000,'Pompiers et secours',1,62), @(9967063,9967000,'Mairie, ecole et services publics',1,63),
    @(9967070,9967000,'Industrie et chantier',1,70), @(9967071,9967000,'Technologie et medias',1,71),
    @(9967072,9967000,'Culture, sport et divertissement',1,72), @(9967073,9967000,'Animaux et accessoires',1,73),
    @(9967080,9967000,'Collections et decoration A-D',1,80), @(9967081,9967000,'Collections et decoration E-H',1,81),
    @(9967082,9967000,'Collections et decoration I-L',1,82), @(9967083,9967000,'Collections et decoration M-P',1,83),
    @(9967084,9967000,'Collections et decoration Q-T',1,84), @(9967085,9967000,'Collections et decoration U-Z',1,85),
    @(9967086,9967000,'Saisons et evenements',1,86), @(9967090,9967000,'Wired et outils staff',7,90)
)

$sb = [Text.StringBuilder]::new()
[void]$sb.AppendLine('-- ParadiseRP catalogue RP v3 - genere le 2026-09-03')
[void]$sb.AppendLine('-- Idempotent. N expose que des meubles presents dans FurnitureData.json et dans la table furniture.')
[void]$sb.AppendLine('SET NAMES utf8mb4;')
[void]$sb.AppendLine('START TRANSACTION;')
[void]$sb.AppendLine('')
[void]$sb.AppendLine('INSERT INTO catalog_pages (id,parent_id,caption,icon_image,visible,enabled,min_rank,min_vip,order_num,page_link,page_layout,page_strings_1,page_strings_2) VALUES')
$pageRows = @()
foreach ($page in $pages) {
    $pageRows += "($($page[0]),$($page[1]),'$($page[2])',1,'1','1',$($page[3]),'0',$($page[4]),'','default_3x3','','')"
}
[void]$sb.AppendLine(($pageRows -join ",`n") + "`nON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption=VALUES(caption),visible=VALUES(visible),enabled=VALUES(enabled),min_rank=VALUES(min_rank),order_num=VALUES(order_num),page_layout=VALUES(page_layout);")
[void]$sb.AppendLine('DELETE FROM catalog_items WHERE page_id BETWEEN 9967000 AND 9967099;')
[void]$sb.AppendLine('')

[void]$sb.AppendLine('INSERT INTO furniture (id,item_name,public_name,type,width,length,stack_height,can_stack,can_sit,is_walkable,sprite_id,allow_recycle,allow_trade,allow_marketplace_sell,allow_gift,allow_inventory_stack,interaction_type,behaviour_data,interaction_modes_count,vending_ids,height_adjustable,effect_id,wired_id,is_rare,clothing_id,extra_rot,allow_lay) VALUES')
$furnitureRows = @()
foreach ($item in $newEntries) {
    $entry = $item.Entry
    $className = [string]$entry.classname
    $lower = $className.ToLowerInvariant()
    $width = if ($entry.PSObject.Properties.Name -contains 'xdim') { [Math]::Max(1, [int]$entry.xdim) } else { 1 }
    $length = if ($entry.PSObject.Properties.Name -contains 'ydim') { [Math]::Max(1, [int]$entry.ydim) } else { 1 }
    $canSit = if ($lower -match 'chair|sofa|bench|stool|seat|ottoman') { 1 } else { 0 }
    $canLay = if ($lower -match 'bed') { 1 } else { 0 }
    $walkable = if ($lower -match 'floor|tile|rug|carpet|path|road') { 1 } else { 0 }
    $stackHeight = if ($walkable) { '0.1' } elseif ($canSit) { '1.0' } elseif ($canLay) { '0.5' } else { '1.0' }
    $interaction = if ($lower -match '(^|_)door|gate') { 'gate' } else { 'default' }
    $type = if ($item.Kind -eq 'wall') { 'i' } else { 's' }
    $furnitureRows += "($($item.Id),'$(Escape-Sql $className)','$(Escape-Sql ([string]$entry.name))','$type',$width,$length,$stackHeight,1,$canSit,$walkable,$($item.Id),'1','1','1','1','1','$interaction',0,1,'0','0',0,0,'0',0,'0',$canLay)"
}
[void]$sb.AppendLine(($furnitureRows -join ",`n") + "`nON DUPLICATE KEY UPDATE item_name=VALUES(item_name),public_name=VALUES(public_name),type=VALUES(type),width=VALUES(width),length=VALUES(length),stack_height=VALUES(stack_height),can_stack=VALUES(can_stack),can_sit=VALUES(can_sit),is_walkable=VALUES(is_walkable),sprite_id=VALUES(sprite_id),interaction_type=VALUES(interaction_type),allow_lay=VALUES(allow_lay);")
[void]$sb.AppendLine('')

$collectionPage = @{ 'gracie_24'=9967002; 'betty_24'=9967003; 'badrum_24'=9967004; 'Habblet'=9967005; 'Hellsinore'=9967005 }
$catalogRows = @()
foreach ($item in $newEntries) {
    $prefix = ($collectionPage.Keys | Where-Object { [string]$item.Entry.classname -like "$_*" } | Select-Object -First 1)
    $pageId = $collectionPage[$prefix]
    $name = Escape-Sql ([string]$item.Entry.name)
    $catalogRows += "($pageId,'$($item.Id)','$name',5,0,0,1,0,0,'1','', '',0,0)"
    $catalogRows += "(9967001,'$($item.Id)','$name',5,0,0,1,0,0,'1','', '',0,0)"
}
[void]$sb.AppendLine('INSERT INTO catalog_items (page_id,item_id,catalog_name,cost_credits,cost_pixels,cost_diamonds,amount,limited_sells,limited_stack,offer_active,extradata,badge,offer_id,points_type) VALUES')
[void]$sb.AppendLine(($catalogRows -join ",`n") + ';')
[void]$sb.AppendLine('')

$allIds = @($target.roomitemtypes.furnitype.id) + @($target.wallitemtypes.furnitype.id) | ForEach-Object { [uint64]$_ } | Sort-Object -Unique
[void]$sb.AppendLine('DROP TEMPORARY TABLE IF EXISTS paradise_catalog_assets;')
[void]$sb.AppendLine('CREATE TEMPORARY TABLE paradise_catalog_assets (id INT UNSIGNED NOT NULL PRIMARY KEY) ENGINE=MEMORY;')
for ($offset = 0; $offset -lt $allIds.Count; $offset += 500) {
    $last = [Math]::Min($offset + 499, $allIds.Count - 1)
    $values = @($allIds[$offset..$last] | ForEach-Object { "($_)" }) -join ','
    [void]$sb.AppendLine("INSERT IGNORE INTO paradise_catalog_assets (id) VALUES $values;")
}
[void]$sb.AppendLine('DROP TEMPORARY TABLE IF EXISTS paradise_catalog_source;')
[void]$sb.AppendLine(@'
CREATE TEMPORARY TABLE paradise_catalog_source (item_id INT UNSIGNED NOT NULL PRIMARY KEY, source_id INT UNSIGNED NOT NULL) ENGINE=MEMORY
SELECT CAST(ci.item_id AS UNSIGNED) item_id, MIN(ci.id) source_id
FROM catalog_items ci
INNER JOIN paradise_catalog_assets a ON a.id=CAST(ci.item_id AS UNSIGNED)
INNER JOIN furniture f ON f.id=a.id AND f.type IN ('s','i')
WHERE ci.offer_active='1' AND a.id NOT BETWEEN 1924000001 AND 1924999999
GROUP BY CAST(ci.item_id AS UNSIGNED);

DROP TEMPORARY TABLE IF EXISTS paradise_catalog_assignment;
CREATE TEMPORARY TABLE paradise_catalog_assignment (item_id INT UNSIGNED NOT NULL PRIMARY KEY, page_id INT NOT NULL) ENGINE=MEMORY
SELECT s.item_id,
CASE
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP '(^|[^a-z])(wf_|wired|trigger|condition|roller|teleport)' THEN 9967090
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'hospital|clinic|medic|doctor|health|ambulance|pharmacy|surgery|patient|dentist' THEN 9967060
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'police|prison|jail|security|cctv|court|judge|crime|detective' THEN 9967061
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'firefighter|fireman|rescue|hydrant|fire|flame|extinguisher' THEN 9967062
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'school|government|cityhall|city_hall|library|public_service' THEN 9967063
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'boat|ship|yacht|port|dock|pier|marina|harbour|harbor|lighthouse' THEN 9967023
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'train|metro|subway|airport|plane|railway' THEN 9967022
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'car_|vehicle|garage|taxi|bus_|bike|bicycle|moto|scooter|automobile|auto_|coche|vehiculo|truck|van_|jeep' THEN 9967021
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'road|street|traffic|parking|sidewalk|pavement|sign_' THEN 9967020
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'beach|ocean|sea_|coast|pool|water|river|pond|sand' THEN 9967031
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'farm|crop|tractor|barn|hay|agri|vegetable|fruit_tree' THEN 9967032
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'tree|plant|flower|bush|grass|rock|stone|garden|jungle|forest|nature|snow' THEN 9967030
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'door|window|gate|fence|railing|divider|partition|curtain|blind' THEN 9967013
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'stair|column|pillar|arch|beam|support|structure' THEN 9967014
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'floor|tile|roof|ceiling|carpet|rug' THEN 9967012
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'wall|facade|brick|panel' THEN 9967011
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'foundation|block|bc_|hfbld|hfdiy|construction' THEN 9967010
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'bed|bath|toilet|shower|sink|wardrobe|bedroom' THEN 9967042
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'lamp|light|candle|lantern|neon' THEN 9967043
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'kitchen|oven|fridge|stove|cook|dish|utensil|microwave' THEN 9967041
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'restaurant|cafe|coffee|diner|food|bakery|pizza|burger|bar_' THEN 9967051
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'shop|store|market|cashier|vending|mall' THEN 9967050
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'office|bank|desk|meeting|business|post_' THEN 9967052
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'hotel|spa|resort|lounge|holiday' THEN 9967053
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'sofa|chair|seat|stool|bench|table|cabinet|shelf|home' THEN 9967040
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'industrial|factory|warehouse|crate|pallet|tool|workbench|scaffold|cement|pipe' THEN 9967070
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'computer|laptop|phone|screen|monitor|server|tech|robot|camera|tv_|television|speaker|console' THEN 9967071
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'sport|game|music|disco|club|cinema|movie|stage|gym|football|basket|trophy|museum|art_|theatre' THEN 9967072
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'pet|horse|dog|cat_|kitten|puppy|animal|fish|bird' THEN 9967073
 WHEN LOWER(CONCAT(f.item_name,' ',f.public_name)) REGEXP 'xmas|hween|easter|valentine|newyear|winter|summer|autumn|spring|party|birthday' THEN 9967086
 WHEN LEFT(LOWER(f.item_name),1) BETWEEN 'a' AND 'd' THEN 9967080
 WHEN LEFT(LOWER(f.item_name),1) BETWEEN 'e' AND 'h' THEN 9967081
 WHEN LEFT(LOWER(f.item_name),1) BETWEEN 'i' AND 'l' THEN 9967082
 WHEN LEFT(LOWER(f.item_name),1) BETWEEN 'm' AND 'p' THEN 9967083
 WHEN LEFT(LOWER(f.item_name),1) BETWEEN 'q' AND 't' THEN 9967084
 ELSE 9967085 END page_id
FROM paradise_catalog_source s INNER JOIN furniture f ON f.id=s.item_id;

INSERT INTO catalog_items (page_id,item_id,catalog_name,cost_credits,cost_pixels,cost_diamonds,amount,limited_sells,limited_stack,offer_active,extradata,badge,offer_id,points_type)
SELECT a.page_id, ci.item_id,
       LEFT(CASE WHEN f.public_name<>'' THEN f.public_name WHEN ci.catalog_name<>'' THEN ci.catalog_name ELSE f.item_name END,100),
       LEAST(20,GREATEST(2,2+(f.width*f.length))),0,0,GREATEST(1,ci.amount),0,0,'1',ci.extradata,ci.badge,ci.offer_id,0
FROM paradise_catalog_assignment a
INNER JOIN paradise_catalog_source s ON s.item_id=a.item_id
INNER JOIN catalog_items ci ON ci.id=s.source_id
INNER JOIN furniture f ON f.id=a.item_id;

DROP TEMPORARY TABLE IF EXISTS paradise_catalog_assignment;
DROP TEMPORARY TABLE IF EXISTS paradise_catalog_source;
DROP TEMPORARY TABLE IF EXISTS paradise_catalog_assets;
COMMIT;
'@)

[IO.File]::WriteAllText($migrationPath, $sb.ToString(), [Text.UTF8Encoding]::new($false))
Write-Host "Catalogue genere : $migrationPath" -ForegroundColor Green
Write-Host "Collections 2024 : $($newEntries.Count) meubles, assets et icones copies." -ForegroundColor Green
Write-Host "FurnitureData : $($allIds.Count) identifiants clients autorises." -ForegroundColor Green
