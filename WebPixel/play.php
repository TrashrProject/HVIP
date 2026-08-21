<?php
/**
 * ParadiseRP clean play entry.
 *
 * Canonical public URL: /play
 * - no forced ?room=ID
 * - no public cache-buster query string
 * - no automatic room resolution in PHP
 * - Nitro receives only a fresh SSO ticket
 *
 * Room navigation is left to the normal Nitro / emulator flow.
 */

require_once "app/init.pz.php";

if (!$Session->Exist(Config::$SessionName)) {
    header("Location: " . Config::$URL . "/");
    exit;
}

header('Cache-Control: no-store, no-cache, must-revalidate, max-age=0');
header('Pragma: no-cache');
header('Expires: 0');
header('X-Frame-Options: SAMEORIGIN');

$PageName = "Play";

/*
 * Keep the browser URL clean. This redirect happens BEFORE creating the SSO,
 * so an old /play?room=1&_=... link cannot consume a ticket twice.
 */
if (!empty($_SERVER['QUERY_STRING'])) {
    header('Location: ' . rtrim(Config::$URL, '/') . '/play', true, 302);
    exit;
}

function pr_play_table_exists(mysqli $con, string $table): bool
{
    if (!preg_match('/^[a-z0-9_]+$/i', $table)) return false;

    $safe = mysqli_real_escape_string($con, $table);
    $res = @mysqli_query($con, "SHOW TABLES LIKE '" . $safe . "'");

    return $res && mysqli_num_rows($res) > 0;
}

