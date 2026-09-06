(() => {
  'use strict';

  if (window.__PARADISE_PHONE_CALLS__) return;
  window.__PARADISE_PHONE_CALLS__ = '3.0.0';

  const API = '/nitro/phone-call-api.php';
  const ICE_SERVERS = [{ urls: 'stun:stun.l.google.com:19302' }];
  const ICE_WAIT_MS = 1800;
  const INCOMING_POLL_MS = 650;
  const STATUS_POLL_MS = 650;
  const REQUEST_TIMEOUT_MS = 5500;
  const MAX_WHISPER_LENGTH = 220;

  const state = {
    csrf: '',
    me: null,
    incoming: null,
    active: null,
    incomingTimer: 0,
    statusTimer: 0,
    clockTimer: 0,
    ringTimer: 0,
    ringContext: null,
    startedAt: 0,
    busy: false,
    incomingBusy: false,
    statusBusy: false
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
    const timeout = setTimeout(() => controller.abort(), options.timeout || REQUEST_TIMEOUT_MS);
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
    toast.dataset.timer = String(window.setTimeout(() => toast.remove(), 3300));
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

  function stopRingtone() {
    clearInterval(state.ringTimer);
    state.ringTimer = 0;
    try { state.ringContext?.close(); } catch {}
    state.ringContext = null;
  }

  function startRingtone() {
    stopRingtone();
    try {
      const AudioCtx = window.AudioContext || window.webkitAudioContext;
      if (!AudioCtx) return;
      const ctx = new AudioCtx();
      state.ringContext = ctx;
      const beep = () => {
        if (ctx.state !== 'running') return;
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        osc.type = 'sine';
        osc.frequency.value = 690;
        gain.gain.setValueAtTime(0.0001, ctx.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.045, ctx.currentTime + 0.02);
        gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 0.24);
        osc.connect(gain).connect(ctx.destination);
        osc.start();
        osc.stop(ctx.currentTime + 0.26);
      };
      beep();
      state.ringTimer = window.setInterval(() => { beep(); window.setTimeout(beep, 330); }, 2400);
    } catch {}
  }

  function roomCanvas() {
    return [...document.querySelectorAll('canvas')]
      .filter(canvas => {
        if (canvas.closest('.nitro-phone-frame,.paradise-call-layer,.phone-camera-shell')) return false;
        if (canvas.width < 320 || canvas.height < 200) return false;
        const style = getComputedStyle(canvas);
        return style.display !== 'none' && style.visibility !== 'hidden' && Number(style.opacity || 1) > 0;
      })
      .sort((a, b) => (b.width * b.height) - (a.width * a.height))[0] || null;
  }

  async function getMedia(type) {
    if (type !== 'video') return new MediaStream();
    const canvas = roomCanvas();
    if (!canvas || typeof canvas.captureStream !== 'function') {
      throw new Error('Impossible de récupérer la vue de la salle pour cet appel vidéo.');
    }
    const stream = canvas.captureStream(12);
    if (!stream.getVideoTracks().length) throw new Error('Impossible de partager la vue de la salle.');
    return stream;
  }

  function addWhisper(body, mine = false) {
    const active = state.active;
    if (!active) return;
    active.messages ||= [];
    active.messages.push({ body: String(body || '').slice(0, MAX_WHISPER_LENGTH), mine, at: Date.now() });
    if (active.messages.length > 80) active.messages.splice(0, active.messages.length - 80);
    refreshWhispers();
    if (!mine) notice(`Murmure de ${active.peerName} : ${body}`, 'info');
  }

  function refreshWhispers() {
    const active = state.active;
    const list = document.querySelector('[data-pcall-whispers]');
    if (!active || !list) return;
    list.innerHTML = (active.messages || []).length
      ? active.messages.map(message => `<div class="pcall-whisper ${message.mine ? 'is-mine' : 'is-peer'}"><b>${message.mine ? 'Moi' : escapeHtml(active.peerName)}</b><span>${escapeHtml(message.body)}</span></div>`).join('')
      : '<div class="pcall-whisper-empty">Écrivez un murmure pour parler pendant l’appel.</div>';
    list.scrollTop = list.scrollHeight;
  }

  function setupDataChannel(peer, channel) {
    if (!channel) return;
    peer.dataChannel = channel;
    channel.onopen = () => {
      const active = state.active;
      if (!active || active.pc !== peer.pc) return;
      active.dataChannel = channel;
      active.connected = true;
      if (!state.startedAt) startClock();
      renderActive();
    };
    channel.onmessage = event => {
      if (state.active?.pc !== peer.pc) return;
      try {
        const payload = JSON.parse(String(event.data || ''));
        const body = String(payload?.body || '').trim().slice(0, MAX_WHISPER_LENGTH);
        if (payload?.type === 'whisper' && body) addWhisper(body, false);
      } catch {}
    };
    channel.onclose = () => {
      if (state.active?.pc === peer.pc) state.active.dataChannel = null;
    };
  }

  function createPeer(localStream, isCaller) {
    const pc = new RTCPeerConnection({ iceServers: ICE_SERVERS });
    const remoteStream = new MediaStream();
    const peer = { pc, remoteStream, dataChannel: null };

    localStream.getTracks().forEach(track => pc.addTrack(track, localStream));
    if (isCaller) setupDataChannel(peer, pc.createDataChannel('paradise-whispers', { ordered: true }));
    else pc.ondatachannel = event => setupDataChannel(peer, event.channel);

    pc.ontrack = event => {
      const tracks = event.streams?.[0]?.getTracks?.() || [event.track];
      tracks.forEach(track => {
        if (!remoteStream.getTracks().some(existing => existing.id === track.id)) remoteStream.addTrack(track);
      });
      bindMedia();
    };

    pc.onconnectionstatechange = () => {
      if (!state.active || state.active.pc !== pc) return;
      if (pc.connectionState === 'connected') {
        state.active.connected = true;
        state.active.dataChannel = peer.dataChannel;
        if (!state.startedAt) startClock();
        renderActive();
      } else if (pc.connectionState === 'failed') {
        notice('La connexion de l’appel a échoué.', 'error');
      }
    };

    return peer;
  }

  function waitForUsableIce(pc, timeout = ICE_WAIT_MS) {
    const hasUsableCandidate = () => {
      const sdp = pc.localDescription?.sdp || '';
      return /a=candidate:/.test(sdp) && (/ typ srflx /.test(sdp) || / typ relay /.test(sdp) || pc.iceGatheringState === 'complete');
    };
    if (hasUsableCandidate()) return Promise.resolve();
    return new Promise(resolve => {
      let done = false;
      const finish = () => {
        if (done) return;
        done = true;
        pc.removeEventListener('icecandidate', check);
        pc.removeEventListener('icegatheringstatechange', check);
        clearTimeout(timer);
        resolve();
      };
      const check = () => { if (hasUsableCandidate()) finish(); };
      const timer = setTimeout(finish, timeout);
      pc.addEventListener('icecandidate', check);
      pc.addEventListener('icegatheringstatechange', check);
    });
  }

  function stopStreams(active) {
    try { active?.localStream?.getTracks().forEach(track => track.stop()); } catch {}
    try { active?.remoteStream?.getTracks().forEach(track => track.stop()); } catch {}
    try { active?.dataChannel?.close(); } catch {}
    try { active?.pc?.close(); } catch {}
  }

  function bindMedia() {
    if (!state.active || state.active.type !== 'video') return;
    const remote = document.querySelector('[data-pcall-remote]');
    const local = document.querySelector('[data-pcall-local]');
    if (remote && remote.srcObject !== state.active.remoteStream) {
      remote.srcObject = state.active.remoteStream;
      remote.play?.().catch(() => {});
    }
    if (local && local.srcObject !== state.active.localStream) {
      local.srcObject = state.active.localStream;
      local.muted = true;
      local.play?.().catch(() => {});
    }
  }

  function controlsHtml() {
    const active = state.active;
    const cameraOff = active?.type === 'video' && active.localStream.getVideoTracks().every(track => !track.enabled);
    return `<div class="pcall-controls">
      ${active?.type === 'video' ? `<button type="button" class="pcall-control" data-pcall-camera>${cameraOff ? '🚫' : '📹'}<span>${cameraOff ? 'Partager salle' : 'Masquer salle'}</span></button>` : ''}
      <button type="button" class="pcall-control is-hangup" data-pcall-hangup>☎<span>Raccrocher</span></button>
    </div>`;
  }

  function whispersHtml() {
    return `<div class="pcall-whisper-panel">
      <div class="pcall-whisper-title"><span>◌</span> Murmures privés</div>
      <div class="pcall-whisper-list" data-pcall-whispers></div>
      <form class="pcall-whisper-form" data-pcall-whisper-form autocomplete="off">
        <input type="text" maxlength="${MAX_WHISPER_LENGTH}" data-pcall-whisper-input placeholder="Écrire un murmure…">
        <button type="submit">➤</button>
      </form>
    </div>`;
  }

  function renderIncoming() {
    const call = state.incoming;
    if (!call) return;
    const video = call.type === 'video';
    getLayer().innerHTML = `<section class="pcall-card pcall-incoming">
      <div class="pcall-topline"><span>ParadisePhone</span><b>${video ? 'APPEL VIDÉO' : 'APPEL MURMURE'}</b></div>
      <div class="pcall-avatar"><img src="${avatarUrl(call.caller.look)}" alt=""></div>
      <h3>${escapeHtml(call.caller.username)}</h3>
      <p>${video ? 'Appel vidéo entrant…' : 'Appel privé entrant…'}</p>
      <div class="pcall-incoming-actions">
        <button type="button" class="is-decline" data-pcall-decline><span>✕</span>Refuser</button>
        <button type="button" class="is-accept" data-pcall-accept><span>${video ? '📹' : '☎'}</span>Décrocher</button>
      </div>
    </section>`;
  }

  function renderAccepting(call) {
    const video = call.type === 'video';
    getLayer().innerHTML = `<section class="pcall-card pcall-outgoing">
      <div class="pcall-topline"><span>ParadisePhone</span><b>${video ? 'VIDÉO' : 'MURMURE'}</b></div>
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
      <div class="pcall-topline"><span>ParadisePhone</span><b>${active.type === 'video' ? 'VIDÉO' : 'MURMURE'}</b></div>
      <div class="pcall-avatar"><img src="${avatarUrl(active.peerLook)}" alt=""></div>
      <h3>${escapeHtml(active.peerName)}</h3>
      <p class="pcall-status">Sonnerie…</p>
      <div class="pcall-pulse"><i></i><i></i><i></i></div>
      <button type="button" class="pcall-big-hangup" data-pcall-hangup>☎</button>
      <small>Raccrocher</small>
    </section>`;
  }

  function renderActive() {
    const active = state.active;
    if (!active) return;
    const status = active.connected ? 'En communication' : 'Connexion…';
    if (active.type === 'video') {
      getLayer().innerHTML = `<section class="pcall-card pcall-active is-video">
        <video autoplay playsinline data-pcall-remote></video>
        <div class="pcall-video-shade"></div>
        <div class="pcall-video-head"><b>${escapeHtml(active.peerName)}</b><span>${status} · <i data-pcall-duration>00:00</i></span><small>Vue de sa salle</small></div>
        <video autoplay muted playsinline data-pcall-local class="pcall-local-video" title="Vue de votre salle"></video>
        ${whispersHtml()}
        ${controlsHtml()}
      </section>`;
    } else {
      getLayer().innerHTML = `<section class="pcall-card pcall-active is-audio is-whisper-call">
        <div class="pcall-topline"><span>ParadisePhone</span><b>APPEL MURMURE</b></div>
        <div class="pcall-avatar is-large"><img src="${avatarUrl(active.peerLook)}" alt=""></div>
        <h3>${escapeHtml(active.peerName)}</h3>
        <div class="pcall-call-meta"><span>${status}</span><strong data-pcall-duration>00:00</strong></div>
        ${whispersHtml()}
        ${controlsHtml()}
      </section>`;
    }
    bindMedia();
    refreshWhispers();
    updateClock();
  }

  async function startCall(target, type = 'audio') {
    if (state.active || state.incoming || state.busy) {
      notice('Le téléphone est déjà occupé.', 'error');
      return;
    }
    state.busy = true;
    let localStream = null;
    let peer = null;
    try {
      localStream = await getMedia(type);
      peer = createPeer(localStream, true);
      const offer = await peer.pc.createOffer();
      await peer.pc.setLocalDescription(offer);
      await waitForUsableIce(peer.pc);

      const payload = await request('start', {
        method: 'POST',
        body: { target, type, offer: peer.pc.localDescription.toJSON ? peer.pc.localDescription.toJSON() : peer.pc.localDescription }
      });

      state.active = {
        id: payload.call.id,
        type,
        isCaller: true,
        peerName: payload.call.callee.username,
        peerLook: payload.call.callee.look,
        localStream,
        remoteStream: peer.remoteStream,
        dataChannel: peer.dataChannel,
        pc: peer.pc,
        connected: false,
        remoteSet: false,
        messages: []
      };
      renderOutgoing();
      startStatusPolling();
    } catch (error) {
      stopStreams({ localStream, remoteStream: peer?.remoteStream, dataChannel: peer?.dataChannel, pc: peer?.pc });
      notice(error.message || 'Impossible de lancer l’appel.', 'error');
    } finally {
      state.busy = false;
    }
  }

  async function acceptIncoming() {
    const call = state.incoming;
    if (!call || state.busy || state.active) return;
    state.busy = true;
    stopRingtone();
    renderAccepting(call);

    let localStream = null;
    let peer = null;
    try {
      localStream = await getMedia(call.type);
      peer = createPeer(localStream, false);
      if (!call.offer) throw new Error('Signal d’appel incomplet. Réessayez.');
      await peer.pc.setRemoteDescription(new RTCSessionDescription(call.offer));
      const answer = await peer.pc.createAnswer();
      await peer.pc.setLocalDescription(answer);
      await waitForUsableIce(peer.pc);

      await request('accept', {
        method: 'POST',
        body: { id: call.id, answer: peer.pc.localDescription.toJSON ? peer.pc.localDescription.toJSON() : peer.pc.localDescription }
      });

      state.active = {
        id: call.id,
        type: call.type,
        isCaller: false,
        peerName: call.caller.username,
        peerLook: call.caller.look,
        localStream,
        remoteStream: peer.remoteStream,
        dataChannel: peer.dataChannel,
        pc: peer.pc,
        connected: false,
        remoteSet: true,
        messages: []
      };
      state.incoming = null;
      startClock();
      renderActive();
      startStatusPolling();
    } catch (error) {
      stopStreams({ localStream, remoteStream: peer?.remoteStream, dataChannel: peer?.dataChannel, pc: peer?.pc });
      state.busy = false;
      state.incoming = call;
      renderIncoming();
      startRingtone();
      notice(error.message || 'Impossible de décrocher. Réessayez.', 'error');
      return;
    } finally {
      state.busy = false;
    }
  }

  async function declineIncoming() {
    const call = state.incoming;
    if (!call) return;
    stopRingtone();
    state.incoming = null;
    removeLayer();
    try { await request('decline', { method: 'POST', body: { id: call.id } }); } catch {}
  }

  async function endCall(remote = false) {
    const active = state.active;
    if (!active) return;
    state.active = null;
    clearInterval(state.statusTimer);
    state.statusTimer = 0;
    stopClock();
    stopStreams(active);
    removeLayer();
    if (!remote) {
      try { await request('end', { method: 'POST', body: { id: active.id } }); } catch {}
    }
  }

  function toggleCamera() {
    if (!state.active || state.active.type !== 'video') return;
    state.active.localStream.getVideoTracks().forEach(track => { track.enabled = !track.enabled; });
    renderActive();
  }

  function sendWhisper(form) {
    const active = state.active;
    if (!active) return;
    const input = form?.querySelector('[data-pcall-whisper-input]');
    const body = String(input?.value || '').trim().slice(0, MAX_WHISPER_LENGTH);
    if (!body) return;
    const channel = active.dataChannel;
    if (!active.connected || !channel || channel.readyState !== 'open') {
      notice('Connexion du murmure en cours…', 'info');
      return;
    }
    try {
      channel.send(JSON.stringify({ type: 'whisper', body, at: Date.now() }));
      addWhisper(body, true);
      input.value = '';
      input.focus();
    } catch {
      notice('Impossible d’envoyer ce murmure.', 'error');
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
          if (active.isCaller && !active.remoteSet && call.answer) {
            await active.pc.setRemoteDescription(new RTCSessionDescription(call.answer));
            active.remoteSet = true;
            startClock(call.answeredAt ? call.answeredAt * 1000 : Date.now());
            renderActive();
          }
        } else if (call.status === 'declined') {
          notice(`${active.peerName} a refusé l’appel.`, 'info');
          await endCall(true);
        } else if (call.status === 'missed') {
          notice('Appel sans réponse.', 'info');
          await endCall(true);
        } else if (call.status === 'ended') {
          notice('Appel terminé.', 'info');
          await endCall(true);
        }
      } catch (error) {
        console.warn('[ParadisePhone Calls] status', error);
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
          stopRingtone();
          removeLayer();
        }
        return;
      }
      if (state.incoming?.id === call.id) return;
      state.incoming = call;
      startRingtone();
      renderIncoming();
    } catch (error) {
      console.warn('[ParadisePhone Calls] incoming', error);
    } finally {
      state.incomingBusy = false;
    }
  }

  function addCallButtons() {
    document.querySelectorAll('.phone-friends-app .friend-row:not([data-paradise-calls-ready])').forEach(row => {
      const nameNode = row.querySelector('.friend-name') || row.querySelector('[class*="friend-name"]');
      const target = nameNode?.textContent?.trim();
      if (!target || target === state.me?.username) return;
      row.dataset.paradiseCallsReady = '1';
      row.classList.add('paradise-callable-friend');
      const actions = document.createElement('span');
      actions.className = 'paradise-call-actions';
      actions.innerHTML = `<button type="button" data-pcall-start="audio" title="Appeler ${escapeHtml(target)}">☎</button><button type="button" data-pcall-start="video" title="Appel vidéo avec ${escapeHtml(target)}">▣</button>`;
      actions.querySelectorAll('button').forEach(button => {
        button.addEventListener('pointerdown', event => event.stopPropagation());
        button.addEventListener('click', event => {
          event.preventDefault();
          event.stopPropagation();
          startCall(target, button.dataset.pcallStart || 'audio');
        });
      });
      row.appendChild(actions);
    });
  }

  document.addEventListener('submit', event => {
    const form = event.target.closest?.('[data-pcall-whisper-form]');
    if (!form) return;
    event.preventDefault();
    event.stopPropagation();
    sendWhisper(form);
  }, true);

  document.addEventListener('click', event => {
    if (event.target.closest('[data-pcall-accept]')) {
      event.preventDefault();
      event.stopPropagation();
      acceptIncoming();
      return;
    }
    if (event.target.closest('[data-pcall-decline]')) {
      event.preventDefault();
      event.stopPropagation();
      declineIncoming();
      return;
    }
    if (event.target.closest('[data-pcall-hangup]')) {
      event.preventDefault();
      event.stopPropagation();
      endCall(false);
      return;
    }
    if (event.target.closest('[data-pcall-camera]')) {
      event.preventDefault();
      event.stopPropagation();
      toggleCamera();
    }
  }, true);

  window.addEventListener('beforeunload', () => {
    if (state.active) {
      try {
        navigator.sendBeacon(`${API}?action=end`, new Blob([JSON.stringify({ id: state.active.id, csrf: state.csrf })], { type: 'application/json' }));
      } catch {}
      stopStreams(state.active);
    }
  });

  const observer = new MutationObserver(() => {
    addCallButtons();
    if ((state.active || state.incoming) && !document.querySelector('.paradise-call-layer')) {
      state.active ? (state.active.connected ? renderActive() : renderOutgoing()) : renderIncoming();
    }
  });
  observer.observe(document.documentElement, { childList: true, subtree: true });

  async function bootstrap() {
    try {
      const payload = await request('bootstrap');
      state.csrf = payload.csrf;
      state.me = payload.me;
      addCallButtons();
      clearInterval(state.incomingTimer);
      state.incomingTimer = window.setInterval(pollIncoming, INCOMING_POLL_MS);
      pollIncoming();
      console.info('[ParadisePhone] appels v3 rapides + décrochage corrigé');
    } catch (error) {
      console.warn('[ParadisePhone Calls] bootstrap', error);
    }
  }

  bootstrap();
})();