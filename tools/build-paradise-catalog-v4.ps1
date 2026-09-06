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

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) { $RepositoryRoot = Split-Path -Parent $PSScriptRoot }
if ($Limit -lt 1000) { throw "Limit doit etre au minimum de 1000." }

$targetFurnitureData = Join-Path $RepositoryRoot "swf_pz\V5-0-2\gamedata\json\FurnitureData.json"
$targetFurnitureDir = Join-Path $RepositoryRoot "swf_pz\V5-0-2\furniture"
$targetIconDir = Join-Path $RepositoryRoot "swf_pz\V5-0-2\dcr\hof_furni\icon"
$sourceFurnitureData = Join-Path $HabboRpRoot "pack\cdn-react\gamedata\FurnitureData.json"
$sourceFurnitureDir = Join-Path $HabboRpRoot "pack\cdn-react\bundled\furniture"
$sourceIconDir = Join-Path $HabboRpRoot "pack\cdn-react\icons"
$migrationPath = Join-Path $RepositoryRoot "migrations\20260903_paradise_catalogue_extension_v4.sql"

foreach ($path in @($Mysql,$targetFurnitureData,$sourceFurnitureData,$sourceFurnitureDir,$sourceIconDir)) {
    if (-not (Test-Path -LiteralPath $path)) { throw "Ressource requise absente : $path" }
}

