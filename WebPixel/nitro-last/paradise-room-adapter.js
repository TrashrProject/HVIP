(() => {
  'use strict';

  if (window.ParadiseRoomAdapter) return;

  const VERSION = '2.0.0-quality-gate';
  const EVENT_NAMES = [
    'paradise:nitro-room',
    'paradise:nitro-room-change',
    'nitro:room',
    'nitro:room-change',
    'nitro:room-state',
    'room:session-change'
  ];
  const BURST_DELAYS = [0, 140, 420, 900, 1700, 3000];
  const BLACKLIST = /^(room|appartement|infos?|information|fermer|close|inventaire|profil|documents|téléphone|telephone|actions|local|paradise|paradiserp|navigator|navigation|catalogue|boutique)$/i;

  let lastSignature = '';
  let destroyed = false;
  let lastSource = null;
  let lastResolvedAt = null;
  const timers = new Set();

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

  function validRoomName(value) {
    const raw = asText(value);
    if (!raw || raw.length < 2 || raw.length > 96 || BLACKLIST.test(raw)) return null;
    if (/^[\d\s:./-]+$/.test(raw)) return null;
    return raw;
  }

  function commit(room, source) {
    if (destroyed || !window.ParadiseStore?.setRoomSnapshot) return false;
    if (!room) return false;
    room.name = validRoomName(room.name);
    if (!room.name && room.id === null) return false;

    const signature = JSON.stringify([room.id, room.name, room.district, room.city, room.playerCount]);
    if (signature === lastSignature) return false;
    lastSignature = signature;
    lastSource = source;
    lastResolvedAt = new Date().toISOString();
    window.ParadiseStore.setRoomSnapshot(room, source);
    return true;
  }

  function setRoom(snapshot, source = 'nitro') {
    return commit(normalize(snapshot), source);
  }

  function roomFromExplicitAttributes(root) {
    if (!root?.querySelectorAll) return null;
    const candidates = root.querySelectorAll('[data-room-name],[data-room-caption],[data-room-id][data-room-name],[data-room-id][data-room-caption]');
    for (const element of candidates) {
      if (element.closest?.('#paradise-ui-root')) continue;
      const room = normalize({
        room_id: element.dataset.roomId,
        room_name: element.dataset.roomName ?? element.dataset.roomCaption,
        district: element.dataset.district ?? element.dataset.zone,
        city: element.dataset.city,
        players: element.dataset.playerCount ?? element.dataset.players
      });
      room.name = validRoomName(room.name);
      if (room.name || room.id !== null) return room;
    }
    return null;
  }

  function candidateText(element) {
    if (!element) return null;
    const preferred = element.querySelector?.('[class*="caption"],[class*="title"],[class*="name"],strong,h1,h2,h3');
    const values = [
      preferred?.textContent,
      element.getAttribute?.('data-room-name'),
      element.getAttribute?.('data-room-caption'),
      element.getAttribute?.('aria-label'),
      element.getAttribute?.('title'),
      element.childElementCount <= 2 ? element.textContent : null
    ];
    for (const value of values) {
      const name = validRoomName(value);
      if (name) return name;
    }
    return null;
  }

  function roomFromNativeInfo(root) {
    if (!root?.querySelectorAll) return null;
    const selectors = [
      '[id*="RoomInfo"]','[id*="room-info"]',
      '[class*="room-info"]','[class*="roomInfo"]',
      '[class*="room-name"]','[class*="roomName"]',
      '[class*="room-caption"]','[class*="roomCaption"]',
      '[class*="room-tools"]','[class*="roomTools"]',
      '[aria-label*="Room"]','[aria-label*="room"]','[aria-label*="appartement"]'
    ];
    const candidates = root.querySelectorAll(selectors.join(','));
    for (const element of candidates) {
      if (element.closest?.('#paradise-ui-root')) continue;
      const rect = element.getBoundingClientRect?.();
      if (rect && (rect.width === 0 || rect.height === 0)) continue;
      const name = candidateText(element);
      if (name) return normalize({ name });
    }
    return null;
  }

  function inspectDom() {
    if (destroyed) return false;
    const root = document.getElementById('root');
    if (!root) return false;
    const explicit = roomFromExplicitAttributes(root);
    if (explicit && commit(explicit, 'nitro-dom-explicit')) return true;
    const nativeInfo = roomFromNativeInfo(root);
    if (nativeInfo && commit(nativeInfo, 'nitro-dom-visible')) return true;
    return false;
  }

  function inspectKnownGlobals() {
    const candidates = [
      window.__ParadiseNitroRoomState,
      window.__NitroRoomState,
      window.NitroRoomState,
      window.roomSession,
      window.RoomSession,
      window.currentRoom,
      window.CurrentRoom
    ];
    for (const candidate of candidates) {
      if (candidate && setRoom(candidate, 'nitro-global')) return true;
    }
    return false;
  }

  function inspectAll() {
    return inspectKnownGlobals() || inspectDom();
  }

  function clearBursts() {
    timers.forEach(timer => window.clearTimeout(timer));
    timers.clear();
  }

  function scheduleBurst(reason = 'transition') {
    if (destroyed) return;
    BURST_DELAYS.forEach(delay => {
      const timer = window.setTimeout(() => {
        timers.delete(timer);
        inspectAll();
      }, delay);
      timers.add(timer);
    });
    window.dispatchEvent(new CustomEvent('paradise:room-scan-scheduled', { detail: { reason } }));
  }

  function onRoomEvent(event) {
    const room = normalize(event);
    if (room.name || room.id !== null) commit(room, 'room-event');
    scheduleBurst('room-event');
  }

  function onRootInteraction(event) {
    if (destroyed) return;
    const target = event.target;
    if (target instanceof Element && target.closest('#paradise-ui-root')) {
      if (target.id === 'pr4-chat-input' && event.type === 'keyup' && event.key === 'Enter') scheduleBurst('chat-transition');
      return;
    }
    if (event.type === 'click' || event.type === 'pointerup') scheduleBurst('nitro-interaction');
  }

  function boot() {
    if (destroyed) return;
    EVENT_NAMES.forEach(name => window.addEventListener(name, onRoomEvent, false));
    window.addEventListener('popstate', () => scheduleBurst('history'), false);
    window.addEventListener('hashchange', () => scheduleBurst('history'), false);
    document.addEventListener('click', onRootInteraction, true);
    document.addEventListener('pointerup', onRootInteraction, true);
    document.addEventListener('keyup', onRootInteraction, true);
    scheduleBurst('boot');
  }

  function destroy() {
    if (destroyed) return;
    destroyed = true;
    clearBursts();
    EVENT_NAMES.forEach(name => window.removeEventListener(name, onRoomEvent, false));
    document.removeEventListener('click', onRootInteraction, true);
    document.removeEventListener('pointerup', onRootInteraction, true);
    document.removeEventListener('keyup', onRootInteraction, true);
  }

  window.ParadiseRoomAdapter = Object.freeze({
    version: VERSION,
    setRoom,
    inspectDom,
    inspectKnownGlobals,
    scan: inspectAll,
    rescan: scheduleBurst,
    destroy,
    getStatus: () => ({
      version: VERSION,
      observer: false,
      polling: false,
      pendingScans: timers.size,
      lastSignature,
      lastSource,
      lastResolvedAt
    })
  });

  window.addEventListener('beforeunload', destroy, { once: true });
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();