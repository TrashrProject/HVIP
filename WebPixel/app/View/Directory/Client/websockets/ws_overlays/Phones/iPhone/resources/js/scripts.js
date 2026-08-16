(function(){
    'use strict';

    var leaveTimer = null;

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

    function showHomeScreen(show){
        var home = document.getElementById('screens_phone');
        if(!home) return;
        home.style.opacity = show ? '1' : '0';
        home.style.pointerEvents = show ? '' : 'none';
        home.style.transition = 'opacity .18s ease';
    }

    function openHealth(){
        var health = document.getElementById('app_Health');
        if(!health) return false;
        if(leaveTimer){ clearTimeout(leaveTimer); leaveTimer = null; }
        hidePhoneApps();
        showHomeScreen(false);
        health.classList.remove('rdp-health-leaving');
        health.style.display = 'block';
        health.setAttribute('aria-hidden','false');
        requestAnimationFrame(function(){
            requestAnimationFrame(function(){ health.classList.add('rdp-health-open'); });
        });
        if(typeof window.VeloraHealthRender === 'function') {
            try { window.VeloraHealthRender(); } catch(_) {}
        }
        return true;
    }

    function closeHealth(restoreHome){
        var health = document.getElementById('app_Health');
        if(!health) return;
        health.classList.remove('rdp-health-open');
        health.classList.add('rdp-health-leaving');
        health.setAttribute('aria-hidden','true');
        leaveTimer = window.setTimeout(function(){
            health.style.display = 'none';
            health.classList.remove('rdp-health-leaving');
            if(restoreHome !== false) showHomeScreen(true);
        }, 230);
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
            closeHealth(true);
            return;
        }

        var homeButton = target.closest('#phone_home');
        if(homeButton) {
            closeHealth(true);
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