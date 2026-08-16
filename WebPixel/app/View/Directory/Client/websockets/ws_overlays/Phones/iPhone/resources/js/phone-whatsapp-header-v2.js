(function(){
    'use strict';
    if(window.__rdpWhatsHeaderV6) return;
    window.__rdpWhatsHeaderV6 = true;

    function extractFigure(value){
        value = String(value || '');
        var m = value.match(/[?&]figure=([^&"')]+)/i);
        if(!m) m = value.match(/figure%3D([^&"')]+)/i);
        if(!m) return '';
        try { return decodeURIComponent(m[1]); } catch(e) { return m[1]; }
    }

    function getFigure(avatar){
        if(!avatar) return '';
        var direct = avatar.getAttribute('data-figure') || '';
        if(direct) return direct;
        var inline = avatar.style.backgroundImage || '';
        var computed = '';
        try { computed = window.getComputedStyle(avatar).backgroundImage || ''; } catch(e) {}
        return extractFigure(inline) || extractFigure(computed) || '';
    }

    function officialAvatar(figure){
        return 'https://www.habbo.com/habbo-imaging/avatarimage?figure=' + encodeURIComponent(figure) + '&gesture=sml&direction=2&head_direction=2&headonly=1&size=m';
    }

    function installAvatar(avatar, figure){
        if(!avatar || !figure) return;
        avatar.setAttribute('data-figure', figure);
        avatar.classList.add('rdp-real-habbo-avatar');

        if(avatar.dataset.rdpRenderedFigure === figure && avatar.querySelector('.rdp-habbo-avatar-img')) return;
        avatar.dataset.rdpRenderedFigure = figure;

        avatar.style.setProperty('background-image', 'none', 'important');
        avatar.innerHTML = '';

        var img = document.createElement('img');
        img.className = 'rdp-habbo-avatar-img';
        img.alt = '';
        img.draggable = false;
        img.src = officialAvatar(figure);
        img.onerror = function(){
            img.style.display = 'none';
            avatar.classList.add('rdp-avatar-failed');
        };
        img.onload = function(){
            avatar.classList.remove('rdp-avatar-failed');
            img.style.display = 'block';
        };
        avatar.appendChild(img);
    }

    function enhanceAvatar(avatar){
        if(!avatar) return;
        var figure = getFigure(avatar);
        if(figure) installAvatar(avatar, figure);
    }

    function enhanceAllAvatars(){
        /* Header of the currently opened conversation. */
        var headerAvatar = document.querySelector('#app_WhatsApp .Whats_Title_Chatting .app_whats_photo .app_contacts_avatar');
        enhanceAvatar(headerAvatar);

        /* Discussions + Contacts tabs. These rows are replaced by websocket sync,
           so we re-apply the real Habbo image every time the list is refreshed. */
        document.querySelectorAll('#WS_WhatsApp .app_contacts_avatar, #WS_WhatsApp_Contacts .app_contacts_avatar').forEach(enhanceAvatar);
    }

    function enhanceStatus(){
        var header = document.querySelector('#app_WhatsApp .Whats_Title_Chatting');
        var status = document.querySelector('#app_WhatsApp .app_whats_lastonline');
        if(!status) return;

        var txt = String(status.textContent || '').replace(/\u00a0/g,' ').trim().toLowerCase();
        var online = txt === 'en ligne' || txt === 'online' || txt === 'en línea' || txt === 'en l&iacute;nea';
        status.classList.toggle('is-online', online);
        if(header) header.classList.toggle('rdp-online', online);
        if(online) status.textContent = 'En ligne';
    }

    function enhanceWhatsApp(){
        enhanceAllAvatars();
        enhanceStatus();
    }

    function boot(){
        enhanceWhatsApp();

        /* Observe the entire WhatsApp app, not only the open chat: open_whatsapp
           replaces the discussion/contact HTML after every server sync. */
        var root = document.getElementById('app_WhatsApp');
        if(root){
            var scheduled = false;
            new MutationObserver(function(){
                if(scheduled) return;
                scheduled = true;
                setTimeout(function(){
                    scheduled = false;
                    enhanceWhatsApp();
                }, 0);
            }).observe(root,{
                childList:true,
                subtree:true,
                attributes:true,
                attributeFilter:['style','class','data-figure']
            });
        }

        setInterval(enhanceWhatsApp,1000);
    }

    if(document.readyState === 'loading') document.addEventListener('DOMContentLoaded',boot);
    else boot();
})();
