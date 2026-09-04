[CmdletBinding()]
param(
    [string]$Mysql = 'C:\xampp\mysql\bin\mysql.exe',
    [string]$MysqlDump = 'C:\xampp\mysql\bin\mysqldump.exe',
    [string]$Database = 'waveplus',
    [switch]$Apply
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$activeRoot = Join-Path $repoRoot 'WebPixel\swf_pz\V5-0-2'
$fdPath = Join-Path $activeRoot 'gamedata\json\FurnitureData.json'
$furnitureDir = Join-Path $activeRoot 'furniture'
$iconDir = Join-Path $activeRoot 'dcr\hof_furni\icon'
$migration = Join-Path $repoRoot 'migrations\20260904_import_all_wave_blocks.sql'
$report = Join-Path $repoRoot 'tools\all-wave-blocks-report.csv'
$backupDir = Join-Path $repoRoot 'backups\catalogue'
$backup = Join-Path $backupDir 'catalog-before-all-wave-blocks.sql'
$syncScript = Join-Path $PSScriptRoot 'sync-catalog-furniture-to-items-base.ps1'

foreach ($path in @($Mysql,$MysqlDump,$fdPath,$furnitureDir,$iconDir,$syncScript)) {
    if (-not (Test-Path -LiteralPath $path)) { throw "Ressource requise absente : $path" }
}

function Escape-Sql([AllowNull()][string]$Value) {
    if ($null -eq $Value) { return '' }
    return $Value.Replace('\\','\\\\').Replace("'","''").Replace("`r",' ').Replace("`n",' ')
}

function Invoke-Rows([string]$Sql) {
    $out = & $Mysql -u root -N -B --raw $Database -e $Sql
    if ($LASTEXITCODE -ne 0) { throw "Requete MySQL echouee." }
    return @($out)
}

function Get-IconName([string]$ClassName) {
    # Windows PowerShell 5 peut reduire un tableau a un scalaire quand il n'y a
    # pas de variante *N. Le @() garantit que .Count existe sous StrictMode.
    $parts = @($ClassName -split '\*',2)
    $base = [string]$parts[0]
    if ($parts.Count -gt 1 -and -not [string]::IsNullOrWhiteSpace([string]$parts[1])) {
        return $base + '_' + [string]$parts[1] + '_icon.png'
    }
    return $base + '_icon.png'
}

function Get-Category([string]$ClassName,[string]$DisplayName) {
    $text = ($ClassName + ' ' + $DisplayName).ToLowerInvariant()
    if ($ClassName -match '^bc_block_1\*') { return 'Grands blocs' }
    if ($ClassName -match '^bc_block_0\*|^bc_block_small\*') { return 'Petits blocs' }
    if ($text -match 'round|sphere|cylinder|cone|pyramid|quarterring|quartercircle|wedge|triangle|diagonal|angle') { return 'Formes de construction' }
    if ($text -match 'stone|brick|marble|concrete|rock|pierre|brique') { return 'Pierre brique marbre' }
    if ($text -match 'wood|wool|grass|sand|soil|terra|bois|laine|gazon') { return 'Bois et nature' }
    if ($text -match 'metal|glass|ice|lava|water|verre|glace') { return 'Metal verre effets' }
    return 'Blocs custom'
}

function Get-FrenchName([string]$Name,[string]$ClassName) {
    $n = if ([string]::IsNullOrWhiteSpace($Name)) { $ClassName } else { $Name }
    $n = $n -replace '_name$',''
    $n = $n -replace '_',' '
    $n = $n -replace '^Large Block\s*','Grand Bloc '
    $n = $n -replace '^Small Block\s*','Petit Bloc '
    $n = $n -replace '^Stone Block\s*','Bloc pierre '
    $n = $n -replace '^Round Block\s*','Bloc rond '
    $n = $n -replace '^Wool Block\s*','Bloc laine '
    $n = $n -replace '^Metal Crate Block\s*','Bloc caisse metal '
    $n = $n -replace '^Building Block\s*','Bloc de construction '
    if ($ClassName -eq 'bc_block_1*13') { return 'Grand Bloc noir' }
    return $n.Trim()
}

# FurnitureData utilise reellement par Nitro.
$fd = Get-Content -LiteralPath $fdPath -Raw | ConvertFrom-Json
$fdById = @{}
foreach ($entry in @($fd.roomitemtypes.furnitype) + @($fd.wallitemtypes.furnitype)) {
    if ($null -eq $entry.id) { continue }
    $key = [string]$entry.id
    if (-not $fdById.ContainsKey($key)) { $fdById[$key] = $entry }
}

# On part de furniture, mais on ne garde que les vrais objets de construction.
$sql = @"
SELECT id,item_name,public_name,type
FROM furniture
WHERE type IN ('s','i')
  AND item_name <> ''
  AND (
      LOWER(item_name) REGEXP 'block|bloc|cube|brick|stone|wool|wood|metalcrate|marble|lava|sand|grass|round|sphere|cylinder|cone|pyramid|quarterring|quartercircle|wedge|tile'
      OR LOWER(public_name) REGEXP 'block|bloc|cube|brick|stone|wool|wood|metal crate|marble|lava|sand|grass|round|sphere|cylinder|cone|pyramid|quarter ring|tile'
  )
ORDER BY id;
"@

# @() autour des appels garantit un vrai tableau meme si MySQL renvoie 0 ou 1 ligne.
$raw = @(Invoke-Rows $sql)
$catalogIds = [Collections.Generic.HashSet[long]]::new()
foreach ($line in @(Invoke-Rows "SELECT DISTINCT CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_ids, ',', 1), ':', 1) AS UNSIGNED) FROM catalog_items WHERE item_ids REGEXP '^[0-9]+';")) {
    $value = 0L
    if ([long]::TryParse(($line -split "`t")[0],[ref]$value)) { [void]$catalogIds.Add($value) }
}
$itemsBaseIds = [Collections.Generic.HashSet[long]]::new()
foreach ($line in @(Invoke-Rows 'SELECT id FROM items_base;')) {
    $value = 0L
    if ([long]::TryParse(($line -split "`t")[0],[ref]$value)) { [void]$itemsBaseIds.Add($value) }
}

$excludedPattern = '(^clothing_)|chair|stool|sofa|armchair|seat|lamp|bed|table|plant|hat|hoodie|shirt|dress|shoe|fridge|toilet|dragon|orb|statue|throne|pillow|cushion|register|bunny|duck|penguin|bookcase|shelf|counter|sink|vase|fan|fountain|parasol|trophy|machine|tv|television|safe|minibar|rug|carpet'
$usable = [Collections.Generic.List[object]]::new()
$seen = [Collections.Generic.HashSet[long]]::new()

foreach ($line in $raw) {
    $parts = @($line -split "`t",-1)
    if ($parts.Count -lt 4) { continue }
    $furnitureId = 0L
    if (-not [long]::TryParse([string]$parts[0],[ref]$furnitureId)) { continue }
    if (-not $seen.Add($furnitureId)) { continue }
    if (-not $fdById.ContainsKey([string]$furnitureId)) { continue }

    $entry = $fdById[[string]$furnitureId]
    $className = [string]$entry.classname
    if ([string]::IsNullOrWhiteSpace($className)) { continue }
    $display = if ($null -ne $entry.name) { [string]$entry.name } elseif (-not [string]::IsNullOrWhiteSpace([string]$parts[2])) { [string]$parts[2] } else { [string]$parts[1] }
    $check = ($className + ' ' + $display).ToLowerInvariant()
    if ($check -match $excludedPattern) { continue }

    $assetParts = @($className -split '\*',2)
    $assetBase = [string]$assetParts[0]
    if ($assetBase -notmatch '^[A-Za-z0-9_.-]+$') { continue }
    $nitroPath = Join-Path $furnitureDir ($assetBase + '.nitro')
    $iconName = Get-IconName $className
    $iconPath = Join-Path $iconDir $iconName

    # Certaines anciennes variantes partagent l'icone principale.
    if (-not (Test-Path -LiteralPath $iconPath)) {
        $fallbackIcon = Join-Path $iconDir ($assetBase + '_icon.png')
        if (Test-Path -LiteralPath $fallbackIcon) {
            $iconPath = $fallbackIcon
            $iconName = Split-Path -Leaf $fallbackIcon
        }
    }

    if (-not (Test-Path -LiteralPath $nitroPath)) { continue }
    if (-not (Test-Path -LiteralPath $iconPath)) { continue }

    $category = Get-Category $className $display
    $nameFr = Get-FrenchName $display $className
    $usable.Add([pscustomobject]@{
        Id = $furnitureId
        ClassName = $className
        Name = $nameFr
        Category = $category
        Nitro = Split-Path -Leaf $nitroPath
        Icon = $iconName
        InItemsBase = $itemsBaseIds.Contains($furnitureId)
        AlreadyInCatalog = $catalogIds.Contains($furnitureId)
    })
}

$usable = @($usable | Sort-Object Category,Name,Id)
if ($usable.Count -eq 0) { throw 'Aucun bloc complet trouve dans les assets actifs.' }

$pages = [ordered]@{
    'Grands blocs'            = 9967601
    'Petits blocs'            = 9967602
    'Pierre brique marbre'    = 9967603
    'Bois et nature'          = 9967604
    'Metal verre effets'      = 9967605
    'Formes de construction'  = 9967606
    'Blocs custom'            = 9967607
}

$parentRows = @(Invoke-Rows 'SELECT parent_id FROM catalog_pages WHERE id=140 LIMIT 1;')
$parentId = 222
if ($parentRows.Count -gt 0) {
    $tmpParent = 0
    if ([int]::TryParse([string](($parentRows[0] -split "`t")[0]),[ref]$tmpParent)) { $parentId = $tmpParent }
}

$sb = [Text.StringBuilder]::new()
[void]$sb.AppendLine('-- ParadiseRP - toutes les familles de blocs completes actuellement disponibles')
[void]$sb.AppendLine('SET NAMES utf8mb4;')
[void]$sb.AppendLine('START TRANSACTION;')

$pageRows = [Collections.Generic.List[string]]::new()
$order = 160
foreach ($kv in $pages.GetEnumerator()) {
    $cap = Escape-Sql $kv.Key
    $pageRows.Add("($($kv.Value),$parentId,'$cap','$cap','default_3x3',1,1,1,$order,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,NULL,'')")
    $order++
}
[void]$sb.AppendLine('INSERT INTO catalog_pages (id,parent_id,caption_save,caption,page_layout,icon_color,icon_image,min_rank,order_num,visible,enabled,club_only,vip_only,page_headline,page_teaser,page_special,page_text1,page_text2,page_text_details,page_text_teaser,room_id,includes) VALUES')
[void]$sb.AppendLine(($pageRows -join ",`n") + "`nON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption_save=VALUES(caption_save),caption=VALUES(caption),order_num=VALUES(order_num),visible='1',enabled='1';")
[void]$sb.AppendLine('DELETE FROM catalog_items WHERE page_id BETWEEN 9967601 AND 9967607;')

$rows = [Collections.Generic.List[string]]::new()
$orderByPage = @{}
foreach ($item in $usable) {
    $pageId = [int]$pages[$item.Category]
    if (-not $orderByPage.ContainsKey($pageId)) { $orderByPage[$pageId] = 0 }
    $orderByPage[$pageId]++
    $name = Escape-Sql $item.Name
    $rows.Add("('$($item.Id)',$pageId,'$name',3,0,0,1,0,0,$($orderByPage[$pageId]),-1,0,'','1','0')")
}

$chunkSize = 200
for ($i=0; $i -lt $rows.Count; $i += $chunkSize) {
    $last = [Math]::Min($i + $chunkSize - 1,$rows.Count - 1)
    $chunk = @($rows[$i..$last])
    [void]$sb.AppendLine('INSERT INTO catalog_items (item_ids,page_id,catalog_name,cost_credits,cost_points,points_type,amount,limited_stack,limited_sells,order_number,offer_id,song_id,extradata,have_offer,club_only) VALUES')
    [void]$sb.AppendLine(($chunk -join ",`n") + ';')
}

[void]$sb.AppendLine('COMMIT;')
[IO.File]::WriteAllText($migration,$sb.ToString(),[Text.UTF8Encoding]::new($false))
$usable | Export-Csv -LiteralPath $report -NoTypeInformation -Encoding UTF8

$missingItemsBase = @($usable | Where-Object { -not $_.InItemsBase }).Count
$alreadyCatalog = @($usable | Where-Object { $_.AlreadyInCatalog }).Count

Write-Host '=== TOUS LES BLOCS WAVE ===' -ForegroundColor Cyan
Write-Host "Blocs complets retenus : $($usable.Count)" -ForegroundColor Green
Write-Host "Deja presents ailleurs dans le catalogue : $alreadyCatalog"
Write-Host "A synchroniser vers items_base : $missingItemsBase" -ForegroundColor $(if($missingItemsBase -gt 0){'Yellow'}else{'Green'})
Write-Host "Parent catalogue utilise : $parentId"
Write-Host 'Repartition :'
$usable | Group-Object Category | Sort-Object Name | ForEach-Object { Write-Host " - $($_.Name) : $($_.Count)" }
Write-Host "Migration : $migration"
Write-Host "Rapport : $report"
Write-Host 'Aucun mobi sans FurnitureData + .nitro + icone n est ajoute.' -ForegroundColor Green

if (-not $Apply) {
    Write-Host 'Mode audit uniquement. Relancer avec -Apply pour ajouter les pages et offres.' -ForegroundColor Yellow
    exit 0
}

New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
& $MysqlDump -u root $Database catalog_pages catalog_items --result-file=$backup
if ($LASTEXITCODE -ne 0) { throw 'Sauvegarde catalogue impossible. Application annulee.' }
Write-Host "Sauvegarde : $backup" -ForegroundColor Cyan

& "$env:ComSpec" /c ('"'+$Mysql+'" -u root --default-character-set=utf8mb4 '+$Database+' < "'+$migration+'"')
if ($LASTEXITCODE -ne 0) { throw "Application SQL echouee (code $LASTEXITCODE)." }

Write-Host 'Pages/offres blocs appliquees.' -ForegroundColor Green
Write-Host 'Synchronisation automatique vers items_base...' -ForegroundColor Cyan
& powershell -ExecutionPolicy Bypass -File $syncScript -Apply
if ($LASTEXITCODE -ne 0) { throw 'La synchronisation items_base a echoue.' }

Write-Host '=== TERMINE ===' -ForegroundColor Green
Write-Host 'Redemarrer WaveRP puis Ctrl+F5 / reconnexion pour voir les nouvelles pages de blocs.' -ForegroundColor Green
