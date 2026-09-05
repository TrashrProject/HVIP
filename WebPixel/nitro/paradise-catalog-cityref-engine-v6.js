(() => {
  'use strict';

  const ROOT = '.nitro-catalog.pc5-catalog';
  const clean = value => String(value || '').replace(/\s+/g, ' ').trim();
  const state = new WeakMap();

  const contentOf = root => root.querySelector(':scope > .nitro-card-content');
  const tabsOf = root => root.querySelector(':scope > .nitro-card-tabs, :scope > [class*="card-tabs"]');

  function engineState(root)
  {
    if(!state.has(root)) state.set(root, { lastSegment:'', tried:new Set(), bound:false, timer:null });
    return state.get(root);
  }

  function clickTarget(node)
  {
    if(!node) return null;
    if(node.matches?.('.layout-grid-item,.nav-item,button,a,[role="button"],[role="tab"]')) return node;
    return node.querySelector?.(':scope > .layout-grid-item') ||
      node.querySelector?.('.layout-grid-item') ||
      node.querySelector?.('button,a,[role="button"],[role="tab"]') || node;
  }

  function nativeTabs(root)
  {
    const tabs = tabsOf(root);
    return tabs ? [ ...tabs.children ].filter(node => node instanceof HTMLElement) : [];
  }

  function dataTab(root)
  {
    const tabs = nativeTabs(root);
    return tabs.find(tab => /Catalogue ParadiseRP complet|9967200/i.test(clean(tab.textContent))) ||
      tabs.find(tab => /^(?:Furni|Furniture|Mobilier)(?:\s*\(\d+\))?$/i.test(clean(tab.textContent))) || null;
  }

  function isActive(node)
  {
    return !!node && (node.matches?.('.active,[aria-selected="true"]') || !!node.querySelector?.('.active,[aria-selected="true"]'));
  }

  function navOf(root)
  {
    const content = contentOf(root);
    if(!content) return null;
    return content.querySelector('#nitro-catalog-main-navigation') || content.querySelector('.nitro-catalog-navigation-grid-container');
  }

  function categoryRows(root)
  {
    const nav = navOf(root);
    if(!nav) return [];

    const sections = [ ...nav.querySelectorAll('.nitro-catalog-navigation-section') ]
      .filter(node => node instanceof HTMLElement && clean(node.textContent));
    if(sections.length) return sections;

    return [ ...nav.querySelectorAll('.layout-grid-item,button,a,[role="button"],.list-group-item') ]
      .filter(node => node instanceof HTMLElement && clean(node.textContent));
  }

  function productNodes(root)
  {
    const content = contentOf(root);
    const nav = navOf(root);
    if(!content) return [];
    return [ ...content.querySelectorAll('.layout-grid-item,[class*="catalog-grid-item"]') ]
      .filter(node => !nav?.contains(node));
  }

  function activeSegment(root)
  {
    return root.querySelector(':scope > .pc5-shell .pc5-segment.is-active')?.dataset?.seg || 'official';
  }

  function segmentPattern(segment)
  {
    switch(segment)
    {
      case 'city': return /construction|ville|police|justice|h[oô]pital|transport|commerce|bureau|nature|jeu|sport/i;
      case 'utility': return /wired|bots?|group|market|utilitaire|staff/i;
      case 'rares': return /rare|limited|collector|crackable|balloon/i;
      case 'custom': return /custom|paradise|habborp|massif|collection/i;
      default: return /mobilier|maison|d[eé]cor|int[eé]rieur|ext[eé]rieur|room event|credit furni/i;
    }
  }

  function forceLayout(root)
  {
    const content = contentOf(root);
    if(content)
    {
      void content.offsetWidth;
      void content.offsetHeight;
    }
    window.dispatchEvent(new Event('resize'));
  }

  function touchShell(root)
  {
    const shell = root.querySelector(':scope > .pc5-shell');
    if(shell) shell.dataset.pcEngineTick = String(Date.now());
  }

  function showStore(shell)
  {
    if(!shell) return;
    shell.querySelectorAll('.pc5-view').forEach(panel => panel.classList.toggle('is-visible', panel.dataset.viewPanel === 'store'));
    shell.querySelectorAll('.pc5-menu-btn').forEach(button => button.classList.remove('is-active'));
  }

  function bindCustomCategoryClicks(root)
  {
    const st = engineState(root);
    if(st.bound) return;
    const shell = root.querySelector(':scope > .pc5-shell');
    if(!shell) return;

    st.bound = true;
    shell.querySelector('.pc5-left')?.addEventListener('click', event => {
      const button = event.target.closest('.pc5-category[data-index]');
      if(!button) return;

      event.preventDefault();
      event.stopImmediatePropagation();

      const rows = categoryRows(root);
      const row = rows[Number(button.dataset.index)];
      const target = clickTarget(row);
      if(!target) return;

      shell.querySelectorAll('.pc5-category').forEach(item => item.classList.remove('is-active'));
      button.classList.add('is-active');
      showStore(shell);
      target.click();
      forceLayout(root);
      setTimeout(() => { touchShell(root); forceLayout(root); }, 120);
    }, true);
  }

  function chooseCategory(root, segment)
  {
    const st = engineState(root);
    const rows = categoryRows(root);
    if(!rows.length) return false;

    const pattern = segmentPattern(segment);
    const info = rows.map(row => {
      const target = clickTarget(row);
      const label = clean(target?.textContent || row.textContent);
      const branch = !!target?.querySelector?.('.fa-icon');
      return { row, target, label, branch };
    }).filter(item => item.target && item.label);

    let candidates = info.filter(item => pattern.test(item.label) && !st.tried.has(item.label));
    if(!candidates.length) candidates = info.filter(item => !item.branch && !st.tried.has(item.label));
    if(!candidates.length) candidates = info.filter(item => !st.tried.has(item.label));
    if(!candidates.length) return false;

    const chosen = candidates.find(item => !item.branch) || candidates[0];
    st.tried.add(chosen.label);
    chosen.target.click();
    forceLayout(root);
    return true;
  }

  function bootstrap(root)
  {
    if(!(root instanceof HTMLElement)) return;
    bindCustomCategoryClicks(root);

    const st = engineState(root);
    const segment = activeSegment(root);
    if(st.lastSegment !== segment)
    {
      st.lastSegment = segment;
      st.tried.clear();
      delete root.dataset.pcEngineReady;
    }

    const tab = dataTab(root);
    if(!tab) return;

    if(!isActive(tab))
    {
      clickTarget(tab)?.click?.();
      forceLayout(root);
      clearTimeout(st.timer);
      st.timer = setTimeout(() => bootstrap(root), 180);
      return;
    }

    const nav = navOf(root);
    if(!nav || !categoryRows(root).length)
    {
      clearTimeout(st.timer);
      st.timer = setTimeout(() => bootstrap(root), 180);
      return;
    }

    touchShell(root);

    if(productNodes(root).length > 0)
    {
      root.dataset.pcEngineReady = '1';
      forceLayout(root);
      touchShell(root);
      return;
    }

    chooseCategory(root, segment);
    clearTimeout(st.timer);
    st.timer = setTimeout(() => {
      forceLayout(root);
      touchShell(root);
      bootstrap(root);
    }, 180);
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
    [100,250,500,900,1400,2200,3200].forEach(delay => setTimeout(refresh, delay));
    new MutationObserver(refresh).observe(document.body, {
      childList:true,
      subtree:true,
      attributes:true,
      attributeFilter:['class','style','aria-selected']
    });
    console.info('[ParadiseRP] CityRef native engine V8 loaded');
  }

  document.readyState === 'loading'
    ? document.addEventListener('DOMContentLoaded', boot, { once:true })
    : boot();
})();
