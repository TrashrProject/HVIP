(() => {
  'use strict';

  const VERSION = '2.0.0-lock';
  const STYLE_ID = 'paradise-native-ui-off-style';
  const KILL_ATTR = 'data-pr-native-ui-killed';
  const KILL_CLASS = 'pr-native-ui-killed';
  const OWN_SELECTOR = '#paradise-rp-hud,#paradise-loader,#paradise-native-ui-off-style,#paradise-rp-hard-ui-killer-style,#paradise-rp-hard-sidewall,#paradise-rp-hard-masks';

  document.documentElement.setAttribute('data-pr-native-ui-off', '1');
  window.__PARADISE_NATIVE_UI_OFF_LOCK__ = VERSION;

  const css = `
    .${KILL_CLASS},[${KILL_ATTR}="1"]{
      display:none!important;
      visibility:hidden!important;
      opacity:0!important;
      pointer-events:none!important;
    }
    #paradise-rp-hard-sidewall,
    #paradise-rp-hard-masks,
    .pr-mask,
    .prhud-cover{
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

  const installCss = () => {
    let style = document.getElementById(STYLE_ID);
    if (!style) {
      style = document.createElement('style');
      style.id = STYLE_ID;
      (document.head || document.documentElement).appendChild(style);
    }
    if (style.textContent !== css) style.textContent = css;
  };

  const textOf = el => String(el?.innerText || el?.textContent || '').trim();
  const clsOf = el => String(el?.className || '').replace(/_/g, '-');

  const isProtected = el => {
    if (!el || el === document.documentElement || el === document.body) return true;
    if (el.matches?.(OWN_SELECTOR) || el.closest?.(OWN_SELECTOR)) return true;
    if (['SCRIPT','STYLE','LINK','META','TITLE','BASE','NOSCRIPT','HEAD'].includes(el.tagName)) return true;
    if (el.id === 'root' || el.tagName === 'CANVAS') return true;
    return false;
  };

  const hasLargeCanvas = el => {
    if (!el || !el.querySelectorAll) return false;
    for (const canvas of el.querySelectorAll('canvas')) {
      const r = canvas.getBoundingClientRect();
      if (r.width > window.innerWidth * .30 && r.height > window.innerHeight * .30) return true;
    }
    return false;
  };

  const nativeText = text => /HabboVIP|habbovip|landing\.view|Promo|Qué hay|Que hay|¿Qué hay|Haz clic|chatear|Skate Urbano|Comprar uno|Dueñ|Dueno|Crédits|Credits|Pixels|Inventaire|Inventory|Boutique|Shop|Métier|Metier|Carte|Accueil|Purse|Wallet/i.test(text || '');
  const nativeClass = el => /(nitro-toolbar|toolbar-view|toolbarview|toolbar|purse|currency|wallet|infostand|info-stand|furni-infostand|avatar-info|user-profile|me-menu|friend-bar|navigation-bar|habbo-toolbar|navigation)/i.test(clsOf(el));

  const looksNativeByZone = (el, r) => {
    const vw = window.innerWidth;
    const vh = window.innerHeight;
    const text = textOf(el);

    const topLeftProfile = r.left < 345 && r.top < 160 && r.width < 350 && r.height < 170;
    const leftRail = r.left < 118 && r.top > 40 && r.top < vh - 85 && r.width < 135 && r.height < vh - 90;
    const bottomLeftOldChat = r.left < 545 && r.bottom > vh - 132 && r.width < 585 && r.height < 140;
    const bottomRightPhone = r.right > vw - 105 && r.bottom > vh - 105 && r.width < 120 && r.height < 120;
    const topRightPurse = r.right > vw - 375 && r.top < 98 && r.width < 380 && r.height < 105;
    const rightObjectPopup = r.right > vw - 395 && r.bottom > vh - 340 && r.width < 405 && r.height < 320;
    const hotelPromoText = r.top < 95 && r.left > 200 && r.right < vw - 200 && r.height < 90 && nativeText(text);

    return topLeftProfile || leftRail || bottomLeftOldChat || bottomRightPhone || topRightPurse || rightObjectPopup || hotelPromoText;
  };

  const isNativeCandidate = (el, r) => {
    if (nativeClass(el)) return true;
    if (nativeText(textOf(el))) return true;
    return looksNativeByZone(el, r);
  };

  const kill = el => {
    if (isProtected(el) || hasLargeCanvas(el)) return;
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
    for (let i = 0; i < 6; i++) {
      const parent = current?.parentElement;
      if (!parent || isProtected(parent) || hasLargeCanvas(parent)) break;
      const r = parent.getBoundingClientRect();
      if (!r || r.width <= 2 || r.height <= 2) break;
      if (r.width > window.innerWidth * .76 || r.height > window.innerHeight * .84) break;
      if (!isNativeCandidate(parent, r)) break;
      target = parent;
      current = parent;
    }
    return target;
  };

  const scan = () => {
    installCss();
    document.getElementById('paradise-rp-hard-sidewall')?.remove();
    document.getElementById('paradise-rp-hard-masks')?.remove();

    if (!document.body) return;
    const nodes = Array.from(document.body.querySelectorAll('*'));
    for (const el of nodes) {
      if (isProtected(el) || el.hasAttribute(KILL_ATTR) || el.hasAttribute('data-prhud-hard-kill')) continue;
      const r = el.getBoundingClientRect();
      if (!r || r.width <= 2 || r.height <= 2) continue;
      if (r.width > window.innerWidth * .78 || r.height > window.innerHeight * .86) continue;
      if (hasLargeCanvas(el)) continue;
      if (!isNativeCandidate(el, r)) continue;
      kill(bestTarget(el));
    }
  };

  const scheduleScan = () => {
    window.requestAnimationFrame(() => {
      scan();
      window.setTimeout(scan, 35);
    });
  };

  const patchInsertion = name => {
    const native = Element.prototype[name];
    if (!native || native.__paradiseNativeOffPatched) return;
    const patched = function(...args) {
      const result = native.apply(this, args);
      scheduleScan();
      return result;
    };
    patched.__paradiseNativeOffPatched = true;
    Element.prototype[name] = patched;
  };

  const boot = () => {
    installCss();
    patchInsertion('appendChild');
    patchInsertion('insertBefore');
    patchInsertion('replaceChild');

    try {
      new MutationObserver(scheduleScan).observe(document.documentElement, {
        childList: true,
        subtree: true,
        attributes: true,
        characterData: true
      });
    } catch (_) {}

    [0, 60, 140, 260, 420, 700, 1100, 1700, 2600, 3900, 5600].forEach(ms => window.setTimeout(scan, ms));

    if (window.__paradiseNativeUiOffInterval) clearInterval(window.__paradiseNativeUiOffInterval);
    window.__paradiseNativeUiOffInterval = window.setInterval(scan, 320);
  };

  window.__paradiseNativeUiOffScan = scan;
  window.__paradiseNativeUiOffVersion = VERSION;

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
