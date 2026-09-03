[CmdletBinding()]
param(
    [string]$Mysql = "C:\xampp\mysql\bin\mysql.exe",
    [string]$Database = "waveplus"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $Mysql)) { throw "mysql.exe introuvable : $Mysql" }

Write-Host '=== AUDIT ARBORESCENCE CATALOGUE ACTUEL ===' -ForegroundColor Cyan

$pageSql = @"
SELECT p.id,p.parent_id,p.caption,p.visible,p.enabled,p.min_rank,p.order_num,
       COUNT(ci.id) AS item_count
FROM catalog_pages p
LEFT JOIN catalog_items ci ON ci.page_id=p.id
GROUP BY p.id,p.parent_id,p.caption,p.visible,p.enabled,p.min_rank,p.order_num
ORDER BY p.parent_id,p.order_num,p.id;
"@

$rows = & $Mysql -u root -N -B --raw $Database -e $pageSql
if ($LASTEXITCODE -ne 0) { throw 'Lecture catalog_pages impossible.' }

$parsed = foreach($line in $rows) {
    $p = $line -split "`t",-1
    if($p.Count -lt 8){ continue }
    [pscustomobject]@{
        Id=[int64]$p[0]
        ParentId=[int64]$p[1]
        Caption=$p[2]
        Visible=$p[3]
        Enabled=$p[4]
        MinRank=[int]$p[5]
        OrderNum=[int]$p[6]
        ItemCount=[int]$p[7]
    }
}

$report = Join-Path $PSScriptRoot 'catalog-tree-current.csv'
$parsed | Export-Csv -LiteralPath $report -NoTypeInformation -Encoding UTF8

Write-Host "Pages totales : $($parsed.Count)" -ForegroundColor White
Write-Host "Pages visibles + actives : $(@($parsed | Where-Object { $_.Visible -eq '1' -and $_.Enabled -eq '1' }).Count)" -ForegroundColor Green
Write-Host "Pages visibles avec mobis : $(@($parsed | Where-Object { $_.Visible -eq '1' -and $_.Enabled -eq '1' -and $_.ItemCount -gt 0 }).Count)" -ForegroundColor Green

Write-Host ''
Write-Host 'Racines / parents principaux visibles :' -ForegroundColor Cyan
$roots = @($parsed | Where-Object { $_.Visible -eq '1' -and $_.Enabled -eq '1' -and ($_.ParentId -eq -1 -or $_.ParentId -eq 0) })
if($roots.Count -eq 0){
    $parentIds = @($parsed | Where-Object { $_.Visible -eq '1' -and $_.Enabled -eq '1' } | Group-Object ParentId | Sort-Object Count -Descending | Select-Object -First 15)
    foreach($g in $parentIds){ Write-Host (" - parent_id {0}: {1} pages" -f $g.Name,$g.Count) }
} else {
    foreach($r in $roots){ Write-Host (" - [{0}] {1} | items directs={2}" -f $r.Id,$r.Caption,$r.ItemCount) }
}

Write-Host ''
Write-Host 'Top 40 pages visibles par nombre de mobis :' -ForegroundColor Cyan
$parsed | Where-Object { $_.Visible -eq '1' -and $_.Enabled -eq '1' -and $_.ItemCount -gt 0 } |
    Sort-Object ItemCount -Descending |
    Select-Object -First 40 |
    ForEach-Object { Write-Host (" - id={0} parent={1} items={2} ordre={3} | {4}" -f $_.Id,$_.ParentId,$_.ItemCount,$_.OrderNum,$_.Caption) }

Write-Host ''
Write-Host 'Parents visibles contenant le plus de sous-pages :' -ForegroundColor Cyan
$parsed | Where-Object { $_.Visible -eq '1' -and $_.Enabled -eq '1' } |
    Group-Object ParentId |
    Sort-Object Count -Descending |
    Select-Object -First 20 |
    ForEach-Object {
        $parent = $parsed | Where-Object Id -eq [int64]$_.Name | Select-Object -First 1
        $name = if($parent){$parent.Caption}else{'(parent non trouve)'}
        Write-Host (" - parent={0} | sous-pages={1} | {2}" -f $_.Name,$_.Count,$name)
    }

Write-Host ''
Write-Host "Rapport : $report" -ForegroundColor Cyan
Write-Host 'AUCUNE modification BDD effectuee.' -ForegroundColor Green
