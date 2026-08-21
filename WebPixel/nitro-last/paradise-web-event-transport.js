(() => {
  'use strict';

  const VERSION = '1.1.0-session-safe';
  if (window.__ParadiseWebEventTransportBooted) return;
  window.__ParadiseWebEventTransportBooted = VERSION;

  // If the legacy RDP WebEvent transport is already present, ParadiseBridge can
  // reuse it directly and this compatibility transport must stay out of the way.
  if (window.rdp_app?.webSocket && window.rdp?.sendData) return;

  const app = window.rdp_app || {
    webSocket: null,
    startedSocket: false,
    UserID: null,
    UName: null,
    Figure: null
  };

  const rdp = window.rdp || {};
  let reconnectTimer = 0;
  let stopped = false;
  let lastEndpoint = null;
  let lastError = null;

  const validUserId = value => {
    if (value === null || value === undefined || value === '') return null;
    const id = Number(value);
    return Number.isInteger(id) && id > 0 ? id : null;
  };

  function buildEndpoint(userId) {
    const configured = String(window.__PARADISE_BRIDGE_WS__ || '').trim();
    if (configured) {
      return configured.endsWith('/') ? `${configured}${userId}` : `${configured}/${userId}`;
    }

    const scheme = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
    return `${scheme}//${window.location.host}/paradise-ws/${userId}`;
  }

  async function resolveSession() {
    const response = await fetch(`../rp-hud-data.php?_=${Date.now()}`, {
      cache: 'no-store',
      credentials: 'same-origin'
    });

    if (!response.ok) throw new Error(`Session HTTP ${response.status}`);
    const data = await response.json();
    const userId = validUserId(data?.id);

    if (!userId) {
      throw new Error(`No authenticated ParadiseRP user id (received ${String(data?.id)})`);
    }

    app.UserID = userId;
    app.UName = data?.username || null;
    app.Figure = data?.look || null;
    return userId;
  }

  function scheduleReconnect() {
    if (stopped || reconnectTimer) return;
    reconnectTimer = window.setTimeout(() => {
      reconnectTimer = 0;
      connect().catch(error => {
        lastError = String(error?.message || error || 'Unknown transport error');
        scheduleReconnect();
      });
    }, 2000);
  }

  async function connect() {
    if (stopped) return null;

    const existing = app.webSocket;
    if (existing && (existing.readyState === WebSocket.OPEN || existing.readyState === WebSocket.CONNECTING)) {
      return existing;
    }

    // Important: Number(null) === 0 in JavaScript. Never treat a missing session
    // value as user 0; resolve the authenticated CMS session first.
    const userId = validUserId(app.UserID) || await resolveSession();
    const endpoint = buildEndpoint(userId);
    lastEndpoint = endpoint;
    lastError = null;

    const socket = new WebSocket(endpoint);
    app.webSocket = socket;

    socket.addEventListener('open', () => {
      app.startedSocket = true;
      lastError = null;
      console.info('[ParadiseBridge] WebEvent connected', { userId, endpoint });
      window.dispatchEvent(new CustomEvent('paradise:webevent-open', { detail: { userId, endpoint } }));
    });

    socket.addEventListener('close', event => {
      app.startedSocket = false;
      lastError = `WebSocket closed (${event.code})`;
      window.dispatchEvent(new CustomEvent('paradise:webevent-close', { detail: { userId, endpoint, code: event.code } }));
      scheduleReconnect();
    });

    socket.addEventListener('error', () => {
      app.startedSocket = false;
      lastError = `WebSocket handshake failed for ${endpoint}`;
    });

    return socket;
  }

  rdp.sendData = function(eventName, data, bypass, json, socket, started, userId) {
    const target = socket || app.webSocket;
    const resolvedUserId = validUserId(userId) || validUserId(app.UserID);

    if (!target || target.readyState !== WebSocket.OPEN) return false;
    if (!resolvedUserId) return false;

    target.send(JSON.stringify({
      UserId: resolvedUserId,
      EventName: String(eventName || ''),
      ExtraData: data == null ? '' : String(data),
      Bypass: Boolean(bypass),
      JSON: Boolean(json)
    }));

    return true;
  };

  window.rdp_app = app;
  window.rdp = rdp;
  window.ParadiseWebEventTransport = Object.freeze({
    version: VERSION,
    connect,
    stop() {
      stopped = true;
      if (reconnectTimer) window.clearTimeout(reconnectTimer);
      reconnectTimer = 0;
      try { app.webSocket?.close(); } catch (_) {}
    },
    getSocket: () => app.webSocket,
    getUserId: () => validUserId(app.UserID),
    getEndpoint: () => lastEndpoint,
    getDiag: () => ({
      version: VERSION,
      userId: validUserId(app.UserID),
      endpoint: lastEndpoint,
      readyState: app.webSocket?.readyState ?? null,
      startedSocket: Boolean(app.startedSocket),
      lastError
    })
  });

  connect().catch(error => {
    lastError = String(error?.message || error || 'Unknown transport error');
    console.warn('[ParadiseBridge] WebEvent transport unavailable', lastError);
    scheduleReconnect();
  });
})();
