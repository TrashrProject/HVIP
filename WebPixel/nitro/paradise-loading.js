(()=>{
  const ID='paradise-loading';
  const root=document.getElementById('root');
  let el=document.getElementById(ID);

  if(!el){
    el=document.createElement('div');
    el.id=ID;
    el.setAttribute('aria-hidden','true');
    el.innerHTML='<div class="pr-logo-wrap"><div class="pr-logo-base"></div><div class="pr-logo-reveal"></div></div>';
    document.body.insertBefore(el,document.body.firstChild);
  }

  let value=4;
  let done=false;
  const started=Date.now();

  const setProgress=(next)=>{
    value=Math.max(value,Math.min(96,next));
    el.style.setProperty('--pr-progress',value+'%');
  };

  setProgress(value);

  const timer=setInterval(()=>{
    if(done)return;
    if(value<32)setProgress(value+Math.random()*5.5);
    else if(value<66)setProgress(value+Math.random()*2.4);
    else if(value<86)setProgress(value+Math.random()*1.15);
    else setProgress(value+Math.random()*.45);
  },420);

  const finish=()=>{
    if(done)return;
    done=true;
    clearInterval(timer);
    el.style.setProperty('--pr-progress','100%');
    el.classList.add('is-complete');
    setTimeout(()=>{
      el.classList.add('is-ready');
      document.documentElement.classList.remove('paradise-booting');
      setTimeout(()=>el.remove(),500);
    },420);
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
    const wait=Math.max(0,1500-elapsed);
    setProgress(96);
    setTimeout(()=>{if(clientLooksReady())finish();},wait+350);
  };

  const observer=new MutationObserver(checkReady);
  if(root)observer.observe(root,{childList:true,subtree:true,attributes:true});
  window.addEventListener('load',()=>setProgress(Math.max(value,48)),{once:true});

  const readyPoll=setInterval(()=>{
    if(done){clearInterval(readyPoll);observer.disconnect();return;}
    checkReady();
  },450);

  setTimeout(()=>{if(!done&&clientLooksReady())finish();},15000);
})();
