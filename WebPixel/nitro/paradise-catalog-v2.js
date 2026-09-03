(() => {
  'use strict';

  const ROOT = '.nitro-catalog';
  let queued = false;

  const text = (node) => (node?.textContent || '').replace(/\s+/g, ' ').trim();
  const add = (node, name) => { if (node instanceof HTMLElement) node.classList.add(name); return node; };

  const getTopNav = (root) => {
    const candidates = [...root.querySelectorAll('div, nav, ul')];
    return candidates.find((node) => {
      const t = text(node);
      return t.length < 260 && /Front Page/i.test(t) && /Furni/i.test(t) && /Clothing/i.test(t) && /Staff/i.test(t);
    }) || null;
  };

  const getCategoryPanel = (root) => {
    const labels = [...root.querySelectorAll('div,span,p,h1,h2,h3,h4')];
    const label = labels.find((node) => /^(cat[eé]gories|categories)$/i.test(text(node)));
    if (!label) return null;
    let node = label.parentElement;
    for (let i = 0; node && node !== root && i < 5; i += 1, node = node.parentElement) {
      if (node.children.length >= 3 && node.scrollHeight > 120) return node;
    }
    return label.parentElement;
  };

  const getGrid = (root) => {
    const direct = root.querySelector('.nitro-catalog-grid');
    if (direct) return direct;
    return [...root.querySelectorAll('[class*="catalog-grid"]')]
      .find((node) => !node.className.includes('grid-item') && node.children.length >= 4) || null;
  };

  const getPurchase = (root) => {
    const direct = root.querySelector('.nitro-catalog-purchase-component, [class*="catalog-purchase"], [class*="purchase-component"]');
    if (direct) return direct;
    const buy = [...root.querySelectorAll('button')].find((button) => /acheter|buy|purchase/i.test(text(button)));
    if (!buy) return null;
    let node = buy.parentElement;
    for (let i = 0; node && node !== root && i < 4; i += 1, node = node.parentElement) {
      if (text(node).length < 900) return node;
    }
    return buy.parentElement;
  };

  const decorate = (root) => {
    if (!(root instanceof HTMLElement)) return;

    root.classList.add('paradise-catalog-v4');
    root.classList.remove('prc-front', 'prc-products', 'prc-other');
    root.dataset.paradiseCatalog = 'v4';

    add(root.querySelector('.nitro-card-header, .nitro-catalog-header'), 'prc-header');
    add(getTopNav(root), 'prc-topnav');

    root.querySelectorAll('input[type="text"],input[type="search"]').forEach((input) => {
      add(input, 'prc-search-input');
      add(input.parentElement, 'prc-search');
    });

    root.querySelectorAll('.nitro-catalog-grid-item').forEach((node) => add(node, 'prc-item'));
    root.querySelectorAll('button').forEach((button) => {
      if (/acheter|buy|purchase/i.test(text(button))) add(button, 'prc-buy-button');
    });

    const grid = getGrid(root);
    const purchase = getPurchase(root);
    const categories = getCategoryPanel(root);

    if (grid && purchase) {
      root.classList.add('prc-products');
      add(grid, 'prc-grid');
      add(purchase, 'prc-purchase');
      add(categories, 'prc-category-panel');
      return;
    }

    const hasPromoImages = root.querySelectorAll('img').length >= 2;
    if (hasPromoImages) root.classList.add('prc-front');
    else root.classList.add('prc-other');
  };

  const scan = () => document.querySelectorAll(ROOT).forEach(decorate);
  const schedule = () => {
    if (queued) return;
    queued = true;
    requestAnimationFrame(() => { queued = false; scan(); });
  };

  const boot = () => {
    scan();
    new MutationObserver(schedule).observe(document.body, { childList: true, subtree: true });
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
