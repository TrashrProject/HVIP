(() => {
  'use strict';

  const STYLE_ID = 'paradise-rp-hard-ui-killer-style';
  const KILL_CLASS = 'prhud-native-hard-kill';
  const KILL_ATTR = 'data-prhud-hard-kill';
  const OWN = '#paradise-rp-hud,#paradise-loader,#paradise-native-ui-off-style,#paradise-rp-hard-ui-killer-style';

  const css = `
    .${KILL_CLASS},[${KILL_ATTR}="1"]{
      display:none!important;visibility:hidden!important;opacity:0!important;pointer-events:none!important;
    }
  `;

  const installStyle = () => {
    let style = document.getElementById(STYLE_ID);
    if (!style) {
      style = document.createElement('style');
      style.id = STYLE_ID;
      document.head.appendChild(style);
    }
    if (style.textContent !== css) style.textContent = css;
    document.getElementById('paradise-rp-hard-sidewall')?.remove();
  };

  const hasRootCanvas = () => {
    const canvas = document.querySelector('#root canvas');
    if (!canvas) return false;
    const r = canvas.getBoundingClientRect();
    return r.width > window.innerWidth * .30 && r.height > window.innerHeight * .30;
  };

  const isOwn = el => {
    if (!el || el === document.body || el === document.documentElement) return true;
    if (el.matches?.(OWN) || el.closest?.(OWN)) return true;
    return ['SCRIPT','STYLE','LINK','META','TITLE','BASE','NOSCRIPT'].includes(el.tagName);
  };

  const hasLargeCanvas = el => {
    if (!el || !el.querySelectorAll) return false;
    for (const canvas of el.querySelectorAll('canvas')) {
      const r = canvas.getBoundingClientRect();
      if (r.width > window.innerWidth * .30 && r.height > window.innerHeight * .30) return true;
    }
    return false;
  };

  const textOf = el => String(el?.innerText || el?.textContent || '').trim();
  const nativeText = text => /HabboVIP|habbovip|landing\.view|Qué hay|Que hay|¿Qué hay|Haz clic|chatear|Skate Urbano|Comprar uno|Dueñ|Dueno|Créditos|Credits|Pixels|Inventaire|Inventory|Boutique|Shop|Métier|Carte|Accueil|Purse|Wallet/i.test(text);
  const nativeClass = el => /(toolbar|toolbarview|purse|currency|wallet|infostand|info-stand|furni-infostand|avatar-info|user-profile|me-menu|friend-bar|navigation-bar|nitro-toolbar)/i.test(String(el.className || '').replace(/_/g, '-'));

  const isNativeUi = (el, r) => {
    const vw = window.innerWidth;
    const vh = window.innerHeight;
    const text = textOf(el);

    const topLeft = r.left < 330 && r.top < 150 && r.width < 340 && r.height < 160;
    const leftRail = r.left < 112 && r.top > 42 && r.bottom < vh - 40 && r.width < 130 && r.height < vh - 95;
    const bottomLeft = r.left < 520 && r.bottom > vh - 128 && r.width < 560 && r.height < 135;
    const bottomPhone = r.right > vw - 98 && r.bottom > vh - 100 && r.width < 112 && r.height < 112;
    const topRight = r.right > vw - 365 && r.top < 92 && r.width < 365 && r.height < 98;
    const rightPopup = r.right > vw - 385 && r.bottom > vh - 330 && r.width < 390 && r.height < 300;
    const hotelPromo = r.top < 90 && r.left > 210 && r.right < vw - 210 && r.height < 84 && nativeText(text);

    return nativeClass(el) || nativeText(text) || topLeft || leftRail || bottomLeft || bottomPhone || topRight || rightPopup || hotelPromo;
  };

  const hardKill = el => {
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
      if (!parent || parent === document.body || parent.id === 'root' || isOwn(parent) || hasLargeCanvas(parent)) break;
      const r = parent.getBoundingClientRect();
      if (!r || r.width <= 2 || r.height <= 2) break;
      if (r.width > window.innerWidth * .74 || r.height > window.innerHeight * .80) break;
      if (!isNativeUi(parent, r)) break;
      target = parent;
      current = parent;
    }
    return target;
  };

  const pass = () => {
    if (!hasRootCanvas() && performance.now() < 3600) return;
    installStyle();

    for (const el of Array.from(document.querySelectorAll('body *'))) {
      if (isOwn(el)) continue;
      if (el.id === 'root' || el.tagName === 'CANVAS') continue;
      if (el.hasAttribute(KILL_ATTR) || el.hasAttribute('data-pr-native-ui-killed')) continue;

      const r = el.getBoundingClientRect();
      if (!r || r.width <= 2 || r.height <= 2) continue;
      if (r.width > window.innerWidth * .76 || r.height > window.innerHeight * .82) continue;
      if (hasLargeCanvas(el)) continue;
      if (!isNativeUi(el, r)) continue;

      hardKill(bestTarget(el));
    }
  };

  installStyle();
  [250,600,1000,1600,2400,3600].forEach(ms => window.setTimeout(pass, ms));

  if (window.__paradiseHardUiKiller) clearInterval(window.__paradiseHardUiKiller);
  window.__paradiseHardUiKiller = window.setInterval(pass, 180);
  window.setTimeout(() => clearInterval(window.__paradiseHardUiKiller), 16000);
})();
