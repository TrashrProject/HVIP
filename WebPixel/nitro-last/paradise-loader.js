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
      document.querySelectorAll('.nitro-loading,[class*="nitro-loading"]').forEach(el => {
        el.style.setProperty('display', 'none', 'important');
        el.style.setProperty('visibility', 'hidden', 'important');
        el.style.setProperty('pointer-events', 'none', 'important');
        try { el.remove(); } catch (_) {}
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
      setTimeout(() => loader.classList.add('is-hidden'), 40);
      setTimeout(() => { try { loader.remove(); } catch (_) {} }, 280);
    };

    const nativeProgress = () => {
      const text = root.innerText || root.textContent || '';
      const values = [...text.matchAll(/(?:^|\s)(100|[1-9]?\d)\s*%/g)].map(m => Number(m[1]));
      return values.length ? Math.max(...values) : null;
    };

    const hasClientSurface = () => {
      return !!document.querySelector('#root canvas,canvas,.nitro-room-view,[class*="room-view"],[class*="hotel-view"],[class*="room-container"],[class*="nitro-room"],.nitro-toolbar,[class*="toolbar"]');
    };

    const inspect = () => {
      if (hidden) return;
      if (hasClientSurface()) {
        hide();
        return;
      }

      const current = nativeProgress();
      if (current !== null) {
        noNativeProgressReads = 0;
        realProgress = Math.max(realProgress, current);
        if (current !== lastNativeProgress) {
          lastNativeProgress = current;
          lastNativeChangeAt = performance.now();
        }
        render(Math.max(progress, Math.min(95, current)));

        if (current >= 90) {
          if (!highProgressSince) highProgressSince = performance.now();
          if (performance.now() - highProgressSince > 1800) {
            hide();
            return;
          }
        } else highProgressSince = 0;

        const stalledFor = performance.now() - lastNativeChangeAt;
        if (current >= 50 && current < 90 && stalledFor > 9000) {
          const alreadyRetried = sessionStorage.getItem('paradise-auto-retry') === '1';
          if (!alreadyRetried) {
            sessionStorage.setItem('paradise-auto-retry', '1');
            status.textContent = 'Reconnexion automatique au client';
            render(Math.max(progress, 88));
            setTimeout(() => location.reload(), 450);
          }
        }
        return;
      }
      noNativeProgressReads++;
    };

    const observer = new MutationObserver(inspect);
    observer.observe(document.documentElement, { childList: true, subtree: true, characterData: true, attributes: true });

    render(4);

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
    setInterval(inspect, 250);

    // Filet de sécurité absolu : le loader ne doit jamais masquer un client déjà utilisable.
    setTimeout(hide, 10000);
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();