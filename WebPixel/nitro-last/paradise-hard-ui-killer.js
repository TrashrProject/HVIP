(() => {
  'use strict';

  const STYLE_ID = 'paradise-rp-hard-ui-killer-style';
  const VERSION = '4.0.0-lock';

  window.__paradiseHardUiKillerVersion = VERSION;

  const installStyle = () => {
    let style = document.getElementById(STYLE_ID);
    if (!style) {
      style = document.createElement('style');
      style.id = STYLE_ID;
      (document.head || document.documentElement).appendChild(style);
    }
    style.textContent = `
      #paradise-rp-hard-sidewall,
      #paradise-rp-hard-masks,
      .pr-mask,
      .prhud-cover,
      .prhud-native-hard-kill,
      [data-prhud-hard-kill="1"]{
        display:none!important;
        visibility:hidden!important;
        opacity:0!important;
        pointer-events:none!important;
      }
    `;
    document.getElementById('paradise-rp-hard-sidewall')?.remove();
    document.getElementById('paradise-rp-hard-masks')?.remove();
  };

  const fallbackScan = () => {
    installStyle();
    if (typeof window.__paradiseNativeUiOffScan === 'function') {
      window.__paradiseNativeUiOffScan();
      return;
    }

    const isOwn = el => {
      if (!el || el === document.body || el === document.documentElement) return true;
      if (el.closest?.('#paradise-rp-hud,#paradise-loader')) return true;
      if (['SCRIPT','STYLE','LINK','META','TITLE','BASE','NOSCRIPT','HEAD'].includes(el.tagName)) return true;
      if (el.id === 'root' || el.tagName === 'CANVAS') return true;
      return false;
    };

    const hasLargeCanvas = el => {
      if (!el?.querySelectorAll) return false;
      for (const canvas of el.querySelectorAll('canvas')) {
        const r = canvas.getBoundingClientRect();
        if (r.width > window.innerWidth * .30 && r.height > window.innerHeight * .30) return true;
      }
      return false;
    };

    const nativeText = text => /HabboVIP|habbovip|landing\.view|Qué hay|Que hay|¿Qué hay|Haz clic|chatear|Skate Urbano|Comprar uno|Crédits|Credits|Pixels|Inventaire|Inventory|Boutique|Shop|Métier|Metier|Carte|Accueil/i.test(text || '');
    const nativeClass = el => /(nitro-toolbar|toolbar|purse|currency|wallet|infostand|avatar-info|user-profile|me-menu|friend-bar|navigation-bar)/i.test(String(el.className || '').replace(/_/g, '-'));

    const nativeZone = r => {
      const vw = window.innerWidth;
      const vh = window.innerHeight;
      return (r.left < 345 && r.top < 160 && r.width < 350 && r.height < 170) ||
        (r.left < 118 && r.top > 40 && r.top < vh - 85 && r.width < 135 && r.height < vh - 90) ||
        (r.left < 545 && r.bottom > vh - 132 && r.width < 585 && r.height < 140) ||
        (r.right > vw - 105 && r.bottom > vh - 105 && r.width < 120 && r.height < 120) ||
        (r.right > vw - 375 && r.top < 98 && r.width < 380 && r.height < 105);
    };

    for (const el of Array.from(document.body?.querySelectorAll('*') || [])) {
      if (isOwn(el) || hasLargeCanvas(el)) continue;
      const r = el.getBoundingClientRect();
      if (!r || r.width <= 2 || r.height <= 2) continue;
      if (r.width > window.innerWidth * .78 || r.height > window.innerHeight * .86) continue;
      if (!nativeClass(el) && !nativeText(el.innerText || el.textContent || '') && !nativeZone(r)) continue;
      try {
        el.setAttribute('data-prhud-hard-kill', '1');
        el.style.setProperty('display', 'none', 'important');
        el.style.setProperty('visibility', 'hidden', 'important');
        el.style.setProperty('opacity', '0', 'important');
        el.style.setProperty('pointer-events', 'none', 'important');
      } catch (_) {}
    }
  };

  installStyle();
  [80, 180, 360, 700, 1200, 2000, 3200, 5000].forEach(ms => window.setTimeout(fallbackScan, ms));

  if (window.__paradiseHardUiKiller) clearInterval(window.__paradiseHardUiKiller);
  window.__paradiseHardUiKiller = window.setInterval(fallbackScan, 420);
})();
