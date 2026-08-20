(() => {
  'use strict';

  const VERSION = '86.0.0-auth-param-guard';
  const query = new URLSearchParams(window.location.search || '');
  const token =
    query.get('sso') ||
    query.get('ticket') ||
    query.get('auth') ||
    query.get('s') ||
    query.get('client') ||
    null;

  const room = query.get('room') || '0';

  if (!window.NitroConfig) window.NitroConfig = {};

  if (!window.NitroConfig['sso.ticket'] && token) {
    window.NitroConfig['sso.ticket'] = token;
  }

  window.__PARADISE_BOOT_AUTH__ = {
    version: VERSION,
    room,
    hasToken: !!window.NitroConfig['sso.ticket'],
    tokenParam: token ? 'present' : 'missing',
    sourceKeys: Array.from(query.keys())
  };

  if (room && room !== '0' && !window.NitroConfig['sso.ticket']) {
    console.error('[ParadiseRP:BOOT] Aucun ticket SSO dans l’URL Nitro. Ouvre /play?room=' + room + ' au lieu de /nitro-last/index.html directement.');
  }
})();
