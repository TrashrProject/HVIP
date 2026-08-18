(() => {
  const boot = () => {
    const loader = document.getElementById('paradise-loader');
    const root = document.getElementById('root');
    const bar = document.querySelector('.pr-loader-bar');
    const percent = document.querySelector('.pr-loader-percent');
    const status = document.querySelector('.pr-loader-status-copy');
    if (!loader || !root || !bar || !percent || !status) return;

    let hidden = false;
    let shown = 4;
    const started = performance.now();

    const isIgnored = (el) => {
      if (!el) return true;
      if (el === loader || loader.contains(el)) return true;
      if (el.closest && el.closest('#paradise-loader')) return true;
      if (el.closest && el.closest('#paradise-rp-hud')) return true;
      return false;
    };

    const visibleRect = (el) => {
      if (!el || isIgnored(el)) return null;
      const rect = el.getBoundingClientRect();
      if (!rect || rect.width <= 3 || rect.height <= 3) return null;
      const style = window.getComputedStyle(el);
      if (style.display === 'none' || style.visibility === 'hidden' || Number(style.opacity || 1) <= 0.02) return null;
      return rect;
    };

    const suppressNativeBlueLoader = () => {
      document.querySelectorAll('.nitro-loading,[class*="nitro-loading"]').forEach(el => {
        if (isIgnored(el)) return;
        el.style.setProperty('opacity', '0', 'important');
        el.style.setProperty('visibility', 'hidden', 'important');
        el.style.setProperty('pointer-events', 'none', 'important');
      });
    };

    const render = value => {
      if (hidden) return;
      shown = Math.max(shown, Math.min(99, value));
      const v = Math.round(shown);
      bar.style.width = v + '%';
      percent.textContent = v + '%';
      if (v < 25) status.textContent = 'Ouverture de ParadiseRP';
      else if (v < 50) status.textContent = 'Chargement du moteur de jeu';
      else if (v < 75) status.textContent = 'Synchronisation de votre personnage';
      else if (v < 94) status.textContent = 'Préparation de votre environnement';
      else status.textContent = 'Connexion à l’hôtel';
    };

    const legacyHudReady = () => {
      const vw = window.innerWidth;
      const vh = window.innerHeight;
      const nodes = document.querySelectorAll('canvas, input, textarea, button, img, div, span, nav, section');
      for (const el of nodes) {
        const rect = visibleRect(el);
        if (!rect) continue;
        if (el.tagName === 'CANVAS' && rect.width > 200 && rect.height > 150) return true;
        if (rect.width > vw * 0.70 || rect.height > vh * 0.70) continue;
        const topLeftHud = rect.left < 330 && rect.top < 180 && rect.width < 340 && rect.height < 180;
        const leftRailHud = rect.left < 80 && rect.top > 70 && rect.bottom < vh - 50 && rect.width < 95;
        const bottomChatHud = rect.bottom > vh - 95 && rect.left < 520 && rect.width < 520 && rect.height < 95;
        const bottomPhoneHud = rect.right > vw - 60 && rect.bottom > vh - 100 && rect.width < 90 && rect.height < 100;
        if (topLeftHud || leftRailHud || bottomChatHud || bottomPhoneHud) return true;
      }
      return false;
    };

    const clientReady = () => {
      const canvas = document.querySelector('#root canvas, canvas');
      if (canvas) {
        const rect = visibleRect(canvas);
        if (rect && rect.width > 200 && rect.height > 150) return true;
      }

      const realUi = document.querySelector(
        '#root .nitro-toolbar,#root [class*="nitro-toolbar"],#root [class*="room-view"],#root [class*="hotel-view"],#root [class*="room-container"],#root [class*="nitro-room"],#root [class*="navigator-container"],#root [class*="chat"],#root input[type="text"],#root textarea'
      );
      const realRect = visibleRect(realUi);
      if (realRect && realRect.width > 20 && realRect.height > 20) return true;

      return legacyHudReady();
    };

    const hide = () => {
      if (hidden) return;
      hidden = true;
      suppressNativeBlueLoader();
      bar.style.width = '100%';
      percent.textContent = '100%';
      status.textContent = 'Bienvenue sur ParadiseRP';
      loader.classList.add('is-hidden');
      setTimeout(() => { try { loader.remove(); } catch (_) {} }, 450);
    };

    const tick = () => {
      if (hidden) return;
      loader.style.setProperty('z-index', '2147483647', 'important');
      suppressNativeBlueLoader();
      const elapsed = performance.now() - started;
      if (clientReady() && elapsed > 900) return hide();

      let target = 18;
      if (elapsed > 700) target = 35;
      if (elapsed > 1500) target = 55;
      if (elapsed > 2600) target = 72;
      if (elapsed > 4200) target = 86;
      if (elapsed > 6200) target = 94;
      if (elapsed > 9000) target = 97;
      if (elapsed > 13000) target = 99;

      // Sécurité : si l'ancien HUD ou le jeu est visible, on ne laisse plus le loader bloquer à 50%.
      if (elapsed > 5500 && legacyHudReady()) return hide();
      if (elapsed > 18000 && (root.childElementCount > 0 || document.querySelector('canvas'))) return hide();

      if (shown < target) shown += Math.max(.22, (target - shown) * .07);
      render(shown);
      requestAnimationFrame(tick);
    };

    const observer = new MutationObserver(() => {
      suppressNativeBlueLoader();
      if (!hidden && clientReady() && performance.now() - started > 900) hide();
    });
    observer.observe(document.body, { childList: true, subtree: true, attributes: true });

    render(4);
    suppressNativeBlueLoader();
    requestAnimationFrame(tick);
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();

// ParadiseRP HUD external overlay loader. This keeps the Nitro bundle untouched.
(() => {
  const load = () => {
    document.querySelectorAll('script[data-paradise-rp-hud="1"]').forEach(script => script.remove());
    const script = document.createElement('script');
    script.src = './paradise-rp-hud.js?v=3';
    script.defer = true;
    script.dataset.paradiseRpHud = '1';
    document.body.appendChild(script);
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', load);
  else load();
})();
