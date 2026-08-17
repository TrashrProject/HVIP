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
function is_staff_name($name) {
    return (bool)preg_match('/staff|fondateur|founder|gerant|gérant|developpeur|développeur|developer|administrat|owner/i', (string)$name);
}

if (!$Session->Exist(Config::$SessionName) || !isset($UData['id'])) fail_json(401, 'Session expirée');

$uid = (int)$UData['id'];
$gender = strtoupper((string)($UData['gender'] ?? 'M')) === 'F' ? 'F' : 'M';
$userRank = (int)($UData['rank'] ?? 1);
$isManager = $userRank >= 6;
$memberships = [];

$res = $DB->Query("SELECT gm.group_id, gm.rank AS member_rank, g.name, g.activity FROM group_memberships gm INNER JOIN groups g ON g.id=gm.group_id WHERE gm.user_id='".$uid."' ORDER BY gm.group_id ASC");
if ($res) {
    while ($row = mysqli_fetch_assoc($res)) {
        $name = trim((string)$row['name']);
        if (is_staff_name($name)) { $isManager = true; continue; }
        $memberships[(int)$row['group_id']] = [
            'rank' => (int)$row['member_rank'],
            'name' => $name,
            'activity' => (string)$row['activity']
        ];
    }
}

$jobsToShow = [];
if ($isManager) {
    $jobs = $DB->Query("SELECT DISTINCT g.id, g.name FROM groups g WHERE EXISTS (SELECT 1 FROM play_jobs_ranks r WHERE r.job=g.id AND ((r.male_figure IS NOT NULL AND r.male_figure<>'') OR (r.female_figure IS NOT NULL AND r.female_figure<>''))) OR EXISTS (SELECT 1 FROM play_jobs_outfits o WHERE o.job_id=g.id AND o.enabled=1 AND ((o.male_figure IS NOT NULL AND o.male_figure<>'') OR (o.female_figure IS NOT NULL AND o.female_figure<>''))) ORDER BY g.id ASC");
    if ($jobs) while ($j = mysqli_fetch_assoc($jobs)) {
        if (is_staff_name($j['name'])) continue;
        $jobsToShow[(int)$j['id']] = ['rank' => 999, 'name' => trim((string)$j['name'])];
    }
} else {
    $jobsToShow = $memberships;
}

$outfits = [];
$categories = [];

foreach ($jobsToShow as $jobId => $membership) {
    $maxRank = $isManager ? 999 : max(1, (int)$membership['rank']);
    $jobName = (string)$membership['name'];
    $before = count($outfits);

    // Tenue historique/principale du grade
    $base = $DB->Query("SELECT rank,name,male_figure,female_figure FROM play_jobs_ranks WHERE job='".$jobId."' AND rank<='".$maxRank."' ORDER BY rank ASC");
    if ($base) while ($row = mysqli_fetch_assoc($base)) {
        $figure = clean_figure($gender === 'F' ? $row['female_figure'] : $row['male_figure']);
        if ($figure === '') continue;
        $rank = (int)$row['rank'];
        $outfits[] = [
            'id' => 'job-'.$jobId.'-rank-'.$rank,
            'kind' => 'base',
            'job_id' => $jobId,
            'rank' => $rank,
            'name' => (string)$row['name'].' — Standard',
            'category' => 'job-'.$jobId,
            'categoryLabel' => $jobName,
            'icon' => '💼',
            'gender' => $gender,
            'figure' => $figure,
            'source' => 'Grade '.$rank
        ];
    }

    // Nouvelles variantes multiples par grade
    $extra = $DB->Query("SELECT id,rank_id,name,male_figure,female_figure,sort_order FROM play_jobs_outfits WHERE job_id='".$jobId."' AND rank_id<='".$maxRank."' AND enabled=1 ORDER BY rank_id ASC, sort_order ASC, id ASC");
    if ($extra) while ($row = mysqli_fetch_assoc($extra)) {
        $figure = clean_figure($gender === 'F' ? $row['female_figure'] : $row['male_figure']);
        if ($figure === '') continue;
        $rank = (int)$row['rank_id'];
        $outfits[] = [
            'id' => 'outfit-'.(int)$row['id'],
            'kind' => 'variant',
            'job_id' => $jobId,
            'rank' => $rank,
            'name' => (string)$row['name'],
            'category' => 'job-'.$jobId,
            'categoryLabel' => $jobName,
            'icon' => '👔',
            'gender' => $gender,
            'figure' => $figure,
            'source' => 'Grade '.$rank.' • variante'
        ];
    }

    $added = count($outfits) - $before;
    if ($added > 0) {
        $categories[] = ['id'=>'job-'.$jobId,'label'=>$jobName,'icon'=>'💼','count'=>$added];
    }
}

$allowed = count($outfits) > 0;
echo json_encode([
    'ok' => true,
    'allowed' => $allowed,
    'gender' => $gender,
    'manager' => $isManager,
    'total' => count($outfits),
    'categories' => $categories,
    'outfits' => $outfits
], JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
