(() => {
  'use strict';
  const LOGO='/Dynamics/img/logos/hv_logo_p.png';
  const apply=()=>{
    document.querySelectorAll('.nitro-catalog .prc-brand-mark').forEach(mark=>{
      if(mark.dataset.cmsLogo==='1')return;
      mark.dataset.cmsLogo='1';
      mark.textContent='';
      const img=document.createElement('img');
      img.src=LOGO;
      img.alt='ParadiseRP';
      img.draggable=false;
      mark.appendChild(img);
    });
  };
  apply();
  new MutationObserver(apply).observe(document.body,{childList:true,subtree:true});
})();
