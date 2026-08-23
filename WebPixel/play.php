<?php
/**
 * ParadiseRP clean play entry.
 * Only the Nitro iframe is rendered here.
 * Stable room boot: explicit /play?room=ID URL, fresh SSO, validated room id.
 */

require_once "app/init.pz.php";

if(!$Session->Exist(Config::$SessionName)):
    header("Location: " . Config::$URL . "/");
    exit;
endif;

header('Cache-Control: no-store, no-cache, must-revalidate, max-age=0');
header('Pragma: no-cache');
header('Expires: 0');
header('X-Frame-Options: SAMEORIGIN');

$PageName = "Play";

function pr_play_table_exists(mysqli $con, string $table): bool {
    if (!preg_match('/^[a-z0-9_]+$/i', $table)) return false;
    $safe = mysqli_real_escape_string($con, $table);
    $res = @mysqli_query($con, "SHOW TABLES LIKE '" . $safe . "'");
    return $res && mysqli_num_rows($res) > 0;
}

function pr_play_columns(mysqli $con, string $table): array {
    $cols = [];
    if (!preg_match('/^[a-z0-9_]+$/i', $table)) return $cols;
    $res = @mysqli_query($con, "SHOW COLUMNS FROM `" . $table . "`");
    if ($res) {
        while ($row = mysqli_fetch_assoc($res)) {
            $field = strtolower((string)($row['Field'] ?? ''));
            if ($field !== '') $cols[$field] = (string)$row['Field'];
        }
    }
    return $cols;
}

function pr_play_room_exists(mysqli $con, int $roomId): bool {
    if ($roomId <= 0) return false;
    if (!pr_play_table_exists($con, 'rooms')) return true;
    $cols = pr_play_columns($con, 'rooms');
    $idCol = $cols['id'] ?? '';
    if ($idCol === '') return true;
    $res = @mysqli_query($con, "SELECT 1 FROM `rooms` WHERE `" . $idCol . "` = '" . $roomId . "' LIMIT 1");
    return $res && mysqli_num_rows($res) > 0;
}

function pr_play_first_valid_room(mysqli $con, string $sql): int {
    $res = @mysqli_query($con, $sql);
    if (!$res) return 0;
    while ($row = mysqli_fetch_assoc($res)) {
        foreach ($row as $value) {
            if (is_numeric($value) && (int)$value > 0 && pr_play_room_exists($con, (int)$value)) return (int)$value;
        }
    }
    return 0;
}

function pr_play_resolve_room(mysqli $con, int $userId, string $username): int {
    if (isset($_GET['room']) && is_numeric($_GET['room'])) {
        $roomId = max(0, (int)$_GET['room']);
        if (pr_play_room_exists($con, $roomId)) return $roomId;
        unset($_SESSION['paradise_last_room_id']);
    }

    if (isset($_SESSION['paradise_last_room_id']) && is_numeric($_SESSION['paradise_last_room_id'])) {
        $roomId = max(0, (int)$_SESSION['paradise_last_room_id']);
        if (pr_play_room_exists($con, $roomId)) return $roomId;
        unset($_SESSION['paradise_last_room_id']);
    }

    if (pr_play_table_exists($con, 'play_apartments_owned')) {
        $roomId = pr_play_first_valid_room($con, "SELECT room_id FROM `play_apartments_owned` WHERE owner = '" . $userId . "' AND room_id > 0 ORDER BY id ASC LIMIT 10");
        if ($roomId > 0) return $roomId;
    }

    if (pr_play_table_exists($con, 'rooms')) {
        $cols = pr_play_columns($con, 'rooms');
        $idCol = $cols['id'] ?? '';
        if ($idCol !== '') {
            foreach (['owner_id', 'userid', 'user_id'] as $ownerCol) {
                if (isset($cols[$ownerCol])) {
                    $roomId = pr_play_first_valid_room($con, "SELECT `" . $idCol . "` FROM `rooms` WHERE `" . $cols[$ownerCol] . "` = '" . $userId . "' AND `" . $idCol . "` > 0 ORDER BY `" . $idCol . "` ASC LIMIT 10");
                    if ($roomId > 0) return $roomId;
                }
            }
            foreach (['owner', 'owner_name', 'username'] as $ownerCol) {
                if (isset($cols[$ownerCol])) {
                    $safeName = mysqli_real_escape_string($con, $username);
                    $roomId = pr_play_first_valid_room($con, "SELECT `" . $idCol . "` FROM `rooms` WHERE `" . $cols[$ownerCol] . "` = '" . $safeName . "' AND `" . $idCol . "` > 0 ORDER BY `" . $idCol . "` ASC LIMIT 10");
                    if ($roomId > 0) return $roomId;
                }
            }

            $order = [];
            foreach (['users_now', 'users', 'score'] as $candidate) if (isset($cols[$candidate])) $order[] = '`' . $cols[$candidate] . '` DESC';
            $order[] = '`' . $idCol . '` ASC';
            $roomId = pr_play_first_valid_room($con, "SELECT `" . $idCol . "` FROM `rooms` WHERE `" . $idCol . "` > 0 ORDER BY " . implode(',', $order) . " LIMIT 20");
            if ($roomId > 0) return $roomId;
        }
    }

    if (pr_play_table_exists($con, 'play_apartments_owned')) {
        $roomId = pr_play_first_valid_room($con, "SELECT room_id FROM `play_apartments_owned` WHERE room_id > 0 ORDER BY id ASC LIMIT 20");
        if ($roomId > 0) return $roomId;
    }

    return 0;
}

