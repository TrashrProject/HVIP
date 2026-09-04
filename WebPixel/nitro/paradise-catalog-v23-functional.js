(() => {
  'use strict';

  const ROOT = '.nitro-catalog';
  const BUILD = 'paradise-catalog-v23-functional';
  const clean = value => String(value || '').replace(/\s+/g, ' ').trim();

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

  const getTabDef = tab => TAB_DEFS.find(def => tab.dataset.prc23Key === def.key) ||
    TAB_DEFS.find(def => def.match.test(clean(tab.textContent))) || null;

  const findTextNode = (tab, def) => {
    const walker = document.createTreeWalker(tab, NodeFilter.SHOW_TEXT);
    let fallback = null;
    let current = walker.nextNode();

    while(current)
    {
      if(current.parentElement?.classList?.contains('prc23-tab-icon'))
      {
        current = walker.nextNode();
        continue;
      }

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

  const getClickable = tab => {
    if(tab.matches('button,a,[role="tab"],[role="button"]')) return tab;
    return tab.querySelector('button,a,[role="tab"],[role="button"]') || tab;
  };

  function decorateTabs(root)
  {
    const tabs = getTabCandidates(root);

    tabs.forEach(tab =>
    {
      const def = getTabDef(tab);
      if(!def) return;

      tab.dataset.prc23Key = def.key;
      tab.classList.add('prc23-tab');

      const raw = clean(tab.textContent);
      const suffix = raw.match(/\s*\(\d+\)\s*$/)?.[0] || '';
      const textNode = findTextNode(tab, def);

      if(textNode)
      {
        const existing = clean(textNode.nodeValue);
        const existingSuffix = existing.match(/\s*\(\d+\)\s*$/)?.[0] || suffix;
        const desired = `${ def.label }${ existingSuffix }`;
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
    return tab.matches('.active,[aria-selected="true"]') ||
      !!tab.querySelector('.active,[aria-selected="true"]');
  }

  function openDefaultStorefront(root)
  {
    if(root.dataset.prc23DefaultOpened === '1') return;

    const tabs = getTabCandidates(root);
    const home = tabs.find(tab => tab.dataset.prc23Key === 'home');
    const furni = tabs.find(tab => tab.dataset.prc23Key === 'furni');
    if(!home || !furni) return;

    const itemGridAlreadyVisible = !!root.querySelector('.layout-grid-item,[class*="catalog-grid-item"]');
    if(itemGridAlreadyVisible || isTabActive(furni))
    {
      root.dataset.prc23DefaultOpened = '1';
      return;
    }

    if(!isTabActive(home)) return;

    root.dataset.prc23DefaultOpened = '1';
    window.setTimeout(() => {
      if(!root.isConnected) return;
      try { getClickable(furni).click(); } catch(_) {}
    }, 80);
  }

  const extractNumber = value => {
    const match = clean(value).match(/\d[\d\s.,]*/);
    return match ? clean(match[0]) : '';
  };

  function readWalletValues()
  {
    const values = [];
    const selectors = [
      '.nitro-purse-container .nitro-purse-button',
      '.nitro-purse-container [class*="currency"]',
      '.nitro-purse [class*="currency"]'
    ];

    selectors.forEach(selector => {
      document.querySelectorAll(selector).forEach(node => {
        const value = extractNumber(node.textContent);
        if(value && !values.includes(value)) values.push(value);
      });
    });

    if(values.length < 2)
    {
      const purse = document.querySelector('.nitro-purse-container,.nitro-purse');
      const matches = clean(purse?.textContent).match(/\d[\d\s.,]*/g) || [];
      matches.map(extractNumber).filter(Boolean).forEach(value => {
        if(!values.includes(value)) values.push(value);
      });
    }

    return { credits: values[0] || '', diamonds: values[1] || '' };
  }

  function updateWallet(root)
  {
    const banner = root.querySelector(':scope > .prc22-brand-banner');
    if(!banner) return;

    const values = readWalletValues();
    const credits = banner.querySelector('.prc22-wallet-credits .prc22-wallet-value');
    const diamonds = banner.querySelector('.prc22-wallet-diamonds .prc22-wallet-value');

    if(credits && values.credits) credits.textContent = values.credits;
    if(diamonds && values.diamonds) diamonds.textContent = values.diamonds;
  }

  function translateActions(root)
  {
    const rules = [
      [/^(Buy|Purchase)$/i, 'Acheter'],
      [/^Gift$/i, 'Offrir'],
      [/^Search$/i, 'Rechercher'],
      [/^Back$/i, 'Retour']
    ];

    root.querySelectorAll('button,a,label,span').forEach(node => {
      if(node.closest('.prc22-brand-banner')) return;
      if(node.closest('.prc23-tab')) return;
      if(node.children.length) return;
      const value = clean(node.textContent);
      if(!value) return;

      for(const [ matcher, replacement ] of rules)
      {
        if(matcher.test(value))
        {
          node.textContent = replacement;
          break;
        }
      }
    });

    root.querySelectorAll('input[type="text"],input[type="search"]').forEach(input => {
      input.placeholder = 'Rechercher un furni...';
      input.setAttribute('aria-label', 'Rechercher dans le catalogue');
    });
  }

  function applyPageMode(root)
  {
    const hasProducts = !!root.querySelector('.layout-grid-item,[class*="catalog-grid-item"]');
    const hasNavigation = !!root.querySelector('#nitro-catalog-main-navigation,.nitro-catalog-navigation-grid-container');

    root.classList.toggle('prc23-store', hasProducts || hasNavigation);
    root.classList.toggle('prc23-home', !hasProducts && !hasNavigation);

    root.querySelectorAll('.layout-grid-item,[class*="catalog-grid-item"]').forEach(item => {
      item.classList.add('prc23-product-card');
    });

    const buyButtons = [ ...root.querySelectorAll('button') ].filter(button => /^(Acheter|Buy|Purchase)$/i.test(clean(button.textContent)));
    buyButtons.forEach(button => button.classList.add('prc23-buy'));
  }

  function decorate(root)
  {
    if(!(root instanceof HTMLElement)) return;
    root.dataset.prc23Build = BUILD;
    decorateTabs(root);
    translateActions(root);
    applyPageMode(root);
    updateWallet(root);
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
        if(mutation.addedNodes.length)
        {
          scheduleDecorate();
          return;
        }
      }
    }).observe(document.body, { childList: true, subtree: true });

    console.info('[ParadiseRP] catalogue functional polish loaded', BUILD);
  }

  document.readyState === 'loading'
    ? document.addEventListener('DOMContentLoaded', boot, { once: true })
    : boot();
})();
