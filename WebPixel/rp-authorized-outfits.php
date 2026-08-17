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
$isStaff = $userRank >= 6;
$jobMemberships = [];

$sql = "SELECT gm.group_id, gm.rank AS member_rank, g.name, g.activity
        FROM group_memberships gm
        INNER JOIN groups g ON g.id = gm.group_id
        WHERE gm.user_id = '" . $uid . "'
        ORDER BY gm.group_id ASC";
$res = $DB->Query($sql);
if ($res) {
    while ($m = mysqli_fetch_assoc($res)) {
        $name = trim((string)$m['name']);
        if (preg_match('/staff|fondateur|founder|gerant|gérant|developpeur|développeur|developer|administrat|owner/i', $name)) {
            $isStaff = true;
            continue;
        }
        $jobMemberships[(int)$m['group_id']] = [
            'rank' => (int)$m['member_rank'],
            'name' => $name,
            'activity' => (string)$m['activity']
        ];
    }
}

$outfits = [];
$categories = [];

if ($isStaff) {
    $jobs = $DB->Query("SELECT DISTINCT pjr.job, g.name FROM play_jobs_ranks pjr INNER JOIN groups g ON g.id=pjr.job WHERE ((pjr.male_figure IS NOT NULL AND pjr.male_figure <> '') OR (pjr.female_figure IS NOT NULL AND pjr.female_figure <> '')) ORDER BY pjr.job ASC");
    if ($jobs) {
        while ($job = mysqli_fetch_assoc($jobs)) {
            $jobId = (int)$job['job'];
            $jobName = trim((string)$job['name']);
            if (preg_match('/staff|fondateur|founder|gerant|gérant|developpeur|développeur|developer|administrat|owner/i', $jobName)) continue;
            $rows = $DB->Query("SELECT job, rank, name, male_figure, female_figure FROM play_jobs_ranks WHERE job='" . $jobId . "' ORDER BY rank ASC");
            $before = count($outfits);
            if ($rows) while ($row = mysqli_fetch_assoc($rows)) {
                $figure = clean_figure($gender === 'F' ? $row['female_figure'] : $row['male_figure']);
                if ($figure === '') continue;
                $rank = (int)$row['rank'];
                $outfits[] = [
                    'id' => 'job-' . $jobId . '-rank-' . $rank,
                    'kind' => 'job', 'job_id' => $jobId, 'rank' => $rank,
                    'name' => (string)$row['name'],
                    'category' => 'job-' . $jobId,
                    'categoryLabel' => $jobName,
                    'icon' => '💼', 'gender' => $gender, 'figure' => $figure,
                    'source' => 'Aperçu gestion'
                ];
            }
            $added = count($outfits) - $before;
            if ($added > 0) $categories[] = ['id'=>'job-'.$jobId,'label'=>$jobName,'icon'=>'💼','count'=>$added];
        }
    }
} else {
    foreach ($jobMemberships as $jobId => $membership) {
        $memberRank = max(1, (int)$membership['rank']);
        $rows = $DB->Query("SELECT job, rank, name, male_figure, female_figure FROM play_jobs_ranks WHERE job='" . $jobId . "' AND rank <= '" . $memberRank . "' ORDER BY rank ASC");
        $before = count($outfits);
        if ($rows) while ($row = mysqli_fetch_assoc($rows)) {
            $figure = clean_figure($gender === 'F' ? $row['female_figure'] : $row['male_figure']);
            if ($figure === '') continue;
            $rank = (int)$row['rank'];
            $outfits[] = [
                'id' => 'job-' . $jobId . '-rank-' . $rank,
                'kind' => 'job', 'job_id' => $jobId, 'rank' => $rank,
                'name' => (string)$row['name'],
                'category' => 'job-' . $jobId,
                'categoryLabel' => $membership['name'],
                'icon' => '💼', 'gender' => $gender, 'figure' => $figure,
                'source' => 'Grade ' . $rank . ' / ' . $memberRank
            ];
        }
        $added = count($outfits) - $before;
        if ($added > 0) $categories[] = ['id'=>'job-'.$jobId,'label'=>$membership['name'],'icon'=>'💼','count'=>$added];
    }
}

$allowed = count($outfits) > 0;
echo json_encode([
    'ok' => true,
    'allowed' => $allowed,
    'gender' => $gender,
    'staff' => $isStaff,
    'staff_name' => $isStaff ? 'Gestion ParadiseRP' : '',
    'total' => count($outfits),
    'categories' => $categories,
    'outfits' => $outfits
], JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
