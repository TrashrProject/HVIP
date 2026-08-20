(() => {
  'use strict';

  const VERSION = '86.2.0-frame-correlation';
  const DEBUG = /(?:^|[?&])prdebug=1(?:&|$)/.test(location.search) || localStorage.getItem('pr_nitro_debug') === '1';
  const MAX_ITEMS = 240;
  const packets = [];
  const assetErrors = [];
  const roomEvents = [];
  const socketCloses = [];
  const socketErrors = [];
  let lastWsFrameIn = null;
  let lastRoomState = 'boot';
  let socketSerial = 0;
  let lastCanvasElement = null;
  let lastCanvasParent = null;
  let dataViewReportSerial = 0;

  const now = () => Math.round(performance.now());
  const isoNow = () => new Date().toISOString();
  const push = (list, item) => {
    list.push(item);
    while (list.length > MAX_ITEMS) list.shift();
  };
  const hex = bytes => Array.from(bytes).map(v => v.toString(16).padStart(2, '0')).join(' ');
  const debugLog = (scope, ...args) => { if (DEBUG) console.log(`[ParadiseRP:${scope}]`, ...args); };
  const warn = (scope, ...args) => console.warn(`[ParadiseRP:${scope}]`, ...args);

  function fnv1a32(bytes) {
    let hash = 0x811c9dc5;
    for (let i = 0; i < bytes.length; i += 1) {
      hash ^= bytes[i];
      hash = Math.imul(hash, 0x01000193) >>> 0;
    }
    return hash.toString(16).padStart(8, '0');
  }

  function exactLength(data) {
    if (data instanceof ArrayBuffer) return data.byteLength;
    if (ArrayBuffer.isView(data)) return data.byteLength;
    if (data instanceof Blob) return data.size;
    return null;
  }

  async function exactArrayBuffer(data) {
    if (data instanceof ArrayBuffer) return data.slice(0);
    if (ArrayBuffer.isView(data)) return data.buffer.slice(data.byteOffset, data.byteOffset + data.byteLength);
    if (data instanceof Blob) return data.arrayBuffer();
    return null;
  }

  function populatePacketBytes(item, buffer) {
    if (!item || !buffer || typeof buffer.byteLength !== 'number') return item;
    const view = new DataView(buffer);
    const bytes = new Uint8Array(buffer);
    const firstCount = Math.min(bytes.length, 32);
    const lastStart = Math.max(0, bytes.length - 32);
    item.byteLength = buffer.byteLength;
    item.declaredContentLength = buffer.byteLength >= 4 ? view.getInt32(0, false) : null;
    item.lengthLE = buffer.byteLength >= 4 ? view.getInt32(0, true) : null;
    item.headerBE = buffer.byteLength >= 6 ? view.getUint16(4, false) : null;
    item.headerHex = item.headerBE === null ? null : `0x${item.headerBE.toString(16).padStart(4, '0')}`;
    item.lengthInvariant = item.declaredContentLength === null ? null : item.declaredContentLength + 4 === item.byteLength;
    item.fnv1a32 = fnv1a32(bytes);
    item.first32 = hex(bytes.subarray(0, firstCount));
    item.last32 = hex(bytes.subarray(lastStart));
    item.parsePending = false;

    if (DEBUG) {
      console.log(
        `[ParadiseRP:WS ${item.direction === 'in' ? 'IN' : 'OUT'} #${item.frameId}] ` +
        `socket=${item.socketId} timestamp=${item.timestamp} ` +
        `declaredContentLength=${item.declaredContentLength} actualWsPayloadLength=${item.byteLength} ` +
        `headerBE=${item.headerBE} lengthInvariant=${item.lengthInvariant} fnv1a32=${item.fnv1a32} ` +
        `first32=${item.first32} last32=${item.last32}`
      );
    }

    const pending = window.__ParadisePendingDataViewCorrelation;
    if (pending && pending.socketId === item.socketId && pending.frameId === item.frameId) {
      const report = window.__ParadiseLastDataViewReport;
      if (report && report.correlationId === pending.correlationId) {
        report.lastWsFrame = { ...item };
        report.byteCorrelationReadyAt = isoNow();
        window.__ParadisePendingDataViewCorrelation = null;
        if (window.__ParadiseNativeConsoleError) {
          window.__ParadiseNativeConsoleError('[ParadiseRP:DataView CORRELATION BYTES READY]', report);
        }
      }
    }
    return item;
  }

  function recordPacket(data, direction, socketId, frameId) {
    const item = {
      at: now(),
      timestamp: isoNow(),
      direction,
      socketId,
      frameId,
      byteLength: exactLength(data),
      declaredContentLength: null,
      headerBE: null,
      headerHex: null,
      lengthInvariant: null,
      fnv1a32: null,
      first32: '',
      last32: '',
      parsePending: true
    };
    push(packets, item);

    if (direction === 'in') {
      lastWsFrameIn = item;
      window.__ParadiseLastWsFrame = item;
    }

    // Blob is the browser's default WebSocket binary type. Assign the frame ID
    // synchronously before Nitro's own FileReader runs, then fill in exact bytes
    // asynchronously without changing event.data.
    exactArrayBuffer(data)
      .then(buffer => {
        if (buffer) populatePacketBytes(item, buffer);
      })
      .catch(error => warn('packet-debug-failed', error));

    return item;
  }

  function websocketMode() {
    return DEBUG && window.WebSocket?.__paradisePatched ? 'debug-passive-wrapper' : 'native-unpatched';
  }

  function makeDump() {
    const canvas = document.querySelector('#root canvas');
    const root = document.querySelector('#root');
    const rootText = String(root?.innerText || root?.textContent || '').slice(0, 500);
    return {
      version: VERSION,
      href: location.href,
      debug: DEBUG,
      websocketMode: websocketMode(),
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
      lastWsFrame: lastWsFrameIn ? { ...lastWsFrameIn } : null,
      lastRoomState,
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
      websocketMode: 'native-unpatched',
      enable() { localStorage.setItem('pr_nitro_debug', '1'); location.reload(); },
      disable() { localStorage.removeItem('pr_nitro_debug'); location.reload(); },
      dump() {
        const report = makeDump();
        try { console.table(report.packets.slice(-60)); } catch (error) { console.warn('[ParadiseRP] console.table packets failed', error); }
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

  // Passive diagnostic wrapper. It is enabled ONLY with prdebug=1 (or the
  // explicit localStorage debug switch). It never rewrites, truncates, delays,
  // catches or skips Nitro packet data.
  function patchWebSocket() {
    if (!DEBUG || !window.WebSocket || window.WebSocket.__paradisePatched) return;
    const NativeWebSocket = window.WebSocket;

    function ParadiseWebSocket(url, protocols) {
      const socket = protocols !== undefined ? new NativeWebSocket(url, protocols) : new NativeWebSocket(url);
      const socketId = ++socketSerial;
      let inSequence = 0;
      let outSequence = 0;
      socket.__paradiseUrl = String(url || '');
      socket.__paradiseSocketId = socketId;
      debugLog('ws', 'passive diagnostic wrapper created', { socketId, url: socket.__paradiseUrl });

      // Registered immediately when the native socket is created, before Nitro
      // adds its own message handler. Frame numbering is therefore synchronous
      // with the browser WebSocket event order.
      socket.addEventListener('message', event => {
        const frameId = ++inSequence;
        const item = recordPacket(event.data, 'in', socketId, frameId);
        debugLog('ws-in', `IN #${frameId}`, { socketId, byteLength: item.byteLength, timestamp: item.timestamp });
      }, true);

      const nativeSend = socket.send.bind(socket);
      socket.send = data => {
        const frameId = ++outSequence;
        recordPacket(data, 'out', socketId, frameId);
        return nativeSend(data);
      };

      socket.addEventListener('open', () => debugLog('ws', 'open', { socketId, url: socket.__paradiseUrl }));
      socket.addEventListener('close', event => {
        const item = {
          at: now(),
          timestamp: isoNow(),
          socketId,
          url: socket.__paradiseUrl,
          code: event.code,
          reason: event.reason || '',
          wasClean: event.wasClean,
          lastWsFrame: lastWsFrameIn ? { ...lastWsFrameIn } : null,
          packetsBeforeClose: packets.slice(-12),
          bootAuth: window.__PARADISE_BOOT_AUTH__ || null
        };
        push(socketCloses, item);
        warn('ws-close', item);
      });
      socket.addEventListener('error', event => {
        const item = { at: now(), timestamp: isoNow(), socketId, url: socket.__paradiseUrl, type: event.type || 'error' };
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
    ParadiseWebSocket.__paradiseNative = NativeWebSocket;
    window.WebSocket = ParadiseWebSocket;
    api.websocketMode = 'debug-passive-wrapper';
  }

  function safeConsoleArg(arg) {
    if (arg instanceof Error) return `${arg.name}: ${arg.message}\n${arg.stack || ''}`;
    if (typeof arg === 'string') return arg;
    try { return JSON.stringify(arg); } catch (_) { return String(arg); }
  }

  function createDataViewReport(source, args, explicitError) {
    const combined = args.map(safeConsoleArg).join(' ');
    const errorObject = explicitError || args.find(arg => arg instanceof Error) || null;
    const strings = args.filter(arg => typeof arg === 'string');
    const eventName = strings.length > 2 ? strings[strings.length - 1] : null;
    const correlationId = ++dataViewReportSerial;
    const report = {
      correlationId,
      at: isoNow(),
      source,
      message: String(errorObject?.message || combined || 'DataView parser error'),
      stack: String(errorObject?.stack || ''),
      nitroEvent: eventName,
      lastWsFrame: lastWsFrameIn ? { ...lastWsFrameIn } : null,
      lastRoomState,
      canvasPresent: !!document.querySelector('#root canvas'),
      websocketState: (() => {
        const closes = socketCloses.slice(-1)[0];
        return closes ? `closed:${closes.code}` : 'no-close-recorded';
      })(),
      lastPackets: packets.slice(-30).map(item => ({ ...item })),
      socketCloses: socketCloses.slice(-10),
      roomEvents: roomEvents.slice(-30)
    };
    window.__ParadiseLastDataViewReport = report;

    if (lastWsFrameIn?.parsePending) {
      window.__ParadisePendingDataViewCorrelation = {
        correlationId,
        socketId: lastWsFrameIn.socketId,
        frameId: lastWsFrameIn.frameId
      };
    }

    return report;
  }

  // Nitro catches parser exceptions internally in getMessagesForWrapper() and
  // reports them through NitroLogger.error -> console.error. window.onerror is
  // therefore insufficient. In debug mode only, observe console.error while
  // preserving the original call and arguments exactly.
  function patchConsoleForNitroParserErrors() {
    if (!DEBUG || console.error.__paradiseNitroObserved) return;
    const nativeConsoleError = console.error.bind(console);
    window.__ParadiseNativeConsoleError = nativeConsoleError;

    function observedConsoleError(...args) {
      try {
        const combined = args.map(safeConsoleArg).join(' ');
        if (/Error parsing message/i.test(combined) && /DataView|Offset is outside the bounds|RangeError/i.test(combined)) {
          const report = createDataViewReport('NitroLogger.error', args, args.find(arg => arg instanceof Error));
          nativeConsoleError('[ParadiseRP:DataView CORRELATION]', report);
        }
      } catch (diagnosticError) {
        nativeConsoleError('[ParadiseRP:DataView diagnostic failed]', diagnosticError);
      }
      return nativeConsoleError(...args);
    }

    observedConsoleError.__paradiseNitroObserved = true;
    observedConsoleError.__paradiseNative = nativeConsoleError;
    console.error = observedConsoleError;
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
      const item = { at: now(), timestamp: isoNow(), tag, url: String(url) };
      push(assetErrors, item);
      warn('asset-error', item);
    }, true);
  }

  function listenForEscapedDataViewErrors() {
    const handle = event => {
      const reason = event?.reason || event?.error || event;
      const text = String(reason?.message || reason || '');
      const stack = String(reason?.stack || '');
      if (!/DataView|Offset is outside the bounds|RangeError/i.test(text + stack)) return;
      const report = createDataViewReport('window-event', [text, reason], reason instanceof Error ? reason : null);
      if (window.__ParadiseNativeConsoleError) window.__ParadiseNativeConsoleError('[ParadiseRP:DataView ESCAPED]', report);
      else console.error('[ParadiseRP:DataView ESCAPED]', report);
    };
    window.addEventListener('error', handle, true);
    window.addEventListener('unhandledrejection', handle, true);
  }

  function currentRoomId() {
    return new URLSearchParams(location.search).get('room') || window.__PARADISE_ROOM_ID__ || window.NitroConfig?.['forward.id'] || null;
  }

  function describeParent(node) {
    if (!node) return null;
    const id = node.id ? `#${node.id}` : '';
    const classes = typeof node.className === 'string' && node.className ? `.${node.className.trim().replace(/\s+/g, '.')}` : '';
    return `${node.nodeName || 'node'}${id}${classes}`;
  }

  function recordCanvasRemoval(canvas, parent, source) {
    const item = {
      at: now(),
      timestamp: isoNow(),
      phase: 'canvas-removed',
      source,
      roomId: currentRoomId(),
      canvas: false,
      parent: describeParent(parent),
      observationStack: String(new Error('Canvas removal observed').stack || ''),
      lastWsFrame: lastWsFrameIn ? { ...lastWsFrameIn } : null,
      lastRoomState
    };
    lastRoomState = 'canvas-removed';
    push(roomEvents, item);
    console.error('[ParadiseRP:CANVAS REMOVED]', item);
  }

  function monitorCanvasRemoval() {
    const root = document.querySelector('#root');
    if (!root || !window.MutationObserver) return;

    const initial = root.querySelector('canvas');
    if (initial) {
      lastCanvasElement = initial;
      lastCanvasParent = initial.parentNode;
    }

    const observer = new MutationObserver(() => {
      if (lastCanvasElement && !lastCanvasElement.isConnected) {
        recordCanvasRemoval(lastCanvasElement, lastCanvasParent, 'MutationObserver');
        lastCanvasElement = null;
        lastCanvasParent = null;
      }

      const canvas = root.querySelector('canvas');
      if (canvas && canvas !== lastCanvasElement) {
        lastCanvasElement = canvas;
        lastCanvasParent = canvas.parentNode;
        const item = {
          at: now(),
          timestamp: isoNow(),
          phase: 'canvas-visible',
          roomId: currentRoomId(),
          canvas: true,
          canvasSize: `${canvas.width}x${canvas.height}`,
          lastWsFrame: lastWsFrameIn ? { ...lastWsFrameIn } : null
        };
        lastRoomState = 'canvas-visible';
        push(roomEvents, item);
        debugLog('ROOM', item);
      }
    });
    observer.observe(root, { childList: true, subtree: true });
  }

  function monitorRoomState() {
    let lastSnapshot = '';
    const tick = () => {
      const canvas = document.querySelector('#root canvas');
      const root = document.querySelector('#root');
      const roomId = currentRoomId();
      const snapshot = JSON.stringify({ room: roomId, canvas: !!canvas, w: canvas?.width || 0, h: canvas?.height || 0 });

      if (canvas) {
        lastCanvasElement = canvas;
        lastCanvasParent = canvas.parentNode;
      }

      if (snapshot !== lastSnapshot) {
        const phase = roomId && !canvas ? 'room-requested-no-canvas' : (canvas ? 'canvas-present' : 'hotel-view-or-boot');
        const item = {
          at: now(),
          timestamp: isoNow(),
          phase,
          roomId,
          canvas: !!canvas,
          canvasSize: canvas ? `${canvas.width}x${canvas.height}` : '',
          rootChildren: root?.children?.length || 0,
          lastWsFrame: lastWsFrameIn ? { ...lastWsFrameIn } : null
        };
        lastRoomState = phase;
        push(roomEvents, item);
        debugLog('ROOM', item);
      }
      lastSnapshot = snapshot;
    };
    tick();
    window.setInterval(tick, DEBUG ? 500 : 6000);
  }

  if (DEBUG) {
    try { patchConsoleForNitroParserErrors(); } catch (error) { warn('console-observer-failed', error); }
    try { patchWebSocket(); } catch (error) { warn('websocket-diagnostic-failed', error); }
  } else {
    debugLog('ws', 'native WebSocket preserved; diagnostic wrapper disabled');
  }

  // Existing asset diagnostics remain separate from packet parser diagnostics.
  // They are not treated as the cause of a DataView RangeError.
  try { patchFetchForAssets(); } catch (error) { warn('fetch-patch-failed', error); }
  try { patchImageSources(); } catch (error) { warn('image-patch-failed', error); }
  listenForAssetErrors();
  listenForEscapedDataViewErrors();
  monitorRoomState();
  monitorCanvasRemoval();

  api.websocketMode = websocketMode();
  api.ready = true;
  if (DEBUG) api.dump();
})();