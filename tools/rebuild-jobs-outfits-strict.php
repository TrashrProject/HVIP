<?php
// Usage: C:\xampp\php\php.exe C:\HVIP\tools\rebuild-jobs-outfits-strict.php
// Reconstruit play_jobs_outfits en mode STRICT : aucun preset fantaisie n'est accepté
// sauf s'il partage réellement les pièces principales du look de référence du grade.

$root = dirname(__DIR__);
$catalogFile = $root . DIRECTORY_SEPARATOR . 'WebPixel' . DIRECTORY_SEPARATOR . 'nitro-last' . DIRECTORY_SEPARATOR . 'rp-outfits.json';
$db = @new mysqli('127.0.0.1', 'root', '', 'hv_rp');
if ($db->connect_errno) { fwrite(STDERR, "Connexion MariaDB impossible: {$db->connect_error}\n"); exit(1); }
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
function figureParts($figure) {
    $out = [];
    foreach (explode('.', (string)$figure) as $part) {
        if (preg_match('/^([a-z]{2})-(\d+)(?:-(\d+))?/i', $part, $m)) {
            $out[strtolower($m[1])] = ['set'=>(int)$m[2], 'raw'=>$part];
        }
    }
    return $out;
}
function stripTypes($figure, $types) {
    $types = array_flip($types);
    $parts = [];
    foreach (explode('.', (string)$figure) as $part) {
        if (!preg_match('/^([a-z]{2})-/i', $part, $m)) continue;
        if (isset($types[strtolower($m[1])])) continue;
        $parts[] = $part;
    }
    return implode('.', $parts);
}
function similarity($base, $candidate) {
    $a = figureParts($base); $b = figureParts($candidate);
    // Pièces principales : chemise/haut, veste, pantalon, chaussures, manteau.
    $core = ['ch','cc','cp','lg','sh'];
    $score = 0; $same = 0;
    foreach ($core as $t) {
        if (isset($a[$t], $b[$t])) {
            if ($a[$t]['set'] === $b[$t]['set']) { $score += 4; $same++; }
            else $score -= 2;
        }
    }
    // Accessoires communs = petit bonus uniquement.
    foreach (['ca','wa','ha','he','ea','fa'] as $t) {
        if (isset($a[$t],$b[$t]) && $a[$t]['set'] === $b[$t]['set']) $score++;
    }
    // Refuse les effets spéciaux et looks sans vraie proximité textile.
    if (isset($b['fx'])) $score -= 20;
    return [$score, $same];
}

$jobsRes = $db->query("SELECT DISTINCT r.job, g.name FROM play_jobs_ranks r INNER JOIN groups g ON g.id=r.job ORDER BY r.job");
if (!$jobsRes) { fwrite(STDERR, "Impossible de lire les métiers.\n"); exit(2); }

$total = 0; $jobs = 0;
while ($job = $jobsRes->fetch_assoc()) {
    $jobId = (int)$job['job'];
    $jobName = trim((string)$job['name']);
    if (preg_match('/staff|fondateur|founder|gerant|gérant|developpeur|développeur|developer|administrat|owner/i', $jobName)) continue;

    $ranks = $db->query("SELECT rank,name,male_figure,female_figure FROM play_jobs_ranks WHERE job={$jobId} ORDER BY rank");
    if (!$ranks || !$ranks->num_rows) continue;

    $db->query("DELETE FROM play_jobs_outfits WHERE job_id={$jobId}");
    $stmt = $db->prepare("INSERT INTO play_jobs_outfits (job_id,rank_id,name,male_figure,female_figure,sort_order,enabled) VALUES (?,?,?,?,?,?,1)");
    if (!$stmt) continue;

    $inserted = 0;
    while ($rankRow = $ranks->fetch_assoc()) {
        $rank = (int)$rankRow['rank'];
        $rankName = trim((string)$rankRow['name']);
        $baseM = cleanFigure($rankRow['male_figure'] ?? '');
        $baseF = cleanFigure($rankRow['female_figure'] ?? '');
        if ($baseM === '' && $baseF === '') continue;

        // Variantes garanties sûres car construites à partir du look officiel du grade.
        $safeVariants = [
            ['Service', $baseM, $baseF],
            ['Sans couvre-chef', stripTypes($baseM,['ha','he']), stripTypes($baseF,['ha','he'])],
            ['Sans accessoires visage', stripTypes($baseM,['ha','he','ea','fa']), stripTypes($baseF,['ha','he','ea','fa'])],
        ];

        // Cherche au maximum 2 vraies variantes customs TRÈS proches du textile de référence.
        $bestM = []; $bestF = [];
        foreach ($catalog as $o) {
            $fig = cleanFigure($o['figure'] ?? ''); if ($fig === '') continue;
            $gender = strtoupper((string)($o['gender'] ?? 'M')) === 'F' ? 'F' : 'M';
            $base = $gender === 'F' ? $baseF : $baseM; if ($base === '') continue;
            list($score,$same) = similarity($base,$fig);
            if ($same < 2 || $score < 6) continue;
            $entry = ['score'=>$score,'figure'=>$fig];
            if ($gender === 'F') $bestF[$fig] = $entry; else $bestM[$fig] = $entry;
        }
        uasort($bestM, fn($a,$b)=>$b['score']<=>$a['score']);
        uasort($bestF, fn($a,$b)=>$b['score']<=>$a['score']);
        $bm = array_values($bestM); $bf = array_values($bestF);
        for ($i=0;$i<2;$i++) {
            $m = $bm[$i]['figure'] ?? '';
            $f = $bf[$i]['figure'] ?? '';
            if ($m === '' && $f === '') continue;
            if ($m === '') $m = $baseM;
            if ($f === '') $f = $baseF;
            $safeVariants[] = ['Variante compatible '.($i+1), $m, $f];
        }

        $seen = [];
        $idx = 0;
        foreach ($safeVariants as $v) {
            [$label,$m,$f] = $v;
            $m = cleanFigure($m); $f = cleanFigure($f);
            if ($m === '' && $f === '') continue;
            $key = $m.'|'.$f;
            if (isset($seen[$key])) continue;
            $seen[$key] = true;
            $idx++;
            $name = $rankName . ' — ' . $label;
            $sort = ($rank*100)+$idx;
            $jid=$jobId;
            $stmt->bind_param('iisssi',$jid,$rank,$name,$m,$f,$sort);
            if ($stmt->execute()) { $inserted++; $total++; }
        }
    }
    $stmt->close(); $jobs++;
    echo "[OK] Job {$jobId} {$jobName} : {$inserted} variantes strictes\n";
}

echo "\n=== Reconstruction STRICT terminée ===\n";
echo "Métiers traités : {$jobs}\n";
echo "Variantes conservées : {$total}\n";
echo "Les presets sans proximité textile réelle ont été supprimés.\n";
$db->close();
