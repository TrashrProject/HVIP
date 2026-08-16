(function(){
    'use strict';

    function hidePhoneApps(){
        var selectors = [
            '#AppViewer', '#app_Settings', '#app_Contacts', '#app_Messages',
            '#app_Services', '#app_Phone', '#app_WhatsApp'
        ];
        selectors.forEach(function(selector){
            var node = document.querySelector(selector);
            if(node) node.style.display = 'none';
        });
    }

    function openHealth(){
        var health = document.getElementById('app_Health');
        if(!health) return false;
        hidePhoneApps();
        health.style.display = 'block';
        health.classList.add('rdp-health-open');
        if(typeof window.VeloraHealthRender === 'function') {
            try { window.VeloraHealthRender(); } catch(_) {}
        }
        return true;
    }

    function closeHealth(){
        var health = document.getElementById('app_Health');
        if(!health) return;
        health.style.display = 'none';
        health.classList.remove('rdp-health-open');
    }

    document.addEventListener('click', function(event){
        var target = event.target;
        if(!target || !target.closest) return;

        var healthIcon = target.closest('.appicon[data-app="_Health"], .rdp-app-tile[data-rdp-key="_Health"]');
        if(healthIcon && document.getElementById('phone_content') && document.getElementById('phone_content').contains(healthIcon)) {
            event.preventDefault();
            event.stopImmediatePropagation();
            openHealth();
            return;
        }

        var back = target.closest('#app_Health .rdp-health-back');
        if(back) {
            event.preventDefault();
            event.stopImmediatePropagation();
            closeHealth();
        }
    }, true);

    window.VeloraOpenHealth = openHealth;
    window.VeloraCloseHealth = closeHealth;
})();

$(document).ready(function(){
    $('#flip').click(function(){
        $('#panel').slideToggle('slow');
        $('#panel_content').slideToggle('slow');
    });
    $('#panel').click(function(){
        $('#panel').slideToggle('slow');
        $('#panel_content').slideToggle('slow');
    });
});