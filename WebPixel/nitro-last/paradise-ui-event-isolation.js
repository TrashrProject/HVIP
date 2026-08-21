(() => {
  'use strict';

  const VERSION = '2.0.0-input-safe-event-isolation';
  const HUD_ID = 'paradise-rp-hud';

  function bind(root){
    if(!root||root.dataset.prEventIsolation===VERSION)return;
    root.dataset.prEventIsolation=VERSION;

    root.addEventListener('click',event=>{
      const interactive=event.target?.closest?.('button, a, input, textarea, select, [data-pr4-action], [data-pr4-item], [data-pr4-cat], [data-pr4-command], [data-pr4-command-cat]');
      if(!interactive)return;
      if(interactive.tagName==='A'&&interactive.hasAttribute('href'))event.preventDefault();
      event.stopPropagation();
    },false);

    root.addEventListener('pointerdown',event=>{
      const interactive=event.target?.closest?.('button, a, input, textarea, select, [data-pr4-action], [data-pr4-item], [data-pr4-cat], [data-pr4-command], [data-pr4-command-cat]');
      if(interactive)event.stopPropagation();
    },false);

    root.addEventListener('keydown',event=>{
      if(event.target?.matches?.('input, textarea, select, [contenteditable="true"]'))event.stopPropagation();
    },false);
    root.addEventListener('keyup',event=>{
      if(event.target?.matches?.('input, textarea, select, [contenteditable="true"]'))event.stopPropagation();
    },false);
  }

  function scan(){bind(document.getElementById(HUD_ID));}
  if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',scan,{once:true});else scan();
  window.__ParadiseUIEventIsolation={version:VERSION,scan};
})();
