(() => {
  'use strict';

  const DEBUG = /(?:^|[?&])prdebug=1(?:&|$)/.test(location.search) || localStorage.getItem('pr_nitro_debug') === '1';
  if (!DEBUG || !window.WebSocket || window.WebSocket.__paradiseRoomTracePatched) return;

  const VERSION = '1.0.0-room-send-stack';
  const BaseWebSocket = window.WebSocket;
  const traces = [];
  const TARGETS = new Set([2230, 2312]);
  const NAMES = {
    2230: 'GetGuestRoom',
    2312: 'OpenFlatConnection'
  };

  const remember = item => {
    traces.push(item);
    while (traces.length > 80) traces.shift();
    console.error('[ParadiseRP:ROOM SEND TRACE]', item);
  };

  const parseHeaderSync = data => {
    try {
      let buffer = null;
      let byteOffset = 0;
      let byteLength = 0;

      if (data instanceof ArrayBuffer) {
        buffer = data;
        byteLength = data.byteLength;
      } else if (ArrayBuffer.isView(data)) {
        buffer = data.buffer;
        byteOffset = data.byteOffset;
        byteLength = data.byteLength;
      } else {
        return null;
      }

      if (!buffer || byteLength < 6) return null;
      return new DataView(buffer, byteOffset, byteLength).getUint16(4, false);
    } catch (_) {
      return null;
    }
  };

  const inspectBlob = async (data, stack, at) => {
    try {
      if (!(data instanceof Blob) || data.size < 6) return;
      const buffer = await data.arrayBuffer();
      if (buffer.byteLength < 6) return;
      const header = new DataView(buffer).getUint16(4, false);
      if (!TARGETS.has(header)) return;
      remember({
        version: VERSION,
        at,
        timestamp: new Date().toISOString(),
        header,
        packet: NAMES[header] || String(header),
        byteLength: buffer.byteLength,
        dataType: 'Blob',
        stack
      });
    } catch (_) {}
  };

  function ParadiseRoomTraceWebSocket(url, protocols) {
    const socket = protocols !== undefined ? new BaseWebSocket(url, protocols) : new BaseWebSocket(url);
    const baseSend = socket.send.bind(socket);

    socket.send = data => {
      const stack = String(new Error('ParadiseRP room send origin').stack || '');
      const at = Math.round(performance.now());
      const header = parseHeaderSync(data);

      if (TARGETS.has(header)) {
        remember({
          version: VERSION,
          at,
          timestamp: new Date().toISOString(),
          header,
          packet: NAMES[header] || String(header),
          byteLength: data?.byteLength ?? data?.length ?? null,
          dataType: data?.constructor?.name || typeof data,
          stack
        });
      } else if (data instanceof Blob) {
        inspectBlob(data, stack, at);
      }

      return baseSend(data);
    };

    return socket;
  }

  ParadiseRoomTraceWebSocket.prototype = BaseWebSocket.prototype;
  Object.setPrototypeOf(ParadiseRoomTraceWebSocket, BaseWebSocket);
  Object.defineProperties(ParadiseRoomTraceWebSocket, {
    CONNECTING: { value: BaseWebSocket.CONNECTING },
    OPEN: { value: BaseWebSocket.OPEN },
    CLOSING: { value: BaseWebSocket.CLOSING },
    CLOSED: { value: BaseWebSocket.CLOSED }
  });
  ParadiseRoomTraceWebSocket.__paradiseRoomTracePatched = true;
  ParadiseRoomTraceWebSocket.__paradiseWrapped = BaseWebSocket;

  window.WebSocket = ParadiseRoomTraceWebSocket;
  window.__ParadiseRoomSendTrace = {
    version: VERSION,
    traces,
    dump() {
      console.table(traces.map((item, index) => ({
        index,
        at: item.at,
        header: item.header,
        packet: item.packet,
        bytes: item.byteLength,
        dataType: item.dataType
      })));
      return traces.slice();
    }
  };

  console.warn('[ParadiseRP:ROOM SEND TRACE] active', VERSION);
})();
