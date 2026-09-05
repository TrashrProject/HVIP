(() => {
  'use strict';

  const ROOT = '.nitro-catalog';
  const clean = value => String(value || '').replace(/\s+/g, ' ').trim();
  const getHeader = root => root.querySelector(':scope > .nitro-card-header, :scope > [class*="card-header"]');
  const getTabs = root => root.querySelector(':scope > .nitro-card-tabs, :scope > [class*="card-tabs"]');
  const getContent = root => root.querySelector(':scope > .nitro-card-content');
  const getClickable = node => node?.matches?.('button,a,[role="button"],[role="tab"]') ? node : node?.querySelector?.('button,a,[role="button"],[role="tab"]');

  const SEGMENTS = [
    { label:'MOBIS OFFICIEL', match:/^(?:Furni|Furniture|Mobilier)(?:\s*\(\d+\))?$/i },
    { label:'MOBIS CUSTOM', match:/^Catalogue ParadiseRP complet.*$/i },
    { label:'MOBIS CITY', match:/^(?:Building|Construction)(?:\s*\(\d+\))?$/i },
    { label:'UTILITAIRES', match:/^Staff(?:\s*\(\d+\))?$/i },
    { label:'RARES', match:/^Front Page(?:\s*\(\d+\))?$/i }
  ];

  function nativeClose(root)
  {
    const header = getHeader(root);
    if(!header) return null;
    return [ ...header.querySelectorAll('button,[role="button"],.close,[class*="close"],[class*="cross"]') ]
      .find(node => !node.classList.contains('pcity-close')) || null;
  }

  function closeCatalog(root)
  {
    const close = nativeClose(root);
    if(close) { close.click(); return; }
    document.dispatchEvent(new KeyboardEvent('keydown', { key:'Escape', code:'Escape', bubbles:true }));
  }

  function nativeTab(root, pattern)
  {
    const tabs = getTabs(root);
    if(!tabs) return null;
    return [ ...tabs.children ].find(tab => pattern.test(clean(tab.textContent))) || null;
  }

  function clickNativeTab(root, pattern)
  {
    const tab = nativeTab(root, pattern);
    getClickable(tab)?.click();
  }

  function isNativeTabActive(tab)
  {
    return !!tab && (tab.matches('.active,[aria-selected="true"]') || !!tab.querySelector('.active,[aria-selected="true"]'));
  }

  function ensureHeader(root)
  {
    let header = root.querySelector(':scope > .pcity-header');
    if(!header)
    {
      header = document.createElement('div');
      header.className = 'pcity-header';
      header.innerHTML = '<div class="pcity-title">Catalogue de Paradise</div><button type="button" class="pcity-close" aria-label="Fermer">×</button>';
      root.prepend(header);
      header.querySelector('.pcity-close')?.addEventListener('pointerdown', event => event.stopPropagation());
      header.querySelector('.pcity-close')?.addEventListener('click', event => {
        event.preventDefault();
        event.stopPropagation();
        closeCatalog(root);
      });
    }
  }

  function findNativeSearch(root)
  {
    const content = getContent(root);
    if(!content) return null;
    const input = content.querySelector('input[type="search"],input[type="text"]');
    if(!input) return null;
    const row = input.closest('.d-flex,.flex-row') || input.parentElement?.parentElement || input.parentElement;
    const button = row?.querySelector('button');
    return { input, button, row };
  }

  function setNativeInputValue(input, value)
  {
    if(!input) return;
    const setter = Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, 'value')?.set;
    setter?.call(input, value);
    input.dispatchEvent(new Event('input', { bubbles:true }));
    input.dispatchEvent(new Event('change', { bubbles:true }));
  }

  function ensureSearch(root)
  {
    let bar = root.querySelector(':scope > .pcity-searchbar');
    if(!bar)
    {
      bar = document.createElement('div');
      bar.className = 'pcity-searchbar';
      bar.innerHTML = '<div class="pcity-search-icon">⌕</div><input class="pcity-search-input" type="search" placeholder="Rechercher un mobi, une catégorie..."><button class="pcity-search-go" type="button" aria-label="Rechercher">⌕</button>';
      root.appendChild(bar);

      const proxy = bar.querySelector('.pcity-search-input');
      const runSearch = () => {
        const native = findNativeSearch(root);
        if(!native) return;
        setNativeInputValue(native.input, proxy.value);
        if(native.button) native.button.click();
      };
      proxy?.addEventListener('input', () => {
        const native = findNativeSearch(root);
        if(native) setNativeInputValue(native.input, proxy.value);
      });
      proxy?.addEventListener('keydown', event => { if(event.key === 'Enter') { event.preventDefault(); runSearch(); } });
      bar.querySelector('.pcity-search-go')?.addEventListener('click', runSearch);
    }

    const native = findNativeSearch(root);
    if(native?.row) native.row.classList.add('pcity-native-search');
  }

  function ensureSegments(root)
  {
    let host = root.querySelector(':scope > .pcity-segments');
    if(!host)
    {
      host = document.createElement('div');
      host.className = 'pcity-segments';
      SEGMENTS.forEach(def => {
        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'pcity-segment';
        button.textContent = def.label;
        button.dataset.pcityLabel = def.label;
        button.addEventListener('click', () => {
          root.classList.remove('pcity-info-mode');
          clickNativeTab(root, def.match);
        });
        host.appendChild(button);
      });
      root.appendChild(host);
    }

    SEGMENTS.forEach(def => {
      const button = [ ...host.children ].find(node => node.dataset.pcityLabel === def.label);
      const tab = nativeTab(root, def.match);
      button?.classList.toggle('is-active', isNativeTabActive(tab));
    });
  }

  function ensureInfoPanel(root, content)
  {
    if(!content || content.querySelector(':scope > .pcity-info-panel')) return;
    const panel = document.createElement('div');
    panel.className = 'pcity-info-panel';
    panel.innerHTML = `
      <div class="pcity-info-left">
        <button type="button" class="pcity-info-button is-active">◉ Informations</button>
        <button type="button" class="pcity-info-button">◌ Historique</button>
        <button type="button" class="pcity-info-button">Nouveautés</button>
        <button type="button" class="pcity-info-button">Gammes</button>
        <button type="button" class="pcity-info-button">Thèmes</button>
        <button type="button" class="pcity-info-button">Types</button>
        <button type="button" class="pcity-info-button">Saisons</button>
        <button type="button" class="pcity-info-button">Événements</button>
        <button type="button" class="pcity-info-button">Construction</button>
        <button type="button" class="pcity-info-button">Lieux publics</button>
        <button type="button" class="pcity-info-button">Jeux</button>
      </div>
      <div class="pcity-info-card">
        <div class="pcity-info-heading"><div class="pcity-info-badge">▦</div><div class="pcity-info-title">Deux, trois trucs à savoir</div></div>
        <div class="pcity-info-copy">
          <p>- Pour décorer ton appart, il suffit d'acheter le mobilier que tu souhaites et de le récupérer dans ton inventaire pour ensuite pouvoir le poser dans ton appartement.</p>
          <p>- Recherche un mobilier en fonction de son nom depuis la barre de recherche, exemple : chaise.</p>
          <p>- Les mobiliers peuvent ensuite être posés avec les commandes de construction disponibles sur ParadiseRP.</p>
          <p>- Les animaux ainsi que les bots apparaissent eux aussi dans l'inventaire, dans leurs catégories dédiées.</p>
        </div>
        <div class="pcity-info-footer">En cas de question, contacte le support ParadiseRP.</div>
      </div>`;
    content.appendChild(panel);
  }

  function decorateCategories(nav)
  {
    if(!nav) return;
    nav.querySelectorAll('button,a,[role="button"],.list-group-item,.nav-link').forEach(node => node.classList.add('pcity-category'));
    [ ...nav.children ].filter(node => node instanceof HTMLElement && node.classList.contains('layout-grid-item')).forEach(node => node.classList.add('pcity-category'));
  }

  function decorateProducts(root, nav)
  {
    const products = [ ...root.querySelectorAll('.layout-grid-item,[class*="catalog-grid-item"]') ].filter(item => !nav?.contains(item));
    products.forEach(item => {
      if(!(item instanceof HTMLElement)) return;
      item.classList.add('pcity-product');
      const inline = item.style.backgroundImage;
      if(inline && inline !== 'none') item.style.setProperty('--pcity-item-image', inline);
      const unique = item.querySelector('.unique-bg-override');
      if(unique instanceof HTMLElement)
      {
        const image = unique.style.backgroundImage;
        if(image && image !== 'none') unique.style.setProperty('--pcity-item-image', image);
      }
    });
    return products;
  }

  function decorateStructure(root)
  {
    const content = getContent(root);
    if(!content) return;
    content.classList.add('pcity-content');
    ensureInfoPanel(root, content);

    const main = content.querySelector(':scope > .grid') || content.querySelector('.grid');
    if(main) main.classList.add('pcity-main');

    const nav = content.querySelector('#nitro-catalog-main-navigation');
    const navWrap = content.querySelector('.nitro-catalog-navigation-grid-container') || nav?.parentElement;
    const side = nav?.closest('.g-col-3,.col-3') || navWrap?.parentElement;
    if(side) side.classList.add('pcity-side');
    if(navWrap) navWrap.classList.add('pcity-nav-wrap');
    if(nav) nav.classList.add('pcity-nav');
    decorateCategories(nav || navWrap);

    let right = null;
    if(main) right = [ ...main.children ].find(node => node !== side && node instanceof HTMLElement) || null;
    if(right) right.classList.add('pcity-right');

    const products = decorateProducts(root, nav || navWrap);
    const first = products[0] || null;
    const grid = first?.parentElement || root.querySelector('.nitro-catalog-grid,[class*="catalog-grid"]');
    if(grid) grid.classList.add('pcity-grid');

    let inner = null;
    if(right && first)
    {
      inner = [ ...right.querySelectorAll('.grid') ].find(node => node.contains(first)) || null;
      if(inner) inner.classList.add('pcity-inner');
    }
    if(inner)
    {
      const gridCol = [ ...inner.children ].find(node => first && node.contains(first)) || null;
      const preview = [ ...inner.children ].find(node => node !== gridCol && node instanceof HTMLElement) || null;
      if(gridCol) gridCol.classList.add('pcity-grid-col');
      if(preview) preview.classList.add('pcity-preview');
    }

    [ ...root.querySelectorAll('button') ].filter(button => /^(?:Acheter|Buy|Purchase)$/i.test(clean(button.textContent))).forEach(button => button.classList.add('pcity-buy'));

    const native = findNativeSearch(root);
    if(native?.row) native.row.classList.add('pcity-native-search');

    if(side && !side.querySelector('.pcity-info-entry'))
    {
      const entry = document.createElement('button');
      entry.type = 'button';
      entry.className = 'pcity-info-button pcity-info-entry';
      entry.textContent = 'Informations';
      entry.addEventListener('click', () => root.classList.add('pcity-info-mode'));
      side.prepend(entry);
    }
  }

  function ensureStoreLoaded(root)
  {
    const content = getContent(root);
    if(!content || root.dataset.pcityStoreBooted === '1') return;
    const nav = content.querySelector('#nitro-catalog-main-navigation,.nitro-catalog-navigation-grid-container');
    const product = content.querySelector('.layout-grid-item,[class*="catalog-grid-item"]');
    if(nav || product) { root.dataset.pcityStoreBooted = '1'; return; }

    const furni = nativeTab(root, /^(?:Furni|Furniture|Mobilier)(?:\s*\(\d+\))?$/i);
    if(furni)
    {
      root.dataset.pcityStoreBooted = '1';
      setTimeout(() => getClickable(furni)?.click(), 80);
      setTimeout(() => root.classList.add('pcity-info-mode'), 220);
    }
  }

  function decorate(root)
  {
    if(!(root instanceof HTMLElement)) return;
    root.classList.add('pcity-catalog');
    ensureHeader(root);
    ensureSearch(root);
    ensureSegments(root);
    decorateStructure(root);
    ensureStoreLoaded(root);
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
    [120, 350, 700, 1400].forEach(delay => setTimeout(refresh, delay));
    new MutationObserver(refresh).observe(document.body, { childList:true, subtree:true, attributes:true, attributeFilter:['class','style'] });
    console.info('[ParadiseRP] City reference catalogue loaded');
  }

  document.readyState === 'loading' ? document.addEventListener('DOMContentLoaded', boot, { once:true }) : boot();
})();
