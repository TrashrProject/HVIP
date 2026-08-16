<?php
/**
 * PixelZone / RDP localhost client entry point.
 * Preserve the RP shell/phone exactly as-is and only force the local Nitro client.
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
$nitroSrc = '/WebPixel/nitro-last/index.html?' . http_build_query($nitroParams, '', '&', PHP_QUERY_RFC3986);
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
        '/WebPixel/nitro-last',
        '/WebPixel/nitro-last',
        '/WebPixel/nitro-last',
        '/WebPixel/nitro-last',
        '/WebPixel/avatar-image.php?figure=',
        '/WebPixel/avatar-image.php?figure=',
        '/WebPixel/avatar-image.php?figure=',
        '/WebPixel/avatar-image.php?figure=',
        'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIHWP4z8DwHwAFgAI/ScL8WQAAAABJRU5ErkJggg==',
        'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIHWP4z8DwHwAFgAI/ScL8WQAAAABJRU5ErkJggg=='
    ),
    $html
);

$localhostShim = <<<'HTML'
<link rel="stylesheet" href="/WebPixel/app/View/Directory/Client/websockets/ws_overlays/Phones/iPhone/resources/css/hvip-phone-modern.css?v=12">
<link rel="stylesheet" href="/WebPixel/app/View/Directory/Client/websockets/ws_overlays/Phones/iPhone/resources/css/phone-presence-toast.css?v=1">
<link rel="stylesheet" href="/WebPixel/app/View/Directory/Client/websockets/ws_overlays/Phones/iPhone/resources/css/phone-whatsapp-header-v2.css?v=4">
<script>
window.swfobject = window.swfobject || { embedSWF: function () {} };
window.Swiper = window.Swiper || function(){ return { update:function(){}, slideTo:function(){}, destroy:function(){} }; };

function rdpIgnoreOptionalPromise(event) {
    var reason = event && event.reason;
    var text = '';
    try { text = String(reason && (reason.message || reason) || ''); } catch (_) {}
    if ((reason && reason.constructor && reason.constructor.name === 'Event') || /Element not found/i.test(text)) {
        event.preventDefault();
        if (event.stopImmediatePropagation) event.stopImmediatePropagation();
        return false;
    }
}
window.addEventListener('unhandledrejection', rdpIgnoreOptionalPromise, true);
window.onunhandledrejection = function(event) {
    if (rdpIgnoreOptionalPromise(event) === false) return true;
};

(function () {
    var TRANSPARENT_PNG = 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIHWP4z8DwHwAFgAI/ScL8WQAAAABJRU5ErkJggg==';

    function cleanUrl(value) {
        value = String(value || '');
        if (!value) return value;

        if (/^https?:\/\/nitro-imager\.kubbo\.(?:city|ch)\//i.test(value)) {
            var q = value.indexOf('?');
            return '/WebPixel/avatar-image.php' + (q >= 0 ? value.substring(q) : '');
        }
        if (/^\/?\?figure=/i.test(value)) {
            return '/WebPixel/avatar-image.php' + value.replace(/^\/?/, '');
        }
        if (/platinos(?:%20|\s|_)+icon(?:%20|\s|_)+s\.png/i.test(value)) {
            return TRANSPARENT_PNG;
        }
        if (/(?:^|\/)styles\.[a-z0-9_-]+\.css(?:\?|$)/i.test(value)) {
            return '/WebPixel/nitro-last/empty-legacy.css';
        }
        return value;
    }

    function cleanStyle(value) {
        value = String(value || '');
        return value
            .replace(/https?:\/\/nitro-imager\.kubbo\.(?:city|ch)\/\?figure=/gi, '/WebPixel/avatar-image.php?figure=')
            .replace(/url\((["']?)\/?\?figure=/gi, 'url($1/WebPixel/avatar-image.php?figure=');
    }

    function patchProperty(proto, prop) {
        var desc = Object.getOwnPropertyDescriptor(proto, prop);
        if (!desc || !desc.set) return;
        Object.defineProperty(proto, prop, {
            get: desc.get,
            set: function(value) { return desc.set.call(this, cleanUrl(value)); },
            configurable: true,
            enumerable: desc.enumerable
        });
    }

    patchProperty(HTMLImageElement.prototype, 'src');
    patchProperty(HTMLSourceElement.prototype, 'src');
    patchProperty(HTMLLinkElement.prototype, 'href');

    var nativeSetAttribute = Element.prototype.setAttribute;
    Element.prototype.setAttribute = function(name, value) {
        var n = String(name).toLowerCase();
        if (n === 'src' || n === 'href') value = cleanUrl(value);
        if (n === 'style') value = cleanStyle(value);
        return nativeSetAttribute.call(this, name, value);
    };

    function repairNode(node) {
        if (!(node instanceof Element)) return;
        ['src', 'href'].forEach(function(attr) {
            if (!node.hasAttribute(attr)) return;
            var raw = node.getAttribute(attr);
            var next = cleanUrl(raw);
            if (next !== raw) nativeSetAttribute.call(node, attr, next);
        });
        if (node.hasAttribute('style')) {
            var rawStyle = node.getAttribute('style') || '';
            var nextStyle = cleanStyle(rawStyle);
            if (nextStyle !== rawStyle) nativeSetAttribute.call(node, 'style', nextStyle);
        }
    }

    new MutationObserver(function(mutations) {
        mutations.forEach(function(mutation) {
            if (mutation.type === 'attributes') repairNode(mutation.target);
            Array.prototype.forEach.call(mutation.addedNodes || [], function(node) {
                repairNode(node);
                if (node.querySelectorAll) node.querySelectorAll('[src],[href],[style]').forEach(repairNode);
            });
        });
    }).observe(document.documentElement, {
        childList: true,
        subtree: true,
        attributes: true,
        attributeFilter: ['src','href','style']
    });

    var nativeInsertAdjacentHTML = Element.prototype.insertAdjacentHTML;
    Element.prototype.insertAdjacentHTML = function(position, html) {
        if (typeof html === 'string') {
            html = html
                .replace(/https?:\/\/nitro-imager\.kubbo\.(?:city|ch)\/\?figure=/gi, '/WebPixel/avatar-image.php?figure=')
                .replace(/(["'])\/?\?figure=/gi, '$1/WebPixel/avatar-image.php?figure=')
                .replace(/(["'])[^"']*platinos(?:%20|\s|_)+icon(?:%20|\s|_)+s\.png/gi, '$1' + TRANSPARENT_PNG);
        }
        return nativeInsertAdjacentHTML.call(this, position, html);
    };
})();

console.info('[RDP] RP shell + local Nitro boot active');
</script>
<script src="/WebPixel/app/View/Directory/Client/websockets/ws_overlays/Phones/iPhone/resources/js/phone-presence-toast.js?v=1" defer></script>
<script src="/WebPixel/app/View/Directory/Client/websockets/ws_overlays/Phones/iPhone/resources/js/phone-whatsapp-send-v2.js?v=6" defer></script>
<script src="/WebPixel/app/View/Directory/Client/websockets/ws_overlays/Phones/iPhone/resources/js/phone-whatsapp-header-v2.js?v=9" defer></script>
<script src="/WebPixel/app/View/Directory/Client/websockets/ws_overlays/Phones/iPhone/resources/js/phone-whatsapp-state.js?v=2" defer></script>
<script src="/WebPixel/app/View/Directory/Client/websockets/ws_overlays/Phones/iPhone/resources/js/phone-whatsapp-global.js?v=3" defer></script>
<script>
(function () {
    if (!window.WebSocket || window.__rdpLocalWsShim) return;
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
                window.__rdpPhoneSocket = socket;
                try {
                    const u = new URL(args[0]);
                    window.__rdpPhoneUserId = (u.pathname || '').replace(/^\/+/, '').split('/')[0] || null;
                } catch (_) {}
                socket.addEventListener('open', function () {
                    window.__rdpPhoneSocket = socket;
                    console.info('[RDP] Phone WebSocket ready', window.__rdpPhoneUserId || '');
                });
                socket.addEventListener('message', function (event) {
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
})();

document.addEventListener('DOMContentLoaded', function () {
    const frame = document.getElementById('RdpNitroFrame') || document.querySelector('iframe.Nitro');
    if (frame) console.info('[RDP] Nitro iframe:', frame.getAttribute('src'));
});
</script>
<style>
html, body { width:100%!important; height:100%!important; margin:0!important; overflow:hidden!important; background:#000!important; }
iframe.Nitro, #RdpNitroFrame { position:fixed!important; inset:0!important; width:100vw!important; height:100vh!important; border:0!important; z-index:1!important; display:block!important; }
#app { position:relative!important; z-index:20!important; }
</style>
HTML;

$html = preg_replace('/<head\b([^>]*)>/i', '<head$1>' . $localhostShim, $html, 1);

echo $html;
