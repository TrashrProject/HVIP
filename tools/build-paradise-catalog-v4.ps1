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
if ($Limit -lt 1000) { throw "Limit doit etre au minimum de 1000." }

$targetFurnitureData = Join-Path $RepositoryRoot "swf_pz\V5-0-2\gamedata\json\FurnitureData.json"
$targetFurnitureDir = Join-Path $RepositoryRoot "swf_pz\V5-0-2\furniture"
$targetIconDir = Join-Path $RepositoryRoot "swf_pz\V5-0-2\dcr\hof_furni\icon"
$sourceFurnitureData = Join-Path $HabboRpRoot "pack\cdn-react\gamedata\FurnitureData.json"
$sourceFurnitureDir = Join-Path $HabboRpRoot "pack\cdn-react\bundled\furniture"
$sourceIconDir = Join-Path $HabboRpRoot "pack\cdn-react\icons"
$migrationPath = Join-Path $RepositoryRoot "migrations\20260903_paradise_catalogue_extension_v4.sql"

foreach ($path in @($Mysql, $targetFurnitureData, $sourceFurnitureData, $sourceFurnitureDir, $sourceIconDir)) {
    if (-not (Test-Path -LiteralPath $path)) { throw "Ressource requise absente : $path" }
}

function Escape-Sql([AllowNull()][string]$Value) {
    if ($null -eq $Value) { return '' }
    return $Value.Replace('\', '\\').Replace("'", "''").Replace("`r", ' ').Replace("`n", ' ')
}

function Get-Category([string]$Name) {
    $n = $Name.ToLowerInvariant()

    # Le pack source contient tres peu de meubles explicitement nommes police,
    # hopital, transport, commerce, etc. Les conserver comme pages separees
    # produisait des sous-pages de 1 a 50 offres. On les regroupe donc dans une
    # seule famille RP urbaine, plus utile et jamais artificiellement vide.
    if ($n -match 'police|prison|security|court|fire|rescue|army|military|cop_|jail|guard|swat|hospital|clinic|medic|medical|doctor|health|ambulance|pharmacy|nurse|surgery|dentist|firstaid|stretcher|wheelchair|xray|defib') { return 9967104 }
    if ($n -match '(^|_)car(_|$)|vehicle|garage|taxi|(^|_)bus(_|$)|busstop|bike|bicycle|moto|scooter|train|metro|airport|plane|airplane|boat|ship|port|tram|rail|parking|petrol|fuel|traffic') { return 9967104 }
    if ($n -match 'shop|store|market|restaurant|cafe|coffee|food|bar_|mall|vending|bakery|diner|kiosk|supermarket|boutique|cashier|checkout|register|counter|menu|pizza|burger|salon') { return 9967104 }
    if ($n -match 'computer|laptop|phone|screen|monitor|server|tech|robot|camera|television|speaker|console|arcade|radio|tablet|keyboard|machine|device') { return 9967104 }
    if ($n -match 'sport|game|music|disco|cinema|stage|gym|football|trophy|art_|pet|animal|dance|club|theatre|theater|basket|tennis|skate|ball|piano|guitar|drum|billiard|pooltable|foosball|dj_') { return 9967104 }
    if ($n -match 'office|bank|school|city|urban|hotel|apartment|public|government|station|reception|lobby|library|museum|streetlight|elevator|lift|locker|mail|queue|barrier|sign_') { return 9967104 }

    if ($n -match 'xmas|christmas|hween|halloween|easter|valentine|newyear|winter|summer|party|birthday|spring|autumn|fall_|snow|festive') { return 9967112 }
    if ($n -match 'tree|plant|flower|bush|grass|rock|garden|forest|farm|beach|water|pool|nature|outdoor|park|pond|river|mountain|sand|soil') { return 9967106 }
    if ($n -match 'sofa|chair|seat|stool|bench|table|cabinet|shelf|bed|bath|toilet|shower|kitchen|lamp|rug|carpet|home|wardrobe|dresser|desk|couch|fridge|oven|sink|mirror') { return 9967103 }
    if ($n -match 'wall|floor|tile|roof|door|window|gate|fence|stair|column|pillar|block|build|construction|road|street|bridge|archi|brick|concrete|woodblock') { return 9967102 }
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
    if ($parts.Count -ge 27 -and -not $furnitureByName.ContainsKey($parts[1])) {
        $furnitureByName[$parts[1]] = $parts
    }
}

$target = Get-Content -LiteralPath $targetFurnitureData -Raw | ConvertFrom-Json
$source = Get-Content -LiteralPath $sourceFurnitureData -Raw | ConvertFrom-Json
$targetNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$targetIds = [Collections.Generic.HashSet[long]]::new()
foreach ($entry in @($target.roomitemtypes.furnitype) + @($target.wallitemtypes.furnitype)) {
    [void]$targetNames.Add([string]$entry.classname)
    [void]$targetIds.Add([long]$entry.id)
}

$sourceEntries = [Collections.Generic.List[object]]::new()
foreach ($entry in @($source.roomitemtypes.furnitype)) { $sourceEntries.Add([pscustomobject]@{ Kind='room'; Entry=$entry }) }
foreach ($entry in @($source.wallitemtypes.furnitype)) { $sourceEntries.Add([pscustomobject]@{ Kind='wall'; Entry=$entry }) }

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
    if ($lower -match 'police|hospital|clinic|medic|fire|rescue|office|bank|school|shop|market|restaurant|cafe|hotel|garage|vehicle|farm|computer|sport|music') { $score += 1400 }
    if ($lower -match 'sofa|chair|table|cabinet|shelf|bed|bath|kitchen|lamp|rug|plant|tree|water|pool') { $score += 900 }
    if ($lower -match 'wall|floor|tile|roof|door|window|gate|fence|stair|column|pillar|block|build|construction|road|bridge') { $score += 450 }
    $size = (Get-Item -LiteralPath $assetPath).Length
    $candidates.Add([pscustomobject]@{ Kind=$item.Kind; Entry=$item.Entry; Db=$db; ClassName=$className; AssetName=$assetName; Score=$score; Size=$size; Category=(Get-Category $searchText) })
}

