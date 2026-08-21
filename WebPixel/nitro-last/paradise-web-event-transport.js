(() => {
  'use strict';

  if (window.__ParadiseWebEventTransportBooted) return;
  window.__ParadiseWebEventTransportBooted = '1.0.0';

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

    if (!response.ok) throw new Error(`HTTP ${response.status}`);
    const data = await response.json();
    const userId = Number(data?.id);

    if (!Number.isInteger(userId) || userId < 0) {
      throw new Error('No authenticated ParadiseRP user id');
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
      connect().catch(() => scheduleReconnect());
    }, 2000);
  }

  async function connect() {
    if (stopped) return null;

    const existing = app.webSocket;
    if (existing && (existing.readyState === WebSocket.OPEN || existing.readyState === WebSocket.CONNECTING)) {
      return existing;
    }

    const userId = Number.isInteger(Number(app.UserID)) && Number(app.UserID) >= 0
      ? Number(app.UserID)
      : await resolveSession();

    const socket = new WebSocket(buildEndpoint(userId));
    app.webSocket = socket;

    socket.addEventListener('open', () => {
      app.startedSocket = true;
      window.dispatchEvent(new CustomEvent('paradise:webevent-open', { detail: { userId } }));
    });

    socket.addEventListener('close', () => {
      app.startedSocket = false;
      window.dispatchEvent(new CustomEvent('paradise:webevent-close', { detail: { userId } }));
      scheduleReconnect();
    });

    socket.addEventListener('error', () => {
      app.startedSocket = false;
    });

    return socket;
  }

  rdp.sendData = function(eventName, data, bypass, json, socket, started, userId) {
    const target = socket || app.webSocket;
    const resolvedUserId = Number(userId ?? app.UserID);

    if (!target || target.readyState !== WebSocket.OPEN) return false;
    if (!Number.isInteger(resolvedUserId) || resolvedUserId < 0) return false;

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
    connect,
    stop() {
      stopped = true;
      if (reconnectTimer) window.clearTimeout(reconnectTimer);
      reconnectTimer = 0;
      try { app.webSocket?.close(); } catch (_) {}
    },
    getSocket: () => app.webSocket,
    getUserId: () => app.UserID
  });

  connect().catch(error => {
    console.warn('[ParadiseBridge] WebEvent transport unavailable', error.message || error);
    scheduleReconnect();
  });
})();
