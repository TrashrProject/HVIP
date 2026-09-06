(() => {
  'use strict';

  if (window.__PARADISE_PHONE_NOTIFICATION_CENTER_V2__) return;
  window.__PARADISE_PHONE_NOTIFICATION_CENTER_V2__ = '2.0.0';

  const API = '/nitro/phone-notifications-api.php';
  const POLL_MS = 5000;
  const MAX_BADGE = 99;

  const state = {
    userId: 0,
    serverTime: 0,
    items: [],
    filter: 'all',
    open: false,
    busy: false,
    timer: 0,
    observer: null,
    observerFrame: 0
  };

  const escapeHtml = value => String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;');

  function frame() {
    return document.querySelector('.nitro-phone-frame');
  }

  function visible(node) {
    if (!node) return false;
    const style = getComputedStyle(node);
    return style.display !== 'none' && style.visibility !== 'hidden' && Number(style.opacity || 1) > 0 && node.getClientRects().length > 0;
  }

  function storageKey() {
    return state.userId ? `paradise.phone.notifications.seen.v1.${state.userId}` : '';
  }

  function readSeen() {
    const key = storageKey();
    if (!key) return { call: 0, gram: 0, initialized: false };
    try {
      const value = JSON.parse(localStorage.getItem(key) || 'null');
      if (!value || typeof value !== 'object') return { call: 0, gram: 0, initialized: false };
      return {
        call: Math.max(0, Number(value.call || 0)),
        gram: Math.max(0, Number(value.gram || 0)),
        initialized: Boolean(value.initialized)
      };
    } catch {
      return { call: 0, gram: 0, initialized: false };
    }
  }

  function writeSeen(seen) {
    const key = storageKey();
    if (!key) return;
    try {
      localStorage.setItem(key, JSON.stringify({
        call: Math.max(0, Number(seen.call || 0)),
        gram: Math.max(0, Number(seen.gram || 0)),
        initialized: true
      }));
      window.dispatchEvent(new Event('paradise-phone-notifications-seen'));
    } catch {}
  }

  function maxId(source) {
    return state.items
      .filter(item => item.source === source)
      .reduce((max, item) => Math.max(max, Number(item.id || 0)), 0);
  }

  function isUnread(item) {
    const seen = readSeen();
    const marker = item.source === 'call' ? seen.call : seen.gram;
    return Number(item.id || 0) > marker;
  }

  function unreadCount(source = 'all') {
    return state.items.filter(item => (source === 'all' || item.source === source) && isUnread(item)).length;
  }

  function markSourceRead(source, upToId = 0) {
    if (!['call', 'gram'].includes(source)) return;
    const seen = readSeen();
    const target = Math.max(Number(upToId || 0), maxId(source));
    if (source === 'call') seen.call = Math.max(Number(seen.call || 0), target);
    else seen.gram = Math.max(Number(seen.gram || 0), target);
    seen.initialized = true;
    writeSeen(seen);
    updateLauncherBadge();
    if (state.open) renderCenter();
  }

  function markAllRead() {
    const seen = readSeen();
    seen.call = Math.max(Number(seen.call || 0), maxId('call'));
    seen.gram = Math.max(Number(seen.gram || 0), maxId('gram'));
    seen.initialized = true;
    writeSeen(seen);
    updateLauncherBadge();
    if (state.open) renderCenter();
  }

  function copyFor(item) {
    const username = item.username || 'Un joueur';
    if (item.source === 'call') {
      return {
        app: 'Téléphone',
        title: item.type === 'missed_video_call' ? 'Appel vidéo manqué' : 'Appel manqué',
        body: `${username} a essayé de vous appeler.`,
        icon: item.type === 'missed_video_call' ? '▣' : '☎'
      };
    }
    if (item.type === 'like') return { app: 'ParadiseGram', title: 'Nouveau J’aime', body: `${username} a aimé votre publication.`, icon: '♥' };
    if (item.type === 'comment') return { app: 'ParadiseGram', title: 'Nouveau commentaire', body: `${username} a commenté votre publication.`, icon: '●' };
    if (item.type === 'follow') return { app: 'ParadiseGram', title: 'Nouvel abonné', body: `${username} a commencé à vous suivre.`, icon: '＋' };
    return { app: 'ParadiseGram', title: 'Nouvelle activité', body: `${username} a interagi avec votre profil.`, icon: '♥' };
  }

  function relativeTime(timestamp) {
    const now = Number(state.serverTime || Date.now() / 1000);
    const seconds = Math.max(0, Math.floor(now - Number(timestamp || 0)));
    if (seconds < 45) return 'maintenant';
    if (seconds < 3600) return `il y a ${Math.max(1, Math.floor(seconds / 60))} min`;
    if (seconds < 86400) return `il y a ${Math.floor(seconds / 3600)} h`;
    return `il y a ${Math.floor(seconds / 86400)} j`;
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

  function findNativeLauncher(source) {
    const root = frame();
    if (!root) return null;
    const words = source === 'gram'
      ? ['paradise gram', 'paradisegram', 'waver gram', 'wave gram', 'instagram']
      : ['amis', 'friends', 'contacts'];
    return [...root.querySelectorAll('button,[role="button"],a')]
      .filter(node => !node.closest('.phone-active-app,.paradise-phone-notification-center,.paradise-phone-notification-host'))
      .map(node => ({ node, label: candidateLabel(node) }))
      .filter(entry => words.some(word => entry.label.includes(word)))
      .sort((a, b) => a.label.length - b.label.length)[0]?.node || null;
  }

  function launcherHost(template) {
    if (!template) return null;
    let parent = template.parentElement;
    for (let depth = 0; parent && depth < 4; depth += 1, parent = parent.parentElement) {
      const direct = [...parent.children];
      const appLike = direct.filter(child => child.matches?.('.phone-app-icon,button,[role="button"],a') || child.querySelector?.('.phone-app-icon'));
      if (appLike.length >= 3 && appLike.length <= 20) return parent;
    }
    return template.parentElement;
  }

  function makeLauncher(template, fallback = false) {
    let launcher;
    if (template && template.matches('button')) {
      launcher = document.createElement('button');
      launcher.type = 'button';
      launcher.className = `${template.className || ''} paradise-phone-notification-app`;
    } else {
      launcher = document.createElement('button');
      launcher.type = 'button';
      launcher.className = 'phone-app-icon paradise-phone-notification-app';
    }
    launcher.removeAttribute('id');
    launcher.removeAttribute('title');
    launcher.removeAttribute('aria-label');
    launcher.setAttribute('aria-label', 'Notifications');
    launcher.dataset.pncLauncher = '1';
    if (fallback) launcher.classList.add('pnc-fallback');
    launcher.innerHTML = `<span class="pnc-launch-icon" aria-hidden="true"></span><span class="pnc-launch-label">Notifications</span><span class="pnc-launch-badge" hidden></span>`;
    return launcher;
  }

  function ensureLauncher() {
    const root = frame();
    if (!root || !visible(root)) return null;
    const existing = root.querySelector('[data-pnc-launcher]');
    if (existing) {
      updateLauncherBadge(existing);
      return existing;
    }

    if (root.querySelector('.phone-active-app')) return null;
    const template = findNativeLauncher('gram') || findNativeLauncher('call') || root.querySelector('.phone-app-icon');
    if (template) {
      const host = launcherHost(template);
      if (host) {
        const launcher = makeLauncher(template, false);
        host.appendChild(launcher);
        updateLauncherBadge(launcher);
        return launcher;
      }
    }

    const launcher = makeLauncher(null, true);
    root.appendChild(launcher);
    updateLauncherBadge(launcher);
    return launcher;
  }

  function updateLauncherBadge(launcher = null) {
    const root = frame();
    const node = launcher || root?.querySelector('[data-pnc-launcher]');
    if (!node) return;
    const badge = node.querySelector('.pnc-launch-badge');
    if (!badge) return;
    const count = unreadCount('all');
    badge.hidden = count <= 0;
    badge.textContent = count > MAX_BADGE ? `${MAX_BADGE}+` : String(count);
  }

  function rowHtml(item) {
    const copy = copyFor(item);
    const unread = isUnread(item);
    const hasAvatar = Boolean(item.look);
    return `<button type="button" class="pnc-row is-${escapeHtml(item.source)} ${unread ? 'is-unread' : ''}" data-pnc-item data-pnc-source="${escapeHtml(item.source)}" data-pnc-id="${Number(item.id || 0)}">
      <span class="pnc-avatar">
        ${hasAvatar ? `<img src="/avatar.php?figure=${encodeURIComponent(item.look)}&size=m&direction=2&head_direction=2&headonly=1&gesture=sml" alt="">` : ''}
        <i>${escapeHtml(copy.icon)}</i>
      </span>
      <span class="pnc-copy">
        <span class="pnc-meta"><strong>${escapeHtml(copy.app)}</strong><time>${escapeHtml(relativeTime(item.createdAt))}</time></span>
        <b>${escapeHtml(copy.title)}</b>
        <small>${escapeHtml(copy.body)}</small>
      </span>
      <span class="pnc-dot" aria-hidden="true"></span>
    </button>`;
  }

  function filteredItems() {
    if (state.filter === 'call') return state.items.filter(item => item.source === 'call');
    if (state.filter === 'gram') return state.items.filter(item => item.source === 'gram');
    return state.items;
  }

  function renderCenter() {
    const root = frame();
    if (!root || !state.open) return;
    let center = root.querySelector(':scope > .paradise-phone-notification-center');
    if (!center) {
      center = document.createElement('section');
      center.className = 'paradise-phone-notification-center';
      center.dataset.pncCenter = '1';
      root.appendChild(center);
    }

    const items = filteredItems();
    const unread = unreadCount('all');
    center.innerHTML = `<header class="pnc-header">
      <button type="button" class="pnc-back" data-pnc-back aria-label="Retour">‹</button>
      <div class="pnc-title"><strong>Notifications</strong><small>${unread ? `${unread} non lue${unread > 1 ? 's' : ''}` : 'Tout est à jour'}</small></div>
      <button type="button" class="pnc-read-all" data-pnc-read-all ${unread ? '' : 'disabled'}>Tout lire</button>
    </header>
    <nav class="pnc-tabs" aria-label="Filtres des notifications">
      <button type="button" class="pnc-tab ${state.filter === 'all' ? 'is-active' : ''}" data-pnc-filter="all">Toutes</button>
      <button type="button" class="pnc-tab ${state.filter === 'call' ? 'is-active' : ''}" data-pnc-filter="call">Appels</button>
      <button type="button" class="pnc-tab ${state.filter === 'gram' ? 'is-active' : ''}" data-pnc-filter="gram">ParadiseGram</button>
    </nav>
    <div class="pnc-list">${items.length ? items.map(rowHtml).join('') : `<div class="pnc-empty"><span class="pnc-empty-icon">⌁</span><strong>Aucune notification</strong><small>${state.filter === 'all' ? 'Vos appels manqués et activités ParadiseGram apparaîtront ici.' : 'Aucune notification dans cette catégorie.'}</small></div>`}</div>`;
  }

  function openCenter() {
    const root = frame();
    if (!root) return;
    const active = root.querySelector('.phone-active-app');
    if (active) {
      const home = active.querySelector('.phone-app-home') || root.querySelector('.phone-app-home');
      home?.click();
    }
    state.open = true;
    state.filter = 'all';
    renderCenter();
    poll(true);
  }

  function closeCenter() {
    state.open = false;
    frame()?.querySelector(':scope > .paradise-phone-notification-center')?.remove();
    window.setTimeout(ensureLauncher, 80);
  }

  function openSource(source) {
    closeCenter();
    markSourceRead(source);

    const activeGram = document.querySelector('.phone-active-app .ppr-gram[data-ppr-ready]');
    const activeFriends = document.querySelector('.phone-active-app .phone-friends-app');
    if (source === 'gram' && activeGram) {
      activeGram.querySelector('[data-pg-nav="activity"]')?.click();
      return;
    }
    if (source === 'call' && activeFriends) return;

    const root = frame();
    const home = root?.querySelector('.phone-app-home');
    if (home && root?.querySelector('.phone-active-app')) home.click();
    window.setTimeout(() => {
      const launcher = findNativeLauncher(source);
      launcher?.click();
      if (source === 'gram') {
        window.setTimeout(() => document.querySelector('.phone-active-app [data-pg-nav="activity"]')?.click(), 350);
      }
    }, 160);
  }

  async function poll(forceRender = false) {
    if (state.busy) return;
    state.busy = true;
    try {
      const response = await fetch(API, { credentials: 'same-origin', cache: 'no-store' });
      const payload = await response.json().catch(() => null);
      if (!response.ok || !payload?.ok) throw new Error(payload?.error || 'Notifications indisponibles.');
      state.userId = Number(payload.me?.id || 0);
      state.serverTime = Number(payload.serverTime || Math.floor(Date.now() / 1000));
      state.items = Array.isArray(payload.items) ? payload.items : [];
      updateLauncherBadge();
      if (state.open || forceRender) renderCenter();
    } catch (error) {
      console.warn('[ParadisePhone Notification Center V2]', error);
      if (state.open) {
        const list = frame()?.querySelector('.paradise-phone-notification-center .pnc-list');
        if (list) list.innerHTML = '<div class="pnc-empty"><span class="pnc-empty-icon">!</span><strong>Notifications indisponibles</strong><small>Réessayez dans quelques secondes.</small></div>';
      }
    } finally {
      state.busy = false;
    }
  }

  document.addEventListener('click', event => {
    const launcher = event.target.closest('[data-pnc-launcher]');
    if (launcher) {
      event.preventDefault();
      event.stopPropagation();
      openCenter();
      return;
    }

    if (!state.open) return;

    const back = event.target.closest('[data-pnc-back]');
    if (back) {
      event.preventDefault();
      event.stopPropagation();
      closeCenter();
      return;
    }

    const readAll = event.target.closest('[data-pnc-read-all]');
    if (readAll) {
      event.preventDefault();
      event.stopPropagation();
      markAllRead();
      return;
    }

    const tab = event.target.closest('[data-pnc-filter]');
    if (tab) {
      event.preventDefault();
      event.stopPropagation();
      state.filter = ['all', 'call', 'gram'].includes(tab.dataset.pncFilter) ? tab.dataset.pncFilter : 'all';
      renderCenter();
      return;
    }

    const row = event.target.closest('[data-pnc-item]');
    if (row) {
      event.preventDefault();
      event.stopPropagation();
      const source = row.dataset.pncSource;
      markSourceRead(source, Number(row.dataset.pncId || 0));
      openSource(source);
      return;
    }

    if (event.target.closest('.phone-home-indicator')) {
      event.preventDefault();
      event.stopPropagation();
      closeCenter();
    }
  }, true);

  window.addEventListener('paradise-phone-notifications-seen', () => {
    updateLauncherBadge();
    if (state.open) renderCenter();
  });
  window.addEventListener('storage', event => {
    if (event.key?.startsWith('paradise.phone.notifications.seen.v1.')) {
      updateLauncherBadge();
      if (state.open) renderCenter();
    }
  });

  function scheduleDomSync() {
    if (state.observerFrame) return;
    state.observerFrame = requestAnimationFrame(() => {
      state.observerFrame = 0;
      const root = frame();
      if (!root) {
        state.open = false;
        return;
      }
      if (state.open) renderCenter();
      else ensureLauncher();
      updateLauncherBadge();
    });
  }

  function bootstrap() {
    state.observer = new MutationObserver(scheduleDomSync);
    state.observer.observe(document.documentElement, { childList: true, subtree: true });
    poll(false);
    window.setTimeout(ensureLauncher, 300);
    clearInterval(state.timer);
    state.timer = window.setInterval(() => poll(false), POLL_MS);
    console.info('[ParadisePhone] centre de notifications V2 actif');
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', bootstrap, { once: true });
  else bootstrap();
})();
