<?php
/**
 * ParadiseRP in-game UI data endpoint.
 * Keeps the payload intentionally small and backward compatible.
 */
header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store, no-cache, must-revalidate, max-age=0');
header('X-Content-Type-Options: nosniff');

function pr_hud_json(array $data, int $code = 200): void {
    http_response_code($code);
    echo json_encode($data, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}

function pr_hud_default(): array {
    return [
        'ok' => false,
        'id' => 0,
        'citizen_id' => 'PR-00000',
        'username' => 'ParadiseRP',
        'role' => 'Citoyen',
        'motto' => '',
        'level' => 1,
        'look' => '',
        'avatar_url' => '',
        'health' => ['current' => 100, 'max' => 100],
        'energy' => ['current' => 100, 'max' => 100],
        'money' => ['credits' => 0, 'pixels' => 0],
        'city' => 'Paradise City',
        'time' => date('H:i')
    ];
}

try {
    require_once __DIR__ . '/app/init.pz.php';

    if (!isset($Session, $DB) || !class_exists('Config')) {
        pr_hud_json(pr_hud_default());
    }

    $username = (string) $Session->Read(Config::$SessionName);
    if ($username === '') {
        pr_hud_json(pr_hud_default());
    }

    $con = $DB->Con();
    $safeUsername = mysqli_real_escape_string($con, $username);

    $columns = [];
    $columnsResult = $DB->Query('SHOW COLUMNS FROM users');
    while ($col = mysqli_fetch_assoc($columnsResult)) {
        $columns[strtolower((string) $col['Field'])] = (string) $col['Field'];
    }

    $wanted = ['id','username','look','motto','rank','credits','activity_points','vip_points','pixels','level'];
    $select = [];
    foreach ($wanted as $name) {
        if (isset($columns[$name])) $select[] = '`' . $columns[$name] . '`';
    }
    if (!$select) $select[] = '*';

    $user = $DB->Select('users', implode(',', $select), "username = '" . $safeUsername . "'");
    if (!$user) {
        pr_hud_json(pr_hud_default());
    }

    $id = isset($user['id']) ? max(0, (int) $user['id']) : 0;
    $credits = isset($user['credits']) ? (int) $user['credits'] : 0;

    $pixels = 0;
    if (isset($user['pixels'])) $pixels = (int) $user['pixels'];
    elseif (isset($user['activity_points'])) $pixels = (int) $user['activity_points'];
    elseif (isset($user['vip_points'])) $pixels = (int) $user['vip_points'];

    $rank = isset($user['rank']) ? (int) $user['rank'] : 1;
    $role = 'Citoyen';
    if ($rank >= 8) $role = 'Fondateur';
    elseif ($rank >= 6) $role = 'Staff';
    elseif ($rank >= 3) $role = 'Équipe';

    $level = 1;
    if (isset($user['level']) && is_numeric($user['level'])) $level = max(1, (int) $user['level']);

    $look = (string) ($user['look'] ?? '');
    $motto = trim((string) ($user['motto'] ?? ''));
    $avatarUrl = '';
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

    pr_hud_json([
        'ok' => true,
        'id' => $id,
        'citizen_id' => 'PR-' . str_pad((string) $id, 5, '0', STR_PAD_LEFT),
        'username' => (string) ($user['username'] ?? $username),
        'role' => $role,
        'motto' => $motto,
        'level' => $level,
        'look' => $look,
        'avatar_url' => $avatarUrl,
        'health' => ['current' => 100, 'max' => 100],
        'energy' => ['current' => 100, 'max' => 100],
        'money' => ['credits' => $credits, 'pixels' => $pixels],
        'city' => 'Paradise City',
        'time' => date('H:i')
    ]);
} catch (Throwable $e) {
    $fallback = pr_hud_default();
    $fallback['error'] = 'hud_data_unavailable';
    pr_hud_json($fallback);
}
