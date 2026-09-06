(function(){
  'use strict';

  function rgbFromColor(value){
    var m = String(value || '').match(/rgba?\((\d+),\s*(\d+),\s*(\d+)/i);
    return m ? [parseInt(m[1],10), parseInt(m[2],10), parseInt(m[3],10)] : null;
  }

  function isDark(el){
    var node = el;
    while(node && node !== document.documentElement){
      var bg = window.getComputedStyle(node).backgroundColor;
      var rgb = rgbFromColor(bg);
      if(rgb && !(rgb[0] === 0 && rgb[1] === 0 && rgb[2] === 0 && /rgba\([^)]*,\s*0\)/.test(bg))){
        var lum = (0.2126 * rgb[0]) + (0.7152 * rgb[1]) + (0.0722 * rgb[2]);
        return lum < 145;
      }
      node = node.parentElement;
    }
    return true;
  }

  function normalizeText(s){
    return String(s || '').replace(/\s+/g,' ').trim().toUpperCase();
  }

  function findMetaBlock(){
    var phone = document.getElementById('phone');
    if(!phone) return null;
    var all = phone.querySelectorAll('*');
    for(var i=0;i<all.length;i++){
      var text = normalizeText(all[i].textContent);
      if(text === 'PRISE LE' || text.indexOf('PRISE LE ') === 0){
        var el = all[i];
        var p = el;
        for(var j=0;j<4 && p && p !== phone;j++, p=p.parentElement){
          var t = normalizeText(p.textContent);
          if(t.indexOf('PRISE LE') !== -1 && /\d{2}\/\d{2}\/\d{4}/.test(t)) return p;
        }
        return el.parentElement || el;
      }
    }
    return null;
  }

  function applyContrast(){
    var block = findMetaBlock();
    if(!block) return;

    var dark = isDark(block);
    var fg = dark ? '#f7f9fc' : '#111827';
    var sub = dark ? '#d7deea' : '#283548';
    var bg = dark ? 'rgba(12,18,29,.96)' : 'rgba(247,249,252,.96)';
    var border = dark ? 'rgba(255,255,255,.10)' : 'rgba(17,24,39,.10)';

    block.style.setProperty('color', fg, 'important');
    block.style.setProperty('background', bg, 'important');
    block.style.setProperty('border-top', '1px solid ' + border, 'important');
    block.style.setProperty('opacity', '1', 'important');

    var descendants = block.querySelectorAll('*');
    for(var i=0;i<descendants.length;i++){
      descendants[i].style.setProperty('color', fg, 'important');
      descendants[i].style.setProperty('opacity', '1', 'important');
      descendants[i].style.setProperty('text-shadow', dark ? '0 1px 2px rgba(0,0,0,.7)' : 'none', 'important');
    }

    var textNodes = block.children;
    for(var k=0;k<textNodes.length;k++){
      if(/PRISE LE/i.test(textNodes[k].textContent || '')){
        textNodes[k].style.setProperty('color', sub, 'important');
        textNodes[k].style.setProperty('font-weight','800','important');
        textNodes[k].style.setProperty('letter-spacing','.5px','important');
      }
    }
  }

  var queued = false;
  function queueApply(){
    if(queued) return;
    queued = true;
    requestAnimationFrame(function(){
      queued = false;
      applyContrast();
    });
  }

  document.addEventListener('click', function(){ setTimeout(queueApply, 20); }, true);
  window.addEventListener('load', queueApply);

  var observer = new MutationObserver(queueApply);
  if(document.documentElement){
    observer.observe(document.documentElement,{subtree:true,childList:true,characterData:true,attributes:true,attributeFilter:['class','style']});
  }

  setInterval(queueApply, 700);
  queueApply();
})();
