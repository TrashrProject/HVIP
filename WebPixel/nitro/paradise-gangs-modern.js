(() => {
  const translations = new Map([
    ['Delete Gang', 'Supprimer'],
    ['Leave Gang', 'Quitter'],
    ['KILLS', 'ÉLIMINATIONS'],
    ['COP KILLS', 'POLICIERS'],
    ['HEISTS', 'BRAQUAGES'],
    ['JAILBREAKS', 'ÉVASIONS'],
    ['TURFS', 'TERRITOIRES'],
    ['EARNED', 'GAINS'],
    ['Preview', 'Aperçu'],
    ['Save', 'Enregistrer'],
    ['Edit', 'Modifier'],
    ['Promote', 'Promouvoir'],
    ['Demote', 'Rétrograder'],
    ['Kick', 'Exclure'],
    ['No members.', 'Aucun membre.'],
    ['Members', 'Membres']
  ]);

  const localize = root => {
    const card = root.matches?.('.gang-card') ? root : root.querySelector?.('.gang-card');
    if (!card) return;
    const walker = document.createTreeWalker(card, NodeFilter.SHOW_TEXT);
    let node;
    while ((node = walker.nextNode())) {
      const value = node.nodeValue.trim();
      const translated = translations.get(value);
      if (translated) node.nodeValue = node.nodeValue.replace(value, translated);
    }
    card.querySelectorAll('[title]').forEach(element => {
      const translated = translations.get(element.title);
      if (translated) element.title = translated;
    });
  };

  const observer = new MutationObserver(mutations => {
    for (const mutation of mutations) {
      if (mutation.target.nodeType === Node.ELEMENT_NODE) localize(mutation.target);
      for (const node of mutation.addedNodes) {
        if (node.nodeType === Node.ELEMENT_NODE) localize(node);
      }
    }
  });

  observer.observe(document.documentElement, { childList: true, subtree: true });
  localize(document.documentElement);
})();
