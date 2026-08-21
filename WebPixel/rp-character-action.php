<?php
/**
 * ParadiseRP Phase 2 authenticated write endpoint.
 * Only character fields explicitly allowed by the Phase 2 design can be changed.
 * Money, reputation, citizen id, jobs, licences and document ownership are never
 * accepted from the browser.
 */
header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store, no-cache, must-revalidate, max-age=0');
header('X-Content-Type-Options: nosniff');

require_once __DIR__ . '/app/init.pz.php';
require_once __DIR__ . '/paradise-character-lib.php';

function pr_character_action_json(array $data, int $status = 200): void
{
    http_response_code($status);
    echo json_encode($data, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    pr_character_action_json(['ok' => false, 'reason' => 'method_not_allowed'], 405);
}

if (($_SERVER['HTTP_X_PARADISE_ACTION'] ?? '') !== 'phase2') {
    pr_character_action_json(['ok' => false, 'reason' => 'missing_action_header'], 403);
}

if (!empty($_SERVER['HTTP_ORIGIN'])) {
    $originHost = strtolower((string)parse_url($_SERVER['HTTP_ORIGIN'], PHP_URL_HOST));
    $requestHost = strtolower(preg_replace('/:\d+$/', '', (string)($_SERVER['HTTP_HOST'] ?? '')));
    if ($originHost !== '' && $requestHost !== '' && $originHost !== $requestHost) {
        pr_character_action_json(['ok' => false, 'reason' => 'origin_rejected'], 403);
    }
}

if (!isset($Session, $DB) || !class_exists('Config')) {
    pr_character_action_json(['ok' => false, 'reason' => 'bootstrap_unavailable'], 503);
}

$username = trim((string)$Session->Read(Config::$SessionName));
if ($username === '') {
    pr_character_action_json(['ok' => false, 'reason' => 'not_connected'], 401);
}

$con = $DB->Con();
if (!($con instanceof mysqli)) {
    pr_character_action_json(['ok' => false, 'reason' => 'database_unavailable'], 503);
}

$stmt = mysqli_prepare($con, 'SELECT `id`,`username` FROM `users` WHERE `username` = ? LIMIT 1');
if (!$stmt) pr_character_action_json(['ok' => false, 'reason' => 'user_lookup_failed'], 500);
mysqli_stmt_bind_param($stmt, 's', $username);
mysqli_stmt_execute($stmt);
$result = mysqli_stmt_get_result($stmt);
$user = $result ? (mysqli_fetch_assoc($result) ?: null) : null;
if ($result) mysqli_free_result($result);
mysqli_stmt_close($stmt);
if (!$user) pr_character_action_json(['ok' => false, 'reason' => 'user_not_found'], 401);
$userId = (int)$user['id'];

$raw = file_get_contents('php://input');
if ($raw === false || strlen($raw) > 12000) {
    pr_character_action_json(['ok' => false, 'reason' => 'invalid_payload'], 400);
}
$input = json_decode($raw !== '' ? $raw : '{}', true);
if (!is_array($input)) pr_character_action_json(['ok' => false, 'reason' => 'invalid_json'], 400);
$action = strtolower(trim((string)($input['action'] ?? '')));

try {
    if ($action === 'create_identity') {
        foreach (['rp_characters','rp_document_types','rp_player_documents'] as $table) {
            if (!pr_character_table_exists($con, $table)) {
                pr_character_action_json(['ok' => false, 'reason' => 'phase2_migration_required', 'table' => $table], 409);
            }
        }
        if (pr_character_row($con, $userId)) {
            pr_character_action_json(['ok' => false, 'reason' => 'identity_already_exists'], 409);
        }

        $errors = [];
        $identity = pr_character_validate_identity($input, $errors);
        if (!$identity) {
            pr_character_action_json(['ok' => false, 'reason' => 'validation_failed', 'errors' => $errors], 422);
        }

        mysqli_begin_transaction($con);
        try {
            $citizenId = pr_character_generate_number('PID', 6);
            $firstName = (string)$identity['first_name'];
            $lastName = (string)$identity['last_name'];
            $birthDate = (string)$identity['birth_date'];
            $gender = $identity['gender'];
            $nationality = (string)$identity['nationality'];
            $biography = $identity['biography'];

            $stmt = mysqli_prepare($con, 'INSERT INTO `rp_characters` (`user_id`,`citizen_id`,`first_name`,`last_name`,`birth_date`,`gender`,`nationality`,`biography`,`reputation`) VALUES (?,?,?,?,?,?,?,?,0)');
            if (!$stmt) throw new RuntimeException('character_insert_prepare_failed');
            mysqli_stmt_bind_param($stmt, 'isssssss', $userId, $citizenId, $firstName, $lastName, $birthDate, $gender, $nationality, $biography);
            if (!mysqli_stmt_execute($stmt)) throw new RuntimeException('character_insert_failed');
            mysqli_stmt_close($stmt);

            $typeCode = 'PLACID_ID';
            $stmt = mysqli_prepare($con, 'SELECT `id` FROM `rp_document_types` WHERE `code` = ? LIMIT 1');
            if (!$stmt) throw new RuntimeException('document_type_prepare_failed');
            mysqli_stmt_bind_param($stmt, 's', $typeCode);
            mysqli_stmt_execute($stmt);
            $typeResult = mysqli_stmt_get_result($stmt);
            $typeRow = $typeResult ? (mysqli_fetch_assoc($typeResult) ?: null) : null;
            if ($typeResult) mysqli_free_result($typeResult);
            mysqli_stmt_close($stmt);
            if (!$typeRow) throw new RuntimeException('identity_document_type_missing');

            $typeId = (int)$typeRow['id'];
            $documentNumber = pr_character_generate_number('PI', 6);
            $stmt = mysqli_prepare($con, "INSERT INTO `rp_player_documents` (`user_id`,`document_type_id`,`document_number`,`issued_at`,`status`) VALUES (?,?,?,NOW(),'VALID')");
            if (!$stmt) throw new RuntimeException('identity_document_prepare_failed');
            mysqli_stmt_bind_param($stmt, 'iis', $userId, $typeId, $documentNumber);
            if (!mysqli_stmt_execute($stmt)) throw new RuntimeException('identity_document_insert_failed');
            mysqli_stmt_close($stmt);

            mysqli_commit($con);
        } catch (Throwable $e) {
            mysqli_rollback($con);
            throw $e;
        }

        pr_character_action_json([
            'ok' => true,
            'action' => 'create_identity',
            'character' => pr_character_snapshot($con, $userId),
            'documents' => pr_character_documents($con, $userId),
        ]);
    }

    if ($action === 'update_biography') {
        $character = pr_character_row($con, $userId);
        if (!$character) pr_character_action_json(['ok' => false, 'reason' => 'identity_required'], 409);
        $biography = pr_character_clean_bio($input['biography'] ?? '');
        $bioLength = function_exists('mb_strlen') ? mb_strlen($biography) : strlen($biography);
        if ($bioLength > 400) {
            pr_character_action_json(['ok' => false, 'reason' => 'validation_failed', 'errors' => ['La biographie est limitée à 400 caractères.']], 422);
        }
        $stmt = mysqli_prepare($con, 'UPDATE `rp_characters` SET `biography` = ? WHERE `user_id` = ? LIMIT 1');
        if (!$stmt) throw new RuntimeException('biography_prepare_failed');
        mysqli_stmt_bind_param($stmt, 'si', $biography, $userId);
        mysqli_stmt_execute($stmt);
        mysqli_stmt_close($stmt);
        pr_character_action_json(['ok' => true, 'action' => 'update_biography', 'character' => pr_character_snapshot($con, $userId)]);
    }

    if ($action === 'consume_ui_event') {
        $eventId = (int)($input['event_id'] ?? 0);
        if ($eventId <= 0 || !pr_character_table_exists($con, 'rp_ui_events')) {
            pr_character_action_json(['ok' => false, 'reason' => 'invalid_event'], 400);
        }
        $stmt = mysqli_prepare($con, "UPDATE `rp_ui_events` SET `status` = 'CONSUMED',`consumed_at` = NOW() WHERE `id` = ? AND `user_id` = ? AND `status` = 'PENDING' LIMIT 1");
        mysqli_stmt_bind_param($stmt, 'ii', $eventId, $userId);
        mysqli_stmt_execute($stmt);
        mysqli_stmt_close($stmt);
        pr_character_action_json(['ok' => true, 'action' => 'consume_ui_event']);
    }

    if ($action === 'view_document_offer') {
        $offerId = (int)($input['offer_id'] ?? 0);
        if ($offerId <= 0 || !pr_character_table_exists($con, 'rp_document_shares')) {
            pr_character_action_json(['ok' => false, 'reason' => 'invalid_offer'], 400);
        }
        $stmt = mysqli_prepare($con, "UPDATE `rp_document_shares` SET `status` = 'VIEWED',`viewed_at` = NOW() WHERE `id` = ? AND `target_user_id` = ? AND `status` = 'PENDING' AND `expires_at` > NOW() LIMIT 1");
        mysqli_stmt_bind_param($stmt, 'ii', $offerId, $userId);
        mysqli_stmt_execute($stmt);
        mysqli_stmt_close($stmt);
        pr_character_action_json(['ok' => true, 'action' => 'view_document_offer']);
    }

    pr_character_action_json(['ok' => false, 'reason' => 'unknown_action'], 400);
} catch (Throwable $e) {
    error_log('[ParadiseRP Phase2] ' . $e->getMessage());
    pr_character_action_json(['ok' => false, 'reason' => 'character_action_failed'], 500);
}
