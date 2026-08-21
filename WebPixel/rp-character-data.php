<?php
/**
 * ParadiseRP Character System V2 - CMS/session read endpoint.
 *
 * This endpoint reads the same authoritative tables as the emulator:
 * - users: Habbo account/cash/look/rank
 * - play_stats: RP vitals/progression/bank
 * - rp_characters: RP identity extension only
 * - group_memberships/groups/play_jobs_ranks: employment
 */
header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store, no-cache, must-revalidate, max-age=0');
header('X-Content-Type-Options: nosniff');

function pr_character_json(array $payload, int $status = 200): void
{
    http_response_code($status);
    echo json_encode($payload, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}

function pr_character_role(int $rank): string
{
    if ($rank >= 8) return 'Fondateur';
    if ($rank >= 6) return 'Staff';
    if ($rank >= 3) return 'Équipe';
    return 'Citoyen';
}

try {
    require_once __DIR__ . '/app/init.pz.php';

    if (!isset($Session, $DB) || !class_exists('Config')) {
        pr_character_json(['ok' => false, 'reason' => 'bootstrap_unavailable'], 503);
    }

    $username = trim((string)$Session->Read(Config::$SessionName));
    if ($username === '') {
        pr_character_json(['ok' => false, 'reason' => 'not_connected'], 401);
    }

    $con = $DB->Con();
    $safeUsername = mysqli_real_escape_string($con, $username);

    $sql = "SELECT
                u.id,
                u.username,
                u.look,
                u.motto,
                u.rank,
                u.credits,
                u.online,
                p.curhealth,
                p.maxhealth,
                p.armor,
                p.hunger,
                p.hygiene,
                p.level,
                p.curxp,
                p.needxp,
                p.bank,
                c.citizen_number,
                c.first_name,
                c.last_name,
                c.birth_date,
                c.gender,
                c.nationality
            FROM users u
            INNER JOIN play_stats p ON p.id = u.id
            LEFT JOIN rp_characters c ON c.user_id = u.id
            WHERE u.username = '" . $safeUsername . "'
            LIMIT 1";

    $result = mysqli_query($con, $sql);
    $row = $result ? mysqli_fetch_assoc($result) : null;

    if (!$row) {
        pr_character_json(['ok' => false, 'reason' => 'character_not_found'], 404);
    }

    $userId = (int)$row['id'];
    $citizenNumber = trim((string)($row['citizen_number'] ?? ''));
    if ($citizenNumber === '') {
        $citizenNumber = 'PR-' . str_pad((string)$userId, 5, '0', STR_PAD_LEFT);
        $safeCitizen = mysqli_real_escape_string($con, $citizenNumber);
        @mysqli_query(
            $con,
            "INSERT IGNORE INTO rp_characters (user_id, citizen_number) VALUES (" . $userId . ", '" . $safeCitizen . "')"
        );
    }

    $job = [
        'jobId' => 0,
        'jobName' => null,
        'jobRank' => 0,
        'rankName' => null
    ];

    $jobResult = @mysqli_query(
        $con,
        "SELECT gm.group_id, gm.rank, g.name AS group_name, jr.name AS rank_name
         FROM group_memberships gm
         INNER JOIN groups g ON g.id = gm.group_id
         LEFT JOIN play_jobs_ranks jr ON jr.job = gm.group_id AND jr.rank = gm.rank
         WHERE gm.user_id = " . $userId . " AND gm.type IN ('1','2')
         ORDER BY gm.type ASC, gm.id ASC
         LIMIT 1"
    );

    if ($jobResult && ($jobRow = mysqli_fetch_assoc($jobResult))) {
        $job = [
            'jobId' => (int)$jobRow['group_id'],
            'jobName' => $jobRow['group_name'] !== null ? (string)$jobRow['group_name'] : null,
            'jobRank' => (int)$jobRow['rank'],
            'rankName' => $jobRow['rank_name'] !== null ? (string)$jobRow['rank_name'] : null
        ];
    }

    $birthDate = !empty($row['birth_date']) ? (string)$row['birth_date'] : null;
    $identityComplete = trim((string)($row['first_name'] ?? '')) !== ''
        && trim((string)($row['last_name'] ?? '')) !== ''
        && $birthDate !== null;

    pr_character_json([
        'ok' => true,
        'v' => 1,
        'player' => [
            'userId' => $userId,
            'username' => (string)$row['username'],
            'look' => (string)$row['look'],
            'motto' => (string)$row['motto'],
            'rank' => (int)$row['rank'],
            'role' => pr_character_role((int)$row['rank']),
            'online' => ((int)$row['online']) === 1
        ],
        'identity' => [
            'citizenNumber' => $citizenNumber,
            'firstName' => $row['first_name'] !== null ? (string)$row['first_name'] : null,
            'lastName' => $row['last_name'] !== null ? (string)$row['last_name'] : null,
            'birthDate' => $birthDate,
            'gender' => $row['gender'] !== null ? (string)$row['gender'] : null,
            'nationality' => $row['nationality'] !== null ? (string)$row['nationality'] : null,
            'complete' => $identityComplete
        ],
        'vitals' => [
            'health' => [
                'current' => (int)$row['curhealth'],
                'max' => max(1, (int)$row['maxhealth'])
            ],
            'armor' => (int)$row['armor'],
            'hunger' => (int)$row['hunger'],
            'hygiene' => (int)$row['hygiene']
        ],
        'progression' => [
            'level' => (int)$row['level'],
            'xp' => (int)$row['curxp'],
            'nextXp' => (int)$row['needxp']
        ],
        'employment' => $job,
        'economy' => [
            'cash' => (int)$row['credits'],
            'bank' => (int)$row['bank']
        ]
    ]);
} catch (Throwable $e) {
    pr_character_json(['ok' => false, 'reason' => 'character_data_unavailable'], 503);
}
