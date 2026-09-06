(function(){
  'use strict';

  function norm(value){
    return String(value || '').replace(/\s+/g, ' ').trim().toUpperCase();
  }

  function paint(el, color, weight){
    if(!el || !el.style) return;
    var c = color || '#ffffff';
    el.style.setProperty('color', c, 'important');
    el.style.setProperty('-webkit-text-fill-color', c, 'important');
    el.style.setProperty('opacity', '1', 'important');
    el.style.setProperty('visibility', 'visible', 'important');
    el.style.setProperty('filter', 'none', 'important');
    el.style.setProperty('mix-blend-mode', 'normal', 'important');
    el.style.setProperty('text-shadow', '0 1px 2px rgba(0,0,0,.9)', 'important');
    if(weight) el.style.setProperty('font-weight', weight, 'important');
  }

  function paintTree(root, color){
    if(!root) return;
    paint(root, color, '800');
    var nodes = root.querySelectorAll ? root.querySelectorAll('*') : [];
    for(var i = 0; i < nodes.length; i++) paint(nodes[i], color, '800');
  }

  function forcePhotoSwipeCaption(){
    /* PhotoSwipe peut déplacer/dupliquer son UI hors de #phone selon l'état de la galerie.
       On cible donc directement les classes réelles, partout dans le document. */
    var bars = document.querySelectorAll('.pswp__caption');
    for(var i = 0; i < bars.length; i++){
      bars[i].style.setProperty('background', '#0b1724', 'important');
      bars[i].style.setProperty('background-color', '#0b1724', 'important');
      bars[i].style.setProperty('opacity', '1', 'important');
      paintTree(bars[i], '#ffffff');
    }

    var centers = document.querySelectorAll('.pswp__caption__center');
    for(var j = 0; j < centers.length; j++) paintTree(centers[j], '#ffffff');

    /* Filet de sécurité : on cherche les vrais nœuds texte affichés "PRISE LE" et date/heure,
       même si le HTML reçu du serveur n'utilise pas les classes PhotoSwipe attendues. */
    var root = document.body || document.documentElement;
    if(!root) return;
    var walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT, null, false);
    var node;
    while((node = walker.nextNode())){
      var text = norm(node.nodeValue);
      if(!text) continue;

      if(text.indexOf('PRISE LE') !== -1){
        var el = node.parentElement;
        paint(el, '#e8f1fb', '800');
        if(el && el.parentElement) paintTree(el.parentElement, '#ffffff');
      }

      if(/\b\d{2}\/\d{2}\/\d{4}\b/.test(text) || /\b\d{1,2}:\d{2}\b/.test(text)){
        var p = node.parentElement;
        var pText = p ? norm(p.textContent) : '';
        var ppText = p && p.parentElement ? norm(p.parentElement.textContent) : '';
        if(pText.indexOf('PRISE LE') !== -1 || ppText.indexOf('PRISE LE') !== -1){
          paint(p, '#ffffff', '800');
          if(p && p.parentElement) paintTree(p.parentElement, '#ffffff');
        }
      }
    }
  }

  function runBurst(){
    forcePhotoSwipeCaption();
    setTimeout(forcePhotoSwipeCaption, 0);
    setTimeout(forcePhotoSwipeCaption, 25);
    setTimeout(forcePhotoSwipeCaption, 75);
    setTimeout(forcePhotoSwipeCaption, 150);
    setTimeout(forcePhotoSwipeCaption, 350);
    setTimeout(forcePhotoSwipeCaption, 800);
  }

  document.addEventListener('click', runBurst, true);
  document.addEventListener('transitionend', runBurst, true);
  window.addEventListener('load', runBurst);

  var observer = new MutationObserver(forcePhotoSwipeCaption);
  observer.observe(document.documentElement, {
    subtree: true,
    childList: true,
    characterData: true,
    attributes: true,
    attributeFilter: ['class','style']
  });

  setInterval(forcePhotoSwipeCaption, 150);
  runBurst();
})();
