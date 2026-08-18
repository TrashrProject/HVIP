<?php
/**
 * ParadiseRP clean play entry.
 * No legacy RDP wrapper UI is rendered here anymore: only the Nitro iframe.
 * Stable boot: fresh SSO, fixed room forwarding, and one safe auto-recovery.
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

function pr_play_first_room_from_query(mysqli $con, string $sql): int {
    $res = @mysqli_query($con, $sql);
    if ($res && mysqli_num_rows($res) > 0) {
        $row = mysqli_fetch_assoc($res);
        foreach ($row as $value) {
            if (is_numeric($value) && (int)$value > 0) return (int)$value;
        }
    }
    return 0;
}

function pr_play_resolve_room(mysqli $con, int $userId): int {
    $roomId = 0;

    if (isset($_GET['room']) && is_numeric($_GET['room'])) {
        $roomId = max(0, (int)$_GET['room']);
        if ($roomId > 0) return $roomId;
    }

    if (isset($_SESSION['paradise_last_room_id']) && is_numeric($_SESSION['paradise_last_room_id'])) {
        $roomId = max(0, (int)$_SESSION['paradise_last_room_id']);
        if ($roomId > 0) return $roomId;
    }

    if (pr_play_table_exists($con, 'play_apartments_owned')) {
        $roomId = pr_play_first_room_from_query($con, "SELECT room_id FROM `play_apartments_owned` WHERE owner = '" . $userId . "' AND room_id > 0 ORDER BY id ASC LIMIT 1");
        if ($roomId > 0) return $roomId;

        $roomId = pr_play_first_room_from_query($con, "SELECT room_id FROM `play_apartments_owned` WHERE room_id > 0 ORDER BY id ASC LIMIT 1");
        if ($roomId > 0) return $roomId;
    }

    if (pr_play_table_exists($con, 'rooms')) {
        $cols = pr_play_columns($con, 'rooms');
        $idCol = $cols['id'] ?? '';
        if ($idCol !== '') {
            $order = [];
            foreach (['users_now', 'users', 'score'] as $candidate) {
                if (isset($cols[$candidate])) $order[] = '`' . $cols[$candidate] . '` DESC';
            }
            $order[] = '`' . $idCol . '` ASC';
            $roomId = pr_play_first_room_from_query($con, "SELECT `" . $idCol . "` FROM `rooms` WHERE `" . $idCol . "` > 0 ORDER BY " . implode(',', $order) . " LIMIT 1");
            if ($roomId > 0) return $roomId;
        }
    }

    return 0;
}

$con = $DB->Con();
$userId = isset($UData['id']) ? (int)$UData['id'] : 0;
$autoRoomId = $userId > 0 ? pr_play_resolve_room($con, $userId) : 0;
if ($autoRoomId > 0) $_SESSION['paradise_last_room_id'] = $autoRoomId;

$ticket = '';
try {
    if (isset($UserMG) && method_exists($UserMG, 'GenerateAUTH') && $userId > 0) {
        $ticket = (string)$UserMG->GenerateAUTH($userId);
        if (method_exists($UserMG, 'GenerateMachineId')) $UserMG->GenerateMachineId($userId);
        if (method_exists($UserMG, 'CheckVIPStatus')) $UserMG->CheckVIPStatus($userId);
    }
} catch (Throwable $e) {
    $ticket = '';
}

// Fallback only if the direct auth method failed.
if ($ticket === '') {
    ob_start();
    try {
        require CLIENT . 'client.php';
    } catch (Throwable $e) {}
    ob_end_clean();
    $ticket = isset($ClientAUTH) ? (string)$ClientAUTH : '';
}

$bootNonce = time() . '-' . mt_rand(1000, 9999);
$nitroParams = ['sso' => $ticket, '_boot' => $bootNonce];
if ($autoRoomId > 0) {
    $nitroParams = ['room' => $autoRoomId, 'sso' => $ticket, '_boot' => $bootNonce];
}

$nitroSrc = '/nitro-last/index.html?' . http_build_query($nitroParams, '', '&', PHP_QUERY_RFC3986);
$nitroSrcHtml = htmlspecialchars($nitroSrc, ENT_QUOTES, 'UTF-8');
$autoRoomJs = (int)$autoRoomId;
?>
<!doctype html>
<html lang="fr">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no">
    <meta name="theme-color" content="#000000">
    <title>ParadiseRP - Client</title>
    <style>
        html, body {
            width: 100%;
            height: 100%;
            margin: 0;
            padding: 0;
            overflow: hidden;
            background: #000;
        }
        #RdpNitroFrame {
            position: fixed;
            inset: 0;
            width: 100vw;
            height: 100vh;
            border: 0;
            display: block;
            background: #000;
        }
        #ParadiseBootNotice {
            position: fixed;
            left: 50%;
            bottom: 22px;
            transform: translateX(-50%);
            z-index: 10;
            display: none;
            padding: 10px 14px;
            border-radius: 12px;
            color: #dff8ff;
            font: 800 13px/1.2 Arial, sans-serif;
            background: rgba(5, 17, 29, .92);
            border: 1px solid rgba(39, 200, 255, .42);
            box-shadow: 0 12px 28px rgba(0,0,0,.38);
        }
    </style>
</head>
<body>
    <iframe id="RdpNitroFrame" src="<?php echo $nitroSrcHtml; ?>" allow="camera none; microphone *"></iframe>
    <div id="ParadiseBootNotice">Reconnexion au monde...</div>
    <script>
    (function () {
        const frame = document.getElementById('RdpNitroFrame');
        const notice = document.getElementById('ParadiseBootNotice');
        const autoRoomId = <?php echo $autoRoomJs; ?>;
        const recoverKey = 'paradise_play_boot_recover_v2';
        const stableKey = 'paradise_play_last_stable_v2';

        const readRecover = () => {
            try { return JSON.parse(sessionStorage.getItem(recoverKey) || '{}') || {}; }
            catch (_) { return {}; }
        };

        const writeRecover = value => {
            try { sessionStorage.setItem(recoverKey, JSON.stringify(value)); } catch (_) {}
        };

        const markStable = () => {
            try {
                sessionStorage.setItem(stableKey, String(Date.now()));
                sessionStorage.removeItem(recoverKey);
            } catch (_) {}
        };

        const recover = reason => {
            const now = Date.now();
            let state = readRecover();
            if (!state.until || state.until < now) state = { count: 0, until: now + 60000 };
            if (state.count >= 2) {
                if (notice) {
                    notice.textContent = 'Chargement instable : actualise une fois si nécessaire.';
                    notice.style.display = 'block';
                }
                return;
            }
            state.count += 1;
            state.reason = reason || 'unknown';
            writeRecover(state);
            if (notice) notice.style.display = 'block';
            const next = new URL(window.location.href);
            if (autoRoomId > 0) next.searchParams.set('room', String(autoRoomId));
            next.searchParams.set('recover', String(state.count));
            next.searchParams.set('_', String(now));
            window.location.replace(next.toString());
        };

        const inspect = () => {
            let doc;
            try { doc = frame.contentDocument || frame.contentWindow.document; } catch (_) { return ''; }
            if (!doc || !doc.body) return 'blank';

            const root = doc.getElementById('root') || doc.body;
            const rootText = String(root.innerText || root.textContent || '').slice(0, 3000);
            if (/landing\.view|NitroPromo|Que hay|Qué hay|What'?s new/i.test(rootText)) return 'hotel_view_no_room';

            const canvas = Array.from(root.querySelectorAll('canvas')).some(c => {
                const r = c.getBoundingClientRect();
                return r.width > 280 && r.height > 220;
            });
            if (canvas) {
                markStable();
                return '';
            }

            const rootChildren = root.children ? root.children.length : 0;
            if (rootChildren <= 0) return 'empty_root';
            return '';
        };

        const scheduleChecks = () => {
            window.setTimeout(() => {
                const bad = inspect();
                if (bad === 'hotel_view_no_room') recover(bad);
            }, 5500);

            window.setTimeout(() => {
                const bad = inspect();
                if (bad) recover(bad);
            }, 9500);

            window.setTimeout(() => {
                const bad = inspect();
                if (!bad) markStable();
            }, 13000);
        };

        frame.addEventListener('load', scheduleChecks);
        scheduleChecks();
    })();
    </script>
</body>
</html>
