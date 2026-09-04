[CmdletBinding()]
param([string]$RepositoryRoot = "")

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) { $RepositoryRoot = Split-Path -Parent $PSScriptRoot }

$sqlPath = Join-Path $RepositoryRoot 'migrations\20260903_paradise_catalogue_mass_habborpbr.sql'
$fdPath = Join-Path $RepositoryRoot 'swf_pz\V5-0-2\gamedata\json\FurnitureData.json'
$sql = [IO.File]::ReadAllText($sqlPath)

$pageRows = @(
    "(9967200,9967000,'Catalogue massif HabboRPbr',1,'1','1',1,0,90,'','default_3x3','','')",
    "(9967201,9967200,'Construction et architecture',1,'1','1',1,0,1,'','default_3x3','','')",
    "(9967202,9967200,'Maison et decoration',1,'1','1',1,0,2,'','default_3x3','','')",
    "(9967203,9967200,'Ville et services',1,'1','1',1,0,3,'','default_3x3','','')",
    "(9967204,9967200,'Commerces et restauration',1,'1','1',1,0,4,'','default_3x3','','')",
    "(9967205,9967200,'Nature et exterieurs',1,'1','1',1,0,5,'','default_3x3','','')",
    "(9967206,9967200,'Jeux loisirs et musique',1,'1','1',1,0,6,'','default_3x3','','')",
    "(9967207,9967200,'Saisons et evenements',1,'1','1',1,0,7,'','default_3x3','','')",
    "(9967208,9967200,'Medical et sante',1,'1','1',1,0,8,'','default_3x3','','')",
    "(9967209,9967200,'Securite et armee',1,'1','1',1,0,9,'','default_3x3','','')",
    "(9967210,9967200,'Transports',1,'1','1',1,0,10,'','default_3x3','','')",
    "(9967211,9967200,'Technologie et bureau',1,'1','1',1,0,11,'','default_3x3','','')",
    "(9967212,9967200,'Collections A-D',1,'1','1',1,0,12,'','default_3x3','','')",
    "(9967213,9967200,'Collections E-H',1,'1','1',1,0,13,'','default_3x3','','')",
    "(9967214,9967200,'Collections I-L',1,'1','1',1,0,14,'','default_3x3','','')",
    "(9967215,9967200,'Collections M-P',1,'1','1',1,0,15,'','default_3x3','','')",
    "(9967216,9967200,'Collections Q-T',1,'1','1',1,0,16,'','default_3x3','','')",
    "(9967217,9967200,'Collections U-Z',1,'1','1',1,0,17,'','default_3x3','','')"
)
$pageBlock = "INSERT INTO catalog_pages (id,parent_id,caption,icon_image,visible,enabled,min_rank,min_vip,order_num,page_link,page_layout,page_strings_1,page_strings_2) VALUES`r`n" +
    ($pageRows -join ",`r`n") + "`r`nON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption=VALUES(caption),order_num=VALUES(order_num),visible='1',enabled='1';"
$sql = [regex]::Replace($sql, 'INSERT INTO catalog_pages \(.*?ON DUPLICATE KEY UPDATE .*?;', [Text.RegularExpressions.MatchEvaluator]{ param($m) $pageBlock }, [Text.RegularExpressions.RegexOptions]::Singleline)

$oldHeader = 'INSERT INTO catalog_items (item_ids,page_id,catalog_name,cost_credits,cost_points,points_type,amount,limited_stack,limited_sells,order_number,offer_id,song_id,extradata,have_offer,club_only) VALUES'
if ($sql.Contains($oldHeader)) {
    $start = $sql.IndexOf($oldHeader, [StringComparison]::Ordinal)
    $valuesStart = $start + $oldHeader.Length
    $end = $sql.IndexOf(";`nCOMMIT;", $valuesStart, [StringComparison]::Ordinal)
    if ($end -lt 0) { $end = $sql.IndexOf(";`r`nCOMMIT;", $valuesStart, [StringComparison]::Ordinal) }
    if ($end -lt 0) { throw 'Fin du bloc catalog_items introuvable.' }
    $rowsText = $sql.Substring($valuesStart, $end - $valuesStart).Trim()
    $converted = [Collections.Generic.List[string]]::new()
    $pattern = "^\('(?<item>\d+)',(?<page>\d+),'(?<name>(?:''|[^'])*)',(?<credits>-?\d+),(?<pixels>-?\d+),(?<ptype>-?\d+),(?<amount>-?\d+),(?<lstack>-?\d+),(?<lsells>-?\d+),(?<ord>-?\d+),(?<offer>-?\d+),(?<song>-?\d+),'(?<extra>(?:''|[^'])*)','(?<active>[01])','(?<club>[01])'\)$"
    foreach ($line in ($rowsText -split "`r?`n")) {
        $clean = $line.Trim().TrimEnd(',')
        $m = [regex]::Match($clean, $pattern)
        if (-not $m.Success) { throw "Ligne catalog_items non reconnue : $clean" }
        $converted.Add("($($m.Groups['page'].Value),'$($m.Groups['item'].Value)','$($m.Groups['name'].Value)',$($m.Groups['credits'].Value),$($m.Groups['pixels'].Value),0,$($m.Groups['amount'].Value),$($m.Groups['lsells'].Value),$($m.Groups['lstack'].Value),'$($m.Groups['active'].Value)','$($m.Groups['extra'].Value)','',$($m.Groups['offer'].Value),$($m.Groups['ptype'].Value))")
    }
    $newBlock = 'INSERT INTO catalog_items (page_id,item_id,catalog_name,cost_credits,cost_pixels,cost_diamonds,amount,limited_sells,limited_stack,offer_active,extradata,badge,offer_id,points_type) VALUES' + "`r`n" + ($converted -join ",`r`n")
    $sql = $sql.Substring(0,$start) + $newBlock + $sql.Substring($end)
}

