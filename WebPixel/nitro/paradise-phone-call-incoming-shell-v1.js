(() => {
  'use strict';

  if (window.__PARADISE_PHONE_CALL_INCOMING_SHELL_V1__) return;
  window.__PARADISE_PHONE_CALL_INCOMING_SHELL_V1__ = '1.0.0';

  const CHECK_MS = 250;
  const OPEN_RETRY_MS = 1400;
  let lastOpenAttempt = 0;

  const getFrame = () => document.querySelector('.nitro-phone-frame');
  const getIncomingLayer = () => document.querySelector('.paradise-call-layer .pcall-card.pcall-incoming')?.closest('.paradise-call-layer') || null;

  function frameVisible(frame) {
    if (!frame) return false;
    const style = getComputedStyle(frame);
    const rect = frame.getBoundingClientRect();
    return style.display !== 'none' && style.visibility !== 'hidden' && Number(style.opacity || 1) > 0 && rect.width > 120 && rect.height > 220;
  }

  function phoneButton() {
    const shell = document.querySelector('.roleplay-left-menu');
    if (!shell) return null;

    return shell.querySelector(
      '.left-menu-button[data-paradise-label="Téléphone"], .left-menu-button[aria-label="Téléphone"], .left-menu-button[title="Téléphone"]'
    ) || [...shell.querySelectorAll('.roleplay-left-menu-buttons .left-menu-button')][4] || null;
  }

  function mountLayerInsidePhone(frame, layer) {
    if (!frame || !layer) return false;
    if (layer.parentElement !== frame) frame.appendChild(layer);
    layer.classList.remove('is-floating');
    return true;
  }

  function tick() {
    const layer = getIncomingLayer();
    if (!layer) return;

    const frame = getFrame();
    if (frameVisible(frame)) {
      mountLayerInsidePhone(frame, layer);
      return;
    }

    const now = Date.now();
    if (now - lastOpenAttempt < OPEN_RETRY_MS) return;

    const button = phoneButton();
    if (!button) return;

    lastOpenAttempt = now;
    button.click();

    // Nitro mounts the frame asynchronously after the native rail click.
    [80, 180, 360, 700].forEach(delay => {
      window.setTimeout(() => {
        const mountedFrame = getFrame();
        const incomingLayer = getIncomingLayer();
        if (frameVisible(mountedFrame) && incomingLayer) mountLayerInsidePhone(mountedFrame, incomingLayer);
      }, delay);
    });
  }

  window.setInterval(tick, CHECK_MS);
  console.info('[ParadisePhone] incoming call opens physical phone shell V1');
})();
