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

$jobs = [];
$sql = "SELECT gm.group_id, gm.rank AS member_rank, g.name, g.activity
        FROM group_memberships gm
        INNER JOIN groups g ON g.id = gm.group_id
        WHERE gm.user_id = '" . $uid . "'
          AND EXISTS(
              SELECT 1 FROM play_jobs_ranks pjr
              WHERE pjr.job = gm.group_id
                AND ((pjr.male_figure IS NOT NULL AND pjr.male_figure <> '')
                  OR (pjr.female_figure IS NOT NULL AND pjr.female_figure <> ''))
          )
        ORDER BY gm.group_id ASC";
$res = $DB->Query($sql);
if ($res) {
    while ($row = mysqli_fetch_assoc($res)) {
        $name = trim((string)$row['name']);
        // Les groupes staff ne sont jamais considérés comme des métiers pour les tenues RP.
        if (preg_match('/staff|fondateur|founder|gerant|gérant|developpeur|développeur|developer|administrat|owner/i', $name)) continue;

        $jobs[] = [
            'group_id' => (int)$row['group_id'],
            'rank' => (int)$row['member_rank'],
            'name' => $name,
            'activity' => (string)$row['activity']
        ];
    }
}

$allowed = count($jobs) > 0;
$primary = $allowed ? $jobs[0]['name'] : '';

echo json_encode([
    'ok' => true,
    'allowed' => $allowed,
    'label' => $allowed ? ('Tenues RP' . ($primary !== '' ? ' • ' . $primary : '')) : '',
    'jobs' => $jobs,
    'staff' => null
], JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
