(() => {
  'use strict';

  if (window.ParadiseStore) return;

  const VERSION = '1.1.0-ui-foundation';
  const listeners = new Set();

  const state = {
    gameplay: {
      player: {
        id: null,
        username: null,
        look: null,
        avatarUrl: null,
        motto: null,
        role: null,
        job: null,
        jobId: null,
        health: null,
        armor: null,
        level: null,
        citizenId: null
      },
      economy: { cash: undefined, bank: undefined },
      room: { id: null, name: null, district: null, city: null, playerCount: undefined },
      notifications: { count: 0 }
    },
    ui: {
      activeWindow: null,
      actionsOpen: false
    },
    meta: {
      connected: false,
      source: null,
      lastUpdatedAt: null,
      lastError: null
    }
  };

  const asText = value => {
    if (value === null || value === undefined) return null;
    const text = String(value).trim();
    return text === '' ? null : text;
  };

  const asNumber = value => {
    if (value === null || value === undefined || value === '') return null;
    const number = Number(value);
    return Number.isFinite(number) ? number : null;
  };

  const normalizeStat = value => {
    if (!value || typeof value !== 'object') return null;
    const current = asNumber(value.current);
    const max = asNumber(value.max);
    if (current === null) return null;
    return {
      current: Math.max(0, current),
      max: max !== null && max > 0 ? max : null
    };
  };

  const emit = (event, detail) => {
    listeners.forEach(listener => {
      try { listener(state, event, detail); } catch (error) { console.warn('[ParadiseStore] listener failed', error); }
    });
    window.dispatchEvent(new CustomEvent('paradise:store-change', { detail: { event, data: detail, state } }));
  };

  const applyServerSnapshot = payload => {
    if (!payload || typeof payload !== 'object' || payload.ok === false) {
      state.meta.connected = false;
      state.meta.lastError = asText(payload?.reason) || 'data_unavailable';
      emit('bridge:error', state.meta.lastError);
      return false;
    }

    const sourceMoney = payload.money && typeof payload.money === 'object' ? payload.money : {};
    const roomRaw = payload.room_name ?? payload.room;
    const nextGameplay = {
      player: {
        id: asNumber(payload.id),
        username: asText(payload.username),
        look: asText(payload.look),
        avatarUrl: asText(payload.avatar_url),
        motto: asText(payload.motto),
        role: asText(payload.role),
        job: asText(payload.job),
        jobId: asNumber(payload.job_id),
        health: normalizeStat(payload.health),
        armor: normalizeStat(payload.armor),
        level: asNumber(payload.level),
        citizenId: asText(payload.citizen_id)
      },
      economy: {
        cash: asNumber(sourceMoney.cash ?? sourceMoney.credits) ?? undefined,
        bank: asNumber(sourceMoney.bank) ?? undefined
      },
      room: {
        id: asNumber(payload.room_id),
        name: asText(roomRaw),
        district: asText(payload.district),
        city: asText(payload.city),
        playerCount: asNumber(payload.players) ?? undefined
      },
      notifications: {
        count: Math.max(0, asNumber(payload.notifications_count) ?? asNumber(payload.notifications?.count) ?? 0)
      }
    };

    const changed = JSON.stringify(state.gameplay) !== JSON.stringify(nextGameplay);
    state.gameplay = nextGameplay;
    state.meta.connected = true;
    state.meta.source = 'rp-hud-data';
    state.meta.lastUpdatedAt = new Date().toISOString();
    state.meta.lastError = null;

    if (changed) {
      emit('gameplay:snapshot', state.gameplay);
      window.dispatchEvent(new CustomEvent('paradise:player-data', { detail: payload }));
    }
    return true;
  };

  const setBridgeError = error => {
    state.meta.connected = false;
    state.meta.lastError = asText(error?.message || error) || 'bridge_error';
    emit('bridge:error', state.meta.lastError);
  };

  const setUi = patch => {
    if (!patch || typeof patch !== 'object') return;
    const next = {};
    if (Object.prototype.hasOwnProperty.call(patch, 'activeWindow')) next.activeWindow = asText(patch.activeWindow);
    if (Object.prototype.hasOwnProperty.call(patch, 'actionsOpen')) next.actionsOpen = Boolean(patch.actionsOpen);

    const before = JSON.stringify(state.ui);
    Object.assign(state.ui, next);
    if (JSON.stringify(state.ui) !== before) emit('ui:change', state.ui);
  };

  window.ParadiseStore = Object.freeze({
    version: VERSION,
    getState: () => state,
    applyServerSnapshot,
    setBridgeError,
    setUi,
    subscribe(listener) {
      if (typeof listener !== 'function') return () => {};
      listeners.add(listener);
      return () => listeners.delete(listener);
    }
  });
})();
