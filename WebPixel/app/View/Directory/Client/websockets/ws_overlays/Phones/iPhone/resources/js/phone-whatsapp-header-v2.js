(function(){
    'use strict';
    if(window.__rdpWhatsHeaderV4) return;
    window.__rdpWhatsHeaderV4 = true;

    function extractFigure(value){
        value = String(value || '');
        var m = value.match(/[?&]figure=([^&"')]+)/i);
        if(!m) m = value.match(/figure%3D([^&"')]+)/i);
        if(!m) return '';
        try { return decodeURIComponent(m[1]); } catch(e) { return m[1]; }
    }

    function getFigure(avatar){
        if(!avatar) return '';
        var inline = avatar.style.backgroundImage || '';
        var computed = '';
        try { computed = window.getComputedStyle(avatar).backgroundImage || ''; } catch(e) {}
        return extractFigure(inline) || extractFigure(computed) || avatar.getAttribute('data-figure') || '';
    }

    function buildAvatar(figure){
        return 'https://nitro-imager.kubbo.ch/?figure=' + encodeURIComponent(figure) + '&gesture=sml&direction=2&head_direction=2&headonly=1&size=m';
    }

    function enhanceHeader(){
        var header = document.querySelector('#app_WhatsApp .Whats_Title_Chatting');
        var photo = header && header.querySelector('.app_whats_photo');
        var avatar = photo && photo.querySelector('.app_contacts_avatar');
        var status = document.querySelector('#app_WhatsApp .app_whats_lastonline');

        if(avatar){
            avatar.classList.add('rdp-real-habbo-avatar');
            var figure = getFigure(avatar);
            if(figure){
                avatar.setAttribute('data-figure', figure);
                if(avatar.dataset.rdpRenderedFigure !== figure){
                    avatar.dataset.rdpRenderedFigure = figure;
                    avatar.style.setProperty('background-image', 'url("' + buildAvatar(figure) + '")', 'important');
                }
            }
        }

        if(status){
            var txt = String(status.textContent || '').replace(/\u00a0/g,' ').trim().toLowerCase();
            var online = txt === 'en ligne' || txt === 'online' || txt === 'en línea' || txt === 'en l&iacute;nea';
            status.classList.toggle('is-online', online);
            if(header) header.classList.toggle('rdp-online', online);
            if(online) status.textContent = 'En ligne';
        }
    }

    function boot(){
        enhanceHeader();
        var root = document.getElementById('WS_WhatsApp_Chatting') || document.getElementById('app_WhatsApp');
        if(root){
            new MutationObserver(function(){ setTimeout(enhanceHeader,0); }).observe(root,{
                childList:true,
                subtree:true,
                attributes:true,
                attributeFilter:['style','class']
            });
        }
        setInterval(enhanceHeader,800);
    }

    if(document.readyState === 'loading') document.addEventListener('DOMContentLoaded',boot);
    else boot();
})();
