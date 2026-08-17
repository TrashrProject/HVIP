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

    const render = value => {
      if (hidden) return;
      shown = Math.max(shown, Math.min(100, value));
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
      if (root.querySelector('canvas')) return true;
      if (root.querySelector('[class*="toolbar"],[class*="room-view"],[class*="hotel-view"],[class*="room-container"],[class*="nitro-room"]')) return true;
      const children = root.children ? root.children.length : 0;
      if (children > 0 && performance.now() - started > 1200) return true;
      return false;
    };

    const hide = () => {
      if (hidden) return;
      hidden = true;
      bar.style.width = '100%';
      percent.textContent = '100%';
      status.textContent = 'Bienvenue sur ParadiseRP';
      loader.classList.add('is-hidden');
      setTimeout(() => { try { loader.remove(); } catch (_) {} }, 650);
    };

    const tick = () => {
      if (hidden) return;
      if (clientReady()) return hide();
      const elapsed = performance.now() - started;
      let target = 18;
      if (elapsed > 700) target = 35;
      if (elapsed > 1500) target = 55;
      if (elapsed > 2600) target = 72;
      if (elapsed > 4200) target = 86;
      if (elapsed > 6200) target = 94;
      if (shown < target) shown += Math.max(.3, (target - shown) * .08);
      render(shown);
      requestAnimationFrame(tick);
    };

    const observer = new MutationObserver(() => { if (clientReady()) hide(); });
    observer.observe(root, { childList: true, subtree: true, attributes: true });

    render(4);
    requestAnimationFrame(tick);

    // Important: le loader ne peut jamais bloquer Nitro, même si la détection échoue.
    setTimeout(hide, 8000);
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();