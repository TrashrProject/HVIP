const fs = require('fs');
const path = require('path');
const net = require('net');
const { WebSocketServer, WebSocket } = require('ws');

const WS_HOST = process.env.NITRO_WS_HOST || '127.0.0.1';
const WS_PORT = Number(process.env.NITRO_WS_PORT || 2097);
const TCP_HOST = process.env.EMU_HOST || '127.0.0.1';
const TCP_PORT = Number(process.env.EMU_PORT || 2021);
const MAX_PACKET = 8 * 1024 * 1024;
const TRACE = process.env.NITRO_TRACE !== '0';
const TRACE_FILE = path.join(__dirname, 'trace.log');
const DROP_HEADERS = new Set(
  String(process.env.NITRO_DROP_HEADERS || '')
    .split(',')
    .map(v => Number(v.trim()))
    .filter(v => Number.isInteger(v) && v > 0)
);

try { fs.writeFileSync(TRACE_FILE, '', 'utf8'); } catch (_) {}

function log(line) {
  console.log(line);
  try { fs.appendFileSync(TRACE_FILE, `${new Date().toISOString()} ${line}\n`, 'utf8'); } catch (_) {}
}

const wss = new WebSocketServer({ host: WS_HOST, port: WS_PORT, perMessageDeflate: false });
let activeSession = null;
let sessionCounter = 0;

log(`[NitroProxy] Listening on ws://${WS_HOST}:${WS_PORT}/`);
log(`[NitroProxy] Forwarding to tcp://${TCP_HOST}:${TCP_PORT}`);
log(`[NitroProxy] Packet trace ${TRACE ? 'enabled' : 'disabled'}`);
log(`[NitroProxy] Trace file: ${TRACE_FILE}`);
log(`[NitroProxy] Compatibility drop headers: ${[...DROP_HEADERS].join(', ') || 'none'}`);

function packetInfo(buffer) {
  if (!buffer || buffer.length < 6) return null;
  try { return { declared: buffer.readUInt32BE(0), header: buffer.readUInt16BE(4) }; }
  catch (_) { return null; }
}

function describePacket(direction, buffer, sessionId) {
  if (!TRACE || !buffer || buffer.length < 6) return;
  try {
    const declared = buffer.readUInt32BE(0);
    const header = buffer.readUInt16BE(4);
    const preview = buffer.subarray(0, Math.min(buffer.length, 32)).toString('hex').match(/.{1,2}/g).join(' ');
    log(`[TRACE ${direction}] session=${sessionId} header=${header} payload=${declared} bytes=${buffer.length} hex=${preview}`);
  } catch (_) {}
}

wss.on('connection', (ws, req) => {
  const remote = req.socket.remoteAddress || 'unknown';
  const sessionId = ++sessionCounter;

  if (activeSession) {
    log(`[NitroProxy] Replacing previous session ${activeSession.id} with ${sessionId}`);
    try { activeSession.ws.terminate(); } catch (_) {}
    try { activeSession.tcp.destroy(); } catch (_) {}
    activeSession = null;
  }

  log(`[NitroProxy] Session ${sessionId}: WebSocket connected from ${remote}`);
  const tcp = net.createConnection({ host: TCP_HOST, port: TCP_PORT, noDelay: true });
  activeSession = { id: sessionId, ws, tcp };

  let tcpReady = false;
  const pending = [];
  let incoming = Buffer.alloc(0);

  tcp.on('connect', () => {
    tcpReady = true;
    log(`[NitroProxy] Session ${sessionId}: TCP connected to emulator ${TCP_HOST}:${TCP_PORT}`);
    while (pending.length) tcp.write(pending.shift());
  });

  ws.on('message', data => {
    try {
      const buffer = Buffer.from(data);
      if (!buffer.length) return;
      describePacket('C->S', buffer, sessionId);
      if (tcpReady) tcp.write(buffer); else pending.push(buffer);
    } catch (err) {
      log(`[NitroProxy] Session ${sessionId}: WS -> TCP error: ${err.message}`);
    }
  });

  tcp.on('data', chunk => {
    incoming = incoming.length ? Buffer.concat([incoming, chunk]) : Buffer.from(chunk);

    while (incoming.length >= 4) {
      const payloadLength = incoming.readUInt32BE(0);
      if (payloadLength <= 0 || payloadLength > MAX_PACKET) {
        log(`[NitroProxy] Session ${sessionId}: invalid packet length ${payloadLength}`);
        incoming = Buffer.alloc(0);
        try { tcp.destroy(); } catch (_) {}
        try { ws.close(1002, 'Invalid emulator packet framing'); } catch (_) {}
        return;
      }

      const frameLength = payloadLength + 4;
      if (incoming.length < frameLength) break;

      const packet = Buffer.from(incoming.subarray(0, frameLength));
      incoming = Buffer.from(incoming.subarray(frameLength));
      describePacket('S->C', packet, sessionId);

      const info = packetInfo(packet);
      if (info && DROP_HEADERS.has(info.header)) {
        log(`[COMPAT] Session ${sessionId}: dropped S->C header=${info.header} payload=${info.declared}`);
        continue;
      }

      if (ws.readyState === WebSocket.OPEN) {
        ws.send(packet, { binary: true }, err => {
          if (err) log(`[NitroProxy] Session ${sessionId}: send error: ${err.message}`);
        });
      }
    }
  });

  tcp.on('error', err => {
    log(`[NitroProxy] Session ${sessionId}: TCP error: ${err.message}`);
    try { ws.close(1011, 'Emulator connection failed'); } catch (_) {}
  });

  ws.on('error', err => log(`[NitroProxy] Session ${sessionId}: WebSocket error: ${err.message}`));

  const cleanup = () => {
    if (activeSession && activeSession.id === sessionId) activeSession = null;
    try { tcp.destroy(); } catch (_) {}
  };

  ws.on('close', () => {
    log(`[NitroProxy] Session ${sessionId}: WebSocket closed`);
    cleanup();
  });

  tcp.on('close', () => {
    log(`[NitroProxy] Session ${sessionId}: TCP closed`);
    if (ws.readyState === WebSocket.OPEN || ws.readyState === WebSocket.CONNECTING) {
      try { ws.close(); } catch (_) {}
    }
    if (activeSession && activeSession.id === sessionId) activeSession = null;
  });
});

wss.on('error', err => {
  log(`[NitroProxy] Server error: ${err.message}`);
  process.exitCode = 1;
});
