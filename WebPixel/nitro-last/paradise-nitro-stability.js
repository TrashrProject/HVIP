(() => {
  'use strict';

  const VERSION = '86.0.0-ws-close-diagnostics';
  const DEBUG = /(?:^|[?&])prdebug=1(?:&|$)/.test(location.search) || localStorage.getItem('pr_nitro_debug') === '1';
  const MAX_ITEMS = 160;
  const packets = [];
  const assetErrors = [];
  const roomEvents = [];
  const socketCloses = [];
  const socketErrors = [];

  const now = () => Math.round(performance.now());
  const push = (list, item) => {
    list.push(item);
    while (list.length > MAX_ITEMS) list.shift();
  };
  const hex = bytes => Array.from(bytes).map(v => v.toString(16).padStart(2, '0')).join(' ');
  const debugLog = (scope, ...args) => { if (DEBUG) console.log(`[ParadiseRP:${scope}]`, ...args); };
  const warn = (scope, ...args) => console.warn(`[ParadiseRP:${scope}]`, ...args);

  function makeDump() {
    const canvas = document.querySelector('#root canvas');
    const root = document.querySelector('#root');
    const rootText = String(root?.innerText || root?.textContent || '').slice(0, 500);
    return {
      version: VERSION,
      href: location.href,
      debug: DEBUG,
      bootAuth: window.__PARADISE_BOOT_AUTH__ || null,
      nitroConfig: {
        room: window.NitroConfig?.['forward.id'] ?? null,
        forwardType: window.NitroConfig?.['forward.type'] ?? null,
        hasSso: !!window.NitroConfig?.['sso.ticket'],
        ssoLength: window.NitroConfig?.['sso.ticket'] ? String(window.NitroConfig['sso.ticket']).length : 0,
        socketUrl: window.NitroConfig?.['socket.url'] || null
      },
      canvas: canvas ? { width: canvas.width, height: canvas.height, clientWidth: canvas.clientWidth, clientHeight: canvas.clientHeight } : null,
      rootChildren: root?.children?.length || 0,
      rootText,
      packets: packets.slice(),
      socketCloses: socketCloses.slice(),
      socketErrors: socketErrors.slice(),
      assetErrors: assetErrors.slice(),
      roomEvents: roomEvents.slice(),
      lastDataViewReport: window.__ParadiseLastDataViewReport || null
    };
  }

  function installApi() {
    const api = {
      version: VERSION,
      enable() { localStorage.setItem('pr_nitro_debug', '1'); location.reload(); },
      disable() { localStorage.removeItem('pr_nitro_debug'); location.reload(); },
      dump() {
        const report = makeDump();
        try { console.table(report.packets.slice(-40)); } catch (error) { console.warn('[ParadiseRP] console.table packets failed', error); }
        try { console.table(report.socketCloses.slice(-10)); } catch (error) { console.warn('[ParadiseRP] console.table closes failed', error); }
        console.log('[ParadiseRP] Nitro debug dump', report);
        return report;
      },
      packets,
      assetErrors,
      roomEvents,
      socketCloses,
      socketErrors
    };
    window.__ParadiseNitroDebug = api;
    window.__ParadiseRPDebug = api;
    window.__ParadiseDebug = api;
    return api;
  }

  const api = installApi();

  function parsePacket(buffer, direction) {
    if (!buffer || typeof buffer.byteLength !== 'number') return null;
    const view = new DataView(buffer);
    const bytes = new Uint8Array(buffer.slice(0, Math.min(buffer.byteLength, 32)));
    const summary = {
      at: now(),
      direction,
      byteLength: buffer.byteLength,
      firstBytes: hex(bytes),
      lengthBE: buffer.byteLength >= 4 ? view.getInt32(0, false) : null,
      lengthLE: buffer.byteLength >= 4 ? view.getInt32(0, true) : null,
      headerInt16At0BE: buffer.byteLength >= 2 ? view.getInt16(0, false) : null,
      headerAfterLengthBE: buffer.byteLength >= 6 ? view.getInt16(4, false) : null,
      headerInt32At4BE: buffer.byteLength >= 8 ? view.getInt32(4, false) : null
    };
    push(packets, summary);
    debugLog('packet', summary);
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
      debugLog('ws', 'wrapper created', socket.__paradiseUrl);

      socket.addEventListener('message', event => { recordPacket(event.data, 'in'); }, true);
      const nativeSend = socket.send.bind(socket);
      socket.send = data => {
        recordPacket(data, 'out');
        return nativeSend(data);
      };

      socket.addEventListener('open', () => debugLog('ws', 'open', socket.__paradiseUrl));
      socket.addEventListener('close', event => {
        const item = {
          at: now(),
          url: socket.__paradiseUrl,
          code: event.code,
          reason: event.reason || '',
          wasClean: event.wasClean,
          packetsBeforeClose: packets.slice(-12),
          bootAuth: window.__PARADISE_BOOT_AUTH__ || null
        };
        push(socketCloses, item);
        warn('ws-close', item);
      });
      socket.addEventListener('error', event => {
        const item = { at: now(), url: socket.__paradiseUrl, type: event.type || 'error' };
        push(socketErrors, item);
        warn('ws-error', item);
      });
      return socket;
    }

    ParadiseWebSocket.prototype = NativeWebSocket.prototype;
    Object.setPrototypeOf(ParadiseWebSocket, NativeWebSocket);
    Object.defineProperties(ParadiseWebSocket, {
      CONNECTING: { value: NativeWebSocket.CONNECTING },
      OPEN: { value: NativeWebSocket.OPEN },
      CLOSING: { value: NativeWebSocket.CLOSING },
      CLOSED: { value: NativeWebSocket.CLOSED }
    });
    ParadiseWebSocket.__paradisePatched = true;
    window.WebSocket = ParadiseWebSocket;
    debugLog('ws', 'patched');
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
        const isLocalAsset = url.origin === location.origin && /\/(swf_pz|SWF|c_images|images|assets)\//i.test(url.pathname);
        if (response.status === 404 && isAsset && isLocalAsset) {
          const fallbackUrl = `/asset-resolver.php?u=${encodeURIComponent(url.pathname)}`;
          warn('asset-404-retry', url.pathname, '=>', fallbackUrl);
          const fallback = await nativeFetch(fallbackUrl, { credentials: 'same-origin', cache: 'force-cache' });
          if (fallback.status !== 404) return fallback;
        }
      } catch (error) {
        warn('asset-fallback-error', error);
      }
      return response;
    };
    window.fetch.__paradiseAssetFallback = true;
  }

  function patchImageSources() {
    const nativeDescriptor = Object.getOwnPropertyDescriptor(HTMLImageElement.prototype, 'src');
    if (!nativeDescriptor || !nativeDescriptor.set || HTMLImageElement.prototype.__paradiseSrcPatched) return;
    Object.defineProperty(HTMLImageElement.prototype, 'src', {
      get: nativeDescriptor.get,
      set(value) {
        let next = value;
        try {
          const url = new URL(String(value || ''), location.href);
          if (url.origin === location.origin && /\/(swf_pz|SWF|c_images)\//i.test(url.pathname)) next = url.href;
        } catch (error) {
          warn('image-src-normalize-failed', error);
        }
        return nativeDescriptor.set.call(this, next);
      },
      configurable: true,
      enumerable: nativeDescriptor.enumerable
    });
    HTMLImageElement.prototype.__paradiseSrcPatched = true;
  }

  function listenForAssetErrors() {
    window.addEventListener('error', event => {
      const target = event.target;
      if (!target || target === window) return;
      const tag = target.tagName;
      const url = target.currentSrc || target.src || target.href || '';
      if (!url || !/^(IMG|LINK|SCRIPT|SOURCE|VIDEO|AUDIO)$/i.test(tag || '')) return;
      const item = { at: now(), tag, url: String(url) };
      push(assetErrors, item);
      warn('asset-error', item);
    }, true);
  }

  function listenForDataViewErrors() {
    const handle = event => {
      const reason = event?.reason || event?.error || event;
      const text = String(reason?.message || reason || '');
      const stack = String(reason?.stack || '');
      if (!/DataView|Offset is outside the bounds|RangeError/i.test(text + stack)) return;
      const report = {
        at: new Date().toISOString(),
        message: text,
        stack,
        lastPackets: packets.slice(-30),
        socketCloses: socketCloses.slice(-10),
        assets404: assetErrors.slice(-30),
        roomEvents: roomEvents.slice(-30)
      };
      window.__ParadiseLastDataViewReport = report;
      console.error('[ParadiseRP:DataView RangeError]', report);
    };
    window.addEventListener('error', handle, true);
    window.addEventListener('unhandledrejection', handle, true);
  }

  function monitorRoomState() {
    let lastSnapshot = '';
    const tick = () => {
      const queryRoom = new URLSearchParams(location.search).get('room');
      const canvas = document.querySelector('#root canvas');
      const root = document.querySelector('#root');
      const snapshot = JSON.stringify({ room: queryRoom || window.__PARADISE_ROOM_ID__ || null, canvas: !!canvas, w: canvas?.width || 0, h: canvas?.height || 0 });
      const item = {
        at: now(),
        phase: snapshot !== lastSnapshot ? 'changed' : 'heartbeat',
        roomId: queryRoom || window.__PARADISE_ROOM_ID__ || null,
        canvas: !!canvas,
        canvasSize: canvas ? `${canvas.width}x${canvas.height}` : '',
        rootChildren: root?.children?.length || 0
      };
      if (snapshot !== lastSnapshot || DEBUG) push(roomEvents, item);
      if (snapshot !== lastSnapshot) debugLog('room', item);
      lastSnapshot = snapshot;
    };
    tick();
    window.setInterval(tick, DEBUG ? 2000 : 6000);
  }

  try { patchWebSocket(); } catch (error) { warn('ws-patch-failed', error); }
  try { patchFetchForAssets(); } catch (error) { warn('fetch-patch-failed', error); }
  try { patchImageSources(); } catch (error) { warn('image-patch-failed', error); }
  listenForAssetErrors();
  listenForDataViewErrors();
  monitorRoomState();

  api.ready = true;
  if (DEBUG) api.dump();
})();
