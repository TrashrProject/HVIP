<?php
declare(strict_types=1);

header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store');
header('X-Content-Type-Options: nosniff');

require_once __DIR__ . '/../app/Controller/Config.class.php';
require_once __DIR__ . '/../app/Controller/DBManager.class.php';
require_once __DIR__ . '/../app/Modal/SessionMG.class.php';

function pcall_json(array $data, int $status = 200): void {
    http_response_code($status);
    echo json_encode($data, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}

function pcall_stmt(mysqli $db, string $sql, string $types = '', array $params = []): mysqli_stmt {
    $stmt = $db->prepare($sql);
    if (!$stmt) throw new RuntimeException('Service d’appel temporairement indisponible.');
    if ($types !== '') $stmt->bind_param($types, ...$params);
    if (!$stmt->execute()) throw new RuntimeException('Opération d’appel impossible.');
    return $stmt;
}

function pcall_body(): array {
    $data = json_decode((string) file_get_contents('php://input'), true);
    return is_array($data) ? $data : [];
}

function pcall_text(mixed $value, int $max, bool $required = false): string {
    $value = trim((string) $value);
    if (($required && $value === '') || mb_strlen($value) > $max) {
        throw new InvalidArgumentException('Donnée d’appel invalide.');
    }
    return $value;
}

function pcall_sdp(mixed $value): array {
    if (!is_array($value)) throw new InvalidArgumentException('Signal WebRTC invalide.');
    $type = (string) ($value['type'] ?? '');
    $sdp = (string) ($value['sdp'] ?? '');
    if (!in_array($type, ['offer', 'answer'], true) || $sdp === '' || strlen($sdp) > 120000) {
        throw new InvalidArgumentException('Signal WebRTC invalide.');
    }
    return ['type' => $type, 'sdp' => $sdp];
}

function pcall_decode_sdp(?string $json): ?array {
    if ($json === null || $json === '') return null;
    $value = json_decode($json, true);
    return is_array($value) ? $value : null;
}

function pcall_expire(mysqli $db, int $now): void {
    $ringCutoff = $now - 90;
    $acceptedCutoff = $now - 21600;
    pcall_stmt($db, "UPDATE phone_calls SET status='missed',ended_at=?,updated_at=? WHERE status='ringing' AND created_at<?", 'iii', [$now, $now, $ringCutoff]);
    pcall_stmt($db, "UPDATE phone_calls SET status='ended',ended_at=?,updated_at=? WHERE status='accepted' AND updated_at<?", 'iii', [$now, $now, $acceptedCutoff]);
}

function pcall_require_participant(array $call, int $userId): void {
    if ((int) $call['caller_id'] !== $userId && (int) $call['callee_id'] !== $userId) {
        pcall_json(['ok' => false, 'error' => 'Accès à cet appel refusé.'], 403);
    }
}

/**
 * Best-effort bridge to the local WaveRP RCON server.
 * It is intentionally fire-and-forget so an RP bubble never slows the phone call.
 */
function pcall_room_message(int $userId, string $message, string $type = 'shout'): void {
    if (!in_array($type, ['talk', 'whisper', 'shout'], true)) $type = 'shout';

    $port = (int) (getenv('RCON_PORT') ?: 30001);
    if ($port < 1 || $port > 65535) $port = 30001;

    $payload = json_encode([
        'key' => 'talkuser',
        'data' => [
            'type' => $type,
            'user_id' => $userId,
            'bubble_id' => -1,
            'message' => $message
        ]
    ], JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);

    if (!is_string($payload) || $payload === '') return;

    $errno = 0;
    $errstr = '';
    $socket = @stream_socket_client(
        'tcp://127.0.0.1:' . $port,
        $errno,
        $errstr,
        0.20,
        STREAM_CLIENT_CONNECT
    );

    if (!is_resource($socket)) {
        error_log('[Paradise Phone Call API] RCON room message unavailable: ' . $errstr . ' (' . $errno . ')');
        return;
    }

    try {
        stream_set_blocking($socket, false);
        @fwrite($socket, $payload);
        @fflush($socket);
        @stream_socket_shutdown($socket, STREAM_SHUT_WR);
    } finally {
        @fclose($socket);
    }
}

try {
    $session = new SessionMG();
    if (!$session->Exist(Config::$SessionName)) {
        pcall_json(['ok' => false, 'error' => 'Session expirée.'], 401);
    }

    $db = (new DBManager())->Con();
    $username = trim((string) $session->Read(Config::$SessionName));
    $user = pcall_stmt($db, 'SELECT id,username,look FROM users WHERE username=? LIMIT 1', 's', [$username])->get_result()->fetch_assoc();
    if (!$user) pcall_json(['ok' => false, 'error' => 'Compte introuvable.'], 401);

    $userId = (int) $user['id'];
    if (!isset($_SESSION['paradise_phone_csrf'])) $_SESSION['paradise_phone_csrf'] = bin2hex(random_bytes(24));
    $csrf = (string) $_SESSION['paradise_phone_csrf'];
    $action = strtolower(trim((string) ($_GET['action'] ?? 'bootstrap')));
    $method = strtoupper((string) ($_SERVER['REQUEST_METHOD'] ?? 'GET'));
    $now = time();

    pcall_expire($db, $now);

    if ($action === 'bootstrap' && $method === 'GET') {
        pcall_json([
            'ok' => true,
            'csrf' => $csrf,
            'me' => ['id' => $userId, 'username' => $user['username'], 'look' => $user['look']]
        ]);
    }

    if ($action === 'incoming' && $method === 'GET') {
        $row = pcall_stmt(
            $db,
            "SELECT c.id,c.caller_id,c.callee_id,c.call_type,c.status,c.offer_sdp,c.created_at,u.username caller_username,u.look caller_look FROM phone_calls c JOIN users u ON u.id=c.caller_id WHERE c.callee_id=? AND c.status='ringing' ORDER BY c.id DESC LIMIT 1",
            'i',
            [$userId]
        )->get_result()->fetch_assoc();

        if (!$row) pcall_json(['ok' => true, 'call' => null]);

        pcall_json(['ok' => true, 'call' => [
            'id' => (int) $row['id'],
            'type' => $row['call_type'],
            'status' => $row['status'],
            'createdAt' => (int) $row['created_at'],
            'offer' => pcall_decode_sdp($row['offer_sdp']),
            'caller' => [
                'id' => (int) $row['caller_id'],
                'username' => $row['caller_username'],
                'look' => $row['caller_look']
            ]
        ]]);
    }

    if ($action === 'status' && $method === 'GET') {
        $callId = max(0, (int) ($_GET['id'] ?? 0));
        $row = pcall_stmt(
            $db,
            'SELECT c.*,cu.username caller_username,cu.look caller_look,tu.username callee_username,tu.look callee_look FROM phone_calls c JOIN users cu ON cu.id=c.caller_id JOIN users tu ON tu.id=c.callee_id WHERE c.id=? LIMIT 1',
            'i',
            [$callId]
        )->get_result()->fetch_assoc();
        if (!$row) pcall_json(['ok' => false, 'error' => 'Appel introuvable.'], 404);
        pcall_require_participant($row, $userId);

        pcall_json(['ok' => true, 'call' => [
            'id' => (int) $row['id'],
            'type' => $row['call_type'],
            'status' => $row['status'],
            'offer' => pcall_decode_sdp($row['offer_sdp']),
            'answer' => pcall_decode_sdp($row['answer_sdp']),
            'createdAt' => (int) $row['created_at'],
            'answeredAt' => $row['answered_at'] !== null ? (int) $row['answered_at'] : null,
            'caller' => ['id' => (int) $row['caller_id'], 'username' => $row['caller_username'], 'look' => $row['caller_look']],
            'callee' => ['id' => (int) $row['callee_id'], 'username' => $row['callee_username'], 'look' => $row['callee_look']]
        ]]);
    }

    if ($method !== 'POST') pcall_json(['ok' => false, 'error' => 'Méthode refusée.'], 405);

    $data = pcall_body();
    if (!hash_equals($csrf, (string) ($data['csrf'] ?? ''))) {
        pcall_json(['ok' => false, 'error' => 'Session de sécurité expirée.'], 403);
    }

    if ($action === 'start') {
        $targetName = pcall_text($data['target'] ?? '', 64, true);
        $type = strtolower(pcall_text($data['type'] ?? 'audio', 8, true));
        if (!in_array($type, ['audio', 'video'], true)) throw new InvalidArgumentException('Type d’appel invalide.');
        $offer = pcall_sdp($data['offer'] ?? null);
        if ($offer['type'] !== 'offer') throw new InvalidArgumentException('Offre WebRTC invalide.');

        $target = pcall_stmt($db, 'SELECT id,username,look,online FROM users WHERE username=? LIMIT 1', 's', [$targetName])->get_result()->fetch_assoc();
        if (!$target) pcall_json(['ok' => false, 'error' => 'Utilisateur introuvable.'], 404);
        $targetId = (int) $target['id'];
        if ($targetId === $userId) throw new InvalidArgumentException('Vous ne pouvez pas vous appeler vous-même.');
        if ((int) $target['online'] !== 1) pcall_json(['ok' => false, 'error' => $target['username'] . ' est hors ligne.'], 409);

        $friend = pcall_stmt(
            $db,
            'SELECT 1 FROM messenger_friendships WHERE (user_one_id=? AND user_two_id=?) OR (user_one_id=? AND user_two_id=?) LIMIT 1',
            'iiii',
            [$userId, $targetId, $targetId, $userId]
        )->get_result()->fetch_row();
        if (!$friend) pcall_json(['ok' => false, 'error' => 'Les appels sont réservés à vos amis.'], 403);

        $busy = pcall_stmt(
            $db,
            "SELECT id FROM phone_calls WHERE status IN ('ringing','accepted') AND (caller_id IN (?,?) OR callee_id IN (?,?)) LIMIT 1",
            'iiii',
            [$userId, $targetId, $userId, $targetId]
        )->get_result()->fetch_row();
        if ($busy) pcall_json(['ok' => false, 'error' => 'L’un des deux téléphones est déjà en communication.'], 409);

        $offerJson = json_encode($offer, JSON_UNESCAPED_SLASHES);
        pcall_stmt(
            $db,
            "INSERT INTO phone_calls(caller_id,callee_id,call_type,status,offer_sdp,created_at,updated_at) VALUES(?,?,?,'ringing',?,?,?)",
            'iissii',
            [$userId, $targetId, $type, $offerJson, $now, $now]
        );
        $callId = (int) $db->insert_id;

        // RP room action: visible around the caller, while the phone content stays private.
        pcall_room_message(
            $userId,
            '* ' . $user['username'] . ' essaie d\'appeler ' . $target['username'] . ' *',
            'shout'
        );

        pcall_json(['ok' => true, 'call' => [
            'id' => $callId,
            'type' => $type,
            'status' => 'ringing',
            'callee' => ['id' => $targetId, 'username' => $target['username'], 'look' => $target['look']]
        ]]);
    }

    $callId = max(0, (int) ($data['id'] ?? 0));
    $call = pcall_stmt($db, 'SELECT * FROM phone_calls WHERE id=? LIMIT 1', 'i', [$callId])->get_result()->fetch_assoc();
    if (!$call) pcall_json(['ok' => false, 'error' => 'Appel introuvable.'], 404);
    pcall_require_participant($call, $userId);

    if ($action === 'accept') {
        if ((int) $call['callee_id'] !== $userId) pcall_json(['ok' => false, 'error' => 'Seul le destinataire peut décrocher.'], 403);
        if ($call['status'] !== 'ringing') pcall_json(['ok' => false, 'error' => 'Cet appel n’est plus disponible.'], 409);
        $answer = pcall_sdp($data['answer'] ?? null);
        if ($answer['type'] !== 'answer') throw new InvalidArgumentException('Réponse WebRTC invalide.');
        $answerJson = json_encode($answer, JSON_UNESCAPED_SLASHES);
        pcall_stmt($db, "UPDATE phone_calls SET status='accepted',answer_sdp=?,answered_at=?,updated_at=? WHERE id=? AND status='ringing'", 'siii', [$answerJson, $now, $now, $callId]);

        // As soon as the recipient answers, both players are explicitly switched to the
        // room-whisper conversation. RoomUserTalkEvent routes their normal room chat
        // privately while this call remains accepted.
        $caller = pcall_stmt($db, 'SELECT id,username FROM users WHERE id=? LIMIT 1', 'i', [(int) $call['caller_id']])->get_result()->fetch_assoc();
        if ($caller) {
            pcall_room_message(
                (int) $caller['id'],
                $user['username'] . ' a décroché. Conversation en murmure privé active.',
                'whisper'
            );
            pcall_room_message(
                $userId,
                'Appel avec ' . $caller['username'] . ' connecté. Conversation en murmure privé active.',
                'whisper'
            );
        }

        pcall_json(['ok' => true, 'status' => 'accepted']);
    }

    if ($action === 'decline') {
        if ((int) $call['callee_id'] !== $userId) pcall_json(['ok' => false, 'error' => 'Refus impossible.'], 403);
        if ($call['status'] === 'ringing') {
            pcall_stmt($db, "UPDATE phone_calls SET status='declined',ended_at=?,updated_at=? WHERE id=?", 'iii', [$now, $now, $callId]);
        }
        pcall_json(['ok' => true, 'status' => 'declined']);
    }

    if ($action === 'end') {
        if (in_array($call['status'], ['ringing', 'accepted'], true)) {
            pcall_stmt($db, "UPDATE phone_calls SET status='ended',ended_at=?,updated_at=? WHERE id=?", 'iii', [$now, $now, $callId]);
        }
        pcall_json(['ok' => true, 'status' => 'ended']);
    }

    pcall_json(['ok' => false, 'error' => 'Action inconnue.'], 404);
} catch (InvalidArgumentException $error) {
    pcall_json(['ok' => false, 'error' => $error->getMessage()], 422);
} catch (Throwable $error) {
    error_log('[Paradise Phone Call API] ' . $error->getMessage());
    pcall_json(['ok' => false, 'error' => 'Impossible de gérer cet appel.'], 500);
}