(() => {
  'use strict';

  const ROOT = '.nitro-catalog';
  const BUILD = 'paradise-catalog-zero-v3';
  const clean = value => String(value || '').replace(/\s+/g, ' ').trim();

  const TAB_DEFS = [
    { key:'home', pattern:/^Front Page(?:\s*\(\d+\))?$/i, label:'Accueil' },
    { key:'furni', pattern:/^(?:Furni|Furniture|Mobilier)(?:\s*\(\d+\))?$/i, label:'Mobilier' },
    { key:'clothing', pattern:/^(?:Clothing|Vêtements)(?:\s*\(\d+\))?$/i, label:'Vêtements' },
    { key:'pets', pattern:/^(?:Pets|Animaux)(?:\s*\(\d+\))?$/i, label:'Animaux' },
    { key:'building', pattern:/^(?:Building|Construction)(?:\s*\(\d+\))?$/i, label:'Construction' },
    { key:'staff', pattern:/^Staff(?:\s*\(\d+\))?$/i, label:'Staff' },
    { key:'all', pattern:/^Catalogue ParadiseRP complet.*$/i, label:'Tout' }
  ];

  const getHeader = root => root.querySelector(':scope > .nitro-card-header, :scope > [class*="card-header"]');
  const getTabs = root => root.querySelector(':scope > .nitro-card-tabs, :scope > [class*="card-tabs"]');
  const getClickable = tab => tab?.matches?.('button,a,[role="tab"],[role="button"]') ? tab : tab?.querySelector?.('button,a,[role="tab"],[role="button"]');

  function nativeClose(root)
  {
    const header = getHeader(root);
    if(!header) return null;
    return [ ...header.querySelectorAll('button,[role="button"],.close,[class*="close"],[class*="cross"]') ]
      .find(node => !node.classList.contains('pz-close')) || null;
  }

  function closeCatalog(root)
  {
    const button = nativeClose(root);
    if(button) { button.click(); return; }
    document.dispatchEvent(new KeyboardEvent('keydown', { key:'Escape', code:'Escape', bubbles:true }));
  }

  function nativeCurrencies()
  {
    const rows = [];
    document.querySelectorAll('.nitro-purse .nitro-currency-icon').forEach(icon => {
      const row = icon.closest('.nitro-purse-seasonal-currency,.nitro-purse-button');
      if(row && !row.closest('.pz-header') && !rows.includes(row)) rows.push(row);
    });
    return rows.slice(0, 3);
  }

  function buildWallet(source, index)
  {
    const wallet = document.createElement('div');
    wallet.className = 'pz-wallet';
    wallet.dataset.pzCurrency = String(index);

    const icon = source.querySelector('.nitro-currency-icon,.currency-icon,[class*="currency-icon"],img,svg,i');
    const iconHost = document.createElement('span');
    iconHost.className = 'pz-wallet-icon';
    if(icon) iconHost.appendChild(icon.cloneNode(true));

    const values = [ ...source.querySelectorAll('span,div,p') ].filter(node => !node.children.length && /\d/.test(clean(node.textContent)));
    const valueHost = document.createElement('span');
    valueHost.className = 'pz-wallet-value';
    valueHost.textContent = clean(values.at(-1)?.textContent || source.textContent) || '—';

    const add = document.createElement('button');
    add.type = 'button';
    add.className = 'pz-wallet-add';
    add.textContent = '+';
    add.setAttribute('aria-label', 'Options de monnaie');

    wallet.append(iconHost, valueHost, add);
    return wallet;
  }

  function ensureHeader(root)
  {
    let header = root.querySelector(':scope > .pz-header');
    if(!header)
    {
      header = document.createElement('div');
      header.className = 'pz-header';
      header.innerHTML = `
        <div class="pz-brand">
          <span class="pz-logo"><img src="/Dynamics/img/logos/hv_logo_p.png" alt="ParadiseRP" draggable="false"></span>
          <span class="pz-brand-copy"><strong>PARADISE MARKET</strong><small>CITY STORE</small></span>
        </div>
        <div class="pz-tagline">Le marché de ta ville</div>
        <div class="pz-wallets" aria-label="Soldes"></div>
        <button type="button" class="pz-close" aria-label="Fermer" title="Fermer">×</button>`;
      root.prepend(header);

      const close = header.querySelector('.pz-close');
      close?.addEventListener('pointerdown', event => event.stopPropagation());
      close?.addEventListener('click', event => {
        event.preventDefault();
        event.stopPropagation();
        closeCatalog(root);
      });
    }

    const host = header.querySelector('.pz-wallets');
    const sources = nativeCurrencies();
    const signature = sources.map(source => clean(source.textContent) + '|' + (source.querySelector('[class*="currency-icon"]')?.className || '')).join('||');
    if(host && host.dataset.pzSignature !== signature)
    {
      host.replaceChildren(...sources.map(buildWallet));
      host.dataset.pzSignature = signature;
    }

    if(host && host.dataset.pzBound !== '1')
    {
      host.dataset.pzBound = '1';
      host.addEventListener('click', event => {
        const button = event.target.closest('.pz-wallet-add');
        if(!button) return;
        event.preventDefault();
        event.stopPropagation();
        const wallet = button.closest('.pz-wallet');
        const source = nativeCurrencies()[Number(wallet?.dataset.pzCurrency || 0)];
        (source?.closest('.nitro-purse-button') || source)?.click();
      });
    }
  }

  function replaceFirstText(node, label)
  {
    const walker = document.createTreeWalker(node, NodeFilter.SHOW_TEXT);
    let current = walker.nextNode();
    while(current)
    {
      const value = clean(current.nodeValue);
      if(value && /[A-Za-zÀ-ÿ]/.test(value))
      {
        current.nodeValue = label;
        return;
      }
      current = walker.nextNode();
    }
  }

  function decorateTabs(root)
  {
    const tabs = getTabs(root);
    if(!tabs) return [];
    tabs.classList.add('pz-tabs');

    const result = [];
    [ ...tabs.children ].filter(node => node instanceof HTMLElement).forEach(tab => {
      const value = clean(tab.textContent);
      const def = TAB_DEFS.find(item => item.pattern.test(value) || tab.dataset.pzTab === item.key);
      if(!def) return;
      tab.classList.add('pz-tab');
      tab.dataset.pzTab = def.key;
      replaceFirstText(tab, def.label);
      const clickable = getClickable(tab);
      clickable?.setAttribute('aria-label', def.label);
      clickable?.setAttribute('title', def.label);
      result.push(tab);
    });
    return result;
  }

  function isTabActive(tab)
  {
    return !!tab && (tab.matches('.active,[aria-selected="true"]') || !!tab.querySelector('.active,[aria-selected="true"]'));
  }

  function clickTab(root, key)
  {
    const tab = root.querySelector(`.pz-tab[data-pz-tab="${ key }"]`);
    getClickable(tab)?.click();
  }

  function currentMode(root, tabs)
  {
    const active = tabs.find(isTabActive);
    if(active) return active.dataset.pzTab || 'store';
    const hasNav = !!root.querySelector('#nitro-catalog-main-navigation,.nitro-catalog-navigation-grid-container');
    const hasProducts = !!root.querySelector('.layout-grid-item,[class*="catalog-grid-item"]');
    return (!hasNav && !hasProducts) ? 'home' : 'store';
  }

  function ensureHomeDashboard(root, content)
  {
    if(!content) return;
    let dashboard = content.querySelector(':scope > .pz-home-dashboard');
    if(dashboard) return;

    dashboard = document.createElement('div');
    dashboard.className = 'pz-home-dashboard';
    dashboard.innerHTML = `
      <section class="pz-home-hero">
        <div>
          <div class="pz-home-kicker">PARADISE MARKET / SAISON 01</div>
          <div class="pz-home-title">Construis ta ville.<br>À ta manière.</div>
          <div class="pz-home-sub">Mobilier, construction, looks et compagnons dans une interface compacte pensée pour ParadiseRP.</div>
        </div>
        <button type="button" class="pz-home-cta" data-pz-go="furni">Explorer le marché <span>→</span></button>
      </section>
      <section class="pz-home-grid">
        <button type="button" class="pz-home-card" data-pz-go="furni"><span class="pz-home-icon">▰</span><strong>Mobilier</strong><small>Équipe ton appartement et tes lieux RP.</small></button>
        <button type="button" class="pz-home-card" data-pz-go="building"><span class="pz-home-icon">▦</span><strong>Construction</strong><small>Blocs, architecture et création de décors.</small></button>
        <button type="button" class="pz-home-card" data-pz-go="clothing"><span class="pz-home-icon">◆</span><strong>Style</strong><small>Looks et vêtements pour ton personnage.</small></button>
        <button type="button" class="pz-home-card" data-pz-go="pets"><span class="pz-home-icon">♣</span><strong>Compagnons</strong><small>Animaux et éléments de vie pour ta ville.</small></button>
      </section>`;
    dashboard.addEventListener('click', event => {
      const button = event.target.closest('[data-pz-go]');
      if(!button) return;
      event.preventDefault();
      clickTab(root, button.dataset.pzGo);
    });
    content.appendChild(dashboard);
  }

  function markCategoryRows(nav)
  {
    if(!nav) return;
    nav.querySelectorAll('button,a,[role="button"],.list-group-item,.nav-link').forEach(node => node.classList.add('pz-category-row'));
    [ ...nav.children ].filter(node => node instanceof HTMLElement && node.classList.contains('layout-grid-item')).forEach(node => node.classList.add('pz-category-row'));
  }

  function markProducts(root, nav)
  {
    const products = [ ...root.querySelectorAll('.layout-grid-item,[class*="catalog-grid-item"]') ].filter(item => !nav?.contains(item));
    products.forEach(item => {
      if(!(item instanceof HTMLElement)) return;
      item.classList.add('pz-product');
      const inline = item.style.backgroundImage;
      if(inline && inline !== 'none') item.style.setProperty('--pz-item-image', inline);
      const unique = item.querySelector('.unique-bg-override');
      if(unique instanceof HTMLElement)
      {
        const image = unique.style.backgroundImage;
        if(image && image !== 'none') unique.style.setProperty('--pz-item-image', image);
      }
    });
    return products;
  }

  function decorateStructure(root)
  {
    const content = root.querySelector(':scope > .nitro-card-content');
    if(!content) return;
    content.classList.add('pz-content');
    ensureHomeDashboard(root, content);

    const main = content.querySelector(':scope > .grid') || content.querySelector('.grid');
    if(main) main.classList.add('pz-main');

    const nav = content.querySelector('#nitro-catalog-main-navigation');
    const navWrap = content.querySelector('.nitro-catalog-navigation-grid-container') || nav?.parentElement;
    const side = nav?.closest('.g-col-3,.col-3') || navWrap?.parentElement;
    if(nav) nav.classList.add('pz-nav');
    if(navWrap) navWrap.classList.add('pz-nav-wrap');
    if(side) side.classList.add('pz-side');
    markCategoryRows(nav || navWrap);

    const searchInput = side?.querySelector('input[type="text"],input[type="search"]');
    const search = searchInput?.closest('.d-flex,.flex-row') || searchInput?.parentElement?.parentElement;
    if(search) search.classList.add('pz-search');
    if(searchInput) searchInput.placeholder = 'Rechercher...';
    search?.querySelectorAll('button').forEach(button => button.classList.add('pz-search-button'));

    let contentCol = null;
    if(main) contentCol = [ ...main.children ].find(node => node !== side && node instanceof HTMLElement) || null;
    if(contentCol) contentCol.classList.add('pz-content-col');

    const products = markProducts(root, nav || navWrap);
    const first = products[0] || null;
    const itemGrid = first?.parentElement || root.querySelector('.nitro-catalog-grid,[class*="catalog-grid"]');
    if(itemGrid) itemGrid.classList.add('pz-grid');

    let inner = null;
    if(contentCol && first)
    {
      inner = [ ...contentCol.querySelectorAll('.grid') ].find(grid => grid.contains(first)) || null;
      if(inner) inner.classList.add('pz-inner');
    }

    if(inner)
    {
      const gridCol = [ ...inner.children ].find(node => first && node.contains(first)) || null;
      const preview = [ ...inner.children ].find(node => node !== gridCol && node instanceof HTMLElement) || null;
      if(gridCol) gridCol.classList.add('pz-grid-col');
      if(preview) preview.classList.add('pz-preview');
    }

    const buyButtons = [ ...root.querySelectorAll('button') ].filter(button => /^(?:Acheter|Buy|Purchase)$/i.test(clean(button.textContent)));
    buyButtons.forEach(button => button.classList.add('pz-buy'));
    const purchase = root.querySelector('.nitro-catalog-purchase-component,[class*="catalog-purchase"],[class*="purchase-component"]') || buyButtons[0]?.closest('[class*="purchase"],.d-flex.flex-column');
    if(purchase) purchase.classList.add('pz-purchase');

    return { content, hasNav:!!nav, productCount:products.length };
  }

  function translateActions(root)
  {
    root.querySelectorAll('button,a,label,span').forEach(node => {
      if(node.closest('.pz-header') || node.closest('.pz-tab') || node.closest('.pz-home-dashboard') || node.children.length) return;
      const value = clean(node.textContent);
      if(/^Buy$/i.test(value) || /^Purchase$/i.test(value)) node.textContent = 'Acheter';
      else if(/^Gift$/i.test(value)) node.textContent = 'Offrir';
      else if(/^Search$/i.test(value)) node.textContent = 'Rechercher';
    });
  }

  function applyMode(root, tabs, structure)
  {
    const mode = currentMode(root, tabs);
    const home = mode === 'home';
    root.classList.toggle('pz-home-mode', home);
    root.classList.toggle('pz-store-mode', !home);
    if(!home && !structure?.hasNav && !structure?.productCount)
    {
      root.classList.remove('pz-store-mode');
    }
  }

  function decorate(root)
  {
    if(!(root instanceof HTMLElement)) return;
    root.classList.add('pz-catalog');
    root.dataset.pzBuild = BUILD;
    ensureHeader(root);
    const tabs = decorateTabs(root);
    const structure = decorateStructure(root);
    translateActions(root);
    applyMode(root, tabs, structure);
  }

  let queued = false;
  function refresh()
  {
    if(queued) return;
    queued = true;
    requestAnimationFrame(() => {
      queued = false;
      document.querySelectorAll(ROOT).forEach(decorate);
    });
  }

  function boot()
  {
    refresh();
    [100, 280, 650, 1200].forEach(delay => setTimeout(refresh, delay));
    new MutationObserver(refresh).observe(document.body, { childList:true, subtree:true, attributes:true, attributeFilter:['class','style'] });
    console.info('[ParadiseRP] catalogue ZERO V3 loaded');
  }

  document.readyState === 'loading' ? document.addEventListener('DOMContentLoaded', boot, { once:true }) : boot();
})();
