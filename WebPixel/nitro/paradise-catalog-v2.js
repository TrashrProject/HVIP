(() => {
  'use strict';

  const ROOT_SELECTOR = '.nitro-catalog';
  const SUCCESS_RE = /(?:achat|acheté|achetée|purchased|purchase)\s*(?:réussi|effectué|success|successful)?/i;
  let scheduled = false;

  const markAll = (root, selectors, className) => {
    selectors.forEach((selector) => {
      root.querySelectorAll(selector).forEach((node) => node.classList.add(className));
    });
  };

  const decorate = (root) => {
    if (!(root instanceof HTMLElement)) return;
    root.classList.add('paradise-catalog-v2');
    root.setAttribute('data-paradise-catalog', 'v2');

    markAll(root, [
      '.nitro-catalog-header',
      '[class*="catalog-header"]'
    ], 'prc-header');

    markAll(root, [
      '.nitro-catalog-navigation',
      '.nitro-catalog-navigation-grid-container',
      '[class*="catalog-navigation"]'
    ], 'prc-navigation');

    markAll(root, [
      '.nitro-catalog-page-navigation',
      '[class*="page-navigation"]'
    ], 'prc-subnavigation');

    markAll(root, [
      '.nitro-catalog-grid',
      '[class*="catalog-grid"]'
    ], 'prc-grid');

    markAll(root, [
      '.nitro-catalog-grid-item',
      '[class*="catalog-grid-item"]'
    ], 'prc-item');

    markAll(root, [
      '.nitro-catalog-layout',
      '[class*="catalog-layout"]'
    ], 'prc-layout');

    markAll(root, [
      '.nitro-catalog-purchase-component',
      '[class*="catalog-purchase"]',
      '[class*="purchase-component"]'
    ], 'prc-purchase');

    root.querySelectorAll('input[type="text"], input[type="search"]').forEach((input) => {
      const hint = `${input.placeholder || ''} ${input.getAttribute('aria-label') || ''}`;
      if (/recher|search|furni|catalog/i.test(hint) || input.closest('[class*="catalog"]')) {
        input.classList.add('prc-search-input');
        const wrapper = input.parentElement;
        if (wrapper) wrapper.classList.add('prc-search');
      }
    });

    root.querySelectorAll('button').forEach((button) => {
      const text = (button.textContent || '').trim();
      if (/acheter|buy|purchase/i.test(text)) button.classList.add('prc-buy-button');
    });
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
    const text = (node.textContent || '').trim();
    if (!text || !SUCCESS_RE.test(text)) return;
    const root = document.querySelector(ROOT_SELECTOR);
    if (!root) return;
    root.classList.remove('prc-purchase-confirmed');
    void root.offsetWidth;
    root.classList.add('prc-purchase-confirmed');
    window.setTimeout(() => root.classList.remove('prc-purchase-confirmed'), 900);
  };

  const observer = new MutationObserver((mutations) => {
    scheduleScan();
    mutations.forEach((mutation) => mutation.addedNodes.forEach((node) => showConfirmedPurchase(node)));
  });

  const boot = () => {
    scan();
    observer.observe(document.body, { childList: true, subtree: true });
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
