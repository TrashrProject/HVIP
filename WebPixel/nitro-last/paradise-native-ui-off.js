(() => {
  'use strict';

  const VERSION = '10.0.0-renderer-safe-cleanup';
  const STYLE_ID = 'paradise-native-ui-off-style';
  const HUD_ID = 'paradise-rp-hud';
  window.__PARADISE_NATIVE_UI_OFF_LOCK__ = VERSION;

  const css = `
    /* ParadiseRP native UI cleanup — renderer safe mode.
       Ne masque jamais les containers qui portent le canvas / Pixi. */
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
    [class*="button-3IzmP"] {
      display: none !important;
      visibility: hidden !important;
      opacity: 0 !important;
      pointer-events: none !important;
    }

    [data-pr-native-ui-killed="1"]:not([data-pr-native-chat-live="1"]):not([data-pr-native-chat-host="1"]):not([data-pr-renderer-safe="1"]) {
      display: none !important;
      visibility: hidden !important;
      opacity: 0 !important;
      pointer-events: none !important;
    }

    #${HUD_ID} .prhud-chat {
      display: none !important;
      visibility: hidden !important;
      opacity: 0 !important;
      pointer-events: none !important;
    }

    [data-pr-renderer-safe="1"],
    [data-pr-renderer-safe="1"] canvas,
    #root canvas {
      display: block !important;
      visibility: visible !important;
      opacity: 1 !important;
      pointer-events: auto !important;
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
      bottom: 25px !important;
      transform: translateX(-50%) !important;
      width: min(560px, calc(100vw - 860px)) !important;
      min-width: 420px !important;
      max-width: 620px !important;
      height: 48px !important;
      min-height: 48px !important;
      max-height: 48px !important;
      z-index: 2147483646 !important;
      display: block !important;
      visibility: visible !important;
      opacity: 1 !important;
      pointer-events: auto !important;
      color: #17272d !important;
      caret-color: #65d7c5 !important;
      background: rgba(255,255,255,.82) !important;
      border: 1px solid rgba(255,255,255,.58) !important;
      border-radius: 14px !important;
      box-shadow: 0 14px 30px rgba(18,34,40,.17), inset 0 1px 0 rgba(255,255,255,.6) !important;
      padding: 0 54px 0 108px !important;
      outline: 0 !important;
      font: 800 13px/48px Inter, "Segoe UI", Arial, sans-serif !important;
      text-align: left !important;
      backdrop-filter: blur(8px) !important;
    }

    [data-pr-native-chat-live="1"]::placeholder,
    #root input[placeholder*="Haz" i]:not(#prhud-chat-input)::placeholder,
    #root input[placeholder*="chatear" i]:not(#prhud-chat-input)::placeholder,
    #root input[placeholder*="chat" i]:not(#prhud-chat-input)::placeholder {
      color: #72868b !important;
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
    if (el.id === 'root' || el.id === 'RdpNitroFrame' || el.id === HUD_ID || el.id === 'paradise-loader') return true;
    if (el.tagName === 'IFRAME' || el.tagName === 'CANVAS') return true;
    if (['SCRIPT', 'STYLE', 'LINK', 'META', 'TITLE', 'BASE', 'NOSCRIPT'].includes(el.tagName)) return true;
    if (el.closest && el.closest(`#${HUD_ID}, #paradise-loader`)) return true;
    if (el.closest && el.closest('[data-pr-renderer-safe="1"]')) return true;
    return false;
  };

  const hasRendererCanvas = el => {
    try {
      if (!el || !el.querySelectorAll) return false;
      return Array.from(el.querySelectorAll('canvas')).some(canvas => {
        const r = canvas.getBoundingClientRect();
        const w = r.width || canvas.width || 0;
        const h = r.height || canvas.height || 0;
        return w > window.innerWidth * 0.18 && h > window.innerHeight * 0.18;
      });
    } catch (error) {
      return false;
    }
  };

  const restoreElement = el => {
    if (!el || el === document.documentElement || el === document.body) return;
    try {
      el.setAttribute('data-pr-renderer-safe', '1');
      el.removeAttribute('data-pr-native-ui-killed');
      el.style.removeProperty('display');
      el.style.removeProperty('visibility');
      el.style.removeProperty('opacity');
      el.style.removeProperty('pointer-events');
      el.style.removeProperty('filter');
      el.style.removeProperty('clip-path');
      el.style.removeProperty('transform');
      el.style.removeProperty('height');
      el.style.removeProperty('width');
      el.style.setProperty('visibility', 'visible', 'important');
      el.style.setProperty('opacity', '1', 'important');
      if (el.tagName === 'CANVAS') {
        el.style.setProperty('display', 'block', 'important');
      }
    } catch (error) {}
  };

  const protectRenderer = () => {
    try {
      document.querySelectorAll('#root canvas').forEach(canvas => {
        restoreElement(canvas);
        let parent = canvas.parentElement;
        for (let depth = 0; parent && depth < 12; depth += 1, parent = parent.parentElement) {
          restoreElement(parent);
          if (parent.id === 'root') break;
        }
      });

      document.querySelectorAll('[data-pr-native-ui-killed="1"]').forEach(el => {
        if (hasRendererCanvas(el)) restoreElement(el);
      });
    } catch (error) {}
  };

  const hideElement = el => {
    protectRenderer();
    if (isProtected(el) || hasRendererCanvas(el)) return;
    if (el.querySelector?.('[data-pr-native-chat-live="1"]') || el.getAttribute?.('data-pr-native-chat-live') === '1') return;
    try {
      el.removeAttribute('data-pr-native-chat-live');
      el.removeAttribute('data-pr-native-chat-host');
      el.setAttribute('data-pr-native-ui-killed', '1');
      el.style.setProperty('display', 'none', 'important');
      el.style.setProperty('visibility', 'hidden', 'important');
      el.style.setProperty('opacity', '0', 'important');
      el.style.setProperty('pointer-events', 'none', 'important');
    } catch (error) {}
  };

  const isNativeChatField = el => {
    if (!el || (el.tagName !== 'INPUT' && el.tagName !== 'TEXTAREA')) return false;
    if (el.id === 'prhud-chat-input' || el.closest?.(`#${HUD_ID}`)) return false;
    const text = `${el.getAttribute('placeholder') || ''} ${el.className || ''} ${el.id || ''} ${el.value || ''}`;
    return /haz|chatear|chat|chatter|parler|message|say/i.test(text);
  };

  const restoreNativeChat = () => {
    try {
      document.querySelectorAll('input, textarea').forEach(input => {
        if (!isNativeChatField(input)) return;

        input.removeAttribute('data-pr-native-ui-killed');
        input.removeAttribute('data-pr-native-chat-parked');
        input.setAttribute('data-pr-native-chat-live', '1');
        if (/haz|chatear/i.test(input.getAttribute('placeholder') || '')) input.setAttribute('placeholder', 'Écrire un message...');
        if (!input.getAttribute('placeholder')) input.setAttribute('placeholder', 'Écrire un message...');

        input.style.setProperty('display', 'block', 'important');
        input.style.setProperty('visibility', 'visible', 'important');
        input.style.setProperty('opacity', '1', 'important');
        input.style.setProperty('pointer-events', 'auto', 'important');

        let parent = input.parentElement;
        for (let i = 0; parent && i < 5; i += 1, parent = parent.parentElement) {
          if (isProtected(parent) || hasRendererCanvas(parent)) break;
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
        }
      });
    } catch (error) {}
  };

  const selectors = [
    '#CombatMode', '#PSVMode', '#TicketMode', '#NavigatorMode', '#FriendsMode', '#SettingsMode',
    '#PhoneMode', '#CatalogMode', '#InventoryMode', '#RoomInfoMode',
    '.menuButton-yNbz6_0', '[class*="menuButton-yNbz6"]', '[class*="button-3IzmP"]',
    '[class*="nitro-toolbar"]', '[class*="habbo-toolbar"]'
  ];

  const killOnlyKnownButtons = () => {
    installCss();
    protectRenderer();
    restoreNativeChat();

    for (const selector of selectors) {
      try {
        document.querySelectorAll(selector).forEach(el => {
          if (isProtected(el) || hasRendererCanvas(el)) return;
          const r = el.getBoundingClientRect?.();
          if (r && (r.width > window.innerWidth * 0.55 || r.height > window.innerHeight * 0.55)) return;
          hideElement(el);
        });
      } catch (error) {}
    }

    // Filet de sécurité: si un précédent scan a caché un parent du renderer,
    // on le réactive immédiatement. C'est ce qui évite l'écran noir aléatoire.
    protectRenderer();
    restoreNativeChat();
  };

  const boot = () => {
    installCss();
    killOnlyKnownButtons();
    [0, 50, 120, 250, 500, 900, 1400, 2200, 4000, 7000, 11000].forEach(ms => setTimeout(killOnlyKnownButtons, ms));
    if (window.__paradiseNativeUiOffSafeInterval) clearInterval(window.__paradiseNativeUiOffSafeInterval);
    window.__paradiseNativeUiOffSafeInterval = setInterval(killOnlyKnownButtons, 700);
  };

  window.__paradiseNativeUiOffScan = killOnlyKnownButtons;
  window.__paradiseNativeUiOffProtectRenderer = protectRenderer;
  window.__paradiseNativeUiOffVersion = VERSION;

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
