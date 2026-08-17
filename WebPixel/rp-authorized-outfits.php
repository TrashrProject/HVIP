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
    if (!preg_match('/^[a-z0-9.\-]+$/i', $value)) return '';
    return $value;
}

if (!$Session->Exist(Config::$SessionName) || !isset($UData['id'])) fail_json(401, 'Session expirée');

$uid = (int)$UData['id'];
$gender = strtoupper((string)($UData['gender'] ?? 'M')) === 'F' ? 'F' : 'M';
$userRank = (int)($UData['rank'] ?? 1);

$memberships = [];
$sql = "SELECT gm.group_id, gm.rank AS member_rank, g.name, g.activity
        FROM group_memberships gm
        INNER JOIN groups g ON g.id = gm.group_id
        WHERE gm.user_id = '" . $uid . "'
        ORDER BY gm.group_id ASC";
$res = $DB->Query($sql);
if ($res) while ($row = mysqli_fetch_assoc($res)) $memberships[] = $row;

$isStaff = false;
$staffName = '';
$jobMemberships = [];
foreach ($memberships as $m) {
    $name = trim((string)$m['name']);
    if (preg_match('/staff|fondateur|founder|gerant|gérant|developpeur|développeur|developer|administrat|owner/i', $name)) {
        $isStaff = true;
        if ($staffName === '') $staffName = $name;
        continue;
    }
    $jobMemberships[(int)$m['group_id']] = [
        'rank' => (int)$m['member_rank'],
        'name' => $name,
        'activity' => (string)$m['activity']
    ];
}
if (!$isStaff && $userRank >= 6) {
    $isStaff = true;
    $staffName = 'Staff ParadiseRP';
}

$outfits = [];
$categories = [];
foreach ($jobMemberships as $jobId => $membership) {
    $memberRank = max(1, (int)$membership['rank']);
    $rows = $DB->Query("SELECT job, rank, name, male_figure, female_figure FROM play_jobs_ranks WHERE job='" . $jobId . "' AND rank <= '" . $memberRank . "' ORDER BY rank ASC");
    $countBefore = count($outfits);
    if ($rows) {
        while ($row = mysqli_fetch_assoc($rows)) {
            $figure = clean_figure($gender === 'F' ? $row['female_figure'] : $row['male_figure']);
            if ($figure === '') continue;
            $rank = (int)$row['rank'];
            $outfits[] = [
                'id' => 'job-' . $jobId . '-rank-' . $rank,
                'kind' => 'job',
                'job_id' => $jobId,
                'rank' => $rank,
                'name' => (string)$row['name'],
                'category' => 'job-' . $jobId,
                'categoryLabel' => $membership['name'],
                'icon' => '💼',
                'gender' => $gender,
                'figure' => $figure,
                'source' => 'Grade ' . $rank . ' / ' . $memberRank
            ];
        }
    }
    $added = count($outfits) - $countBefore;
    if ($added > 0) {
        $categories[] = [
            'id' => 'job-' . $jobId,
            'label' => $membership['name'],
            'icon' => '💼',
            'count' => $added
        ];
    }
}

// Les membres du staff disposent aussi des presets RP génériques générés sur le serveur.
if ($isStaff) {
    $file = __DIR__ . '/nitro-last/rp-outfits.json';
    if (is_file($file)) {
        $catalog = json_decode(file_get_contents($file), true);
        $staffCount = 0;
        foreach (($catalog['outfits'] ?? []) as $o) {
            $oGender = strtoupper((string)($o['gender'] ?? 'M')) === 'F' ? 'F' : 'M';
            if ($oGender !== $gender) continue;
            $figure = clean_figure($o['figure'] ?? '');
            if ($figure === '') continue;
            $id = (string)($o['id'] ?? '');
            if (!preg_match('/^rp-\d+$/', $id)) continue;
            $outfits[] = [
                'id' => 'staff-' . $id,
                'kind' => 'staff',
                'name' => (string)($o['name'] ?? 'Tenue staff'),
                'category' => 'staff',
                'categoryLabel' => $staffName ?: 'Staff ParadiseRP',
                'icon' => '⭐',
                'gender' => $gender,
                'figure' => $figure,
                'source' => (string)($o['categoryLabel'] ?? 'Preset RP')
            ];
            $staffCount++;
            if ($staffCount >= 40) break;
        }
        if ($staffCount > 0) $categories[] = ['id' => 'staff', 'label' => $staffName ?: 'Staff ParadiseRP', 'icon' => '⭐', 'count' => $staffCount];
    }
}

$allowed = count($outfits) > 0;
echo json_encode([
    'ok' => true,
    'allowed' => $allowed,
    'gender' => $gender,
    'staff' => $isStaff,
    'staff_name' => $staffName,
    'total' => count($outfits),
    'categories' => $categories,
    'outfits' => $outfits
], JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
