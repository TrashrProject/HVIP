(() => {
  'use strict';

  const VERSION = '11.0.0-migration-safe';
  const STYLE_ID = 'paradise-native-ui-off-style';
  const HUD_ID = 'paradise-rp-hud';

  if (window.__PARADISE_NATIVE_UI_OFF_LOCK__ === VERSION) return;
  window.__PARADISE_NATIVE_UI_OFF_LOCK__ = VERSION;

  const css = `
    #CombatMode,#PSVMode,#TicketMode,#PhoneMode,#InventoryMode,#RoomInfoMode,#MessengerMode,#HelpMode,
    .menuButton-yNbz6_0,.button-3IzmP_0.menuButton-yNbz6_0,[class*="menuButton-yNbz6"],
    [class*="nitro-toolbar"],[class*="habbo-toolbar"] {
      display:none!important;visibility:hidden!important;opacity:0!important;pointer-events:none!important;
    }
    #root [data-pr-native-chat-bridge="1"] {
      position:fixed!important;left:-10000px!important;top:0!important;width:1px!important;min-width:1px!important;max-width:1px!important;
      height:1px!important;min-height:1px!important;max-height:1px!important;padding:0!important;margin:0!important;border:0!important;
      box-shadow:none!important;background:transparent!important;color:transparent!important;opacity:0!important;pointer-events:none!important;z-index:-1!important;
    }
    #paradise-rp-hard-sidewall,#paradise-rp-hard-masks,.pr-mask,.prhud-cover,#${HUD_ID} .prhud-chat {
      display:none!important;visibility:hidden!important;opacity:0!important;pointer-events:none!important;
    }
    #root canvas { visibility:visible!important;opacity:1!important; }
  `;

  function installCss(){ let style=document.getElementById(STYLE_ID); if(!style){style=document.createElement('style');style.id=STYLE_ID;document.head.appendChild(style);} if(style.textContent!==css)style.textContent=css; }
  function isNativeChatField(el){ if(!el||(el.tagName!=='INPUT'&&el.tagName!=='TEXTAREA'))return false; if(el.id==='pr4-chat-input'||el.closest?.(`#${HUD_ID}`))return false; const text=`${el.getAttribute('placeholder')||''} ${el.className||''} ${el.id||''}`; return /haz|chatear|chat|chatter|parler|message|say/i.test(text); }
  function markChat(el){ if(!isNativeChatField(el))return false; if(el.dataset.prNativeChatBridge==='1')return true; el.dataset.prNativeChatBridge='1'; el.setAttribute('aria-hidden','true'); el.setAttribute('tabindex','-1'); return true; }
  function scanNode(node){ if(!node||node.nodeType!==1)return; if(node.matches?.('input, textarea'))markChat(node); node.querySelectorAll?.('input, textarea').forEach(markChat); }
  function scan(){ installCss(); const nitroRoot=document.getElementById('root'); if(nitroRoot)scanNode(nitroRoot); }
  function boot(){ scan(); const nitroRoot=document.getElementById('root'); if(!nitroRoot||window.__ParadiseNativeUiObserver)return; const observer=new MutationObserver(records=>{ for(const record of records)record.addedNodes.forEach(scanNode); }); observer.observe(nitroRoot,{childList:true,subtree:true}); window.__ParadiseNativeUiObserver=observer; }

  window.__paradiseNativeUiOffScan=scan;
  window.__paradiseNativeUiOffVersion=VERSION;
  if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',boot,{once:true});else boot();
})();
