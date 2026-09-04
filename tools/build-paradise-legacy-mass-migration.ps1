[CmdletBinding()]
param([string]$RepositoryRoot = "")

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) { $RepositoryRoot = Split-Path -Parent $PSScriptRoot }

$sourcePath = Join-Path $RepositoryRoot 'migrations\20260903_paradise_catalogue_mass_habborpbr.sql'
$targetPath = Join-Path $RepositoryRoot 'migrations\20260904_paradise_catalogue_mass_habborpbr_legacy.sql'
$lines = [IO.File]::ReadAllLines($sourcePath)
$result = [Collections.Generic.List[string]]::new()

$legacyPages = @(
    "(9967200,-1,'Catalogue massif HabboRPbr','Catalogue massif HabboRPbr','default_3x3',1,1,1,90,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')",
    "(9967201,9967200,'Construction et architecture','Construction et architecture','default_3x3',1,1,1,1,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')",
    "(9967202,9967200,'Maison et decoration','Maison et decoration','default_3x3',1,1,1,2,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')",
    "(9967203,9967200,'Ville et services','Ville et services','default_3x3',1,1,1,3,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')",
    "(9967204,9967200,'Commerces et restauration','Commerces et restauration','default_3x3',1,1,1,4,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')",
    "(9967205,9967200,'Nature et exterieurs','Nature et exterieurs','default_3x3',1,1,1,5,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')",
    "(9967206,9967200,'Jeux loisirs et musique','Jeux loisirs et musique','default_3x3',1,1,1,6,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')",
    "(9967207,9967200,'Saisons et evenements','Saisons et evenements','default_3x3',1,1,1,7,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')",
    "(9967208,9967200,'Medical et sante','Medical et sante','default_3x3',1,1,1,8,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')",
    "(9967209,9967200,'Securite et armee','Securite et armee','default_3x3',1,1,1,9,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')",
    "(9967210,9967200,'Transports','Transports','default_3x3',1,1,1,10,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')",
    "(9967211,9967200,'Technologie et bureau','Technologie et bureau','default_3x3',1,1,1,11,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')",
    "(9967212,9967200,'Collections A-D','Collections A-D','default_3x3',1,1,1,12,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')",
    "(9967213,9967200,'Collections E-H','Collections E-H','default_3x3',1,1,1,13,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')",
    "(9967214,9967200,'Collections I-L','Collections I-L','default_3x3',1,1,1,14,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')",
    "(9967215,9967200,'Collections M-P','Collections M-P','default_3x3',1,1,1,15,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')",
    "(9967216,9967200,'Collections Q-T','Collections Q-T','default_3x3',1,1,1,16,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')",
    "(9967217,9967200,'Collections U-Z','Collections U-Z','default_3x3',1,1,1,17,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')"
)

$skipPages = $false
$catalogMode = $false
$converted = 0
$pattern = "^\((?<page>\d+),'(?<item>\d+)','(?<name>(?:''|[^'])*)',(?<credits>-?\d+),(?<points>-?\d+),(?<diamonds>-?\d+),(?<amount>-?\d+),(?<lsells>-?\d+),(?<lstack>-?\d+),'(?<active>[01])','(?<extra>(?:''|[^'])*)','(?<badge>(?:''|[^'])*)',(?<offer>-?\d+),(?<ptype>-?\d+)\)(?<end>[,;]?)$"

foreach ($line in $lines) {
    if ($line.StartsWith('INSERT INTO catalog_pages ')) {
        $result.Add('INSERT INTO catalog_pages (id,parent_id,caption_save,caption,page_layout,icon_color,icon_image,min_rank,order_num,visible,enabled,club_only,vip_only,page_headline,page_teaser,page_special,page_text1,page_text2,page_text_details,page_text_teaser,room_id,includes) VALUES')
        for ($i=0; $i -lt $legacyPages.Count; $i++) { $result.Add($legacyPages[$i] + $(if($i -eq $legacyPages.Count-1){''}else{','})) }
        $result.Add("ON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption_save=VALUES(caption_save),caption=VALUES(caption),order_num=VALUES(order_num),visible='1',enabled='1';")
        $skipPages = $true
        continue
    }
    if ($skipPages) {
        if ($line.StartsWith('DELETE FROM catalog_items ')) { $skipPages = $false; $result.Add($line) }
        continue
    }
    if ($line.StartsWith('INSERT INTO catalog_items ')) {
        $result.Add('INSERT INTO catalog_items (item_ids,page_id,catalog_name,cost_credits,cost_points,points_type,amount,limited_stack,limited_sells,order_number,offer_id,song_id,extradata,have_offer,club_only) VALUES')
        $catalogMode = $true
        continue
    }
    if ($catalogMode) {
        if ($line.Trim() -eq ';') { $result.Add(';'); $catalogMode = $false; continue }
        $m = [regex]::Match($line.Trim(),$pattern)
        if (-not $m.Success) { throw "Ligne catalogue moderne non reconnue : $line" }
        $result.Add("('$($m.Groups['item'].Value)',$($m.Groups['page'].Value),'$($m.Groups['name'].Value)',$($m.Groups['credits'].Value),$($m.Groups['points'].Value),$($m.Groups['ptype'].Value),$($m.Groups['amount'].Value),$($m.Groups['lstack'].Value),$($m.Groups['lsells'].Value),1,$($m.Groups['offer'].Value),0,'$($m.Groups['extra'].Value)','$($m.Groups['active'].Value)','0')$($m.Groups['end'].Value)")
        $converted++
        if ($m.Groups['end'].Value -eq ';') { $catalogMode = $false }
        continue
    }
    $result.Add($line)
}

if ($converted -ne 8883) { throw "Nombre d'offres converties inattendu : $converted." }
[IO.File]::WriteAllLines($targetPath,$result,[Text.UTF8Encoding]::new($false))
Write-Host "Migration legacy generee : $targetPath ($converted offres)." -ForegroundColor Green
