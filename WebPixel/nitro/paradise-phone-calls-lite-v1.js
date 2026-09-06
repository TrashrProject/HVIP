(() => {
  'use strict';

  if (window.__PARADISE_PHONE_CALLS_LITE_V1__) return;
  window.__PARADISE_PHONE_CALLS_LITE_V1__ = '1.0.0';

  const API = '/nitro/phone-call-api.php';
  const INCOMING_POLL_MS = 1200;
  const STATUS_POLL_MS = 900;
  const UI_SCAN_MS = 1000;
  const REQUEST_TIMEOUT_MS = 4500;
  const DUMMY_OFFER = { type: 'offer', sdp: 'v=0\r\ns=ParadisePhone-Lite\r\n' };
  const DUMMY_ANSWER = { type: 'answer', sdp: 'v=0\r\ns=ParadisePhone-Lite\r\n' };

  const state = {
    csrf: '',
    me: null,
    incoming: null,
    active: null,
    busy: false,
    incomingBusy: false,
    statusBusy: false,
    incomingTimer: 0,
    statusTimer: 0,
    uiTimer: 0,
    clockTimer: 0,
    startedAt: 0
  };

  const escapeHtml = value => String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;');

  async function request(action, options = {}) {
    const method = options.method || 'GET';
    const query = options.query ? `&${options.query}` : '';
    const controller = new AbortController();
    const timeout = window.setTimeout(() => controller.abort(), options.timeout || REQUEST_TIMEOUT_MS);

    try {
      const response = await fetch(`${API}?action=${encodeURIComponent(action)}${query}`, {
        method,
        credentials: 'same-origin',
        cache: 'no-store',
        signal: controller.signal,
        headers: method === 'POST' ? { 'Content-Type': 'application/json' } : undefined,
        body: method === 'POST' ? JSON.stringify({ ...options.body, csrf: state.csrf }) : undefined
      });
      const payload = await response.json().catch(() => null);
      if (!response.ok || !payload?.ok) throw new Error(payload?.error || 'Impossible de gérer cet appel.');
      return payload;
    } catch (error) {
      if (error?.name === 'AbortError') throw new Error('Le serveur met trop de temps à répondre.');
      throw error;
    } finally {
      clearTimeout(timeout);
    }
  }

  function avatarUrl(look) {
    return `/avatar.php?figure=${encodeURIComponent(look || '')}&size=m&direction=2&head_direction=2`;
  }

  function getPhoneFrame() {
    return document.querySelector('.nitro-phone-frame');
  }

  function getLayer() {
    let layer = document.querySelector('.paradise-call-layer');
    const frame = getPhoneFrame();
    const host = frame || document.body;

    if (!layer) {
      layer = document.createElement('div');
      layer.className = 'paradise-call-layer';
      host.appendChild(layer);
    } else if (layer.parentElement !== host) {
      host.appendChild(layer);
    }

    layer.classList.toggle('is-floating', !frame);
    return layer;
  }

  function removeLayer() {
    document.querySelector('.paradise-call-layer')?.remove();
  }

  function notice(message, kind = 'info') {
    let toast = document.querySelector('.paradise-call-toast');
    if (!toast) {
      toast = document.createElement('div');
      toast.className = 'paradise-call-toast';
      document.body.appendChild(toast);
    }
    toast.className = `paradise-call-toast is-${kind}`;
    toast.textContent = message;
    clearTimeout(Number(toast.dataset.timer || 0));
    toast.dataset.timer = String(window.setTimeout(() => toast.remove(), 3000));
  }

  function formatDuration(seconds) {
    const value = Math.max(0, Math.floor(seconds));
    return `${String(Math.floor(value / 60)).padStart(2, '0')}:${String(value % 60).padStart(2, '0')}`;
  }

  function updateClock() {
    const node = document.querySelector('[data-pcall-duration]');
    if (node && state.startedAt) node.textContent = formatDuration((Date.now() - state.startedAt) / 1000);
  }

  function startClock(startedAt = Date.now()) {
    state.startedAt = startedAt;
    clearInterval(state.clockTimer);
    state.clockTimer = window.setInterval(updateClock, 1000);
    updateClock();
  }

  function stopClock() {
    clearInterval(state.clockTimer);
    state.clockTimer = 0;
    state.startedAt = 0;
  }

  function renderIncoming() {
    const call = state.incoming;
    if (!call) return;
    getLayer().innerHTML = `<section class="pcall-card pcall-incoming">
      <div class="pcall-topline"><span>ParadisePhone</span><b>APPEL PRIVÉ</b></div>
      <div class="pcall-avatar"><img src="${avatarUrl(call.caller.look)}" alt=""></div>
      <h3>${escapeHtml(call.caller.username)}</h3>
      <p>Appel entrant…</p>
      <div class="pcall-incoming-actions">
        <button type="button" class="is-decline" data-pcall-lite-decline><span>✕</span>Refuser</button>
        <button type="button" class="is-accept" data-pcall-lite-accept><span>☎</span>Décrocher</button>
      </div>
    </section>`;
  }

  function renderOutgoing() {
    const active = state.active;
    if (!active) return;
    getLayer().innerHTML = `<section class="pcall-card pcall-outgoing">
      <div class="pcall-topline"><span>ParadisePhone</span><b>APPEL PRIVÉ</b></div>
      <div class="pcall-avatar"><img src="${avatarUrl(active.peerLook)}" alt=""></div>
      <h3>${escapeHtml(active.peerName)}</h3>
      <p class="pcall-status">Sonnerie…</p>
      <div class="pcall-pulse"><i></i><i></i><i></i></div>
      <button type="button" class="pcall-big-hangup" data-pcall-lite-hangup>☎</button>
      <small>Raccrocher</small>
    </section>`;
  }

  function renderActive() {
    const active = state.active;
    if (!active) return;
    getLayer().innerHTML = `<section class="pcall-card pcall-active is-audio">
      <div class="pcall-topline"><span>ParadisePhone</span><b>EN APPEL</b></div>
      <div class="pcall-avatar is-large"><img src="${avatarUrl(active.peerLook)}" alt=""></div>
      <h3>${escapeHtml(active.peerName)}</h3>
      <p class="pcall-status">En communication</p>
      <strong class="pcall-duration" data-pcall-duration>00:00</strong>
      <div class="pcall-controls">
        <button type="button" class="pcall-control is-hangup" data-pcall-lite-hangup>☎<span>Raccrocher</span></button>
      </div>
    </section>`;
    updateClock();
  }

  async function startCall(target) {
    if (!target || state.busy || state.active || state.incoming) return;
    state.busy = true;
    try {
      const payload = await request('start', {
        method: 'POST',
        body: { target, type: 'audio', offer: DUMMY_OFFER }
      });

      state.active = {
        id: payload.call.id,
        peerName: payload.call.callee.username,
        peerLook: payload.call.callee.look,
        isCaller: true,
        accepted: false
      };
      renderOutgoing();
      startStatusPolling();
    } catch (error) {
      notice(error.message || 'Impossible de lancer l’appel.', 'error');
    } finally {
      state.busy = false;
    }
  }

  async function acceptIncoming() {
    const call = state.incoming;
    if (!call || state.busy || state.active) return;
    state.busy = true;
    try {
      await request('accept', {
        method: 'POST',
        body: { id: call.id, answer: DUMMY_ANSWER }
      });

      state.active = {
        id: call.id,
        peerName: call.caller.username,
        peerLook: call.caller.look,
        isCaller: false,
        accepted: true
      };
      state.incoming = null;
      startClock();
      renderActive();
      startStatusPolling();
    } catch (error) {
      notice(error.message || 'Impossible de décrocher.', 'error');
      renderIncoming();
    } finally {
      state.busy = false;
    }
  }

  async function declineIncoming() {
    const call = state.incoming;
    if (!call || state.busy) return;
    state.busy = true;
    state.incoming = null;
    removeLayer();
    try {
      await request('decline', { method: 'POST', body: { id: call.id } });
    } catch (error) {
      notice(error.message || 'Impossible de refuser l’appel.', 'error');
    } finally {
      state.busy = false;
    }
  }

  async function endCall(remote = false) {
    const active = state.active;
    if (!active) return;
    state.active = null;
    clearInterval(state.statusTimer);
    state.statusTimer = 0;
    stopClock();
    removeLayer();

    if (!remote) {
      try {
        await request('end', { method: 'POST', body: { id: active.id } });
      } catch {}
    }
  }

  function startStatusPolling() {
    clearInterval(state.statusTimer);

    const tick = async () => {
      if (state.statusBusy) return;
      const active = state.active;
      if (!active) return;
      state.statusBusy = true;
      try {
        const payload = await request('status', { query: `id=${encodeURIComponent(active.id)}` });
        const call = payload.call;
        if (!state.active || state.active.id !== active.id) return;

        if (call.status === 'accepted') {
          if (!active.accepted) {
            active.accepted = true;
            startClock(call.answeredAt ? call.answeredAt * 1000 : Date.now());
            renderActive();
          }
        } else if (call.status === 'declined') {
          notice(`${active.peerName} a refusé l’appel.`);
          await endCall(true);
        } else if (call.status === 'missed') {
          notice('Appel sans réponse.');
          await endCall(true);
        } else if (call.status === 'ended') {
          notice('Appel terminé.');
          await endCall(true);
        }
      } catch (error) {
        console.warn('[ParadisePhone Lite] status', error);
      } finally {
        state.statusBusy = false;
      }
    };

    tick();
    state.statusTimer = window.setInterval(tick, STATUS_POLL_MS);
  }

  async function pollIncoming() {
    if (!state.csrf || state.active || state.busy || state.incomingBusy) return;
    state.incomingBusy = true;
    try {
      const payload = await request('incoming');
      const call = payload.call;

      if (!call) {
        if (state.incoming) {
          state.incoming = null;
          removeLayer();
        }
        return;
      }

      if (state.incoming?.id === call.id) return;
      state.incoming = call;
      renderIncoming();
    } catch (error) {
      console.warn('[ParadisePhone Lite] incoming', error);
    } finally {
      state.incomingBusy = false;
    }
  }

  function addCallButtons() {
    const app = document.querySelector('.phone-friends-app');
    if (!app) return;

    app.querySelectorAll('.friend-row:not([data-paradise-calls-lite])').forEach(row => {
      const nameNode = row.querySelector('.friend-name') || row.querySelector('[class*="friend-name"]');
      const target = nameNode?.textContent?.trim();
      if (!target || target === state.me?.username) return;

      row.dataset.paradiseCallsLite = '1';
      row.classList.add('paradise-callable-friend');

      const actions = document.createElement('span');
      actions.className = 'paradise-call-actions';
      actions.innerHTML = `<button type="button" data-pcall-lite-start="${escapeHtml(target)}" title="Appeler ${escapeHtml(target)}">☎</button>`;
      row.appendChild(actions);
    });
  }

  function syncCallLayerHost() {
    const layer = document.querySelector('.paradise-call-layer');
    if (!layer) return;
    const frame = getPhoneFrame();
    const host = frame || document.body;
    if (layer.parentElement !== host) host.appendChild(layer);
    layer.classList.toggle('is-floating', !frame);
  }

  document.addEventListener('click', event => {
    const start = event.target.closest?.('[data-pcall-lite-start]');
    if (start) {
      event.preventDefault();
      event.stopPropagation();
      startCall(start.dataset.pcallLiteStart || '');
      return;
    }

    if (event.target.closest?.('[data-pcall-lite-accept]')) {
      event.preventDefault();
      event.stopPropagation();
      acceptIncoming();
      return;
    }

    if (event.target.closest?.('[data-pcall-lite-decline]')) {
      event.preventDefault();
      event.stopPropagation();
      declineIncoming();
      return;
    }

    if (event.target.closest?.('[data-pcall-lite-hangup]')) {
      event.preventDefault();
      event.stopPropagation();
      endCall(false);
    }
  }, true);

  document.addEventListener('pointerdown', event => {
    if (event.target.closest?.('[data-pcall-lite-start],[data-pcall-lite-accept],[data-pcall-lite-decline],[data-pcall-lite-hangup]')) {
      event.stopPropagation();
    }
  }, true);

  async function bootstrap() {
    try {
      const payload = await request('bootstrap');
      state.csrf = payload.csrf;
      state.me = payload.me;

      addCallButtons();
      clearInterval(state.incomingTimer);
      clearInterval(state.uiTimer);
      state.incomingTimer = window.setInterval(pollIncoming, INCOMING_POLL_MS);
      state.uiTimer = window.setInterval(() => {
        addCallButtons();
        syncCallLayerHost();
      }, UI_SCAN_MS);
      pollIncoming();
      console.info('[ParadisePhone] appels lite v1 actifs');
    } catch (error) {
      console.warn('[ParadisePhone Lite] bootstrap', error);
    }
  }

  bootstrap();
})();
