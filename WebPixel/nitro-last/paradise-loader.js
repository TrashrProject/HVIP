(() => {
  const boot = () => {
    const loader = document.getElementById('paradise-loader');
    const root = document.getElementById('root');
    const bar = document.querySelector('.pr-loader-bar');
    const percent = document.querySelector('.pr-loader-percent');
    const status = document.querySelector('.pr-loader-status-copy');
    if (!loader || !root || !bar || !percent || !status) return;

    const navigation = performance.getEntriesByType && performance.getEntriesByType('navigation')[0];
    const isReload = navigation ? navigation.type === 'reload' : false;
    const hasLoadedOnce = sessionStorage.getItem('paradise-client-loaded') === '1';

    let progress = 2;
    let hidden = false;
    let realProgress = 0;
    const startedAt = performance.now();

    const setStatus = value => {
      if (value < 18) status.textContent = 'Ouverture de ParadiseRP';
      else if (value < 38) status.textContent = 'Chargement du moteur de jeu';
      else if (value < 62) status.textContent = 'Synchronisation de votre personnage';
      else if (value < 82) status.textContent = 'Préparation de votre environnement';
      else if (value < 97) status.textContent = 'Connexion à l’hôtel';
      else status.textContent = 'Bienvenue sur ParadiseRP';
    };

    const render = value => {
      progress = Math.max(progress, Math.min(100, Number(value) || 0));
      const rounded = Math.round(progress);
      bar.style.width = `${rounded}%`;
      percent.textContent = `${rounded}%`;
      setStatus(rounded);
    };

    const hide = (fast = false) => {
      if (hidden) return;
      hidden = true;
      sessionStorage.setItem('paradise-client-loaded', '1');
      render(100);
      loader.classList.add('is-ready');
      const delay = fast ? 80 : 180;
      setTimeout(() => loader.classList.add('is-hidden'), delay);
      setTimeout(() => loader.remove(), fast ? 450 : 700);
    };

    const hasPlayableSurface = () => {
      const canvas = root.querySelector('canvas');
      if (canvas && canvas.width > 0 && canvas.height > 0) return true;
      return !!root.querySelector(
        '.nitro-room-view,[class*="room-view"],[class*="hotel-view"],[class*="room-container"],[class*="nitro-room"]'
      );
    };

    const readRealProgress = () => {
      const text = root.innerText || root.textContent || '';
      const matches = [...text.matchAll(/(?:^|\s)(100|[1-9]?\d)\s*%/g)];
      if (matches.length) {
        realProgress = Math.max(realProgress, ...matches.map(match => Number(match[1])));
        render(realProgress);
      }

      if (hasPlayableSurface()) {
        hide(true);
        return true;
      }

      return false;
    };

    // A reload must never trap the player behind our decorative overlay.
    // Nitro keeps loading underneath; the overlay is removed almost immediately.
    if (isReload || hasLoadedOnce) {
      render(74);
      status.textContent = 'Reconnexion à votre session';
      setTimeout(() => {
        readRealProgress();
        if (!hidden) hide(true);
      }, 650);
    }

    const observer = new MutationObserver(() => readRealProgress());
    observer.observe(root, { childList: true, subtree: true, characterData: true, attributes: true });

    render(progress);

    // Smooth visual progress for a first launch only. It never blocks game readiness.
    const tick = () => {
      if (hidden) return;
      readRealProgress();
      if (hidden) return;

      const elapsed = performance.now() - startedAt;
      let target;
      if (elapsed < 1000) target = 16;
      else if (elapsed < 2200) target = 34;
      else if (elapsed < 3800) target = 56;
      else if (elapsed < 5600) target = 76;
      else target = 91;

      if (realProgress > target) target = realProgress;
      progress += Math.max(0.35, (target - progress) * 0.075);
      render(progress);
      requestAnimationFrame(tick);
    };
    requestAnimationFrame(tick);

    window.addEventListener('load', () => {
      setTimeout(() => {
        readRealProgress();
        // Once the document is fully loaded, our overlay stops being blocking.
        if (!hidden && performance.now() - startedAt > 2500) hide();
      }, 250);
    });

    // Hard guarantees: this UI is decorative and must never lock access to Nitro.
    setTimeout(() => { if (!hidden && root.children.length > 0) hide(); }, 6500);
    setTimeout(() => { if (!hidden) hide(true); }, 9000);
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();