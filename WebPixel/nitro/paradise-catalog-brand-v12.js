(() => {
  'use strict';
  const ROOT='.nitro-catalog';
  const LOGO='/Dynamics/img/logos/hv_logo_p.png';

  function apply(root){
    if(!(root instanceof Element)) return;
    root.querySelectorAll('.prc-brand-mark').forEach(mark=>{
      if(mark.querySelector('img')) return;
      mark.textContent='';
      const img=document.createElement('img');
      img.src=LOGO;
      img.alt='ParadiseRP';
      img.draggable=false;
      mark.appendChild(img);
    });
  }

  document.querySelectorAll(ROOT).forEach(apply);
  new MutationObserver(mutations=>{
    for(const mutation of mutations){
      for(const node of mutation.addedNodes){
        if(!(node instanceof Element)) continue;
        if(node.matches(ROOT)) apply(node);
        node.querySelectorAll?.(ROOT).forEach(apply);
      }
    }
  }).observe(document.body,{childList:true,subtree:true});
})();
