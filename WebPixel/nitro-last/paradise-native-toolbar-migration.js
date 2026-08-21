(() => {
  'use strict';

  if (window.ParadiseNativeToolbarMigration) return;

  const VERSION = '1.0.0-phase11';
  const DEBUG = /(?:^|[?&])prdebug=1(?:&|$)/.test(location.search) || localStorage.getItem('pr_nitro_debug') === '1';
  const KNOWN = {
    CombatMode: { label: 'Combat', mode: 'proxy' },
    PSVMode: { label: 'PSV', mode: 'proxy' },
    TicketMode: { label: 'Tickets', mode: 'proxy' },
    PhoneMode: { label: 'Téléphone', mode: 'replaced', paradise: 'phone' },
    InventoryMode: { label: 'Inventaire', mode: 'replaced', paradise: 'inventory' },
    RoomInfoMode: { label: 'Infos appartement', mode: 'proxy' },
    MessengerMode: { label: 'Messenger Nitro', mode: 'proxy' },
    HelpMode: { label: 'Aide Nitro', mode: 'proxy' }
  };

  const audit = [];
  const migrated = new WeakSet();
  let observer = null;
  let scheduled = false;
  let destroyed = false;

  const clean = value => String(value ?? '').replace(/\s+/g, ' ').trim();

  function describe(element) {
    const id = clean(element.id);
    const known = KNOWN[id] || null;
    const title = clean(element.getAttribute?.('title'));
    const aria = clean(element.getAttribute?.('aria-label'));
    const text = clean(element.textContent);
    const label = known?.label || aria || title || text || id || 'Outil Nitro';
    const haystack = `${id} ${title} ${aria} ${text}`.toLowerCase();

    if (known) return { id, label, mode: known.mode, paradise: known.paradise || null };
    if (/invent|bag|sac/.test(haystack)) return { id, label: label || 'Inventaire', mode: 'replaced', paradise: 'inventory' };
    if (/phone|tel[eé]phone/.test(haystack)) return { id, label: label || 'Téléphone', mode: 'replaced', paradise: 'phone' };

    // Everything else is preserved through a click proxy instead of deleting logic.
    return { id, label, mode: 'proxy', paradise: null };
  }

  function proxyHost() {
    return document.querySelector('#paradise-rp-hud .pr-actions-menu');
  }

  function proxyKey(element, info) {
    return clean(info.id || element.getAttribute?.('data-action') || element.getAttribute?.('aria-label') || info.label)
      .toLowerCase()
      .replace(/[^a-z0-9_-]+/g, '-');
  }

  function ensureProxy(element, info) {
    const host = proxyHost();
    if (!host) return false;
    const key = proxyKey(element, info) || `legacy-${audit.length + 1}`;
    if (host.querySelector(`[data-pr-legacy-key="${CSS.escape(key)}"]`)) return true;

    const button = document.createElement('button');
    button.type = 'button';
    button.dataset.prLegacyProxy = '1';
    button.dataset.prLegacyKey = key;
    button.title = `${info.label} · fonction Nitro conservée`;
    button.innerHTML = `<span class="pr-legacy-dot" aria-hidden="true"></span><span></span>`;
    button.querySelector('span:last-child').textContent = info.label;
    button.addEventListener('click', event => {
      event.preventDefault();
      try {
        element.click();
      } catch (error) {
        console.warn('[ParadiseRP] legacy Nitro action failed', info.label, error);
      }
      window.ParadiseStore?.setUi?.({ actionsOpen: false });
    });
    host.appendChild(button);
    return true;
  }

  function migrateElement(element, source = 'known-selector') {
    if (!element || migrated.has(element) || element.closest?.('#paradise-ui-root')) return false;
    const info = describe(element);
    const preserved = info.mode === 'replaced' ? true : ensureProxy(element, info);
    if (!preserved) return false;

    migrated.add(element);
    element.dataset.prNativeMigrated = '1';
    audit.push({
      id: info.id || null,
      label: info.label,
      function: info.mode === 'replaced' ? `remplacée par Paradise ${info.paradise}` : 'fonction Nitro conservée via proxy',
      source,
      preserved: true
    });
    return true;
  }

  function knownButtons(root) {
    if (!root?.querySelectorAll) return [];
    const selectors = [
      '#CombatMode','#PSVMode','#TicketMode','#PhoneMode','#InventoryMode','#RoomInfoMode','#MessengerMode','#HelpMode',
      '[class*="menuButton-yNbz6"]'
    ];
    return [...root.querySelectorAll(selectors.join(','))];
  }

  function looksLikeLeftToolbar(element) {
    if (!element || element.closest?.('#paradise-ui-root')) return false;
    const style = getComputedStyle(element);
    if (!['fixed', 'absolute'].includes(style.position)) return false;
    const rect = element.getBoundingClientRect();
    if (rect.width < 18 || rect.width > 78 || rect.height < 70 || rect.height > innerHeight * .85) return false;
    if (rect.left > 12 || rect.right > 90) return false;
    const buttons = element.querySelectorAll('button,[role="button"]');
    return buttons.length >= 2 && buttons.length <= 12;
  }

  function migrateWrapper(wrapper) {
    const buttons = [...wrapper.querySelectorAll('button,[role="button"]')]
      .filter(button => !button.closest?.('#paradise-ui-root'));
    if (!buttons.length) return false;

    let handled = 0;
    buttons.forEach(button => {
      if (migrated.has(button) || migrateElement(button, 'left-toolbar-audit')) handled++;
    });

    if (handled === buttons.length) {
      wrapper.dataset.prNativeToolbarMigrated = '1';
      return true;
    }
    return false;
  }

  function findWrappers(root) {
    if (!root?.querySelectorAll) return [];
    const candidates = root.querySelectorAll([
      'nav', 'aside', '[role="navigation"]',
      '[class*="toolbar"]', '[class*="Toolbar"]',
      '[class*="side-menu"]', '[class*="sideMenu"]',
      '[class*="menu-container"]', '[class*="menuContainer"]'
    ].join(','));
    return [...candidates].filter(looksLikeLeftToolbar);
  }

  function scan() {
    scheduled = false;
    if (destroyed) return;
    const root = document.getElementById('root');
    if (!root || !proxyHost()) return;

    knownButtons(root).forEach(button => migrateElement(button));
    findWrappers(root).forEach(migrateWrapper);

    window.__ParadiseNativeToolbarAudit = audit.slice();
    if (DEBUG && audit.length) console.table(audit);
  }

  function scheduleScan() {
    if (scheduled || destroyed) return;
    scheduled = true;
    requestAnimationFrame(scan);
  }

  function boot() {
    scheduleScan();
    const root = document.getElementById('root');
    if (!root) return;
    observer = new MutationObserver(scheduleScan);
    observer.observe(root, { childList: true, subtree: true });
  }

  function destroy() {
    destroyed = true;
    observer?.disconnect();
    observer = null;
  }

  window.ParadiseNativeToolbarMigration = Object.freeze({
    version: VERSION,
    scan,
    getAudit: () => audit.slice(),
    destroy
  });

  window.addEventListener('beforeunload', destroy, { once: true });
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();