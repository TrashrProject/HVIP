(() => {
  'use strict';

  if (window.ParadiseQualityGatePhone) return;

  const VERSION = '1.1.0-core-v1-phone-fixes';
  const ACTION_URL = '../rp-phone-action.php';
  let destroyed = false;
  let readRequest = null;
  let scheduled = false;

  function schedulePolish() {
    if (destroyed || scheduled) return;
    scheduled = true;
    requestAnimationFrame(() => requestAnimationFrame(() => {
      scheduled = false;
      if (!destroyed) pruneInactiveSettings();
    }));
  }

  function pruneInactiveSettings() {
    const device = document.querySelector('#paradise-rp-hud .pp-device');
    if (!device) return;
    device.querySelectorAll('input[data-pp-setting="silent"],input[data-pp-setting="sounds"]').forEach(input => {
      const row = input.closest('label');
      if (row) row.remove();
    });
    const settings = device.querySelector('.pp-settings');
    if (settings) settings.dataset.ppQualitySettings = 'notifications-only';
  }

  async function markNotificationsRead() {
    if (destroyed || readRequest) return readRequest;
    const phone = window.ParadiseStore?.getState?.().phone;
    if (!phone?.available || !Array.isArray(phone.notifications) || !phone.notifications.some(item => !item?.read)) return false;

    readRequest = (async () => {
      try {
        const response = await fetch(ACTION_URL, {
          method: 'POST',
          credentials: 'same-origin',
          cache: 'no-store',
          headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            'X-Paradise-Action': 'phase4'
          },
          body: JSON.stringify({ action: 'read_notifications' })
        });
        if (!response.ok) return false;
        const payload = await response.json();
        if (!payload?.ok) return false;
        await window.ParadisePhoneV1?.refresh?.();
        return true;
      } catch (_) {
        return false;
      } finally {
        readRequest = null;
      }
    })();

    return readRequest;
  }

  function onClick(event) {
    const target = event.target;
    if (!(target instanceof Element)) return;
    if (!target.closest('#paradise-rp-hud .pp-device')) return;

    const notifications = target.closest('[data-pp-app="notifications"]');
    if (notifications) window.setTimeout(markNotificationsRead, 0);
    window.setTimeout(schedulePolish, 0);
    window.setTimeout(schedulePolish, 80);
  }

  function onPhoneEvent() {
    schedulePolish();
  }

  function destroy() {
    if (destroyed) return;
    destroyed = true;
    document.removeEventListener('click', onClick, false);
    window.removeEventListener('paradise:phone', onPhoneEvent, false);
  }

  document.addEventListener('click', onClick, false);
  window.addEventListener('paradise:phone', onPhoneEvent, false);
  window.addEventListener('beforeunload', destroy, { once: true });
  schedulePolish();

  window.ParadiseQualityGatePhone = Object.freeze({
    version: VERSION,
    markNotificationsRead,
    refresh: schedulePolish,
    getStatus: () => ({
      version: VERSION,
      destroyed,
      pending: Boolean(readRequest),
      settingsMode: document.querySelector('#paradise-rp-hud .pp-settings')?.dataset.ppQualitySettings || null
    })
  });
})();
