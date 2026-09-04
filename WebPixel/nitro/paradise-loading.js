(()=>{
  const ID='paradise-loading';
  const root=document.getElementById('root');
  let el=document.getElementById(ID);

  if(!el){
    el=document.createElement('div');
    el.id=ID;
    el.setAttribute('aria-hidden','true');
    el.innerHTML='<div class="pr-bg"></div><div class="pr-ambient"></div><div class="pr-fog"></div><div class="pr-logo-wrap"><div class="pr-logo-base"></div><div class="pr-logo-reveal"></div><div class="pr-particles"></div></div><div class="pr-flash"></div>';
    document.body.insertBefore(el,document.body.firstChild);
  }

  const particles=el.querySelector('.pr-particles');
  if(particles&&!particles.childElementCount){
    const spots=[
      [18,24,3.8,.2],[26,70,4.3,1.1],[34,15,3.6,2],[42,78,4.7,.6],
      [54,12,4.1,1.7],[62,74,3.9,.9],[71,22,4.5,2.4],[79,67,3.7,1.4],
      [87,31,4.8,.4],[12,55,4.2,2.1],[91,54,3.5,1.2],[48,5,4.6,2.8],
      [22,43,4.4,1.9],[66,46,3.9,.35],[76,39,4.5,2.2],[38,56,4.1,1.3]
    ];
    spots.forEach(([x,y,dur,delay])=>{
      const p=document.createElement('span');
      p.className='pr-particle';
      p.style.left=x+'%';
      p.style.top=y+'%';
      p.style.setProperty('--pr-dur',dur+'s');
      p.style.setProperty('--pr-delay',delay+'s');
      particles.appendChild(p);
    });
  }

  let value=4;
  let done=false;
  const started=Date.now();

  const setProgress=(next)=>{
    value=Math.max(value,Math.min(96,next));
    el.style.setProperty('--pr-progress',value+'%');
    el.style.setProperty('--pr-progress-num',String(Math.round(value)));
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
    el.style.setProperty('--pr-progress-num','100');
    el.classList.add('is-complete');

    setTimeout(()=>{
      el.classList.add('is-flashing');
    },220);

    setTimeout(()=>{
      el.classList.add('is-ready');
      document.documentElement.classList.remove('paradise-booting');
      setTimeout(()=>el.remove(),560);
    },620);
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
