const net = require('net');
const { WebSocketServer, WebSocket } = require('ws');

const WS_HOST = process.env.NITRO_WS_HOST || '127.0.0.1';
const WS_PORT = Number(process.env.NITRO_WS_PORT || 2097);
const TCP_HOST = process.env.EMU_HOST || '127.0.0.1';
const TCP_PORT = Number(process.env.EMU_PORT || 2021);

// RDP/Nitro packet framing confirmed in GamePacketParser.cs and Nitro's
// EvaWire codec:
// [4-byte signed big-endian content length][2-byte packet header][payload]
// The 4-byte prefix is NOT included in the announced length.
const LENGTH_PREFIX_BYTES = 4;
const MIN_CONTENT_LENGTH = 2;
// Server -> client room payloads can be much larger than the emulator's old
// client -> server 5120-byte guard, so keep a corruption guard without
// imposing that legacy incoming limit on outgoing packets.
const MAX_CONTENT_LENGTH = 16 * 1024 * 1024;
const DEBUG_FRAMING = process.env.NITRO_PROXY_DEBUG !== '0';

const wss = new WebSocketServer({ host: WS_HOST, port: WS_PORT, perMessageDeflate: false });

console.log(`[NitroProxy] Listening on ws://${WS_HOST}:${WS_PORT}/`);
console.log(`[NitroProxy] Forwarding to tcp://${TCP_HOST}:${TCP_PORT}`);
console.log('[NitroProxy] Packet-aware TCP -> WebSocket transport enabled');
console.log(`[NitroProxy] Framing: ${LENGTH_PREFIX_BYTES}-byte BE length prefix, max content ${MAX_CONTENT_LENGTH} bytes`);

function framingLog(message) {
  if (DEBUG_FRAMING) console.log(message);
}

function fnv1a32(buffer) {
  let hash = 0x811c9dc5;
  for (let i = 0; i < buffer.length; i += 1) {
    hash ^= buffer[i];
    hash = Math.imul(hash, 0x01000193) >>> 0;
  }
  return hash.toString(16).padStart(8, '0');
}

function edgeHex(buffer, fromStart) {
  const count = Math.min(32, buffer.length);
  if (!count) return '';
  const slice = fromStart ? buffer.subarray(0, count) : buffer.subarray(buffer.length - count);
  return slice.toString('hex').match(/.{1,2}/g)?.join(' ') || '';
}

