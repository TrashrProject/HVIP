[CmdletBinding()]
param(
    [string]$RepositoryRoot = "",
    [string]$HabboRpRoot = "C:\xampp\htdocs\HabboRPbr",
    [string]$Mysql = "C:\xampp\mysql\bin\mysql.exe",
    [string]$Database = "waveplus",
    [switch]$Apply
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) { $RepositoryRoot = Split-Path -Parent $PSScriptRoot }

$targetFd   = Join-Path $RepositoryRoot 'swf_pz\V5-0-2\gamedata\json\FurnitureData.json'
$targetFurn = Join-Path $RepositoryRoot 'swf_pz\V5-0-2\furniture'
$targetIcon = Join-Path $RepositoryRoot 'swf_pz\V5-0-2\dcr\hof_furni\icon'
$sourceFd   = Join-Path $HabboRpRoot 'pack\cdn-react\gamedata\FurnitureData.json'
$sourceFurn = Join-Path $HabboRpRoot 'pack\cdn-react\bundled\furniture'
$sourceIcon = Join-Path $HabboRpRoot 'pack\cdn-react\icons'
$sqlPath    = Join-Path $RepositoryRoot 'migrations\20260903_paradise_catalogue_fr_clean.sql'
$reportPath = Join-Path $PSScriptRoot 'catalogue-fr-clean-report.csv'

foreach ($p in @($Mysql,$targetFd,$targetFurn,$targetIcon)) {
    if (-not (Test-Path -LiteralPath $p)) { throw "Ressource requise absente : $p" }
}

