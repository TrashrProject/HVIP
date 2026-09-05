(() => {
  'use strict';

  const ROOT = '.nitro-catalog.pc5-catalog';
  const clean = value => String(value || '').replace(/\s+/g, ' ').trim();

  const SEGMENT_MAP = {
    official: /^(?:Furni|Furniture|Mobilier)(?:\s*\(\d+\))?$/i,
    custom: /^Catalogue ParadiseRP complet.*$/i,
    city: /^(?:Building|Construction)(?:\s*\(\d+\))?$/i,
    utility: /^Staff(?:\s*\(\d+\))?$/i,
    rares: /^(?:Furni|Furniture|Mobilier)(?:\s*\(\d+\))?$/i
  };

  const nativeTabs = root => root.querySelector(':scope > .nitro-card-tabs, :scope > [class*="card-tabs"]');
  const nativeContent = root => root.querySelector(':scope > .nitro-card-content');
  const nativeNav = root => nativeContent(root)?.querySelector('#nitro-catalog-main-navigation') || nativeContent(root)?.querySelector('.nitro-catalog-navigation-grid-container');

  function clickTarget(node)
  {
    if(!node) return false;
    const target = node.matches?.('button,a,[role="button"],[role="tab"],.layout-grid-item,.nav-item')
      ? node
      : node.querySelector?.('button,a,[role="button"],[role="tab"],.layout-grid-item,.nav-item');
    (target || node).dispatchEvent(new MouseEvent('click', { bubbles:true, cancelable:true, view:window }));
    return true;
  }

  function findNativeTab(root, pattern)
  {
    const tabs = nativeTabs(root);
    if(!tabs) return null;
    const candidates = [ ...tabs.querySelectorAll('.nav-item,[role="tab"],button,a') ];
    return candidates.find(node => pattern.test(clean(node.textContent)))
      || [ ...tabs.children ].find(node => pattern.test(clean(node.textContent)))
      || null;
  }

  function activateTab(root, key)
  {
    const pattern = SEGMENT_MAP[key] || SEGMENT_MAP.official;
    const tab = findNativeTab(root, pattern);
    if(!tab) return false;
    clickTarget(tab);
    return true;
  }

  function nativeOfferCount(root)
  {
    const content = nativeContent(root);
    const nav = nativeNav(root);
    if(!content) return 0;
    return [ ...content.querySelectorAll('.layout-grid-item') ]
      .filter(node => !nav?.contains(node)).length;
  }

  function navigationItems(root)
  {
    const nav = nativeNav(root);
    if(!nav) return [];
    return [ ...nav.querySelectorAll('.nitro-catalog-navigation-section > .layout-grid-item, .nitro-catalog-navigation-section .layout-grid-item') ]
      .filter((node, index, all) => all.indexOf(node) === index && clean(node.textContent));
  }

  function openUsefulCategory(root, key, pass = 0)
  {
    if(pass > 8 || nativeOfferCount(root) > 0) return;
    const items = navigationItems(root);
    if(!items.length)
    {
      setTimeout(() => openUsefulCategory(root, key, pass + 1), 110);
      return;
    }

    let chosen = null;

    if(key === 'rares') chosen = items.find(node => /rare|limited|collector/i.test(clean(node.textContent)));
    if(!chosen) chosen = items.find(node => node.classList.contains('inset'));
    if(!chosen) chosen = items.find(node => /mobilier|furni|maison|construction|d[eé]cor|int[eé]rieur|ville|nature|custom/i.test(clean(node.textContent)));
    if(!chosen) chosen = items[0];

    clickTarget(chosen);
    window.dispatchEvent(new Event('resize'));

    setTimeout(() => openUsefulCategory(root, key, pass + 1), 130);
  }

  function showNativeStore(root, key)
  {
    root.classList.add('pc9-native-store');
    root.dataset.pc9Segment = key;

    const shell = root.querySelector(':scope > .pc5-shell');
    shell?.querySelectorAll('.pc5-menu-btn').forEach(button => button.classList.remove('is-active'));

    if(!activateTab(root, key))
    {
      setTimeout(() => showNativeStore(root, key), 120);
      return;
    }

    [100, 240, 450, 750].forEach(delay => setTimeout(() => {
      window.dispatchEvent(new Event('resize'));
      openUsefulCategory(root, key);
    }, delay));
  }

  function showCustomPanel(root)
  {
    root.classList.remove('pc9-native-store');
  }

  function setReactInputValue(input, value)
  {
    if(!input) return;
    const setter = Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, 'value')?.set;
    if(setter) setter.call(input, value);
    else input.value = value;
    input.dispatchEvent(new Event('input', { bubbles:true }));
    input.dispatchEvent(new Event('change', { bubbles:true }));
  }

  function nativeSearch(root, query)
  {
    const content = nativeContent(root);
    if(!content) return false;
    const input = content.querySelector('input[type="search"],input[type="text"]');
    if(!input) return false;

    setReactInputValue(input, query);
    const wrapper = input.closest('.d-flex,.flex-row,.row') || input.parentElement?.parentElement || input.parentElement;
    const button = wrapper?.querySelector('button');
    button?.click();
    return true;
  }

  function bind(root)
  {
    if(!(root instanceof HTMLElement) || root.dataset.pc9Bound === '1') return;
    const shell = root.querySelector(':scope > .pc5-shell');
    if(!shell) return;
    root.dataset.pc9Bound = '1';

    const segments = shell.querySelector('.pc5-segments');
    segments?.addEventListener('click', event => {
      const button = event.target.closest('.pc5-segment[data-seg]');
      if(!button) return;
      const key = button.dataset.seg || 'official';
      setTimeout(() => showNativeStore(root, key), 0);
    });

    shell.querySelector('.pc5-left')?.addEventListener('click', event => {
      if(event.target.closest('.pc5-menu-btn[data-view]')) showCustomPanel(root);
    });

    const customInput = shell.querySelector('.pc5-search-input');
    const runSearch = () => {
      const query = clean(customInput?.value);
      showNativeStore(root, root.dataset.pc9Segment || 'official');
      setTimeout(() => {
        if(!nativeSearch(root, query)) setTimeout(() => nativeSearch(root, query), 180);
      }, 150);
    };

    customInput?.addEventListener('keydown', event => {
      if(event.key !== 'Enter') return;
      event.preventDefault();
      runSearch();
    });
    shell.querySelector('.pc5-search-go')?.addEventListener('click', runSearch);

    console.info('[ParadiseRP] CityRef V9 native-store bridge bound');
  }

  let queued = false;
  function refresh()
  {
    if(queued) return;
    queued = true;
    requestAnimationFrame(() => {
      queued = false;
      document.querySelectorAll(ROOT).forEach(bind);
    });
  }

  function boot()
  {
    refresh();
    [80, 220, 500, 1000].forEach(delay => setTimeout(refresh, delay));
    new MutationObserver(refresh).observe(document.body, { childList:true, subtree:true });
    console.info('[ParadiseRP] CityRef V9 real Nitro store engine loaded');
  }

  document.readyState === 'loading'
    ? document.addEventListener('DOMContentLoaded', boot, { once:true })
    : boot();
})();
