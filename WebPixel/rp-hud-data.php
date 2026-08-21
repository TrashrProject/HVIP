<?php
/**
 * ParadiseRP UI data bridge.
 * Read-only: gameplay values remain authoritative in existing systems, while
 * Phase 2 character/documents are exposed from the additive RP tables.
 */
header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store, no-cache, must-revalidate, max-age=0');
header('X-Content-Type-Options: nosniff');

require_once __DIR__ . '/paradise-character-lib.php';

function pr_hud_json(array $data, int $code = 200): void {
    http_response_code($code);
    echo json_encode($data, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}

function pr_hud_empty(string $reason = 'not_connected'): array {
    return [
        'ok' => false,
        'reason' => $reason,
        'id' => null,
        'citizen_id' => null,
        'username' => null,
        'role' => null,
        'job' => null,
        'job_id' => null,
        'motto' => null,
        'level' => null,
        'look' => null,
        'avatar_url' => null,
        'health' => null,
        'armor' => null,
        'money' => ['credits' => null, 'cash' => null, 'bank' => null, 'pixels' => null],
        'character' => ['exists' => false],
        'documents' => [],
        'reputation' => ['general' => 0],
        'statistics' => [],
        'document_offer' => null,
        'ui_event' => null,
        'city' => null,
        'district' => null,
        'room_id' => null,
        'room_name' => null,
        'room' => null,
        'players' => null,
        'notifications_count' => 0,
        'time' => date('H:i')
    ];
}

function pr_hud_int_or_null($value): ?int {
    if ($value === null || $value === '' || !is_numeric($value)) return null;
    return (int) $value;
}

function pr_hud_stat(?int $current, ?int $max = null): ?array {
    if ($current === null) return null;
    return [
        'current' => max(0, $current),
        'max' => ($max !== null && $max > 0) ? $max : 100
    ];
}

function pr_hud_columns(mysqli $con, string $table): array {
    $columns = [];
    $result = @mysqli_query($con, 'SHOW COLUMNS FROM `' . $table . '`');
    if (!$result) return $columns;
    while ($column = mysqli_fetch_assoc($result)) {
        $columns[strtolower((string) $column['Field'])] = (string) $column['Field'];
    }
    mysqli_free_result($result);
    return $columns;
}

function pr_hud_first(array $source, array $columns, array $names, $fallback = null) {
    foreach ($names as $name) {
        $key = strtolower($name);
        if (!isset($columns[$key])) continue;
        $column = $columns[$key];
        if (array_key_exists($column, $source) && $source[$column] !== null && $source[$column] !== '') {
            return $source[$column];
        }
    }
    return $fallback;
}

function pr_hud_column(array $columns, array $names): ?string {
    foreach ($names as $name) {
        $key = strtolower($name);
        if (isset($columns[$key])) return $columns[$key];
    }
    return null;
}

function pr_hud_row_by_user(mysqli $con, string $table, array $columns, int $userId): ?array {
    if (!$columns) return null;
    $owner = pr_hud_column($columns, ['user_id', 'userid', 'user', 'owner_id', 'owner', 'id']);
    if ($owner === null) return null;
    $sql = 'SELECT * FROM `' . $table . '` WHERE `' . $owner . '` = ' . (int) $userId . ' LIMIT 1';
    $result = @mysqli_query($con, $sql);
    if (!$result) return null;
    $row = mysqli_fetch_assoc($result) ?: null;
    mysqli_free_result($result);
    return $row;
}

function pr_hud_row_by_id(mysqli $con, string $table, array $columns, int $id): ?array {
    if (!$columns) return null;
    $idColumn = pr_hud_column($columns, ['id', 'room_id', 'group_id']);
    if ($idColumn === null) return null;
    $sql = 'SELECT * FROM `' . $table . '` WHERE `' . $idColumn . '` = ' . (int) $id . ' LIMIT 1';
    $result = @mysqli_query($con, $sql);
    if (!$result) return null;
    $row = mysqli_fetch_assoc($result) ?: null;
    mysqli_free_result($result);
    return $row;
}

try {
    require_once __DIR__ . '/app/init.pz.php';

    if (!isset($Session, $DB) || !class_exists('Config')) {
        pr_hud_json(pr_hud_empty('bootstrap_unavailable'));
    }

    $username = (string) $Session->Read(Config::$SessionName);
    if ($username === '') {
        pr_hud_json(pr_hud_empty('not_connected'));
    }

    $con = $DB->Con();
    if (!($con instanceof mysqli)) {
        pr_hud_json(pr_hud_empty('database_unavailable'));
    }

    $usersColumns = pr_hud_columns($con, 'users');
    $usernameColumn = pr_hud_column($usersColumns, ['username']);
    if ($usernameColumn === null) {
        pr_hud_json(pr_hud_empty('users_schema_unavailable'));
    }

    $safeUsername = mysqli_real_escape_string($con, $username);
    $result = mysqli_query($con, "SELECT * FROM `users` WHERE `{$usernameColumn}` = '{$safeUsername}' LIMIT 1");
    $user = $result ? (mysqli_fetch_assoc($result) ?: null) : null;
    if ($result) mysqli_free_result($result);
    if (!$user) {
        pr_hud_json(pr_hud_empty('user_not_found'));
    }

    $id = pr_hud_int_or_null(pr_hud_first($user, $usersColumns, ['id']));
    if ($id === null) {
        pr_hud_json(pr_hud_empty('user_id_unavailable'));
    }

    $statsColumns = pr_hud_columns($con, 'play_stats');
    $stats = pr_hud_row_by_user($con, 'play_stats', $statsColumns, $id) ?: [];
    $userStatsColumns = pr_hud_columns($con, 'user_stats');
    $userStats = pr_hud_row_by_user($con, 'user_stats', $userStatsColumns, $id) ?: [];

    $rank = pr_hud_int_or_null(pr_hud_first($user, $usersColumns, ['rank']));
    $role = null;
    if ($rank !== null) {
        if ($rank >= 8) $role = 'Fondateur';
        elseif ($rank >= 6) $role = 'Staff';
        elseif ($rank >= 3) $role = 'Équipe';
        else $role = 'Citoyen';
    }

    $look = trim((string) pr_hud_first($user, $usersColumns, ['look'], ''));
    $avatarUrl = null;
    if ($look !== '' && preg_match('/^[a-z0-9.\-]+$/i', $look)) {
        $avatarUrl = '/avatar-image.php?' . http_build_query([
            'figure' => $look,
            'direction' => '2',
            'head_direction' => '3',
            'gesture' => 'std',
            'action' => 'std',
            'size' => 'l'
        ], '', '&', PHP_QUERY_RFC3986);
    }

    $healthCurrent = pr_hud_int_or_null(pr_hud_first($stats, $statsColumns, ['curhealth','health','hp','vie']));
    if ($healthCurrent === null) $healthCurrent = pr_hud_int_or_null(pr_hud_first($user, $usersColumns, ['health','hp','vie']));
    $healthMax = pr_hud_int_or_null(pr_hud_first($stats, $statsColumns, ['maxhealth','max_health','health_max','hp_max','max_hp']));
    if ($healthMax === null) $healthMax = pr_hud_int_or_null(pr_hud_first($user, $usersColumns, ['max_health','health_max','hp_max','max_hp']));

    $armorCurrent = pr_hud_int_or_null(pr_hud_first($stats, $statsColumns, ['armor','armour','shield','bouclier']));
    if ($armorCurrent === null) $armorCurrent = pr_hud_int_or_null(pr_hud_first($user, $usersColumns, ['armor','armour','shield','bouclier']));
    $armorMax = pr_hud_int_or_null(pr_hud_first($stats, $statsColumns, ['maxarmor','max_armor','armor_max','shield_max','max_shield']));
    if ($armorMax === null) $armorMax = pr_hud_int_or_null(pr_hud_first($user, $usersColumns, ['max_armor','armor_max','shield_max','max_shield']));

    $cash = pr_hud_int_or_null(pr_hud_first($user, $usersColumns, ['cash','credits','money']));
    $bank = pr_hud_int_or_null(pr_hud_first($stats, $statsColumns, ['bank','banque','bank_balance','account_balance']));
    if ($bank === null) $bank = pr_hud_int_or_null(pr_hud_first($user, $usersColumns, ['bank','banque','bank_balance','account_balance']));
    $pixels = pr_hud_int_or_null(pr_hud_first($user, $usersColumns, ['pixels','activity_points','vip_points']));

    $level = pr_hud_int_or_null(pr_hud_first($stats, $statsColumns, ['level']));
    if ($level === null) $level = pr_hud_int_or_null(pr_hud_first($user, $usersColumns, ['level']));

    $jobRaw = pr_hud_first($stats, $statsColumns, ['job_name','job','job_id','work','profession'], null);
    if ($jobRaw === null) $jobRaw = pr_hud_first($user, $usersColumns, ['job_name','job','job_id','metier','work','profession'], null);
    $jobId = is_numeric($jobRaw) ? (int) $jobRaw : null;
    $job = (!is_numeric($jobRaw) && trim((string) $jobRaw) !== '') ? trim((string) $jobRaw) : null;

    if ($jobId !== null && $jobId > 0) {
        $groupsColumns = pr_hud_columns($con, 'groups');
        $group = pr_hud_row_by_id($con, 'groups', $groupsColumns, $jobId);
        if ($group) {
            $job = trim((string) pr_hud_first($group, $groupsColumns, ['name','title','caption'], '')) ?: $job;
        }
    }
    if ($jobId !== null && $jobId <= 0) $jobId = null;

    $roomRaw = pr_hud_first($stats, $statsColumns, ['room_id','current_room_id','current_room','room'], null);
    if ($roomRaw === null) $roomRaw = pr_hud_first($user, $usersColumns, ['room_id','current_room_id','current_room','room'], null);
    $roomId = is_numeric($roomRaw) ? (int) $roomRaw : null;
    $roomName = (!is_numeric($roomRaw) && trim((string) $roomRaw) !== '') ? trim((string) $roomRaw) : null;

    if ($roomId !== null && $roomId > 0) {
        $roomsColumns = pr_hud_columns($con, 'rooms');
        $roomRow = pr_hud_row_by_id($con, 'rooms', $roomsColumns, $roomId);
        if ($roomRow) {
            $resolvedName = trim((string) pr_hud_first($roomRow, $roomsColumns, ['caption','name','room_name','title'], ''));
            if ($resolvedName !== '') $roomName = $resolvedName;
        }
    }
    if ($roomId !== null && $roomId <= 0) $roomId = null;

    $playerCount = null;
    if ($roomId !== null) {
        $onlineColumn = pr_hud_column($usersColumns, ['online']);
        $userRoomColumn = pr_hud_column($usersColumns, ['room_id','current_room_id','current_room','room']);
        if ($onlineColumn !== null && $userRoomColumn !== null) {
            $countResult = @mysqli_query($con, 'SELECT COUNT(*) AS total FROM `users` WHERE `' . $onlineColumn . '` = 1 AND `' . $userRoomColumn . '` = ' . (int) $roomId);
            if ($countResult && ($countRow = mysqli_fetch_assoc($countResult))) {
                $playerCount = pr_hud_int_or_null($countRow['total'] ?? null);
            }
            if ($countResult) mysqli_free_result($countResult);
        }
    }

    $district = pr_hud_first($user, $usersColumns, ['district','zone'], null);
    $city = pr_hud_first($user, $usersColumns, ['city'], null);

    // Phase 2: additive character/document layer. Missing migration degrades cleanly.
    $character = pr_character_snapshot($con, $id);
    $documents = pr_character_documents($con, $id);
    $documentOffer = pr_character_offer($con, $id);
    $uiEvent = pr_character_ui_event($con, $id);

    $accountCreated = pr_hud_first($user, $usersColumns, ['account_created','created_at','reg_timestamp'], null);
    $onlineTime = pr_hud_int_or_null(pr_hud_first($userStats, $userStatsColumns, ['onlinetime','online_time'], null));
    $roomVisits = pr_hud_int_or_null(pr_hud_first($userStats, $userStatsColumns, ['roomvisits','room_visits'], null));

    pr_hud_json([
        'ok' => true,
        'id' => $id,
        'citizen_id' => $character['citizen_id'] ?? null,
        'username' => (string) ($user[$usernameColumn] ?? $username),
        'role' => $role,
        'job' => $job,
        'job_id' => $jobId,
        'motto' => trim((string) pr_hud_first($user, $usersColumns, ['motto'], '')),
        'level' => $level,
        'look' => $look !== '' ? $look : null,
        'avatar_url' => $avatarUrl,
        'health' => pr_hud_stat($healthCurrent, $healthMax),
        'armor' => pr_hud_stat($armorCurrent, $armorMax),
        'money' => [
            'credits' => $cash,
            'cash' => $cash,
            'bank' => $bank,
            'pixels' => $pixels
        ],
        'character' => $character,
        'documents' => $documents,
        'reputation' => ['general' => (int)($character['reputation'] ?? 0)],
        'statistics' => [
            'account_created' => $accountCreated,
            'online_time' => $onlineTime,
            'room_visits' => $roomVisits,
        ],
        'document_offer' => $documentOffer,
        'ui_event' => $uiEvent,
        'city' => $city,
        'district' => $district,
        'room_id' => $roomId,
        'room_name' => $roomName,
        'room' => $roomName,
        'players' => $playerCount,
        'notifications_count' => $documentOffer ? 1 : 0,
        'time' => date('H:i')
    ]);
} catch (Throwable $e) {
    error_log('[ParadiseRP HUD] ' . $e->getMessage());
    pr_hud_json(pr_hud_empty('hud_data_unavailable'));
}
