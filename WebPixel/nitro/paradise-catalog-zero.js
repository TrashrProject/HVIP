(() => {
  'use strict';

  const ROOT = '.nitro-catalog';
  const BUILD = 'paradise-catalog-zero-v4-city';
  const clean = value => String(value || '').replace(/\s+/g, ' ').trim();

  const TAB_DEFS = [
    { key:'home', pattern:/^(?:Front Page|Accueil)(?:\s*\(\d+\))?$/i, label:'Accueil' },
    { key:'furni', pattern:/^(?:Furni|Furniture|Mobilier|Mobilier officiel)(?:\s*\(\d+\))?$/i, label:'Mobilier officiel' },
    { key:'clothing', pattern:/^(?:Clothing|Vêtements)(?:\s*\(\d+\))?$/i, label:'Vêtements' },
    { key:'pets', pattern:/^(?:Pets|Animaux)(?:\s*\(\d+\))?$/i, label:'Animaux' },
    { key:'building', pattern:/^(?:Building|Construction)(?:\s*\(\d+\))?$/i, label:'Construction' },
    { key:'staff', pattern:/^Staff(?:\s*\(\d+\))?$/i, label:'Staff' },
    { key:'all', pattern:/^(?:Catalogue ParadiseRP complet.*|Tout|Tous)$/i, label:'Catalogue complet' }
  ];

  const getHeader = root => root.querySelector(':scope > .nitro-card-header, :scope > [class*="card-header"]');
  const getTabs = root => root.querySelector(':scope > .nitro-card-tabs, :scope > [class*="card-tabs"]');
  const getClickable = node => node?.matches?.('button,a,[role="tab"],[role="button"]') ? node : node?.querySelector?.('button,a,[role="tab"],[role="button"]');

  function nativeClose(root)
  {
    const header = getHeader(root);
    if(!header) return null;
    return [ ...header.querySelectorAll('button,[role="button"],.close,[class*="close"],[class*="cross"]') ]
      .find(node => !node.classList.contains('pz4-close')) || null;
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
      if(row && !row.closest('.pz4-header') && !rows.includes(row)) rows.push(row);
    });
    return rows.slice(0, 3);
  }

  function buildWallet(source)
  {
    const wallet = document.createElement('div');
    wallet.className = 'pz4-wallet';

    const icon = source.querySelector('.nitro-currency-icon,.currency-icon,[class*="currency-icon"],img,svg,i');
    const iconHost = document.createElement('span');
    iconHost.className = 'pz4-wallet-icon';
    if(icon) iconHost.appendChild(icon.cloneNode(true));

    const values = [ ...source.querySelectorAll('span,div,p') ].filter(node => !node.children.length && /\d/.test(clean(node.textContent)));
    const value = document.createElement('span');
    value.textContent = clean(values.at(-1)?.textContent || source.textContent) || '—';

    wallet.append(iconHost, value);
    return wallet;
  }

  function ensureHeader(root)
  {
    let header = root.querySelector(':scope > .pz4-header');
    if(!header)
    {
      header = document.createElement('div');
      header.className = 'pz4-header';
      header.innerHTML = `
        <span class="pz4-title-mark" aria-hidden="true">P</span>
        <div class="pz4-title">Catalogue de Paradise <small>RP Market</small></div>
        <div class="pz4-wallets" aria-label="Soldes"></div>
        <button type="button" class="pz4-close" aria-label="Fermer" title="Fermer">×</button>`;
      root.prepend(header);

      const close = header.querySelector('.pz4-close');
      close?.addEventListener('pointerdown', event => event.stopPropagation());
      close?.addEventListener('click', event => {
        event.preventDefault();
        event.stopPropagation();
        closeCatalog(root);
      });
    }

    const host = header.querySelector('.pz4-wallets');
    const sources = nativeCurrencies();
    const signature = sources.map(source => clean(source.textContent) + '|' + (source.querySelector('[class*="currency-icon"]')?.className || '')).join('||');
    if(host && host.dataset.signature !== signature)
    {
      host.replaceChildren(...sources.map(buildWallet));
      host.dataset.signature = signature;
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
    tabs.classList.add('pz4-tabs');

    const result = [];
    [ ...tabs.children ].filter(node => node instanceof HTMLElement).forEach(tab => {
      const value = clean(tab.textContent);
      const def = TAB_DEFS.find(item => item.pattern.test(value) || tab.dataset.pz4Tab === item.key);
      if(!def) return;

      tab.classList.add('pz4-tab');
      tab.dataset.pz4Tab = def.key;
      replaceFirstText(tab, def.label);

      const clickable = getClickable(tab);
      clickable?.setAttribute('aria-label', def.label);
      clickable?.setAttribute('title', def.label);
      result.push(tab);
    });

    return result;
  }

  function clickTab(root, key)
  {
    const tab = root.querySelector(`.pz4-tab[data-pz4-tab="${ key }"]`);
    getClickable(tab)?.click();
  }

  function isTabActive(tab)
  {
    return !!tab && (tab.matches('.active,[aria-selected="true"]') || !!tab.querySelector('.active,[aria-selected="true"]'));
  }

  function getMode(root, tabs)
  {
    const active = tabs.find(isTabActive);
    if(active) return active.dataset.pz4Tab || 'store';

    const hasNav = !!root.querySelector('#nitro-catalog-main-navigation,.nitro-catalog-navigation-grid-container');
    const hasProducts = !!root.querySelector('.layout-grid-item,[class*="catalog-grid-item"]');
    return (!hasNav && !hasProducts) ? 'home' : 'store';
  }

  function findNativeSearch(root)
  {
    const content = root.querySelector(':scope > .nitro-card-content');
    if(!content) return null;
    const nav = content.querySelector('#nitro-catalog-main-navigation');
    const navWrap = content.querySelector('.nitro-catalog-navigation-grid-container') || nav?.parentElement;
    const side = nav?.closest('.g-col-3,.col-3') || navWrap?.parentElement;
    return side?.querySelector('input[type="search"],input[type="text"]') || null;
  }

  function triggerNativeSearch(root, value, submit = false)
  {
    const input = findNativeSearch(root);
    if(!input) return;
    if(input.value !== value) input.value = value;
    input.dispatchEvent(new Event('input', { bubbles:true }));
    input.dispatchEvent(new Event('change', { bubbles:true }));

    if(submit)
    {
      input.dispatchEvent(new KeyboardEvent('keydown', { key:'Enter', code:'Enter', bubbles:true }));
      input.dispatchEvent(new KeyboardEvent('keyup', { key:'Enter', code:'Enter', bubbles:true }));
      const searchHost = input.closest('.d-flex,.flex-row') || input.parentElement?.parentElement;
      const button = searchHost?.querySelector('button');
      if(button) button.click();
    }
  }

  function ensureSearchShell(root)
  {
    let shell = root.querySelector(':scope > .pz4-search-shell');
    if(shell) return;

    shell = document.createElement('div');
    shell.className = 'pz4-search-shell';
    shell.innerHTML = `
      <div class="pz4-search-wrap">
        <span class="pz4-search-icon" aria-hidden="true">⌕</span>
        <input class="pz4-search-input" type="text" autocomplete="off" placeholder="Rechercher un mobi, une catégorie...">
      </div>
      <button type="button" class="pz4-search-go" aria-label="Rechercher">⌕</button>`;

    const input = shell.querySelector('.pz4-search-input');
    const go = shell.querySelector('.pz4-search-go');

    input.addEventListener('input', () => triggerNativeSearch(root, input.value, false));
    input.addEventListener('keydown', event => {
      if(event.key !== 'Enter') return;
      event.preventDefault();
      triggerNativeSearch(root, input.value, true);
    });
    go.addEventListener('click', () => triggerNativeSearch(root, input.value, true));

    root.appendChild(shell);
  }

  function ensureHome(root, content)
  {
    if(!content) return;
    let home = content.querySelector(':scope > .pz4-home');
    if(home) return;

    home = document.createElement('div');
    home.className = 'pz4-home';
    home.innerHTML = `
      <aside class="pz4-home-menu">
        <button class="pz4-home-link is-active" type="button" data-pz4-home="home"><span class="pz4-menu-icon">●</span>INFORMATIONS</button>
        <button class="pz4-home-link" type="button" data-pz4-home="furni"><span class="pz4-menu-icon">✦</span>NOUVEAUTÉS</button>
        <button class="pz4-home-link" type="button" data-pz4-home="furni"><span class="pz4-menu-icon">▣</span>MOBILIER</button>
        <button class="pz4-home-link" type="button" data-pz4-home="building"><span class="pz4-menu-icon">⌂</span>CONSTRUCTION</button>
        <button class="pz4-home-link" type="button" data-pz4-home="clothing"><span class="pz4-menu-icon">◆</span>VÊTEMENTS</button>
        <button class="pz4-home-link" type="button" data-pz4-home="pets"><span class="pz4-menu-icon">♣</span>ANIMAUX</button>
        <button class="pz4-home-link" type="button" data-pz4-home="staff"><span class="pz4-menu-icon">♛</span>STAFF</button>
        <button class="pz4-home-link" type="button" data-pz4-home="all"><span class="pz4-menu-icon">▤</span>CATALOGUE COMPLET</button>
      </aside>
      <section class="pz4-info-card">
        <div class="pz4-info-head">
          <div class="pz4-info-badge" aria-hidden="true">▦</div>
          <div class="pz4-info-title">Deux, trois trucs à savoir</div>
        </div>
        <div class="pz4-info-text">
          <p>- Pour décorer ton appartement ou tes lieux RP, choisis le mobilier souhaité puis achète-le. Il sera ensuite disponible dans ton inventaire.</p>
          <p>- Utilise la barre de recherche en haut pour retrouver rapidement un mobi ou une famille de mobilier.</p>
          <p>- Les catégories à gauche permettent de parcourir les collections, les blocs, les lieux publics, les éléments RP et les catalogues spéciaux.</p>
          <p>- Les animaux, vêtements et contenus spéciaux disposent de leurs propres sections en haut de la fenêtre.</p>
        </div>
        <div class="pz4-info-support">En cas de question, n'hésite pas à contacter le support ParadiseRP.</div>
      </section>`;

    home.addEventListener('click', event => {
      const button = event.target.closest('[data-pz4-home]');
      if(!button) return;
      const key = button.dataset.pz4Home;
      if(key === 'home') return;
      event.preventDefault();
      clickTab(root, key);
    });

    content.appendChild(home);
  }

  function markCategoryRows(nav)
  {
    if(!nav) return;
    nav.querySelectorAll('button,a,[role="button"],.list-group-item,.nav-link').forEach(node => node.classList.add('pz4-category-row'));
    [ ...nav.children ].filter(node => node instanceof HTMLElement && node.classList.contains('layout-grid-item')).forEach(node => node.classList.add('pz4-category-row'));
  }

  function markProducts(root, nav)
  {
    const products = [ ...root.querySelectorAll('.layout-grid-item,[class*="catalog-grid-item"]') ].filter(item => !nav?.contains(item));
    products.forEach(item => {
      if(!(item instanceof HTMLElement)) return;
      item.classList.add('pz4-product');
      const inline = item.style.backgroundImage;
      if(inline && inline !== 'none') item.style.setProperty('--pz4-item-image', inline);

      const unique = item.querySelector('.unique-bg-override');
      if(unique instanceof HTMLElement)
      {
        const image = unique.style.backgroundImage;
        if(image && image !== 'none') unique.style.setProperty('--pz4-item-image', image);
      }
    });
    return products;
  }

  function decorateStructure(root)
  {
    const content = root.querySelector(':scope > .nitro-card-content');
    if(!content) return { content:null, hasNav:false, productCount:0 };

    content.classList.add('pz4-content');
    ensureHome(root, content);

    const main = content.querySelector(':scope > .grid') || content.querySelector('.grid');
    if(main) main.classList.add('pz4-main');

    const nav = content.querySelector('#nitro-catalog-main-navigation');
    const navWrap = content.querySelector('.nitro-catalog-navigation-grid-container') || nav?.parentElement;
    const side = nav?.closest('.g-col-3,.col-3') || navWrap?.parentElement;

    if(nav) nav.classList.add('pz4-nav');
    if(navWrap) navWrap.classList.add('pz4-nav-wrap');
    if(side) side.classList.add('pz4-side');
    markCategoryRows(nav || navWrap);

    const nativeSearch = side?.querySelector('input[type="text"],input[type="search"]');
    const nativeSearchHost = nativeSearch?.closest('.d-flex,.flex-row') || nativeSearch?.parentElement?.parentElement;
    if(nativeSearchHost) nativeSearchHost.classList.add('pz4-native-search');

    let contentCol = null;
    if(main) contentCol = [ ...main.children ].find(node => node !== side && node instanceof HTMLElement) || null;
    if(contentCol) contentCol.classList.add('pz4-content-col');

    const products = markProducts(root, nav || navWrap);
    const first = products[0] || null;
    const itemGrid = first?.parentElement || root.querySelector('.nitro-catalog-grid,[class*="catalog-grid"]');
    if(itemGrid) itemGrid.classList.add('pz4-grid');

    let inner = null;
    if(contentCol && first)
    {
      inner = [ ...contentCol.querySelectorAll('.grid') ].find(grid => grid.contains(first)) || null;
      if(inner) inner.classList.add('pz4-inner');
    }

    if(inner)
    {
      const gridCol = [ ...inner.children ].find(node => first && node.contains(first)) || null;
      const preview = [ ...inner.children ].find(node => node !== gridCol && node instanceof HTMLElement) || null;
      if(gridCol) gridCol.classList.add('pz4-grid-col');
      if(preview) preview.classList.add('pz4-preview');
    }

    const buyButtons = [ ...root.querySelectorAll('button') ].filter(button => /^(?:Acheter|Buy|Purchase)$/i.test(clean(button.textContent)));
    buyButtons.forEach(button => button.classList.add('pz4-buy'));
    const purchase = root.querySelector('.nitro-catalog-purchase-component,[class*="catalog-purchase"],[class*="purchase-component"]') || buyButtons[0]?.closest('[class*="purchase"],.d-flex.flex-column');
    if(purchase) purchase.classList.add('pz4-purchase');

    return { content, hasNav:!!nav, productCount:products.length };
  }

  function translateActions(root)
  {
    root.querySelectorAll('button,a,label,span').forEach(node => {
      if(node.closest('.pz4-header') || node.closest('.pz4-tabs') || node.closest('.pz4-home') || node.closest('.pz4-search-shell') || node.children.length) return;
      const value = clean(node.textContent);
      if(/^Buy$/i.test(value) || /^Purchase$/i.test(value)) node.textContent = 'Acheter';
      else if(/^Gift$/i.test(value)) node.textContent = 'Offrir';
      else if(/^Search$/i.test(value)) node.textContent = 'Rechercher';
    });
  }

  function applyMode(root, tabs, structure)
  {
    const mode = getMode(root, tabs);
    const home = mode === 'home';
    root.classList.toggle('pz4-home-mode', home);
    root.classList.toggle('pz4-store-mode', !home && (!!structure?.hasNav || !!structure?.productCount));
  }

  function decorate(root)
  {
    if(!(root instanceof HTMLElement)) return;
    root.classList.add('pz-catalog');
    root.dataset.pzBuild = BUILD;
    ensureHeader(root);
    ensureSearchShell(root);
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
    [100, 260, 600, 1100].forEach(delay => setTimeout(refresh, delay));
    new MutationObserver(refresh).observe(document.body, { childList:true, subtree:true, attributes:true, attributeFilter:['class','style'] });
    console.info('[ParadiseRP] catalogue ZERO V4 City-style loaded');
  }

  document.readyState === 'loading' ? document.addEventListener('DOMContentLoaded', boot, { once:true }) : boot();
})();
