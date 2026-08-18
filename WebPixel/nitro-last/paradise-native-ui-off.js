(() => {
  'use strict';

  const VERSION = '9.0.0-native-chat-visible';
  const STYLE_ID = 'paradise-native-ui-off-style';
  window.__PARADISE_NATIVE_UI_OFF_LOCK__ = VERSION;

  const css = `
    /* ParadiseRP native UI off - safe mode.
       Le vrai champ chat Nitro reste visible et fonctionnel. */
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
    [data-pr-native-ui-killed="1"]:not([data-pr-native-chat-live="1"]):not([data-pr-native-chat-host="1"]) {
      display: none !important;
      visibility: hidden !important;
      opacity: 0 !important;
      pointer-events: none !important;
    }

    #paradise-rp-hud .prhud-chat {
      display: none !important;
      visibility: hidden !important;
      opacity: 0 !important;
      pointer-events: none !important;
    }

    [data-pr-native-chat-host="1"] {
      display: block !important;
      visibility: visible !important;
      opacity: 1 !important;
      pointer-events: none !important;
      background: transparent !important;
      border: 0 !important;
      box-shadow: none !important;
      overflow: visible !important;
    }

    [data-pr-native-chat-live="1"],
    #root input[placeholder*="Haz" i]:not(#prhud-chat-input),
    #root input[placeholder*="chatear" i]:not(#prhud-chat-input),
    #root input[placeholder*="chat" i]:not(#prhud-chat-input),
    #root textarea[placeholder*="Haz" i]:not(#prhud-chat-input),
    #root textarea[placeholder*="chatear" i]:not(#prhud-chat-input),
    #root textarea[placeholder*="chat" i]:not(#prhud-chat-input) {
      position: fixed !important;
      left: 50% !important;
      bottom: 64px !important;
      transform: translateX(-50%) !important;
      width: 610px !important;
      height: 44px !important;
      min-width: 610px !important;
      min-height: 44px !important;
      max-width: calc(100vw - 620px) !important;
      max-height: 44px !important;
      z-index: 2147483646 !important;
      display: block !important;
      visibility: visible !important;
      opacity: 1 !important;
      pointer-events: auto !important;
      color: #dce8f3 !important;
      caret-color: #38ddff !important;
      background: linear-gradient(180deg, rgba(8,24,37,.972), rgba(2,8,14,.992)) !important;
      border: 1px solid rgba(39,200,255,.48) !important;
      border-radius: 14px !important;
      box-shadow: 0 14px 28px rgba(0,0,0,.42), inset 0 1px 0 rgba(255,255,255,.10) !important;
      padding: 0 52px 0 18px !important;
      outline: 0 !important;
      font: 800 13px/44px Inter, "Segoe UI", Arial, sans-serif !important;
      text-align: left !important;
    }

    [data-pr-native-chat-live="1"]::placeholder,
    #root input[placeholder*="Haz" i]:not(#prhud-chat-input)::placeholder,
    #root input[placeholder*="chatear" i]:not(#prhud-chat-input)::placeholder,
    #root input[placeholder*="chat" i]:not(#prhud-chat-input)::placeholder {
      color: #7d8d9f !important;
      opacity: 1 !important;
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
    if (el.querySelector?.('[data-pr-native-chat-live="1"]') || el.getAttribute?.('data-pr-native-chat-live') === '1') return;
    try {
      el.removeAttribute('data-pr-native-chat-live');
      el.removeAttribute('data-pr-native-chat-host');
      el.setAttribute('data-pr-native-ui-killed', '1');
      el.style.setProperty('display', 'none', 'important');
      el.style.setProperty('visibility', 'hidden', 'important');
      el.style.setProperty('opacity', '0', 'important');
      el.style.setProperty('pointer-events', 'none', 'important');
    } catch (_) {}
  };

  const getText = el => {
    try {
      return String((el.innerText || el.textContent || el.getAttribute?.('placeholder') || el.getAttribute?.('title') || el.getAttribute?.('aria-label') || '')).trim();
    } catch (_) {
      return '';
    }
  };

  const isNativeChatField = el => {
    if (!el || (el.tagName !== 'INPUT' && el.tagName !== 'TEXTAREA')) return false;
    if (el.id === 'prhud-chat-input' || el.closest?.('#paradise-rp-hud')) return false;
    const text = `${el.getAttribute('placeholder') || ''} ${el.className || ''} ${el.id || ''} ${el.value || ''}`;
    return /haz|chatear|chat|parler|message|say/i.test(text);
  };

  const restoreNativeChat = () => {
    try {
      document.querySelectorAll('input, textarea').forEach(input => {
        if (!isNativeChatField(input)) return;

        input.removeAttribute('data-pr-native-ui-killed');
        input.removeAttribute('data-pr-native-chat-parked');
        input.setAttribute('data-pr-native-chat-live', '1');
        if (/haz|chatear/i.test(input.getAttribute('placeholder') || '')) input.setAttribute('placeholder', 'Clique ici pour chatter...');

        try {
          input.style.setProperty('display', 'block', 'important');
          input.style.setProperty('visibility', 'visible', 'important');
          input.style.setProperty('opacity', '1', 'important');
          input.style.setProperty('pointer-events', 'auto', 'important');
        } catch (_) {}

        let parent = input.parentElement;
        for (let i = 0; parent && i < 5; i += 1, parent = parent.parentElement) {
          if (isProtected(parent) || hasLargeCanvas(parent)) break;
          try {
            parent.removeAttribute('data-pr-native-ui-killed');
            parent.removeAttribute('data-pr-native-chat-parked');
            parent.setAttribute('data-pr-native-chat-host', '1');
            parent.style.setProperty('display', 'block', 'important');
            parent.style.setProperty('visibility', 'visible', 'important');
            parent.style.setProperty('opacity', '1', 'important');
            parent.style.setProperty('pointer-events', 'none', 'important');
            parent.style.setProperty('background', 'transparent', 'important');
            parent.style.setProperty('border', '0', 'important');
            parent.style.setProperty('box-shadow', 'none', 'important');
            parent.style.setProperty('overflow', 'visible', 'important');
          } catch (_) {}
        }
      });
    } catch (_) {}
  };

  const selectors = [
    '#CombatMode', '#PSVMode', '#TicketMode', '#NavigatorMode', '#FriendsMode', '#SettingsMode',
    '#PhoneMode', '#CatalogMode', '#InventoryMode', '#RoomInfoMode',
    '.menuButton-yNbz6_0', '[class*="menuButton-yNbz6"]', '[class*="button-3IzmP"]',
    '[class*="nitro-toolbar"]', '[class*="toolbar"]', '[class*="toolbar-item"]',
    '[class*="habbo-toolbar"]'
  ];

  const isLegacyText = text => /\[(CALLE|INT|SALA|ROOM)\]|HabboVIP|Que hay|Qué hay|What'?s new|proposito|propósito|seguridad|sitio web|Bubble/i.test(text || '');

  const isSmallVisibleBox = el => {
    if (isProtected(el) || hasLargeCanvas(el)) return false;
    if (el.querySelector?.('[data-pr-native-chat-live="1"]') || el.getAttribute?.('data-pr-native-chat-live') === '1') return false;
    let r;
    try { r = el.getBoundingClientRect(); } catch (_) { return false; }
    if (!r || r.width <= 2 || r.height <= 2) return false;
    if (r.width > Math.min(520, window.innerWidth * 0.42)) return false;
    if (r.height > Math.min(170, window.innerHeight * 0.22)) return false;
    return true;
  };

  const isBottomLeftLegacy = el => {
    if (el.querySelector?.('[data-pr-native-chat-live="1"]') || el.getAttribute?.('data-pr-native-chat-live') === '1') return false;
    if (!isSmallVisibleBox(el)) return false;
    let r;
    try { r = el.getBoundingClientRect(); } catch (_) { return false; }

    const vw = window.innerWidth;
    const vh = window.innerHeight;
    const text = getText(el);

    const oldRoomLabel = r.left < 280 && r.top > vh - 300 && r.width < 300 && r.height < 105 && isLegacyText(text);
    const oldBottomIcons = r.left < 260 && r.top > vh - 135 && r.width < 280 && r.height < 125;
    const oldLowerLeftRail = r.left < 55 && r.top > vh - 315 && r.width < 65 && r.height < 315;
    const oldPhone = r.right > vw - 85 && r.bottom > vh - 100 && r.width < 85 && r.height < 100;
    const oldTopRightToast = r.right > vw - 360 && r.top < 95 && r.width < 360 && r.height < 95 && isLegacyText(text);

    return oldRoomLabel || oldBottomIcons || oldLowerLeftRail || oldPhone || oldTopRightToast;
  };

  const killLegacyTextBlocks = () => {
    try {
      document.querySelectorAll('div, span, p, section, aside, label, button').forEach(el => {
        if (!isSmallVisibleBox(el)) return;
        const text = getText(el);
        if (!isLegacyText(text)) return;
        hideElement(el);
      });
    } catch (_) {}
  };

  const killOnlyKnownButtons = () => {
    installCss();
    restoreNativeChat();

    for (const selector of selectors) {
      try {
        document.querySelectorAll(selector).forEach(el => {
          if (isProtected(el)) return;
          if (el.querySelector?.('[data-pr-native-chat-live="1"]')) return;
          const r = el.getBoundingClientRect();
          if (r.width > window.innerWidth * 0.55 || r.height > window.innerHeight * 0.55) return;
          hideElement(el);
        });
      } catch (_) {}
    }

    try {
      document.querySelectorAll('button, img, div, span, i, p, section, aside').forEach(el => {
        if (isBottomLeftLegacy(el)) hideElement(el);
      });
    } catch (_) {}

    killLegacyTextBlocks();
    restoreNativeChat();
  };

  const boot = () => {
    installCss();
    killOnlyKnownButtons();
    [0, 50, 120, 250, 500, 900, 1400, 2200, 4000, 7000].forEach(ms => setTimeout(killOnlyKnownButtons, ms));
    if (window.__paradiseNativeUiOffSafeInterval) clearInterval(window.__paradiseNativeUiOffSafeInterval);
    window.__paradiseNativeUiOffSafeInterval = setInterval(killOnlyKnownButtons, 700);
  };

  window.__paradiseNativeUiOffScan = killOnlyKnownButtons;
  window.__paradiseNativeUiOffVersion = VERSION;

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
