(() => {
  const boot = () => {
    const loader = document.getElementById('paradise-loader');
    const root = document.getElementById('root');
    const bar = document.querySelector('.pr-loader-bar');
    const percent = document.querySelector('.pr-loader-percent');
    const status = document.querySelector('.pr-loader-status-copy');
    if (!loader || !root || !bar || !percent || !status) return;

    let progress = 3;
    let realProgress = 0;
    let hidden = false;
    let lastRealUpdate = performance.now();
    const startedAt = performance.now();

    const setProgress = (value) => {
      if (hidden) return;
      const next = Math.max(progress, Math.min(100, Math.round(Number(value) || 0)));
      progress = next;
      bar.style.width = `${next}%`;
      percent.textContent = `${next}%`;

      if (next < 18) status.textContent = 'Initialisation du client';
      else if (next < 42) status.textContent = 'Chargement des ressources';
      else if (next < 68) status.textContent = 'Connexion à ParadiseRP';
      else if (next < 90) status.textContent = 'Préparation de votre session';
      else status.textContent = 'Entrée dans ParadiseRP';
    };

    const hide = () => {
      if (hidden) return;
      hidden = true;
      progress = 100;
      bar.style.width = '100%';
      percent.textContent = '100%';
      status.textContent = 'Bienvenue à ParadiseRP';
      setTimeout(() => loader.classList.add('is-hidden'), 120);
      setTimeout(() => loader.remove(), 650);
    };

    const hasGameSurface = () => {
      return !!root.querySelector(
        'canvas, .nitro-room-view, [class*="room-view"], [class*="hotel-view"], [class*="nitro-room"], [class*="nitro-hotel"]'
      );
    };

    const readProgress = () => {
      if (hidden) return;

      if (hasGameSurface()) {
        setProgress(100);
        setTimeout(hide, 120);
        return;
      }

      const text = root.innerText || root.textContent || '';
      const matches = [...text.matchAll(/(?:^|\s)(100|[1-9]?\d)\s*%/g)];
      if (matches.length) {
        const value = Math.max(...matches.map(match => Number(match[1])));
        if (value > realProgress) {
          realProgress = value;
          lastRealUpdate = performance.now();
        }

        // Never let the decorative loader visually lag behind Nitro.
        // Keep 100% reserved for the moment the game surface actually appears.
        setProgress(value >= 100 ? 96 : Math.min(96, value));
      }

      // Nitro sometimes stops reporting progress around 50-70% while it is still
      // building the client. Continue the visual progress smoothly instead of
      // making the player think the client is frozen.
      const stalledFor = performance.now() - lastRealUpdate;
      if (stalledFor > 1200 && progress < 94) {
        const elapsed = performance.now() - startedAt;
        const softTarget = Math.min(94, 35 + Math.floor(elapsed / 180));
        if (softTarget > progress) setProgress(Math.min(progress + 1, softTarget));
      }
    };

    const observer = new MutationObserver(readProgress);
    observer.observe(root, { childList: true, subtree: true, characterData: true, attributes: true });

    setProgress(3);

    // Smooth cosmetic progression from the first frame. Real Nitro progress can
    // jump ahead at any time and always takes priority.
    const animation = setInterval(() => {
      if (hidden) {
        clearInterval(animation);
        return;
      }

      readProgress();

      if (progress < 30) setProgress(progress + 2);
      else if (progress < 60) setProgress(progress + 1);
      else if (progress < 88 && performance.now() - lastRealUpdate > 700) setProgress(progress + 1);
    }, 320);

    window.addEventListener('load', () => setTimeout(readProgress, 80));

    // Extra polling covers Nitro updates that do not mutate visible text.
    const poll = setInterval(() => {
      if (hidden) {
        clearInterval(poll);
        return;
      }
      readProgress();
    }, 250);

    // Do not leave the custom overlay blocking the client indefinitely.
    // If Nitro has mounted content, reveal it after a reasonable startup window.
    setTimeout(() => {
      if (!hidden && root.children.length > 0) hide();
    }, 18000);

    // Absolute fallback for unusually slow/legacy sessions.
    setTimeout(() => {
      if (!hidden) hide();
    }, 28000);
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();