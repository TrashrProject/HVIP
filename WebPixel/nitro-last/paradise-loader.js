(() => {
  'use strict';

  const NATIVE_OFF_SRC = './paradise-native-ui-off.js?v=3';
  const HUD_SRC = './paradise-rp-hud.js?v=18';
  const KILLER_SRC = './paradise-hard-ui-killer.js?v=4';
  const HUD_CSS_SRC = './paradise-rp-hud.css?v=18';
  const MAX_LOADER_MS = 1900;

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
    try {
      document.querySelectorAll(`script[${attr}="1"]`).forEach(script => script.remove());
      const script = document.createElement('script');
      script.src = src;
      script.defer = true;
      script.setAttribute(attr, '1');
      (document.body || document.documentElement).appendChild(script);
      return script;
    } catch (_) {
      return null;
    }
  };

  const loadNativeOff = () => loadScript(NATIVE_OFF_SRC, 'data-paradise-native-ui-off');
  const loadHud = () => {
    forceHudCss();
    loadScript(HUD_SRC, 'data-paradise-rp-hud');
  };
  const loadKiller = () => loadScript(KILLER_SRC, 'data-paradise-rp-killer');
  const rescanNative = () => { try { window.__paradiseNativeUiOffScan?.(); } catch (_) {} };

  const setProgress = value => {
    const v = Math.max(0, Math.min(100, Math.round(value)));
    const bar = document.querySelector('.pr-loader-bar');
    const percent = document.querySelector('.pr-loader-percent');
    const status = document.querySelector('.pr-loader-status-copy');
    if (bar) bar.style.width = `${v}%`;
    if (percent) percent.textContent = `${v}%`;
    if (status) {
      if (v < 35) status.textContent = 'Ouverture de ParadiseRP';
      else if (v < 70) status.textContent = 'Chargement du monde';
      else if (v < 98) status.textContent = 'Préparation du HUD RP';
      else status.textContent = 'Bienvenue sur ParadiseRP';
    }
  };

  const hideNativeBlueLoader = () => {
    try {
      document.querySelectorAll('.nitro-loading,[class*="nitro-loading" i]').forEach(el => {
        if (el.id === 'paradise-loader' || el.closest?.('#paradise-loader')) return;
        el.style.setProperty('display', 'none', 'important');
        el.style.setProperty('opacity', '0', 'important');
        el.style.setProperty('visibility', 'hidden', 'important');
        el.style.setProperty('pointer-events', 'none', 'important');
      });
    } catch (_) {}
  };

  const removeLoaderNow = () => {
    try {
      const loader = document.getElementById('paradise-loader');
      if (!loader) return;
      loader.style.setProperty('display', 'none', 'important');
      loader.style.setProperty('opacity', '0', 'important');
      loader.style.setProperty('visibility', 'hidden', 'important');
      loader.style.setProperty('pointer-events', 'none', 'important');
      loader.classList.add('is-hidden');
      loader.remove();
    } catch (_) {}
  };

  const finalBoot = () => {
    removeLoaderNow();
    hideNativeBlueLoader();
    loadNativeOff();
    loadHud();
    window.setTimeout(loadKiller, 120);
    [80, 220, 480, 900, 1500, 2600, 4200, 6500].forEach(ms => {
      window.setTimeout(() => {
        forceHudCss();
        hideNativeBlueLoader();
        rescanNative();
      }, ms);
    });
  };

  const boot = () => {
    // On charge le HUD tout de suite, mais on ne laisse plus jamais le loader décider tout seul.
    forceHudCss();
    loadHud();

    const started = performance.now();
    let done = false;

    const finish = () => {
      if (done) return;
      done = true;
      setProgress(100);
      finalBoot();
    };

    const tick = () => {
      if (done) return;
      const elapsed = performance.now() - started;
      const progress = Math.min(99, 12 + elapsed / MAX_LOADER_MS * 88);
      setProgress(progress);
      if (elapsed >= MAX_LOADER_MS) return finish();
      window.requestAnimationFrame(tick);
    };

    // Double sécurité : même si une erreur visuelle arrive, le loader disparaît.
    window.setTimeout(finish, MAX_LOADER_MS + 120);
    window.setTimeout(finalBoot, MAX_LOADER_MS + 650);
    window.setTimeout(finalBoot, MAX_LOADER_MS + 1800);
    window.addEventListener('error', () => window.setTimeout(finalBoot, 60));
    window.addEventListener('unhandledrejection', () => window.setTimeout(finalBoot, 60));

    tick();
  };

  try {
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
    else boot();
  } catch (_) {
    finalBoot();
  }
})();