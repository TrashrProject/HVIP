(function(){
  'use strict';

  function injectStyle(){
    if(document.getElementById('paradise-photo-caption-readable')) return;
    var style = document.createElement('style');
    style.id = 'paradise-photo-caption-readable';
    style.textContent = [
      '.pswp__caption{background:#0b1724!important;background-color:#0b1724!important;opacity:1!important;visibility:visible!important;}',
      '.pswp__caption__center{color:#eef4fb!important;-webkit-text-fill-color:#eef4fb!important;opacity:1!important;visibility:visible!important;text-shadow:0 1px 2px rgba(0,0,0,.9)!important;}',
      '.pswp__caption__center *{color:#eef4fb!important;-webkit-text-fill-color:#eef4fb!important;opacity:1!important;visibility:visible!important;filter:none!important;mix-blend-mode:normal!important;}',
      '.pswp__caption__center small,.pswp__caption__center span,.pswp__caption__center b,.pswp__caption__center strong,.pswp__caption__center div,.pswp__caption__center p,.pswp__caption__center time{color:#fff!important;-webkit-text-fill-color:#fff!important;opacity:1!important;}',
      '.paradise-photo-meta-label{display:block!important;color:#c9d6e5!important;-webkit-text-fill-color:#c9d6e5!important;font-size:10px!important;line-height:12px!important;font-weight:800!important;letter-spacing:.45px!important;text-transform:uppercase!important;}',
      '.paradise-photo-meta-date{display:block!important;color:#fff!important;-webkit-text-fill-color:#fff!important;font-size:12px!important;line-height:15px!important;font-weight:900!important;letter-spacing:.1px!important;text-shadow:0 1px 2px #000!important;}'
    ].join('');
    document.head.appendChild(style);
  }

  function normalize(value){
    return String(value || '').replace(/\s+/g,' ').trim();
  }

  function rebuildCaption(center){
    if(!center) return;

    var text = normalize(center.textContent);
    if(!/PRISE\s+LE/i.test(text)) return;

    var dateMatch = text.match(/\b\d{2}\/\d{2}\/\d{4}\b/);
    if(!dateMatch) return;

    var timeMatch = text.match(/\b\d{1,2}:\d{2}\b/);
    var dateText = dateMatch[0] + (timeMatch ? ' ' + timeMatch[0] : '');

    if(center.getAttribute('data-paradise-meta') === dateText) return;

    /*
     * La date etait auparavant stylée par les vieux styles de Gallery/PhotoSwipe.
     * On ne tente plus d'ecraser ces styles : on reconstruit uniquement le petit
     * footer de métadonnées avec nos propres elements.
     */
    center.innerHTML = '';

    var label = document.createElement('span');
    label.className = 'paradise-photo-meta-label';
    label.textContent = 'PRISE LE';

    var date = document.createElement('span');
    date.className = 'paradise-photo-meta-date';
    date.textContent = dateText;

    center.appendChild(label);
    center.appendChild(date);
    center.setAttribute('data-paradise-meta', dateText);

    center.style.setProperty('color','#ffffff','important');
    center.style.setProperty('-webkit-text-fill-color','#ffffff','important');
    center.style.setProperty('opacity','1','important');
    center.style.setProperty('visibility','visible','important');
  }

  function fixAll(){
    injectStyle();
    var centers = document.querySelectorAll('.pswp__caption__center');
    for(var i=0;i<centers.length;i++) rebuildCaption(centers[i]);
  }

  var queued = false;
  function queueFix(){
    if(queued) return;
    queued = true;
    requestAnimationFrame(function(){
      queued = false;
      fixAll();
    });
  }

  document.addEventListener('click', function(){
    queueFix();
    setTimeout(queueFix, 25);
    setTimeout(queueFix, 100);
    setTimeout(queueFix, 300);
    setTimeout(queueFix, 700);
  }, true);

  window.addEventListener('load', queueFix);

  var observer = new MutationObserver(queueFix);
  observer.observe(document.documentElement, {
    subtree:true,
    childList:true,
    characterData:true
  });

  setInterval(queueFix, 250);
  queueFix();
})();
