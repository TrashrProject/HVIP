const net = require('net');
const { WebSocketServer, WebSocket } = require('ws');

const WS_HOST = process.env.NITRO_WS_HOST || '127.0.0.1';
const WS_PORT = Number(process.env.NITRO_WS_PORT || 2097);
const TCP_HOST = process.env.EMU_HOST || '127.0.0.1';
const TCP_PORT = Number(process.env.EMU_PORT || 2021);

const wss = new WebSocketServer({ host: WS_HOST, port: WS_PORT, perMessageDeflate: false });

console.log(`[NitroProxy] Listening on ws://${WS_HOST}:${WS_PORT}/`);
console.log(`[NitroProxy] Forwarding to tcp://${TCP_HOST}:${TCP_PORT}`);
console.log('[NitroProxy] Raw transport enabled');

wss.on('connection', (ws, req) => {
  const remote = req.socket.remoteAddress || 'unknown';
  console.log(`[NitroProxy] WebSocket connected from ${remote}`);

  const tcp = net.createConnection({ host: TCP_HOST, port: TCP_PORT, noDelay: true });
  let tcpReady = false;
  const pending = [];

  tcp.on('connect', () => {
    tcpReady = true;
    console.log(`[NitroProxy] TCP connected to emulator ${TCP_HOST}:${TCP_PORT}`);
    while (pending.length) tcp.write(pending.shift());
  });

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

  tcp.on('data', (chunk) => {
    try {
      if (ws.readyState === WebSocket.OPEN && chunk && chunk.length) {
        ws.send(Buffer.from(chunk), { binary: true });
      }
    } catch (err) {
      console.error('[NitroProxy] TCP -> WS error:', err.message);
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
