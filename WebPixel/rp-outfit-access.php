<?php
require_once __DIR__ . '/app/init.pz.php';

header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store');

if (!$Session->Exist(Config::$SessionName) || !isset($UData['id'])) {
    http_response_code(401);
    echo json_encode(['ok' => false, 'allowed' => false, 'error' => 'Session expirée']);
    exit;
}

$uid = (int)$UData['id'];
$userRank = isset($UData['rank']) ? (int)$UData['rank'] : 1;

$memberships = [];
$sql = "SELECT gm.group_id, gm.rank AS member_rank, g.name, g.activity,
               EXISTS(
                   SELECT 1 FROM play_jobs_ranks pjr
                   WHERE pjr.job = gm.group_id
                   AND ((pjr.male_figure IS NOT NULL AND pjr.male_figure <> '')
                     OR (pjr.female_figure IS NOT NULL AND pjr.female_figure <> ''))
               ) AS has_job_outfits
        FROM group_memberships gm
        INNER JOIN groups g ON g.id = gm.group_id
        WHERE gm.user_id = '" . $uid . "'
        ORDER BY gm.group_id ASC";
$res = $DB->Query($sql);
if ($res) {
    while ($row = mysqli_fetch_assoc($res)) {
        $memberships[] = $row;
    }
}

$jobs = [];
$staff = null;
foreach ($memberships as $m) {
    $groupId = (int)$m['group_id'];
    $memberRank = (int)$m['member_rank'];
    $name = trim((string)$m['name']);
    $isStaffName = (bool)preg_match('/staff|fondateur|founder|gerant|gérant|developpeur|développeur|developer|administrat|owner/i', $name);

    if ($isStaffName) {
        $staff = [
            'group_id' => $groupId,
            'rank' => $memberRank,
            'name' => $name ?: 'Staff ParadiseRP'
        ];
        continue;
    }

    if ((int)$m['has_job_outfits'] === 1) {
        $jobs[] = [
            'group_id' => $groupId,
            'rank' => $memberRank,
            'name' => $name,
            'activity' => (string)$m['activity']
        ];
    }
}

// Les hauts rangs staff du CMS restent autorisés même si leur groupe staff n'a pas été renommé.
if ($staff === null && $userRank >= 6) {
    $staff = [
        'group_id' => 0,
        'rank' => $userRank,
        'name' => 'Staff ParadiseRP'
    ];
}

$allowed = count($jobs) > 0 || $staff !== null;
$primary = count($jobs) ? $jobs[0]['name'] : ($staff['name'] ?? '');

echo json_encode([
    'ok' => true,
    'allowed' => $allowed,
    'label' => $allowed ? ('Tenues RP' . ($primary !== '' ? ' • ' . $primary : '')) : '',
    'jobs' => $jobs,
    'staff' => $staff,
    'user_rank' => $userRank
], JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
