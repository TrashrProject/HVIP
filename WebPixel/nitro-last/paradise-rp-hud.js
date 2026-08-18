(() => {
  'use strict';

  const VERSION = '7.0.0';
  const HUD_ID = 'paradise-rp-hud';
  const STYLE_ID = 'paradise-rp-hud-css';
  const DATA_URL = '../rp-hud-data.php';
  const CSS_URL = './paradise-rp-hud.css?v=7';

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
    time: '17:22'
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
      link.href = CSS_URL;
      return;
    }
    link = document.createElement('link');
    link.id = STYLE_ID;
    link.rel = 'stylesheet';
    link.href = CSS_URL;
    document.head.appendChild(link);
  };

  const isProtected = el => {
    if (!el || el === document.body || el === document.documentElement) return true;
    if (el.closest?.(`#${HUD_ID}, #paradise-loader`)) return true;
    if (el.tagName === 'CANVAS') return true;
    if (el.querySelector?.('canvas')) return true;
    return false;
  };

  const hideLegacyElement = el => {
    if (isProtected(el)) return;
    el.classList?.add('prhud-legacy-hidden');
    el.style.setProperty('opacity', '0', 'important');
    el.style.setProperty('visibility', 'hidden', 'important');
    el.style.setProperty('pointer-events', 'none', 'important');
  };

  const inZone = (rect, zone) => {
    if (!rect) return false;
    return rect.right > zone.x1 && rect.left < zone.x2 && rect.bottom > zone.y1 && rect.top < zone.y2;
  };

  const zones = () => {
    const w = window.innerWidth;
    const h = window.innerHeight;
    return [
      { name: 'old-player', x1: 0, y1: 0, x2: 335, y2: 156 },
      { name: 'old-edge-icons', x1: 0, y1: 72, x2: 54, y2: Math.max(430, h - 92) },
      { name: 'old-left-rail', x1: 0, y1: 150, x2: 122, y2: 560 },
      { name: 'old-bottom-chat', x1: 0, y1: h - 105, x2: 500, y2: h },
      { name: 'old-phone', x1: w - 85, y1: h - 95, x2: w, y2: h },
      { name: 'old-top-right-text', x1: w - 260, y1: 0, x2: w, y2: 80 }
    ];
  };

  const sweepLegacyUi = () => {
    const allZones = zones();

    // 1) Cache les éléments trouvés directement sous les zones historiques de Nitro.
    const samplePoints = [];
    allZones.forEach(z => {
      const stepX = Math.max(18, Math.floor((z.x2 - z.x1) / 5));
      const stepY = Math.max(18, Math.floor((z.y2 - z.y1) / 5));
      for (let x = z.x1 + 5; x < z.x2; x += stepX) {
        for (let y = z.y1 + 5; y < z.y2; y += stepY) samplePoints.push([x, y]);
      }
    });

    samplePoints.forEach(([x, y]) => {
      document.elementsFromPoint(x, y).forEach(el => {
        if (isProtected(el)) return;
        const rect = el.getBoundingClientRect();
        if (!rect || rect.width <= 1 || rect.height <= 1) return;
        if (rect.width > window.innerWidth * .82 || rect.height > window.innerHeight * .82) return;
        hideLegacyElement(el);

        // Si l'élément est un morceau interne du vieux HUD, on remonte sur son conteneur raisonnable.
        let parent = el.parentElement;
        for (let i = 0; i < 4 && parent && !isProtected(parent); i++, parent = parent.parentElement) {
          const p = parent.getBoundingClientRect();
          if (!p || p.width <= 1 || p.height <= 1) break;
          if (p.width > 380 || p.height > 220) break;
          if (allZones.some(z => inZone(p, z))) hideLegacyElement(parent);
        }
      });
    });

    // 2) Deuxième passe par rectangles pour les éléments qui ne sont pas topmost.
    document.querySelectorAll('body *').forEach(el => {
      if (isProtected(el)) return;
      const rect = el.getBoundingClientRect();
      if (!rect || rect.width <= 2 || rect.height <= 2) return;
      if (rect.width > 460 || rect.height > 260) return;
      if (!allZones.some(z => inZone(rect, z))) return;
      hideLegacyElement(el);
    });
  };

  const startLegacyCleaner = () => {
    sweepLegacyUi();
    const fast = window.setInterval(sweepLegacyUi, 180);
    window.setTimeout(() => window.clearInterval(fast), 16000);
    window.setInterval(sweepLegacyUi, 1400);
    window.addEventListener('resize', sweepLegacyUi);
    const obs = new MutationObserver(sweepLegacyUi);
    obs.observe(document.body, { childList: true, subtree: true, attributes: true });
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
      <div class="prhud-mask prhud-mask-player" aria-hidden="true"></div>
      <div class="prhud-mask prhud-mask-edge" aria-hidden="true"></div>
      <div class="prhud-mask prhud-mask-phone" aria-hidden="true"></div>
      <div class="prhud-mask prhud-mask-topright" aria-hidden="true"></div>

      <section class="prhud-panel prhud-player" aria-label="Joueur">
        <div class="prhud-avatar">${avatar}<b>${safe(data.level)}</b></div>
        <div class="prhud-player-main">
          <div class="prhud-player-title"><strong>${safe(data.username)}</strong><span>${safe(data.role)}</span></div>
          <div class="prhud-stat red"><i>❤</i><em><u style="width:${pct(data.health.current, data.health.max)}"></u></em><small>${safe(data.health.current)} / ${safe(data.health.max)}</small></div>
          <div class="prhud-stat blue"><i>ϟ</i><em><u style="width:${pct(data.energy.current, data.energy.max)}"></u></em><small>${safe(data.energy.current)} / ${safe(data.energy.max)}</small></div>
        </div>
        <div class="prhud-player-money"><span>💵 ${fmt(data.money.cash ?? data.money.credits)}</span><span>💎 ${fmt(data.money.diamonds ?? data.money.pixels)}</span></div>
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
    sweepLegacyUi();
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', () => setTimeout(mount, 250));
  else setTimeout(mount, 250);
})();
