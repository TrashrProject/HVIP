(() => {
  'use strict';

  if (window.__ParadisePhoneFinalPolish) return;
  window.__ParadisePhoneFinalPolish = '1.1.0-final-polish';

  let destroyed = false;
  let scheduled = false;
  let unsubscribe = () => {};

  const root = () => document.querySelector('#paradise-rp-hud .pp-device');
  const text = value => value == null ? '' : String(value).trim();

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

  function enhanceNav(phone, isHome) {
    const nav = phone.querySelector('.pp-nav');
    if (!nav) return;

    const home = nav.querySelector('[data-pp-home]');
    const close = nav.querySelector('[data-window-close="phone"]');

    if (home && home.dataset.ppFinalNav !== '1') {
      home.innerHTML = navIcons.home;
      home.dataset.ppFinalNav = '1';
      home.title = 'Accueil ParadisePhone';
      home.setAttribute('aria-label', 'Accueil ParadisePhone');
    }

    if (home) {
      home.classList.toggle('is-active', isHome);
      if (isHome) home.setAttribute('aria-current', 'page');
      else home.removeAttribute('aria-current');
    }

    if (close && close.dataset.ppFinalNav !== '1') {
      close.innerHTML = navIcons.close;
      close.dataset.ppFinalNav = '1';
      close.title = 'Fermer ParadisePhone';
      close.setAttribute('aria-label', 'Fermer ParadisePhone');
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

  function removeNonFunctionalLiveStrip(home) {
    home?.querySelectorAll('.ppf-live-strip').forEach(node => node.remove());
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
    const isHome = Boolean(home);
    phone.classList.toggle('ppf-is-home', isHome);

    enhanceNav(phone, isHome);
    tagPages(phone);

    if (home) {
      ensureScene(home);
      removeNonFunctionalLiveStrip(home);
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
    if ([
      'room:change',
      'gameplay:snapshot',
      'phone:update',
      'ui:change',
      'player:update',
      'character:update'
    ].includes(eventName)) {
      window.ParadisePhoneVisualParity?.refresh?.();
      schedule();
    }
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
    version: '1.1.0-final-polish',
    refresh: schedule,
    getStatus: () => {
      const phone = root();
      const state = store();
      return {
        version: '1.1.0-final-polish',
        mounted: Boolean(phone?.dataset.ppFinal === '1'),
        home: Boolean(phone?.classList.contains('ppf-is-home')),
        liveWidget: Boolean(phone?.querySelector('.ppf-live-strip')),
        username: text(state.gameplay?.player?.username) || null,
        look: text(state.gameplay?.player?.look) || null,
        phoneNumber: text(state.phone?.number) || null
      };
    }
  });

  window.addEventListener('beforeunload', destroy, { once: true });
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
