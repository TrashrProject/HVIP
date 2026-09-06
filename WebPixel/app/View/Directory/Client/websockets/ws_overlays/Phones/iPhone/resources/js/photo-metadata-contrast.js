(function(){
  'use strict';

  function norm(value){
    return String(value || '').replace(/\s+/g, ' ').trim().toUpperCase();
  }

  function hardPaint(el, color, weight){
    if(!el || !el.style) return;
    el.style.setProperty('color', color, 'important');
    el.style.setProperty('-webkit-text-fill-color', color, 'important');
    el.style.setProperty('opacity', '1', 'important');
    el.style.setProperty('visibility', 'visible', 'important');
    el.style.setProperty('filter', 'none', 'important');
    el.style.setProperty('mix-blend-mode', 'normal', 'important');
    el.style.setProperty('text-shadow', '0 1px 2px rgba(0,0,0,.9)', 'important');
    if(weight) el.style.setProperty('font-weight', weight, 'important');
  }

  function paintAncestors(el){
    var p = el;
    for(var i = 0; i < 5 && p; i++, p = p.parentElement){
      p.style && p.style.setProperty('opacity', '1', 'important');
      p.style && p.style.setProperty('visibility', 'visible', 'important');
      p.style && p.style.setProperty('filter', 'none', 'important');
      p.style && p.style.setProperty('mix-blend-mode', 'normal', 'important');
    }
  }

  function forceDateNode(el){
    if(!el) return;
    hardPaint(el, '#ffffff', '900');
    el.style.setProperty('font-size', '12px', 'important');
    el.style.setProperty('line-height', '15px', 'important');
    paintAncestors(el);

    if(el.querySelectorAll){
      var descendants = el.querySelectorAll('*');
      for(var i = 0; i < descendants.length; i++){
        hardPaint(descendants[i], '#ffffff', '900');
        descendants[i].style.setProperty('font-size', '12px', 'important');
      }
    }
  }

  function forceMetadata(){
    var roots = document.querySelectorAll('#phone, #gallery, .pswp, .pswp__caption, .pswp__caption__center');
    if(!roots.length) roots = [document.body];

    var seen = [];
    for(var r = 0; r < roots.length; r++){
      var root = roots[r];
      if(!root || seen.indexOf(root) !== -1) continue;
      seen.push(root);

      var walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT, null, false);
      var node;
      while((node = walker.nextNode())){
        var text = norm(node.nodeValue);
        if(!text) continue;

        if(text.indexOf('PRISE LE') !== -1){
          hardPaint(node.parentElement, '#dce6f2', '800');
          paintAncestors(node.parentElement);
        }

        if(/\b\d{2}\/\d{2}\/\d{4}\b/.test(text)){
          forceDateNode(node.parentElement);
          if(node.parentElement && node.parentElement.parentElement){
            var parent = node.parentElement.parentElement;
            var kids = parent.querySelectorAll('*');
            for(var k = 0; k < kids.length; k++){
              if(/\b\d{2}\/\d{2}\/\d{4}\b/.test(norm(kids[k].textContent)) || /\b\d{1,2}:\d{2}\b/.test(norm(kids[k].textContent))){
                forceDateNode(kids[k]);
              }
            }
          }
        }
      }
    }

    var captions = document.querySelectorAll('.pswp__caption, .pswp__caption__center');
    for(var c = 0; c < captions.length; c++){
      captions[c].style.setProperty('opacity', '1', 'important');
      captions[c].style.setProperty('visibility', 'visible', 'important');
      captions[c].style.setProperty('background-color', '#0b1724', 'important');
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
    setTimeout(run, 20);
    setTimeout(run, 80);
    setTimeout(run, 180);
    setTimeout(run, 400);
    setTimeout(run, 900);
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

  setInterval(run, 200);
  run();
})();
