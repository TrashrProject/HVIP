(() => {
  'use strict';

  if (window.ParadiseQualityGatePhone) return;

  const VERSION = '1.0.0-core-v1-phone-fixes';
  const ACTION_URL = '../rp-phone-action.php';
  let destroyed = false;
  let readRequest = null;

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
    const notifications = target.closest('#paradise-rp-hud .pp-device [data-pp-app="notifications"]');
    if (!notifications) return;
    window.setTimeout(markNotificationsRead, 0);
  }

  function destroy() {
    if (destroyed) return;
    destroyed = true;
    document.removeEventListener('click', onClick, false);
  }

  document.addEventListener('click', onClick, false);
  window.addEventListener('beforeunload', destroy, { once: true });

  window.ParadiseQualityGatePhone = Object.freeze({
    version: VERSION,
    markNotificationsRead,
    getStatus: () => ({ version: VERSION, destroyed, pending: Boolean(readRequest) })
  });
})();
