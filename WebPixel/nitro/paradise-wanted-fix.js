(() => {
  const replacements = new Map([
    ['wantedTitle', 'Personnes recherchées'],
    ['Wanted Title', 'Personnes recherchées'],
    ['Wanted', 'Personnes recherchées']
  ]);

  const fixText = root => {
    if (!root) return;
    const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
    let node;
    while ((node = walker.nextNode())) {
      const value = (node.nodeValue || '').trim();
      if (replacements.has(value)) node.nodeValue = node.nodeValue.replace(value, replacements.get(value));
    }
  };

  const run = () => fixText(document.body);

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', run, { once: true });
  } else {
    run();
  }

  const observer = new MutationObserver(records => {
    for (const record of records) {
      for (const node of record.addedNodes) {
        if (node.nodeType === Node.TEXT_NODE) {
          const value = (node.nodeValue || '').trim();
          if (replacements.has(value)) node.nodeValue = node.nodeValue.replace(value, replacements.get(value));
        } else if (node.nodeType === Node.ELEMENT_NODE) {
          fixText(node);
        }
      }
    }
  });

  const startObserver = () => {
    if (document.body) observer.observe(document.body, { childList: true, subtree: true });
  };
  if (document.body) startObserver();
  else document.addEventListener('DOMContentLoaded', startObserver, { once: true });
})();
