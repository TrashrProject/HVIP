(() => {
  'use strict';

  const VERSION = '84.0.0-nitro-room-diagnostics';
  const DEBUG = /(?:^|[?&])prdebug=1(?:&|$)/.test(location.search) || localStorage.getItem('pr_nitro_debug') === '1';
  const MAX_PACKETS = 80;
  const packets = [];
  const assetErrors = [];
  const roomEvents = [];

  const now = () => Math.round(performance.now());
  const log = (scope, ...args) => {
    if (!DEBUG) return;
    console.log(`[ParadiseRP:${scope}]`, ...args);
  };
  const warn = (scope, ...args) => console.warn(`[ParadiseRP:${scope}]`, ...args);

  const hex = bytes => Array.from(bytes).map(v => v.toString(16).padStart(2, '0')).join(' ');

  function parsePacket(buffer, direction) {
    const bytes = new Uint8Array(buffer.slice(0, Math.min(buffer.byteLength, 24)));
    const view = new DataView(buffer);
    const summary = {
      at: now(),
      direction,
      byteLength: buffer.byteLength,
      firstBytes: hex(bytes),
      lengthBE: buffer.byteLength >= 4 ? view.getInt32(0, false) : null,
      headerAfterLengthBE: buffer.byteLength >= 6 ? view.getInt16(4, false) : null,
      headerInt32At4BE: buffer.byteLength >= 8 ? view.getInt32(4, false) : null,
      headerInt16At0BE: buffer.byteLength >= 2 ? view.getInt16(0, false) : null
    };
    packets.push(summary);
    while (packets.length > MAX_PACKETS) packets.shift();
    log('packet', summary);
    return summary;
  }

  async function recordPacket(data, direction) {
    try {
      if (data instanceof ArrayBuffer) return parsePacket(data, direction);
      if (ArrayBuffer.isView(data)) return parsePacket(data.buffer.slice(data.byteOffset, data.byteOffset + data.byteLength), direction);
      if (data instanceof Blob) return parsePacket(await data.arrayBuffer(), direction);
    } catch (error) {
      warn('packet-debug-failed', error);
    }
    return null;
  }

  function patchWebSocket() {
    if (!window.WebSocket || window.WebSocket.__paradisePatched) return;
    const NativeWebSocket = window.WebSocket;

    function ParadiseWebSocket(url, protocols) {
      const socket = protocols !== undefined ? new NativeWebSocket(url, protocols) : new NativeWebSocket(url);
      socket.__paradiseUrl = String(url || '');
      log('ws', 'open wrapper', socket.__paradiseUrl);

      socket.addEventListener('message', event => {
        recordPacket(event.data, 'in');
      }, true);

      const nativeSend = socket.send.bind(socket);
      socket.send = data => {
        recordPacket(data, 'out');
        return nativeSend(data);
      };

      socket.addEventListener('open', () => log('ws', 'open', socket.__paradiseUrl));
      socket.addEventListener('close', event => warn('ws-close', { code: event.code, reason: event.reason, wasClean: event.wasClean }));
      socket.addEventListener('error', event => warn('ws-error', event));
      return socket;
    }

    ParadiseWebSocket.prototype = NativeWebSocket.prototype;
    Object.setPrototypeOf(ParadiseWebSocket, NativeWebSocket);
    ParadiseWebSocket.CONNECTING = NativeWebSocket.CONNECTING;
    ParadiseWebSocket.OPEN = NativeWebSocket.OPEN;
    ParadiseWebSocket.CLOSING = NativeWebSocket.CLOSING;
    ParadiseWebSocket.CLOSED = NativeWebSocket.CLOSED;
    ParadiseWebSocket.__paradisePatched = true;
    window.WebSocket = ParadiseWebSocket;
  }

  function patchFetchForAssets() {
    if (!window.fetch || window.fetch.__paradiseAssetFallback) return;
    const nativeFetch = window.fetch.bind(window);
    const ASSET_EXT = /\.(png|gif|jpe?g|webp|svg|ico|nitro|json|mp3|ogg|wav|ttf|otf|woff2?|eot)(?:[?#]|$)/i;

    window.fetch = async function(input, init) {
      const response = await nativeFetch(input, init);
      try {
        const raw = typeof input === 'string' || input instanceof URL ? String(input) : String(input?.url || '');
        const url = new URL(raw, location.href);
        const isAsset = ASSET_EXT.test(url.pathname);
        const isLocalAsset = url.origin === location.origin && /\/(swf_pz|SWF)\//i.test(url.pathname);
        if (response.status === 404 && isAsset && isLocalAsset) {
          const fallbackUrl = `/asset-resolver.php?u=${encodeURIComponent(url.pathname)}`;
          warn('asset-404-retry', url.pathname, '=>', fallbackUrl);
          const fallback = await nativeFetch(fallbackUrl, { credentials: 'same-origin', cache: 'force-cache' });
          if (fallback.ok) return fallback;
        }
      } catch (error) {
        warn('asset-fallback-error', error);
      }
      return response;
    };
    window.fetch.__paradiseAssetFallback = true;
  }

  function listenForAssetErrors() {
    window.addEventListener('error', event => {
      const target = event.target;
      if (!target || target === window) return;
      const tag = target.tagName;
      const url = target.currentSrc || target.src || target.href || '';
      if (!url || !/^(IMG|LINK|SCRIPT|SOURCE|VIDEO|AUDIO)$/i.test(tag || '')) return;
      const item = { at: now(), tag, url: String(url) };
      assetErrors.push(item);
      while (assetErrors.length > 80) assetErrors.shift();
      warn('asset-error', item);
    }, true);
  }

  function listenForDataViewErrors() {
    const handle = error => {
      const text = String(error?.message || error?.reason?.message || error?.reason || error || '');
      const stack = String(error?.error?.stack || error?.reason?.stack || '');
      if (!/DataView|Offset is outside the bounds|RangeError/i.test(text + stack)) return;
      const report = {
        at: new Date().toISOString(),
        message: text,
        lastPackets: packets.slice(-20),
        assets404: assetErrors.slice(-20),
        roomEvents: roomEvents.slice(-20)
      };
      window.__ParadiseLastDataViewReport = report;
      console.error('[ParadiseRP:DataView RangeError]', report);
    };
    window.addEventListener('error', handle, true);
    window.addEventListener('unhandledrejection', handle, true);
  }

  function monitorRoomState() {
    let lastRoomId = null;
    const readRoomId = () => {
      const queryRoom = new URLSearchParams(location.search).get('room');
      const globalRoom = window.__PARADISE_ROOM_ID__;
      return queryRoom || globalRoom || null;
    };
    const tick = () => {
      const roomId = readRoomId();
      const canvas = document.querySelector('#root canvas');
      const event = {
        at: now(),
        roomId,
        canvas: !!canvas,
        canvasSize: canvas ? `${canvas.width}x${canvas.height}` : '',
        rootChildren: document.querySelector('#root')?.children?.length || 0
      };
      if (roomId !== lastRoomId) {
        lastRoomId = roomId;
        roomEvents.push({ ...event, phase: 'room-context-changed' });
        log('room', 'context changed', event);
      }
      if (DEBUG) roomEvents.push({ ...event, phase: 'heartbeat' });
      while (roomEvents.length > 80) roomEvents.shift();
    };
    tick();
    window.setInterval(tick, DEBUG ? 2000 : 6000);
  }

  patchWebSocket();
  patchFetchForAssets();
  listenForAssetErrors();
  listenForDataViewErrors();
  monitorRoomState();

  window.__ParadiseNitroDebug = {
    version: VERSION,
    enable() { localStorage.setItem('pr_nitro_debug', '1'); location.reload(); },
    disable() { localStorage.removeItem('pr_nitro_debug'); location.reload(); },
    dump() {
      const report = {
        version: VERSION,
        packets: packets.slice(),
        assetErrors: assetErrors.slice(),
        roomEvents: roomEvents.slice(),
        lastDataViewReport: window.__ParadiseLastDataViewReport || null
      };
      console.table(report.packets.slice(-20));
      return report;
    }
  };
})();
