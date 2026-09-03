(() => {
  'use strict';
  const ROOT='.nitro-catalog';
  let queued=false;
  const text=n=>(n?.textContent||'').replace(/\s+/g,' ').trim();
  const add=(n,c)=>{if(n instanceof HTMLElement)n.classList.add(c);return n};
  const translations=new Map([['Front Page','Accueil'],['Furni','Mobilier'],['Furniture','Mobilier'],['Clothing','Vêtements'],['Pets','Animaux'],['Building','Construction'],['Buy','Acheter'],['Purchase','Acheter']]);

  function banner(root){
    if(root.querySelector(':scope>.prc-brand-banner'))return;
    const b=document.createElement('div');
    b.className='prc-brand-banner';
    b.innerHTML='<div class="prc-brand-left"><span class="prc-brand-mark">▥</span><span class="prc-brand-copy"><strong>CATALOGUE</strong><em>ParadiseRP</em></span></div><div class="prc-brand-tagline">Des milliers de furnis<br>pour rendre votre ville unique !</div>';
    const c=root.querySelector(':scope>.nitro-card-content'); c?root.insertBefore(b,c):root.prepend(b);
  }
  function translate(root){
    root.querySelectorAll('button,a,span,p,label,div').forEach(n=>{if(n.children.length)return;const v=text(n),m=v.match(/\s*\(\d+\)\s*$/),k=v.replace(/\s*\(\d+\)\s*$/,'');if(translations.has(k))n.textContent=translations.get(k)+(m?m[0]:'');});
    root.querySelectorAll('input').forEach(i=>{const p=i.placeholder||'';if(/search|recherch/i.test(p))i.placeholder='Rechercher un furni...';});
  }
  function nav(root){
    let n=root.querySelector('.nitro-catalog-navigation,[class*="catalog-navigation"]');
    if(n)return n;
    return [...root.querySelectorAll('div,nav,ul')].filter(n=>{const t=text(n);return t.length<350&&[/Accueil|Front Page/i,/Furni|Mobilier/i,/Vêtements|Clothing/i,/Animaux|Pets/i,/Construction|Building/i].filter(r=>r.test(t)).length>=3}).sort((a,b)=>a.children.length-b.children.length)[0]||null;
  }
  function grid(root){return root.querySelector('.nitro-catalog-grid')||[...root.querySelectorAll('[class*="catalog-grid"]')].find(n=>!/grid-item/.test(String(n.className))&&n.children.length>=3)||null}
  function purchase(root){return root.querySelector('.nitro-catalog-purchase-component,[class*="catalog-purchase"],[class*="purchase-component"]')||null}
  function category(root,g){
    const direct=root.querySelector('.nitro-catalog-navigation-grid,[class*="navigation-grid"]');if(direct&&direct!==g)return direct;
    if(!g)return null;const gr=g.getBoundingClientRect();
    return [...root.querySelectorAll('div,ul')].filter(n=>n!==g&&!n.contains(g)&&n.children.length>=2&&n.getBoundingClientRect().right<=gr.left+40).sort((a,b)=>b.getBoundingClientRect().height-a.getBoundingClientRect().height)[0]||null;
  }
  function common(a,b,c,root){let n=a?.parentElement;while(n&&n!==root){if(n.contains(b)&&n.contains(c))return n;n=n.parentElement}return null}
  function branch(a,n){if(!a||!n)return null;while(n.parentElement&&n.parentElement!==a)n=n.parentElement;return n.parentElement===a?n:null}
  function decorate(root){
    if(!(root instanceof HTMLElement))return;
    root.classList.remove('paradise-catalog-v4','paradise-catalog-v5');root.classList.add('paradise-catalog-v7');
    banner(root);add(root.querySelector(':scope>.nitro-card-header'),'prc-native-header');translate(root);add(nav(root),'prc-topnav');
    root.querySelectorAll('input[type=text],input[type=search]').forEach(i=>{add(i,'prc-search-input');add(i.parentElement,'prc-search')});
    root.querySelectorAll('.nitro-catalog-grid-item,[class*="catalog-grid-item"]').forEach(n=>add(n,'prc-item'));
    root.querySelectorAll('button').forEach(b=>{if(/acheter|buy|purchase/i.test(text(b)))add(b,'prc-buy-button')});
    const g=grid(root),p=purchase(root),c=category(root,g);
    const isProductLayout=!!(g&&p&&c);
    root.classList.toggle('prc-products',isProductLayout);
    root.classList.toggle('prc-info-page',!isProductLayout);
    if(g)add(g,'prc-grid');
    if(p)add(p,'prc-purchase');
    if(c)add(c,'prc-category-panel');
    if(!isProductLayout)return;
    const a=common(c,g,p,root),bc=branch(a,c),bg=branch(a,g),bp=branch(a,p);
    if(a&&bc&&bg&&bp&&new Set([bc,bg,bp]).size===3){add(a,'prc-product-layout');add(bc,'prc-col-categories');add(bg,'prc-col-products');add(bp,'prc-col-preview')}
  }
  const scan=()=>document.querySelectorAll(ROOT).forEach(decorate);
  const schedule=()=>{if(queued)return;queued=true;requestAnimationFrame(()=>{queued=false;scan()})};
  const boot=()=>{scan();new MutationObserver(schedule).observe(document.body,{childList:true,subtree:true})};
  document.readyState==='loading'?document.addEventListener('DOMContentLoaded',boot,{once:true}):boot();
})();
