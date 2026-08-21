(() => {
  'use strict';
  if (window.ParadiseQualityGate) return;

  const VERSION = '1.0.0-quality-gate';
  const HUD_ID = 'paradise-rp-hud';
  const EMPTY_TEXT = /^(Non renseigné|Non renseignée|Indisponible|Localisation inconnue|Localisation indisponible)$/i;
  let hud = null;
  let unsubscribe = () => {};
  let scheduled = false;
  let destroyed = false;
  const timers = new Set();

  const esc = value => String(value ?? '').replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('>','&gt;').replaceAll('"','&quot;').replaceAll("'",'&#039;');
  const text = value => value === null || value === undefined ? '' : String(value).trim();
  const state = () => window.ParadiseStore?.getState?.() || null;

  const pixelIcons = {
    messages: '<svg viewBox="0 0 24 24" aria-hidden="true" shape-rendering="crispEdges"><path fill="#fff" d="M4 5h16v11H9l-5 4V5Zm3 4h2v2H7V9Zm4 0h2v2h-2V9Zm4 0h2v2h-2V9Z"/></svg>',
    contacts: '<svg viewBox="0 0 24 24" aria-hidden="true" shape-rendering="crispEdges"><path fill="#3b8584" d="M5 3h14v18H5V3Zm3 3h8v2H8V6Zm4 4a3 3 0 1 1 0 6 3 3 0 0 1 0-6Zm-4 8c1-2 2.4-3 4-3s3 1 4 3H8Z"/><path fill="#77cfc5" d="M3 6h2v3H3V6Zm0 5h2v3H3v-3Zm0 5h2v3H3v-3Z"/></svg>',
    calls: '<svg viewBox="0 0 24 24" aria-hidden="true" shape-rendering="crispEdges"><path fill="#73591d" d="M7 3h4l2 5-3 2c1 2 2 3 4 4l2-3 5 2v4c0 2-2 4-4 4C10 20 4 14 3 7c0-2 2-4 4-4Z"/><path fill="#fff3c9" d="m8 5 1 3-2 1c2 4 4 6 8 8l1-2 3 1v1c0 1-1 2-2 2C11 18 6 13 5 7c0-1 1-2 2-2h1Z"/></svg>',
    notifications: '<svg viewBox="0 0 24 24" aria-hidden="true" shape-rendering="crispEdges"><path fill="#b68c38" d="M10 3h4v2c3 1 5 4 5 8v3h2v3H3v-3h2v-3c0-4 2-7 5-8V3Zm-2 13h8v-3c0-3-1-5-4-5s-4 2-4 5v3Zm2 4h4v2h-4v-2Z"/></svg>',
    settings: '<svg viewBox="0 0 24 24" aria-hidden="true" shape-rendering="crispEdges"><path fill="#4c7c7b" d="M9 2h6l1 3 3-1 3 5-2 2 2 2-3 5-3-1-1 3H9l-1-3-3 1-3-5 2-2-2-2 3-5 3 1 1-3Zm3 6a4 4 0 1 0 0 8 4 4 0 0 0 0-8Zm0 2a2 2 0 1 1 0 4 2 2 0 0 1 0-4Z"/></svg>',
    call: '<svg viewBox="0 0 24 24" aria-hidden="true" shape-rendering="crispEdges"><path fill="#73591d" d="M7 3h4l2 5-3 2c1 2 2 3 4 4l2-3 5 2v4c0 2-2 4-4 4C10 20 4 14 3 7c0-2 2-4 4-4Z"/></svg>',
    message: '<svg viewBox="0 0 24 24" aria-hidden="true" shape-rendering="crispEdges"><path fill="#3b8584" d="M4 5h16v11H9l-5 4V5Zm3 4h10v2H7V9Zm0 4h7v2H7v-2Z"/></svg>'
  };

  function schedule(fn = enhanceAll, delay = 0) {
    if (destroyed) return;
    if (delay > 0) {
      const timer = setTimeout(() => { timers.delete(timer); schedule(fn); }, delay);
      timers.add(timer);
      return;
    }
    if (scheduled) return;
    scheduled = true;
    requestAnimationFrame(() => {
      scheduled = false;
      if (!destroyed) fn();
    });
  }

  function roomLabel(room) {
    const name = text(room?.name);
    return name || 'Synchronisation de la room…';
  }

  function syncRoomUi() {
    const store = state();
    if (!store || !hud) return;
    const room = store.gameplay?.room || {};
    const label = roomLabel(room);
    const meta = [text(room.district) || text(room.city), Number.isFinite(Number(room.playerCount)) ? `${Math.round(Number(room.playerCount))} joueur${Number(room.playerCount) > 1 ? 's' : ''}` : ''].filter(Boolean).join(' · ');

    hud.querySelectorAll('[data-bind="room-name"]').forEach(node => { node.textContent = label; });
    hud.querySelectorAll('[data-bind="room-meta"]').forEach(node => { node.textContent = meta; });
    hud.querySelectorAll('[data-bind="profile-room"]').forEach(node => { node.textContent = label; });

    const chip = hud.querySelector('.pr-room-chip');
    if (chip) {
      chip.classList.toggle('is-live', Boolean(text(room.name)));
      chip.classList.toggle('is-resolving', !text(room.name));
      chip.classList.remove('is-connecting');
    }

    hud.querySelectorAll('.pr2-info').forEach(info => {
      const key = text(info.querySelector('span')?.textContent).toLowerCase();
      if (key === 'localisation') {
        const value = info.querySelector('strong');
        if (value) value.textContent = label;
        info.dataset.prqInfo = 'location';
      }
    });
  }

  function enhanceOverview() {
    if (!hud) return;
    hud.querySelectorAll('.pr2-stat-card').forEach((card, index) => {
      card.dataset.prqGameplayStat = index === 0 ? 'health' : 'armor';
    });
    hud.querySelectorAll('.pr2-info').forEach(info => {
      const label = text(info.querySelector('span')?.textContent).toLowerCase();
      if (label.includes('métier') || label.includes('metier')) info.dataset.prqInfo = 'job';
      else if (label.includes('localisation')) info.dataset.prqInfo = 'location';
      else if (label.includes('argent')) info.dataset.prqInfo = 'cash';
      else if (label.includes('banque')) info.dataset.prqInfo = 'bank';
    });
    syncRoomUi();
  }

  function enhanceIdentity() {
    const panel = hud?.querySelector('[data-pr2-panel="identity"]');
    const dossier = panel?.querySelector('.pr2-dossier');
    if (!dossier || dossier.dataset.prqIdentity === '1') return;
    dossier.dataset.prqIdentity = '1';

    const grid = dossier.querySelector('.pr2-dossier-grid');
    const head = dossier.querySelector('.pr2-dossier-head');
    if (!grid || !head) return;

    const fields = [...grid.children].filter(node => node.classList?.contains('pr2-field'));
    let emptyCount = 0;
    let citizenId = '';
    fields.forEach(field => {
      const label = text(field.querySelector('span')?.textContent);
      const strong = field.querySelector('strong');
      const value = text(strong?.textContent);
      if (/citizen id/i.test(label)) {
        citizenId = value;
        field.dataset.prqCitizenId = '1';
      }
      if (EMPTY_TEXT.test(value)) {
        emptyCount++;
        field.classList.add('is-empty');
        if (strong) strong.textContent = 'À compléter';
      }
    });

    const status = document.createElement('div');
    status.className = 'prq-dossier-status';
    status.innerHTML = emptyCount > 0
      ? `<strong>Dossier citoyen incomplet</strong><span>${emptyCount} information${emptyCount > 1 ? 's' : ''} à compléter${citizenId && !EMPTY_TEXT.test(citizenId) ? ` · ${esc(citizenId)}` : ''}</span>`
      : `<strong>Dossier citoyen vérifié</strong><span>${citizenId && !EMPTY_TEXT.test(citizenId) ? esc(citizenId) : 'Identité persistante ParadiseRP'}</span>`;
    head.insertAdjacentElement('afterend', status);

    const civilLabels = /^(Prénom|Nom|Date de naissance|Âge calculé|Nationalité \/ origine|Genre RP)$/i;
    const civil = document.createElement('section');
    const admin = document.createElement('section');
    civil.className = admin.className = 'prq-dossier-section';
    civil.innerHTML = '<h3>Identité civile</h3><div class="prq-dossier-section-grid"></div>';
    admin.innerHTML = '<h3>Administration de Placid Island</h3><div class="prq-dossier-section-grid"></div>';
    const civilGrid = civil.querySelector('.prq-dossier-section-grid');
    const adminGrid = admin.querySelector('.prq-dossier-section-grid');
    fields.forEach(field => {
      const label = text(field.querySelector('span')?.textContent);
      (civilLabels.test(label) ? civilGrid : adminGrid).appendChild(field);
    });
    grid.replaceWith(civil, admin);
  }

  function enhanceDocuments() {
    const panel = hud?.querySelector('[data-pr2-panel="documents"]');
    if (!panel) return;
    panel.querySelectorAll('.pr2-doc-tile').forEach(tile => {
      const name = tile.querySelector('strong');
      if (name) name.title = text(name.textContent);
    });
    panel.querySelectorAll('.pr2-doc-empty').forEach(empty => {
      const title = empty.querySelector('strong');
      const copy = empty.querySelector('p');
      if (title && /aucun document sélectionné/i.test(title.textContent || '')) title.textContent = 'Votre portefeuille de documents';
      if (copy && /documents réellement délivrés/i.test(copy.textContent || '')) copy.textContent = 'Les documents officiels délivrés à votre personnage apparaîtront ici. ParadiseRP n’affiche aucun faux document.';
    });
  }

  function enhancePhoneIcons() {
    hud?.querySelectorAll('.pp-app-icon[data-icon]').forEach(node => {
      const key = text(node.dataset.icon).toLowerCase();
      const markup = pixelIcons[key] || pixelIcons[key === 'notification' ? 'notifications' : key === 'phone' ? 'calls' : ''];
      if (markup && node.dataset.prqIcon !== key) {
        node.innerHTML = markup;
        node.dataset.prqIcon = key;
      }
    });
  }

  function enhancePhoneHero() {
    const hero = hud?.querySelector('.pp-hero');
    if (!hero) return;
    const store = state();
    const name = text(store?.character?.fullName) || text(store?.gameplay?.player?.username);
    if (name && !hero.querySelector('.pp-owner')) {
      const owner = document.createElement('span');
      owner.className = 'pp-owner';
      owner.textContent = name;
      hero.appendChild(owner);
    }
  }

  function enhancePhoneEmptyStates() {
    hud?.querySelectorAll('.pp-empty').forEach(empty => {
      const title = empty.querySelector('strong');
      const copy = empty.querySelector('span');
      if (!title || !copy) return;
      if (/aucun contact/i.test(title.textContent || '')) copy.textContent = 'Partagez votre numéro avec les habitants de Placid Island puis ajoutez-les à votre carnet.';
      if (/aucune conversation/i.test(title.textContent || '')) copy.textContent = 'Vos conversations privées apparaîtront ici. Commencez avec « Nouveau message ».';
    });
  }

  function enhancePhone() {
    enhancePhoneIcons();
    enhancePhoneHero();
    enhancePhoneEmptyStates();
  }

  function auditOldToolbar() {
    const root = document.getElementById('root');
    if (!root) return;
    const buttons = [...root.querySelectorAll('#CombatMode,#PSVMode,#TicketMode,#PhoneMode,#InventoryMode,#RoomInfoMode,#MessengerMode,#HelpMode,[class*="menuButton-yNbz6"]')]
      .filter(node => !node.closest('#paradise-ui-root'));
    const candidates = new Set();
    buttons.forEach(button => {
      let node = button.parentElement;
      for (let depth = 0; node && depth < 5; depth++, node = node.parentElement) {
        if (node.id === 'root') break;
        const rect = node.getBoundingClientRect?.();
        if (!rect) continue;
        const buttonCount = node.querySelectorAll('button,[role="button"],#CombatMode,#PSVMode,#TicketMode,#PhoneMode,#InventoryMode,#RoomInfoMode,#MessengerMode,#HelpMode').length;
        if (rect.left <= 12 && rect.width >= 20 && rect.width <= 82 && rect.height >= 60 && rect.height <= 320 && buttonCount >= 2 && buttonCount <= 14) candidates.add(node);
      }
    });
    candidates.forEach(wrapper => { wrapper.dataset.prqOldToolbar = '1'; });
  }

  function enhanceAll() {
    hud = document.getElementById(HUD_ID);
    if (!hud) return;
    enhanceOverview();
    enhanceIdentity();
    enhanceDocuments();
    enhancePhone();
    auditOldToolbar();
  }

  function onStoreChange(store, eventName) {
    if (destroyed) return;
    if (eventName === 'room:change' || eventName === 'gameplay:snapshot') syncRoomUi();
    if (['room:change','gameplay:snapshot','character:update','documents:update','ui:change'].includes(eventName)) schedule(enhanceAll);
  }

  function onPhoneEvent() {
    schedule(enhancePhone);
    schedule(enhancePhone, 60);
  }

  function onClick(event) {
    if (destroyed) return;
    const target = event.target;
    if (!(target instanceof Element)) return;
    if (target.closest('#paradise-ui-root')) {
      schedule(enhanceAll);
      schedule(enhanceAll, 80);
    } else {
      window.ParadiseRoomAdapter?.rescan?.('quality-gate-click');
      schedule(auditOldToolbar, 250);
    }
  }

  function onEscape(event) {
    if (event.key !== 'Escape') return;
    const activeWindow = state()?.ui?.activeWindow;
    if (activeWindow !== 'phone') return;
    const phone = hud?.querySelector('.pp-device');
    if (!phone) return;
    const pageBack = phone.querySelector('.pp-chat .pp-back,.pp-app-page .pp-back,.pp-call .pp-back');
    event.preventDefault();
    event.stopImmediatePropagation();
    if (pageBack) pageBack.click();
    else window.ParadiseWindowManager?.closeWindow?.('phone');
  }

  function boot() {
    hud = document.getElementById(HUD_ID);
    if (!hud || !window.ParadiseStore) {
      schedule(boot, 80);
      return;
    }
    unsubscribe = window.ParadiseStore.subscribe(onStoreChange);
    window.addEventListener('paradise:phone', onPhoneEvent, false);
    document.addEventListener('click', onClick, true);
    window.addEventListener('keydown', onEscape, true);
    enhanceAll();
    [120, 420, 1100, 2600].forEach(delay => schedule(enhanceAll, delay));
    window.ParadiseRoomAdapter?.rescan?.('quality-gate-boot');
    console.info('[ParadiseRP] strict visual quality gate active', { version: VERSION });
  }

  function destroy() {
    destroyed = true;
    unsubscribe();
    timers.forEach(timer => clearTimeout(timer));
    timers.clear();
    window.removeEventListener('paradise:phone', onPhoneEvent, false);
    document.removeEventListener('click', onClick, true);
    window.removeEventListener('keydown', onEscape, true);
  }

  window.ParadiseQualityGate = Object.freeze({
    version: VERSION,
    refresh: enhanceAll,
    syncRoom: syncRoomUi,
    auditToolbar: auditOldToolbar,
    destroy
  });

  window.addEventListener('beforeunload', destroy, { once: true });
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();