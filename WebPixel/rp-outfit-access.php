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
$userRank = (int)($UData['rank'] ?? 1);
$isStaff = $userRank >= 6;
$staffName = '';
$jobs = [];

$sql = "SELECT gm.group_id, gm.rank AS member_rank, g.name, g.activity
        FROM group_memberships gm
        INNER JOIN groups g ON g.id = gm.group_id
        WHERE gm.user_id = '" . $uid . "'
        ORDER BY gm.group_id ASC";
$res = $DB->Query($sql);
if ($res) {
    while ($row = mysqli_fetch_assoc($res)) {
        $name = trim((string)$row['name']);
        $isStaffGroup = (bool)preg_match('/staff|fondateur|founder|gerant|gérant|developpeur|développeur|developer|administrat|owner/i', $name);
        if ($isStaffGroup) {
            $isStaff = true;
            if ($staffName === '') $staffName = $name;
            continue;
        }

        $groupId = (int)$row['group_id'];
        $has = $DB->Query("SELECT 1 FROM play_jobs_ranks WHERE job='" . $groupId . "' AND ((male_figure IS NOT NULL AND male_figure <> '') OR (female_figure IS NOT NULL AND female_figure <> '')) LIMIT 1");
        if ($has && mysqli_num_rows($has) > 0) {
            $jobs[] = [
                'group_id' => $groupId,
                'rank' => (int)$row['member_rank'],
                'name' => $name,
                'activity' => (string)$row['activity']
            ];
        }
    }
}

$allowed = $isStaff || count($jobs) > 0;
$primary = count($jobs) ? $jobs[0]['name'] : ($isStaff ? 'Gestion' : '');

echo json_encode([
    'ok' => true,
    'allowed' => $allowed,
    'label' => $allowed ? ('Tenues RP' . ($primary !== '' ? ' • ' . $primary : '')) : '',
    'jobs' => $jobs,
    'staff' => $isStaff ? ['name' => ($staffName ?: 'Gestion ParadiseRP'), 'preview_all_jobs' => true] : null
], JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
