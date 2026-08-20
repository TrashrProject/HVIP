(() => {
  'use strict';

  const VERSION = '70.0.0-game-ui-v3';
  const HUD_ID = 'paradise-rp-hud';
  const STYLE_ID = 'paradise-rp-hud-css';
  const CSS_URL = './paradise-rp-hud.css?v=70';
  const DATA_URL = '../rp-hud-data.php';

  const DEFAULT_DATA = {
    ok: false,
    id: 1024,
    citizen_id: 'PR-01024',
    username: 'Luiz',
    role: 'Staff',
    motto: 'Downtown Marina',
    level: 7,
    look: '',
    avatar_url: '',
    health: { current: 100, max: 100 },
    energy: { current: 82, max: 100 },
    money: { credits: 500, pixels: 0, cash: 500, diamonds: 0, bank: 12450 },
    city: 'Paradise City',
    district: 'Downtown Marina',
    room: { name: 'Downtown Marina', owner: 'ParadiseRP', users: 14 },
    time: ''
  };

  const state = {
    data: DEFAULT_DATA,
    open: null,
    quick: false,
    phoneApp: 'home',
    inventoryCategory: 'all',
    commandQuery: '',
    commandCategory: 'all',
    toast: null,
    inspector: false
  };

  const esc = value => String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;');

  const num = value => Number.isFinite(Number(value)) ? Number(value) : 0;
  const fmt = value => new Intl.NumberFormat('fr-FR').format(num(value));
  const pct = (cur, max) => `${Math.max(0, Math.min(100, (num(cur) / Math.max(1, num(max))) * 100))}%`;
  const now = () => new Date().toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' });
  const dateLabel = () => new Date().toLocaleDateString('fr-FR', { weekday: 'long', day: '2-digit', month: 'long' });

  const ICONS = {
    user: '<path d="M12 12.2a4.1 4.1 0 1 0 0-8.2 4.1 4.1 0 0 0 0 8.2Z"/><path d="M4.5 20c.8-4.2 3.2-6.2 7.5-6.2s6.7 2 7.5 6.2"/>',
    phone: '<rect x="7" y="2.8" width="10" height="18.4" rx="2"/><path d="M10.3 5.2h3.4M11 18.3h2"/>',
    bag: '<path d="M6 8h12l1 12H5L6 8Z"/><path d="M9 8V6a3 3 0 0 1 6 0v2"/>',
    wallet: '<path d="M4 6.5h14a2 2 0 0 1 2 2V18H5.5A2.5 2.5 0 0 1 3 15.5v-9A2.5 2.5 0 0 1 5.5 4H18"/><path d="M15 11h5v4h-5a2 2 0 0 1 0-4Z"/>',
    briefcase: '<rect x="3" y="7" width="18" height="12" rx="2"/><path d="M9 7V5.5A1.5 1.5 0 0 1 10.5 4h3A1.5 1.5 0 0 1 15 5.5V7M3 12h18"/>',
    command: '<path d="m5 7 5 5-5 5M12 17h7"/>',
    id: '<rect x="3" y="5" width="18" height="14" rx="2"/><circle cx="8" cy="11" r="2"/><path d="M5.7 15c.5-1.5 1.3-2.2 2.3-2.2s1.8.7 2.3 2.2M13 9h5M13 13h5"/>',
    close: '<path d="m6 6 12 12M18 6 6 18"/>',
    chevron: '<path d="m9 5 7 7-7 7"/>',
    search: '<circle cx="11" cy="11" r="6"/><path d="m16 16 4 4"/>',
    message: '<path d="M4 5h16v11H9l-5 4V5Z"/>',
    contacts: '<circle cx="9" cy="9" r="3"/><path d="M3.5 19c.5-3.5 2.3-5.2 5.5-5.2s5 1.7 5.5 5.2M16 8h5M18.5 5.5v5"/>',
    taxi: '<path d="M5 9h14l2 4v5H3v-5l2-4Z"/><path d="m7 9 1.5-4h7L17 9M6 18v2M18 18v2"/><circle cx="7" cy="14" r="1"/><circle cx="17" cy="14" r="1"/>',
    home: '<path d="m3.5 11 8.5-7 8.5 7"/><path d="M6 10v10h12V10M10 20v-6h4v6"/>',
    social: '<circle cx="12" cy="12" r="8"/><path d="M8 13.5c1 1.2 2.3 1.8 4 1.8s3-.6 4-1.8M9 9h.01M15 9h.01"/>',
    settings: '<circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.8 1.8 0 0 0 .4 2l.1.1-2.8 2.8-.1-.1a1.8 1.8 0 0 0-2-.4 1.8 1.8 0 0 0-1.1 1.6v.2H10V21a1.8 1.8 0 0 0-1.1-1.6 1.8 1.8 0 0 0-2 .4l-.1.1L4 17.1l.1-.1a1.8 1.8 0 0 0 .4-2A1.8 1.8 0 0 0 3 13.9h-.2V10H3a1.8 1.8 0 0 0 1.5-1.1 1.8 1.8 0 0 0-.4-2L4 6.8 6.8 4l.1.1a1.8 1.8 0 0 0 2 .4A1.8 1.8 0 0 0 10 3V2.8h3.9V3a1.8 1.8 0 0 0 1.1 1.5 1.8 1.8 0 0 0 2-.4l.1-.1L20 6.8l-.1.1a1.8 1.8 0 0 0-.4 2A1.8 1.8 0 0 0 21 10h.2v3.9H21a1.8 1.8 0 0 0-1.6 1.1Z"/>',
    map: '<path d="m4 6 5-2 6 2 5-2v14l-5 2-6-2-5 2V6Z"/><path d="M9 4v14M15 6v14"/>',
    building: '<path d="M5 20V5h10v15M15 9h4v11M8 8h2M8 12h2M8 16h2"/>',
    car: '<path d="M5 9h14l2 4v5H3v-5l2-4Z"/><path d="m7 9 1.5-4h7L17 9"/>',
    doc: '<path d="M6 3h8l4 4v14H6V3Z"/><path d="M14 3v5h5M9 12h6M9 16h6"/>',
    bell: '<path d="M18 9a6 6 0 0 0-12 0c0 7-2 7-2 7h16s-2 0-2-7"/><path d="M10 20a2 2 0 0 0 4 0"/>',
    plus: '<path d="M12 5v14M5 12h14"/>',
    check: '<path d="m5 12 4 4L19 6"/>',
    alert: '<path d="M12 3 2.8 20h18.4L12 3Z"/><path d="M12 9v5M12 17h.01"/>',
    spark: '<path d="m12 3 1.6 4.4L18 9l-4.4 1.6L12 15l-1.6-4.4L6 9l4.4-1.6L12 3Z"/>',
    send: '<path d="M3 20 21 12 3 4l3 7h8l-8 2-3 7Z"/>',
    friends: '<path d="M8.8 11.4a3.3 3.3 0 1 0 0-6.6 3.3 3.3 0 0 0 0 6.6Z"/><path d="M3.6 19.2c.6-3.8 2.3-5.6 5.2-5.6s4.6 1.8 5.2 5.6"/><path d="M16 12.4a2.8 2.8 0 1 0-1-5.4M15.5 14c2 .3 3.4 1.8 4 4.6"/>',
    trophy: '<path d="M8 4h8v4a4 4 0 0 1-8 0V4Z"/><path d="M8 6H4v1a4 4 0 0 0 4 4M16 6h4v1a4 4 0 0 1-4 4M12 12v4M9 20h6M10 16h4"/>'
  };

  const icon = (name, cls = '') => `<span class="pr3-icon ${cls}"><svg viewBox="0 0 24 24" aria-hidden="true">${ICONS[name] || ICONS.spark}</svg></span>`;
  const pixelIcon = label => `<span class="pr3-pixel-icon" aria-hidden="true">${esc(label)}</span>`;

  const LEFT_APPS = [
    ['inventory', 'Inventaire', 'bag', 'I'], ['phone', 'Téléphone', 'phone', 'P'], ['documents', 'Documents', 'id', 'D'],
    ['garage', 'Véhicules', 'car', 'V'], ['housing', 'Propriétés', 'home', 'H'], ['progress', 'Progression', 'trophy', 'G']
  ];

  const BAR_LEFT = [
    ['inventory', 'Inventaire', 'bag'], ['phone', 'Téléphone', 'phone'], ['documents', 'Docs', 'id'], ['garage', 'Véhicules', 'car']
  ];

  const BAR_RIGHT = [
    ['friends', 'Amis', 'friends'], ['group', 'Groupe', 'building'], ['notifications', 'Notifs', 'bell'], ['quick', 'Actions', 'plus']
  ];

  const PHONE_APPS = [
    ['messages', 'Messages', 'message', 'blue'], ['calls', 'Appels', 'phone', 'aqua'], ['contacts', 'Contacts', 'contacts', 'green'],
    ['bank', 'Banque', 'wallet', 'green'], ['taxi', 'Taxi', 'taxi', 'gold'], ['housing', 'Immo', 'home', 'coral'],
    ['jobs', 'Jobs', 'briefcase', 'violet'], ['social', 'Paradise', 'social', 'pink'], ['news', 'News', 'bell', 'coral']
  ];

  const COMMANDS = [
    { name: ':me', cat: 'RP', desc: 'Afficher une action RP.', syntax: ':me observe la scène' },
    { name: ':pay', cat: 'Social', desc: "Donner de l'argent à un joueur.", syntax: ':pay Luiz 500' },
    { name: ':give', cat: 'RP', desc: 'Donner un objet.', syntax: ':give Luiz téléphone' },
    { name: ':tel', cat: 'Général', desc: 'Ouvrir le ParadisePhone.', syntax: ':tel' },
    { name: ':id', cat: 'Général', desc: "Afficher votre carte d'identité.", syntax: ':id' },
    { name: ':trabajar', cat: 'Travail', desc: 'Accéder aux actions de métier.', syntax: ':trabajar' },
    { name: ':commands', cat: 'Général', desc: 'Afficher le centre de commandes.', syntax: ':commands' }
  ];

  const JOBS = [
    ['Police de Paradise', 'Maintenez l\'ordre dans les quartiers de l\'île.', '650 $', 'blue', '🚓'],
    ['EMS Paradise', 'Urgences, soins et assistance médicale.', '590 $', 'coral', '🚑'],
    ['Taxi Marina', 'Transportez les citoyens entre les lieux RP.', '420 $', 'gold', '🚕'],
    ['Mécanicien', 'Réparez, inspectez et dépannez les véhicules.', '480 $', 'orange', '🔧'],
    ['Restaurant', 'Service, cuisine et économie sociale.', '390 $', 'green', '🍕'],
    ['Immobilier', 'Vente, location et visites de propriétés.', '520 $', 'violet', '🏠']
  ];

  function ensureCss() {
    let link = document.getElementById(STYLE_ID);
    if (!link) {
      link = document.createElement('link');
      link.id = STYLE_ID;
      link.rel = 'stylesheet';
      document.head.appendChild(link);
    }
    if (!String(link.getAttribute('href') || '').includes('v=70')) link.href = CSS_URL;
  }

  function avatarUrl(data = state.data, headonly = true) {
    const look = String(data.look || '').trim();
    const head = headonly ? '&headonly=1' : '';
    if (look && /^[a-z0-9.\-]+$/i.test(look)) {
      return `../avatar-image.php?figure=${encodeURIComponent(look)}&direction=2&head_direction=3&gesture=sml&action=std&size=l${head}&hud=70`;
    }
    return String(data.avatar_url || '');
  }

  function nativeChat() {
    try {
      return [...document.querySelectorAll('#root [data-pr-native-chat-live="1"], #root input[placeholder*="chat" i], #root input[placeholder*="chatter" i], #root textarea[placeholder*="chat" i]')]
        .find(el => el && !el.disabled && !el.readOnly && !el.closest(`#${HUD_ID}`)) || null;
    } catch (_) { return null; }
  }

  function setNativeChat(text, focus = true) {
    const input = nativeChat();
    if (!input) return false;
    try {
      const win = input.ownerDocument?.defaultView || window;
      const proto = input instanceof win.HTMLTextAreaElement ? win.HTMLTextAreaElement.prototype : win.HTMLInputElement.prototype;
      const setter = Object.getOwnPropertyDescriptor(proto, 'value')?.set;
      if (setter) setter.call(input, String(text || '')); else input.value = String(text || '');
      input.dispatchEvent(new win.Event('input', { bubbles: true, composed: true }));
      input.dispatchEvent(new win.Event('change', { bubbles: true, composed: true }));
      if (focus) input.focus({ preventScroll: true });
      input.setSelectionRange?.(input.value.length, input.value.length);
      return true;
    } catch (_) { return false; }
  }

  function toast(title, body = '', tone = 'info') {
    state.toast = { title, body, tone };
    render();
    window.clearTimeout(window.__pr3ToastTimer);
    window.__pr3ToastTimer = window.setTimeout(() => { state.toast = null; render(); }, 3500);
  }

  function button(label, iconName, action, variant = 'normal') {
    return `<button type="button" class="pr3-btn pr3-btn-${esc(variant)}" data-pr3-action="${esc(action)}">${iconName ? icon(iconName) : ''}<span>${esc(label)}</span></button>`;
  }

  function windowShell({ key, title, subtitle = '', iconName = 'spark', tone = 'aqua', body, footer = '', toolbar = '' }) {
    return `<section class="pr3-window pr3-window-${esc(key)} pr3-tone-${esc(tone)}" role="dialog" aria-label="${esc(title)}">
      <header class="pr3-window-head">
        <div class="pr3-window-title">${icon(iconName)}<div><strong>${esc(title)}</strong>${subtitle ? `<small>${esc(subtitle)}</small>` : ''}</div></div>
        <button type="button" class="pr3-square-btn" data-pr3-action="close" aria-label="Fermer">${icon('close')}</button>
      </header>
      ${toolbar ? `<div class="pr3-window-toolbar">${toolbar}</div>` : ''}
      <div class="pr3-window-body">${body}</div>
      ${footer ? `<footer class="pr3-window-foot">${footer}</footer>` : ''}
    </section>`;
  }

  function profileView() {
    const d = state.data;
    const ava = avatarUrl(d, false);
    const citizen = d.citizen_id || `PR-${String(num(d.id)).padStart(5, '0')}`;
    return windowShell({
      key: 'profile', title: 'FICHE JOUEUR', subtitle: 'Identité ParadiseRP', iconName: 'user', tone: 'aqua',
      toolbar: `<button class="is-active">Profil</button><button>Documents</button><button>Badges</button><button>Stats</button>`,
      body: `<div class="pr3-profile-grid">
        <div class="pr3-character-stage"><div class="pr3-stage-sky"></div>${ava ? `<img src="${esc(ava)}" alt="${esc(d.username)}">` : '<b>RP</b>'}<span>LVL ${fmt(d.level)}</span></div>
        <div class="pr3-profile-info">
          <div class="pr3-name-row"><div><strong>${esc(d.username)}</strong><small>@${esc(d.username)}</small></div><em>${icon('spark')} ${esc(d.role || 'Citoyen')}</em></div>
          <div class="pr3-info-table">
            <p><span>Citoyen</span><b>${esc(citizen)}</b></p><p><span>Position</span><b>${esc(d.district || d.city || 'Paradise')}</b></p><p><span>Métier</span><b>${esc(d.role || 'Citoyen')}</b></p><p><span>Solde</span><b>${fmt(d.money?.credits)} $</b></p>
          </div>
          <div class="pr3-badges"><i>STAFF</i><i>PARADISE</i><i>RP</i></div>
        </div>
      </div>`,
      footer: `${button('Carte ID', 'id', 'open:documents', 'secondary')}${button('Préparer :id', 'command', 'command::id', 'primary')}`
    });
  }

  function inventoryView() {
    const cats = [['all','Tout'],['food','Nourriture'],['tools','Outils'],['docs','Documents'],['keys','Clés']];
    const items = [['🥤','Eau fraîche','x2'],['🪪','Carte ID','DOC'],['🔑','Clés villa','x1'],['📱','Téléphone','x1'],['🔧','Kit outil','x1'],['☕','Café','x3']];
    return windowShell({
      key: 'inventory', title: 'INVENTAIRE', subtitle: '28 / 50 kg', iconName: 'bag', tone: 'violet',
      toolbar: `<label class="pr3-search">${icon('search')}<input placeholder="Rechercher un objet..."></label>`,
      body: `<div class="pr3-inventory-layout"><aside class="pr3-slot-cats">${cats.map(c => `<button type="button" class="${state.inventoryCategory === c[0] ? 'is-active' : ''}" data-pr3-category="${esc(c[0])}">${esc(c[1])}</button>`).join('')}</aside><div class="pr3-slot-grid">${items.map((i, idx) => `<button type="button" class="pr3-slot ${idx === 1 ? 'is-selected' : ''}"><span>${i[0]}</span><b>${esc(i[1])}</b><small>${esc(i[2])}</small></button>`).join('')}${Array.from({ length: 10 }, () => '<button type="button" class="pr3-slot is-empty" disabled></button>').join('')}</div><aside class="pr3-item-inspector"><span class="pr3-big-sprite">🪪</span><strong>Carte ID</strong><small>Document citoyen Paradise City.</small><div>${button('Utiliser', 'check', 'command::id', 'primary')}${button('Donner', 'send', 'command::give ', 'secondary')}${button('Jeter', 'alert', 'noop', 'danger')}</div></aside></div>`
    });
  }

  function phoneView() {
    const d = state.data;
    if (state.phoneApp !== 'home') {
      const app = PHONE_APPS.find(a => a[0] === state.phoneApp) || PHONE_APPS[0];
      return `<section class="pr3-phone pr3-phone-open" role="dialog" aria-label="ParadisePhone"><div class="pr3-phone-frame"><header><span>${esc(d.time || now())}</span><i></i><button type="button" data-pr3-action="close">${icon('close')}</button></header><main class="pr3-phone-screen"><button type="button" class="pr3-phone-back" data-pr3-phone-app="home">‹ Accueil</button><div class="pr3-phone-empty"><span class="tone-${esc(app[3])}">${icon(app[2])}</span><strong>${esc(app[1])}</strong><small>Application ParadisePhone prête pour les modules RP.</small>${app[0] === 'bank' ? button('Ouvrir banque', 'wallet', 'open:bank', 'primary') : ''}${app[0] === 'jobs' ? button('Métiers', 'briefcase', 'open:jobs', 'primary') : ''}</div></main><footer><button data-pr3-phone-app="home"><span></span></button></footer></div></section>`;
    }
    return `<section class="pr3-phone pr3-phone-open" role="dialog" aria-label="ParadisePhone"><div class="pr3-phone-frame"><header><span>${esc(d.time || now())}</span><i></i><button type="button" data-pr3-action="close">${icon('close')}</button></header><main class="pr3-phone-screen"><div class="pr3-phone-wallpaper"><strong>${esc(d.time || now())}</strong><small>${esc(dateLabel())}</small><p>ParadisePhone</p></div><div class="pr3-phone-widget"><span>Compte</span><b>${fmt(d.money?.credits)} $</b></div><div class="pr3-phone-app-grid">${PHONE_APPS.map((a, idx) => `<button type="button" data-pr3-phone-app="${esc(a[0])}"><span class="tone-${esc(a[3])}">${icon(a[2])}${idx === 0 ? '<i>3</i>' : ''}</span><small>${esc(a[1])}</small></button>`).join('')}</div></main><footer><button data-pr3-phone-app="home"><span></span></button></footer></div></section>`;
  }

  function bankView() {
    const d = state.data;
    const citizen = d.citizen_id || `PR-${String(num(d.id)).padStart(5, '0')}`;
    return windowShell({ key: 'bank', title: 'PARADISE BANK', subtitle: citizen, iconName: 'wallet', tone: 'green', body: `<div class="pr3-bank-layout"><section class="pr3-bank-card"><small>Compte principal</small><strong>${fmt(d.money?.bank ?? d.money?.credits)} $</strong><span>${esc(citizen)} · Paradise Card</span></section><form class="pr3-transfer" data-pr3-form="transfer"><label>Destinataire<input name="player" placeholder="Luiz" autocomplete="off"></label><label>Montant<input name="amount" placeholder="500" inputmode="numeric"></label><button class="pr3-btn pr3-btn-primary" type="submit">${icon('send')} Envoyer</button></form><div class="pr3-transactions"><p><b>+650 $</b><span>Salaire Police</span></p><p><b>-42 $</b><span>Paradise Café</span></p><p><b>-350 $</b><span>Loyer Marina</span></p></div></div>` });
  }

  function jobsView() {
    return windowShell({ key: 'jobs', title: 'MÉTIERS', subtitle: 'Catalogue RP Paradise', iconName: 'briefcase', tone: 'gold', body: `<div class="pr3-job-catalog">${JOBS.map(j => `<article class="pr3-job-card pr3-job-${esc(j[3])}"><div class="pr3-job-art"><span>${j[4]}</span></div><div><strong>${esc(j[0])}</strong><p>${esc(j[1])}</p><small>${esc(j[2])} / service</small></div><button type="button" data-pr3-action="command::trabajar">Découvrir</button></article>`).join('')}</div>` });
  }

  function commandsView() {
    const cats = ['all','Général','RP','Social','Travail','Admin'];
    const q = state.commandQuery.trim().toLowerCase();
    const list = COMMANDS.filter(c => (state.commandCategory === 'all' || c.cat === state.commandCategory) && (!q || `${c.name} ${c.cat} ${c.desc} ${c.syntax}`.toLowerCase().includes(q)));
    return windowShell({ key: 'commands', title: 'COMMANDES', subtitle: 'Centre de commandes Paradise', iconName: 'command', tone: 'blue', toolbar: `<div class="pr3-tabs">${cats.map(c => `<button type="button" class="${state.commandCategory === c ? 'is-active' : ''}" data-pr3-command-cat="${esc(c)}">${esc(c === 'all' ? 'Toutes' : c)}</button>`).join('')}</div><label class="pr3-search">${icon('search')}<input data-pr3-command-search value="${esc(state.commandQuery)}" placeholder="Rechercher une commande..."></label>`, body: `<div class="pr3-command-list">${list.length ? list.map(c => `<button type="button" class="pr3-command-row" data-pr3-command="${esc(c.name)}"><code>${esc(c.name)}</code><span><b>${esc(c.desc)}</b><small>${esc(c.syntax)} · ${esc(c.cat)}</small></span>${icon('chevron')}</button>`).join('') : '<div class="pr3-empty"><b>Aucun résultat</b><small>Essayez une autre catégorie.</small></div>'}</div>` });
  }

  function docsView() {
    const d = state.data;
    const ava = avatarUrl(d, true);
    const citizen = d.citizen_id || `PR-${String(num(d.id)).padStart(5, '0')}`;
    return windowShell({ key: 'documents', title: 'DOCUMENTS', subtitle: 'Objets RP officiels', iconName: 'doc', tone: 'aqua', body: `<div class="pr3-doc-layout"><section class="pr3-id-card"><header><b>PARADISE CITY</b><small>IDENTITÉ CITOYENNE</small></header><main>${ava ? `<img src="${esc(ava)}" alt="">` : '<span>RP</span>'}<div><strong>${esc(d.username)}</strong><p>${esc(citizen)}</p><small>${esc(d.role || 'Citoyen')}</small></div></main><footer>Valide · ParadiseRP</footer></section><div class="pr3-doc-stack"><button>${icon('id')} Carte citoyenne</button><button>${icon('car')} Permis de conduire</button><button>${icon('wallet')} Paradise Card</button><button>${icon('briefcase')} Badge métier</button></div></div>` });
  }

  function simpleView(key) {
    const map = {
      garage: ['VÉHICULES', 'Garage RP', 'car', 'Aucun véhicule synchronisé', 'Les véhicules apparaîtront ici.'],
      housing: ['PROPRIÉTÉS', 'Immobilier Paradise', 'home', 'Aucune propriété synchronisée', 'Les biens apparaîtront ici.'],
      progress: ['PROGRESSION', 'Parcours RP', 'trophy', 'Progression en préparation', 'Niveaux, badges et objectifs arriveront ici.'],
      group: ['GROUPE', 'Couche sociale', 'building', 'Aucun groupe actif', 'Les groupes et entreprises seront reliés ici.'],
      friends: ['AMIS', 'Social Paradise', 'friends', 'Liste non synchronisée', 'Les amis connectés apparaîtront ici.'],
      notifications: ['NOTIFICATIONS', 'Centre de jeu', 'bell', 'Aucune notification', 'Les alertes RP récentes apparaîtront ici.']
    };
    const x = map[key] || map.notifications;
    return windowShell({ key, title: x[0], subtitle: x[1], iconName: x[2], tone: 'aqua', body: `<div class="pr3-empty"><span>${icon(x[2])}</span><b>${esc(x[3])}</b><small>${esc(x[4])}</small></div>` });
  }

  function activeView() {
    if (!state.open) return '';
    if (state.open === 'phone') return phoneView();
    if (state.open === 'profile') return profileView();
    if (state.open === 'inventory') return inventoryView();
    if (state.open === 'bank') return bankView();
    if (state.open === 'jobs') return jobsView();
    if (state.open === 'commands') return commandsView();
    if (state.open === 'documents') return docsView();
    return simpleView(state.open);
  }

  function playerHud() {
    const d = state.data;
    const ava = avatarUrl(d, true);
    return `<button type="button" class="pr3-player-hud" data-pr3-action="open:profile">
      <span class="pr3-avatar-frame">${ava ? `<img src="${esc(ava)}" alt="${esc(d.username)}">` : '<b>RP</b>'}<i></i></span>
      <span class="pr3-player-copy"><strong>${esc(d.username)}</strong><em>${esc(d.role || 'Citoyen')}</em><small>${esc(d.district || d.city || 'Paradise')}</small></span>
      <span class="pr3-player-bars"><u style="--p:${pct(d.health?.current, d.health?.max)}"></u><u class="energy" style="--p:${pct(d.energy?.current, d.energy?.max)}"></u></span>
    </button>`;
  }

  function statusHud() {
    const d = state.data;
    return `<section class="pr3-status-hud" aria-label="Paradise Status">
      <button type="button" data-pr3-action="open:bank"><small>Cash</small><b>${fmt(d.money?.credits)} $</b></button>
      <button type="button" data-pr3-action="open:bank"><small>Banque</small><b>${fmt(d.money?.bank ?? 0)} $</b></button>
      <button type="button" data-pr3-action="open:jobs"><small>Statut</small><b>${esc(d.role || 'Citoyen')}</b></button>
      <button type="button" data-pr3-action="noop"><small>Heure</small><b>${esc(d.time || now())}</b></button>
      <button type="button" class="pr3-notif" data-pr3-action="open:notifications">${icon('bell')}<i>2</i></button>
    </section>`;
  }

  function leftBar() {
    return `<nav class="pr3-leftbar" aria-label="Paradise Quick Bar">${LEFT_APPS.map(app => `<button type="button" data-pr3-action="open:${esc(app[0])}" data-pr3-tip="${esc(app[1])} · ${esc(app[3])}">${icon(app[2])}<small>${esc(app[3])}</small></button>`).join('')}</nav>`;
  }

  function roomPanel() {
    const r = state.data.room || {};
    return `<button type="button" class="pr3-room-info" data-pr3-action="open:room"><small>ROOM</small><strong>${esc(r.name || state.data.district || 'Downtown Marina')}</strong><span>👥 ${fmt(r.users || 14)} · ${esc(r.owner || 'ParadiseRP')}</span></button>`;
  }

  function inspector() {
    if (!state.inspector) return '';
    return `<aside class="pr3-object-inspector"><header><strong>Cadillac</strong><button type="button" data-pr3-action="inspector-off">${icon('close')}</button></header><div class="pr3-car-sprite">🚗</div><p><b>El Dorado</b><span>Propriétaire : ${esc(state.data.username)}</span></p><div>${button('Conduire', 'car', 'noop', 'primary')}${button('Clés', 'id', 'noop', 'secondary')}${button('Coffre', 'bag', 'noop', 'secondary')}</div></aside>`;
  }

  function bottomBar() {
    const d = state.data;
    const ava = avatarUrl(d, true);
    return `<section class="pr3-paradise-bar" aria-label="Paradise Bar">
      <div class="pr3-bar-left"><button type="button" class="pr3-mini-player" data-pr3-action="open:profile">${ava ? `<img src="${esc(ava)}" alt="">` : icon('user')}<span>LVL ${fmt(d.level)}</span></button>${BAR_LEFT.map(x => `<button type="button" class="pr3-bar-btn" data-pr3-action="open:${esc(x[0])}">${icon(x[2])}<small>${esc(x[1])}</small></button>`).join('')}</div>
      <div class="pr3-bar-chat"><span>${icon('message')}<b>LOCAL</b></span><em>Cliquez ici pour discuter...</em></div>
      <div class="pr3-bar-right">${BAR_RIGHT.map(x => `<button type="button" class="pr3-bar-btn ${x[0] === 'quick' ? 'is-action' : ''}" data-pr3-action="${x[0] === 'quick' ? 'toggle-quick' : `open:${esc(x[0])}`}" data-pr3-tip="${esc(x[1])}">${icon(x[2])}<small>${esc(x[1])}</small></button>`).join('')}</div>
    </section>`;
  }

  function quickMenu() {
    return `<div class="pr3-quick-menu ${state.quick ? 'is-open' : ''}"><header>ACTIONS RAPIDES</header>${button('Payer', 'wallet', 'open:bank', 'ghost')}${button('Donner objet', 'bag', 'command::give ', 'ghost')}${button('Taxi', 'taxi', 'open:phone', 'ghost')}${button('Véhicules', 'car', 'open:garage', 'ghost')}${button('Documents', 'id', 'open:documents', 'ghost')}${button('Inspecteur objet', 'spark', 'inspector-on', 'ghost')}</div>`;
  }

  function hud() {
    return `<div class="pr3-shell" data-version="${esc(VERSION)}">${playerHud()}${statusHud()}${leftBar()}${roomPanel()}${inspector()}${bottomBar()}${quickMenu()}<div class="pr3-layer ${state.open ? 'is-open' : ''}">${activeView()}</div>${state.toast ? `<div class="pr3-toast pr3-toast-${esc(state.toast.tone)}">${icon(state.toast.tone === 'danger' ? 'alert' : state.toast.tone === 'success' ? 'check' : 'spark')}<div><strong>${esc(state.toast.title)}</strong>${state.toast.body ? `<small>${esc(state.toast.body)}</small>` : ''}</div><i></i></div>` : ''}</div>`;
  }

  function render() {
    ensureCss();
    let root = document.getElementById(HUD_ID);
    if (!root) { root = document.createElement('div'); root.id = HUD_ID; document.body.appendChild(root); }
    root.innerHTML = hud();
    bind(root);
    if (state.open === 'commands') window.setTimeout(() => root.querySelector('[data-pr3-command-search]')?.focus(), 40);
  }

  function open(key) { state.open = key; state.quick = false; if (key === 'phone') state.phoneApp = 'home'; render(); }

  function action(value) {
    if (!value || value === 'noop') return;
    if (value === 'close') { state.open = null; render(); return; }
    if (value === 'toggle-quick') { state.quick = !state.quick; render(); return; }
    if (value === 'inspector-on') { state.inspector = true; state.quick = false; render(); return; }
    if (value === 'inspector-off') { state.inspector = false; render(); return; }
    if (value.startsWith('open:')) { open(value.slice(5)); return; }
    if (value.startsWith('command:')) {
      const command = value.slice(8);
      state.open = null; state.quick = false; render();
      if (setNativeChat(command)) toast('Commande prête', command || 'Saisissez la suite puis Entrée.', 'success');
      else toast('Chat indisponible', 'Le champ Nitro est introuvable.', 'danger');
    }
  }

  function bind(root) {
    root.onclick = event => {
      const act = event.target.closest('[data-pr3-action]');
      if (act) { action(act.getAttribute('data-pr3-action')); return; }
      const phone = event.target.closest('[data-pr3-phone-app]');
      if (phone) { state.phoneApp = phone.getAttribute('data-pr3-phone-app') || 'home'; render(); return; }
      const cat = event.target.closest('[data-pr3-category]');
      if (cat) { state.inventoryCategory = cat.getAttribute('data-pr3-category') || 'all'; render(); return; }
      const cmdCat = event.target.closest('[data-pr3-command-cat]');
      if (cmdCat) { state.commandCategory = cmdCat.getAttribute('data-pr3-command-cat') || 'all'; render(); return; }
      const cmd = event.target.closest('[data-pr3-command]');
      if (cmd) { const c = cmd.getAttribute('data-pr3-command') || ''; state.open = null; render(); if (setNativeChat(c)) toast('Commande prête', c, 'success'); return; }
      if (event.target.classList.contains('pr3-layer') && state.open && state.open !== 'phone') { state.open = null; render(); }
    };

    root.oninput = event => {
      if (!event.target.matches('[data-pr3-command-search]')) return;
      state.commandQuery = event.target.value || '';
      const list = root.querySelector('.pr3-command-list');
      if (!list) return;
      const q = state.commandQuery.trim().toLowerCase();
      const filtered = COMMANDS.filter(c => (state.commandCategory === 'all' || c.cat === state.commandCategory) && (!q || `${c.name} ${c.cat} ${c.desc} ${c.syntax}`.toLowerCase().includes(q)));
      list.innerHTML = filtered.length ? filtered.map(c => `<button type="button" class="pr3-command-row" data-pr3-command="${esc(c.name)}"><code>${esc(c.name)}</code><span><b>${esc(c.desc)}</b><small>${esc(c.syntax)} · ${esc(c.cat)}</small></span>${icon('chevron')}</button>`).join('') : '<div class="pr3-empty"><b>Aucun résultat</b><small>Essayez une autre catégorie.</small></div>';
    };

    root.onsubmit = event => {
      const form = event.target.closest('[data-pr3-form="transfer"]');
      if (!form) return;
      event.preventDefault();
      const fd = new FormData(form);
      const player = String(fd.get('player') || '').trim();
      const amount = String(fd.get('amount') || '').replace(/[^0-9]/g, '');
      if (!player || !amount || Number(amount) <= 0) { toast('Paiement incomplet', 'Renseignez joueur et montant.', 'danger'); return; }
      state.open = null; render();
      const command = `:pay ${player} ${amount}`;
      if (setNativeChat(command)) toast('Paiement préparé', command, 'success');
    };
  }

  function keyboard(event) {
    const target = event.target;
    const typing = target && (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA' || target.isContentEditable);
    if (event.key === 'Escape' && (state.open || state.quick || state.inspector)) { event.preventDefault(); state.open = null; state.quick = false; state.inspector = false; render(); return; }
    if (!typing && event.key.toLowerCase() === 'p') { event.preventDefault(); open('phone'); return; }
    if (!typing && event.key.toLowerCase() === 'i') { event.preventDefault(); open('inventory'); return; }
    if (!typing && event.key.toLowerCase() === 'm') { event.preventDefault(); open('jobs'); return; }
    if (!typing && event.key.toLowerCase() === 'c') { event.preventDefault(); open('commands'); return; }
    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') { event.preventDefault(); open('commands'); return; }
    if (event.key !== 'Enter' || !typing || target.closest?.(`#${HUD_ID}`)) return;
    const value = String(target.value || '').trim().toLowerCase();
    const mapped = { ':commands': 'commands', ':tel': 'phone', ':phone': 'phone' }[value];
    if (!mapped) return;
    event.preventDefault(); event.stopImmediatePropagation(); setNativeChat('', false); open(mapped);
  }

  async function loadData(first = false) {
    try {
      const response = await fetch(`${DATA_URL}?_=${Date.now()}`, { cache: 'no-store', credentials: 'same-origin' });
      if (!response.ok) throw new Error(String(response.status));
      const json = await response.json();
      state.data = { ...DEFAULT_DATA, ...(json || {}), health: { ...DEFAULT_DATA.health, ...(json?.health || {}) }, energy: { ...DEFAULT_DATA.energy, ...(json?.energy || {}) }, money: { ...DEFAULT_DATA.money, ...(json?.money || {}) }, room: { ...DEFAULT_DATA.room, ...(json?.room || {}) } };
    } catch (_) { if (first) state.data = DEFAULT_DATA; }
    const active = document.activeElement;
    const insideUi = active && active.closest?.(`#${HUD_ID} .pr3-window, #${HUD_ID} .pr3-phone`);
    if (!insideUi) render();
  }

  function boot() {
    ensureCss(); render(); loadData(true);
    window.addEventListener('keydown', keyboard, true);
    window.setInterval(() => loadData(false), 10000);
    window.setInterval(() => { try { window.__paradiseNativeUiOffScan?.(); } catch (_) {} }, 1800);
    window.__ParadiseRPUI = { open, toast, version: VERSION };
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();