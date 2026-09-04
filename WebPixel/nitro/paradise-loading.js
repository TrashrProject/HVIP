(()=>{
  const ID='paradise-loading';
  const root=document.getElementById('root');
  let el=document.getElementById(ID);

  if(!el){
    el=document.createElement('div');
    el.id=ID;
    el.setAttribute('aria-hidden','true');
    el.innerHTML='<div class="pr-backdrop"></div><div class="pr-grid"></div><div class="pr-city"></div><div class="pr-hotel"></div><div class="pr-glow"></div><div class="pr-vignette"></div><div class="pr-center"><div class="pr-logo"></div><div class="pr-kicker">Bienvenue à ParadiseRP</div><div class="pr-title">Chargement de la ville...</div><div class="pr-track"><div class="pr-fill"></div></div><div class="pr-status">Préparation du client...</div></div><div class="pr-corner">ParadiseRP • Habbo Roleplay</div>';
    document.body.insertBefore(el,document.body.firstChild);
  }

  const fill=el.querySelector('.pr-fill');
  const status=el.querySelector('.pr-status');
  let value=4;
  let done=false;
  const started=Date.now();

  const set=(next,text)=>{
    value=Math.max(value,Math.min(94,next));
    if(fill)fill.style.width=value+'%';
    if(text&&status)status.textContent=text;
  };

  const timer=setInterval(()=>{
    if(done)return;
    if(value<35)set(value+Math.random()*5,'Chargement des ressources...');
    else if(value<68)set(value+Math.random()*2.2,'Connexion à ParadiseRP...');
    else set(value+Math.random()*.75,'Initialisation de la ville...');
  },430);

  const finish=()=>{
    if(done)return;
    done=true;
    clearInterval(timer);
    if(fill)fill.style.width='100%';
    if(status)status.textContent='Bienvenue sur ParadiseRP !';
    setTimeout(()=>{
      el.classList.add('is-ready');
      document.documentElement.classList.remove('paradise-booting');
      setTimeout(()=>el.remove(),520);
    },300);
  };

  const clientLooksReady=()=>{
    if(!root)return false;
    const canvas=root.querySelector('canvas');
    if(!canvas)return false;
    const rect=canvas.getBoundingClientRect();
    return rect.width>100&&rect.height>100;
  };

  const checkReady=()=>{
    if(done||!clientLooksReady())return;
    const elapsed=Date.now()-started;
    const wait=Math.max(0,1800-elapsed);
    set(96,'Finalisation...');
    setTimeout(()=>{ if(clientLooksReady()) finish(); },wait+500);
  };

  const observer=new MutationObserver(checkReady);
  if(root)observer.observe(root,{childList:true,subtree:true,attributes:true});
  window.addEventListener('load',()=>set(Math.max(value,48),'Chargement des ressources...'),{once:true});
  const readyPoll=setInterval(()=>{if(done){clearInterval(readyPoll);observer.disconnect();return;}checkReady();},500);
  setTimeout(()=>{if(!done&&clientLooksReady())finish();},15000);
})();
