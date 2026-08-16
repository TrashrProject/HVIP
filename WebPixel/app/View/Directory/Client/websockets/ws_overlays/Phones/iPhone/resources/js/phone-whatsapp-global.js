(function(){
    'use strict';
    if(window.__rdpWhatsGlobalV1) return;
    window.__rdpWhatsGlobalV1 = true;

    var STORAGE_CHAT = 'rdp_whatsapp_last_chat';
    var refreshSent = false;

    function getSocket(){
        if(window.__rdpPhoneSocket && window.__rdpPhoneSocket.readyState === 1) return window.__rdpPhoneSocket;
        try {
            if(window.rdp_app && rdp_app.webSocket && rdp_app.webSocket.readyState === 1) return rdp_app.webSocket;
        } catch(_){}
        return null;
    }

    function getUserId(){
        if(window.__rdpPhoneUserId) return window.__rdpPhoneUserId;
        try { if(window.rdp_app && rdp_app.UserID) return rdp_app.UserID; } catch(_){}
        return null;
    }

    function sendPhone(extraData){
        var socket = getSocket();
        var uid = getUserId();
        if(!socket || !uid) return false;
        try {
            socket.send(JSON.stringify({
                UserId: parseInt(uid,10) || uid,
                EventName: 'event_phone',
                Bypass: false,
                ExtraData: extraData,
                JSON: false
            }));
            return true;
        } catch(e){
            console.warn('[RDP WhatsApp] impossible d\'envoyer', extraData, e);
            return false;
        }
    }

    function refreshWhatsApp(){
        if(sendPhone('in_app,WhatsApp')){
            refreshSent = true;
            console.info('[RDP WhatsApp] discussions synchronisées');
            return true;
        }
        return false;
    }

    function openChat(username){
        username = String(username || '').trim();
        if(!username) return;
        localStorage.setItem(STORAGE_CHAT, username);
        sendPhone('open_whatschats,' + username.replace(/[|,]/g,''));

        var title = document.querySelector('#app_WhatsApp .Whats_Title');
        var menu = document.getElementById('What_Menu');
        var list = document.getElementById('WS_WhatsApp');
        var contacts = document.getElementById('WS_WhatsApp_Contacts');
        var chatting = document.getElementById('WS_WhatsApp_Chatting');
        if(title) title.style.display = 'none';
        if(menu) menu.style.display = 'none';
        if(list) list.style.display = 'none';
        if(contacts) contacts.style.display = 'none';
        if(chatting) chatting.style.display = '';
    }

    function removeComposeRow(){
        var old = document.getElementById('RdpWhatsGlobalCompose');
        if(old && old.parentNode) old.parentNode.removeChild(old);
    }

    function renderComposeRow(query){
        removeComposeRow();
        query = String(query || '').trim();
        if(!query) return;

        var host = document.getElementById('WS_WhatsApp');
        if(!host) return;

        var row = document.createElement('div');
        row.id = 'RdpWhatsGlobalCompose';
        row.className = 'app_msg_content rdp-whats-global-compose';
        row.setAttribute('data-whatsname', query);
        row.innerHTML = '<div class="rdp-global-compose-icon">+</div>' +
                        '<div class="app_whats_chat_info">' +
                        '<div class="app_msg_dest"></div>' +
                        '<div class="app_msg_lastmsg">Démarrer une nouvelle conversation</div>' +
                        '</div>';
        row.querySelector('.app_msg_dest').textContent = 'Écrire à ' + query;
        row.addEventListener('click', function(e){
            e.preventDefault();
            e.stopPropagation();
            openChat(query);
        });
        host.insertBefore(row, host.firstChild);
    }

    function bindSearch(){
        var input = document.getElementById('WhatsUniversalSearch');
        if(!input || input.dataset.rdpGlobal === '1') return;
        input.dataset.rdpGlobal = '1';
        input.setAttribute('placeholder', 'Rechercher ou écrire à un pseudo');

        input.addEventListener('input', function(){
            renderComposeRow(input.value);
        });
        input.addEventListener('keydown', function(e){
            if(e.key !== 'Enter') return;
            var q = String(input.value || '').trim();
            if(!q) return;
            e.preventDefault();
            e.stopImmediatePropagation();
            openChat(q);
        }, true);
    }

    function bindRows(){
        document.addEventListener('click', function(e){
            var row = e.target.closest && e.target.closest('#app_WhatsApp .app_msg_content[data-whatsname]');
            if(!row || row.id === 'RdpWhatsGlobalCompose') return;
            var name = String(row.getAttribute('data-whatsname') || '').trim();
            if(name) localStorage.setItem(STORAGE_CHAT, name);
        }, true);
    }

    function boot(){
        bindRows();
        var tries = 0;
        var timer = setInterval(function(){
            bindSearch();
            var app = document.getElementById('app_WhatsApp');
            if(app && getSocket() && !refreshSent) refreshWhatsApp();
            if(++tries > 120) clearInterval(timer);
        }, 250);

        window.addEventListener('focus', function(){
            if(document.getElementById('app_WhatsApp')){
                refreshSent = false;
                refreshWhatsApp();
            }
        });
    }

    if(document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
    else boot();
})();
