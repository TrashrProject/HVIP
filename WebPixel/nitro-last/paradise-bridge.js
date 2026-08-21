(() => {
  'use strict';

  if (window.ParadiseBridge) return;

  const VERSION = '1.0.0-http-bootstrap';
  const API_URL = '../rp-hud-data.php';
  const POLL_MS = 10000;

  let timer = 0;
  let running = false;
  let request = null;
  let lastPayload = null;

  async function refresh() {
    if (!window.ParadiseStore) return false;
    if (request) return request;

    request = (async () => {
      try {
        const response = await fetch(`${API_URL}?_=${Date.now()}`, {
          cache: 'no-store',
          credentials: 'same-origin',
          headers: { Accept: 'application/json' }
        });

        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        const payload = await response.json();
        lastPayload = payload;
        return window.ParadiseStore.applyServerSnapshot(payload);
      } catch (error) {
        window.ParadiseStore.setBridgeError(error);
        return false;
      } finally {
        request = null;
      }
    })();

    return request;
  }

  function start() {
    if (running) return;
    running = true;
    refresh();
    timer = window.setInterval(refresh, POLL_MS);
  }

  function stop() {
    running = false;
    if (timer) window.clearInterval(timer);
    timer = 0;
  }

  function onVisibilityChange() {
    if (!running || document.visibilityState !== 'visible') return;
    refresh();
  }

  document.addEventListener('visibilitychange', onVisibilityChange, false);
  window.addEventListener('beforeunload', stop, { once: true });

  window.ParadiseBridge = Object.freeze({
    version: VERSION,
    start,
    stop,
    refresh,
    getLastPayload: () => lastPayload,
    getStatus: () => ({ running, pollingMs: POLL_MS, pending: Boolean(request), api: API_URL })
  });

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', start, { once: true });
  } else {
    start();
  }
})();
