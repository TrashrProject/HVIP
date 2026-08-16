(function () {
    'use strict';

    if (window.__rdpWhatsSendV4) return;
    window.__rdpWhatsSendV4 = true;

    function getSocket() {
        if (window.__rdpPhoneSocket && window.__rdpPhoneSocket.readyState === 1) {
            return window.__rdpPhoneSocket;
        }
        try {
            if (window.rdp_app && rdp_app.webSocket && rdp_app.webSocket.readyState === 1) {
                return rdp_app.webSocket;
            }
        } catch (_) {}
        return null;
    }

    function getUserId() {
        if (window.__rdpPhoneUserId) return window.__rdpPhoneUserId;
        try {
            if (window.rdp_app && rdp_app.UserID) return rdp_app.UserID;
        } catch (_) {}
        return null;
    }

    function sendNow() {
        var input = document.getElementById('mensaje');
        var name = document.querySelector('#app_WhatsApp .app_whats_name');
        if (!input || !name) return false;

        var text = String(input.value || '').trim();
        var target = String(name.textContent || '').trim();
        if (!text || !target || target === 'Contact') return false;

        text = text.replace(/\|/g, '¦').replace(/\r?\n/g, '::br::');
        target = target.replace(/\|/g, '').trim();

        var socket = getSocket();
        var uid = getUserId();
        if (!socket || !uid) {
            console.warn('[RDP WhatsApp] Socket RP 2087 indisponible.', {
                socket: !!socket,
                uid: uid,
                captured: !!window.__rdpPhoneSocket
            });
            return false;
        }

        var payload = {
            UserId: parseInt(uid, 10) || uid,
            EventName: 'event_phone',
            Bypass: false,
            ExtraData: 'send_whatsapp,|' + text + '|' + target,
            JSON: false
        };

        try {
            socket.send(JSON.stringify(payload));
            console.info('[RDP WhatsApp] paquet envoyé', payload.ExtraData);
            input.value = '';
            return true;
        } catch (e) {
            console.error('[RDP WhatsApp] échec envoi', e);
            return false;
        }
    }

    document.addEventListener('click', function (event) {
        var button = event.target.closest && event.target.closest('#app_WhatsApp .rdp-wa-send');
        if (!button) return;
        event.preventDefault();
        event.stopImmediatePropagation();
        sendNow();
    }, true);

    document.addEventListener('keydown', function (event) {
        if (event.key !== 'Enter' || event.shiftKey) return;
        var input = event.target && event.target.closest && event.target.closest('#app_WhatsApp #mensaje');
        if (!input) return;
        event.preventDefault();
        event.stopImmediatePropagation();
        sendNow();
    }, true);

    window.RdpWhatsAppSend = sendNow;
})();
