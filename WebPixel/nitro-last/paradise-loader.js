(() => {
  'use strict';

  const HUD_SRC = './paradise-rp-hud.js?v=7';
  const MAX_BLOCK_MS = 2600;
  const HARD_KILL_MS = 4200;

  const loadHud = () => {
    document.querySelectorAll('script[data-paradise-rp-hud="1"]').forEach(script => script.remove());
    const script = document.createElement('script');
    script.src = HUD_SRC;
    script.defer = true;
    script.dataset.paradiseRpHud = '1';
    document.body.appendChild(script);
  };

  const boot = () => {
    const loader = document.getElementById('paradise-loader');
    const bar = document.querySelector('.pr-loader-bar');
    const percent = document.querySelector('.pr-loader-percent');
    const status = document.querySelector('.pr-loader-status-copy');

    loadHud();

    if (!loader) return;

    let hidden = false;
    let shown = 8;
    const started = performance.now();

    const render = value => {
      if (hidden) return;
      shown = Math.max(shown, Math.min(100, value));
      const v = Math.round(shown);
      if (bar) bar.style.width = v + '%';
      if (percent) percent.textContent = v + '%';
      if (status) {
        if (v < 35) status.textContent = 'Ouverture de ParadiseRP';
        else if (v < 65) status.textContent = 'Chargement du moteur de jeu';
        else if (v < 92) status.textContent = 'Préparation de votre environnement';
        else status.textContent = 'Bienvenue sur ParadiseRP';
      }
    };

    const forceRemoveNativeBlueLoader = () => {
      document.querySelectorAll('.nitro-loading,[class*="nitro-loading"]').forEach(el => {
        if (el === loader || loader.contains(el)) return;
        el.style.setProperty('opacity', '0', 'important');
        el.style.setProperty('visibility', 'hidden', 'important');
        el.style.setProperty('pointer-events', 'none', 'important');
      });
    };

    const hide = () => {
      if (hidden) return;
      hidden = true;
      forceRemoveNativeBlueLoader();
      render(100);
      loader.classList.add('is-hidden');
      loader.style.setProperty('opacity', '0', 'important');
      loader.style.setProperty('visibility', 'hidden', 'important');
      loader.style.setProperty('pointer-events', 'none', 'important');
      window.setTimeout(() => {
        try { loader.remove(); } catch (_) {}
        loadHud();
      }, 350);
    };

    const hasGameSurface = () => {
      const root = document.getElementById('root') || document.body;
      if (root.querySelector('canvas')) return true;
      if (root.querySelector('input[type="text"], textarea, [class*="chat"], [class*="toolbar"], [class*="room"], [class*="hotel"]')) return true;
      return root.children.length > 0;
    };

    const tick = () => {
      if (hidden) return;
      forceRemoveNativeBlueLoader();
      const elapsed = performance.now() - started;

      let target = 22;
      if (elapsed > 350) target = 38;
      if (elapsed > 750) target = 58;
      if (elapsed > 1200) target = 78;
      if (elapsed > 1700) target = 92;
      if (elapsed > 2200) target = 99;

      if (shown < target) shown += Math.max(0.8, (target - shown) * 0.16);
      render(shown);

      if ((elapsed > 900 && hasGameSurface()) || elapsed > MAX_BLOCK_MS) return hide();

      requestAnimationFrame(tick);
    };

    const observer = new MutationObserver(() => {
      forceRemoveNativeBlueLoader();
      if (!hidden && performance.now() - started > 900 && hasGameSurface()) hide();
    });
    observer.observe(document.body, { childList: true, subtree: true, attributes: true });

    render(shown);
    requestAnimationFrame(tick);
    window.setTimeout(hide, HARD_KILL_MS);
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();
