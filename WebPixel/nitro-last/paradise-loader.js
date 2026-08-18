(() => {
  'use strict';

  const HUD_SRC = './paradise-rp-hud.js?v=12';
  const KILLER_SRC = './paradise-hard-ui-killer.js?v=2';
  const SOFT_HIDE_MS = 1700;
  const HARD_HIDE_MS = 2600;

  const loadScript = (src, attr) => {
    document.querySelectorAll(`script[${attr}="1"]`).forEach(script => script.remove());
    const script = document.createElement('script');
    script.src = src;
    script.defer = true;
    script.setAttribute(attr, '1');
    document.body.appendChild(script);
  };

  const loadHud = () => loadScript(HUD_SRC, 'data-paradise-rp-hud');
  const loadKiller = () => loadScript(KILLER_SRC, 'data-paradise-rp-killer');

  const boot = () => {
    loadHud();

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
      window.setTimeout(() => {
        hideNativeBlueLoader();
        loadHud();
        loadKiller();
      }, 400);
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
      return root.childElementCount > 0 && performance.now() - started > 1400;
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

      window.setTimeout(() => {
        try { loader.remove(); } catch (_) {}
        loadHud();
      }, 120);

      window.setTimeout(() => {
        hideNativeBlueLoader();
        loadHud();
        loadKiller();
      }, 650);

      window.setTimeout(() => {
        hideNativeBlueLoader();
        loadHud();
        loadKiller();
      }, 1400);
    };

    const tick = () => {
      if (hidden) return;

      const elapsed = performance.now() - started;
      let target = 18;
      if (elapsed > 250) target = 34;
      if (elapsed > 550) target = 52;
      if (elapsed > 900) target = 74;
      if (elapsed > 1250) target = 92;
      if (elapsed > 1550) target = 99;

      render(shown + Math.max(1, (target - shown) * 0.22));

      if ((elapsed > SOFT_HIDE_MS && hasGameSurface()) || elapsed > HARD_HIDE_MS) {
        hide();
        return;
      }

      window.requestAnimationFrame(tick);
    };

    try {
      new MutationObserver(() => {
        if (!hidden && performance.now() - started > SOFT_HIDE_MS && hasGameSurface()) hide();
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
    try { loadHud(); } catch (__) {}
  }
})();
