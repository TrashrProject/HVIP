(() => {
  'use strict';

  const NATIVE_OFF_SRC = './paradise-native-ui-off.js?v=3';
  const HUD_SRC = './paradise-rp-hud.js?v=18';
  const KILLER_SRC = './paradise-hard-ui-killer.js?v=4';
  const HUD_CSS_SRC = './paradise-rp-hud.css?v=18';
  const SOFT_HIDE_MS = 1350;
  const HARD_HIDE_MS = 2350;

  const forceHudCss = () => {
    let link = document.getElementById('paradise-rp-hud-css');
    if (!link) {
      link = document.createElement('link');
      link.id = 'paradise-rp-hud-css';
      link.rel = 'stylesheet';
      (document.head || document.documentElement).appendChild(link);
    }
    if (!String(link.getAttribute('href') || '').includes('v=18')) link.href = HUD_CSS_SRC;
  };

  const loadScript = (src, attr) => {
    document.querySelectorAll(`script[${attr}="1"]`).forEach(script => script.remove());
    const script = document.createElement('script');
    script.src = src;
    script.defer = true;
    script.setAttribute(attr, '1');
    document.body.appendChild(script);
  };

  const loadNativeOff = () => loadScript(NATIVE_OFF_SRC, 'data-paradise-native-ui-off');
  const loadHud = () => { forceHudCss(); loadScript(HUD_SRC, 'data-paradise-rp-hud'); };
  const loadKiller = () => loadScript(KILLER_SRC, 'data-paradise-rp-killer');
  const rescanNative = () => { try { window.__paradiseNativeUiOffScan?.(); } catch (_) {} };

  const boot = () => {
    loadNativeOff();
    loadHud();

    const cssKeeper = window.setInterval(() => { forceHudCss(); rescanNative(); }, 250);
    window.setTimeout(() => window.clearInterval(cssKeeper), 9000);

    const loader = document.getElementById('paradise-loader');
    const bar = document.querySelector('.pr-loader-bar');
    const percent = document.querySelector('.pr-loader-percent');
    const status = document.querySelector('.pr-loader-status-copy');

    const hideNativeBlueLoader = () => {
      document.querySelectorAll('.nitro-loading,[class*="nitro-loading"]').forEach(el => {
        if (el === loader || loader?.contains(el)) return;
        el.style.setProperty('display', 'none', 'important');
        el.style.setProperty('opacity', '0', 'important');
        el.style.setProperty('visibility', 'hidden', 'important');
        el.style.setProperty('pointer-events', 'none', 'important');
      });
    };

    if (!loader) {
      window.setTimeout(() => { hideNativeBlueLoader(); loadNativeOff(); loadHud(); loadKiller(); rescanNative(); }, 350);
      return;
    }

    let hidden = false;
    const started = performance.now();
    let shown = 8;

    const render = value => {
      shown = Math.max(shown, Math.min(100, value));
      const v = Math.round(shown);
      if (bar) bar.style.width = `${v}%`;
      if (percent) percent.textContent = `${v}%`;
      if (status) {
        if (v < 35) status.textContent = 'Ouverture de ParadiseRP';
        else if (v < 65) status.textContent = 'Chargement du monde';
        else if (v < 92) status.textContent = 'Préparation du HUD RP';
        else status.textContent = 'Bienvenue sur ParadiseRP';
      }
    };

    const hasGameSurface = () => {
      const root = document.getElementById('root');
      if (!root) return false;
      if (root.querySelector('canvas')) return true;
      return root.childElementCount > 0 && performance.now() - started > 1200;
    };

    const hide = () => {
      if (hidden) return;
      hidden = true;
      render(100);
      hideNativeBlueLoader();
      loader.style.setProperty('opacity', '0', 'important');
      loader.style.setProperty('visibility', 'hidden', 'important');
      loader.style.setProperty('pointer-events', 'none', 'important');
      loader.classList.add('is-hidden');

      const after = delay => window.setTimeout(() => {
        try { loader.remove(); } catch (_) {}
        hideNativeBlueLoader();
        loadNativeOff();
        loadHud();
        loadKiller();
        rescanNative();
      }, delay);
      after(120); after(650); after(1350); after(2600);
    };

    const tick = () => {
      if (hidden) return;
      const elapsed = performance.now() - started;
      let target = 18;
      if (elapsed > 240) target = 34;
      if (elapsed > 520) target = 55;
      if (elapsed > 850) target = 76;
      if (elapsed > 1180) target = 92;
      if (elapsed > 1500) target = 99;
      render(shown + Math.max(1, (target - shown) * .24));
      if ((elapsed > SOFT_HIDE_MS && hasGameSurface()) || elapsed > HARD_HIDE_MS) return hide();
      window.requestAnimationFrame(tick);
    };

    try {
      new MutationObserver(() => {
        if (!hidden && performance.now() - started > SOFT_HIDE_MS && hasGameSurface()) hide();
        else rescanNative();
      }).observe(document.body, { childList: true, subtree: true });
    } catch (_) {}

    window.setTimeout(hide, HARD_HIDE_MS + 250);
    render(shown);
    window.requestAnimationFrame(tick);
  };

  try {
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
    else boot();
  } catch (_) {
    try { document.getElementById('paradise-loader')?.remove(); } catch (__) {}
    try { loadNativeOff(); loadHud(); loadKiller(); } catch (__) {}
  }
})();