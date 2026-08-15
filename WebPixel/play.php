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

$localhostShim = <<<'HTML'
<link rel="stylesheet" href="/WebPixel/app/View/Directory/Client/websockets/ws_overlays/Phones/iPhone/resources/css/hvip-phone-modern.css?v=8">
<script>
(function () {
    window.swfobject = window.swfobject || { embedSWF: function () {} };

    if (window.WebSocket && !window.__rdpLocalWsShim) {
        window.__rdpLocalWsShim = true;
        const NativeWebSocket = window.WebSocket;
        window.WebSocket = new Proxy(NativeWebSocket, {
            construct(Target, args) {
                let isRdpSocket = false;
                if (typeof args[0] === 'string' && /^wss:\/\/(127\.0\.0\.1|localhost):2087\//i.test(args[0])) {
                    args[0] = args[0].replace(/^wss:/i, 'ws:');
                    isRdpSocket = true;
                }

                const socket = Reflect.construct(Target, args);
                if (isRdpSocket) {
                    socket.addEventListener('message', function(event) {
                        if (typeof event.data !== 'string' || !/^compose_loader\|/i.test(event.data)) return;
                        event.stopImmediatePropagation();
                        const parts = event.data.split('|');
                        const amount = parseInt(parts[1], 10);
                        if (Number.isFinite(amount) && typeof window.SumLoader === 'function') {
                            try { window.SumLoader(amount, 800); } catch (_) {}
                        }
                    });
                }
                return socket;
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
