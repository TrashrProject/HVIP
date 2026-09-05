(() => {
  'use strict';

  const ROOT = '.nitro-catalog';
  const BUILD = 'paradise-catalog-final-interactions';
  const clean = value => String(value || '').replace(/\s+/g, ' ').trim();
  const timers = new WeakMap();

  const TAB_DEFS = [
    { key: 'home', match: /^(Front Page|Accueil)(?:\s*\(\d+\))?$/i, label: 'Accueil', icon: '⌂' },
    { key: 'furni', match: /^(Furni|Furniture|Mobilier)(?:\s*\(\d+\))?$/i, label: 'Mobilier', icon: '▰' },
    { key: 'clothing', match: /^(Clothing|Vêtements)(?:\s*\(\d+\))?$/i, label: 'Vêtements', icon: '♟' },
    { key: 'pets', match: /^(Pets|Animaux)(?:\s*\(\d+\))?$/i, label: 'Animaux', icon: '♣' },
    { key: 'building', match: /^(Building|Construction)(?:\s*\(\d+\))?$/i, label: 'Construction', icon: '▦' },
    { key: 'staff', match: /^Staff(?:\s*\(\d+\))?$/i, label: 'Staff', icon: '♛' }
  ];

  const findTopNav = root => root.querySelector(':scope > .nitro-card-tabs, :scope > [class*="card-tabs"], .nitro-card-tabs');
  const isKnownTab = node => !!node.dataset.prc23Key || TAB_DEFS.some(def => def.match.test(clean(node.textContent)));

  const getTabCandidates = root => {
    const nav = findTopNav(root);
    if(!nav) return [];
    const direct = [ ...nav.children ].filter(node => node instanceof HTMLElement);
    const resolved = direct.filter(isKnownTab);
    if(resolved.length) return resolved;
    return [ ...nav.querySelectorAll('li,button,a,[role="tab"]') ].filter(isKnownTab);
  };

  const getTabDef = tab => TAB_DEFS.find(def => tab.dataset.prc23Key === def.key) || TAB_DEFS.find(def => def.match.test(clean(tab.textContent))) || null;
  const getClickable = tab => tab.matches('button,a,[role="tab"],[role="button"]') ? tab : (tab.querySelector('button,a,[role="tab"],[role="button"]') || tab);

  const findTextNode = (tab, def) => {
    const walker = document.createTreeWalker(tab, NodeFilter.SHOW_TEXT);
    let fallback = null;
    let current = walker.nextNode();
    while(current)
    {
      if(current.parentElement?.classList?.contains('prc23-tab-icon')) { current = walker.nextNode(); continue; }
      const value = clean(current.nodeValue);
      if(value)
      {
        if(def.match.test(value)) return current;
        if(!fallback && /[A-Za-zÀ-ÿ]/.test(value)) fallback = current;
      }
      current = walker.nextNode();
    }
    return fallback;
  };

  function pulseClass(root, className, duration = 180)
  {
    root.classList.remove(className);
    void root.offsetWidth;
    root.classList.add(className);
    window.setTimeout(() => root.classList.remove(className), duration);
  }

  function decorateTabs(root)
  {
    getTabCandidates(root).forEach(tab => {
      const def = getTabDef(tab);
      if(!def) return;
      tab.dataset.prc23Key = def.key;
      tab.classList.add('prc23-tab');

      const textNode = findTextNode(tab, def);
      if(textNode)
      {
        const desired = def.label;
        if(clean(textNode.nodeValue) !== desired) textNode.nodeValue = desired;
      }

      const clickable = getClickable(tab);
      clickable.setAttribute('aria-label', def.label);
      if(!tab.querySelector(':scope > .prc23-tab-icon'))
      {
        const icon = document.createElement('span');
        icon.className = 'prc23-tab-icon';
        icon.setAttribute('aria-hidden', 'true');
        icon.textContent = def.icon;
        tab.prepend(icon);
      }
    });
  }

  function isTabActive(tab)
  {
    return tab.matches('.active,[aria-selected="true"]') || !!tab.querySelector('.active,[aria-selected="true"]');
  }

  function openDefaultStorefront(root)
  {
    if(root.dataset.prc23DefaultOpened === '1') return;
    const tabs = getTabCandidates(root);
    const home = tabs.find(tab => tab.dataset.prc23Key === 'home');
    const furni = tabs.find(tab => tab.dataset.prc23Key === 'furni');
    if(!home || !furni) return;
    const itemGridAlreadyVisible = !!root.querySelector('.layout-grid-item,[class*="catalog-grid-item"]');
    if(itemGridAlreadyVisible || isTabActive(furni)) { root.dataset.prc23DefaultOpened = '1'; return; }
    if(!isTabActive(home)) return;
    root.dataset.prc23DefaultOpened = '1';
    window.setTimeout(() => {
      if(!root.isConnected) return;
      try { getClickable(furni).click(); } catch(_) {}
    }, 80);
  }

  const getNativeCurrencies = () => {
    const currencies = [];
    document.querySelectorAll('.nitro-purse .nitro-currency-icon').forEach(icon => {
      const row = icon.closest('.nitro-purse-seasonal-currency,.nitro-purse-button');
      if(row && !row.closest('.prc22-brand-banner') && !currencies.includes(row)) currencies.push(row);
    });
    return currencies;
  };

  function makeWallet(source, index)
  {
    const wallet = document.createElement('div');
    wallet.className = 'prc22-wallet';
    wallet.dataset.prcCurrencyIndex = String(index);
    wallet.setAttribute('role', 'status');

    const icon = source.querySelector('.nitro-currency-icon,.currency-icon,[class*="currency-icon"],img,svg,i');
    const value = [ ...source.querySelectorAll('span,div,p') ].reverse().find(node => /\d/.test(clean(node.textContent)) && !node.querySelector('*'));
    const iconHost = document.createElement('span');
    iconHost.className = 'prc22-wallet-icon';
    iconHost.setAttribute('aria-hidden', 'true');
    if(icon) iconHost.appendChild(icon.cloneNode(true));

    const valueHost = document.createElement('span');
    valueHost.className = 'prc22-wallet-value';
    valueHost.textContent = clean(value?.textContent || source.textContent) || '—';

    const add = document.createElement('button');
    add.type = 'button';
    add.className = 'prc22-wallet-add';
    add.textContent = '+';
    add.setAttribute('aria-label', `Ouvrir les options de la monnaie ${ index + 1 }`);
    add.title = 'Plus d’options';
    wallet.append(iconHost, valueHost, add);
    return wallet;
  }

  function updateWallet(root)
  {
    const banner = root.querySelector(':scope > .prc22-brand-banner');
    if(!banner) return;
    const host = banner.querySelector('.prc22-wallets');
    const sources = getNativeCurrencies();
    if(!host || !sources.length) return;
    const signature = sources.map(source => `${ source.className }|${ clean(source.textContent) }|${ source.querySelector('[class*="currency-icon"]')?.className || '' }`).join('||');
    if(host.dataset.prcWalletSignature === signature) return;
    const hadWallet = !!host.children.length;
    host.replaceChildren(...sources.map(makeWallet));
    host.dataset.prcWalletSignature = signature;
    const changed = hadWallet;
    if(changed && root.dataset.prcPurchasePending === '1')
    {
      const buy = root.querySelector('.prc23-buy,.prc22-buy-button');
      if(buy)
      {
        buy.classList.add('prc-purchase-success');
        window.setTimeout(() => buy.classList.remove('prc-purchase-success'), 420);
      }
      root.dataset.prcPurchasePending = '0';
    }
  }

  function bindWalletProxy(root)
  {
    const host = root.querySelector('.prc22-wallets');
    if(!host || host.dataset.prcWalletBound === '1') return;
    host.dataset.prcWalletBound = '1';
    host.addEventListener('click', event => {
      const button = event.target.closest('.prc22-wallet-add');
      if(!button) return;
      event.preventDefault();
      event.stopPropagation();
      const wallet = button.closest('.prc22-wallet');
      const source = getNativeCurrencies()[Number(wallet?.dataset.prcCurrencyIndex)];
      const clickable = source?.closest('.nitro-purse-button') || source;
      clickable?.click();
    });
  }

  function translateActions(root)
  {
    const rules = [ [/^(Buy|Purchase)$/i, 'Acheter'], [/^Gift$/i, 'Offrir'], [/^Search$/i, 'Rechercher'], [/^Back$/i, 'Retour'] ];
    root.querySelectorAll('button,a,label,span').forEach(node => {
      if(node.closest('.prc22-brand-banner') || node.closest('.prc23-tab') || node.children.length) return;
      const value = clean(node.textContent);
      if(!value) return;
      for(const [ matcher, replacement ] of rules)
      {
        if(matcher.test(value)) { node.textContent = replacement; break; }
      }
    });
    root.querySelectorAll('input[type="text"],input[type="search"]').forEach(input => {
      input.placeholder = 'Rechercher un furni...';
      input.setAttribute('aria-label', 'Rechercher dans le catalogue');
    });
  }

  function ensureSearchClear(root)
  {
    const search = root.querySelector('.prc22-search');
    if(!search || search.querySelector('.prc-search-clear')) return;
    const input = search.querySelector('input[type="search"],input[type="text"]');
    if(!input) return;
    const clear = document.createElement('button');
    clear.type = 'button';
    clear.className = 'prc-search-clear';
    clear.setAttribute('aria-label', 'Effacer la recherche');
    clear.textContent = '×';
    const sync = () => clear.classList.toggle('is-visible', !!input.value);
    clear.addEventListener('click', () => {
      input.value = '';
      input.dispatchEvent(new Event('input', { bubbles: true }));
      input.dispatchEvent(new Event('change', { bubbles: true }));
      input.focus();
      sync();
      pulseClass(root, 'prc-grid-transition', 170);
    });
    input.addEventListener('input', sync);
    search.appendChild(clear);
    sync();
  }

  function applyPageMode(root)
  {
    const hasProducts = !!root.querySelector('.layout-grid-item,[class*="catalog-grid-item"]');
    const hasNavigation = !!root.querySelector('#nitro-catalog-main-navigation,.nitro-catalog-navigation-grid-container');
    root.classList.toggle('prc23-store', hasProducts || hasNavigation);
    root.classList.toggle('prc23-home', !hasProducts && !hasNavigation);
    root.querySelectorAll('.layout-grid-item,[class*="catalog-grid-item"]').forEach(item => item.classList.add('prc23-product-card'));
    [ ...root.querySelectorAll('button') ].filter(button => /^(Acheter|Buy|Purchase)$/i.test(clean(button.textContent))).forEach(button => button.classList.add('prc23-buy'));
  }

  function bindInteractions(root)
  {
    if(root.dataset.prcInteractionsBound === '1') return;
    root.dataset.prcInteractionsBound = '1';
    root.addEventListener('click', event => {
      const tab = event.target.closest('.prc23-tab');
      if(tab) pulseClass(root, 'prc-page-transition', 170);

      const category = event.target.closest('#nitro-catalog-main-navigation button,#nitro-catalog-main-navigation a,#nitro-catalog-main-navigation [role="button"],#nitro-catalog-main-navigation .layout-grid-item,.nitro-catalog-navigation-grid-container button,.nitro-catalog-navigation-grid-container a,.nitro-catalog-navigation-grid-container [role="button"],.nitro-catalog-navigation-grid-container > .layout-grid-item');
      if(category) pulseClass(root, 'prc-grid-transition', 170);

      const product = event.target.closest('.prc23-product-card,.prc22-item');
      if(product) pulseClass(root, 'prc-preview-transition', 160);

      const buy = event.target.closest('.prc23-buy,.prc22-buy-button');
      if(buy) root.dataset.prcPurchasePending = '1';
    }, { passive: true });
  }

  function decorate(root)
  {
    if(!(root instanceof HTMLElement)) return;
    root.dataset.prc23Build = BUILD;
    decorateTabs(root);
    translateActions(root);
    applyPageMode(root);
    ensureSearchClear(root);
    bindInteractions(root);
    updateWallet(root);
    bindWalletProxy(root);
    openDefaultStorefront(root);
  }

  let scheduled = false;
  function scheduleDecorate()
  {
    if(scheduled) return;
    scheduled = true;
    window.setTimeout(() => {
      scheduled = false;
      document.querySelectorAll(ROOT).forEach(decorate);
    }, 40);
  }

  function boot()
  {
    scheduleDecorate();
    [120, 350, 800, 1600].forEach(delay => window.setTimeout(scheduleDecorate, delay));
    let walletAttempts = 0;
    const walletTimer = window.setInterval(() => {
      walletAttempts += 1;
      document.querySelectorAll(ROOT).forEach(updateWallet);
      if(walletAttempts >= 15) window.clearInterval(walletTimer);
    }, 1000);

    new MutationObserver(mutations => {
      for(const mutation of mutations)
      {
        if(mutation.addedNodes.length || mutation.type === 'characterData') { scheduleDecorate(); return; }
      }
    }).observe(document.body, { childList: true, characterData: true, subtree: true });

    console.info('[ParadiseRP] catalogue interactions loaded', BUILD);
  }

  document.readyState === 'loading' ? document.addEventListener('DOMContentLoaded', boot, { once: true }) : boot();
})();
