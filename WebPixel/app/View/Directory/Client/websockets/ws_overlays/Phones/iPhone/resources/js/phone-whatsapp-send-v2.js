(function () {
    'use strict';

    if (window.__rdpWhatsSendV2) return;
    window.__rdpWhatsSendV2 = true;

    function ready() {
        return !!(window.rdp && window.rdp_app && rdp_app.webSocket && rdp_app.startedSocket);
    }

    function sendNow() {
        var input = document.getElementById('mensaje');
        var name = document.querySelector('#app_WhatsApp .app_whats_name');
        if (!input || !name) return false;

        var text = String(input.value || '').trim();
        var target = String(name.textContent || '').trim();
        if (!text || !target || target === 'Contact') return false;
        if (!ready()) {
            console.warn('[RDP WhatsApp] WebSocket indisponible');
            return false;
        }

        text = text.replace(/\|/g, '¦').replace(/\r?\n/g, '::br::');
        target = target.replace(/\|/g, '').trim();

        // Exact format expected by PhoneWebEvent.cs:
        // Data = send_whatsapp,|MESSAGE|USERNAME
        var data = 'send_whatsapp,|' + text + '|' + target;

        try {
            rdp['sendData'](
                'event_phone',
                data,
                false,
                false,
                rdp_app['webSocket'],
                rdp_app['startedSocket'],
                rdp_app['UserID']
            );
            console.info('[RDP WhatsApp] message envoyé à', target);
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
