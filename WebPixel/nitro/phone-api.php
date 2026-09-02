<?php
declare(strict_types=1);

header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store');
header('X-Content-Type-Options: nosniff');

require_once __DIR__ . '/../app/Controller/Config.class.php';
require_once __DIR__ . '/../app/Controller/DBManager.class.php';
require_once __DIR__ . '/../app/Modal/SessionMG.class.php';

function phone_json(array $data, int $status = 200): void {
    http_response_code($status);
    echo json_encode($data, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}

function phone_stmt(mysqli $db, string $sql, string $types = '', array $params = []): mysqli_stmt {
    $stmt = $db->prepare($sql);
    if (!$stmt) throw new RuntimeException('Service temporairement indisponible.');
    if ($types !== '') $stmt->bind_param($types, ...$params);
    if (!$stmt->execute()) throw new RuntimeException('Opération impossible.');
    return $stmt;
}

function phone_body(): array {
    $data = json_decode((string)file_get_contents('php://input'), true);
    return is_array($data) ? $data : [];
}

function phone_text(mixed $value, int $max, bool $required = false): string {
    $value = trim((string)$value);
    if (($required && $value === '') || mb_strlen($value) > $max) {
        throw new InvalidArgumentException('Le contenu saisi est invalide.');
    }
    return $value;
}

function phone_image_url(mixed $value): ?string {
    $url = trim((string)$value);
    if ($url === '') return null;
    if (strlen($url) > 500 || filter_var($url, FILTER_VALIDATE_URL) === false
        || strtolower((string)parse_url($url, PHP_URL_SCHEME)) !== 'https') {
        throw new InvalidArgumentException('Utilisez une adresse d’image HTTPS valide.');
    }
    return $url;
}

function phone_feed(mysqli $db, int $userId, string $csrf): void {
    $posts = [];
    $result = phone_stmt($db, 'SELECT p.id,p.user_id,p.body,p.image_url,p.created_at,u.username,u.look,(SELECT COUNT(*) FROM phone_gram_likes l WHERE l.post_id=p.id) likes,EXISTS(SELECT 1 FROM phone_gram_likes l2 WHERE l2.post_id=p.id AND l2.user_id=?) liked FROM phone_gram_posts p JOIN users u ON u.id=p.user_id ORDER BY p.created_at DESC,p.id DESC LIMIT 100', 'i', [$userId])->get_result();
    while ($row = $result->fetch_assoc()) {
        $comments = [];
        $commentResult = phone_stmt($db, 'SELECT c.id,c.body,c.created_at,u.username FROM phone_gram_comments c JOIN users u ON u.id=c.user_id WHERE c.post_id=? ORDER BY c.id ASC LIMIT 100', 'i', [(int)$row['id']])->get_result();
        while ($comment = $commentResult->fetch_assoc()) {
            $comments[] = ['id'=>(int)$comment['id'],'username'=>$comment['username'],'body'=>$comment['body'],'createdAt'=>(int)$comment['created_at']];
        }
        $posts[] = ['id'=>(int)$row['id'],'username'=>$row['username'],'look'=>$row['look'],'body'=>$row['body'],'imageUrl'=>$row['image_url'],'createdAt'=>(int)$row['created_at'],'likes'=>(int)$row['likes'],'liked'=>(bool)$row['liked'],'canDelete'=>(int)$row['user_id']===$userId,'comments'=>$comments];
    }
    phone_json(['ok'=>true,'csrf'=>$csrf,'posts'=>$posts]);
}

try {
    $session = new SessionMG();
    if (!$session->Exist(Config::$SessionName)) phone_json(['ok'=>false,'error'=>'Session expirée.'], 401);
    $db = (new DBManager())->Con();
    $username = trim((string)$session->Read(Config::$SessionName));
    $user = phone_stmt($db, 'SELECT id,username FROM users WHERE username=? LIMIT 1', 's', [$username])->get_result()->fetch_assoc();
    if (!$user) phone_json(['ok'=>false,'error'=>'Compte introuvable.'], 401);
    $userId = (int)$user['id'];
    if (!isset($_SESSION['paradise_phone_csrf'])) $_SESSION['paradise_phone_csrf'] = bin2hex(random_bytes(24));
    $csrf = (string)$_SESSION['paradise_phone_csrf'];
    $action = strtolower(trim((string)($_GET['action'] ?? '')));

    if ($action === 'gallery' && $_SERVER['REQUEST_METHOD'] === 'GET') {
        $photos = [];
        $result = phone_stmt($db, 'SELECT id,room_id,timestamp,url FROM camera_photos WHERE user_id=? ORDER BY timestamp DESC,id DESC LIMIT 250', 'i', [$userId])->get_result();
        while ($row = $result->fetch_assoc()) $photos[] = ['id'=>(int)$row['id'],'roomId'=>(int)$row['room_id'],'timestamp'=>(int)$row['timestamp'],'url'=>$row['url']];
        phone_json(['ok'=>true,'photos'=>$photos]);
    }

    if ($action !== 'feed') phone_json(['ok'=>false,'error'=>'Application inconnue.'], 404);
    if ($_SERVER['REQUEST_METHOD'] === 'GET') phone_feed($db, $userId, $csrf);
    if ($_SERVER['REQUEST_METHOD'] !== 'POST') phone_json(['ok'=>false,'error'=>'Méthode refusée.'], 405);

    $data = phone_body();
    if (!hash_equals($csrf, (string)($data['csrf'] ?? ''))) phone_json(['ok'=>false,'error'=>'Session de sécurité expirée.'], 403);
    $operation = strtolower(trim((string)($data['action'] ?? '')));
    $postId = max(0, (int)($data['postId'] ?? 0));
    $now = time();

    if ($operation === 'create') {
        $body = phone_text($data['body'] ?? '', 500);
        $image = phone_image_url($data['imageUrl'] ?? '');
        if ($body === '' && $image === null) throw new InvalidArgumentException('Ajoutez un texte ou une image.');
        phone_stmt($db, 'INSERT INTO phone_gram_posts(user_id,body,image_url,created_at) VALUES(?,?,?,?)', 'issi', [$userId,$body,$image,$now]);
        phone_json(['ok'=>true]);
    }
    $post = phone_stmt($db, 'SELECT id,user_id FROM phone_gram_posts WHERE id=? LIMIT 1', 'i', [$postId])->get_result()->fetch_assoc();
    if (!$post) phone_json(['ok'=>false,'error'=>'Publication introuvable.'], 404);
    if ($operation === 'like') {
        $liked = phone_stmt($db, 'SELECT 1 FROM phone_gram_likes WHERE post_id=? AND user_id=?', 'ii', [$postId,$userId])->get_result()->fetch_row();
        if ($liked) phone_stmt($db, 'DELETE FROM phone_gram_likes WHERE post_id=? AND user_id=?', 'ii', [$postId,$userId]);
        else phone_stmt($db, 'INSERT INTO phone_gram_likes(post_id,user_id,created_at) VALUES(?,?,?)', 'iii', [$postId,$userId,$now]);
        phone_json(['ok'=>true,'liked'=>!$liked]);
    }
    if ($operation === 'comment') {
        $body = phone_text($data['body'] ?? '', 240, true);
        phone_stmt($db, 'INSERT INTO phone_gram_comments(post_id,user_id,body,created_at) VALUES(?,?,?,?)', 'iisi', [$postId,$userId,$body,$now]);
        phone_json(['ok'=>true]);
    }
    if ($operation === 'delete') {
        if ((int)$post['user_id'] !== $userId) phone_json(['ok'=>false,'error'=>'Suppression refusée.'], 403);
        $db->begin_transaction();
        try {
            phone_stmt($db, 'DELETE FROM phone_gram_comments WHERE post_id=?', 'i', [$postId]);
            phone_stmt($db, 'DELETE FROM phone_gram_likes WHERE post_id=?', 'i', [$postId]);
            phone_stmt($db, 'DELETE FROM phone_gram_posts WHERE id=? AND user_id=?', 'ii', [$postId,$userId]);
            $db->commit();
        } catch (Throwable $error) { $db->rollback(); throw $error; }
        phone_json(['ok'=>true]);
    }
    phone_json(['ok'=>false,'error'=>'Action inconnue.'], 404);
} catch (InvalidArgumentException $error) {
    phone_json(['ok'=>false,'error'=>$error->getMessage()], 422);
} catch (Throwable $error) {
    error_log('[Paradise Phone API] '.$error->getMessage());
    phone_json(['ok'=>false,'error'=>'Impossible de charger cette application.'], 500);
}
