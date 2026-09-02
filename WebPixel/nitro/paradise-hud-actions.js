(()=>{
    const ROOT='#paradise-hud-v2';
    const ICONS=[
        `<svg class="phud-action-icon phud-action-icon-chat" viewBox="0 0 24 24" aria-hidden="true"><path d="M5 5.5h14a2 2 0 0 1 2 2v7a2 2 0 0 1-2 2h-7.2L7.3 20v-3.5H5a2 2 0 0 1-2-2v-7a2 2 0 0 1 2-2Z" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linejoin="round"/><path d="M7.5 9h9M7.5 12.5h6" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/></svg>`,
        `<svg class="phud-action-icon phud-action-icon-alert" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 15.5h8l-1.15-6.1A2.9 2.9 0 0 0 12 7a2.9 2.9 0 0 0-2.85 2.4L8 15.5Z" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round"/><path d="M6.5 18h11M12 3.5v1.7M5.6 6.1l1.3 1M18.4 6.1l-1.3 1" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/></svg>`,
        `<svg class="phud-action-icon phud-action-icon-settings" viewBox="0 0 24 24" aria-hidden="true"><path d="M9.7 4.5 10.2 3h3.6l.5 1.5 1.5.9 1.5-.3 1.8 3.1-1 1.2v1.8l1 1.2-1.8 3.1-1.5-.3-1.5.9-.5 1.5h-3.6l-.5-1.5-1.5-.9-1.5.3-1.8-3.1 1-1.2V9.4l-1-1.2 1.8-3.1 1.5.3 1.5-.9Z" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linejoin="round"/><circle cx="12" cy="10.3" r="2.5" fill="none" stroke="currentColor" stroke-width="1.7"/></svg>`
    ];

    const LABELS=['Messages','Alerte','Réglages'];

    function polish(){
        const root=document.querySelector(ROOT);
        if(!root)return;

        const buttons=[...root.querySelectorAll('.phud-actions .phud-action')];
        if(buttons.length<3)return;

        buttons.slice(0,3).forEach((button,index)=>{
            if(button.dataset.paradiseActionIcon==='v12')return;
            button.innerHTML=ICONS[index];
            button.dataset.paradiseActionIcon='v12';
            button.setAttribute('aria-label',LABELS[index]);
            if(!button.getAttribute('title'))button.setAttribute('title',LABELS[index]);
            button.classList.add(`phud-action-${index}`);
        });
    }

    const observer=new MutationObserver(()=>queueMicrotask(polish));
    observer.observe(document.documentElement,{childList:true,subtree:true});
    setInterval(polish,500);
    setTimeout(polish,0);
})();
