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

    const suppressNativeBlueLoader = () => {
      document.querySelectorAll('.nitro-loading,[class*="nitro-loading"]').forEach(el => {
        if (el === loader || loader.contains(el)) return;
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

    const clientReady = () => {
      if (!root) return false;

      const canvas = root.querySelector('canvas');
      if (canvas) {
        const rect = canvas.getBoundingClientRect();
        if (rect.width > 200 && rect.height > 150) return true;
      }

      const realUi = root.querySelector(
        '.nitro-toolbar,[class*="nitro-toolbar"],[class*="room-view"],[class*="hotel-view"],[class*="room-container"],[class*="nitro-room"],[class*="navigator-container"],[class*="hotel-view"]'
      );
      if (realUi) {
        const rect = realUi.getBoundingClientRect();
        if (rect.width > 20 && rect.height > 20) return true;
      }

      return false;
    };

    const hide = () => {
      if (hidden) return;
      hidden = true;
      suppressNativeBlueLoader();
      bar.style.width = '100%';
      percent.textContent = '100%';
      status.textContent = 'Bienvenue sur ParadiseRP';
      loader.classList.add('is-hidden');
      setTimeout(() => { try { loader.remove(); } catch (_) {} }, 650);
    };

    const tick = () => {
      if (hidden) return;
      suppressNativeBlueLoader();
      if (clientReady()) return hide();

      const elapsed = performance.now() - started;
      let target = 18;
      if (elapsed > 700) target = 35;
      if (elapsed > 1500) target = 55;
      if (elapsed > 2600) target = 72;
      if (elapsed > 4200) target = 86;
      if (elapsed > 6200) target = 94;
      if (elapsed > 9000) target = 97;
      if (elapsed > 13000) target = 99;

      if (shown < target) shown += Math.max(.22, (target - shown) * .07);
      render(shown);
      requestAnimationFrame(tick);
    };

    const observer = new MutationObserver(() => {
      suppressNativeBlueLoader();
      if (clientReady()) hide();
    });
    observer.observe(root, { childList: true, subtree: true, attributes: true });

    render(4);
    suppressNativeBlueLoader();
    requestAnimationFrame(tick);

    // Pas de timeout qui révèle l'écran bleu Nitro : le loader ParadiseRP reste
    // affiché jusqu'à ce qu'une vraie surface de jeu soit détectée.
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();