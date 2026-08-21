(() => {
  'use strict';
  if (window.__ParadisePhoneV1Ux) return;
  window.__ParadisePhoneV1Ux = '1.0.0';

  function phoneRoot() { return document.querySelector('#paradise-rp-hud .pp-device'); }

  function injectNewSms() {
    const root = phoneRoot();
    if (!root || root.querySelector('[data-pp-new-sms]')) return;
    const page = [...root.querySelectorAll('.pp-app-page')].find(node => node.querySelector('header strong')?.textContent?.trim() === 'Messages');
    if (!page) return;
    const list = page.querySelector('.pp-list');
    if (!list) return;
    const box = document.createElement('form');
    box.className = 'pp-new-sms';
    box.dataset.ppNewSms = '1';
    box.innerHTML = '<div><input maxlength="64" autocomplete="off" data-pp-new-target placeholder="Contact ou 555-0184"><textarea maxlength="500" rows="1" data-pp-new-body placeholder="Nouveau message..."></textarea><button type="submit">Envoyer</button></div>';
    list.before(box);
  }

  async function submitNewSms(form) {
    const target = String(form.querySelector('[data-pp-new-target]')?.value || '').trim();
    const body = String(form.querySelector('[data-pp-new-body]')?.value || '').trim();
    if (!target || !body) return;
    const button = form.querySelector('button[type="submit"]');
    if (button) button.disabled = true;
    try {
      await window.ParadisePhoneV1?.send?.(target, body);
      if (form.isConnected) {
        form.querySelector('[data-pp-new-body]').value = '';
        await window.ParadisePhoneV1?.refresh?.();
      }
    } catch (_) {
      // Main ParadisePhone action layer owns user-visible error feedback.
    } finally {
      if (button?.isConnected) button.disabled = false;
    }
  }

  document.addEventListener('submit', event => {
    const form = event.target.closest?.('[data-pp-new-sms]');
    if (!form) return;
    event.preventDefault();
    event.stopPropagation();
    submitNewSms(form);
  }, true);

  document.addEventListener('keydown', event => {
    const input = event.target;
    if (!(input instanceof HTMLTextAreaElement)) return;
    if (!input.matches('[data-pp-message-input],[data-pp-new-body]')) return;
    if (event.key !== 'Enter' || event.shiftKey) return;
    event.preventDefault();
    event.stopImmediatePropagation();
    const form = input.closest('form');
    if (form) form.requestSubmit();
  }, true);

  const observer = new MutationObserver(injectNewSms);
  function boot() {
    observer.observe(document.getElementById('paradise-ui-root') || document.body, { childList: true, subtree: true });
    injectNewSms();
  }
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once:true }); else boot();
})();
