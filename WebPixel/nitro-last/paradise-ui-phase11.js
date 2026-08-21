(() => {
  'use strict';

  if (window.ParadisePhase11) return;

  const VERSION = '1.0.0-phase11';
  let unsubscribe = () => {};
  let previousCash;
  let previousBank;
  let destroyed = false;

  const text = value => value === null || value === undefined ? '' : String(value).trim();
  const number = value => Number.isFinite(Number(value)) ? Number(value) : null;

  function root() {
    return document.getElementById('paradise-rp-hud');
  }

  function roleTone(role) {
    const value = text(role).toLowerCase();
    if (/police|policier|gendar|security|sécur/.test(value)) return 'blue';
    if (/ems|médec|medec|ambul|hopital|hôpital|secours/.test(value)) return 'coral';
    if (/gouvern|maire|minist|présid|presid/.test(value)) return 'gold';
    if (/staff|fondateur|admin|équipe|equipe|modo|manager/.test(value)) return 'aqua';
    return 'neutral';
  }

  function syncRoleBadges(state) {
    const hud = root();
    if (!hud) return;
    const role = text(state?.gameplay?.player?.role) || 'Citoyen';
    const tone = roleTone(role);
    hud.querySelectorAll('[data-bind="role"], [data-bind="profile-role"]').forEach(chip => {
      chip.dataset.roleTone = tone;
      chip.title = `Paradise Badge · ${role}`;
    });
  }

  function syncActiveWindow(state) {
    const hud = root();
    if (!hud) return;
    const active = text(state?.ui?.activeWindow);
    hud.querySelectorAll('[data-window-open]').forEach(button => {
      const isActive = text(button.dataset.windowOpen) === active;
      button.classList.toggle('is-active', isActive);
      button.setAttribute('aria-pressed', isActive ? 'true' : 'false');
    });
  }

  function syncRoom(state) {
    const hud = root();
    if (!hud) return;
    const room = state?.gameplay?.room || {};
    const meta = state?.meta || {};
    const name = text(room.name);
    const chip = hud.querySelector('.pr-room-chip');
    const nameNodes = hud.querySelectorAll('[data-bind="room-name"]');
    const profileNodes = hud.querySelectorAll('[data-bind="profile-room"]');

    if (name) {
      nameNodes.forEach(node => { node.textContent = name; });
      profileNodes.forEach(node => { node.textContent = name; });
      chip?.classList.remove('is-connecting');
      chip?.classList.toggle('is-live', /^(nitro|room-event|nitro-dom)/i.test(text(meta.roomSource)));
      return;
    }

    const fallback = meta.connected ? 'Localisation inconnue' : 'Connexion à Placid...';
    nameNodes.forEach(node => { node.textContent = fallback; });
    profileNodes.forEach(node => { node.textContent = fallback; });
    hud.querySelectorAll('[data-bind="room-meta"]').forEach(node => { node.textContent = ''; });
    chip?.classList.add('is-connecting');
    chip?.classList.remove('is-live');
  }

  function pulseEconomy(selector) {
    const element = root()?.querySelector(selector);
    if (!element) return;
    element.classList.remove('is-value-updating');
    void element.offsetWidth;
    element.classList.add('is-value-updating');
    window.setTimeout(() => element.classList.remove('is-value-updating'), 320);
  }

  function syncEconomy(state, animate = true) {
    const cash = number(state?.gameplay?.economy?.cash);
    const bank = number(state?.gameplay?.economy?.bank);

    if (animate && previousCash !== undefined && cash !== null && previousCash !== cash) pulseEconomy('.pr-economy-cash');
    if (animate && previousBank !== undefined && bank !== null && previousBank !== bank) pulseEconomy('.pr-economy-bank');

    if (cash !== null) previousCash = cash;
    if (bank !== null) previousBank = bank;
  }

  function syncNotificationState(state) {
    const hud = root();
    if (!hud) return;
    const count = Math.max(0, Math.round(number(state?.gameplay?.notifications?.count) || 0));
    const button = hud.querySelector('.pr-notification-button');
    if (button) button.title = count > 0 ? `Notifications · ${count}` : 'Notifications';
  }

  function syncAll(state, animateEconomy = true) {
    if (!state || destroyed) return;
    syncRoleBadges(state);
    syncActiveWindow(state);
    syncRoom(state);
    syncEconomy(state, animateEconomy);
    syncNotificationState(state);
  }

  function sourceMap() {
    const state = window.ParadiseStore?.getState?.();
    const roomSource = text(state?.meta?.roomSource) || 'rp-hud-data';
    return {
      username: 'rp-hud-data.php / authenticated users row',
      role: 'rp-hud-data.php / users.rank mapping',
      job: 'rp-hud-data.php / play_stats/users + groups when available',
      health: 'rp-hud-data.php / play_stats/users',
      armor: 'rp-hud-data.php / play_stats/users',
      cash: 'rp-hud-data.php / users',
      bank: 'rp-hud-data.php / play_stats/users',
      room: roomSource,
      time: 'client clock (30s display refresh)',
      notifications: 'rp-hud-data.php when a real count exists'
    };
  }

  function boot() {
    if (destroyed || !window.ParadiseStore || !root()) return;
    const state = window.ParadiseStore.getState();
    syncAll(state, false);

    unsubscribe = window.ParadiseStore.subscribe((nextState, eventName) => {
      if (['gameplay:snapshot', 'room:change', 'bridge:error', 'ui:change', 'room:authority-release'].includes(eventName)) {
        syncAll(nextState, eventName === 'gameplay:snapshot');
      }
    });
  }

  function destroy() {
    if (destroyed) return;
    destroyed = true;
    unsubscribe();
  }

  window.ParadisePhase11 = Object.freeze({
    version: VERSION,
    sync: () => syncAll(window.ParadiseStore?.getState?.(), false),
    getSourceMap: sourceMap,
    getToolbarAudit: () => window.ParadiseNativeToolbarMigration?.getAudit?.() || [],
    destroy
  });

  window.addEventListener('beforeunload', destroy, { once: true });
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();