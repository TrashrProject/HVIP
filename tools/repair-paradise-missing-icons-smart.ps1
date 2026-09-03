[CmdletBinding()]
param(
    [string]$RepositoryRoot = "C:\HVIP",
    [string]$Mysql = "C:\xampp\mysql\bin\mysql.exe",
    [string]$Database = "waveplus"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$targetFd   = Join-Path $RepositoryRoot 'swf_pz\V5-0-2\gamedata\json\FurnitureData.json'
$targetFurn = Join-Path $RepositoryRoot 'swf_pz\V5-0-2\furniture'
$targetIcon = Join-Path $RepositoryRoot 'swf_pz\V5-0-2\dcr\hof_furni\icon'
$reportPath = Join-Path $RepositoryRoot 'tools\repair-paradise-missing-icons-smart.csv'

foreach ($p in @($Mysql,$targetFd,$targetFurn,$targetIcon)) {
    if (-not (Test-Path -LiteralPath $p)) { throw "Ressource requise absente : $p" }
}

$fd = Get-Content -LiteralPath $targetFd -Raw | ConvertFrom-Json
$fdById = @{}
$fdByClass = @{}
foreach ($e in @($fd.roomitemtypes.furnitype)+@($fd.wallitemtypes.furnitype)) {
    if ($null -ne $e.id) { $fdById[[long]$e.id] = $e }
    if ($null -ne $e.classname -and -not $fdByClass.ContainsKey([string]$e.classname)) { $fdByClass[[string]$e.classname] = $e }
}

$furnSql = "SELECT id,item_name,public_name,type FROM furniture WHERE type IN ('s','i') AND item_name<>'';"
$catalogSql = "SELECT id,item_ids,page_id,catalog_name FROM catalog_items WHERE item_ids REGEXP '^[0-9]+';"
$furnRows = & $Mysql -u root -N -B --raw $Database -e $furnSql
if ($LASTEXITCODE -ne 0) { throw 'Lecture de furniture impossible.' }
$catRows = & $Mysql -u root -N -B --raw $Database -e $catalogSql
if ($LASTEXITCODE -ne 0) { throw 'Lecture de catalog_items impossible.' }

$furnById = @{}
foreach ($line in $furnRows) {
    $p = $line -split "`t",-1
    if ($p.Count -ge 4) {
        $furnById[[long]$p[0]] = [pscustomobject]@{ Id=[long]$p[0]; ClassName=$p[1]; PublicName=$p[2]; Type=$p[3] }
    }
}

Write-Host 'Indexation des PNG locaux...' -ForegroundColor Cyan
$pngIndex = @{}
Get-ChildItem -LiteralPath $RepositoryRoot -Filter *.png -File -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
    $key = $_.Name.ToLowerInvariant()
    if (-not $pngIndex.ContainsKey($key)) { $pngIndex[$key] = $_.FullName }
}

function Try-CopyLocalIcon([string]$assetName,[string]$destination) {
    $variants = @(
        ($assetName + '_icon.png'),
        ($assetName + '.png'),
        ('icon_' + $assetName + '.png'),
        ($assetName + '_small.png')
    )
    foreach ($v in $variants) {
        $k = $v.ToLowerInvariant()
        if ($pngIndex.ContainsKey($k)) {
            Copy-Item -LiteralPath $pngIndex[$k] -Destination $destination -Force
            return $pngIndex[$k]
        }
    }
    return $null
}

function Try-DownloadIcon([string]$assetName,[string]$destination) {
    $encoded = [uri]::EscapeDataString($assetName)
    $urls = @(
        "https://swf.habbovip.us/V5-0-2/dcr/hof_furni/icon/${encoded}_icon.png",
        "https://swfs.habbovip.us/V5-0-2/dcr/hof_furni/icon/${encoded}_icon.png"
    )
    foreach ($url in $urls) {
        try {
            Invoke-WebRequest -Uri $url -OutFile $destination -UseBasicParsing -TimeoutSec 12
            if ((Test-Path -LiteralPath $destination) -and ((Get-Item -LiteralPath $destination).Length -gt 100)) { return $url }
            Remove-Item -LiteralPath $destination -Force -ErrorAction SilentlyContinue
        } catch {
            Remove-Item -LiteralPath $destination -Force -ErrorAction SilentlyContinue
        }
    }
    return $null
}

$seen = [Collections.Generic.HashSet[long]]::new()
$report = [Collections.Generic.List[object]]::new()
$missingBefore=0; $localRecovered=0; $webRecovered=0; $stillMissing=0

foreach ($line in $catRows) {
    $p = $line -split "`t",-1
    if ($p.Count -lt 4) { continue }
    $firstId = (($p[1] -split '[,:;]')[0] -replace '[^0-9]','')
    if ([string]::IsNullOrWhiteSpace($firstId)) { continue }
    $fid=[long]$firstId
    if (-not $seen.Add($fid)) { continue }
    if (-not $furnById.ContainsKey($fid)) { continue }
    $f=$furnById[$fid]
    $className=[string]$f.ClassName
    $assetName=($className -split '\*')[0]
    if ([string]::IsNullOrWhiteSpace($assetName)) { continue }

    $iconPath = Join-Path $targetIcon ($assetName+'_icon.png')
    if (Test-Path -LiteralPath $iconPath) { continue }
    $missingBefore++

    $source = Try-CopyLocalIcon $assetName $iconPath
    $method = ''
    if ($null -ne $source) {
        $localRecovered++
        $method='local'
    } else {
        $source = Try-DownloadIcon $assetName $iconPath
        if ($null -ne $source) {
            $webRecovered++
            $method='cdn'
        } else {
            $stillMissing++
            $method='missing'
        }
    }

    $report.Add([pscustomobject]@{
        FurnitureId=$fid
        ClassName=$className
        AssetName=$assetName
        Method=$method
        Source=$source
        Destination=$iconPath
    })
}

$report | Export-Csv -LiteralPath $reportPath -NoTypeInformation -Encoding UTF8

Write-Host "`n=== RECUPERATION INTELLIGENTE ICONES ===" -ForegroundColor Cyan
Write-Host "Icones manquantes avant : $missingBefore" -ForegroundColor White
Write-Host "Recuperees depuis fichiers locaux avec noms alternatifs : $localRecovered" -ForegroundColor Green
Write-Host "Telechargees depuis CDN : $webRecovered" -ForegroundColor Green
Write-Host "Icones encore manquantes : $stillMissing" -ForegroundColor Yellow
Write-Host "Rapport : $reportPath" -ForegroundColor Cyan
