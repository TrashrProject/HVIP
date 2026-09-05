<?php
declare(strict_types=1);

require_once __DIR__ . '/app/init.pz.php';

header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store, no-cache, must-revalidate');
header('X-Content-Type-Options: nosniff');

function restaurant_menu_reply(int $status, array $body): void {
    http_response_code($status);
    echo json_encode($body, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}

if (!$Session->Exist(Config::$SessionName) || !isset($UData['id'])) {
    restaurant_menu_reply(401, ['ok' => false, 'error' => 'Session expirée.']);
}

$origin = (string)($_SERVER['HTTP_ORIGIN'] ?? '');
$host = preg_replace('/:\d+$/', '', (string)($_SERVER['HTTP_HOST'] ?? ''));
if ($origin !== '' && parse_url($origin, PHP_URL_HOST) !== $host) {
    restaurant_menu_reply(403, ['ok' => false, 'error' => 'Origine refusée.']);
}
if (strtoupper((string)($_SERVER['REQUEST_METHOD'] ?? 'GET')) !== 'GET') {
    restaurant_menu_reply(405, ['ok' => false, 'error' => 'Méthode refusée.']);
}

$db = $DB->Con();
mysqli_set_charset($db, 'utf8mb4');
$userId = (int)$UData['id'];
$jobStmt = mysqli_prepare($db,
    'SELECT j.id,j.name,j.display_name FROM users_roleplay ur '
    . 'INNER JOIN jobs j ON j.id=ur.job_id '
    . 'WHERE ur.user_id=? AND j.active=1 LIMIT 1');
if (!$jobStmt) restaurant_menu_reply(500, ['ok' => false, 'error' => 'Menu indisponible.']);
mysqli_stmt_bind_param($jobStmt, 'i', $userId);
mysqli_stmt_execute($jobStmt);
$job = mysqli_fetch_assoc(mysqli_stmt_get_result($jobStmt));
mysqli_stmt_close($jobStmt);

if (!$job || !in_array(strtolower((string)$job['name']), ['zycroque', 'tastycrousty'], true)) {
    restaurant_menu_reply(403, ['ok' => false, 'error' => 'Vous ne travaillez pas dans un restaurant.']);
}

$imageByItem = [
    9 => '/Dynamics/img/food/pomme.png', 10 => '/Dynamics/img/food/banane.png',
    11 => '/Dynamics/img/food/sandwich.png', 12 => '/Dynamics/img/food/burger.png',
    13 => '/Dynamics/img/food/pizza.png', 14 => '/Dynamics/img/food/tacos.png',
    15 => '/Dynamics/img/food/sushi.png', 16 => '/Dynamics/img/food/steak.png',
    18 => '/nitro/inventory-items/comida3.png', 19 => '/nitro/inventory-items/lagosta.png',
    20 => '/Dynamics/img/food/boeuf.png', 21 => '/nitro/inventory-items/comida4.png',
    6119 => '/nitro/inventory-items/atum.png', 6120 => '/nitro/inventory-items/salmao.png',
    6123 => '/Dynamics/img/food/tastycrousty.png',
    91001 => '/Dynamics/img/food/sandwich.png', 91002 => '/nitro/inventory-items/comida1.png',
    91003 => '/nitro/inventory-items/snack.png', 91004 => '/Dynamics/img/food/tastycrousty.png'
];

$menuStmt = mysqli_prepare($db,
    'SELECT rm.code,rm.display_name,rm.item_id,rm.price,COALESCE(i.extra_data,\'0\') hunger '
    . 'FROM restaurant_menu rm LEFT JOIN rp_items i ON i.id=rm.item_id '
    . 'WHERE rm.job_id=? AND rm.active=1 ORDER BY rm.id');
if (!$menuStmt) restaurant_menu_reply(500, ['ok' => false, 'error' => 'Menu indisponible.']);
$jobId = (int)$job['id'];
mysqli_stmt_bind_param($menuStmt, 'i', $jobId);
mysqli_stmt_execute($menuStmt);
$result = mysqli_stmt_get_result($menuStmt);
$items = [];
while ($row = mysqli_fetch_assoc($result)) {
    $itemId = (int)$row['item_id'];
    $items[] = [
        'code' => (string)$row['code'], 'name' => (string)$row['display_name'],
        'id' => $itemId, 'price' => (int)$row['price'],
        'hunger' => max(0, (int)$row['hunger']),
        'image' => $imageByItem[$itemId] ?? '/nitro/inventory-items/unknown.svg'
    ];
}
mysqli_stmt_close($menuStmt);
if (!$items) restaurant_menu_reply(404, ['ok' => false, 'error' => 'La carte de ce restaurant est vide.']);

restaurant_menu_reply(200, [
    'ok' => true, 'restaurant' => (string)$job['display_name'], 'items' => $items
]);
