(() => {
  'use strict';

  const ROOT = '.nitro-catalog';
  const BUILD = 'pcity-v5-proxy-shell';
  const clean = value => String(value || '').replace(/\s+/g, ' ').trim();
  const getHeader = root => root.querySelector(':scope > .nitro-card-header, :scope > [class*="card-header"]');
  const getTabs = root => root.querySelector(':scope > .nitro-card-tabs, :scope > [class*="card-tabs"]');
  const getContent = root => root.querySelector(':scope > .nitro-card-content');
  const getClickable = node => node?.matches?.('button,a,[role="button"],[role="tab"]') ? node : node?.querySelector?.('button,a,[role="button"],[role="tab"]');

  const SEGMENTS = [
    { key:'official', label:'MOBIS OFFICIEL', tab:/^(?:Furni|Furniture|Mobilier)(?:\s*\(\d+\))?$/i },
    { key:'custom', label:'MOBIS CUSTOM', tab:/^Catalogue ParadiseRP complet.*$/i },
    { key:'city', label:'MOBIS CITY', tab:/^(?:Building|Construction)(?:\s*\(\d+\))?$/i },
    { key:'utility', label:'UTILITAIRES', tab:/^Staff(?:\s*\(\d+\))?$/i },
    { key:'rares', label:'RARES', tab:/^(?:Furni|Furniture|Mobilier)(?:\s*\(\d+\))?$/i, category:/rare|limited|collector/i }
  ];

  const state = new WeakMap();

  function rootState(root)
  {
    if(!state.has(root)) state.set(root, { view:'info', segment:'official', categoryIndex:-1, productIndex:-1, categorySig:'', productSig:'' });
    return state.get(root);
  }

  function nativeClose(root)
  {
    const header = getHeader(root);
    if(!header) return null;
    return [ ...header.querySelectorAll('button,[role="button"],.close,[class*="close"],[class*="cross"]') ][0] || null;
  }

  function closeCatalog(root)
  {
    const button = nativeClose(root);
    if(button) { button.click(); return; }
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

  function findNativeSearch(root)
  {
    const content = getContent(root);
    const input = content?.querySelector('input[type="search"],input[type="text"]');
    if(!input) return null;
    const row = input.closest('.d-flex,.flex-row') || input.parentElement?.parentElement || input.parentElement;
    return { input, button:row?.querySelector('button'), row };
  }

  function setNativeInputValue(input, value)
  {
    if(!input) return;
    const setter = Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, 'value')?.set;
    setter?.call(input, value);
    input.dispatchEvent(new Event('input', { bubbles:true }));
    input.dispatchEvent(new Event('change', { bubbles:true }));
  }

  function nativeNav(root)
  {
    const content = getContent(root);
    if(!content) return null;
    return content.querySelector('#nitro-catalog-main-navigation') || content.querySelector('.nitro-catalog-navigation-grid-container');
  }

  function nativeCategories(root)
  {
    const nav = nativeNav(root);
    if(!nav) return [];
    let rows = [ ...nav.children ].filter(node => node instanceof HTMLElement && clean(node.textContent));
    if(!rows.length)
    {
      rows = [ ...nav.querySelectorAll('button,a,[role="button"],.list-group-item,.layout-grid-item') ]
        .filter(node => clean(node.textContent));
    }
    return rows.map(node => ({ node, click:getClickable(node) || node, label:clean(node.textContent) }));
  }

  function elementBackground(node)
  {
    if(!(node instanceof HTMLElement)) return '';
    const all = [ node, ...node.querySelectorAll('*') ];
    for(const el of all)
    {
      if(!(el instanceof HTMLElement)) continue;
      const inline = el.style.backgroundImage;
      if(inline && inline !== 'none') return inline;
      const computed = getComputedStyle(el).backgroundImage;
      if(computed && computed !== 'none') return computed;
      const img = el.matches('img[src]') ? el : el.querySelector?.('img[src]');
      if(img?.src) return `url("${ img.src }")`;
    }
    return '';
  }

  function nativeProducts(root)
  {
    const content = getContent(root);
    const nav = nativeNav(root);
    if(!content) return [];
    const items = [ ...content.querySelectorAll('.layout-grid-item,[class*="catalog-grid-item"]') ]
      .filter(node => !nav?.contains(node));
    return items
      .map(node => ({ node, click:getClickable(node) || node, image:elementBackground(node) }))
      .filter(item => item.image);
  }

  function isNativeSelected(node)
  {
    return !!node && (node.matches('.active,.selected,[aria-selected="true"]') || !!node.querySelector('.active,.selected,[aria-selected="true"]'));
  }

  function nativePurchase(root)
  {
    return getContent(root)?.querySelector('.nitro-catalog-purchase-component,[class*="catalog-purchase"],[class*="purchase-component"]') || null;
  }

  function nativeAction(root, regex)
  {
    return [ ...getContent(root)?.querySelectorAll('button') || [] ].find(button => regex.test(clean(button.textContent))) || null;
  }

  function ensureShell(root)
  {
    root.classList.add('pc5-catalog');
    root.dataset.pc5Build = BUILD;

    let shell = root.querySelector(':scope > .pc5-shell');
    if(shell) return shell;

    shell = document.createElement('div');
    shell.className = 'pc5-shell';
    shell.innerHTML = `
      <div class="pc5-titlebar">
        <div class="pc5-title">Catalogue de Paradise</div>
        <button type="button" class="pc5-close" aria-label="Fermer">×</button>
      </div>
      <div class="pc5-searchbar">
        <div class="pc5-search-icon">⌕</div>
        <input class="pc5-search-input" type="search" placeholder="Rechercher un mobi, une catégorie...">
        <button type="button" class="pc5-search-go" aria-label="Rechercher">⌕</button>
      </div>
      <div class="pc5-segments">
        ${ SEGMENTS.map(seg => `<button type="button" class="pc5-segment" data-seg="${ seg.key }">${ seg.label }</button>`).join('') }
      </div>
      <div class="pc5-body">
        <aside class="pc5-left">
          <div class="pc5-left-scroll">
            <button type="button" class="pc5-menu-btn is-active" data-view="info">Informations</button>
            <button type="button" class="pc5-menu-btn" data-view="history">Historique</button>
            <div class="pc5-category-list"></div>
          </div>
        </aside>
        <section class="pc5-right">
          <div class="pc5-view pc5-info is-visible" data-view-panel="info">
            <div class="pc5-info-heading">
              <div class="pc5-info-badge">▦</div>
              <div class="pc5-info-title">Deux, trois trucs à savoir</div>
            </div>
            <div class="pc5-info-copy">
              <p>- Pour décorer ton appart, il suffit d'acheter le mobilier que tu souhaites et de le récupérer dans ton inventaire pour ensuite pouvoir le poser dans ton appartement.</p>
              <p>- Recherche un mobilier en fonction de son nom depuis la barre de recherche, exemple : chaise.</p>
              <p>- Les mobiliers peuvent ensuite être posés avec les commandes de construction de ParadiseRP.</p>
              <p>- Les animaux ainsi que les bots apparaissent eux aussi dans l'inventaire, dans leurs catégories dédiées.</p>
            </div>
            <div class="pc5-info-footer">En cas de question n'hésite pas à contacter notre support.</div>
          </div>
          <div class="pc5-view pc5-history" data-view-panel="history">
            <h3>Historique du catalogue</h3>
            <p>Le catalogue ParadiseRP regroupe le mobilier officiel, les créations custom, les blocs de construction et les contenus dédiés au roleplay.</p>
            <p>Les catégories affichées à gauche restent celles du catalogue Nitro afin de conserver toutes les interactions et tous les achats fonctionnels.</p>
          </div>
          <div class="pc5-view pc5-store" data-view-panel="store">
            <div class="pc5-products"></div>
            <div class="pc5-preview">
              <div class="pc5-preview-art"></div>
              <div class="pc5-preview-name">Sélectionne un mobi</div>
              <div class="pc5-preview-desc">Choisis un objet dans la grille pour afficher ses informations.</div>
              <div class="pc5-preview-price"></div>
              <div class="pc5-preview-actions">
                <button type="button" class="pc5-buy">Acheter</button>
                <button type="button" class="pc5-gift">Offrir</button>
              </div>
            </div>
          </div>
        </section>
      </div>`;
    root.appendChild(shell);

    shell.querySelector('.pc5-close')?.addEventListener('pointerdown', event => event.stopPropagation());
    shell.querySelector('.pc5-close')?.addEventListener('click', event => {
      event.preventDefault();
      event.stopPropagation();
      closeCatalog(root);
    });

    const searchInput = shell.querySelector('.pc5-search-input');
    const runSearch = () => {
      const native = findNativeSearch(root);
      if(!native)
      {
        clickNativeTab(root, SEGMENTS[0].tab);
        setTimeout(runSearch, 100);
        return;
      }
      setNativeInputValue(native.input, searchInput.value);
      native.button?.click();
      showView(root, 'store');
      setTimeout(() => syncAll(root), 120);
    };
    searchInput?.addEventListener('input', () => {
      const native = findNativeSearch(root);
      if(native) setNativeInputValue(native.input, searchInput.value);
    });
    searchInput?.addEventListener('keydown', event => {
      if(event.key === 'Enter') { event.preventDefault(); runSearch(); }
    });
    shell.querySelector('.pc5-search-go')?.addEventListener('click', runSearch);

    shell.querySelector('.pc5-segments')?.addEventListener('click', event => {
      const button = event.target.closest('.pc5-segment');
      if(!button) return;
      const seg = SEGMENTS.find(item => item.key === button.dataset.seg);
      if(!seg) return;
      const st = rootState(root);
      st.segment = seg.key;
      st.categoryIndex = -1;
      st.productIndex = -1;
      clickNativeTab(root, seg.tab);
      showView(root, 'store');
      syncSegmentState(root);
      if(seg.category)
      {
        setTimeout(() => {
          const categories = nativeCategories(root);
          const hit = categories.find(item => seg.category.test(item.label));
          hit?.click?.click();
          setTimeout(() => syncAll(root), 100);
        }, 120);
      }
      else setTimeout(() => syncAll(root), 120);
    });

    shell.querySelector('.pc5-left')?.addEventListener('click', event => {
      const view = event.target.closest('.pc5-menu-btn[data-view]');
      if(view) { showView(root, view.dataset.view); return; }
      const category = event.target.closest('.pc5-category[data-index]');
      if(category)
      {
        const index = Number(category.dataset.index);
        const native = nativeCategories(root)[index];
        if(native)
        {
          rootState(root).categoryIndex = index;
          native.click.click();
          showView(root, 'store');
          setTimeout(() => syncAll(root), 90);
        }
      }
    });

    shell.querySelector('.pc5-products')?.addEventListener('click', event => {
      const button = event.target.closest('.pc5-product[data-index]');
      if(!button) return;
      const index = Number(button.dataset.index);
      const products = nativeProducts(root);
      const item = products[index];
      if(!item) return;
      rootState(root).productIndex = index;
      item.click.click();
      setTimeout(() => syncAll(root), 70);
    });

    shell.querySelector('.pc5-buy')?.addEventListener('click', () => nativeAction(root, /^(?:Acheter|Buy|Purchase)$/i)?.click());
    shell.querySelector('.pc5-gift')?.addEventListener('click', () => nativeAction(root, /^(?:Offrir|Gift)$/i)?.click());

    return shell;
  }

  function showView(root, view)
  {
    const st = rootState(root);
    st.view = view;
    const shell = root.querySelector(':scope > .pc5-shell');
    if(!shell) return;
    shell.querySelectorAll('.pc5-view').forEach(panel => panel.classList.toggle('is-visible', panel.dataset.viewPanel === (view === 'info' ? 'info' : view === 'history' ? 'history' : 'store')));
    shell.querySelectorAll('.pc5-menu-btn').forEach(button => button.classList.toggle('is-active', button.dataset.view === view));
    if(view !== 'store') shell.querySelectorAll('.pc5-category').forEach(button => button.classList.remove('is-active'));
  }

  function syncSegmentState(root)
  {
    const shell = root.querySelector(':scope > .pc5-shell');
    if(!shell) return;
    const active = rootState(root).segment;
    shell.querySelectorAll('.pc5-segment').forEach(button => button.classList.toggle('is-active', button.dataset.seg === active));
  }

  function syncCategories(root)
  {
    const shell = root.querySelector(':scope > .pc5-shell');
    const host = shell?.querySelector('.pc5-category-list');
    if(!host) return;
    const categories = nativeCategories(root);
    const sig = categories.map(item => item.label).join('|');
    const st = rootState(root);
    if(sig !== st.categorySig)
    {
      host.replaceChildren(...categories.map((item, index) => {
        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'pc5-category';
        button.dataset.index = String(index);
        button.title = item.label;
        button.textContent = item.label;
        return button;
      }));
      st.categorySig = sig;
    }
    const current = categories.findIndex(item => isNativeSelected(item.node));
    if(current >= 0) st.categoryIndex = current;
    host.querySelectorAll('.pc5-category').forEach(button => button.classList.toggle('is-active', Number(button.dataset.index) === st.categoryIndex && st.view === 'store'));
  }

  function syncProducts(root)
  {
    const shell = root.querySelector(':scope > .pc5-shell');
    const host = shell?.querySelector('.pc5-products');
    if(!host) return;
    const products = nativeProducts(root);
    const sig = products.map(item => item.image).join('|');
    const st = rootState(root);
    if(sig !== st.productSig)
    {
      host.replaceChildren(...products.map((item, index) => {
        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'pc5-product';
        button.dataset.index = String(index);
        button.style.backgroundImage = item.image;
        button.title = item.node.getAttribute('aria-label') || item.node.getAttribute('title') || `Mobi ${ index + 1 }`;
        return button;
      }));
      st.productSig = sig;
    }
    const selected = products.findIndex(item => isNativeSelected(item.node));
    if(selected >= 0) st.productIndex = selected;
    host.querySelectorAll('.pc5-product').forEach(button => button.classList.toggle('is-selected', Number(button.dataset.index) === st.productIndex));
  }

  function leafTexts(node)
  {
    if(!node) return [];
    return [ ...node.querySelectorAll('*') ]
      .filter(el => !el.children.length)
      .map(el => clean(el.textContent))
      .filter(Boolean);
  }

  function syncPreview(root)
  {
    const shell = root.querySelector(':scope > .pc5-shell');
    if(!shell) return;
    const products = nativeProducts(root);
    const st = rootState(root);
    const item = products[st.productIndex] || products.find(product => isNativeSelected(product.node));
    const art = shell.querySelector('.pc5-preview-art');
    const name = shell.querySelector('.pc5-preview-name');
    const desc = shell.querySelector('.pc5-preview-desc');
    const price = shell.querySelector('.pc5-preview-price');
    if(!item)
    {
      art.style.backgroundImage = '';
      name.textContent = 'Sélectionne un mobi';
      desc.textContent = 'Choisis un objet dans la grille pour afficher ses informations.';
      price.textContent = '';
      return;
    }

    art.style.backgroundImage = item.image || '';
    const purchase = nativePurchase(root);
    const texts = leafTexts(purchase);
    const ignored = /^(Acheter|Buy|Purchase|Offrir|Gift|Choisir une quantité|Quantity|\+|-|\d+)$/i;
    const candidates = texts.filter(text => !ignored.test(text));
    const title = candidates.find(text => /[A-Za-zÀ-ÿ]/.test(text) && text.length < 80) || item.node.getAttribute('aria-label') || 'Mobi sélectionné';
    const numbers = texts.filter(text => /\d/.test(text));
    name.textContent = title;
    desc.textContent = candidates.filter(text => text !== title).slice(0, 3).join(' ') || 'Objet du catalogue ParadiseRP.';
    price.textContent = numbers.length ? `Prix : ${ numbers.at(-1) }` : '';
  }

  function syncAll(root)
  {
    ensureShell(root);
    syncSegmentState(root);
    syncCategories(root);
    syncProducts(root);
    syncPreview(root);
  }

  let queued = false;
  function refresh()
  {
    if(queued) return;
    queued = true;
    requestAnimationFrame(() => {
      queued = false;
      document.querySelectorAll(ROOT).forEach(syncAll);
    });
  }

  function boot()
  {
    refresh();
    [100, 260, 600, 1100].forEach(delay => setTimeout(refresh, delay));
    new MutationObserver(refresh).observe(document.body, { childList:true, subtree:true, attributes:true, attributeFilter:['class','style'] });
    console.info('[ParadiseRP] catalogue City reference V5 proxy shell loaded');
  }

  document.readyState === 'loading' ? document.addEventListener('DOMContentLoaded', boot, { once:true }) : boot();
})();
