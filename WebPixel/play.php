<?php
/**
 * ParadiseRP clean play entry.
 * No legacy RDP wrapper UI is rendered here anymore: only the Nitro iframe.
 */

require_once "app/init.pz.php";

if(!$Session->Exist(Config::$SessionName)):
    header("Location: " . Config::$URL . "/");
    exit;
endif;

header('Cache-Control: no-store, no-cache, must-revalidate, max-age=0');
header('Pragma: no-cache');
header('Expires: 0');

$PageName = "Play";

$autoRoomId = 0;
if (isset($_GET['room']) && is_numeric($_GET['room'])) {
    $autoRoomId = max(0, (int) $_GET['room']);
}

if ($autoRoomId <= 0 && isset($UData['id'])) {
    $uid = (int) $UData['id'];
    $roomResult = $DB->Query("SELECT room_id FROM `play_apartments_owned` WHERE owner = '" . $uid . "' AND room_id > 0 ORDER BY id ASC LIMIT 1");
    if ($roomResult && mysqli_num_rows($roomResult) > 0) {
        $roomRow = mysqli_fetch_assoc($roomResult);
        $autoRoomId = (int) $roomRow['room_id'];
    }
}

if ($autoRoomId <= 0) {
    $roomResult = $DB->Query("SELECT room_id FROM `play_apartments_owned` WHERE room_id > 0 ORDER BY id ASC LIMIT 1");
    if ($roomResult && mysqli_num_rows($roomResult) > 0) {
        $roomRow = mysqli_fetch_assoc($roomResult);
        $autoRoomId = (int) $roomRow['room_id'];
    }
}

// Keep the original client bootstrap only to generate the same SSO ticket variable.
// The HTML it outputs is discarded so the old RDP/HabboVIP UI cannot appear.
ob_start();
try {
    require CLIENT . 'client.php';
} catch (Throwable $e) {
    // Keep page alive; if client.php fails, Nitro will simply receive an empty SSO.
}
ob_end_clean();

$ticket = isset($ClientAUTH) ? (string) $ClientAUTH : '';
$nitroParams = array('sso' => $ticket);
if ($autoRoomId > 0) {
    $nitroParams = array('room' => $autoRoomId, 'sso' => $ticket);
}

$nitroSrc = '/nitro-last/index.html?' . http_build_query($nitroParams, '', '&', PHP_QUERY_RFC3986);
$nitroSrcHtml = htmlspecialchars($nitroSrc, ENT_QUOTES, 'UTF-8');
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
    </style>
</head>
<body>
    <iframe id="RdpNitroFrame" src="<?php echo $nitroSrcHtml; ?>" allow="camera none; microphone *"></iframe>
</body>
</html>
