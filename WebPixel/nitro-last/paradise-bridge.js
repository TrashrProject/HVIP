(() => {
  'use strict';

  if (window.ParadiseBridge) return;

  const VERSION = '1.0.2-http-bootstrap-diag';
  const API_URL = '../rp-hud-data.php';
  const POLL_MS = 10000;

  let timer = 0;
  let running = false;
  let request = null;
  let lastPayload = null;
  let lastError = null;
  let destroyed = false;

  async function refresh() {
    if (destroyed || !window.ParadiseStore) return false;
    if (request) return request;

    request = (async () => {
      try {
        const response = await fetch(`${API_URL}?_=${Date.now()}`, {
          cache: 'no-store',
          credentials: 'same-origin',
          headers: { Accept: 'application/json' }
        });

        if (!response.ok) {
          throw new Error(`HTTP ${response.status} sur ${API_URL}`);
        }

        const payload = await response.json();
        lastPayload = payload;
        lastError = null;

        if (!payload || payload.ok === false) {
          const reason = payload?.reason || 'unknown_payload_error';
          lastError = reason;
          console.warn('[ParadiseRP:bridge] HUD data unavailable', {
            reason,
            phase2: payload?.phase2 || null,
            api: API_URL
          });
        } else if (payload.phase2 && payload.phase2.available === false) {
          console.warn('[ParadiseRP:bridge] Core HUD OK, Phase 2 supplement unavailable', payload.phase2);
        }

        return window.ParadiseStore.applyServerSnapshot(payload);
      } catch (error) {
        lastError = error?.message || String(error);
        console.error('[ParadiseRP:bridge] HUD request failed', {
          error: lastError,
          api: API_URL
        });
        window.ParadiseStore.setBridgeError(error);
        return false;
      } finally {
        request = null;
      }
    })();

    return request;
  }

  function start() {
    if (destroyed || running) return;
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

  function destroy() {
    if (destroyed) return;
    destroyed = true;
    stop();
    document.removeEventListener('visibilitychange', onVisibilityChange, false);
  }

  document.addEventListener('visibilitychange', onVisibilityChange, false);
  window.addEventListener('beforeunload', destroy, { once: true });

  window.ParadiseBridge = Object.freeze({
    version: VERSION,
    start,
    stop,
    destroy,
    refresh,
    getLastPayload: () => lastPayload,
    getLastError: () => lastError,
    getStatus: () => ({
      running,
      destroyed,
      pollingMs: POLL_MS,
      pending: Boolean(request),
      api: API_URL,
      lastError,
      lastPayloadOk: lastPayload?.ok ?? null,
      phase2: lastPayload?.phase2 ?? null
    })
  });

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', start, { once: true });
  else start();
})();
