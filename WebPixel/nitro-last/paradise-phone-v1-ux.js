(() => {
  'use strict';

  if (window.__ParadisePhoneV1Ux === '3.0.0-visual-parity') return;
  window.__ParadisePhoneV1Ux = '3.0.0-visual-parity';

  let destroyed = false;
  let scheduled = false;
  let unsubscribe = () => {};

  const phoneRoot = () => document.querySelector('#paradise-rp-hud .pp-device');
  const text = value => value == null ? '' : String(value).trim();
  const esc = value => String(value ?? '')
    .replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;').replaceAll("'", '&#039;');

  const icons = {
    messages: `<svg viewBox="0 0 48 48" aria-hidden="true"><path class="ppp-icon-shadow" d="M9 13h30v22H20l-8 7v-7H9z"/><path class="ppp-icon-main" d="M7 10h31v22H18l-8 7v-7H7z"/><path class="ppp-icon-light" d="M12 16h20v3H12zm0 6h15v3H12z"/><path class="ppp-icon-pixel" d="M34 13h3v3h-3z"/></svg>`,
    contacts: `<svg viewBox="0 0 48 48" aria-hidden="true"><path class="ppp-icon-shadow" d="M12 7h27v35H12z"/><path class="ppp-icon-main" d="M9 5h27v35H9z"/><path class="ppp-icon-light" d="M14 10h16v4H14z"/><circle class="ppp-icon-accent" cx="23" cy="22" r="6"/><path class="ppp-icon-accent" d="M14 35c1-7 5-10 9-10s8 3 9 10z"/><path class="ppp-icon-pixel" d="M6 10h3v5H6zm0 9h3v5H6zm0 9h3v5H6z"/></svg>`,
    calls: `<svg viewBox="0 0 48 48" aria-hidden="true"><path class="ppp-icon-shadow" d="M13 8l8-2 5 11-6 5c3 7 7 11 14 14l5-6 6 3v7c0 4-4 7-8 7C22 44 7 29 5 14c0-4 4-7 8-6z"/><path class="ppp-icon-main" d="M11 6l8-2 5 11-6 5c3 7 7 11 14 14l5-6 7 3v7c0 4-4 7-8 7C20 42 5 27 3 12c0-4 4-7 8-6z"/><path class="ppp-icon-light" d="M10 10l5-1 3 6-4 3c3 8 8 13 16 16l3-4 6 3v2c0 2-2 4-4 4C22 36 9 23 7 11c0-1 2-2 3-1z"/></svg>`,
    notifications: `<svg viewBox="0 0 48 48" aria-hidden="true"><path class="ppp-icon-shadow" d="M13 34h26l-4-6V19c0-8-5-13-12-13S11 11 11 19v9l-4 6h6z"/><path class="ppp-icon-main" d="M11 32h26l-4-6V18c0-7-4-12-11-12S11 11 11 18v8l-4 6z"/><path class="ppp-icon-light" d="M17 27h10v-9c0-4-2-7-5-7s-5 3-5 7z"/><path class="ppp-icon-accent" d="M18 36h8c-1 5-7 5-8 0z"/><path class="ppp-icon-pixel" d="M31 9h4v4h-4z"/></svg>`,
    settings: `<svg viewBox="0 0 48 48" aria-hidden="true"><path class="ppp-icon-shadow" d="M20 4h9l2 6 6-2 5 8-5 5 3 6-6 7-6-2-3 6h-9l-2-6-6 2-5-8 5-5-3-6 6-7 6 2z"/><path class="ppp-icon-main" d="M18 3h9l2 6 6-2 5 8-5 5 3 6-6 7-6-2-3 6h-9l-2-6-6 2-5-8 5-5-3-6 6-7 6 2z"/><circle class="ppp-icon-light" cx="22" cy="22" r="8"/><circle class="ppp-icon-accent" cx="22" cy="22" r="4"/></svg>`,
    message: `<svg viewBox="0 0 32 32" aria-hidden="true"><path class="ppp-icon-main" d="M4 6h24v17H13l-7 5v-5H4z"/><path class="ppp-icon-light" d="M8 11h15v2H8zm0 5h11v2H8z"/></svg>`,
    call: `<svg viewBox="0 0 32 32" aria-hidden="true"><path class="ppp-icon-main" d="M7 3h7l3 8-5 4c2 5 5 8 10 10l4-5 5 2v6c0 2-2 4-5 4C14 29 3 18 2 7c0-2 2-4 5-4z"/></svg>`,
    plus: `<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M10 4h4v6h6v4h-6v6h-4v-6H4v-4h6z"/></svg>`,
    back: `<svg viewBox="0 0 24 24" aria-hidden="true"><path d="m14.5 5-7 7 7 7 2-2-5-5 5-5z"/></svg>`
  };

  function scheduleEnhance() {
    if (destroyed || scheduled) return;
    scheduled = true;
    requestAnimationFrame(() => requestAnimationFrame(() => {
      scheduled = false;
      if (!destroyed) enhanceAll();
    }));
  }

  function playerData() {
    const store = window.ParadiseStore?.getState?.() || {};
    const player = store.gameplay?.player || {};
    const character = store.character || {};
    const fullName = text(character.fullName) ||
      [text(character.firstName), text(character.lastName)].filter(Boolean).join(' ');
    return {
      username: text(player.username) || 'Citoyen',
      displayName: fullName || text(player.username) || 'Citoyen',
      look: text(player.look)
    };
  }

  function avatarMarkup(player) {
    if (!player.look || !/^[a-z0-9.\-]+$/i.test(player.look)) {
      return `<span class="ppp-avatar-fallback">${esc((player.username || 'P').slice(0, 1).toUpperCase())}</span>`;
    }
    const url = `../avatar-image.php?figure=${encodeURIComponent(player.look)}&direction=2&head_direction=3&gesture=sml&action=std&size=m&phone=parity`;
    return `<img src="${esc(url)}" alt="${esc(player.username)}" draggable="false">`;
  }

  function enhanceFrame(root) {
    root.dataset.ppVisualParity = '1';
    const brand = root.querySelector('.pp-status > span');
    if (brand) brand.textContent = 'PARADISE';
    const notch = root.querySelector('.pp-notch');
    if (notch) notch.setAttribute('aria-hidden', 'true');
  }

  function enhanceHero(root) {
    const hero = root.querySelector('.pp-home .pp-hero');
    if (!hero) return;
    const number = text(hero.querySelector('strong')?.textContent) || '---';
    const player = playerData();
    const signature = `${number}|${player.username}|${player.look}`;
    if (hero.dataset.ppParitySignature === signature) return;
    hero.dataset.ppParitySignature = signature;
    hero.innerHTML = `
      <div class="ppp-card-copy">
        <small>PLACID ISLAND</small>
        <strong>${esc(number)}</strong>
        <span>ParadisePhone</span>
        <b>${esc(player.displayName)}</b>
      </div>
      <div class="ppp-card-avatar">${avatarMarkup(player)}</div>
      <i class="ppp-card-chip" aria-hidden="true"><span></span></i>
      <em class="ppp-card-mark" aria-hidden="true">P</em>`;
  }

  function appIconKey(node) {
    const raw = text(node?.dataset?.icon).toLowerCase();
    if (raw === 'notification') return 'notifications';
    if (raw === 'phone') return 'calls';
    if (raw === 'call') return 'call';
    if (raw === 'message') return 'message';
    return raw;
  }

  function enhanceIcons(root) {
    root.querySelectorAll('.pp-app-icon[data-icon]').forEach(node => {
      const key = appIconKey(node);
      const markup = icons[key];
      if (!markup) return;
      if (node.dataset.ppParityIcon === key) return;
      node.innerHTML = markup;
      node.dataset.ppParityIcon = key;
      node.setAttribute('aria-hidden', 'true');
    });

    root.querySelectorAll('.pp-app[data-pp-app="calls"] > span:last-of-type').forEach(label => {
      if (text(label.textContent) === 'Téléphone') label.textContent = 'Appels';
    });
  }

  function addEmptyArt(empty, type) {
    if (!empty || empty.querySelector('[data-pp-parity-empty-art]')) return;
    const art = document.createElement('div');
    art.className = `ppp-empty-art is-${type}`;
    art.dataset.ppParityEmptyArt = '1';
    art.innerHTML = type === 'contacts'
      ? `<span class="ppp-empty-avatar">${icons.contacts}</span><i></i><span class="ppp-empty-card">${icons.message}</span>`
      : `<span>${icons.messages}</span><i></i><b></b>`;
    empty.prepend(art);
  }

  function enhanceContacts(root) {
    const page = [...root.querySelectorAll('.pp-app-page')].find(node =>
      text(node.querySelector('header strong')?.textContent) === 'Contacts');
    if (!page) return;
    const header = page.querySelector('header');
    const add = page.querySelector('[data-pp-add-contact]');
    if (header && add && !add.classList.contains('ppp-header-action')) {
      add.classList.add('ppp-header-action');
      add.innerHTML = `${icons.plus}<span>Ajouter</span>`;
      const slot = header.lastElementChild;
      if (slot && slot !== add) slot.replaceWith(add);
    }

    const empty = page.querySelector('.pp-empty');
    addEmptyArt(empty, 'contacts');

    page.querySelectorAll('.pp-row-actions button').forEach(button => {
      if (button.dataset.ppMessageNumber) {
        button.classList.add('is-message');
        button.setAttribute('aria-label', 'Message');
        button.title = 'Message';
        button.innerHTML = icons.message;
      } else if (button.dataset.ppCallNumber) {
        button.classList.add('is-call');
        button.setAttribute('aria-label', 'Appeler');
        button.title = 'Appeler';
        button.innerHTML = icons.call;
      } else if (button.dataset.ppDeleteContact) {
        button.classList.add('is-delete');
        button.setAttribute('aria-label', 'Supprimer');
        button.title = 'Supprimer';
        button.textContent = '×';
      }
    });
  }

  function enhanceMessages(root) {
    const page = [...root.querySelectorAll('.pp-app-page')].find(node =>
      text(node.querySelector('header strong')?.textContent) === 'Messages');
    if (page) {
      const trigger = page.querySelector('[data-pp-new-message-trigger]');
      if (trigger) {
        trigger.classList.add('ppp-new-message');
        trigger.innerHTML = icons.plus;
      }
      const empty = page.querySelector('.pp-empty');
      addEmptyArt(empty, 'messages');
    }

    const chat = root.querySelector('.pp-chat');
    if (chat) {
      const call = chat.querySelector('header > button:last-child');
      if (call?.dataset.ppCallNumber) {
        call.classList.add('ppp-chat-call');
        call.setAttribute('aria-label', 'Appeler');
        call.title = 'Appeler';
        call.innerHTML = icons.call;
      }
    }
  }

  function enhanceCalls(root) {
    const page = [...root.querySelectorAll('.pp-app-page')].find(node => {
      const title = text(node.querySelector('header strong')?.textContent);
      return title === 'Téléphone' || title === 'Appels';
    });
    if (page) {
      const title = page.querySelector('header strong');
      if (title) title.textContent = 'Appels';
    }

    const call = root.querySelector('.pp-call');
    if (!call) return;
    call.querySelectorAll('.pp-call-actions button').forEach(button => {
      if (button.dataset.ppParityButton === '1') return;
      const label = text(button.textContent);
      const iconMarkup = button.classList.contains('is-answer') ? icons.call : `<svg viewBox="0 0 32 32" aria-hidden="true"><path d="M6 24 24 6l2 2L8 26zM6 8l2-2 18 18-2 2z"/></svg>`;
      button.innerHTML = `${iconMarkup}<span>${esc(label)}</span>`;
      button.dataset.ppParityButton = '1';
    });
  }

  function enhanceNotifications(root) {
    const page = [...root.querySelectorAll('.pp-app-page')].find(node =>
      text(node.querySelector('header strong')?.textContent) === 'Notifications');
    if (!page) return;
    const empty = page.querySelector('.pp-empty');
    if (empty && !empty.querySelector('[data-pp-parity-empty-art]')) {
      const art = document.createElement('div');
      art.className = 'ppp-empty-art is-notifications';
      art.dataset.ppParityEmptyArt = '1';
      art.innerHTML = icons.notifications;
      empty.prepend(art);
    }
  }

  function enhanceSettings(root) {
    const page = [...root.querySelectorAll('.pp-app-page')].find(node =>
      text(node.querySelector('header strong')?.textContent) === 'Paramètres');
    if (!page) return;
    page.querySelectorAll('.pp-settings label').forEach((label, index) => {
      label.dataset.ppSettingRow = String(index + 1);
    });
  }

  function enhanceBackButtons(root) {
    root.querySelectorAll('.pp-back').forEach(button => {
      if (button.dataset.ppParityBack === '1') return;
      button.innerHTML = icons.back;
      button.dataset.ppParityBack = '1';
      button.setAttribute('aria-label', 'Retour');
    });
  }

  function enhanceAll() {
    const root = phoneRoot();
    if (!root) return;
    enhanceFrame(root);
    enhanceHero(root);
    enhanceIcons(root);
    enhanceContacts(root);
    enhanceMessages(root);
    enhanceCalls(root);
    enhanceNotifications(root);
    enhanceSettings(root);
    enhanceBackButtons(root);
  }

  function openSheet(page = null) {
    const root = phoneRoot();
    const targetPage = page || [...(root?.querySelectorAll('.pp-app-page') || [])].find(node =>
      text(node.querySelector('header strong')?.textContent) === 'Messages');
    const sheet = targetPage?.querySelector('[data-pp-new-message-sheet]');
    if (!sheet) return;
    sheet.hidden = false;
    requestAnimationFrame(() => sheet.querySelector('[data-pp-new-target]')?.focus());
  }

  function closeSheet(page = null, clear = false) {
    const root = phoneRoot();
    const targetPage = page || [...(root?.querySelectorAll('.pp-app-page') || [])].find(node =>
      text(node.querySelector('header strong')?.textContent) === 'Messages');
    const sheet = targetPage?.querySelector('[data-pp-new-message-sheet]');
    if (!sheet) return;
    sheet.hidden = true;
    if (clear) {
      const target = sheet.querySelector('[data-pp-new-target]');
      const body = sheet.querySelector('[data-pp-new-body]');
      if (target) target.value = '';
      if (body) body.value = '';
    }
  }

  function createSheet() {
    const form = document.createElement('form');
    form.className = 'pp-new-message-sheet';
    form.dataset.ppNewMessageSheet = '1';
    form.hidden = true;
    form.innerHTML = `
      <div class="ppp-sheet-title"><strong>Nouveau message</strong><small>ParadisePhone</small></div>
      <input maxlength="64" autocomplete="off" data-pp-new-target placeholder="Contact ou 555-XXXX" aria-label="Destinataire">
      <textarea maxlength="500" rows="2" data-pp-new-body placeholder="Écrire un message..." aria-label="Message"></textarea>
      <div class="pp-new-message-actions">
        <button type="button" data-pp-new-cancel>Annuler</button>
        <button type="submit" class="is-send">Envoyer</button>
      </div>`;
    return form;
  }

  function ensureMessageComposer() {
    const root = phoneRoot();
    if (!root) return;
    const page = [...root.querySelectorAll('.pp-app-page')].find(node =>
      text(node.querySelector('header strong')?.textContent) === 'Messages');
    if (!page) return;
    const header = page.querySelector('header');
    const list = page.querySelector('.pp-list');
    if (!header || !list) return;

    let trigger = header.querySelector('[data-pp-new-message-trigger]');
    if (!trigger) {
      trigger = document.createElement('button');
      trigger.type = 'button';
      trigger.className = 'pp-new-message-trigger ppp-new-message';
      trigger.dataset.ppNewMessageTrigger = '1';
      trigger.title = 'Nouveau message';
      trigger.setAttribute('aria-label', 'Nouveau message');
      trigger.innerHTML = icons.plus;
      const slot = header.lastElementChild;
      if (slot && slot.tagName === 'SPAN' && !text(slot.textContent)) slot.replaceWith(trigger);
      else header.appendChild(trigger);
    }

    let sheet = page.querySelector('[data-pp-new-message-sheet]');
    if (!sheet) {
      sheet = createSheet();
      list.before(sheet);
    }

    const empty = list.querySelector('.pp-empty');
    if (empty && !empty.querySelector('[data-pp-empty-new-message]')) {
      const button = document.createElement('button');
      button.type = 'button';
      button.className = 'pp-primary ppp-empty-cta';
      button.dataset.ppEmptyNewMessage = '1';
      button.textContent = 'Nouveau message';
      empty.appendChild(button);
    }
  }

  async function submitNewSms(form) {
    const target = text(form.querySelector('[data-pp-new-target]')?.value);
    const body = text(form.querySelector('[data-pp-new-body]')?.value);
    if (!target || !body) return;
    const button = form.querySelector('button[type="submit"]');
    if (button) button.disabled = true;
    try {
      await window.ParadisePhoneV1?.send?.(target, body);
      closeSheet(form.closest('.pp-app-page'), true);
      await window.ParadisePhoneV1?.refresh?.();
      scheduleEnhance();
    } catch (_) {
      // ParadisePhone V1 already owns user-visible error feedback.
    } finally {
      if (button?.isConnected) button.disabled = false;
    }
  }

  function onClick(event) {
    const target = event.target;
    if (!(target instanceof Element)) return;

    if (target.closest('[data-pp-new-message-trigger],[data-pp-empty-new-message]')) {
      event.preventDefault();
      event.stopPropagation();
      openSheet();
      return;
    }
    if (target.closest('[data-pp-new-cancel]')) {
      event.preventDefault();
      event.stopPropagation();
      closeSheet();
      return;
    }

    if (target.closest('#paradise-rp-hud .pp-device,[data-window-open="phone"]')) {
      scheduleEnhance();
      requestAnimationFrame(ensureMessageComposer);
    }
  }

  function onSubmit(event) {
    const form = event.target.closest?.('[data-pp-new-message-sheet]');
    if (!form) return;
    event.preventDefault();
    event.stopPropagation();
    submitNewSms(form);
  }

  function onKeyDown(event) {
    const input = event.target;
    if (!(input instanceof HTMLTextAreaElement)) return;
    if (!input.matches('[data-pp-message-input],[data-pp-new-body]')) return;
    if (event.key !== 'Enter' || event.shiftKey) return;
    event.preventDefault();
    event.stopImmediatePropagation();
    input.closest('form')?.requestSubmit();
  }

  function onPhoneUpdate() {
    scheduleEnhance();
    requestAnimationFrame(ensureMessageComposer);
  }

  function onStoreChange(_store, eventName) {
    if (['ui:change', 'gameplay:snapshot', 'character:update'].includes(eventName)) scheduleEnhance();
  }

  function ensureStyleOrder() {
    const style = document.getElementById('paradise-phone-v1-ux-css');
    const guard = document.getElementById('paradise-clickthrough-guard-css');
    if (style && guard && style.nextElementSibling !== guard) {
      guard.parentNode?.insertBefore(style, guard);
    }
  }

  function boot() {
    ensureStyleOrder();
    document.addEventListener('click', onClick, true);
    document.addEventListener('submit', onSubmit, true);
    document.addEventListener('keydown', onKeyDown, true);
    window.addEventListener('paradise:phone', onPhoneUpdate, false);
    unsubscribe = window.ParadiseStore?.subscribe?.(onStoreChange) || (() => {});
    ensureMessageComposer();
    scheduleEnhance();
  }

  function destroy() {
    if (destroyed) return;
    destroyed = true;
    unsubscribe();
    document.removeEventListener('click', onClick, true);
    document.removeEventListener('submit', onSubmit, true);
    document.removeEventListener('keydown', onKeyDown, true);
    window.removeEventListener('paradise:phone', onPhoneUpdate, false);
  }

  window.ParadisePhoneVisualParity = Object.freeze({
    version: '3.0.0-visual-parity',
    refresh: scheduleEnhance,
    destroy,
    getStatus: () => ({
      version: '3.0.0-visual-parity',
      mounted: Boolean(phoneRoot()),
      destroyed
    })
  });

  window.addEventListener('beforeunload', destroy, { once: true });
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();