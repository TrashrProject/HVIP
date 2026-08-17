param([string]$Root='C:\HVIP')
$ErrorActionPreference='Stop'

$mysqlCandidates = @(
    'C:\xampp\mysql\bin\mysql.exe',
    'C:\xampp\mariadb\bin\mysql.exe'
)
$mysql = $mysqlCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $mysql) { throw 'mysql.exe / mariadb client introuvable dans C:\xampp.' }

$db = 'hv_rp'
Write-Host '=== ParadiseRP - Gobierno Federal : tenues RP ===' -ForegroundColor Cyan
Write-Host 'Copie uniquement les figures valides du Gobierno Central (job 11) vers Gobierno Federal (job 12).' -ForegroundColor DarkGray

$sql = @"
START TRANSACTION;

UPDATE play_jobs_ranks dst
INNER JOIN play_jobs_ranks src
    ON src.job = 11 AND src.rank = dst.rank
SET
    dst.male_figure = CASE
        WHEN src.male_figure IS NOT NULL
         AND src.male_figure <> ''
         AND src.male_figure NOT LIKE '%undefined%'
        THEN src.male_figure ELSE dst.male_figure END,
    dst.female_figure = CASE
        WHEN src.female_figure IS NOT NULL
         AND src.female_figure <> ''
         AND src.female_figure NOT LIKE '%undefined%'
        THEN src.female_figure ELSE dst.female_figure END
WHERE dst.job = 12;

COMMIT;

SELECT job, rank, name, male_figure, female_figure
FROM play_jobs_ranks
WHERE job = 12
ORDER BY rank;
"@

$tmp = Join-Path $env:TEMP ('paradiserp_job12_' + [guid]::NewGuid().ToString('N') + '.sql')
try {
    [IO.File]::WriteAllText($tmp, $sql, (New-Object Text.UTF8Encoding($false)))
    & $mysql -u root $db --default-character-set=utf8mb4 -e "source $($tmp.Replace('\','/'))"
    if ($LASTEXITCODE -ne 0) { throw "Import SQL echoue : $LASTEXITCODE" }
    Write-Host ''
    Write-Host 'Gobierno Federal configure. Les grades 1 a 6 utilisent maintenant des tenues gouvernementales valides.' -ForegroundColor Green
    Write-Host 'Ferme /play, rouvre puis Ctrl+F5.' -ForegroundColor Yellow
}
finally {
    Remove-Item $tmp -Force -ErrorAction SilentlyContinue
}
