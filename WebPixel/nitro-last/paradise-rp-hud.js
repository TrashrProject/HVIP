(() => {
  'use strict';

  const VERSION = '21.0.0';
  const HUD_ID = 'paradise-rp-hud';
  const STYLE_ID = 'paradise-rp-hud-css';
  const DATA_URL = '../rp-hud-data.php';
  const CSS_URL = './paradise-rp-hud.css?v=21';

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
    time: ''
  };

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
  const currentTime = () => new Date().toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' });

  const svg = {
    user: '<svg viewBox="0 0 24 24"><path d="M12 12.2a4.2 4.2 0 1 0 0-8.4 4.2 4.2 0 0 0 0 8.4Z"/><path d="M4.7 20.2c.8-4.1 3.2-6.1 7.3-6.1s6.5 2 7.3 6.1"/></svg>',
    phone: '<svg viewBox="0 0 24 24"><rect x="7" y="2.7" width="10" height="18.6" rx="2.2"/><path d="M10.4 5h3.2M11 18.1h2"/></svg>',
    id: '<svg viewBox="0 0 24 24"><rect x="3" y="5" width="18" height="14" rx="2"/><path d="M7 10h4M7 14h5.5M15 10h2.7M15 14h2.7"/></svg>',
    briefcase: '<svg viewBox="0 0 24 24"><rect x="3" y="7" width="18" height="12" rx="2"/><path d="M9 7V5.3c0-.8.5-1.3 1.3-1.3h3.4c.8 0 1.3.5 1.3 1.3V7M3 12h18M12 11v2"/></svg>',
    home: '<svg viewBox="0 0 24 24"><path d="M3.8 11.2 12 4.2l8.2 7"/><path d="M6.2 10.4v9h11.6v-9"/><path d="M10 19.4v-5.1h4v5.1"/></svg>',
    cart: '<svg viewBox="0 0 24 24"><path d="M4 5h2l1.5 9.3h9.7l2-6.4H7"/><circle cx="10" cy="19" r="1.3"/><circle cx="17" cy="19" r="1.3"/></svg>',
    bag: '<svg viewBox="0 0 24 24"><path d="M6.2 8.2h11.6l1 12H5.2l1-12Z"/><path d="M9 8.2V6a3 3 0 0 1 6 0v2.2"/></svg>',
    terminal: '<svg viewBox="0 0 24 24"><path d="m5 7 5 5-5 5M12 17h7"/></svg>',
    pin: '<svg viewBox="0 0 24 24"><path d="M12 21s6-5.3 6-11a6 6 0 0 0-12 0c0 5.7 6 11 6 11Z"/><circle cx="12" cy="10" r="2.2"/></svg>',
    clock: '<svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="8.5"/><path d="M12 7.5v5l3.6 2"/></svg>',
    heart: '<svg viewBox="0 0 24 24"><path d="M12 20s-7.4-4.3-8.6-9.2C2.6 7.4 4.6 5 7.4 5c1.7 0 3 1 4.6 2.8C13.6 6 14.9 5 16.6 5c2.8 0 4.8 2.4 4 5.8C19.4 15.7 12 20 12 20Z"/></svg>',
    bolt: '<svg viewBox="0 0 24 24"><path d="m13.2 2.8-8 11.1h6.2L10.8 21l8-11.1h-6.2l.6-7.1Z"/></svg>',
    gem: '<svg viewBox="0 0 24 24"><path d="M6.8 4h10.4l3.3 4.7L12 20 3.5 8.7 6.8 4Z"/><path d="M4 8.7h16M8.5 4 12 20 15.5 4"/></svg>',
    credit: '<svg viewBox="0 0 24 24"><rect x="3.8" y="6" width="16.4" height="12" rx="2"/><path d="M3.8 10h16.4"/></svg>',
    star: '<svg viewBox="0 0 24 24"><path d="m12 3 2.5 5.3 5.8.8-4.2 4.1 1 5.8-5.1-2.8L6.9 19l1-5.8-4.2-4.1 5.8-.8L12 3Z"/></svg>',
    group: '<svg viewBox="0 0 24 24"><path d="M9.7 11.3a3.6 3.6 0 1 0 0-7.2 3.6 3.6 0 0 0 0 7.2Z"/><path d="M3.8 20c.7-4.4 2.7-6.5 5.9-6.5s5.2 2.1 5.9 6.5"/><path d="M16.3 12.2a3 3 0 1 0-1.1-5.8M15.8 14c2.2.3 3.7 2 4.4 5"/></svg>',
    chat: '<svg viewBox="0 0 24 24"><path d="M4 6.5A3.5 3.5 0 0 1 7.5 3h9A3.5 3.5 0 0 1 20 6.5v4A3.5 3.5 0 0 1 16.5 14H11l-4.2 4.2V14A3.5 3.5 0 0 1 4 10.5v-4Z"/></svg>',
    send: '<svg viewBox="0 0 24 24"><path d="M3.5 20.5 21 12 3.5 3.5 6.4 11H14l-7.6 2-2.9 7.5Z"/></svg>'
  };

  const icon = name => `<span class="prhud-icon prhud-icon-${safe(name)}">${svg[name] || svg.home}</span>`;

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

  const ensureCss = () => {
    let link = document.getElementById(STYLE_ID);
    if (!link) {
      link = document.createElement('link');
      link.id = STYLE_ID;
      link.rel = 'stylesheet';
      document.head.appendChild(link);
    }
    if (!String(link.getAttribute('href') || '').includes('v=21')) link.href = CSS_URL;
  };

  const getAvatarUrl = data => {
    if (data.avatar_url) return String(data.avatar_url);
    if (data.look) return `../avatar-image.php?figure=${encodeURIComponent(data.look)}&direction=2&head_direction=2&gesture=sml&size=l`;
    return '';
  };

  const nativeInputs = () => [...document.querySelectorAll('input[type="text"], input:not([type]), textarea')]
    .filter(el => !el.closest(`#${HUD_ID}`) && !el.disabled && !el.readOnly);

  const sendToNativeChat = text => {
    const input = nativeInputs().sort((a, b) => b.getBoundingClientRect().bottom - a.getBoundingClientRect().bottom)[0];
    if (!input) return false;
    const proto = input instanceof HTMLTextAreaElement ? HTMLTextAreaElement.prototype : HTMLInputElement.prototype;
    const setter = Object.getOwnPropertyDescriptor(proto, 'value')?.set;
    input.focus();
    if (setter) setter.call(input, text); else input.value = text;
    input.dispatchEvent(new Event('input', { bubbles: true }));
    input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', code: 'Enter', keyCode: 13, which: 13, bubbles: true, cancelable: true }));
    input.dispatchEvent(new KeyboardEvent('keyup', { key: 'Enter', code: 'Enter', keyCode: 13, which: 13, bubbles: true, cancelable: true }));
    return true;
  };

  const focusChat = value => {
    const input = document.querySelector('#prhud-chat-input');
    if (!input) return;
    input.value = value || '';
    input.focus();
    if (value) input.select?.();
  };

  const runAction = item => {
    if (!item) return;
    focusChat(item.command || '');
  };

  const build = raw => {
    const data = {
      ...DEFAULT_DATA,
      ...(raw || {}),
      health: { ...DEFAULT_DATA.health, ...(raw?.health || {}) },
      energy: { ...DEFAULT_DATA.energy, ...(raw?.energy || {}) },
      money: { ...DEFAULT_DATA.money, ...(raw?.money || {}) }
    };
    const avatarUrl = getAvatarUrl(data);
    const time = data.time || currentTime();

    return `
      <section class="prhud-player prhud-panel" aria-label="Joueur">
        <span class="prhud-player-glow"></span>
        <div class="prhud-avatar">
          ${avatarUrl ? `<img src="${safe(avatarUrl)}" alt="${safe(data.username)}">` : `<span class="prhud-avatar-fallback">RP</span>`}
          <b>${safe(data.level || 1)}</b>
        </div>
        <div class="prhud-player-main">
          <strong>${safe(data.username)}</strong>
          <span>${safe(data.role || 'Citoyen')}</span>
          <div class="prhud-stat red">${icon('heart')}<em><u style="width:${pct(data.health.current, data.health.max)}"></u></em><small>${fmt(data.health.current)} / ${fmt(data.health.max)}</small></div>
          <div class="prhud-stat blue">${icon('bolt')}<em><u style="width:${pct(data.energy.current, data.energy.max)}"></u></em><small>${fmt(data.energy.current)} / ${fmt(data.energy.max)}</small></div>
        </div>
        <div class="prhud-player-money"><span>${icon('credit')}$ ${fmt(data.money.cash)}</span><span>${icon('gem')}${fmt(data.money.diamonds)}</span></div>
      </section>

      <section class="prhud-meta">
        <div class="prhud-chip prhud-panel">${icon('clock')}<b>${safe(time)}</b></div>
        <div class="prhud-chip prhud-panel">${icon('pin')}<b>${safe(data.city || 'Paradise City')}</b></div>
      </section>

      <section class="prhud-money">
        <div class="prhud-money-card prhud-panel credits">${icon('credit')}<strong>${fmt(data.money.credits)}</strong><span>Crédits</span><button type="button" data-prhud-action="credits">+</button></div>
        <div class="prhud-money-card prhud-panel pixels"><i>H</i><strong>${fmt(data.money.pixels)}</strong><span>Pixels</span><button type="button" data-prhud-action="pixels">+</button></div>
        <button class="prhud-menu" type="button" aria-label="Menu">≡</button>
      </section>

      <nav class="prhud-rail prhud-panel" aria-label="Navigation RP">
        ${railItems.map(item => `<button type="button" class="prhud-rail-btn ${item.key === 'home' ? 'is-active' : ''}" data-prhud-key="${safe(item.key)}">${icon(item.icon)}<small>${safe(item.label)}</small></button>`).join('')}
      </nav>

      <section class="prhud-quests prhud-panel" aria-label="Quêtes quotidiennes">
        <h3>${icon('star')}Quêtes quotidiennes</h3>
        <p>${icon('user')}<span>Se connecter</span><b>1/1</b></p>
        <p>${icon('group')}<span>Parler à 3 joueurs</span><b>2/3</b></p>
        <p>${icon('briefcase')}<span>Travailler 30 min</span><b>0/30</b></p>
      </section>

      <form class="prhud-chat prhud-panel" id="prhud-chat-form">
        <button class="prhud-chat-bubble" type="button" aria-label="Chat">${icon('chat')}</button>
        <select class="prhud-chat-channel" aria-label="Canal"><option>Discussion générale</option><option>Chuchoter</option><option>Crier</option></select>
        <input id="prhud-chat-input" class="prhud-chat-input" autocomplete="off" placeholder="Clique ici pour chatter...">
        <button class="prhud-chat-emoji" type="button" aria-label="Emote">☻</button>
        <button class="prhud-chat-send" type="submit" aria-label="Envoyer">${icon('send')}</button>
      </form>

      <nav class="prhud-dock prhud-panel" aria-label="Actions RP">
        ${dockItems.map(item => `<button type="button" class="prhud-dock-btn ${item.key === 'home' ? 'is-home' : ''}" data-prhud-key="${safe(item.key)}">${icon(item.icon)}<small>${safe(item.label)}</small></button>`).join('')}
      </nav>`;
  };

  const bind = root => {
    root.querySelectorAll('[data-prhud-key]').forEach(button => {
      button.addEventListener('click', () => {
        const key = button.getAttribute('data-prhud-key');
        const item = dockItems.find(x => x.key === key) || railItems.find(x => x.key === key);
        runAction(item);
      });
    });

    const form = root.querySelector('#prhud-chat-form');
    const input = root.querySelector('#prhud-chat-input');
    form?.addEventListener('submit', event => {
      event.preventDefault();
      const text = input?.value?.trim() || '';
      if (!text) return;
      sendToNativeChat(text);
      input.value = '';
      input.focus();
    });
  };

  let lastData = null;
  const render = data => {
    ensureCss();
    let root = document.getElementById(HUD_ID);
    if (!root) {
      root = document.createElement('div');
      root.id = HUD_ID;
      root.setAttribute('data-version', VERSION);
      document.body.appendChild(root);
    }
    root.innerHTML = build(data);
    bind(root);
  };

  const loadData = async () => {
    try {
      const response = await fetch(`${DATA_URL}?_=${Date.now()}`, { cache: 'no-store', credentials: 'same-origin' });
      if (!response.ok) throw new Error(String(response.status));
      const json = await response.json();
      lastData = json && typeof json === 'object' ? json : DEFAULT_DATA;
    } catch (_) {
      lastData = lastData || DEFAULT_DATA;
    }
    render(lastData);
  };

  const boot = () => {
    ensureCss();
    render(DEFAULT_DATA);
    loadData();
    setInterval(loadData, 5000);
    setInterval(() => {
      try { window.__paradiseNativeUiOffScan?.(); } catch (_) {}
    }, 1500);
  };

  try {
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
    else boot();
  } catch (error) {
    console.error('[ParadiseRP HUD] boot failed', error);
  }
})();
