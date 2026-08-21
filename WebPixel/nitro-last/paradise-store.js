(() => {
  'use strict';

  if (window.ParadiseStore) return;

  const VERSION = '1.2.0-phase11';
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
      lastError: null,
      roomSource: null,
      roomUpdatedAt: null
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

  const normalizeRoom = value => {
    const source = value && typeof value === 'object' ? value : {};
    const nested = source.room && typeof source.room === 'object' ? source.room : {};
    const rawName = source.room_name ?? source.roomName ?? source.caption ?? source.name ?? nested.caption ?? nested.name ?? (typeof source.room === 'string' ? source.room : null);
    const rawId = source.room_id ?? source.roomId ?? source.id ?? nested.id ?? nested.roomId ?? null;
    return {
      id: asNumber(rawId),
      name: asText(rawName),
      district: asText(source.district ?? source.zone ?? nested.district ?? nested.zone),
      city: asText(source.city ?? nested.city),
      playerCount: asNumber(source.players ?? source.playerCount ?? source.users ?? nested.players ?? nested.playerCount) ?? undefined
    };
  };

  const emit = (event, detail) => {
    listeners.forEach(listener => {
      try { listener(state, event, detail); } catch (error) { console.warn('[ParadiseStore] listener failed', error); }
    });
    window.dispatchEvent(new CustomEvent('paradise:store-change', { detail: { event, data: detail, state } }));
  };

  const hasLiveRoomAuthority = () => {
    const source = asText(state.meta.roomSource) || '';
    return /^(nitro|room-event|nitro-dom)/i.test(source);
  };

  const applyServerSnapshot = payload => {
    if (!payload || typeof payload !== 'object' || payload.ok === false) {
      state.meta.connected = false;
      state.meta.lastError = asText(payload?.reason) || 'data_unavailable';
      emit('bridge:error', state.meta.lastError);
      return false;
    }

    const sourceMoney = payload.money && typeof payload.money === 'object' ? payload.money : {};
    const serverRoom = normalizeRoom({
      room_id: payload.room_id,
      room_name: payload.room_name ?? payload.room,
      district: payload.district,
      city: payload.city,
      players: payload.players
    });

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
      room: hasLiveRoomAuthority() ? state.gameplay.room : serverRoom,
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

    if (!hasLiveRoomAuthority()) {
      state.meta.roomSource = 'rp-hud-data';
      state.meta.roomUpdatedAt = state.meta.lastUpdatedAt;
    }

    if (changed) {
      emit('gameplay:snapshot', state.gameplay);
      window.dispatchEvent(new CustomEvent('paradise:player-data', { detail: payload }));
    }
    return true;
  };

  const setRoomSnapshot = (snapshot, source = 'nitro') => {
    const nextRoom = normalizeRoom(snapshot);
    const previous = state.gameplay.room;
    const changedRoom = previous.id !== nextRoom.id || previous.name !== nextRoom.name;

    // A district belongs to a room. Do not carry it into a different room unless
    // the new room event actually provides one. City may remain global.
    if (nextRoom.district === null && !changedRoom) nextRoom.district = previous.district;
    if (nextRoom.city === null) nextRoom.city = previous.city;
    if (nextRoom.playerCount === undefined && !changedRoom) nextRoom.playerCount = previous.playerCount;

    const changed = JSON.stringify(previous) !== JSON.stringify(nextRoom);
    state.gameplay.room = nextRoom;
    state.meta.roomSource = asText(source) || 'nitro';
    state.meta.roomUpdatedAt = new Date().toISOString();

    if (changed) {
      emit('room:change', nextRoom);
      window.dispatchEvent(new CustomEvent('paradise:room-data', {
        detail: { room: nextRoom, source: state.meta.roomSource }
      }));
    }
    return true;
  };

  const releaseRoomAuthority = () => {
    if (!hasLiveRoomAuthority()) return false;
    state.meta.roomSource = 'rp-hud-data';
    state.meta.roomUpdatedAt = new Date().toISOString();
    emit('room:authority-release', state.gameplay.room);
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
    setRoomSnapshot,
    releaseRoomAuthority,
    setBridgeError,
    setUi,
    subscribe(listener) {
      if (typeof listener !== 'function') return () => {};
      listeners.add(listener);
      return () => listeners.delete(listener);
    }
  });
})();