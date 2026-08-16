(function(){
    'use strict';
    if(window.__rdpWhatsHeaderV5) return;
    window.__rdpWhatsHeaderV5 = true;

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

        if(avatar.dataset.rdpRenderedFigure === figure) return;
        avatar.dataset.rdpRenderedFigure = figure;

        /*
         * The old retro imager (nitro-imager.kubbo.city/.ch) no longer resolves.
         * Use a real <img> so DNS/image failures cannot leave a CSS-only empty box.
         * Keep the full figure string untouched; custom parts that Habbo's public
         * imager does not know are simply ignored instead of hiding the avatar.
         */
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

    function enhanceHeader(){
        var header = document.querySelector('#app_WhatsApp .Whats_Title_Chatting');
        var photo = header && header.querySelector('.app_whats_photo');
        var avatar = photo && photo.querySelector('.app_contacts_avatar');
        var status = document.querySelector('#app_WhatsApp .app_whats_lastonline');

        if(avatar){
            var figure = getFigure(avatar);
            if(figure) installAvatar(avatar, figure);
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
                attributeFilter:['style','class','data-figure']
            });
        }
        setInterval(enhanceHeader,800);
    }

    if(document.readyState === 'loading') document.addEventListener('DOMContentLoaded',boot);
    else boot();
})();