function pr_play_ticket_columns(mysqli $con): array {
    $cols = pr_play_columns($con, 'users');
    $candidates = ['rdpticket', 'auth_ticket', 'sso_ticket', 'sso', 'ticket', 'login_ticket', 'client_ticket', 'auth_token'];
    $found = [];
    foreach ($candidates as $candidate) if (isset($cols[$candidate])) $found[] = $cols[$candidate];
    return array_values(array_unique($found));
}

function pr_play_read_ticket(mysqli $con, int $userId): string {
    if ($userId < 0 || !pr_play_table_exists($con, 'users')) return '';
    $cols = pr_play_ticket_columns($con);
    if (!$cols) return '';
    $select = implode(',', array_map(fn($c) => '`' . $c . '`', $cols));
    $res = @mysqli_query($con, "SELECT " . $select . " FROM `users` WHERE `id` = '" . $userId . "' LIMIT 1");
    if (!$res) return '';
    $row = mysqli_fetch_assoc($res);
    if (!$row) return '';
    foreach ($cols as $col) {
        $value = trim((string)($row[$col] ?? ''));
        if ($value !== '') return $value;
    }
    return '';
}

function pr_play_store_ticket(mysqli $con, int $userId, string $ticket): bool {
    if ($userId < 0 || $ticket === '' || !pr_play_table_exists($con, 'users')) return false;
    $cols = pr_play_ticket_columns($con);
    if (!$cols) return false;
    $safeTicket = mysqli_real_escape_string($con, $ticket);
    $sets = [];
    foreach ($cols as $col) $sets[] = '`' . $col . "` = '" . $safeTicket . "'";

    $userCols = pr_play_columns($con, 'users');
    if (isset($userCols['online'])) $sets[] = '`' . $userCols['online'] . "` = '0'";

    $sql = "UPDATE `users` SET " . implode(', ', $sets) . " WHERE `id` = '" . $userId . "' LIMIT 1";
    return (bool) @mysqli_query($con, $sql);
}

function pr_play_fresh_ticket(): string {
    try { return AppFunctions::Random(4) . '-' . AppFunctions::Random(4) . '-' . AppFunctions::Random(4) . '-' . AppFunctions::Random(12) . '-RDP'; }
    catch (Throwable $e) { return 'PRP-' . sha1(uniqid('', true) . mt_rand()) . '-RDP'; }
}

function pr_play_generate_ticket(mysqli $con, int $userId, $UserMG): string {
    $ticket = '';

    if (isset($UserMG) && method_exists($UserMG, 'GenerateAUTH') && $userId >= 0) {
        try { $ticket = trim((string)$UserMG->GenerateAUTH($userId)); }
        catch (Throwable $e) {
            try { $ticket = trim((string)$UserMG->GenerateAUTH()); }
            catch (Throwable $ignored) { $ticket = ''; }
        }
    }

    $dbTicket = pr_play_read_ticket($con, $userId);
    if ($ticket === '' && $dbTicket !== '') $ticket = $dbTicket;
    if ($ticket === '') $ticket = pr_play_fresh_ticket();

    pr_play_store_ticket($con, $userId, $ticket);

    if (isset($UserMG) && $userId >= 0) {
        try { if (method_exists($UserMG, 'GenerateMachineId')) $UserMG->GenerateMachineId($userId); } catch (Throwable $e) {}
        try { if (method_exists($UserMG, 'CheckVIPStatus')) $UserMG->CheckVIPStatus($userId); } catch (Throwable $e) {}
    }

    return $ticket;
}

