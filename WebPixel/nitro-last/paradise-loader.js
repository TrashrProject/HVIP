(() => {
  const boot = () => {
    const loader = document.getElementById('paradise-loader');
    const root = document.getElementById('root');
    const bar = document.querySelector('.pr-loader-bar');
    const percent = document.querySelector('.pr-loader-percent');
    const status = document.querySelector('.pr-loader-status-copy');
    if (!loader || !root || !bar || !percent || !status) return;

    let progress = 2;
    let hidden = false;
    let realProgress = 0;
    let lastNativeProgress = -1;
    let lastNativeChangeAt = performance.now();
    let noNativeProgressReads = 0;
    let highProgressSince = 0;
    const startedAt = performance.now();

    const setStatus = value => {
      if (value < 18) status.textContent = 'Ouverture de ParadiseRP';
      else if (value < 38) status.textContent = 'Chargement du moteur de jeu';
      else if (value < 62) status.textContent = 'Synchronisation de votre personnage';
      else if (value < 82) status.textContent = 'Préparation de votre environnement';
      else if (value < 96) status.textContent = 'Connexion à l’hôtel';
      else status.textContent = 'Bienvenue sur ParadiseRP';
    };

    const render = value => {
      progress = Math.max(progress, Math.min(100, Number(value) || 0));
      const rounded = Math.round(progress);
      bar.style.width = `${rounded}%`;
      percent.textContent = `${rounded}%`;
      setStatus(rounded);
    };

    const dismissNativeLoader = () => {
      root.querySelectorAll('.nitro-loading,[class*="nitro-loading"]').forEach(el => {
        el.style.setProperty('display', 'none', 'important');
        el.style.setProperty('visibility', 'hidden', 'important');
        el.style.setProperty('pointer-events', 'none', 'important');
      });
    };

    const hide = () => {
      if (hidden) return;
      hidden = true;
      dismissNativeLoader();
      sessionStorage.setItem('paradise-client-loaded', '1');
      sessionStorage.removeItem('paradise-auto-retry');
      render(100);
      status.textContent = 'Bienvenue sur ParadiseRP';
      loader.classList.add('is-ready');
      setTimeout(() => loader.classList.add('is-hidden'), 120);
      setTimeout(() => loader.remove(), 520);
    };

    const nativeProgress = () => {
      const text = root.innerText || root.textContent || '';
      const values = [...text.matchAll(/(?:^|\s)(100|[1-9]?\d)\s*%/g)].map(m => Number(m[1]));
      return values.length ? Math.max(...values) : null;
    };

    const hasReadyUi = () => {
      return !!root.querySelector(
        '.nitro-room-view,[class*="room-view"],[class*="hotel-view"],[class*="room-container"],[class*="nitro-room"],.nitro-toolbar,[class*="toolbar"],canvas'
      );
    };

    const inspect = () => {
      if (hidden) return;
      const current = nativeProgress();

      if (current !== null) {
        noNativeProgressReads = 0;
        realProgress = Math.max(realProgress, current);

        if (current !== lastNativeProgress) {
          lastNativeProgress = current;
          lastNativeChangeAt = performance.now();
        }

        render(Math.max(progress, Math.min(95, current)));

        // Sur ce build Nitro la progression native peut rester à 90% alors que
        // le websocket et le client sont déjà initialisés. Dans ce cas on enlève
        // l'écran de chargement natif au lieu de bloquer l'utilisateur.
        if (current >= 90) {
          if (!highProgressSince) highProgressSince = performance.now();
          if ((performance.now() - highProgressSince) > 2200 && hasReadyUi()) {
            status.textContent = 'Ouverture de l’hôtel';
            hide();
            return;
          }
        } else {
          highProgressSince = 0;
        }

        const stalledFor = performance.now() - lastNativeChangeAt;
        if (current >= 50 && current < 90 && stalledFor > 9000) {
          const alreadyRetried = sessionStorage.getItem('paradise-auto-retry') === '1';
          if (!alreadyRetried) {
            sessionStorage.setItem('paradise-auto-retry', '1');
            status.textContent = 'Reconnexion automatique au client';
            render(Math.max(progress, 88));
            setTimeout(() => location.reload(), 450);
          } else {
            status.textContent = 'Finalisation de la connexion';
          }
        }
        return;
      }

      noNativeProgressReads++;
      if (hasReadyUi() && noNativeProgressReads >= 2) hide();
    };

    const observer = new MutationObserver(inspect);
    observer.observe(root, { childList: true, subtree: true, characterData: true, attributes: true });

    const navigation = performance.getEntriesByType && performance.getEntriesByType('navigation')[0];
    if (navigation && navigation.type === 'reload') {
      render(22);
      status.textContent = 'Reconnexion à votre session';
    } else {
      render(4);
    }

    const tick = () => {
      if (hidden) return;
      inspect();
      if (hidden) return;

      const elapsed = performance.now() - startedAt;
      let target = 18;
      if (elapsed > 900) target = 34;
      if (elapsed > 1800) target = 52;
      if (elapsed > 3000) target = 68;
      if (elapsed > 4500) target = 80;
      if (elapsed > 6500) target = 90;
      if (realProgress > target) target = Math.min(95, realProgress);

      if (progress < target) progress += Math.max(0.18, (target - progress) * 0.055);
      render(progress);
      requestAnimationFrame(tick);
    };

    requestAnimationFrame(tick);
    setInterval(inspect, 350);

    // Fallback absolu : si Nitro a déjà monté son canvas mais garde son overlay
    // de chargement, on libère le client au lieu de laisser ParadiseRP bloqué.
    setTimeout(() => {
      if (!hidden && hasReadyUi()) hide();
    }, 18000);
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();