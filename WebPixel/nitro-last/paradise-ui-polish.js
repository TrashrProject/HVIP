(() => {
  'use strict';

  const VERSION = '82.0.0-targeted-polish-dynamic';
  const HUD_ID = 'paradise-rp-hud';
  const DATA_URL = '../rp-hud-data.php';

  const cache = {
    data: null,
    lastPayload: '',
    timer: 0,
    mutationTimer: 0
  };

  const esc = value => String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;');

  const isFilled = value => value !== undefined && value !== null && String(value).trim() !== '';
  const asText = value => isFilled(value) ? String(value).trim() : '';
  const toNumber = value => Number.isFinite(Number(value)) ? Number(value) : null;
  const format = value => {
    const n = toNumber(value);
    return n === null ? '' : new Intl.NumberFormat('fr-FR').format(n);
  };

  const paths = {
    phone: '<rect x="7" y="2.8" width="10" height="18.4" rx="2"/><path d="M10.4 5h3.2M11 18.2h2"/>',
    bag: '<path d="M6 8h12l1 12H5L6 8Z"/><path d="M9 8V6a3 3 0 0 1 6 0v2"/>',
    doc: '<path d="M6 3h8l4 4v14H6V3Z"/><path d="M14 3v5h5M9 12h6M9 16h6"/>',
    car: '<path d="M5 9h14l2 4v5H3v-5l2-4Z"/><path d="m7 9 1.5-4h7L17 9M6 18v2M18 18v2"/>',
    chat: '<path d="M4 5h16v11H9l-5 4V5Z"/>',
    pin: '<path d="M12 21s6-5.3 6-11a6 6 0 0 0-12 0c0 5.7 6 11 6 11Z"/><circle cx="12" cy="10" r="2"/>',
    heart: '<path d="M20.8 4.6a5.4 5.4 0 0 0-7.6 0L12 5.8l-1.2-1.2a5.4 5.4 0 1 0-7.6 7.6L12 21l8.8-8.8a5.4 5.4 0 0 0 0-7.6Z"/>',
    shield: '<path d="M12 3 20 6v6c0 4.6-3 7.2-8 9-5-1.8-8-4.4-8-9V6l8-3Z"/>',
    briefcase: '<rect x="3" y="7" width="18" height="12" rx="2"/><path d="M9 7V5.5A1.5 1.5 0 0 1 10.5 4h3A1.5 1.5 0 0 1 15 5.5V7M3 12h18"/>',
    plus: '<path d="M12 5v14M5 12h14"/>',
    wallet: '<path d="M4 6.5h14a2 2 0 0 1 2 2V18H5.5A2.5 2.5 0 0 1 3 15.5v-9A2.5 2.5 0 0 1 5.5 4H18"/><path d="M15 11h5v4h-5a2 2 0 0 1 0-4Z"/>',
    sun: '<circle cx="12" cy="12" r="4"/><path d="M12 2v2M12 20v2M4 12H2M22 12h-2M5 5l1.4 1.4M17.6 17.6 19 19M19 5l-1.4 1.4M6.4 17.6 5 19"/>'
  };

  const icon = name => `<span class="pr4-icon"><svg viewBox="0 0 24 24" aria-hidden="true">${paths[name] || paths.plus}</svg></span>`;

  function avatarUrl(data) {
    const raw = asText(data?.avatar_url);
    if (raw) return raw;
    const look = asText(data?.look);
    if (look && /^[a-z0-9.\-]+$/i.test(look)) {
      return `../avatar-image.php?figure=${encodeURIComponent(look)}&direction=2&head_direction=3&gesture=sml&action=std&size=l&hud=82`;
    }
    return '';
  }

  function ratio(current, max) {
    const c = toNumber(current);
    const m = toNumber(max);
    if (c === null || m === null || m <= 0) return null;
    return Math.max(0, Math.min(100, Math.round((c / m) * 100)));
  }

  function statLine(kind, label, current, max) {
    const percent = ratio(current, max);
    const c = toNumber(current);
    const m = toNumber(max);
    if (percent === null || c === null || m === null) return '';
    let tone = kind;
    if (kind === 'health') tone = percent <= 25 ? 'health danger' : percent <= 55 ? 'health warn' : 'health';
    return `<div class="pr4-polish-stat ${tone}"><span>${icon(kind === 'armor' ? 'shield' : 'heart')}<b>${esc(label)}</b></span><em>${Math.round(c)} / ${Math.round(m)}</em><i><u style="width:${percent}%"></u></i></div>`;
  }

  function nativeChat() {
    try {
      return [...document.querySelectorAll('#root [data-pr-native-chat-live="1"], #root input[placeholder*="chat" i], #root textarea[placeholder*="chat" i], #root input[placeholder*="chatter" i]')]
        .find(el => el && !el.disabled && !el.readOnly && !el.closest(`#${HUD_ID}`)) || null;
    } catch (_) { return null; }
  }

  function focusChat() {
    const input = nativeChat();
    if (!input) return false;
    input.focus({ preventScroll: true });
    try { input.setSelectionRange(input.value.length, input.value.length); } catch (_) {}
    return true;
  }

  async function loadData() {
    try {
      const response = await fetch(`${DATA_URL}?polish=${Date.now()}`, { cache: 'no-store', credentials: 'same-origin' });
      if (!response.ok) return;
      const json = await response.json();
      cache.data = json || null;
      apply();
    } catch (_) {}
  }

  function action(actionName) {
    try {
      window.__ParadiseRPUI?.open?.(actionName);
      return true;
    } catch (_) { return false; }
  }

  function patchBottomNavigation(root) {
    const left = root.querySelector('.pr4-dock-left');
    const right = root.querySelector('.pr4-dock-right');
    if (!left || !right) return;

    left.querySelectorAll('[data-pr4-action="open:phone"]').forEach(btn => btn.remove());
    right.querySelectorAll('[data-pr4-action="open:social"], [data-pr4-action="open:notifications"]').forEach(btn => btn.remove());

    let phoneBtn = right.querySelector('[data-pr4-polish-phone="1"]');
    const moreBtn = right.querySelector('[data-pr4-action="toggle-more"]');
    if (!phoneBtn) {
      phoneBtn = document.createElement('button');
      phoneBtn.type = 'button';
      phoneBtn.className = 'app pr4-polish-phone';
      phoneBtn.dataset.pr4PolishPhone = '1';
      phoneBtn.dataset.tip = 'Téléphone · P';
      phoneBtn.innerHTML = `${icon('phone')}<span>Téléphone</span>`;
      phoneBtn.addEventListener('click', event => {
        event.preventDefault();
        event.stopPropagation();
        action('phone');
      });
      right.insertBefore(phoneBtn, moreBtn || right.firstChild);
    }

    if (moreBtn) {
      moreBtn.classList.add('pr4-polish-actions');
      moreBtn.dataset.tip = 'Actions rapides';
      const label = moreBtn.querySelector('span');
      if (label) label.textContent = 'Actions';
    }
  }

  function patchChat(root) {
    const module = root.querySelector('.pr4-chat-module');
    if (!module) return;
    module.classList.add('pr4-polish-chat');
    module.innerHTML = `<span>LOCAL</span><i>${icon('chat')}</i><small>Écrire un message...</small><button type="button" aria-label="Focus chat">${icon('chat')}</button>`;
    module.onclick = event => {
      event.preventDefault();
      focusChat();
    };
  }

  function patchProfile(root) {
    const data = cache.data;
    if (!data || data.ok === false) return;

    const card = root.querySelector('.pr4-player-card');
    if (card) {
      const username = asText(data.username) || 'Joueur';
      const role = asText(data.role);
      const job = asText(data.job || data.métier || data.profession);
      const location = asText(data.room || data.district || data.city);
      const ava = avatarUrl(data);
      const armor = data.armor || data.shield || data.armour || null;
      const health = data.health || null;
      const stats = [
        statLine('health', 'Santé', health?.current, health?.max),
        statLine('armor', 'Armure', armor?.current, armor?.max)
      ].filter(Boolean).join('');

      card.classList.add('pr4-player-card-polished');
      card.innerHTML = `
        <span class="pr4-player-avatar">${ava ? `<img src="${esc(ava)}" alt="${esc(username)}">` : '<b>RP</b>'}<i></i></span>
        <span class="pr4-player-text">
          <strong>${esc(username)}</strong>
          <span class="pr4-player-tags">${role ? `<em>${esc(role)}</em>` : ''}${job ? `<em class="job">${esc(job)}</em>` : ''}</span>
          ${location ? `<small>${esc(location)}</small>` : ''}
          ${stats ? `<span class="pr4-player-stats">${stats}</span>` : ''}
        </span>`;
    }

    const room = root.querySelector('.pr4-room');
    if (room) {
      const primary = asText(data.room || data.district || data.city);
      const secondaryParts = [asText(data.district && data.room ? data.district : ''), asText(data.players) ? `${format(data.players)} joueurs` : ''].filter(Boolean);
      if (!primary) {
        room.hidden = true;
      } else {
        room.hidden = false;
        room.classList.add('pr4-room-polished');
        room.innerHTML = `${icon('pin')}<span><strong>${esc(primary)}</strong>${secondaryParts.length ? `<small>${esc(secondaryParts.join(' · '))}</small>` : ''}</span>`;
      }
    }
  }

  function patchStatus(root) {
    const data = cache.data;
    if (!data || data.ok === false) return;
    const money = data.money || {};
    const status = root.querySelector('.pr4-status');
    if (!status) return;

    const cash = status.querySelector('.cash b');
    const bank = status.querySelector('.bank b');
    const staff = status.querySelector('.staff b');
    const time = status.querySelector('.time b');
    const notif = status.querySelector('.notif b');

    if (cash && toNumber(money.cash ?? money.credits) !== null) cash.textContent = `${format(money.cash ?? money.credits)} $`;
    if (bank) {
      const bankValue = money.bank;
      if (toNumber(bankValue) === null) bank.closest('button')?.remove();
      else bank.textContent = `${format(bankValue)} $`;
    }
    if (staff) staff.textContent = asText(data.role) || asText(data.job) || 'Citoyen';
    if (time) time.textContent = asText(data.time) || new Date().toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' });
    if (notif) notif.textContent = format(data.notifications?.count ?? data.notifications_count ?? 0) || '0';
  }

  function apply() {
    const root = document.getElementById(HUD_ID);
    if (!root) return;
    patchBottomNavigation(root);
    patchChat(root);
    patchProfile(root);
    patchStatus(root);
  }

  function bindKeyboard() {
    window.addEventListener('keydown', event => {
      const input = nativeChat();
      const typingNativeChat = input && event.target === input;
      if (event.key === 'Escape' && typingNativeChat) {
        event.preventDefault();
        input.blur();
      }
      if (event.key === 'Enter' && !event.target?.closest?.(`#${HUD_ID}`) && document.activeElement !== input) {
        const tag = event.target?.tagName;
        const typing = tag === 'INPUT' || tag === 'TEXTAREA' || event.target?.isContentEditable;
        if (!typing && focusChat()) event.preventDefault();
      }
    }, true);
  }

  function boot() {
    bindKeyboard();
    loadData();
    window.setInterval(loadData, 5000);
    const observer = new MutationObserver(() => {
      window.clearTimeout(cache.mutationTimer);
      cache.mutationTimer = window.setTimeout(apply, 25);
    });
    observer.observe(document.body, { childList: true, subtree: true });
    window.__ParadiseRPPolish = { version: VERSION, refresh: loadData, focusChat };
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
