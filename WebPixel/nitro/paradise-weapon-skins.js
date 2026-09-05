(()=>{
  'use strict';

  const labels={tazor:'Arme de choc',ak47:'AK47',akm:'AKM',g36:'G36'};
  const api='/weapon-skins.php';
  const positionKey='paradise.weaponSkins.position.v1';

  let root=null;
  let data=[];
  let filterKey=null;
  let busy=false;

  const esc=s=>String(s??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));

  const weaponKeyFromName=name=>{
    const n=String(name||'').toLowerCase().replace(/[^a-z0-9]/g,'');
    if(n.includes('ak47'))return'ak47';
    if(n==='akm'||n.includes('fusilakm'))return'akm';
    if(n.includes('g36'))return'g36';
    if(n.includes('tazor')||n.includes('tazer'))return'tazor';
    return null;
  };

  function sendCommand(command){
    const input=document.querySelector('.nitro-chat-input-container .chat-input, input.chat-input');
    if(!input)return false;
    const setter=Object.getOwnPropertyDescriptor(HTMLInputElement.prototype,'value')?.set;
    setter?setter.call(input,command):(input.value=command);
    input.dispatchEvent(new Event('input',{bubbles:true}));
    input.focus();
    setTimeout(()=>input.dispatchEvent(new KeyboardEvent('keydown',{key:'Enter',code:'Enter',keyCode:13,which:13,bubbles:true})),40);
    return true;
  }

  async function reapplyIfEquipped(weaponKey){
    try{
      const r=await fetch('/inventory.php?t='+Date.now(),{credentials:'same-origin',cache:'no-store'});
      const j=await r.json();
      if(!r.ok||!j.ok)return false;
      const equipped=(j.slots||[]).find(s=>Number(s.slot_index)===0);
      if(!equipped||weaponKeyFromName(equipped.display_name)!==weaponKey)return false;
      return sendCommand(':equiper '+equipped.display_name);
    }catch(_){
      return false;
    }
  }

  async function load(){
    const r=await fetch(api+'?t='+Date.now(),{credentials:'same-origin',headers:{Accept:'application/json'},cache:'no-store'});
    const j=await r.json();
    if(!r.ok||!j.ok)throw Error(j.error||'Chargement impossible');
    data=j.skins||[];
    render();
  }

  function render(){
    if(!root)return;
    const visible=filterKey?data.filter(s=>s.weapon_key===filterKey):data;
    const by={};
    visible.forEach(s=>(by[s.weapon_key]??=[]).push(s));
    const body=root.querySelector('.pws-body');

    body.innerHTML=Object.keys(by).map(k=>{
      const skins=by[k];
      const active=skins.find(s=>s.equipped)||skins.find(s=>s.is_default)||skins[0];
      return `<section class="pws-group" data-weapon="${esc(k)}">
        <div class="pws-group-title">${esc(labels[k]||k)}</div>
        <div class="pws-preview-row">
          <div class="pws-preview">
            <img class="pws-weapon" src="weapon-skins/${esc(active.image)}" alt="">
            <span>${esc(active.name)}</span>
          </div>
          <div class="pws-avatar-wrap">
            <img class="pws-avatar" src="weapon-skins/${esc(active.avatar_image)}" alt="">
            <div class="pws-avatar-info">
              <b>${esc(active.name)}</b>
              <small>Aperçu du skin sélectionné</small>
            </div>
          </div>
        </div>
        <div class="pws-options">
          ${skins.map(s=>`<button class="pws-option ${s.equipped?'equipped':s.owned?'owned':'locked'}" data-id="${s.id}" ${s.owned&&!busy?'':'disabled'}>
            <span class="pws-check">${s.equipped?'✓':'•'}</span>
            <span class="pws-option-name">${esc(s.name)}</span>
          </button>`).join('')}
        </div>
      </section>`;
    }).join('')||'<div class="pws-empty">Aucun skin disponible pour cette arme.</div>';

    body.querySelectorAll('[data-id]').forEach(b=>b.addEventListener('click',()=>equip(+b.dataset.id)));
  }

  function applySelectedSkin(payload){
    if(!payload||!payload.weapon_key)return;
    const detail={weaponKey:String(payload.weapon_key),effectId:Number(payload.effect_id||0),skinId:Number(payload.skin_id||0)};
    window.ParadiseSelectedWeaponSkin=detail;
    window.dispatchEvent(new CustomEvent('paradise:weapon-skin-changed',{detail}));
    try{localStorage.setItem('paradise.weaponSkin.'+detail.weaponKey,String(detail.effectId))}catch(_){}
  }

  async function equip(id){
    if(busy)return;
    const status=root?.querySelector('.pws-status');
    const selected=data.find(s=>s.id===id);
    if(!selected)return;

    busy=true;
    if(status){
      status.classList.remove('is-error');
      status.textContent='Enregistrement du skin…';
    }
    render();

    try{
      const r=await fetch(api,{
        method:'POST',
        credentials:'same-origin',
        cache:'no-store',
        headers:{'Content-Type':'application/json','X-Requested-With':'XMLHttpRequest'},
        body:JSON.stringify({skin_id:id})
      });
      const j=await r.json();
      if(!r.ok||!j.ok)throw Error(j.error||'Impossible de sélectionner ce skin.');
      data.forEach(s=>{if(s.weapon_key===selected.weapon_key)s.equipped=s.id===id});
      applySelectedSkin(j);
      render();
      const live=await reapplyIfEquipped(selected.weapon_key);
      if(status)status.textContent=live?'Skin sélectionné • arme conservée équipée':'Skin sélectionné • il sera repris au prochain équipement';
      setTimeout(()=>window.ParadiseInventory?.refresh?.(),500);
    }catch(e){
      if(status){
        status.classList.add('is-error');
        status.textContent=e.message;
      }
      await load().catch(()=>{});
    }finally{
      busy=false;
      render();
    }
  }

  function clampPosition(win,left,top){
    const margin=6;
    const rect=win.getBoundingClientRect();
    const maxLeft=Math.max(margin,window.innerWidth-rect.width-margin);
    const maxTop=Math.max(margin,window.innerHeight-rect.height-margin);
    return {
      left:Math.min(Math.max(margin,left),maxLeft),
      top:Math.min(Math.max(margin,top),maxTop)
    };
  }

  function savePosition(win){
    const rect=win.getBoundingClientRect();
    try{
      localStorage.setItem(positionKey,JSON.stringify({left:Math.round(rect.left),top:Math.round(rect.top)}));
    }catch(_){}
  }

  function applySavedPosition(win){
    let saved=null;
    try{saved=JSON.parse(localStorage.getItem(positionKey)||'null')}catch(_){}
    if(!saved||!Number.isFinite(saved.left)||!Number.isFinite(saved.top))return;
    requestAnimationFrame(()=>{
      const p=clampPosition(win,saved.left,saved.top);
      win.style.transform='none';
      win.style.left=p.left+'px';
      win.style.top=p.top+'px';
    });
  }

  function installDrag(win){
    const handle=win.querySelector('.pws-title');
    if(!handle)return;

    let dragging=false;
    let offsetX=0;
    let offsetY=0;

    const move=e=>{
      if(!dragging)return;
      const p=clampPosition(win,e.clientX-offsetX,e.clientY-offsetY);
      win.style.left=p.left+'px';
      win.style.top=p.top+'px';
    };

    const stop=e=>{
      if(!dragging)return;
      dragging=false;
      win.classList.remove('is-dragging');
      try{handle.releasePointerCapture(e.pointerId)}catch(_){}
      savePosition(win);
    };

    handle.addEventListener('pointerdown',e=>{
      if(e.button!==0||e.target.closest('.pws-close,button,a,input,select,textarea'))return;
      const rect=win.getBoundingClientRect();
      dragging=true;
      offsetX=e.clientX-rect.left;
      offsetY=e.clientY-rect.top;
      win.style.transform='none';
      win.style.left=rect.left+'px';
      win.style.top=rect.top+'px';
      win.classList.add('is-dragging');
      handle.setPointerCapture?.(e.pointerId);
      e.preventDefault();
    });

    handle.addEventListener('pointermove',move);
    handle.addEventListener('pointerup',stop);
    handle.addEventListener('pointercancel',stop);

    window.addEventListener('resize',()=>{
      if(!document.body.contains(win))return;
      const rect=win.getBoundingClientRect();
      const p=clampPosition(win,rect.left,rect.top);
      win.style.transform='none';
      win.style.left=p.left+'px';
      win.style.top=p.top+'px';
    });
  }

  function close(){
    root?.remove();
    root=null;
    filterKey=null;
  }

  function open(weaponKey=null){
    filterKey=weaponKey||null;

    if(root){
      root.querySelector('.pws-title span').textContent=filterKey?'Skins • '+(labels[filterKey]||filterKey):'Skins d’armes';
      render();
      return;
    }

    root=document.createElement('div');
    root.className='pws-backdrop';
    root.innerHTML=`<div class="pws-window" role="dialog" aria-modal="true" aria-label="Skins d'armes">
      <div class="pws-title">
        <span>${filterKey?'Skins • '+esc(labels[filterKey]||filterKey):'Skins d’armes'}</span>
        <button class="pws-close" aria-label="Fermer">×</button>
      </div>
      <div class="pws-toolbar">
        <span class="pws-tab">Collection</span>
        ${filterKey?'<button class="pws-show-all" type="button">Toutes les armes</button>':''}
      </div>
      <div class="pws-body"><div class="pws-empty">Chargement…</div></div>
      <div class="pws-status">Choisis le skin à utiliser.</div>
    </div>`;

    document.body.append(root);

    const win=root.querySelector('.pws-window');
    installDrag(win);
    applySavedPosition(win);

    root.querySelector('.pws-close').onclick=close;
    root.querySelector('.pws-show-all')?.addEventListener('click',()=>{
      filterKey=null;
      root.querySelector('.pws-title span').textContent='Skins d’armes';
      root.querySelector('.pws-show-all')?.remove();
      render();
    });

    root.addEventListener('mousedown',e=>{if(e.target===root)close()});
    load().catch(e=>{root.querySelector('.pws-body').innerHTML=`<div class="pws-empty">${esc(e.message)}</div>`});
  }

  document.addEventListener('click',e=>{
    const b=e.target.closest('button[data-paradise-index="6"],button[title="Skin d’armes"],button[aria-label="Skin d’armes"],button[title="Paramètres"],button[aria-label="Paramètres"]');
    if(!b)return;
    e.preventDefault();
    e.stopImmediatePropagation();
    open();
  },true);

  window.ParadiseWeaponSkins={open,close};
})();
