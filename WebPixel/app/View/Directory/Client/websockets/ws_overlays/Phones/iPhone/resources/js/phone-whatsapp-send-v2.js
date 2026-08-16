(function () {
    'use strict';

    if (window.__rdpWhatsSendV5) return;
    window.__rdpWhatsSendV5 = true;

    var STORE_KEY = 'rdp_whatsapp_pending_v1';

    function getSocket() {
        if (window.__rdpPhoneSocket && window.__rdpPhoneSocket.readyState === 1) return window.__rdpPhoneSocket;
        try {
            if (window.rdp_app && rdp_app.webSocket && rdp_app.webSocket.readyState === 1) return rdp_app.webSocket;
        } catch (_) {}
        return null;
    }

    function getUserId() {
        if (window.__rdpPhoneUserId) return window.__rdpPhoneUserId;
        try { if (window.rdp_app && rdp_app.UserID) return rdp_app.UserID; } catch (_) {}
        return null;
    }

    function currentTarget() {
        var name = document.querySelector('#app_WhatsApp .app_whats_name');
        return name ? String(name.textContent || '').trim() : '';
    }

    function loadPending() {
        try { return JSON.parse(localStorage.getItem(STORE_KEY) || '{}') || {}; }
        catch (_) { return {}; }
    }

    function savePending(data) {
        try { localStorage.setItem(STORE_KEY, JSON.stringify(data)); } catch (_) {}
    }

    function normalizeText(value) {
        var div = document.createElement('div');
        div.innerHTML = String(value || '').replace(/::br::/g, '<br>');
        return String(div.textContent || '').replace(/\s+/g, ' ').trim();
    }

    function queuePending(target, text) {
        var all = loadPending();
        all[target] = all[target] || [];
        all[target].push({ id: Date.now() + '-' + Math.random().toString(36).slice(2), text: text, created: Date.now() });
        savePending(all);
    }

    function renderedOutgoingTexts() {
        return Array.prototype.map.call(document.querySelectorAll('#app_whats_messages .Msg_To_Whats:not(.rdp-persistent-pending)'), function (el) {
            return normalizeText(el.textContent || '');
        });
    }

    function reconcilePending(target) {
        if (!target) return [];
        var all = loadPending();
        var list = all[target] || [];
        var confirmed = renderedOutgoingTexts();

        list = list.filter(function (item) {
            var idx = confirmed.indexOf(normalizeText(item.text));
            if (idx !== -1) {
                confirmed.splice(idx, 1);
                return false;
            }
            return Date.now() - item.created < 120000;
        });

        if (list.length) all[target] = list; else delete all[target];
        savePending(all);
        return list;
    }

    function appendPendingBubble(item) {
        var box = document.getElementById('app_whats_messages');
        if (!box || box.querySelector('[data-rdp-pending-id="' + item.id + '"]')) return;

        var row = document.createElement('div');
        row.className = 'Msg_Container rdp-persistent-pending-row';
        row.setAttribute('data-rdp-pending-id', item.id);

        var bubble = document.createElement('div');
        bubble.className = 'Msg_To_Whats rdp-persistent-pending';
        bubble.setAttribute('data-rdp-time', new Date(item.created).toLocaleTimeString([], {hour:'2-digit', minute:'2-digit'}));
        bubble.textContent = item.text;
        row.appendChild(bubble);
        box.appendChild(row);
    }

    function restorePending() {
        var target = currentTarget();
        if (!target || target === 'Contact') return;
        reconcilePending(target).forEach(appendPendingBubble);
        var box = document.getElementById('app_whats_messages');
        if (box) box.scrollTop = box.scrollHeight;
    }

    function sendNow() {
        var input = document.getElementById('mensaje');
        var target = currentTarget();
        if (!input || !target || target === 'Contact') return false;

        var rawText = String(input.value || '').trim();
        if (!rawText) return false;

        var wireText = rawText.replace(/\|/g, '¦').replace(/\r?\n/g, '::br::');
        target = target.replace(/\|/g, '').trim();

        var socket = getSocket();
        var uid = getUserId();
        if (!socket || !uid) {
            console.warn('[RDP WhatsApp] Socket RP 2087 indisponible.');
            return false;
        }

        var payload = {
            UserId: parseInt(uid, 10) || uid,
            EventName: 'event_phone',
            Bypass: false,
            ExtraData: 'send_whatsapp,|' + wireText + '|' + target,
            JSON: false
        };

        try {
            queuePending(target, rawText);
            restorePending();
            socket.send(JSON.stringify(payload));
            input.value = '';
            console.info('[RDP WhatsApp] paquet envoyé', payload.ExtraData);
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

    var observer = new MutationObserver(function () { setTimeout(restorePending, 30); });
    function bootObserver() {
        var root = document.getElementById('WS_WhatsApp_Chatting') || document.getElementById('app_WhatsApp');
        if (root) observer.observe(root, { childList:true, subtree:true });
        restorePending();
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', bootObserver);
    else bootObserver();

    window.RdpWhatsAppSend = sendNow;
    window.RdpWhatsAppRestorePending = restorePending;
})();
