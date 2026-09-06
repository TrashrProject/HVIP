(function(){
  'use strict';

  function normalizeText(s){
    return String(s || '').replace(/\s+/g,' ').trim().toUpperCase();
  }

  function findMetaBlock(){
    var phone = document.getElementById('phone');
    if(!phone) return null;

    var all = phone.querySelectorAll('*');
    var label = null;

    for(var i=0;i<all.length;i++){
      var text = normalizeText(all[i].textContent);
      if(text === 'PRISE LE' || text.indexOf('PRISE LE ') === 0){
        label = all[i];
        break;
      }
    }

    if(!label) return null;

    var p = label;
    while(p && p !== phone){
      var t = normalizeText(p.textContent);
      if(t.indexOf('PRISE LE') !== -1 && /\d{2}\/\d{2}\/\d{4}/.test(t)){
        return p;
      }
      p = p.parentElement;
    }

    return label.parentElement || label;
  }

  function forceReadable(block){
    if(!block) return;

    /* On garde volontairement ce bandeau sombre dans les deux thèmes.
       Comme ça le label + date/heure restent toujours lisibles. */
    block.style.setProperty('background', '#0b1724', 'important');
    block.style.setProperty('background-color', '#0b1724', 'important');
    block.style.setProperty('color', '#ffffff', 'important');
    block.style.setProperty('opacity', '1', 'important');
    block.style.setProperty('border-top', '1px solid rgba(255,255,255,.08)', 'important');

    var nodes = block.querySelectorAll('*');
    for(var i=0;i<nodes.length;i++){
      nodes[i].style.setProperty('color', '#ffffff', 'important');
      nodes[i].style.setProperty('opacity', '1', 'important');
      nodes[i].style.setProperty('text-shadow', '0 1px 2px rgba(0,0,0,.75)', 'important');
      nodes[i].style.removeProperty('filter');
    }

    /* Renforce spécialement PRISE LE + la date/heure. */
    for(var j=0;j<nodes.length;j++){
      var text = normalizeText(nodes[j].textContent);
      if(text === 'PRISE LE' || text.indexOf('PRISE LE ') === 0){
        nodes[j].style.setProperty('color', '#dce6f2', 'important');
        nodes[j].style.setProperty('font-weight', '800', 'important');
        nodes[j].style.setProperty('letter-spacing', '.5px', 'important');
      }
      if(/\d{2}\/\d{2}\/\d{4}/.test(text)){
        nodes[j].style.setProperty('color', '#ffffff', 'important');
        nodes[j].style.setProperty('font-weight', '800', 'important');
      }
    }
  }

  function apply(){
    forceReadable(findMetaBlock());
  }

  var queued = false;
  function queueApply(){
    if(queued) return;
    queued = true;
    requestAnimationFrame(function(){
      queued = false;
      apply();
    });
  }

  document.addEventListener('click', function(){
    setTimeout(queueApply, 0);
    setTimeout(queueApply, 80);
    setTimeout(queueApply, 250);
  }, true);

  window.addEventListener('load', queueApply);

  var observer = new MutationObserver(queueApply);
  if(document.documentElement){
    observer.observe(document.documentElement, {
      subtree:true,
      childList:true,
      characterData:true,
      attributes:true,
      attributeFilter:['class','style']
    });
  }

  setInterval(queueApply, 300);
  queueApply();
})();
