(() => {
  'use strict';

  if (window.__PARADISE_PHONE_NOTIFICATION_STATUS_V4__) return;
  window.__PARADISE_PHONE_NOTIFICATION_STATUS_V4__ = '4.0.0';

  let observer = null;
  let scheduled = false;
  let timer = 0;

  function frame() {
    return document.querySelector('.nitro-phone-frame');
  }

  function visible(node) {
    if (!node) return false;
    const style = getComputedStyle(node);
    return style.display !== 'none' && style.visibility !== 'hidden' && Number(style.opacity || 1) > 0 && node.getClientRects().length > 0;
  }

  function removeLegacyLauncher(root) {
    root?.querySelectorAll('.paradise-phone-notification-app').forEach(node => node.remove());
  }

  function ensureBell() {
    const root = frame();
    if (!root || !visible(root)) return null;

    removeLegacyLauncher(root);

    let bell = root.querySelector('[data-pnc-status-bell]');
    if (bell) return bell;

    // Remove any legacy launcher that could make the old notification-center code
    // believe the home-screen application still exists.
    root.querySelectorAll('[data-pnc-launcher]').forEach(node => {
      if (!node.matches('[data-pnc-status-bell]')) node.remove();
    });

    bell = document.createElement('button');
    bell.type = 'button';
    bell.className = 'pnc-status-bell';
    bell.dataset.pncLauncher = '1';
    bell.dataset.pncStatusBell = '1';
    bell.setAttribute('aria-label', 'Notifications');
    bell.setAttribute('title', 'Notifications');
    bell.innerHTML = '<span class="pnc-status-bell-glyph" aria-hidden="true"></span><span class="pnc-launch-badge" hidden></span>';
    root.appendChild(bell);
    return bell;
  }

  function schedule() {
    if (scheduled) return;
    scheduled = true;
    requestAnimationFrame(() => {
      scheduled = false;
      ensureBell();
    });
  }

  function bootstrap() {
    observer = new MutationObserver(schedule);
    observer.observe(document.documentElement, {
      childList: true,
      subtree: true,
      attributes: true,
      attributeFilter: ['class', 'style']
    });

    ensureBell();
    window.setTimeout(ensureBell, 0);
    window.setTimeout(ensureBell, 60);
    window.setTimeout(ensureBell, 140);
    window.setTimeout(ensureBell, 300);

    clearInterval(timer);
    timer = window.setInterval(ensureBell, 350);

    console.info('[ParadisePhone] notifications V4 — clochette status bar active');
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', bootstrap, { once: true });
  else bootstrap();
})();
