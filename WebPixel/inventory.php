<?php
declare(strict_types=1);

require_once __DIR__ . '/app/init.pz.php';

header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store, no-cache, must-revalidate');
header('X-Content-Type-Options: nosniff');

function inventory_reply(int $status, array $body): void {
    http_response_code($status);
    echo json_encode($body, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}

if (!$Session->Exist(Config::$SessionName) || !isset($UData['id'])) {
    inventory_reply(401, ['ok' => false, 'error' => 'Session expiree.']);
}

$origin = (string)($_SERVER['HTTP_ORIGIN'] ?? '');
$host = preg_replace('/:\d+$/', '', (string)($_SERVER['HTTP_HOST'] ?? ''));
if ($origin !== '' && parse_url($origin, PHP_URL_HOST) !== $host) {
    inventory_reply(403, ['ok' => false, 'error' => 'Origine refusee.']);
}

if (strtoupper((string)($_SERVER['REQUEST_METHOD'] ?? 'GET')) !== 'GET') {
    inventory_reply(405, ['ok' => false, 'error' => 'Methode refusee.']);
}

$images = [
    0 => 'tazer.png', 1 => 'medic.png', 2 => 'kevlar.png', 3 => 'snack.png',
    4 => 'pistola.png', 5 => 'bat.png', 6 => 'espadavip.png', 7 => 'weed.png',
    8 => 'cocaine.png', 9 => 'comida1.png', 10 => 'comida2.png', 11 => 'comida3.png',
    12 => 'comida4.png', 13 => 'comida5.png', 14 => 'taco.png', 15 => 'salmao.png',
    16 => 'comida6.png', 18 => 'comida3.png', 19 => 'lagosta.png', 20 => 'comida6.png',
    21 => 'comida4.png', 1001 => 'bandage.png', 1002 => 'energy-drink.png',
    1003 => 'armor-kit.png', 1004 => 'deluxe-medkit.png',
    6109 => 'usp-s.png', 6110 => 'ak47.png', 6111 => 'colete.png', 6112 => 'sniper.png',
    6113 => 'mp5.png', 6114 => 'reparo.png', 6115 => 'vara.png', 6116 => 'g36.png',
    6117 => 'akm.png', 6118 => 'semente.png', 6119 => 'atum.png', 6120 => 'salmao.png',
    6121 => 'carrot.png', 6122 => 'munitions.png'
];

// Only weapons that currently have ParadiseRP skin families are mapped here.
$weaponKeys = [
    6110 => 'ak47',
    6116 => 'g36',
    6117 => 'akm'
];

$db = $DB->Con();
$userId = (int)$UData['id'];

$equippedSkins = [];
$skinStmt = mysqli_prepare($db,
    'SELECT s.weapon_key,s.id,s.name,s.effect_id,s.image,s.avatar_image '
    . 'FROM paradise_user_weapon_skins us '
    . 'INNER JOIN paradise_weapon_skins s ON s.id=us.skin_id '
    . 'WHERE us.user_id=? AND us.equipped=1');
if ($skinStmt) {
    mysqli_stmt_bind_param($skinStmt, 'i', $userId);
    mysqli_stmt_execute($skinStmt);
    $skinResult = mysqli_stmt_get_result($skinStmt);
    while ($skin = mysqli_fetch_assoc($skinResult)) {
        $equippedSkins[(string)$skin['weapon_key']] = [
            'id' => (int)$skin['id'],
            'name' => (string)$skin['name'],
            'effect_id' => (int)$skin['effect_id'],
            'image' => (string)$skin['image'],
            'avatar_image' => (string)$skin['avatar_image']
        ];
    }
    mysqli_stmt_close($skinStmt);
}

$stmt = mysqli_prepare($db,
    'SELECT ui.slot_index, ui.item_id, ui.quantity, ui.durability, ui.is_deposit_box, '
    . 'i.name, i.interaction_type, i.extra_data, i.max '
    . 'FROM user_inventory ui INNER JOIN rp_items i ON i.id=ui.item_id '
    . 'WHERE ui.user_id=? AND ui.is_deposit_box=0 AND ui.quantity>0 '
    . 'ORDER BY ui.slot_index ASC');

if (!$stmt) inventory_reply(500, ['ok' => false, 'error' => 'Inventaire indisponible.']);
mysqli_stmt_bind_param($stmt, 'i', $userId);
mysqli_stmt_execute($stmt);
$result = mysqli_stmt_get_result($stmt);
$slots = [];
while ($row = mysqli_fetch_assoc($result)) {
    $itemId = (int)$row['item_id'];
    $slotIndex = (int)$row['slot_index'];
    if ($slotIndex < 0 || $slotIndex > 11) continue;

    $weaponKey = $weaponKeys[$itemId] ?? null;
    $skin = $weaponKey !== null ? ($equippedSkins[$weaponKey] ?? null) : null;

    $slots[] = [
        'slot_index' => $slotIndex,
        'item_id' => $itemId,
        'display_name' => (string)$row['name'],
        'interaction_type' => (string)$row['interaction_type'],
        'extra_data' => (string)$row['extra_data'],
        'quantity' => (int)$row['quantity'],
        'durability' => (int)$row['durability'],
        'is_broken' => (int)$row['durability'] <= 0,
        'equipped' => $slotIndex < 2,
        'image_url' => 'inventory-items/' . ($images[$itemId] ?? 'unknown.svg'),
        'weapon_key' => $weaponKey,
        'skin' => $skin
    ];
}
mysqli_stmt_close($stmt);

inventory_reply(200, ['ok' => true, 'total_slots' => 12, 'slots' => $slots]);
