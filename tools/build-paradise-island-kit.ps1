[CmdletBinding()]
param(
    [string]$RepositoryRoot = "",
    [string]$Mysql = "C:\xampp\mysql\bin\mysql.exe",
    [string]$Database = "waveplus"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) { $RepositoryRoot = Split-Path -Parent $PSScriptRoot }

$fdPath = Join-Path $RepositoryRoot "swf_pz\V5-0-2\gamedata\json\FurnitureData.json"
$furnitureDir = Join-Path $RepositoryRoot "swf_pz\V5-0-2\furniture"
$iconDir = Join-Path $RepositoryRoot "swf_pz\V5-0-2\dcr\hof_furni\icon"
$modernPath = Join-Path $RepositoryRoot "migrations\20260904_paradise_island_builder_kit.sql"
$legacyPath = Join-Path $RepositoryRoot "migrations\20260904_paradise_island_builder_kit_legacy.sql"

function Escape-Sql([AllowNull()][string]$Value) {
    if ($null -eq $Value) { return '' }
    return $Value.Replace('\','\\').Replace("'","''").Replace("`r",' ').Replace("`n",' ')
}

function Get-IconName([string]$ClassName) {
    $parts = @($ClassName -split '\*',2)
    if ($parts.Count -gt 1) { return $parts[0] + '_' + $parts[1] + '_icon.png' }
    return $parts[0] + '_icon.png'
}

function Get-Category([string]$ClassName,[string]$Name) {
    $text = ($ClassName + ' ' + $Name).ToLowerInvariant()
    if ($ClassName -match '^bc_block_1\*') { return 'Grands cubes de couleur' }
    if ($ClassName -match '^bc_block_0\*|^bc_block_small\*') { return 'Petits cubes de couleur' }
    if ($text -match 'water|sea|ocean|river|pool|pond|wave|beach|sand|praia|agua|eau|plage|dock|harbo|port_|pier|jetty') { return 'Eau plages ports' }
    if ($text -match 'road|street|route|traffic|asphalt|pavement|sidewalk|crosswalk|parking|highway|rail_|tram|bridge|pont') { return 'Routes ponts ville' }
    if ($text -match 'door|gate|window|porte|porta|janela|fenetre|archway|elevator|ascenseur') { return 'Portes fenetres passages' }
    if ($text -match 'fence|barrier|railings|railing|barriere|cloture|cerca|stair|ladder|step|escalier|escada|ramp') { return 'Escaliers barrieres clotures' }
    if ($text -match 'wall|mur|parede|brick|brique|tijolo|facade|partition|divider|column|pillar|pilier|colonne|roof|toit|telhado') { return 'Murs facades toitures' }
    if ($text -match 'floor|tile|dalle|piso|(^|_)sol(_|$)|carpet|rug|tapis|marble|concrete|cement|gravel|gravier|deck') { return 'Sols dalles terrains' }
    if ($text -match 'tree|plant|flower|bush|grass|garden|arbre|arvore|plante|planta|fleur|flor|forest|jungle|palm|hedge|haie|veget') { return 'Arbres plantes jardins' }
    if ($text -match 'rock|stone|soil|terrain|mountain|hill|cliff|cave|rocher|pierre|terre|moss|log|wood|bois') { return 'Roches reliefs nature' }
    if ($text -match 'lamp|light|lantern|torch|neon|sign|arrow|panneau|fleche|signal|trafficlight|streetlight|lumiere|projector') { return 'Eclairage panneaux urbains' }
    if ($text -match 'industrial|warehouse|factory|metal|steel|beam|pipe|crate|container|generator|machine|workshop|garage|chantier|construction') { return 'Industrie chantier garage' }
    return 'Formes structures custom'
}

function Get-DisplayName([object]$Item) {
    $special = @{
        '5480' = 'Grand Cube noir'
        '5466' = 'Petit Cube noir'
        '996661582' = 'Cube noir compact'
        '987654444' = 'Bloc noir plat'
    }
    $key = [string]$Item.Db[0]
    if ($special.ContainsKey($key)) { return $special[$key] }
    $name = [string]$Item.Db[2]
    if ([string]::IsNullOrWhiteSpace($name) -or $name -match '_name$') { $name = [string]$Item.ClassName }
    return (($name -replace '\*',' variante ' -replace '_',' ') -replace '\s+',' ').Trim()
}

$pages = [ordered]@{
    'Grands cubes de couleur'       = 9967301
    'Petits cubes de couleur'       = 9967302
    'Formes structures custom'      = 9967303
    'Murs facades toitures'         = 9967304
    'Sols dalles terrains'          = 9967305
    'Routes ponts ville'            = 9967306
    'Eau plages ports'              = 9967307
    'Roches reliefs nature'         = 9967308
    'Arbres plantes jardins'        = 9967309
    'Portes fenetres passages'      = 9967310
    'Escaliers barrieres clotures'  = 9967311
    'Eclairage panneaux urbains'    = 9967312
    'Industrie chantier garage'     = 9967313
}
$rootPage = 9967300

# La source historique associait deux fois la variante 14 et supprimait le vrai
# grand cube noir (variante 13). Faire un remplacement textuel ciblé conserve
# l'encodage historique et évite de gonfler le JSON de 35 Mo à plus de 100 Mo.
$fdRaw = [IO.File]::ReadAllText($fdPath,[Text.UTF8Encoding]::new($false))
$wrongBlack = '"id":5480,"classname":"bc_block_1*14"'
$rightBlack = '"id":5480,"classname":"bc_block_1*13"'
if ($fdRaw.Contains($wrongBlack)) {
    $fdRaw = $fdRaw.Replace($wrongBlack,$rightBlack)
    [IO.File]::WriteAllText($fdPath,$fdRaw,[Text.UTF8Encoding]::new($false))
}
$fd = $fdRaw | ConvertFrom-Json

$fdById = @{}
foreach ($entry in @($fd.roomitemtypes.furnitype) + @($fd.wallitemtypes.furnitype)) {
    $key = [string]$entry.id
    if (-not $fdById.ContainsKey($key)) { $fdById[$key] = $entry }
}

$query = @"
SELECT id,item_name,public_name,type,width,length,stack_height,can_stack,can_sit,
       COALESCE(is_walkable,0),COALESCE(sprite_id,0),allow_recycle,allow_trade,
       allow_marketplace_sell,allow_gift,allow_inventory_stack,interaction_type,
       behaviour_data,interaction_modes_count,vending_ids,height_adjustable,effect_id,
       wired_id,is_rare,clothing_id,extra_rot,COALESCE(allow_lay,0)
FROM furniture
WHERE type IN ('s','i') AND item_name<>'' AND LOWER(CONCAT(item_name,' ',public_name)) REGEXP
'block|bloc|cube|wall|mur|floor|tile|dalle|road|street|asphalt|pavement|sidewalk|water|sea|ocean|sand|beach|grass|soil|terrain|rock|stone|brick|marble|wood|metal|glass|fence|barrier|gate|door|window|stair|ladder|column|pillar|roof|bridge|rail|tree|plant|flower|bush|garden|park|outdoor|lamp|light|sign|arrow|traffic|industrial|warehouse|harbor|port|dock|boat|construction|builder|cement|concrete|gravel|gravier|route|sol|piso|parede|porta|janela|escada|arvore|planta|flor|praia|agua';
"@
$raw = & $Mysql -u root -N -B --raw $Database -e $query
if ($LASTEXITCODE -ne 0) { throw "Lecture MySQL impossible dans $Database." }

$selected = [Collections.Generic.List[object]]::new()
$seen = [Collections.Generic.HashSet[long]]::new()
foreach ($line in $raw) {
    $db = @($line -split "`t",-1)
    if ($db.Count -lt 27) { continue }
    $id = [long]$db[0]
    if (-not $seen.Add($id) -or -not $fdById.ContainsKey([string]$id)) { continue }
    $entry = $fdById[[string]$id]
    $className = [string]$entry.classname
    if ([string]::IsNullOrWhiteSpace($className)) { continue }
    $parts = @($className -split '\*',2)
    $assetBase = $parts[0]
    if ($assetBase -notmatch '^[A-Za-z0-9_.-]+$') { continue }
    if (-not (Test-Path -LiteralPath (Join-Path $furnitureDir ($assetBase + '.nitro')))) { continue }
    if (-not (Test-Path -LiteralPath (Join-Path $iconDir (Get-IconName $className)))) { continue }
    $display = if ([string]::IsNullOrWhiteSpace([string]$db[2])) { $className } else { [string]$db[2] }
    $selected.Add([pscustomobject]@{ Db=$db; ClassName=$className; Category=(Get-Category $className $display) })
}

$selected = @($selected | Sort-Object @{Expression={if($_.Db[0] -in @('5480','5466','996661582','987654444')){0}else{1}}},Category,ClassName)
if ($selected.Count -lt 1000) { throw "Audit insuffisant : seulement $($selected.Count) mobis complets." }

function Get-FurnitureRows {
    foreach ($item in $selected) {
        $d = $item.Db
        $values = @($d[0],"'$(Escape-Sql $d[1])'","'$(Escape-Sql $d[2])'","'$($d[3])'",$d[4],$d[5],$d[6],$d[7],$d[8],$d[9],$d[0],"'$($d[11])'","'$($d[12])'","'$($d[13])'","'$($d[14])'","'$($d[15])'","'$(Escape-Sql $d[16])'",$d[17],$d[18],"'$(Escape-Sql $d[19])'","'$(Escape-Sql $d[20])'",$d[21],$d[22],"'$($d[23])'",$d[24],"'$($d[25])'",$d[26])
        '(' + ($values -join ',') + ')'
    }
}

function Add-FurnitureSql([Text.StringBuilder]$Builder) {
    $rows = @(Get-FurnitureRows)
    $header = 'INSERT INTO furniture (id,item_name,public_name,type,width,length,stack_height,can_stack,can_sit,is_walkable,sprite_id,allow_recycle,allow_trade,allow_marketplace_sell,allow_gift,allow_inventory_stack,interaction_type,behaviour_data,interaction_modes_count,vending_ids,height_adjustable,effect_id,wired_id,is_rare,clothing_id,extra_rot,allow_lay) VALUES'
    $update = 'ON DUPLICATE KEY UPDATE item_name=VALUES(item_name),public_name=VALUES(public_name),type=VALUES(type),width=VALUES(width),length=VALUES(length),stack_height=VALUES(stack_height),can_stack=VALUES(can_stack),can_sit=VALUES(can_sit),is_walkable=VALUES(is_walkable),sprite_id=VALUES(sprite_id),interaction_type=VALUES(interaction_type),behaviour_data=VALUES(behaviour_data),interaction_modes_count=VALUES(interaction_modes_count),vending_ids=VALUES(vending_ids),allow_lay=VALUES(allow_lay);'
    for ($offset=0; $offset -lt $rows.Count; $offset+=200) {
        $last = [Math]::Min($offset+199,$rows.Count-1)
        [void]$Builder.AppendLine($header)
        [void]$Builder.AppendLine((@($rows[$offset..$last]) -join ",`n") + "`n$update")
    }
}

function Build-Migration([bool]$Legacy,[string]$Destination) {
    $sb = [Text.StringBuilder]::new()
    [void]$sb.AppendLine('-- ParadiseRP - kit massif pour construire une ile complete')
    [void]$sb.AppendLine('SET NAMES utf8mb4;')
    [void]$sb.AppendLine('START TRANSACTION;')
    $pageRows = [Collections.Generic.List[string]]::new()
    $pageRows.Add($(if($Legacy){"($rootPage,9967200,'Kit construction ile','Kit construction ile','default_3x3',1,1,1,30,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')"}else{"($rootPage,9967200,'Kit construction ile',1,'1','1',1,0,30,'','default_3x3','','')"}))
    $order = 1
    foreach ($kv in $pages.GetEnumerator()) {
        $cap = Escape-Sql $kv.Key
        $pageRows.Add($(if($Legacy){"($($kv.Value),$rootPage,'$cap','$cap','default_3x3',1,1,1,$order,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')"}else{"($($kv.Value),$rootPage,'$cap',1,'1','1',1,0,$order,'','default_3x3','','')"}))
        $order++
    }
    if ($Legacy) {
        [void]$sb.AppendLine('INSERT INTO catalog_pages (id,parent_id,caption_save,caption,page_layout,icon_color,icon_image,min_rank,order_num,visible,enabled,club_only,vip_only,page_headline,page_teaser,page_special,page_text1,page_text2,page_text_details,page_text_teaser,room_id,includes) VALUES')
        [void]$sb.AppendLine(($pageRows -join ",`n") + "`nON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption_save=VALUES(caption_save),caption=VALUES(caption),order_num=VALUES(order_num),visible='1',enabled='1';")
    } else {
        [void]$sb.AppendLine('INSERT INTO catalog_pages (id,parent_id,caption,icon_image,visible,enabled,min_rank,min_vip,order_num,page_link,page_layout,page_strings_1,page_strings_2) VALUES')
        [void]$sb.AppendLine(($pageRows -join ",`n") + "`nON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption=VALUES(caption),order_num=VALUES(order_num),visible='1',enabled='1';")
    }
    [void]$sb.AppendLine('DELETE FROM catalog_items WHERE page_id BETWEEN 9967300 AND 9967399;')
    # Supprime l'ancien faux "bloc noir" dont aucun asset Nitro n'existe.
    [void]$sb.AppendLine($(if($Legacy){"DELETE FROM catalog_items WHERE item_ids='2678830';"}else{"DELETE FROM catalog_items WHERE item_id='2678830';"}))
    Add-FurnitureSql $sb
    $rows = [Collections.Generic.List[string]]::new()
    $orderByPage = @{}
    $offer = 1950400000
    foreach ($item in $selected) {
        $pageId = [int]$pages[$item.Category]
        if (-not $orderByPage.ContainsKey($pageId)) { $orderByPage[$pageId] = 0 }
        $orderByPage[$pageId]++
        $name = Escape-Sql (Get-DisplayName $item)
        if ($Legacy) { $rows.Add("('$($item.Db[0])',$pageId,'$name',3,0,0,1,0,0,$($orderByPage[$pageId]),$offer,0,'','1','0')") }
        else { $rows.Add("($pageId,'$($item.Db[0])','$name',3,0,0,1,0,0,'1','','',$offer,0)") }
        $offer++
    }
    $header = if($Legacy){'INSERT INTO catalog_items (item_ids,page_id,catalog_name,cost_credits,cost_points,points_type,amount,limited_stack,limited_sells,order_number,offer_id,song_id,extradata,have_offer,club_only) VALUES'}else{'INSERT INTO catalog_items (page_id,item_id,catalog_name,cost_credits,cost_pixels,cost_diamonds,amount,limited_sells,limited_stack,offer_active,extradata,badge,offer_id,points_type) VALUES'}
    for ($offset=0; $offset -lt $rows.Count; $offset+=300) {
        $last = [Math]::Min($offset+299,$rows.Count-1)
        [void]$sb.AppendLine($header)
        [void]$sb.AppendLine((@($rows[$offset..$last]) -join ",`n") + ';')
    }
    [void]$sb.AppendLine('COMMIT;')
    [IO.File]::WriteAllText($Destination,$sb.ToString(),[Text.UTF8Encoding]::new($false))
}

Build-Migration $false $modernPath
Build-Migration $true $legacyPath
Write-Host "Mobis de construction complets : $($selected.Count)" -ForegroundColor Green
$selected | Group-Object Category | Sort-Object Name | ForEach-Object { Write-Host " - $($_.Name) : $($_.Count)" }
Write-Host "Grand Cube noir restaure : ID 5480 / bc_block_1*13" -ForegroundColor Green
Write-Host "Migrations : $modernPath et $legacyPath" -ForegroundColor Green
