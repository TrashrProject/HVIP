(() => {
  'use strict';

  const ROOT_SELECTOR = '.nitro-catalog';
  const SUCCESS_RE = /(?:achat|acheté|achetée|purchased|purchase)\s*(?:réussi|effectué|success|successful)?/i;
  let scheduled = false;

  const text = (node) => (node && node.textContent ? node.textContent.replace(/\s+/g, ' ').trim() : '');

  const addClass = (node, className) => {
    if (node instanceof HTMLElement) node.classList.add(className);
    return node;
  };

  const markAll = (root, selectors, className) => {
    selectors.forEach((selector) => root.querySelectorAll(selector).forEach((node) => addClass(node, className)));
  };

  const findByText = (root, regex) => {
    const nodes = root.querySelectorAll('button, div, span, p, h1, h2, h3, h4, li');
    for (const node of nodes) {
      const value = text(node);
      if (value && value.length < 90 && regex.test(value)) return node;
    }
    return null;
  };

  const climbPanel = (node, root, predicate) => {
    let current = node;
    for (let i = 0; current && current !== root && i < 7; i += 1, current = current.parentElement) {
      if (current instanceof HTMLElement && predicate(current)) return current;
    }
    return null;
  };

  const findCategoryPanel = (root) => {
    const title = findByText(root, /^(cat[eé]gories|categories)$/i);
    if (!title) return null;
    return climbPanel(title, root, (node) => {
      const value = text(node);
      return node.children.length >= 2 && value.length > 40 && value.length < 4000;
    }) || title.parentElement;
  };

  const findTopNav = (root) => {
    const labels = [/front page/i, /furni/i, /clothing/i, /pets/i, /building/i, /staff/i];
    const candidates = [...root.querySelectorAll('div, nav, ul')];
    let best = null;
    let bestScore = 0;
    for (const node of candidates) {
      const value = text(node);
      if (!value || value.length > 300) continue;
      const score = labels.reduce((sum, re) => sum + (re.test(value) ? 1 : 0), 0);
      if (score >= 4 && score > bestScore) {
        best = node;
        bestScore = score;
      }
    }
    return best;
  };

  const findGrid = (root) => {
    const selectors = [
      '.nitro-catalog-grid',
      '[class*="catalog-grid"]:not([class*="grid-item"])'
    ];
    for (const selector of selectors) {
      const nodes = [...root.querySelectorAll(selector)];
      const node = nodes.find((candidate) => candidate.querySelectorAll('img').length >= 3 || candidate.children.length >= 4);
      if (node) return node;
    }
    return null;
  };

  const findPurchase = (root) => {
    const direct = root.querySelector('.nitro-catalog-purchase-component, [class*="catalog-purchase"], [class*="purchase-component"]');
    if (direct) return direct;
    const buy = [...root.querySelectorAll('button')].find((button) => /acheter|buy|purchase/i.test(text(button)));
    if (!buy) return null;
    return climbPanel(buy, root, (node) => node.querySelectorAll('button').length <= 5 && text(node).length < 1000) || buy.parentElement;
  };

  const decorateHeader = (root) => {
    const header = root.querySelector('.nitro-card-header, .nitro-catalog-header, [class*="catalog-header"]');
    if (!header) return;
    addClass(header, 'prc-header');
    if (!header.querySelector('.prc-brand-copy')) {
      const brand = document.createElement('span');
      brand.className = 'prc-brand-copy';
      brand.innerHTML = '<strong>CATALOGUE</strong><small>ParadiseRP</small>';
      header.appendChild(brand);
    }
    if (!header.querySelector('.prc-slogan')) {
      const slogan = document.createElement('span');
      slogan.className = 'prc-slogan';
      slogan.textContent = 'Des milliers de furnis pour rendre votre ville unique !';
      header.appendChild(slogan);
    }
  };

  const decorate = (root) => {
    if (!(root instanceof HTMLElement)) return;
    root.classList.add('paradise-catalog-v3');
    root.setAttribute('data-paradise-catalog', 'v3');

    decorateHeader(root);

    const topNav = findTopNav(root);
    addClass(topNav, 'prc-topnav');

    const categoryPanel = findCategoryPanel(root);
    addClass(categoryPanel, 'prc-category-panel');

    const grid = findGrid(root);
    addClass(grid, 'prc-grid');

    const purchase = findPurchase(root);
    addClass(purchase, 'prc-purchase');

    markAll(root, ['.nitro-catalog-grid-item', '[class*="catalog-grid-item"]'], 'prc-item');

    root.querySelectorAll('input[type="text"], input[type="search"]').forEach((input) => {
      const hint = `${input.placeholder || ''} ${input.getAttribute('aria-label') || ''}`;
      if (/recher|search|furni|catalog/i.test(hint) || input.closest('[class*="catalog"]')) {
        addClass(input, 'prc-search-input');
        addClass(input.parentElement, 'prc-search');
      }
    });

    root.querySelectorAll('button').forEach((button) => {
      const value = text(button);
      if (/acheter|buy|purchase/i.test(value)) addClass(button, 'prc-buy-button');
      if (/staff/i.test(value)) addClass(button, 'prc-staff-tab');
    });

    if (categoryPanel && !categoryPanel.querySelector('.prc-category-label')) {
      const label = document.createElement('div');
      label.className = 'prc-category-label';
      label.textContent = 'CATÉGORIES';
      categoryPanel.prepend(label);
    }
  };

  const scan = () => document.querySelectorAll(ROOT_SELECTOR).forEach(decorate);

  const scheduleScan = () => {
    if (scheduled) return;
    scheduled = true;
    requestAnimationFrame(() => {
      scheduled = false;
      scan();
    });
  };

  const showConfirmedPurchase = (node) => {
    if (!(node instanceof Element)) return;
    const value = text(node);
    if (!value || !SUCCESS_RE.test(value)) return;
    const root = document.querySelector(ROOT_SELECTOR);
    if (!root) return;
    root.classList.remove('prc-purchase-confirmed');
    void root.offsetWidth;
    root.classList.add('prc-purchase-confirmed');
    window.setTimeout(() => root.classList.remove('prc-purchase-confirmed'), 900);
  };

  const observer = new MutationObserver((mutations) => {
    scheduleScan();
    mutations.forEach((mutation) => mutation.addedNodes.forEach(showConfirmedPurchase));
  });

  const boot = () => {
    scan();
    observer.observe(document.body, { childList: true, subtree: true });
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
