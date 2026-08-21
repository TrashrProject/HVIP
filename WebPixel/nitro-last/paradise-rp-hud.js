(() => {
  'use strict';

  const VERSION = '81.0.0-stable-window-chat-runtime';
  const HUD_ID = 'paradise-rp-hud';
  const STYLE_ID = 'paradise-rp-hud-css';
  const CSS_URL = './paradise-rp-hud.css?v=80';
  const DATA_URL = '../rp-hud-data.php';

  if (window.__ParadiseRPHudBooted) return;
  window.__ParadiseRPHudBooted = VERSION;

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
    window: { key: null, phase: 'closed' },
    more: false,
    phoneApp: 'home',
    inventoryCategory: 'all',
    commandQuery: '',
    commandCategory: 'Toutes',
    selectedItem: 'phone',
    chatDraft: '',
    toastTimer: 0
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
    key: '<circle cx="8" cy="12" r="3"/><path d="M11 12h10M17 12v3M20 12v2"/>',
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
    { id: 'phone', name: 'ParadisePhone', qty: 1, type: 'tool', sprite: 'phone' },
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

  let root = null;

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

  function findNativeChat() {
    try {
      const explicit = document.querySelector('#root [data-pr-native-chat-bridge="1"]');
      if (explicit) return explicit;
      return [...document.querySelectorAll('#root input, #root textarea')]
        .find(el => {
          if (!el || el.disabled || el.readOnly || el.closest(`#${HUD_ID}`)) return false;
          const text = `${el.placeholder || ''} ${el.className || ''} ${el.id || ''}`;
          return /haz|chatear|chat|chatter|parler|message|say/i.test(text);
        }) || null;
    } catch (_) { return null; }
  }

  function setNativeValue(input, value) {
    const win = input.ownerDocument?.defaultView || window;
    const proto = input instanceof win.HTMLTextAreaElement ? win.HTMLTextAreaElement.prototype : win.HTMLInputElement.prototype;
    const setter = Object.getOwnPropertyDescriptor(proto, 'value')?.set;
    if (setter) setter.call(input, String(value ?? '')); else input.value = String(value ?? '');
    input.dispatchEvent(new win.Event('input', { bubbles: true, composed: true }));
    input.dispatchEvent(new win.Event('change', { bubbles: true, composed: true }));
  }

  function sendChat(text) {
    const message = String(text || '').trim();
    if (!message) return true;
    const input = findNativeChat();
    if (!input) return false;
    try {
      const win = input.ownerDocument?.defaultView || window;
      setNativeValue(input, message);
      input.focus({ preventScroll: true });
      const init = { key: 'Enter', code: 'Enter', bubbles: true, cancelable: true, composed: true };
      input.dispatchEvent(new win.KeyboardEvent('keydown', init));
      input.dispatchEvent(new win.KeyboardEvent('keypress', init));
      input.dispatchEvent(new win.KeyboardEvent('keyup', init));
      window.setTimeout(() => { try { input.blur(); } catch (_) {} }, 0);
      return true;
    } catch (_) { return false; }
  }

  const sprite = type => `<span class="pr4-sprite pr4-sprite-${esc(type)}"><i></i></span>`;
  const btn = (label, action, iconName = '', variant = 'secondary') => `<button type="button" class="pr4-btn pr4-btn-${variant}" data-pr4-action="${esc(action)}">${iconName ? icon(iconName) : ''}<span>${esc(label)}</span></button>`;

  function windowShell({ key, title, subtitle = '', iconName = 'spark', tone = 'aqua', body = '', footer = '' }) {
    return `<section class="pr4-window pr4-window-${esc(key)} tone-${esc(tone)}" data-pr-window="${esc(key)}" role="dialog" aria-label="${esc(title)}"><header class="pr4-window-head"><div class="pr4-titlemark">${icon(iconName)}<div><strong>${esc(title)}</strong>${subtitle ? `<small>${esc(subtitle)}</small>` : ''}</div></div><button type="button" class="pr4-close" data-pr4-action="close" aria-label="Fermer">${icon('close')}</button></header><div class="pr4-window-body">${body}</div>${footer ? `<footer class="pr4-window-foot">${footer}</footer>` : ''}</section>`;
  }

  function profileView() {
    const d = state.data, ava = avatarUrl(d);
    return windowShell({ key:'profile', title:'Profil Paradise', subtitle:'Fiche citoyenne', iconName:'user', tone:'aqua', body:`<div class="pr4-profile-grid"><aside class="pr4-profile-stage"><div class="pr4-mini-room"><b></b><em></em><span></span></div>${ava ? `<img src="${esc(ava)}" alt="${esc(d.username)}">` : '<strong>RP</strong>'}</aside><section class="pr4-profile-info"><div class="pr4-profile-name"><h2 data-pr-bind="username">${esc(d.username)}</h2><span data-pr-bind="role">${esc(d.role || 'Citoyen')}</span></div><div class="pr4-tabs"><button type="button" class="is-active">Profil</button><button type="button">Documents</button><button type="button">Badges</button></div><div class="pr4-data-list"><p><span>Identité</span><b data-pr-bind="citizen">${esc(d.citizen_id || 'PR-01024')}</b></p><p><span>Quartier</span><b data-pr-bind="district">${esc(d.district || 'Downtown Marina')}</b></p><p><span>Compte</span><b data-pr-bind="bank">${fmt(d.money?.bank || 0)} $</b></p><p><span>Réputation</span><b data-pr-bind="level">Niveau ${fmt(d.level || 1)}</b></p></div></section></div>`, footer:`${btn('Documents','open:documents','doc')}${btn('Fermer','close','','ghost')}` });
  }

  function inventoryView() {
    const selected = items.find(x => x.id === state.selectedItem) || items[0];
    const cats = [['all','Tous'],['food','Nourriture'],['tool','Outils'],['doc','Documents'],['keys','Clés']];
    const filtered = state.inventoryCategory === 'all' ? items : items.filter(item => item.type === state.inventoryCategory);
    return windowShell({ key:'inventory', title:'Inventaire', subtitle:'28 / 50 kg', iconName:'bag', tone:'violet', body:`<div class="pr4-inv-layout"><nav class="pr4-inv-cats">${cats.map(c=>`<button type="button" class="${state.inventoryCategory===c[0]?'is-active':''}" data-pr4-cat="${esc(c[0])}">${esc(c[1])}</button>`).join('')}</nav><div class="pr4-slot-grid">${filtered.map(item=>`<button type="button" class="pr4-slot ${state.selectedItem===item.id?'is-selected':''}" data-pr4-item="${esc(item.id)}">${sprite(item.sprite)}<b>${esc(item.name)}</b><small>x${fmt(item.qty)}</small></button>`).join('')}${Array.from({length:Math.max(0,12-filtered.length)}).map(()=>'<button type="button" class="pr4-slot is-empty" disabled></button>').join('')}</div><aside class="pr4-item-panel">${sprite(selected.sprite)}<strong>${esc(selected.name)}</strong><small>Objet ParadiseRP</small><p>Sélectionnez une action pour utiliser cet objet dans la room.</p><div>${btn('Utiliser','toast:Objet utilisé','spark','primary')}${btn('Donner','command::give','user')}${btn('Jeter','toast:Action annulée','close','danger')}</div></aside></div>` });
  }

  function phoneView() {
    const d = state.data;
    const apps = [['messages','Messages','chat','blue'],['bank','Banque','wallet','green'],['taxi','Taxi','car','gold'],['social','Social','group','aqua'],['jobs','Jobs','briefcase','violet'],['housing','Maison','home','coral']];
    return `<section class="pr4-phone" data-pr-window="phone" role="dialog" aria-label="ParadisePhone"><div class="pr4-phone-frame"><header><span data-pr-bind="time">${esc(d.time||clock())}</span><i></i><button type="button" data-pr4-action="close">${icon('close')}</button></header><main><section class="pr4-phone-hero"><small>${esc(day())}</small><strong data-pr-bind="time">${esc(d.time||clock())}</strong><span><b data-pr-bind="district">${esc(d.district||'Paradise Marina')}</b> · Soleil</span></section><div class="pr4-phone-wallet"><span>Paradise Bank</span><b data-pr-bind="bank">${fmt(d.money?.bank||0)} $</b></div><div class="pr4-phone-apps">${apps.map(a=>`<button type="button" class="app-${a[3]}" data-pr4-action="${a[0]==='bank'?'open:bank':a[0]==='jobs'?'open:jobs':`phone:${a[0]}`}">${icon(a[2])}<span>${esc(a[1])}</span></button>`).join('')}</div></main><footer><button type="button" data-pr4-action="close"></button></footer></div></section>`;
  }

  function bankView() { const d=state.data; return windowShell({ key:'bank', title:'Paradise Bank', subtitle:'Compte principal', iconName:'wallet', tone:'green', body:`<div class="pr4-bank-card"><small>Compte principal</small><strong data-pr-bind="bank">${fmt(d.money?.bank||0)} $</strong><span data-pr-bind="citizen">${esc(d.citizen_id||'PR-01024')}</span></div><div class="pr4-bank-actions">${btn('Envoyer','command::pay','wallet','primary')}${btn('Recevoir','toast:IBAN copié','copy')}${btn('Historique','toast:Historique bientôt prêt','doc')}</div><div class="pr4-transactions"><p><b>Salaire Staff</b><span>+500 $</span></p><p><b>Paradise Market</b><span>-42 $</span></p><p><b>Location Marina</b><span>-350 $</span></p></div>` }); }
  function jobsView() { return windowShell({ key:'jobs', title:'Métiers', subtitle:'Catalogue RP', iconName:'briefcase', tone:'gold', body:`<div class="pr4-jobs">${jobs.map(j=>`<article class="pr4-job tone-${j[3]}"><div class="pr4-job-scene"><span></span><i></i></div><div><strong>${esc(j[0])}</strong><p>${esc(j[1])}</p><small>${esc(j[2])} / service</small></div><button type="button" data-pr4-action="command::trabajar">Découvrir</button></article>`).join('')}</div>` }); }
  function docsView() { const d=state.data, ava=avatarUrl(d); return windowShell({ key:'documents', title:'Documents RP', subtitle:'Objets officiels', iconName:'doc', tone:'aqua', body:`<div class="pr4-docs-layout"><article class="pr4-id-card"><div class="brand"><b>PARADISE ID</b><span>CARTE CITOYENNE</span></div><div class="identity">${ava?`<img src="${esc(ava)}" alt="">`:'<em>RP</em>'}<section><strong data-pr-bind="username">${esc(d.username)}</strong><p data-pr-bind="citizen">${esc(d.citizen_id||'PR-01024')}</p><small data-pr-bind="role">${esc(d.role||'Citoyen')}</small></section></div><footer>ParadiseRP · valide</footer></article><div class="pr4-doc-stack"><button type="button">${icon('doc')}<span><b>Carte d’identité</b><small>Disponible</small></span></button><button type="button">${icon('car')}<span><b>Permis</b><small>Non synchronisé</small></span></button><button type="button">${icon('wallet')}<span><b>Paradise Card</b><small>Disponible</small></span></button></div></div>` }); }
  function vehiclesView() { return windowShell({ key:'vehicles', title:'Véhicules', subtitle:'Garage Paradise', iconName:'car', tone:'coral', body:`<div class="pr4-vehicle"><div class="pr4-car-sprite"><i></i></div><section><small>Cadillac</small><strong>El Dorado</strong><p>Plaque PR-204 · Carburant 82 % · Déverrouillé</p><div>${btn('Sortir','toast:Véhicule localisé','car','primary')}${btn('Clés','toast:Menu clés ouvert','key')}${btn('Localiser','toast:Position envoyée','pin')}</div></section></div>` }); }

  function commandsView() {
    const cats=['Toutes','Général','RP','Social','Travail'];
    return windowShell({ key:'commands', title:'Command Center', subtitle:'Commandes ParadiseRP', iconName:'command', tone:'blue', body:`<div class="pr4-command-layout"><nav>${cats.map(c=>`<button type="button" class="${state.commandCategory===c?'is-active':''}" data-pr4-command-cat="${esc(c)}">${esc(c)}</button>`).join('')}</nav><section><label class="pr4-search">${icon('search')}<input data-pr4-command-search value="${esc(state.commandQuery)}" placeholder="Rechercher une commande..."></label><div class="pr4-command-list">${commands.map(c=>`<button type="button" data-pr4-command="${esc(c.name)}" data-pr-command-cat="${esc(c.cat)}" data-pr-command-text="${esc(`${c.name} ${c.title} ${c.desc}`.toLowerCase())}"><code>${esc(c.name)}</code><span><b>${esc(c.title)}</b><small>${esc(c.desc)} · ${esc(c.syntax)}</small></span>${icon('copy')}</button>`).join('')}<div class="pr4-empty" data-pr-command-empty hidden>Aucune commande trouvée.</div></div></section></div>` });
  }

  function simpleWindow(key,title,iconName,body){ return windowShell({ key,title,subtitle:'ParadiseRP',iconName,body:`<div class="pr4-empty-state">${icon(iconName)}<strong>${esc(title)}</strong><p>${esc(body)}</p></div>` }); }
  function activeView(){ const key=state.window.key; if(!key)return''; if(key==='phone')return phoneView(); if(key==='profile')return profileView(); if(key==='inventory')return inventoryView(); if(key==='bank')return bankView(); if(key==='jobs')return jobsView(); if(key==='documents')return docsView(); if(key==='vehicles')return vehiclesView(); if(key==='commands')return commandsView(); if(key==='notifications')return simpleWindow('notifications','Notifications','bell','Votre centre de notifications Paradise apparaîtra ici.'); if(key==='social')return simpleWindow('social','Social','group','Amis, groupes et messages seront regroupés ici.'); return simpleWindow(key,'ParadiseRP','spark','Module en cours de synchronisation.'); }

  function shellHtml(){ const d=state.data, ava=avatarUrl(d); return `<div class="pr4-shell" data-version="${VERSION}"><button type="button" class="pr4-player-card" data-pr4-action="open:profile"><span class="pr4-player-avatar">${ava?`<img data-pr-bind-img="avatar" src="${esc(ava)}" alt="${esc(d.username)}">`:'<b data-pr-avatar-fallback>RP</b>'}<i></i></span><span class="pr4-player-text"><strong data-pr-bind="username">${esc(d.username)}</strong><em data-pr-bind="role">${esc(d.role||'Citoyen')}</em><small data-pr-bind="district">${esc(d.district||'Downtown Marina')}</small></span></button><div class="pr4-status" aria-label="Paradise Status"><button type="button" class="cash" data-tip="Argent liquide" data-pr4-action="open:bank">${icon('wallet')}<b data-pr-bind="cash">${fmt(d.money?.cash??d.money?.credits??0)} $</b></button><button type="button" class="bank" data-tip="Compte bancaire" data-pr4-action="open:bank">${icon('wallet')}<b data-pr-bind="bank">${fmt(d.money?.bank||0)} $</b></button><button type="button" class="staff" data-tip="Statut actuel" data-pr4-action="open:jobs">${icon('shield')}<b data-pr-bind="role">${esc(d.role||'Citoyen')}</b></button><button type="button" class="time" data-tip="Heure RP">${icon('sun')}<b data-pr-bind="time">${esc(d.time||clock())}</b></button><button type="button" class="notif" data-tip="Notifications" data-pr4-action="open:notifications">${icon('bell')}<b data-pr-bind="notifications">0</b></button></div><button type="button" class="pr4-room" data-pr4-action="open:room">${icon('pin')}<span><strong data-pr-bind="district">${esc(d.district||'Downtown Marina')}</strong><small><b data-pr-bind="room">${esc(d.room||'ParadiseRP')}</b> · <b data-pr-bind="players">${fmt(d.players||0)} joueurs</b></small></span></button><div class="pr4-bottom"><nav class="pr4-dock pr4-dock-left">${ava?`<button type="button" class="avatar" data-tip="Profil" data-pr4-action="open:profile"><img data-pr-bind-img="avatar" src="${esc(ava)}" alt=""></button>`:`<button type="button" class="avatar" data-pr4-action="open:profile"><b>RP</b></button>`}${dockLeft.map(x=>`<button type="button" class="app app-${x.key}" data-tip="${esc(x.label)} · ${esc(x.shortcut)}" data-pr4-action="open:${esc(x.key)}">${icon(x.icon)}<span>${esc(x.label)}</span></button>`).join('')}</nav><label class="pr4-chat-module" for="pr4-chat-input"><span>LOCAL</span><i>${icon('chat')}</i><input id="pr4-chat-input" type="text" autocomplete="off" spellcheck="false" placeholder="Écrire un message..." aria-label="Chat ParadiseRP"></label><nav class="pr4-dock pr4-dock-right">${dockRight.map(x=>`<button type="button" class="${x.key==='more'?'more':'app'}" data-tip="${esc(x.label)}" data-pr4-action="${x.key==='more'?'toggle-more':`open:${x.key}`}">${icon(x.icon)}<span>${esc(x.label)}</span></button>`).join('')}</nav></div><div class="pr4-more-menu" hidden>${moreActions.map(a=>`<button type="button" data-pr4-action="open:${a[0]}">${icon(a[2])}<span>${esc(a[1])}</span></button>`).join('')}</div><aside class="pr4-object-card"><header>${icon('car')}<b>Cadillac</b></header><div class="mini-car"></div><p>El Dorado · <span data-pr-bind="username">${esc(d.username)}</span></p><div><button type="button" data-pr4-action="open:vehicles">Conduire</button><button type="button" data-pr4-action="toast:Clés ouvertes">Clés</button></div></aside><div class="pr4-layer"></div><div class="pr4-toast-host" aria-live="polite"></div></div>`; }

  function bindValue(name){ const d=state.data; switch(name){ case'username':return d.username||'Joueur'; case'role':return d.role||'Citoyen'; case'district':return d.district||d.city||'Paradise City'; case'room':return d.room||'ParadiseRP'; case'players':return`${fmt(d.players||0)} joueurs`; case'cash':return`${fmt(d.money?.cash??d.money?.credits??0)} $`; case'bank':return`${fmt(d.money?.bank||0)} $`; case'time':return d.time||clock(); case'citizen':return d.citizen_id||'PR-01024'; case'level':return`Niveau ${fmt(d.level||1)}`; case'notifications':return fmt(d.notifications?.count??d.notifications_count??0); default:return''; } }
  function updateBindings(){ if(!root)return; root.querySelectorAll('[data-pr-bind]').forEach(el=>{ const next=bindValue(el.dataset.prBind); if(el.textContent!==next)el.textContent=next; }); const ava=avatarUrl(state.data); root.querySelectorAll('[data-pr-bind-img="avatar"]').forEach(img=>{ if(ava&&img.getAttribute('src')!==ava)img.setAttribute('src',ava); }); }

  function renderWindow(){ const layer=root?.querySelector('.pr4-layer'); if(!layer)return; const key=state.window.key; if(!key){ if(layer.childNodes.length)layer.replaceChildren(); layer.classList.remove('is-open'); delete layer.dataset.windowKey; state.window.phase='closed'; return; } state.window.phase='opening'; layer.innerHTML=activeView(); layer.classList.add('is-open'); layer.dataset.windowKey=key; state.window.phase='open'; updateBindings(); if(key==='commands')window.setTimeout(()=>{ const input=layer.querySelector('[data-pr4-command-search]'); if(input){ input.focus({preventScroll:true}); input.setSelectionRange(input.value.length,input.value.length); filterCommands(); } },0); }
  function openWindow(key){ if(!key)return; if(key==='room'){ showToast(state.data.district||'Downtown Marina',`${state.data.room||'ParadiseRP'} · ${fmt(state.data.players||0)} joueurs`,'info'); return; } if(state.window.key===key&&state.window.phase==='open')return; state.window.key=key; state.window.phase='opening'; state.more=false; renderMore(); renderWindow(); }
  function closeWindow(){ if(!state.window.key&&state.window.phase==='closed')return; state.window.phase='closing'; state.window.key=null; renderWindow(); }
  function renderMore(){ const menu=root?.querySelector('.pr4-more-menu'); if(menu)menu.hidden=!state.more; }
  function showToast(title,body='',tone='info'){ const host=root?.querySelector('.pr4-toast-host'); if(!host)return; host.innerHTML=`<div class="pr4-toast tone-${esc(tone)}">${icon(tone==='danger'?'shield':'spark')}<span><strong>${esc(title)}</strong>${body?`<small>${esc(body)}</small>`:''}</span><i></i></div>`; window.clearTimeout(state.toastTimer); state.toastTimer=window.setTimeout(()=>{ if(host)host.replaceChildren(); },3300); }
  function focusChat(){ const input=root?.querySelector('#pr4-chat-input'); if(!input)return false; input.focus({preventScroll:true}); input.setSelectionRange(input.value.length,input.value.length); return true; }
  function queueCommand(command){ const input=root?.querySelector('#pr4-chat-input'); if(!input)return; closeWindow(); input.value=String(command||''); state.chatDraft=input.value; focusChat(); }

  function filterCommands(){ const layer=root?.querySelector('.pr4-layer'); if(!layer||state.window.key!=='commands')return; const q=state.commandQuery.trim().toLowerCase(); let visible=0; layer.querySelectorAll('[data-pr4-command]').forEach(btn=>{ const cat=btn.dataset.prCommandCat||'', text=btn.dataset.prCommandText||''; const ok=(state.commandCategory==='Toutes'||cat===state.commandCategory)&&(!q||text.includes(q)); btn.hidden=!ok; if(ok)visible+=1; }); layer.querySelectorAll('[data-pr4-command-cat]').forEach(btn=>btn.classList.toggle('is-active',btn.dataset.pr4CommandCat===state.commandCategory)); const empty=layer.querySelector('[data-pr-command-empty]'); if(empty)empty.hidden=visible>0; }

  function doAction(value){ if(!value)return; if(value==='close'){closeWindow();return;} if(value==='toggle-more'){state.more=!state.more;renderMore();return;} if(value.startsWith('open:')){openWindow(value.slice(5));return;} if(value.startsWith('toast:')){showToast(value.slice(6),'','info');return;} if(value.startsWith('command:')){queueCommand(value.slice(8));return;} if(value.startsWith('phone:')){state.phoneApp=value.slice(6)||'home';showToast('ParadisePhone','Application bientôt prête','info');} }
  function handleClick(event){ const action=event.target.closest('[data-pr4-action]'); if(action){doAction(action.getAttribute('data-pr4-action'));return;} const item=event.target.closest('[data-pr4-item]'); if(item){state.selectedItem=item.getAttribute('data-pr4-item')||'phone';if(state.window.key==='inventory')renderWindow();return;} const cat=event.target.closest('[data-pr4-cat]'); if(cat){state.inventoryCategory=cat.getAttribute('data-pr4-cat')||'all';const first=items.find(x=>state.inventoryCategory==='all'||x.type===state.inventoryCategory);if(first)state.selectedItem=first.id;if(state.window.key==='inventory')renderWindow();return;} const command=event.target.closest('[data-pr4-command]'); if(command){queueCommand(command.getAttribute('data-pr4-command')||'');return;} const ccat=event.target.closest('[data-pr4-command-cat]'); if(ccat){state.commandCategory=ccat.getAttribute('data-pr4-command-cat')||'Toutes';filterCommands();return;} if(event.target.classList.contains('pr4-layer'))closeWindow(); }
  function handleInput(event){ if(event.target.id==='pr4-chat-input'){state.chatDraft=event.target.value||'';return;} if(event.target.matches('[data-pr4-command-search]')){state.commandQuery=event.target.value||'';filterCommands();} }
  function handleHudKeydown(event){ if(event.target.id!=='pr4-chat-input')return; if(event.key==='Enter'){event.preventDefault();event.stopPropagation();const text=event.target.value;if(!String(text||'').trim()){event.target.blur();return;}if(sendChat(text)){event.target.value='';state.chatDraft='';event.target.blur();}else showToast('Chat indisponible','Le champ Nitro réseau n’est pas encore prêt.','danger');return;} if(event.key==='Escape'){event.preventDefault();event.stopPropagation();event.target.blur();} }
  function handleGlobalKeydown(event){ const tag=event.target?.tagName; const typing=tag==='INPUT'||tag==='TEXTAREA'||event.target?.isContentEditable; if(typing)return; if(event.key==='Enter'){event.preventDefault();event.stopImmediatePropagation();focusChat();return;} if(event.key==='Escape'&&(state.window.key||state.more)){event.preventDefault();if(state.window.key)closeWindow();state.more=false;renderMore();return;} if(event.ctrlKey||event.metaKey||event.altKey){if((event.ctrlKey||event.metaKey)&&event.key.toLowerCase()==='k'){event.preventDefault();openWindow('commands');}return;} const shortcuts={p:'phone',i:'inventory',d:'documents',v:'vehicles',c:'commands'};const key=event.key.toLowerCase();if(shortcuts[key]){event.preventDefault();openWindow(shortcuts[key]);} }

  async function loadData(first=false){ try{ const response=await fetch(`${DATA_URL}?_=${Date.now()}`,{cache:'no-store',credentials:'same-origin'}); if(!response.ok)throw new Error(String(response.status)); const json=await response.json(); state.data={...DEFAULT_DATA,...(json||{}),health:{...DEFAULT_DATA.health,...(json?.health||{})},energy:{...DEFAULT_DATA.energy,...(json?.energy||{})},money:{...DEFAULT_DATA.money,...(json?.money||{})}}; }catch(_){if(first)state.data=DEFAULT_DATA;} updateBindings(); window.dispatchEvent(new CustomEvent('paradise:player-data',{detail:state.data})); }

  function mount(){ ensureCss(); root=document.getElementById(HUD_ID); if(!root){root=document.createElement('div');root.id=HUD_ID;document.body.appendChild(root);} if(root.dataset.prMounted==='1')return; root.dataset.prMounted='1'; root.innerHTML=shellHtml(); root.addEventListener('click',handleClick,false); root.addEventListener('input',handleInput,false); root.addEventListener('keydown',handleHudKeydown,false); window.addEventListener('keydown',handleGlobalKeydown,true); updateBindings(); }
  function boot(){ mount(); loadData(true); window.setInterval(()=>loadData(false),10000); window.__ParadiseRPUI={version:VERSION,open:openWindow,close:closeWindow,toast:showToast,focusChat,sendChat,getData:()=>state.data,getState:()=>({...state,data:state.data}),refresh:()=>loadData(false)}; window.__ParadiseWindowManager={get current(){return state.window.key;},get phase(){return state.window.phase;},open:openWindow,close:closeWindow}; }

  if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',boot,{once:true});else boot();
})();
