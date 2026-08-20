(() => {
  'use strict';

  const VERSION = '50.0.0-paradise-design-system';
  const HUD_ID = 'paradise-rp-hud';
  const STYLE_ID = 'paradise-rp-hud-css';
  const CSS_URL = './paradise-rp-hud.css?v=50';
  const DATA_URL = '../rp-hud-data.php';

  const DEFAULT_DATA = {
    ok: false,
    id: 0,
    citizen_id: 'PR-00000',
    username: 'ParadiseRP',
    role: 'Citoyen',
    motto: '',
    level: 1,
    look: '',
    avatar_url: '',
    health: { current: 100, max: 100 },
    energy: { current: 100, max: 100 },
    money: { credits: 0, pixels: 0 },
    city: 'Paradise City',
    time: ''
  };

  const state = {
    data: DEFAULT_DATA,
    open: null,
    phoneApp: 'home',
    inventoryCategory: 'all',
    commandQuery: '',
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
  const now = () => new Date().toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' });

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
    check: '<path d="m5 12 4 4L19 6"/>',
    alert: '<path d="M12 3 2.8 20h18.4L12 3Z"/><path d="M12 9v5M12 17h.01"/>',
    spark: '<path d="m12 3 1.6 4.4L18 9l-4.4 1.6L12 15l-1.6-4.4L6 9l4.4-1.6L12 3ZM18.5 14l.8 2.2 2.2.8-2.2.8-.8 2.2-.8-2.2-2.2-.8 2.2-.8.8-2.2Z"/>'
  };

  const icon = (name, cls = '') => `<span class="prx-icon ${cls}"><svg viewBox="0 0 24 24" aria-hidden="true">${ICONS[name] || ICONS.spark}</svg></span>`;

  const APPS = [
    ['messages', 'Messages', 'message'], ['contacts', 'Contacts', 'contacts'], ['bank', 'Banque', 'wallet'],
    ['taxi', 'Taxi', 'taxi'], ['jobs', 'Emplois', 'briefcase'], ['housing', 'Immobilier', 'home'],
    ['social', 'Paradise Social', 'social'], ['settings', 'Paramètres', 'settings']
  ];

  const COMMANDS = [
    { name: ':me', cat: 'RP', desc: 'Afficher une action RP.', syntax: ':me action' },
    { name: ':give', cat: 'RP', desc: 'Donner un objet à un joueur.', syntax: ':give joueur objet' },
    { name: ':pay', cat: 'Social', desc: "Donner de l'argent à un joueur.", syntax: ':pay joueur montant' },
    { name: ':tel', cat: 'Général', desc: 'Ouvrir le téléphone.', syntax: ':tel' },
    { name: ':id', cat: 'Général', desc: "Afficher votre carte d'identité.", syntax: ':id' },
    { name: ':trabajar', cat: 'Travail', desc: 'Accéder aux actions de métier.', syntax: ':trabajar' },
    { name: ':commands', cat: 'Général', desc: 'Ouvrir cette liste de commandes.', syntax: ':commands' }
  ];

  const JOBS = [
    ['Police', 'Sécurité publique et interventions.', 'Service public'],
    ['EMS', 'Soins, urgences et assistance médicale.', 'Service public'],
    ['Taxi', 'Transport des citoyens dans Paradise City.', 'Ouvert'],
    ['Restaurant', 'Service, cuisine et gestion commerciale.', 'Ouvert'],
    ['Mécanicien', 'Entretien et assistance véhicules.', 'Ouvert'],
    ['Immobilier', 'Vente, location et visites de biens.', 'Ouvert']
  ];

  function ensureCss() {
    let link = document.getElementById(STYLE_ID);
    if (!link) {
      link = document.createElement('link');
      link.id = STYLE_ID;
      link.rel = 'stylesheet';
      document.head.appendChild(link);
    }
    if (!String(link.href || '').includes('v=50')) link.href = CSS_URL;
  }

  function avatarUrl(data = state.data) {
    const look = String(data.look || '').trim();
    if (look && /^[a-z0-9.\-]+$/i.test(look)) {
      return `../avatar-image.php?figure=${encodeURIComponent(look)}&direction=2&head_direction=3&gesture=sml&action=std&size=l&headonly=1&hud=50`;
    }
    return String(data.avatar_url || '');
  }

  function nativeChat() {
    try {
      return [...document.querySelectorAll('#root [data-pr-native-chat-live="1"], #root input[placeholder*="chat" i], #root textarea[placeholder*="chat" i]')]
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
    window.__prxToastTimer = window.setTimeout(() => { state.toast = null; render(); }, 3200);
  }

  const button = (label, iconName, action, variant = 'secondary', extra = '') =>
    `<button type="button" class="prx-btn prx-btn-${variant}" data-prx-action="${esc(action)}" ${extra}>${iconName ? icon(iconName) : ''}<span>${esc(label)}</span></button>`;

  function windowShell({ key, title, subtitle = '', iconName = 'spark', body, footer = '', cls = '' }) {
    return `<section class="prx-window ${esc(cls)}" data-prx-window="${esc(key)}" role="dialog" aria-label="${esc(title)}">
      <header class="prx-window-head">
        <div class="prx-window-title">${icon(iconName)}<div><strong>${esc(title)}</strong>${subtitle ? `<small>${esc(subtitle)}</small>` : ''}</div></div>
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
      key: 'profile', title: 'Profil citoyen', subtitle: 'Informations publiques', iconName: 'user', cls: 'prx-window-profile',
      body: `<div class="prx-profile-layout">
        <div class="prx-avatar-stage"><span class="prx-pixel-sun"></span>${ava ? `<img src="${esc(ava)}" alt="${esc(d.username)}">` : '<b>RP</b>'}<small>Niveau ${fmt(d.level)}</small></div>
        <div class="prx-profile-main">
          <div class="prx-profile-name"><span><strong>${esc(d.username)}</strong><small>${esc(d.motto || 'Citoyen de Paradise City')}</small></span><em>${esc(d.role || 'Citoyen')}</em></div>
          <div class="prx-info-list">
            <p><span>Identité citoyenne</span><b>${esc(citizen)}</b></p>
            <p><span>Statut</span><b>${esc(d.role || 'Citoyen')}</b></p>
            <p><span>Réputation</span><b>Niveau ${fmt(d.level)}</b></p>
            <p><span>Ville</span><b>${esc(d.city || 'Paradise City')}</b></p>
          </div>
        </div>
      </div>`,
      footer: `${button("Carte d'identité", 'id', 'command::id', 'secondary')}${button('Fermer', '', 'close', 'ghost')}`
    });
  }

  function bankView() {
    const d = state.data;
    const citizen = d.citizen_id || `PR-${String(num(d.id)).padStart(5, '0')}`;
    return windowShell({
      key: 'bank', title: 'Paradise Bank', subtitle: 'Compte personnel', iconName: 'wallet', cls: 'prx-window-bank',
      body: `<div class="prx-bank-hero"><small>Solde disponible</small><strong>${fmt(d.money?.credits)} $</strong><span>${esc(citizen)}</span></div>
        <div class="prx-segment"><button class="is-active">Envoyer</button><button>Recevoir</button><button>Historique</button></div>
        <form class="prx-transfer" data-prx-form="transfer">
          <label><span>Destinataire</span><input name="player" autocomplete="off" placeholder="Nom du joueur"></label>
          <label><span>Montant</span><input name="amount" inputmode="numeric" autocomplete="off" placeholder="250"></label>
          <button class="prx-btn prx-btn-primary" type="submit">${icon('send')}<span>Préparer le paiement</span></button>
        </form>
        <div class="prx-note">Le paiement est préparé dans le chat pour vous laisser confirmer la commande en jeu.</div>`
    });
  }

  function inventoryView() {
    const categories = [['all','Tout'],['food','Nourriture'],['clothes','Vêtements'],['tools','Outils'],['docs','Documents'],['keys','Clés']];
    return windowShell({
      key: 'inventory', title: 'Inventaire', subtitle: 'Vos objets', iconName: 'bag', cls: 'prx-window-inventory',
      body: `<div class="prx-inventory-tabs">${categories.map(([k,l]) => `<button type="button" class="${state.inventoryCategory === k ? 'is-active' : ''}" data-prx-category="${k}">${esc(l)}</button>`).join('')}</div>
        <div class="prx-inventory-grid">
          <button class="prx-item is-featured" type="button"><span class="prx-item-sprite">🥤</span><b>Eau fraîche</b><small>x2</small></button>
          <button class="prx-item" type="button"><span class="prx-item-sprite">🪪</span><b>Carte ID</b><small>Document</small></button>
          <button class="prx-item" type="button"><span class="prx-item-sprite">🔑</span><b>Clés</b><small>x1</small></button>
          <button class="prx-item prx-item-empty" type="button" disabled><span>+</span></button>
          <button class="prx-item prx-item-empty" type="button" disabled><span>+</span></button>
          <button class="prx-item prx-item-empty" type="button" disabled><span>+</span></button>
        </div>
        <div class="prx-item-detail"><div><strong>Sélectionnez un objet</strong><small>Les actions apparaîtront ici.</small></div><div class="prx-item-actions"><button disabled>Utiliser</button><button disabled>Donner</button><button disabled>Jeter</button></div></div>`
    });
  }

  function jobsView() {
    return windowShell({
      key: 'jobs', title: 'Métiers', subtitle: 'Trouver une activité à Paradise City', iconName: 'briefcase', cls: 'prx-window-jobs',
      body: `<div class="prx-job-list">${JOBS.map((j, i) => `<article class="prx-job-row"><div class="prx-job-art"><span>${['🚓','🚑','🚕','🍕','🔧','🏠'][i]}</span></div><div><strong>${esc(j[0])}</strong><p>${esc(j[1])}</p><small>${esc(j[2])}</small></div><button type="button" data-prx-action="command::trabajar">Voir</button></article>`).join('')}</div>`
    });
  }

  function commandsView() {
    const q = state.commandQuery.trim().toLowerCase();
    const list = COMMANDS.filter(c => !q || `${c.name} ${c.cat} ${c.desc}`.toLowerCase().includes(q));
    return windowShell({
      key: 'commands', title: 'Commandes', subtitle: 'Rechercher puis utiliser une commande', iconName: 'command', cls: 'prx-window-commands',
      body: `<label class="prx-search">${icon('search')}<input data-prx-command-search autocomplete="off" placeholder="Rechercher une commande..." value="${esc(state.commandQuery)}"></label>
        <div class="prx-command-list">${list.length ? list.map(c => `<button type="button" class="prx-command-row" data-prx-command="${esc(c.name)}"><code>${esc(c.name)}</code><span><strong>${esc(c.desc)}</strong><small>${esc(c.syntax)} · ${esc(c.cat)}</small></span>${icon('chevron')}</button>`).join('') : `<div class="prx-empty"><span>⌕</span><strong>Aucun résultat</strong><small>Essayez un autre mot-clé.</small></div>`}</div>`
    });
  }

  function docsView() {
    const d = state.data;
    const ava = avatarUrl(d);
    const citizen = d.citizen_id || `PR-${String(num(d.id)).padStart(5, '0')}`;
    return windowShell({
      key: 'documents', title: 'Documents', subtitle: 'Identité et licences', iconName: 'doc', cls: 'prx-window-docs',
      body: `<div class="prx-id-doc"><div class="prx-id-brand"><span class="prx-id-mark">P</span><div><b>PARADISE CITY</b><small>CARTE CITOYENNE</small></div></div><div class="prx-id-content">${ava ? `<img src="${esc(ava)}" alt="">` : '<span class="prx-id-avatar">RP</span>'}<div><strong>${esc(d.username)}</strong><p>${esc(citizen)}</p><small>${esc(d.role || 'Citoyen')}</small></div></div><div class="prx-id-footer">Valide · ParadiseRP</div></div>
        <div class="prx-doc-list"><button>${icon('id')}<span><b>Carte citoyenne</b><small>Disponible</small></span>${icon('chevron')}</button><button disabled>${icon('car')}<span><b>Permis de conduire</b><small>Aucune donnée synchronisée</small></span></button><button disabled>${icon('briefcase')}<span><b>Licence professionnelle</b><small>Aucune donnée synchronisée</small></span></button></div>`
    });
  }

  function simpleView(key) {
    const views = {
      housing: ['Immobilier', 'Trouvez votre prochain logement sans quitter votre room.', 'home', 'Aucun bien synchronisé', 'Les propriétés disponibles apparaîtront ici.'],
      garage: ['Garage', 'Vos véhicules personnels.', 'car', 'Aucun véhicule synchronisé', 'Vos véhicules apparaîtront ici.'],
      business: ['Entreprise', 'Gestion simple de votre activité.', 'building', 'Aucune entreprise liée', 'Rejoignez ou créez une entreprise pour accéder à cet espace.'],
      wardrobe: ['Dressing', 'Votre style, sans catalogue surchargé.', 'shirt', 'Aucune tenue chargée', 'Les vêtements synchronisés apparaîtront ici.']
    };
    const v = views[key] || views.business;
    return windowShell({ key, title: v[0], subtitle: v[1], iconName: v[2], body: `<div class="prx-empty prx-empty-large"><span>${icon(v[2])}</span><strong>${esc(v[3])}</strong><small>${esc(v[4])}</small></div>` });
  }

  function phoneView() {
    const d = state.data;
    const app = state.phoneApp;
    const appContent = app === 'home' ? `<div class="prx-phone-time"><strong>${esc(d.time || now())}</strong><small>Paradise City</small></div><div class="prx-phone-widget"><span>Solde</span><strong>${fmt(d.money?.credits)} $</strong></div><div class="prx-phone-apps">${APPS.map(a => `<button type="button" data-prx-phone-app="${a[0]}"><span>${icon(a[2])}</span><small>${esc(a[1])}</small></button>`).join('')}</div>` : `<div class="prx-phone-sub"><button type="button" data-prx-phone-app="home">‹ Accueil</button><div class="prx-empty"><span>${icon(APPS.find(a => a[0] === app)?.[2] || 'spark')}</span><strong>${esc(APPS.find(a => a[0] === app)?.[1] || 'Application')}</strong><small>${app === 'bank' ? `Solde actuel : ${fmt(d.money?.credits)} $` : 'Cette application utilise la nouvelle interface ParadiseRP.'}</small>${app === 'bank' ? button('Ouvrir la banque', 'wallet', 'open:bank', 'primary') : ''}${app === 'jobs' ? button('Voir les métiers', 'briefcase', 'open:jobs', 'primary') : ''}${app === 'housing' ? button('Voir les biens', 'home', 'open:housing', 'primary') : ''}</div></div>`;
    return `<section class="prx-phone" role="dialog" aria-label="ParadisePhone"><div class="prx-phone-frame"><header><span>${esc(d.time || now())}</span><i></i><button type="button" data-prx-action="close">${icon('close')}</button></header><main>${appContent}</main><footer><button type="button" data-prx-phone-app="home"><span></span></button></footer></div></section>`;
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
    return `<div class="prx-shell" data-prx-version="${VERSION}">
      <div class="prx-hud-top">
        <button type="button" class="prx-player-pill" data-prx-action="open:profile">${ava ? `<img src="${esc(ava)}" alt="${esc(d.username)}">` : '<span class="prx-avatar-fallback">RP</span>'}<span><strong>${esc(d.username)}</strong><small>${esc(d.role || 'Citoyen')}</small></span>${icon('chevron')}</button>
        <div class="prx-hud-status"><button type="button" data-prx-action="open:bank"><small>Solde</small><strong>${fmt(d.money?.credits)} $</strong></button><button type="button" data-prx-action="open:jobs"><small>Statut</small><strong>${esc(d.role || 'Citoyen')}</strong></button></div>
      </div>

      <nav class="prx-quickbar" aria-label="Actions rapides">
        <button type="button" data-prx-action="open:phone" data-tip="Téléphone · F2">${icon('phone')}<small>Téléphone</small></button>
        <button type="button" data-prx-action="open:inventory" data-tip="Inventaire · F3">${icon('bag')}<small>Inventaire</small></button>
        <button type="button" class="prx-quick-main" data-prx-action="open:profile" data-tip="Profil · F4">${ava ? `<img src="${esc(ava)}" alt="">` : icon('user')}<small>Profil</small></button>
        <button type="button" data-prx-action="open:jobs" data-tip="Métiers">${icon('briefcase')}<small>Métiers</small></button>
        <button type="button" data-prx-action="open:commands" data-tip="Commandes · Ctrl+K">${icon('command')}<small>Commandes</small></button>
      </nav>

      <button type="button" class="prx-more" data-prx-action="toggle-more" aria-label="Plus d'actions">${icon('plus')}</button>
      <div class="prx-more-menu" hidden>
        ${button('Banque', 'wallet', 'open:bank', 'ghost')}${button('Documents', 'doc', 'open:documents', 'ghost')}${button('Immobilier', 'home', 'open:housing', 'ghost')}${button('Garage', 'car', 'open:garage', 'ghost')}${button('Entreprise', 'building', 'open:business', 'ghost')}${button('Dressing', 'shirt', 'open:wardrobe', 'ghost')}
      </div>

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
    window.setTimeout(() => document.querySelector(`#${HUD_ID} [data-prx-command-search]`)?.focus(), state.open === 'commands' ? 40 : 0);
  }

  function open(key) {
    state.open = key;
    if (key === 'phone') state.phoneApp = 'home';
    render();
  }

  function actionFrom(value, root) {
    if (!value) return;
    if (value === 'close') { state.open = null; render(); return; }
    if (value.startsWith('open:')) { open(value.slice(5)); return; }
    if (value.startsWith('command:')) {
      const command = value.slice(8);
      state.open = null;
      render();
      if (setNativeChat(command)) toast('Commande prête', 'Appuyez sur Entrée pour confirmer.', 'success');
      else toast('Chat indisponible', "Le champ de discussion n'a pas été trouvé.", 'danger');
      return;
    }
    if (value === 'toggle-more') {
      const menu = root.querySelector('.prx-more-menu');
      if (menu) menu.hidden = !menu.hidden;
    }
  }

  function bindRoot(root) {
    root.onclick = event => {
      const actionButton = event.target.closest('[data-prx-action]');
      if (actionButton) { actionFrom(actionButton.getAttribute('data-prx-action'), root); return; }
      const appButton = event.target.closest('[data-prx-phone-app]');
      if (appButton) { state.phoneApp = appButton.getAttribute('data-prx-phone-app') || 'home'; render(); return; }
      const categoryButton = event.target.closest('[data-prx-category]');
      if (categoryButton) { state.inventoryCategory = categoryButton.getAttribute('data-prx-category') || 'all'; render(); return; }
      const commandButton = event.target.closest('[data-prx-command]');
      if (commandButton) {
        const command = commandButton.getAttribute('data-prx-command') || '';
        state.open = null; render();
        if (setNativeChat(command)) toast('Commande prête', command, 'success');
        return;
      }
      if (event.target.classList.contains('prx-layer') && state.open && state.open !== 'phone') { state.open = null; render(); }
    };

    root.oninput = event => {
      if (event.target.matches('[data-prx-command-search]')) {
        state.commandQuery = event.target.value || '';
        const list = root.querySelector('.prx-command-list');
        if (!list) return;
        const q = state.commandQuery.trim().toLowerCase();
        const filtered = COMMANDS.filter(c => !q || `${c.name} ${c.cat} ${c.desc}`.toLowerCase().includes(q));
        list.innerHTML = filtered.length ? filtered.map(c => `<button type="button" class="prx-command-row" data-prx-command="${esc(c.name)}"><code>${esc(c.name)}</code><span><strong>${esc(c.desc)}</strong><small>${esc(c.syntax)} · ${esc(c.cat)}</small></span>${icon('chevron')}</button>`).join('') : `<div class="prx-empty"><span>⌕</span><strong>Aucun résultat</strong><small>Essayez un autre mot-clé.</small></div>`;
      }
    };

    root.onsubmit = event => {
      const form = event.target.closest('[data-prx-form="transfer"]');
      if (!form) return;
      event.preventDefault();
      const fd = new FormData(form);
      const player = String(fd.get('player') || '').trim();
      const amount = String(fd.get('amount') || '').replace(/[^0-9]/g, '');
      if (!player || !amount || Number(amount) <= 0) { toast('Paiement incomplet', 'Renseignez un joueur et un montant.', 'danger'); return; }
      state.open = null; render();
      const command = `:pay ${player} ${amount}`;
      if (setNativeChat(command)) toast('Paiement préparé', command, 'success');
      else toast('Chat indisponible', 'Impossible de préparer le paiement.', 'danger');
    };
  }

  function keyboard(event) {
    if (event.key === 'Escape' && state.open) { event.preventDefault(); state.open = null; render(); return; }
    if (event.key === 'F2') { event.preventDefault(); open('phone'); return; }
    if (event.key === 'F3') { event.preventDefault(); open('inventory'); return; }
    if (event.key === 'F4') { event.preventDefault(); open('profile'); return; }
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
    const typingWindow = active && active.closest?.(`#${HUD_ID} .prx-window, #${HUD_ID} .prx-phone`);
    if (!typingWindow) render();
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