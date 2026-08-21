(() => {
  'use strict';

  if (window.__ParadisePhoneStrictLayout) return;
  window.__ParadisePhoneStrictLayout = '1.0.0-strict-layout';

  let destroyed = false;
  let scheduled = false;
  let unsubscribe = () => {};

  const root = () => document.querySelector('#paradise-rp-hud .pp-device');
  const text = value => value == null ? '' : String(value).trim();
  const esc = value => String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;');

  const icons = {
    messages: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M4 5h16v12H9l-5 4V5zm3 4v2h10V9H7zm0 4v2h7v-2H7z"/></svg>',
    notifications: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M12 3a6 6 0 0 0-6 6v4l-2 3v1h16v-1l-2-3V9a6 6 0 0 0-6-6zm-2 16h4a2 2 0 0 1-4 0z"/></svg>'
  };

  function store() {
    return window.ParadiseStore?.getState?.() || {};
  }

  function schedule() {
    if (destroyed || scheduled) return;
    scheduled = true;
    requestAnimationFrame(() => requestAnimationFrame(() => {
      scheduled = false;
      if (!destroyed) enhance();
    }));
  }

  function phoneState() {
    return store().phone || {};
  }

  function unreadNotifications(phone) {
    return Array.isArray(phone.notifications)
      ? phone.notifications.filter(item => !item?.read).length
      : 0;
  }

  function latestConversation(phone) {
    if (!Array.isArray(phone.conversations) || !phone.conversations.length) return null;
    return phone.conversations[0] || null;
  }

  function latestNotification(phone) {
    if (!Array.isArray(phone.notifications) || !phone.notifications.length) return null;
    return phone.notifications[0] || null;
  }

  function messageSummary(phone) {
    const unread = Math.max(0, Number(phone.unreadCount) || 0);
    const latest = latestConversation(phone);
    if (unread > 0) return `${unread} non lu${unread > 1 ? 's' : ''}`;
    if (latest) return text(latest.name) || text(latest.number) || 'Conversation récente';
    return 'Aucune conversation';
  }

  function messageDetail(phone) {
    const latest = latestConversation(phone);
    if (!latest) return 'Ouvrir Messages';
    return text(latest.last_message) || 'Conversation disponible';
  }

  function notificationSummary(phone) {
    const unread = unreadNotifications(phone);
    if (unread > 0) return `${unread} non lue${unread > 1 ? 's' : ''}`;
    const latest = latestNotification(phone);
    if (latest) return text(latest.title) || 'À jour';
    return 'Aucune alerte';
  }

  function notificationDetail(phone) {
    const latest = latestNotification(phone);
    if (!latest) return 'Ouvrir Notifications';
    return text(latest.body) || text(latest.title) || 'Notification disponible';
  }

  function shortcutMarkup(kind, title, summary, detail, badge) {
    return `
      <button type="button" class="ppf-real-shortcut is-${kind}" data-ppf-shortcut="${kind}">
        <span class="ppf-real-shortcut-icon">${icons[kind]}</span>
        <span class="ppf-real-shortcut-copy">
          <strong>${esc(title)}</strong>
          ${badge > 0 ? `<b>${badge}</b>` : ''}
        </span>
        <small title="${esc(detail)}">${esc(summary)}</small>
      </button>`;
  }

  function ensureRealZone(home) {
    let zone = home.querySelector('.ppf-real-zone');
    if (!zone) {
      zone = document.createElement('section');
      zone.className = 'ppf-real-zone';
      zone.dataset.ppfRealZone = '1';
      zone.setAttribute('aria-label', 'Aperçu ParadisePhone');
      home.appendChild(zone);
    }

    const phone = phoneState();
    const messageUnread = Math.max(0, Number(phone.unreadCount) || 0);
    const notificationUnread = unreadNotifications(phone);
    const signature = [
      messageUnread,
      notificationUnread,
      messageSummary(phone),
      messageDetail(phone),
      notificationSummary(phone),
      notificationDetail(phone)
    ].join('|');

    if (zone.dataset.ppfSignature === signature) return;
    zone.dataset.ppfSignature = signature;
    zone.innerHTML = `
      <div class="ppf-real-zone-head">
        <strong>Aperçu</strong>
        <span>Données Paradise</span>
      </div>
      <div class="ppf-real-shortcuts">
        ${shortcutMarkup('messages', 'Messages', messageSummary(phone), messageDetail(phone), messageUnread)}
        ${shortcutMarkup('notifications', 'Alertes', notificationSummary(phone), notificationDetail(phone), notificationUnread)}
      </div>`;
  }

  function removeLegacyFillers(home) {
    home.querySelectorAll('.ppf-live-strip').forEach(node => node.remove());
  }

  function enhance() {
    const phone = root();
    if (!phone) return;
    const home = phone.querySelector('.pp-home');
    if (!home) return;

    phone.dataset.ppStrictLayout = '1';
    removeLegacyFillers(home);
    ensureRealZone(home);
  }

  function openExistingApp(kind) {
    const phone = root();
    const target = phone?.querySelector(`.pp-app[data-pp-app="${kind}"]`);
    if (!target) return false;
    target.click();
    return true;
  }

  function onClick(event) {
    const target = event.target;
    if (!(target instanceof Element)) return;

    const shortcut = target.closest('[data-ppf-shortcut]');
    if (shortcut && root()?.contains(shortcut)) {
      event.preventDefault();
      event.stopPropagation();
      const kind = shortcut.dataset.ppfShortcut;
      if (kind === 'messages' || kind === 'notifications') openExistingApp(kind);
      window.setTimeout(schedule, 60);
      return;
    }

    if (target.closest('#paradise-rp-hud .pp-device')) {
      schedule();
      window.setTimeout(schedule, 80);
    }
  }

  function onStoreChange(_state, eventName) {
    if ([
      'phone:update',
      'ui:change',
      'gameplay:snapshot',
      'room:change',
      'player:update',
      'character:update'
    ].includes(eventName)) schedule();
  }

  function boot() {
    document.addEventListener('click', onClick, true);
    window.addEventListener('paradise:phone', schedule, false);
    window.addEventListener('paradise:store-change', schedule, false);
    if (window.ParadiseStore?.subscribe) unsubscribe = window.ParadiseStore.subscribe(onStoreChange) || (() => {});
    schedule();
  }

  function destroy() {
    if (destroyed) return;
    destroyed = true;
    document.removeEventListener('click', onClick, true);
    window.removeEventListener('paradise:phone', schedule, false);
    window.removeEventListener('paradise:store-change', schedule, false);
    try { unsubscribe(); } catch (_) {}
  }

  window.ParadisePhoneStrictLayout = Object.freeze({
    version: '1.0.0-strict-layout',
    refresh: schedule,
    getStatus: () => {
      const phone = phoneState();
      return {
        version: '1.0.0-strict-layout',
        mounted: Boolean(root()?.dataset.ppStrictLayout === '1'),
        home: Boolean(root()?.querySelector('.pp-home')),
        realZone: Boolean(root()?.querySelector('.ppf-real-zone')),
        unreadMessages: Math.max(0, Number(phone.unreadCount) || 0),
        unreadNotifications: unreadNotifications(phone)
      };
    }
  });

  window.addEventListener('beforeunload', destroy, { once: true });
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
