(() => {
  'use strict';

  const ROOT = '.nitro-catalog';
  const text = node => (node?.textContent || '').replace(/\s+/g, ' ').trim();
  const add = (node, cls) => { if (node instanceof HTMLElement) node.classList.add(cls); return node; };
  const translations = new Map([
    ['Front Page', 'Accueil'], ['Furni', 'Mobilier'], ['Furniture', 'Mobilier'],
    ['Clothing', 'Vêtements'], ['Pets', 'Animaux'], ['Building', 'Construction'],
    ['Buy', 'Acheter'], ['Purchase', 'Acheter']
  ]);

  function findNativeHeader(root) {
    return root.querySelector(':scope > .nitro-card-header, :scope > [class*="card-header"], .nitro-catalog-header');
  }

  function findNativeClose(root) {
    const header = findNativeHeader(root);
    if (!header) return null;
    return [...header.querySelectorAll('button,[role="button"],.close,[class*="close"],[class*="cross"]')]
      .find(node => !node.classList.contains('prc-close')) || null;
  }

  function closeCatalog(root) {
    const native = findNativeClose(root);
    if (native) {
      native.click();
      return;
    }
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', code: 'Escape', bubbles: true }));
  }

  function ensureBanner(root) {
    let banner = root.querySelector(':scope > .prc-brand-banner');
    if (!banner) {
      banner = document.createElement('div');
      banner.className = 'prc-brand-banner';
      banner.innerHTML = '<div class="prc-brand-left"><span class="prc-brand-mark"><img src="/Dynamics/img/logos/hv_logo_p.png" alt="ParadiseRP" draggable="false"></span><span class="prc-brand-copy"><strong>CATALOGUE</strong><em>ParadiseRP</em></span></div><div class="prc-brand-tagline">Des milliers de furnis<br>pour rendre votre ville unique !</div><button type="button" class="prc-close" aria-label="Fermer" title="Fermer">×</button>';
      const content = root.querySelector(':scope > .nitro-card-content');
      content ? root.insertBefore(banner, content) : root.prepend(banner);
    }
    const close = banner.querySelector('.prc-close');
    if (close && close.dataset.ready !== '1') {
      close.dataset.ready = '1';
      close.addEventListener('pointerdown', event => event.stopPropagation());
      close.addEventListener('click', event => {
        event.preventDefault();
        event.stopPropagation();
        closeCatalog(root);
      });
    }
  }

  function translate(root) {
    root.querySelectorAll('button,a,span,p,label').forEach(node => {
      if (node.children.length) return;
      const value = text(node);
      const suffix = value.match(/\s*\(\d+\)\s*$/)?.[0] || '';
      const key = value.replace(/\s*\(\d+\)\s*$/, '');
      if (translations.has(key)) node.textContent = translations.get(key) + suffix;
    });
    root.querySelectorAll('input[type="text"],input[type="search"]').forEach(input => {
      if (/search|recherch/i.test(input.placeholder || '')) input.placeholder = 'Rechercher un furni...';
      add(input, 'prc-search-input');
      add(input.parentElement, 'prc-search');
    });
  }

  function nav(root) {
    return root.querySelector('.nitro-catalog-navigation,[class*="catalog-navigation"]') ||
      [...root.querySelectorAll('nav,ul,div')].find(node => {
        const t = text(node);
        return t && t.length < 260 && [/Accueil|Front Page/i,/Furni|Mobilier/i,/Vêtements|Clothing/i,/Animaux|Pets/i,/Construction|Building/i].filter(r => r.test(t)).length >= 3;
      }) || null;
  }

  function grid(root) {
    return root.querySelector('.nitro-catalog-grid') ||
      [...root.querySelectorAll('[class*="catalog-grid"]')].find(node => !/grid-item/.test(String(node.className)) && node.children.length >= 3) || null;
  }

  function purchase(root) {
    return root.querySelector('.nitro-catalog-purchase-component,[class*="catalog-purchase"],[class*="purchase-component"]') || null;
  }

  function category(root, g) {
    const direct = root.querySelector('.nitro-catalog-navigation-grid,[class*="navigation-grid"]');
    return direct && direct !== g ? direct : null;
  }

  function commonAncestor(a, b, c, root) {
    let node = a?.parentElement;
    while (node && node !== root) {
      if (node.contains(b) && node.contains(c)) return node;
      node = node.parentElement;
    }
    return null;
  }

  function directBranch(ancestor, node) {
    if (!ancestor || !node) return null;
    let current = node;
    while (current.parentElement && current.parentElement !== ancestor) current = current.parentElement;
    return current.parentElement === ancestor ? current : null;
  }

  function decorate(root) {
    if (!(root instanceof HTMLElement)) return;
    if (root.dataset.paradiseDecorated === '1') return;
    root.dataset.paradiseDecorated = '1';

    /* Keep the structural classes used by the stable base CSS, but no recurring rescans. */
    root.classList.add('paradise-catalog-v7', 'paradise-catalog-v9', 'paradise-catalog-v18');
    ensureBanner(root);
    add(findNativeHeader(root), 'prc-native-header');
    add(nav(root), 'prc-topnav');
    translate(root);

    const g = grid(root);
    const p = purchase(root);
    const c = category(root, g);
    add(g, 'prc-grid');
    add(p, 'prc-purchase');
    add(c, 'prc-category-panel');

    root.querySelectorAll('.nitro-catalog-grid-item,[class*="catalog-grid-item"]').forEach(item => add(item, 'prc-item'));
    root.querySelectorAll('button').forEach(button => {
      if (/acheter|buy|purchase|offrir/i.test(text(button))) add(button, 'prc-buy-button');
    });

    const isProduct = !!(g && p && c);
    root.classList.toggle('prc-products', isProduct);
    root.classList.toggle('prc-info-page', !isProduct);

    if (isProduct) {
      const ancestor = commonAncestor(c, g, p, root);
      const bc = directBranch(ancestor, c);
      const bg = directBranch(ancestor, g);
      const bp = directBranch(ancestor, p);
      if (ancestor && bc && bg && bp && new Set([bc, bg, bp]).size === 3) {
        add(ancestor, 'prc-product-layout');
        add(bc, 'prc-col-categories');
        add(bg, 'prc-col-products');
        add(bp, 'prc-col-preview');
      }
    }
  }

  function decorateWhenReady(root) {
    decorate(root);
    /* One delayed pass only, for Nitro elements mounted just after the window. */
    window.setTimeout(() => {
      if (!root.isConnected) return;
      root.dataset.paradiseDecorated = '';
      decorate(root);
    }, 180);
  }

  function boot() {
    document.querySelectorAll(ROOT).forEach(decorateWhenReady);
    new MutationObserver(mutations => {
      for (const mutation of mutations) {
        for (const node of mutation.addedNodes) {
          if (!(node instanceof Element)) continue;
          if (node.matches(ROOT)) decorateWhenReady(node);
          node.querySelectorAll?.(ROOT).forEach(decorateWhenReady);
        }
      }
    }).observe(document.body, { childList: true, subtree: true });
  }

  document.readyState === 'loading'
    ? document.addEventListener('DOMContentLoaded', boot, { once: true })
    : boot();
})();
