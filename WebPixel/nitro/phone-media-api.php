<?php
declare(strict_types=1);

header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store');
header('X-Content-Type-Options: nosniff');

require_once __DIR__ . '/../app/Controller/Config.class.php';
require_once __DIR__ . '/../app/Controller/DBManager.class.php';
require_once __DIR__ . '/../app/Modal/SessionMG.class.php';

function media_json(array $data, int $status = 200): void {
    http_response_code($status);
    echo json_encode($data, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}

function media_stmt(mysqli $db, string $sql, string $types = '', array $params = []): mysqli_stmt {
    $stmt = $db->prepare($sql);
    if (!$stmt) throw new RuntimeException('Service galerie indisponible.');
    if ($types !== '') $stmt->bind_param($types, ...$params);
    if (!$stmt->execute()) throw new RuntimeException('Impossible de sauvegarder la photo.');
    return $stmt;
}

function media_body(): array {
    $data = json_decode((string) file_get_contents('php://input'), true);
    return is_array($data) ? $data : [];
}

try {
    $session = new SessionMG();
    if (!$session->Exist(Config::$SessionName)) media_json(['ok' => false, 'error' => 'Session expirée.'], 401);

    $db = (new DBManager())->Con();
    $username = trim((string) $session->Read(Config::$SessionName));
    $user = media_stmt($db, 'SELECT id FROM users WHERE username=? LIMIT 1', 's', [$username])->get_result()->fetch_assoc();
    if (!$user) media_json(['ok' => false, 'error' => 'Compte introuvable.'], 401);
    $userId = (int) $user['id'];

    if (!isset($_SESSION['paradise_phone_media_csrf'])) {
        $_SESSION['paradise_phone_media_csrf'] = bin2hex(random_bytes(24));
    }
    $csrf = (string) $_SESSION['paradise_phone_media_csrf'];
    $action = strtolower(trim((string) ($_GET['action'] ?? 'list')));

    if ($_SERVER['REQUEST_METHOD'] === 'GET' && $action === 'list') {
        $photos = [];
        $result = media_stmt($db, 'SELECT id,room_id,timestamp,url FROM camera_photos WHERE user_id=? ORDER BY timestamp DESC,id DESC LIMIT 250', 'i', [$userId])->get_result();
        while ($row = $result->fetch_assoc()) {
            $photos[] = [
                'id' => (int) $row['id'],
                'roomId' => (int) $row['room_id'],
                'timestamp' => (int) $row['timestamp'],
                'url' => (string) $row['url']
            ];
        }
        media_json(['ok' => true, 'csrf' => $csrf, 'photos' => $photos]);
    }

    if ($_SERVER['REQUEST_METHOD'] !== 'POST' || $action !== 'save') {
        media_json(['ok' => false, 'error' => 'Action inconnue.'], 404);
    }

    $data = media_body();
    if (!hash_equals($csrf, (string) ($data['csrf'] ?? ''))) {
        media_json(['ok' => false, 'error' => 'Session de sécurité expirée.'], 403);
    }

    $source = trim((string) ($data['source'] ?? ''));
    $roomId = max(0, (int) ($data['roomId'] ?? 0));
    $timestamp = max(1, (int) ($data['timestamp'] ?? time()));
    if ($source === '') throw new InvalidArgumentException('Photo vide.');

    $url = null;
    if (preg_match('#^data:image/(png|jpeg|webp);base64,#i', $source, $match)) {
        $encoded = substr($source, strpos($source, ',') + 1);
        if (strlen($encoded) > 8 * 1024 * 1024) throw new InvalidArgumentException('Photo trop volumineuse.');
        $binary = base64_decode($encoded, true);
        if ($binary === false || strlen($binary) < 100 || strlen($binary) > 5 * 1024 * 1024) {
            throw new InvalidArgumentException('Photo invalide.');
        }
        $extension = strtolower($match[1]) === 'jpeg' ? 'jpg' : strtolower($match[1]);
        $relativeDir = 'camera-photos/' . $userId;
        $absoluteDir = __DIR__ . '/' . $relativeDir;
        if (!is_dir($absoluteDir) && !mkdir($absoluteDir, 0775, true) && !is_dir($absoluteDir)) {
            throw new RuntimeException('Impossible de créer le dossier photo.');
        }
        $filename = bin2hex(random_bytes(16)) . '.' . $extension;
        $absolutePath = $absoluteDir . '/' . $filename;
        if (file_put_contents($absolutePath, $binary, LOCK_EX) === false) {
            throw new RuntimeException('Impossible d’enregistrer la photo.');
        }
        $url = '/nitro/' . $relativeDir . '/' . $filename;
    } else {
        $parsed = parse_url($source);
        $scheme = strtolower((string) ($parsed['scheme'] ?? ''));
        $host = strtolower((string) ($parsed['host'] ?? ''));
        $currentHost = strtolower((string) ($_SERVER['HTTP_HOST'] ?? ''));
        $currentHost = preg_replace('/:\d+$/', '', $currentHost);
        if (!in_array($scheme, ['http', 'https'], true) || $host === '' || ($host !== $currentHost && $host !== 'paradiserp.fr' && $host !== 'www.paradiserp.fr')) {
            throw new InvalidArgumentException('Source photo refusée.');
        }
        if (strlen($source) > 1000) throw new InvalidArgumentException('Adresse photo trop longue.');
        $url = $source;
    }

    $existing = media_stmt($db, 'SELECT id,room_id,timestamp,url FROM camera_photos WHERE user_id=? AND url=? LIMIT 1', 'is', [$userId, $url])->get_result()->fetch_assoc();
    if ($existing) {
        media_json(['ok' => true, 'photo' => [
            'id' => (int) $existing['id'],
            'roomId' => (int) $existing['room_id'],
            'timestamp' => (int) $existing['timestamp'],
            'url' => (string) $existing['url']
        ]]);
    }

    media_stmt($db, 'INSERT INTO camera_photos(user_id,room_id,timestamp,url) VALUES(?,?,?,?)', 'iiis', [$userId, $roomId, $timestamp, $url]);
    $id = (int) $db->insert_id;
    media_json(['ok' => true, 'photo' => ['id' => $id, 'roomId' => $roomId, 'timestamp' => $timestamp, 'url' => $url]]);
} catch (InvalidArgumentException $error) {
    media_json(['ok' => false, 'error' => $error->getMessage()], 422);
} catch (Throwable $error) {
    error_log('[Paradise Phone Media] ' . $error->getMessage());
    media_json(['ok' => false, 'error' => 'Impossible de sauvegarder la photo.'], 500);
}
