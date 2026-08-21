(() => {
  'use strict';
  const ID='pr4-command-phone';
  const COMMANDS=[
    [':phone','Ouvre ou ferme ParadisePhone.'],
    [':number / :numero','Affiche votre numéro ParadisePhone.'],
    [':contacts','Ouvre directement vos contacts.'],
    [':addcontact <numéro> <nom>','Ajoute un numéro connu à vos contacts.'],
    [':sms <contact/numéro> <message>','Envoie un SMS privé, même dans une autre room.'],
    [':call <contact/numéro>','Lance un appel RP privé.'],
    [':answer','Décroche un appel entrant.'],
    [':decline','Refuse un appel entrant.'],
    [':hangup','Termine ou annule l’appel en cours.']
  ];
  const esc=v=>String(v??'').replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('>','&gt;').replaceAll('"','&quot;').replaceAll("'",'&#039;');
  function inject(){if(document.getElementById(ID))return true;const body=document.querySelector('#paradise-rp-hud .pr2-command-body');if(!body)return false;const section=document.createElement('div');section.id=ID;section.innerHTML=`<div class="pr2-command-category" style="margin-top:12px">TÉLÉPHONE</div>${COMMANDS.map(([c,d])=>`<div class="pr2-command-row"><code>${esc(c)}</code><span>${esc(d)}</span></div>`).join('')}`;body.appendChild(section);return true;}
  function boot(){if(inject())return;const o=new MutationObserver(()=>{if(inject())o.disconnect();});o.observe(document.body,{childList:true,subtree:true});setTimeout(()=>o.disconnect(),30000);}
  if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',boot,{once:true});else boot();
})();
