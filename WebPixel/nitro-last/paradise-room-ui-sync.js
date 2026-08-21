(() => {
  'use strict';

  if (window.ParadiseRoomUiSync) return;

  const VERSION = '1.0.0-direct-room-ui-sync';
  let unsubscribe = () => {};
  let destroyed = false;
  let repairing = false;

  const text = value => value === null || value === undefined ? '' : String(value).trim();

  function repairMojibake(value) {
    const raw = text(value);
    if (!raw || !/[ÃÂâ]/.test(raw)) return raw;
    try {
      const bytes = new Uint8Array([...raw].map(char => char.charCodeAt(0)));
      const decoded = new TextDecoder('utf-8', { fatal: true }).decode(bytes);
      return decoded && decoded !== raw ? decoded : raw;
    } catch (_) {
      return raw;
    }
  }

  function roomLabel(room) {
    const name = repairMojibake(room?.name);
    return name || 'Synchronisation de la room…';
  }

  function roomMeta(room) {
    const parts = [];
    const district = repairMojibake(room?.district);
    const city = repairMojibake(room?.city);
    if (district || city) parts.push(district || city);
    const count = Number(room?.playerCount);
    if (Number.isFinite(count)) parts.push(`${Math.round(count)} joueur${Math.round(count) > 1 ? 's' : ''}`);
    return parts.join(' · ');
  }

  function sync() {
    if (destroyed) return;
    const store = window.ParadiseStore?.getState?.();
    const hud = document.getElementById('paradise-rp-hud');
    if (!store || !hud) return;

    const room = store.gameplay?.room || {};
    const rawName = text(room.name);
    const cleanName = repairMojibake(rawName);

    // Repair the canonical store value once when Nitro exposes UTF-8 text as mojibake.
    if (!repairing && rawName && cleanName && cleanName !== rawName && window.ParadiseStore?.setRoomSnapshot) {
      repairing = true;
      try {
        window.ParadiseStore.setRoomSnapshot({ ...room, name: cleanName }, 'room-name-repair');
      } finally {
        repairing = false;
      }
    }

    const label = cleanName || roomLabel(room);
    const meta = roomMeta(room);

    hud.querySelectorAll('[data-bind="room-name"]').forEach(node => { node.textContent = label; });
    hud.querySelectorAll('[data-bind="room-meta"]').forEach(node => { node.textContent = meta; });
    hud.querySelectorAll('[data-bind="profile-room"]').forEach(node => { node.textContent = label; });

    hud.querySelectorAll('.pr2-info').forEach(info => {
      const key = text(info.querySelector('span')?.textContent).toLowerCase();
      if (key === 'localisation') {
        const value = info.querySelector('strong');
        if (value) value.textContent = label;
      }
    });

    const chip = hud.querySelector('.pr-room-chip');
    if (chip) {
      chip.classList.toggle('is-live', Boolean(cleanName));
      chip.classList.toggle('is-resolving', !cleanName);
      chip.classList.remove('is-connecting');
    }
  }

  function boot() {
    if (destroyed) return;
    if (!window.ParadiseStore || !document.getElementById('paradise-rp-hud')) {
      window.setTimeout(boot, 80);
      return;
    }
    unsubscribe = window.ParadiseStore.subscribe((_store, eventName) => {
      if (eventName === 'room:change' || eventName === 'gameplay:snapshot' || eventName === 'ui:change') sync();
    }) || (() => {});
    window.addEventListener('paradise:room-data', sync, false);
    sync();
  }

  function destroy() {
    if (destroyed) return;
    destroyed = true;
    unsubscribe();
    window.removeEventListener('paradise:room-data', sync, false);
  }

  window.ParadiseRoomUiSync = Object.freeze({
    version: VERSION,
    sync,
    destroy,
    getStatus: () => ({
      version: VERSION,
      room: window.ParadiseStore?.getState?.().gameplay?.room || null
    })
  });

  window.addEventListener('beforeunload', destroy, { once: true });
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
