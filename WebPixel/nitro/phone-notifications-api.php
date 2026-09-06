<?php
declare(strict_types=1);

header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store');
header('X-Content-Type-Options: nosniff');

require_once __DIR__ . '/../app/Controller/Config.class.php';
require_once __DIR__ . '/../app/Controller/DBManager.class.php';
require_once __DIR__ . '/../app/Modal/SessionMG.class.php';

function pnotif_json(array $data, int $status = 200): void {
    http_response_code($status);
    echo json_encode($data, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}

function pnotif_stmt(mysqli $db, string $sql, string $types = '', array $params = []): mysqli_stmt {
    $stmt = $db->prepare($sql);
    if (!$stmt) throw new RuntimeException('Service de notifications temporairement indisponible.');
    if ($types !== '') $stmt->bind_param($types, ...$params);
    if (!$stmt->execute()) throw new RuntimeException('Impossible de charger les notifications.');
    return $stmt;
}

try {
    $session = new SessionMG();
    if (!$session->Exist(Config::$SessionName)) {
        pnotif_json(['ok' => false, 'error' => 'Session expirée.'], 401);
    }

    $db = (new DBManager())->Con();
    $username = trim((string) $session->Read(Config::$SessionName));
    $user = pnotif_stmt(
        $db,
        'SELECT id,username,look FROM users WHERE username=? LIMIT 1',
        's',
        [$username]
    )->get_result()->fetch_assoc();

    if (!$user) pnotif_json(['ok' => false, 'error' => 'Compte introuvable.'], 401);

    $userId = (int) $user['id'];
    $now = time();

    // Keep the notification feed consistent even if the dedicated call UI was closed.
    $ringCutoff = $now - 90;
    pnotif_stmt(
        $db,
        "UPDATE phone_calls SET status='missed',ended_at=?,updated_at=? WHERE callee_id=? AND status='ringing' AND created_at<?",
        'iiii',
        [$now, $now, $userId, $ringCutoff]
    );

    $items = [];

    $calls = pnotif_stmt(
        $db,
        "SELECT c.id,c.call_type,c.created_at,c.ended_at,u.id actor_id,u.username,u.look
         FROM phone_calls c
         JOIN users u ON u.id=c.caller_id
         WHERE c.callee_id=? AND c.status='missed'
         ORDER BY COALESCE(c.ended_at,c.updated_at,c.created_at) DESC,c.id DESC
         LIMIT 25",
        'i',
        [$userId]
    )->get_result();

    while ($row = $calls->fetch_assoc()) {
        $items[] = [
            'source' => 'call',
            'id' => (int) $row['id'],
            'type' => $row['call_type'] === 'video' ? 'missed_video_call' : 'missed_call',
            'actorId' => (int) $row['actor_id'],
            'username' => (string) $row['username'],
            'look' => (string) $row['look'],
            'postId' => null,
            'createdAt' => (int) ($row['ended_at'] ?? $row['created_at'])
        ];
    }

    $gram = pnotif_stmt(
        $db,
        "SELECT n.id,n.actor_id,n.type,n.post_id,n.created_at,u.username,u.look
         FROM phone_gram_notifications n
         JOIN users u ON u.id=n.actor_id
         WHERE n.user_id=?
         ORDER BY n.created_at DESC,n.id DESC
         LIMIT 40",
        'i',
        [$userId]
    )->get_result();

    while ($row = $gram->fetch_assoc()) {
        $items[] = [
            'source' => 'gram',
            'id' => (int) $row['id'],
            'type' => (string) $row['type'],
            'actorId' => (int) $row['actor_id'],
            'username' => (string) $row['username'],
            'look' => (string) $row['look'],
            'postId' => $row['post_id'] !== null ? (int) $row['post_id'] : null,
            'createdAt' => (int) $row['created_at']
        ];
    }

    usort($items, static function (array $a, array $b): int {
        $time = ((int) $b['createdAt']) <=> ((int) $a['createdAt']);
        if ($time !== 0) return $time;
        return ((int) $b['id']) <=> ((int) $a['id']);
    });

    // Phone notifications are intentionally short-lived in the UI. Keeping seven days
    // still allows a player to see missed calls/activity after reconnecting without
    // turning the phone into an endless historical log.
    $cutoff = $now - (7 * 86400);
    $items = array_values(array_filter($items, static fn(array $item): bool => (int) $item['createdAt'] >= $cutoff));
    $items = array_slice($items, 0, 60);

    pnotif_json([
        'ok' => true,
        'me' => [
            'id' => $userId,
            'username' => (string) $user['username'],
            'look' => (string) $user['look']
        ],
        'serverTime' => $now,
        'items' => $items
    ]);
} catch (Throwable $error) {
    error_log('[ParadisePhone notifications] ' . $error->getMessage());
    pnotif_json(['ok' => false, 'error' => 'Service de notifications temporairement indisponible.'], 500);
}
