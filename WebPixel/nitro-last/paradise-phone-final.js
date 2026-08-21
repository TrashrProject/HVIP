(() => {
  'use strict';

  if (window.__ParadisePhoneFinalPolish) return;
  window.__ParadisePhoneFinalPolish = '1.0.0-final-home-composition';

  let destroyed = false;
  let scheduled = false;
  let unsubscribe = () => {};

  const root = () => document.querySelector('#paradise-rp-hud .pp-device');
  const text = value => value == null ? '' : String(value).trim();
  const esc = value => String(value ?? '')
    .replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;').replaceAll("'", '&#039;');

  const navIcons = {
    home: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M4 11 12 4l8 7v9h-6v-6h-4v6H4z"/></svg>',
    close: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M6 11h12v2H6z"/></svg>'
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

  function enhanceNav(phone) {
    const nav = phone.querySelector('.pp-nav');
    if (!nav) return;
    const home = nav.querySelector('[data-pp-home]');
    const close = nav.querySelector('[data-window-close="phone"]');
    if (home && home.dataset.ppFinalNav !== '1') {
      home.innerHTML = navIcons.home;
      home.dataset.ppFinalNav = '1';
      home.title = 'Accueil ParadisePhone';
    }
    if (close && close.dataset.ppFinalNav !== '1') {
      close.innerHTML = navIcons.close;
      close.dataset.ppFinalNav = '1';
      close.title = 'Fermer ParadisePhone';
    }
  }

  function ensureScene(home) {
    if (!home.querySelector('.ppf-home-scene')) {
      const scene = document.createElement('div');
      scene.className = 'ppf-home-scene';
      scene.setAttribute('aria-hidden', 'true');
      home.prepend(scene);
    }
  }

  function currentRoomLabel() {
    const room = store().gameplay?.room || {};
    return text(room.name);
  }

  function unreadCount() {
    return Math.max(0, Number(store().phone?.unreadCount) || 0);
  }

  function ensureLiveStrip(home) {
    let strip = home.querySelector('.ppf-live-strip');
    if (!strip) {
      strip = document.createElement('div');
      strip.className = 'ppf-live-strip';
      strip.innerHTML = `
        <span class="ppf-live-mark" aria-hidden="true">P</span>
        <span class="ppf-live-copy"><small>PLACID LIVE</small><strong data-ppf-room></strong></span>
        <span class="ppf-live-count" data-ppf-unread></span>`;
      home.appendChild(strip);
    }

    const room = currentRoomLabel();
    const unread = unreadCount();
    const roomNode = strip.querySelector('[data-ppf-room]');
    const unreadNode = strip.querySelector('[data-ppf-unread]');

    if (roomNode) roomNode.textContent = room || 'ParadiseRP';
    if (unreadNode) unreadNode.textContent = unread > 0 ? `${unread} msg` : 'En ligne';
    strip.hidden = false;
  }

  function tagPages(phone) {
    phone.querySelectorAll('.pp-app-page').forEach(page => {
      const title = text(page.querySelector('header strong')?.textContent).toLowerCase();
      page.dataset.ppFinalPage = title
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .replace(/[^a-z0-9]+/g, '-');
    });
  }

  function enhance() {
    const phone = root();
    if (!phone) return;

    phone.dataset.ppFinal = '1';
    const home = phone.querySelector('.pp-home');
    phone.classList.toggle('ppf-is-home', Boolean(home));

    enhanceNav(phone);
    tagPages(phone);

    if (home) {
      ensureScene(home);
      ensureLiveStrip(home);
    }
  }

  function onClick(event) {
    const target = event.target;
    if (!(target instanceof Element)) return;
    if (!target.closest('#paradise-rp-hud .pp-device')) return;
    schedule();
    window.setTimeout(schedule, 80);
  }

  function onPhoneEvent() {
    schedule();
  }

  function onStoreChange(_state, eventName) {
    if (['room:change', 'gameplay:snapshot', 'phone:update', 'ui:change'].includes(eventName)) schedule();
  }

  function boot() {
    document.addEventListener('click', onClick, true);
    window.addEventListener('paradise:phone', onPhoneEvent, false);
    window.addEventListener('paradise:store-change', onPhoneEvent, false);
    if (window.ParadiseStore?.subscribe) unsubscribe = window.ParadiseStore.subscribe(onStoreChange) || (() => {});
    schedule();
  }

  function destroy() {
    if (destroyed) return;
    destroyed = true;
    document.removeEventListener('click', onClick, true);
    window.removeEventListener('paradise:phone', onPhoneEvent, false);
    window.removeEventListener('paradise:store-change', onPhoneEvent, false);
    try { unsubscribe(); } catch (_) {}
  }

  window.ParadisePhoneFinalPolish = Object.freeze({
    version: '1.0.0-final-home-composition',
    refresh: schedule,
    getStatus: () => ({
      version: '1.0.0-final-home-composition',
      mounted: Boolean(root()?.dataset.ppFinal === '1'),
      home: Boolean(root()?.classList.contains('ppf-is-home')),
      room: currentRoomLabel() || null,
      unread: unreadCount()
    })
  });

  window.addEventListener('beforeunload', destroy, { once: true });
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
