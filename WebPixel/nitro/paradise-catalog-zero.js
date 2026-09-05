(() => {
  'use strict';

  const ROOT_SELECTOR = '.nitro-catalog';
  const BUILD = 'paradise-catalog-real-data-v6';
  const clean = value => String(value || '').replace(/\s+/g, ' ').trim();
  const children = node => node ? [ ...node.children ].filter(child => child instanceof HTMLElement) : [];

  // Presentation-only translations. The node, page id, handler and server data stay untouched.
  const LABELS = new Map([
    [ 'front page', 'Accueil' ], [ 'information', 'Informations' ], [ 'informations', 'Informations' ],
    [ 'furni', 'Mobilier' ], [ 'furniture', 'Mobilier' ], [ 'clothing', 'Vêtements' ],
    [ 'pets', 'Animaux' ], [ 'building', 'Construction' ], [ 'hairdos', 'Cheveux' ],
    [ 'hats', 'Chapeaux' ], [ 'accessories', 'Accessoires' ], [ 'dresses', 'Robes' ],
    [ 'shirts', 'Hauts' ], [ 'jackets', 'Vestes' ], [ 'trousers', 'Pantalons' ],
    [ 'skirts', 'Jupes' ], [ 'shoes', 'Chaussures' ], [ 'effects', 'Effets' ],
    [ 'buy', 'Acheter' ], [ 'purchase', 'Acheter' ], [ 'gift', 'Offrir' ]
  ]);

  const getHeader = root => root.querySelector(':scope > .nitro-card-header, :scope > [class*="card-header"]');
  const getTabs = root => root.querySelector(':scope > .nitro-card-tabs, :scope > [class*="card-tabs"]');
  const getContent = root => root.querySelector(':scope > .nitro-card-content, :scope > [class*="card-content"]');

  function directChildContaining(boundary, descendant) {
    if(!boundary || !descendant || !boundary.contains(descendant)) return null;
    let node = descendant;
    while(node.parentElement && node.parentElement !== boundary) node = node.parentElement;
    return node.parentElement === boundary ? node : null;
  }

  function textLeaf(node) {
    if(!node) return null;
    const walker = document.createTreeWalker(node, NodeFilter.SHOW_TEXT);
    let text = walker.nextNode();
    while(text) {
      if(clean(text.nodeValue) && !text.parentElement?.closest('button,[role="button"]')) return text;
      text = walker.nextNode();
    }
    return null;
  }

  function translateLabel(node) {
    if(!node) return;
    const walker = document.createTreeWalker(node, NodeFilter.SHOW_TEXT);
    let text = walker.nextNode();
    while(text) {
      const raw = clean(text.nodeValue);
      const match = raw.match(/^(.+?)(\s*\(\d+\))?$/);
      const translated = match && LABELS.get(match[1].toLocaleLowerCase('fr'));
      if(match && (translated || match[2])) {
        // The emulator appends the page id for accounts with ACC_CATALOG_IDS.
        // It is useful for diagnostics but is not an item counter, so keep it out of the player-facing label.
        const next = translated || match[1];
        if(raw !== next) text.nodeValue = next;
        return;
      }
      text = walker.nextNode();
    }
  }

  function decorateHeader(root) {
    const header = getHeader(root);
    if(!header) return;
    header.classList.add('pc6-header');
    const title = textLeaf(header);
    if(title && clean(title.nodeValue) !== 'Catalogue de Paradise') title.nodeValue = 'Catalogue de Paradise';
    header.querySelectorAll('button,[role="button"],.close,[class*="close"]').forEach(button => {
      button.classList.add('pc6-close');
      button.setAttribute('aria-label', 'Fermer le catalogue');
      button.setAttribute('title', 'Fermer');
    });
  }

  function decorateTabs(root) {
    const tabs = getTabs(root);
    if(!tabs) return;
    tabs.classList.add('pc6-tabs');
    children(tabs).forEach(tab => {
      tab.classList.add('pc6-tab');
      translateLabel(tab);
      const label = clean(tab.textContent);
      if(label) tab.setAttribute('title', label);
    });
  }

  function decorateNavigation(navigation) {
    if(!navigation) return;
    navigation.classList.add('pc6-navigation');
    navigation.querySelectorAll('.nitro-catalog-navigation-section').forEach(section => {
      section.classList.add('pc6-navigation-section');
      const row = children(section)[0];
      if(row) {
        row.classList.add('pc6-category');
        translateLabel(row);
      }
    });
  }

  function findStoreRow(contentColumn, product) {
    if(!contentColumn) return null;
    let node = product;
    let candidate = null;
    while(node?.parentElement && node.parentElement !== contentColumn) {
      const parent = node.parentElement;
      const siblings = children(parent);
      if(siblings.length === 2 && siblings.some(child => child.contains(product)) && siblings.some(child => !child.contains(product))) {
        candidate = parent;
      }
      node = parent;
    }
    return candidate || children(contentColumn).find(child => child.matches('.row,.d-flex') && children(child).length === 2) || null;
  }

  function decorateStore(contentColumn, navigation) {
    if(!contentColumn) return;
    contentColumn.classList.add('pc6-content-column');

    const products = [ ...contentColumn.querySelectorAll('.layout-grid-item') ]
      .filter(item => !navigation?.contains(item));
    products.forEach(product => product.classList.add('pc6-product'));
    new Set(products.map(item => item.parentElement).filter(Boolean))
      .forEach(grid => grid.classList.add('pc6-product-grid'));

    const product = products[0] || null;
    const storeRow = findStoreRow(contentColumn, product);
    if(storeRow) {
      storeRow.classList.add('pc6-store-row');
      const columns = children(storeRow);
      const productColumn = product ? columns.find(column => column.contains(product)) : columns[0];
      const preview = columns.find(column => column !== productColumn);
      productColumn?.classList.add('pc6-products-column');
      preview?.classList.add('pc6-preview');
    }

    contentColumn.querySelectorAll('.nitro-catalog-header').forEach(header => header.classList.add('pc6-page-header'));
    contentColumn.querySelectorAll('.spinner-border,[class*="spinner"]').forEach(spinner => spinner.classList.add('pc6-loading'));
    contentColumn.querySelectorAll('button').forEach(button => {
      translateLabel(button);
      const label = clean(button.textContent);
      if(/^(Acheter|Confirmer|Rent|Louer)$/i.test(label)) button.classList.add('pc6-buy');
      if(/^Offrir$/i.test(label)) button.classList.add('pc6-gift');
    });

    // Localize only Nitro's known empty-state copy; no content or category is injected.
    const walker = document.createTreeWalker(contentColumn, NodeFilter.SHOW_TEXT);
    let text = walker.nextNode();
    while(text) {
      const value = clean(text.nodeValue);
      if(/^Need some inspiration\??$/i.test(value)) text.nodeValue = 'Aucun élément à afficher';
      else if(/^Look no further\.?$/i.test(value)) text.nodeValue = 'Sélectionnez une catégorie pour découvrir le catalogue.';
      text = walker.nextNode();
    }
  }

  function decorateContent(root) {
    const content = getContent(root);
    if(!content) return;
    content.classList.add('pc6-content');

    const navigation = content.querySelector('#nitro-catalog-main-navigation');
    const navigationBox = content.querySelector('.nitro-catalog-navigation-grid-container') || navigation?.parentElement;
    const main = directChildContaining(content, navigation);
    const side = directChildContaining(main, navigation);
    const contentColumn = main ? children(main).find(child => child !== side) : null;

    main?.classList.add('pc6-main');
    side?.classList.add('pc6-side');
    navigationBox?.classList.add('pc6-navigation-box');
    decorateNavigation(navigation);

    const input = side?.querySelector('input[type="search"],input[type="text"]');
    const search = directChildContaining(side, input);
    if(search && input) {
      search.classList.add('pc6-search');
      input.classList.add('pc6-search-input');
      input.placeholder = 'Rechercher un mobi, vêtement, catégorie...';
      input.setAttribute('aria-label', 'Rechercher dans le catalogue');
      children(search).find(child => child.contains(input))?.classList.add('pc6-search-field');
      children(search).filter(child => !child.contains(input)).forEach(action => action.classList.add('pc6-search-action'));
    }

    decorateStore(contentColumn, navigation || navigationBox);
  }

  function decorate(root) {
    if(!(root instanceof HTMLElement)) return;
    root.classList.add('pc6-catalog');
    root.dataset.paradiseCatalogBuild = BUILD;
    decorateHeader(root);
    decorateTabs(root);
    decorateContent(root);
  }

  let frame = 0;
  function refresh() {
    if(frame) return;
    frame = requestAnimationFrame(() => {
      frame = 0;
      document.querySelectorAll(ROOT_SELECTOR).forEach(decorate);
    });
  }

  function boot() {
    refresh();
    [ 100, 300, 700, 1400 ].forEach(delay => window.setTimeout(refresh, delay));
    document.addEventListener('click', event => {
      if(event.target.closest(ROOT_SELECTOR)) window.setTimeout(refresh, 0);
    }, true);
    new MutationObserver(refresh).observe(document.body, { childList: true, subtree: true });
    console.info('[ParadiseRP] catalogue V2 données réelles chargé');
  }

  document.readyState === 'loading'
    ? document.addEventListener('DOMContentLoaded', boot, { once: true })
    : boot();
})();
