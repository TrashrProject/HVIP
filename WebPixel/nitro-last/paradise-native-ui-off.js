(() => {
  'use strict';

  const VERSION = '3.0.0-lock';
  const STYLE_ID = 'paradise-native-ui-off-style';
  const KILL_ATTR = 'data-pr-native-ui-killed';
  const KILL_CLASS = 'pr-native-ui-killed';
  const OWN_SELECTOR = '#paradise-rp-hud,#paradise-loader,#paradise-native-ui-off-style,#paradise-rp-hard-ui-killer-style,#paradise-rp-hard-sidewall,#paradise-rp-hard-masks';

  window.__PARADISE_NATIVE_UI_OFF_LOCK__ = VERSION;

  const baseCss = `
    .${KILL_CLASS},[${KILL_ATTR}="1"],
    #CombatMode,#PSVMode,#TicketMode,#NavigatorMode,#FriendsMode,#SettingsMode,
    [id="CombatMode"],[id="PSVMode"],
    .menuButton-yNbz6_0.button-3IzmP_0,
    .menuButton-yNbz6_0,
    .button-3IzmP_0[data-pr-native-old="1"]{
      display:none!important;
      visibility:hidden!important;
      opacity:0!important;
      pointer-events:none!important;
    }
    #paradise-rp-hard-sidewall,#paradise-rp-hard-masks,.pr-mask,.prhud-cover{
      display:none!important;
      visibility:hidden!important;
      opacity:0!important;
      pointer-events:none!important;
    }
    html[data-pr-native-ui-off="1"] .nitro-toolbar,
    html[data-pr-native-ui-off="1"] [class*="nitro-toolbar" i],
    html[data-pr-native-ui-off="1"] [class*="toolbar-view" i],
    html[data-pr-native-ui-off="1"] [class*="toolbarview" i],
    html[data-pr-native-ui-off="1"] [class*="purse" i],
    html[data-pr-native-ui-off="1"] [class*="currency" i],
    html[data-pr-native-ui-off="1"] [class*="wallet" i],
    html[data-pr-native-ui-off="1"] [class*="infostand" i],
    html[data-pr-native-ui-off="1"] [class*="info-stand" i],
    html[data-pr-native-ui-off="1"] [class*="furni-infostand" i],
    html[data-pr-native-ui-off="1"] [class*="avatar-info" i],
    html[data-pr-native-ui-off="1"] [class*="user-profile" i],
    html[data-pr-native-ui-off="1"] [class*="me-menu" i],
    html[data-pr-native-ui-off="1"] [class*="friend-bar" i],
    html[data-pr-native-ui-off="1"] [class*="navigation-bar" i]{
      display:none!important;
      visibility:hidden!important;
      opacity:0!important;
      pointer-events:none!important;
    }
  `;

  const safeDoc = doc => !!(doc && doc.documentElement && doc.body);

  const docs = () => {
    const out = [];
    const add = doc => { if (safeDoc(doc) && !out.includes(doc)) out.push(doc); };
    add(document);
    try { if (window.parent && window.parent !== window) add(window.parent.document); } catch (_) {}
    for (const doc of [...out]) {
      try { doc.querySelectorAll('iframe').forEach(frame => { try { add(frame.contentDocument); } catch (_) {} }); } catch (_) {}
    }
    return out;
  };

  const installCss = doc => {
    if (!safeDoc(doc)) return;
    doc.documentElement.setAttribute('data-pr-native-ui-off', '1');
    let style = doc.getElementById(STYLE_ID);
    if (!style) {
      style = doc.createElement('style');
      style.id = STYLE_ID;
      (doc.head || doc.documentElement).appendChild(style);
    }
    if (style.textContent !== baseCss) style.textContent = baseCss;
  };

  const textOf = el => String(el?.innerText || el?.textContent || '').trim();
  const clsOf = el => String(el?.className || '').replace(/_/g, '-');

  const protectedNode = el => {
    if (!el || el === el.ownerDocument?.documentElement || el === el.ownerDocument?.body) return true;
    if (el.matches?.(OWN_SELECTOR) || el.closest?.(OWN_SELECTOR)) return true;
    if (['SCRIPT','STYLE','LINK','META','TITLE','BASE','NOSCRIPT','HEAD','IFRAME'].includes(el.tagName)) return true;
    if (el.id === 'root' || el.tagName === 'CANVAS') return true;
    return false;
  };

  const hasLargeCanvas = el => {
    if (!el || !el.querySelectorAll) return false;
    for (const canvas of el.querySelectorAll('canvas')) {
      const r = canvas.getBoundingClientRect();
      if (r.width > window.innerWidth * .28 && r.height > window.innerHeight * .28) return true;
    }
    return false;
  };

  const nativeText = text => /HabboVIP|habbovip|landing\.view|Promo|Qué hay|Que hay|¿Qué hay|Haz clic|chatear|Skate Urbano|Comprar uno|Dueñ|Dueno|Crédits|Credits|Pixels|Purse|Wallet/i.test(text || '');
  const nativeClass = el => /(menuButton-yNbz6|button-3IzmP|nitro-toolbar|toolbar-view|toolbarview|purse|currency|wallet|infostand|info-stand|furni-infostand|avatar-info|user-profile|me-menu|friend-bar|navigation-bar|habbo-toolbar|navigation)/i.test(clsOf(el));
  const nativeId = el => /^(CombatMode|PSVMode|TicketMode|NavigatorMode|FriendsMode|SettingsMode)$/i.test(String(el?.id || ''));

  const inOldZone = (el, r) => {
    const vw = window.innerWidth;
    const vh = window.innerHeight;
    const text = textOf(el);
    const topLeft = r.left < 345 && r.top < 170 && r.width < 355 && r.height < 180;
    const leftButtons = r.left < 120 && r.top > 55 && r.top < vh - 80 && r.width < 140 && r.height < 150;
    const bottomLeftChat = r.left < 545 && r.bottom > vh - 135 && r.width < 590 && r.height < 145;
    const bottomRightPhone = r.right > vw - 110 && r.bottom > vh - 110 && r.width < 125 && r.height < 125;
    const topRight = r.right > vw - 390 && r.top < 105 && r.width < 390 && r.height < 110;
    const objectPopup = r.right > vw - 430 && r.bottom > vh - 360 && r.width < 430 && r.height < 340 && nativeText(text);
    const promoText = r.top < 95 && r.left > 160 && r.right < vw - 160 && r.height < 95 && nativeText(text);
    return topLeft || leftButtons || bottomLeftChat || bottomRightPhone || topRight || objectPopup || promoText;
  };

  const isCandidate = (el, r) => {
    if (nativeId(el) || nativeClass(el)) return true;
    if (nativeText(textOf(el)) && inOldZone(el, r)) return true;
    return inOldZone(el, r) && nativeClass(el);
  };

  const kill = el => {
    if (protectedNode(el) || hasLargeCanvas(el)) return;
    try {
      el.classList.add(KILL_CLASS);
      el.setAttribute(KILL_ATTR, '1');
      el.style.setProperty('display', 'none', 'important');
      el.style.setProperty('visibility', 'hidden', 'important');
      el.style.setProperty('opacity', '0', 'important');
      el.style.setProperty('pointer-events', 'none', 'important');
    } catch (_) {}
  };

  const bestTarget = el => {
    let target = el;
    let current = el;
    for (let i = 0; i < 5; i++) {
      const parent = current?.parentElement;
      if (!parent || protectedNode(parent) || hasLargeCanvas(parent)) break;
      const r = parent.getBoundingClientRect();
      if (!r || r.width <= 2 || r.height <= 2) break;
      if (r.width > window.innerWidth * .80 || r.height > window.innerHeight * .88) break;
      if (!isCandidate(parent, r)) break;
      target = parent;
      current = parent;
    }
    return target;
  };

  const scanDoc = doc => {
    installCss(doc);
    try {
      doc.getElementById('paradise-rp-hard-sidewall')?.remove();
      doc.getElementById('paradise-rp-hard-masks')?.remove();
      doc.querySelectorAll('#CombatMode,#PSVMode,.menuButton-yNbz6_0,.button-3IzmP_0').forEach(el => kill(el));
    } catch (_) {}

    const nodes = Array.from(doc.body?.querySelectorAll('*') || []);
    for (const el of nodes) {
      if (protectedNode(el) || el.hasAttribute(KILL_ATTR)) continue;
      const r = el.getBoundingClientRect();
      if (!r || r.width <= 2 || r.height <= 2) continue;
      if (r.width > window.innerWidth * .80 || r.height > window.innerHeight * .88) continue;
      if (hasLargeCanvas(el)) continue;
      if (!isCandidate(el, r)) continue;
      kill(bestTarget(el));
    }
  };

  const scan = () => docs().forEach(scanDoc);
  const scheduleScan = () => window.requestAnimationFrame(() => { scan(); window.setTimeout(scan, 30); });

  const patchDoc = doc => {
    if (!safeDoc(doc) || doc.__paradiseNativeOffPatched) return;
    doc.__paradiseNativeOffPatched = true;
    try { new MutationObserver(scheduleScan).observe(doc.documentElement, { childList: true, subtree: true, attributes: true, characterData: true }); } catch (_) {}
  };

  const boot = () => {
    docs().forEach(doc => { installCss(doc); patchDoc(doc); });
    [0, 40, 90, 160, 280, 460, 760, 1200, 1900, 3000, 4600, 6500].forEach(ms => window.setTimeout(scan, ms));
    if (window.__paradiseNativeUiOffInterval) clearInterval(window.__paradiseNativeUiOffInterval);
    window.__paradiseNativeUiOffInterval = window.setInterval(() => { docs().forEach(patchDoc); scan(); }, 240);
  };

  window.__paradiseNativeUiOffScan = scan;
  window.__paradiseNativeUiOffVersion = VERSION;

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();