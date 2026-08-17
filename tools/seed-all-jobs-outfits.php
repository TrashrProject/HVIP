<?php
// Usage: C:\xampp\php\php.exe C:\HVIP\tools\seed-all-jobs-outfits.php
// Génère en une fois les variantes RP pour tous les métiers configurés.

$root = dirname(__DIR__);
$catalogFile = $root . DIRECTORY_SEPARATOR . 'WebPixel' . DIRECTORY_SEPARATOR . 'nitro-last' . DIRECTORY_SEPARATOR . 'rp-outfits.json';

$db = @new mysqli('127.0.0.1', 'root', '', 'hv_rp');
if ($db->connect_errno) {
    fwrite(STDERR, "Connexion MariaDB impossible: {$db->connect_error}\n");
    exit(1);
}
$db->set_charset('utf8mb4');

$catalog = [];
if (is_file($catalogFile)) {
    $tmp = json_decode(file_get_contents($catalogFile), true);
    if (is_array($tmp)) $catalog = $tmp['outfits'] ?? [];
}

function cleanFigure($value) {
    $value = trim((string)$value);
    if ($value === '' || stripos($value, 'undefined') !== false) return '';
    return preg_match('/^[a-z0-9.\-]+$/i', $value) ? $value : '';
}

function textOfOutfit($o) {
    return strtolower(trim(
        (string)($o['name'] ?? '') . ' ' .
        (string)($o['category'] ?? '') . ' ' .
        (string)($o['categoryLabel'] ?? '') . ' ' .
        (string)($o['source'] ?? '')
    ));
}

function collectPresets($catalog, $keywords) {
    $male = [];
    $female = [];
    foreach ($catalog as $o) {
        $text = textOfOutfit($o);
        $matched = false;
        foreach ($keywords as $kw) {
            if ($kw !== '' && strpos($text, strtolower($kw)) !== false) { $matched = true; break; }
        }
        if (!$matched) continue;
        $figure = cleanFigure($o['figure'] ?? '');
        if ($figure === '') continue;
        $gender = strtoupper((string)($o['gender'] ?? 'M')) === 'F' ? 'F' : 'M';
        $entry = ['figure' => $figure, 'name' => trim((string)($o['name'] ?? 'Preset RP'))];
        if ($gender === 'F') $female[$figure] = $entry;
        else $male[$figure] = $entry;
    }
    return [array_values($male), array_values($female)];
}

$profiles = [
    1 => [
        'label' => 'Hospital',
        'keywords' => ['medical','medic','doctor','docteur','hospital','hopital','hôpital','nurse','infirm','ambulance','samu','surgeon'],
        'variants' => ['Consultation','Urgences','Bloc opératoire','Réanimation','SAMU / Ambulance','Garde de nuit','Chef de service','Direction médicale']
    ],
    2 => [
        'label' => 'Basurero',
        'keywords' => ['worker','ouvrier','garbage','trash','clean','cleaner','maintenance','utility','construction'],
        'variants' => ['Service urbain','Collecte matin','Collecte nuit','Haute visibilité','Maintenance','Chef d’équipe']
    ],
    3 => [
        'label' => 'Taller Mecánico',
        'keywords' => ['mechanic','mecano','mécano','garage','worker','workshop','repair','maintenance','engineer'],
        'variants' => ['Atelier standard','Diagnostic','Réparation lourde','Dépannage','Chef d’atelier','Direction garage']
    ],
    4 => [
        'label' => 'Fábrica de Armas',
        'keywords' => ['military','militar','army','security','tactical','worker','factory','industrial'],
        'variants' => ['Production','Contrôle qualité','Entrepôt','Sécurité interne','Superviseur','Direction usine']
    ],
    6 => [
        'label' => 'Camioneros',
        'keywords' => ['driver','truck','camion','worker','transport','road','delivery','logistic'],
        'variants' => ['Route standard','Long trajet','Livraison','Entrepôt','Chef de convoi','Direction transport']
    ],
    7 => [
        'label' => 'Guardaespaldas',
        'keywords' => ['security','guard','bodyguard','garde','tactical','agent','suit','formal','military'],
        'variants' => ['Protection rapprochée','Service discret','Intervention','Escorte VIP','Chef sécurité','Direction protection']
    ],
    8 => [
        'label' => 'Mineros',
        'keywords' => ['miner','mine','worker','construction','industrial','helmet','ouvrier'],
        'variants' => ['Extraction','Galerie','Sécurité mine','Maintenance','Chef d’équipe','Direction minière']
    ],
    9 => [
        'label' => 'Policía',
        'keywords' => ['police','policia','policía','swat','security','sheriff','officer','oficial','tactical','riot','fbi'],
        'variants' => ['Service','Patrouille','Intervention','Tactique','Cérémonie','Opération spéciale']
    ],
    10 => [
        'label' => "McDonald's",
        'keywords' => ['restaurant','food','fastfood','cook','chef','waiter','server','cashier','kitchen','uniform'],
        'variants' => ['Service comptoir','Caisse','Cuisine','Rush','Supervision','Direction']
    ],
    11 => [
        'label' => 'Gobierno Central',
        'keywords' => ['government','gouvernement','justice','formal','suit','business','office','president','senator','diplomat'],
        'variants' => ['Officielle','Réunion gouvernementale','Cérémonie','Diplomatique','Déplacement officiel','Protocole','Prestige','Direction']
    ],
    12 => [
        'label' => 'Gobierno Federal',
        'keywords' => ['government','gouvernement','justice','formal','suit','business','office','president','senator','diplomat'],
        'variants' => ['Officielle','Réunion fédérale','Cérémonie','Diplomatique','Déplacement officiel','Protocole','Prestige','Direction']
    ],
    13 => [
        'label' => 'Cafetería Bobba Ball',
        'keywords' => ['restaurant','cafe','café','food','cook','chef','waiter','server','cashier','kitchen','uniform'],
        'variants' => ['Service salle','Caisse','Cuisine','Service premium','Supervision','Direction']
    ],
    14 => [
        'label' => 'Subway',
        'keywords' => ['restaurant','food','fastfood','cook','chef','waiter','server','cashier','kitchen','uniform'],
        'variants' => ['Service comptoir','Caisse','Préparation','Rush','Supervision','Direction']
    ],
    15 => [
        'label' => 'Heladería',
        'keywords' => ['restaurant','ice','glace','food','waiter','server','cashier','uniform','shop'],
        'variants' => ['Service glace','Caisse','Préparation','Service terrasse','Supervision','Direction']
    ],
    16 => [
        'label' => 'Bubble Juice',
        'keywords' => ['restaurant','drink','juice','bubble','food','waiter','server','cashier','uniform','shop'],
        'variants' => ['Service bar','Caisse','Préparation boissons','Rush','Supervision','Direction']
    ]
];

