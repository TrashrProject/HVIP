(() => {
  'use strict';

  if (window.ParadiseQualityGateInput) return;

  const VERSION = '1.0.0-core-v1-input-escape';
  let destroyed = false;

  const isEditable = element => Boolean(element && (
    element.matches?.('input, textarea, select, [contenteditable="true"]') ||
    element.closest?.('[contenteditable="true"]')
  ));

  function stop(event) {
    event.preventDefault();
    event.stopPropagation();
    event.stopImmediatePropagation();
  }

  function phoneIsOpen() {
    return window.ParadiseWindowManager?.getActiveWindow?.() === 'phone' &&
      Boolean(document.querySelector('#paradise-rp-hud .pp-device'));
  }

  function handlePhoneEscape(event) {
    const device = document.querySelector('#paradise-rp-hud .pp-device');
    if (!device) return false;

    const conversationBack = device.querySelector('.pp-chat header [data-pp-app="messages"]');
    if (conversationBack) {
      stop(event);
      conversationBack.click();
      return true;
    }

    const appPage = device.querySelector('.pp-app-page, .pp-call');
    if (appPage) {
      stop(event);
      window.ParadisePhoneV1?.open?.('home');
      return true;
    }

    stop(event);
    window.ParadiseWindowManager?.closeWindow?.('phone');
    return true;
  }

  function onKeyDown(event) {
    if (destroyed || event.key !== 'Escape') return;

    const active = document.activeElement;
    if (isEditable(active) && active?.closest?.('#paradise-ui-root')) {
      stop(event);
      active.blur?.();
      return;
    }

    if (phoneIsOpen() && handlePhoneEscape(event)) return;

    const state = window.ParadiseStore?.getState?.();
    if (state?.ui?.activeWindow) {
      stop(event);
      window.ParadiseWindowManager?.closeWindow?.(state.ui.activeWindow);
      return;
    }

    if (state?.ui?.actionsOpen) {
      stop(event);
      window.ParadiseStore?.setUi?.({ actionsOpen: false });
    }
  }

  function destroy() {
    if (destroyed) return;
    destroyed = true;
    window.removeEventListener('keydown', onKeyDown, true);
  }

  window.addEventListener('keydown', onKeyDown, true);
  window.addEventListener('beforeunload', destroy, { once: true });

  window.ParadiseQualityGateInput = Object.freeze({
    version: VERSION,
    destroy,
    getStatus: () => ({ version: VERSION, destroyed })
  });
})();
