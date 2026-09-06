<?php
declare(strict_types=1);

header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store');
header('X-Content-Type-Options: nosniff');

require_once __DIR__ . '/../app/Controller/Config.class.php';
require_once __DIR__ . '/../app/Controller/DBManager.class.php';
require_once __DIR__ . '/../app/Modal/SessionMG.class.php';

function paradise_friends_json(array $data, int $status = 200): void
{
    http_response_code($status);
    echo json_encode($data, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}

try {
    $session = new SessionMG();
    if (!$session->Exist(Config::$SessionName)) {
        paradise_friends_json(['ok' => false, 'error' => 'Session expirée.'], 401);
    }

    $db = (new DBManager())->Con();
    $sessionUsername = trim((string) $session->Read(Config::$SessionName));

    $me = $db->prepare('SELECT id FROM users WHERE username=? LIMIT 1');
    if (!$me) throw new RuntimeException('Service temporairement indisponible.');
    $me->bind_param('s', $sessionUsername);
    $me->execute();
    if (!$me->get_result()->fetch_assoc()) {
        paradise_friends_json(['ok' => false, 'error' => 'Compte introuvable.'], 401);
    }

    $rawNames = $_GET['names'] ?? [];
    if (!is_array($rawNames)) $rawNames = [$rawNames];

    $names = [];
    foreach ($rawNames as $rawName) {
        $name = trim((string) $rawName);
        if ($name === '' || mb_strlen($name) > 64) continue;
        $names[$name] = $name;
        if (count($names) >= 80) break;
    }
    $names = array_values($names);

    if (!$names) paradise_friends_json(['ok' => true, 'users' => []]);

    $placeholders = implode(',', array_fill(0, count($names), '?'));
    $sql = "SELECT username, look FROM users WHERE username IN ($placeholders)";
    $stmt = $db->prepare($sql);
    if (!$stmt) throw new RuntimeException('Service temporairement indisponible.');

    $types = str_repeat('s', count($names));
    $stmt->bind_param($types, ...$names);
    if (!$stmt->execute()) throw new RuntimeException('Impossible de charger les avatars.');

    $users = [];
    $result = $stmt->get_result();
    while ($row = $result->fetch_assoc()) {
        $username = (string) ($row['username'] ?? '');
        $look = (string) ($row['look'] ?? '');
        if ($username !== '' && $look !== '') $users[$username] = $look;
    }

    paradise_friends_json(['ok' => true, 'users' => $users]);
} catch (Throwable $error) {
    error_log('[ParadisePhone Friends API] ' . $error->getMessage());
    paradise_friends_json(['ok' => false, 'error' => 'Service temporairement indisponible.'], 500);
}