// Ne touche qu'aux jobs explicitement listés ci-dessus; aucun groupe staff.
$totalInserted = 0;
$totalJobs = 0;

foreach ($profiles as $jobId => $profile) {
    $ranks = [];
    $res = $db->query("SELECT rank, name, male_figure, female_figure FROM play_jobs_ranks WHERE job=" . (int)$jobId . " ORDER BY rank ASC");
    if (!$res || !$res->num_rows) {
        echo "[SKIP] Job {$jobId} {$profile['label']} : aucun grade.\n";
        continue;
    }
    while ($row = $res->fetch_assoc()) $ranks[] = $row;

    list($malePresets, $femalePresets) = collectPresets($catalog, $profile['keywords']);

    // Fallback gouvernement fédéral: si figures de base vides, on reprend le même rang du gouvernement central.
    $fallbackGov = [];
    if ($jobId === 12) {
        $fg = $db->query("SELECT rank, male_figure, female_figure FROM play_jobs_ranks WHERE job=11 ORDER BY rank ASC");
        if ($fg) while ($r = $fg->fetch_assoc()) $fallbackGov[(int)$r['rank']] = $r;
    }

    $db->query("DELETE FROM play_jobs_outfits WHERE job_id=" . (int)$jobId);
    $stmt = $db->prepare("INSERT INTO play_jobs_outfits (job_id, rank_id, name, male_figure, female_figure, sort_order, enabled) VALUES (?, ?, ?, ?, ?, ?, 1)");
    if (!$stmt) {
        echo "[ERREUR] Job {$jobId}: prepare SQL impossible.\n";
        continue;
    }

    $mIndex = 0;
    $fIndex = 0;
    $inserted = 0;

    foreach ($ranks as $rankRow) {
        $rank = (int)$rankRow['rank'];
        $rankName = trim((string)$rankRow['name']);
        $baseMale = cleanFigure($rankRow['male_figure'] ?? '');
        $baseFemale = cleanFigure($rankRow['female_figure'] ?? '');

        if ($jobId === 12 && isset($fallbackGov[$rank])) {
            if ($baseMale === '') $baseMale = cleanFigure($fallbackGov[$rank]['male_figure'] ?? '');
            if ($baseFemale === '') $baseFemale = cleanFigure($fallbackGov[$rank]['female_figure'] ?? '');
        }

        foreach ($profile['variants'] as $i => $variant) {
            $m = count($malePresets) ? $malePresets[$mIndex % count($malePresets)]['figure'] : $baseMale;
            $f = count($femalePresets) ? $femalePresets[$fIndex % count($femalePresets)]['figure'] : $baseFemale;

            if ($m === '') $m = $baseMale;
            if ($f === '') $f = $baseFemale;
            if ($m === '' && $f === '') continue;

            $name = $rankName . ' — ' . $variant;
            $sort = ($rank * 100) + ($i + 1);
            $jid = (int)$jobId;
            $stmt->bind_param('iisssi', $jid, $rank, $name, $m, $f, $sort);
            if ($stmt->execute()) $inserted++;

            if (count($malePresets)) $mIndex++;
            if (count($femalePresets)) $fIndex++;
        }
    }
    $stmt->close();

    $totalInserted += $inserted;
    $totalJobs++;
    echo "[OK] Job {$jobId} {$profile['label']} : " . count($ranks) . " grades, {$inserted} tenues";
    echo " | presets H:" . count($malePresets) . " F:" . count($femalePresets) . "\n";
}

echo "\n=== ParadiseRP - Génération globale terminée ===\n";
echo "Métiers traités : {$totalJobs}\n";
echo "Tenues générées : {$totalInserted}\n\n";

$check = $db->query("SELECT pjo.job_id, g.name, COUNT(*) AS total FROM play_jobs_outfits pjo LEFT JOIN groups g ON g.id=pjo.job_id GROUP BY pjo.job_id, g.name ORDER BY pjo.job_id");
if ($check) {
    while ($row = $check->fetch_assoc()) {
        echo " - Job {$row['job_id']} " . ($row['name'] ?: '?') . " : {$row['total']} tenues\n";
    }
}

$db->close();
