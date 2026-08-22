(() => {
  'use strict';

  if (window.__ParadisePhoneStrictLayout) return;
  window.__ParadisePhoneStrictLayout = '4.0.0-home-v3-correct-target';

  const VERSION = '4.0.0-home-v3-correct-target';
  const LAUNCH_DELAY = 170;
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
    messages: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="g-main" d="M7 10h34v24H20l-9 8v-8H7z"/><path class="g-dark" d="M13 16h22v4H13zm0 8h17v4H13z"/><path class="g-hi" d="M11 12h26v2H11z"/></svg>',
    contacts: '<svg viewBox="0 0 48 48" aria-hidden="true"><rect class="g-main" x="11" y="7" width="29" height="34" rx="4"/><path class="g-dark" d="M11 14H6v4h5zm0 9H6v4h5zm0 9H6v4h5z"/><circle class="g-dark" cx="25" cy="20" r="6"/><path class="g-dark" d="M16 35c1-7 5-10 9-10s8 3 9 10z"/><path class="g-hi" d="M15 10h21v2H15z"/></svg>',
    calls: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="g-main" d="M13 7l9 11-6 6c4 8 8 12 16 16l6-6 10 8-6 6c-3 3-8 2-13-1C17 40 7 30 2 17 0 12 1 8 4 6l9-6z" transform="translate(1 0) scale(.9)"/><path class="g-hi" d="m15 8 5 6-2 2-5-6z"/></svg>',
    bank: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="g-main" d="M5 17 24 6l19 11v5H5z"/><path class="g-dark" d="M9 24h6v14H9zm12 0h6v14h-6zm12 0h6v14h-6zM5 40h38v4H5z"/><path class="g-hi" d="M11 17 24 10l13 7z"/></svg>',
    jobs: '<svg viewBox="0 0 48 48" aria-hidden="true"><rect class="g-main" x="5" y="15" width="38" height="28" rx="5"/><path class="g-dark" d="M18 15v-4c0-4 2-6 6-6s6 2 6 6v4h-5v-4h-2v4zM5 24h38v7H5z"/><rect class="g-hi" x="20" y="25" width="8" height="7" rx="2"/></svg>',
    estate: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="g-main" d="M4 22 24 6l20 16v22H30V31H18v13H4z"/><path class="g-dark" d="M17 20h14v9H17z"/><path class="g-hi" d="m24 10 14 11-3 3-11-9-11 9-3-3z"/></svg>',
    paradise: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="g-main" d="M9 5h18c10 0 15 6 15 14s-6 14-15 14h-8v10H9zm10 9v10h7c4 0 6-2 6-5s-2-5-6-5z"/><path class="g-hi" d="M31 7h8v7h-8z"/></svg>',
    news: '<svg viewBox="0 0 48 48" aria-hidden="true"><rect class="g-main" x="6" y="6" width="36" height="36" rx="5"/><rect class="g-dark" x="11" y="12" width="14" height="13" rx="2"/><path class="g-dark" d="M29 13h8v3h-8zm0 7h8v3h-8zM11 30h26v3H11zm0 6h19v3H11z"/><path class="g-hi" d="M10 9h28v2H10z"/></svg>',
    enterprise: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="g-main" d="M6 12h16v32H6zm20-7h16v39H26z"/><path class="g-dark" d="M10 17h8v5h-8zm0 9h8v5h-8zm0 9h8v5h-8zM30 11h8v5h-8zm0 9h8v5h-8zm0 9h8v5h-8zm0 9h8v5h-8z"/><path class="g-hi" d="M9 14h10v2H9zm20-7h10v2H29z"/></svg>',
    taxi: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="g-main" d="M8 19h32l5 11v11h-6v-5H9v5H3V30z"/><path class="g-dark" d="m13 14 4-7h14l4 7h-6l-2-3h-8l-2 3zM10 23h28l3 7H7z"/><circle class="g-hi" cx="12" cy="33" r="3"/><circle class="g-hi" cx="36" cy="33" r="3"/></svg>',
    settings: '<svg viewBox="0 0 48 48" aria-hidden="true"><path class="g-main" d="m21 4 6 1 2 6 5 2 6-2 4 5-4 5 1 5 5 4-2 6-6 1-3 4-1 6-6 1-3-5h-6l-3 5-6-2v-6l-4-3-6-1-1-6 5-4v-5l-4-5 4-5 6 2 5-3z"/><circle class="g-dark" cx="24" cy="25" r="8"/><circle class="g-hi" cx="24" cy="25" r="3"/></svg>',
    appstore: '<svg viewBox="0 0 48 48" aria-hidden="true"><rect class="g-main" x="5" y="5" width="38" height="38" rx="10"/><path class="g-dark" d="M18 35h-7l12-22h6l-3 6 11 16h-7l-7-11zm6-22 3-5 4 2-3 5z"/><path class="g-hi" d="M11 9h26v2H11z"/></svg>',
    camera: '<svg viewBox="0 0 48 48" aria-hidden="true"><rect class="g-main" x="5" y="13" width="38" height="29" rx="6"/><path class="g-dark" d="m14 13 4-6h12l4 6z"/><circle class="g-dark" cx="24" cy="27" r="9"/><circle class="g-hi" cx="24" cy="27" r="4"/></svg>',
    gallery: '<svg viewBox="0 0 48 48" aria-hidden="true"><rect class="g-main" x="5" y="6" width="38" height="36" rx="6"/><circle class="g-hi" cx="32" cy="16" r="5"/><path class="g-dark" d="M8 36 18 24l7 7 5-5 10 10z"/></svg>',
    browser: '<svg viewBox="0 0 48 48" aria-hidden="true"><circle class="g-main" cx="24" cy="24" r="19"/><path class="g-dark" d="M5 22h38v5H5zM22 5h5v38h-5z"/><path class="g-hi" fill-rule="evenodd" d="M24 6c7 6 10 13 10 18s-3 12-10 18c-7-6-10-13-10-18S17 12 24 6zm0 7c-3 4-5 8-5 11s2 8 5 11c3-3 5-8 5-11s-2-7-5-11z"/></svg>'
  });

  const PhoneAppRegistry = Object.freeze([
    { id:'messages', label:'Messages', icon:'messages', accent:'orange', component:'messages', enabled:true, order:10, launcher:true, dock:false, badgeCount:p=>Math.max(0,Number(p?.unreadCount)||0) },
    { id:'contacts', label:'Contacts', icon:'contacts', accent:'cream', component:'contacts', enabled:true, order:20, launcher:true, dock:false },
    { id:'calls', label:'Appels', icon:'calls', accent:'green', component:'calls', enabled:true, order:30, launcher:true, dock:false },
    { id:'bank', label:'Banque', icon:'bank', accent:'aqua', enabled:false, order:40, launcher:true, dock:false },
    { id:'jobs', label:'Jobs', icon:'jobs', accent:'brown', enabled:false, order:50, launcher:true, dock:false },
    { id:'estate', label:'Immobilier', icon:'estate', accent:'sand', enabled:false, order:60, launcher:true, dock:false },
    { id:'paradise', label:'Paradise', icon:'paradise', accent:'blue', enabled:false, order:70, launcher:true, dock:false },
    { id:'news', label:'Actualités', icon:'news', accent:'grayblue', enabled:false, order:80, launcher:true, dock:false },
    { id:'enterprise', label:'Entreprise', icon:'enterprise', accent:'teal', enabled:false, order:90, launcher:true, dock:false },
    { id:'taxi', label:'Taxi', icon:'taxi', accent:'yellow', enabled:false, order:100, launcher:true, dock:false },
    { id:'settings', label:'Paramètres', icon:'settings', accent:'warm', component:'settings', enabled:true, order:110, launcher:true, dock:false },
    { id:'appstore', label:'App Store', icon:'appstore', accent:'ocean', enabled:false, order:120, launcher:true, dock:false },
    { id:'notifications', label:'Notifications', icon:'news', accent:'grayblue', component:'notifications', enabled:true, order:130, launcher:false, dock:false, badgeCount:p=>Array.isArray(p?.notifications)?p.notifications.filter(n=>!n?.read).length:0 },
    { id:'camera', label:'Caméra', icon:'camera', accent:'camera', enabled:false, order:200, launcher:false, dock:true },
    { id:'gallery', label:'Galerie', icon:'gallery', accent:'gallery', enabled:false, order:210, launcher:false, dock:true },
    { id:'browser', label:'Navigateur', icon:'browser', accent:'browser', enabled:false, order:220, launcher:false, dock:true }
  ]);

  const state = () => window.ParadiseStore?.getState?.() || {};
  const phoneState = () => state().phone || {};
  const byId = id => PhoneAppRegistry.find(item => item.id === id) || null;
  const badgeFor = (item,p) => Math.max(0, Number(typeof item.badgeCount === 'function' ? item.badgeCount(p) : 0) || 0);

  function appMarkup(item,p) {
    const badge = badgeFor(item,p);
    return `<button type="button" class="ppv3-app is-${esc(item.accent)}${item.enabled?'':' is-coming'}" data-ppv3-app="${esc(item.id)}" aria-label="${esc(item.label)}${item.enabled?'':' — bientôt disponible'}">
      <span class="ppv3-app-icon">${glyphs[item.icon] || glyphs.paradise}</span>
      ${badge>0?`<b class="ppv3-badge">${badge>99?'99+':badge}</b>`:''}
      <span class="ppv3-app-label">${esc(item.label)}</span>
    </button>`;
  }

  function dockMarkup(item) {
    return `<button type="button" class="ppv3-dock-app is-${esc(item.accent)}" data-ppv3-app="${esc(item.id)}" aria-label="${esc(item.label)} — bientôt disponible"><span>${glyphs[item.icon] || glyphs.paradise}</span></button>`;
  }

  function launcherMarkup(p) {
    const apps = PhoneAppRegistry.filter(item=>item.launcher).sort((a,b)=>a.order-b.order);
    const dock = PhoneAppRegistry.filter(item=>item.dock).sort((a,b)=>a.order-b.order);
    return `<div class="ppv3-launcher" data-ppv3-launcher="1">
      <div class="ppv3-wallpaper" aria-hidden="true"></div>
      <div class="ppv3-title">ParadisePhone</div>
      <div class="ppv3-app-grid">${apps.map(item=>appMarkup(item,p)).join('')}</div>
      <div class="ppv3-page-indicator" aria-label="Page 1 sur 2"><i class="is-active"></i><i></i></div>
      <div class="ppv3-dock" aria-label="Dock ParadisePhone">${dock.map(dockMarkup).join('')}</div>
    </div>`;
  }

  function nowTime() {
    try { return new Intl.DateTimeFormat('fr-FR',{hour:'2-digit',minute:'2-digit'}).format(new Date()); }
    catch (_) { return '--:--'; }
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
  }

  function launcherSignature(p) {
    return PhoneAppRegistry.map(item=>`${item.id}:${badgeFor(item,p)}`).join('|');
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
    const home = phone.querySelector('.pp-home');
    const isHome = Boolean(home);
    phone.dataset.ppStrictLayout = '4';
    phone.dataset.ppLauncherVersion = VERSION;
    phone.classList.toggle('ppv3-is-home', isHome);
    refreshStatus(phone,isHome);
    if (!isHome) return;

    home.querySelectorAll('.ppf-real-zone,.ppf-live-strip,.ppf-home-scene,.pp-hero,.pp-app-grid').forEach(node=>node.remove());
    mountLauncher(home, phoneState());
  }

  function showToast(item) {
    const phone = root();
    if (!phone) return;
    phone.querySelector('.ppv3-toast')?.remove();
    const toast = document.createElement('div');
    toast.className = 'ppv3-toast';
    toast.innerHTML = `<strong>${esc(item?.label || 'ParadisePhone')}</strong><span>Application bientôt disponible.</span>`;
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
    const button = target.closest('[data-ppv3-app]');
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
    launch(byId(button.dataset.ppv3App),button);
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
    },15000);
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
      mounted: Boolean(root()?.dataset.ppStrictLayout === '4'),
      home: Boolean(root()?.classList.contains('ppv3-is-home')),
      launcher: Boolean(root()?.querySelector('[data-ppv3-launcher]')),
      launcherApps: root()?.querySelectorAll('.ppv3-app').length || 0,
      dockApps: root()?.querySelectorAll('.ppv3-dock-app').length || 0,
      unreadMessages: Math.max(0,Number(phoneState()?.unreadCount)||0),
      numberPreserved: text(phoneState()?.number) || null
    })
  });

  window.addEventListener('beforeunload',destroy,{once:true});
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded',boot,{once:true});
  else boot();
})();
