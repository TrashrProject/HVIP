const net = require('net');
const { WebSocketServer, WebSocket } = require('ws');

const WS_HOST = process.env.NITRO_WS_HOST || '127.0.0.1';
const WS_PORT = Number(process.env.NITRO_WS_PORT || 2097);
const TCP_HOST = process.env.EMU_HOST || '127.0.0.1';
const TCP_PORT = Number(process.env.EMU_PORT || 2021);
const MAX_PACKET = 8 * 1024 * 1024;
const TRACE = process.env.NITRO_TRACE !== '0';

const wss = new WebSocketServer({ host: WS_HOST, port: WS_PORT, perMessageDeflate: false });

console.log(`[NitroProxy] Listening on ws://${WS_HOST}:${WS_PORT}/`);
console.log(`[NitroProxy] Forwarding to tcp://${TCP_HOST}:${TCP_PORT}`);
console.log(`[NitroProxy] Packet trace ${TRACE ? 'enabled' : 'disabled'}`);

function describePacket(direction, buffer) {
  if (!TRACE || !buffer || buffer.length < 6) return;
  try {
    const declared = buffer.readUInt32BE(0);
    const header = buffer.readUInt16BE(4);
    const preview = buffer.subarray(0, Math.min(buffer.length, 24)).toString('hex').match(/.{1,2}/g).join(' ');
    console.log(`[TRACE ${direction}] header=${header} payload=${declared} bytes=${buffer.length} hex=${preview}`);
  } catch (_) {}
}

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

  ws.on('message', (data) => {
    try {
      const buffer = Buffer.isBuffer(data) ? data : Buffer.from(data);
      if (!buffer.length) return;
      describePacket('C->S', buffer);
      if (tcpReady) tcp.write(buffer);
      else pending.push(buffer);
    } catch (err) {
      console.error('[NitroProxy] WS -> TCP error:', err.message);
    }
  });

  // The emulator is a TCP stream while Nitro expects complete Habbo packets
  // per WebSocket message. TCP may split one packet across several chunks or
  // merge several packets into one chunk, so rebuild frames using the 4-byte
  // big-endian Habbo length prefix before forwarding them to Nitro.
  tcp.on('data', (chunk) => {
    incoming = incoming.length ? Buffer.concat([incoming, chunk]) : chunk;

    while (incoming.length >= 4) {
      const payloadLength = incoming.readUInt32BE(0);

      if (payloadLength <= 0 || payloadLength > MAX_PACKET) {
        console.error(`[NitroProxy] Invalid packet length ${payloadLength}; closing connection to avoid corrupting Nitro.`);
        incoming = Buffer.alloc(0);
        try { tcp.destroy(); } catch (_) {}
        try { ws.close(1002, 'Invalid emulator packet framing'); } catch (_) {}
        return;
      }

      const frameLength = payloadLength + 4;
      if (incoming.length < frameLength) break;

      const packet = incoming.subarray(0, frameLength);
      incoming = incoming.subarray(frameLength);
      describePacket('S->C', packet);

      if (ws.readyState === WebSocket.OPEN) {
        ws.send(packet, { binary: true });
      }
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