wss.on('connection', (ws, req) => {
  const remote = req.socket.remoteAddress || 'unknown';
  console.log(`[NitroProxy] WebSocket connected from ${remote}`);

  const tcp = net.createConnection({ host: TCP_HOST, port: TCP_PORT, noDelay: true });
  let tcpReady = false;
  let tcpBuffer = Buffer.alloc(0);
  let outSequence = 0;
  const pending = [];

  const closeForFramingError = (reason) => {
    console.error(`[WS PROXY] ${reason}`);
    tcpBuffer = Buffer.alloc(0);
    try {
      if (ws.readyState === WebSocket.OPEN || ws.readyState === WebSocket.CONNECTING) {
        ws.close(1002, 'Invalid Nitro packet framing');
      }
    } catch (_) {}
    try { tcp.destroy(); } catch (_) {}
  };

  const flushTcpFramesToWebSocket = () => {
    while (tcpBuffer.length >= LENGTH_PREFIX_BYTES) {
      const contentLength = tcpBuffer.readInt32BE(0);

      if (contentLength < MIN_CONTENT_LENGTH || contentLength > MAX_CONTENT_LENGTH) {
        const prefix = tcpBuffer.subarray(0, Math.min(16, tcpBuffer.length)).toString('hex');
        closeForFramingError(
          `Invalid Nitro packet length: ${contentLength}; available=${tcpBuffer.length}; prefix=${prefix}; at=${new Date().toISOString()}`
        );
        return;
      }

      const totalLength = LENGTH_PREFIX_BYTES + contentLength;

      if (tcpBuffer.length < totalLength) {
        framingLog(`[WS PROXY WAIT] need=${totalLength} available=${tcpBuffer.length}`);
        return;
      }

      if (ws.readyState !== WebSocket.OPEN) {
        try { tcp.destroy(); } catch (_) {}
        return;
      }

      // Copy exactly one complete Nitro packet. No header, length or payload
      // bytes are altered; only TCP stream boundaries are reconstructed.
      const frame = Buffer.from(tcpBuffer.subarray(0, totalLength));
      const remainingLength = tcpBuffer.length - totalLength;
      tcpBuffer = remainingLength > 0
        ? Buffer.from(tcpBuffer.subarray(totalLength))
        : Buffer.alloc(0);

      const frameId = ++outSequence;
      const actualWsPayloadLength = frame.byteLength;
      const packetHeader = frame.length >= 6 ? frame.readUInt16BE(4) : null;
      const hash = fnv1a32(frame);
      const first32 = edgeHex(frame, true);
      const last32 = edgeHex(frame, false);
      const timestamp = new Date().toISOString();
      const lengthInvariant = contentLength + LENGTH_PREFIX_BYTES === actualWsPayloadLength;

      framingLog(
        `[WS PROXY FRAME] contentLength=${contentLength} totalLength=${totalLength} remainingBuffer=${tcpBuffer.length}`
      );
      framingLog(
        `[WS → NITRO #${frameId}] timestamp=${timestamp} ` +
        `declaredContentLength=${contentLength} totalTcpPacketLength=${totalLength} ` +
        `actualWsPayloadLength=${actualWsPayloadLength} headerBE=${packetHeader} ` +
        `byteOffset=${frame.byteOffset} byteLength=${frame.byteLength} ` +
        `underlyingArrayBufferLength=${frame.buffer.byteLength} ` +
        `lengthInvariant=${lengthInvariant} fnv1a32=${hash} ` +
        `first32=${first32} last32=${last32}`
      );

      // Important: send the Buffer VIEW itself, never frame.buffer. A Node
      // Buffer can use a larger pooled backing ArrayBuffer. ws.send(frame)
      // sends only frame.byteOffset..frame.byteOffset+frame.byteLength.
      ws.send(frame, { binary: true }, (err) => {
        if (err) {
          console.error(`[NitroProxy] WebSocket send error OUT #${frameId}:`, err.message);
          return;
        }
        framingLog(
          `[WS → NITRO #${frameId} SENT] actualWsPayloadLength=${actualWsPayloadLength} fnv1a32=${hash}`
        );
      });
    }

    if (tcpBuffer.length > 0) {
      framingLog(`[WS PROXY WAIT] needHeader=${LENGTH_PREFIX_BYTES} available=${tcpBuffer.length}`);
    }
  };

  tcp.on('connect', () => {
    tcpReady = true;
    console.log(`[NitroProxy] TCP connected to emulator ${TCP_HOST}:${TCP_PORT}`);
    while (pending.length) tcp.write(pending.shift());
  });

  // Browser -> emulator: keep the WebSocket message bytes unchanged.
  // GamePacketParser on the emulator already owns the TCP stream reassembly
  // and can parse one packet, multiple packets, or fragmented TCP writes.
  ws.on('message', (data, isBinary) => {
    try {
      const buffer = Buffer.from(data);
      if (!buffer.length) return;

      framingLog(
        `[WS PROXY TX] wsFrame=${buffer.length} binary=${Boolean(isBinary)} tcpReady=${tcpReady}`
      );

      if (tcpReady) tcp.write(buffer);
      else pending.push(buffer);
    } catch (err) {
      console.error('[NitroProxy] WS -> TCP error:', err.message);
    }
  });

  // Emulator -> browser: TCP is a byte stream, so a data event is NOT a
  // packet boundary. Accumulate bytes and emit one WebSocket message per
  // complete Nitro packet.
  tcp.on('data', (chunk) => {
    try {
      if (!chunk || !chunk.length) return;

      const bufferBefore = tcpBuffer.length;
      tcpBuffer = bufferBefore > 0
        ? Buffer.concat([tcpBuffer, chunk], bufferBefore + chunk.length)
        : Buffer.from(chunk);

      framingLog(
        `[WS PROXY RX] tcpChunk=${chunk.length} bufferBefore=${bufferBefore} bufferAfter=${tcpBuffer.length}`
      );

      flushTcpFramesToWebSocket();
    } catch (err) {
      console.error('[NitroProxy] TCP -> WS framing error:', err.message);
      try { ws.close(1011, 'Nitro packet framing error'); } catch (_) {}
      try { tcp.destroy(); } catch (_) {}
    }
  });

  tcp.on('error', (err) => {
    console.error('[NitroProxy] TCP error:', err.message);
    try { ws.close(1011, 'Emulator connection failed'); } catch (_) {}
  });

  ws.on('error', (err) => {
    console.error('[NitroProxy] WebSocket error:', err.message);
  });

  ws.on('close', (code, reason) => {
    console.log(`[NitroProxy] WebSocket closed code=${code} reason=${reason?.toString?.() || ''}`);
    tcpBuffer = Buffer.alloc(0);
    try { tcp.destroy(); } catch (_) {}
  });

  tcp.on('close', () => {
    if (tcpBuffer.length > 0) {
      console.warn(`[WS PROXY] TCP closed with ${tcpBuffer.length} unconsumed byte(s) in framing buffer`);
    }
    if (ws.readyState === WebSocket.OPEN || ws.readyState === WebSocket.CONNECTING) {
      try { ws.close(); } catch (_) {}
    }
  });
});

wss.on('error', (err) => {
  console.error('[NitroProxy] Server error:', err.message);
  process.exitCode = 1;
});