(() => {
  'use strict';

  if (window.__PARADISE_PHONE_NOTIFICATIONS_V1__) return;
  window.__PARADISE_PHONE_NOTIFICATIONS_V1__ = '1.2.0';

  const API = '/nitro/phone-notifications-api.php';
  const POLL_MS = 5000;
  const RECENT_FIRST_RUN_SECONDS = 15 * 60;
  const MAX_BADGE = 99;

  const state = {
    userId: 0,
    serverTime: 0,
    items: [],
    busy: false,
    timer: 0,
    observer: null,
    observerFrame: 0,
    announced: new Set(),
    toastTimer: 0
  };

  const escapeHtml = value => String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;');

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

  function writeSeen(value) {
    const key = storageKey();
    if (!key) return;
    try {
      localStorage.setItem(key, JSON.stringify({
        call: Math.max(0, Number(value.call || 0)),
        gram: Math.max(0, Number(value.gram || 0)),
        initialized: true
      }));
    } catch {}
  }

  function maxId(source, predicate = () => true) {
    return state.items
      .filter(item => item.source === source && predicate(item))
      .reduce((max, item) => Math.max(max, Number(item.id || 0)), 0);
  }

  function initializeSeenIfNeeded() {
    const seen = readSeen();
    if (seen.initialized) return seen;

    const recentCutoff = Number(state.serverTime || Math.floor(Date.now() / 1000)) - RECENT_FIRST_RUN_SECONDS;
    const baseline = {
      call: maxId('call', item => Number(item.createdAt || 0) < recentCutoff),
      gram: maxId('gram', item => Number(item.createdAt || 0) < recentCutoff),
      initialized: true
    };
    writeSeen(baseline);
    return baseline;
  }

  function unreadItems(source) {
    const seen = readSeen();
    const marker = source === 'call' ? seen.call : seen.gram;
    return state.items.filter(item => item.source === source && Number(item.id || 0) > marker);
  }

  function unreadCount(source) {
    return unreadItems(source).length;
  }

  function markRead(source, upToId = 0) {
    if (!['call', 'gram'].includes(source)) return;
    const seen = readSeen();
    const before = source === 'call' ? seen.call : seen.gram;
    const target = Math.max(Number(upToId || 0), maxId(source));
    const after = Math.max(before, target);
    if (after === before && seen.initialized) return;

    if (source === 'call') seen.call = after;
    else seen.gram = after;
    seen.initialized = true;
    writeSeen(seen);
    renderBadges();
  }

  function notificationText(item) {
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
    const seconds = Math.max(0, Math.floor(Number(state.serverTime || Date.now() / 1000) - Number(timestamp || 0)));
    if (seconds < 45) return 'maintenant';
    if (seconds < 3600) return `il y a ${Math.max(1, Math.floor(seconds / 60))} min`;
    if (seconds < 86400) return `il y a ${Math.floor(seconds / 3600)} h`;
    return `il y a ${Math.floor(seconds / 86400)} j`;
  }

  function getFrame() {
    return document.querySelector('.nitro-phone-frame');
  }

  function visible(node) {
    if (!node) return false;
    const style = getComputedStyle(node);
    return style.display !== 'none' && style.visibility !== 'hidden' && Number(style.opacity || 1) > 0 && node.getClientRects().length > 0;
  }

  function ensureToastHost() {
    const frame = getFrame();
    if (!frame) return null;
    let host = frame.querySelector(':scope > .paradise-phone-notification-host');
    if (!host) {
      host = document.createElement('div');
      host.className = 'paradise-phone-notification-host';
      frame.appendChild(host);
    }
    return host;
  }

  function showToast(item) {
    if (!item) return false;
    const frame = getFrame();
    if (!visible(frame)) return false;
    const host = ensureToastHost();
    if (!host) return false;

    const copy = notificationText(item);
    const hasAvatar = Boolean(item.look);
    host.innerHTML = `<article class="paradise-phone-notification is-${escapeHtml(item.source)}" data-pnotif-source="${escapeHtml(item.source)}" data-pnotif-id="${Number(item.id || 0)}">
      <button type="button" class="pnotif-main" data-pnotif-open>
        <span class="pnotif-visual ${hasAvatar ? 'has-avatar' : ''}">
          ${hasAvatar ? `<img src="/avatar.php?figure=${encodeURIComponent(item.look)}&size=m&direction=2&head_direction=2&headonly=1" alt="">` : `<b>${copy.icon}</b>`}
          <i>${copy.icon}</i>
        </span>
        <span class="pnotif-copy">
          <span class="pnotif-app"><strong>${escapeHtml(copy.app)}</strong><time>${escapeHtml(relativeTime(item.createdAt))}</time></span>
          <b>${escapeHtml(copy.title)}</b>
          <small>${escapeHtml(copy.body)}</small>
        </span>
      </button>
      <button type="button" class="pnotif-close" data-pnotif-close aria-label="Fermer">×</button>
    </article>`;

    const card = host.querySelector('.paradise-phone-notification');
    requestAnimationFrame(() => card?.classList.add('is-visible'));
    clearTimeout(state.toastTimer);
    state.toastTimer = window.setTimeout(() => hideToast(false), 6200);
    return true;
  }

  function hideToast(markCurrent = false) {
    const host = getFrame()?.querySelector(':scope > .paradise-phone-notification-host');
    const card = host?.querySelector('.paradise-phone-notification');
    if (!card) return;
    if (markCurrent) markRead(card.dataset.pnotifSource, Number(card.dataset.pnotifId || 0));
    card.classList.remove('is-visible');
    clearTimeout(state.toastTimer);
    state.toastTimer = window.setTimeout(() => card.remove(), 180);
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

  function findLauncher(source) {
    const frame = getFrame();
    if (!frame) return null;
    const words = source === 'gram'
      ? ['paradise gram', 'paradisegram', 'waver gram', 'wave gram', 'instagram']
      : ['amis', 'friends', 'contacts'];
    const candidates = [...frame.querySelectorAll('button,[role="button"],a')]
      .filter(node => !node.closest('.phone-active-app,.paradise-phone-notification-host'))
      .map(node => ({ node, label: candidateLabel(node) }))
      .filter(entry => words.some(word => entry.label.includes(word)))
      .sort((a, b) => a.label.length - b.label.length);
    return candidates[0]?.node || null;
  }

  function badgeFor(source) {
    const count = unreadCount(source);
    const launcher = findLauncher(source);
    if (!launcher) return;
    launcher.classList.add('paradise-phone-badge-anchor');
    let badge = launcher.querySelector(':scope > .paradise-phone-notification-badge');
    if (!count) {
      badge?.remove();
      return;
    }
    if (!badge) {
      badge = document.createElement('span');
      badge.className = 'paradise-phone-notification-badge';
      badge.setAttribute('aria-hidden', 'true');
      launcher.appendChild(badge);
    }
    const value = count > MAX_BADGE ? `${MAX_BADGE}+` : String(count);
    if (badge.textContent !== value) badge.textContent = value;
  }

  function renderBadges() {
    badgeFor('gram');
    badgeFor('call');
  }

  function openSource(source) {
    markRead(source);
    hideToast(false);

    const activeGram = document.querySelector('.phone-active-app .ppr-gram[data-ppr-ready]');
    const activeFriends = document.querySelector('.phone-active-app .phone-friends-app');
    if (source === 'gram' && activeGram) {
      activeGram.querySelector('[data-pg-nav="activity"]')?.click();
      return;
    }
    if (source === 'call' && activeFriends) return;

    const home = document.querySelector('.nitro-phone-frame .phone-app-home');
    if (home && document.querySelector('.nitro-phone-frame .phone-active-app')) home.click();

    window.setTimeout(() => {
      const launcher = findLauncher(source);
      launcher?.click();
      if (source === 'gram') {
        window.setTimeout(() => document.querySelector('.phone-active-app [data-pg-nav="activity"]')?.click(), 350);
      }
    }, 180);
  }

  function syncOpenedApps() {
    const gram = document.querySelector('.phone-active-app .ppr-gram[data-ppr-ready]');
    if (visible(gram) && unreadCount('gram')) markRead('gram');
    const friends = document.querySelector('.phone-active-app .phone-friends-app');
    if (visible(friends) && unreadCount('call')) markRead('call');
    renderBadges();
  }

  function announceNewItems() {
    const unread = [...unreadItems('call'), ...unreadItems('gram')]
      .sort((a, b) => Number(a.createdAt || 0) - Number(b.createdAt || 0));
    const fresh = unread.filter(item => !state.announced.has(`${item.source}:${item.id}`));
    if (!fresh.length || !visible(getFrame())) return;

    const newest = fresh[fresh.length - 1];
    if (!showToast(newest)) return;
    fresh.forEach(item => state.announced.add(`${item.source}:${item.id}`));
  }

  function scheduleDomSync() {
    if (state.observerFrame) return;
    state.observerFrame = requestAnimationFrame(() => {
      state.observerFrame = 0;
      syncOpenedApps();
      announceNewItems();
    });
  }

  async function poll() {
    if (state.busy) return;
    state.busy = true;
    try {
      const response = await fetch(API, { credentials: 'same-origin', cache: 'no-store' });
      const payload = await response.json().catch(() => null);
      if (!response.ok || !payload?.ok) throw new Error(payload?.error || 'Notifications indisponibles.');

      state.userId = Number(payload.me?.id || 0);
      state.serverTime = Number(payload.serverTime || Math.floor(Date.now() / 1000));
      state.items = Array.isArray(payload.items) ? payload.items : [];
      initializeSeenIfNeeded();
      renderBadges();
      announceNewItems();
      syncOpenedApps();
    } catch (error) {
      console.warn('[ParadisePhone notifications]', error);
    } finally {
      state.busy = false;
    }
  }

  document.addEventListener('click', event => {
    const close = event.target.closest('[data-pnotif-close]');
    if (close) {
      event.preventDefault();
      event.stopPropagation();
      hideToast(true);
      return;
    }

    const open = event.target.closest('[data-pnotif-open]');
    if (open) {
      event.preventDefault();
      event.stopPropagation();
      const card = open.closest('[data-pnotif-source]');
      openSource(card?.dataset.pnotifSource || '');
    }
  }, true);

  function bootstrap() {
    state.observer = new MutationObserver(scheduleDomSync);
    state.observer.observe(document.documentElement, { childList: true, subtree: true });
    poll();
    clearInterval(state.timer);
    state.timer = window.setInterval(poll, POLL_MS);
    console.info('[ParadisePhone] notifications V1.2 actives');
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', bootstrap, { once: true });
  else bootstrap();
})();
