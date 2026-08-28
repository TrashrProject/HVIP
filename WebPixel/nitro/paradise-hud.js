(()=>{
    const ID='paradise-hud-v2';

    let source=null;
    let sourceRect=null;
    let lastSig='';
    let cachedImages=[];
    let cachedActions=[];

    const norm=value=>String(value||'')
        .replace(/\u00a0/g,' ')
        .replace(/\s+/g,' ')
        .trim();

    const esc=value=>String(value||'').replace(/[&<>"']/g,char=>({
        '&':'&amp;',
        '<':'&lt;',
        '>':'&gt;',
        '"':'&quot;',
        "'":'&#39;'
    }[char]));

    const isTime=value=>/^(?:[01]?\d|2[0-3]):[0-5]\d(?:\s?[AP]M)?$/i.test(norm(value));
    const isNumber=value=>/^\d[\d\s.,]*$/.test(norm(value));
    const isVipKey=value=>/generic\.(?:not\.)?vip/i.test(norm(value));

    function textTokens(element){
        const tokens=[];
        const walker=document.createTreeWalker(element,NodeFilter.SHOW_TEXT);

        while(walker.nextNode()){
            const value=norm(walker.currentNode.nodeValue);
            if(value)tokens.push(value);
        }

        return tokens;
    }

    function visible(element){
        if(!element)return false;
        const rect=element.getBoundingClientRect();
        return rect.width>0&&rect.height>0;
    }

    function findSource(){
        if(source&&source.isConnected)return source;

        source=null;
        sourceRect=null;
        lastSig='';
        cachedImages=[];
        cachedActions=[];

        let best=null;

        for(const element of document.querySelectorAll('body div, body section, body aside')){
            if(element.id===ID||element.closest('#'+ID))continue;

            const raw=String(element.textContent||'');
            if(!/generic\.(?:not\.)?vip/i.test(raw))continue;

            const tokens=textTokens(element);
            if(!tokens.some(isTime))continue;
            if(tokens.filter(isNumber).length<3)continue;

            const rect=element.getBoundingClientRect();
            if(rect.width<120||rect.width>380||rect.height<80||rect.height>380)continue;

            if(!best||rect.width*rect.height<best.rect.width*best.rect.height){
                best={element,rect};
            }
        }

        if(!best)return null;

        source=best.element;
        sourceRect=best.rect;
        source.dataset.paradiseHudSource='1';
        return source;
    }

    function findRoom(tokens){
        const usable=tokens.filter(value=>
            value&&
            !isTime(value)&&
            !isNumber(value)&&
            !isVipKey(value)&&
            !/^ParadiseRP$/i.test(value)
        );

        let room=usable.find(value=>/[A-Za-zÀ-ÿ]/.test(value)&&/\[\d+\]/.test(value));

        if(!room){
            for(let i=0;i<usable.length;i++){
                const current=usable[i];
                const next=usable[i+1]||'';
                const after=usable[i+2]||'';

                if(/[A-Za-zÀ-ÿ]/.test(current)&&/^\[\d+\]$/.test(next)){
                    room=`${current} ${next}`;
                    if(/^\[v\d+\]$/i.test(after))room+=` ${after}`;
                    break;
                }
            }
        }

        if(!room){
            room=usable.find(value=>/[A-Za-zÀ-ÿ]/.test(value)&&/\[v\d+\]/i.test(value));
        }

        if(!room)return null;

        let version='';
        const versionMatch=room.match(/\[(v\d+)\]/i);
        if(versionMatch){
            version=versionMatch[1];
            room=norm(room.replace(versionMatch[0],''));
        }

        if(!room||/^ParadiseRP$/i.test(room))return null;
        return {room,version};
    }

    function captureAssets(element){
        if(!cachedImages.length){
            cachedImages=[...element.querySelectorAll('img')]
                .filter(visible)
                .slice(0,3);
        }

        if(!cachedActions.length){
            let actions=[...element.querySelectorAll('button')].filter(visible);

            if(actions.length<3){
                actions=[...element.querySelectorAll('[role="button"], .cursor-pointer')]
                    .filter(node=>node!==element&&visible(node))
                    .filter((node,index,array)=>!array.some((other,otherIndex)=>otherIndex!==index&&other.contains(node)));
            }

            cachedActions=actions.slice(-3);
        }
    }

    function extract(element,capture=false){
        const tokens=textTokens(element);
        const time=tokens.find(isTime)||'';
        const vipToken=tokens.find(isVipKey)||'';
        const numbers=tokens.filter(isNumber).slice(0,3);
        const roomData=findRoom(tokens);

        if(!time||numbers.length<3||!roomData){
            return {valid:false};
        }

        if(capture)captureAssets(element);

        return {
            valid:true,
            room:roomData.room,
            version:roomData.version,
            time,
            vip:!!vipToken&&!/generic\.not\.vip/i.test(vipToken),
            nums:numbers,
            imgs:cachedImages,
            actions:cachedActions
        };
    }

    function iconHTML(image,index){
        if(image){
            const clone=image.cloneNode(true);
            clone.removeAttribute('class');
            clone.removeAttribute('style');
            clone.removeAttribute('width');
            clone.removeAttribute('height');
            return clone.outerHTML;
        }

        return `<span class="phud-money-dot phud-money-dot-${index}"></span>`;
    }

    function actionHTML(button,index){
        if(button){
            const html=String(button.innerHTML||'').trim();
            if(html)return html;
        }

        return `<span class="phud-action-fallback">${index===1?'!':'•'}</span>`;
    }

    function render(data){
        let hud=document.getElementById(ID);

        if(!hud){
            hud=document.createElement('div');
            hud.id=ID;
            document.body.appendChild(hud);
        }

        const rect=sourceRect||{left:40,top:8};
        hud.style.setProperty('--phud-left',Math.max(6,rect.left)+'px');
        hud.style.setProperty('--phud-top',Math.max(6,rect.top)+'px');

        hud.innerHTML=`
            <div class="phud-shell">
                <div class="phud-room" title="${esc(data.room)}">
                    <div class="phud-room-name">${esc(data.room)}</div>
                    ${data.version?`<div class="phud-version">${esc(data.version)}</div>`:''}
                </div>

                <div class="phud-money">
                    ${[0,1,2].map(index=>`
                        <div class="phud-money-item">
                            <span class="phud-money-icon">${iconHTML(data.imgs[index],index)}</span>
                            <span class="phud-money-value">${esc(data.nums[index])}</span>
                        </div>
                    `).join('')}
                </div>

                <div class="phud-bottom">
                    <div class="phud-time">${esc(data.time)}</div>
                    <div class="phud-vip ${data.vip?'is-active':''}">${data.vip?'VIP ON':'VIP OFF'}</div>
                    <div class="phud-actions">
                        ${[0,1,2].map(index=>`
                            <button class="phud-action" data-action="${index}" type="button" ${data.actions[index]?.title?`title="${esc(data.actions[index].title)}"`:''}>
                                ${actionHTML(data.actions[index],index)}
                            </button>
                        `).join('')}
                    </div>
                </div>
            </div>`;

        hud.querySelectorAll('.phud-action').forEach(button=>{
            button.addEventListener('click',()=>{
                const index=Number(button.dataset.action);
                const original=cachedActions[index];
                if(!original||!original.isConnected)return;
                try{original.click()}catch{}
            });
        });
    }

    function inspect(){
        const element=findSource();
        if(!element)return;

        const firstPass=!element.classList.contains('paradise-hud-source-hidden');

        if(firstPass){
            const rect=element.getBoundingClientRect();
            sourceRect={left:rect.left,top:rect.top,width:rect.width,height:rect.height};
        }

        const data=extract(element,firstPass);

        if(!data.valid){
            if(firstPass){
                document.getElementById(ID)?.remove();
            }
            return;
        }

        const signature=JSON.stringify([
            data.room,
            data.version,
            data.time,
            data.vip,
            data.nums
        ]);

        if(firstPass||signature!==lastSig){
            lastSig=signature;
            render(data);
        }

        if(firstPass){
            element.classList.add('paradise-hud-source-hidden');
        }
    }

    const observer=new MutationObserver(()=>queueMicrotask(inspect));
    observer.observe(document.documentElement,{childList:true,subtree:true,characterData:true});

    setInterval(inspect,500);
    setTimeout(inspect,0);
})();
