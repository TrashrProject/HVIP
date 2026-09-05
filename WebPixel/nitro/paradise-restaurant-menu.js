(()=>{
  'use strict';

  const SIGNAL='PARADISE_RESTAURANT_OPEN:';
  const OVERLAY_ID='waverp-commands-overlay';
  const API='/restaurant-menu.php';
  const seenSignals=new Set();
  let loading=false;

  const esc=value=>String(value??'').replace(/[&<>"']/g,char=>({
    '&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'
  })[char]);
  const removeOverlay=()=>{
    const overlay=document.getElementById(OVERLAY_ID);
    overlay?._restaurantCleanup?.();
    overlay?.remove();
  };

  const makeDraggable=(panel,handle)=>{
    let drag=null;
    handle.style.cursor='move';
    handle.style.touchAction='none';
    handle.style.userSelect='none';

    const clamp=(value,min,max)=>Math.min(Math.max(value,min),Math.max(min,max));
    const keepOnScreen=(left,top)=>({
      left:clamp(left,4,window.innerWidth-panel.offsetWidth-4),
      top:clamp(top,4,window.innerHeight-panel.offsetHeight-4)
    });
    const place=(left,top)=>{
      const position=keepOnScreen(left,top);
      panel.style.left=`${position.left}px`;
      panel.style.top=`${position.top}px`;
    };

    handle.addEventListener('pointerdown',event=>{
      if(event.button!==0||event.target.closest('button,input,a'))return;
      const rect=panel.getBoundingClientRect();
      panel.style.position='fixed';
      panel.style.width=`${rect.width}px`;
      panel.style.height=`${rect.height}px`;
      panel.style.margin='0';
      panel.style.right='auto';
      panel.style.bottom='auto';
      place(rect.left,rect.top);
      drag={pointerId:event.pointerId,x:event.clientX,y:event.clientY,left:rect.left,top:rect.top};
      handle.setPointerCapture(event.pointerId);
      event.preventDefault();
    });
    handle.addEventListener('pointermove',event=>{
      if(!drag||event.pointerId!==drag.pointerId)return;
      place(drag.left+event.clientX-drag.x,drag.top+event.clientY-drag.y);
      event.preventDefault();
    });
    const stop=event=>{
      if(!drag||event.pointerId!==drag.pointerId)return;
      drag=null;
      if(handle.hasPointerCapture(event.pointerId))handle.releasePointerCapture(event.pointerId);
    };
    handle.addEventListener('pointerup',stop);
    handle.addEventListener('pointercancel',stop);
    const handleResize=()=>{
      if(panel.style.position==='fixed')place(panel.offsetLeft,panel.offsetTop);
    };
    window.addEventListener('resize',handleResize);
    return ()=>window.removeEventListener('resize',handleResize);
  };

  const render=menu=>{
    removeOverlay();
    let query='';
    const overlay=document.createElement('div');
    overlay.id=OVERLAY_ID;
    overlay.className='wrc-food-mode';
    overlay.innerHTML=`<div class="wrc-window"><div class="wrc-titlebar">ParadiseRP — ${esc(menu.restaurant)}<button class="wrc-close" type="button" aria-label="Fermer">×</button></div><div class="wrc-content"><div class="wrc-head"><div><h2>Menu du restaurant</h2><div class="wrc-count"></div></div><div class="wrc-hint">Plats disponibles</div></div><div class="wrc-search-wrap"><span class="wrc-search-icon"></span><input class="wrc-search" type="text" placeholder="Rechercher un plat..."></div><div class="wrc-food-grid"></div><div class="wrc-footer"><span>:preparer [nom du plat]</span><button class="wrc-footer-close" type="button">Fermer</button></div></div></div>`;
    document.body.appendChild(overlay);

    const panel=overlay.querySelector('.wrc-window');
    overlay._restaurantCleanup=makeDraggable(panel,overlay.querySelector('.wrc-titlebar'));
    const grid=overlay.querySelector('.wrc-food-grid');
    const count=overlay.querySelector('.wrc-count');
    const draw=()=>{
      const term=query.trim().toLowerCase();
      const items=menu.items.filter(item=>!term||`${item.name} ${item.code} ${item.price}`.toLowerCase().includes(term));
      count.textContent=`${menu.items.length} plat(s) disponible(s)`;
      grid.innerHTML=items.length?items.map(item=>`<div class="wrc-food-card"><div class="wrc-food-image"><img src="${esc(item.image)}" alt="${esc(item.name)}"></div><div class="wrc-food-info"><div class="wrc-food-name">${esc(item.name)}</div><div class="wrc-food-id">${esc(item.code)}</div><div class="wrc-food-hunger">${Number(item.price)||0} crédits · +${Number(item.hunger)||0} point(s) de faim</div></div><button class="wrc-food-copy" data-copy=":preparer ${esc(item.code)}" type="button">Copier</button></div>`).join(''):'<div class="wrc-empty">Aucun plat trouvé.</div>';
      grid.querySelectorAll('.wrc-food-copy').forEach(button=>button.addEventListener('click',async()=>{
        try{
          await navigator.clipboard.writeText(button.dataset.copy||'');
          const label=button.textContent;
          button.textContent='Copié';
          setTimeout(()=>button.textContent=label,900);
        }catch{}
      }));
    };

    overlay.querySelector('.wrc-search').addEventListener('input',event=>{query=event.target.value;draw()});
    overlay.querySelector('.wrc-close').addEventListener('click',removeOverlay);
    overlay.querySelector('.wrc-footer-close').addEventListener('click',removeOverlay);
    draw();
  };

  const loadMenu=async()=>{
    if(loading)return;
    loading=true;
    try{
      const response=await fetch(`${API}?t=${Date.now()}`,{
        credentials:'same-origin',cache:'no-store',headers:{Accept:'application/json'}
      });
      const payload=await response.json();
      if(!response.ok||!payload.ok)throw new Error(payload.error||'Menu indisponible.');
      render(payload);
    }catch(error){
      console.error('[ParadiseRP Restaurant]',error);
    }finally{
      loading=false;
    }
  };

  const consumeCandidate=element=>{
    if(!(element instanceof Element))return;
    const text=String(element.textContent||'');
    const start=text.indexOf(SIGNAL);
    if(start<0)return;
    const token=(text.slice(start).match(/^PARADISE_RESTAURANT_OPEN:\d+/)||[])[0];
    if(!token||seenSignals.has(token))return;
    seenSignals.add(token);
    element.style.setProperty('display','none','important');
    loadMenu();
  };

  const inspectNode=node=>{
    const element=node instanceof Element?node:node?.parentElement;
    if(!element)return;
    const bubble=element.closest('.bubble-container,.chat-bubble');
    if(bubble)consumeCandidate(bubble);
    if(element.matches?.('.bubble-container,.chat-bubble'))consumeCandidate(element);
    element.querySelectorAll?.('.bubble-container,.chat-bubble').forEach(consumeCandidate);
  };

  new MutationObserver(mutations=>mutations.forEach(mutation=>{
    inspectNode(mutation.target);
    mutation.addedNodes.forEach(inspectNode);
  })).observe(document.body,{childList:true,subtree:true,characterData:true});
  document.querySelectorAll('.bubble-container,.chat-bubble').forEach(consumeCandidate);
})();
