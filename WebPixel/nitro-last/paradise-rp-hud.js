(() => {
  'use strict';

  const VERSION = '2.0.0';
  const HUD_ID = 'paradise-rp-hud';
  const STYLE_ID = 'paradise-rp-hud-css';
  const DATA_URL = '../rp-hud-data.php';
  const CSS_URL = './paradise-rp-hud.css?v=2';

  const DEFAULT_DATA = {
    ok: false,
    username: 'ParadiseRP',
    role: 'Citoyen',
    level: 7,
    look: '',
    avatar_url: '',
    health: { current: 315, max: 500 },
    energy: { current: 31, max: 100 },
    money: { credits: 319, pixels: 224 },
    city: 'Paradise City',
    time: '16:45'
  };

  const dockItems = [
    { key: 'player', label: 'Joueur', icon: '👤', command: '' },
    { key: 'phone', label: 'Téléphone', icon: '📱', command: ':tel' },
    { key: 'id', label: 'Carte ID', icon: '🪪', command: ':id' },
    { key: 'job', label: 'Métier', icon: '💼', command: ':trabajar' },
    { key: 'home', label: 'Accueil', icon: '⌂', command: '' },
    { key: 'shop', label: 'Boutique', icon: '🛒', command: '' },
    { key: 'bag', label: 'Inventaire', icon: '🎒', command: '' },
    { key: 'cmd', label: 'Commandes', icon: '>_', command: ':commands' }
  ];

  const railItems = [
    { key: 'home', label: 'Accueil', icon: '⌂', command: '' },
    { key: 'bag', label: 'Inventaire', icon: '▣', command: '' },
    { key: 'job', label: 'Métier', icon: '▤', command: ':trabajar' },
    { key: 'map', label: 'Carte', icon: '⌖', command: '' },
    { key: 'shop', label: 'Boutique', icon: '▥', command: '' }
  ];

  const safe = (value) => String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;');

  const clamp = (value, min, max) => Math.max(min, Math.min(max, Number(value) || 0));
  const fmt = (value) => new Intl.NumberFormat('fr-FR').format(Number(value) || 0);
  const pct = (cur, max) => `${clamp((Number(cur) / Math.max(1, Number(max))) * 100, 0, 100)}%`;

  const ensureCss = () => {
    let link = document.getElementById(STYLE_ID);
    if (!link) {
      link = document.createElement('link');
      link.id = STYLE_ID;
      link.rel = 'stylesheet';
      document.head.appendChild(link);
    }
    link.href = CSS_URL;
  };

  const isInsideHud = (el) => !!el.closest?.(`#${HUD_ID}, #paradise-loader`);

  const hideElement = (el) => {
    if (!el || isInsideHud(el) || el.id === 'root' || el.tagName === 'CANVAS') return;
    el.classList?.add('prhud-native-hidden');
  };

  const hideBySelectors = () => {
    const root = document.getElementById('root');
    if (!root) return;

    const selectors = [
      '[class*="nitro-toolbar"]',
      '[class*="toolbar"]',
      '[class*="nitro-chat-input"]',
      '[class*="chat-input"]',
      '[class*="chat-widget"]',
      '[class*="avatar-info"]',
      '[class*="infostand"]',
      '[class*="purse"]',
      '[class*="currency"]',
      '[class*="wallet"]',
      '[class*="side-bar"]',
      '[class*="sidebar"]'
    ];

    selectors.forEach(selector => {
      root.querySelectorAll(selector).forEach(hideElement);
    });
  };

  const hideByScreenZones = () => {
    const root = document.getElementById('root');
    if (!root) return;

    const vw = window.innerWidth;
    const vh = window.innerHeight;
    const nodes = root.querySelectorAll('div, section, nav, aside, form, input, textarea, button, img, span');

    nodes.forEach(el => {
      if (isInsideHud(el) || el.id === 'root' || el.tagName === 'CANVAS') return;
      const rect = el.getBoundingClientRect();
      if (!rect || rect.width <= 2 || rect.height <= 2) return;
      if (rect.width > vw * 0.72 || rect.height > vh * 0.72) return;

      const style = window.getComputedStyle(el);
      const positioned = style.position === 'absolute' || style.position === 'fixed' || style.position === 'sticky';
      const looksInteractive = /button|input|textarea|select|img/i.test(el.tagName) || el.getAttribute('role') === 'button' || el.onclick || el.className;
      if (!positioned && !looksInteractive) return;

      const legacyTopLeft = rect.left < 325 && rect.top < 170 && rect.width < 340 && rect.height < 170;
      const legacyLeftRail = rect.left < 55 && rect.top > 85 && rect.bottom < vh - 80 && rect.width < 90;
      const legacyBottomLeft = rect.bottom > vh - 92 && rect.left < 430 && rect.width < 460 && rect.height < 95;
      const legacyBottomRight = rect.right > vw - 70 && rect.bottom > vh - 98 && rect.width < 96 && rect.height < 96;
      const legacyTinyEdge = rect.left < 26 && rect.width < 42 && rect.height < 70;

      if (legacyTopLeft || legacyLeftRail || legacyBottomLeft || legacyBottomRight || legacyTinyEdge) {
        hideElement(el);
        const parent = el.parentElement;
        if (parent && parent !== root && !isInsideHud(parent)) {
          const p = parent.getBoundingClientRect();
          if (p.width < 520 && p.height < 180) hideElement(parent);
        }
      }
    });
  };

  const hideLegacyHud = () => {
    try {
      hideBySelectors();
      hideByScreenZones();
    } catch (_) {}
  };

  const findNativeChatInput = () => {
    const inputs = [...document.querySelectorAll('input[type="text"], input:not([type]), textarea')]
      .filter(el => !el.closest(`#${HUD_ID}`) && !el.disabled && !el.readOnly);

    if (!inputs.length) return null;
    return inputs
      .map(el => ({ el, rect: el.getBoundingClientRect(), ph: String(el.getAttribute('placeholder') || '') }))
      .filter(x => (x.rect.width > 80 && x.rect.height > 15) || /chat|chatear|parler/i.test(x.ph))
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

  const sendToNativeChat = (text) => {
    const native = findNativeChatInput();
    if (!native) return false;

    native.focus();
    setNativeValue(native, text);
    native.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', code: 'Enter', keyCode: 13, which: 13, bubbles: true, cancelable: true }));
    native.dispatchEvent(new KeyboardEvent('keyup', { key: 'Enter', code: 'Enter', keyCode: 13, which: 13, bubbles: true, cancelable: true }));
    hideLegacyHud();
    return true;
  };

  const focusOverlayWith = (text = '') => {
    const overlayInput = document.querySelector('#prhud-chat-input');
    if (!overlayInput) return;
    overlayInput.value = text;
    overlayInput.focus();
    overlayInput.select?.();
  };

  const action = (item) => {
    if (!item) return;
    if (item.command) return focusOverlayWith(item.command);
    focusOverlayWith('');
  };

  const playerMarkup = (data) => {
    const avatar = data.avatar_url
      ? `<img src="${safe(data.avatar_url)}" alt="${safe(data.username)}" draggable="false">`
      : `<div class="prhud-avatar-fallback">${safe(String(data.username || 'P').charAt(0).toUpperCase())}</div>`;

    return `
      <section class="prhud-panel prhud-player" aria-label="Statut joueur">
        <div class="prhud-avatar">${avatar}<div class="prhud-level">${safe(data.level)}</div></div>
        <div class="prhud-player-main">
          <div class="prhud-player-name"><span>${safe(data.username)}</span><i></i></div>
          <div class="prhud-role">${safe(data.role)}</div>
          <div class="prhud-bars">
            <div class="prhud-stat"><b>❤</b><span><i style="width:${pct(data.health.current, data.health.max)}"></i></span><em>${safe(data.health.current)} / ${safe(data.health.max)}</em></div>
            <div class="prhud-stat energy"><b>ϟ</b><span><i style="width:${pct(data.energy.current, data.energy.max)}"></i></span><em>${safe(data.energy.current)} / ${safe(data.energy.max)}</em></div>
          </div>
        </div>
      </section>`;
  };

  const moneyMarkup = (data) => `
    <section class="prhud-money" aria-label="Monnaies">
      <div class="prhud-panel prhud-money-card credits"><span class="prhud-money-ico">▰</span><strong>${fmt(data.money.credits)}</strong><small>Crédits</small><button type="button">+</button></div>
      <div class="prhud-panel prhud-money-card pixels"><span class="prhud-money-ico">H</span><strong>${fmt(data.money.pixels)}</strong><small>Pixels</small><button type="button">+</button></div>
      <button class="prhud-panel prhud-menu" type="button" aria-label="Menu">☰</button>
    </section>`;

  const dockMarkup = () => dockItems.map(item => `
    <button class="prhud-dock-btn ${item.key === 'home' ? 'home active' : ''}" data-pr-action="${safe(item.key)}" type="button" aria-label="${safe(item.label)}">
      <span class="prhud-dock-ico">${safe(item.icon)}</span><span>${safe(item.label)}</span>
    </button>`).join('');

  const railMarkup = () => railItems.map((item, index) => `
    <button class="prhud-rail-btn ${index === 0 ? 'active' : ''}" data-pr-rail="${safe(item.key)}" type="button" aria-label="${safe(item.label)}">
      <span>${safe(item.icon)}</span><small>${safe(item.label)}</small>
    </button>`).join('');

  const build = (data) => {
    const d = {
      ...DEFAULT_DATA,
      ...data,
      health: { ...DEFAULT_DATA.health, ...(data.health || {}) },
      energy: { ...DEFAULT_DATA.energy, ...(data.energy || {}) },
      money: { ...DEFAULT_DATA.money, ...(data.money || {}) }
    };

    return `
      ${playerMarkup(d)}
      <div class="prhud-topmeta"><div class="prhud-panel">${safe(d.time)} <span>☀</span></div><div class="prhud-panel"><span>⌖</span>${safe(d.city)}</div></div>
      ${moneyMarkup(d)}
      <nav class="prhud-panel prhud-rail" aria-label="Navigation rapide">${railMarkup()}</nav>
      <section class="prhud-panel prhud-quests" aria-label="Quêtes quotidiennes"><h3>Quêtes quotidiennes</h3><p><span>Se connecter</span><b>1/1</b></p><p><span>Parler à 3 joueurs</span><b>2/3</b></p><p><span>Travailler 30 min</span><b>0/30</b></p></section>
      <form class="prhud-panel prhud-chat" id="prhud-chat-form" autocomplete="off"><select aria-label="Canal"><option>Discussion générale</option><option>Chuchoter</option><option>Crier</option></select><input id="prhud-chat-input" type="text" placeholder="Clique ici pour chatter..." autocomplete="off"><button class="emoji" type="button" aria-label="Emoji">☺</button><button class="send" type="submit" aria-label="Envoyer">›</button></form>
      <nav class="prhud-panel prhud-dock" aria-label="Barre RP principale">${dockMarkup()}</nav>`;
  };

  const hydrate = (hud) => {
    hud.querySelectorAll('[data-pr-action]').forEach(btn => {
      const item = dockItems.find(x => x.key === btn.dataset.prAction);
      btn.addEventListener('click', () => action(item));
    });
    hud.querySelectorAll('[data-pr-rail]').forEach(btn => {
      const item = railItems.find(x => x.key === btn.dataset.prRail);
      btn.addEventListener('click', () => action(item));
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
      if (!res.ok) throw new Error(`HUD data ${res.status}`);
      return await res.json();
    } catch (error) {
      console.warn('[ParadiseHUD] fallback data', error);
      return DEFAULT_DATA;
    }
  };

  const remount = async () => {
    ensureCss();
    let hud = document.getElementById(HUD_ID);
    if (!hud) {
      hud = document.createElement('div');
      hud.id = HUD_ID;
      document.body.appendChild(hud);
    }
    hud.dataset.version = VERSION;
    hud.innerHTML = build(DEFAULT_DATA);
    hydrate(hud);
    hideLegacyHud();

    const data = await loadData();
    if (!document.body.contains(hud)) return;
    hud.innerHTML = build(data && typeof data === 'object' ? data : DEFAULT_DATA);
    hydrate(hud);
    hideLegacyHud();
  };

  const boot = () => {
    window.setTimeout(remount, 750);
    window.setInterval(hideLegacyHud, 350);
    new MutationObserver(hideLegacyHud).observe(document.body, { childList: true, subtree: true, attributes: true });
    window.addEventListener('resize', hideLegacyHud);
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();
