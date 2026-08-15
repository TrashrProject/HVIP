const net = require('net');
const { WebSocketServer, WebSocket } = require('ws');

const WS_HOST = process.env.NITRO_WS_HOST || '127.0.0.1';
const WS_PORT = Number(process.env.NITRO_WS_PORT || 2097);
const TCP_HOST = process.env.EMU_HOST || '127.0.0.1';
const TCP_PORT = Number(process.env.EMU_PORT || 2021);
const MAX_PACKET_SIZE = 8 * 1024 * 1024;

const wss = new WebSocketServer({ host: WS_HOST, port: WS_PORT, perMessageDeflate: false });

console.log(`[NitroProxy] Listening on ws://${WS_HOST}:${WS_PORT}/`);
console.log(`[NitroProxy] Forwarding to tcp://${TCP_HOST}:${TCP_PORT}`);
console.log('[NitroProxy] TCP packet framing enabled');

wss.on('connection', (ws, req) => {
  const remote = req.socket.remoteAddress || 'unknown';
  console.log(`[NitroProxy] WebSocket connected from ${remote}`);

  const tcp = net.createConnection({ host: TCP_HOST, port: TCP_PORT, noDelay: true });
  let tcpReady = false;
  const pending = [];
  let incoming = Buffer.alloc(0);

  tcp.on('connect', () => {
    tcpReady = true;
    console.log(`[NitroProxy] TCP connected to emulator ${TCP_HOST}:${TCP_PORT}`);
    while (pending.length) tcp.write(pending.shift());
  });

  // Nitro already sends complete Habbo frames over WebSocket. Forward them
  // unchanged to the emulator's TCP socket.
  ws.on('message', (data) => {
    try {
      const buffer = Buffer.isBuffer(data) ? Buffer.from(data) : Buffer.from(data);
      if (!buffer.length) return;
      if (tcpReady) tcp.write(buffer);
      else pending.push(buffer);
    } catch (err) {
      console.error('[NitroProxy] WS -> TCP error:', err.message);
    }
  });

  // TCP is a byte stream: one `data` event is NOT guaranteed to equal one
  // Habbo packet. Nitro expects complete protocol frames, so rebuild frames
  // from the 4-byte big-endian length prefix before sending them by WS.
  tcp.on('data', (chunk) => {
    try {
      incoming = incoming.length ? Buffer.concat([incoming, chunk]) : Buffer.from(chunk);

      while (incoming.length >= 4) {
        const declaredLength = incoming.readUInt32BE(0);

        // A Habbo frame contains at least the 2-byte packet header.
        if (declaredLength < 2 || declaredLength > MAX_PACKET_SIZE) {
          console.warn(`[NitroProxy] Invalid TCP frame length ${declaredLength}; falling back to raw forwarding for this chunk.`);
          if (ws.readyState === WebSocket.OPEN && incoming.length) {
            ws.send(Buffer.from(incoming), { binary: true });
          }
          incoming = Buffer.alloc(0);
          return;
        }

        const totalLength = declaredLength + 4;
        if (incoming.length < totalLength) break;

        const packet = Buffer.from(incoming.subarray(0, totalLength));
        incoming = Buffer.from(incoming.subarray(totalLength));

        if (ws.readyState === WebSocket.OPEN) {
          ws.send(packet, { binary: true });
        }
      }
    } catch (err) {
      console.error('[NitroProxy] TCP -> WS framing error:', err.message);
    }
  });

  tcp.on('error', (err) => {
    console.error('[NitroProxy] TCP error:', err.message);
    try { ws.close(1011, 'Emulator connection failed'); } catch (_) {}
  });

  ws.on('error', (err) => {
    console.error('[NitroProxy] WebSocket error:', err.message);
  });

  ws.on('close', () => {
    console.log('[NitroProxy] WebSocket closed');
    try { tcp.destroy(); } catch (_) {}
  });

  tcp.on('close', () => {
    if (ws.readyState === WebSocket.OPEN || ws.readyState === WebSocket.CONNECTING) {
      try { ws.close(); } catch (_) {}
    }
  });
});

wss.on('error', (err) => {
  console.error('[NitroProxy] Server error:', err.message);
  process.exitCode = 1;
});
