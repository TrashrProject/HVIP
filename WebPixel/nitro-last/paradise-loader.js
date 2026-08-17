(() => {
  const boot = () => {
    const loader = document.getElementById('paradise-loader');
    const root = document.getElementById('root');
    const bar = document.querySelector('.pr-loader-bar');
    const percent = document.querySelector('.pr-loader-percent');
    const status = document.querySelector('.pr-loader-status-copy');
    if (!loader || !root || !bar || !percent || !status) return;

    let lastProgress = 0;
    let seenRealProgress = false;
    let hidden = false;

    const setProgress = (value) => {
      let next = Math.max(lastProgress, Math.min(100, Math.round(Number(value) || 0)));
      lastProgress = next;
      bar.style.width = `${next}%`;
      percent.textContent = `${next}%`;

      if (next < 20) status.textContent = 'Initialisation du client';
      else if (next < 45) status.textContent = 'Chargement des ressources';
      else if (next < 70) status.textContent = 'Synchronisation avec le serveur';
      else if (next < 95) status.textContent = 'Préparation de votre session';
      else status.textContent = 'Bienvenue à ParadiseRP';
    };

    const hide = () => {
      if (hidden) return;
      hidden = true;
      setProgress(100);
      setTimeout(() => loader.classList.add('is-hidden'), 220);
      setTimeout(() => loader.remove(), 900);
    };

    const readProgress = () => {
      const text = root.innerText || root.textContent || '';
      const matches = [...text.matchAll(/(?:^|\s)(100|[1-9]?\d)\s*%/g)];
      if (matches.length) {
        const value = Math.max(...matches.map(match => Number(match[1])));
        seenRealProgress = true;
        setProgress(value);
        if (value >= 100) setTimeout(hide, 350);
        return;
      }

      if (seenRealProgress && lastProgress >= 90) {
        const hasGameSurface = root.querySelector('canvas, .nitro-room-view, [class*="room-view"], [class*="hotel-view"]');
        if (hasGameSurface) setTimeout(hide, 450);
      }
    };

    const observer = new MutationObserver(readProgress);
    observer.observe(root, { childList: true, subtree: true, characterData: true });

    setProgress(4);
    const staged = [9, 16, 24, 31];
    staged.forEach((value, index) => {
      setTimeout(() => {
        if (!seenRealProgress && !hidden) setProgress(value);
      }, 650 + (index * 800));
    });

    window.addEventListener('load', () => setTimeout(readProgress, 250));
    setInterval(() => {
      if (!hidden) readProgress();
    }, 700);

    // Fail-safe: never trap the player behind the decorative loader.
    setTimeout(() => {
      if (!hidden && root.children.length > 0) hide();
    }, 45000);
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();