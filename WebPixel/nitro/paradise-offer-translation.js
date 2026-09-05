(()=>{
  'use strict';

  const isRestaurantOfferWindow = element => {
    if (!(element instanceof Element)) return false;
    const text = String(element.textContent || '');
    return text.includes('Prise de commande -');
  };

  const improveRestaurantPopupSpacing = windowRoot => {
    if (!(windowRoot instanceof Element)) return;

    const all = [windowRoot, ...windowRoot.querySelectorAll('*')];
    const leaves = all.filter(node => node.children.length === 0);
    const yesLabel = leaves.find(node => String(node.textContent || '').trim() === 'Oui');
    const noLabel = leaves.find(node => String(node.textContent || '').trim() === 'Non');
    const yesButton = yesLabel?.closest('button');
    const noButton = noLabel?.closest('button');

    if (!yesButton || !noButton) return;

    // Espace réel sous les boutons.
    const buttonsRow = yesButton.parentElement;
    if (buttonsRow) {
      buttonsRow.style.setProperty('margin-bottom', '22px', 'important');
      buttonsRow.style.setProperty('padding-bottom', '8px', 'important');
    }

    // Retrouve le vrai panneau Nitro (et pas seulement le contenu interne)
    // puis agrandit réellement sa hauteur vers le bas.
    let current = buttonsRow;
    let popup = null;

    while (current && current !== document.body) {
      const rect = current.getBoundingClientRect();
      const text = String(current.textContent || '');

      if (
        text.includes('Prise de commande -') &&
        rect.width >= 260 && rect.width <= 430 &&
        rect.height >= 90 && rect.height <= 230
      ) {
        popup = current;
      }

      current = current.parentElement;
    }

    if (popup) {
      const rect = popup.getBoundingClientRect();
      popup.style.setProperty('height', `${Math.ceil(rect.height + 34)}px`, 'important');
      popup.style.setProperty('min-height', `${Math.ceil(rect.height + 34)}px`, 'important');
      popup.style.setProperty('max-height', 'none', 'important');
      popup.style.setProperty('padding-bottom', '18px', 'important');
      popup.style.setProperty('overflow', 'visible', 'important');
      popup.style.setProperty('box-sizing', 'border-box', 'important');
    }

    // Sécurité : si le parent direct du contenu fixe lui aussi la hauteur,
    // on lui laisse de la place pour ne plus couper le bas arrondi.
    let parent = popup?.parentElement;
    if (parent && parent !== document.body) {
      const rect = parent.getBoundingClientRect();
      if (rect.width >= 260 && rect.width <= 450 && rect.height <= 260) {
        parent.style.setProperty('height', 'auto', 'important');
        parent.style.setProperty('min-height', `${Math.ceil(rect.height + 30)}px`, 'important');
        parent.style.setProperty('overflow', 'visible', 'important');
      }
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

    requestAnimationFrame(() => {
      improveRestaurantPopupSpacing(windowRoot);
      setTimeout(() => improveRestaurantPopupSpacing(windowRoot), 60);
    });
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
