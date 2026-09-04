(() => {
  'use strict';

  const ROOT = '.nitro-catalog.paradise-catalog-v22';

  function restoreIcons(root)
  {
    root.querySelectorAll('.layout-grid-item,[class*="catalog-grid-item"]').forEach(item =>
    {
      if(!(item instanceof HTMLElement)) return;

      const inlineImage = item.style.backgroundImage;
      if(inlineImage && inlineImage !== 'none')
      {
        item.style.setProperty('--prc23-native-item-image', inlineImage);
        item.classList.add('prc23-has-native-image');
      }

      const unique = item.querySelector('.unique-bg-override');
      if(unique instanceof HTMLElement)
      {
        const uniqueImage = unique.style.backgroundImage;
        if(uniqueImage && uniqueImage !== 'none') unique.style.setProperty('--prc23-native-item-image', uniqueImage);
      }
    });
  }

  let queued = false;
  function refresh()
  {
    if(queued) return;
    queued = true;
    requestAnimationFrame(() =>
    {
      queued = false;
      document.querySelectorAll(ROOT).forEach(restoreIcons);
    });
  }

  function boot()
  {
    refresh();
    [100, 300, 700, 1400].forEach(delay => setTimeout(refresh, delay));

    new MutationObserver(refresh).observe(document.body, {
      childList: true,
      subtree: true,
      attributes: true,
      attributeFilter: [ 'style', 'class' ]
    });

    console.info('[ParadiseRP] catalogue native item icons restored');
  }

  document.readyState === 'loading'
    ? document.addEventListener('DOMContentLoaded', boot, { once: true })
    : boot();
})();
