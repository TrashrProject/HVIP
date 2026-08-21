(() => {
  'use strict';
  if (window.__ParadisePhoneV1Ux) return;
  window.__ParadisePhoneV1Ux = '2.0.0-quality-gate';

  let destroyed = false;
  let scheduled = false;

  const phoneRoot = () => document.querySelector('#paradise-rp-hud .pp-device');

  function scheduleEnhance() {
    if (destroyed || scheduled) return;
    scheduled = true;
    requestAnimationFrame(() => {
      scheduled = false;
      enhanceMessages();
    });
  }

  function messagePage() {
    const root = phoneRoot();
    if (!root) return null;
    return [...root.querySelectorAll('.pp-app-page')].find(node => node.querySelector('header strong')?.textContent?.trim() === 'Messages') || null;
  }

  function createSheet() {
    const form = document.createElement('form');
    form.className = 'pp-new-message-sheet';
    form.dataset.ppNewMessageSheet = '1';
    form.hidden = true;
    form.innerHTML = `
      <input maxlength="64" autocomplete="off" data-pp-new-target placeholder="Contact ou 555-XXXX" aria-label="Destinataire">
      <textarea maxlength="500" rows="2" data-pp-new-body placeholder="Écrire un message..." aria-label="Message"></textarea>
      <div class="pp-new-message-actions">
        <button type="button" data-pp-new-cancel>Annuler</button>
        <button type="submit" class="is-send">Envoyer</button>
      </div>`;
    return form;
  }

  function enhanceMessages() {
    const page = messagePage();
    if (!page) return;
    const header = page.querySelector('header');
    const list = page.querySelector('.pp-list');
    if (!header || !list) return;

    let trigger = header.querySelector('[data-pp-new-message-trigger]');
    if (!trigger) {
      trigger = document.createElement('button');
      trigger.type = 'button';
      trigger.className = 'pp-new-message-trigger';
      trigger.dataset.ppNewMessageTrigger = '1';
      trigger.title = 'Nouveau message';
      trigger.setAttribute('aria-label', 'Nouveau message');
      const slot = header.lastElementChild;
      if (slot && slot.tagName === 'SPAN' && !slot.textContent.trim()) slot.replaceWith(trigger);
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
      button.className = 'pp-primary';
      button.dataset.ppEmptyNewMessage = '1';
      button.textContent = 'Nouveau message';
      empty.appendChild(button);
    }
  }

  function openSheet(page = messagePage()) {
    if (!page) return;
    const sheet = page.querySelector('[data-pp-new-message-sheet]');
    if (!sheet) return;
    sheet.hidden = false;
    requestAnimationFrame(() => sheet.querySelector('[data-pp-new-target]')?.focus());
  }

  function closeSheet(page = messagePage(), clear = false) {
    if (!page) return;
    const sheet = page.querySelector('[data-pp-new-message-sheet]');
    if (!sheet) return;
    sheet.hidden = true;
    if (clear) {
      const target = sheet.querySelector('[data-pp-new-target]');
      const body = sheet.querySelector('[data-pp-new-body]');
      if (target) target.value = '';
      if (body) body.value = '';
    }
  }

  async function submitNewSms(form) {
    const target = String(form.querySelector('[data-pp-new-target]')?.value || '').trim();
    const body = String(form.querySelector('[data-pp-new-body]')?.value || '').trim();
    if (!target || !body) return;
    const button = form.querySelector('button[type="submit"]');
    if (button) button.disabled = true;
    try {
      await window.ParadisePhoneV1?.send?.(target, body);
      closeSheet(form.closest('.pp-app-page'), true);
      await window.ParadisePhoneV1?.refresh?.();
      scheduleEnhance();
    } catch (_) {
      // Main ParadisePhone action layer owns user-visible error feedback.
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
      closeSheet(messagePage());
      return;
    }
    if (target.closest('[data-pp-app="messages"],[data-pp-home],.pp-back')) {
      requestAnimationFrame(scheduleEnhance);
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
    const form = input.closest('form');
    if (form) form.requestSubmit();
  }

  function onPhoneUpdate() {
    requestAnimationFrame(scheduleEnhance);
  }

  function boot() {
    document.addEventListener('click', onClick, true);
    document.addEventListener('submit', onSubmit, true);
    document.addEventListener('keydown', onKeyDown, true);
    window.addEventListener('paradise:phone', onPhoneUpdate, false);
    scheduleEnhance();
  }

  function destroy() {
    destroyed = true;
    document.removeEventListener('click', onClick, true);
    document.removeEventListener('submit', onSubmit, true);
    document.removeEventListener('keydown', onKeyDown, true);
    window.removeEventListener('paradise:phone', onPhoneUpdate, false);
  }

  window.addEventListener('beforeunload', destroy, { once: true });
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();