function pr_play_auth_debug(mysqli $con, int $userId, string $ticket): array {
    $result = ['userId' => $userId, 'ticketLength' => strlen($ticket), 'ticketSuffix' => substr($ticket, -4), 'columns' => [], 'rdpticketMatches' => false, 'online' => null];
    if ($userId < 0 || $ticket === '' || !pr_play_table_exists($con, 'users')) return $result;
    $cols = pr_play_columns($con, 'users');
    $select = [];
    foreach (['rdpticket', 'auth_ticket', 'sso_ticket', 'sso', 'ticket', 'online'] as $candidate) if (isset($cols[$candidate])) $select[] = '`' . $cols[$candidate] . '`';
    if (!$select) return $result;
    $res = @mysqli_query($con, "SELECT " . implode(',', $select) . " FROM `users` WHERE `id` = '" . $userId . "' LIMIT 1");
    $row = $res ? mysqli_fetch_assoc($res) : null;
    if (!$row) return $result;
    foreach ($row as $key => $value) {
        if (strtolower($key) === 'online') { $result['online'] = (string)$value; continue; }
        $value = (string)$value;
        $matches = hash_equals($ticket, $value);
        $result['columns'][$key] = ['present' => $value !== '', 'length' => strlen($value), 'matchesSentTicket' => $matches];
        if (strtolower($key) === 'rdpticket') $result['rdpticketMatches'] = $matches;
    }
    return $result;
}

$con = $DB->Con();
$userId = isset($UData['id']) ? (int)$UData['id'] : -1;
$username = isset($UData['username']) ? (string)$UData['username'] : '';
$autoRoomId = $userId >= 0 ? pr_play_resolve_room($con, $userId, $username) : 0;
if ($autoRoomId > 0) $_SESSION['paradise_last_room_id'] = $autoRoomId;

$currentRoom = (isset($_GET['room']) && is_numeric($_GET['room'])) ? (int)$_GET['room'] : 0;
if ($autoRoomId > 0 && $currentRoom !== $autoRoomId) {
    $target = rtrim(Config::$URL, '/') . '/play?room=' . $autoRoomId;
    if (isset($_GET['prdebug']) && $_GET['prdebug'] === '1') $target .= '&prdebug=1';
    $target .= '&_=' . time();
    header('Location: ' . $target, true, 302);
    exit;
}

$ticket = pr_play_generate_ticket($con, $userId, $UserMG ?? null);

if ($ticket === '') {
    ob_start();
    try { require CLIENT . 'client.php'; } catch (Throwable $e) {}
    ob_end_clean();
    $ticket = isset($ClientAUTH) ? (string)$ClientAUTH : '';
    if ($ticket !== '') pr_play_store_ticket($con, $userId, $ticket);
}

$authDebug = pr_play_auth_debug($con, $userId, $ticket);

$bootNonce = time() . '-' . mt_rand(1000, 9999);
$nitroParams = ['sso' => $ticket, '_boot' => $bootNonce];
if ($autoRoomId > 0) $nitroParams = ['room' => $autoRoomId, 'sso' => $ticket, '_boot' => $bootNonce];
if (isset($_GET['prdebug']) && $_GET['prdebug'] === '1') $nitroParams['prdebug'] = '1';

