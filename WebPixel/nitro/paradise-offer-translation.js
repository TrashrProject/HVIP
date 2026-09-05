(()=>{
  'use strict';

  const TITLE_PREFIX='Prise de commande -';
  const STYLE_ID='paradise-order-offer-style';
  let scheduled=false;

  const text=node=>String(node?.textContent||'').trim();
  const leaves=root=>[root,...root.querySelectorAll('*')]
    .filter(node=>node instanceof Element&&node.children.length===0);

  const installStyle=()=>{
    if(document.getElementById(STYLE_ID))return;
    const style=document.createElement('style');
    style.id=STYLE_ID;
    style.textContent=`
      .paradise-order-offer{width:380px!important;min-width:380px!important;min-height:188px!important;height:auto!important;max-height:none!important;overflow:hidden!important;border:2px solid #075985!important;border-radius:10px!important;box-shadow:0 12px 34px #0008,0 0 0 2px #ffffff80!important;background:#eaf4fa!important}
      .paradise-order-offer,.paradise-order-offer *{box-sizing:border-box!important}
      .paradise-order-offer-title{font-weight:800!important;letter-spacing:.1px!important}
      .paradise-order-offer-prompt{display:block!important;margin:3px 4px 7px!important;color:#173b55!important;font-size:15px!important;font-weight:800!important;line-height:20px!important;text-align:left!important}
      .paradise-order-offer-detail{margin:0 4px 12px;color:#587487;font-size:11px;line-height:15px}
      .paradise-order-offer-actions{display:flex!important;gap:9px!important;width:100%!important;margin:3px 0 0!important;padding:0 4px 13px!important}
      .paradise-order-offer-actions>button{flex:1 1 0!important;min-width:112px!important;height:36px!important;margin:0!important;border-radius:5px!important;color:#fff!important;font-size:12px!important;font-weight:800!important;text-shadow:0 1px #0004!important;box-shadow:inset 0 1px #ffffff59,0 2px #0002!important}
      .paradise-order-offer-accept{background:linear-gradient(#35ca68,#1a9e49)!important;border:1px solid #117c35!important}
      .paradise-order-offer-refuse{background:linear-gradient(#ef665c,#c83c35)!important;border:1px solid #9e2c27!important}
      .paradise-order-offer-actions>button:hover{filter:brightness(1.08)}
      @media(max-width:430px){.paradise-order-offer{width:calc(100vw - 24px)!important;min-width:0!important}}
    `;
    document.head.appendChild(style);
  };

  const findWindow=titleLeaf=>{
    let current=titleLeaf.parentElement;
    while(current&&current!==document.body){
      const currentLeaves=leaves(current);
      const acceptLeaf=currentLeaves.find(node=>['offer.accept','Oui','Accepter'].includes(text(node)));
      const refuseLeaf=currentLeaves.find(node=>['generic.close','Fermer','Non','Refuser'].includes(text(node)));
      const acceptButton=acceptLeaf?.closest('button');
      const refuseButton=refuseLeaf?.closest('button');
      if(acceptButton&&refuseButton){
        const rect=current.getBoundingClientRect();
        if(rect.width>=250&&rect.width<=520&&rect.height>=70&&rect.height<=300){
          return {popup:current,acceptLeaf,refuseLeaf,acceptButton,refuseButton};
        }
      }
      current=current.parentElement;
    }
    return null;
  };

  const enhance=titleLeaf=>{
    if(!(titleLeaf instanceof Element))return;
    const found=findWindow(titleLeaf);
    if(!found||found.popup.dataset.paradiseOrderEnhanced==='1')return;
    const {popup,acceptLeaf,refuseLeaf,acceptButton,refuseButton}=found;

    const restaurant=text(titleLeaf).slice(TITLE_PREFIX.length).trim()||'Restaurant';
    const popupLeaves=leaves(popup);
    const promptLeaf=popupLeaves.find(node=>text(node)==='notifications.rpoffer')
      ||popupLeaves.find(node=>text(node)==='Accepter cette prise de commande ?');

    popup.dataset.paradiseOrderEnhanced='1';
    popup.classList.add('paradise-order-offer');
    titleLeaf.textContent=`Commande • ${restaurant}`;
    titleLeaf.classList.add('paradise-order-offer-title');

    if(promptLeaf){
      promptLeaf.textContent='Souhaitez-vous accepter cette prise de commande ?';
      promptLeaf.classList.add('paradise-order-offer-prompt');
      if(!promptLeaf.parentElement.querySelector('.paradise-order-offer-detail')){
        const detail=document.createElement('div');
        detail.className='paradise-order-offer-detail';
        detail.textContent='Le serveur pourra ensuite préparer et vous servir votre plat.';
        promptLeaf.insertAdjacentElement('afterend',detail);
      }
    }

    acceptLeaf.textContent='Accepter';
    refuseLeaf.textContent='Refuser';
    acceptButton.classList.add('paradise-order-offer-accept');
    refuseButton.classList.add('paradise-order-offer-refuse');
    const actions=acceptButton.parentElement;
    if(actions&&actions.contains(refuseButton))actions.classList.add('paradise-order-offer-actions');
  };

  const enhanceAll=()=>{
    scheduled=false;
    installStyle();
    leaves(document.body)
      .filter(node=>text(node).startsWith(TITLE_PREFIX))
      .forEach(enhance);
  };

  const schedule=()=>{
    if(scheduled)return;
    scheduled=true;
    requestAnimationFrame(enhanceAll);
  };

  new MutationObserver(schedule).observe(document.body,{childList:true,subtree:true,characterData:true});
  schedule();
})();
