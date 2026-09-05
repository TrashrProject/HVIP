(()=>{
  'use strict';

  const isRestaurantOfferWindow = element => {
    if (!(element instanceof Element)) return false;
    const text = String(element.textContent || '');
    return text.includes('Prise de commande -');
  };

  const improveRestaurantPopupSpacing = windowRoot => {
    if (!(windowRoot instanceof Element)) return;

    const leaves = [windowRoot, ...windowRoot.querySelectorAll('*')].filter(node => node.children.length === 0);
    const yesLabel = leaves.find(node => String(node.textContent || '').trim() === 'Oui');
    const noLabel = leaves.find(node => String(node.textContent || '').trim() === 'Non');
    const yesButton = yesLabel?.closest('button');
    const noButton = noLabel?.closest('button');

    if (!yesButton || !noButton) return;

    const buttonsRow = yesButton.parentElement;
    if (buttonsRow) {
      buttonsRow.style.setProperty('margin-bottom', '10px', 'important');
      buttonsRow.style.setProperty('padding-bottom', '2px', 'important');
    }

    let current = buttonsRow;
    while (current && current !== document.body) {
      const text = String(current.textContent || '');
      const rect = current.getBoundingClientRect();
      if (text.includes('Prise de commande -') && rect.width >= 250 && rect.width <= 500 && rect.height <= 260) {
        current.style.setProperty('height', 'auto', 'important');
        current.style.setProperty('min-height', `${Math.ceil(rect.height + 14)}px`, 'important');
        current.style.setProperty('padding-bottom', '12px', 'important');
        current.style.setProperty('overflow', 'visible', 'important');
        break;
      }
      current = current.parentElement;
    }
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

    requestAnimationFrame(() => improveRestaurantPopupSpacing(windowRoot));
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
