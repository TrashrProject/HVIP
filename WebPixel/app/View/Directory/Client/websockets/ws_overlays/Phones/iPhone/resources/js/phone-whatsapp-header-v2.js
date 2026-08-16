(function(){
    'use strict';
    if(window.__rdpWhatsHeaderV2) return;
    window.__rdpWhatsHeaderV2 = true;

    function extractFigureFromBackground(value){
        value = String(value || '');
        var match = value.match(/[?&]figure=([^&"')]+)/i);
        if(!match) match = value.match(/figure%3D([^&"')]+)/i);
        if(!match) return '';
        try { return decodeURIComponent(match[1]); } catch(e) { return match[1]; }
    }

    function buildAvatarUrl(figure){
        return 'https://www.habbo.com/habbo-imaging/avatarimage?figure=' + encodeURIComponent(figure) + '&headonly=1&size=m&direction=2&head_direction=2&gesture=sml';
    }

    function enhanceHeader(){
        var photo = document.querySelector('#app_WhatsApp .Whats_Title_Chatting .app_whats_photo');
        var avatar = photo && photo.querySelector('.app_contacts_avatar');
        var status = document.querySelector('#app_WhatsApp .app_whats_lastonline');

        if(avatar){
            var inlineBg = avatar.style.backgroundImage || '';
            var computedBg = '';
            try { computedBg = window.getComputedStyle(avatar).backgroundImage || ''; } catch(e) {}
            var figure = extractFigureFromBackground(inlineBg) || extractFigureFromBackground(computedBg);
            if(figure && avatar.dataset.rdpFigure !== figure){
                avatar.dataset.rdpFigure = figure;
                avatar.style.setProperty('background-image','url("' + buildAvatarUrl(figure) + '")','important');
            }
        }

        if(status){
            var txt = String(status.textContent || '').trim().toLowerCase();
            var online = txt === 'en ligne' || txt === 'online';
            status.classList.toggle('is-online', online);
            if(online && status.textContent.trim().toLowerCase() !== 'en ligne') status.textContent = 'en ligne';
        }
    }

    function boot(){
        enhanceHeader();
        var root = document.getElementById('WS_WhatsApp_Chatting') || document.getElementById('app_WhatsApp');
        if(root){
            new MutationObserver(function(){ setTimeout(enhanceHeader,0); }).observe(root,{childList:true,subtree:true,attributes:true,attributeFilter:['style','class']});
        }
        setInterval(enhanceHeader,1200);
    }

    if(document.readyState === 'loading') document.addEventListener('DOMContentLoaded',boot);
    else boot();
})();
