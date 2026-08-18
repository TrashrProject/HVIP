(() => {
  'use strict';

  const NATIVE_OFF_SRC = './paradise-native-ui-off.js?v=4';
  const HUD_SRC = './paradise-rp-hud.js?v=19';
  const HUD_CSS_SRC = './paradise-rp-hud.css?v=19';
  const MAX_LOADER_MS = 900;

  const addScript = (src, marker) => {
    try {
      document.querySelectorAll(`script[${marker}="1"]`).forEach(el => el.remove());
      const script = document.createElement('script');
      script.src = src;
      script.defer = true;
      script.setAttribute(marker, '1');
      (document.body || document.documentElement).appendChild(script);
      return script;
    } catch (_) { return null; }
  };

  const forceHudCss = () => {
    try {
      let link = document.getElementById('paradise-rp-hud-css');
      if (!link) {
        link = document.createElement('link');
        link.id = 'paradise-rp-hud-css';
        link.rel = 'stylesheet';
        (document.head || document.documentElement).appendChild(link);
      }
      if (!String(link.getAttribute('href') || '').includes('v=19')) link.href = HUD_CSS_SRC;
    } catch (_) {}
  };

  const loadEverything = () => {
    forceHudCss();
    addScript(NATIVE_OFF_SRC, 'data-paradise-native-ui-off');
    addScript(HUD_SRC, 'data-paradise-rp-hud');
    setTimeout(() => { try { window.__paradiseNativeUiOffScan?.(); } catch (_) {} }, 80);
    setTimeout(forceHudCss, 160);
    setTimeout(forceHudCss, 550);
    setTimeout(forceHudCss, 1300);
  };

  const hideNativeBlueLoader = () => {
    try {
      document.querySelectorAll('.nitro-loading,[class*="nitro-loading" i]').forEach(el => {
        if (el.id === 'paradise-loader' || el.closest?.('#paradise-loader')) return;
        el.style.setProperty('display', 'none', 'important');
        el.style.setProperty('visibility', 'hidden', 'important');
        el.style.setProperty('opacity', '0', 'important');
        el.style.setProperty('pointer-events', 'none', 'important');
      });
    } catch (_) {}
  };

  const removeParadiseLoader = () => {
    try {
      const loader = document.getElementById('paradise-loader');
      if (!loader) return;
      loader.style.setProperty('display', 'none', 'important');
      loader.style.setProperty('visibility', 'hidden', 'important');
      loader.style.setProperty('opacity', '0', 'important');
      loader.style.setProperty('pointer-events', 'none', 'important');
      loader.remove();
    } catch (_) {}
  };

  const setProgress = value => {
    try {
      const v = Math.round(Math.max(0, Math.min(100, value)));
      const bar = document.querySelector('.pr-loader-bar');
      const percent = document.querySelector('.pr-loader-percent');
      const status = document.querySelector('.pr-loader-status-copy');
      if (bar) bar.style.width = `${v}%`;
      if (percent) percent.textContent = `${v}%`;
      if (status) status.textContent = v >= 100 ? 'Bienvenue sur ParadiseRP' : 'Chargement du client';
    } catch (_) {}
  };

  const release = () => {
    setProgress(100);
    removeParadiseLoader();
    hideNativeBlueLoader();
    loadEverything();
  };

  const boot = () => {
    loadEverything();
    const started = performance.now();
    const tick = () => {
      const elapsed = performance.now() - started;
      setProgress(Math.min(99, 15 + elapsed / MAX_LOADER_MS * 84));
      if (elapsed >= MAX_LOADER_MS) return release();
      requestAnimationFrame(tick);
    };
    setTimeout(release, MAX_LOADER_MS + 120);
    setTimeout(release, MAX_LOADER_MS + 700);
    setTimeout(release, MAX_LOADER_MS + 1800);
    window.addEventListener('error', () => setTimeout(release, 60));
    window.addEventListener('unhandledrejection', () => setTimeout(release, 60));
    tick();
  };

  try {
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
    else boot();
  } catch (_) {
    release();
  }
})();
