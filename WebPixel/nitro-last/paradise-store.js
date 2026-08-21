(() => {
  'use strict';

  if (window.ParadiseStore) return;

  const VERSION = '3.0.0-inventory-v2';
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
    character: {
      exists: false,
      firstName: null,
      lastName: null,
      fullName: null,
      birthDate: null,
      age: null,
      gender: null,
      nationality: null,
      citizenId: null,
      biography: null,
      reputation: 0,
      createdAt: null,
      updatedAt: null
    },
    documents: [],
    reputation: { general: 0 },
    statistics: { accountCreated: null, onlineTime: null, roomVisits: null },
    inventory: {
      items: [],
      weight: 0,
      capacity: 50,
      baseCapacity: 50,
      capacityBonus: 0,
      slotsUsed: 0,
      maxSlots: 30,
      lastUpdatedAt: null,
      lastError: null
    },
    offers: { document: null },
    ui: {
      activeWindow: null,
      actionsOpen: false,
      profileTab: 'overview',
      profileDocument: null,
      presentedDocument: null,
      onboarding: false,
      inventorySelected: null,
      inventoryFilter: 'all'
    },
    meta: {
      connected: false,
      source: null,
      lastUpdatedAt: null,
      lastError: null,
      roomSource: null,
      roomUpdatedAt: null,
      pendingUiEvent: null
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
    return { current: Math.max(0, current), max: max !== null && max > 0 ? max : null };
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

  const normalizeCharacter = value => {
    const source = value && typeof value === 'object' ? value : {};
    return {
      exists: Boolean(source.exists),
      firstName: asText(source.first_name ?? source.firstName),
      lastName: asText(source.last_name ?? source.lastName),
      fullName: asText(source.full_name ?? source.fullName),
      birthDate: asText(source.birth_date ?? source.birthDate),
      age: asNumber(source.age),
      gender: asText(source.gender),
      nationality: asText(source.nationality),
      citizenId: asText(source.citizen_id ?? source.citizenId),
      biography: asText(source.biography) || '',
      reputation: asNumber(source.reputation) ?? 0,
      createdAt: asText(source.created_at ?? source.createdAt),
      updatedAt: asText(source.updated_at ?? source.updatedAt)
    };
  };

  const normalizeDocuments = value => Array.isArray(value) ? value.map(item => ({
    id: asNumber(item?.id),
    type: asText(item?.type),
    name: asText(item?.name),
    category: asText(item?.category),
    number: asText(item?.number),
    status: asText(item?.status) || 'UNKNOWN',
    issuedAt: asText(item?.issued_at ?? item?.issuedAt),
    expiresAt: asText(item?.expires_at ?? item?.expiresAt),
    canExpire: Boolean(item?.can_expire ?? item?.canExpire),
    metadata: asText(item?.metadata)
  })) : [];

  const normalizeInventoryItem = item => {
    const source = item && typeof item === 'object' ? item : {};
    return {
      key: asText(source.key) || `item:${asText(source.id) || Math.random().toString(36).slice(2)}`,
      id: asNumber(source.id),
      source: asText(source.source) || 'inventory',
      definitionId: asNumber(source.definition_id ?? source.definitionId),
      documentId: asNumber(source.document_id ?? source.documentId),
      documentType: asText(source.document_type ?? source.documentType),
      code: asText(source.code),
      name: asText(source.name) || 'Objet',
      description: asText(source.description) || '',
      category: (asText(source.category) || 'OBJECT').toUpperCase(),
      weight: Math.max(0, asNumber(source.weight) ?? 0),
      totalWeight: Math.max(0, asNumber(source.total_weight ?? source.totalWeight) ?? 0),
      quantity: Math.max(0, Math.round(asNumber(source.quantity) ?? 0)),
      maxStack: Math.max(1, Math.round(asNumber(source.max_stack ?? source.maxStack) ?? 1)),
      icon: asText(source.icon),
      usable: Boolean(source.usable),
      tradeable: Boolean(source.tradeable),
      droppable: Boolean(source.droppable),
      effectType: (asText(source.effect_type ?? source.effectType) || 'NONE').toUpperCase(),
      metadata: asText(source.metadata),
      slot: asNumber(source.slot),
      status: asText(source.status),
      number: asText(source.number),
      actions: Array.isArray(source.actions) ? source.actions.map(value => String(value).toLowerCase()) : [],
      locked: Boolean(source.locked)
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
    const serverRoom = normalizeRoom({ room_id: payload.room_id, room_name: payload.room_name ?? payload.room, district: payload.district, city: payload.city, players: payload.players });
    const nextGameplay = {
      player: {
        id: asNumber(payload.id), username: asText(payload.username), look: asText(payload.look), avatarUrl: asText(payload.avatar_url), motto: asText(payload.motto),
        role: asText(payload.role), job: asText(payload.job), jobId: asNumber(payload.job_id), health: normalizeStat(payload.health), armor: normalizeStat(payload.armor),
        level: asNumber(payload.level), citizenId: asText(payload.citizen_id)
      },
      economy: { cash: asNumber(sourceMoney.cash ?? sourceMoney.credits) ?? undefined, bank: asNumber(sourceMoney.bank) ?? undefined },
      room: hasLiveRoomAuthority() ? state.gameplay.room : serverRoom,
      notifications: { count: Math.max(0, asNumber(payload.notifications_count) ?? asNumber(payload.notifications?.count) ?? 0) }
    };

    const nextCharacter = normalizeCharacter(payload.character);
    const nextDocuments = normalizeDocuments(payload.documents);
    const nextReputation = { general: asNumber(payload.reputation?.general) ?? nextCharacter.reputation ?? 0 };
    const nextStatistics = {
      accountCreated: asText(payload.statistics?.account_created ?? payload.statistics?.accountCreated),
      onlineTime: asNumber(payload.statistics?.online_time ?? payload.statistics?.onlineTime),
      roomVisits: asNumber(payload.statistics?.room_visits ?? payload.statistics?.roomVisits)
    };
    const nextOffer = payload.document_offer && typeof payload.document_offer === 'object' ? payload.document_offer : null;
    const nextUiEvent = payload.ui_event && typeof payload.ui_event === 'object' ? payload.ui_event : null;

    const gameplayChanged = JSON.stringify(state.gameplay) !== JSON.stringify(nextGameplay);
    const characterChanged = JSON.stringify(state.character) !== JSON.stringify(nextCharacter);
    const documentsChanged = JSON.stringify(state.documents) !== JSON.stringify(nextDocuments);
    const reputationChanged = JSON.stringify(state.reputation) !== JSON.stringify(nextReputation);
    const statisticsChanged = JSON.stringify(state.statistics) !== JSON.stringify(nextStatistics);
    const offerChanged = JSON.stringify(state.offers.document) !== JSON.stringify(nextOffer);
    const uiEventChanged = JSON.stringify(state.meta.pendingUiEvent) !== JSON.stringify(nextUiEvent);

    state.gameplay = nextGameplay;
    state.character = nextCharacter;
    state.documents = nextDocuments;
    state.reputation = nextReputation;
    state.statistics = nextStatistics;
    state.offers.document = nextOffer;
    state.meta.pendingUiEvent = nextUiEvent;
    state.meta.connected = true;
    state.meta.source = 'rp-hud-data';
    state.meta.lastUpdatedAt = new Date().toISOString();
    state.meta.lastError = null;

    if (!hasLiveRoomAuthority()) {
      state.meta.roomSource = 'rp-hud-data';
      state.meta.roomUpdatedAt = state.meta.lastUpdatedAt;
    }

    if (gameplayChanged) {
      emit('gameplay:snapshot', state.gameplay);
      window.dispatchEvent(new CustomEvent('paradise:player-data', { detail: payload }));
    }
    if (characterChanged) emit('character:update', state.character);
    if (documentsChanged) emit('documents:update', state.documents);
    if (reputationChanged) emit('reputation:update', state.reputation);
    if (statisticsChanged) emit('statistics:update', state.statistics);
    if (offerChanged && nextOffer) emit('document:offer', nextOffer);
    if (uiEventChanged && nextUiEvent) emit('ui:event', nextUiEvent);
    return true;
  };

  const applyInventorySnapshot = payload => {
    if (!payload || typeof payload !== 'object' || payload.ok === false || !payload.inventory) {
      state.inventory.lastError = asText(payload?.reason) || 'inventory_unavailable';
      emit('inventory:error', state.inventory.lastError);
      return false;
    }
    const source = payload.inventory;
    const next = {
      items: Array.isArray(source.items) ? source.items.map(normalizeInventoryItem) : [],
      weight: Math.max(0, asNumber(source.weight) ?? 0),
      capacity: Math.max(0, asNumber(source.capacity) ?? 50),
      baseCapacity: Math.max(0, asNumber(source.base_capacity ?? source.baseCapacity) ?? 50),
      capacityBonus: Math.max(0, asNumber(source.capacity_bonus ?? source.capacityBonus) ?? 0),
      slotsUsed: Math.max(0, Math.round(asNumber(source.slots_used ?? source.slotsUsed) ?? 0)),
      maxSlots: Math.max(1, Math.round(asNumber(source.max_slots ?? source.maxSlots) ?? 30)),
      lastUpdatedAt: new Date().toISOString(),
      lastError: null
    };
    const changed = JSON.stringify(state.inventory) !== JSON.stringify(next);
    state.inventory = next;
    const selected = state.ui.inventorySelected;
    if (selected && !next.items.some(item => item.key === selected)) state.ui.inventorySelected = null;
    if (changed) emit('inventory:update', state.inventory);
    return true;
  };

  const setRoomSnapshot = (snapshot, source = 'nitro') => {
    const nextRoom = normalizeRoom(snapshot);
    const previous = state.gameplay.room;
    const changedRoom = previous.id !== nextRoom.id || previous.name !== nextRoom.name;
    if (nextRoom.district === null && !changedRoom) nextRoom.district = previous.district;
    if (nextRoom.city === null) nextRoom.city = previous.city;
    if (nextRoom.playerCount === undefined && !changedRoom) nextRoom.playerCount = previous.playerCount;
    const changed = JSON.stringify(previous) !== JSON.stringify(nextRoom);
    state.gameplay.room = nextRoom;
    state.meta.roomSource = asText(source) || 'nitro';
    state.meta.roomUpdatedAt = new Date().toISOString();
    if (changed) {
      emit('room:change', nextRoom);
      window.dispatchEvent(new CustomEvent('paradise:room-data', { detail: { room: nextRoom, source: state.meta.roomSource } }));
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
    if (Object.prototype.hasOwnProperty.call(patch, 'profileTab')) next.profileTab = asText(patch.profileTab) || 'overview';
    if (Object.prototype.hasOwnProperty.call(patch, 'profileDocument')) next.profileDocument = asText(patch.profileDocument);
    if (Object.prototype.hasOwnProperty.call(patch, 'presentedDocument')) next.presentedDocument = patch.presentedDocument || null;
    if (Object.prototype.hasOwnProperty.call(patch, 'onboarding')) next.onboarding = Boolean(patch.onboarding);
    if (Object.prototype.hasOwnProperty.call(patch, 'inventorySelected')) next.inventorySelected = asText(patch.inventorySelected);
    if (Object.prototype.hasOwnProperty.call(patch, 'inventoryFilter')) next.inventoryFilter = asText(patch.inventoryFilter) || 'all';
    const before = JSON.stringify(state.ui);
    Object.assign(state.ui, next);
    if (JSON.stringify(state.ui) !== before) emit('ui:change', state.ui);
  };

  const setInventoryUi = patch => {
    const safe = {};
    if (Object.prototype.hasOwnProperty.call(patch || {}, 'selected')) safe.inventorySelected = asText(patch.selected);
    if (Object.prototype.hasOwnProperty.call(patch || {}, 'filter')) safe.inventoryFilter = asText(patch.filter) || 'all';
    setUi(safe);
  };

  const clearDocumentOffer = offerId => {
    const currentId = asNumber(state.offers.document?.id);
    if (offerId === undefined || offerId === null || currentId === asNumber(offerId)) {
      state.offers.document = null;
      emit('document:offer-cleared', offerId ?? currentId);
    }
  };

  const clearUiEvent = eventId => {
    const currentId = asNumber(state.meta.pendingUiEvent?.id);
    if (eventId === undefined || eventId === null || currentId === asNumber(eventId)) {
      state.meta.pendingUiEvent = null;
      emit('ui:event-cleared', eventId ?? currentId);
    }
  };

  window.ParadiseStore = Object.freeze({
    version: VERSION,
    getState: () => state,
    applyServerSnapshot,
    applyInventorySnapshot,
    setRoomSnapshot,
    releaseRoomAuthority,
    setBridgeError,
    setUi,
    setInventoryUi,
    clearDocumentOffer,
    clearUiEvent,
    subscribe(listener) {
      if (typeof listener !== 'function') return () => {};
      listeners.add(listener);
      return () => listeners.delete(listener);
    }
  });
})();
