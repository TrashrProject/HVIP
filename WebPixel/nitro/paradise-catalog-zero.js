(() => {
  'use strict';

  const ROOT_SELECTOR = '.nitro-catalog';
  const BUILD = 'paradise-catalog-city-v5';
  const clean = value => String(value || '').replace(/\s+/g, ' ').trim();
  const elementChildren = node => node ? [ ...node.children ].filter(child => child instanceof HTMLElement) : [];

  const TAB_RULES = [
    { key: 'home', match: /^(?:Front Page|Accueil|Informations?)(?:\s*\(\d+\))?$/i, label: 'MOBIS OFFICIEL' },
    { key: 'official', match: /^(?:Furni|Furniture|Mobilier|Mobis? officiels?)(?:\s*\(\d+\))?$/i, label: 'MOBIS OFFICIEL' },
    { key: 'custom', match: /^(?:Mobis? custom|Custom)(?:\s*\(\d+\))?$/i, label: 'MOBIS CUSTOM' },
    { key: 'city', match: /^(?:Mobis? city|City)(?:\s*\(\d+\))?$/i, label: 'MOBIS CITY' },
    { key: 'utilities', match: /^(?:Utilitaires?|Utilities)(?:\s*\(\d+\))?$/i, label: 'UTILITAIRES' },
    { key: 'rares', match: /^(?:Rares?|Prestige)(?:\s*\(\d+\))?$/i, label: 'RARES' }
  ];

  const MENU_ITEMS = [
    [ 'info', 'i', 'INFORMATIONS' ],
    [ 'history', '&#9677;', 'HISTORIQUE' ],
    [ 'news', 'N', 'NOUVEAUTÉS', true ],
    [ 'ranges', 'G', 'GAMMES', true ],
    [ 'themes', 'T', 'THÈMES', true ],
    [ 'types', 'D', 'TYPES', true ],
    [ 'seasons', 'S', 'SAISONS', true ],
    [ 'events', 'E', 'ÉVÉNEMENTS', true ],
    [ 'building', 'B', 'CONSTRUCTION', true ],
    [ 'public', 'P', 'LIEUX PUBLICS', true ],
    [ 'games', 'J', 'JEUX', true ]
  ];

  const MENU_TARGETS = {
    history: /historique/i,
    news: /nouveaut|new|recent/i,
    ranges: /gamme|collection/i,
    themes: /th[èe]me/i,
    types: /type/i,
    seasons: /saison|no[eë]l|hiver|summer|[ée]t[ée]/i,
    events: /[ée]v[ée]nement|f[êe]te|halloween/i,
    building: /construction|architecture|build/i,
    public: /lieux publics?|ville|services publics?/i,
    games: /jeux|games|sport/i
  };

  const getHeader = root => root.querySelector(':scope > .nitro-card-header, :scope > [class*="card-header"]');
  const getTabs = root => root.querySelector(':scope > .nitro-card-tabs, :scope > [class*="card-tabs"]');
  const getContent = root => root.querySelector(':scope > .nitro-card-content, :scope > [class*="card-content"]');
  const getClickable = node => node?.matches?.('button,a,[role="tab"],[role="button"]') ? node : node?.querySelector?.('button,a,[role="tab"],[role="button"]');

  function isActive(node)
  {
    return !!node && (node.matches('.active,.selected,[aria-selected="true"]') || !!node.querySelector('.active,.selected,[aria-selected="true"]'));
  }

  function nativeClose(root)
  {
    const header = getHeader(root);
    if(!header) return null;
    return [ ...header.querySelectorAll('button,[role="button"],.close,[class*="close"],[class*="cross"]') ]
      .find(node => !node.classList.contains('pc5-close')) || null;
  }

  function closeCatalog(root)
  {
    const close = nativeClose(root);
    if(close) close.click();
    else document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', code: 'Escape', bubbles: true }));
  }

  function ensureChrome(root)
  {
    if(!root.querySelector(':scope > .pc5-header'))
    {
      const header = document.createElement('div');
      header.className = 'pc5-header';
      header.innerHTML = `
        <strong class="pc5-title">Catalogue de Paradise</strong>
        <button class="pc5-close" type="button" aria-label="Fermer le catalogue" title="Fermer">&times;</button>`;
      header.querySelector('.pc5-close')?.addEventListener('pointerdown', event => event.stopPropagation());
      header.querySelector('.pc5-close')?.addEventListener('click', event => {
        event.preventDefault();
        event.stopPropagation();
        closeCatalog(root);
      });
      root.prepend(header);
    }

    if(!root.querySelector(':scope > .pc5-search'))
    {
      const search = document.createElement('form');
      search.className = 'pc5-search';
      search.setAttribute('role', 'search');
      search.innerHTML = `
        <span class="pc5-search-icon" aria-hidden="true">
          <svg viewBox="0 0 20 20"><circle cx="8" cy="8" r="5"></circle><path d="m12 12 5 5"></path></svg>
        </span>
        <input class="pc5-search-input" type="search" autocomplete="off" placeholder="Rechercher un mobi, une catégorie..." aria-label="Rechercher dans le catalogue">
        <button class="pc5-search-submit" type="submit" aria-label="Lancer la recherche" title="Rechercher">
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M3 4h2l2.2 10.2a2 2 0 0 0 2 1.6h8.5a2 2 0 0 0 1.9-1.4L22 7H7"></path><circle cx="10" cy="20" r="1.5"></circle><circle cx="18" cy="20" r="1.5"></circle></svg>
        </button>`;
      const input = search.querySelector('.pc5-search-input');
      input?.addEventListener('input', () => runNativeSearch(root, input.value, false));
      search.addEventListener('submit', event => {
        event.preventDefault();
        runNativeSearch(root, input?.value || '', true);
      });
      root.appendChild(search);
    }
  }

  function replaceLabel(node, label)
  {
    const walker = document.createTreeWalker(node, NodeFilter.SHOW_TEXT);
    let text = walker.nextNode();
    while(text)
    {
      const value = clean(text.nodeValue);
      if(value && /[A-Za-zÀ-ÿ]/.test(value))
      {
        if(value !== label) text.nodeValue = label;
        return;
      }
      text = walker.nextNode();
    }
  }

  function decorateTabs(root)
  {
    const rail = getTabs(root);
    if(!rail) return [];
    rail.classList.add('pc5-tabs');

    const tabs = elementChildren(rail);
    tabs.forEach((tab, index) => {
      const value = clean(tab.textContent);
      const known = TAB_RULES.find(rule => rule.match.test(value) || tab.dataset.pc5Key === rule.key);
      tab.classList.add('pc5-tab');
      tab.dataset.pc5Key = known?.key || `section-${ index }`;
      if(known)
      {
        replaceLabel(tab, known.label);
        const clickable = getClickable(tab) || tab;
        clickable.setAttribute('aria-label', known.label);
        clickable.setAttribute('title', known.label);
      }
    });
    return tabs;
  }

  function nativeSearchInput(root)
  {
    const navigation = root.querySelector('#nitro-catalog-main-navigation');
    const content = getContent(root);
    const main = directChildContaining(content, navigation);
    const side = directChildContaining(main, navigation);
    return side?.querySelector('input[type="search"],input[type="text"]') || null;
  }

  function runNativeSearch(root, value, submit, retry = true)
  {
    const input = nativeSearchInput(root);
    if(!input)
    {
      if(submit && retry)
      {
        const tabs = elementChildren(getTabs(root));
        const official = tabs.find(tab => tab.dataset.pc5Key === 'official') || tabs.find(tab => tab.dataset.pc5Key !== 'home');
        (getClickable(official) || official)?.click();
        window.setTimeout(() => runNativeSearch(root, value, true, false), 180);
      }
      return;
    }
    const setter = Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, 'value')?.set;
    if(setter) setter.call(input, value);
    else input.value = value;
    input.dispatchEvent(new Event('input', { bubbles: true }));
    input.dispatchEvent(new Event('change', { bubbles: true }));
    if(!submit) return;
    input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', code: 'Enter', bubbles: true }));
    input.dispatchEvent(new KeyboardEvent('keyup', { key: 'Enter', code: 'Enter', bubbles: true }));
    const host = input.closest('form,.d-flex,.flex-row') || input.parentElement?.parentElement;
    host?.querySelector('button')?.click();
  }

  function homeMarkup()
  {
    const rows = MENU_ITEMS.map(([ key, icon, label, branch ], index) => `
      <button class="pc5-home-row${ index === 0 ? ' is-active' : '' }" type="button" data-pc5-menu="${ key }">
        <span class="pc5-menu-icon" aria-hidden="true">${ icon }</span>
        <span>${ label }</span>
        ${ branch ? '<span class="pc5-menu-arrow" aria-hidden="true">&#9662;</span>' : '' }
      </button>`).join('');

    return `
      <div class="pc5-home">
        <aside class="pc5-home-menu" aria-label="Rubriques du catalogue">${ rows }</aside>
        <section class="pc5-info">
          <header class="pc5-info-header">
            <span class="pc5-info-badge" aria-hidden="true"><span>RP</span></span>
            <h2>Deux, trois trucs à savoir</h2>
          </header>
          <div class="pc5-info-copy">
            <p>- Pour décorer ton appart ou construire tes lieux RP, achète le mobilier souhaité puis récupère-le dans ton inventaire.</p>
            <p>- Recherche un mobilier par son nom grâce à la barre située en haut du catalogue, par exemple : bloc noir.</p>
            <p>- Les mobiliers peuvent ensuite être posés avec les commandes de construction prévues sur ParadiseRP.</p>
            <p>- Les animaux, vêtements et objets RP disposent de leurs propres catégories quand ils sont disponibles.</p>
          </div>
          <strong class="pc5-support">En cas de question, n'hésite pas à contacter le support.</strong>
        </section>
      </div>`;
  }

  function ensureHome(content)
  {
    if(content.querySelector(':scope > .pc5-home')) return;
    content.insertAdjacentHTML('beforeend', homeMarkup());
    const home = content.querySelector(':scope > .pc5-home');
    home?.addEventListener('click', event => {
      const button = event.target.closest('[data-pc5-menu]');
      const root = button?.closest(ROOT_SELECTOR);
      if(!button || !root || button.dataset.pc5Menu === 'info') return;
      event.preventDefault();

      const tabs = elementChildren(getTabs(root));
      const official = tabs.find(tab => tab.dataset.pc5Key === 'official') || tabs.find(tab => tab.dataset.pc5Key !== 'home');
      (getClickable(official) || official)?.click();

      const target = MENU_TARGETS[button.dataset.pc5Menu];
      if(!target) return;
      [ 100, 250 ].forEach(delay => window.setTimeout(() => {
        const navigation = root.querySelector('#nitro-catalog-main-navigation');
        const category = [ ...(navigation?.querySelectorAll('.pc5-category,button,a,[role="button"],.layout-grid-item') || []) ]
          .find(node => target.test(clean(node.textContent)));
        (getClickable(category) || category)?.click();
      }, delay));
    });
  }

  function directChildContaining(boundary, descendant)
  {
    if(!descendant || !boundary) return null;
    let node = descendant;
    while(node?.parentElement && node.parentElement !== boundary) node = node.parentElement;
    return node?.parentElement === boundary ? node : null;
  }

  function findRowAround(descendant, boundary)
  {
    if(!descendant || !boundary) return null;
    let node = descendant;
    let candidate = null;
    while(node?.parentElement && node.parentElement !== boundary)
    {
      const parent = node.parentElement;
      const children = elementChildren(parent);
      if(children.length > 1 && children.some(child => child.contains(descendant)) && children.some(child => !child.contains(descendant))) candidate = parent;
      node = parent;
    }
    return candidate;
  }

  function markCategoryRows(navigation)
  {
    if(!navigation) return;
    const candidates = [ ...navigation.querySelectorAll('.nitro-catalog-navigation-section,button,a,[role="button"],.layout-grid-item') ];
    candidates.forEach(node => {
      if(!(node instanceof HTMLElement)) return;
      const nestedClickable = node.querySelector('button,a,[role="button"]');
      if(node.matches('.nitro-catalog-navigation-section'))
      {
        if(nestedClickable) nestedClickable.classList.add('pc5-category');
        return;
      }
      if(!node.parentElement?.closest('button,a,[role="button"]')) node.classList.add('pc5-category');
    });
  }

  function markProducts(root, navigation)
  {
    const candidates = [ ...root.querySelectorAll('.layout-grid-item,[class*="catalog-grid-item"]') ];
    const products = candidates.filter(item => item instanceof HTMLElement && !navigation?.contains(item) && !item.closest('.pc5-tabs,.pc5-home'));
    products.forEach(item => {
      item.classList.add('pc5-product');
      if(item.style.backgroundImage && item.style.backgroundImage !== 'none') item.style.setProperty('--pc5-product-image', item.style.backgroundImage);
      item.querySelectorAll('.unique-bg-override,[style*="background-image"]').forEach(image => {
        if(image instanceof HTMLElement && image.style.backgroundImage && image.style.backgroundImage !== 'none') image.style.setProperty('--pc5-product-image', image.style.backgroundImage);
      });
    });
    return products;
  }

  function decorateContent(root)
  {
    const content = getContent(root);
    if(!content) return { hasStore: false, hasNavigation: false };
    content.classList.add('pc5-content');
    ensureHome(content);

    const navigation = content.querySelector('#nitro-catalog-main-navigation');
    const navigationBox = content.querySelector('.nitro-catalog-navigation-grid-container') || navigation?.parentElement;
    const nativeMain = directChildContaining(content, navigation);
    const side = directChildContaining(nativeMain, navigation);
    const contentColumn = nativeMain ? elementChildren(nativeMain).find(child => child !== side && !child.classList.contains('pc5-home')) : null;

    nativeMain?.classList.add('pc5-main');
    side?.classList.add('pc5-side');
    contentColumn?.classList.add('pc5-store-content');
    navigationBox?.classList.add('pc5-navigation-box');
    navigation?.classList.add('pc5-navigation');
    markCategoryRows(navigation || navigationBox);

    const search = side?.querySelector('input[type="search"],input[type="text"]');
    const searchHost = search?.closest('form,.d-flex,.flex-row') || search?.parentElement?.parentElement;
    searchHost?.classList.add('pc5-native-search');

    const products = markProducts(root, navigation || navigationBox);
    const firstProduct = products[0] || null;
    const productGrid = firstProduct?.parentElement || content.querySelector('.nitro-catalog-grid,[class*="catalog-grid"]');
    productGrid?.classList.add('pc5-product-grid');

    const storeBoundary = contentColumn || nativeMain || content;
    const inner = firstProduct ? findRowAround(firstProduct, storeBoundary) : null;
    if(inner)
    {
      inner.classList.add('pc5-store-row');
      const gridColumn = elementChildren(inner).find(child => child.contains(firstProduct));
      const preview = elementChildren(inner).find(child => child !== gridColumn);
      gridColumn?.classList.add('pc5-grid-column');
      preview?.classList.add('pc5-preview');
    }

    const buyButtons = [ ...root.querySelectorAll('button') ].filter(button => /^(?:Acheter|Buy|Purchase)$/i.test(clean(button.textContent)));
    buyButtons.forEach(button => button.classList.add('pc5-buy'));
    const purchase = root.querySelector('.nitro-catalog-purchase-component,[class*="catalog-purchase"],[class*="purchase-component"]') || buyButtons[0]?.parentElement;
    purchase?.classList.add('pc5-purchase');

    return { hasStore: products.length > 0 || !!navigation, hasNavigation: !!navigation };
  }

  function translate(root)
  {
    const translations = [ [ /^(?:Buy|Purchase)$/i, 'Acheter' ], [ /^Gift$/i, 'Offrir' ], [ /^Search$/i, 'Rechercher' ] ];
    root.querySelectorAll('button,a,label,span').forEach(node => {
      if(node.children.length || node.closest('.pc5-header,.pc5-search,.pc5-home,.pc5-tabs')) return;
      const value = clean(node.textContent);
      const rule = translations.find(([ pattern ]) => pattern.test(value));
      if(rule) node.textContent = rule[1];
    });
  }

  function applyMode(root, tabs, structure)
  {
    const activeTab = tabs.find(isActive);
    const activeKey = activeTab?.dataset.pc5Key || '';
    const home = activeKey === 'home' || !structure.hasStore;
    root.classList.toggle('pc5-home-mode', home);
    root.classList.toggle('pc5-store-mode', !home);
  }

  function decorate(root)
  {
    if(!(root instanceof HTMLElement)) return;
    root.classList.add('pc5-catalog');
    root.dataset.paradiseCatalogBuild = BUILD;
    ensureChrome(root);
    const tabs = decorateTabs(root);
    const structure = decorateContent(root);
    translate(root);
    applyMode(root, tabs, structure);
  }

  let frame = 0;
  function refresh()
  {
    if(frame) return;
    frame = requestAnimationFrame(() => {
      frame = 0;
      document.querySelectorAll(ROOT_SELECTOR).forEach(decorate);
    });
  }

  function boot()
  {
    refresh();
    [ 100, 300, 700, 1400 ].forEach(delay => window.setTimeout(refresh, delay));
    document.addEventListener('click', event => {
      if(event.target.closest(ROOT_SELECTOR)) window.setTimeout(refresh, 0);
    }, true);
    new MutationObserver(refresh).observe(document.body, { childList: true, subtree: true });
    console.info('[ParadiseRP] catalogue City V5 chargé');
  }

  document.readyState === 'loading'
    ? document.addEventListener('DOMContentLoaded', boot, { once: true })
    : boot();
})();
