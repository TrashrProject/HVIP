(() => {
  'use strict';

  const VERSION = '1.0.1-character-snapshot';
  const boundSockets = new WeakSet();
  let lastSnapshot = null;
  let discoveryTimer = 0;
  let socketHookInstalled = false;

  const fmt = value => new Intl.NumberFormat('fr-FR').format(Number(value) || 0);

  function parseParadiseMessage(raw) {
    const text = String(raw || '');
    const prefix = 'compose_paradise|';
    if (!text.startsWith(prefix)) return null;

    try {
      return JSON.parse(text.slice(prefix.length));
    } catch (error) {
      console.warn('[ParadiseBridge] invalid JSON envelope', error);
      return null;
    }
  }

  function toHudData(snapshot) {
    const player = snapshot?.player || {};
    const identity = snapshot?.identity || {};
    const vitals = snapshot?.vitals || {};
    const progression = snapshot?.progression || {};
    const employment = snapshot?.employment || {};
    const economy = snapshot?.economy || {};
    const room = snapshot?.room || {};

    return {
      ok: true,
      id: player.userId ?? null,
      citizen_id: identity.citizenNumber ?? null,
      username: player.username ?? null,
      role: player.role ?? 'Citoyen',
      job: employment.jobName ?? null,
      motto: player.motto ?? '',
      level: progression.level ?? 1,
      look: player.look ?? '',
      health: vitals.health || { current: 100, max: 100 },
      armor: { current: Number(vitals.armor) || 0, max: 100 },
      money: {
        credits: economy.cash ?? 0,
        cash: economy.cash ?? 0,
        bank: economy.bank ?? 0
      },
      city: room.city ?? null,
      district: room.city ?? null,
      room: room.name ?? null
    };
  }

  function updateHudBindings(data) {
    const root = document.getElementById('paradise-rp-hud');
    if (!root || !data) return;

    const values = {
      username: data.username || 'Joueur',
      role: data.role || 'Citoyen',
      district: data.district || data.city || 'Paradise City',
      room: data.room || 'ParadiseRP',
      cash: `${fmt(data.money?.cash ?? data.money?.credits ?? 0)} $`,
      bank: `${fmt(data.money?.bank ?? 0)} $`,
      citizen: data.citizen_id || '',
      level: `Niveau ${fmt(data.level || 1)}`
    };

    root.querySelectorAll('[data-pr-bind]').forEach(element => {
      const key = element.getAttribute('data-pr-bind');
      if (Object.prototype.hasOwnProperty.call(values, key) && values[key] !== '') {
        element.textContent = values[key];
      }
    });
  }

  function syncLegacyHud(snapshot) {
    const data = toHudData(snapshot);
    const ui = window.__ParadiseRPUI;

    if (ui && typeof ui.getData === 'function') {
      const current = ui.getData();
      if (current && typeof current === 'object') {
        Object.assign(current, data);
        current.money = { ...(current.money || {}), ...(data.money || {}) };
        current.health = { ...(current.health || {}), ...(data.health || {}) };
        current.armor = { ...(current.armor || {}), ...(data.armor || {}) };
      }
    }

    updateHudBindings(data);
    window.dispatchEvent(new CustomEvent('paradise:authoritative-player-data', { detail: data }));
  }

  function applyEnvelope(envelope) {
    if (!envelope || !window.ParadiseStore) return;
    if (!window.ParadiseStore.applyEnvelope(envelope)) return;

    if (envelope.event === 'player:snapshot') {
      lastSnapshot = envelope.data;
      syncLegacyHud(lastSnapshot);
      console.info('[ParadiseBridge] Character snapshot synchronized', {
        userId: lastSnapshot?.player?.userId,
        room: lastSnapshot?.room?.name,
        job: lastSnapshot?.employment?.jobName
      });
    }
  }

  function requestSnapshot(socket) {
    if (!socket || socket.readyState !== WebSocket.OPEN) return false;
    if (!window.rdp || !window.rdp_app) return false;

    try {
      window.rdp.sendData(
        'event_characterbar',
        'paradise_snapshot',
        true,
        false,
        socket,
        true,
        window.rdp_app.UserID
      );
      return true;
    } catch (error) {
      console.warn('[ParadiseBridge] snapshot request failed', error);
      return false;
    }
  }

  function bindSocket(socket) {
    if (!socket || boundSockets.has(socket)) return;
    boundSockets.add(socket);

    socket.addEventListener('message', event => {
      const envelope = parseParadiseMessage(event.data);
      if (envelope) applyEnvelope(envelope);
    });

    socket.addEventListener('open', () => {
      window.setTimeout(() => requestSnapshot(socket), 50);
    });

    if (socket.readyState === WebSocket.OPEN) {
      window.setTimeout(() => requestSnapshot(socket), 0);
    }
  }

  function installSocketHook() {
    const app = window.rdp_app;
    if (!app || socketHookInstalled) return false;

    let socket = app.webSocket || null;
    if (socket) bindSocket(socket);

    try {
      Object.defineProperty(app, 'webSocket', {
        configurable: true,
        enumerable: true,
        get() { return socket; },
        set(next) {
          socket = next;
          if (next) bindSocket(next);
        }
      });
      socketHookInstalled = true;
      return true;
    } catch (_) {
      return false;
    }
  }

  function bootstrapDiscovery() {
    if (installSocketHook()) {
      if (discoveryTimer) window.clearInterval(discoveryTimer);
      discoveryTimer = 0;
      return;
    }

    if (!discoveryTimer) {
      discoveryTimer = window.setInterval(() => {
        if (installSocketHook()) {
          window.clearInterval(discoveryTimer);
          discoveryTimer = 0;
        }
      }, 250);
    }
  }

  // The existing HUD still polls PHP during migration. Re-apply the authoritative
  // emulator snapshot after a fallback refresh so SQL polling can never overwrite it.
  window.addEventListener('paradise:player-data', () => {
    if (!lastSnapshot) return;
    queueMicrotask(() => syncLegacyHud(lastSnapshot));
  });

  window.addEventListener('paradise:request-snapshot', () => {
    bootstrapDiscovery();
    requestSnapshot(window.rdp_app?.webSocket);
  });

  bootstrapDiscovery();

  window.ParadiseBridge = Object.freeze({
    version: VERSION,
    requestSnapshot: () => {
      bootstrapDiscovery();
      return requestSnapshot(window.rdp_app?.webSocket);
    },
    getLastSnapshot: () => lastSnapshot
  });
})();
