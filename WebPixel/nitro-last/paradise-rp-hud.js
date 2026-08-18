(() => {
  'use strict';

  const VERSION = '9.0.0';
  const HUD_ID = 'paradise-rp-hud';
  const MASK_ID = 'paradise-rp-hard-masks';
  const STYLE_ID = 'paradise-rp-hud-css';
  const DATA_URL = '../rp-hud-data.php';
  const CSS_URL = './paradise-rp-hud.css?v=9';

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
    time: '17:38'
  };

  const dockItems = [
    { key: 'player', label: 'Joueur', icon: '♟', command: '' },
    { key: 'phone', label: 'Téléphone', icon: '▯', command: ':tel' },
    { key: 'id', label: 'Carte ID', icon: '▤', command: ':id' },
    { key: 'job', label: 'Métier', icon: '▣', command: ':trabajar' },
    { key: 'home', label: 'Accueil', icon: '⌂', command: '' },
    { key: 'shop', label: 'Boutique', icon: '▥', command: '' },
    { key: 'bag', label: 'Inventaire', icon: '▢', command: '' },
    { key: 'cmd', label: 'Commandes', icon: '>_', command: ':commands' }
  ];

  const railItems = [
    { key: 'home', label: 'Accueil', icon: '⌂', command: '' },
    { key: 'bag', label: 'Inventaire', icon: '▢', command: '' },
    { key: 'job', label: 'Métier', icon: '▤', command: ':trabajar' },
    { key: 'map', label: 'Carte', icon: '⌖', command: '' },
    { key: 'shop', label: 'Boutique', icon: '▥', command: '' }
  ];

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
      if (!link.href.includes('v=9')) link.href = CSS_URL;
      return;
    }
    link = document.createElement('link');
    link.id = STYLE_ID;
    link.rel = 'stylesheet';
    link.href = CSS_URL;
    document.head.appendChild(link);
  };

  const ensureMasks = () => {
    let masks = document.getElementById(MASK_ID);
    if (!masks) {
      masks = document.createElement('div');
      masks.id = MASK_ID;
      masks.setAttribute('aria-hidden', 'true');
      masks.innerHTML = `
        <i class="pr-mask pr-mask-top-left"></i>
        <i class="pr-mask pr-mask-left-icons"></i>
        <i class="pr-mask pr-mask-left-label"></i>
        <i class="pr-mask pr-mask-bottom-left"></i>
        <i class="pr-mask pr-mask-bottom-right"></i>
        <i class="pr-mask pr-mask-top-right"></i>`;
      document.body.appendChild(masks);
    }
    return masks;
  };

  const bringToFront = () => {
    const masks = document.getElementById(MASK_ID);
    const hud = document.getElementById(HUD_ID);
    if (masks) document.body.appendChild(masks);
    if (hud) document.body.appendChild(hud);
  };

  const isOwnElement = el => {
    if (!el || el === document.documentElement || el === document.body) return true;
    if (el.closest(`#${HUD_ID}, #${MASK_ID}, #paradise-loader`)) return true;
    if (['SCRIPT', 'STYLE', 'LINK', 'META', 'TITLE', 'BASE', 'NOSCRIPT'].includes(el.tagName)) return true;
    return false;
  };

  const inLegacyZone = rect => {
    const vw = window.innerWidth;
    const vh = window.innerHeight;
    if (rect.left < 350 && rect.top < 165) return true;
    if (rect.left < 118 && rect.top > 80 && rect.top < vh - 90) return true;
    if (rect.left < 270 && rect.bottom > vh - 285 && rect.bottom < vh - 85) return true;
    if (rect.left < 520 && rect.bottom > vh - 130) return true;
    if (rect.right > vw - 90 && rect.bottom > vh - 95) return true;
    if (rect.right > vw - 295 && rect.top < 95) return true;
    return false;
  };

  const killLegacyDomInZones = () => {
    const nodes = document.querySelectorAll('body *');
    nodes.forEach(el => {
      if (isOwnElement(el)) return;
      if (el.id === 'root' || el.tagName === 'CANVAS') return;
      if (el.querySelector && el.querySelector('canvas')) return;
      const rect = el.getBoundingClientRect();
      if (!rect || rect.width <= 2 || rect.height <= 2) return;
      if (rect.width > window.innerWidth * 0.82 || rect.height > window.innerHeight * 0.82) return;
      if (!inLegacyZone(rect)) return;
      el.classList.add('prhud-native-kill');
      el.setAttribute('data-prhud-killed', '1');
    });
  };

  const findNativeChatInput = () => {
    const inputs = [...document.querySelectorAll('input[type="text"], input:not([type]), textarea')]
      .filter(el => !el.closest(`#${HUD_ID}`) && !el.disabled && !el.readOnly && !el.classList.contains('prhud-native-kill'));
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
        <span class="prhud-dock-icon">${safe(item.icon)}</span>
        <small>${safe(item.label)}</small>
      </button>`).join('');

    const rail = railItems.map((item, index) => `
      <button class="prhud-rail-btn ${index === 0 ? 'is-active' : ''}" data-pr-rail="${safe(item.key)}" type="button">
        <span>${safe(item.icon)}</span>
        <small>${safe(item.label)}</small>
      </button>`).join('');

    return `
      <section class="prhud-panel prhud-player">
        <div class="prhud-avatar">${avatar}<b>${safe(data.level)}</b></div>
        <div class="prhud-player-main">
          <strong>${safe(data.username)}</strong>
          <span>${safe(data.role)}</span>
          <div class="prhud-stat red"><i>❤</i><em><u style="width:${pct(data.health.current, data.health.max)}"></u></em><small>${safe(data.health.current)} / ${safe(data.health.max)}</small></div>
          <div class="prhud-stat blue"><i>ϟ</i><em><u style="width:${pct(data.energy.current, data.energy.max)}"></u></em><small>${safe(data.energy.current)} / ${safe(data.energy.max)}</small></div>
        </div>
        <div class="prhud-player-money"><span>$ ${fmt(data.money.cash ?? data.money.credits)}</span><span>◆ ${fmt(data.money.diamonds ?? data.money.pixels)}</span></div>
      </section>

      <div class="prhud-meta">
        <div class="prhud-panel prhud-chip">◷ ${safe(data.time)}</div>
        <div class="prhud-panel prhud-chip">✦ ${safe(data.city)}</div>
      </div>

      <section class="prhud-money">
        <div class="prhud-panel prhud-money-card credits"><i>▰</i><strong>${fmt(data.money.credits)}</strong><span>Crédits</span><button type="button">+</button></div>
        <div class="prhud-panel prhud-money-card pixels"><i>H</i><strong>${fmt(data.money.pixels)}</strong><span>Pixels</span><button type="button">+</button></div>
        <button class="prhud-panel prhud-menu" type="button">☰</button>
      </section>

      <nav class="prhud-panel prhud-rail">${rail}</nav>

      <section class="prhud-panel prhud-quests">
        <h3>Quêtes quotidiennes</h3>
        <p><span>Se connecter</span><b>1/1</b></p>
        <p><span>Parler à 3 joueurs</span><b>2/3</b></p>
        <p><span>Travailler 30 min</span><b>0/30</b></p>
      </section>

      <form class="prhud-panel prhud-chat" id="prhud-chat-form" autocomplete="off">
        <button class="prhud-chat-bubble" type="button">●</button>
        <select class="prhud-chat-channel"><option>Discussion générale</option><option>Chuchoter</option><option>Crier</option></select>
        <input id="prhud-chat-input" class="prhud-chat-input" type="text" placeholder="Clique ici pour chatter..." autocomplete="off">
        <button class="prhud-chat-emoji" type="button">☺</button>
        <button class="prhud-chat-send" type="submit">›</button>
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

  const mount = async () => {
    ensureCss();
    ensureMasks();
    document.getElementById(HUD_ID)?.remove();

    const hud = document.createElement('div');
    hud.id = HUD_ID;
    hud.dataset.version = VERSION;
    hud.innerHTML = build(DEFAULT_DATA);
    document.body.appendChild(hud);
    hydrate(hud);
    bringToFront();
    killLegacyDomInZones();

    const data = await loadData();
    if (!document.body.contains(hud)) return;
    hud.innerHTML = build(data && typeof data === 'object' ? data : DEFAULT_DATA);
    hydrate(hud);
    bringToFront();
    killLegacyDomInZones();

    if (window.__paradiseHudCleaner) clearInterval(window.__paradiseHudCleaner);
    window.__paradiseHudCleaner = setInterval(() => {
      ensureMasks();
      bringToFront();
      killLegacyDomInZones();
    }, 250);
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', () => setTimeout(mount, 150));
  else setTimeout(mount, 150);
})();