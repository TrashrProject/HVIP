(() => {
  'use strict';

  const VERSION = '60.0.0-paradise-visual-v2';
  const HUD_ID = 'paradise-rp-hud';
  const STYLE_ID = 'paradise-rp-hud-css';
  const CSS_URL = './paradise-rp-hud.css?v=60';
  const DATA_URL = '../rp-hud-data.php';

  const DEFAULT_DATA = {
    ok: false,
    id: 1024,
    citizen_id: 'PR-01024',
    username: 'Luiz',
    role: 'Staff',
    motto: 'Paradise City',
    level: 7,
    look: '',
    avatar_url: '',
    health: { current: 100, max: 100 },
    energy: { current: 82, max: 100 },
    money: { credits: 500, pixels: 0, cash: 500, diamonds: 0 },
    city: 'Paradise City',
    district: 'Downtown Marina',
    time: ''
  };

  const state = {
    data: DEFAULT_DATA,
    open: null,
    more: false,
    phoneApp: 'home',
    inventoryCategory: 'all',
    commandQuery: '',
    commandCat: 'Toutes',
    toast: null,
    selectedItem: 'Téléphone Paradise',
    notificationCenter: false
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
  const today = () => new Date().toLocaleDateString('fr-FR', { weekday: 'long', day: '2-digit', month: 'long' });

  const ICONS = {
    user: '<path d="M12 12.1a4 4 0 1 0 0-8 4 4 0 0 0 0 8Z"/><path d="M4.8 20c.7-4 3.1-6 7.2-6s6.5 2 7.2 6"/>',
    phone: '<rect x="7" y="2.8" width="10" height="18.4" rx="2.2"/><path d="M10.3 5.2h3.4M11 18.3h2"/>',
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
    shirt: '<path d="m8 4 4 2 4-2 4 3-3 4v9H7v-9L4 7l4-3Z"/>',
    doc: '<path d="M6 3h8l4 4v14H6V3Z"/><path d="M14 3v5h5M9 12h6M9 16h6"/>',
    send: '<path d="M3 20 21 12 3 4l3 7h8l-8 2-3 7Z"/>',
    plus: '<path d="M12 5v14M5 12h14"/>',
    bell: '<path d="M18 9a6 6 0 1 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9Z"/><path d="M10 21h4"/>',
    check: '<path d="m5 12 4 4L19 6"/>',
    alert: '<path d="M12 3 2.8 20h18.4L12 3Z"/><path d="M12 9v5M12 17h.01"/>',
    spark: '<path d="m12 3 1.6 4.4L18 9l-4.4 1.6L12 15l-1.6-4.4L6 9l4.4-1.6L12 3ZM18.5 14l.8 2.2 2.2.8-2.2.8-.8 2.2-.8-2.2-2.2-.8 2.2-.8.8-2.2Z"/>',
    heart: '<path d="M12 20s-7-4.4-8.3-9A4.8 4.8 0 0 1 12 6.7 4.8 4.8 0 0 1 20.3 11C19 15.6 12 20 12 20Z"/>',
    bolt: '<path d="m13 2-8 12h6l-1 8 9-13h-6l1-7Z"/>',
    copy: '<rect x="8" y="8" width="11" height="11" rx="2"/><path d="M5 15H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v1"/>'
  };

  const icon = (name, cls = '') => `<span class="prx-icon ${cls}"><svg viewBox="0 0 24 24" aria-hidden="true">${ICONS[name] || ICONS.spark}</svg></span>`;

  const DOCK = [
    { key: 'phone', label: 'Téléphone', icon: 'phone', action: 'open:phone', tone: 'aqua', keycap: 'F2' },
    { key: 'inventory', label: 'Inventaire', icon: 'bag', action: 'open:inventory', tone: 'violet', keycap: 'F3' },
    { key: 'profile', label: 'Profil', icon: 'user', action: 'open:profile', tone: 'lagoon', keycap: 'F4' },
    { key: 'jobs', label: 'Métiers', icon: 'briefcase', action: 'open:jobs', tone: 'gold', keycap: 'M' },
    { key: 'commands', label: 'Commandes', icon: 'command', action: 'open:commands', tone: 'blue', keycap: 'Ctrl K' }
  ];

  const PHONE_APPS = [
    ['messages', 'Messages', 'message', 'blue', '3'], ['contacts', 'Contacts', 'contacts', 'aqua', ''], ['bank', 'Banque', 'wallet', 'green', ''],
    ['taxi', 'Taxi', 'taxi', 'gold', ''], ['jobs', 'Jobs', 'briefcase', 'violet', ''], ['housing', 'Logements', 'home', 'coral', ''],
    ['social', 'Social', 'social', 'pink', '1'], ['news', 'News', 'spark', 'red', ''], ['settings', 'Réglages', 'settings', 'gray', '']
  ];

  const COMMANDS = [
    { name: ':me', cat: 'RP', title: 'Action RP', desc: 'Afficher une action autour de votre personnage.', syntax: ':me sort son badge' },
    { name: ':pay', cat: 'Social', title: 'Payer un joueur', desc: "Préparer un paiement RP vers un joueur.", syntax: ':pay Luiz 500' },
    { name: ':give', cat: 'RP', title: 'Donner un objet', desc: 'Transférer un objet à un autre citoyen.', syntax: ':give Luiz téléphone' },
    { name: ':tel', cat: 'Général', title: 'Téléphone', desc: 'Ouvrir le ParadisePhone.', syntax: ':tel' },
    { name: ':id', cat: 'Général', title: 'Carte identité', desc: 'Afficher la Paradise ID.', syntax: ':id' },
    { name: ':trabajar', cat: 'Travail', title: 'Actions métier', desc: 'Ouvrir les interactions liées au métier.', syntax: ':trabajar' },
    { name: ':commands', cat: 'Général', title: 'Command Center', desc: 'Ouvrir la liste des commandes.', syntax: ':commands' }
  ];

  const JOBS = [
    ['Police', 'Police de Paradise', "Maintenez l'ordre et protégez les citoyens.", '650 $ / service', 'blue', '🚓'],
    ['EMS', 'Paradise Medical', 'Soins, urgences et assistance médicale.', '620 $ / service', 'coral', '🚑'],
    ['Taxi', 'Paradise Taxi', "Transport rapide dans toute l'île.", 'À la course', 'gold', '🚕'],
    ['Restaurant', 'La Perle', 'Cuisine, service et business social.', 'Pourboires + salaire', 'green', '🍕'],
    ['Mécano', 'Marina Garage', 'Réparations, tuning et dépannage.', 'À la mission', 'orange', '🔧'],
    ['Immobilier', 'Paradise Realty', 'Villas, studios et quartiers premium.', 'Commission', 'aqua', '🏠']
  ];

  const ITEMS = [
    ['Téléphone Paradise', 'Outil principal pour contacter les citoyens.', '📱', 'Équipement', 'x1'],
    ['Carte ID', 'Document officiel Paradise City.', '🪪', 'Document', 'x1'],
    ['Eau fraîche', 'Restaure un peu votre énergie.', '🥤', 'Nourriture', 'x2'],
    ['Clés Villa', 'Accès propriété privée.', '🔑', 'Clés', 'x1'],
    ['Badge Staff', 'Accréditation interne.', '🛡️', 'Badge', 'x1'],
    ['Ticket Taxi', 'Course prioritaire.', '🎫', 'Divers', 'x3']
  ];

  function ensureCss() {
    let link = document.getElementById(STYLE_ID);
    if (!link) {
      link = document.createElement('link');
      link.id = STYLE_ID;
      link.rel = 'stylesheet';
      document.head.appendChild(link);
    }
    if (!String(link.href || '').includes('v=60')) link.href = CSS_URL;
  }

  function avatarUrl(data = state.data) {
    const look = String(data.look || '').trim();
    if (look && /^[a-z0-9.\-]+$/i.test(look)) {
      return `../avatar-image.php?figure=${encodeURIComponent(look)}&direction=2&head_direction=3&gesture=sml&action=std&size=l&headonly=1&hud=60`;
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
    window.clearTimeout(window.__prxToastTimer);
    window.__prxToastTimer = window.setTimeout(() => { state.toast = null; render(); }, 3600);
  }

  const btn = (label, iconName, action, variant = 'secondary', attrs = '') =>
    `<button type="button" class="prx-btn prx-btn-${variant}" data-prx-action="${esc(action)}" ${attrs}>${iconName ? icon(iconName) : ''}<span>${esc(label)}</span></button>`;

  function windowShell({ key, title, subtitle = '', iconName = 'spark', body, footer = '', cls = '', tone = 'aqua' }) {
    return `<section class="prx-window prx-tone-${esc(tone)} ${esc(cls)}" data-prx-window="${esc(key)}" role="dialog" aria-label="${esc(title)}">
      <header class="prx-window-head">
        <div class="prx-window-title"><span class="prx-window-icon">${icon(iconName)}</span><div><strong>${esc(title)}</strong>${subtitle ? `<small>${esc(subtitle)}</small>` : ''}</div></div>
        <button type="button" class="prx-icon-btn" data-prx-action="close" aria-label="Fermer">${icon('close')}</button>
      </header>
      <div class="prx-window-body">${body}</div>
      ${footer ? `<footer class="prx-window-foot">${footer}</footer>` : ''}
    </section>`;
  }

  function profileView() {
    const d = state.data;
    const ava = avatarUrl(d);
    const citizen = d.citizen_id || `PR-${String(num(d.id)).padStart(5, '0')}`;
    return windowShell({
      key: 'profile', title: 'Profil Paradise', subtitle: 'Identité citoyenne et progression', iconName: 'user', tone: 'aqua', cls: 'prx-window-profile',
      body: `<div class="prx-profile-v2">
        <div class="prx-profile-stage"><div class="prx-mini-room"><span></span><i></i><b></b></div>${ava ? `<img src="${esc(ava)}" alt="${esc(d.username)}">` : '<strong>RP</strong>'}<em>Niv. ${fmt(d.level)}</em></div>
        <div class="prx-profile-info">
          <div class="prx-profile-heading"><div><strong>${esc(d.username)}</strong><small>@${esc(d.username)}</small></div><span>${icon('spark')} ${esc(d.role || 'Citoyen')}</span></div>
          <div class="prx-profile-tabs"><button class="is-active">Identité</button><button>Documents</button><button>Badges</button></div>
          <div class="prx-stat-grid">
            <article><small>Citoyen</small><b>${esc(citizen)}</b></article>
            <article><small>Métier</small><b>${esc(d.role || 'Citoyen')}</b></article>
            <article><small>Solde</small><b>${fmt(d.money?.credits)} $</b></article>
            <article><small>Réputation</small><b>${fmt(d.level)} étoiles</b></article>
          </div>
          <div class="prx-badges"><span>🏝️ Paradise</span><span>🛡️ Staff</span><span>💬 Social</span></div>
        </div>
      </div>`,
      footer: `${btn('Documents', 'doc', 'open:documents', 'secondary')}${btn('Carte ID', 'id', 'command::id', 'primary')}`
    });
  }

  function bankView() {
    const d = state.data;
    const citizen = d.citizen_id || `PR-${String(num(d.id)).padStart(5, '0')}`;
    return windowShell({
      key: 'bank', title: 'Paradise Bank', subtitle: 'Compte personnel sécurisé', iconName: 'wallet', tone: 'green', cls: 'prx-window-bank',
      body: `<div class="prx-bank-layout">
        <section class="prx-paradise-card"><small>Compte principal</small><strong>${fmt(d.money?.credits)} $</strong><span>${esc(citizen)}</span><i>Paradise Card</i></section>
        <section class="prx-bank-actions"><button class="is-active">Envoyer</button><button>Recevoir</button><button>Historique</button></section>
        <form class="prx-transfer" data-prx-form="transfer"><label><span>Joueur</span><input name="player" autocomplete="off" placeholder="Luiz"></label><label><span>Montant</span><input name="amount" inputmode="numeric" autocomplete="off" placeholder="500"></label><button class="prx-btn prx-btn-success" type="submit">${icon('send')}<span>Préparer</span></button></form>
        <div class="prx-transactions"><p><span>Restaurant Paradise</span><b>-42 $</b></p><p><span>Salaire Staff</span><b class="plus">+500 $</b></p><p><span>Location appartement</span><b>-120 $</b></p></div>
      </div>`
    });
  }

  function inventoryView() {
    const cats = [['all','Tout','bag'],['food','Nourriture','spark'],['clothes','Vêtements','shirt'],['tools','Outils','settings'],['docs','Docs','doc'],['keys','Clés','id']];
    const selected = ITEMS.find(i => i[0] === state.selectedItem) || ITEMS[0];
    return windowShell({
      key: 'inventory', title: 'Inventaire', subtitle: '28 / 50 kg · organisation rapide', iconName: 'bag', tone: 'violet', cls: 'prx-window-inventory',
      body: `<div class="prx-inv-layout">
        <aside class="prx-inv-cats">${cats.map(c => `<button type="button" class="${state.inventoryCategory === c[0] ? 'is-active' : ''}" data-prx-category="${c[0]}">${icon(c[2])}<span>${esc(c[1])}</span></button>`).join('')}</aside>
        <main class="prx-inv-main"><label class="prx-search prx-search-inv">${icon('search')}<input placeholder="Rechercher un objet..."></label><div class="prx-inv-grid">${ITEMS.map(item => `<button type="button" class="prx-item ${item[0] === selected[0] ? 'is-selected' : ''}" data-prx-item="${esc(item[0])}"><span>${item[2]}</span><b>${esc(item[0])}</b><small>${esc(item[4])}</small></button>`).join('')}</div></main>
        <aside class="prx-item-panel"><div class="prx-big-sprite">${selected[2]}</div><strong>${esc(selected[0])}</strong><small>${esc(selected[3])}</small><p>${esc(selected[1])}</p><button>UTILISER</button><button>DONNER</button><button class="danger">JETER</button></aside>
      </div>`
    });
  }

  function jobsView() {
    return windowShell({
      key: 'jobs', title: 'Métiers Paradise', subtitle: 'Choisissez votre rôle dans la ville', iconName: 'briefcase', tone: 'gold', cls: 'prx-window-jobs',
      body: `<div class="prx-jobs-grid">${JOBS.map(j => `<article class="prx-job-card prx-job-${esc(j[4])}"><div class="prx-job-scene"><span>${j[5]}</span><i></i></div><small>${esc(j[1])}</small><strong>${esc(j[0])}</strong><p>${esc(j[2])}</p><footer><b>${esc(j[3])}</b><button type="button" data-prx-action="command::trabajar">Découvrir →</button></footer></article>`).join('')}</div>`
    });
  }

  function commandsView() {
    const q = state.commandQuery.trim().toLowerCase();
    const cats = ['Toutes', 'Général', 'RP', 'Social', 'Travail', 'Admin'];
    const list = COMMANDS.filter(c => (state.commandCat === 'Toutes' || c.cat === state.commandCat) && (!q || `${c.name} ${c.title} ${c.desc}`.toLowerCase().includes(q)));
    return windowShell({
      key: 'commands', title: 'Command Center', subtitle: 'Recherche rapide des commandes RP', iconName: 'command', tone: 'blue', cls: 'prx-window-commands',
      body: `<label class="prx-command-search">${icon('search')}<input data-prx-command-search autocomplete="off" placeholder="Rechercher : :pay, téléphone, métier..." value="${esc(state.commandQuery)}"></label>
        <div class="prx-command-tabs">${cats.map(c => `<button type="button" class="${state.commandCat === c ? 'is-active' : ''}" data-prx-command-cat="${esc(c)}">${esc(c)}</button>`).join('')}</div>
        <div class="prx-command-list">${list.length ? list.map(c => `<article class="prx-command-row"><code>${esc(c.name)}</code><div><strong>${esc(c.title)}</strong><p>${esc(c.desc)}</p><small>${esc(c.syntax)} · ${esc(c.cat)}</small></div><button type="button" data-prx-command="${esc(c.name)}">${icon('copy')} Copier</button></article>`).join('') : `<div class="prx-empty"><span>⌕</span><strong>Aucun résultat</strong><small>Essayez une autre catégorie ou un autre mot-clé.</small></div>`}</div>`
    });
  }

  function docsView() {
    const d = state.data;
    const ava = avatarUrl(d);
    const citizen = d.citizen_id || `PR-${String(num(d.id)).padStart(5, '0')}`;
    return windowShell({
      key: 'documents', title: 'Documents RP', subtitle: 'Paradise ID, permis et badges', iconName: 'doc', tone: 'aqua', cls: 'prx-window-docs',
      body: `<div class="prx-id-card"><header><b>PARADISE ID</b><span>${icon('spark')}</span></header><main>${ava ? `<img src="${esc(ava)}" alt="">` : '<i>RP</i>'}<div><strong>${esc(d.username)}</strong><p>${esc(citizen)}</p><small>${esc(d.role || 'Citoyen')}</small></div></main><footer>Valide · Paradise City</footer></div>
        <div class="prx-docs-list"><button>${icon('id')}<span><b>Carte citoyenne</b><small>Disponible</small></span></button><button>${icon('car')}<span><b>Permis véhicule</b><small>Non synchronisé</small></span></button><button>${icon('briefcase')}<span><b>Badge professionnel</b><small>${esc(d.role || 'Citoyen')}</small></span></button></div>`
    });
  }

  function simpleView(key) {
    const views = {
      housing: ['Immobilier', 'Villas, appartements et studios Paradise.', 'home', '🏠', 'Aucun bien synchronisé'],
      garage: ['Garage', 'Véhicules personnels et clés.', 'car', '🚗', 'Aucun véhicule synchronisé'],
      business: ['Entreprise', 'Gestion de société et employés.', 'building', '🏢', 'Aucune entreprise liée'],
      wardrobe: ['Dressing', 'Tenues et identité visuelle.', 'shirt', '👕', 'Aucune tenue synchronisée'],
      notifications: ['Notifications', 'Historique des alertes Paradise.', 'bell', '🔔', 'Aucune notification récente']
    };
    const v = views[key] || views.housing;
    return windowShell({ key, title: v[0], subtitle: v[1], iconName: v[2], tone: 'aqua', body: `<div class="prx-empty prx-empty-rich"><span>${v[3]}</span><strong>${esc(v[4])}</strong><small>Cette section est prête côté UI et attend les données serveur.</small></div>` });
  }

  function phoneView() {
    const d = state.data;
    const app = state.phoneApp;
    const content = app === 'home'
      ? `<div class="prx-phone-wallpaper"><strong>${esc(d.time || now())}</strong><small>${esc(today())}</small><div><span>☀️ Paradise</span><b>${fmt(d.money?.credits)} $</b></div></div><div class="prx-phone-apps">${PHONE_APPS.map(a => `<button type="button" class="phone-${a[3]}" data-prx-phone-app="${a[0]}"><span>${icon(a[2])}${a[4] ? `<b>${a[4]}</b>` : ''}</span><small>${esc(a[1])}</small></button>`).join('')}</div>`
      : `<div class="prx-phone-sub"><button type="button" data-prx-phone-app="home">‹ Accueil</button><div class="prx-empty"><span>${icon(PHONE_APPS.find(a => a[0] === app)?.[2] || 'spark')}</span><strong>${esc(PHONE_APPS.find(a => a[0] === app)?.[1] || 'Application')}</strong><small>${app === 'bank' ? `Solde actuel : ${fmt(d.money?.credits)} $` : 'Application prête pour les données serveur.'}</small>${app === 'bank' ? btn('Ouvrir banque', 'wallet', 'open:bank', 'success') : ''}${app === 'jobs' ? btn('Voir métiers', 'briefcase', 'open:jobs', 'warning') : ''}${app === 'housing' ? btn('Voir logements', 'home', 'open:housing', 'primary') : ''}</div></div>`;
    return `<section class="prx-phone" role="dialog" aria-label="ParadisePhone"><div class="prx-phone-frame"><span class="prx-phone-side"></span><header><span>${esc(d.time || now())}</span><i></i><button type="button" data-prx-action="close">${icon('close')}</button></header><main>${content}</main><footer><button type="button" data-prx-phone-app="home"><span></span></button></footer></div></section>`;
  }

  function activeWindow() {
    if (!state.open) return '';
    if (state.open === 'phone') return phoneView();
    if (state.open === 'profile') return profileView();
    if (state.open === 'bank') return bankView();
    if (state.open === 'inventory') return inventoryView();
    if (state.open === 'jobs') return jobsView();
    if (state.open === 'commands') return commandsView();
    if (state.open === 'documents') return docsView();
    return simpleView(state.open);
  }

  function hud() {
    const d = state.data;
    const ava = avatarUrl(d);
    const active = state.open || 'profile';
    return `<div class="prx-shell" data-prx-version="${VERSION}">
      <section class="prx-player-card" data-prx-action="open:profile">
        <div class="prx-avatar-ring">${ava ? `<img src="${esc(ava)}" alt="${esc(d.username)}">` : '<b>RP</b>'}<i></i></div>
        <div class="prx-player-content"><div class="prx-player-name"><strong>${esc(d.username)}</strong><span>${icon('spark')} ${esc(d.role || 'Citoyen')}</span></div><small>● En ligne · ${esc(d.district || d.city || 'Paradise')}</small><div class="prx-health"><em style="width:${pct(d.health?.current, d.health?.max)}"></em></div></div>
        <button type="button">${icon('chevron')}</button>
      </section>

      <section class="prx-zone-chip"><span>🌴</span><strong>${esc(d.district || 'Paradise Downtown')}</strong></section>

      <section class="prx-status-bar"><button type="button" class="money" data-prx-action="open:bank"><span>💵</span><small>Solde</small><strong>${fmt(d.money?.credits ?? d.money?.cash)} $</strong></button><button type="button" class="role" data-prx-action="open:jobs"><span>🛡️</span><small>Statut</small><strong>${esc(d.role || 'Citoyen')}</strong></button><button type="button" class="time"><span>☀️</span><small>Heure</small><strong>${esc(d.time || now())}</strong></button><button type="button" class="notif" data-prx-action="open:notifications">${icon('bell')}<b>2</b></button></section>

      <nav class="prx-dock" aria-label="Dock ParadiseRP">
        ${DOCK.map(item => `<button type="button" class="prx-dock-btn prx-${item.tone} ${active === item.key ? 'is-active' : ''}" data-prx-action="${esc(item.action)}" data-tip="${esc(item.label)} · ${esc(item.keycap)}"><span>${icon(item.icon)}<i></i></span><strong>${esc(item.label)}</strong><small>${esc(item.keycap)}</small></button>`).join('')}
      </nav>

      <button type="button" class="prx-action-orb ${state.more ? 'is-open' : ''}" data-prx-action="toggle-more" aria-label="Actions rapides"><span>${icon('plus')}</span><small>Actions</small></button>
      <div class="prx-action-menu ${state.more ? 'is-open' : ''}">
        <header><strong>Actions rapides</strong><small>RP contextuel</small></header>
        ${btn('Payer un joueur', 'wallet', 'open:bank', 'ghost')}${btn('Donner un objet', 'bag', 'open:inventory', 'ghost')}${btn('Appeler taxi', 'taxi', 'open:phone', 'ghost')}${btn('Mes véhicules', 'car', 'open:garage', 'ghost')}${btn('Mes propriétés', 'home', 'open:housing', 'ghost')}${btn('Documents', 'doc', 'open:documents', 'ghost')}
      </div>

      <div class="prx-context-card"><span>⌁</span><div><strong>Paradise Market</strong><small><kbd>E</kbd> Interagir</small></div></div>
      <div class="prx-layer ${state.open ? 'is-open' : ''}">${activeWindow()}</div>
      ${state.toast ? `<div class="prx-toast prx-toast-${esc(state.toast.tone)}">${icon(state.toast.tone === 'success' ? 'check' : state.toast.tone === 'danger' ? 'alert' : 'spark')}<div><strong>${esc(state.toast.title)}</strong>${state.toast.body ? `<small>${esc(state.toast.body)}</small>` : ''}</div><i></i></div>` : ''}
    </div>`;
  }

  function render() {
    ensureCss();
    let root = document.getElementById(HUD_ID);
    if (!root) {
      root = document.createElement('div');
      root.id = HUD_ID;
      document.body.appendChild(root);
    }
    root.innerHTML = hud();
    bindRoot(root);
    if (state.open === 'commands') window.setTimeout(() => document.querySelector(`#${HUD_ID} [data-prx-command-search]`)?.focus(), 40);
  }

  function open(key) {
    state.open = key;
    state.more = false;
    if (key === 'phone') state.phoneApp = 'home';
    render();
  }

  function actionFrom(value, root) {
    if (!value) return;
    if (value === 'close') { state.open = null; state.more = false; render(); return; }
    if (value === 'toggle-more') { state.more = !state.more; render(); return; }
    if (value.startsWith('open:')) { open(value.slice(5)); return; }
    if (value.startsWith('command:')) {
      const command = value.slice(8);
      state.open = null;
      state.more = false;
      render();
      if (setNativeChat(command)) toast('Commande prête', command, 'success');
      else toast('Chat introuvable', 'Le champ Nitro natif est indisponible.', 'danger');
    }
  }

  function bindRoot(root) {
    root.onclick = event => {
      const itemButton = event.target.closest('[data-prx-item]');
      if (itemButton) { state.selectedItem = itemButton.getAttribute('data-prx-item') || state.selectedItem; render(); return; }
      const cmdButton = event.target.closest('[data-prx-command]');
      if (cmdButton) { const c = cmdButton.getAttribute('data-prx-command') || ''; state.open = null; render(); if (setNativeChat(c)) toast('Commande copiée dans le chat', c, 'success'); return; }
      const catButton = event.target.closest('[data-prx-category]');
      if (catButton) { state.inventoryCategory = catButton.getAttribute('data-prx-category') || 'all'; render(); return; }
      const cmdCat = event.target.closest('[data-prx-command-cat]');
      if (cmdCat) { state.commandCat = cmdCat.getAttribute('data-prx-command-cat') || 'Toutes'; render(); return; }
      const phoneApp = event.target.closest('[data-prx-phone-app]');
      if (phoneApp) { state.phoneApp = phoneApp.getAttribute('data-prx-phone-app') || 'home'; render(); return; }
      const actionButton = event.target.closest('[data-prx-action], .prx-player-card');
      if (actionButton) { actionFrom(actionButton.getAttribute('data-prx-action'), root); return; }
      if (event.target.classList.contains('prx-layer') && state.open && state.open !== 'phone') { state.open = null; render(); }
    };
    root.oninput = event => {
      if (event.target.matches('[data-prx-command-search]')) { state.commandQuery = event.target.value || ''; render(); }
    };
    root.onsubmit = event => {
      const form = event.target.closest('[data-prx-form="transfer"]');
      if (!form) return;
      event.preventDefault();
      const fd = new FormData(form);
      const player = String(fd.get('player') || '').trim();
      const amount = String(fd.get('amount') || '').replace(/[^0-9]/g, '');
      if (!player || !amount || Number(amount) <= 0) { toast('Paiement incomplet', 'Ajoutez un joueur et un montant.', 'danger'); return; }
      state.open = null; render();
      const command = `:pay ${player} ${amount}`;
      if (setNativeChat(command)) toast('Paiement préparé', command, 'success');
      else toast('Chat indisponible', 'Impossible de préparer la commande.', 'danger');
    };
  }

  function keyboard(event) {
    if (event.key === 'Escape' && (state.open || state.more)) { event.preventDefault(); state.open = null; state.more = false; render(); return; }
    if (event.key === 'F2') { event.preventDefault(); open('phone'); return; }
    if (event.key === 'F3') { event.preventDefault(); open('inventory'); return; }
    if (event.key === 'F4') { event.preventDefault(); open('profile'); return; }
    if (!event.ctrlKey && !event.metaKey && event.key.toLowerCase() === 'm') { event.preventDefault(); open('jobs'); return; }
    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') { event.preventDefault(); open('commands'); return; }

    const target = event.target;
    if (event.key !== 'Enter' || !target || (target.tagName !== 'INPUT' && target.tagName !== 'TEXTAREA')) return;
    if (target.closest?.(`#${HUD_ID}`)) return;
    const value = String(target.value || '').trim().toLowerCase();
    const local = { ':commands': 'commands', ':tel': 'phone', ':phone': 'phone' }[value];
    if (!local) return;
    event.preventDefault();
    event.stopImmediatePropagation();
    setNativeChat('', false);
    open(local);
  }

  async function loadData(first = false) {
    try {
      const response = await fetch(`${DATA_URL}?_=${Date.now()}`, { cache: 'no-store', credentials: 'same-origin' });
      if (!response.ok) throw new Error(String(response.status));
      const json = await response.json();
      state.data = {
        ...DEFAULT_DATA,
        ...(json || {}),
        health: { ...DEFAULT_DATA.health, ...(json?.health || {}) },
        energy: { ...DEFAULT_DATA.energy, ...(json?.energy || {}) },
        money: { ...DEFAULT_DATA.money, ...(json?.money || {}) }
      };
    } catch (_) {
      if (first) state.data = DEFAULT_DATA;
    }
    const active = document.activeElement;
    const typing = active && active.closest?.(`#${HUD_ID} .prx-window, #${HUD_ID} .prx-phone`);
    if (!typing) render();
  }

  function boot() {
    ensureCss();
    render();
    loadData(true);
    window.addEventListener('keydown', keyboard, true);
    window.setInterval(() => loadData(false), 10000);
    window.setInterval(() => { try { window.__paradiseNativeUiOffScan?.(); } catch (_) {} }, 1800);
    window.__ParadiseRPUI = { open, toast, version: VERSION };
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();