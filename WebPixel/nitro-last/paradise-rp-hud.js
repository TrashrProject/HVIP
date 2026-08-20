(() => {
  'use strict';

  const VERSION = '40.0.0-paradise-ui-system';
  const HUD_ID = 'paradise-rp-hud';
  const CORE_CSS_ID = 'paradise-rp-hud-css';
  const COMPAT_CSS_ID = 'paradise-rp-dock-css';
  const CORE_CSS_URL = './paradise-rp-hud.css?v=40';
  const COMPAT_CSS_URL = './paradise-dock-redesign.css?v=10';
  const DATA_URL = '../rp-hud-data.php';

  const DEFAULT_DATA = {
    ok: false,
    username: 'ParadiseRP',
    role: 'Citoyen',
    level: 1,
    look: '',
    avatar_url: '',
    city: 'Paradise City',
    district: 'Marina District',
    organization: 'Aucune organisation',
    reputation: 42,
    playtime: '0h',
    citizen_id: 'PR-0001',
    health: { current: 315, max: 500 },
    energy: { current: 31, max: 100 },
    money: { credits: 789, pixels: 5000, cash: 1789, bank: 3420, savings: 12500, diamonds: 224 }
  };

  const state = {
    data: DEFAULT_DATA,
    active: null,
    activeDock: 'home',
    notices: [
      { id: 'welcome', type: 'success', title: 'ParadiseRP', text: 'Nouvelle interface chargée.' }
    ]
  };

  const safe = value => String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;');

  const n = value => Number.isFinite(Number(value)) ? Number(value) : 0;
  const fmt = value => new Intl.NumberFormat('fr-FR').format(n(value));
  const clamp = (value, min, max) => Math.max(min, Math.min(max, n(value)));
  const percent = (current, max) => `${clamp((n(current) / Math.max(1, n(max))) * 100, 0, 100).toFixed(0)}%`;
  const now = () => new Date().toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' });

  const icons = {
    user: '<svg viewBox="0 0 24 24"><path d="M12 12.2a4.2 4.2 0 1 0 0-8.4 4.2 4.2 0 0 0 0 8.4Z"/><path d="M4.5 20c.8-4.1 3.3-6.2 7.5-6.2s6.7 2.1 7.5 6.2"/></svg>',
    phone: '<svg viewBox="0 0 24 24"><rect x="7" y="2.7" width="10" height="18.6" rx="2.4"/><path d="M10.3 5.1h3.4M11 18h2"/></svg>',
    bag: '<svg viewBox="0 0 24 24"><path d="M6.2 8.2h11.6l1 12H5.2l1-12Z"/><path d="M9 8.2V6a3 3 0 0 1 6 0v2.2"/></svg>',
    bank: '<svg viewBox="0 0 24 24"><path d="M4 10h16L12 4 4 10Z"/><path d="M6 10v8M10 10v8M14 10v8M18 10v8M4 20h16"/></svg>',
    work: '<svg viewBox="0 0 24 24"><rect x="3" y="7" width="18" height="12" rx="2"/><path d="M9 7V5.4c0-.8.5-1.4 1.4-1.4h3.2c.9 0 1.4.6 1.4 1.4V7M3 12h18M12 11v2"/></svg>',
    home: '<svg viewBox="0 0 24 24"><path d="M3.8 11.2 12 4.2l8.2 7"/><path d="M6.2 10.4v9h11.6v-9"/><path d="M10 19.4v-5.1h4v5.1"/></svg>',
    id: '<svg viewBox="0 0 24 24"><rect x="3.5" y="5" width="17" height="14" rx="2"/><path d="M7 10h4M7 14h5.5M15 10h2.5M15 14h2.5"/></svg>',
    cmd: '<svg viewBox="0 0 24 24"><path d="m5 7 5 5-5 5M12 17h7"/></svg>',
    map: '<svg viewBox="0 0 24 24"><path d="m4 6 5-2 6 2 5-2v14l-5 2-6-2-5 2V6Z"/><path d="M9 4v14M15 6v14"/></svg>',
    heart: '<svg viewBox="0 0 24 24"><path d="M12 20s-7.4-4.3-8.6-9.2C2.6 7.4 4.6 5 7.4 5c1.7 0 3 1 4.6 2.8C13.6 6 14.9 5 16.6 5c2.8 0 4.8 2.4 4 5.8C19.4 15.7 12 20 12 20Z"/></svg>',
    energy: '<svg viewBox="0 0 24 24"><path d="m13.2 2.8-8 11.1h6.2L10.8 21l8-11.1h-6.2l.6-7.1Z"/></svg>',
    close: '<svg viewBox="0 0 24 24"><path d="M6 6l12 12M18 6 6 18"/></svg>',
    search: '<svg viewBox="0 0 24 24"><circle cx="11" cy="11" r="6.5"/><path d="m16 16 4 4"/></svg>',
    send: '<svg viewBox="0 0 24 24"><path d="M3.5 20.5 21 12 3.5 3.5 6.4 11H14l-7.6 2-2.9 7.5Z"/></svg>',
    bell: '<svg viewBox="0 0 24 24"><path d="M6.5 10.5a5.5 5.5 0 0 1 11 0v4.2l2 2.3h-15l2-2.3v-4.2Z"/><path d="M10 20h4"/></svg>'
  };

  const icon = name => `<span class="prp-icon prp-icon-${safe(name)}">${icons[name] || icons.home}</span>`;

  const dock = [
    { key: 'home', label: 'Accueil', icon: 'home', open: null },
    { key: 'phone', label: 'Phone', icon: 'phone', open: 'phone', command: ':tel' },
    { key: 'inventory', label: 'Inventaire', icon: 'bag', open: 'inventory' },
    { key: 'bank', label: 'Banque', icon: 'bank', open: 'bank' },
    { key: 'jobs', label: 'Métiers', icon: 'work', open: 'jobs', command: ':trabajar' },
    { key: 'documents', label: 'Docs', icon: 'id', open: 'documents', command: ':id' },
    { key: 'map', label: 'Ville', icon: 'map', open: 'city' },
    { key: 'commands', label: 'Commandes', icon: 'cmd', open: 'commands', command: ':commands' }
  ];

  const quickActions = [
    { key: 'interact', label: 'Interagir', icon: 'user' },
    { key: 'identity', label: 'Carte ID', icon: 'id', command: ':id' },
    { key: 'work', label: 'Travail', icon: 'work', command: ':trabajar' },
    { key: 'phone', label: 'Phone', icon: 'phone', command: ':tel' }
  ];

  const commands = [
    ['Général', ':commands', 'Ouvre ce menu des commandes.', ':commands'],
    ['RP', ':me', 'Décrit une action RP visible en room.', ':me sort sa carte bancaire'],
    ['Social', ':pay', 'Donner de l’argent à un joueur proche.', ':pay Nathan 250'],
    ['Travail', ':trabajar', 'Afficher les actions métier disponibles.', ':trabajar'],
    ['Documents', ':id', 'Afficher sa carte citoyenne.', ':id'],
    ['Téléphone', ':tel', 'Ouvrir le ParadisePhone.', ':tel'],
    ['Inventaire', ':give', 'Donner un objet à un joueur.', ':give eau Nathan 1']
  ];

  const jobs = [
    ['Police', 'Sécurité publique', '650 $', 'Ouvert', 'Patrouille, amendes, enquêtes, interventions.'],
    ['EMS', 'Urgences médicales', '590 $', 'Recrute', 'Soins, réanimation, transport hôpital.'],
    ['Taxi', 'Mobilité ville', '420 $', 'Ouvert', 'Courses rapides dans Paradise City.'],
    ['Restaurant', 'Commerce social', '380 $', '2 places', 'Service, cuisine, événements RP.'],
    ['Immobilier', 'Villas & apparts', '520 $', 'Bientôt', 'Visites, contrats, locations.']
  ];

  const items = [
    ['Clé Villa', 'documents', 'Accès Palm District', 1],
    ['Burger', 'food', 'Restaure l’énergie', 3],
    ['Bandage', 'care', 'Premier soin rapide', 2],
    ['Radio Pro', 'tool', 'Communication métier', 1],
    ['Carte SIM', 'tech', 'ParadisePhone', 1],
    ['Contrat', 'documents', 'Document RP', 1]
  ];

  const ensureLink = (id, href) => {
    let link = document.getElementById(id);
    if (!link) {
      link = document.createElement('link');
      link.id = id;
      link.rel = 'stylesheet';
      document.head.appendChild(link);
    }
    if (link.getAttribute('href') !== href) link.setAttribute('href', href);
  };

  const ensureCss = () => {
    ensureLink(CORE_CSS_ID, CORE_CSS_URL);
    ensureLink(COMPAT_CSS_ID, COMPAT_CSS_URL);
  };

  const mergeData = raw => ({
    ...DEFAULT_DATA,
    ...(raw || {}),
    health: { ...DEFAULT_DATA.health, ...(raw?.health || {}) },
    energy: { ...DEFAULT_DATA.energy, ...(raw?.energy || {}) },
    money: { ...DEFAULT_DATA.money, ...(raw?.money || {}) }
  });

  const avatarUrl = data => {
    const look = String(data?.look || '').trim();
    if (look && /^[a-z0-9.\-]+$/i.test(look)) return `../avatar-image.php?figure=${encodeURIComponent(look)}&direction=2&head_direction=3&gesture=sml&action=std&size=l&headonly=1&hud=40`;
    return String(data.avatar_url || '');
  };

  const nativeChat = () => {
    const candidates = document.querySelectorAll('#root [data-pr-native-chat-live="1"], #root input[placeholder*="chat" i], #root textarea[placeholder*="chat" i], #root input[placeholder*="chatter" i], #root input[placeholder*="chatear" i]');
    return [...candidates].find(el => el && el.id !== 'prhud-chat-input' && !el.disabled && !el.readOnly) || null;
  };

  const setNativeChatValue = value => {
    const input = nativeChat();
    if (!input) return false;
    const text = String(value || '');
    try {
      input.focus({ preventScroll: true });
      const win = input.ownerDocument?.defaultView || window;
      const proto = input instanceof win.HTMLTextAreaElement ? win.HTMLTextAreaElement.prototype : win.HTMLInputElement.prototype;
      const setter = Object.getOwnPropertyDescriptor(proto, 'value')?.set;
      if (setter) setter.call(input, text); else input.value = text;
      input.dispatchEvent(new win.Event('input', { bubbles: true, composed: true }));
      input.dispatchEvent(new win.Event('change', { bubbles: true, composed: true }));
      input.setSelectionRange?.(text.length, text.length);
      return true;
    } catch (_) { return false; }
  };

  const notify = (type, title, text) => {
    state.notices = [{ id: `${Date.now()}-${Math.random()}`, type, title, text }, ...state.notices].slice(0, 3);
    render();
    window.setTimeout(() => {
      state.notices = state.notices.slice(0, 2);
      render();
    }, 3600);
  };

  const money = () => state.data.money || DEFAULT_DATA.money;

  const statLine = (label, value, current, max, type) => `
    <div class="prp-stat prp-stat-${safe(type)}">
      <span>${safe(label)}</span><b>${safe(value)}</b>
      <i><u style="width:${percent(current, max)}"></u></i>
    </div>`;

  const playerCard = () => {
    const d = state.data;
    const ava = avatarUrl(d);
    return `
      <section class="prp-player-card prp-card prp-float" aria-label="Personnage">
        <div class="prp-avatar-scene">
          <div class="prp-pixel-room"><span></span><i></i></div>
          ${ava ? `<img src="${safe(ava)}" alt="${safe(d.username)}">` : '<strong>PR</strong>'}
          <em>Niv. ${safe(d.level)}</em>
        </div>
        <div class="prp-player-info">
          <div class="prp-player-head">
            <span>Citoyen #${safe(d.citizen_id || 'PR-0001')}</span>
            <strong>${safe(d.username)}</strong>
            <small>${safe(d.role || 'Citoyen')} · ${safe(d.organization || 'Indépendant')}</small>
          </div>
          <div class="prp-stats">
            ${statLine('Vie', `${fmt(d.health.current)}/${fmt(d.health.max)}`, d.health.current, d.health.max, 'health')}
            ${statLine('Énergie', `${fmt(d.energy.current)}%`, d.energy.current, d.energy.max, 'energy')}
          </div>
        </div>
      </section>`;
  };

  const topBar = () => `
    <section class="prp-citybar prp-card" aria-label="Ville">
      <span>${icon('map')} ${safe(state.data.city || 'Paradise City')}</span>
      <b>${safe(state.data.district || 'Marina District')}</b>
      <span>${now()}</span>
    </section>`;

  const wallet = () => `
    <section class="prp-wallet" aria-label="Portefeuille">
      <button type="button" data-open="bank"><small>Banque</small><b>${fmt(money().bank)}</b></button>
      <button type="button" data-open="inventory"><small>Liquide</small><b>${fmt(money().cash)}</b></button>
      <button type="button" data-open="bank" class="is-accent">+</button>
    </section>`;

  const quickBar = () => `
    <nav class="prp-quickbar" aria-label="Actions rapides">
      ${quickActions.map(a => `<button type="button" title="${safe(a.label)}" data-rp-action="${safe(a.key)}" ${a.command ? `data-command="${safe(a.command)}"` : ''}>${icon(a.icon)}<small>${safe(a.label)}</small></button>`).join('')}
    </nav>`;

  const objectives = () => `
    <aside class="prp-objectives prp-card" aria-label="Progression RP">
      <header><span>Agenda RP</span><b>Aujourd’hui</b></header>
      <div><span>Présence en ville</span><i><u style="width:100%"></u></i><b>OK</b></div>
      <div><span>Interactions RP</span><i><u style="width:66%"></u></i><b>2/3</b></div>
      <div><span>Service métier</span><i><u style="width:18%"></u></i><b>6m</b></div>
    </aside>`;

  const dockBar = () => `
    <nav class="prp-dock" aria-label="Menu principal">
      ${dock.map(item => `<button type="button" class="${state.activeDock === item.key ? 'is-active' : ''}" data-dock="${safe(item.key)}" ${item.open ? `data-open="${safe(item.open)}"` : ''} ${item.command ? `data-command="${safe(item.command)}"` : ''}>${icon(item.icon)}<span>${safe(item.label)}</span></button>`).join('')}
    </nav>`;

  const notices = () => `
    <div class="prp-notices" aria-live="polite">
      ${state.notices.map(n => `<article class="prp-notice is-${safe(n.type)}"><b>${safe(n.title)}</b><span>${safe(n.text)}</span><i></i></article>`).join('')}
    </div>`;

  const phoneWindow = () => `
    <div class="prp-phone">
      <div class="prp-phone-status"><span>Paradise</span><b>${now()}</b><span>98%</span></div>
      <div class="prp-phone-hero"><small>ParadisePhone</small><strong>Marina en ligne</strong><span>3 nouvelles activités RP proches</span></div>
      <div class="prp-app-grid">
        ${[
          ['Messages','phone'], ['Banque','bank'], ['GPS','map'], ['Taxi','map'], ['Emploi','jobs'], ['Immo','city'], ['Social','user'], ['Réglages','cmd']
        ].map(([label, open]) => `<button type="button" data-open="${safe(open)}"><i>${safe(label[0])}</i><span>${safe(label)}</span></button>`).join('')}
      </div>
    </div>`;

  const bankWindow = () => `
    <div class="prp-bank-grid">
      <section class="prp-balance-card"><small>Solde principal</small><strong>${fmt(money().bank)} $</strong><span>IBAN RP · PR-${safe(state.data.citizen_id || '0001')}</span></section>
      <section class="prp-balance-card is-savings"><small>Épargne</small><strong>${fmt(money().savings)} $</strong><span>Objectif Villa Paradise Bay</span></section>
      <div class="prp-actions-row"><button data-rp-action="transfer">Envoyer</button><button data-rp-action="receive">Recevoir</button><button data-rp-action="history">Historique</button></div>
      <section class="prp-list">
        ${[
          ['Restaurant Paradise','-42 $','neg'], ['Salaire Police','+650 $','pos'], ['Location appartement','-350 $','neg'], ['Prime activité RP','+125 $','pos']
        ].map(t => `<p><span>${safe(t[0])}</span><b class="${safe(t[2])}">${safe(t[1])}</b></p>`).join('')}
      </section>
    </div>`;

  const inventoryWindow = () => `
    <div class="prp-inventory-grid">
      <aside class="prp-inv-character"><div class="prp-mini-room"></div><strong>${safe(state.data.username)}</strong><span>Poids 8.4 / 25 kg</span></aside>
      <section class="prp-items">
        ${items.map((item, index) => `<button type="button" class="prp-item" data-item="${index}"><i>${safe(item[0].slice(0, 2).toUpperCase())}</i><b>${safe(item[0])}</b><small>x${safe(item[3])}</small></button>`).join('')}
      </section>
      <aside class="prp-item-info"><small>Objet sélectionné</small><strong>Clé Villa</strong><p>Accès Palm District. Utilisable devant une propriété liée.</p><button data-rp-action="use-item">Utiliser</button><button data-rp-action="give-item">Donner</button><button class="ghost" data-rp-action="drop-item">Jeter</button></aside>
    </div>`;

  const jobsWindow = () => `
    <div class="prp-job-grid">
      ${jobs.map(j => `<article class="prp-job-card"><div class="prp-job-scene"></div><small>${safe(j[1])}</small><strong>${safe(j[0])}</strong><p>${safe(j[4])}</p><footer><span>${safe(j[2])}</span><b>${safe(j[3])}</b></footer><button data-rp-action="apply-job">Postuler</button></article>`).join('')}
    </div>`;

  const documentsWindow = () => `
    <div class="prp-docs-grid">
      ${[
        ['Carte citoyenne', state.data.citizen_id || 'PR-0001', state.data.username],
        ['Permis conduire', 'VALIDE', 'Catégorie B'],
        ['Badge métier', state.data.role || 'Citoyen', state.data.organization || 'Indépendant']
      ].map(d => `<article class="prp-doc-card"><span>ParadiseRP</span><strong>${safe(d[0])}</strong><b>${safe(d[1])}</b><small>${safe(d[2])}</small></article>`).join('')}
    </div>`;

  const commandsWindow = () => `
    <div class="prp-commands">
      <label class="prp-search">${icon('search')}<input type="search" placeholder="Rechercher une commande, catégorie, exemple..."></label>
      <section class="prp-command-list">
        ${commands.map(c => `<article data-command-row="${safe(c.join(' ').toLowerCase())}"><small>${safe(c[0])}</small><button type="button" data-command="${safe(c[1])}">${safe(c[1])}</button><span>${safe(c[2])}</span><code>${safe(c[3])}</code></article>`).join('')}
      </section>
    </div>`;

  const cityWindow = () => `
    <div class="prp-city-grid">
      ${[
        ['Paradise Bay', 'Quartier riche · villas · marina'],
        ['Downtown', 'Commerces · banque · mairie'],
        ['Palm District', 'Résidentiel · agences · taxis'],
        ['South Beach', 'Quartier populaire · business RP']
      ].map(c => `<article><div class="prp-district-scene"></div><strong>${safe(c[0])}</strong><p>${safe(c[1])}</p><button data-rp-action="gps">Marquer GPS</button></article>`).join('')}
    </div>`;

  const profileWindow = () => `
    <div class="prp-profile-grid">
      <section class="prp-profile-main"><div class="prp-mini-room"></div><strong>${safe(state.data.username)}</strong><span>${safe(state.data.role)} · Réputation ${fmt(state.data.reputation)}</span></section>
      <section class="prp-profile-stats"><p><span>ID citoyen</span><b>${safe(state.data.citizen_id)}</b></p><p><span>Organisation</span><b>${safe(state.data.organization)}</b></p><p><span>Temps de jeu</span><b>${safe(state.data.playtime)}</b></p><p><span>Banque</span><b>${fmt(money().bank)} $</b></p></section>
    </div>`;

  const windowBody = key => ({
    phone: phoneWindow,
    bank: bankWindow,
    inventory: inventoryWindow,
    jobs: jobsWindow,
    documents: documentsWindow,
    commands: commandsWindow,
    city: cityWindow,
    profile: profileWindow
  }[key] || profileWindow)();

  const windowTitle = key => ({
    phone: ['ParadisePhone', 'Applications RP et services ville', 'phone'],
    bank: ['Paradise Bank', 'Comptes personnels et transactions', 'bank'],
    inventory: ['Inventaire', 'Objets, documents et actions rapides', 'bag'],
    jobs: ['Métiers', 'Carrières, salaires et candidatures', 'work'],
    documents: ['Documents RP', 'Identité, permis et badges', 'id'],
    commands: ['Commandes', 'Recherche et exemples utilisables', 'cmd'],
    city: ['Carte ville', 'Quartiers, GPS et zones RP', 'map'],
    profile: ['Personnage', 'Identité citoyenne et progression', 'user']
  }[key] || ['ParadiseRP', 'Interface RP', 'home']);

  const windowLayer = () => {
    if (!state.active) return '<div class="prp-window-layer"></div>';
    const [title, subtitle, iconName] = windowTitle(state.active);
    return `
      <div class="prp-window-layer is-open">
        <section class="prp-window ${state.active === 'phone' ? 'is-phone-window' : ''}" role="dialog" aria-label="${safe(title)}">
          <header class="prp-window-head">
            <div>${icon(iconName)}<span><b>${safe(title)}</b><small>${safe(subtitle)}</small></span></div>
            <button type="button" data-close aria-label="Fermer">${icon('close')}</button>
          </header>
          <main>${windowBody(state.active)}</main>
        </section>
      </div>`;
  };

  const build = () => `
    <div class="prp-shell" data-version="${safe(VERSION)}">
      <div class="prp-skyline" aria-hidden="true"><span></span><i></i><b></b></div>
      ${playerCard()}
      ${topBar()}
      ${wallet()}
      ${quickBar()}
      ${objectives()}
      <section class="prp-pocket"><span>Cash <b>${fmt(money().cash)} $</b></span><span>Pixels <b>${fmt(money().pixels)}</b></span></section>
      ${dockBar()}
      <button class="prp-floating-profile" type="button" data-open="profile">${icon('user')} Profil</button>
      ${windowLayer()}
      ${notices()}
    </div>`;

  const attachCommandSearch = root => {
    const input = root.querySelector('.prp-commands input');
    if (!input) return;
    input.addEventListener('input', () => {
      const q = input.value.trim().toLowerCase();
      root.querySelectorAll('[data-command-row]').forEach(row => {
        row.hidden = q && !row.getAttribute('data-command-row').includes(q);
      });
    });
  };

  const bind = root => {
    root.querySelectorAll('[data-open]').forEach(el => {
      el.addEventListener('click', event => {
        event.preventDefault();
        const key = el.getAttribute('data-open');
        state.active = key;
        const dockItem = dock.find(item => item.open === key);
        if (dockItem) state.activeDock = dockItem.key;
        render();
      });
    });
    root.querySelectorAll('[data-close]').forEach(el => el.addEventListener('click', () => { state.active = null; render(); }));
    root.querySelectorAll('[data-command]').forEach(el => {
      el.addEventListener('click', event => {
        const command = el.getAttribute('data-command');
        if (!command) return;
        event.preventDefault();
        setNativeChatValue(command);
        notify('info', 'Commande prête', `${command} est placé dans le chat.`);
      });
    });
    root.querySelectorAll('[data-dock]').forEach(el => {
      el.addEventListener('click', () => {
        state.activeDock = el.getAttribute('data-dock') || 'home';
        if (state.activeDock === 'home') state.active = null;
      });
    });
    root.querySelectorAll('[data-rp-action]').forEach(el => el.addEventListener('click', () => notify('success', 'Action RP', 'Action enregistrée côté interface.')));
    attachCommandSearch(root);
  };

  let root;
  const render = () => {
    ensureCss();
    root = document.getElementById(HUD_ID);
    if (!root) {
      root = document.createElement('div');
      root.id = HUD_ID;
      document.body.appendChild(root);
    }
    root.innerHTML = build();
    bind(root);
  };

  const loadData = async () => {
    try {
      const response = await fetch(`${DATA_URL}?_=${Date.now()}`, { cache: 'no-store', credentials: 'same-origin' });
      if (!response.ok) throw new Error(String(response.status));
      const json = await response.json();
      if (json && typeof json === 'object') state.data = mergeData(json);
    } catch (_) {
      state.data = mergeData(state.data || DEFAULT_DATA);
    }
    render();
  };

  const boot = () => {
    ensureCss();
    state.data = mergeData(DEFAULT_DATA);
    render();
    loadData();
    setInterval(loadData, 6000);
    setInterval(() => { try { window.__paradiseNativeUiOffScan?.(); } catch (_) {} }, 1600);
    document.addEventListener('keydown', event => {
      if (event.key === 'Escape' && state.active) { state.active = null; render(); }
      if (event.key === 'F2') { event.preventDefault(); state.active = 'phone'; state.activeDock = 'phone'; render(); }
      if (event.key === 'F3') { event.preventDefault(); state.active = 'inventory'; state.activeDock = 'inventory'; render(); }
    });
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();