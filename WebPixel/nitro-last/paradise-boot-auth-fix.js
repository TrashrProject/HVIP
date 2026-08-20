(() => {
  'use strict';

  const VERSION = '86.1.0-single-room-forward-debug';
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

  // The emulator already sends RoomForwardComposer(home_room) after a
  // successful SSO authentication. During room-stability debugging, do not
  // also bootstrap Nitro with forward.id/forward.type: that produced two
  // identical GetGuestRoom + OpenFlatConnection sequences and initialized the
  // same room twice. Keep the requested room id separately for diagnostics.
  if (debugSingleForward && room && room !== '0') {
    window.__PARADISE_REQUESTED_ROOM_ID__ = Number.parseInt(room, 10) || 0;
    window.NitroConfig['forward.type'] = -1;
    window.NitroConfig['forward.id'] = 0;
    console.warn('[ParadiseRP:BOOT] Debug single-room-forward active; waiting for emulator RoomForward.', {
      requestedRoom: window.__PARADISE_REQUESTED_ROOM_ID__
    });
  }

  window.__PARADISE_BOOT_AUTH__ = {
    version: VERSION,
    room,
    hasToken: !!window.NitroConfig['sso.ticket'],
    tokenParam: token ? 'present' : 'missing',
    singleForwardDebug: debugSingleForward,
    sourceKeys: Array.from(query.keys())
  };

  if (room && room !== '0' && !window.NitroConfig['sso.ticket']) {
    console.error('[ParadiseRP:BOOT] Aucun ticket SSO dans l’URL Nitro. Ouvre /play?room=' + room + ' au lieu de /nitro-last/index.html directement.');
  }
})();
