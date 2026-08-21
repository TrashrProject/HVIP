(() => {
  'use strict';

  if (window.__ParadiseUiFoundationBooted) return;
  window.__ParadiseUiFoundationBooted = '1.0.0';

  const ROOT_ID = 'paradise-ui-root';
  const HUD_ID = 'paradise-rp-hud';
  const WINDOW_NAMES = new Set(['profile', 'inventory', 'documents', 'vehicles', 'phone']);

  const icons = {
    user: '<path d="M12 12a4 4 0 1 0 0-8 4 4 0 0 0 0 8Zm-7 8c.8-4 3.2-6 7-6s6.2 2 7 6"/>',
    bag: '<path d="M6 8h12l1 12H5L6 8Zm3 0V6a3 3 0 0 1 6 0v2"/>',
    document: '<path d="M7 3h7l4 4v14H7V3Zm7 0v5h4M10 12h5M10 16h5"/>',
    car: '<path d="m5 14 2-5h10l2 5v5h-2v-2H7v2H5v-5Zm3-1h8M8 15h.01M16 15h.01"/>',
    phone: '<rect x="7" y="2.5" width="10" height="19" rx="2"/><path d="M10 5h4M11 18.5h2"/>',
    actions: '<path d="M6 4v16M12 4v16M18 4v16M3 9h6M9 15h6M15 8h6"/>',
    cash: '<path d="M4 7h16v10H4V7Zm3 3h.01M17 14h.01M12 14a2 2 0 1 0 0-4 2 2 0 0 0 0 4Z"/>',
    bank: '<path d="m4 9 8-5 8 5M5 9h14M7 9v8M11 9v8M15 9v8M19 17H5M4 20h16"/>',
    briefcase: '<rect x="4" y="7" width="16" height="12" rx="2"/><path d="M9 7V5h6v2M4 12h16M10 12v2h4v-2"/>',
    clock: '<circle cx="12" cy="12" r="8"/><path d="M12 7v5l3 2"/>',
    bell: '<path d="M6 16h12l-1.5-2V10a4.5 4.5 0 0 0-9 0v4L6 16Zm4 3h4"/>',
    heart: '<path d="M20.8 5.4a5.2 5.2 0 0 0-7.4 0L12 6.8l-1.4-1.4a5.2 5.2 0 0 0-7.4 7.4L12 21l8.8-8.2a5.2 5.2 0 0 0 0-7.4Z"/>',
    shield: '<path d="M12 3 20 6v6c0 4.5-3 7.2-8 9-5-1.8-8-4.5-8-9V6l8-3Z"/>',
    pin: '<path d="M12 21s6-5.5 6-11a6 6 0 1 0-12 0c0 5.5 6 11 6 11Z"/><circle cx="12" cy="10" r="2"/>',
    close: '<path d="m7 7 10 10M17 7 7 17"/>'
  };

  const icon = (name, className = '') => `<span class="pr-icon ${className}" aria-hidden="true"><svg viewBox="0 0 24 24">${icons[name] || icons.user}</svg></span>`;
  const text = value => value === null || value === undefined || String(value).trim() === '' ? null : String(value).trim();
  const number = value => Number.isFinite(Number(value)) ? Number(value) : null;
  const money = value => {
    const n = number(value);
    return n === null ? '— $' : `${new Intl.NumberFormat('fr-FR').format(Math.round(n))} $`;
  };
  const esc = value => String(value ?? '').replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;').replaceAll('"', '&quot;').replaceAll("'", '&#039;');

  function avatarUrl(player) {
    const direct = text(player?.avatarUrl);
    if (direct) return direct;
    const look = text(player?.look);
    if (!look || !/^[a-z0-9.\-]+$/i.test(look)) return null;
    return `../avatar-image.php?figure=${encodeURIComponent(look)}&direction=2&head_direction=3&gesture=sml&action=std&size=l&foundation=1`;
  }

  function statRatio(stat) {
    const current = number(stat?.current);
    const max = number(stat?.max);
    if (current === null || max === null || max <= 0) return null;
    return Math.max(0, Math.min(100, (current / max) * 100));
  }

  function shell() {
    return `
      <div id="${HUD_ID}" class="pr-foundation" data-pr-ui-foundation="1">
        <section class="pr-player-stack" aria-label="Informations joueur">
          <article class="pr-surface pr-player-card">
            <div class="pr-avatar-wrap">
              <div class="pr-avatar-stage" data-bind="avatar-stage"><span class="pr-avatar-fallback">RP</span></div>
              <span class="pr-online-dot" title="Session active"></span>
            </div>
            <div class="pr-player-main">
              <div class="pr-player-heading">
                <div>
                  <strong data-bind="username">Joueur</strong>
                  <span class="pr-role-chip" data-bind="role">Citoyen</span>
                </div>
              </div>
              <span class="pr-job-line">${icon('briefcase')}<span data-bind="job">Sans emploi</span></span>
              <div class="pr-stat pr-stat-health" data-stat="health">
                <div class="pr-stat-top"><span>${icon('heart')}Santé</span><b data-bind="health-value">— / —</b></div>
                <span class="pr-meter"><i data-bind="health-bar"></i></span>
              </div>
              <div class="pr-stat pr-stat-armor" data-stat="armor">
                <div class="pr-stat-top"><span>${icon('shield')}Armure</span><b data-bind="armor-value">— / —</b></div>
                <span class="pr-meter"><i data-bind="armor-bar"></i></span>
              </div>
            </div>
          </article>
          <div class="pr-surface pr-room-chip">
            ${icon('pin')}
            <span><strong data-bind="room-name">Localisation indisponible</strong><small data-bind="room-meta"></small></span>
          </div>
        </section>

        <section class="pr-surface pr-economy" aria-label="Économie et statut">
          <div class="pr-economy-item pr-economy-cash" title="Argent liquide">${icon('cash')}<strong data-bind="cash">— $</strong></div>
          <div class="pr-separator"></div>
          <div class="pr-economy-item pr-economy-bank" title="Banque">${icon('bank')}<strong data-bind="bank">— $</strong></div>
          <div class="pr-separator"></div>
          <div class="pr-economy-item pr-economy-job" title="Métier / statut">${icon('briefcase')}<span><strong data-bind="status">Sans emploi</strong><small data-bind="role-small"></small></span></div>
          <div class="pr-separator"></div>
          <div class="pr-economy-item pr-economy-time" title="Heure locale">${icon('clock')}<strong data-bind="time">--:--</strong></div>
          <button class="pr-icon-button pr-notification-button" type="button" data-action="notifications" title="Notifications" aria-label="Notifications">
            ${icon('bell')}<span class="pr-badge" data-bind="notification-badge" hidden>0</span>
          </button>
        </section>

        <nav class="pr-surface pr-bottom-nav" aria-label="Navigation ParadiseRP">
          <button type="button" class="pr-avatar-button" data-window-open="profile" title="Profil">
            <span data-bind="mini-avatar"><span class="pr-mini-avatar-fallback">RP</span></span>
          </button>
          <button type="button" class="pr-nav-button" data-window-open="inventory" title="Inventaire">${icon('bag')}<span>Inventaire</span></button>
          <button type="button" class="pr-nav-button" data-window-open="documents" title="Documents">${icon('document')}<span>Documents</span></button>
          <button type="button" class="pr-nav-button" data-window-open="vehicles" title="Véhicules">${icon('car')}<span>Véhicules</span></button>
        </nav>

        <div class="pr-surface pr-chatbar" aria-label="Chat ParadiseRP">
          <button type="button" class="pr-chat-scope" title="Canal local">LOCAL</button>
          <div class="pr-chat-divider"></div>
          <input id="pr4-chat-input" type="text" autocomplete="off" spellcheck="false" maxlength="250" placeholder="Écrire un message..." aria-label="Chat ParadiseRP">
          <span class="pr-chat-hint">Entrée pour envoyer</span>
        </div>

        <div class="pr-bottom-right">
          <button type="button" class="pr-surface pr-corner-button" data-window-open="phone" title="Téléphone">${icon('phone')}<span>Téléphone</span></button>
          <div class="pr-actions-wrap">
            <button type="button" class="pr-surface pr-corner-button" data-action="toggle-actions" aria-expanded="false" title="Actions">${icon('actions')}<span>Actions</span></button>
            <div class="pr-surface pr-actions-menu" data-bind="actions-menu" hidden>
              <button type="button" data-window-open="profile">${icon('user')}<span>Profil</span></button>
              <button type="button" data-window-open="inventory">${icon('bag')}<span>Inventaire</span></button>
              <button type="button" data-window-open="documents">${icon('document')}<span>Documents</span></button>
              <button type="button" data-window-open="vehicles">${icon('car')}<span>Véhicules</span></button>
            </div>
          </div>
        </div>

        <div class="pr4-window-layer pr-window-layer" data-bind="window-layer" aria-live="polite">
          ${windowMarkup('profile', 'user', 'Mon profil', profileBody())}
          ${windowMarkup('inventory', 'bag', 'Inventaire', emptyState('bag', 'Inventaire prêt', 'Aucun objet RP n’est chargé dans cette fondation.'))}
          ${windowMarkup('documents', 'document', 'Documents', emptyState('document', 'Aucun document chargé', 'Identité, permis et licences seront branchés sur les vraies données lors de leur module dédié.'))}
          ${windowMarkup('vehicles', 'car', 'Véhicules', emptyState('car', 'Aucun véhicule chargé', 'Le garage sera connecté au système véhicule existant dans une phase dédiée.'))}
          ${windowMarkup('phone', 'phone', 'Téléphone', emptyState('phone', 'Téléphone', 'La fenêtre est prête. Les applications et la messagerie seront ajoutées dans la phase Téléphone.'))}
        </div>
      </div>`;
  }

  function windowMarkup(name, iconName, title, body) {
    return `<section class="pr-window" data-window="${name}" aria-hidden="true">
      <header class="pr-window-header">
        <div class="pr-window-title">${icon(iconName)}<div><strong>${esc(title)}</strong><small>ParadiseRP</small></div></div>
        <button type="button" class="pr-window-close" data-window-close="${name}" aria-label="Fermer" title="Fermer">${icon('close')}</button>
      </header>
      <div class="pr-window-body">${body}</div>
    </section>`;
  }

  function emptyState(iconName, title, description) {
    return `<div class="pr-empty-state">${icon(iconName, 'pr-empty-icon')}<strong>${esc(title)}</strong><p>${esc(description)}</p></div>`;
  }

  function profileBody() {
    return `<div class="pr-profile-grid">
      <div class="pr-profile-identity">
        <div class="pr-profile-avatar" data-bind="profile-avatar"><span class="pr-profile-avatar-fallback">RP</span></div>
        <div><strong data-bind="profile-username">Joueur</strong><span class="pr-role-chip" data-bind="profile-role">Citoyen</span><small data-bind="profile-job">Sans emploi</small></div>
      </div>
      <div class="pr-profile-details">
        <div><span>${icon('pin')}Localisation</span><strong data-bind="profile-room">Indisponible</strong></div>
        <div><span>${icon('heart')}Santé</span><strong data-bind="profile-health">— / —</strong></div>
        <div><span>${icon('shield')}Armure</span><strong data-bind="profile-armor">— / —</strong></div>
        <div><span>${icon('cash')}Argent</span><strong data-bind="profile-cash">— $</strong></div>
        <div><span>${icon('bank')}Banque</span><strong data-bind="profile-bank">— $</strong></div>
        <div><span>${icon('briefcase')}Métier</span><strong data-bind="profile-employment">Sans emploi</strong></div>
      </div>
    </div>`;
  }

  function mount() {
    const host = document.getElementById(ROOT_ID);
    if (!host || document.getElementById(HUD_ID)) return;

    host.innerHTML = shell();
    const root = document.getElementById(HUD_ID);
    if (!root) return;

    const bindings = new Map();
    root.querySelectorAll('[data-bind]').forEach(element => {
      const key = element.dataset.bind;
      if (!bindings.has(key)) bindings.set(key, []);
      bindings.get(key).push(element);
    });

    const all = key => bindings.get(key) || [];
    const setText = (key, value) => all(key).forEach(element => { element.textContent = value; });
    const setHtml = (key, value) => all(key).forEach(element => { element.innerHTML = value; });

    function updateAvatar(key, player, fallbackClass) {
      const url = avatarUrl(player);
      const username = text(player?.username) || 'Joueur';
      const html = url
        ? `<img src="${esc(url)}" alt="${esc(username)}" draggable="false">`
        : `<span class="${fallbackClass}">RP</span>`;
      setHtml(key, html);
    }

    function updateStat(kind, stat) {
      const current = number(stat?.current);
      const max = number(stat?.max);
      const value = current !== null && max !== null ? `${Math.round(current)} / ${Math.round(max)}` : '— / —';
      const ratio = statRatio(stat);
      setText(`${kind}-value`, value);
      all(`${kind}-bar`).forEach(bar => {
        bar.style.width = `${ratio ?? 0}%`;
        bar.dataset.empty = ratio === null ? '1' : '0';
      });
      setText(`profile-${kind}`, value);
      root.querySelectorAll(`[data-stat="${kind}"]`).forEach(element => {
        element.dataset.level = ratio === null ? 'unknown' : ratio <= 25 ? 'danger' : ratio <= 55 ? 'warn' : 'ok';
      });
    }

    function updateClock() {
      setText('time', new Intl.DateTimeFormat('fr-FR', { hour: '2-digit', minute: '2-digit' }).format(new Date()));
    }

    function updateGameplay() {
      const store = window.ParadiseStore?.getState?.();
      if (!store) return;
      const { player, economy, room, notifications } = store.gameplay;

      const username = text(player.username) || 'Joueur';
      const role = text(player.role) || 'Citoyen';
      const job = text(player.job) || 'Sans emploi';
      const roomName = text(room.name) || 'Localisation indisponible';
      const secondary = [text(room.district) || text(room.city), number(room.playerCount) !== null ? `${Math.round(number(room.playerCount))} joueur${Math.round(number(room.playerCount)) > 1 ? 's' : ''}` : null].filter(Boolean).join(' · ');

      setText('username', username);
      setText('role', role);
      setText('job', job);
      setText('room-name', roomName);
      setText('room-meta', secondary);
      setText('cash', money(economy.cash));
      setText('bank', money(economy.bank));
      setText('status', job);
      setText('role-small', role !== job ? role : '');

      updateAvatar('avatar-stage', player, 'pr-avatar-fallback');
      updateAvatar('mini-avatar', player, 'pr-mini-avatar-fallback');
      updateAvatar('profile-avatar', player, 'pr-profile-avatar-fallback');
      updateStat('health', player.health);
      updateStat('armor', player.armor);

      setText('profile-username', username);
      setText('profile-role', role);
      setText('profile-job', job);
      setText('profile-room', roomName);
      setText('profile-cash', money(economy.cash));
      setText('profile-bank', money(economy.bank));
      setText('profile-employment', job);

      const count = Math.max(0, Math.round(number(notifications.count) ?? 0));
      all('notification-badge').forEach(badge => {
        badge.textContent = String(count);
        badge.hidden = count <= 0;
      });
    }

    function syncUi() {
      const ui = window.ParadiseStore?.getState?.().ui;
      if (!ui) return;
      root.querySelectorAll('.pr-window').forEach(panel => {
        const open = panel.dataset.window === ui.activeWindow;
        panel.classList.toggle('is-open', open);
        panel.setAttribute('aria-hidden', open ? 'false' : 'true');
      });

      const menu = root.querySelector('[data-bind="actions-menu"]');
      const toggle = root.querySelector('[data-action="toggle-actions"]');
      if (menu) menu.hidden = !ui.actionsOpen;
      if (toggle) toggle.setAttribute('aria-expanded', ui.actionsOpen ? 'true' : 'false');
    }

    const openWindow = name => {
      if (!WINDOW_NAMES.has(name)) return false;
      window.ParadiseStore?.setUi?.({ activeWindow: name, actionsOpen: false });
      return true;
    };
    const closeWindow = name => {
      const current = window.ParadiseStore?.getState?.().ui.activeWindow;
      if (!name || current === name) window.ParadiseStore?.setUi?.({ activeWindow: null, actionsOpen: false });
      return true;
    };
    const toggleWindow = name => {
      if (!WINDOW_NAMES.has(name)) return false;
      const current = window.ParadiseStore?.getState?.().ui.activeWindow;
      return current === name ? closeWindow(name) : openWindow(name);
    };

    window.ParadiseWindowManager = Object.freeze({
      openWindow,
      closeWindow,
      toggleWindow,
      getActiveWindow: () => window.ParadiseStore?.getState?.().ui.activeWindow || null
    });

    function onClick(event) {
      const open = event.target.closest('[data-window-open]');
      if (open && root.contains(open)) {
        event.preventDefault();
        openWindow(open.dataset.windowOpen);
        return;
      }

      const close = event.target.closest('[data-window-close]');
      if (close && root.contains(close)) {
        event.preventDefault();
        closeWindow(close.dataset.windowClose);
        return;
      }

      const action = event.target.closest('[data-action]');
      if (!action || !root.contains(action)) return;

      if (action.dataset.action === 'toggle-actions') {
        event.preventDefault();
        const opened = Boolean(window.ParadiseStore?.getState?.().ui.actionsOpen);
        window.ParadiseStore?.setUi?.({ actionsOpen: !opened });
      } else if (action.dataset.action === 'notifications') {
        event.preventDefault();
        // No fake notification center in Phase 1: the top bell remains a truthful status indicator only.
      }
    }

    function onDocumentClick(event) {
      const ui = window.ParadiseStore?.getState?.().ui;
      if (!ui?.actionsOpen) return;
      if (event.target.closest('.pr-actions-wrap')) return;
      window.ParadiseStore.setUi({ actionsOpen: false });
    }

    function onKeyDown(event) {
      if (event.key !== 'Escape') return;
      const active = document.activeElement;
      if (active && (active.tagName === 'INPUT' || active.tagName === 'TEXTAREA')) return;
      if (window.ParadiseStore?.getState?.().ui.activeWindow) closeWindow();
      else if (window.ParadiseStore?.getState?.().ui.actionsOpen) window.ParadiseStore.setUi({ actionsOpen: false });
    }

    const unsubscribe = window.ParadiseStore?.subscribe?.((_state, eventName) => {
      if (eventName === 'gameplay:snapshot' || eventName === 'bridge:error') updateGameplay();
      if (eventName === 'ui:change') syncUi();
    }) || (() => {});

    const clockTimer = window.setInterval(updateClock, 30000);
    root.addEventListener('click', onClick, false);
    document.addEventListener('click', onDocumentClick, false);
    document.addEventListener('keydown', onKeyDown, false);

    updateGameplay();
    updateClock();
    syncUi();

    function destroy() {
      unsubscribe();
      window.clearInterval(clockTimer);
      root.removeEventListener('click', onClick, false);
      document.removeEventListener('click', onDocumentClick, false);
      document.removeEventListener('keydown', onKeyDown, false);
    }

    window.addEventListener('beforeunload', destroy, { once: true });
    window.__ParadiseRPUI = Object.freeze({
      version: '1.0.0-ui-foundation',
      open: openWindow,
      close: closeWindow,
      toggle: toggleWindow,
      refresh: () => window.ParadiseBridge?.refresh?.(),
      getData: () => window.ParadiseBridge?.getLastPayload?.() || null,
      getState: () => window.ParadiseStore?.getState?.() || null,
      destroy
    });
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', mount, { once: true });
  else mount();
})();
