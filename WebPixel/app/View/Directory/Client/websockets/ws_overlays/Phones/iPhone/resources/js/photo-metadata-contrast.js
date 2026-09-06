(function(){
  'use strict';

  function norm(value){
    return String(value || '').replace(/\s+/g, ' ').trim().toUpperCase();
  }

  function paint(el, color, weight){
    if(!el || !el.style) return;
    el.style.setProperty('color', color || '#ffffff', 'important');
    el.style.setProperty('-webkit-text-fill-color', color || '#ffffff', 'important');
    el.style.setProperty('opacity', '1', 'important');
    el.style.setProperty('visibility', 'visible', 'important');
    el.style.setProperty('filter', 'none', 'important');
    el.style.setProperty('text-shadow', '0 1px 2px rgba(0,0,0,.85)', 'important');
    if(weight) el.style.setProperty('font-weight', weight, 'important');
  }

  function forceNodeAndFamily(el, color){
    if(!el) return;
    paint(el, color, '800');

    var descendants = el.querySelectorAll ? el.querySelectorAll('*') : [];
    for(var i = 0; i < descendants.length; i++) paint(descendants[i], color, '800');

    if(el.parentElement){
      paint(el.parentElement, color, '800');
      var siblings = el.parentElement.children;
      for(var j = 0; j < siblings.length; j++){
        paint(siblings[j], color, '800');
        var nested = siblings[j].querySelectorAll ? siblings[j].querySelectorAll('*') : [];
        for(var k = 0; k < nested.length; k++) paint(nested[k], color, '800');
      }
    }
  }

  function forceMetadata(){
    var phone = document.getElementById('phone');
    if(!phone) return;

    var walker = document.createTreeWalker(phone, NodeFilter.SHOW_TEXT, null, false);
    var node;
    var found = false;

    while((node = walker.nextNode())){
      var text = norm(node.nodeValue);
      if(!text) continue;

      if(text.indexOf('PRISE LE') !== -1){
        found = true;
        forceNodeAndFamily(node.parentElement, '#e8f1fb');
      }

      if(/\b\d{2}\/\d{2}\/\d{4}\b/.test(text) || /\b\d{1,2}:\d{2}\b/.test(text)){
        var parentText = node.parentElement ? norm(node.parentElement.textContent) : '';
        var familyText = node.parentElement && node.parentElement.parentElement ? norm(node.parentElement.parentElement.textContent) : '';
        if(parentText.indexOf('PRISE LE') !== -1 || familyText.indexOf('PRISE LE') !== -1 || found){
          forceNodeAndFamily(node.parentElement, '#ffffff');
        }
      }
    }

    /* Filet de sécurité : le footer PhotoSwipe est toujours sombre dans ParadiseRP. */
    var captions = phone.querySelectorAll('#gallery .pswp__caption, #gallery .pswp__caption__center, #gallery .pswp__caption *, #app_Gallery .pswp__caption, #app_Gallery .pswp__caption__center, #app_Gallery .pswp__caption *');
    for(var c = 0; c < captions.length; c++) paint(captions[c], '#ffffff', '800');

    var captionBars = phone.querySelectorAll('#gallery .pswp__caption, #app_Gallery .pswp__caption');
    for(var b = 0; b < captionBars.length; b++){
      captionBars[b].style.setProperty('background', '#0b1724', 'important');
      captionBars[b].style.setProperty('background-color', '#0b1724', 'important');
      captionBars[b].style.setProperty('opacity', '1', 'important');
    }
  }

  var running = false;
  function run(){
    if(running) return;
    running = true;
    requestAnimationFrame(function(){
      running = false;
      forceMetadata();
    });
  }

  document.addEventListener('click', function(){
    run();
    setTimeout(run, 30);
    setTimeout(run, 120);
    setTimeout(run, 350);
    setTimeout(run, 800);
  }, true);

  window.addEventListener('load', run);

  var observer = new MutationObserver(run);
  if(document.documentElement){
    observer.observe(document.documentElement, {
      subtree: true,
      childList: true,
      characterData: true,
      attributes: true,
      attributeFilter: ['class', 'style']
    });
  }

  setInterval(run, 250);
  run();
})();
