<?php
declare(strict_types=1);

require_once __DIR__ . '/app/init.pz.php';

header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store');
header('X-Content-Type-Options: nosniff');

function skin_reply(int $status, array $body): void {
    http_response_code($status);
    echo json_encode($body, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}

if (!$Session->Exist(Config::$SessionName) || !isset($UData['id'])) {
    skin_reply(401, ['ok' => false, 'error' => 'Session expirée.']);
}

$origin = (string)($_SERVER['HTTP_ORIGIN'] ?? '');
$host = (string)($_SERVER['HTTP_HOST'] ?? '');
if ($origin !== '' && parse_url($origin, PHP_URL_HOST) !== preg_replace('/:\d+$/', '', $host)) {
    skin_reply(403, ['ok' => false, 'error' => 'Origine refusée.']);
}

$db = $DB->Con();
$userId = (int)$UData['id'];
$method = strtoupper((string)($_SERVER['REQUEST_METHOD'] ?? 'GET'));

if ($method === 'POST') {
    $payload = json_decode((string)file_get_contents('php://input'), true);
    $skinId = (int)($payload['skin_id'] ?? 0);
    if ($skinId < 1) skin_reply(422, ['ok' => false, 'error' => 'Skin invalide.']);

    $stmt = mysqli_prepare($db, 'SELECT s.id, s.weapon_key, s.effect_id
        FROM paradise_weapon_skins s
        INNER JOIN paradise_user_weapon_skins us ON us.skin_id=s.id AND us.user_id=?
        WHERE s.id=?
          AND EXISTS (
              SELECT 1
              FROM user_inventory ui
              INNER JOIN rp_items i ON i.id=ui.item_id
              WHERE ui.user_id=?
                AND ui.quantity>0
                AND i.interaction_type=\'weapon\'
                AND LOWER(i.name)=LOWER(s.weapon_key)
          )
        LIMIT 1');
    mysqli_stmt_bind_param($stmt, 'iii', $userId, $skinId, $userId);
    mysqli_stmt_execute($stmt);
    $owned = mysqli_fetch_assoc(mysqli_stmt_get_result($stmt));
    mysqli_stmt_close($stmt);
    if (!$owned) skin_reply(403, ['ok' => false, 'error' => 'Vous ne possédez pas cette arme ou ce skin.']);

    mysqli_begin_transaction($db);
    try {
        $off = mysqli_prepare($db, 'UPDATE paradise_user_weapon_skins us INNER JOIN paradise_weapon_skins s ON s.id=us.skin_id SET us.equipped=0 WHERE us.user_id=? AND s.weapon_key=?');
        mysqli_stmt_bind_param($off, 'is', $userId, $owned['weapon_key']);
        mysqli_stmt_execute($off);
        mysqli_stmt_close($off);
        $on = mysqli_prepare($db, 'UPDATE paradise_user_weapon_skins SET equipped=1 WHERE user_id=? AND skin_id=?');
        mysqli_stmt_bind_param($on, 'ii', $userId, $skinId);
        mysqli_stmt_execute($on);
        mysqli_stmt_close($on);
        mysqli_commit($db);
    } catch (Throwable $e) {
        mysqli_rollback($db);
        skin_reply(500, ['ok' => false, 'error' => 'Sauvegarde impossible.']);
    }

    skin_reply(200, [
        'ok' => true,
        'skin_id' => $skinId,
        'weapon_key' => (string)$owned['weapon_key'],
        'effect_id' => (int)$owned['effect_id']
    ]);
}

if ($method !== 'GET') skin_reply(405, ['ok' => false, 'error' => 'Méthode refusée.']);

// Le skin Standard est disponible automatiquement, mais uniquement pour les armes
// réellement présentes dans l'inventaire/coffre du joueur. Les autres skins doivent
// déjà exister dans paradise_user_weapon_skins pour être considérés comme possédés.
$grant = mysqli_prepare($db, 'INSERT IGNORE INTO paradise_user_weapon_skins(user_id,skin_id,equipped)
    SELECT ?,s.id,1
    FROM paradise_weapon_skins s
    WHERE s.is_default=1
      AND EXISTS (
          SELECT 1
          FROM user_inventory ui
          INNER JOIN rp_items i ON i.id=ui.item_id
          WHERE ui.user_id=?
            AND ui.quantity>0
            AND i.interaction_type=\'weapon\'
            AND LOWER(i.name)=LOWER(s.weapon_key)
      )');
mysqli_stmt_bind_param($grant, 'ii', $userId, $userId);
mysqli_stmt_execute($grant);
mysqli_stmt_close($grant);

$stmt = mysqli_prepare($db, 'SELECT s.id,s.weapon_key,s.name,s.effect_id,s.image,s.avatar_image,s.is_default,1 owned,COALESCE(us.equipped,0) equipped
    FROM paradise_weapon_skins s
    INNER JOIN paradise_user_weapon_skins us ON us.skin_id=s.id AND us.user_id=?
    WHERE EXISTS (
        SELECT 1
        FROM user_inventory ui
        INNER JOIN rp_items i ON i.id=ui.item_id
        WHERE ui.user_id=?
          AND ui.quantity>0
          AND i.interaction_type=\'weapon\'
          AND LOWER(i.name)=LOWER(s.weapon_key)
    )
    ORDER BY FIELD(s.weapon_key,\'tazor\',\'ak47\',\'akm\',\'g36\'),s.sort_order,s.id');
mysqli_stmt_bind_param($stmt, 'ii', $userId, $userId);
mysqli_stmt_execute($stmt);
$result = mysqli_stmt_get_result($stmt);
$skins = [];
while ($row = mysqli_fetch_assoc($result)) {
    $row['id'] = (int)$row['id'];
    $row['effect_id'] = (int)$row['effect_id'];
    $row['owned'] = true;
    $row['equipped'] = (bool)$row['equipped'];
    $row['is_default'] = (bool)$row['is_default'];
    $skins[] = $row;
}
mysqli_stmt_close($stmt);
skin_reply(200, ['ok' => true, 'skins' => $skins]);
