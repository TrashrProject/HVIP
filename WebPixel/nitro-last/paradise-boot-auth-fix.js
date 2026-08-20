(() => {
  'use strict';

  const VERSION = '86.2.0-neutral-room-forward-debug';
  const query = new URLSearchParams(window.location.search || '');
  const token =
    query.get('sso') ||
    query.get('ticket') ||
    query.get('auth') ||
    query.get('s') ||
    query.get('client') ||
    null;

  const room = query.get('room') || '0';
  const debugSingleForward =
    /(?:^|[?&])prdebug=1(?:&|$)/.test(window.location.search || '') ||
    localStorage.getItem('pr_nitro_debug') === '1';

  if (!window.NitroConfig) window.NitroConfig = {};

  if (!window.NitroConfig['sso.ticket'] && token) {
    window.NitroConfig['sso.ticket'] = token;
  }

  // The emulator already sends RoomForwardComposer after successful SSO.
  // Nitro treats forward.type === -1 as an instruction to use roomIdToEnter,
  // which creates another room session in parallel with the emulator's
  // RoomForward path. Use a neutral value (0) during this debug so neither
  // forward.id nor roomIdToEnter auto-entry is triggered by the client.
  // The emulator RoomForward is then the sole room-session creator.
  if (debugSingleForward && room && room !== '0') {
    window.__PARADISE_REQUESTED_ROOM_ID__ = Number.parseInt(room, 10) || 0;
    window.NitroConfig['forward.type'] = 0;
    window.NitroConfig['forward.id'] = 0;
    console.warn('[ParadiseRP:BOOT] Neutral room-forward debug active; waiting only for emulator RoomForward.', {
      requestedRoom: window.__PARADISE_REQUESTED_ROOM_ID__,
      forwardType: window.NitroConfig['forward.type'],
      forwardId: window.NitroConfig['forward.id']
    });
  }

  window.__PARADISE_BOOT_AUTH__ = {
    version: VERSION,
    room,
    hasToken: !!window.NitroConfig['sso.ticket'],
    tokenParam: token ? 'present' : 'missing',
    singleForwardDebug: debugSingleForward,
    neutralForward: debugSingleForward,
    sourceKeys: Array.from(query.keys())
  };

  if (room && room !== '0' && !window.NitroConfig['sso.ticket']) {
    console.error('[ParadiseRP:BOOT] Aucun ticket SSO dans l’URL Nitro. Ouvre /play?room=' + room + ' au lieu de /nitro-last/index.html directement.');
  }
})();
