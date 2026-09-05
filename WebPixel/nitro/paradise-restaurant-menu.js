(()=>{
  const MARKER='PARADISE_RESTAURANT_MENU';
  const OVERLAY_ID='waverp-commands-overlay';
  const normalize=s=>String(s||'').replace(/\u00a0/g,' ').replace(/\r/g,'');
  const esc=s=>String(s||'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));

  const parseMenu=text=>{
    const lines=normalize(text).split('\n').map(v=>v.trim()).filter(Boolean);
    const nameLine=lines.find(v=>v.startsWith('RESTAURANT_NAME|'));
    const restaurant=nameLine?nameLine.slice('RESTAURANT_NAME|'.length):'Restaurant';
    const items=lines.filter(v=>v.startsWith('RESTAURANT|')).map(line=>{
      const parts=line.split('|');
      return {
        code:parts[1]||'',
        name:parts[2]||'Plat',
        id:Number(parts[3])||0,
        price:Number(parts[4])||0,
        hunger:Number(parts[5])||0,
        image:parts[6]||''
      };
    }).filter(item=>item.code&&item.id>0);
    return {restaurant,items};
  };

  const hasCloseControl=el=>[...el.querySelectorAll('button,[role="button"]')].some(b=>/^(fermer|close|×|✕|x)$/i.test(((b.getAttribute('aria-label')||'')+' '+(b.textContent||'')).trim()));
  const promoteWindow=el=>{let best=el,node=el;for(let i=0;i<7&&node;i++,node=node.parentElement){if(node===document.body)break;const text=normalize(node.innerText||node.textContent||'');if(!text.includes('Message de ParadiseRP'))continue;const r=node.getBoundingClientRect();if(r.width>=250&&r.width<=800&&r.height>=120&&r.height<=750&&hasCloseControl(node))best=node}return best};
  const findAlert=()=>{for(const raw of document.querySelectorAll('body div, body section')){if(raw.closest('#'+OVERLAY_ID))continue;const text=normalize(raw.innerText||raw.textContent||'');if(!text.includes(MARKER)||!text.includes('Message de ParadiseRP'))continue;const el=promoteWindow(raw);const rect=el.getBoundingClientRect();if(rect.width>=250&&rect.height>=120)return {el,text}}return null};
  const dismissOriginal=alert=>{if(!alert)return;const controls=[...alert.querySelectorAll('button,[role="button"]')];const close=controls.find(b=>/^(fermer|close)$/i.test((b.textContent||'').trim()))||controls.find(b=>/^(×|✕|x)$/i.test(((b.getAttribute('aria-label')||'')+' '+(b.textContent||'')).trim()));if(close)try{close.click()}catch{}if(alert.isConnected){alert.style.setProperty('display','none','important');alert.style.setProperty('visibility','hidden','important');alert.style.setProperty('pointer-events','none','important')}};
  const removeOverlay=()=>document.getElementById(OVERLAY_ID)?.remove();

  const render=menu=>{
    removeOverlay();
    let query='';
    const overlay=document.createElement('div');
    overlay.id=OVERLAY_ID;
    overlay.classList.add('wrc-food-mode');
    overlay.innerHTML=`<div class="wrc-window"><div class="wrc-titlebar">ParadiseRP — ${esc(menu.restaurant)}<button class="wrc-close" type="button">×</button></div><div class="wrc-content"><div class="wrc-head"><div><h2>Menu du restaurant</h2><div class="wrc-count"></div></div><div class="wrc-hint">Plats disponibles</div></div><div class="wrc-search-wrap"><span class="wrc-search-icon"></span><input class="wrc-search" type="text" placeholder="Rechercher un plat..."></div><div class="wrc-food-grid"></div><div class="wrc-footer"><span>:preparer [nom du plat]</span><button class="wrc-footer-close" type="button">Fermer</button></div></div></div>`;
    document.body.appendChild(overlay);
    const grid=overlay.querySelector('.wrc-food-grid');
    const count=overlay.querySelector('.wrc-count');
    const draw=()=>{
      const q=query.toLowerCase();
      const items=menu.items.filter(item=>!q||`${item.name} ${item.code} ${item.price}`.toLowerCase().includes(q));
      count.textContent=`${menu.items.length} plat(s) disponible(s)`;
      grid.innerHTML=items.length?items.map(item=>`<div class="wrc-food-card"><div class="wrc-food-image"><img src="${esc(item.image)}" alt="${esc(item.name)}"></div><div class="wrc-food-info"><div class="wrc-food-name">${esc(item.name)}</div><div class="wrc-food-id">${esc(item.code)}</div><div class="wrc-food-hunger">${item.price} crédits · +${item.hunger} point(s) de faim</div></div><button class="wrc-food-copy" data-copy=":preparer ${esc(item.code)}" type="button">Copier</button></div>`).join(''):'<div class="wrc-empty">Aucun plat trouvé.</div>';
      grid.querySelectorAll('.wrc-food-copy').forEach(button=>button.addEventListener('click',async()=>{try{await navigator.clipboard.writeText(button.dataset.copy||'');const old=button.textContent;button.textContent='Copié';setTimeout(()=>button.textContent=old,900)}catch{}}));
    };
    overlay.querySelector('.wrc-search').addEventListener('input',event=>{query=event.target.value;draw()});
    overlay.querySelector('.wrc-close').addEventListener('click',removeOverlay);
    overlay.querySelector('.wrc-footer-close').addEventListener('click',removeOverlay);
    draw();
  };

  const inspect=()=>{
    const found=findAlert();
    if(!found||found.el.dataset.paradiseRestaurantHandled==='1')return;
    const menu=parseMenu(found.text);
    if(!menu.items.length)return;
    found.el.dataset.paradiseRestaurantHandled='1';
    dismissOriginal(found.el);
    render(menu);
  };

  new MutationObserver(inspect).observe(document.body,{childList:true,subtree:true,characterData:true});
  setInterval(inspect,500);
  setTimeout(inspect,0);
})();
