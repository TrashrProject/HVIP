(function(){
    'use strict';

    var leaveTimer = null;
    var guardTimer = null;
    var healthPinned = false;

    function injectHealthFixes(){
        if(document.getElementById('velora-health-v3-layout-fix')) return;
        var style = document.createElement('style');
        style.id = 'velora-health-v3-layout-fix';
        style.textContent = [
            '#phone #app_Health.velora-health-v3{top:31px!important;bottom:auto!important;height:559px!important;max-height:559px!important;border-radius:0 0 25px 25px!important;}',
            '#phone #app_Health .vh3-shell{height:559px!important;padding:12px 13px 14px!important;gap:9px!important;}',
            '#phone #app_Health .vh3-topbar{min-height:37px!important;height:37px!important;flex:0 0 37px!important;align-items:center!important;}',
            '#phone #app_Health .vh3-back{width:32px!important;height:32px!important;flex:0 0 32px!important;}',
            '#phone #app_Health .vh3-brand{justify-content:center!important;transform:none!important;}',
            '#phone #app_Health .vh3-brand small{font-size:7px!important;line-height:8px!important;margin:0!important;}',
            '#phone #app_Health .vh3-brand strong{font-size:18px!important;line-height:20px!important;margin-top:2px!important;}',
            '#phone #app_Health .vh3-live{font-size:7px!important;padding:5px 8px!important;line-height:10px!important;}',
            '#phone #app_Health .vh3-overview{min-height:82px!important;padding:12px!important;grid-template-columns:46px minmax(0,1fr) 54px!important;gap:9px!important;flex:0 0 82px!important;}',
            '#phone #app_Health .vh3-overview-icon{width:46px!important;height:46px!important;padding:9px!important;}',
            '#phone #app_Health .vh3-overview-copy span{font-size:7px!important;line-height:9px!important;}',
            '#phone #app_Health .vh3-overview-copy strong{font-size:17px!important;line-height:19px!important;margin-top:1px!important;}',
            '#phone #app_Health .vh3-overview-copy small{display:block!important;font-size:8px!important;line-height:10px!important;margin-top:2px!important;max-height:20px!important;overflow:hidden!important;}',
            '#phone #app_Health .vh3-score{width:54px!important;justify-content:flex-end!important;align-items:baseline!important;}',
            '#phone #app_Health .vh3-score strong{font-size:31px!important;line-height:31px!important;}',
            '#phone #app_Health .vh3-score span{font-size:8px!important;line-height:10px!important;margin-left:1px!important;}',
            '#phone #app_Health .vh3-vitals{min-height:176px!important;gap:8px!important;flex:1 1 176px!important;}',
            '#phone #app_Health .vh3-card{min-height:80px!important;padding:10px!important;overflow:hidden!important;}',
            '#phone #app_Health .vh3-card-head{position:relative!important;display:grid!important;grid-template-columns:34px minmax(0,1fr)!important;gap:8px!important;align-items:center!important;}',
            '#phone #app_Health .vh3-icon{width:34px!important;height:34px!important;padding:8px!important;}',
            '#phone #app_Health .vh3-card-head>div{min-width:0!important;padding-right:28px!important;}',
            '#phone #app_Health .vh3-card-head small{display:block!important;font-size:7px!important;line-height:9px!important;letter-spacing:.04em!important;white-space:nowrap!important;overflow:hidden!important;text-overflow:ellipsis!important;}',
            '#phone #app_Health .vh3-card-head strong{display:block!important;font-size:12px!important;line-height:15px!important;margin-top:2px!important;white-space:nowrap!important;overflow:hidden!important;text-overflow:ellipsis!important;}',
            '#phone #app_Health .vh3-card-head>b{position:absolute!important;right:0!important;top:50%!important;transform:translateY(-50%)!important;margin:0!important;font-size:8px!important;line-height:10px!important;white-space:nowrap!important;}',
            '#phone #app_Health .vh3-card.sync .vh3-card-head>div{padding-right:8px!important;}',
            '#phone #app_Health .vh3-card.sync .vh3-card-head>b{right:0!important;top:4px!important;transform:none!important;font-size:9px!important;}',
            '#phone #app_Health .vh3-progress{height:5px!important;margin-top:8px!important;}',
            '#phone #app_Health .vh3-advice{min-height:57px!important;padding:9px 10px!important;gap:9px!important;flex:0 0 57px!important;}',
            '#phone #app_Health .vh3-advice>span{width:31px!important;height:31px!important;padding:7px!important;}',
            '#phone #app_Health .vh3-advice strong{font-size:10px!important;line-height:12px!important;}',
            '#phone #app_Health .vh3-advice small{font-size:8px!important;line-height:10px!important;margin-top:2px!important;}',
            '#phone #app_Health .vh3-emergency{min-height:151px!important;padding:11px 12px!important;flex:0 0 151px!important;overflow:hidden!important;}',
            '#phone #app_Health .vh3-ambulance{width:36px!important;height:36px!important;padding:7px!important;}',
            '#phone #app_Health .vh3-emergency-head small{font-size:7px!important;line-height:9px!important;}',
            '#phone #app_Health .vh3-emergency-head strong{font-size:13px!important;line-height:16px!important;}',
            '#phone #app_Health .vh3-emergency p{font-size:8px!important;line-height:11px!important;margin:7px 0 8px!important;}',
            '#phone #app_Health .vh3-call{height:46px!important;border-radius:12px!important;}',
            '#phone #app_Health .vh3-call small{font-size:6px!important;line-height:8px!important;}',
            '#phone #app_Health .vh3-call strong{font-size:10px!important;line-height:12px!important;}',
            '#phone #app_Health .vh3-call-status{font-size:7px!important;line-height:9px!important;margin-top:5px!important;}',
            '#phone #app_Health .vh3-confirm{top:0!important;height:559px!important;border-radius:0 0 25px 25px!important;}'
        ].join('');
        document.head.appendChild(style);
    }

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
        home.style.visibility = show ? 'visible' : 'hidden';
        home.style.pointerEvents = show ? '' : 'none';
        home.style.transition = 'opacity .18s ease';
    }

    function enforceHealth(){
        if(!healthPinned) return;
        var health = document.getElementById('app_Health');
        if(!health) return;
        hidePhoneApps();
        showHomeScreen(false);
        health.style.display = 'block';
        health.classList.add('rdp-health-open');
        health.classList.remove('rdp-health-leaving');
        health.setAttribute('aria-hidden','false');
    }

    function startGuard(){
        if(guardTimer) clearInterval(guardTimer);
        guardTimer = setInterval(enforceHealth, 180);
    }

    function stopGuard(){
        if(guardTimer){ clearInterval(guardTimer); guardTimer = null; }
    }

    function openHealth(){
        var health = document.getElementById('app_Health');
        if(!health) return false;
        injectHealthFixes();
        if(leaveTimer){ clearTimeout(leaveTimer); leaveTimer = null; }
        healthPinned = true;
        window.__rdpCurrentApp = '_Health';
        hidePhoneApps();
        showHomeScreen(false);
        health.classList.remove('rdp-health-leaving');
        health.style.display = 'block';
        health.setAttribute('aria-hidden','false');
        requestAnimationFrame(function(){
            requestAnimationFrame(function(){ health.classList.add('rdp-health-open'); });
        });
        startGuard();
        if(typeof window.VeloraHealthRender === 'function') {
            try { window.VeloraHealthRender(); } catch(_) {}
        }
        return true;
    }

    function closeHealth(restoreHome){
        var health = document.getElementById('app_Health');
        healthPinned = false;
        window.__rdpCurrentApp = null;
        stopGuard();
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
        if(homeButton && healthPinned) {
            closeHealth(true);
        }
    }, true);

    injectHealthFixes();
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