function pr_play_columns(mysqli $con, string $table): array
{
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

function pr_play_ticket_columns(mysqli $con): array
{
    $cols = pr_play_columns($con, 'users');
    $candidates = [
        'rdpticket',
        'auth_ticket',
        'sso_ticket',
        'sso',
        'ticket',
        'login_ticket',
        'client_ticket',
        'auth_token'
    ];

    $found = [];

    foreach ($candidates as $candidate) {
        if (isset($cols[$candidate])) $found[] = $cols[$candidate];
    }

    return array_values(array_unique($found));
}

function pr_play_read_ticket(mysqli $con, int $userId): string
{
    if ($userId < 0 || !pr_play_table_exists($con, 'users')) return '';

    $cols = pr_play_ticket_columns($con);
    if (!$cols) return '';

    $select = implode(',', array_map(fn($c) => '`' . $c . '`', $cols));
    $res = @mysqli_query(
        $con,
        "SELECT " . $select . " FROM `users` WHERE `id` = '" . $userId . "' LIMIT 1"
    );

    if (!$res) return '';

    $row = mysqli_fetch_assoc($res);
    if (!$row) return '';

    foreach ($cols as $col) {
        $value = trim((string)($row[$col] ?? ''));
        if ($value !== '') return $value;
    }

    return '';
}

function pr_play_store_ticket(mysqli $con, int $userId, string $ticket): bool
{
    if ($userId < 0 || $ticket === '' || !pr_play_table_exists($con, 'users')) return false;

    $cols = pr_play_ticket_columns($con);
    if (!$cols) return false;

    $safeTicket = mysqli_real_escape_string($con, $ticket);
    $sets = [];

    foreach ($cols as $col) {
        $sets[] = '`' . $col . "` = '" . $safeTicket . "'";
    }

    $userCols = pr_play_columns($con, 'users');
    if (isset($userCols['online'])) $sets[] = '`' . $userCols['online'] . "` = '0'";

    $sql = "UPDATE `users` SET " . implode(', ', $sets) .
        " WHERE `id` = '" . $userId . "' LIMIT 1";

    return (bool)@mysqli_query($con, $sql);
}

function pr_play_fresh_ticket(): string
{
    try {
        return AppFunctions::Random(4) . '-' .
            AppFunctions::Random(4) . '-' .
            AppFunctions::Random(4) . '-' .
            AppFunctions::Random(12) . '-RDP';
    } catch (Throwable $e) {
        return 'PRP-' . sha1(uniqid('', true) . mt_rand()) . '-RDP';
    }
}

function pr_play_generate_ticket(mysqli $con, int $userId, $UserMG): string
{
    $ticket = '';

    if (isset($UserMG) && method_exists($UserMG, 'GenerateAUTH') && $userId >= 0) {
        try {
            $ticket = trim((string)$UserMG->GenerateAUTH($userId));
        } catch (Throwable $e) {
            try {
                $ticket = trim((string)$UserMG->GenerateAUTH());
            } catch (Throwable $ignored) {
                $ticket = '';
            }
        }
    }

    $dbTicket = pr_play_read_ticket($con, $userId);

    if ($ticket === '' && $dbTicket !== '') $ticket = $dbTicket;
    if ($ticket === '') $ticket = pr_play_fresh_ticket();

    pr_play_store_ticket($con, $userId, $ticket);

    if (isset($UserMG) && $userId >= 0) {
        try {
            if (method_exists($UserMG, 'GenerateMachineId')) $UserMG->GenerateMachineId($userId);
        } catch (Throwable $e) {
        }

        try {
            if (method_exists($UserMG, 'CheckVIPStatus')) $UserMG->CheckVIPStatus($userId);
        } catch (Throwable $e) {
        }
    }

    return $ticket;
}

function pr_play_auth_debug(mysqli $con, int $userId, string $ticket): array
{
    $result = [
        'userId' => $userId,
        'ticketLength' => strlen($ticket),
        'ticketSuffix' => substr($ticket, -4),
        'columns' => [],
        'rdpticketMatches' => false,
        'online' => null
    ];

    if ($userId < 0 || $ticket === '' || !pr_play_table_exists($con, 'users')) return $result;

    $cols = pr_play_columns($con, 'users');
    $select = [];

    foreach (['rdpticket', 'auth_ticket', 'sso_ticket', 'sso', 'ticket', 'online'] as $candidate) {
        if (isset($cols[$candidate])) $select[] = '`' . $cols[$candidate] . '`';
    }

    if (!$select) return $result;

    $res = @mysqli_query(
        $con,
        "SELECT " . implode(',', $select) . " FROM `users` WHERE `id` = '" . $userId . "' LIMIT 1"
    );

    $row = $res ? mysqli_fetch_assoc($res) : null;
    if (!$row) return $result;

    foreach ($row as $key => $value) {
        if (strtolower($key) === 'online') {
            $result['online'] = (string)$value;
            continue;
        }

        $value = (string)$value;
        $matches = hash_equals($ticket, $value);

        $result['columns'][$key] = [
            'present' => $value !== '',
            'length' => strlen($value),
            'matchesSentTicket' => $matches
        ];

        if (strtolower($key) === 'rdpticket') $result['rdpticketMatches'] = $matches;
    }

    return $result;
}

$con = $DB->Con();
$userId = isset($UData['id']) ? (int)$UData['id'] : -1;

$ticket = pr_play_generate_ticket($con, $userId, $UserMG ?? null);

if ($ticket === '') {
    ob_start();

    try {
        require CLIENT . 'client.php';
    } catch (Throwable $e) {
    }

    ob_end_clean();

    $ticket = isset($ClientAUTH) ? trim((string)$ClientAUTH) : '';

    if ($ticket !== '') pr_play_store_ticket($con, $userId, $ticket);
}

$authDebug = pr_play_auth_debug($con, $userId, $ticket);

/*
 * No room parameter is passed to Nitro. The SSO itself changes on a fresh
 * /play load, so a public &_=timestamp cache-buster is unnecessary.
 */
$nitroParams = ['sso' => $ticket];
$nitroSrc = '/nitro-last/index.html?' . http_build_query(
    $nitroParams,
    '',
    '&',
    PHP_QUERY_RFC3986
);

$nitroSrcHtml = htmlspecialchars($nitroSrc, ENT_QUOTES, 'UTF-8');
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
        html,
        body {
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
    </style>
</head>
<body>
    <iframe
        id="RdpNitroFrame"
        src="<?php echo $nitroSrcHtml; ?>"
        allow="camera none; microphone *"
    ></iframe>

    <script>
    window.__PARADISE_PLAY_AUTH__ = <?php echo $authDebugJs; ?>;
    </script>
</body>
</html>
