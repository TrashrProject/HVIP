<?php
require_once __DIR__ . '/app/init.pz.php';

header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store');

function fail_json($code, $message) {
    http_response_code($code);
    echo json_encode(['ok' => false, 'error' => $message], JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}
function clean_figure($value) {
    $value = trim((string)$value);
    if ($value === '' || stripos($value, 'undefined') !== false) return '';
    return preg_match('/^[a-z0-9.\-]+$/i', $value) ? $value : '';
}

if (!$Session->Exist(Config::$SessionName) || !isset($UData['id'])) fail_json(401, 'Session expirée');

$data = json_decode(file_get_contents('php://input') ?: '{}', true);
$outfitId = isset($data['id']) ? (string)$data['id'] : '';
$uid = (int)$UData['id'];
$gender = strtoupper((string)($UData['gender'] ?? 'M')) === 'F' ? 'F' : 'M';
$userRank = (int)($UData['rank'] ?? 1);
$figure = '';
$name = 'Tenue RP';

if (preg_match('/^job-(\d+)-rank-(\d+)$/', $outfitId, $m)) {
    $jobId = (int)$m[1];
    $rank = (int)$m[2];
    $membership = $DB->Query("SELECT gm.rank AS member_rank FROM group_memberships gm WHERE gm.user_id='" . $uid . "' AND gm.group_id='" . $jobId . "' LIMIT 1");
    if (!$membership || mysqli_num_rows($membership) === 0) fail_json(403, 'Tu ne fais pas partie de ce métier');
    $member = mysqli_fetch_assoc($membership);
    if ($rank < 1 || $rank > (int)$member['member_rank']) fail_json(403, 'Cette tenue dépasse ton grade');

    $rows = $DB->Query("SELECT name, male_figure, female_figure FROM play_jobs_ranks WHERE job='" . $jobId . "' AND rank='" . $rank . "' LIMIT 1");
    if (!$rows || mysqli_num_rows($rows) === 0) fail_json(404, 'Tenue métier introuvable');
    $row = mysqli_fetch_assoc($rows);
    $figure = clean_figure($gender === 'F' ? $row['female_figure'] : $row['male_figure']);
    $name = (string)$row['name'];
    if ($figure === '') fail_json(422, 'Cette tenue métier n’est pas configurée pour ton genre');
} elseif (preg_match('/^staff-(rp-\d+)$/', $outfitId, $m)) {
    $isStaff = $userRank >= 6;
    if (!$isStaff) {
        $staffQuery = $DB->Query("SELECT g.name FROM group_memberships gm INNER JOIN groups g ON g.id=gm.group_id WHERE gm.user_id='" . $uid . "'");
        if ($staffQuery) {
            while ($row = mysqli_fetch_assoc($staffQuery)) {
                if (preg_match('/staff|fondateur|founder|gerant|gérant|developpeur|développeur|developer|administrat|owner/i', (string)$row['name'])) { $isStaff = true; break; }
            }
        }
    }
    if (!$isStaff) fail_json(403, 'Cette tenue est réservée au staff');

    $file = __DIR__ . '/nitro-last/rp-outfits.json';
    if (!is_file($file)) fail_json(503, 'Catalogue staff indisponible');
    $catalog = json_decode(file_get_contents($file), true);
    $selected = null;
    foreach (($catalog['outfits'] ?? []) as $o) if (($o['id'] ?? '') === $m[1]) { $selected = $o; break; }
    if (!$selected) fail_json(404, 'Tenue staff introuvable');
    $selectedGender = strtoupper((string)($selected['gender'] ?? 'M')) === 'F' ? 'F' : 'M';
    if ($selectedGender !== $gender) fail_json(422, 'Tenue incompatible avec ton genre');
    $figure = clean_figure($selected['figure'] ?? '');
    $name = (string)($selected['name'] ?? 'Tenue staff');
    if ($figure === '') fail_json(422, 'Données de tenue invalides');
} else {
    fail_json(400, 'Tenue invalide');
}

$escapedFigure = mysqli_real_escape_string($DB->Connection, $figure);
$escapedGender = mysqli_real_escape_string($DB->Connection, $gender);
$result = $DB->Query("UPDATE users SET look='" . $escapedFigure . "', gender='" . $escapedGender . "' WHERE id='" . $uid . "' LIMIT 1");
if ($result === false) fail_json(500, 'Impossible de sauvegarder la tenue');

echo json_encode(['ok' => true, 'id' => $outfitId, 'name' => $name, 'figure' => $figure, 'gender' => $gender, 'reload' => true], JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
