(() => {
  'use strict';

  const VERSION = '6.0.0';
  const HUD_ID = 'paradise-rp-hud';
  const STYLE_ID = 'paradise-rp-hud-css';
  const DATA_URL = '../rp-hud-data.php';
  const CSS_URL = './paradise-rp-hud.css?v=6';

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
    time: '17:16'
  };

  const dockItems = [
    { key: 'player', label: 'Joueur', icon: '♟', command: '' },
    { key: 'phone', label: 'Téléphone', icon: '▯', command: ':tel' },
    { key: 'id', label: 'Carte ID', icon: '▤', command: ':id' },
    { key: 'job', label: 'Métier', icon: '▣', command: ':trabajar' },
    { key: 'home', label: 'Accueil', icon: '⌂', command: '' },
    { key: 'shop', label: 'Boutique', icon: '⌑', command: '' },
    { key: 'bag', label: 'Inventaire', icon: '◰', command: '' },
    { key: 'cmd', label: 'Commandes', icon: '>_', command: ':commands' }
  ];

  const railItems = [
    { key: 'home', label: 'Accueil', icon: '⌂', command: '' },
    { key: 'bag', label: 'Inventaire', icon: '□', command: '' },
    { key: 'job', label: 'Métier', icon: '▤', command: ':trabajar' },
    { key: 'map', label: 'Carte', icon: '⌖', command: '' },
    { key: 'shop', label: 'Boutique', icon: '▦', command: '' }
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
      if (!link.href.includes('v=6')) link.href = CSS_URL;
      return;
    }
    link = document.createElement('link');
    link.id = STYLE_ID;
    link.rel = 'stylesheet';
    link.href = CSS_URL;
    document.head.appendChild(link);
  };

  const isHudNode = el => !!(el && (el.closest?.(`#${HUD_ID}`) || el.closest?.('#paradise-loader')));

  const hideLegacyElement = el => {
    if (!el || isHudNode(el) || el.id === 'root' || el.tagName === 'CANVAS') return;
    if (el.querySelector?.('canvas')) return;
    el.classList?.add('prhud-legacy-hidden');
    el.style.setProperty('opacity', '0', 'important');
    el.style.setProperty('visibility', 'hidden', 'important');
    el.style.setProperty('pointer-events', 'none', 'important');
  };

  const shouldHideLegacy = el => {
    if (!el || isHudNode(el) || el.id === HUD_ID || el.id === 'root' || el.tagName === 'CANVAS') return false;
    if (el.querySelector?.('canvas')) return false;

    const rect = el.getBoundingClientRect?.();
    if (!rect || rect.width <= 3 || rect.height <= 3) return false;

    const vw = window.innerWidth;
    const vh = window.innerHeight;
    if (rect.width > vw * 0.55 || rect.height > vh * 0.55) return false;

    const style = getComputedStyle(el);
    if (style.display === 'none' || Number(style.opacity || 1) <= 0.02) return false;

    const topLeftHud = rect.left < 330 && rect.top < 155 && rect.width < 330 && rect.height < 155;
    const leftIconStack = rect.left < 52 && rect.top > 70 && rect.top < vh - 90 && rect.width < 58 && rect.height < 420;
    const leftRailHud = rect.left < 112 && rect.top > 150 && rect.top < 560 && rect.width < 125 && rect.height < 410;
    const bottomNativeChat = rect.bottom > vh - 92 && rect.left < 450 && rect.width < 470 && rect.height < 94;
    const bottomPhone = rect.right > vw - 75 && rect.bottom > vh - 105 && rect.width < 90 && rect.height < 105;
    const oldCurrencyTopLeft = rect.left < 190 && rect.top < 132 && rect.width < 190 && rect.height < 80;

    return topLeftHud || leftIconStack || leftRailHud || bottomNativeChat || bottomPhone || oldCurrencyTopLeft;
  };

  const hideLegacyHud = () => {
    try {
      document.querySelectorAll('body *').forEach(el => {
        if (shouldHideLegacy(el)) hideLegacyElement(el);
      });
    } catch (_) {}
  };

  const startLegacyCleaner = () => {
    hideLegacyHud();
    let runs = 0;
    const timer = setInterval(() => {
      runs += 1;
      hideLegacyHud();
      if (runs > 80) clearInterval(timer);
    }, 250);

    const observer = new MutationObserver(hideLegacyHud);
    observer.observe(document.body, { childList: true, subtree: true, attributes: true });
    setTimeout(() => observer.disconnect(), 30000);
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
    if (text) {
      const native = findNativeChatInput();
      if (native) {
        native.focus();
        setNativeValue(native, text);
      }
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
      ? `<img src="${safe(data.avatar_url)}" alt="${safe(data.username)}" draggable="false">`
      : `<div class="prhud-avatar-fallback">${safe(String(data.username || 'P').slice(0, 1).toUpperCase())}</div>`;

    const dock = dockItems.map(item => `
      <button class="prhud-dock-btn ${item.key === 'home' ? 'is-home is-active' : ''}" data-pr-action="${safe(item.key)}" type="button" aria-label="${safe(item.label)}">
        <span class="prhud-dock-icon">${safe(item.icon)}</span>
        <small>${safe(item.label)}</small>
      </button>`).join('');

    const rail = railItems.map((item, index) => `
      <button class="prhud-rail-btn ${index === 0 ? 'is-active' : ''}" data-pr-rail="${safe(item.key)}" type="button" aria-label="${safe(item.label)}">
        <span>${safe(item.icon)}</span>
        <small>${safe(item.label)}</small>
      </button>`).join('');

    return `
      <div class="prhud-mask prhud-mask-top-left" aria-hidden="true"></div>
      <div class="prhud-mask prhud-mask-left-edge" aria-hidden="true"></div>
      <div class="prhud-mask prhud-mask-bottom-right" aria-hidden="true"></div>

      <section class="prhud-panel prhud-player" aria-label="Joueur">
        <div class="prhud-avatar">${avatar}<b>${safe(data.level)}</b></div>
        <div class="prhud-player-main">
          <strong>${safe(data.username)}</strong>
          <span>${safe(data.role)}</span>
          <div class="prhud-stat red"><i>❤</i><em><u style="width:${pct(data.health.current, data.health.max)}"></u></em><small>${safe(data.health.current)} / ${safe(data.health.max)}</small></div>
          <div class="prhud-stat blue"><i>ϟ</i><em><u style="width:${pct(data.energy.current, data.energy.max)}"></u></em><small>${safe(data.energy.current)} / ${safe(data.energy.max)}</small></div>
        </div>
        <div class="prhud-player-money"><span>$ ${fmt(data.money.cash ?? data.money.credits)}</span><span>♦ ${fmt(data.money.diamonds ?? data.money.pixels)}</span></div>
      </section>

      <div class="prhud-meta">
        <div class="prhud-panel prhud-chip">◷ ${safe(data.time)}</div>
        <div class="prhud-panel prhud-chip">⌖ ${safe(data.city)}</div>
      </div>

      <section class="prhud-money" aria-label="Monnaies">
        <div class="prhud-panel prhud-money-card credits"><i>▰</i><strong>${fmt(data.money.credits)}</strong><span>Crédits</span><button type="button">+</button></div>
        <div class="prhud-panel prhud-money-card pixels"><i>H</i><strong>${fmt(data.money.pixels)}</strong><span>Pixels</span><button type="button">+</button></div>
        <button class="prhud-panel prhud-menu" type="button" aria-label="Menu">☰</button>
      </section>

      <nav class="prhud-panel prhud-rail" aria-label="Navigation gauche">${rail}</nav>

      <section class="prhud-panel prhud-quests" aria-label="Quêtes quotidiennes">
        <h3>Quêtes quotidiennes</h3>
        <p><span>Se connecter</span><b>1/1</b></p>
        <p><span>Parler à 3 joueurs</span><b>2/3</b></p>
        <p><span>Travailler 30 min</span><b>0/30</b></p>
      </section>

      <form class="prhud-panel prhud-chat" id="prhud-chat-form" autocomplete="off">
        <button class="prhud-chat-bubble" type="button" aria-label="Chat">●</button>
        <select class="prhud-chat-channel" aria-label="Canal"><option>Discussion générale</option><option>Chuchoter</option><option>Crier</option></select>
        <input id="prhud-chat-input" class="prhud-chat-input" type="text" placeholder="Clique ici pour chatter..." autocomplete="off">
        <button class="prhud-chat-emoji" type="button" aria-label="Emoji">☺</button>
        <button class="prhud-chat-send" type="submit" aria-label="Envoyer">›</button>
      </form>

      <nav class="prhud-panel prhud-dock" aria-label="Barre principale RP">${dock}</nav>
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
    document.getElementById(HUD_ID)?.remove();
    const hud = document.createElement('div');
    hud.id = HUD_ID;
    hud.dataset.version = VERSION;
    hud.innerHTML = build(DEFAULT_DATA);
    document.body.appendChild(hud);
    hydrate(hud);
    startLegacyCleaner();

    const data = await loadData();
    if (!document.body.contains(hud)) return;
    hud.innerHTML = build(data && typeof data === 'object' ? data : DEFAULT_DATA);
    hydrate(hud);
    startLegacyCleaner();
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', () => setTimeout(mount, 250));
  else setTimeout(mount, 250);
})();
