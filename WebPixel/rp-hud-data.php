<?php
/**
 * ParadiseRP in-game UI data endpoint.
 * The HUD must be connected to the current session/user and must not expose fake player values.
 */
header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store, no-cache, must-revalidate, max-age=0');
header('X-Content-Type-Options: nosniff');

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
        'motto' => null,
        'level' => null,
        'look' => null,
        'avatar_url' => null,
        'health' => null,
        'armor' => null,
        'energy' => null,
        'money' => [
            'credits' => null,
            'cash' => null,
            'bank' => null,
            'pixels' => null
        ],
        'city' => null,
        'district' => null,
        'room' => null,
        'players' => null,
        'notifications_count' => 0,
        'time' => date('H:i')
    ];
}

function pr_hud_first(array $source, array $columns, array $names, $fallback = null) {
    foreach ($names as $name) {
        $key = strtolower($name);
        if (isset($columns[$key])) {
            $column = $columns[$key];
            if (array_key_exists($column, $source) && $source[$column] !== null && $source[$column] !== '') {
                return $source[$column];
            }
        }
    }
    return $fallback;
}

function pr_hud_int_or_null($value): ?int {
    if ($value === null || $value === '') return null;
    if (!is_numeric($value)) return null;
    return (int) $value;
}

function pr_hud_stat(?int $current, ?int $max = null): ?array {
    if ($current === null) return null;
    $max = ($max !== null && $max > 0) ? $max : 100;
    return [
        'current' => max(0, $current),
        'max' => max(1, $max)
    ];
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
    $safeUsername = mysqli_real_escape_string($con, $username);

    $columns = [];
    $columnsResult = $DB->Query('SHOW COLUMNS FROM users');
    while ($col = mysqli_fetch_assoc($columnsResult)) {
        $columns[strtolower((string) $col['Field'])] = (string) $col['Field'];
    }

    $wanted = [
        'id','username','look','motto','rank','credits','cash','money','bank','activity_points','vip_points','pixels','level',
        'health','hp','vie','max_health','health_max','armor','armour','shield','max_armor','armor_max','shield_max',
        'energy','energie','stamina','max_energy','energy_max','job','metier','work','profession','room','room_id','current_room','current_room_id','zone','district','city'
    ];

    $select = [];
    foreach ($wanted as $name) {
        if (isset($columns[$name])) $select[] = '`' . $columns[$name] . '`';
    }
    if (!$select) $select[] = '*';

    $user = $DB->Select('users', implode(',', array_unique($select)), "username = '" . $safeUsername . "'");
    if (!$user) {
        pr_hud_json(pr_hud_empty('user_not_found'));
    }

    $id = pr_hud_int_or_null(pr_hud_first($user, $columns, ['id']));
    $rank = pr_hud_int_or_null(pr_hud_first($user, $columns, ['rank']));
    $role = null;
    if ($rank !== null) {
        if ($rank >= 8) $role = 'Fondateur';
        elseif ($rank >= 6) $role = 'Staff';
        elseif ($rank >= 3) $role = 'Équipe';
        else $role = 'Citoyen';
    }

    $look = trim((string) pr_hud_first($user, $columns, ['look'], ''));
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

    $cash = pr_hud_int_or_null(pr_hud_first($user, $columns, ['cash','credits','money']));
    $bank = pr_hud_int_or_null(pr_hud_first($user, $columns, ['bank','banque','account_balance','bank_balance']));
    $pixels = pr_hud_int_or_null(pr_hud_first($user, $columns, ['pixels','activity_points','vip_points']));

    $health = pr_hud_stat(
        pr_hud_int_or_null(pr_hud_first($user, $columns, ['health','hp','vie'])),
        pr_hud_int_or_null(pr_hud_first($user, $columns, ['max_health','health_max','hp_max','max_hp']))
    );
    $armor = pr_hud_stat(
        pr_hud_int_or_null(pr_hud_first($user, $columns, ['armor','armour','shield','bouclier'])),
        pr_hud_int_or_null(pr_hud_first($user, $columns, ['max_armor','armor_max','shield_max','max_shield']))
    );
    $energy = pr_hud_stat(
        pr_hud_int_or_null(pr_hud_first($user, $columns, ['energy','energie','stamina'])),
        pr_hud_int_or_null(pr_hud_first($user, $columns, ['max_energy','energy_max','stamina_max']))
    );

    $room = pr_hud_first($user, $columns, ['room','current_room','room_name'], null);
    $district = pr_hud_first($user, $columns, ['district','zone'], null);
    $city = pr_hud_first($user, $columns, ['city'], null);

    pr_hud_json([
        'ok' => true,
        'id' => $id,
        'citizen_id' => $id !== null ? 'PR-' . str_pad((string) $id, 5, '0', STR_PAD_LEFT) : null,
        'username' => (string) ($user[$columns['username'] ?? 'username'] ?? $username),
        'role' => $role,
        'job' => pr_hud_first($user, $columns, ['job','metier','work','profession'], null),
        'motto' => trim((string) pr_hud_first($user, $columns, ['motto'], '')),
        'level' => pr_hud_int_or_null(pr_hud_first($user, $columns, ['level'])),
        'look' => $look !== '' ? $look : null,
        'avatar_url' => $avatarUrl,
        'health' => $health,
        'armor' => $armor,
        'energy' => $energy,
        'money' => [
            'credits' => $cash,
            'cash' => $cash,
            'bank' => $bank,
            'pixels' => $pixels
        ],
        'city' => $city,
        'district' => $district,
        'room' => $room,
        'players' => null,
        'notifications_count' => 0,
        'time' => date('H:i')
    ]);
} catch (Throwable $e) {
    $fallback = pr_hud_empty('hud_data_unavailable');
    $fallback['error'] = 'hud_data_unavailable';
    pr_hud_json($fallback);
}
