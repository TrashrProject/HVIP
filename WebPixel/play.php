<?php
/**
 * PixelZone / RDP localhost client entry point.
 * Keep the original RP overlay shell and replace only the remote Nitro iframe.
 */

require_once "app/init.pz.php";

if(!$Session->Exist(Config::$SessionName)):
    header("Location: " . Config::$URL . "/");
    exit;
endif;

$PageName = "Play";

// Pick an apartment automatically for localhost testing.
// Priority: explicit ?room=ID -> first apartment owned by the logged user ->
// first existing apartment in the RP database.
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

// The original client template contains the complete RDP RP UI (HUD, phone,
// commands, stats, jobs, taxi, gangs, etc.). Capture it so localhost-only
// substitutions can be applied without destroying that integration.
ob_start();
require_once CLIENT . 'client.php';
$html = ob_get_clean();

$localNitro = Config::$URL . '/nitro-last/index.html?';
if ($autoRoomId > 0) {
    $localNitro .= 'room=' . $autoRoomId . '&';
}
$localNitro .= 'sso=';

$html = str_replace(
    array(
        'https://nitro.habbovip.us/index.html?sso=',
        'https://dev.habbovip.us/index.html?sso='
    ),
    array($localNitro, $localNitro),
    $html
);

// Flash is no longer used, but the legacy template still calls swfobject.
// Also convert only the dedicated RDP overlay websocket (2087) from WSS to
// plain WS on localhost. Nitro itself uses the separate 2097 game bridge.
$localhostShim = <<<'HTML'
<script>
(function () {
    window.swfobject = window.swfobject || { embedSWF: function () {} };

    if (window.WebSocket && !window.__rdpLocalWsShim) {
        window.__rdpLocalWsShim = true;
        const NativeWebSocket = window.WebSocket;
        window.WebSocket = new Proxy(NativeWebSocket, {
            construct(Target, args) {
                if (typeof args[0] === 'string' && /^wss:\/\/(127\.0\.0\.1|localhost):2087\//i.test(args[0])) {
                    args[0] = args[0].replace(/^wss:/i, 'ws:');
                }
                return Reflect.construct(Target, args);
            }
        });
    }
})();
</script>
<style>
html, body { width:100%; height:100%; margin:0; overflow:hidden; background:#000; }
iframe.Nitro { position:fixed; inset:0; width:100%; height:100%; border:0; z-index:1; }
#app { position:relative; z-index:20; }
</style>
HTML;

$html = preg_replace('/<head(.*?)>/i', '<head$1>' . $localhostShim, $html, 1);

echo $html;
