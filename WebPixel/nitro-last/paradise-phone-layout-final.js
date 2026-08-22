(() => {
  'use strict';

  if (window.__ParadisePhoneStrictLayout) return;
  window.__ParadisePhoneStrictLayout = '5.0.0-official-home';

  const VERSION = '5.0.0-official-home';
  const LAUNCH_DELAY = 180;
  let destroyed = false;
  let scheduled = false;
  let unsubscribe = () => {};
  let clockTimer = 0;
  let toastTimer = 0;

  const root = () => document.querySelector('#paradise-rp-hud .pp-device');
  const text = value => value == null ? '' : String(value).trim();
  const esc = value => String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;');

  const glyphs = Object.freeze({
    messages: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="g-main" d="M8 9h32a4 4 0 0 1 4 4v19a4 4 0 0 1-4 4H21l-10 8v-8H8a4 4 0 0 1-4-4V13a4 4 0 0 1 4-4Z"/></svg>',
    contacts: '<svg viewBox="0 0 48 48" aria-hidden="true"><circle class="g-main" cx="24" cy="17" r="9"/><path class="g-main" d="M8 42c1-10 7-15 16-15s15 5 16 15H8Z"/></svg>',
    calls: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="g-main" d="m14 6 9 12-6 5c4 8 8 12 16 16l5-6 12 9-6 6c-3 3-8 2-13-1C18 40 8 30 2 17 0 12 1 8 4 6l10-6Z" transform="translate(1 0) scale(.88)"/></svg>',
    bank: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="g-main" d="M4 18 24 6l20 12v5H4v-5Zm5 8h6v13H9V26Zm12 0h6v13h-6V26Zm12 0h6v13h-6V26ZM5 41h38v4H5v-4Z"/></svg>',
    jobs: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="g-main" d="M17 14V9c0-3 2-5 5-5h4c3 0 5 2 5 5v5h10a5 5 0 0 1 5 5v20a5 5 0 0 1-5 5H7a5 5 0 0 1-5-5V19a5 5 0 0 1 5-5h10Zm5 0h4v-4h-4v4Zm-20 9h44v6H2v-6Z"/></svg>',
    estate: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="g-main" d="M3 22 24 5l21 17-4 5-3-3v20H10V24l-3 3-4-5Zm14 3v14h6v-9h7v9h3V25l-9-8-7 8Z"/></svg>',
    paradise: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="g-main" d="M24 18c-4-8-12-10-17-8 5 1 9 4 11 8-7-3-13 0-17 4 7-1 12 1 16 5-5 1-9 5-11 10 6-4 11-4 16-2v10h5V27c5-4 10-6 17-5-4-4-10-7-17-4 2-4 6-7 11-8-5-2-13 0-17 8h-3Z"/></svg>',
    news: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="g-main" d="M6 5h32a5 5 0 0 1 5 5v33H9a6 6 0 0 1-6-6V8a3 3 0 0 1 3-3Zm5 8v12h13V13H11Zm18 0v4h8v-4h-8Zm0 8v4h8v-4h-8ZM11 30v4h26v-4H11Zm0 8v3h21v-3H11Z"/></svg>',
    enterprise: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="g-main" d="M5 14h16v30H5V14Zm22-9h16v39H27V5ZM10 19h6v5h-6v-5Zm0 9h6v5h-6v-5Zm0 9h6v5h-6v-5Zm22-25h6v5h-6v-5Zm0 9h6v5h-6v-5Zm0 9h6v5h-6v-5Zm0 9h6v5h-6v-5Z"/></svg>',
    taxi: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="g-main" d="M16 11h16l3 6h5l5 12v11h-6v-5H9v5H3V29l5-12h5l3-6Zm4 0-2 6h12l-2-6h-8ZM9 23l-3 8h36l-3-8H9Zm5 5a4 4 0 1 0 0 8 4 4 0 0 0 0-8Zm20 0a4 4 0 1 0 0 8 4 4 0 0 0 0-8Z"/></svg>',
    settings: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="g-main" d="m21 3 6 1 2 6 5 2 6-2 4 5-4 5 1 5 5 4-2 6-6 1-3 4-1 6-6 1-3-5h-6l-3 5-6-2v-6l-4-3-6-1-1-6 5-4v-5l-4-5 4-5 6 2 5-3Zm3 14a8 8 0 1 0 0 16 8 8 0 0 0 0-16Z"/></svg>',
    appstore: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="g-main" d="M12 14h24l5 30H7l5-30Zm7 0v-3a5 5 0 0 1 10 0v3h4v-3a9 9 0 0 0-18 0v3h4Zm2 13c0 5 2 8 7 8s7-3 7-8h-4c0 3-1 4-3 4s-3-1-3-4h-4Z"/></svg>',
    profile: '<svg viewBox="0 0 48 48" aria-hidden="true"><circle class="g-main" cx="24" cy="16" r="10"/><path class="g-main" d="M5 45c1-12 8-18 19-18s18 6 19 18H5Z"/></svg>',
    mail: '<svg viewBox="0 0 48 48" aria-hidden="true"><rect class="g-main" x="4" y="9" width="40" height="30" rx="5"/><path class="g-cut" d="m8 14 16 13 16-13 2 3-18 15L6 17l2-3Z"/></svg>',
    browser: '<svg viewBox="0 0 48 48" aria-hidden="true"><circle class="g-main" cx="24" cy="24" r="20"/><path class="g-cut" d="M4 22h40v4H4v-4Zm18-18h4v40h-4V4Zm2 0c8 7 12 13 12 20s-4 13-12 20c-8-7-12-13-12-20S16 11 24 4Zm0 6c-5 5-8 10-8 14s3 9 8 14c5-5 8-10 8-14s-3-9-8-14Z" fill-rule="evenodd"/></svg>',
    palm: '<svg viewBox="0 0 48 48" aria-hidden="true"><path d="M24 20c-4-8-11-10-17-8 5 1 9 4 12 8-7-3-13 0-17 5 7-2 12 0 17 4-4 3-6 8-7 14h5c1-6 3-10 7-13 2 5 3 9 3 14h5c0-6-1-11-3-16 5-3 10-4 17-2-4-5-10-8-17-5 3-4 7-7 12-8-6-2-13 0-17 8h-3Z"/></svg>'
  });

  const PhoneAppRegistry = Object.freeze([
    { id:'messages', label:'Messages', icon:'messages', accent:'messages', component:'messages', enabled:true, order:10, launcher:true, dock:false, badgeCount:p=>Math.max(0,Number(p?.unreadCount)||0) },
    { id:'contacts', label:'Contacts', icon:'contacts', accent:'contacts', component:'contacts', enabled:true, order:20, launcher:true, dock:false },
    { id:'calls', label:'Appels', icon:'calls', accent:'calls', component:'calls', enabled:true, order:30, launcher:true, dock:false },
    { id:'bank', label:'Banque', icon:'bank', accent:'bank', enabled:false, order:40, launcher:true, dock:false },
    { id:'jobs', label:'Jobs', icon:'jobs', accent:'jobs', enabled:false, order:50, launcher:true, dock:false },
    { id:'estate', label:'Immobilier', icon:'estate', accent:'estate', enabled:false, order:60, launcher:true, dock:false },
    { id:'paradise', label:'Paradise', icon:'paradise', accent:'paradise', enabled:false, order:70, launcher:true, dock:false },
    { id:'news', label:'Actualités', icon:'news', accent:'news', enabled:false, order:80, launcher:true, dock:false },
    { id:'enterprise', label:'Entreprise', icon:'enterprise', accent:'enterprise', enabled:false, order:90, launcher:true, dock:false },
    { id:'taxi', label:'Taxi', icon:'taxi', accent:'taxi', enabled:false, order:100, launcher:true, dock:false },
    { id:'settings', label:'Paramètres', icon:'settings', accent:'settings', component:'settings', enabled:true, order:110, launcher:true, dock:false },
    { id:'appstore', label:'App Store', icon:'appstore', accent:'appstore', enabled:false, order:120, launcher:true, dock:false },
    { id:'notifications', label:'Notifications', icon:'news', accent:'news', component:'notifications', enabled:true, order:130, launcher:false, dock:false, badgeCount:p=>Array.isArray(p?.notifications)?p.notifications.filter(n=>!n?.read).length:0 },
    { id:'profile', label:'Profil', icon:'profile', accent:'dock-profile', enabled:false, order:200, launcher:false, dock:true },
    { id:'mail', label:'Mail', icon:'mail', accent:'dock-mail', enabled:false, order:210, launcher:false, dock:true },
    { id:'browser', label:'Navigateur', icon:'browser', accent:'dock-browser', enabled:false, order:220, launcher:false, dock:true }
  ]);

  const state = () => window.ParadiseStore?.getState?.() || {};
  const phoneState = () => state().phone || {};
  const byId = id => PhoneAppRegistry.find(item => item.id === id) || null;
  const badgeFor = (item,p) => Math.max(0, Number(typeof item.badgeCount === 'function' ? item.badgeCount(p) : 0) || 0);

  function appMarkup(item,p) {
    const badge = badgeFor(item,p);
    return `<button type="button" class="ppv4-app is-${esc(item.accent)}${item.enabled?'':' is-coming'}" data-ppv4-app="${esc(item.id)}" aria-label="${esc(item.label)}${item.enabled?'':' — indisponible'}">
      <span class="ppv4-app-icon">${glyphs[item.icon] || glyphs.paradise}</span>
      ${badge>0?`<b class="ppv4-badge">${badge>99?'99+':badge}</b>`:''}
      <span class="ppv4-app-label">${esc(item.label)}</span>
    </button>`;
  }

  function dockMarkup(item) {
    return `<button type="button" class="ppv4-dock-app is-${esc(item.accent)}" data-ppv4-app="${esc(item.id)}" aria-label="${esc(item.label)} — indisponible"><span>${glyphs[item.icon] || glyphs.paradise}</span></button>`;
  }

  function launcherMarkup(p) {
    const apps = PhoneAppRegistry.filter(item=>item.launcher).sort((a,b)=>a.order-b.order);
    const dock = PhoneAppRegistry.filter(item=>item.dock).sort((a,b)=>a.order-b.order);
    return `<div class="ppv4-launcher" data-ppv4-launcher="1">
      <div class="ppv4-brand">
        <span class="ppv4-brand-palm">${glyphs.palm}</span>
        <strong>ParadisePhone</strong>
        <span class="ppv4-brand-rule"><i></i></span>
      </div>
      <div class="ppv4-app-grid">${apps.map(item=>appMarkup(item,p)).join('')}</div>
      <div class="ppv4-page-indicator" aria-label="Page 1 sur 3"><i class="is-active"></i><i></i><i></i></div>
      <div class="ppv4-dock" aria-label="Dock ParadisePhone">${dock.map(dockMarkup).join('')}</div>
    </div>`;
  }

  function nowTime() {
    try { return new Intl.DateTimeFormat('fr-FR',{hour:'2-digit',minute:'2-digit'}).format(new Date()); }
    catch (_) { return '--:--'; }
  }

  function ensureStatusExtras(status,isHome) {
    const right = status?.querySelector(':scope > div');
    if (!right) return;
    let wifi = right.querySelector('.ppv4-wifi');
    if (isHome && !wifi) {
      wifi = document.createElement('i');
      wifi.className = 'ppv4-wifi';
      wifi.setAttribute('aria-hidden','true');
      const battery = right.querySelector('.pp-battery');
      right.insertBefore(wifi,battery || null);
    } else if (!isHome && wifi) {
      wifi.remove();
    }
  }

  function refreshStatus(phone,isHome) {
    const status = phone?.querySelector('.pp-status');
    if (!status) return;
    const time = status.querySelector('strong');
    const center = status.querySelector(':scope > span');
    if (time) time.textContent = nowTime();
    if (center) {
      center.textContent = isHome ? '' : 'Paradise';
      center.toggleAttribute('aria-hidden', isHome);
    }
    ensureStatusExtras(status,isHome);
  }

  function launcherSignature(p) {
    return PhoneAppRegistry.map(item=>`${item.id}:${badgeFor(item,p)}`).join('|');
  }

  function mountLauncher(home,p) {
    const signature = launcherSignature(p);
    const existing = home.querySelector('[data-ppv4-launcher]');
    if (existing && existing.dataset.signature === signature) return;
    home.innerHTML = launcherMarkup(p);
    const launcher = home.querySelector('[data-ppv4-launcher]');
    if (launcher) launcher.dataset.signature = signature;
  }

  function enhance() {
    const phone = root();
    if (!phone) return;
    const home = phone.querySelector('.pp-home');
    const isHome = Boolean(home);
    phone.dataset.ppStrictLayout = '5';
    phone.dataset.ppLauncherVersion = VERSION;
    phone.classList.toggle('ppv4-is-home', isHome);
    refreshStatus(phone,isHome);
    if (!isHome) return;

    home.querySelectorAll('.ppf-real-zone,.ppf-live-strip,.ppf-home-scene,.pp-hero,.pp-app-grid,[data-ppv3-launcher]').forEach(node=>node.remove());
    mountLauncher(home, phoneState());
  }

  function showToast(item) {
    const phone = root();
    if (!phone) return;
    phone.querySelector('.ppv4-toast')?.remove();
    const toast = document.createElement('div');
    toast.className = 'ppv4-toast';
    toast.innerHTML = `<strong>${esc(item?.label || 'ParadisePhone')}</strong><span>Cette application n’est pas encore disponible.</span>`;
    phone.appendChild(toast);
    clearTimeout(toastTimer);
    toastTimer = window.setTimeout(()=>toast.remove(),2200);
  }

  function launch(item,button) {
    if (!item) return;
    if (!item.enabled || !item.component) {
      showToast(item);
      return;
    }

    const launcher = root()?.querySelector('[data-ppv4-launcher]');
    launcher?.classList.add('is-launching');
    button?.classList.add('is-pressed');
    window.setTimeout(()=>{
      window.ParadisePhoneV1?.open?.(item.component);
      schedule();
    },LAUNCH_DELAY);
  }

  function onClick(event) {
    const target = event.target;
    if (!(target instanceof Element)) return;
    const button = target.closest('[data-ppv4-app]');
    if (!button || !root()?.contains(button)) {
      if (target.closest('#paradise-rp-hud .pp-device')) {
        schedule();
        window.setTimeout(schedule,90);
      }
      return;
    }

    event.preventDefault();
    event.stopPropagation();
    event.stopImmediatePropagation();
    launch(byId(button.dataset.ppv4App),button);
  }

  function onStoreChange(_next,eventName) {
    if ([
      'phone:update',
      'ui:change',
      'gameplay:snapshot',
      'room:change',
      'player:update',
      'character:update'
    ].includes(eventName)) schedule();
  }

  function schedule() {
    if (destroyed || scheduled) return;
    scheduled = true;
    requestAnimationFrame(()=>requestAnimationFrame(()=>{
      scheduled = false;
      if (!destroyed) enhance();
    }));
  }

  function boot() {
    document.addEventListener('click',onClick,true);
    window.addEventListener('paradise:phone',schedule,false);
    window.addEventListener('paradise:store-change',schedule,false);
    if (window.ParadiseStore?.subscribe) unsubscribe = window.ParadiseStore.subscribe(onStoreChange) || (()=>{});
    clockTimer = window.setInterval(()=>{
      const phone = root();
      refreshStatus(phone,Boolean(phone?.querySelector('.pp-home')));
    },30000);
    schedule();
  }

  function destroy() {
    if (destroyed) return;
    destroyed = true;
    clearInterval(clockTimer);
    clearTimeout(toastTimer);
    document.removeEventListener('click',onClick,true);
    window.removeEventListener('paradise:phone',schedule,false);
    window.removeEventListener('paradise:store-change',schedule,false);
    try { unsubscribe(); } catch (_) {}
  }

  window.PhoneAppRegistry = PhoneAppRegistry;
  window.ParadisePhoneStrictLayout = Object.freeze({
    version: VERSION,
    refresh: schedule,
    registry: PhoneAppRegistry,
    getStatus: () => ({
      version: VERSION,
      mounted: Boolean(root()?.dataset.ppStrictLayout === '5'),
      home: Boolean(root()?.classList.contains('ppv4-is-home')),
      launcher: Boolean(root()?.querySelector('[data-ppv4-launcher]')),
      launcherApps: root()?.querySelectorAll('.ppv4-app').length || 0,
      dockApps: root()?.querySelectorAll('.ppv4-dock-app').length || 0,
      unreadMessages: Math.max(0,Number(phoneState()?.unreadCount)||0),
      numberPreserved: text(phoneState()?.number) || null
    })
  });

  window.addEventListener('beforeunload',destroy,{once:true});
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded',boot,{once:true});
  else boot();
})();
