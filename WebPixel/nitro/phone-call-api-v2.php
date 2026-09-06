<?php
declare(strict_types=1);

/* ParadiseRP — ParadisePhone call state guard V2
 * Keeps accepted calls alive only while a participant is actively polling status.
 * This prevents an abandoned/closed call from blocking both phones for hours.
 */

require_once __DIR__ . '/../app/Controller/Config.class.php';
require_once __DIR__ . '/../app/Controller/DBManager.class.php';
require_once __DIR__ . '/../app/Modal/SessionMG.class.php';

function pcallv2_stmt(mysqli $db, string $sql, string $types = '', array $params = []): ?mysqli_stmt {
    $stmt = $db->prepare($sql);
    if (!$stmt) return null;
    if ($types !== '') $stmt->bind_param($types, ...$params);
    if (!$stmt->execute()) {
        $stmt->close();
        return null;
    }
    return $stmt;
}

try {
    $session = new SessionMG();

    if ($session->Exist(Config::$SessionName)) {
        $db = (new DBManager())->Con();
        $username = trim((string) $session->Read(Config::$SessionName));
        $userStmt = pcallv2_stmt($db, 'SELECT id FROM users WHERE username=? LIMIT 1', 's', [$username]);
        $user = $userStmt?->get_result()->fetch_assoc();
        $userStmt?->close();

        if ($user) {
            $userId = (int) $user['id'];
            $now = time();
            $action = strtolower(trim((string) ($_GET['action'] ?? 'bootstrap')));
            $method = strtoupper((string) ($_SERVER['REQUEST_METHOD'] ?? 'GET'));

            /* A live Stable V2 call polls status every ~800 ms. Refresh its heartbeat
             * BEFORE sweeping stale rows so a legitimate long call never expires. */
            if ($action === 'status' && $method === 'GET') {
                $callId = max(0, (int) ($_GET['id'] ?? 0));
                if ($callId > 0) {
                    $heartbeat = pcallv2_stmt(
                        $db,
                        "UPDATE phone_calls SET updated_at=? WHERE id=? AND status='accepted' AND (caller_id=? OR callee_id=?)",
                        'iiii',
                        [$now, $callId, $userId, $userId]
                    );
                    $heartbeat?->close();
                }
            }

            /* Previously accepted rows stayed busy for six hours if a tab was closed,
             * refreshed or crashed without sending action=end. Twenty seconds without
             * any participant heartbeat now means the call is abandoned. */
            $acceptedCutoff = $now - 20;
            $cleanup = pcallv2_stmt(
                $db,
                "UPDATE phone_calls SET status='ended',ended_at=?,updated_at=? WHERE status='accepted' AND updated_at<?",
                'iii',
                [$now, $now, $acceptedCutoff]
            );
            $cleanup?->close();
        }
    }
} catch (Throwable $error) {
    /* Guard is best-effort: never make the phone API unavailable because cleanup failed. */
    error_log('[Paradise Phone Call Guard V2] ' . $error->getMessage());
}

require __DIR__ . '/phone-call-api.php';
