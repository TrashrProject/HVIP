(() => {
  'use strict';

  if (window.__PARADISE_PHONE_CALLS_STABLE_V2__) return;
  window.__PARADISE_PHONE_CALLS_STABLE_V2__ = '2.0.0';

  const API = '/nitro/phone-call-api.php';
  const INCOMING_POLL_MS = 1000;
  const STATUS_POLL_MS = 800;
  const UI_SCAN_MS = 1000;
  const REQUEST_TIMEOUT_MS = 5000;
  const ICE_WAIT_MS = 1200;
  const ICE_SERVERS = [{ urls: 'stun:stun.l.google.com:19302' }];
  const DUMMY_OFFER = { type: 'offer', sdp: 'v=0\r\ns=ParadisePhone-Stable\r\n' };
  const DUMMY_ANSWER = { type: 'answer', sdp: 'v=0\r\ns=ParadisePhone-Stable\r\n' };

  const state = {
    csrf: '',
    me: null,
    incoming: null,
    active: null,
    busy: false,
    bootstrapBusy: false,
    incomingBusy: false,
    statusBusy: false,
    incomingTimer: 0,
    statusTimer: 0,
    uiTimer: 0,
    clockTimer: 0,
    startedAt: 0
  };

  const escapeHtml = value => String(value ?? '')
    .replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;').replaceAll("'", '&#039;');

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
      layer.className = 'paradise-call-layer paradise-call-stable-v2';
      host.appendChild(layer);
    } else if (layer.parentElement !== host) {
      host.appendChild(layer);
    }

    layer.classList.add('paradise-call-stable-v2');
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
    toast.dataset.timer = String(window.setTimeout(() => toast.remove(), 3200));
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

  function roomCanvas() {
    return [...document.querySelectorAll('canvas')]
      .filter(canvas => {
        if (!(canvas instanceof HTMLCanvasElement)) return false;
        if (canvas.closest('.nitro-phone-frame,.paradise-call-layer,.phone-camera-shell')) return false;
        if (canvas.width < 320 || canvas.height < 200) return false;
        const rect = canvas.getBoundingClientRect();
        if (rect.width < 240 || rect.height < 160) return false;
        const style = getComputedStyle(canvas);
        return style.display !== 'none' && style.visibility !== 'hidden' && Number(style.opacity || 1) > 0;
      })
      .sort((a, b) => (b.width * b.height) - (a.width * a.height))[0] || null;
  }

  async function getVideoStream() {
    const canvas = roomCanvas();
    if (!canvas || typeof canvas.captureStream !== 'function') {
      throw new Error('Impossible de récupérer la vue de la salle.');
    }
    const stream = canvas.captureStream(8);
    if (!stream.getVideoTracks().length) throw new Error('Impossible de partager la salle.');
    return stream;
  }

  function createVideoPeer(localStream, isCaller) {
    const pc = new RTCPeerConnection({ iceServers: ICE_SERVERS });
    const remoteStream = new MediaStream();
    localStream.getTracks().forEach(track => pc.addTrack(track, localStream));

    pc.ontrack = event => {
      const tracks = event.streams?.[0]?.getTracks?.() || [event.track];
      tracks.forEach(track => {
        if (!remoteStream.getTracks().some(existing => existing.id === track.id)) remoteStream.addTrack(track);
      });
      bindVideo();
    };

    pc.onconnectionstatechange = () => {
      const active = state.active;
      if (!active || active.pc !== pc) return;
      if (pc.connectionState === 'connected') {
        active.connected = true;
        renderActive();
      } else if (pc.connectionState === 'failed') {
        notice('La connexion vidéo a échoué.', 'error');
      }
    };

    return { pc, remoteStream, localStream, isCaller };
  }

  function waitForIce(pc) {
    if (pc.iceGatheringState === 'complete') return Promise.resolve();
    return new Promise(resolve => {
      let finished = false;
      const finish = () => {
        if (finished) return;
        finished = true;
        clearTimeout(timer);
        pc.removeEventListener('icegatheringstatechange', onChange);
        resolve();
      };
      const onChange = () => { if (pc.iceGatheringState === 'complete') finish(); };
      const timer = window.setTimeout(finish, ICE_WAIT_MS);
      pc.addEventListener('icegatheringstatechange', onChange);
    });
  }

  function stopVideo(active) {
    try { active?.localStream?.getTracks().forEach(track => track.stop()); } catch {}
    try { active?.remoteStream?.getTracks().forEach(track => track.stop()); } catch {}
    try { active?.pc?.close(); } catch {}
  }

  function bindVideo() {
    const active = state.active;
    if (!active || active.type !== 'video') return;
    const remote = document.querySelector('[data-pcall-remote]');
    const local = document.querySelector('[data-pcall-local]');

    if (remote && remote.srcObject !== active.remoteStream) {
      remote.srcObject = active.remoteStream;
      remote.play?.().catch(() => {});
    }
    if (local && local.srcObject !== active.localStream) {
      local.srcObject = active.localStream;
      local.muted = true;
      local.play?.().catch(() => {});
    }
  }

  function renderIncoming() {
    const call = state.incoming;
    if (!call) return;
    const video = call.type === 'video';
    getLayer().innerHTML = `<section class="pcall-card pcall-incoming">
      <div class="pcall-topline"><span>ParadisePhone</span><b>${video ? 'APPEL VIDÉO' : 'APPEL PRIVÉ'}</b></div>
      <div class="pcall-avatar"><img src="${avatarUrl(call.caller.look)}" alt=""></div>
      <h3>${escapeHtml(call.caller.username)}</h3>
      <p>${video ? 'Appel vidéo entrant…' : 'Appel entrant…'}</p>
      <div class="pcall-incoming-actions">
        <button type="button" class="is-decline" data-pcall-stable-decline><span>✕</span>Refuser</button>
        <button type="button" class="is-accept" data-pcall-stable-accept><span>${video ? '▣' : '☎'}</span>Décrocher</button>
      </div>
    </section>`;
  }

  function renderAccepting(call) {
    getLayer().innerHTML = `<section class="pcall-card pcall-outgoing">
      <div class="pcall-topline"><span>ParadisePhone</span><b>${call.type === 'video' ? 'VIDÉO' : 'APPEL PRIVÉ'}</b></div>
      <div class="pcall-avatar"><img src="${avatarUrl(call.caller.look)}" alt=""></div>
      <h3>${escapeHtml(call.caller.username)}</h3>
      <p class="pcall-status">Décrochage…</p>
      <div class="pcall-pulse"><i></i><i></i><i></i></div>
    </section>`;
  }

  function renderOutgoing() {
    const active = state.active;
    if (!active) return;
    getLayer().innerHTML = `<section class="pcall-card pcall-outgoing">
      <div class="pcall-topline"><span>ParadisePhone</span><b>${active.type === 'video' ? 'APPEL VIDÉO' : 'APPEL PRIVÉ'}</b></div>
      <div class="pcall-avatar"><img src="${avatarUrl(active.peerLook)}" alt=""></div>
      <h3>${escapeHtml(active.peerName)}</h3>
      <p class="pcall-status">Sonnerie…</p>
      <div class="pcall-pulse"><i></i><i></i><i></i></div>
      <button type="button" class="pcall-big-hangup" data-pcall-stable-hangup>☎</button>
      <small>Raccrocher</small>
    </section>`;
  }

  function renderActive() {
    const active = state.active;
    if (!active) return;
    const status = active.type === 'video' && !active.connected ? 'Connexion vidéo…' : 'En communication';

    if (active.type === 'video') {
      getLayer().innerHTML = `<section class="pcall-card pcall-active is-video">
        <video autoplay playsinline data-pcall-remote></video>
        <div class="pcall-video-shade"></div>
        <div class="pcall-video-head">
          <b>${escapeHtml(active.peerName)}</b>
          <span>${status} · <i data-pcall-duration>00:00</i></span>
          <small>Vue de sa salle</small>
        </div>
        <video autoplay muted playsinline data-pcall-local class="pcall-local-video" title="Vue de votre salle"></video>
        <div class="pcall-controls">
          <button type="button" class="pcall-control" data-pcall-stable-camera>▣<span>${active.videoEnabled === false ? 'Partager salle' : 'Masquer salle'}</span></button>
          <button type="button" class="pcall-control is-hangup" data-pcall-stable-hangup>☎<span>Raccrocher</span></button>
        </div>
      </section>`;
      bindVideo();
    } else {
      getLayer().innerHTML = `<section class="pcall-card pcall-active is-audio">
        <div class="pcall-topline"><span>ParadisePhone</span><b>EN APPEL</b></div>
        <div class="pcall-avatar is-large"><img src="${avatarUrl(active.peerLook)}" alt=""></div>
        <h3>${escapeHtml(active.peerName)}</h3>
        <p class="pcall-status">En communication</p>
        <strong class="pcall-duration" data-pcall-duration>00:00</strong>
        <small>Écrivez normalement dans la salle : vos messages passent en murmure privé.</small>
        <div class="pcall-controls">
          <button type="button" class="pcall-control is-hangup" data-pcall-stable-hangup>☎<span>Raccrocher</span></button>
        </div>
      </section>`;
    }
    updateClock();
  }

  async function ensureBootstrap(showError = true) {
    if (state.csrf && state.me) return true;
    if (state.bootstrapBusy) return false;
    state.bootstrapBusy = true;
    try {
      const payload = await request('bootstrap');
      state.csrf = payload.csrf;
      state.me = payload.me;
      return true;
    } catch (error) {
      console.warn('[ParadisePhone Stable V2] bootstrap', error);
      if (showError) notice(error.message || 'Service d’appel indisponible.', 'error');
      return false;
    } finally {
      state.bootstrapBusy = false;
    }
  }

  async function startCall(target, type) {
    if (!target || !['audio', 'video'].includes(type) || state.busy || state.active || state.incoming) return;
    if (!(await ensureBootstrap(true))) return;
    if (target === state.me?.username) return;

    state.busy = true;
    let media = null;
    let peer = null;
    try {
      let offer = DUMMY_OFFER;
      if (type === 'video') {
        media = await getVideoStream();
        peer = createVideoPeer(media, true);
        await peer.pc.setLocalDescription(await peer.pc.createOffer({ offerToReceiveVideo: true }));
        await waitForIce(peer.pc);
        offer = peer.pc.localDescription?.toJSON?.() || { type: peer.pc.localDescription.type, sdp: peer.pc.localDescription.sdp };
      }

      const payload = await request('start', {
        method: 'POST',
        body: { target, type, offer }
      });

      state.active = {
        id: payload.call.id,
        type,
        peerName: payload.call.callee.username,
        peerLook: payload.call.callee.look,
        isCaller: true,
        accepted: false,
        answerApplied: false,
        pc: peer?.pc || null,
        localStream: media,
        remoteStream: peer?.remoteStream || null,
        connected: type === 'audio',
        videoEnabled: true
      };
      renderOutgoing();
      startStatusPolling();
    } catch (error) {
      try { media?.getTracks().forEach(track => track.stop()); } catch {}
      try { peer?.pc?.close(); } catch {}
      notice(error.message || 'Impossible de lancer l’appel.', 'error');
    } finally {
      state.busy = false;
    }
  }

  async function acceptIncoming() {
    const call = state.incoming;
    if (!call || state.busy || state.active) return;
    state.busy = true;
    renderAccepting(call);

    let media = null;
    let peer = null;
    try {
      let answer = DUMMY_ANSWER;
      if (call.type === 'video') {
        media = await getVideoStream();
        peer = createVideoPeer(media, false);
        await peer.pc.setRemoteDescription(call.offer);
        await peer.pc.setLocalDescription(await peer.pc.createAnswer());
        await waitForIce(peer.pc);
        answer = peer.pc.localDescription?.toJSON?.() || { type: peer.pc.localDescription.type, sdp: peer.pc.localDescription.sdp };
      }

      await request('accept', { method: 'POST', body: { id: call.id, answer } });

      state.active = {
        id: call.id,
        type: call.type,
        peerName: call.caller.username,
        peerLook: call.caller.look,
        isCaller: false,
        accepted: true,
        answerApplied: true,
        pc: peer?.pc || null,
        localStream: media,
        remoteStream: peer?.remoteStream || null,
        connected: call.type === 'audio',
        videoEnabled: true
      };
      state.incoming = null;
      startClock();
      renderActive();
      startStatusPolling();
    } catch (error) {
      try { media?.getTracks().forEach(track => track.stop()); } catch {}
      try { peer?.pc?.close(); } catch {}
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
    stopVideo(active);
    removeLayer();

    if (!remote) {
      try { await request('end', { method: 'POST', body: { id: active.id } }); } catch {}
    }
  }

  async function applyVideoAnswer(active, answer) {
    if (!active?.pc || !answer || active.answerApplied) return;
    await active.pc.setRemoteDescription(answer);
    active.answerApplied = true;
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
          }
          if (active.type === 'video' && active.isCaller && !active.answerApplied && call.answer) {
            await applyVideoAnswer(active, call.answer);
          }
          if (!document.querySelector('.pcall-active')) renderActive();
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
        console.warn('[ParadisePhone Stable V2] status', error);
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
      console.warn('[ParadisePhone Stable V2] incoming', error);
    } finally {
      state.incomingBusy = false;
    }
  }

  function friendNameFromRow(row) {
    const preferred = row.querySelector('.friend-name,[class*="friend-name"]');
    if (preferred?.textContent?.trim()) return preferred.textContent.trim();
    const texts = [...row.querySelectorAll('span,strong,b,div')]
      .map(node => node.textContent?.trim()).filter(Boolean)
      .filter(value => value.length <= 64 && !/^(message|supprimer|delete|ami|amis)$/i.test(value));
    return texts[0] || '';
  }

  function callIconSvg() {
    return '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M7.2 3.5 4.5 6.2c-.8.8-.9 2-.4 3 2.1 4.4 5.3 7.6 9.7 9.7 1 .5 2.2.4 3-.4l2.7-2.7-4.1-3-1.8 1.8c-2.2-1.2-4-3-5.2-5.2l1.8-1.8-3-4.1Z"/></svg>';
  }

  function videoIconSvg() {
    return '<svg viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="6" width="12" height="12" rx="2"></rect><path d="m15 10 5-3v10l-5-3z"></path></svg>';
  }

  function addCallButtons() {
    const app = document.querySelector('.phone-friends-app');
    if (!app) return;

    app.querySelectorAll('.friend-row:not([data-paradise-calls-stable-v2])').forEach(row => {
      const target = friendNameFromRow(row);
      if (!target || target === state.me?.username) return;

      row.dataset.paradiseCallsStableV2 = '1';
      row.classList.add('paradise-callable-friend');
      row.querySelector('.paradise-call-actions')?.remove();

      const actions = document.createElement('span');
      actions.className = 'paradise-call-actions paradise-call-actions-stable-v2';
      actions.innerHTML = `<button type="button" data-pcall-stable-start="audio" data-pcall-target="${escapeHtml(target)}" title="Appeler ${escapeHtml(target)}" aria-label="Appeler ${escapeHtml(target)}">${callIconSvg()}</button><button type="button" data-pcall-stable-start="video" data-pcall-target="${escapeHtml(target)}" title="Appel vidéo avec ${escapeHtml(target)}" aria-label="Appel vidéo avec ${escapeHtml(target)}">${videoIconSvg()}</button>`;
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
    const start = event.target.closest?.('[data-pcall-stable-start]');
    if (start) {
      event.preventDefault();
      event.stopPropagation();
      startCall(start.dataset.pcallTarget || '', start.dataset.pcallStableStart || 'audio');
      return;
    }
    if (event.target.closest?.('[data-pcall-stable-accept]')) {
      event.preventDefault(); event.stopPropagation(); acceptIncoming(); return;
    }
    if (event.target.closest?.('[data-pcall-stable-decline]')) {
      event.preventDefault(); event.stopPropagation(); declineIncoming(); return;
    }
    if (event.target.closest?.('[data-pcall-stable-hangup]')) {
      event.preventDefault(); event.stopPropagation(); endCall(false); return;
    }
    if (event.target.closest?.('[data-pcall-stable-camera]')) {
      event.preventDefault(); event.stopPropagation();
      const active = state.active;
      if (!active || active.type !== 'video') return;
      const tracks = active.localStream?.getVideoTracks?.() || [];
      const enable = tracks.every(track => !track.enabled);
      tracks.forEach(track => { track.enabled = enable; });
      active.videoEnabled = enable;
      renderActive();
    }
  }, true);

  document.addEventListener('pointerdown', event => {
    if (event.target.closest?.('[data-pcall-stable-start],[data-pcall-stable-accept],[data-pcall-stable-decline],[data-pcall-stable-hangup],[data-pcall-stable-camera]')) {
      event.stopPropagation();
    }
  }, true);

  function startTimers() {
    clearInterval(state.incomingTimer);
    clearInterval(state.uiTimer);
    state.incomingTimer = window.setInterval(pollIncoming, INCOMING_POLL_MS);
    state.uiTimer = window.setInterval(() => {
      addCallButtons();
      syncCallLayerHost();
    }, UI_SCAN_MS);
  }

  async function bootstrap() {
    addCallButtons();
    startTimers();
    await ensureBootstrap(false);
    addCallButtons();
    pollIncoming();
    console.info('[ParadisePhone] appels stable V2 actifs');
  }

  bootstrap();
})();
