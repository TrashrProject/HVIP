(function(){
    'use strict';
    if(window.__rdpWhatsHeaderV9) return;
    window.__rdpWhatsHeaderV9 = true;

    function extractFigure(value){
        value = String(value || '');
        var m = value.match(/[?&]figure=([^&\"')]+)/i);
        if(!m) m = value.match(/figure%3D([^&\"')]+)/i);
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

    function avatarUrl(figure){
        return 'https://www.habbo.com/habbo-imaging/avatarimage?figure=' + encodeURIComponent(figure) + '&gesture=sml&direction=2&head_direction=2&headonly=1&size=m';
    }

    function forceAvatarBox(avatar){
        if(!avatar) return;
        avatar.style.setProperty('position', 'relative', 'important');
        avatar.style.setProperty('display', 'block', 'important');
        avatar.style.setProperty('flex', '0 0 44px', 'important');
        avatar.style.setProperty('width', '44px', 'important');
        avatar.style.setProperty('height', '44px', 'important');
        avatar.style.setProperty('min-width', '44px', 'important');
        avatar.style.setProperty('min-height', '44px', 'important');
        avatar.style.setProperty('max-width', '44px', 'important');
        avatar.style.setProperty('max-height', '44px', 'important');
        avatar.style.setProperty('margin', '0 9px 0 0', 'important');
        avatar.style.setProperty('padding', '0', 'important');
        avatar.style.setProperty('overflow', 'hidden', 'important');
        avatar.style.setProperty('border-radius', '50%', 'important');
        avatar.style.setProperty('background-color', '#20343a', 'important');
        avatar.style.setProperty('box-sizing', 'border-box', 'important');
        avatar.style.setProperty('transform', 'none', 'important');
    }

    function installAvatar(avatar, figure){
        if(!avatar || !figure) return;
        forceAvatarBox(avatar);
        avatar.setAttribute('data-figure', figure);
        avatar.classList.add('rdp-real-habbo-avatar');

        var existing = avatar.querySelector('.rdp-habbo-avatar-img');
        if(avatar.dataset.rdpRenderedFigure === figure && existing){
            forceAvatarImage(existing);
            return;
        }

        avatar.dataset.rdpRenderedFigure = figure;
        avatar.style.setProperty('background-image', 'none', 'important');
        avatar.innerHTML = '';

        var img = document.createElement('img');
        img.className = 'rdp-habbo-avatar-img';
        img.alt = '';
        img.draggable = false;
        forceAvatarImage(img);
        img.src = avatarUrl(figure);
        img.onerror = function(){
            img.removeAttribute('src');
            img.style.display = 'none';
            avatar.classList.add('rdp-avatar-failed');
        };
        img.onload = function(){
            avatar.classList.remove('rdp-avatar-failed');
            forceAvatarImage(img);
        };
        avatar.appendChild(img);
    }

    function forceAvatarImage(img){
        if(!img) return;
        img.style.setProperty('display', 'block', 'important');
        img.style.setProperty('position', 'absolute', 'important');
        img.style.setProperty('left', '50%', 'important');
        img.style.setProperty('top', '50%', 'important');
        img.style.setProperty('width', '64px', 'important');
        img.style.setProperty('height', '64px', 'important');
        img.style.setProperty('min-width', '64px', 'important');
        img.style.setProperty('min-height', '64px', 'important');
        img.style.setProperty('max-width', 'none', 'important');
        img.style.setProperty('max-height', 'none', 'important');
        img.style.setProperty('object-fit', 'contain', 'important');
        img.style.setProperty('transform', 'translate(-50%, -48%)', 'important');
        img.style.setProperty('margin', '0', 'important');
        img.style.setProperty('padding', '0', 'important');
    }

    function enhanceAvatar(avatar){
        if(!avatar) return;
        forceAvatarBox(avatar);
        var figure = getFigure(avatar);
        if(figure) installAvatar(avatar, figure);
    }

    function enhanceAllAvatars(){
        document.querySelectorAll('#phone .app_contacts_avatar').forEach(enhanceAvatar);
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

    function enhancePhone(){
        enhanceAllAvatars();
        enhanceStatus();
    }

    function boot(){
        enhancePhone();
        var root = document.getElementById('phone');
        if(root){
            var scheduled = false;
            new MutationObserver(function(){
                if(scheduled) return;
                scheduled = true;
                setTimeout(function(){ scheduled = false; enhancePhone(); }, 0);
            }).observe(root,{childList:true,subtree:true,attributes:true,attributeFilter:['style','class','data-figure']});
        }
        setInterval(enhancePhone,750);
    }

    if(document.readyState === 'loading') document.addEventListener('DOMContentLoaded',boot);
    else boot();
})();
