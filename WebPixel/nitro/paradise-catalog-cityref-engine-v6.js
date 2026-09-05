(() => {
  'use strict';

  const ROOT = '.nitro-catalog.pc5-catalog';
  const clean = value => String(value || '').replace(/\s+/g, ' ').trim();

  function nativeContent(root)
  {
    return root.querySelector(':scope > .nitro-card-content');
  }

  function hasStore(root)
  {
    const content = nativeContent(root);
    if(!content) return false;
    return !!content.querySelector('#nitro-catalog-main-navigation,.nitro-catalog-navigation-grid-container');
  }

  function findFurniTab(root)
  {
    const tabs = root.querySelector(':scope > .nitro-card-tabs, :scope > [class*="card-tabs"]');
    if(!tabs) return null;
    return [ ...tabs.children ].find(tab => /^(?:Furni|Furniture|Mobilier)(?:\s*\(\d+\))?$/i.test(clean(tab.textContent))) || null;
  }

  function clickNode(node)
  {
    if(!node) return false;
    const target = node.matches?.('button,a,[role="button"],[role="tab"]') ? node : node.querySelector?.('button,a,[role="button"],[role="tab"]');
    (target || node).click?.();
    return true;
  }

  function ensureStore(root)
  {
    if(!(root instanceof HTMLElement)) return;
    if(hasStore(root))
    {
      root.dataset.pc6StoreReady = '1';
      return;
    }

    const attempts = Number(root.dataset.pc6BootAttempts || 0);
    if(attempts >= 8) return;

    const furni = findFurniTab(root);
    if(!furni) return;

    root.dataset.pc6BootAttempts = String(attempts + 1);
    clickNode(furni);

    setTimeout(() => {
      if(hasStore(root)) root.dataset.pc6StoreReady = '1';
    }, 140);
  }

  let queued = false;
  function refresh()
  {
    if(queued) return;
    queued = true;
    requestAnimationFrame(() => {
      queued = false;
      document.querySelectorAll(ROOT).forEach(ensureStore);
    });
  }

  function boot()
  {
    refresh();
    [120, 320, 700, 1200, 1800].forEach(delay => setTimeout(refresh, delay));
    new MutationObserver(refresh).observe(document.body, {
      childList:true,
      subtree:true,
      attributes:true,
      attributeFilter:['class','style']
    });
    console.info('[ParadiseRP] CityRef V6 native engine fix loaded');
  }

  document.readyState === 'loading'
    ? document.addEventListener('DOMContentLoaded', boot, { once:true })
    : boot();
})();
