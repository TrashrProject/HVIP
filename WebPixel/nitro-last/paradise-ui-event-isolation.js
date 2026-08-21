(() => {
  'use strict';

  const VERSION = '1.0.0-stable-event-isolation';
  const HUD_ID = 'paradise-rp-hud';

  function bind(root) {
    if (!root || root.dataset.prEventIsolation === '1') return;
    root.dataset.prEventIsolation = '1';

    // Let Paradise's own root.onclick handler run, but stop the same click from
    // bubbling into Nitro/document-level handlers afterwards.
    root.addEventListener('click', event => {
      const interactive = event.target?.closest?.('button, a, input, textarea, select, [data-pr4-action], [data-pr4-item], [data-pr4-cat], [data-pr4-command], [data-pr4-command-cat]');
      if (!interactive) return;
      event.preventDefault();
      event.stopPropagation();
    }, false);

    root.addEventListener('pointerdown', event => {
      const interactive = event.target?.closest?.('button, a, input, textarea, select, [data-pr4-action], [data-pr4-item], [data-pr4-cat], [data-pr4-command], [data-pr4-command-cat]');
      if (!interactive) return;
      // Do not cancel the pointer event: Paradise buttons still need it.
      // Only prevent it from reaching Nitro's document-level pointer handlers.
      event.stopPropagation();
    }, false);
  }

  function scan() {
    bind(document.getElementById(HUD_ID));
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', scan, { once: true });
  else scan();

  const observer = new MutationObserver(scan);
  observer.observe(document.documentElement, { childList: true, subtree: true });

  window.__ParadiseUIEventIsolation = { version: VERSION, scan };
})();