$nitroSrc = '/nitro/index.html?' . http_build_query($nitroParams, '', '&', PHP_QUERY_RFC3986);
$nitroSrcHtml = htmlspecialchars($nitroSrc, ENT_QUOTES, 'UTF-8');
$autoRoomJs = (int)$autoRoomId;
$ticketJs = json_encode($ticket, JSON_UNESCAPED_SLASHES);
$authDebugJs = json_encode($authDebug, JSON_UNESCAPED_SLASHES);
?>
<!doctype html>
<html lang="fr">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no">
    <meta name="theme-color" content="#000000">
    <title>ParadiseRP - Client</title>
    <style>
        html, body { width: 100%; height: 100%; margin: 0; padding: 0; overflow: hidden; background: #000; }
        #RdpNitroFrame { position: fixed; inset: 0; width: 100vw; height: 100vh; border: 0; display: block; background: #000; }
        #ParadiseBootNotice {
            position: fixed; left: 50%; bottom: 22px; transform: translateX(-50%); z-index: 10; display: none;
            padding: 10px 14px; border-radius: 12px; color: #dff8ff; font: 800 13px/1.2 Arial, sans-serif;
            background: rgba(5,17,29,.92); border: 1px solid rgba(39,200,255,.42); box-shadow: 0 12px 28px rgba(0,0,0,.38);
        }
    </style>
</head>
<body>
    <iframe id="RdpNitroFrame" src="<?php echo $nitroSrcHtml; ?>" allow="camera none; microphone *"></iframe>
    <div id="ParadiseBootNotice">Reconnexion à l'appart...</div>
    <script>
    (function () {
        window.__PARADISE_PLAY_AUTH__ = <?php echo $authDebugJs; ?>;

        const frame = document.getElementById('RdpNitroFrame');
        const notice = document.getElementById('ParadiseBootNotice');
        const autoRoomId = <?php echo $autoRoomJs; ?>;
        const ticket = <?php echo $ticketJs; ?>;
        const recoverKey = 'paradise_play_room_recover_v5_' + autoRoomId;
        const debug = new URLSearchParams(window.location.search).get('prdebug') === '1';

        if (debug) console.log('[ParadiseRP:play-auth]', window.__PARADISE_PLAY_AUTH__);

        const getState = () => { try { return JSON.parse(sessionStorage.getItem(recoverKey) || '{}') || {}; } catch (_) { return {}; } };
        const setState = value => { try { sessionStorage.setItem(recoverKey, JSON.stringify(value)); } catch (_) {} };

        const forceFrameRoom = reason => {
            if (!autoRoomId || !frame) return;
            const now = Date.now();
            let state = getState();
            if (!state.until || state.until < now) state = { count: 0, until: now + 60000 };
            if (state.count >= 2) return;
            state.count += 1;
            state.reason = reason || 'roomless';
            setState(state);
            if (notice) notice.style.display = 'block';
            const next = new URL('/nitro/index.html', window.location.origin);
            next.searchParams.set('room', String(autoRoomId));
            next.searchParams.set('sso', ticket || '');
            next.searchParams.set('_boot', String(now));
            next.searchParams.set('force_room', String(state.count));
            if (debug) next.searchParams.set('prdebug', '1');
            frame.src = next.toString();
        };

        const inspect = () => {
            let doc;
            try { doc = frame.contentDocument || frame.contentWindow.document; } catch (_) { return 'cross'; }
            if (!doc || !doc.body) return 'blank';
            const root = doc.getElementById('root') || doc.body;
            const text = String(root.innerText || root.textContent || '').slice(0, 4000);
            if (/hotel|landing|what'?s new|que hay|qué hay|navigator/i.test(text) && !/discussion générale|quetes quotidiennes|quêtes quotidiennes/i.test(text)) return 'hotel_view';
            if ((root.children ? root.children.length : 0) <= 0) return 'empty_root';
            return '';
        };

        const scheduleChecks = () => {
            window.setTimeout(() => {
                if (!autoRoomId) return;
                try {
                    const url = new URL(frame.contentWindow.location.href);
                    if (url.searchParams.get('room') !== String(autoRoomId)) forceFrameRoom('missing_room_param');
                    if (!url.searchParams.get('sso')) forceFrameRoom('missing_sso_param');
                } catch (_) {}
            }, 1200);
            window.setTimeout(() => { const bad = inspect(); if (bad === 'hotel_view') forceFrameRoom(bad); }, 6500);
            window.setTimeout(() => { const bad = inspect(); if (bad === 'empty_root' || bad === 'blank') forceFrameRoom(bad); }, 10500);
        };

        frame.addEventListener('load', scheduleChecks);
        scheduleChecks();
    })();
    </script>
</body>
</html>
