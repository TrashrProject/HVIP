(()=>{
  'use strict';

  const TITLE_PREFIX='Prise de commande -';

  const leafText = element => String(element?.textContent || '').trim();

  const findLeaf = (root, predicate) => {
    if (!(root instanceof Element)) return null;
    const nodes=[root,...root.querySelectorAll('*')];
    return nodes.find(node=>node.children.length===0 && predicate(leafText(node))) || null;
  };

  const findPopup = root => {
    const titleLeaf=findLeaf(root,text=>text.startsWith(TITLE_PREFIX));
    if(!titleLeaf) return null;

    const yesLeaf=findLeaf(root,text=>text==='Oui' || text==='offer.accept');
    const noLeaf=findLeaf(root,text=>text==='Non' || text==='Fermer');
    if(!yesLeaf || !noLeaf) return null;

    const yesButton=yesLeaf.closest('button');
    const noButton=noLeaf.closest('button');
    if(!yesButton || !noButton) return null;

    // Cherche le plus petit ancêtre commun qui contient le titre ET les deux boutons.
    let current=titleLeaf.parentElement;
    while(current && current!==document.body){
      if(current.contains(yesButton) && current.contains(noButton)){
        const rect=current.getBoundingClientRect();
        if(rect.width>=260 && rect.width<=460 && rect.height>=80 && rect.height<=260){
          return { popup: current, titleLeaf, yesLeaf, noLeaf, yesButton, noButton };
        }
      }
      current=current.parentElement;
    }
    return null;
  };

  const applyPopupFix = root => {
    const found=findPopup(root);
    if(!found) return;

    const { popup, yesLeaf, noLeaf, yesButton, noButton }=found;

    if(leafText(yesLeaf)==='offer.accept') yesLeaf.textContent='Oui';
    if(leafText(noLeaf)==='Fermer') noLeaf.textContent='Non';

    const promptLeaf=findLeaf(popup,text=>text==='notifications.rpoffer' || text==='Accepter cette prise de commande ?');
    if(promptLeaf && leafText(promptLeaf)==='notifications.rpoffer') {
      promptLeaf.textContent='Accepter cette prise de commande ?';
    }

    // On agrandit LE VRAI conteneur de fenêtre, pas juste la ligne des boutons.
    // Hauteur fixe volontairement plus grande pour garder un vrai espace sous Oui / Non.
    popup.style.setProperty('height','150px','important');
    popup.style.setProperty('min-height','150px','important');
    popup.style.setProperty('max-height','none','important');
    popup.style.setProperty('overflow','visible','important');
    popup.style.setProperty('box-sizing','border-box','important');

    const buttonRow=yesButton.parentElement;
    if(buttonRow){
      buttonRow.style.setProperty('margin-bottom','22px','important');
      buttonRow.style.setProperty('padding-bottom','10px','important');
    }

    // Certains wrappers Nitro ont aussi une hauteur fixe : on les libère.
    let parent=popup.parentElement;
    for(let i=0;i<3 && parent && parent!==document.body;i++,parent=parent.parentElement){
      const rect=parent.getBoundingClientRect();
      if(rect.width>=260 && rect.width<=480 && rect.height<=260){
        parent.style.setProperty('min-height','150px','important');
        parent.style.setProperty('height','auto','important');
        parent.style.setProperty('max-height','none','important');
        parent.style.setProperty('overflow','visible','important');
      }
    }
  };

  const inspect = node => {
    const element=node instanceof Element ? node : node?.parentElement;
    if(!element) return;

    const text=String(element.textContent || '');
    if(text.includes(TITLE_PREFIX)){
      applyPopupFix(element);
      requestAnimationFrame(()=>applyPopupFix(element));
      setTimeout(()=>applyPopupFix(element),50);
      setTimeout(()=>applyPopupFix(element),200);
    }

    element.querySelectorAll?.('*').forEach(child=>{
      if(String(child.textContent || '').includes(TITLE_PREFIX)) applyPopupFix(child);
    });
  };

  new MutationObserver(mutations=>{
    for(const mutation of mutations){
      inspect(mutation.target);
      mutation.addedNodes.forEach(inspect);
    }
  }).observe(document.body,{childList:true,subtree:true,characterData:true});

  inspect(document.body);
})();
