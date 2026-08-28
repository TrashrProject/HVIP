(()=>{
    const ID='paradise-hud-v2';
    let source=null;
    let lastSig='';

    const norm=s=>String(s||'').replace(/\u00a0/g,' ').replace(/\s+/g,' ').trim();
    const esc=s=>String(s||'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
    const visible=e=>{const r=e.getBoundingClientRect();return r.width>0&&r.height>0};
    const getLines=e=>String(e.innerText||'').split(/\n+/).map(norm).filter(Boolean);
    const isTime=s=>/^(?:[01]?\d|2[0-3]):[0-5]\d(?:\s?[AP]M)?$/i.test(s);
    const isNumber=s=>/^\d[\d\s.,]*$/.test(s);

    function findSource(){
        if(source&&source.isConnected)return source;
        const all=[...document.querySelectorAll('body div,body section')];
        let best=null;
        for(const el of all){
            if(el.id===ID||el.closest('#'+ID))continue;
            const text=String(el.innerText||'');
            if(!/generic\.not\.vip|generic\.vip/i.test(text))continue;
            const lines=getLines(el);
            if(!lines.some(isTime))continue;
            if(lines.filter(isNumber).length<3)continue;
            const r=el.getBoundingClientRect();
            if(r.width<120||r.width>360||r.height<90||r.height>360)continue;
            if(!best||r.width*r.height<best.r.width*best.r.height)best={el,r};
        }
        if(best){
            source=best.el;
            source.dataset.paradiseHudSource='1';
        }
        return source;
    }

    function extract(el){
        const lines=getLines(el);
        const time=lines.find(isTime)||'';
        const vipLine=lines.find(x=>/generic\.(?:not\.)?vip/i.test(x))||'';
        const nums=lines.filter(isNumber).slice(0,3);

        const candidates=lines.filter(x=>
            !isTime(x)&&
            !isNumber(x)&&
            !/generic\.(?:not\.)?vip/i.test(x)&&
            !/^ParadiseRP$/i.test(x)
        );

        let roomLine=
            candidates.find(x=>/\[\d+\]/.test(x))||
            candidates.find(x=>/\[v\d+\]/i.test(x))||
            candidates[0]||
            'ParadiseRP';

        let version='';
        const vm=roomLine.match(/\[(v\d+)\]/i);
        if(vm){
            version=vm[1];
            roomLine=norm(roomLine.replace(vm[0],''));
        }

        const imgs=[...el.querySelectorAll('img')]
            .filter(img=>{const r=img.getBoundingClientRect();return r.width>4&&r.height>4})
            .slice(0,3);

        let actions=[...el.querySelectorAll('button')].filter(visible);
        if(actions.length<3){
            actions=[...el.querySelectorAll('[role="button"],.cursor-pointer')]
                .filter(x=>visible(x)&&x!==el)
                .filter((x,i,a)=>!a.some((y,j)=>j!==i&&y.contains(x)))
                .slice(-3);
        } else {
            actions=actions.slice(-3);
        }

        return {
            room:roomLine,
            version,
            time,
            vip:!/generic\.not\.vip/i.test(vipLine),
            nums,
            imgs,
            actions
        };
    }

    function iconHTML(img,i){
        if(img){
            const c=img.cloneNode(true);
            c.removeAttribute('class');
            c.removeAttribute('style');
            return c.outerHTML;
        }
        return '<span class="phud-money-dot"></span>';
    }

    function actionHTML(btn,i){
        if(!btn)return ['💬','🚨','⚙'][i]||'•';
        const clone=btn.cloneNode(true);
        clone.removeAttribute('class');
        clone.removeAttribute('style');
        clone.removeAttribute('id');
        const h=clone.innerHTML.trim();
        return h||(['💬','🚨','⚙'][i]||'•');
    }

    function render(data,rect){
        let hud=document.getElementById(ID);
        if(!hud){
            hud=document.createElement('div');
            hud.id=ID;
            document.body.appendChild(hud);
        }

        hud.style.setProperty('--phud-left',Math.max(6,rect.left)+'px');
        hud.style.setProperty('--phud-top',Math.max(6,rect.top)+'px');

        hud.innerHTML=`
            <div class="phud-shell">
                <div class="phud-room" title="${esc(data.room)}">
                    <div class="phud-room-name">${esc(data.room)}</div>
                    ${data.version?`<div class="phud-version">${esc(data.version)}</div>`:''}
                </div>
                <div class="phud-money">
                    ${[0,1,2].map(i=>`
                        <div class="phud-money-item">
                            <span class="phud-money-icon">${iconHTML(data.imgs[i],i)}</span>
                            <span class="phud-money-value">${esc(data.nums[i]||'0')}</span>
                        </div>
                    `).join('')}
                </div>
                <div class="phud-bottom">
                    <div class="phud-time">${esc(data.time)}</div>
                    <div class="phud-vip ${data.vip?'is-active':''}">${data.vip?'VIP':'VIP OFF'}</div>
                    <div class="phud-actions">
                        ${[0,1,2].map(i=>`<button class="phud-action" data-action="${i}" type="button">${actionHTML(data.actions[i],i)}</button>`).join('')}
                    </div>
                </div>
            </div>`;

        hud.querySelectorAll('.phud-action').forEach(b=>b.addEventListener('click',()=>{
            const i=Number(b.dataset.action);
            try{data.actions[i]?.click()}catch{}
        }));
    }

    function inspect(){
        const el=findSource();
        if(!el)return;

        if(el.classList.contains('paradise-hud-source-hidden')){
            const d=extract(el);
            const sig=JSON.stringify([d.room,d.version,d.time,d.vip,d.nums]);
            if(sig!==lastSig){
                lastSig=sig;
                const prev=el.dataset.paradiseHudRect?.split(',').map(Number);
                const rect=prev&&prev.length===4
                    ?{left:prev[0],top:prev[1],width:prev[2],height:prev[3]}
                    :{left:40,top:8,width:202,height:92};
                render(d,rect);
            }
            return;
        }

        const rect=el.getBoundingClientRect();
        el.dataset.paradiseHudRect=[rect.left,rect.top,rect.width,rect.height].join(',');
        const d=extract(el);
        lastSig=JSON.stringify([d.room,d.version,d.time,d.vip,d.nums]);
        render(d,rect);
        el.classList.add('paradise-hud-source-hidden');
    }

    new MutationObserver(()=>queueMicrotask(inspect)).observe(document.documentElement,{childList:true,subtree:true,characterData:true});
    setInterval(inspect,600);
    setTimeout(inspect,0);
})();