(() => {
  'use strict';

  const STYLE_ID = 'paradise-native-ui-off-style';
  const KILL_ATTR = 'data-pr-native-ui-killed';
  const KILL_CLASS = 'pr-native-ui-killed';
  const OWN = '#paradise-rp-hud,#paradise-loader,#paradise-native-ui-off-style,#paradise-rp-hard-ui-killer-style,#paradise-rp-hard-sidewall';
  const START = performance.now();

  document.documentElement.setAttribute('data-pr-native-ui-off', '1');

  const css = `
    .${KILL_CLASS},[${KILL_ATTR}="1"]{
      display:none!important;visibility:hidden!important;opacity:0!important;pointer-events:none!important;
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
      display:none!important;visibility:hidden!important;opacity:0!important;pointer-events:none!important;
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

  const isOwn = el => {
    if (!el || el === document.documentElement || el === document.body) return true;
    if (el.matches?.(OWN) || el.closest?.(OWN)) return true;
    return ['SCRIPT','STYLE','LINK','META','TITLE','BASE','NOSCRIPT'].includes(el.tagName);
  };

  const hasLargeCanvas = el => {
    if (!el || !el.querySelectorAll) return false;
    for (const canvas of el.querySelectorAll('canvas')) {
      const r = canvas.getBoundingClientRect();
      if (r.width > window.innerWidth * .34 && r.height > window.innerHeight * .34) return true;
    }
    return false;
  };

  const textOf = el => String(el?.innerText || el?.textContent || '').trim();
  const textLooksNative = text => /HabboVIP|habbovip|landing\.view|Qué hay|Que hay|¿Qué hay|Haz clic|chatear|Skate Urbano|Comprar uno|Dueñ|Dueno|Créditos|Credits|Pixels|Inventaire|Inventory|Boutique|Shop|Métier|Carte|Accueil|Purse|Wallet/i.test(text);
  const classLooksNative = el => /(^|\s)(nitro-toolbar|toolbar|purse|currency|wallet|infostand|info-stand|furni-infostand|avatar-info|user-profile|me-menu|friend-bar|navigation-bar)(\s|$)/i.test(String(el.className || '').replace(/_/g, '-'));

  const inNativeZone = (el, r) => {
    const vw = window.innerWidth;
    const vh = window.innerHeight;
    const text = textOf(el);

    const topLeft = r.left < 330 && r.top < 150 && r.width < 340 && r.height < 160;
    const leftRail = r.left < 112 && r.top > 42 && r.bottom < vh - 38 && r.width < 128 && r.height < vh - 95;
    const bottomLeft = r.left < 520 && r.bottom > vh - 128 && r.width < 560 && r.height < 135;
    const bottomPhone = r.right > vw - 98 && r.bottom > vh - 100 && r.width < 112 && r.height < 112;
    const topRight = r.right > vw - 365 && r.top < 92 && r.width < 365 && r.height < 98;
    const rightPopup = r.right > vw - 380 && r.bottom > vh - 320 && r.width < 385 && r.height < 280;
    const hotelPromo = r.top < 90 && r.left > 210 && r.right < vw - 210 && r.height < 82 && textLooksNative(text);

    return topLeft || leftRail || bottomLeft || bottomPhone || topRight || rightPopup || hotelPromo || classLooksNative(el);
  };

  const kill = el => {
    if (!el || isOwn(el) || el.id === 'root' || el.tagName === 'CANVAS' || hasLargeCanvas(el)) return;
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
      if (!parent || parent.id === 'root' || parent === document.body || isOwn(parent) || hasLargeCanvas(parent)) break;
      const r = parent.getBoundingClientRect();
      if (!r || r.width <= 2 || r.height <= 2) break;
      if (r.width > window.innerWidth * .72 || r.height > window.innerHeight * .72) break;
      if (!inNativeZone(parent, r) && !textLooksNative(textOf(parent))) break;
      target = parent;
      current = parent;
    }
    return target;
  };

  const scan = () => {
    installCss();
    if (!document.body) return;
    const nodes = Array.from(document.body.querySelectorAll('*'));
    for (const el of nodes) {
      if (isOwn(el)) continue;
      if (el.id === 'root' || el.tagName === 'CANVAS') continue;
      if (el.hasAttribute(KILL_ATTR)) continue;
      const r = el.getBoundingClientRect();
      if (!r || r.width <= 2 || r.height <= 2) continue;
      if (r.width > window.innerWidth * .76 || r.height > window.innerHeight * .82) continue;
      if (hasLargeCanvas(el)) continue;
      if (!inNativeZone(el, r) && !textLooksNative(textOf(el))) continue;
      kill(bestTarget(el));
    }
  };

  const scheduleScan = () => {
    requestAnimationFrame(() => {
      scan();
      setTimeout(scan, 40);
    });
  };

  const patchInsertion = (name) => {
    const native = Element.prototype[name];
    if (!native || native.__paradisePatched) return;
    const patched = function(...args) {
      const result = native.apply(this, args);
      scheduleScan();
      return result;
    };
    patched.__paradisePatched = true;
    Element.prototype[name] = patched;
  };

  installCss();
  patchInsertion('appendChild');
  patchInsertion('insertBefore');
  patchInsertion('replaceChild');

  const boot = () => {
    scan();
    try {
      new MutationObserver(scheduleScan).observe(document.body, { childList: true, subtree: true, attributes: true, characterData: true });
    } catch (_) {}
    [80,180,360,700,1100,1700,2600,3800,5200].forEach(ms => setTimeout(scan, ms));
    if (window.__paradiseNativeUiOff) clearInterval(window.__paradiseNativeUiOff);
    window.__paradiseNativeUiOff = setInterval(scan, 220);
    setTimeout(() => { if (performance.now() - START > 9000) clearInterval(window.__paradiseNativeUiOff); }, 12000);
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
