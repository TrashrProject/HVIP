(() => {
  'use strict';

  if (window.__PARADISE_PHONE_NOTIFICATION_FAST_V3__) return;
  window.__PARADISE_PHONE_NOTIFICATION_FAST_V3__ = '3.3.0';

  const API_PATH = '/nitro/phone-notifications-api.php';
  const STORAGE_PREFIX = 'paradise.phone.notifications.deleted.v1.';
  const MAX_DELETED = 250;

  let currentUserId = 0;
  let observer = null;
  let scheduled = false;
  const originalFetch = window.fetch.bind(window);

  function storageKey(userId = currentUserId) {
    return userId ? `${STORAGE_PREFIX}${userId}` : '';
  }

  function readDeleted(userId = currentUserId) {
    const key = storageKey(userId);
    if (!key) return [];
    try {
      const value = JSON.parse(localStorage.getItem(key) || '[]');
      return Array.isArray(value) ? value.filter(item => typeof item === 'string').slice(-MAX_DELETED) : [];
    } catch {
      return [];
    }
  }

  function writeDeleted(values, userId = currentUserId) {
    const key = storageKey(userId);
    if (!key) return;
    try {
      localStorage.setItem(key, JSON.stringify([...new Set(values)].slice(-MAX_DELETED)));
    } catch {}
  }

  function itemKey(source, id) {
    return `${String(source || '')}:${Number(id || 0)}`;
  }

  function isNotificationsRequest(input) {
    try {
      const value = typeof input === 'string' ? input : input?.url;
      if (!value) return false;
      const url = new URL(value, window.location.href);
      return url.pathname === API_PATH;
    } catch {
      return false;
    }
  }

  // Filter deleted notifications at the data layer so they stay deleted after refresh,
  // reconnect and future polling without touching gameplay/call/ParadiseGram records.
  window.fetch = async function paradiseNotificationFetch(input, init) {
    const response = await originalFetch(input, init);
    if (!isNotificationsRequest(input)) return response;

    try {
      const payload = await response.clone().json();
      if (!payload?.ok || !Array.isArray(payload.items)) return response;

      const userId = Number(payload.me?.id || 0);
      if (userId) currentUserId = userId;
      const deleted = new Set(readDeleted(userId));
      if (!deleted.size) return response;

      payload.items = payload.items.filter(item => !deleted.has(itemKey(item.source, item.id)));

      const headers = new Headers(response.headers);
      headers.delete('content-length');
      headers.set('content-type', 'application/json; charset=utf-8');
      return new Response(JSON.stringify(payload), {
        status: response.status,
        statusText: response.statusText,
        headers
      });
    } catch {
      return response;
    }
  };

  function ensureStyles() {
    if (document.getElementById('paradise-notification-delete-v33')) return;
    const style = document.createElement('style');
    style.id = 'paradise-notification-delete-v33';
    style.textContent = `
      .nitro-phone-frame .pnc-row{grid-template-columns:44px minmax(0,1fr) 30px!important;padding-right:7px!important}
      .nitro-phone-frame .pnc-row .pnc-dot{display:none!important}
      .nitro-phone-frame .pnc-delete{
        width:27px!important;height:27px!important;padding:0!important;margin:0!important;
        border:1px solid rgba(235,88,106,.34)!important;border-radius:8px!important;
        background:rgba(116,35,50,.34)!important;color:#ff9eaa!important;
        display:flex!important;align-items:center!important;justify-content:center!important;
        font:900 16px/1 Arial,sans-serif!important;cursor:pointer!important;
        transition:background .12s ease,border-color .12s ease,transform .12s ease!important;
      }
      .nitro-phone-frame .pnc-delete:hover{background:rgba(169,45,63,.54)!important;border-color:rgba(255,111,128,.68)!important;color:#fff!important}
      .nitro-phone-frame .pnc-delete:active{transform:scale(.92)!important}
      .nitro-phone-frame .pnc-row.pnc-removing{opacity:0!important;transform:translateX(10px)!important;transition:opacity .13s ease,transform .13s ease!important}
      @media(max-height:650px){.nitro-phone-frame .pnc-row{grid-template-columns:40px minmax(0,1fr) 28px!important}.nitro-phone-frame .pnc-delete{width:25px!important;height:25px!important;font-size:15px!important}}
    `;
    document.head.appendChild(style);
  }

  function ensureDeleteButtons() {
    const center = document.querySelector('.nitro-phone-frame .paradise-phone-notification-center');
    if (!center) return;

    const deleted = new Set(readDeleted());
    center.querySelectorAll('[data-pnc-item]').forEach(row => {
      const key = itemKey(row.dataset.pncSource, row.dataset.pncId);
      if (deleted.has(key)) {
        row.remove();
        return;
      }
      if (row.querySelector('[data-pnc-delete]')) return;

      const button = document.createElement('button');
      button.type = 'button';
      button.className = 'pnc-delete';
      button.dataset.pncDelete = '1';
      button.setAttribute('aria-label', 'Supprimer cette notification');
      button.setAttribute('title', 'Supprimer');
      button.textContent = '×';
      row.appendChild(button);
    });
  }

  function decrementBadgeIfUnread(row) {
    if (!row.classList.contains('is-unread')) return;
    const badge = document.querySelector('.nitro-phone-frame [data-pnc-status-bell] .pnc-launch-badge');
    if (!badge || badge.hidden) return;
    const value = Number.parseInt(badge.textContent || '0', 10);
    if (!Number.isFinite(value) || value <= 0) return;
    const next = Math.max(0, value - 1);
    badge.textContent = String(next);
    badge.hidden = next === 0;
  }

  function deleteRow(row) {
    if (!row || !currentUserId) return;
    const key = itemKey(row.dataset.pncSource, row.dataset.pncId);
    if (!key || key.endsWith(':0')) return;

    const deleted = readDeleted();
    if (!deleted.includes(key)) {
      deleted.push(key);
      writeDeleted(deleted);
    }

    decrementBadgeIfUnread(row);
    row.classList.add('pnc-removing');
    window.setTimeout(() => row.remove(), 135);
  }

  function scheduleSync() {
    if (scheduled) return;
    scheduled = true;
    requestAnimationFrame(() => {
      scheduled = false;
      ensureDeleteButtons();
    });
  }

  async function resolveUser() {
    try {
      const response = await originalFetch(API_PATH, { credentials: 'same-origin', cache: 'no-store' });
      const payload = await response.json().catch(() => null);
      currentUserId = Number(payload?.me?.id || 0);
      scheduleSync();
    } catch {}
  }

  function bootstrap() {
    ensureStyles();
    resolveUser();

    observer = new MutationObserver(scheduleSync);
    observer.observe(document.documentElement, { childList: true, subtree: true });

    document.addEventListener('click', event => {
      const button = event.target.closest('[data-pnc-delete]');
      if (!button) return;

      event.preventDefault();
      event.stopPropagation();
      event.stopImmediatePropagation();
      deleteRow(button.closest('[data-pnc-item]'));
    }, true);

    console.info('[ParadisePhone] notifications V3.3 — suppression locale persistante active');
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', bootstrap, { once: true });
  else bootstrap();
})();
