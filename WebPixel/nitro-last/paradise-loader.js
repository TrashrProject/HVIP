(() => {
  'use strict';

  const NATIVE_OFF_SRC = './paradise-native-ui-off.js?v=4';
  const HUD_SRC = './paradise-rp-hud.js?v=20';
  const HUD_CSS_SRC = './paradise-rp-hud.css?v=20';

  let released = false;

  const addScript = (src, marker) => {
    try {
      if (document.querySelector(`script[${marker}="1"]`)) return;
      const script = document.createElement('script');
      script.src = src;
      script.defer = true;
      script.setAttribute(marker, '1');
      (document.body || document.documentElement).appendChild(script);
    } catch (_) {}
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
      link.href = HUD_CSS_SRC;
    } catch (_) {}
  };

  const killLoader = () => {
    try {
      const loader = document.getElementById('paradise-loader');
      if (loader) {
        loader.style.setProperty('display', 'none', 'important');
        loader.style.setProperty('visibility', 'hidden', 'important');
        loader.style.setProperty('opacity', '0', 'important');
        loader.style.setProperty('pointer-events', 'none', 'important');
        loader.remove();
      }
      document.querySelectorAll('.nitro-loading,[class*="nitro-loading"]').forEach(el => {
        if (el.id === 'paradise-loader' || el.closest?.('#paradise-loader')) return;
        el.style.setProperty('display', 'none', 'important');
        el.style.setProperty('visibility', 'hidden', 'important');
        el.style.setProperty('opacity', '0', 'important');
        el.style.setProperty('pointer-events', 'none', 'important');
      });
    } catch (_) {}
  };

  const bootHud = () => {
    forceHudCss();
    addScript(NATIVE_OFF_SRC, 'data-paradise-native-ui-off');
    addScript(HUD_SRC, 'data-paradise-rp-hud');
    try { window.__paradiseNativeUiOffScan?.(); } catch (_) {}
  };

  const release = () => {
    if (released) return;
    released = true;
    killLoader();
    bootHud();
  };

  const start = () => {
    bootHud();
    setTimeout(release, 250);
    setTimeout(release, 700);
    setTimeout(release, 1300);
    setInterval(() => {
      killLoader();
      bootHud();
    }, 1500);
  };

  try {
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', start, { once: true });
    else start();
  } catch (_) {
    release();
  }
})();