function Escape-Sql([AllowNull()][string]$Value) {
    if ($null -eq $Value) { return '' }
    return $Value.Replace('\','\\').Replace("'","''").Replace("`r",' ').Replace("`n",' ')
}

function Get-Category([string]$Name) {
    $n = $Name.ToLowerInvariant()

    # Evenements / saisons (FR, EN, NL et noms de packs custom connus)
    if ($n -match 'xmas|christmas|noel|noël|kerst|pasen|easter|hween|halloween|valentine|newyear|winter|summer|spring|autumn|fall_|snow|festive|ornament|ornement|wreath|couronne|pumpkin|citrouille|lantern|mask|butterbeer|hp_') { return 9967112 }

    # Loisirs, jeux, musique et objets ludiques
    if ($n -match 'falque_domino|diy_mahjong|yvess5_|circus|bpm|puzzle|puzzlebox|doll|dollpress|dress|sport|game|gaming|music|disco|cinema|stage|gym|football|trophy|pet|animal|dance|club|theatre|theater|basket|tennis|skate|ball|piano|guitar|drum|billiard|pooltable|foosball|dj_|arcade') { return 9967111 }

    # Commerce / restauration (inclut quelques termes portugais des packs custom)
    if ($n -match '(^|_)pops_|shop|store|market|restaurant|cafe|coffee|food|bar_|mall|vending|bakery|diner|kiosk|supermarket|boutique|cashier|checkout|register|counter|menu|pizza|burger|salon|caixa_registradora|torradeira|pipoca|iogurte|fogueiro|lixeira|bule') { return 9967107 }

    # Ville, services publics, securite, sante, transport et technologie
    if ($n -match 'police|prison|security|court|fire|rescue|army|military|cop_|jail|guard|swat|hospital|clinic|medic|medical|doctor|health|ambulance|pharmacy|nurse|surgery|dentist|firstaid|stretcher|wheelchair|xray|defib|(^|_)car(_|$)|vehicle|garage|taxi|(^|_)bus(_|$)|busstop|bike|bicycle|moto|scooter|train|metro|airport|plane|airplane|boat|ship|port|tram|rail|parking|petrol|fuel|traffic|office|bank|school|city|urban|hotel|apartment|public|government|station|reception|lobby|library|museum|streetlight|elevator|lift|locker|mail|queue|barrier|sign_|computer|laptop|phone|screen|monitor|server|tech|robot|camera|television|speaker|console|radio|tablet|keyboard|machine|device') { return 9967104 }

    # Nature / exterieur
    if ($n -match 'tree|plant|flower|fleur|fleurs|sakura|bush|grass|rock|garden|forest|farm|beach|water|pool|nature|outdoor|park|pond|river|mountain|sand|soil|wood_log|pot_|little_pot|japandi_beam|wood_log') { return 9967106 }

    # Maison / decoration
    if ($n -match 'sofa|chair|seat|stool|bench|table|cabinet|shelf|bed|nightstand|bath|toilet|shower|kitchen|lamp|rug|carpet|home|wardrobe|dresser|desk|couch|fridge|oven|sink|mirror|blanket|painting|candle|bol($|_)|bowl|poubelle|napkin|napkins|cushion|pillow|frame|vase|potjes') { return 9967103 }

    # Construction / architecture
    if ($n -match 'wall|floor|tile|roof|door|window|gate|fence|stair|column|pillar|block|build|construction|road|street|bridge|archi|brick|concrete|woodblock|voute|beam|ladder|plank|paal|pole') { return 9967102 }

    # Certains packs sont tres melanges : on ne force pas toute une collection
    # dans une categorie semantique fausse. Ils restent en Collections custom.
    return 9967113
}

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

$furnitureByName = @{}
foreach ($line in $rawRows) {
    $parts = $line -split "`t", -1
    if ($parts.Count -ge 27 -and -not $furnitureByName.ContainsKey($parts[1])) { $furnitureByName[$parts[1]] = $parts }
}

$target = Get-Content -LiteralPath $targetFurnitureData -Raw | ConvertFrom-Json
$source = Get-Content -LiteralPath $sourceFurnitureData -Raw | ConvertFrom-Json
$targetNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$targetIds = [Collections.Generic.HashSet[long]]::new()
foreach ($entry in @($target.roomitemtypes.furnitype)+@($target.wallitemtypes.furnitype)) {
    [void]$targetNames.Add([string]$entry.classname)
    [void]$targetIds.Add([long]$entry.id)
}

$sourceEntries = [Collections.Generic.List[object]]::new()
foreach ($entry in @($source.roomitemtypes.furnitype)) { $sourceEntries.Add([pscustomobject]@{Kind='room';Entry=$entry}) }
foreach ($entry in @($source.wallitemtypes.furnitype)) { $sourceEntries.Add([pscustomobject]@{Kind='wall';Entry=$entry}) }

$candidates = [Collections.Generic.List[object]]::new()
$candidateNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$candidateIds = [Collections.Generic.HashSet[long]]::new()
foreach ($item in $sourceEntries) {
    $className = [string]$item.Entry.classname
    $assetName = $className.Split('*')[0]
    if ($assetName -notmatch '^[A-Za-z0-9_.-]+$' -or $targetNames.Contains($className)) { continue }
    if (-not $furnitureByName.ContainsKey($className)) { continue }
    $db = $furnitureByName[$className]
    if ($targetIds.Contains([long]$db[0])) { continue }
    if (-not $candidateNames.Add($className) -or -not $candidateIds.Add([long]$db[0])) { continue }
    $assetPath = Join-Path $sourceFurnitureDir ($assetName + '.nitro')
    $iconPath = Join-Path $sourceIconDir ($assetName + '_icon.png')
    if (-not (Test-Path -LiteralPath $assetPath) -or -not (Test-Path -LiteralPath $iconPath)) { continue }

    $entryName = if ($null -ne $item.Entry.name) { [string]$item.Entry.name } else { '' }
    $entryDescription = if ($null -ne $item.Entry.description) { [string]$item.Entry.description } else { '' }
    $searchText = "$className $($db[2]) $entryName $entryDescription"
    $lower = $searchText.ToLowerInvariant()
    $score = 0
    if ($lower -match '(^|_)(20(2[0-6])|2[0-6])(_|$)|_24|_25|_26') { $score += 10000 }
    if ($lower -match 'police|hospital|clinic|medic|fire|rescue|office|bank|school|shop|market|restaurant|cafe|hotel|garage|vehicle|farm|computer|sport|music|game|kitchen|home') { $score += 1400 }
    if ($lower -match 'sofa|chair|table|cabinet|shelf|bed|bath|lamp|rug|plant|tree|water|pool|flower|fleur') { $score += 900 }
    if ($lower -match 'wall|floor|tile|roof|door|window|gate|fence|stair|column|pillar|block|build|road|bridge') { $score += 450 }
    $size = (Get-Item -LiteralPath $assetPath).Length
    $candidates.Add([pscustomobject]@{Kind=$item.Kind;Entry=$item.Entry;Db=$db;ClassName=$className;AssetName=$assetName;Score=$score;Size=$size;Category=(Get-Category $searchText)})
}

# Cibles de selection : les quotas favorisent les categories RP, tout en
# gardant une page Custom pour les packs communautaires reellement mixtes.
$quotas = [ordered]@{
    '9967102'=220
    '9967103'=360
    '9967104'=330
    '9967106'=220
    '9967107'=220
    '9967111'=250
    '9967112'=250
    '9967113'=150
}

$selectedList = [Collections.Generic.List[object]]::new()
$selectedIds = [Collections.Generic.HashSet[long]]::new()
$remainingByCategory = @{}
foreach ($category in $quotas.Keys) {
    $orderedCandidates = @($candidates | Where-Object Category -eq ([int]$category) | Sort-Object @{Expression='Score';Descending=$true},@{Expression='Size';Descending=$false},ClassName)
    $take = [Math]::Min([int]$quotas[$category],[Math]::Max(0,$Limit-$selectedList.Count))
    $taken = 0
    foreach ($candidate in @($orderedCandidates | Select-Object -First $take)) {
        if ($selectedIds.Add([long]$candidate.Db[0])) { $selectedList.Add($candidate); $taken++ }
    }
    $remainingByCategory[$category] = @($orderedCandidates | Select-Object -Skip $taken)
}

# Redistribue le reliquat sans laisser Collections custom absorber tout le reste.
$remainingIndex = @{}
foreach ($category in $quotas.Keys) { $remainingIndex[$category]=0 }
$redistributionOrder = @('9967103','9967104','9967107','9967111','9967106','9967102','9967112','9967113')
while ($selectedList.Count -lt $Limit) {
    $addedThisRound = 0
    foreach ($category in $redistributionOrder) {
        $queue = @($remainingByCategory[$category])
        $index = [int]$remainingIndex[$category]
        while ($index -lt $queue.Count) {
            $candidate = $queue[$index]
            $index++
            $remainingIndex[$category] = $index
            if ($selectedIds.Add([long]$candidate.Db[0])) {
                $selectedList.Add($candidate)
                $addedThisRound++
                break
            }
        }
        if ($selectedList.Count -eq $Limit) { break }
    }
    if ($addedThisRound -eq 0) { break }
}

$selected = @($selectedList)
if ($selected.Count -ne $Limit) { throw "Seulement $($selected.Count) meubles complets trouves sur $Limit demandes." }

New-Item -ItemType Directory -Path $targetFurnitureDir -Force | Out-Null
New-Item -ItemType Directory -Path $targetIconDir -Force | Out-Null

$repairedAssets=0; $repairedIcons=0
foreach ($entry in @($target.roomitemtypes.furnitype)+@($target.wallitemtypes.furnitype)) {
    $assetName = ([string]$entry.classname).Split('*')[0]
    if ($assetName -notmatch '^[A-Za-z0-9_.-]+$') { continue }
    $sourceAsset=Join-Path $sourceFurnitureDir ($assetName+'.nitro')
    $targetAsset=Join-Path $targetFurnitureDir ($assetName+'.nitro')
    if (-not (Test-Path -LiteralPath $targetAsset) -and (Test-Path -LiteralPath $sourceAsset)) { Copy-Item -LiteralPath $sourceAsset -Destination $targetAsset -Force; $repairedAssets++ }
    $sourceIcon=Join-Path $sourceIconDir ($assetName+'_icon.png')
    $targetIcon=Join-Path $targetIconDir ($assetName+'_icon.png')
    if (-not (Test-Path -LiteralPath $targetIcon) -and (Test-Path -LiteralPath $sourceIcon)) { Copy-Item -LiteralPath $sourceIcon -Destination $targetIcon -Force; $repairedIcons++ }
}

$newEntries=[Collections.Generic.List[object]]::new()
foreach ($item in $selected) {
    Copy-Item -LiteralPath (Join-Path $sourceFurnitureDir ($item.AssetName+'.nitro')) -Destination (Join-Path $targetFurnitureDir ($item.AssetName+'.nitro')) -Force
    Copy-Item -LiteralPath (Join-Path $sourceIconDir ($item.AssetName+'_icon.png')) -Destination (Join-Path $targetIconDir ($item.AssetName+'_icon.png')) -Force
    $entry=$item.Entry
    $entry.id=[long]$item.Db[0]; $entry.offerid=[long]$item.Db[0]
    if (-not [string]::IsNullOrWhiteSpace([string]$item.Db[2])) { $entry.name=[string]$item.Db[2] }
    if ([string]::IsNullOrWhiteSpace([string]$entry.description)) { $entry.description='Mobilier compatible ParadiseRP.' }
    $newEntries.Add([pscustomobject]@{Kind=$item.Kind;Entry=$entry;Db=$item.Db;AssetName=$item.AssetName;Category=$item.Category})
}

$target.roomitemtypes.furnitype=@($target.roomitemtypes.furnitype)+@($newEntries|Where-Object Kind -eq 'room'|ForEach-Object Entry)
$target.wallitemtypes.furnitype=@($target.wallitemtypes.furnitype)+@($newEntries|Where-Object Kind -eq 'wall'|ForEach-Object Entry)
[IO.File]::WriteAllText($targetFurnitureData,($target|ConvertTo-Json -Depth 20 -Compress),[Text.UTF8Encoding]::new($false))

$pages=@(
    @(9967100,9967000,'Extension 2000 meubles',1),
    @(9967102,9967100,'Construction et architecture',1),
    @(9967103,9967100,'Maison et decoration',2),
    @(9967104,9967100,'Ville services et transports',3),
    @(9967107,9967100,'Commerces et restauration',4),
    @(9967106,9967100,'Nature et exterieurs',5),
    @(9967111,9967100,'Loisirs jeux et musique',6),
    @(9967112,9967100,'Saisons et evenements',7),
    @(9967113,9967100,'Collections custom',8)
)

$sb=[Text.StringBuilder]::new()
[void]$sb.AppendLine('-- ParadiseRP catalogue extension v4 - 2000 meubles verifies')
[void]$sb.AppendLine('-- Chaque meuble possede une ligne serveur, un FurnitureData, un .nitro et une icone.')
[void]$sb.AppendLine('SET NAMES utf8mb4;')
[void]$sb.AppendLine('START TRANSACTION;')
[void]$sb.AppendLine('INSERT INTO catalog_pages (id,parent_id,caption,icon_image,visible,enabled,min_rank,min_vip,order_num,page_link,page_layout,page_strings_1,page_strings_2) VALUES')
$pageRows=foreach($p in $pages){"($($p[0]),$($p[1]),'$($p[2])',1,'1','1',1,0,$($p[3]),'','default_3x3','','')"}
[void]$sb.AppendLine(($pageRows -join ",`n")+"`nON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption=VALUES(caption),visible='1',enabled='1',order_num=VALUES(order_num);")
[void]$sb.AppendLine('DELETE FROM catalog_items WHERE page_id BETWEEN 9967100 AND 9967199;')
[void]$sb.AppendLine('DELETE FROM catalog_pages WHERE id BETWEEN 9967101 AND 9967199 AND id NOT IN (9967102,9967103,9967104,9967106,9967107,9967111,9967112,9967113);')
[void]$sb.AppendLine('')
[void]$sb.AppendLine('INSERT INTO furniture (id,item_name,public_name,type,width,length,stack_height,can_stack,can_sit,is_walkable,sprite_id,allow_recycle,allow_trade,allow_marketplace_sell,allow_gift,allow_inventory_stack,interaction_type,behaviour_data,interaction_modes_count,vending_ids,height_adjustable,effect_id,wired_id,is_rare,clothing_id,extra_rot,allow_lay) VALUES')
$furnitureRows=foreach($item in $newEntries){
    $d=$item.Db
    $values=@($d[0],"'$(Escape-Sql $d[1])'","'$(Escape-Sql $d[2])'","'$($d[3])'",$d[4],$d[5],$d[6],$d[7],$d[8],$d[9],$d[0],"'$($d[11])'","'$($d[12])'","'$($d[13])'","'$($d[14])'","'$($d[15])'","'$(Escape-Sql $d[16])'",$d[17],$d[18],"'$(Escape-Sql $d[19])'","'$(Escape-Sql $d[20])'",$d[21],$d[22],"'$($d[23])'",$d[24],"'$($d[25])'",$d[26])
    '('+($values -join ',')+')'
}
[void]$sb.AppendLine(($furnitureRows -join ",`n")+"`nON DUPLICATE KEY UPDATE item_name=VALUES(item_name),public_name=VALUES(public_name),type=VALUES(type),width=VALUES(width),length=VALUES(length),stack_height=VALUES(stack_height),can_stack=VALUES(can_stack),can_sit=VALUES(can_sit),is_walkable=VALUES(is_walkable),sprite_id=VALUES(sprite_id),interaction_type=VALUES(interaction_type),behaviour_data=VALUES(behaviour_data),interaction_modes_count=VALUES(interaction_modes_count),vending_ids=VALUES(vending_ids),allow_lay=VALUES(allow_lay);")
[void]$sb.AppendLine('')
[void]$sb.AppendLine('INSERT INTO catalog_items (page_id,item_id,catalog_name,cost_credits,cost_pixels,cost_diamonds,amount,limited_sells,limited_stack,offer_active,extradata,badge,offer_id,points_type) VALUES')
$offer=1950100000
$catalogRows=foreach($item in $newEntries){
    $d=$item.Db
    $price=[Math]::Min(25,[Math]::Max(3,2+([int]$d[4]*[int]$d[5])))
    $name=if([string]::IsNullOrWhiteSpace([string]$d[2])){[string]$d[1]}else{[string]$d[2]}
    $row="($($item.Category),'$($d[0])','$(Escape-Sql $name)',$price,0,0,1,0,0,'1','','',$offer,0)"
    $offer++
    $row
}
[void]$sb.AppendLine(($catalogRows -join ",`n")+';')
[void]$sb.AppendLine('COMMIT;')
[IO.File]::WriteAllText($migrationPath,$sb.ToString(),[Text.UTF8Encoding]::new($false))

$uniqueAssets=@($newEntries.AssetName|Sort-Object -Unique)
$totalBytes=($uniqueAssets|ForEach-Object{(Get-Item -LiteralPath (Join-Path $targetFurnitureDir ($_+'.nitro'))).Length}|Measure-Object -Sum).Sum
Write-Host "Catalogue v4 genere : $migrationPath" -ForegroundColor Green
Write-Host "Ajoutes : $($newEntries.Count) definitions, $($uniqueAssets.Count) assets, $([Math]::Round($totalBytes/1MB,1)) MiB." -ForegroundColor Green
Write-Host "Repares : $repairedAssets assets existants et $repairedIcons icones existantes." -ForegroundColor Green
Write-Host "Repartition finale :" -ForegroundColor Cyan
foreach($page in $pages|Where-Object{$_[0]-ne 9967100}){
    $count=@($newEntries|Where-Object Category -eq ([int]$page[0])).Count
    $available=@($candidates|Where-Object Category -eq ([int]$page[0])).Count
    $color=if($count -eq 0){'Red'}elseif($count -lt 75){'Yellow'}else{'Green'}
    Write-Host (" - {0}: {1} offres (sur {2} candidates)" -f $page[2],$count,$available) -ForegroundColor $color
}
