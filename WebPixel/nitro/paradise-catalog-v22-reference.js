(() => {
  'use strict';

  const ROOT = '.nitro-catalog';
  const text = node => (node?.textContent || '').replace(/\s+/g, ' ').trim();
  const add = (node, cls) => { if(node instanceof HTMLElement) node.classList.add(cls); return node; };

  const translations = new Map([
    ['Front Page', 'Accueil'], ['Furni', 'Mobilier'], ['Furniture', 'Mobilier'],
    ['Clothing', 'Vêtements'], ['Pets', 'Animaux'], ['Building', 'Construction'],
    ['Buy', 'Acheter'], ['Purchase', 'Acheter'], ['Gift', 'Offrir']
  ]);

  const getHeader = root => root.querySelector(':scope > .nitro-card-header, :scope > [class*="card-header"]');
  const findTopNav = root => root.querySelector(':scope > .nitro-card-tabs, :scope > [class*="card-tabs"], .nitro-card-tabs');

  function getNativeClose(root)
  {
    const header = getHeader(root);
    if(!header) return null;
    return [ ...header.querySelectorAll('button,[role="button"],.close,[class*="close"],[class*="cross"]') ]
      .find(node => !node.classList.contains('prc22-close')) || null;
  }

  function closeCatalog(root)
  {
    const native = getNativeClose(root);
    if(native) { native.click(); return; }
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', code: 'Escape', bubbles: true }));
  }

  function clickTab(root, matcher)
  {
    const nav = findTopNav(root);
    if(!nav) return;
    const target = [ ...nav.querySelectorAll('button,a,[role="button"],li,div') ]
      .find(node => matcher.test(text(node)) && text(node).length < 80);
    target?.click();
  }

  function ensureBanner(root)
  {
    let banner = root.querySelector(':scope > .prc22-brand-banner');
    if(!banner)
    {
      banner = document.createElement('div');
      banner.className = 'prc22-brand-banner';
      banner.innerHTML = `
        <div class="prc22-brand-left">
          <span class="prc22-brand-logo"><img src="/Dynamics/img/logos/hv_logo_p.png" alt="ParadiseRP" draggable="false"></span>
          <span class="prc22-brand-copy"><strong>CATALOGUE</strong><em>ParadiseRP</em></span>
        </div>
        <div class="prc22-header-center" aria-hidden="true"><span>Des milliers de furnis<br>pour rendre votre ville unique !</span></div>
        <div class="prc22-wallets" role="group" aria-label="Soldes du joueur"></div>
        <button type="button" class="prc22-close" aria-label="Fermer" title="Fermer">×</button>`;
      root.prepend(banner);

      const close = banner.querySelector('.prc22-close');
      close?.addEventListener('pointerdown', event => event.stopPropagation());
      close?.addEventListener('click', event => {
        event.preventDefault();
        event.stopPropagation();
        closeCatalog(root);
      });
    }

  }

  function translate(root)
  {
    root.querySelectorAll('button,a,span,p,label').forEach(node => {
      if(node.closest('.prc22-brand-banner') || node.children.length) return;
      const value = text(node);
      if(!value) return;
      const suffix = value.match(/\s*\(\d+\)\s*$/)?.[0] || '';
      const key = value.replace(/\s*\(\d+\)\s*$/, '');
      if(translations.has(key)) node.textContent = translations.get(key) + suffix;
    });
    root.querySelectorAll('input[type="text"],input[type="search"]').forEach(input => { input.placeholder = 'Rechercher un furni...'; });
  }

  function decorateStructure(root)
  {
    const content = root.querySelector(':scope > .nitro-card-content');
    if(!content) return;
    add(findTopNav(root), 'prc22-topnav');
    const mainGrid = content.querySelector(':scope > .grid') || content.querySelector('.grid');
    add(mainGrid, 'prc22-main-layout');
    const navigation = content.querySelector('#nitro-catalog-main-navigation');
    const navContainer = content.querySelector('.nitro-catalog-navigation-grid-container') || navigation?.parentElement;
    const sideColumn = navigation?.closest('.g-col-3,.col-3') || navContainer?.parentElement;
    add(navigation, 'prc22-category-panel'); add(navContainer, 'prc22-category-wrap'); add(sideColumn, 'prc22-side-column');
    const searchInput = sideColumn?.querySelector('input[type="text"],input[type="search"]');
    add(searchInput?.closest('.d-flex,.flex-row') || searchInput?.parentElement?.parentElement, 'prc22-search');

    let contentColumn = null;
    if(mainGrid) contentColumn = [ ...mainGrid.children ].find(node => node !== sideColumn && node instanceof HTMLElement) || null;
    add(contentColumn, 'prc22-content-column');
    const itemCells = [ ...root.querySelectorAll('.layout-grid-item,[class*="catalog-grid-item"]') ];
    const firstCell = itemCells[0] || null;
    const itemGrid = firstCell?.parentElement || root.querySelector('.nitro-catalog-grid,[class*="catalog-grid"]');
    add(itemGrid, 'prc22-grid'); itemCells.forEach(cell => add(cell, 'prc22-item'));

    let innerGrid = null;
    if(contentColumn) innerGrid = [ ...contentColumn.querySelectorAll('.grid') ].find(grid => grid.contains(firstCell)) || null;
    add(innerGrid, 'prc22-inner-layout');
    let gridColumn = null, previewColumn = null;
    if(innerGrid)
    {
      gridColumn = [ ...innerGrid.children ].find(node => firstCell && node.contains(firstCell)) || null;
      previewColumn = [ ...innerGrid.children ].find(node => node !== gridColumn && node instanceof HTMLElement) || null;
    }
    add(gridColumn, 'prc22-grid-column'); add(previewColumn, 'prc22-preview-column');
    const buyButtons = [ ...root.querySelectorAll('button') ].filter(button => /acheter|buy|purchase|offrir/i.test(text(button)));
    buyButtons.forEach(button => add(button, 'prc22-buy-button'));
    const purchase = root.querySelector('.nitro-catalog-purchase-component,[class*="catalog-purchase"],[class*="purchase-component"]') || buyButtons[0]?.closest('[class*="purchase"],.d-flex.flex-column') || null;
    add(purchase, 'prc22-purchase');
    ensureStaffPromo(root, sideColumn);
  }

  function ensureStaffPromo(root, sideColumn)
  {
    if(!sideColumn || sideColumn.querySelector('.prc22-staff-promo')) return;
    const promo = document.createElement('div');
    promo.className = 'prc22-staff-promo';
    promo.setAttribute('role', 'button'); promo.setAttribute('tabindex', '0');
    promo.innerHTML = `<span class="prc22-staff-crown" aria-hidden="true"></span><span class="prc22-staff-copy"><strong>Catalogue Staff</strong><small>Furnis exclusifs</small></span><span class="prc22-staff-arrow">›</span>`;
    const openStaff = () => clickTab(root, /staff/i);
    promo.addEventListener('click', openStaff);
    promo.addEventListener('keydown', event => { if(event.key === 'Enter' || event.key === ' ') { event.preventDefault(); openStaff(); } });
    sideColumn.appendChild(promo);
  }

  function decorate(root)
  {
    if(!(root instanceof HTMLElement)) return;
    root.classList.add('paradise-catalog-v22');
    ensureBanner(root); translate(root); decorateStructure(root);
  }

  function decorateWithSettling(root)
  {
    decorate(root);
    window.setTimeout(() => root.isConnected && decorate(root), 120);
    window.setTimeout(() => root.isConnected && decorate(root), 420);
  }

  function boot()
  {
    document.querySelectorAll(ROOT).forEach(decorateWithSettling);
    new MutationObserver(mutations => {
      let refreshExisting = false;
      for(const mutation of mutations)
      {
        for(const node of mutation.addedNodes)
        {
          if(!(node instanceof Element)) continue;
          if(node.matches(ROOT)) decorateWithSettling(node);
          node.querySelectorAll?.(ROOT).forEach(decorateWithSettling);
          if(node.closest?.(ROOT) || node.querySelector?.(ROOT)) refreshExisting = true;
        }
      }
      if(refreshExisting) document.querySelectorAll(ROOT).forEach(root => window.setTimeout(() => decorate(root), 30));
    }).observe(document.body, { childList: true, subtree: true });
  }

  document.readyState === 'loading' ? document.addEventListener('DOMContentLoaded', boot, { once: true }) : boot();
})();
