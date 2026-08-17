(() => {
  const queue = [];
  const queued = new WeakSet();
  let active = 0;
  const MAX_ACTIVE = 1;
  const TIMEOUT = 65000;

  function setStatus(img, text) {
    const card = img.closest('.pr-rp-hair-card');
    if (!card) return;
    const status = card.querySelector('.pr-rp-hair-thumb > span');
    if (status) status.textContent = text;
  }

  function normalizePreviewUrl(raw) {
    try {
      const url = new URL(raw, window.location.origin);
      if (!url.pathname.endsWith('/avatar-image.php')) return raw;
      // Le rendu headonly de l'imager upstream se bloque avec certains customs.
      // On force le chemin FULL déjà validé par le grand aperçu.
      url.searchParams.delete('headonly');
      url.searchParams.set('size', 'l');
      url.searchParams.set('direction', '2');
      url.searchParams.set('head_direction', '2');
      return url.pathname + '?' + url.searchParams.toString();
    } catch (_) {
      return raw.replace(/([?&])headonly=1(&|$)/i, '$1').replace(/([?&])size=n(&|$)/i, '$1size=l$2');
    }
  }

  function finish(task, ok) {
    if (!task || task.done) return;
    task.done = true;
    clearTimeout(task.timer);
    active = Math.max(0, active - 1);
    const card = task.img.closest('.pr-rp-hair-card');
    if (card) {
      card.classList.toggle('is-ready', !!ok);
      card.classList.toggle('is-error', !ok);
    }
    if (!ok) setStatus(task.img, 'Indisponible');
    pump();
  }

  function start(task) {
    if (!task || task.done || !task.img.isConnected) return;
    active++;
    const img = task.img;
    setStatus(img, 'Rendu Nitro…');
    img.onload = () => finish(task, true);
    img.onerror = () => finish(task, false);
    task.timer = setTimeout(() => {
      try { img.removeAttribute('src'); } catch (_) {}
      finish(task, false);
    }, TIMEOUT);
    img.src = task.url;
  }

  function pump() {
    while (active < MAX_ACTIVE && queue.length) {
      const task = queue.shift();
      if (!task || task.done || !task.img.isConnected) continue;
      start(task);
    }
  }

  function enqueue(img) {
    if (!img || queued.has(img)) return;
    const raw = img.dataset.previewUrl || img.getAttribute('src') || img.dataset.src || '';
    if (!raw || !raw.includes('/avatar-image.php?')) return;
    queued.add(img);
    const normalized = normalizePreviewUrl(raw);
    img.dataset.src = normalized;
    try { img.removeAttribute('src'); } catch (_) {}
    const card = img.closest('.pr-rp-hair-card');
    if (card) {
      card.classList.remove('is-ready', 'is-error');
      setStatus(img, 'En attente…');
    }
    queue.push({ img, url: normalized, done: false, timer: null });
    pump();
  }

  function scan(root) {
    if (!root || root.nodeType !== 1) return;
    if (root.matches?.('.pr-rp-hair-card img')) enqueue(root);
    root.querySelectorAll?.('.pr-rp-hair-card img').forEach(enqueue);
  }

  const observer = new MutationObserver(mutations => {
    for (const mutation of mutations) {
      for (const node of mutation.addedNodes) scan(node);
    }
  });

  const begin = () => {
    observer.observe(document.documentElement, { childList: true, subtree: true });
    document.querySelectorAll('.pr-rp-hair-card img').forEach(enqueue);
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', begin, { once: true });
  else begin();
})();
