(() => {
  'use strict';

  if (window.__PARADISE_PHONE_NOTIFICATION_FAST_V3__) return;
  window.__PARADISE_PHONE_NOTIFICATION_FAST_V3__ = '3.0.0';

  let observer = null;
  let burstTimer = 0;
  let fallbackTimer = 0;
  let scheduled = false;

  function frame() {
    return document.querySelector('.nitro-phone-frame');
  }

  function visible(node) {
    if (!node) return false;
    const style = getComputedStyle(node);
    return style.display !== 'none' && style.visibility !== 'hidden' && Number(style.opacity || 1) > 0 && node.getClientRects().length > 0;
  }

  function candidateLabel(node) {
    return [
      node.getAttribute?.('aria-label'),
      node.getAttribute?.('title'),
      node.getAttribute?.('data-app'),
      node.getAttribute?.('data-app-name'),
      node.textContent
    ].filter(Boolean).join(' ').replace(/\s+/g, ' ').trim().toLocaleLowerCase('fr-FR');
  }

  function nativeTemplate(root) {
    const preferred = [...root.querySelectorAll('button,[role="button"],a')]
      .filter(node => !node.closest('.phone-active-app,.paradise-phone-notification-center,.paradise-phone-notification-host'))
      .map(node => ({ node, label: candidateLabel(node) }))
      .find(entry => /paradise\s*gram|contacts|amis|friends/.test(entry.label));
    return preferred?.node || root.querySelector('.phone-app-icon,button[aria-label],button');
  }

  function launcherHost(template) {
    if (!template) return null;
    let parent = template.parentElement;
    for (let depth = 0; parent && depth < 5; depth += 1, parent = parent.parentElement) {
      const children = [...parent.children];
      const appLike = children.filter(child =>
        child.matches?.('.phone-app-icon,button,[role="button"],a') ||
        child.querySelector?.('.phone-app-icon')
      );
      if (appLike.length >= 3 && appLike.length <= 20) return parent;
    }
    return template.parentElement;
  }

  function buildLauncher(template) {
    const button = document.createElement('button');
    button.type = 'button';
    button.className = template?.matches?.('button')
      ? `${template.className || ''} paradise-phone-notification-app`
      : 'phone-app-icon paradise-phone-notification-app';
    button.removeAttribute('id');
    button.setAttribute('aria-label', 'Notifications');
    button.dataset.pncLauncher = '1';
    button.dataset.pncFast = '1';
    button.innerHTML = '<span class="pnc-launch-icon" aria-hidden="true"></span><span class="pnc-launch-label">Notifications</span><span class="pnc-launch-badge" hidden></span>';
    return button;
  }

  function ensureNow() {
    const root = frame();
    if (!root || !visible(root)) return false;

    const existing = root.querySelector('[data-pnc-launcher]');
    if (existing) return true;

    // Never inject on top of an opened application.
    if (root.querySelector('.phone-active-app')) return false;

    const template = nativeTemplate(root);
    if (!template) return false;
    const host = launcherHost(template);
    if (!host) return false;

    host.appendChild(buildLauncher(template));
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

  function startBurst() {
    clearInterval(burstTimer);
    const started = performance.now();
    ensureNow();
    burstTimer = window.setInterval(() => {
      const done = ensureNow();
      if (done || performance.now() - started > 1800) {
        clearInterval(burstTimer);
        burstTimer = 0;
      }
    }, 60);
  }

  function bootstrap() {
    observer = new MutationObserver(() => {
      scheduleEnsure();
      const root = frame();
      if (root && visible(root) && !root.querySelector('[data-pnc-launcher]') && !root.querySelector('.phone-active-app')) {
        startBurst();
      }
    });
    observer.observe(document.documentElement, { childList: true, subtree: true, attributes: true, attributeFilter: ['class', 'style'] });

    document.addEventListener('click', () => window.setTimeout(startBurst, 0), true);
    startBurst();

    // Lightweight safety net: keeps the launcher instant after React rebuilds the phone home.
    fallbackTimer = window.setInterval(() => {
      const root = frame();
      if (root && visible(root) && !root.querySelector('.phone-active-app')) ensureNow();
    }, 250);

    console.info('[ParadisePhone] notification launcher V3 instant actif');
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', bootstrap, { once: true });
  else bootstrap();
})();
