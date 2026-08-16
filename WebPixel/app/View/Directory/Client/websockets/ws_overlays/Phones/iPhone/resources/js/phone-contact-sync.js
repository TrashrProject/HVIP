(function () {
    'use strict';

    if (window.__rdpPhoneContactSync) return;
    window.__rdpPhoneContactSync = true;

    function phoneReady() {
        return !!(window.rdp && window.rdp_app && rdp_app.webSocket && rdp_app.startedSocket);
    }

    function sendPhone(action) {
        if (!phoneReady()) return false;
        try {
            rdp.sendData('event_phone', action, false, false, rdp_app.webSocket, rdp_app.startedSocket, rdp_app.UserID);
            return true;
        } catch (_) {
            return false;
        }
    }

    function clean(value) {
        return String(value || '').replace(/\s+/g, ' ').trim();
    }

    function contactName(node) {
        if (!node) return '';
        var name = node.querySelector('.app_contacts_name, .app_msg_dest');
        return clean(name && name.textContent);
    }

    function avatarHtml(node) {
        if (!node) return '';
        var avatar = node.querySelector('.app_contacts_avatar, .app_whats_profile_photo');
        return avatar ? avatar.outerHTML : '';
    }

    function existingWhatsNames() {
        var map = Object.create(null);
        document.querySelectorAll('#WS_WhatsApp_Contacts .app_msg_content').forEach(function (row) {
            var n = contactName(row);
            if (n) map[n.toLowerCase()] = row;
        });
        return map;
    }

    function openConversation(username, avatar) {
        if (!username) return;

        var chats = document.getElementById('WS_WhatsApp');
        var contacts = document.getElementById('WS_WhatsApp_Contacts');
        var chat = document.getElementById('WS_WhatsApp_Chatting');
        var title = document.querySelector('#app_WhatsApp .Whats_Title');
        var menu = document.getElementById('What_Menu');
        var profile = document.querySelector('#app_WhatsApp .app_whats_photo');
        var name = document.querySelector('#app_WhatsApp .app_whats_name');
        var state = document.querySelector('#app_WhatsApp .app_whats_lastonline');

        if (chats) chats.style.display = 'none';
        if (contacts) contacts.style.display = 'none';
        if (chat) chat.style.display = '';
        if (title) title.style.display = 'none';
        if (menu) menu.style.display = 'none';
        if (profile && avatar) profile.innerHTML = avatar;
        if (name) name.textContent = username;
        if (state) state.textContent = 'hors ligne';

        sendPhone('open_whatschats,|' + username);
    }

    function buildOfflineRow(source) {
        var username = contactName(source);
        if (!username) return null;

        var row = document.createElement('div');
        row.className = 'app_msg_content rdp-offline-contact';
        row.setAttribute('data-rdp-contact', username);

        var avatar = avatarHtml(source);
        row.innerHTML =
            '<div class="app_msg_data">' +
                '<div class="app_whats_profile_photo">' + avatar + '</div>' +
                '<div class="app_whats_chat_info">' +
                    '<div class="app_msg_dest"></div>' +
                    '<div class="app_msg_lastmsg rdp-contact-status">Hors ligne</div>' +
                '</div>' +
            '</div>';

        var dest = row.querySelector('.app_msg_dest');
        if (dest) dest.textContent = username;

        row.addEventListener('click', function () {
            openConversation(username, avatar);
        });

        return row;
    }

    function markOnlineRows() {
        document.querySelectorAll('#WS_WhatsApp_Contacts .app_msg_content:not(.rdp-offline-contact)').forEach(function (row) {
            var last = row.querySelector('.app_msg_lastmsg');
            if (last && !clean(last.textContent)) last.textContent = 'En ligne';
            row.classList.add('rdp-online-contact');
        });
    }

    function syncContactsIntoWhatsApp() {
        var source = document.getElementById('WS_Contacts');
        var target = document.getElementById('WS_WhatsApp_Contacts');
        if (!source || !target) return;

        var existing = existingWhatsNames();
        var sourceRows = source.querySelectorAll('.app_contacts_content');
        if (!sourceRows.length) return;

        markOnlineRows();

        sourceRows.forEach(function (sourceRow) {
            var username = contactName(sourceRow);
            if (!username) return;
            var key = username.toLowerCase();
            if (existing[key]) return;

            var row = buildOfflineRow(sourceRow);
            if (!row) return;
            target.appendChild(row);
            existing[key] = row;
        });

        target.querySelectorAll('.rdp-offline-contact').forEach(function (row) {
            var n = contactName(row).toLowerCase();
            var onlineDuplicate = Array.prototype.some.call(
                target.querySelectorAll('.app_msg_content:not(.rdp-offline-contact)'),
                function (candidate) { return contactName(candidate).toLowerCase() === n; }
            );
            if (onlineDuplicate) row.remove();
        });
    }

    function requestFullSync() {
        sendPhone('open_contacts,');
        sendPhone('open_whatsapp,');
        setTimeout(syncContactsIntoWhatsApp, 180);
        setTimeout(syncContactsIntoWhatsApp, 500);
    }

    function patchIncoming() {
        if (!window.rdp || typeof rdp.IncomingPacket !== 'function') return false;
        if (rdp.IncomingPacket.__rdpContactSyncPatched) return true;

        var original = rdp.IncomingPacket;
        var wrapped = function (RDPEvent, ExtraData) {
            var result = original.apply(this, arguments);
            if (RDPEvent === 'compose_phone') {
                var action = clean(ExtraData).toLowerCase();
                if (action === 'open_contacts' || action === 'open_whatsapp' || action === 'contact_online') {
                    setTimeout(syncContactsIntoWhatsApp, 0);
                    setTimeout(syncContactsIntoWhatsApp, 120);
                }
            }
            return result;
        };
        wrapped.__rdpContactSyncPatched = true;
        rdp.IncomingPacket = wrapped;
        return true;
    }

    function bindUI() {
        var contactsTab = document.getElementById('WhatsContacts');
        if (contactsTab && !contactsTab.dataset.rdpOfflineSync) {
            contactsTab.dataset.rdpOfflineSync = '1';
            contactsTab.addEventListener('click', function () {
                requestFullSync();
            }, true);
        }

        document.addEventListener('click', function (event) {
            var icon = event.target.closest && event.target.closest('#phone .appicon[data-app]');
            if (!icon) return;
            var app = icon.getAttribute('data-app');
            if (app === '_WhatsApp' || app === 'WhatsApp' || app === '_Contacts' || app === 'Contacts') {
                setTimeout(requestFullSync, 100);
            }
        }, true);
    }

    function boot() {
        bindUI();
        var tries = 0;
        var timer = setInterval(function () {
            patchIncoming();
            if (phoneReady()) {
                requestFullSync();
                if (patchIncoming()) clearInterval(timer);
            }
            if (++tries > 60) clearInterval(timer);
        }, 250);

        var source = document.getElementById('WS_Contacts');
        var target = document.getElementById('WS_WhatsApp_Contacts');
        if (window.MutationObserver) {
            if (source) new MutationObserver(syncContactsIntoWhatsApp).observe(source, { childList: true, subtree: true });
            if (target) new MutationObserver(function () { setTimeout(syncContactsIntoWhatsApp, 0); }).observe(target, { childList: true });
        }
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
    else boot();
})();
