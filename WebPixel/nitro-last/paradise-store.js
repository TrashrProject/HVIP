(() => {
  'use strict';

  if (window.ParadiseStore) return;

  const listeners = new Set();
  const state = {
    connected: false,
    version: 1,
    player: null,
    identity: null,
    vitals: null,
    progression: null,
    employment: null,
    economy: null,
    room: null,
    inventory: null,
    phone: null,
    vehicles: null,
    properties: null,
    notifications: [],
    lastSnapshotAt: null
  };

  const emit = (event, detail) => {
    listeners.forEach(listener => {
      try { listener(state, event, detail); } catch (_) {}
    });
    window.dispatchEvent(new CustomEvent('paradise:store-change', {
      detail: { event, data: detail, state }
    }));
  };

  const applySnapshot = snapshot => {
    if (!snapshot || typeof snapshot !== 'object') return false;

    state.player = snapshot.player || null;
    state.identity = snapshot.identity || null;
    state.vitals = snapshot.vitals || null;
    state.progression = snapshot.progression || null;
    state.employment = snapshot.employment || null;
    state.economy = snapshot.economy || null;
    state.room = snapshot.room || null;
    state.connected = true;
    state.lastSnapshotAt = new Date().toISOString();

    emit('player:snapshot', snapshot);
    return true;
  };

  const applyEnvelope = envelope => {
    if (!envelope || Number(envelope.v) !== 1 || !envelope.event) return false;

    switch (envelope.event) {
      case 'player:snapshot':
        return applySnapshot(envelope.data);
      default:
        emit(envelope.event, envelope.data);
        return true;
    }
  };

  window.ParadiseStore = Object.freeze({
    getState: () => state,
    applyEnvelope,
    applySnapshot,
    subscribe(listener) {
      if (typeof listener !== 'function') return () => {};
      listeners.add(listener);
      return () => listeners.delete(listener);
    }
  });
})();
