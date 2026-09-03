(() => {
  'use strict';

  const ROOT = '.nitro-catalog';
  const VERSION = 'v6';
  let queued = false;

  const exactTranslations = new Map([
    ['Front Page', 'Accueil'],
    ['Furni', 'Mobilier'],
    ['Furniture', 'Mobilier'],
    ['Clothing', 'Vêtements'],
    ['Pets', 'Animaux'],
    ['Building', 'Construction'],
    ['Clothes Shop', 'Boutique de vêtements'],
    ['Redeem a voucher code here:', 'Utiliser un code promotionnel'],
    ['Redeem a voucher code here', 'Utiliser un code promotionnel'],
    ['Search', 'Rechercher'],
    ['Buy', 'Acheter'],
    ['Purchase', 'Acheter']
  ]);

  const text = (node) => (node?.textContent || '').replace(/\s+/g, ' ').trim();
  const add = (node, name) => { if (node instanceof HTMLElement) node.classList.add(name); return node; };

  const translateLeaf = (node) => {
    if (!(node instanceof HTMLElement) || node.children.length) return;
    const current = text(node);
    if (!current) return;

    const clean = current.replace(/\s*\(\d+\)\s*$/, '');
    const translated = exactTranslations.get(clean);
    if (!translated || node.dataset.prcTranslation === translated) return;

    const suffix = current.match(/\s*\(\d+\)\s*$/)?.[0] || '';
    node.dataset.prcOriginalText = current;
    node.dataset.prcTranslation = translated;
    node.textContent = `${translated}${suffix}`;
  };

  const translateVisibleText = (root) => {
    root.querySelectorAll('button, a, span, div, p, label').forEach((node) => {
      if (node.children.length === 0) translateLeaf(node);
    });

    root.querySelectorAll('input').forEach((input) => {
      const placeholder = (input.getAttribute('placeholder') || '').trim();
      if (/redeem a voucher code here:?/i.test(placeholder)) input.setAttribute('placeholder', 'Utiliser un code promotionnel');
      else if (/search|recherch/i.test(placeholder)) input.setAttribute('placeholder', 'Rechercher un furni...');
    });
  };

  const ensureBrandBanner = (root) => {
    let banner = root.querySelector(':scope > .prc-brand-banner');
    if (banner) return banner;

    banner = document.createElement('div');
    banner.className = 'prc-brand-banner';
    banner.setAttribute('aria-hidden', 'true');
    banner.innerHTML = `
      <div class="prc-brand-left">
        <span class="prc-brand-mark">▥</span>
        <span class="prc-brand-copy"><strong>CATALOGUE</strong><em>ParadiseRP</em></span>
      </div>
      <div class="prc-brand-tagline">Des milliers de furnis<br>pour rendre votre ville unique !</div>
      <div class="prc-brand-glow"></div>`;

    const content = root.querySelector(':scope > .nitro-card-content');
    if (content) root.insertBefore(banner, content);
    else root.prepend(banner);
    return banner;
  };

  const getTopNav = (root) => {
    const direct = root.querySelector('.nitro-catalog-navigation, [class*="catalog-navigation"], [class*="catalog-tabs"]');
    if (direct) return direct;

    const candidates = [...root.querySelectorAll('div, nav, ul')];
    let best = null;
    let score = 0;
    const labels = [/Front Page|Accueil/i, /Furni|Mobilier/i, /Clothing|Vêtements/i, /Pets|Animaux/i, /Building|Construction/i, /Staff/i];

    for (const node of candidates) {
      const value = text(node);
      if (!value || value.length > 320) continue;
      const current = labels.reduce((sum, re) => sum + (re.test(value) ? 1 : 0), 0);
      if (current >= 3 && current > score) {
        best = node;
        score = current;
      }
    }
    return best;
  };

  const getCategoryPanel = (root) => {
    const direct = root.querySelector('.nitro-catalog-navigation-grid, [class*="catalog-navigation-grid"], [class*="catalog-category"]');
    if (direct && direct.querySelectorAll('button,[role="button"],.list-group-item').length >= 2) return direct;

    const labels = [...root.querySelectorAll('div,span,p,h1,h2,h3,h4')];
    const label = labels.find((node) => /^(cat[eé]gories|categories)$/i.test(text(node)));
    if (label) {
      let node = label.parentElement;
      for (let i = 0; node && node !== root && i < 6; i += 1, node = node.parentElement) {
        const buttons = node.querySelectorAll('button,[role="button"],.list-group-item').length;
        if (buttons >= 2 || (node.children.length >= 3 && node.scrollHeight > 120)) return node;
      }
    }

    const grid = getGrid(root);
    if (!grid) return null;
    const candidates = [...root.querySelectorAll('div, ul')]
      .filter((node) => node !== grid && !node.contains(grid))
      .filter((node) => node.querySelectorAll('button,[role="button"],.list-group-item').length >= 3);
    return candidates.find((node) => node.getBoundingClientRect().left < grid.getBoundingClientRect().left) || null;
  };

  const getGrid = (root) => {
    const direct = root.querySelector('.nitro-catalog-grid');
    if (direct && direct.children.length >= 2) return direct;

    return [...root.querySelectorAll('[class*="catalog-grid"]')]
      .find((node) => !String(node.className).includes('grid-item') && node.children.length >= 3) || null;
  };

  const getPurchase = (root) => {
    const direct = root.querySelector('.nitro-catalog-purchase-component, [class*="catalog-purchase"], [class*="purchase-component"]');
    if (direct) return direct;

    const buy = [...root.querySelectorAll('button')].find((button) => /acheter|buy|purchase/i.test(text(button)));
    if (!buy) return null;

    let node = buy.parentElement;
    for (let i = 0; node && node !== root && i < 6; i += 1, node = node.parentElement) {
      const value = text(node);
      if (value.length > 20 && value.length < 1200) return node;
    }
    return buy.parentElement;
  };

  const getCommonAncestor = (nodes, root) => {
    const valid = nodes.filter(Boolean);
    if (valid.length < 2) return null;

    let current = valid[0].parentElement;
    while (current && current !== root) {
      if (valid.every((node) => current.contains(node))) return current;
      current = current.parentElement;
    }
    return null;
  };

  const directBranch = (ancestor, target) => {
    if (!ancestor || !target) return null;
    let node = target;
    while (node.parentElement && node.parentElement !== ancestor) node = node.parentElement;
    return node.parentElement === ancestor ? node : null;
  };

  const markProductLayout = (root, categories, grid, purchase) => {
    const common = getCommonAncestor([categories, grid, purchase], root);
    if (!common) return;

    const catBranch = directBranch(common, categories);
    const gridBranch = directBranch(common, grid);
    const purchaseBranch = directBranch(common, purchase);
    const branches = [catBranch, gridBranch, purchaseBranch].filter(Boolean);

    if (new Set(branches).size !== 3) return;

    add(common, 'prc-product-layout');
    add(catBranch, 'prc-col-categories');
    add(gridBranch, 'prc-col-products');
    add(purchaseBranch, 'prc-col-preview');
  };

  const decorateItems = (root) => {
    root.querySelectorAll('.nitro-catalog-grid-item,[class*="catalog-grid-item"]').forEach((node) => {
      add(node, 'prc-item');
      if (node.matches('.active,.selected,[aria-selected="true"]')) add(node, 'prc-item-selected');
      else node.classList.remove('prc-item-selected');
    });

    root.querySelectorAll('button').forEach((button) => {
      if (/acheter|buy|purchase/i.test(text(button))) add(button, 'prc-buy-button');
    });
  };

  const markPromoCards = (root) => {
    const images = [...root.querySelectorAll('img')].filter((img) => {
      const rect = img.getBoundingClientRect();
      return rect.width >= 180 || rect.height >= 100 || img.naturalWidth >= 180 || img.naturalHeight >= 100;
    });

    const cards = [];
    images.forEach((img) => {
      const card = img.closest('a,button') || img.parentElement;
      if (card && card !== root && !cards.includes(card)) {
        add(card, 'prc-promo-card');
        cards.push(card);
      }
    });

    if (cards.length >= 2) {
      const parent = cards[0].parentElement;
      if (parent && cards.every((card) => card.parentElement === parent)) add(parent, 'prc-promo-grid');
    }
  };

  const decorate = (root) => {
    if (!(root instanceof HTMLElement)) return;

    root.classList.add('paradise-catalog-v6');
    root.classList.remove('paradise-catalog-v4', 'paradise-catalog-v5', 'prc-front', 'prc-products', 'prc-other');
    root.dataset.paradiseCatalog = VERSION;

    ensureBrandBanner(root);
    add(root.querySelector(':scope > .nitro-card-header, .nitro-catalog-header'), 'prc-native-header');
    add(getTopNav(root), 'prc-topnav');
    translateVisibleText(root);

    root.querySelectorAll('input[type="text"],input[type="search"]').forEach((input) => {
      const hint = `${input.placeholder || ''} ${input.getAttribute('aria-label') || ''}`;
      if (/voucher|promotionnel/i.test(hint)) {
        add(input, 'prc-voucher-input');
        add(input.parentElement, 'prc-voucher');
      } else {
        add(input, 'prc-search-input');
        add(input.parentElement, 'prc-search');
      }
    });

    decorateItems(root);

    const grid = getGrid(root);
    const purchase = getPurchase(root);
    const categories = getCategoryPanel(root);

    if (grid && purchase) {
      root.classList.add('prc-products');
      add(grid, 'prc-grid');
      add(purchase, 'prc-purchase');
      add(categories, 'prc-category-panel');
      markProductLayout(root, categories, grid, purchase);
      return;
    }

    const hasPromoImages = root.querySelectorAll('img').length >= 2;
    if (hasPromoImages) {
      root.classList.add('prc-front');
      markPromoCards(root);
    } else root.classList.add('prc-other');
  };

  const scan = () => document.querySelectorAll(ROOT).forEach(decorate);
  const schedule = () => {
    if (queued) return;
    queued = true;
    requestAnimationFrame(() => { queued = false; scan(); });
  };

  const boot = () => {
    scan();
    new MutationObserver(schedule).observe(document.body, { childList: true, subtree: true, attributes: true, attributeFilter: ['class', 'aria-selected'] });
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
