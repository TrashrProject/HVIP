(() => {
  'use strict';

  const VERSION = '6.0.0-safe-legacy-labels';
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

  const getText = el => {
    try {
      return String((el.innerText || el.textContent || el.getAttribute?.('placeholder') || el.getAttribute?.('title') || el.getAttribute?.('aria-label') || '')).trim();
    } catch (_) {
      return '';
    }
  };

  const isLegacyText = text => /\[(CALLE|INT|SALA|ROOM)\]|Haz clic|chatear|HabboVIP|Que hay|Qué hay|What'?s new|proposito|propósito|seguridad|sitio web|Bubble/i.test(text || '');

  const isSmallVisibleBox = el => {
    if (isProtected(el) || hasLargeCanvas(el)) return false;
    let r;
    try { r = el.getBoundingClientRect(); } catch (_) { return false; }
    if (!r || r.width <= 2 || r.height <= 2) return false;
    if (r.width > Math.min(520, window.innerWidth * 0.42)) return false;
    if (r.height > Math.min(170, window.innerHeight * 0.22)) return false;
    return true;
  };

  const isNativeChatField = el => {
    if (!el || (el.tagName !== 'INPUT' && el.tagName !== 'TEXTAREA')) return false;
    if (el.id === 'prhud-chat-input' || (el.closest && el.closest('#paradise-rp-hud'))) return false;
    const text = String(el.getAttribute('placeholder') || el.value || '');
    return /haz|chatear|chat|parler/i.test(text);
  };

  const isBottomLeftLegacy = el => {
    if (!isSmallVisibleBox(el)) return false;
    let r;
    try { r = el.getBoundingClientRect(); } catch (_) { return false; }

    const vw = window.innerWidth;
    const vh = window.innerHeight;
    const text = getText(el);

    const oldChatBar = r.left < 470 && r.top > vh - 145 && r.width < 470 && r.height < 95;
    const oldRoomLabel = r.left < 280 && r.top > vh - 300 && r.width < 300 && r.height < 105 && isLegacyText(text);
    const oldBottomIcons = r.left < 260 && r.top > vh - 135 && r.width < 280 && r.height < 125;
    const oldLowerLeftRail = r.left < 55 && r.top > vh - 315 && r.width < 65 && r.height < 315;
    const oldPhone = r.right > vw - 85 && r.bottom > vh - 100 && r.width < 85 && r.height < 100;
    const oldTopRightToast = r.right > vw - 360 && r.top < 95 && r.width < 360 && r.height < 95 && isLegacyText(text);

    return oldChatBar || oldRoomLabel || oldBottomIcons || oldLowerLeftRail || oldPhone || oldTopRightToast;
  };

  const killLegacyTextBlocks = () => {
    try {
      document.querySelectorAll('div, span, p, section, aside, label, button').forEach(el => {
        if (!isSmallVisibleBox(el)) return;
        const text = getText(el);
        if (!isLegacyText(text)) return;
        hideElement(el);
        let parent = el.parentElement;
        for (let i = 0; parent && i < 3; i += 1, parent = parent.parentElement) {
          if (isSmallVisibleBox(parent) && (isBottomLeftLegacy(parent) || isLegacyText(getText(parent)))) hideElement(parent);
        }
      });
    } catch (_) {}
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
      document.querySelectorAll('button, img, input, textarea, div, span, i, p, section, aside').forEach(el => {
        if (isBottomLeftLegacy(el)) hideElement(el);
      });
    } catch (_) {}

    killLegacyTextBlocks();
  };

  const boot = () => {
    installCss();
    killOnlyKnownButtons();
    [0, 50, 120, 250, 500, 900, 1400, 2200, 4000, 7000].forEach(ms => setTimeout(killOnlyKnownButtons, ms));
    if (window.__paradiseNativeUiOffSafeInterval) clearInterval(window.__paradiseNativeUiOffSafeInterval);
    window.__paradiseNativeUiOffSafeInterval = setInterval(killOnlyKnownButtons, 400);
  };

  window.__paradiseNativeUiOffScan = killOnlyKnownButtons;
  window.__paradiseNativeUiOffVersion = VERSION;

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