function Escape-Sql([AllowNull()][string]$s) {
    if ($null -eq $s) { return '' }
    return $s.Replace('\','\\').Replace("'","''").Replace("`r",' ').Replace("`n",' ')
}

function Normalize([string]$s) {
    if ([string]::IsNullOrWhiteSpace($s)) { return '' }
    return (($s -replace '[_.-]+',' ') -replace '\s+',' ').Trim()
}

# Traduction volontairement conservative : les noms propres/collections restent intacts,
# les termes descriptifs usuels sont traduits. Cela evite de fabriquer des noms absurdes.
$translations = [ordered]@{
    'christmas'='Noel'; 'xmas'='Noel'; 'halloween'='Halloween'; 'easter'='Paques'; 'valentine'='Saint Valentin';
    'new year'='Nouvel An'; 'winter'='Hiver'; 'summer'='Ete'; 'spring'='Printemps'; 'autumn'='Automne';
    'chair'='Chaise'; 'armchair'='Fauteuil'; 'sofa'='Canape'; 'couch'='Canape'; 'stool'='Tabouret'; 'bench'='Banc';
    'table'='Table'; 'desk'='Bureau'; 'bed'='Lit'; 'pillow'='Oreiller'; 'cushion'='Coussin'; 'cabinet'='Meuble';
    'wardrobe'='Armoire'; 'shelf'='Etagere'; 'bookcase'='Bibliotheque'; 'lamp'='Lampe'; 'light'='Lumiere';
    'rug'='Tapis'; 'carpet'='Tapis'; 'mirror'='Miroir'; 'painting'='Tableau'; 'frame'='Cadre'; 'vase'='Vase';
    'kitchen'='Cuisine'; 'fridge'='Refrigerateur'; 'oven'='Four'; 'sink'='Evier'; 'toilet'='Toilettes'; 'shower'='Douche';
    'bath'='Baignoire'; 'door'='Porte'; 'window'='Fenetre'; 'wall'='Mur'; 'floor'='Sol'; 'roof'='Toit';
    'stairs'='Escalier'; 'stair'='Escalier'; 'fence'='Cloture'; 'gate'='Portail'; 'pillar'='Pilier'; 'column'='Colonne';
    'brick'='Brique'; 'concrete'='Beton'; 'road'='Route'; 'street'='Rue'; 'bridge'='Pont';
    'tree'='Arbre'; 'plant'='Plante'; 'flower'='Fleur'; 'garden'='Jardin'; 'grass'='Herbe'; 'rock'='Rocher';
    'stone'='Pierre'; 'water'='Eau'; 'beach'='Plage'; 'pool'='Piscine'; 'forest'='Foret'; 'farm'='Ferme';
    'shop'='Boutique'; 'store'='Magasin'; 'market'='Marche'; 'restaurant'='Restaurant'; 'cafe'='Cafe';
    'coffee'='Cafe'; 'bakery'='Boulangerie'; 'cash register'='Caisse'; 'cashier'='Caisse'; 'hotel'='Hotel';
    'hospital'='Hopital'; 'clinic'='Clinique'; 'doctor'='Medecin'; 'nurse'='Infirmier'; 'ambulance'='Ambulance';
    'pharmacy'='Pharmacie'; 'police'='Police'; 'prison'='Prison'; 'security'='Securite'; 'army'='Armee';
    'military'='Militaire'; 'car'='Voiture'; 'taxi'='Taxi'; 'bus'='Bus'; 'train'='Train'; 'bike'='Velo';
    'bicycle'='Velo'; 'motorcycle'='Moto'; 'boat'='Bateau'; 'plane'='Avion'; 'airport'='Aeroport'; 'garage'='Garage';
    'computer'='Ordinateur'; 'laptop'='Ordinateur portable'; 'phone'='Telephone'; 'screen'='Ecran'; 'monitor'='Ecran';
    'camera'='Camera'; 'television'='Television'; 'speaker'='Enceinte'; 'radio'='Radio'; 'game'='Jeu';
    'arcade'='Arcade'; 'music'='Musique'; 'piano'='Piano'; 'guitar'='Guitare'; 'drum'='Batterie'; 'sport'='Sport';
    'football'='Football'; 'basketball'='Basket'; 'tennis'='Tennis'; 'office'='Bureau'; 'school'='Ecole'; 'bank'='Banque'
}

function Get-FrenchName([string]$publicName,[string]$className,[string]$fdName) {
    $base = $publicName
    if ([string]::IsNullOrWhiteSpace($base) -or $base -match '^\s*(null|none|default)\s*$') { $base = $fdName }
    if ([string]::IsNullOrWhiteSpace($base)) { $base = $className }
    $base = Normalize $base
    $result = $base
    foreach ($kv in $translations.GetEnumerator()) {
        $pattern = '(?i)(?<![A-Za-z0-9])' + [regex]::Escape($kv.Key) + '(?![A-Za-z0-9])'
        $result = [regex]::Replace($result,$pattern,$kv.Value)
    }
    if ($result.Length -gt 96) { $result = $result.Substring(0,96).Trim() }
    return $result
}

function Get-Category([string]$text) {
    $n = $text.ToLowerInvariant()
    if ($n -match 'xmas|christmas|noel|kerst|easter|pasen|hween|halloween|valentine|newyear|new year|carnav|festive|pumpkin|snow|winter|summer|spring|autumn') { return 'Fetes et saisons' }
    if ($n -match 'hospital|clinic|medic|doctor|nurse|ambulance|pharmacy|surgery|dentist|firstaid|stretcher|wheelchair|xray|defib') { return 'Hopital et sante' }
    if ($n -match 'police|prison|security|court|swat|jail|guard') { return 'Police et securite' }
    if ($n -match 'army|military|bunker|barrack|weapon rack') { return 'Armee et militaire' }
    if ($n -match '(^|[_ -])car([_ -]|$)|vehicle|garage|taxi|(^|[_ -])bus([_ -]|$)|busstop|bike|bicycle|moto|scooter|train|metro|airport|plane|boat|ship|tram|rail|parking|petrol|traffic') { return 'Transports et vehicules' }
    if ($n -match 'shop|store|market|restaurant|cafe|coffee|food|bar_|mall|vending|bakery|diner|kiosk|boutique|cashier|checkout|register|counter|menu|pizza|burger|salon') { return 'Commerces et restauration' }
    if ($n -match 'office|bank|school|city|urban|hotel|apartment|government|station|reception|lobby|library|museum|streetlight|elevator|locker|mail|queue|barrier|sign_') { return 'Ville et services' }
    if ($n -match 'computer|laptop|phone|screen|monitor|server|tech|robot|camera|television|speaker|console|radio|tablet|keyboard|machine|device') { return 'Technologie et bureau' }
    if ($n -match 'tree|plant|flower|fleur|sakura|bush|grass|rock|garden|forest|farm|beach|water|pool|nature|outdoor|park|pond|river|mountain|sand|soil') { return 'Nature et exterieurs' }
    if ($n -match 'game|gaming|music|disco|cinema|stage|gym|football|trophy|dance|theatre|theater|basket|tennis|skate|ball|piano|guitar|drum|billiard|pooltable|foosball|dj_|arcade|chess|domino|mahjong|sport') { return 'Jeux sport et musique' }
    if ($n -match 'sofa|chair|seat|stool|bench|table|cabinet|shelf|bed|nightstand|bath|toilet|shower|kitchen|lamp|rug|carpet|home|wardrobe|dresser|desk|couch|fridge|oven|sink|mirror|blanket|painting|candle|bowl|cushion|pillow|frame|vase') { return 'Maison et decoration' }
    if ($n -match 'wall|floor|tile|roof|door|window|gate|fence|stair|column|pillar|block|build|construction|road|street|bridge|brick|concrete|beam|ladder|plank') { return 'Construction et architecture' }
    if ($n -match 'rare|throne|dragon|gold|silver|diamond|crystal|royal|luxury|luxe') { return 'Rares et prestige' }
    if ($n -match 'pet|dog|cat|horse|animal|bird|fish|bear|lion|monkey|rabbit|bunny') { return 'Animaux' }
    return 'Collections et divers'
}

$pages = [ordered]@{
    'Maison et decoration'          = 9967401
    'Construction et architecture' = 9967402
    'Ville et services'             = 9967403
    'Commerces et restauration'     = 9967404
    'Nature et exterieurs'          = 9967405
    'Jeux sport et musique'         = 9967406
    'Fetes et saisons'              = 9967407
    'Hopital et sante'              = 9967408
    'Police et securite'            = 9967409
    'Armee et militaire'            = 9967410
    'Transports et vehicules'       = 9967411
    'Technologie et bureau'         = 9967412
    'Animaux'                       = 9967413
    'Rares et prestige'             = 9967414
    'Collections et divers'         = 9967415
}
$rootPage = 9967400
$quarantinePage = 9967499

# FurnitureData actuel + source de secours.
$fd = Get-Content -LiteralPath $targetFd -Raw | ConvertFrom-Json
$fdEntries = @($fd.roomitemtypes.furnitype)+@($fd.wallitemtypes.furnitype)
$fdById = @{}
$fdByClass = @{}
foreach ($e in $fdEntries) {
    if ($null -ne $e.id) { $fdById[[long]$e.id] = $e }
    if ($null -ne $e.classname) { $fdByClass[[string]$e.classname] = $e }
}
$sourceByClass = @{}
if (Test-Path -LiteralPath $sourceFd) {
    $sfd = Get-Content -LiteralPath $sourceFd -Raw | ConvertFrom-Json
    foreach ($e in @($sfd.roomitemtypes.furnitype)+@($sfd.wallitemtypes.furnitype)) {
        if ($null -ne $e.classname -and -not $sourceByClass.ContainsKey([string]$e.classname)) { $sourceByClass[[string]$e.classname] = $e }
    }
}

# Etat reel de la BDD.
$furnSql = "SELECT id,item_name,public_name,type FROM furniture WHERE type IN ('s','i') AND item_name<>'';"
$catalogSql = "SELECT id,item_ids,page_id,catalog_name FROM catalog_items WHERE item_ids REGEXP '^[0-9]+';"
$furnRows = & $Mysql -u root -N -B --raw $Database -e $furnSql
if ($LASTEXITCODE -ne 0) { throw 'Lecture de furniture impossible.' }
$catRows = & $Mysql -u root -N -B --raw $Database -e $catalogSql
if ($LASTEXITCODE -ne 0) { throw 'Lecture de catalog_items impossible.' }

$furnById = @{}
foreach ($line in $furnRows) {
    $p = $line -split "`t",-1
    if ($p.Count -ge 4) { $furnById[[long]$p[0]] = [pscustomobject]@{ Id=[long]$p[0]; ClassName=$p[1]; PublicName=$p[2]; Type=$p[3] } }
}

$rows = [Collections.Generic.List[object]]::new()
$repairedNitro=0; $repairedIcons=0; $repairedFd=0
foreach ($line in $catRows) {
    $p = $line -split "`t",-1
    if ($p.Count -lt 4) { continue }
    $cid=[long]$p[0]
    $firstId = (($p[1] -split '[,:;]')[0] -replace '[^0-9]','')
    if ([string]::IsNullOrWhiteSpace($firstId)) { continue }
    $fid=[long]$firstId
    if (-not $furnById.ContainsKey($fid)) { continue }
    $f=$furnById[$fid]
    $className=[string]$f.ClassName
    $assetName=($className -split '\*')[0]

    # FurnitureData manquant : recupere la definition depuis HabboRPbr si possible.
    if (-not $fdById.ContainsKey($fid) -and $sourceByClass.ContainsKey($className)) {
        $src=$sourceByClass[$className]
        if ($f.Type -eq 'i') { $fd.wallitemtypes.furnitype = @($fd.wallitemtypes.furnitype)+@($src) }
        else { $fd.roomitemtypes.furnitype = @($fd.roomitemtypes.furnitype)+@($src) }
        $fdById[$fid]=$src; $fdByClass[$className]=$src; $repairedFd++
    }

    $nitro=Join-Path $targetFurn ($assetName+'.nitro')
    $icon=Join-Path $targetIcon ($assetName+'_icon.png')
    if (-not (Test-Path -LiteralPath $nitro) -and (Test-Path -LiteralPath (Join-Path $sourceFurn ($assetName+'.nitro')))) {
        Copy-Item -LiteralPath (Join-Path $sourceFurn ($assetName+'.nitro')) -Destination $nitro -Force; $repairedNitro++
    }
    if (-not (Test-Path -LiteralPath $icon) -and (Test-Path -LiteralPath (Join-Path $sourceIcon ($assetName+'_icon.png')))) {
        Copy-Item -LiteralPath (Join-Path $sourceIcon ($assetName+'_icon.png')) -Destination $icon -Force; $repairedIcons++
    }

    $hasFd=$fdById.ContainsKey($fid)
    $hasNitro=Test-Path -LiteralPath $nitro
    $hasIcon=Test-Path -LiteralPath $icon
    $fdName=''
    $fdDesc=''
    if ($hasFd) {
        $fde=$fdById[$fid]
        if ($null -ne $fde.name) { $fdName=[string]$fde.name }
        if ($null -ne $fde.description) { $fdDesc=[string]$fde.description }
    }
    $search="$className $($f.PublicName) $fdName $fdDesc"
    $category=Get-Category $search
    $french=Get-FrenchName $f.PublicName $className $fdName
    $ok=$hasFd -and $hasNitro -and $hasIcon
    $rows.Add([pscustomobject]@{CatalogItemId=$cid;FurnitureId=$fid;ClassName=$className;OldPage=[int]$p[2];OldName=$p[3];FrenchName=$french;Category=$category;HasFurnitureData=$hasFd;HasNitro=$hasNitro;HasIcon=$hasIcon;Valid=$ok})
}

# Ecrit le FurnitureData repare localement.
[IO.File]::WriteAllText($targetFd,($fd | ConvertTo-Json -Depth 30 -Compress),[Text.UTF8Encoding]::new($false))

$valid=@($rows|Where-Object Valid)
$invalid=@($rows|Where-Object {-not $_.Valid})

$sb=[Text.StringBuilder]::new()
[void]$sb.AppendLine('-- ParadiseRP - catalogue FR propre, trie et valide')
[void]$sb.AppendLine('SET NAMES utf8mb4;')
[void]$sb.AppendLine('START TRANSACTION;')
[void]$sb.AppendLine("DELETE FROM catalog_pages WHERE id BETWEEN $rootPage AND $quarantinePage;")
$pageRows=[Collections.Generic.List[string]]::new()
$pageRows.Add("($rootPage,9967000,'Catalogue ParadiseRP','Catalogue ParadiseRP','default_3x3',1,1,1,1,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')")
$order=1
foreach($kv in $pages.GetEnumerator()) {
    $cap=Escape-Sql $kv.Key
    $pageRows.Add("($($kv.Value),$rootPage,'$cap','$cap','default_3x3',1,1,1,$order,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')")
    $order++
}
$pageRows.Add("($quarantinePage,$rootPage,'Elements incomplets','Elements incomplets','default_3x3',1,1,7,99,'0','0','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')")
[void]$sb.AppendLine('INSERT INTO catalog_pages (id,parent_id,caption_save,caption,page_layout,icon_color,icon_image,min_rank,order_num,visible,enabled,club_only,vip_only,page_headline,page_teaser,page_special,page_text1,page_text2,page_text_details,page_text_teaser,room_id,includes) VALUES')
[void]$sb.AppendLine(($pageRows -join ",`n")+';')

# Mise a jour en lots pour eviter max_allowed_packet.
$all=@($rows)
$chunkSize=300
for($offset=0;$offset -lt $all.Count;$offset+=$chunkSize) {
    $chunk=@($all|Select-Object -Skip $offset -First $chunkSize)
    [void]$sb.AppendLine('UPDATE catalog_items SET page_id = CASE id')
    foreach($r in $chunk) { $targetPageId=if($r.Valid){[int]$pages[$r.Category]}else{$quarantinePage}; [void]$sb.AppendLine(" WHEN $($r.CatalogItemId) THEN $targetPageId") }
    [void]$sb.AppendLine(' ELSE page_id END, catalog_name = CASE id')
    foreach($r in $chunk) { [void]$sb.AppendLine(" WHEN $($r.CatalogItemId) THEN '$(Escape-Sql $r.FrenchName)'") }
    [void]$sb.AppendLine(" ELSE catalog_name END WHERE id IN ($($chunk.CatalogItemId -join ','));")
}

# Les anciennes pages ParadiseRP sont cachees apres reclassement pour ne pas afficher de doublons.
[void]$sb.AppendLine("UPDATE catalog_pages SET visible='0',enabled='0' WHERE id BETWEEN 9967100 AND 9967399;")
[void]$sb.AppendLine('COMMIT;')
[IO.File]::WriteAllText($sqlPath,$sb.ToString(),[Text.UTF8Encoding]::new($false))

$rows | Export-Csv -LiteralPath $reportPath -NoTypeInformation -Encoding UTF8
Write-Host '=== CATALOGUE PARADISERP FR PREPARE ===' -ForegroundColor Cyan
Write-Host "Offres mobilier analysees : $($rows.Count)" -ForegroundColor White
Write-Host "Offres valides avec FurnitureData + .nitro + icone : $($valid.Count)" -ForegroundColor Green
Write-Host "Offres incompletes masquees : $($invalid.Count)" -ForegroundColor $(if($invalid.Count){'Yellow'}else{'Green'})
Write-Host "FurnitureData repares : $repairedFd" -ForegroundColor Green
Write-Host ".nitro repares : $repairedNitro" -ForegroundColor Green
Write-Host "Icones reparees : $repairedIcons" -ForegroundColor Green
Write-Host 'Repartition finale :' -ForegroundColor Cyan
$valid | Group-Object Category | Sort-Object Count -Descending | ForEach-Object { Write-Host (" - {0}: {1}" -f $_.Name,$_.Count) -ForegroundColor Green }
if($invalid.Count -gt 0) {
    Write-Host 'Manquants restants :' -ForegroundColor Yellow
    Write-Host " - FurnitureData : $(@($invalid|Where-Object {-not $_.HasFurnitureData}).Count)"
    Write-Host " - .nitro : $(@($invalid|Where-Object {-not $_.HasNitro}).Count)"
    Write-Host " - icones : $(@($invalid|Where-Object {-not $_.HasIcon}).Count)"
}
Write-Host "Migration : $sqlPath" -ForegroundColor Cyan
Write-Host "Rapport : $reportPath" -ForegroundColor Cyan

if($Apply) {
    $proc=Start-Process -FilePath $Mysql -ArgumentList @('-u','root',"--database=$Database",'--default-character-set=utf8mb4') -RedirectStandardInput $sqlPath -NoNewWindow -Wait -PassThru
    if($proc.ExitCode -ne 0){throw "Application SQL echouee (code $($proc.ExitCode))."}
    Write-Host 'Migration appliquee avec succes.' -ForegroundColor Green
}

