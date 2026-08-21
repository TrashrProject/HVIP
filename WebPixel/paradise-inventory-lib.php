<?php
/** ParadiseRP Phase 3 — read-only Inventory V2 projection helpers. */

function pr_inventory_table_exists(mysqli $con, string $table): bool
{
    if (!preg_match('/^[a-z0-9_]+$/i', $table)) return false;
    $safe = mysqli_real_escape_string($con, $table);
    $result = @mysqli_query($con, "SHOW TABLES LIKE '{$safe}'");
    $exists = $result && mysqli_num_rows($result) > 0;
    if ($result) mysqli_free_result($result);
    return $exists;
}

function pr_inventory_profile(mysqli $con, int $userId): array
{
    $default = ['base_capacity' => 50.0, 'capacity_bonus' => 0.0, 'capacity' => 50.0, 'max_slots' => 30];
    if (!pr_inventory_table_exists($con, 'rp_inventory_profiles')) return $default;
    $stmt = mysqli_prepare($con, 'SELECT `base_capacity`,`capacity_bonus`,`max_slots` FROM `rp_inventory_profiles` WHERE `user_id`=? LIMIT 1');
    if (!$stmt) return $default;
    mysqli_stmt_bind_param($stmt, 'i', $userId);
    mysqli_stmt_execute($stmt);
    $result = mysqli_stmt_get_result($stmt);
    $row = $result ? (mysqli_fetch_assoc($result) ?: null) : null;
    if ($result) mysqli_free_result($result);
    mysqli_stmt_close($stmt);
    if (!$row) return $default;
    $base = max(0.0, (float)$row['base_capacity']);
    $bonus = max(0.0, (float)$row['capacity_bonus']);
    return [
        'base_capacity' => $base,
        'capacity_bonus' => $bonus,
        'capacity' => $base + $bonus,
        'max_slots' => max(1, (int)$row['max_slots']),
    ];
}

function pr_inventory_safe_icon($value): ?string
{
    $value = trim((string)$value);
    if ($value === '') return null;
    // Phase 3 never emits remote URLs. Verified local paths can be configured later.
    if (preg_match('#^(?:\./|/)[a-z0-9_./%\-]+$#i', $value)) return $value;
    return null;
}

function pr_inventory_items(mysqli $con, int $userId): array
{
    if (!pr_inventory_table_exists($con, 'rp_inventory_items') || !pr_inventory_table_exists($con, 'rp_item_definitions')) return [];
    $sql = 'SELECT i.`id`,i.`quantity`,i.`metadata`,i.`slot`,d.`id` AS definition_id,d.`code`,d.`name`,d.`description`,d.`category`,d.`weight`,d.`max_stack`,d.`icon`,d.`usable`,d.`tradeable`,d.`droppable`,d.`effect_type` FROM `rp_inventory_items` i INNER JOIN `rp_item_definitions` d ON d.`id`=i.`item_definition_id` WHERE i.`owner_user_id`=? AND i.`quantity`>0 ORDER BY COALESCE(i.`slot`,2147483647),i.`id`';
    $stmt = mysqli_prepare($con, $sql);
    if (!$stmt) return [];
    mysqli_stmt_bind_param($stmt, 'i', $userId);
    mysqli_stmt_execute($stmt);
    $result = mysqli_stmt_get_result($stmt);
    $items = [];
    if ($result) {
        while ($row = mysqli_fetch_assoc($result)) {
            $qty = max(0, (int)$row['quantity']);
            $weight = max(0.0, (float)$row['weight']);
            $effect = strtoupper((string)$row['effect_type']);
            $actions = [];
            if ((bool)$row['usable']) $actions[] = 'use';
            if ((bool)$row['tradeable']) $actions[] = 'give';
            if ((bool)$row['droppable']) $actions[] = 'drop';
            if ($effect === 'KEY') $actions = array_values(array_unique(array_merge(['inspect'], $actions)));
            $items[] = [
                'key' => 'item:' . (string)$row['id'],
                'id' => (int)$row['id'],
                'source' => 'inventory',
                'definition_id' => (int)$row['definition_id'],
                'code' => (string)$row['code'],
                'name' => (string)$row['name'],
                'description' => (string)$row['description'],
                'category' => strtoupper((string)$row['category']),
                'weight' => $weight,
                'total_weight' => $weight * $qty,
                'quantity' => $qty,
                'max_stack' => max(1, (int)$row['max_stack']),
                'icon' => pr_inventory_safe_icon($row['icon']),
                'usable' => (bool)$row['usable'],
                'tradeable' => (bool)$row['tradeable'],
                'droppable' => (bool)$row['droppable'],
                'effect_type' => $effect,
                'metadata' => $row['metadata'] !== null ? (string)$row['metadata'] : null,
                'slot' => $row['slot'] !== null ? (int)$row['slot'] : null,
                'actions' => $actions,
                'locked' => false,
            ];
        }
        mysqli_free_result($result);
    }
    mysqli_stmt_close($stmt);
    return $items;
}

function pr_inventory_document_items(mysqli $con, int $userId): array
{
    $lib = __DIR__ . '/paradise-character-lib.php';
    if (!is_file($lib)) return [];
    require_once $lib;
    if (!function_exists('pr_character_documents')) return [];

    $out = [];
    foreach (pr_character_documents($con, $userId) as $document) {
        $status = strtoupper((string)($document['status'] ?? 'UNKNOWN'));
        $valid = $status === 'VALID';
        $out[] = [
            'key' => 'document:' . (int)$document['id'],
            'id' => null,
            'source' => 'document',
            'document_id' => (int)$document['id'],
            'document_type' => (string)$document['type'],
            'code' => 'DOCUMENT_' . (string)$document['type'],
            'name' => (string)$document['name'],
            'description' => 'Document officiel lié à votre identité ParadiseRP.',
            'category' => 'DOCUMENT',
            'weight' => 0.0,
            'total_weight' => 0.0,
            'quantity' => 1,
            'max_stack' => 1,
            'icon' => null,
            'usable' => $valid,
            'tradeable' => false,
            'droppable' => false,
            'effect_type' => 'DOCUMENT',
            'metadata' => null,
            'slot' => null,
            'status' => $status,
            'number' => $document['number'] ?? null,
            'actions' => $valid ? ['view','present'] : ['view'],
            'locked' => true,
        ];
    }
    return $out;
}

function pr_inventory_snapshot(mysqli $con, int $userId): array
{
    $profile = pr_inventory_profile($con, $userId);
    $physical = pr_inventory_items($con, $userId);
    $documents = pr_inventory_document_items($con, $userId);
    $weight = 0.0;
    foreach ($physical as $item) $weight += (float)$item['total_weight'];
    return [
        'items' => array_merge($physical, $documents),
        'physical_items' => count($physical),
        'documents' => count($documents),
        'slots_used' => count($physical),
        'max_slots' => $profile['max_slots'],
        'weight' => round($weight, 3),
        'capacity' => round((float)$profile['capacity'], 3),
        'base_capacity' => round((float)$profile['base_capacity'], 3),
        'capacity_bonus' => round((float)$profile['capacity_bonus'], 3),
    ];
}
