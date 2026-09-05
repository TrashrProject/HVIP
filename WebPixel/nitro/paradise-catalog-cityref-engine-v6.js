(() => {
  'use strict';

  const ROOT = '.nitro-catalog.pc5-catalog';
  const clean = value => String(value || '').replace(/\s+/g, ' ').trim();

  const contentOf = root => root.querySelector(':scope > .nitro-card-content');
  const tabsOf = root => root.querySelector(':scope > .nitro-card-tabs, :scope > [class*="card-tabs"]');
  const clickable = node => node?.matches?.('button,a,[role="button"],[role="tab"]') ? node : node?.querySelector?.('button,a,[role="button"],[role="tab"]') || node;

  function furniTab(root)
  {
    const tabs = tabsOf(root);
    if(!tabs) return null;
    return [ ...tabs.children ].find(tab => /^(?:Furni|Furniture|Mobilier)(?:\s*\(\d+\))?$/i.test(clean(tab.textContent))) || null;
  }

  function navOf(root)
  {
    const content = contentOf(root);
    return content?.querySelector('#nitro-catalog-main-navigation') || content?.querySelector('.nitro-catalog-navigation-grid-container') || null;
  }

  function categoryRows(root)
  {
    const nav = navOf(root);
    if(!nav) return [];
    let rows = [ ...nav.children ].filter(node => node instanceof HTMLElement && clean(node.textContent));
    if(!rows.length)
    {
      rows = [ ...nav.querySelectorAll('button,a,[role="button"],.list-group-item,.layout-grid-item') ]
        .filter(node => clean(node.textContent));
    }
    return rows;
  }

  function productCount(root)
  {
    const content = contentOf(root);
    const nav = navOf(root);
    if(!content) return 0;
    return [ ...content.querySelectorAll('.layout-grid-item,[class*="catalog-grid-item"]') ]
      .filter(node => !nav?.contains(node)).length;
  }

  function forceResize(root)
  {
    window.dispatchEvent(new Event('resize'));
    root.dispatchEvent(new Event('resize', { bubbles:true }));
    const content = contentOf(root);
    if(content)
    {
      content.style.setProperty('--pc-engine-touch', String(Date.now()));
      void content.offsetHeight;
    }
  }

  function clickFirstUsefulCategory(root)
  {
    const rows = categoryRows(root);
    if(!rows.length) return false;
    const preferred = rows.find(node => /mobilier|furni|maison|construction|d[eé]cor|int[eé]rieur/i.test(clean(node.textContent))) || rows[0];
    clickable(preferred)?.click?.();
    return true;
  }

  function syncShell(root)
  {
    // The V5 shell listens to DOM mutations. Touching the shell forces its synchronizer to re-run.
    const shell = root.querySelector(':scope > .pc5-shell');
    if(shell) shell.dataset.pcEngineTick = String(Date.now());
  }

  function bootstrap(root)
  {
    if(!(root instanceof HTMLElement)) return;

    const attempts = Number(root.dataset.pcEngineAttempts || 0);
    if(productCount(root) > 0)
    {
      root.dataset.pcEngineReady = '1';
      syncShell(root);
      return;
    }

    if(attempts >= 18) return;
    root.dataset.pcEngineAttempts = String(attempts + 1);

    const nav = navOf(root);
    if(!nav)
    {
      clickable(furniTab(root))?.click?.();
      forceResize(root);
      setTimeout(() => bootstrap(root), 120);
      return;
    }

    // Nitro loaded the Furni tree but no page yet: open a real category so offers are mounted.
    if(!productCount(root))
    {
      clickFirstUsefulCategory(root);
      forceResize(root);
      setTimeout(() => {
        forceResize(root);
        syncShell(root);
        bootstrap(root);
      }, 140);
    }
  }

  let queued = false;
  function refresh()
  {
    if(queued) return;
    queued = true;
    requestAnimationFrame(() => {
      queued = false;
      document.querySelectorAll(ROOT).forEach(bootstrap);
    });
  }

  function boot()
  {
    refresh();
    [80,180,320,520,800,1200,1800,2600].forEach(delay => setTimeout(refresh, delay));
    new MutationObserver(refresh).observe(document.body, {
      childList:true,
      subtree:true,
      attributes:true,
      attributeFilter:['class','style','aria-selected']
    });
    console.info('[ParadiseRP] CityRef native engine V7 loaded');
  }

  document.readyState === 'loading'
    ? document.addEventListener('DOMContentLoaded', boot, { once:true })
    : boot();
})();
