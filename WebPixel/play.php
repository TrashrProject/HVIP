<?php
/**
 * ParadiseRP / local Nitro client entry point.
 * The old RDP wrapper UI is disabled here at the parent page level.
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

ob_start();
require CLIENT . 'client.php';
$html = ob_get_clean();

// The old client page was originally a Flash/RDP shell. Keep the page boot,
// but remove the Flash bootstrap and force our local Nitro iframe.
$html = preg_replace(
    '/\s*swfobject\.embedSWF\([^;]*\);/is',
    "\n        // Legacy Flash bootstrap disabled for local Nitro.",
    $html,
    1
);

$nitroParams = array('sso' => $ClientAUTH);
if ($autoRoomId > 0) {
    $nitroParams = array('room' => $autoRoomId, 'sso' => $ClientAUTH);
}

$nitroSrc = '/nitro-last/index.html?' . http_build_query($nitroParams, '', '&', PHP_QUERY_RFC3986);
$nitroSrcHtml = htmlspecialchars($nitroSrc, ENT_QUOTES, 'UTF-8');
$forcedIframe = '<iframe id="RdpNitroFrame" src="' . $nitroSrcHtml . '" class="Nitro" allow="camera none; microphone *"></iframe>';

$iframeCount = 0;
$html = preg_replace(
    '/<iframe\b[^>]*class=["\'][^"\']*\bNitro\b[^"\']*["\'][^>]*>\s*<\/iframe>/is',
    $forcedIframe,
    $html,
    1,
    $iframeCount
);

if ($iframeCount === 0) {
    $html = preg_replace('/<body\b([^>]*)>/i', '<body$1>' . $forcedIframe, $html, 1);
}

$html = str_ireplace(
    array(
        'https://nitro.habbovip.us',
        'http://nitro.habbovip.us',
        'https://dev.habbovip.us',
        'http://dev.habbovip.us',
        'https://nitro-imager.kubbo.city/?figure=',
        'http://nitro-imager.kubbo.city/?figure=',
        'https://nitro-imager.kubbo.ch/?figure=',
        'http://nitro-imager.kubbo.ch/?figure=',
        'https://dynamics.habbovip.us/img/extras/platinos_icon_s.png',
        'http://dynamics.habbovip.us/img/extras/platinos_icon_s.png'
    ),
    array(
        '/nitro-last',
        '/nitro-last',
        '/nitro-last',
        '/nitro-last',
        '/avatar-image.php?figure=',
        '/avatar-image.php?figure=',
        '/avatar-image.php?figure=',
        '/avatar-image.php?figure=',
        'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIHWP4z8DwHwAFgAI/ScL8WQAAAABJRU5ErkJggg==',
        'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIHWP4z8DwHwAFgAI/ScL8WQAAAABJRU5ErkJggg=='
    ),
    $html
);

$localhostShim = <<<'HTML'
<style id="paradise-parent-ui-off-css">
html, body {
    width: 100% !important;
    height: 100% !important;
    margin: 0 !important;
    padding: 0 !important;
    overflow: hidden !important;
    background: #000 !important;
}
iframe.Nitro,
#RdpNitroFrame {
    position: fixed !important;
    inset: 0 !important;
    width: 100vw !important;
    height: 100vh !important;
    border: 0 !important;
    z-index: 1 !important;
    display: block !important;
    visibility: visible !important;
    opacity: 1 !important;
}
/* Parent RDP/HabboVIP legacy UI. This is outside the Nitro iframe, so it must be killed here, not inside nitro-last. */
#app,
#CombatMode,
#PSVMode,
#TicketMode,
#NavigatorMode,
#FriendsMode,
#SettingsMode,
#MessengerMode,
#InventoryMode,
#CatalogMode,
#RoomInfoMode,
#HotelViewMode,
#HelpMode,
.menuButton-yNbz6_0,
.button-3IzmP_0,
[class*="menuButton-yNbz6_0"],
[class*="button-3IzmP_0"],
[class*="leftMenu"],
[class*="legacy"],
[class*="RDP"] {
    display: none !important;
    visibility: hidden !important;
    opacity: 0 !important;
    pointer-events: none !important;
}
body > div:not(#root):not(#paradise-rp-hud):not(#paradise-loader):not(#RdpNitroFrame),
body > aside,
body > nav {
    pointer-events: none !important;
}
</style>
<script id="paradise-parent-ui-off-js">
(function () {
    'use strict';
    window.__PARADISE_PARENT_RDP_UI_OFF__ = '1.0.0';

    var selectors = [
        '#app',
        '#CombatMode', '#PSVMode', '#TicketMode', '#NavigatorMode', '#FriendsMode', '#SettingsMode',
        '#MessengerMode', '#InventoryMode', '#CatalogMode', '#RoomInfoMode', '#HotelViewMode', '#HelpMode',
        '.menuButton-yNbz6_0', '.button-3IzmP_0',
        '[class*="menuButton-yNbz6_0"]', '[class*="button-3IzmP_0"]',
        '[class*="leftMenu"]', '[class*="habbo-toolbar"]', '[class*="nitro-toolbar"]'
    ];

    function protect(el) {
        if (!el) return true;
        if (el.id === 'RdpNitroFrame') return true;
        if (el.id === 'paradise-loader') return true;
        if (el.id === 'paradise-rp-hud') return true;
        if (el.closest && (el.closest('#RdpNitroFrame') || el.closest('#paradise-loader') || el.closest('#paradise-rp-hud'))) return true;
        if (el.tagName === 'SCRIPT' || el.tagName === 'STYLE' || el.tagName === 'LINK' || el.tagName === 'META' || el.tagName === 'HEAD') return true;
        return false;
    }

    function hideElement(el) {
        if (protect(el)) return;
        try {
            el.setAttribute('data-paradise-parent-ui-killed', '1');
            el.style.setProperty('display', 'none', 'important');
            el.style.setProperty('visibility', 'hidden', 'important');
            el.style.setProperty('opacity', '0', 'important');
            el.style.setProperty('pointer-events', 'none', 'important');
            if (el.id === 'app' || /menuButton-yNbz6_0|button-3IzmP_0/i.test(String(el.className || ''))) {
                el.remove();
            }
        } catch (_) {}
    }

    function fixFrame() {
        var frame = document.getElementById('RdpNitroFrame') || document.querySelector('iframe.Nitro');
        if (!frame) return;
        frame.id = 'RdpNitroFrame';
        frame.classList.add('Nitro');
        frame.style.setProperty('position', 'fixed', 'important');
        frame.style.setProperty('inset', '0', 'important');
        frame.style.setProperty('width', '100vw', 'important');
        frame.style.setProperty('height', '100vh', 'important');
        frame.style.setProperty('border', '0', 'important');
        frame.style.setProperty('z-index', '1', 'important');
        frame.style.setProperty('display', 'block', 'important');
        frame.style.setProperty('visibility', 'visible', 'important');
        frame.style.setProperty('opacity', '1', 'important');
    }

    function killLegacy() {
        fixFrame();
        selectors.forEach(function (selector) {
            try { document.querySelectorAll(selector).forEach(hideElement); } catch (_) {}
        });
    }

    function boot() {
        killLegacy();
        try {
            new MutationObserver(killLegacy).observe(document.documentElement, {
                childList: true,
                subtree: true,
                attributes: true,
                attributeFilter: ['id', 'class', 'style']
            });
        } catch (_) {}
        [0, 40, 100, 220, 420, 800, 1400, 2400, 4200, 7000].forEach(function (ms) {
            window.setTimeout(killLegacy, ms);
        });
        window.setInterval(killLegacy, 650);
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
    else boot();
})();
</script>
<script>
window.swfobject = window.swfobject || { embedSWF: function () {} };
window.Swiper = window.Swiper || function(){ return { update:function(){}, slideTo:function(){}, destroy:function(){} }; };
window.addEventListener('unhandledrejection', function(event) {
    var reason = event && event.reason;
    var text = '';
    try { text = String(reason && (reason.message || reason) || ''); } catch (_) {}
    if ((reason && reason.constructor && reason.constructor.name === 'Event') || /Element not found/i.test(text)) event.preventDefault();
}, true);
(function () {
    if (!window.WebSocket || window.__rdpLocalWsShim) return;
    window.__rdpLocalWsShim = true;
    var NativeWebSocket = window.WebSocket;
    window.WebSocket = new Proxy(NativeWebSocket, {
        construct: function(Target, args) {
            if (typeof args[0] === 'string' && /^wss:\/\/(127\.0\.0\.1|localhost):2087\//i.test(args[0])) {
                args[0] = args[0].replace(/^wss:\/\/(127\.0\.0\.1|localhost):2087\//i, 'wss://paradiserp.fr/ws/');
            }
            return Reflect.construct(Target, args);
        }
    });
})();
document.addEventListener('DOMContentLoaded', function () {
    var frame = document.getElementById('RdpNitroFrame') || document.querySelector('iframe.Nitro');
    if (frame) console.info('[ParadiseRP] Nitro iframe:', frame.getAttribute('src'));
});
</script>
HTML;

$html = preg_replace('/<head\b([^>]*)>/i', '<head$1>' . $localhostShim, $html, 1);

echo $html;
