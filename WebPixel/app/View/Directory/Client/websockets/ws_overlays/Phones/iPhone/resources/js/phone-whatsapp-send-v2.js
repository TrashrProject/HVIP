(function () {
    'use strict';

    console.info('[RDP WhatsApp] direct sender v3 loaded');

    function socketReady() {
        return !!(window.rdp_app && rdp_app.webSocket && rdp_app.startedSocket && rdp_app.webSocket.readyState === 1);
    }

    function getComposer() {
        return {
            input: document.querySelector('#app_WhatsApp #mensaje'),
            target: document.querySelector('#app_WhatsApp .app_whats_name'),
            button: document.querySelector('#app_WhatsApp .rdp-wa-send')
        };
    }

    function showError(message) {
        console.warn('[RDP WhatsApp]', message);
        var box = document.getElementById('phone_error');
        var text = document.getElementById('phone_error_msg');
        if (text) text.textContent = message;
        if (box) box.style.display = 'block';
    }

    function sendWhatsApp() {
        var c = getComposer();
        if (!c.input || !c.target) return false;

        var text = String(c.input.value || '').trim();
        var target = String(c.target.textContent || '').trim();

        if (!text) return false;
        if (!target || target === 'Contact') {
            showError('Destinataire WhatsApp introuvable.');
            return false;
        }
        if (!socketReady()) {
            showError('Connexion du téléphone indisponible.');
            return false;
        }

        text = text.replace(/\|/g, '¦').replace(/\r?\n/g, '::br::');
        target = target.replace(/\|/g, '').trim();

        var extraData = 'send_whatsapp,|' + text + '|' + target;
        var packet = {
            UserId: rdp_app.UserID,
            EventName: 'event_phone',
            Bypass: false,
            ExtraData: extraData,
            JSON: false
        };

        try {
            rdp_app.webSocket.send(JSON.stringify(packet));
            console.info('[RDP WhatsApp] SEND', extraData);
            c.input.value = '';
            c.input.focus();
            return true;
        } catch (e) {
            console.error('[RDP WhatsApp] send failed', e);
            showError('Impossible d’envoyer le message.');
            return false;
        }
    }

    function bindButton() {
        var c = getComposer();
        if (!c.button) return false;
        if (c.button.dataset.rdpDirectSendV3 === '1') return true;

        c.button.dataset.rdpDirectSendV3 = '1';
        c.button.onclick = function (e) {
            if (e) {
                e.preventDefault();
                e.stopPropagation();
            }
            sendWhatsApp();
            return false;
        };

        c.button.addEventListener('pointerup', function (e) {
            e.preventDefault();
            e.stopImmediatePropagation();
            sendWhatsApp();
        }, true);

        return true;
    }

    function bindInput() {
        var c = getComposer();
        if (!c.input) return false;
        if (c.input.dataset.rdpDirectSendV3 === '1') return true;
        c.input.dataset.rdpDirectSendV3 = '1';
        c.input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                e.stopImmediatePropagation();
                sendWhatsApp();
            }
        }, true);
        return true;
    }

    function bindAll() {
        bindButton();
        bindInput();
    }

    document.addEventListener('DOMContentLoaded', bindAll);
    document.addEventListener('click', function (e) {
        var button = e.target && e.target.closest ? e.target.closest('#app_WhatsApp .rdp-wa-send') : null;
        if (!button) return;
        e.preventDefault();
        e.stopImmediatePropagation();
        sendWhatsApp();
    }, true);

    var tries = 0;
    var timer = setInterval(function () {
        bindAll();
        tries++;
        if (tries > 240) clearInterval(timer);
    }, 250);

    window.RdpWhatsAppSend = sendWhatsApp;
})();
