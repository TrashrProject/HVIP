(() => {
  const queue = [];
  const queued = new WeakSet();
  let active = 0;
  const MAX_ACTIVE = 2;
  const TIMEOUT = 25000;

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
    pump();
  }

  function start(task) {
    if (!task || task.done || !task.img.isConnected) return;
    active++;
    const img = task.img;
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
    const raw = img.getAttribute('src') || img.dataset.src || '';
    if (!raw || !raw.includes('/avatar-image.php?')) return;
    queued.add(img);
    img.dataset.src = raw;
    try { img.removeAttribute('src'); } catch (_) {}
    const card = img.closest('.pr-rp-hair-card');
    if (card) {
      card.classList.remove('is-ready', 'is-error');
      const status = card.querySelector('.pr-rp-hair-thumb > span');
      if (status) status.textContent = 'En attente…';
    }
    queue.push({ img, url: raw, done: false, timer: null });
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
