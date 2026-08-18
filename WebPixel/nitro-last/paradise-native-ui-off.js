(() => {
  'use strict';

  const VERSION = '5.0.0-safe-bottom-left';
  const STYLE_ID = 'paradise-native-ui-off-style';
  window.__PARADISE_NATIVE_UI_OFF_LOCK__ = VERSION;

  const css = `
    /* ParadiseRP native UI off - SAFE MODE
       Aucun parent large, iframe, canvas ou root n'est supprimé. */
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

    #root input[placeholder*="Haz" i],
    #root input[placeholder*="chatear" i],
    #root input[placeholder*="chat" i],
    #root textarea[placeholder*="Haz" i],
    #root textarea[placeholder*="chatear" i],
    #root textarea[placeholder*="chat" i],
    body input[placeholder*="Haz" i]:not(#prhud-chat-input),
    body input[placeholder*="chatear" i]:not(#prhud-chat-input),
    body textarea[placeholder*="Haz" i]:not(#prhud-chat-input),
    body textarea[placeholder*="chatear" i]:not(#prhud-chat-input) {
      position: fixed !important;
      left: -99999px !important;
      top: auto !important;
      bottom: auto !important;
      width: 1px !important;
      height: 1px !important;
      opacity: 0 !important;
      pointer-events: none !important;
      visibility: hidden !important;
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

  const isProtected = el => {
    if (!el || el === document.documentElement || el === document.body) return true;
    if (el.id === 'root' || el.id === 'RdpNitroFrame' || el.id === 'paradise-rp-hud' || el.id === 'paradise-loader') return true;
    if (el.tagName === 'IFRAME' || el.tagName === 'CANVAS') return true;
    if (el.closest && el.closest('#paradise-rp-hud, #paradise-loader')) return true;
    if (['SCRIPT', 'STYLE', 'LINK', 'META', 'TITLE', 'BASE', 'NOSCRIPT'].includes(el.tagName)) return true;
    return false;
  };

  const hasLargeCanvas = el => {
    try {
      if (!el || !el.querySelectorAll) return false;
      for (const canvas of el.querySelectorAll('canvas')) {
        const r = canvas.getBoundingClientRect();
        if (r.width > window.innerWidth * 0.35 && r.height > window.innerHeight * 0.35) return true;
      }
    } catch (_) {}
    return false;
  };

  const hideElement = el => {
    if (isProtected(el) || hasLargeCanvas(el)) return;
    try {
      el.setAttribute('data-pr-native-ui-killed', '1');
      el.style.setProperty('display', 'none', 'important');
      el.style.setProperty('visibility', 'hidden', 'important');
      el.style.setProperty('opacity', '0', 'important');
      el.style.setProperty('pointer-events', 'none', 'important');
    } catch (_) {}
  };

  const moveNativeChatAway = el => {
    if (isProtected(el)) return;
    try {
      el.setAttribute('data-pr-native-ui-killed', '1');
      el.style.setProperty('position', 'fixed', 'important');
      el.style.setProperty('left', '-99999px', 'important');
      el.style.setProperty('top', 'auto', 'important');
      el.style.setProperty('bottom', 'auto', 'important');
      el.style.setProperty('width', '1px', 'important');
      el.style.setProperty('height', '1px', 'important');
      el.style.setProperty('opacity', '0', 'important');
      el.style.setProperty('pointer-events', 'none', 'important');
      el.style.setProperty('visibility', 'hidden', 'important');
    } catch (_) {}
  };

  const selectors = [
    '#CombatMode', '#PSVMode', '#TicketMode', '#NavigatorMode', '#FriendsMode', '#SettingsMode',
    '#PhoneMode', '#CatalogMode', '#InventoryMode', '#RoomInfoMode',
    '.menuButton-yNbz6_0', '[class*="menuButton-yNbz6"]', '[class*="button-3IzmP"]',
    '[class*="nitro-toolbar"]', '[class*="toolbar"]', '[class*="toolbar-item"]',
    '[class*="habbo-toolbar"]', '[class*="chatinput"]', '[class*="chat-input"]'
  ];

  const isNativeChatField = el => {
    if (!el || (el.tagName !== 'INPUT' && el.tagName !== 'TEXTAREA')) return false;
    if (el.id === 'prhud-chat-input' || (el.closest && el.closest('#paradise-rp-hud'))) return false;
    const text = String(el.getAttribute('placeholder') || el.value || '');
    return /haz|chatear|chat|parler/i.test(text);
  };

  const isBottomLeftLegacy = el => {
    if (isProtected(el)) return false;
    let r;
    try { r = el.getBoundingClientRect(); } catch (_) { return false; }
    if (!r || r.width <= 2 || r.height <= 2) return false;
    if (r.width > window.innerWidth * 0.38 || r.height > window.innerHeight * 0.35) return false;

    const vw = window.innerWidth;
    const vh = window.innerHeight;

    const oldChatBar = r.left < 460 && r.top > vh - 125 && r.width < 460 && r.height < 90;
    const oldBottomIcons = r.left < 245 && r.top > vh - 120 && r.width < 260 && r.height < 115;
    const oldLowerLeftRail = r.left < 48 && r.top > vh - 285 && r.width < 56 && r.height < 285;
    const oldPhone = r.right > vw - 80 && r.bottom > vh - 95 && r.width < 80 && r.height < 95;

    return oldChatBar || oldBottomIcons || oldLowerLeftRail || oldPhone;
  };

  const killOnlyKnownButtons = () => {
    installCss();

    for (const selector of selectors) {
      try {
        document.querySelectorAll(selector).forEach(el => {
          if (isProtected(el)) return;
          const r = el.getBoundingClientRect();
          if (r.width > window.innerWidth * 0.55 || r.height > window.innerHeight * 0.55) return;
          hideElement(el);
        });
      } catch (_) {}
    }

    try {
      document.querySelectorAll('input, textarea').forEach(el => {
        if (isNativeChatField(el)) {
          moveNativeChatAway(el);
          let parent = el.parentElement;
          for (let i = 0; parent && i < 3; i += 1, parent = parent.parentElement) {
            if (isBottomLeftLegacy(parent)) hideElement(parent);
          }
        }
      });
    } catch (_) {}

    try {
      document.querySelectorAll('button, img, input, textarea, div, span, i').forEach(el => {
        if (isBottomLeftLegacy(el)) hideElement(el);
      });
    } catch (_) {}
  };

  const boot = () => {
    installCss();
    killOnlyKnownButtons();
    [0, 50, 120, 250, 500, 900, 1400, 2200, 4000, 7000].forEach(ms => setTimeout(killOnlyKnownButtons, ms));
    if (window.__paradiseNativeUiOffSafeInterval) clearInterval(window.__paradiseNativeUiOffSafeInterval);
    window.__paradiseNativeUiOffSafeInterval = setInterval(killOnlyKnownButtons, 450);
  };

  window.__paradiseNativeUiOffScan = killOnlyKnownButtons;
  window.__paradiseNativeUiOffVersion = VERSION;

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
