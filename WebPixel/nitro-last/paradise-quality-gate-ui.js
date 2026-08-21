(() => {
  'use strict';

  if (window.ParadiseQualityGateUi) return;

  const VERSION = '1.0.0-core-v1-ui-coherence';
  let destroyed = false;
  let scheduled = false;

  function schedule() {
    if (destroyed || scheduled) return;
    scheduled = true;
    requestAnimationFrame(() => requestAnimationFrame(() => {
      scheduled = false;
      if (!destroyed) reconcile();
    }));
  }

  function reconcile() {
    const hud = document.getElementById('paradise-rp-hud');
    if (!hud) return;

    // Vehicles must remain in the validated bottom-left HUD structure, but the
    // duplicate Actions shortcut is misleading until a real vehicle domain exists.
    hud.querySelector('.pr-actions-menu [data-window-open="vehicles"]')?.remove();

    const bell = hud.querySelector('[data-action="notifications"]');
    if (bell) {
      bell.title = 'Notifications ParadisePhone';
      bell.setAttribute('aria-label', 'Ouvrir les notifications ParadisePhone');
      bell.dataset.qgNotificationRoute = 'phone';
    }
  }

  function onClick(event) {
    const target = event.target;
    if (!(target instanceof Element)) return;
    const bell = target.closest('#paradise-rp-hud [data-action="notifications"][data-qg-notification-route="phone"]');
    if (!bell) return;

    event.preventDefault();
    event.stopPropagation();
    event.stopImmediatePropagation();

    const phone = window.ParadiseStore?.getState?.().phone;
    if (!phone?.available || !phone?.hasDevice) {
      window.ParadiseSystemFeedback?.show?.('Vous devez posséder un téléphone pour consulter vos notifications.', 'ERROR');
      return;
    }
    window.ParadisePhoneV1?.open?.('notifications');
  }

  function destroy() {
    if (destroyed) return;
    destroyed = true;
    document.removeEventListener('click', onClick, true);
    window.removeEventListener('paradise:store-change', schedule, false);
  }

  document.addEventListener('click', onClick, true);
  window.addEventListener('paradise:store-change', schedule, false);
  window.addEventListener('beforeunload', destroy, { once: true });
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', schedule, { once: true });
  else schedule();

  window.ParadiseQualityGateUi = Object.freeze({
    version: VERSION,
    refresh: schedule,
    getStatus: () => ({
      version: VERSION,
      destroyed,
      actionsVehiclesRemoved: !document.querySelector('#paradise-rp-hud .pr-actions-menu [data-window-open="vehicles"]'),
      notificationBellRouted: Boolean(document.querySelector('#paradise-rp-hud [data-qg-notification-route="phone"]'))
    })
  });
})();
