(() => {
  'use strict';

  if (window.__ParadisePhoneStrictLayout) return;
  window.__ParadisePhoneStrictLayout = '3.0.0-home-v3-launcher';

  const VERSION = '3.0.0-home-v3-launcher';
  const LAUNCH_DELAY = 165;
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

  const svg = {
    messages: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="s" d="M8 11h32v23H19l-8 7v-7H8z"/><path class="d" d="M14 17h20v3H14zm0 7h16v3H14z"/></svg>',
    contacts: '<svg viewBox="0 0 48 48" aria-hidden="true"><rect class="s" x="10" y="7" width="29" height="34" rx="4"/><path class="d" d="M10 14H6v4h4zm0 9H6v4h4zm0 9H6v4h4z"/><circle class="d" cx="25" cy="20" r="6"/><path class="d" d="M16 35c1-7 5-10 9-10s8 3 9 10z"/></svg>',
    calls: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="s" d="M13 8l8 10-5 5c4 7 8 11 15 15l5-5 10 8-5 6c-2 2-6 2-10 0C19 41 8 30 3 17c-2-4-1-7 1-9l9-6z" transform="translate(1 -1) scale(.9)"/></svg>',
    bank: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="s" d="M5 17 24 6l19 11v5H5z"/><path class="d" d="M9 24h6v14H9zm12 0h6v14h-6zm12 0h6v14h-6zM5 40h38v4H5z"/></svg>',
    jobs: '<svg viewBox="0 0 48 48" aria-hidden="true"><rect class="s" x="6" y="15" width="36" height="27" rx="5"/><path class="d" d="M18 15v-4c0-3 2-5 5-5h2c3 0 5 2 5 5v4h-5v-3h-2v3zM6 24h36v6H6z"/><rect class="h" x="20" y="25" width="8" height="6" rx="2"/></svg>',
    estate: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="s" d="M4 22 24 6l20 16v22H30V31H18v13H4z"/><path class="d" d="M17 20h14v9H17z"/></svg>',
    paradise: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="s" d="M9 6h18c9 0 14 6 14 14 0 9-6 14-15 14h-7v9H9zm10 9v10h6c4 0 6-2 6-5s-2-5-6-5z"/><path class="h" d="M32 6h7v7h-7z"/></svg>',
    news: '<svg viewBox="0 0 48 48" aria-hidden="true"><rect class="s" x="7" y="7" width="34" height="35" rx="4"/><rect class="d" x="12" y="13" width="13" height="12" rx="2"/><path class="d" d="M29 14h7v3h-7zm0 7h7v3h-7zM12 30h24v3H12zm0 6h18v3H12z"/></svg>',
    enterprise: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="s" d="M7 12h15v32H7zm19-7h15v39H26z"/><path class="d" d="M11 17h7v5h-7zm0 9h7v5h-7zm0 9h7v5h-7zM30 11h7v5h-7zm0 9h7v5h-7zm0 9h7v5h-7zm0 9h7v5h-7z"/></svg>',
    taxi: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="s" d="M8 19h32l5 11v11h-6v-5H9v5H3V30z"/><path class="d" d="m13 14 4-7h14l4 7h-6l-2-3h-8l-2 3zM10 23h28l3 7H7z"/><circle class="h" cx="12" cy="33" r="3"/><circle class="h" cx="36" cy="33" r="3"/></svg>',
    settings: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="s" d="m21 4 6 1 2 6 5 2 6-2 4 5-4 5 1 5 5 4-2 6-6 1-3 4-1 6-6 1-3-5h-6l-3 5-6-2v-6l-4-3-6-1-1-6 5-4v-5l-4-5 4-5 6 2 5-3z"/><circle class="d" cx="24" cy="25" r="8"/></svg>',
    appstore: '<svg viewBox="0 0 48 48" aria-hidden="true"><rect class="s" x="5" y="5" width="38" height="38" rx="10"/><path class="d" d="M18 35h-7l12-22h6l-3 6 11 16h-7l-7-11zm6-22 3-5 4 2-3 5z"/></svg>',
    camera: '<svg viewBox="0 0 48 48" aria-hidden="true"><rect class="s" x="5" y="13" width="38" height="29" rx="6"/><path class="d" d="m14 13 4-6h12l4 6z"/><circle class="d" cx="24" cy="27" r="9"/><circle class="h" cx="24" cy="27" r="4"/></svg>',
    gallery: '<svg viewBox="0 0 48 48" aria-hidden="true"><rect class="s" x="5" y="6" width="38" height="36" rx="6"/><circle class="h" cx="32" cy="16" r="5"/><path class="d" d="M8 36 18 24l7 7 5-5 10 10z"/></svg>',
    browser: '<svg viewBox="0 0 48 48" aria-hidden="true"><circle class="s" cx="24" cy="24" r="19"/><path class="d" d="M5 22h38v5H5zM22 5h5v38h-5z"/><path class="d" fill-rule="evenodd" d="M24 5c8 7 11 14 11 19S32 36 24 43C16 36 13 29 13 24S16 12 24 5zm0 7c-4 5-6 9-6 12s2 8 6 12c4-4 6-9 6-12s-2-7-6-12z"/></svg>'
  };

  const APP_REGISTRY = Object.freeze([
    { id:'messages', label:'Messages', icon:'messages', accent:'orange', component:'messages', enabled:true, order:10, dock:false, launcher:true, badgeCount:p=>Math.max(0,Number(p?.unreadCount)||0) },
    { id:'contacts', label:'Contacts', icon:'contacts', accent:'cream', component:'contacts', enabled:true, order:20, dock:false, launcher:true },
    { id:'calls', label:'Appels', icon:'calls', accent:'green', component:'calls', enabled:true, order:30, dock:false, launcher:true },
    { id:'bank', label:'Banque', icon:'bank', accent:'aqua', component:null, enabled:false, order:40, dock:false, launcher:true },
    { id:'jobs', label:'Jobs', icon:'jobs', accent:'brown', component:null, enabled:false, order:50, dock:false, launcher:true },
    { id:'estate', label:'Immobilier', icon:'estate', accent:'sand', component:null, enabled:false, order:60, dock:false, launcher:true },
    { id:'paradise', label:'Paradise', icon:'paradise', accent:'blue', component:null, enabled:false, order:70, dock:false, launcher:true },
    { id:'news', label:'Actualités', icon:'news', accent:'grayblue', component:null, enabled:false, order:80, dock:false, launcher:true },
    { id:'enterprise', label:'Entreprise', icon:'enterprise', accent:'teal', component:null, enabled:false, order:90, dock:false, launcher:true },
    { id:'taxi', label:'Taxi', icon:'taxi', accent:'yellow', component:null, enabled:false, order:100, dock:false, launcher:true },
    { id:'settings', label:'Paramètres', icon:'settings', accent:'warm', component:'settings', enabled:true, order:110, dock:false, launcher:true },
    { id:'appstore', label:'App Store', icon:'appstore', accent:'ocean', component:null, enabled:false, order:120, dock:false, launcher:true },
    { id:'notifications', label:'Notifications', icon:'news', accent:'grayblue', component:'notifications', enabled:true, order:130, dock:false, launcher:false, badgeCount:p=>Array.isArray(p?.notifications)?p.notifications.filter(n=>!n?.read).length:0 },
    { id:'camera', label:'Caméra', icon:'camera', accent:'dockdark', component:null, enabled:false, order:200, dock:true, launcher:false },
    { id:'gallery', label:'Galerie', icon:'gallery', accent:'dockgallery', component:null, enabled:false, order:210, dock:true, launcher:false },
    { id:'browser', label:'Navigateur', icon:'browser', accent:'dockbrowser', component:null, enabled:false, order:220, dock:true, launcher:false }
  ]);

  const getState = () => window.ParadiseStore?.getState?.() || {};
  const phoneState = () => getState().phone || {};
  const byId = id => APP_REGISTRY.find(app => app.id === id) || null;
  const badgeFor = (item,p) => Math.max(0, Number(typeof item.badgeCount === 'function' ? item.badgeCount(p) : 0) || 0);

  function appMarkup(item,p) {
    const badge = badgeFor(item,p);
    return `<button type="button" class="ppv3-app is-${esc(item.accent)}${item.enabled?'':' is-coming'}" data-pp-launcher-app="${esc(item.id)}" aria-label="${esc(item.label)}${item.enabled?'':' — bientôt disponible'}">
      <span class="ppv3-app-icon">${svg[item.icon]||svg.paradise}</span>
      ${badge>0?`<b class="ppv3-badge">${badge>99?'99+':badge}</b>`:''}
      <span class="ppv3-app-label">${esc(item.label)}</span>
    </button>`;
  }

  function dockMarkup(item) {
    return `<button type="button" class="ppv3-dock-app is-${esc(item.accent)}" data-pp-launcher-app="${esc(item.id)}" aria-label="${esc(item.label)} — bientôt disponible"><span>${svg[item.icon]||svg.paradise}</span></button>`;
  }

  function launcherMarkup(p) {
    const apps = APP_REGISTRY.filter(item=>item.launcher&&!item.dock).sort((a,b)=>a.order-b.order);
    const dock = APP_REGISTRY.filter(item=>item.dock).sort((a,b)=>a.order-b.order);
    return `<div class="ppv3-launcher" data-ppv3-launcher="1">
      <div class="ppv3-app-grid">${apps.map(item=>appMarkup(item,p)).join('')}</div>
      <div class="ppv3-page-indicator" aria-label="Page 1 sur 2"><i class="is-active"></i><i></i></div>
      <div class="ppv3-dock" aria-label="Dock ParadisePhone">${dock.map(dockMarkup).join('')}</div>
    </div>`;
  }

  function nowTime() {
    try { return new Intl.DateTimeFormat('fr-FR',{hour:'2-digit',minute:'2-digit'}).format(new Date()); }
    catch (_) { return '--:--'; }
  }

  function refreshStatus(phone) {
    const status = phone?.querySelector('.pp-status');
    if (!status) return;
    const time = status.querySelector('strong');
    const brand = status.querySelector(':scope > span');
    if (time) time.textContent = nowTime();
    if (brand) {
      brand.textContent = 'ParadisePhone';
      brand.dataset.ppv3Brand = '1';
    }
  }

  function launcherSignature(p) {
    return APP_REGISTRY.map(item=>`${item.id}:${badgeFor(item,p)}`).join('|');
  }

  function mountLauncher(home,p) {
    const signature = launcherSignature(p);
    const existing = home.querySelector('[data-ppv3-launcher]');
    if (existing && existing.dataset.signature === signature) return;
    home.innerHTML = launcherMarkup(p);
    const launcher = home.querySelector('[data-ppv3-launcher]');
    if (launcher) launcher.dataset.signature = signature;
  }

  function enhance() {
    const phone = root();
    if (!phone) return;
    refreshStatus(phone);
    const home = phone.querySelector('.pp-home');
    const isHome = Boolean(home);
    phone.dataset.ppStrictLayout = '3';
    phone.dataset.ppLauncherVersion = VERSION;
    phone.classList.toggle('ppv3-is-home', isHome);
    if (!isHome) return;
    phone.querySelectorAll('.ppf-real-zone,.ppf-live-strip,.ppf-home-scene').forEach(node=>node.remove());
    mountLauncher(home, phoneState());
  }

  function showToast(label) {
    const phone = root();
    if (!phone) return;
    phone.querySelector('.ppv3-toast')?.remove();
    const toast = document.createElement('div');
    toast.className = 'ppv3-toast';
    toast.innerHTML = `<strong>${esc(label)}</strong><span>Application bientôt disponible.</span>`;
    phone.appendChild(toast);
    clearTimeout(toastTimer);
    toastTimer = window.setTimeout(()=>toast.remove(),2200);
  }

  function launch(item,button) {
    if (!item) return;
    if (!item.enabled || !item.component) {
      showToast(item.label);
      return;
    }
    const launcher = root()?.querySelector('[data-ppv3-launcher]');
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
    const button = target.closest('[data-pp-launcher-app]');
    if (button && root()?.contains(button)) {
      event.preventDefault();
      event.stopPropagation();
      event.stopImmediatePropagation();
      launch(byId(button.dataset.ppLauncherApp),button);
      return;
    }
    if (target.closest('#paradise-rp-hud .pp-device')) {
      schedule();
      window.setTimeout(schedule,90);
    }
  }

  function onStoreChange(_state,eventName) {
    if (['phone:update','ui:change','gameplay:snapshot','room:change','player:update','character:update'].includes(eventName)) schedule();
  }

  function boot() {
    document.addEventListener('click',onClick,true);
    window.addEventListener('paradise:phone',schedule,false);
    window.addEventListener('paradise:store-change',schedule,false);
    if (window.ParadiseStore?.subscribe) unsubscribe = window.ParadiseStore.subscribe(onStoreChange)||(()=>{});
    clockTimer = window.setInterval(()=>refreshStatus(root()),30000);
    schedule();
  }

  function destroy() {
    if (destroyed) return;
    destroyed = true;
    document.removeEventListener('click',onClick,true);
    window.removeEventListener('paradise:phone',schedule,false);
    window.removeEventListener('paradise:store-change',schedule,false);
    clearInterval(clockTimer);
    clearTimeout(toastTimer);
    try { unsubscribe(); } catch (_) {}
  }

  window.ParadisePhoneAppRegistry = Object.freeze({
    version: VERSION,
    getApps: () => APP_REGISTRY.map(item=>({
      id:item.id,label:item.label,icon:item.icon,accent:item.accent,component:item.component,
      enabled:item.enabled,badgeCount:badgeFor(item,phoneState()),order:item.order,dock:item.dock,launcher:item.launcher
    })),
    get: id => { const item=byId(id); return item ? { ...item, badgeCount:badgeFor(item,phoneState()) } : null; }
  });

  window.ParadisePhoneStrictLayout = Object.freeze({
    version: VERSION,
    refresh: schedule,
    getStatus: () => ({
      version: VERSION,
      mounted: Boolean(root()?.dataset.ppStrictLayout === '3'),
      home: Boolean(root()?.classList.contains('ppv3-is-home')),
      launcher: Boolean(root()?.querySelector('[data-ppv3-launcher]')),
      mainApps: APP_REGISTRY.filter(item=>item.launcher&&!item.dock).length,
      dockApps: APP_REGISTRY.filter(item=>item.dock).length,
      notificationsRoute: 'global-hud-bell',
      unreadMessages: Math.max(0,Number(phoneState().unreadCount)||0)
    })
  });

  window.addEventListener('beforeunload',destroy,{once:true});
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded',boot,{once:true});
  else boot();
})();
