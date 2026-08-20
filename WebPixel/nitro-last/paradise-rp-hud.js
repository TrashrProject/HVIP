(() => {
  'use strict';

  const VERSION = '80.0.0-paradise-ui-v4-modern';
  const HUD_ID = 'paradise-rp-hud';
  const STYLE_ID = 'paradise-rp-hud-css';
  const CSS_URL = './paradise-rp-hud.css?v=80';
  const DATA_URL = '../rp-hud-data.php';

  const DEFAULT_DATA = {
    ok: false,
    id: 1024,
    citizen_id: 'PR-01024',
    username: 'Luiz',
    role: 'Staff',
    level: 7,
    look: '',
    avatar_url: '',
    health: { current: 100, max: 100 },
    energy: { current: 82, max: 100 },
    money: { credits: 500, pixels: 0, cash: 500, bank: 12450, diamonds: 0 },
    city: 'Paradise City',
    district: 'Downtown Marina',
    room: 'Paradise Hospital',
    players: 14,
    time: ''
  };

  const state = {
    data: DEFAULT_DATA,
    open: null,
    more: false,
    phoneApp: 'home',
    inventoryCategory: 'all',
    commandQuery: '',
    commandCategory: 'Toutes',
    selectedItem: 'phone',
    toast: null
  };

  const esc = value => String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;');
  const num = value => Number.isFinite(Number(value)) ? Number(value) : 0;
  const fmt = value => new Intl.NumberFormat('fr-FR').format(num(value));
  const clock = () => new Date().toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' });
  const day = () => new Date().toLocaleDateString('fr-FR', { weekday: 'long', day: '2-digit', month: 'long' });

  const paths = {
    user: '<path d="M12 12a4 4 0 1 0 0-8 4 4 0 0 0 0 8Z"/><path d="M4.5 20c.8-4 3.2-6 7.5-6s6.7 2 7.5 6"/>',
    phone: '<rect x="7" y="2.8" width="10" height="18.4" rx="2"/><path d="M10.4 5h3.2M11 18.2h2"/>',
    bag: '<path d="M6 8h12l1 12H5L6 8Z"/><path d="M9 8V6a3 3 0 0 1 6 0v2"/>',
    doc: '<path d="M6 3h8l4 4v14H6V3Z"/><path d="M14 3v5h5M9 12h6M9 16h6"/>',
    car: '<path d="M5 9h14l2 4v5H3v-5l2-4Z"/><path d="m7 9 1.5-4h7L17 9M6 18v2M18 18v2"/>',
    briefcase: '<rect x="3" y="7" width="18" height="12" rx="2"/><path d="M9 7V5.5A1.5 1.5 0 0 1 10.5 4h3A1.5 1.5 0 0 1 15 5.5V7M3 12h18"/>',
    command: '<path d="m5 7 5 5-5 5M12 17h7"/>',
    wallet: '<path d="M4 6.5h14a2 2 0 0 1 2 2V18H5.5A2.5 2.5 0 0 1 3 15.5v-9A2.5 2.5 0 0 1 5.5 4H18"/><path d="M15 11h5v4h-5a2 2 0 0 1 0-4Z"/>',
    bell: '<path d="M18 9a6 6 0 0 0-12 0c0 7-2.5 7-2.5 7h17S18 16 18 9Z"/><path d="M10 20a2 2 0 0 0 4 0"/>',
    plus: '<path d="M12 5v14M5 12h14"/>',
    close: '<path d="m6 6 12 12M18 6 6 18"/>',
    search: '<circle cx="11" cy="11" r="6"/><path d="m16 16 4 4"/>',
    chat: '<path d="M4 5h16v11H9l-5 4V5Z"/>',
    home: '<path d="m3.5 11 8.5-7 8.5 7"/><path d="M6 10v10h12V10M10 20v-6h4v6"/>',
    group: '<path d="M9 11a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7Z"/><path d="M3.8 20c.5-4 2.3-6 5.2-6s4.7 2 5.2 6"/><path d="M16 11.5a3 3 0 1 0-.8-5.8M15.5 14c2.2.4 3.7 2.1 4.7 5"/>',
    shield: '<path d="M12 3 20 6v6c0 4.6-3 7.2-8 9-5-1.8-8-4.4-8-9V6l8-3Z"/>',
    sun: '<circle cx="12" cy="12" r="4"/><path d="M12 2v2M12 20v2M4 12H2M22 12h-2M5 5l1.4 1.4M17.6 17.6 19 19M19 5l-1.4 1.4M6.4 17.6 5 19"/>',
    pin: '<path d="M12 21s6-5.3 6-11a6 6 0 0 0-12 0c0 5.7 6 11 6 11Z"/><circle cx="12" cy="10" r="2"/>',
    copy: '<rect x="8" y="8" width="11" height="11" rx="2"/><path d="M5 15H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v1"/>',
    spark: '<path d="m12 3 1.5 4.5L18 9l-4.5 1.5L12 15l-1.5-4.5L6 9l4.5-1.5L12 3Z"/>'
  };
  const icon = (name, extra = '') => `<span class="pr4-icon ${extra}"><svg viewBox="0 0 24 24" aria-hidden="true">${paths[name] || paths.spark}</svg></span>`;

  const dockLeft = [
    { key: 'inventory', label: 'Inventaire', icon: 'bag', shortcut: 'I' },
    { key: 'phone', label: 'Téléphone', icon: 'phone', shortcut: 'P' },
    { key: 'documents', label: 'Documents', icon: 'doc', shortcut: 'D' },
    { key: 'vehicles', label: 'Véhicules', icon: 'car', shortcut: 'V' }
  ];
  const dockRight = [
    { key: 'social', label: 'Social', icon: 'group' },
    { key: 'notifications', label: 'Notifications', icon: 'bell' },
    { key: 'more', label: 'Actions', icon: 'plus' }
  ];
  const moreActions = [
    ['bank', 'Banque', 'wallet'], ['jobs', 'Métiers', 'briefcase'], ['housing', 'Propriétés', 'home'],
    ['documents', 'Documents', 'doc'], ['vehicles', 'Véhicules', 'car'], ['commands', 'Commandes', 'command']
  ];
  const commands = [
    { name: ':me', cat: 'RP', title: 'Action RP', desc: 'Afficher une action roleplay.', syntax: ':me regarde autour de lui' },
    { name: ':pay', cat: 'Social', title: 'Payer un joueur', desc: "Donner de l'argent à un citoyen.", syntax: ':pay Luiz 500' },
    { name: ':give', cat: 'RP', title: 'Donner un objet', desc: 'Donner un objet à proximité.', syntax: ':give Luiz telephone' },
    { name: ':tel', cat: 'Général', title: 'Téléphone', desc: 'Ouvrir le ParadisePhone.', syntax: ':tel' },
    { name: ':id', cat: 'Général', title: 'Carte ID', desc: 'Présenter votre identité.', syntax: ':id' },
    { name: ':trabajar', cat: 'Travail', title: 'Métier', desc: 'Accéder aux actions de métier.', syntax: ':trabajar' },
    { name: ':commands', cat: 'Général', title: 'Commandes', desc: 'Ouvrir le Command Center.', syntax: ':commands' }
  ];
  const items = [
    { id: 'phone', name: 'ParadisePhone', qty: 1, type: 'outil', sprite: 'phone' },
    { id: 'id', name: 'Carte ID', qty: 1, type: 'doc', sprite: 'card' },
    { id: 'water', name: 'Eau fraîche', qty: 2, type: 'food', sprite: 'drink' },
    { id: 'keys', name: 'Clés villa', qty: 1, type: 'keys', sprite: 'key' },
    { id: 'radio', name: 'Radio RP', qty: 1, type: 'tool', sprite: 'radio' }
  ];
  const jobs = [
    ['Police', 'Sécurité de Paradise City', '650 $', 'blue'],
    ['EMS', 'Soins et urgences', '620 $', 'coral'],
    ['Taxi', 'Transport citoyen', '420 $', 'gold'],
    ['Mécano', 'Réparation véhicules', '520 $', 'orange'],
    ['Restaurant', 'Service et cuisine', '380 $', 'green'],
    ['Immobilier', 'Logements et visites', '550 $', 'aqua']
  ];

  function ensureCss() {
    let link = document.getElementById(STYLE_ID);
    if (!link) {
      link = document.createElement('link');
      link.id = STYLE_ID;
      link.rel = 'stylesheet';
      document.head.appendChild(link);
    }
    if (!String(link.getAttribute('href') || '').includes('v=80')) link.href = CSS_URL;
  }

  function avatarUrl(data = state.data) {
    const look = String(data.look || '').trim();
    if (look && /^[a-z0-9.\-]+$/i.test(look)) {
      return `../avatar-image.php?figure=${encodeURIComponent(look)}&direction=2&head_direction=3&gesture=sml&action=std&size=l&hud=80`;
    }
    return String(data.avatar_url || '');
  }

  function nativeChat() {
    try {
      return [...document.querySelectorAll('#root [data-pr-native-chat-live="1"], #root input[placeholder*="chat" i], #root textarea[placeholder*="chat" i], #root input[placeholder*="chatter" i]')]
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

  function showToast(title, body = '', tone = 'info') {
    state.toast = { title, body, tone };
    render();
    window.clearTimeout(window.__pr4Toast);
    window.__pr4Toast = window.setTimeout(() => { state.toast = null; render(); }, 3300);
  }

  const sprite = type => `<span class="pr4-sprite pr4-sprite-${esc(type)}"><i></i></span>`;
  const btn = (label, action, iconName = '', variant = 'secondary') => `<button type="button" class="pr4-btn pr4-btn-${variant}" data-pr4-action="${esc(action)}">${iconName ? icon(iconName) : ''}<span>${esc(label)}</span></button>`;

  function windowShell({ key, title, subtitle = '', iconName = 'spark', tone = 'aqua', body = '', footer = '' }) {
    return `<section class="pr4-window pr4-window-${esc(key)} tone-${esc(tone)}" role="dialog" aria-label="${esc(title)}">
      <header class="pr4-window-head">
        <div class="pr4-titlemark">${icon(iconName)}<div><strong>${esc(title)}</strong>${subtitle ? `<small>${esc(subtitle)}</small>` : ''}</div></div>
        <button type="button" class="pr4-close" data-pr4-action="close" aria-label="Fermer">${icon('close')}</button>
      </header>
      <div class="pr4-window-body">${body}</div>
      ${footer ? `<footer class="pr4-window-foot">${footer}</footer>` : ''}
    </section>`;
  }

  function profileView() {
    const d = state.data;
    const ava = avatarUrl(d);
    return windowShell({
      key: 'profile', title: 'Profil Paradise', subtitle: 'Fiche citoyenne', iconName: 'user', tone: 'aqua',
      body: `<div class="pr4-profile-grid">
        <aside class="pr4-profile-stage"><div class="pr4-mini-room"><b></b><em></em><span></span></div>${ava ? `<img src="${esc(ava)}" alt="${esc(d.username)}">` : '<strong>RP</strong>'}</aside>
        <section class="pr4-profile-info"><div class="pr4-profile-name"><h2>${esc(d.username)}</h2><span>${esc(d.role || 'Citoyen')}</span></div>
        <div class="pr4-tabs"><button class="is-active">Profil</button><button>Documents</button><button>Badges</button></div>
        <div class="pr4-data-list"><p><span>Identité</span><b>${esc(d.citizen_id || 'PR-01024')}</b></p><p><span>Quartier</span><b>${esc(d.district || 'Downtown Marina')}</b></p><p><span>Compte</span><b>${fmt(d.money?.bank || 12450)} $</b></p><p><span>Réputation</span><b>Niveau ${fmt(d.level || 1)}</b></p></div></section>
      </div>`,
      footer: `${btn('Documents', 'open:documents', 'doc')}${btn('Fermer', 'close', '', 'ghost')}`
    });
  }

  function inventoryView() {
    const selected = items.find(x => x.id === state.selectedItem) || items[0];
    const cats = [['all', 'Tous'], ['food', 'Nourriture'], ['tool', 'Outils'], ['doc', 'Documents'], ['keys', 'Clés']];
    return windowShell({
      key: 'inventory', title: 'Inventaire', subtitle: '28 / 50 kg', iconName: 'bag', tone: 'violet',
      body: `<div class="pr4-inv-layout"><nav class="pr4-inv-cats">${cats.map(c => `<button type="button" class="${state.inventoryCategory === c[0] ? 'is-active' : ''}" data-pr4-cat="${esc(c[0])}">${esc(c[1])}</button>`).join('')}</nav>
      <div class="pr4-slot-grid">${items.map(item => `<button type="button" class="pr4-slot ${state.selectedItem === item.id ? 'is-selected' : ''}" data-pr4-item="${esc(item.id)}">${sprite(item.sprite)}<b>${esc(item.name)}</b><small>x${fmt(item.qty)}</small></button>`).join('')}${Array.from({length:7}).map(() => '<button type="button" class="pr4-slot is-empty" disabled></button>').join('')}</div>
      <aside class="pr4-item-panel">${sprite(selected.sprite)}<strong>${esc(selected.name)}</strong><small>Objet ParadiseRP</small><p>Sélectionnez une action pour utiliser cet objet dans la room.</p><div>${btn('Utiliser','toast:Objet utilisé','spark','primary')}${btn('Donner','command::give','user')}${btn('Jeter','toast:Action annulée','close','danger')}</div></aside></div>`
    });
  }

  function phoneView() {
    const d = state.data;
    const apps = [
      ['messages','Messages','chat','blue'], ['bank','Banque','wallet','green'], ['taxi','Taxi','car','gold'], ['social','Social','group','aqua'], ['jobs','Jobs','briefcase','violet'], ['housing','Maison','home','coral']
    ];
    return `<section class="pr4-phone" role="dialog" aria-label="ParadisePhone"><div class="pr4-phone-frame"><header><span>${esc(d.time || clock())}</span><i></i><button type="button" data-pr4-action="close">${icon('close')}</button></header><main><section class="pr4-phone-hero"><small>${esc(day())}</small><strong>${esc(d.time || clock())}</strong><span>${esc(d.district || 'Paradise Marina')} · Soleil</span></section><div class="pr4-phone-wallet"><span>Paradise Bank</span><b>${fmt(d.money?.bank || 12450)} $</b></div><div class="pr4-phone-apps">${apps.map(a => `<button type="button" class="app-${a[3]}" data-pr4-action="${a[0] === 'bank' ? 'open:bank' : a[0] === 'jobs' ? 'open:jobs' : 'toast:Application bientôt prête'}">${icon(a[2])}<span>${esc(a[1])}</span></button>`).join('')}</div></main><footer><button type="button" data-pr4-action="close"></button></footer></div></section>`;
  }

  function bankView() {
    const d = state.data;
    return windowShell({
      key: 'bank', title: 'Paradise Bank', subtitle: 'Compte principal', iconName: 'wallet', tone: 'green',
      body: `<div class="pr4-bank-card"><small>Compte principal</small><strong>${fmt(d.money?.bank || 12450)} $</strong><span>${esc(d.citizen_id || 'PR-01024')}</span></div><div class="pr4-bank-actions">${btn('Envoyer','command::pay','wallet','primary')}${btn('Recevoir','toast:IBAN copié','copy')}${btn('Historique','toast:Historique bientôt prêt','doc')}</div><div class="pr4-transactions"><p><b>Salaire Staff</b><span>+500 $</span></p><p><b>Paradise Market</b><span>-42 $</span></p><p><b>Location Marina</b><span>-350 $</span></p></div>`
    });
  }

  function jobsView() {
    return windowShell({
      key: 'jobs', title: 'Métiers', subtitle: 'Catalogue RP', iconName: 'briefcase', tone: 'gold',
      body: `<div class="pr4-jobs">${jobs.map(j => `<article class="pr4-job tone-${j[3]}"><div class="pr4-job-scene"><span></span><i></i></div><div><strong>${esc(j[0])}</strong><p>${esc(j[1])}</p><small>${esc(j[2])} / service</small></div><button type="button" data-pr4-action="command::trabajar">Découvrir</button></article>`).join('')}</div>`
    });
  }

  function docsView() {
    const d = state.data;
    const ava = avatarUrl(d);
    return windowShell({
      key: 'documents', title: 'Documents RP', subtitle: 'Objets officiels', iconName: 'doc', tone: 'aqua',
      body: `<div class="pr4-docs-layout"><article class="pr4-id-card"><div class="brand"><b>PARADISE ID</b><span>CARTE CITOYENNE</span></div><div class="identity">${ava ? `<img src="${esc(ava)}" alt="">` : '<em>RP</em>'}<section><strong>${esc(d.username)}</strong><p>${esc(d.citizen_id || 'PR-01024')}</p><small>${esc(d.role || 'Citoyen')}</small></section></div><footer>ParadiseRP · valide</footer></article><div class="pr4-doc-stack"><button>${icon('doc')}<span><b>Carte d’identité</b><small>Disponible</small></span></button><button>${icon('car')}<span><b>Permis</b><small>Non synchronisé</small></span></button><button>${icon('wallet')}<span><b>Paradise Card</b><small>Disponible</small></span></button></div></div>`
    });
  }

  function vehiclesView() {
    return windowShell({
      key: 'vehicles', title: 'Véhicules', subtitle: 'Garage Paradise', iconName: 'car', tone: 'coral',
      body: `<div class="pr4-vehicle"><div class="pr4-car-sprite"><i></i></div><section><small>Cadillac</small><strong>El Dorado</strong><p>Plaque PR-204 · Carburant 82 % · Déverrouillé</p><div>${btn('Sortir','toast:Véhicule localisé','car','primary')}${btn('Clés','toast:Menu clés ouvert','key')}${btn('Localiser','toast:Position envoyée','pin')}</div></section></div>`
    });
  }

  function commandsView() {
    const cats = ['Toutes','Général','RP','Social','Travail'];
    const q = state.commandQuery.trim().toLowerCase();
    const list = commands.filter(c => (state.commandCategory === 'Toutes' || c.cat === state.commandCategory) && (!q || `${c.name} ${c.title} ${c.desc}`.toLowerCase().includes(q)));
    return windowShell({
      key: 'commands', title: 'Command Center', subtitle: 'Commandes ParadiseRP', iconName: 'command', tone: 'blue',
      body: `<div class="pr4-command-layout"><nav>${cats.map(c => `<button type="button" class="${state.commandCategory === c ? 'is-active' : ''}" data-pr4-command-cat="${esc(c)}">${esc(c)}</button>`).join('')}</nav><section><label class="pr4-search">${icon('search')}<input data-pr4-command-search value="${esc(state.commandQuery)}" placeholder="Rechercher une commande..."></label><div class="pr4-command-list">${list.map(c => `<button type="button" data-pr4-command="${esc(c.name)}"><code>${esc(c.name)}</code><span><b>${esc(c.title)}</b><small>${esc(c.desc)} · ${esc(c.syntax)}</small></span>${icon('copy')}</button>`).join('') || '<div class="pr4-empty">Aucune commande trouvée.</div>'}</div></section></div>`
    });
  }

  function simpleWindow(key, title, iconName, body) {
    return windowShell({ key, title, subtitle: 'ParadiseRP', iconName, body: `<div class="pr4-empty-state">${icon(iconName)}<strong>${esc(title)}</strong><p>${esc(body)}</p></div>` });
  }

  function activeView() {
    if (!state.open) return '';
    if (state.open === 'phone') return phoneView();
    if (state.open === 'profile') return profileView();
    if (state.open === 'inventory') return inventoryView();
    if (state.open === 'bank') return bankView();
    if (state.open === 'jobs') return jobsView();
    if (state.open === 'documents') return docsView();
    if (state.open === 'vehicles') return vehiclesView();
    if (state.open === 'commands') return commandsView();
    if (state.open === 'notifications') return simpleWindow('notifications', 'Notifications', 'bell', 'Votre centre de notifications Paradise apparaîtra ici.');
    if (state.open === 'social') return simpleWindow('social', 'Social', 'group', 'Amis, groupes et messages seront regroupés ici.');
    return simpleWindow(state.open, 'ParadiseRP', 'spark', 'Module en cours de synchronisation.');
  }

  function hud() {
    const d = state.data;
    const ava = avatarUrl(d);
    return `<div class="pr4-shell" data-version="${VERSION}">
      <button type="button" class="pr4-player-card" data-pr4-action="open:profile">
        <span class="pr4-player-avatar">${ava ? `<img src="${esc(ava)}" alt="${esc(d.username)}">` : '<b>RP</b>'}<i></i></span>
        <span class="pr4-player-text"><strong>${esc(d.username)}</strong><em>${esc(d.role || 'Citoyen')}</em><small>${esc(d.district || 'Downtown Marina')}</small></span>
      </button>

      <div class="pr4-status" aria-label="Paradise Status">
        <button type="button" class="cash" data-tip="Argent liquide" data-pr4-action="open:bank">${icon('wallet')}<b>${fmt(d.money?.cash ?? d.money?.credits ?? 0)} $</b></button>
        <button type="button" class="bank" data-tip="Compte bancaire" data-pr4-action="open:bank">${icon('wallet')}<b>${fmt(d.money?.bank || 12450)} $</b></button>
        <button type="button" class="staff" data-tip="Statut actuel" data-pr4-action="open:jobs">${icon('shield')}<b>${esc(d.role || 'Citoyen')}</b></button>
        <button type="button" class="time" data-tip="Heure RP">${icon('sun')}<b>${esc(d.time || clock())}</b></button>
        <button type="button" class="notif" data-tip="Notifications" data-pr4-action="open:notifications">${icon('bell')}<b>3</b></button>
      </div>

      <button type="button" class="pr4-room" data-pr4-action="open:room">${icon('pin')}<span><strong>${esc(d.district || 'Downtown Marina')}</strong><small>${esc(d.room || 'ParadiseRP')} · ${fmt(d.players || 14)} joueurs</small></span></button>

      <div class="pr4-bottom">
        <nav class="pr4-dock pr4-dock-left">${ava ? `<button type="button" class="avatar" data-tip="Profil" data-pr4-action="open:profile"><img src="${esc(ava)}" alt=""></button>` : `<button type="button" class="avatar" data-pr4-action="open:profile"><b>RP</b></button>`}${dockLeft.map(x => `<button type="button" class="app app-${x.key}" data-tip="${esc(x.label)} · ${esc(x.shortcut)}" data-pr4-action="open:${esc(x.key)}">${icon(x.icon)}<span>${esc(x.label)}</span></button>`).join('')}</nav>
        <div class="pr4-chat-module"><span>LOCAL</span><i>${icon('chat')}</i><small>Discuter...</small></div>
        <nav class="pr4-dock pr4-dock-right">${dockRight.map(x => `<button type="button" class="${x.key === 'more' ? 'more' : 'app'}" data-tip="${esc(x.label)}" data-pr4-action="${x.key === 'more' ? 'toggle-more' : `open:${x.key}`}">${icon(x.icon)}<span>${esc(x.label)}</span></button>`).join('')}</nav>
      </div>

      <div class="pr4-more-menu" ${state.more ? '' : 'hidden'}>${moreActions.map(a => `<button type="button" data-pr4-action="open:${a[0]}">${icon(a[2])}<span>${esc(a[1])}</span></button>`).join('')}</div>
      <aside class="pr4-object-card"><header>${icon('car')}<b>Cadillac</b></header><div class="mini-car"></div><p>El Dorado · Luiz</p><div><button type="button" data-pr4-action="open:vehicles">Conduire</button><button type="button" data-pr4-action="toast:Clés ouvertes">Clés</button></div></aside>
      <div class="pr4-layer ${state.open ? 'is-open' : ''}">${activeView()}</div>
      ${state.toast ? `<div class="pr4-toast tone-${esc(state.toast.tone)}">${icon(state.toast.tone === 'danger' ? 'shield' : 'spark')}<span><strong>${esc(state.toast.title)}</strong>${state.toast.body ? `<small>${esc(state.toast.body)}</small>` : ''}</span><i></i></div>` : ''}
    </div>`;
  }

  function open(key) {
    state.open = key;
    state.more = false;
    if (key === 'room') state.open = null, showToast(state.data.district || 'Downtown Marina', `${state.data.room || 'ParadiseRP'} · ${fmt(state.data.players || 14)} joueurs`, 'info');
    else render();
  }

  function doAction(value, root) {
    if (!value) return;
    if (value === 'close') { state.open = null; state.more = false; render(); return; }
    if (value === 'toggle-more') { state.more = !state.more; render(); return; }
    if (value.startsWith('open:')) { open(value.slice(5)); return; }
    if (value.startsWith('toast:')) { showToast(value.slice(6), '', 'info'); return; }
    if (value.startsWith('command:')) {
      const command = value.slice(8);
      state.open = null; render();
      if (setNativeChat(command)) showToast('Commande prête', command, 'success');
      else showToast('Chat introuvable', 'Le champ Nitro est indisponible.', 'danger');
    }
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
    bind(root);
    if (state.open === 'commands') window.setTimeout(() => root.querySelector('[data-pr4-command-search]')?.focus(), 40);
  }

  function bind(root) {
    root.onclick = event => {
      const action = event.target.closest('[data-pr4-action]');
      if (action) { doAction(action.getAttribute('data-pr4-action'), root); return; }
      const item = event.target.closest('[data-pr4-item]');
      if (item) { state.selectedItem = item.getAttribute('data-pr4-item') || 'phone'; render(); return; }
      const cat = event.target.closest('[data-pr4-cat]');
      if (cat) { state.inventoryCategory = cat.getAttribute('data-pr4-cat') || 'all'; render(); return; }
      const command = event.target.closest('[data-pr4-command]');
      if (command) { doAction(`command:${command.getAttribute('data-pr4-command') || ''}`, root); return; }
      const ccat = event.target.closest('[data-pr4-command-cat]');
      if (ccat) { state.commandCategory = ccat.getAttribute('data-pr4-command-cat') || 'Toutes'; render(); return; }
      if (event.target.classList.contains('pr4-layer')) { state.open = null; render(); }
    };
    root.oninput = event => {
      if (event.target.matches('[data-pr4-command-search]')) {
        state.commandQuery = event.target.value || '';
        render();
      }
    };
  }

  function keydown(event) {
    const tag = event.target?.tagName;
    const typing = tag === 'INPUT' || tag === 'TEXTAREA' || event.target?.isContentEditable;
    if (event.key === 'Escape' && (state.open || state.more)) { event.preventDefault(); state.open = null; state.more = false; render(); return; }
    if (!typing) {
      const k = event.key.toLowerCase();
      if (k === 'p') { event.preventDefault(); open('phone'); return; }
      if (k === 'i') { event.preventDefault(); open('inventory'); return; }
      if (k === 'd') { event.preventDefault(); open('documents'); return; }
      if (k === 'v') { event.preventDefault(); open('vehicles'); return; }
      if (k === 'c' || ((event.ctrlKey || event.metaKey) && k === 'k')) { event.preventDefault(); open('commands'); return; }
    }
    if (event.key === 'Enter' && typing && !event.target.closest?.(`#${HUD_ID}`)) {
      const val = String(event.target.value || '').trim().toLowerCase();
      const map = { ':commands': 'commands', ':tel': 'phone', ':phone': 'phone', ':id': 'documents' };
      if (map[val]) { event.preventDefault(); event.stopImmediatePropagation(); setNativeChat('', false); open(map[val]); }
    }
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
    if (!document.activeElement?.closest?.(`#${HUD_ID} .pr4-window, #${HUD_ID} .pr4-phone`)) render();
  }

  function boot() {
    ensureCss();
    render();
    loadData(true);
    window.addEventListener('keydown', keydown, true);
    window.setInterval(() => loadData(false), 10000);
    window.setInterval(() => { try { window.__paradiseNativeUiOffScan?.(); } catch (_) {} }, 1600);
    window.__ParadiseRPUI = { open, toast: showToast, version: VERSION };
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();