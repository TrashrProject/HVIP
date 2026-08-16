(function () {
    'use strict';

    if (window.__rdpWhatsAppSendFix) return;
    window.__rdpWhatsAppSendFix = true;

    function ready() {
        return !!(window.rdp && window.rdp_app && rdp_app.webSocket && rdp_app.startedSocket);
    }

    function cleanMessage(value) {
        return String(value || '')
            .replace(/\|/g, '¦')
            .replace(/\r?\n/g, '::br::')
            .trim();
    }

    function targetName() {
        var el = document.querySelector('#app_WhatsApp .app_whats_name');
        return el ? String(el.textContent || el.innerText || '').trim() : '';
    }

    function appendPending(message) {
        var box = document.getElementById('app_whats_messages');
        if (!box) return;

        var row = document.createElement('div');
        row.className = 'Msg_Container rdp-pending-row';

        var bubble = document.createElement('div');
        bubble.className = 'Msg_To_Whats rdp-pending';
        bubble.textContent = message.replace(/::br::/g, '\n');

        var now = new Date();
        bubble.setAttribute('data-rdp-time', String(now.getHours()).padStart(2, '0') + ':' + String(now.getMinutes()).padStart(2, '0'));

        row.appendChild(bubble);
        box.appendChild(row);
        box.scrollTop = box.scrollHeight;
    }

    function sendCurrentMessage() {
        var input = document.getElementById('mensaje');
        if (!input) return false;

        var raw = String(input.value || '').trim();
        var to = targetName();
        if (!raw || !to || to === 'Contact') return false;
        if (!ready()) {
            console.warn('[RDP WhatsApp] websocket non prêt');
            return false;
        }

        var message = cleanMessage(raw);
        var payload = 'send_whatsapp,|' + message + '|' + to;

        try {
            rdp.sendData('event_phone', payload, false, false, rdp_app.webSocket, rdp_app.startedSocket, rdp_app.UserID);
            input.value = '';
            var legacy = document.querySelector('#WS_WhatsApp_Chatting .rdp-legacy-textto');
            if (legacy) legacy.innerHTML = '';
            appendPending(message);
            return true;
        } catch (error) {
            console.error('[RDP WhatsApp] erreur envoi', error);
            return false;
        }
    }

    document.addEventListener('click', function (event) {
        var button = event.target.closest && event.target.closest('#WS_WhatsApp_Chatting .rdp-wa-send, #WS_WhatsApp_Chatting #TextTo');
        if (!button) return;
        event.preventDefault();
        event.stopImmediatePropagation();
        sendCurrentMessage();
    }, true);

    document.addEventListener('keydown', function (event) {
        var input = event.target;
        if (!input || input.id !== 'mensaje') return;
        if (event.key !== 'Enter' || event.shiftKey) return;
        event.preventDefault();
        event.stopImmediatePropagation();
        sendCurrentMessage();
    }, true);
})();
