(() => {
    'use strict';
    if (window.__PARADISE_TUNES_PREMIUM__) return;
    window.__PARADISE_TUNES_PREMIUM__ = '1.0.0';

    const API = '/nitro/paradise-tunes-api.php';
    const esc = value => String(value ?? '').replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('>','&gt;').replaceAll('"','&quot;').replaceAll("'",'&#039;');
    const fmt = seconds => Number.isFinite(seconds) ? `${Math.floor(seconds / 60)}:${String(Math.floor(seconds % 60)).padStart(2,'0')}` : '0:00';
    const cover = track => track?.coverUrl || '';
    const youtubeId = value => {
        try {
            const url = new URL(value);
            const host = url.hostname.toLowerCase().replace(/^www\./, '');
            let id = host === 'youtu.be' ? url.pathname.split('/').filter(Boolean)[0] : null;
            if (host === 'youtube.com' || host === 'music.youtube.com' || host.endsWith('.youtube.com')) {
                id ||= url.searchParams.get('v');
                id ||= url.pathname.match(/^\/(?:embed|shorts|live)\/([^/?]+)/)?.[1];
            }
            return /^[A-Za-z0-9_-]{11}$/.test(id || '') ? id : null;
        } catch { return null; }
    };

    class ParadiseTunes {
        constructor() {
            this.root = null; this.csrf = ''; this.user = null; this.tracks = []; this.playlists = [];
            this.tab = 'home'; this.previousTab = 'home'; this.filter = 'tracks'; this.query = ''; this.overlay = null; this.menuTrack = null;
            this.audio = new Audio(); this.audio.preload = 'metadata'; this.current = null; this.queue = []; this.queueIndex = -1;
            this.playing = false; this.currentTime = 0; this.duration = 0; this.volume = .8; this.repeat = 0; this.shuffle = false;
            this.audio.volume = this.volume;
            this.audio.addEventListener('timeupdate', () => { this.currentTime = this.audio.currentTime; this.duration = this.audio.duration || 0; this.updatePlayer(); });
            this.audio.addEventListener('play', () => { this.playing = true; this.renderChrome(); });
            this.audio.addEventListener('pause', () => { this.playing = false; this.renderChrome(); });
            this.audio.addEventListener('ended', () => this.repeat === 2 ? this.play(this.current, this.queue, true) : this.next(true));
            this.audio.addEventListener('error', () => this.notice('Cette source audio n’est pas compatible.'));
            window.addEventListener('message', event => this.youtubeMessage(event));
        }
        async request(options = {}) {
            const controller = new AbortController(), timer = setTimeout(() => controller.abort(), 9000);
            try {
                const response = await fetch(API, { credentials:'same-origin', cache:'no-store', signal:controller.signal, ...options });
                const data = await response.json().catch(() => null);
                if (!response.ok || !data?.ok) throw new Error(data?.error || 'Impossible de charger Paradise Tunes.');
                return data;
            } finally { clearTimeout(timer); }
        }
        async action(payload) { return this.request({ method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify({...payload, csrf:this.csrf}) }); }
        async mount(root) {
            if (root.dataset.tunesPremium) return;
            this.root = root; root.dataset.tunesPremium = '1'; root.className = 'ppr-app pt-app';
            root.addEventListener('click', event => this.click(event)); root.addEventListener('input', event => this.input(event)); root.addEventListener('submit', event => this.submit(event));
            this.skeleton();
            try { await this.load(); } catch (error) { this.error(error.message); }
        }
        skeleton() { this.root.innerHTML = `<div class="pt-skeleton"><i></i><i></i><div><i></i><i></i></div><div><i></i><i></i></div></div>`; }
        async load() {
            const data = await this.request(); this.csrf=data.csrf; this.user=data.user; this.tracks=data.tracks||[]; this.playlists=data.playlists||[];
            if (this.current) this.current = this.tracks.find(track => track.id === this.current.id) || this.current;
            this.render();
        }
        greeting() { const hour=new Date().getHours(); return `${hour<12?'Bonjour':hour<18?'Bon après-midi':'Bonsoir'}, ${this.user?.username||''}`; }
        recent() { try { const ids=JSON.parse(localStorage.getItem('paradise.tunes.recent')||'[]'); return ids.map(id=>this.tracks.find(track=>track.id===id)).filter(Boolean).slice(0,8); } catch { return []; } }
        addRecent(id) { const ids=[id,...this.recent().map(track=>track.id).filter(value=>value!==id)].slice(0,12); localStorage.setItem('paradise.tunes.recent',JSON.stringify(ids)); }
        render() {
            this.root.innerHTML = `<main class="pt-main" data-pt-main>${this.page()}</main><div data-pt-chrome></div>${this.overlayHtml()}`;
            this.renderChrome();
        }
        page() { if(this.overlay==='player') return this.playerPage(); if(this.overlay?.type==='playlist') return this.playlistPage(this.overlay.id); if(this.tab==='search') return this.searchPage(); if(this.tab==='library') return this.libraryPage(); return this.homePage(); }
        header(title, subtitle='') { return `<header class="pt-header"><div><small>${esc(subtitle)}</small><h1>${esc(title)}</h1></div><button type="button" data-pt-add aria-label="Ajouter une musique">＋</button></header>`; }
        empty(title,text) { return `<div class="pt-empty"><div class="pt-cover-placeholder">♪</div><strong>${esc(title)}</strong><span>${esc(text)}</span></div>`; }
        card(track) { return `<button class="pt-card" type="button" data-pt-play="${track.id}">${this.coverHtml(track)}<strong>${esc(track.title)}</strong><span>${esc(track.artist||'Artiste inconnu')}</span></button>`; }
        coverHtml(track, cls='') { return `<span class="pt-cover ${cls}">${cover(track)?`<img src="${esc(cover(track))}" alt="" loading="lazy" onerror="this.remove()">`:''}<i>PT</i></span>`; }
        row(track) { return `<div class="pt-track-row">${this.coverHtml(track)}<button type="button" class="pt-track-info" data-pt-play="${track.id}"><strong>${esc(track.title)}</strong><span>${esc(track.artist||'Artiste inconnu')}</span></button><button type="button" class="pt-heart ${track.favorite?'active':''}" data-pt-favorite="${track.id}" aria-label="Favori">${track.favorite?'♥':'♡'}</button><button type="button" class="pt-more" data-pt-menu="${track.id}" aria-label="Options">•••</button></div>`; }
        section(title, tracks) { return `<section class="pt-section"><h2>${esc(title)}</h2>${tracks.length?`<div class="pt-horizontal">${tracks.map(track=>this.card(track)).join('')}</div>`:this.empty('Aucune musique','Ajoutez des titres à votre bibliothèque.')}</section>`; }
        homePage() {
            const favorites=this.tracks.filter(track=>track.favorite), recent=this.recent(), added=this.tracks.slice(0,8);
            return `${this.header('Paradise Tunes',this.greeting())}<div class="pt-scroll">${recent.length?this.section('Écoutés récemment',recent):''}${favorites.length?this.section('Vos favoris',favorites):''}${this.playlists.length?`<section class="pt-section"><h2>Playlists Paradise</h2><div class="pt-horizontal">${this.playlists.map(p=>this.playlistCard(p)).join('')}</div></section>`:''}${this.section('Récemment ajoutés',added)}</div>`;
        }
        searchPage() {
            const q=this.query.trim().toLowerCase(), results=q?this.tracks.filter(t=>`${t.title} ${t.artist} ${t.genre}`.toLowerCase().includes(q)):[];
            const playlists=q?this.playlists.filter(p=>p.name.toLowerCase().includes(q)):[];
            return `${this.header('Rechercher')}<div class="pt-search"><span>⌕</span><input data-pt-search value="${esc(this.query)}" placeholder="Que voulez-vous écouter ?" autofocus></div><div class="pt-scroll pt-results">${!q?this.empty('Trouvez votre musique','Recherchez un titre, un artiste ou une playlist.'):results.length||playlists.length?`${playlists.map(p=>this.playlistRow(p)).join('')}${results.map(t=>this.row(t)).join('')}`:this.empty('Aucun résultat trouvé','Essayez une autre recherche.')}</div>`;
        }
        libraryPage() {
            const favorites=this.tracks.filter(track=>track.favorite), shown=this.filter==='favorites'?favorites:this.tracks;
            return `${this.header('Votre bibliothèque')}<div class="pt-filters"><button class="${this.filter==='tracks'?'active':''}" data-pt-filter="tracks">Titres</button><button class="${this.filter==='playlists'?'active':''}" data-pt-filter="playlists">Playlists</button><button class="${this.filter==='favorites'?'active':''}" data-pt-filter="favorites">Favoris</button></div><div class="pt-scroll pt-library">${this.filter==='playlists'?(this.playlists.length?this.playlists.map(p=>this.playlistRow(p)).join(''):this.empty('Aucune playlist','Créez votre première playlist.')):(shown.length?shown.map(t=>this.row(t)).join(''):this.empty(this.filter==='favorites'?'Aucun favori':'Votre bibliothèque est vide',this.filter==='favorites'?'Ajoutez des titres avec le cœur.':'Ajoutez votre première musique pour commencer.'))}</div>`;
        }
        playlistCard(p) { const first=this.tracks.find(t=>t.id===p.trackIds[0]); return `<button type="button" class="pt-card" data-pt-playlist="${p.id}">${this.coverHtml({coverUrl:p.coverUrl||first?.coverUrl})}<strong>${esc(p.name)}</strong><span>${p.trackIds.length} titre${p.trackIds.length>1?'s':''}</span></button>`; }
        playlistRow(p) { return `<button type="button" class="pt-playlist-row" data-pt-playlist="${p.id}"><span class="pt-cover"><i>PT</i></span><span><strong>${esc(p.name)}</strong><small>${p.trackIds.length} titre${p.trackIds.length>1?'s':''}</small></span><b>›</b></button>`; }
        playlistPage(id) { const p=this.playlists.find(item=>item.id===id); if(!p)return this.empty('Playlist introuvable',''); const tracks=p.trackIds.map(trackId=>this.tracks.find(t=>t.id===trackId)).filter(Boolean); return `<header class="pt-subheader"><button data-pt-close-overlay>‹</button><span>Playlist</span><button data-pt-rename-playlist="${p.id}">✎</button></header><div class="pt-scroll pt-playlist-page"><div class="pt-playlist-hero"><span class="pt-cover pt-cover-large"><i>PT</i></span><h1>${esc(p.name)}</h1><p>Créée par ${esc(this.user.username)} · ${tracks.length} titre${tracks.length>1?'s':''}</p><button data-pt-play-all="${p.id}" ${tracks.length?'':'disabled'}>▶</button><button class="pt-playlist-delete" data-pt-delete-playlist="${p.id}">Supprimer la playlist</button></div>${tracks.length?tracks.map(t=>this.row(t)).join(''):this.empty('Cette playlist est vide','Ajoutez des titres depuis leur menu.')}</div>`; }
        playerPage() { const t=this.current; if(!t){this.overlay=null;return this.homePage();} const videoId=youtubeId(t.audioUrl); const media=videoId?`<div class="pt-youtube-now"><iframe data-pt-youtube src="https://www.youtube-nocookie.com/embed/${encodeURIComponent(videoId)}?autoplay=1&enablejsapi=1&playsinline=1&controls=1&origin=${encodeURIComponent(location.origin)}" title="${esc(t.title)}" allow="autoplay; encrypted-media; picture-in-picture" allowfullscreen></iframe></div>`:this.coverHtml(t,'pt-cover-now'); return `<div class="pt-player-page"><header class="pt-subheader"><button data-pt-close-overlay>⌄</button><span>Lecture en cours</span><i></i></header>${media}<div class="pt-now-meta"><strong>${esc(t.title)}</strong><span>${esc(t.artist||'Artiste inconnu')}</span><button data-pt-favorite="${t.id}">${t.favorite?'♥':'♡'}</button></div><input class="pt-progress" type="range" min="0" max="${this.duration||0}" value="${this.currentTime}" data-pt-seek><div class="pt-times"><span>${fmt(this.currentTime)}</span><span>${fmt(this.duration)}</span></div><div class="pt-controls"><button class="${this.shuffle?'active':''}" data-pt-shuffle>⌘</button><button data-pt-prev>◀</button><button class="pt-play-large" data-pt-toggle>${this.playing?'Ⅱ':'▶'}</button><button data-pt-next>▶</button><button class="${this.repeat?'active':''}" data-pt-repeat>${this.repeat===2?'1↻':'↻'}</button></div><div class="pt-volume"><span>−</span><input type="range" min="0" max="1" step=".05" value="${this.volume}" data-pt-volume><span>＋</span></div></div>`; }
        overlayHtml() { if(this.overlay==='add')return `<div class="pt-modal"><form data-pt-track-form><header><strong>Ajouter une musique</strong><button type="button" data-pt-close-modal>×</button></header><input name="title" maxlength="100" required placeholder="Titre *"><input name="artist" maxlength="80" placeholder="Artiste"><input name="audioUrl" type="url" required placeholder="URL audio HTTPS *"><input name="coverUrl" type="url" placeholder="URL de la pochette"><input name="genre" maxlength="40" placeholder="Genre"><textarea name="description" maxlength="500" placeholder="Description"></textarea><button type="submit">Ajouter</button><small data-pt-form-status></small></form></div>`; if(this.overlay==='new-playlist')return `<div class="pt-modal"><form data-pt-playlist-form><header><strong>Créer une playlist</strong><button type="button" data-pt-close-modal>×</button></header><input name="name" maxlength="80" required placeholder="Nom de la playlist"><input name="coverUrl" type="url" placeholder="URL de la pochette"><button type="submit">Créer</button><small data-pt-form-status></small></form></div>`; return this.menuTrack?this.trackMenu(this.menuTrack):''; }
        trackMenu(track) { const activePlaylist=this.overlay?.type==='playlist'?this.playlists.find(p=>p.id===this.overlay.id):null; return `<div class="pt-menu-backdrop" data-pt-close-menu><div class="pt-menu" onclick="event.stopPropagation()"><strong>${esc(track.title)}</strong><button data-pt-favorite="${track.id}">${track.favorite?'Retirer des favoris':'Ajouter aux favoris'}</button><button data-pt-queue="${track.id}">Lire ensuite</button>${activePlaylist?.trackIds.includes(track.id)?`<button data-pt-remove-playlist="${activePlaylist.id}" data-track="${track.id}">Retirer de cette playlist</button>`:''}${this.playlists.map(p=>`<button data-pt-add-playlist="${p.id}" data-track="${track.id}">Ajouter à ${esc(p.name)}</button>`).join('')}<button data-pt-new-playlist>Créer une playlist</button>${track.ownerId===this.user.id?`<button class="danger" data-pt-delete="${track.id}">Supprimer</button>`:''}</div></div>`; }
        chrome() { if(this.overlay==='player')return ''; return `${this.current?`<div class="pt-mini" data-pt-open-player>${this.coverHtml(this.current)}<span><strong>${esc(this.current.title)}</strong><small>${esc(this.current.artist||'Artiste inconnu')}</small></span><button data-pt-toggle>${this.playing?'Ⅱ':'▶'}</button><i style="width:${this.duration?Math.min(100,this.currentTime/this.duration*100):0}%"></i></div>`:''}<nav class="pt-nav"><button class="${this.tab==='home'?'active':''}" data-pt-tab="home"><i>⌂</i>Accueil</button><button class="${this.tab==='search'?'active':''}" data-pt-tab="search"><i>⌕</i>Recherche</button><button class="${this.tab==='library'?'active':''}" data-pt-tab="library"><i>▤</i>Bibliothèque</button></nav>`; }
        renderChrome() { const node=this.root?.querySelector('[data-pt-chrome]'); if(node)node.innerHTML=this.chrome(); this.updatePlayer(); }
        updatePlayer() { const progress=this.root?.querySelector('[data-pt-seek]'); if(progress&&!progress.matches(':active')){progress.max=String(this.duration||0);progress.value=String(this.currentTime||0);} const mini=this.root?.querySelector('.pt-mini>i');if(mini)mini.style.width=`${this.duration?Math.min(100,this.currentTime/this.duration*100):0}%`; const times=this.root?.querySelector('.pt-times');if(times)times.innerHTML=`<span>${fmt(this.currentTime)}</span><span>${fmt(this.duration)}</span>`; }
        async play(track, queue=this.tracks, force=false) { if(!track)return; this.queue=queue;this.queueIndex=Math.max(0,queue.findIndex(t=>t.id===track.id)); if(!force&&this.current?.id===track.id){return this.toggle();} this.current=track;this.addRecent(track.id);this.currentTime=0;this.duration=0;if(youtubeId(track.audioUrl)){this.audio.pause();this.playing=true;this.overlay='player';this.render();return;}this.audio.src=track.audioUrl;try{await this.audio.play();}catch{this.notice('Cette source audio n’est pas compatible.');}this.render(); }
        youtubeCommand(command, args=[]) { const frame=this.root?.querySelector('[data-pt-youtube]');frame?.contentWindow?.postMessage(JSON.stringify({event:'command',func:command,args}), 'https://www.youtube-nocookie.com'); }
        youtubeMessage(event) { if(!/^(?:https:\/\/)?(?:www\.)?(?:youtube(?:-nocookie)?\.com)$/i.test(event.origin))return;let data=event.data;try{if(typeof data==='string')data=JSON.parse(data);}catch{return;}const info=data?.info;if(data?.event!=='infoDelivery'||!info)return;if(Number.isFinite(info.currentTime))this.currentTime=info.currentTime;if(Number.isFinite(info.duration))this.duration=info.duration;if(info.playerState===1)this.playing=true;if(info.playerState===2)this.playing=false;if(info.playerState===0)this.next(true);const toggle=this.root?.querySelector('.pt-play-large');if(toggle)toggle.textContent=this.playing?'Ⅱ':'▶';this.updatePlayer(); }
        toggle(){if(!this.current)return;if(youtubeId(this.current.audioUrl)){this.youtubeCommand(this.playing?'pauseVideo':'playVideo');this.playing=!this.playing;this.renderChrome();return;}this.audio.paused?this.audio.play().catch(()=>this.notice('Lecture impossible.')):this.audio.pause();}
        back(){if(this.menuTrack){this.menuTrack=null;this.render();return true;}if(this.overlay){this.overlay=null;this.render();return true;}if(this.tab!=='home'){const destination=this.previousTab||'home';this.previousTab=this.tab;this.tab=destination;this.render();return true;}return false;}
        next(auto=false){if(!this.queue.length)return;if(this.shuffle)this.queueIndex=Math.floor(Math.random()*this.queue.length);else this.queueIndex++;if(this.queueIndex>=this.queue.length){if(this.repeat===1)this.queueIndex=0;else{this.queueIndex=this.queue.length-1;if(auto)this.audio.pause();return;}}this.play(this.queue[this.queueIndex],this.queue,true);}
        prev(){if(this.audio.currentTime>4){this.audio.currentTime=0;return;}this.queueIndex=Math.max(0,this.queueIndex-1);this.play(this.queue[this.queueIndex],this.queue,true);}
        notice(message){let node=this.root?.querySelector('.pt-toast');if(!node){node=document.createElement('div');node.className='pt-toast';this.root?.append(node);}node.textContent=message;setTimeout(()=>node.remove(),3500);}
        error(message){this.root.innerHTML=`<div class="pt-error"><strong>Impossible de charger Paradise Tunes.</strong><span>${esc(message)}</span><button data-pt-retry>Réessayer</button></div>`;}
        async click(event) {
            const el=event.target.closest('button,[data-pt-open-player],[data-pt-close-menu]');if(!el)return;
            if(el.dataset.ptRetry!==undefined)return this.mountRetry(); if(el.dataset.ptTab){if(el.dataset.ptTab!==this.tab)this.previousTab=this.tab;this.tab=el.dataset.ptTab;this.overlay=null;return this.render();}
            if(el.dataset.ptAdd!==undefined){this.overlay=this.tab==='library'&&this.filter==='playlists'?'new-playlist':'add';return this.render();} if(el.dataset.ptCloseModal!==undefined){this.overlay=null;return this.render();}
            if(el.dataset.ptPlay){const t=this.tracks.find(t=>t.id===+el.dataset.ptPlay);return this.play(t,this.tracks);}
            if(el.dataset.ptToggle!==undefined){event.stopPropagation();return this.toggle();} if(el.dataset.ptNext!==undefined)return this.next();if(el.dataset.ptPrev!==undefined)return this.prev();
            if(el.dataset.ptOpenPlayer!==undefined){this.overlay='player';return this.render();} if(el.dataset.ptCloseOverlay!==undefined){this.overlay=null;return this.render();}
            if(el.dataset.ptShuffle!==undefined){this.shuffle=!this.shuffle;return this.render();}if(el.dataset.ptRepeat!==undefined){this.repeat=(this.repeat+1)%3;return this.render();}
            if(el.dataset.ptFilter){this.filter=el.dataset.ptFilter;return this.render();} if(el.dataset.ptPlaylist){this.overlay={type:'playlist',id:+el.dataset.ptPlaylist};return this.render();}
            if(el.dataset.ptPlayAll){const p=this.playlists.find(p=>p.id===+el.dataset.ptPlayAll),q=p.trackIds.map(id=>this.tracks.find(t=>t.id===id)).filter(Boolean);return this.play(q[0],q);}
            if(el.dataset.ptMenu){this.menuTrack=this.tracks.find(t=>t.id===+el.dataset.ptMenu);return this.render();}if(el.dataset.ptCloseMenu!==undefined){this.menuTrack=null;return this.render();}
            if(el.dataset.ptNewPlaylist!==undefined){this.menuTrack=null;this.overlay='new-playlist';return this.render();}
            if(el.dataset.ptFavorite){const t=this.tracks.find(t=>t.id===+el.dataset.ptFavorite);if(!t)return;try{const d=await this.action({action:'favorite.toggle',trackId:t.id});t.favorite=d.favorite;this.menuTrack=null;this.render();}catch(e){this.notice(e.message);}return;}
            if(el.dataset.ptQueue){const t=this.tracks.find(t=>t.id===+el.dataset.ptQueue);this.queue.splice(Math.max(0,this.queueIndex+1),0,t);this.menuTrack=null;this.notice('Ajouté à la file d’attente.');this.render();return;}
            if(el.dataset.ptAddPlaylist){try{await this.action({action:'playlist.add',playlistId:+el.dataset.ptAddPlaylist,trackId:+el.dataset.track});this.menuTrack=null;await this.load();this.notice('Ajouté à la playlist.');}catch(e){this.notice(e.message);}return;}
            if(el.dataset.ptRemovePlaylist){try{await this.action({action:'playlist.remove',playlistId:+el.dataset.ptRemovePlaylist,trackId:+el.dataset.track});this.menuTrack=null;await this.load();this.notice('Retiré de la playlist.');}catch(e){this.notice(e.message);}return;}
            if(el.dataset.ptDelete){if(!confirm('Supprimer cette musique ?'))return;try{await this.action({action:'track.delete',trackId:+el.dataset.ptDelete});this.menuTrack=null;await this.load();}catch(e){this.notice(e.message);}return;}
            if(el.dataset.ptDeletePlaylist){if(!confirm('Supprimer cette playlist ?'))return;try{await this.action({action:'playlist.delete',playlistId:+el.dataset.ptDeletePlaylist});this.overlay=null;await this.load();}catch(e){this.notice(e.message);}return;}
            if(el.dataset.ptRenamePlaylist){const playlist=this.playlists.find(p=>p.id===+el.dataset.ptRenamePlaylist),name=prompt('Nouveau nom de la playlist',playlist?.name||'');if(!name?.trim())return;try{await this.action({action:'playlist.rename',playlistId:playlist.id,name:name.trim()});await this.load();this.overlay={type:'playlist',id:playlist.id};this.render();}catch(e){this.notice(e.message);}}
        }
        input(event){if(event.target.dataset.ptSearch!==undefined){this.query=event.target.value;const pos=event.target.selectionStart;this.render();const input=this.root.querySelector('[data-pt-search]');input?.focus();input?.setSelectionRange(pos,pos);}if(event.target.dataset.ptSeek!==undefined){if(youtubeId(this.current?.audioUrl))this.youtubeCommand('seekTo',[+event.target.value,true]);else this.audio.currentTime=+event.target.value;}if(event.target.dataset.ptVolume!==undefined){this.volume=+event.target.value;this.audio.volume=this.volume;if(youtubeId(this.current?.audioUrl))this.youtubeCommand('setVolume',[Math.round(this.volume*100)]);}}
        async submit(event){event.preventDefault();const form=event.target,status=form.querySelector('[data-pt-form-status]'),button=form.querySelector('button[type=submit]');button.disabled=true;try{const d=new FormData(form);if(form.dataset.ptTrackForm!==undefined)await this.action({action:'track.create',title:d.get('title'),artist:d.get('artist'),audioUrl:d.get('audioUrl'),coverUrl:d.get('coverUrl'),genre:d.get('genre'),description:d.get('description')});else await this.action({action:'playlist.create',name:d.get('name'),coverUrl:d.get('coverUrl')});this.overlay=null;await this.load();this.notice(form.dataset.ptTrackForm!==undefined?'Musique ajoutée à votre bibliothèque.':'Playlist créée.');}catch(e){status.textContent=e.message;button.disabled=false;}}
        mountRetry(){delete this.root.dataset.tunesPremium;this.mount(this.root);}
    }

    const app = window.__PARADISE_TUNES_PLAYER__ = new ParadiseTunes();
    const scan = root => { const candidates=[...(root.matches?.('.app-wavetunes .phone-coming-soon')?[root]:[]),...root.querySelectorAll?.('.app-wavetunes .phone-coming-soon')||[]];candidates.forEach(node=>{node.className='phone-coming-soon ppr-app ppr-tunes';node.closest('.app-wavetunes')?.classList.add('app-paradise-tunes');});const nodes=[...(root.matches?.('.ppr-tunes')?[root]:[]),...root.querySelectorAll?.('.ppr-tunes')||[]];nodes.forEach(node=>app.mount(node));document.querySelectorAll('.phone-app-icon .label').forEach(label=>{if(label.textContent.trim()==='Wave Tunes')label.textContent='Paradise Tunes';}); };
    const observer=new MutationObserver(records=>records.forEach(record=>record.addedNodes.forEach(node=>node.nodeType===1&&scan(node))));
    const boot=()=>{observer.observe(document.getElementById('root')||document.body,{childList:true,subtree:true});scan(document.body);};
    document.addEventListener('click',event=>{const button=event.target.closest('.phone-app-home');if(!button||!app.root?.isConnected||!button.closest('.phone-active-app')?.contains(app.root))return;if(app.back()){event.preventDefault();event.stopImmediatePropagation();}},true);
    document.readyState==='loading'?document.addEventListener('DOMContentLoaded',boot,{once:true}):boot();
})();
