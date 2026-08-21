(() => {
  'use strict';

  if (window.ParadiseRoomReconciler) return;

  const VERSION = '1.0.0-quality-gate';
  const WEAK_SOURCES = new Set(['nitro-dom-visible', 'nitro-dom']);
  const WEAK_TTL_MS = 2500;
  const CONFIRM_THROTTLE_MS = 4500;
  let lastConfirmAt = 0;
  let confirmTimer = 0;
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

  function serverRoom(payload) {
    if (!payload || typeof payload !== 'object' || payload.ok === false) return null;
    const room = {
      id: asNumber(payload.room_id),
      name: asText(payload.room_name ?? payload.room),
      district: asText(payload.district),
      city: asText(payload.city),
      playerCount: asNumber(payload.players)
    };
    return room.name || room.id !== null ? room : null;
  }

  function roomAgeMs(meta) {
    const stamp = Date.parse(meta?.roomUpdatedAt || '');
    return Number.isFinite(stamp) ? Math.max(0, Date.now() - stamp) : Number.POSITIVE_INFINITY;
  }

  function sameRoom(a, b) {
    if (!a || !b) return false;
    const aId = asNumber(a.id);
    const bId = asNumber(b.id);
    if (aId !== null && bId !== null) return aId === bId;
    const aName = asText(a.name)?.toLocaleLowerCase('fr-FR');
    const bName = asText(b.name)?.toLocaleLowerCase('fr-FR');
    return Boolean(aName && bName && aName === bName);
  }

  function reconcile(payload) {
    if (destroyed || !window.ParadiseStore?.setRoomSnapshot) return false;
    const confirmed = serverRoom(payload);
    if (!confirmed) return false;

    const store = window.ParadiseStore.getState?.();
    if (!store) return false;
    const current = store.gameplay?.room || {};
    const source = asText(store.meta?.roomSource) || '';
    const age = roomAgeMs(store.meta);

    if (sameRoom(current, confirmed)) {
      // A server-confirmed match promotes a weak DOM candidate to a durable source.
      if (WEAK_SOURCES.has(source) && age >= WEAK_TTL_MS) {
        window.ParadiseStore.setRoomSnapshot(confirmed, 'rp-hud-data-confirmed');
        return true;
      }
      return false;
    }

    const currentMissing = !asText(current.name) && asNumber(current.id) === null;
    const weakExpired = WEAK_SOURCES.has(source) && age >= WEAK_TTL_MS;
    const serverCanDisambiguate = asNumber(confirmed.id) !== null && asNumber(current.id) !== null && asNumber(confirmed.id) !== asNumber(current.id);
    const nonLiveSource = !/^(nitro|room-event)/i.test(source) || source === 'rp-hud-data' || source === 'rp-hud-data-confirmed';

    if (currentMissing || weakExpired || serverCanDisambiguate || nonLiveSource) {
      window.ParadiseStore.setRoomSnapshot(confirmed, 'rp-hud-data-confirmed');
      return true;
    }
    return false;
  }

  function requestConfirmation(reason = 'room-confirm') {
    if (destroyed || !window.ParadiseBridge?.refresh) return false;
    const now = Date.now();
    if (now - lastConfirmAt < CONFIRM_THROTTLE_MS) return false;
    lastConfirmAt = now;
    window.clearTimeout(confirmTimer);
    confirmTimer = window.setTimeout(async () => {
      confirmTimer = 0;
      if (destroyed) return;
      await window.ParadiseBridge.refresh();
      const payload = window.ParadiseBridge.getLastPayload?.();
      reconcile(payload);
    }, 650);
    return true;
  }

  function onPlayerData(event) {
    reconcile(event.detail);
  }

  function onScanScheduled(event) {
    const reason = asText(event.detail?.reason) || '';
    if (!/nitro-interaction|history|room-event|chat-transition/.test(reason)) return;
    const store = window.ParadiseStore?.getState?.();
    const source = asText(store?.meta?.roomSource) || '';
    const age = roomAgeMs(store?.meta);
    if (!store?.gameplay?.room?.name || WEAK_SOURCES.has(source) || age >= CONFIRM_THROTTLE_MS) {
      requestConfirmation(reason);
    }
  }

  function boot() {
    window.addEventListener('paradise:player-data', onPlayerData, false);
    window.addEventListener('paradise:room-scan-scheduled', onScanScheduled, false);
    const payload = window.ParadiseBridge?.getLastPayload?.();
    if (payload) reconcile(payload);
  }

  function destroy() {
    if (destroyed) return;
    destroyed = true;
    window.clearTimeout(confirmTimer);
    confirmTimer = 0;
    window.removeEventListener('paradise:player-data', onPlayerData, false);
    window.removeEventListener('paradise:room-scan-scheduled', onScanScheduled, false);
  }

  window.ParadiseRoomReconciler = Object.freeze({
    version: VERSION,
    reconcile,
    confirm: requestConfirmation,
    getStatus: () => ({
      version: VERSION,
      weakTtlMs: WEAK_TTL_MS,
      confirmThrottleMs: CONFIRM_THROTTLE_MS,
      lastConfirmAt: lastConfirmAt ? new Date(lastConfirmAt).toISOString() : null,
      pending: Boolean(confirmTimer)
    }),
    destroy
  });

  window.addEventListener('beforeunload', destroy, { once: true });
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
