(() => {
  'use strict';

  if (window.ParadiseRoomAdapter) return;

  const VERSION = '1.0.0-phase11';
  const EVENT_NAMES = [
    'paradise:nitro-room',
    'paradise:nitro-room-change',
    'nitro:room',
    'nitro:room-change',
    'nitro:room-state',
    'room:session-change'
  ];

  let observer = null;
  let lastSignature = '';
  let destroyed = false;

  const asText = value => {
    if (value === null || value === undefined) return null;
    const text = String(value).replace(/\s+/g, ' ').trim();
    return text || null;
  };

  const asNumber = value => {
    if (value === null || value === undefined || value === '') return null;
    const number = Number(value);
    return Number.isFinite(number) ? number : null;
  };

  function normalize(value) {
    const source = value && typeof value === 'object' ? value : {};
    const detail = source.detail && typeof source.detail === 'object' ? source.detail : source;
    const nested = detail.room && typeof detail.room === 'object' ? detail.room : {};
    const session = detail.roomSession && typeof detail.roomSession === 'object' ? detail.roomSession : {};

    return {
      id: asNumber(detail.room_id ?? detail.roomId ?? detail.id ?? nested.id ?? nested.roomId ?? session.roomId ?? session.id),
      name: asText(detail.room_name ?? detail.roomName ?? detail.caption ?? detail.name ?? nested.caption ?? nested.name ?? session.roomName ?? session.name ?? (typeof detail.room === 'string' ? detail.room : null)),
      district: asText(detail.district ?? detail.zone ?? nested.district ?? nested.zone),
      city: asText(detail.city ?? nested.city),
      playerCount: asNumber(detail.players ?? detail.playerCount ?? nested.players ?? nested.playerCount)
    };
  }

  function commit(room, source) {
    if (destroyed || !window.ParadiseStore?.setRoomSnapshot) return false;
    if (!room || (!room.name && room.id === null)) return false;

    const signature = JSON.stringify([room.id, room.name, room.district, room.city, room.playerCount]);
    if (signature === lastSignature) return false;
    lastSignature = signature;
    window.ParadiseStore.setRoomSnapshot(room, source);
    return true;
  }

  function setRoom(snapshot, source = 'nitro') {
    return commit(normalize(snapshot), source);
  }

  function roomFromExplicitAttributes(root) {
    if (!root?.querySelectorAll) return null;
    const candidates = root.querySelectorAll('[data-room-name], [data-room-id][data-room-name], [data-room-caption]');
    for (const element of candidates) {
      if (element.closest?.('#paradise-ui-root')) continue;
      const room = normalize({
        room_id: element.dataset.roomId,
        room_name: element.dataset.roomName ?? element.dataset.roomCaption,
        district: element.dataset.district ?? element.dataset.zone,
        city: element.dataset.city,
        players: element.dataset.playerCount ?? element.dataset.players
      });
      if (room.name || room.id !== null) return room;
    }
    return null;
  }

  function roomFromNativeInfo(root) {
    if (!root?.querySelectorAll) return null;
    const candidates = root.querySelectorAll([
      '[id*="RoomInfo"]',
      '[id*="room-info"]',
      '[class*="room-info"]',
      '[class*="roomInfo"]',
      '[class*="room-tools"]',
      '[class*="roomTools"]'
    ].join(','));

    for (const element of candidates) {
      if (element.closest?.('#paradise-ui-root')) continue;
      const titleNode = element.querySelector?.('[class*="title"], [class*="caption"], strong, h1, h2, h3');
      const raw = asText(titleNode?.textContent ?? element.getAttribute?.('aria-label') ?? element.getAttribute?.('title'));
      if (!raw || raw.length < 2 || raw.length > 96) continue;
      if (/^(room|appartement|infos?|information|fermer|close)$/i.test(raw)) continue;
      return normalize({ name: raw });
    }
    return null;
  }

  function inspectDom() {
    const root = document.getElementById('root');
    if (!root) return false;
    const explicit = roomFromExplicitAttributes(root);
    if (explicit && commit(explicit, 'nitro-dom')) return true;
    const nativeInfo = roomFromNativeInfo(root);
    if (nativeInfo && commit(nativeInfo, 'nitro-dom')) return true;
    return false;
  }

  function inspectKnownGlobals() {
    const candidates = [
      window.__ParadiseNitroRoomState,
      window.__NitroRoomState,
      window.NitroRoomState,
      window.roomSession,
      window.RoomSession
    ];
    for (const candidate of candidates) {
      if (candidate && setRoom(candidate, 'nitro')) return true;
    }
    return false;
  }

  function onRoomEvent(event) {
    const room = normalize(event);
    if (room.name || room.id !== null) commit(room, 'room-event');
  }

  function boot() {
    if (destroyed) return;
    EVENT_NAMES.forEach(name => window.addEventListener(name, onRoomEvent, false));
    inspectKnownGlobals();
    inspectDom();

    const root = document.getElementById('root');
    if (root && !observer) {
      observer = new MutationObserver(() => inspectDom());
      observer.observe(root, {
        childList: true,
        subtree: true,
        attributes: true,
        attributeFilter: ['data-room-name', 'data-room-id', 'data-room-caption', 'aria-label', 'title']
      });
    }
  }

  function destroy() {
    if (destroyed) return;
    destroyed = true;
    EVENT_NAMES.forEach(name => window.removeEventListener(name, onRoomEvent, false));
    observer?.disconnect();
    observer = null;
  }

  window.ParadiseRoomAdapter = Object.freeze({
    version: VERSION,
    setRoom,
    inspectDom,
    inspectKnownGlobals,
    destroy,
    getStatus: () => ({
      version: VERSION,
      observer: Boolean(observer),
      lastSignature,
      polling: false
    })
  });

  window.addEventListener('beforeunload', destroy, { once: true });
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();