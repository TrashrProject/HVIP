(() => {
  'use strict';

  if (window.__PARADISE_PHONE_CALL_CLOSE_GUARD_V9__) return;
  window.__PARADISE_PHONE_CALL_CLOSE_GUARD_V9__ = '9.0.0';

  const FRAME_SELECTOR = '.nitro-phone-frame';
  const LAYER_SELECTOR = '.paradise-call-layer.paradise-call-stable-v2';
  const CLOSE_SELECTOR = [
    '.phone-close',
    '[class*="phone-close"]',
    '[data-phone-close]',
    '[aria-label*="fermer" i]',
    '[title*="fermer" i]',
    '[aria-label*="close" i]',
    '[title*="close" i]'
  ].join(',');

  let shellGoneSince = 0;
  let fallbackBusy = false;

  function activeLayer() {
    return document.querySelector(LAYER_SELECTOR);
  }

  function hasLiveCall(layer) {
    return !!layer?.querySelector('.pcall-active,.pcall-outgoing');
  }

  function isIncoming(layer) {
    return !!layer?.querySelector('.pcall-incoming');
  }

  function clickCallAction(kind) {
    const layer = activeLayer();
    if (!layer) return false;

    const selector = kind === 'decline'
      ? '[data-pcall-stable-decline]'
      : '[data-pcall-stable-hangup]';

    const button = layer.querySelector(selector);
    if (!(button instanceof HTMLElement)) return false;

    button.click();
    return true;
  }

  function looksLikePhysicalClose(target, frame) {
    if (!(target instanceof Element) || !frame) return false;
    if (target.closest(CLOSE_SELECTOR)) return true;

    const button = target.closest('button,[role="button"]');
    if (!button || !frame.contains(button)) return false;

    const text = (button.textContent || '').trim().toLowerCase();
    const label = `${button.getAttribute('aria-label') || ''} ${button.getAttribute('title') || ''}`.toLowerCase();
    if (/^(×|✕|x)$/.test(text) || /fermer|close/.test(label)) return true;

    // Last-resort match for the red physical X in the phone's top-right corner.
    const fr = frame.getBoundingClientRect();
    const br = button.getBoundingClientRect();
    const nearRight = Math.abs(br.right - fr.right) <= 24 || br.left >= fr.right - 32;
    const nearTop = br.top <= fr.top + 28;
    return nearRight && nearTop;
  }

  function finishBeforeShellClose() {
    const layer = activeLayer();
    if (!layer) return;

    // Closing the physical phone while it rings means declining that incoming call.
    if (isIncoming(layer)) {
      clickCallAction('decline');
      return;
    }

    // An established/outgoing call must be hung up before React removes the shell.
    if (hasLiveCall(layer)) clickCallAction('hangup');
  }

  document.addEventListener('pointerdown', event => {
    const frame = event.target instanceof Element ? event.target.closest(FRAME_SELECTOR) : null;
    if (!frame || !looksLikePhysicalClose(event.target, frame)) return;
    finishBeforeShellClose();
  }, true);

  document.addEventListener('click', event => {
    const frame = event.target instanceof Element ? event.target.closest(FRAME_SELECTOR) : null;
    if (!frame || !looksLikePhysicalClose(event.target, frame)) return;
    finishBeforeShellClose();
  }, true);

  // Safety net: if another part of Nitro removes the phone shell without a click,
  // never leave the call surface floating by itself over the hotel.
  window.setInterval(() => {
    const frame = document.querySelector(FRAME_SELECTOR);
    const layer = activeLayer();

    if (!layer || !hasLiveCall(layer)) {
      shellGoneSince = 0;
      fallbackBusy = false;
      return;
    }

    if (frame) {
      shellGoneSince = 0;
      fallbackBusy = false;
      return;
    }

    if (!shellGoneSince) shellGoneSince = Date.now();
    if (Date.now() - shellGoneSince < 650 || fallbackBusy) return;

    fallbackBusy = true;
    const hungUp = clickCallAction('hangup');

    // Even if Stable V2 is between two render states and exposes no hangup button,
    // the detached visual must not remain on screen. Server heartbeat cleanup remains
    // the final safety net for that rare case.
    if (!hungUp) layer.remove();
  }, 250);

  // Prevent even a single visible frame of the detached/ghost call surface.
  const style = document.createElement('style');
  style.id = 'paradise-phone-call-close-guard-v9-style';
  style.textContent = `
    body > .paradise-call-layer.paradise-call-stable-v2.is-floating:has(.pcall-active),
    body > .paradise-call-layer.paradise-call-stable-v2.is-floating:has(.pcall-outgoing){
      display:none!important;
      visibility:hidden!important;
      pointer-events:none!important;
    }
  `;
  document.head.appendChild(style);

  console.info('[ParadisePhone] call close guard V9 active');
})();