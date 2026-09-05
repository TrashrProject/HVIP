(()=>{
  'use strict';

  const TITLE_PREFIX='Prise de commande -';
  const ENHANCED_TITLE_PREFIX='Commande \u2022 ';
  const STYLE_ID='paradise-order-offer-style';
  const CHECK_INTERVAL_MS=250;
  let scheduled=false;

  const cleanText=node=>String(node?.textContent||'').trim();
  const leafElements=root=>{
    if(!(root instanceof Element))return [];
    return [root,...root.querySelectorAll('*')]
      .filter(node=>node instanceof Element&&node.children.length===0);
  };
  const setText=(node,value)=>{
    if(node&&cleanText(node)!==value)node.textContent=value;
  };

  const installStyle=()=>{
    if(document.getElementById(STYLE_ID))return;
    const style=document.createElement('style');
    style.id=STYLE_ID;
    style.textContent=`
      .nitro-alert.nitro-alert-offer,.nitro-alert.nitro-alert-offer.paradise-order-offer{width:390px!important;min-width:390px!important;height:190px!important;min-height:190px!important;max-height:190px!important;overflow:hidden!important;border:2px solid #0877ae!important;border-radius:10px!important;background:linear-gradient(180deg,#eef8fd 0%,#dcecf5 100%)!important;box-shadow:0 14px 38px #0009,0 0 0 2px #ffffff80!important;resize:none!important}
      .nitro-alert.nitro-alert-offer>.content-area{height:148px!important;min-height:148px!important;max-height:148px!important;overflow:hidden!important;padding:12px 14px 14px!important}
      .paradise-order-offer,.paradise-order-offer *{box-sizing:border-box!important}
      .paradise-order-offer-title{font-weight:800!important;letter-spacing:.1px!important;text-shadow:0 1px #00466d!important}
      .paradise-order-offer-prompt{display:block!important;margin:4px 6px 5px!important;color:#153d57!important;font-size:14px!important;font-weight:800!important;line-height:19px!important;text-align:left!important}
      .paradise-order-offer-detail{margin:0 6px 12px!important;color:#527185!important;font-size:11px!important;line-height:15px!important;text-align:left!important}
      .paradise-order-offer-actions{display:flex!important;gap:10px!important;width:100%!important;margin:2px 0 0!important;padding:0 6px 13px!important}
      .paradise-order-offer-actions>button,.nitro-alert.nitro-alert-offer button.btn-success,.nitro-alert.nitro-alert-offer button.btn-danger{flex:1 1 0!important;min-width:120px!important;height:37px!important;margin:0!important;border-radius:6px!important;color:#fff!important;font-size:13px!important;font-weight:800!important;text-shadow:0 1px #0005!important;box-shadow:inset 0 1px #ffffff66,0 2px 3px #0003!important;transition:filter .12s ease,transform .12s ease!important}
      .paradise-order-offer-actions>button:hover{filter:brightness(1.08)!important;transform:translateY(-1px)!important}
      .paradise-order-offer-actions>button:active{transform:translateY(1px)!important}
      .paradise-order-offer-accept,.nitro-alert.nitro-alert-offer button.btn-success{background:linear-gradient(#39d66f,#169847)!important;border:1px solid #087735!important}
      .paradise-order-offer-refuse,.nitro-alert.nitro-alert-offer button.btn-danger{background:linear-gradient(#ef665e,#c63a34)!important;border:1px solid #982923!important}
      @media(max-width:430px){.nitro-alert.nitro-alert-offer,.nitro-alert.nitro-alert-offer.paradise-order-offer{width:calc(100vw - 24px)!important;min-width:0!important}}
    `;
    document.head.appendChild(style);
  };

  const findPopup=anchor=>{
    let current=anchor?.parentElement;
    while(current&&current!==document.body){
      const currentLeaves=leafElements(current);
      const acceptLeaf=currentLeaves.find(node=>['offer.accept','Accepter'].includes(cleanText(node)));
      const refuseLeaf=currentLeaves.find(node=>['generic.close','Fermer','Non','Refuser'].includes(cleanText(node)));
      const acceptButton=acceptLeaf?.closest('button');
      const refuseButton=refuseLeaf?.closest('button');
      if(acceptButton&&refuseButton){
        const popup=current.closest('.nitro-alert.nitro-alert-offer')||current;
        return {popup,acceptLeaf,refuseLeaf,acceptButton,refuseButton};
      }
      current=current.parentElement;
    }
    return null;
  };

  const enhanceFromAnchor=anchor=>{
    const found=findPopup(anchor);
    if(!found)return;
    const {popup,acceptLeaf,refuseLeaf,acceptButton,refuseButton}=found;
    const popupLeaves=leafElements(popup);
    const rawTitle=popupLeaves.find(node=>cleanText(node).startsWith(TITLE_PREFIX));
    const enhancedTitle=popupLeaves.find(node=>cleanText(node).startsWith(ENHANCED_TITLE_PREFIX));
    const titleLeaf=rawTitle||enhancedTitle;
    if(!titleLeaf)return;

    const previousRestaurant=popup.dataset.paradiseRestaurant||'';
    const titleText=cleanText(titleLeaf);
    const restaurant=titleText.startsWith(TITLE_PREFIX)
      ?titleText.slice(TITLE_PREFIX.length).trim()
      :titleText.slice(ENHANCED_TITLE_PREFIX.length).trim()||previousRestaurant;
    popup.dataset.paradiseRestaurant=restaurant||'Restaurant';
    popup.classList.add('paradise-order-offer');
    titleLeaf.classList.add('paradise-order-offer-title');
    setText(titleLeaf,`${ENHANCED_TITLE_PREFIX}${popup.dataset.paradiseRestaurant}`);

    const currentLeaves=leafElements(popup);
    const promptLeaf=currentLeaves.find(node=>[
      'notifications.rpoffer',
      'Accepter cette prise de commande ?',
      'Souhaitez-vous accepter cette prise de commande ?'
    ].includes(cleanText(node)));
    if(promptLeaf){
      promptLeaf.classList.add('paradise-order-offer-prompt');
      setText(promptLeaf,'Souhaitez-vous accepter cette prise de commande ?');
      let detail=promptLeaf.parentElement?.querySelector('.paradise-order-offer-detail');
      if(!detail){
        detail=document.createElement('div');
        detail.className='paradise-order-offer-detail';
        promptLeaf.insertAdjacentElement('afterend',detail);
      }
      setText(detail,'Le serveur pourra ensuite pr\u00e9parer et vous servir votre plat.');
    }

    setText(acceptLeaf,'Accepter');
    setText(refuseLeaf,'Refuser');
    acceptButton.classList.add('paradise-order-offer-accept');
    refuseButton.classList.add('paradise-order-offer-refuse');
    const actions=acceptButton.parentElement;
    if(actions&&actions.contains(refuseButton))actions.classList.add('paradise-order-offer-actions');
  };

  const enhanceAll=()=>{
    scheduled=false;
    if(!document.body)return;
    installStyle();
    const candidates=leafElements(document.body).filter(node=>{
      const value=cleanText(node);
      return value.startsWith(TITLE_PREFIX)||value.startsWith(ENHANCED_TITLE_PREFIX);
    });
    candidates.forEach(enhanceFromAnchor);
  };

  const schedule=()=>{
    if(scheduled)return;
    scheduled=true;
    requestAnimationFrame(enhanceAll);
  };

  const start=()=>{
    new MutationObserver(schedule).observe(document.body,{childList:true,subtree:true,characterData:true});
    window.setInterval(enhanceAll,CHECK_INTERVAL_MS);
    schedule();
  };

  if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',start,{once:true});
  else start();
})();
