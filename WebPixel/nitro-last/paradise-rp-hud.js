(() => {
  'use strict';

  const VERSION = '1.0.0';
  const HUD_ID = 'paradise-rp-hud';
  const STYLE_ID = 'paradise-rp-hud-css';
  const DATA_URL = '../rp-hud-data.php';
  const CSS_URL = './paradise-rp-hud.css?v=1';

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
    time: '21:45'
  };

  const nav = [
    { key: 'player', label: 'Joueur', icon: '🧑', command: '' },
    { key: 'phone', label: 'Téléphone', icon: '📱', command: ':tel' },
    { key: 'id', label: 'Carte ID', icon: '🪪', command: ':id' },
    { key: 'job', label: 'Métier', icon: '💼', command: ':trabajar' },
    { key: 'home', label: 'Accueil', icon: '⌂', command: '' },
    { key: 'shop', label: 'Boutique', icon: '🛒', command: '' },
    { key: 'bag', label: 'Inventaire', icon: '🎒', command: '' },
    { key: 'cmd', label: 'Commandes', icon: '>_', command: ':commands' }
  ];

  const rail = [
    { key: 'home', label: 'Accueil', icon: '⌂', command: '' },
    { key: 'inventory', label: 'Inventaire', icon: '▣', command: '' },
    { key: 'job', label: 'Métier', icon: '▣', command: ':trabajar' },
    { key: 'map', label: 'Carte', icon: '⌖', command: '' },
    { key: 'shop', label: 'Boutique', icon: '⌗', command: '' }
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
    if (document.getElementById(STYLE_ID)) return;
    const link = document.createElement('link');
    link.id = STYLE_ID;
    link.rel = 'stylesheet';
    link.href = CSS_URL;
    document.head.appendChild(link);
  };

  const findNativeChatInput = () => {
    const inputs = [...document.querySelectorAll('input[type="text"], input:not([type]), textarea')]
      .filter(el => !el.closest(`#${HUD_ID}`) && !el.disabled && !el.readOnly);

    if (!inputs.length) return null;
    return inputs
      .map(el => ({ el, rect: el.getBoundingClientRect() }))
      .filter(x => x.rect.width > 80 && x.rect.height > 15)
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
    return true;
  };

  const focusNativeWith = (text = '') => {
    const overlayInput = document.querySelector('#prhud-chat-input');
    if (overlayInput) {
      overlayInput.value = text;
      overlayInput.focus();
      overlayInput.select?.();
    }

    const native = findNativeChatInput();
    if (native && text) {
      native.focus();
      setNativeValue(native, text);
    }
  };

  const action = (item) => {
    if (!item) return;
    if (item.command) return focusNativeWith(item.command);
    if (item.key === 'home') return window.dispatchEvent(new CustomEvent('paradise:home'));
    focusNativeWith('');
  };

  const build = (data) => {
    const merged = {
      ...DEFAULT_DATA,
      ...data,
      health: { ...DEFAULT_DATA.health, ...(data.health || {}) },
      energy: { ...DEFAULT_DATA.energy, ...(data.energy || {}) },
      money: { ...DEFAULT_DATA.money, ...(data.money || {}) }
    };

    const avatar = merged.avatar_url
      ? `<img src="${safe(merged.avatar_url)}" alt="${safe(merged.username)}" draggable="false" onerror="this.replaceWith(Object.assign(document.createElement('div'), { className: 'prhud-avatar-fallback', textContent: '${safe(String(merged.username || 'P').charAt(0).toUpperCase())}' }))">`
      : `<div class="prhud-avatar-fallback">${safe(String(merged.username || 'P').charAt(0).toUpperCase())}</div>`;

    const dock = nav.map(item => `
      <button class="prhud-dock-btn ${item.key === 'home' ? 'home active' : ''}" data-pr-action="${safe(item.key)}" type="button" aria-label="${safe(item.label)}">
        <span class="prhud-dock-ico">${safe(item.icon)}</span>
        <span class="prhud-dock-label">${safe(item.label)}</span>
      </button>`).join('');

    const railItems = rail.map((item, index) => `
      <button class="prhud-rail-btn ${index === 0 ? 'active' : ''}" data-pr-rail="${safe(item.key)}" type="button" aria-label="${safe(item.label)}">
        <span class="prhud-rail-ico">${safe(item.icon)}</span>
        <span class="prhud-rail-label">${safe(item.label)}</span>
      </button>`).join('');

    return `
      <section class="prhud-panel prhud-player" aria-label="Statut joueur">
        <div class="prhud-avatar">${avatar}<div class="prhud-level">${safe(merged.level)}</div></div>
        <div class="prhud-player-name"><span>${safe(merged.username)}</span><i class="prhud-online-dot"></i></div>
        <div class="prhud-role">${safe(merged.role)}</div>
        <div class="prhud-bars">
          <div class="prhud-stat">
            <span class="prhud-stat-icon">❤</span>
            <span class="prhud-stat-track"><i class="prhud-stat-fill" style="width:${pct(merged.health.current, merged.health.max)}"></i></span>
            <span class="prhud-stat-value">${safe(merged.health.current)} / ${safe(merged.health.max)}</span>
          </div>
          <div class="prhud-stat">
            <span class="prhud-stat-icon">⚡</span>
            <span class="prhud-stat-track"><i class="prhud-stat-fill energy" style="width:${pct(merged.energy.current, merged.energy.max)}"></i></span>
            <span class="prhud-stat-value">${safe(merged.energy.current)} / ${safe(merged.energy.max)}</span>
          </div>
        </div>
      </section>

      <div class="prhud-meta">
        <div class="prhud-panel prhud-meta-chip">${safe(merged.time)} <span>☀</span></div>
        <div class="prhud-panel prhud-meta-chip"><span>⌖</span> ${safe(merged.city)}</div>
      </div>

      <section class="prhud-money" aria-label="Monnaies">
        <div class="prhud-panel prhud-money-card credits">
          <span class="prhud-money-icon">▰</span>
          <span><strong class="prhud-money-number">${fmt(merged.money.credits)}</strong><small class="prhud-money-label">Crédits</small></span>
          <button class="prhud-money-plus" type="button" aria-label="Acheter des crédits">+</button>
        </div>
        <div class="prhud-panel prhud-money-card pixels">
          <span class="prhud-money-icon">H</span>
          <span><strong class="prhud-money-number">${fmt(merged.money.pixels)}</strong><small class="prhud-money-label">Pixels</small></span>
          <button class="prhud-money-plus" type="button" aria-label="Acheter des pixels">+</button>
        </div>
        <button class="prhud-panel prhud-menu-btn" type="button" aria-label="Menu">☰</button>
      </section>

      <nav class="prhud-panel prhud-rail" aria-label="Navigation rapide gauche">${railItems}</nav>

      <section class="prhud-panel prhud-quests" aria-label="Quêtes quotidiennes">
        <h3>QUÊTES QUOTIDIENNES</h3>
        <div class="prhud-quest-row"><span>Se connecter</span><b class="prhud-quest-progress done">1/1 ●</b></div>
        <div class="prhud-quest-row"><span>Parler à 3 joueurs</span><b class="prhud-quest-progress">2/3 ○</b></div>
        <div class="prhud-quest-row"><span>Travailler 30 min</span><b class="prhud-quest-progress">0/30 ○</b></div>
      </section>

      <form class="prhud-panel prhud-chat" id="prhud-chat-form" autocomplete="off">
        <select class="prhud-chat-channel" aria-label="Canal de discussion">
          <option>Discussion générale</option>
          <option>Chuchoter</option>
          <option>Crier</option>
        </select>
        <input id="prhud-chat-input" class="prhud-chat-input" type="text" placeholder="Clique ici pour chatter..." autocomplete="off">
        <button class="prhud-chat-emoji" type="button" aria-label="Emojis">☺</button>
        <button class="prhud-chat-send" type="submit" aria-label="Envoyer">›</button>
      </form>

      <nav class="prhud-panel prhud-dock" aria-label="Barre RP principale">${dock}</nav>
    `;
  };

  const hydrate = (hud) => {
    hud.querySelectorAll('[data-pr-action]').forEach(btn => {
      const item = nav.find(x => x.key === btn.dataset.prAction);
      btn.addEventListener('click', () => action(item));
    });

    hud.querySelectorAll('[data-pr-rail]').forEach(btn => {
      const item = rail.find(x => x.key === btn.dataset.prRail);
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

    input?.addEventListener('focus', () => {
      const native = findNativeChatInput();
      if (native && !input.value) native.focus();
    });
  };

  const loadData = async () => {
    try {
      const res = await fetch(`${DATA_URL}?_=${Date.now()}`, { credentials: 'include', cache: 'no-store' });
      if (!res.ok) throw new Error(`HUD data ${res.status}`);
      return await res.json();
    } catch (error) {
      console.warn('[ParadiseHUD] données indisponibles, fallback visuel utilisé', error);
      return DEFAULT_DATA;
    }
  };

  const mount = async () => {
    if (document.getElementById(HUD_ID)) return;
    ensureCss();

    const hud = document.createElement('div');
    hud.id = HUD_ID;
    hud.dataset.version = VERSION;
    hud.innerHTML = build(DEFAULT_DATA);
    document.body.appendChild(hud);
    hydrate(hud);

    const data = await loadData();
    if (!document.body.contains(hud)) return;
    hud.innerHTML = build(data && typeof data === 'object' ? data : DEFAULT_DATA);
    hydrate(hud);
  };

  const boot = () => {
    // Petit délai volontaire : laisse Nitro créer son canvas avant l'overlay.
    window.setTimeout(mount, 900);
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();
