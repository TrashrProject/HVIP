(() => {
  'use strict';

  const VERSION = '4.0.0-safe';
  const STYLE_ID = 'paradise-native-ui-off-style';
  window.__PARADISE_NATIVE_UI_OFF_LOCK__ = VERSION;

  const css = `
    /* ParadiseRP native UI off - SAFE MODE
       Important : aucun parent large, iframe, canvas ou root n'est supprimé. */
    #CombatMode,
    #PSVMode,
    #TicketMode,
    #NavigatorMode,
    #FriendsMode,
    #SettingsMode,
    #PhoneMode,
    #CatalogMode,
    #InventoryMode,
    #RoomInfoMode,
    .menuButton-yNbz6_0,
    .button-3IzmP_0.menuButton-yNbz6_0,
    [class*="menuButton-yNbz6"],
    [class*="button-3IzmP"],
    [data-pr-native-ui-killed="1"] {
      display: none !important;
      visibility: hidden !important;
      opacity: 0 !important;
      pointer-events: none !important;
    }

    #paradise-rp-hard-sidewall,
    #paradise-rp-hard-masks,
    .pr-mask,
    .prhud-cover {
      display: none !important;
      visibility: hidden !important;
      opacity: 0 !important;
      pointer-events: none !important;
    }
  `;

  const installCss = () => {
    let style = document.getElementById(STYLE_ID);
    if (!style) {
      style = document.createElement('style');
      style.id = STYLE_ID;
      (document.head || document.documentElement).appendChild(style);
    }
    if (style.textContent !== css) style.textContent = css;
  };

  const killOnlyKnownButtons = () => {
    installCss();
    const selectors = [
      '#CombatMode', '#PSVMode', '#TicketMode', '#NavigatorMode', '#FriendsMode', '#SettingsMode',
      '#PhoneMode', '#CatalogMode', '#InventoryMode', '#RoomInfoMode',
      '.menuButton-yNbz6_0', '[class*="menuButton-yNbz6"]', '[class*="button-3IzmP"]'
    ];
    for (const selector of selectors) {
      try {
        document.querySelectorAll(selector).forEach(el => {
          if (!el || el.id === 'root' || el.id === 'RdpNitroFrame' || el.tagName === 'IFRAME' || el.tagName === 'CANVAS') return;
          el.setAttribute('data-pr-native-ui-killed', '1');
          el.style.setProperty('display', 'none', 'important');
          el.style.setProperty('visibility', 'hidden', 'important');
          el.style.setProperty('opacity', '0', 'important');
          el.style.setProperty('pointer-events', 'none', 'important');
        });
      } catch (_) {}
    }
  };

  const boot = () => {
    installCss();
    killOnlyKnownButtons();
    [50, 150, 350, 700, 1200, 2200, 4000, 7000].forEach(ms => setTimeout(killOnlyKnownButtons, ms));
    if (window.__paradiseNativeUiOffSafeInterval) clearInterval(window.__paradiseNativeUiOffSafeInterval);
    window.__paradiseNativeUiOffSafeInterval = setInterval(killOnlyKnownButtons, 750);
  };

  window.__paradiseNativeUiOffScan = killOnlyKnownButtons;
  window.__paradiseNativeUiOffVersion = VERSION;

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
