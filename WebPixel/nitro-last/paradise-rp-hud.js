(() => {
  'use strict';

  const VERSION = '14.0.0';
  const HUD_ID = 'paradise-rp-hud';
  const STYLE_ID = 'paradise-rp-hud-css';
  const DATA_URL = '../rp-hud-data.php';
  const CSS_URL = './paradise-rp-hud.css?v=14';

  const DEFAULT_DATA = {
    ok: false,
    username: 'ParadiseRP',
    role: 'Citoyen',
    level: 7,
    look: '',
    avatar_url: '',
    health: { current: 315, max: 500 },
    energy: { current: 31, max: 100 },
    money: { credits: 789, pixels: 5000, cash: 1789, diamonds: 224 },
    city: 'Paradise City',
    time: '18:36'
  };

  const dockItems = [
    { key: 'player', label: 'Joueur', icon: 'user', command: '' },
    { key: 'phone', label: 'Téléphone', icon: 'phone', command: ':tel' },
    { key: 'id', label: 'Carte ID', icon: 'id', command: ':id' },
    { key: 'job', label: 'Métier', icon: 'briefcase', command: ':trabajar' },
    { key: 'home', label: 'Accueil', icon: 'home', command: '' },
    { key: 'shop', label: 'Boutique', icon: 'cart', command: '' },
    { key: 'bag', label: 'Inventaire', icon: 'bag', command: '' },
    { key: 'cmd', label: 'Commandes', icon: 'terminal', command: ':commands' }
  ];

  const railItems = [
    { key: 'home', label: 'Accueil', icon: 'home', command: '' },
    { key: 'bag', label: 'Inventaire', icon: 'bag', command: '' },
    { key: 'job', label: 'Métier', icon: 'briefcase', command: ':trabajar' },
    { key: 'map', label: 'Carte', icon: 'pin', command: '' },
    { key: 'shop', label: 'Boutique', icon: 'cart', command: '' }
  ];

  const svg = {
    user: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M12 12.2a4.2 4.2 0 1 0 0-8.4 4.2 4.2 0 0 0 0 8.4Z"/><path d="M4.7 20.2c.8-4.1 3.2-6.1 7.3-6.1s6.5 2 7.3 6.1"/></svg>',
    phone: '<svg viewBox="0 0 24 24" aria-hidden="true"><rect x="7" y="2.7" width="10" height="18.6" rx="2.2"/><path d="M10.4 5h3.2M11 18.1h2"/></svg>',
    id: '<svg viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="5" width="18" height="14" rx="2"/><path d="M7 10h4M7 14h5.5M15 10h2.7M15 14h2.7"/></svg>',
    briefcase: '<svg viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="7" width="18" height="12" rx="2"/><path d="M9 7V5.3c0-.8.5-1.3 1.3-1.3h3.4c.8 0 1.3.5 1.3 1.3V7M3 12h18M12 11v2"/></svg>',
    home: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M3.8 11.2 12 4.2l8.2 7"/><path d="M6.2 10.4v9h11.6v-9"/><path d="M10 19.4v-5.1h4v5.1"/></svg>',
    cart: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M4 5h2l1.5 9.3h9.7l2-6.4H7"/><circle cx="10" cy="19" r="1.3"/><circle cx="17" cy="19" r="1.3"/></svg>',
    bag: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M6.2 8.2h11.6l1 12H5.2l1-12Z"/><path d="M9 8.2V6a3 3 0 0 1 6 0v2.2"/></svg>',
    terminal: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="m5 7 5 5-5 5M12 17h7"/></svg>',
    pin: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M12 21s6-5.3 6-11a6 6 0 0 0-12 0c0 5.7 6 11 6 11Z"/><circle cx="12" cy="10" r="2.2"/></svg>',
    clock: '<svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="8.5"/><path d="M12 7.5v5l3.6 2"/></svg>',
    heart: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M12 20s-7.4-4.3-8.6-9.2C2.6 7.4 4.6 5 7.4 5c1.7 0 3 1 4.6 2.8C13.6 6 14.9 5 16.6 5c2.8 0 4.8 2.4 4 5.8C19.4 15.7 12 20 12 20Z"/></svg>',
    bolt: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="m13.2 2.8-8 11.1h6.2L10.8 21l8-11.1h-6.2l.6-7.1Z"/></svg>',
    gem: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M6.8 4h10.4l3.3 4.7L12 20 3.5 8.7 6.8 4Z"/><path d="M4 8.7h16M8.5 4 12 20 15.5 4"/></svg>',
    credit: '<svg viewBox="0 0 24 24" aria-hidden="true"><rect x="3.8" y="6" width="16.4" height="12" rx="2"/><path d="M3.8 10h16.4"/></svg>',
    star: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="m12 3 2.5 5.3 5.8.8-4.2 4.1 1 5.8-5.1-2.8L6.9 19l1-5.8-4.2-4.1 5.8-.8L12 3Z"/></svg>',
    group: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M9.7 11.3a3.6 3.6 0 1 0 0-7.2 3.6 3.6 0 0 0 0 7.2Z"/><path d="M3.8 20c.7-4.4 2.7-6.5 5.9-6.5s5.2 2.1 5.9 6.5"/><path d="M16.3 12.2a3 3 0 1 0-1.1-5.8M15.8 14c2.2.3 3.7 2 4.4 5"/></svg>',
    chat: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M4 6.5A3.5 3.5 0 0 1 7.5 3h9A3.5 3.5 0 0 1 20 6.5v4A3.5 3.5 0 0 1 16.5 14H11l-4.2 4.2V14A3.5 3.5 0 0 1 4 10.5v-4Z"/></svg>',
    send: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M3.5 20.5 21 12 3.5 3.5 6.4 11H14l-7.6 2-2.9 7.5Z"/></svg>'
  };

  const icon = name => `<span class="prhud-icon prhud-icon-${safe(name)}">${svg[name] || svg.home}</span>`;

  const safe = value => String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;');

  const number = value => Number.isFinite(Number(value)) ? Number(value) : 0;
  const clamp = (value, min, max) => Math.max(min, Math.min(max, number(value)));
  const fmt = value => new Intl.NumberFormat('fr-FR').format(number(value));
  const pct = (cur, max) => `${clamp(number(cur) / Math.max(1, number(max)) * 100, 0, 100)}%`;

  const ensureCss = () => {
    let link = document.getElementById(STYLE_ID);
    if (link) {
      if (!link.href.includes('v=14')) link.href = CSS_URL;
      return;
    }
    link = document.createElement('link');
    link.id = STYLE_ID;
    link.rel = 'stylesheet';
    link.href = CSS_URL;
    document.head.appendChild(link);
  };

  const removeOldMasks = () => {
    document.querySelectorAll('#paradise-rp-hard-masks, .pr-mask, .prhud-cover').forEach(el => {
      try { el.remove(); } catch (_) {}
    });
  };

  const hasLargeGameCanvas = el => {
    if (!el || !el.querySelectorAll) return false;
    const canvases = el.querySelectorAll('canvas');
    for (const canvas of canvases) {
      const r = canvas.getBoundingClientRect();
      if (r.width > window.innerWidth * 0.38 && r.height > window.innerHeight * 0.38) return true;
    }
    return false;
  };

  const isOwnElement = el => {
    if (!el || el === document.documentElement || el === document.body) return true;
    if (el.closest(`#${HUD_ID}, #paradise-loader`)) return true;
    if (['SCRIPT', 'STYLE', 'LINK', 'META', 'TITLE', 'BASE', 'NOSCRIPT'].includes(el.tagName)) return true;
    return false;
  };

  const inLegacyZone = rect => {
    const vw = window.innerWidth;
    const vh = window.innerHeight;
    const topLeft = rect.left < 330 && rect.top < 155 && rect.width < 335 && rect.height < 160;
    const leftIcons = rect.left < 115 && rect.top > 65 && rect.top < vh - 92 && rect.width < 125 && rect.height < vh - 145;
    const bottomLeft = rect.left < 470 && rect.bottom > vh - 125 && rect.width < 500 && rect.height < 125;
    const bottomPhone = rect.right > vw - 92 && rect.bottom > vh - 100 && rect.width < 110 && rect.height < 110;
    const oldTopRight = rect.right > vw - 300 && rect.top < 85 && rect.width < 310 && rect.height < 90;
    return topLeft || leftIcons || bottomLeft || bottomPhone || oldTopRight;
  };

  const hideNode = el => {
    el.classList.add('prhud-native-kill');
    el.setAttribute('data-prhud-killed', '1');
    el.style.setProperty('display', 'none', 'important');
    el.style.setProperty('visibility', 'hidden', 'important');
    el.style.setProperty('opacity', '0', 'important');
    el.style.setProperty('pointer-events', 'none', 'important');
  };

  const killLegacyHud = () => {
    removeOldMasks();
    const nodes = Array.from(document.querySelectorAll('body *'));
    for (const el of nodes) {
      if (isOwnElement(el)) continue;
      if (el.id === 'root' || el.tagName === 'CANVAS') continue;

      const rect = el.getBoundingClientRect();
      if (!rect || rect.width <= 2 || rect.height <= 2) continue;
      if (rect.width > window.innerWidth * 0.65 || rect.height > window.innerHeight * 0.75) continue;
      if (!inLegacyZone(rect)) continue;
      if (hasLargeGameCanvas(el)) continue;

      hideNode(el);
    }
  };

  const findNativeChatInput = () => {
    const inputs = [...document.querySelectorAll('input[type="text"], input:not([type]), textarea')]
      .filter(el => !el.closest(`#${HUD_ID}`) && !el.disabled && !el.readOnly);
    if (!inputs.length) return null;
    return inputs
      .map(el => ({ el, rect: el.getBoundingClientRect(), ph: String(el.getAttribute('placeholder') || '') }))
      .filter(x => x.rect.width > 70 || /chat|chatear|parler/i.test(x.ph))
      .sort((a, b) => b.rect.bottom - a.rect.bottom)[0]?.el || null;
  };

  const setNativeValue = (input, value) => {
    const proto = input instanceof HTMLTextAreaElement ? HTMLTextAreaElement.prototype : HTMLInputElement.prototype;
    const setter = Object.getOwnPropertyDescriptor(proto, 'value')?.set;
    if (setter) setter.call(input, value);
    else input.value = value;
    input.dispatchEvent(new Event('input', { bubbles: true }));
    input.dispatchEvent(new Event('change', { bubbles: true }));
  };

  const sendToNativeChat = text => {
    const native = findNativeChatInput();
    if (!native) return false;
    native.focus();
    setNativeValue(native, text);
    native.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', code: 'Enter', keyCode: 13, which: 13, bubbles: true, cancelable: true }));
    native.dispatchEvent(new KeyboardEvent('keyup', { key: 'Enter', code: 'Enter', keyCode: 13, which: 13, bubbles: true, cancelable: true }));
    return true;
  };

  const focusChat = text => {
    const input = document.querySelector('#prhud-chat-input');
    if (input) {
      input.value = text || '';
      input.focus();
      if (text) input.select?.();
    }
  };

  const runAction = item => {
    if (!item) return;
    if (item.command) return focusChat(item.command);
    return focusChat('');
  };

  const build = raw => {
    const data = {
      ...DEFAULT_DATA,
      ...raw,
      health: { ...DEFAULT_DATA.health, ...(raw?.health || {}) },
      energy: { ...DEFAULT_DATA.energy, ...(raw?.energy || {}) },
      money: { ...DEFAULT_DATA.money, ...(raw?.money || {}) }
    };

    const avatar = data.avatar_url
      ? `<img src="${safe(data.avatar_url)}" alt="${safe(data.username)}" draggable="false" onerror="this.style.display='none'">`
      : `<div class="prhud-avatar-fallback">${safe(String(data.username || 'P').slice(0, 1).toUpperCase())}</div>`;

    const dock = dockItems.map(item => `
      <button class="prhud-dock-btn ${item.key === 'home' ? 'is-home is-active' : ''}" data-pr-action="${safe(item.key)}" type="button">
        ${icon(item.icon)}
        <small>${safe(item.label)}</small>
      </button>`).join('');

    const rail = railItems.map((item, index) => `
      <button class="prhud-rail-btn ${index === 0 ? 'is-active' : ''}" data-pr-rail="${safe(item.key)}" type="button">
        ${icon(item.icon)}
        <small>${safe(item.label)}</small>
      </button>`).join('');

    return `
      <section class="prhud-panel prhud-player">
        <div class="prhud-player-glow"></div>
        <div class="prhud-avatar">${avatar}<b>${safe(data.level)}</b></div>
        <div class="prhud-player-main">
          <strong>${safe(data.username)}</strong>
          <span>${safe(data.role)}</span>
          <div class="prhud-stat red">${icon('heart')}<em><u style="width:${pct(data.health.current, data.health.max)}"></u></em><small>${safe(data.health.current)} / ${safe(data.health.max)}</small></div>
          <div class="prhud-stat blue">${icon('bolt')}<em><u style="width:${pct(data.energy.current, data.energy.max)}"></u></em><small>${safe(data.energy.current)} / ${safe(data.energy.max)}</small></div>
        </div>
        <div class="prhud-player-money"><span>${icon('credit')} $ ${fmt(data.money.cash ?? data.money.credits)}</span><span>${icon('gem')} ${fmt(data.money.diamonds ?? data.money.pixels)}</span></div>
      </section>

      <div class="prhud-meta">
        <div class="prhud-panel prhud-chip">${icon('clock')}<span>${safe(data.time)}</span></div>
        <div class="prhud-panel prhud-chip">${icon('pin')}<span>${safe(data.city)}</span></div>
      </div>

      <section class="prhud-money">
        <div class="prhud-panel prhud-money-card credits">${icon('credit')}<strong>${fmt(data.money.credits)}</strong><span>Crédits</span><button type="button">+</button></div>
        <div class="prhud-panel prhud-money-card pixels"><i>H</i><strong>${fmt(data.money.pixels)}</strong><span>Pixels</span><button type="button">+</button></div>
        <button class="prhud-panel prhud-menu" type="button">☰</button>
      </section>

      <nav class="prhud-panel prhud-rail">${rail}</nav>

      <section class="prhud-panel prhud-quests">
        <h3>${icon('star')} <span>Quêtes quotidiennes</span></h3>
        <p>${icon('user')}<span>Se connecter</span><b>1/1</b></p>
        <p>${icon('group')}<span>Parler à 3 joueurs</span><b>2/3</b></p>
        <p>${icon('briefcase')}<span>Travailler 30 min</span><b>0/30</b></p>
      </section>

      <form class="prhud-panel prhud-chat" id="prhud-chat-form" autocomplete="off">
        <button class="prhud-chat-bubble" type="button">${icon('chat')}</button>
        <select class="prhud-chat-channel"><option>Discussion générale</option><option>Chuchoter</option><option>Crier</option></select>
        <input id="prhud-chat-input" class="prhud-chat-input" type="text" placeholder="Clique ici pour chatter..." autocomplete="off">
        <button class="prhud-chat-emoji" type="button">☺</button>
        <button class="prhud-chat-send" type="submit">${icon('send')}</button>
      </form>

      <nav class="prhud-panel prhud-dock">${dock}</nav>
    `;
  };

  const hydrate = hud => {
    hud.querySelectorAll('[data-pr-action]').forEach(btn => {
      const item = dockItems.find(x => x.key === btn.dataset.prAction);
      btn.addEventListener('click', () => runAction(item));
    });
    hud.querySelectorAll('[data-pr-rail]').forEach(btn => {
      const item = railItems.find(x => x.key === btn.dataset.prRail);
      btn.addEventListener('click', () => runAction(item));
    });

    const form = hud.querySelector('#prhud-chat-form');
    const input = hud.querySelector('#prhud-chat-input');
    form?.addEventListener('submit', event => {
      event.preventDefault();
      const text = input?.value?.trim() || '';
      if (!text) return;
      if (sendToNativeChat(text)) input.value = '';
    });
  };

  const loadData = async () => {
    try {
      const res = await fetch(`${DATA_URL}?_=${Date.now()}`, { credentials: 'include', cache: 'no-store' });
      if (!res.ok) throw new Error(`HUD ${res.status}`);
      return await res.json();
    } catch (_) {
      return DEFAULT_DATA;
    }
  };

  const bringToFront = () => {
    const hud = document.getElementById(HUD_ID);
    if (hud) document.body.appendChild(hud);
  };

  const mount = async () => {
    ensureCss();
    removeOldMasks();
    document.getElementById(HUD_ID)?.remove();

    const hud = document.createElement('div');
    hud.id = HUD_ID;
    hud.dataset.version = VERSION;
    hud.innerHTML = build(DEFAULT_DATA);
    document.body.appendChild(hud);
    hydrate(hud);
    bringToFront();
    killLegacyHud();

    const data = await loadData();
    if (!document.body.contains(hud)) return;
    hud.innerHTML = build(data && typeof data === 'object' ? data : DEFAULT_DATA);
    hydrate(hud);
    bringToFront();
    killLegacyHud();

    if (window.__paradiseHudCleaner) clearInterval(window.__paradiseHudCleaner);
    window.__paradiseHudCleaner = setInterval(() => {
      removeOldMasks();
      bringToFront();
      killLegacyHud();
    }, 180);
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', () => setTimeout(mount, 150));
  else setTimeout(mount, 150);
})();
