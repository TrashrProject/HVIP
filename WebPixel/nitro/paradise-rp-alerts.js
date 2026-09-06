(()=>{
    'use strict';

    const STACK_ID='paradise-rp-alert-stack';
    const HUD_ID='paradise-hud-v2';
    const LIFE_MS=8500;
    const MAX_ALERTS=4;
    const recent=new Map();

    const norm=value=>String(value||'')
        .replace(/\u00a0/g,' ')
        .replace(/\s+/g,' ')
        .trim();

    function ensureStack(){
        let stack=document.getElementById(STACK_ID);
        if(stack)return stack;
        stack=document.createElement('div');
        stack.id=STACK_ID;
        stack.setAttribute('aria-live','polite');
        stack.setAttribute('aria-atomic','false');
        document.body.appendChild(stack);
        positionStack();
        return stack;
    }

    function positionStack(){
        const stack=document.getElementById(STACK_ID);
        if(!stack)return;
        const hud=document.getElementById(HUD_ID);
        if(!hud){
            stack.style.left='auto';
            stack.style.right='8px';
            stack.style.top='158px';
            stack.style.width='248px';
            return;
        }
        const rect=hud.getBoundingClientRect();
        const width=Math.max(190,Math.round(rect.width||248));
        const maxLeft=Math.max(8,window.innerWidth-width-8);
        const left=Math.min(Math.max(8,Math.round(rect.left)),maxLeft);
        const top=Math.min(window.innerHeight-40,Math.max(8,Math.round(rect.bottom+9)));
        stack.style.left=left+'px';
        stack.style.right='auto';
        stack.style.top=top+'px';
        stack.style.width=width+'px';
    }

    function cleanRecent(){
        const now=Date.now();
        for(const [key,time] of recent){
            if(now-time>12000)recent.delete(key);
        }
    }

    function iconFor(type){
        if(type==='wanted')return '🚨';
        if(type==='death')return '✚';
        if(type==='jail')return '🔒';
        return '•';
    }

    function showAlert(type,title,message,key){
        cleanRecent();
        const dedupe=key||`${type}|${message}`;
        const previous=recent.get(dedupe)||0;
        if(Date.now()-previous<6000)return;
        recent.set(dedupe,Date.now());

        const stack=ensureStack();
        const card=document.createElement('div');
        card.className=`prp-alert prp-alert-${type}`;
        card.innerHTML=`
            <div class="prp-alert-icon">${iconFor(type)}</div>
            <div class="prp-alert-copy">
                <div class="prp-alert-title"></div>
                <div class="prp-alert-message"></div>
            </div>
            <button class="prp-alert-close" type="button" aria-label="Fermer">×</button>`;
        card.querySelector('.prp-alert-title').textContent=title;
        card.querySelector('.prp-alert-message').textContent=message;
        card.querySelector('.prp-alert-close').addEventListener('click',()=>removeCard(card));

        stack.prepend(card);
        while(stack.children.length>MAX_ALERTS)stack.lastElementChild?.remove();
        requestAnimationFrame(()=>{
            positionStack();
            card.classList.add('is-visible');
        });
        setTimeout(()=>removeCard(card),LIFE_MS);
    }

    function removeCard(card){
        if(!card||!card.isConnected)return;
        card.classList.remove('is-visible');
        card.classList.add('is-leaving');
        setTimeout(()=>card.remove(),220);
    }

    function parseText(raw){
        const text=norm(raw);
        if(!text||text.length>600)return;
        if(text.includes('AVIS DE RECHERCHE')||text.includes('INCARCÉRATION')||text.includes('DÉCÈS'))return;

        let match=text.match(/\bPlace\s+([A-Za-z0-9_.-]{1,32})\s+au niveau de recherche\s+(\d{1,2})\b/i);
        if(!match)match=text.match(/\b([A-Za-z0-9_.-]{1,32})\s+est maintenant recherch(?:é|ée)\s+(?:au\s+)?niveau\s*(\d{1,2})\b/i);
        if(match){
            const name=match[1];
            const level=match[2];
            showAlert('wanted','AVIS DE RECHERCHE',`${name} est maintenant recherché — Niveau ${level}`,`wanted|${name.toLowerCase()}|${level}`);
            return;
        }

        match=text.match(/\b([A-Za-z0-9_.-]{1,32})\s+(?:vient de mourir|est mort(?:e)?|est décéd(?:é|ée)|a été tu(?:é|ée))\b/i);
        if(match){
            const name=match[1];
            showAlert('death','DÉCÈS',`${name} vient de mourir.`,`death|${name.toLowerCase()}`);
            return;
        }

        match=text.match(/\bPlace\s+([A-Za-z0-9_.-]{1,32})\s+en prison\b/i);
        if(!match)match=text.match(/\b([A-Za-z0-9_.-]{1,32})\s+(?:a été|est) emprisonn(?:é|ée)\b/i);
        if(match){
            const name=match[1];
            showAlert('jail','INCARCÉRATION',`${name} vient d'être emprisonné.`,`jail|${name.toLowerCase()}`);
        }
    }

    function inspectNode(node){
        if(!node)return;
        if(node.nodeType===Node.TEXT_NODE){
            if(node.parentElement?.closest('#'+STACK_ID))return;
            parseText(node.nodeValue);
            return;
        }
        if(node.nodeType!==Node.ELEMENT_NODE)return;
        if(node.id===STACK_ID||node.closest?.('#'+STACK_ID))return;
        parseText(node.textContent);
    }

    const observer=new MutationObserver(records=>{
        for(const record of records){
            for(const node of record.addedNodes)inspectNode(node);
            if(record.type==='characterData')inspectNode(record.target);
        }
        positionStack();
    });

    function boot(){
        ensureStack();
        observer.observe(document.documentElement,{childList:true,subtree:true,characterData:true});
        window.addEventListener('resize',positionStack,{passive:true});
        setInterval(positionStack,500);
        setTimeout(positionStack,0);
        window.ParadiseRPAlerts={
            show:(type,title,message,key)=>showAlert(type,title,message,key),
            reposition:positionStack
        };
    }

    if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',boot,{once:true});
    else boot();
})();
