<?php
// Usage: C:\xampp\php\php.exe C:\HVIP\tools\seed-police-outfits.php
// Remplit play_jobs_outfits pour la Police (job 9) en utilisant les presets RP déjà générés.

$root = dirname(__DIR__);
$catalogFile = $root . DIRECTORY_SEPARATOR . 'WebPixel' . DIRECTORY_SEPARATOR . 'nitro-last' . DIRECTORY_SEPARATOR . 'rp-outfits.json';

if (!is_file($catalogFile)) {
    fwrite(STDERR, "rp-outfits.json introuvable: {$catalogFile}\n");
    exit(1);
}

$db = @new mysqli('127.0.0.1', 'root', '', 'hv_rp');
if ($db->connect_errno) {
    fwrite(STDERR, "Connexion MariaDB impossible: {$db->connect_error}\n");
    exit(2);
}
$db->set_charset('utf8mb4');

$catalog = json_decode(file_get_contents($catalogFile), true);
if (!is_array($catalog)) {
    fwrite(STDERR, "Catalogue RP JSON invalide.\n");
    exit(3);
}

function cleanFigure($value) {
    $value = trim((string)$value);
    if ($value === '' || stripos($value, 'undefined') !== false) return '';
    return preg_match('/^[a-z0-9.\-]+$/i', $value) ? $value : '';
}

function normText($o) {
    return strtolower(trim(
        (string)($o['name'] ?? '') . ' ' .
        (string)($o['category'] ?? '') . ' ' .
        (string)($o['categoryLabel'] ?? '') . ' ' .
        (string)($o['source'] ?? '')
    ));
}

$keywords = [
    'police','policia','policía','swat','security','seguridad','guard','garde',
    'military','militaire','militar','army','tactical','tactique','sheriff','agent',
    'officer','oficial','detective','intervention','riot','fbi','cia'
];

$male = [];
$female = [];
foreach (($catalog['outfits'] ?? []) as $o) {
    $text = normText($o);
    $match = false;
    foreach ($keywords as $kw) {
        if (strpos($text, $kw) !== false) { $match = true; break; }
    }
    if (!$match) continue;

    $figure = cleanFigure($o['figure'] ?? '');
    if ($figure === '') continue;

    $gender = strtoupper((string)($o['gender'] ?? 'M')) === 'F' ? 'F' : 'M';
    $entry = [
        'name' => trim((string)($o['name'] ?? 'Tenue Police')),
        'figure' => $figure
    ];

    if ($gender === 'F') $female[] = $entry;
    else $male[] = $entry;
}

// Déduplique par figure.
$dedupe = function(array $items) {
    $seen = [];
    $out = [];
    foreach ($items as $item) {
        if (isset($seen[$item['figure']])) continue;
        $seen[$item['figure']] = true;
        $out[] = $item;
    }
    return $out;
};
$male = $dedupe($male);
$female = $dedupe($female);

$ranks = [];
$res = $db->query("SELECT rank, name, male_figure, female_figure FROM play_jobs_ranks WHERE job=9 ORDER BY rank ASC");
if (!$res || !$res->num_rows) {
    fwrite(STDERR, "Aucun grade trouvé pour le job Police (9).\n");
    exit(4);
}
while ($row = $res->fetch_assoc()) $ranks[] = $row;

// On recrée uniquement les variantes Police générées par ce script.
$db->query("DELETE FROM play_jobs_outfits WHERE job_id=9");

$variantNames = [
    'Tenue de service',
    'Tenue de patrouille',
    'Tenue d’intervention',
    'Tenue tactique',
    'Tenue cérémonie',
    'Tenue opération spéciale'
];

$maleIndex = 0;
$femaleIndex = 0;
$inserted = 0;

$stmt = $db->prepare("INSERT INTO play_jobs_outfits (job_id, rank_id, name, male_figure, female_figure, sort_order, enabled) VALUES (9, ?, ?, ?, ?, ?, 1)");
if (!$stmt) {
    fwrite(STDERR, "Prepare SQL impossible: {$db->error}\n");
    exit(5);
}

foreach ($ranks as $rankRow) {
    $rank = (int)$rankRow['rank'];
    $rankName = trim((string)$rankRow['name']);
    $baseMale = cleanFigure($rankRow['male_figure'] ?? '');
    $baseFemale = cleanFigure($rankRow['female_figure'] ?? '');

    // 6 variantes par grade. Si le catalogue manque de looks, on retombe sur le look standard du grade.
    for ($i = 0; $i < count($variantNames); $i++) {
        $m = $male[$maleIndex % max(1, count($male))]['figure'] ?? $baseMale;
        $f = $female[$femaleIndex % max(1, count($female))]['figure'] ?? $baseFemale;

        if ($m === '') $m = $baseMale;
        if ($f === '') $f = $baseFemale;
        if ($m === '' && $f === '') continue;

        $name = $rankName . ' — ' . $variantNames[$i];
        $sort = ($rank * 100) + ($i + 1);

        $stmt->bind_param('isssi', $rank, $name, $m, $f, $sort);
        if ($stmt->execute()) $inserted++;

        if (count($male)) $maleIndex++;
        if (count($female)) $femaleIndex++;
    }
}

$stmt->close();

echo "=== ParadiseRP - Pack Police RP ===\n";
echo "Grades détectés : " . count($ranks) . "\n";
echo "Presets Police homme détectés : " . count($male) . "\n";
echo "Presets Police femme détectés : " . count($female) . "\n";
echo "Tenues ajoutées dans play_jobs_outfits : {$inserted}\n\n";

$check = $db->query("SELECT rank_id, COUNT(*) AS total FROM play_jobs_outfits WHERE job_id=9 GROUP BY rank_id ORDER BY rank_id");
if ($check) {
    while ($row = $check->fetch_assoc()) {
        echo " - Grade {$row['rank_id']} : {$row['total']} variantes\n";
    }
}

if (!$male && !$female) {
    echo "\nATTENTION: aucun preset Police/SWAT n'a été trouvé dans rp-outfits.json.\n";
    echo "Les variantes utilisent alors seulement les looks standards déjà présents dans play_jobs_ranks.\n";
}

$db->close();
