(function(){
    'use strict';
    if(window.__rdpWhatsHeaderV3) return;
    window.__rdpWhatsHeaderV3 = true;

    function enhanceHeader(){
        var header = document.querySelector('#app_WhatsApp .Whats_Title_Chatting');
        var photo = header && header.querySelector('.app_whats_photo');
        var avatar = photo && photo.querySelector('.app_contacts_avatar');
        var status = document.querySelector('#app_WhatsApp .app_whats_lastonline');

        /*
         * Important: do not rebuild the avatar URL here.
         * PhoneWebEvent already injects the player's actual Habbo figure/look.
         * Keeping that background preserves custom clothes/hair and the exact skin.
         */
        if(avatar){
            avatar.classList.add('rdp-real-habbo-avatar');
        }

        if(status){
            var txt = String(status.textContent || '').trim().toLowerCase();
            var online = txt === 'en ligne' || txt === 'online';
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
        setInterval(enhanceHeader,1200);
    }

    if(document.readyState === 'loading') document.addEventListener('DOMContentLoaded',boot);
    else boot();
})();