# Six familles robustes, adaptees a ce que le pack contient reellement.
# Le but est d'eviter les pages artificielles de 0, 1 ou 2 meubles.
$quotas = [ordered]@{
    '9967102'=320
    '9967103'=430
    '9967104'=280
    '9967106'=300
    '9967112'=300
    '9967113'=370
}

$selectedList = [Collections.Generic.List[object]]::new()
$selectedIds = [Collections.Generic.HashSet[long]]::new()
$remainingByCategory = @{}

foreach ($category in $quotas.Keys) {
    $orderedCandidates = @($candidates | Where-Object Category -eq ([int]$category) | Sort-Object @{Expression='Score';Descending=$true}, @{Expression='Size';Descending=$false}, ClassName)
    $take = [Math]::Min([int]$quotas[$category], [Math]::Max(0, $Limit - $selectedList.Count))
    $taken = 0
    foreach ($candidate in @($orderedCandidates | Select-Object -First $take)) {
        if ($selectedIds.Add([long]$candidate.Db[0])) {
            $selectedList.Add($candidate)
            $taken++
        }
    }
    $remainingByCategory[$category] = @($orderedCandidates | Select-Object -Skip $taken)
}

# Les quotas sont des cibles, pas des limites strictes. Si une famille manque
# de candidats, le reliquat est redistribue de facon reguliere entre les autres.
$remainingIndex = @{}
foreach ($category in $quotas.Keys) { $remainingIndex[$category] = 0 }
while ($selectedList.Count -lt $Limit) {
    $addedThisRound = 0
    foreach ($category in $quotas.Keys) {
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

# Repare aussi les fichiers physiques recuperables des definitions deja presentes.
$repairedAssets = 0
$repairedIcons = 0
foreach ($entry in @($target.roomitemtypes.furnitype) + @($target.wallitemtypes.furnitype)) {
    $assetName = ([string]$entry.classname).Split('*')[0]
    if ($assetName -notmatch '^[A-Za-z0-9_.-]+$') { continue }
    $sourceAsset = Join-Path $sourceFurnitureDir ($assetName + '.nitro')
    $targetAsset = Join-Path $targetFurnitureDir ($assetName + '.nitro')
    if (-not (Test-Path -LiteralPath $targetAsset) -and (Test-Path -LiteralPath $sourceAsset)) {
        Copy-Item -LiteralPath $sourceAsset -Destination $targetAsset -Force
        $repairedAssets++
    }
    $sourceIcon = Join-Path $sourceIconDir ($assetName + '_icon.png')
    $targetIcon = Join-Path $targetIconDir ($assetName + '_icon.png')
    if (-not (Test-Path -LiteralPath $targetIcon) -and (Test-Path -LiteralPath $sourceIcon)) {
        Copy-Item -LiteralPath $sourceIcon -Destination $targetIcon -Force
        $repairedIcons++
    }
}

$newEntries = [Collections.Generic.List[object]]::new()
foreach ($item in $selected) {
    $assetPath = Join-Path $sourceFurnitureDir ($item.AssetName + '.nitro')
    $iconPath = Join-Path $sourceIconDir ($item.AssetName + '_icon.png')
    Copy-Item -LiteralPath $assetPath -Destination (Join-Path $targetFurnitureDir ($item.AssetName + '.nitro')) -Force
    Copy-Item -LiteralPath $iconPath -Destination (Join-Path $targetIconDir ($item.AssetName + '_icon.png')) -Force

    $entry = $item.Entry
    $entry.id = [long]$item.Db[0]
    $entry.offerid = [long]$item.Db[0]
    if (-not [string]::IsNullOrWhiteSpace([string]$item.Db[2])) { $entry.name = [string]$item.Db[2] }
    if ([string]::IsNullOrWhiteSpace([string]$entry.description)) { $entry.description = 'Mobilier compatible ParadiseRP.' }
    $newEntries.Add([pscustomobject]@{ Kind=$item.Kind; Entry=$entry; Db=$item.Db; AssetName=$item.AssetName; Category=$item.Category })
}

$target.roomitemtypes.furnitype = @($target.roomitemtypes.furnitype) + @($newEntries | Where-Object Kind -eq 'room' | ForEach-Object Entry)
$target.wallitemtypes.furnitype = @($target.wallitemtypes.furnitype) + @($newEntries | Where-Object Kind -eq 'wall' | ForEach-Object Entry)
$json = $target | ConvertTo-Json -Depth 20 -Compress
[IO.File]::WriteAllText($targetFurnitureData, $json, [Text.UTF8Encoding]::new($false))

$pages = @(
    @(9967100,9967000,'Extension 2000 meubles',1),
    @(9967102,9967100,'Construction et routes',1),
    @(9967103,9967100,'Maison et decoration',2),
    @(9967104,9967100,'Vie urbaine, services et loisirs',3),
    @(9967106,9967100,'Nature et exterieurs',4),
    @(9967112,9967100,'Saisons et evenements',5),
    @(9967113,9967100,'Collections diverses',6)
)

$sb = [Text.StringBuilder]::new()
[void]$sb.AppendLine('-- ParadiseRP catalogue extension v4 - 2000 meubles verifies')
[void]$sb.AppendLine('-- Chaque meuble possede une ligne serveur, un FurnitureData, un .nitro et une icone.')
[void]$sb.AppendLine('SET NAMES utf8mb4;')
[void]$sb.AppendLine('START TRANSACTION;')
[void]$sb.AppendLine('INSERT INTO catalog_pages (id,parent_id,caption,icon_image,visible,enabled,min_rank,min_vip,order_num,page_link,page_layout,page_strings_1,page_strings_2) VALUES')
$pageRows = foreach ($p in $pages) { "($($p[0]),$($p[1]),'$($p[2])',1,'1','1',1,0,$($p[3]),'','default_3x3','','')" }
[void]$sb.AppendLine(($pageRows -join ",`n") + "`nON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption=VALUES(caption),visible='1',enabled='1',order_num=VALUES(order_num);")
[void]$sb.AppendLine('DELETE FROM catalog_items WHERE page_id BETWEEN 9967100 AND 9967199;')
[void]$sb.AppendLine('DELETE FROM catalog_pages WHERE id BETWEEN 9967101 AND 9967199 AND id NOT IN (9967102,9967103,9967104,9967106,9967112,9967113);')
[void]$sb.AppendLine('')
[void]$sb.AppendLine('INSERT INTO furniture (id,item_name,public_name,type,width,length,stack_height,can_stack,can_sit,is_walkable,sprite_id,allow_recycle,allow_trade,allow_marketplace_sell,allow_gift,allow_inventory_stack,interaction_type,behaviour_data,interaction_modes_count,vending_ids,height_adjustable,effect_id,wired_id,is_rare,clothing_id,extra_rot,allow_lay) VALUES')
$furnitureRows = foreach ($item in $newEntries) {
    $d = $item.Db
    $values = @($d[0],"'$(Escape-Sql $d[1])'","'$(Escape-Sql $d[2])'","'$($d[3])'",$d[4],$d[5],$d[6],$d[7],$d[8],$d[9],$d[10],"'$($d[11])'","'$($d[12])'","'$($d[13])'","'$($d[14])'","'$($d[15])'","'$(Escape-Sql $d[16])'",$d[17],$d[18],"'$(Escape-Sql $d[19])'","'$(Escape-Sql $d[20])'",$d[21],$d[22],"'$($d[23])'",$d[24],"'$($d[25])'",$d[26])
    '(' + ($values -join ',') + ')'
}
[void]$sb.AppendLine(($furnitureRows -join ",`n") + "`nON DUPLICATE KEY UPDATE item_name=VALUES(item_name),public_name=VALUES(public_name),type=VALUES(type),width=VALUES(width),length=VALUES(length),stack_height=VALUES(stack_height),can_stack=VALUES(can_stack),can_sit=VALUES(can_sit),is_walkable=VALUES(is_walkable),sprite_id=VALUES(sprite_id),interaction_type=VALUES(interaction_type),behaviour_data=VALUES(behaviour_data),interaction_modes_count=VALUES(interaction_modes_count),vending_ids=VALUES(vending_ids),allow_lay=VALUES(allow_lay);")
[void]$sb.AppendLine('')
[void]$sb.AppendLine('INSERT INTO catalog_items (page_id,item_id,catalog_name,cost_credits,cost_pixels,cost_diamonds,amount,limited_sells,limited_stack,offer_active,extradata,badge,offer_id,points_type) VALUES')
$offer = 1950100000
$catalogRows = foreach ($item in $newEntries) {
    $d = $item.Db
    $price = [Math]::Min(25, [Math]::Max(3, 2 + ([int]$d[4] * [int]$d[5])))
    $name = if ([string]::IsNullOrWhiteSpace([string]$d[2])) { [string]$d[1] } else { [string]$d[2] }
    $row = "($($item.Category),'$($d[0])','$(Escape-Sql $name)',$price,0,0,1,0,0,'1','','',$offer,0)"
    $offer++
    $row
}
[void]$sb.AppendLine(($catalogRows -join ",`n") + ';')
[void]$sb.AppendLine('COMMIT;')
[IO.File]::WriteAllText($migrationPath, $sb.ToString(), [Text.UTF8Encoding]::new($false))

$uniqueAssets = @($newEntries.AssetName | Sort-Object -Unique)
$totalBytes = ($uniqueAssets | ForEach-Object { (Get-Item -LiteralPath (Join-Path $targetFurnitureDir ($_ + '.nitro'))).Length } | Measure-Object -Sum).Sum
Write-Host "Catalogue v4 genere : $migrationPath" -ForegroundColor Green
Write-Host "Ajoutes : $($newEntries.Count) definitions, $($uniqueAssets.Count) assets, $([Math]::Round($totalBytes / 1MB, 1)) MiB." -ForegroundColor Green
Write-Host "Repares : $repairedAssets assets existants et $repairedIcons icones existantes." -ForegroundColor Green
Write-Host "Repartition finale :" -ForegroundColor Cyan
foreach ($page in $pages | Where-Object { $_[0] -ne 9967100 }) {
    $count = @($newEntries | Where-Object Category -eq ([int]$page[0])).Count
    $available = @($candidates | Where-Object Category -eq ([int]$page[0])).Count
    $color = if ($count -eq 0) { 'Red' } elseif ($count -lt 75) { 'Yellow' } else { 'Green' }
    Write-Host (" - {0}: {1} offres (sur {2} candidates)" -f $page[2], $count, $available) -ForegroundColor $color
}
