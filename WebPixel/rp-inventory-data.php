<?php
/** ParadiseRP Phase 3 — authenticated read-only inventory snapshot. */
header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store, no-cache, must-revalidate, max-age=0');
header('X-Content-Type-Options: nosniff');

function pr_inventory_json(array $data, int $status = 200): void
{
    http_response_code($status);
    echo json_encode($data, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}

try {
    require_once __DIR__ . '/app/init.pz.php';
    require_once __DIR__ . '/paradise-inventory-lib.php';

    if (!isset($Session, $DB) || !class_exists('Config')) {
        pr_inventory_json(['ok' => false, 'reason' => 'bootstrap_unavailable']);
    }

    $username = trim((string)$Session->Read(Config::$SessionName));
    if ($username === '') pr_inventory_json(['ok' => false, 'reason' => 'not_connected']);

    $con = $DB->Con();
    if (!($con instanceof mysqli)) pr_inventory_json(['ok' => false, 'reason' => 'database_unavailable']);
    if (!mysqli_set_charset($con, 'utf8mb4')) {
        pr_inventory_json(['ok' => false, 'reason' => 'database_charset_unavailable']);
    }

    $safe = mysqli_real_escape_string($con, $username);
    $result = mysqli_query($con, "SELECT `id`,`username` FROM `users` WHERE `username`='{$safe}' LIMIT 1");
    $user = $result ? (mysqli_fetch_assoc($result) ?: null) : null;
    if ($result) mysqli_free_result($result);
    if (!$user) pr_inventory_json(['ok' => false, 'reason' => 'user_not_found']);

    if (!pr_inventory_table_exists($con, 'rp_item_definitions') || !pr_inventory_table_exists($con, 'rp_inventory_items')) {
        pr_inventory_json([
            'ok' => false,
            'reason' => 'inventory_migration_required',
            'inventory' => ['items' => [], 'weight' => 0, 'capacity' => 50, 'slots_used' => 0, 'max_slots' => 30],
        ]);
    }

    $snapshot = pr_inventory_snapshot($con, (int)$user['id']);
    pr_inventory_json([
        'ok' => true,
        'user_id' => (int)$user['id'],
        'username' => (string)$user['username'],
        'inventory' => $snapshot,
        'server_time' => date(DATE_ATOM),
    ]);
} catch (Throwable $error) {
    error_log('[ParadiseRP Inventory V2] ' . $error->getMessage());
    pr_inventory_json(['ok' => false, 'reason' => 'inventory_unavailable']);
}
