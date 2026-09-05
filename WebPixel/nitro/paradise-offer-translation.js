(()=>{
  'use strict';

  const isRestaurantOfferWindow = element => {
    if (!(element instanceof Element)) return false;
    const text = String(element.textContent || '');
    return text.includes('Prise de commande -');
  };

  const translateWindow = root => {
    if (!(root instanceof Element)) return;

    const candidates = [root, ...root.querySelectorAll('*')];
    let windowRoot = null;

    for (const element of candidates) {
      if (isRestaurantOfferWindow(element)) {
        windowRoot = element;
        break;
      }
    }

    if (!windowRoot) return;

    const nodes = [windowRoot, ...windowRoot.querySelectorAll('*')];
    for (const node of nodes) {
      if (node.children.length > 0) continue;
      const value = String(node.textContent || '').trim();

      if (value === 'offer.accept') {
        node.textContent = 'Oui';
      } else if (value === 'Fermer') {
        node.textContent = 'Non';
      } else if (value === 'notifications.rpoffer') {
        node.textContent = 'Accepter cette prise de commande ?';
      }
    }
  };

  const inspect = node => {
    const element = node instanceof Element ? node : node?.parentElement;
    if (!element) return;
    translateWindow(element);
    element.querySelectorAll?.('*').forEach(child => {
      if (String(child.textContent || '').includes('Prise de commande -')) translateWindow(child);
    });
  };

  new MutationObserver(mutations => {
    for (const mutation of mutations) {
      inspect(mutation.target);
      mutation.addedNodes.forEach(inspect);
    }
  }).observe(document.body, { childList: true, subtree: true, characterData: true });

  inspect(document.body);
})();
