(function () {
  if (window.__nitroPacketDebuggerInstalled) return;
  window.__nitroPacketDebuggerInstalled = true;

  const NativeWebSocket = window.WebSocket;
  const history = [];
  const MAX_HISTORY = 40;

  function parseBuffer(buffer) {
    try {
      if (!(buffer instanceof ArrayBuffer) || buffer.byteLength < 6) return null;
      const view = new DataView(buffer);
      const payload = view.getUint32(0, false);
      const header = view.getUint16(4, false);
      const bytes = new Uint8Array(buffer, 0, Math.min(buffer.byteLength, 64));
      const hex = Array.from(bytes).map(v => v.toString(16).padStart(2, '0')).join(' ');
      return { header, payload, bytes: buffer.byteLength, hex, at: Math.round(performance.now()) };
    } catch (_) {
      return null;
    }
  }

  function remember(packet) {
    if (!packet) return;
    window.__lastNitroPacket = packet;
    history.push(packet);
    if (history.length > MAX_HISTORY) history.shift();
    window.__nitroPacketHistory = history;
  }

  class DebugWebSocket extends NativeWebSocket {
    constructor(...args) {
      super(...args);
      this.addEventListener('message', event => {
        if (event.data instanceof ArrayBuffer) {
          remember(parseBuffer(event.data));
        } else if (event.data instanceof Blob) {
          event.data.arrayBuffer().then(buf => remember(parseBuffer(buf))).catch(() => {});
        }
      }, true);
    }
  }

  window.WebSocket = DebugWebSocket;

  const native = {
    log: console.log.bind(console),
    warn: console.warn.bind(console),
    error: console.error.bind(console)
  };

  let reporting = false;
  function maybeReport(method, args) {
    if (reporting) return;
    const text = args.map(v => {
      try { return typeof v === 'string' ? v : (v && v.stack) || JSON.stringify(v); }
      catch (_) { return String(v); }
    }).join(' ');

    if (!/(Error parsing message|DataView|Offset is outside the bounds|reading ['\"]slice['\"])/i.test(text)) return;

    reporting = true;
    const last = window.__lastNitroPacket || null;
    const last12 = history.slice(-12);
    native.error('[NitroDebug] PARSER_ERROR_LAST_PACKET_JSON=' + JSON.stringify(last));
    native.error('[NitroDebug] LAST_12_PACKETS_JSON=' + JSON.stringify(last12));
    if (last) {
      native.error('[NitroDebug] SUSPECT_HEADER=' + last.header + ' PAYLOAD=' + last.payload + ' BYTES=' + last.bytes);
    }
    reporting = false;
  }

  console.log = function (...args) { native.log(...args); maybeReport('log', args); };
  console.warn = function (...args) { native.warn(...args); maybeReport('warn', args); };
  console.error = function (...args) { native.error(...args); maybeReport('error', args); };

  native.log('[NitroDebug] Packet debugger enabled');
})();
