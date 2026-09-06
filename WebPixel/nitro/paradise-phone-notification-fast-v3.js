(() => {
  'use strict';

  if (window.__PARADISE_PHONE_NOTIFICATION_FAST_V3__) return;
  window.__PARADISE_PHONE_NOTIFICATION_FAST_V3__ = '3.1.0';

  let observer = null;
  let scheduled = false;
  let fallbackTimer = 0;

  function frame() {
    return document.querySelector('.nitro-phone-frame');
  }

  function visible(node) {
    if (!node) return false;
    const style = getComputedStyle(node);
    return style.display !== 'none' && style.visibility !== 'hidden' && Number(style.opacity || 1) > 0 && node.getClientRects().length > 0;
  }

  function removeLegacyApp(root) {
    root?.querySelectorAll('.paradise-phone-notification-app').forEach(node => node.remove());
    root?.querySelectorAll('[data-pnc-launcher]:not([data-pnc-status-bell])').forEach(node => node.remove());
  }

  function ensureNow() {
    const root = frame();
    if (!root || !visible(root)) return false;

    removeLegacyApp(root);

    let bell = root.querySelector('[data-pnc-status-bell]');
    if (bell) return true;

    bell = document.createElement('button');
    bell.type = 'button';
    bell.className = 'pnc-status-bell';
    bell.dataset.pncLauncher = '1';
    bell.dataset.pncStatusBell = '1';
    bell.setAttribute('aria-label', 'Notifications');
    bell.setAttribute('title', 'Notifications');
    bell.innerHTML = '<span class="pnc-status-bell-glyph" aria-hidden="true"></span><span class="pnc-launch-badge" hidden></span>';
    root.appendChild(bell);
    return true;
  }

  function scheduleEnsure() {
    if (scheduled) return;
    scheduled = true;
    requestAnimationFrame(() => {
      scheduled = false;
      ensureNow();
    });
  }

  function bootstrap() {
    observer = new MutationObserver(scheduleEnsure);
    observer.observe(document.documentElement, {
      childList: true,
      subtree: true,
      attributes: true,
      attributeFilter: ['class', 'style']
    });

    ensureNow();
    window.setTimeout(ensureNow, 0);
    window.setTimeout(ensureNow, 60);
    window.setTimeout(ensureNow, 140);
    window.setTimeout(ensureNow, 300);

    fallbackTimer = window.setInterval(ensureNow, 300);

    console.info('[ParadisePhone] notifications V3.1 — clochette status bar active');
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', bootstrap, { once: true });
  else bootstrap();
})();