function Split-SqlInsert {
    param(
        [string]$Content,
        [string]$Header,
        [string]$Terminator,
        [int]$ChunkSize
    )
    $start = $Content.IndexOf($Header, [StringComparison]::Ordinal)
    if ($start -lt 0) { throw "INSERT introuvable : $Header" }
    # Un fichier deja fractionne contient plusieurs occurrences : ne pas le retraiter.
    if ($Content.IndexOf($Header, $start + $Header.Length, [StringComparison]::Ordinal) -ge 0) { return $Content }
    $rowsStart = $start + $Header.Length
    $end = $Content.IndexOf($Terminator, $rowsStart, [StringComparison]::Ordinal)
    if ($end -lt 0) { throw "Terminateur INSERT introuvable : $Terminator" }
    $rows = @($Content.Substring($rowsStart,$end-$rowsStart).Trim() -split "`r?`n" | ForEach-Object { $_.Trim().TrimEnd(',') } | Where-Object { $_ })
    $blocks = [Collections.Generic.List[string]]::new()
    for ($offset=0; $offset -lt $rows.Count; $offset+=$ChunkSize) {
        $last = [Math]::Min($offset+$ChunkSize-1,$rows.Count-1)
        $blocks.Add($Header + "`r`n" + (@($rows[$offset..$last]) -join ",`r`n") + "`r`n" + $Terminator)
    }
    return $Content.Substring(0,$start) + ($blocks -join "`r`n") + $Content.Substring($end+$Terminator.Length)
}

$furnitureHeader = 'INSERT INTO furniture (id,item_name,public_name,type,width,length,stack_height,can_stack,can_sit,is_walkable,sprite_id,allow_recycle,allow_trade,allow_marketplace_sell,allow_gift,allow_inventory_stack,interaction_type,behaviour_data,interaction_modes_count,vending_ids,height_adjustable,effect_id,wired_id,is_rare,clothing_id,extra_rot,allow_lay) VALUES'
$furnitureTerminator = 'ON DUPLICATE KEY UPDATE item_name=VALUES(item_name),public_name=VALUES(public_name),type=VALUES(type),width=VALUES(width),length=VALUES(length),stack_height=VALUES(stack_height),can_stack=VALUES(can_stack),can_sit=VALUES(can_sit),is_walkable=VALUES(is_walkable),sprite_id=VALUES(sprite_id),interaction_type=VALUES(interaction_type),behaviour_data=VALUES(behaviour_data),interaction_modes_count=VALUES(interaction_modes_count),vending_ids=VALUES(vending_ids),allow_lay=VALUES(allow_lay);'
$catalogHeader = 'INSERT INTO catalog_items (page_id,item_id,catalog_name,cost_credits,cost_pixels,cost_diamonds,amount,limited_sells,limited_stack,offer_active,extradata,badge,offer_id,points_type) VALUES'
$sql = Split-SqlInsert -Content $sql -Header $furnitureHeader -Terminator $furnitureTerminator -ChunkSize 250
$sql = Split-SqlInsert -Content $sql -Header $catalogHeader -Terminator ';' -ChunkSize 500

# Construit le mapping depuis les lignes furniture de la migration.
$mapping = @{}
foreach ($line in ($sql -split "`r?`n")) {
    $m = [regex]::Match($line, "^\((?<id>\d+),'(?<name>(?:''|[^'])*)','(?:''|[^'])*','[si]',")
    if ($m.Success) { $mapping[$m.Groups['name'].Value.Replace("''", "'")] = [long]$m.Groups['id'].Value }
}
if ($mapping.Count -lt 8000) { throw "Mapping anormalement petit : $($mapping.Count)." }

$fd = Get-Content -LiteralPath $fdPath -Raw | ConvertFrom-Json
$updated = 0
foreach ($entry in @($fd.roomitemtypes.furnitype) + @($fd.wallitemtypes.furnitype)) {
    $name = [string]$entry.classname
    if ($mapping.ContainsKey($name)) {
        $id = [long]$mapping[$name]
        if ([long]$entry.id -ne $id -or [long]$entry.offerid -ne $id) { $updated++ }
        $entry.id = $id
        $entry.offerid = $id
    }
}

[IO.File]::WriteAllText($sqlPath, $sql, [Text.UTF8Encoding]::new($false))
[IO.File]::WriteAllText($fdPath, ($fd | ConvertTo-Json -Depth 30 -Compress), [Text.UTF8Encoding]::new($false))
Write-Host "Migration convertie au schema WavePlus : $($mapping.Count) meubles." -ForegroundColor Green
Write-Host "FurnitureData corriges : $updated identifiants." -ForegroundColor Green
