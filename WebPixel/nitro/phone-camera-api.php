<?php
declare(strict_types=1);

header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store');
header('X-Content-Type-Options: nosniff');

require_once __DIR__ . '/../app/Controller/Config.class.php';
require_once __DIR__ . '/../app/Controller/DBManager.class.php';
require_once __DIR__ . '/../app/Modal/SessionMG.class.php';

function camera_json(array $data, int $status = 200): void {
    http_response_code($status);
    echo json_encode($data, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}

function camera_stmt(mysqli $db, string $sql, string $types = '', array $params = []): mysqli_stmt {
    $stmt = $db->prepare($sql);
    if (!$stmt) throw new RuntimeException('Service caméra indisponible.');
    if ($types !== '') $stmt->bind_param($types, ...$params);
    if (!$stmt->execute()) throw new RuntimeException('Impossible de sauvegarder la photo.');
    return $stmt;
}

try {
    if ($_SERVER['REQUEST_METHOD'] !== 'POST') camera_json(['ok' => false, 'error' => 'Méthode refusée.'], 405);

    $session = new SessionMG();
    if (!$session->Exist(Config::$SessionName)) camera_json(['ok' => false, 'error' => 'Session expirée.'], 401);

    $db = (new DBManager())->Con();
    $username = trim((string)$session->Read(Config::$SessionName));
    $user = camera_stmt($db, 'SELECT id FROM users WHERE username=? LIMIT 1', 's', [$username])->get_result()->fetch_assoc();
    if (!$user) camera_json(['ok' => false, 'error' => 'Compte introuvable.'], 401);
    $userId = (int)$user['id'];

    $payload = json_decode((string)file_get_contents('php://input'), true);
    if (!is_array($payload)) camera_json(['ok' => false, 'error' => 'Photo invalide.'], 422);

    $imageData = (string)($payload['imageData'] ?? '');
    if (!preg_match('#^data:image/png;base64,([A-Za-z0-9+/=]+)$#', $imageData, $matches)) {
        camera_json(['ok' => false, 'error' => 'Format de photo invalide.'], 422);
    }

    $binary = base64_decode($matches[1], true);
    if ($binary === false || strlen($binary) < 100 || strlen($binary) > 4 * 1024 * 1024) {
        camera_json(['ok' => false, 'error' => 'Photo trop lourde ou invalide.'], 422);
    }

    $info = @getimagesizefromstring($binary);
    if (!$info || ($info[2] ?? 0) !== IMAGETYPE_PNG) camera_json(['ok' => false, 'error' => 'Image PNG invalide.'], 422);
    if (($info[0] ?? 0) < 160 || ($info[1] ?? 0) < 160 || ($info[0] ?? 0) > 1024 || ($info[1] ?? 0) > 1024) {
        camera_json(['ok' => false, 'error' => 'Dimensions de photo invalides.'], 422);
    }

    $directory = __DIR__ . '/camera-photos';
    if (!is_dir($directory) && !mkdir($directory, 0755, true) && !is_dir($directory)) {
        throw new RuntimeException('Impossible de créer le dossier des photos.');
    }

    $now = time();
    $name = sprintf('%d_%d_%s.png', $userId, $now, bin2hex(random_bytes(5)));
    $absolute = $directory . '/' . $name;
    if (file_put_contents($absolute, $binary, LOCK_EX) === false) throw new RuntimeException('Impossible d’écrire la photo.');

    $url = '/nitro/camera-photos/' . $name;
    $roomId = max(0, (int)($payload['roomId'] ?? 0));

    try {
        camera_stmt($db, 'INSERT INTO camera_photos(user_id,room_id,timestamp,url) VALUES(?,?,?,?)', 'iiis', [$userId, $roomId, $now, $url]);
    } catch (Throwable $error) {
        @unlink($absolute);
        throw $error;
    }

    $id = (int)$db->insert_id;
    camera_json(['ok' => true, 'photo' => ['id' => $id, 'roomId' => $roomId, 'timestamp' => $now, 'url' => $url]]);
} catch (Throwable $error) {
    error_log('[Paradise Phone Camera] ' . $error->getMessage());
    camera_json(['ok' => false, 'error' => 'Impossible de sauvegarder cette photo.'], 500);
}
