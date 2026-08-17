<?php
// Usage: C:\xampp\php\php.exe C:\HVIP\tools\seed-hospital-outfits.php
// Remplit play_jobs_outfits pour l'Hôpital (job 1) avec plusieurs variantes par grade existant.

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
    'medical','médical','medic','médic','doctor','docteur','médecin','medecin',
    'hospital','hôpital','hopital','nurse','infirm','surgeon','chirurg',
    'ambulance','paramedic','paramédic','emergency','urgence','health','santé','sante'
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
        'name' => trim((string)($o['name'] ?? 'Tenue Hôpital')),
        'figure' => $figure
    ];
    if ($gender === 'F') $female[] = $entry;
    else $male[] = $entry;
}

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
$res = $db->query("SELECT rank, name, male_figure, female_figure FROM play_jobs_ranks WHERE job=1 ORDER BY rank ASC");
if (!$res || !$res->num_rows) {
    fwrite(STDERR, "Aucun grade trouvé pour l'Hôpital (job 1).\n");
    exit(4);
}
while ($row = $res->fetch_assoc()) $ranks[] = $row;

$db->query("DELETE FROM play_jobs_outfits WHERE job_id=1");

$variantNames = [
    'Consultation',
    'Urgences',
    'Bloc opératoire',
    'Réanimation',
    'SAMU / Ambulance',
    'Garde de nuit',
    'Chef de service',
    'Direction médicale'
];

$maleIndex = 0;
$femaleIndex = 0;
$inserted = 0;
$stmt = $db->prepare("INSERT INTO play_jobs_outfits (job_id, rank_id, name, male_figure, female_figure, sort_order, enabled) VALUES (1, ?, ?, ?, ?, ?, 1)");
if (!$stmt) {
    fwrite(STDERR, "Prepare SQL impossible: {$db->error}\n");
    exit(5);
}

foreach ($ranks as $rankRow) {
    $rank = (int)$rankRow['rank'];
    $rankName = trim((string)$rankRow['name']);
    $baseMale = cleanFigure($rankRow['male_figure'] ?? '');
    $baseFemale = cleanFigure($rankRow['female_figure'] ?? '');

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

echo "=== ParadiseRP - Pack Hôpital RP ===\n";
echo "Grades détectés : " . count($ranks) . "\n";
echo "Presets médicaux homme détectés : " . count($male) . "\n";
echo "Presets médicaux femme détectés : " . count($female) . "\n";
echo "Tenues ajoutées dans play_jobs_outfits : {$inserted}\n\n";

$check = $db->query("SELECT rank_id, COUNT(*) AS total FROM play_jobs_outfits WHERE job_id=1 GROUP BY rank_id ORDER BY rank_id");
if ($check) {
    while ($row = $check->fetch_assoc()) {
        echo " - Grade {$row['rank_id']} : {$row['total']} variantes\n";
    }
}

if (!$male && !$female) {
    echo "\nATTENTION: aucun preset médical n'a été trouvé dans rp-outfits.json.\n";
    echo "Les variantes utilisent alors le look standard de play_jobs_ranks.\n";
}

$db->close();
