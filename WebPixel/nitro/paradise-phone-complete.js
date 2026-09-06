(() => {
    'use strict';

    if (window.__PARADISE_PHONE_COMPLETE__) return;
    window.__PARADISE_PHONE_COMPLETE__ = '1.5.1';

    const API = '/nitro/phone-api.php';
    const runtime = window.__PARADISE_PHONE_RUNTIME__ = window.__PARADISE_PHONE_RUNTIME__ || { photos: [] };
    const translations = new Map([
        ['friendlist.search.label', 'Rechercher un ami'],
        ['friendlist.friends.online', 'Amis en ligne'],
        ['friendlist.friends', 'Amis hors ligne'],
        ['friendlist.search.nothingfound', 'Aucun ami trouvé'],
        ['friendlist.search.nofriendsfound', 'Aucun ami trouvé'],
        ['friendlist.friendrequests', "Demandes d'amis"],
        ['friendlist.tip.im', 'Envoyer un message'],
        ['friendlist.tip.remove', "Supprimer l'ami"],
        ['friendlist.tip.accept', 'Accepter'],
        ['friendlist.tip.decline', 'Refuser'],
        ['friendlist.tooltip.online', 'En ligne'],
        ['friendlist.tooltip.offline', 'Hors ligne'],
        ['camera.gallery.title', 'Galerie'],
        ['camera.gallery.photo', 'Photo'],
        ['camera.gallery.taken', 'Prise le'],
        ['camera.gallery.empty', "Aucune photo. Prenez-en une avec l'appareil photo."],
        ['generic.loading', 'Chargement...'],
        ['generic.back', 'Retour'],
        ['generic.done', 'Terminé'],
        ['Camera', 'Appareil photo'],
        ['Gallery', 'Galerie'],
        ['Settings', 'Paramètres'],
        ['Wave Tunes', 'Paradise Tunes'],
        ['Waver Gram', 'Paradise Gram'],
        ['Recipient', 'Destinataire'],
        ['Amount', 'Montant'],
        ['Wallet:', 'Espèces :'],
        ['Bank:', 'Banque :'],
        ['Total Funds:', 'Solde total :'],
        ['Not enough in wallet', "Pas assez d'espèces"],
        ['Not enough in bank', 'Solde bancaire insuffisant'],
        ['Insufficient funds', 'Solde insuffisant']
    ]);

    const escapeHtml = value => String(value ?? '')
        .replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;').replaceAll("'", '&#039;');

    async function request(action, options = {}) {
        const controller = new AbortController();
        const timeout = setTimeout(() => controller.abort(), options.timeout || 8000);
        try {
            const response = await fetch(`${API}?action=${encodeURIComponent(action)}`, {
                credentials: 'same-origin', cache: 'no-store', signal: controller.signal,
                ...options
            });
            const payload = await response.json().catch(() => null);
            if (!response.ok || !payload?.ok) throw new Error(payload?.error || 'Impossible de charger cette application.');
            return payload;
        } catch (error) {
            if (error?.name === 'AbortError') throw new Error('Le serveur ne répond pas. Réessayez.');
            throw error;
        } finally {
            clearTimeout(timeout);
        }
    }

    function translate(root) {
        if (!root || root.nodeType !== Node.ELEMENT_NODE) return;
        const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
        const nodes = [];
        while (walker.nextNode()) nodes.push(walker.currentNode);
        for (const node of nodes) {
            const raw = node.nodeValue?.trim();
            if (raw && translations.has(raw)) node.nodeValue = node.nodeValue.replace(raw, translations.get(raw));
        }
        root.querySelectorAll('[placeholder],[title]').forEach(node => {
            for (const name of ['placeholder', 'title']) {
                const value = node.getAttribute(name);
                if (translations.has(value)) node.setAttribute(name, translations.get(value));
            }
        });
    }

    function errorView(message) {
        return `<div class="ppr-state ppr-error"><strong>Impossible de charger cette application.</strong><span>${escapeHtml(message)}</span><button type="button" data-ppr-retry>Réessayer</button></div>`;
    }

    function formatDate(timestamp) {
        const date = new Date(Number(timestamp) * 1000);
        return Number.isNaN(date.getTime()) ? '' : date.toLocaleString('fr-FR', { dateStyle: 'short', timeStyle: 'short' });
    }

    function avatar(look, name, size = 'm') {
        return `<img src="/avatar.php?figure=${encodeURIComponent(look || '')}&size=${encodeURIComponent(size)}&direction=2&head_direction=2" alt="${escapeHtml(name)}" loading="lazy">`;
    }

    function headAvatar(look, name) {
        return `<span class="pg-nav-avatar"><img src="/avatar.php?figure=${encodeURIComponent(look || '')}&size=m&direction=2&head_direction=2&headonly=1&gesture=sml" alt="${escapeHtml(name)}"></span>`;
    }

    function youtubeId(value) {
        try {
            const url = new URL(value);
            let id = null;
            if (url.hostname === 'youtu.be') id = url.pathname.slice(1).split('/')[0] || null;
            if (/(^|\.)youtube\.com$/i.test(url.hostname)) {
                if (url.pathname === '/watch') id = url.searchParams.get('v');
                const match = url.pathname.match(/^\/(?:shorts|embed|live)\/([^/?]+)/);
                id ||= match?.[1] || null;
            }
            return /^[A-Za-z0-9_-]{11}$/.test(id || '') ? id : null;
        } catch {}
        return null;
    }

    const tunes = {
        key: 'paradise.phone.tunes.v1', audio: new Audio(), current: null, root: null, timer: 0,
        list() { try { const value = JSON.parse(localStorage.getItem(this.key) || '[]'); return Array.isArray(value) ? value : []; } catch { return []; } },
        save(value) { localStorage.setItem(this.key, JSON.stringify(value.slice(0, 50))); },
        mount(root) {
            this.root = root;
            root.className = 'phone-coming-soon ppr-app ppr-tunes';
            root.dataset.pprReady = '1';
            this.render();
            root.addEventListener('submit', event => {
                const form = event.target.closest('[data-tunes-add]');
                if (!form) return;
                event.preventDefault();
                const data = new FormData(form), title = String(data.get('title') || '').trim(), url = String(data.get('url') || '').trim();
                try {
                    const parsed = new URL(url);
                    if (parsed.protocol !== 'https:') throw new Error();
                    const list = this.list();
                    if (list.some(track => track.url === parsed.href)) return this.notice('Ce morceau existe déjà.');
                    list.unshift({ id: `${Date.now()}`, title: title || parsed.hostname, url: parsed.href, youtubeId: youtubeId(parsed.href) });
                    this.save(list); this.render();
                } catch { this.notice('Ajoutez une URL audio HTTPS valide.'); }
            });
            root.addEventListener('click', event => {
                const play = event.target.closest('[data-tunes-play]');
                const remove = event.target.closest('[data-tunes-remove]');
                if (play) this.toggle(play.dataset.tunesPlay);
                if (remove) { const list = this.list().filter(track => track.id !== remove.dataset.tunesRemove); this.save(list); if (this.current === remove.dataset.tunesRemove) this.stop(); this.render(); }
            });
            root.addEventListener('input', event => {
                if (event.target.matches('[data-tunes-volume]')) this.audio.volume = Number(event.target.value);
                if (event.target.matches('[data-tunes-progress]') && Number.isFinite(this.audio.duration)) this.audio.currentTime = Number(event.target.value) * this.audio.duration;
            });
            this.audio.preload = 'metadata';
            this.audio.addEventListener('error', () => this.notice('Lecture impossible pour cette URL.'));
            this.audio.addEventListener('ended', () => { this.current = null; this.render(); });
        },
        notice(message) { const node = this.root?.querySelector('[data-tunes-status]'); if (node) node.textContent = message; },
        stop() { this.audio.pause(); this.audio.removeAttribute('src'); this.current = null; clearInterval(this.timer); },
        async toggle(id) {
            const track = this.list().find(item => item.id === id);
            if (!track) return;
            const videoId = track.youtubeId || youtubeId(track.url);
            if (videoId) {
                this.audio.pause();
                this.current = this.current === id ? null : id;
                this.render();
                return;
            }
            if (this.current === id && !this.audio.paused) { this.audio.pause(); return this.render(); }
            if (this.current !== id) { this.audio.src = track.url; this.current = id; }
            try {
                await this.audio.play();
                clearInterval(this.timer);
                this.timer = setInterval(() => this.updateProgress(), 500);
                this.render();
            } catch { this.notice('Le navigateur a refusé la lecture de ce fichier.'); }
        },
        updateProgress() {
            const input = this.root?.querySelector('[data-tunes-progress]');
            if (input && Number.isFinite(this.audio.duration) && this.audio.duration > 0) input.value = String(this.audio.currentTime / this.audio.duration);
        },
        render() {
            if (!this.root) return;
            const list = this.list();
            const currentTrack = list.find(track => track.id === this.current);
            const currentYoutubeId = currentTrack && (currentTrack.youtubeId || youtubeId(currentTrack.url));
            this.root.innerHTML = `<header class="ppr-header"><strong>Paradise Tunes</strong><span data-tunes-status>${this.current ? 'Lecture en cours' : 'Bibliothèque'}</span></header>
                <form class="ppr-tunes-add" data-tunes-add><input name="title" maxlength="80" placeholder="Titre"><input name="url" type="url" required placeholder="Lien YouTube ou audio HTTPS"><button type="submit">Ajouter</button></form>
                ${currentYoutubeId ? `<div class="ppr-youtube"><iframe src="https://www.youtube-nocookie.com/embed/${encodeURIComponent(currentYoutubeId)}?autoplay=1&playsinline=1" title="${escapeHtml(currentTrack.title)}" allow="autoplay; encrypted-media; picture-in-picture" allowfullscreen></iframe></div>` : `<div class="ppr-tunes-controls"><input type="range" min="0" max="1" step="0.01" value="0" data-tunes-progress aria-label="Progression"><label>Volume <input type="range" min="0" max="1" step="0.05" value="${this.audio.volume}" data-tunes-volume></label></div>`}
                <div class="ppr-scroll ppr-track-list">${list.length ? list.map(track => `<div class="ppr-track"><button type="button" class="ppr-play" data-tunes-play="${track.id}" title="Lecture ou pause">${this.current === track.id && !this.audio.paused ? 'Ⅱ' : '▶'}</button><div><strong>${escapeHtml(track.title)}</strong><small>${escapeHtml(new URL(track.url).hostname)}</small></div><button type="button" class="ppr-icon-danger" data-tunes-remove="${track.id}" title="Supprimer">×</button></div>`).join('') : '<div class="ppr-state"><strong>Aucun morceau</strong><span>Ajoutez une URL audio HTTPS pour créer votre bibliothèque.</span></div>'}</div>`;
        }
    };

    const gram = {
        root: null, csrf: '', data: null, loading: false, view: 'home', viewHistory: [], selectedPhotoId: 0, profileUserId: 0, connectionType: 'followers', connectionUserId: 0, connections: [], connectionsLoading: false,
        async mount(root) {
            this.root = root;
            root.className = 'phone-coming-soon ppr-app ppr-gram pg-v2';
            root.dataset.pprReady = '1';
            root.addEventListener('submit', event => this.submit(event));
            root.addEventListener('click', event => this.click(event));
            root.addEventListener('input', event => this.filterPeople(event));
            await this.load();
        },
        async load() {
            if (!this.root || this.loading) return;
            this.loading = true;
            if (!this.data) this.root.innerHTML = '<div class="ppr-state"><span class="ppr-loader"></span><strong>Chargement de Paradise Gram...</strong></div>';
            try {
                this.data = await request('feed');
                this.csrf = this.data.csrf;
                if (!this.profileUserId) this.profileUserId = Number(this.data.me?.id || 0);
                this.render();
            } catch (error) { this.root.innerHTML = errorView(error.message); }
            finally { this.loading = false; }
        },
        async action(payload) {
            return request('feed', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ ...payload, csrf: this.csrf }) });
        },
        snapshot() {
            return { view: this.view, profileUserId: this.profileUserId, connectionType: this.connectionType, connectionUserId: this.connectionUserId };
        },
        navigate(view, changes = {}) {
            if (view !== this.view || Object.keys(changes).some(key => this[key] !== changes[key])) this.viewHistory.push(this.snapshot());
            Object.assign(this, changes);
            this.view = view;
            this.render();
        },
        back() {
            const viewerBack = this.root?.querySelector('[data-pg-viewer-back]');
            if (viewerBack) { viewerBack.click(); return true; }
            const previous = this.viewHistory.pop();
            if (!previous) return false;
            Object.assign(this, previous);
            this.render();
            return true;
        },
        async openConnections(userId, type) {
            this.connectionUserId = Number(userId || this.data?.me?.id || 0);
            this.connectionType = type === 'following' ? 'following' : 'followers';
            this.connections = [];
            this.connectionsLoading = true;
            this.navigate('connections');
            try {
                const response = await fetch(`${API}?action=connections&userId=${this.connectionUserId}&type=${this.connectionType}`, { credentials:'same-origin', cache:'no-store' });
                const payload = await response.json().catch(() => null);
                if (!response.ok || !payload?.ok) throw new Error(payload?.error || 'Impossible de charger cette liste.');
                this.connections = Array.isArray(payload.people) ? payload.people : [];
                this.connectionsLoading = false;
                this.render();
            } catch (error) {
                this.connections = null;
                this.connectionsLoading = false;
                this.render();
            }
        },
        async submit(event) {
            const publish = event.target.closest('[data-pg-publish]');
            const comment = event.target.closest('[data-pg-comment]');
            if (!publish && !comment) return;
            event.preventDefault();
            const button = event.target.querySelector('button[type="submit"]');
            if (button?.disabled) return;
            if (button) button.disabled = true;
            try {
                const data = new FormData(event.target);
                if (publish) {
                    if (!this.selectedPhotoId) throw new Error('Choisissez une photo de votre galerie.');
                    await this.action({ action: 'create', photoId: this.selectedPhotoId, body: String(data.get('body') || '') });
                    this.selectedPhotoId = 0;
                    this.navigate('home');
                } else {
                    await this.action({ action: 'comment', postId: Number(comment.dataset.pgComment), body: String(data.get('body') || '') });
                }
                await this.load();
            } catch (error) { this.notice(error.message); if (button) button.disabled = false; }
        },
        async click(event) {
            if (event.target.closest('[data-ppr-retry]')) return this.load();
            if (event.target.closest('[data-pg-connections-back]')) return this.back();
            if (event.target.closest('[data-pg-back]')) return this.back();
            const connections = event.target.closest('[data-pg-connections]');
            if (connections) return this.openConnections(Number(connections.dataset.pgUser), connections.dataset.pgConnections);
            const nav = event.target.closest('[data-pg-nav]');
            if (nav) return this.navigate(nav.dataset.pgNav, nav.dataset.pgNav === 'profile' ? { profileUserId: Number(this.data?.me?.id || 0) } : {});
            const photo = event.target.closest('[data-pg-photo]');
            if (photo) { this.selectedPhotoId = Number(photo.dataset.pgPhoto); return this.render(); }
            const profile = event.target.closest('[data-pg-profile]');
            if (profile) return this.navigate('profile', { profileUserId: Number(profile.dataset.pgProfile) });
            const like = event.target.closest('[data-pg-like]');
            const remove = event.target.closest('[data-pg-delete]');
            const follow = event.target.closest('[data-pg-follow]');
            const removeComment = event.target.closest('[data-pg-delete-comment]');
            if (!like && !remove && !follow && !removeComment) return;
            if (remove && !confirm('Supprimer cette publication ?')) return;
            const button = like || remove || follow || removeComment;
            if (button.disabled) return;
            button.disabled = true;
            try {
                if (like) await this.action({ action: 'like', postId: Number(like.dataset.pgLike) });
                if (remove) await this.action({ action: 'delete', postId: Number(remove.dataset.pgDelete) });
                if (follow) {
                    const result = await this.action({ action: 'follow', targetUserId: Number(follow.dataset.pgFollow) });
                    const targetId = Number(follow.dataset.pgFollow);
                    if (Array.isArray(this.connections)) this.connections.forEach(person => { if (Number(person.id) === targetId) person.following = Boolean(result.following); });
                }
                if (removeComment) await this.action({ action: 'delete_comment', commentId: Number(removeComment.dataset.pgDeleteComment) });
                await this.load();
            } catch (error) { this.notice(error.message); button.disabled = false; }
        },
        notice(message) { const node = this.root?.querySelector('[data-pg-status]'); if (node) node.textContent = message; },
        filterPeople(event) {
            const input = event.target.closest('[data-pg-people-search]');
            if (!input) return;
            const query = input.value.trim().toLocaleLowerCase('fr-FR');
            this.root.querySelectorAll('[data-pg-person-name]').forEach(card => {
                card.hidden = query !== '' && !String(card.dataset.pgPersonName || '').toLocaleLowerCase('fr-FR').includes(query);
            });
        },
        navButton(view, icon, label) {
            const active = this.view === view || (view === 'profile' && this.view === 'connections');
            return `<button type="button" class="${active ? 'active' : ''}" data-pg-nav="${view}" title="${label}">${icon}</button>`;
        },
        render() {
            if (!this.root || !this.data) return;
            let content;
            if (this.view === 'explore') content = this.explore();
            else if (this.view === 'publish') content = this.publish();
            else if (this.view === 'activity') content = this.activity();
            else if (this.view === 'profile') content = this.profile();
            else if (this.view === 'connections') content = this.connectionList();
            else content = this.home();
            const currentProfile = this.findProfile(this.profileUserId);
            const title = this.view === 'home' ? 'Accueil' : this.view === 'explore' ? 'Explorer' : this.view === 'publish' ? 'Publier' : this.view === 'activity' ? 'Activité' : this.view === 'connections' ? (this.connectionType === 'followers' ? 'Abonnés' : 'Abonnements') : `@${currentProfile?.username || this.data.me?.username || 'profil'}`;
            const me = this.data.me || {};
            this.root.innerHTML = `<div class="pg-shell"><header class="pg-top"><strong>ParadiseGram</strong><span data-pg-status>${title}</span></header><main class="pg-body">${content}</main><nav class="pg-nav">${this.navButton('home','⌂','Accueil')}${this.navButton('explore','⌕','Explorer')}${this.navButton('publish','＋','Publier')}${this.navButton('activity','♡','Activité')}${this.navButton('profile',headAvatar(me.look,me.username || 'Profil'),'Profil')}</nav></div>`;
        },
        home() {
            const posts = this.data.posts || [];
            return posts.length ? `<div class="pg-feed">${posts.map(post => this.post(post)).join('')}</div>` : '<div class="pg-empty"><strong>Le fil est vide</strong><span>Publiez une photo depuis votre galerie.</span></div>';
        },
        post(post) {
            const comments = post.comments || [];
            return `<article class="pg-post"><header class="pg-post-head"><button type="button" class="pg-user" data-pg-profile="${post.userId}"><span class="pg-avatar">${avatar(post.look, post.username)}</span><span><strong>${escapeHtml(post.username)}</strong><small>${formatDate(post.createdAt)}</small></span></button>${post.canDelete ? `<button type="button" class="pg-delete" data-pg-delete="${post.id}" title="Supprimer">×</button>` : `<button type="button" class="pg-follow ${post.following ? 'active' : ''}" data-pg-follow="${post.userId}">${post.following ? 'Abonné' : 'Suivre'}</button>`}</header><img class="pg-photo" src="${escapeHtml(post.imageUrl || '')}" alt="Publication de ${escapeHtml(post.username)}" loading="lazy"><div class="pg-actions"><button type="button" class="pg-heart ${post.liked ? 'active' : ''}" data-pg-like="${post.id}">${post.liked ? '♥' : '♡'}</button><span>◯</span></div><div class="pg-likes">${Number(post.likes || 0)} J’aime</div>${post.body ? `<div class="pg-caption"><button type="button" class="pg-caption-user" data-pg-profile="${post.userId}">${escapeHtml(post.username)}</button><span class="pg-caption-text">${escapeHtml(post.body)}</span></div>` : ''}<div class="pg-comments">${comments.slice(-4).map(comment => `<div><span><button type="button" class="pg-comment-user" data-pg-profile="${comment.userId}">${escapeHtml(comment.username)}</button> ${escapeHtml(comment.body)}</span>${comment.canDelete ? `<button type="button" data-pg-delete-comment="${comment.id}" title="Supprimer">×</button>` : ''}</div>`).join('')}</div><form class="pg-comment-form" data-pg-comment="${post.id}"><input name="body" maxlength="240" required placeholder="Ajouter un commentaire…"><button type="submit">Publier</button></form></article>`;
        },
        explore() {
            const posts = [...(this.data.posts || [])].filter(post => post.imageUrl).sort((a, b) => Number(b.likes || 0) - Number(a.likes || 0));
            const people = (this.data.suggestions || []).slice(0, 60);
            return `<section class="pg-explore"><label class="pg-search"><span>⌕</span><input type="search" data-pg-people-search placeholder="Rechercher une personne"></label><h2>Découvrir des profils</h2><div class="pg-people">${people.length ? people.map(person => `<article class="pg-person" data-pg-person-name="${escapeHtml(person.username)}"><button type="button" class="pg-person-main" data-pg-profile="${person.id}"><span class="pg-avatar">${avatar(person.look, person.username)}</span><span><strong>${escapeHtml(person.username)}</strong><small>${Number(person.followers || 0)} abonné${Number(person.followers || 0) > 1 ? 's' : ''} · ${Number(person.posts || 0)} publication${Number(person.posts || 0) > 1 ? 's' : ''}</small></span></button><button type="button" class="pg-follow ${person.following ? 'active' : ''}" data-pg-follow="${person.id}">${person.following ? 'Abonné' : 'Suivre'}</button></article>`).join('') : '<div class="pg-empty">Aucun profil à découvrir.</div>'}</div><h2>Explorer</h2>${posts.length ? `<div class="pg-grid pg-explore-grid">${posts.map(post => `<button type="button" data-pg-profile="${post.userId}" title="Voir le profil de ${escapeHtml(post.username)}"><img src="${escapeHtml(post.imageUrl)}" alt="Photo de ${escapeHtml(post.username)}" loading="lazy"><span>♥ ${Number(post.likes || 0)}</span></button>`).join('')}</div>` : '<div class="pg-empty">Aucune photo à explorer.</div>'}</section>`;
        },
        publish() {
            const gallery = this.data.gallery || [];
            const selected = gallery.find(photo => Number(photo.id) === this.selectedPhotoId);
            return `<section class="pg-publish">${selected ? `<img class="pg-selected" src="${escapeHtml(selected.url)}" alt="Photo sélectionnée">` : '<div class="pg-publish-intro"><strong>Choisissez une photo</strong><span>Uniquement depuis votre galerie ParadiseRP.</span></div>'}<div class="pg-picker">${gallery.length ? gallery.map(photo => `<button type="button" class="${Number(photo.id) === this.selectedPhotoId ? 'active' : ''}" data-pg-photo="${photo.id}"><img src="${escapeHtml(photo.url)}" alt="Photo de galerie" loading="lazy"></button>`).join('') : '<div class="pg-empty">Aucune photo dans votre galerie.</div>'}</div><form class="pg-publish-form" data-pg-publish><textarea name="body" maxlength="500" placeholder="Écrire une légende…"></textarea><button type="submit" ${selected ? '' : 'disabled'}>Publier</button></form></section>`;
        },
        activity() {
            const items = this.data.activity || [];
            const text = item => item.type === 'like' ? 'a aimé votre publication.' : item.type === 'comment' ? 'a commenté votre publication.' : 'a commencé à vous suivre.';
            return items.length ? `<div class="pg-activity">${items.map(item => `<button type="button" class="pg-activity-row" data-pg-profile="${item.userId || 0}"><span class="pg-avatar">${avatar(item.look, item.username)}</span><div><span><strong>${escapeHtml(item.username)}</strong> ${text(item)}</span><small>${formatDate(item.createdAt)}</small></div><b>›</b></button>`).join('')}</div>` : '<div class="pg-empty"><strong>Aucune activité</strong><span>Les likes, commentaires et abonnements apparaîtront ici.</span></div>';
        },
        findProfile(userId) {
            const id = Number(userId || this.data?.me?.id || 0);
            if (id === Number(this.data?.me?.id || 0)) return { ...this.data.me, following: false };
            const suggested = (this.data?.suggestions || []).find(person => Number(person.id) === id);
            const connected = (this.connections || []).find(person => Number(person.id) === id);
            const post = (this.data?.posts || []).find(item => Number(item.userId) === id);
            if (suggested) return suggested;
            if (connected) return connected;
            return post ? { id, username:post.username, look:post.look, posts:0, followers:Number(post.followers || 0), followingCount:Number(post.followingCount || 0), following:Boolean(post.following) } : null;
        },
        connectionList() {
            const label = this.connectionType === 'followers' ? 'Abonnés' : 'Abonnements';
            if (this.connectionsLoading) return `<section class="pg-connections"><button type="button" class="pg-profile-back" data-pg-connections-back>‹ Profil</button><h2>${label}</h2><div class="pg-empty"><span class="ppr-loader"></span><strong>Chargement…</strong></div></section>`;
            if (this.connections === null) return `<section class="pg-connections"><button type="button" class="pg-profile-back" data-pg-connections-back>‹ Profil</button><div class="pg-empty"><strong>Liste indisponible</strong><span>Impossible de charger les ${label.toLocaleLowerCase('fr-FR')}.</span></div></section>`;
            if (!this.connections.length) return `<section class="pg-connections"><button type="button" class="pg-profile-back" data-pg-connections-back>‹ Profil</button><h2>${label}</h2><div class="pg-empty"><strong>${this.connectionType === 'followers' ? 'Aucun abonné' : 'Aucun abonnement'}</strong><span>Cette liste est vide pour le moment.</span></div></section>`;
            const meId = Number(this.data?.me?.id || 0);
            return `<section class="pg-connections"><button type="button" class="pg-profile-back" data-pg-connections-back>‹ Profil</button><h2>${label}</h2><div class="pg-connection-list">${this.connections.map(person => `<article class="pg-person"><button type="button" class="pg-person-main" data-pg-profile="${person.id}"><span class="pg-avatar">${avatar(person.look,person.username)}</span><span><strong>${escapeHtml(person.username)}</strong><small>${Number(person.followers || 0)} abonné${Number(person.followers || 0)>1?'s':''} · ${Number(person.posts || 0)} publication${Number(person.posts || 0)>1?'s':''}</small></span></button>${Number(person.id)===meId?'':`<button type="button" class="pg-follow ${person.following?'active':''}" data-pg-follow="${person.id}">${person.following?'Abonné':'Suivre'}</button>`}</article>`).join('')}</div></section>`;
        },
        profile() {
            const userId = Number(this.profileUserId || this.data.me?.id || 0);
            const posts = (this.data.posts || []).filter(post => Number(post.userId) === userId);
            const isMe = userId === Number(this.data.me?.id || 0);
            const profile = this.findProfile(userId) || { id:userId, username:'Utilisateur', look:'', posts:posts.length, followers:0, followingCount:0, following:false };
            return `<section class="pg-profile"><button type="button" class="pg-profile-back" data-pg-back>‹ Retour</button><header class="pg-profile-head"><span class="pg-profile-avatar">${avatar(profile.look, profile.username, 'l')}</span><div><strong>${escapeHtml(profile.username)}</strong><small>@${escapeHtml(profile.username)}</small>${isMe ? '<span class="pg-me-badge">Votre profil</span>' : `<button type="button" class="pg-profile-follow ${profile.following ? 'active' : ''}" data-pg-follow="${userId}">${profile.following ? 'Abonné ✓' : 'Suivre'}</button>`}</div></header><div class="pg-stats"><span><strong>${Number(profile.posts ?? posts.length)}</strong>publications</span><button type="button" data-pg-connections="followers" data-pg-user="${userId}"><strong>${Number(profile.followers || 0)}</strong>abonnés</button><button type="button" data-pg-connections="following" data-pg-user="${userId}"><strong>${Number(profile.followingCount || 0)}</strong>abonnements</button></div><h2 class="pg-profile-section-title">Publications</h2><div class="pg-grid pg-profile-grid">${posts.length ? posts.map(post => `<button type="button" data-pg-post="${post.id}"><img src="${escapeHtml(post.imageUrl || '')}" alt="Publication de ${escapeHtml(profile.username)}" loading="lazy"></button>`).join('') : '<div class="pg-empty"><strong>Aucune publication</strong><span>Ce profil n’a encore rien publié.</span></div>'}</div></section>`;
        }
    };

    async function mountGallery(root) {
        if (root.dataset.pprReady) return;
        root.dataset.pprReady = '1';
        const render = photos => {
            root.innerHTML = `<div class="pgal-topbar"><span class="pgal-title">Galerie</span><span class="pgal-count">${photos.length}</span></div>${photos.length ? `<div class="pgal-grid">${photos.map((photo, index) => `<button type="button" class="pgal-cell" data-ppr-photo="${index}"><img src="${escapeHtml(photo.url)}" alt="Photo" loading="lazy"></button>`).join('')}</div>` : '<div class="ppr-state"><strong>Aucune photo</strong><span>Prenez une photo avec l’appareil photo.</span></div>'}`;
            root.onclick = event => {
                const button = event.target.closest('[data-ppr-photo]');
                const back = event.target.closest('[data-ppr-gallery-back]');
                if (back) return render(photos);
                if (!button) return;
                const photo = photos[Number(button.dataset.pprPhoto)];
                root.innerHTML = `<div class="pgal-topbar"><button class="pgal-icon" data-ppr-gallery-back title="Retour">‹</button><span class="pgal-title">Photo</span><span></span></div><div class="pgal-viewer"><img src="${escapeHtml(photo.url)}" alt="Photo"></div><div class="pgal-meta"><div class="pgal-meta-label">Prise le</div><div class="pgal-meta-value">${formatDate(photo.timestamp)}</div></div>`;
            };
        };
        root.innerHTML = '<div class="ppr-state"><span class="ppr-loader"></span><strong>Chargement de la galerie...</strong></div>';
        try {
            const payload = await request('gallery');
            const merged = [...runtime.photos, ...(payload.photos || [])].filter((photo, index, all) => photo.url && all.findIndex(other => other.url === photo.url) === index);
            render(merged);
        } catch (error) {
            root.innerHTML = errorView(error.message);
            root.querySelector('[data-ppr-retry]')?.addEventListener('click', () => { delete root.dataset.pprReady; mountGallery(root); });
        }
    }

    async function syncBank(root) {
        if (root.dataset.pprBankLoading) return;
        root.dataset.pprBankLoading = '1';
        try {
            const data = await request('bank');
            const values = root.querySelectorAll('.bank-card .value');
            const setValue = (node, value) => {
                if (!node) return;
                [...node.childNodes].filter(child => child.nodeType === Node.TEXT_NODE).forEach(child => child.remove());
                node.append(document.createTextNode(Number(value || 0).toLocaleString('fr-FR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })));
            };
            setValue(values[0], data.bankBalance);
            setValue(values[1], data.walletBalance);
        } catch (error) {
            root.dataset.pprBankError = error.message;
        } finally {
            delete root.dataset.pprBankLoading;
        }
    }

    function captureCameraPhoto(root) {
        root.querySelectorAll('.pce-preview img, .phone-camera-preview img, .pcam-preview-img').forEach(image => {
            const url = image.currentSrc || image.src;
            if (!url || image.dataset.pprCaptured) return;
            image.dataset.pprCaptured = '1';
            const photo = { url, timestamp: Math.floor(Date.now() / 1000), roomId: 0 };
            if (!runtime.photos.some(item => item.url === url)) runtime.photos.unshift(photo);
        });
    }

    const enhancedCameras = new WeakSet();

    function enhanceCamera(root) {
        captureCameraPhoto(root);
        if (enhancedCameras.has(root)) return;
        enhancedCameras.add(root);
        // Le viseur et la capture sont rendus nativement par Nitro/WebGL. Copier
        // le canvas principal dans un canvas 2D renvoyait des images noires.
    }

    const visualSettings = {
        key: 'paradise.phone.visual.v1',
        defaults: { borderColor: '#0277ff', theme: 'dark', wallpaper: '' },
        read() {
            try { return { ...this.defaults, ...JSON.parse(localStorage.getItem(this.key) || '{}') }; }
            catch { return { ...this.defaults }; }
        },
        write(change) {
            const value = { ...this.read(), ...change };
            localStorage.setItem(this.key, JSON.stringify(value));
            this.apply(value);
        },
        apply(value = this.read()) {
            document.querySelectorAll('.nitro-phone-frame').forEach(frame => {
                frame.dataset.paradiseTheme = value.theme;
                frame.style.setProperty('--phone-border-color', value.borderColor);
                frame.classList.toggle('theme-light', value.theme === 'light');
                frame.classList.toggle('theme-dark', value.theme !== 'light');
                if (value.wallpaper) frame.style.setProperty('--phone-wallpaper', value.wallpaper);
                else frame.style.removeProperty('--phone-wallpaper');
                const screen = frame.querySelector('.phone-screen');
                if (screen) screen.style.setProperty('background-color', value.theme === 'light' ? '#f5f7fa' : '#081321', 'important');
                const wallpaper = frame.querySelector('.phone-wallpaper');
                if (wallpaper) {
                    if (value.wallpaper) wallpaper.style.setProperty('background', value.wallpaper, 'important');
                    else wallpaper.style.removeProperty('background');
                }
                frame.querySelectorAll('.wallpaper-swatch').forEach(swatch => {
                    const background = swatch.style.background || getComputedStyle(swatch).background;
                    swatch.classList.toggle('is-selected', Boolean(value.wallpaper) && background === value.wallpaper);
                });
            });
        }
    };

    function enhanceSettings(root) {
        const settings = root.matches?.('.phone-settings-app') ? root : root.querySelector('.phone-settings-app');
        if (!settings || settings.dataset.pprSettingsReady) return;
        settings.dataset.pprSettingsReady = '1';
        const colorInput = settings.querySelector('input[type="color"]');
        const themeSelect = settings.querySelector('select');
        const current = visualSettings.read();
        const theme = current.theme === 'light' ? 'light' : 'dark';
        const borderColor = current.borderColor;
        visualSettings.apply({ ...current, theme, borderColor });

        if (colorInput) {
            colorInput.classList.add('ppr-native-setting');
            const palette = document.createElement('div');
            palette.className = 'ppr-border-palette';
            palette.setAttribute('aria-label', 'Couleur de la bordure');
            ['#0277ff', '#00a6a6', '#7c4dff', '#db3b5a', '#f39c12', '#202733'].forEach(color => {
                const button = document.createElement('button');
                button.type = 'button';
                button.dataset.phoneBorder = color;
                button.title = color;
                button.style.backgroundColor = color;
                button.classList.toggle('is-selected', color.toLowerCase() === borderColor.toLowerCase());
                palette.append(button);
            });
            colorInput.after(palette);
        }

        if (themeSelect) {
            themeSelect.classList.add('ppr-native-setting');
            const choices = document.createElement('div');
            choices.className = 'ppr-theme-choices';
            choices.innerHTML = `<button type="button" data-phone-theme="light">Jour</button><button type="button" data-phone-theme="dark">Nuit</button>`;
            choices.querySelectorAll('button').forEach(button => {
                button.classList.toggle('is-selected', button.dataset.phoneTheme === theme);
                const activateTheme = event => {
                    event.preventDefault();
                    event.stopPropagation();
                    const nextTheme = button.dataset.phoneTheme;
                    visualSettings.write({ theme: nextTheme });
                    themeSelect.value = nextTheme;
                    choices.querySelectorAll('button').forEach(choice => choice.classList.toggle('is-selected', choice === button));
                };
                button.addEventListener('pointerdown', activateTheme);
                button.addEventListener('click', activateTheme);
            });
            themeSelect.after(choices);
        }
    }

    function enhance(root) {
        translate(root);
        visualSettings.apply();
        enhanceSettings(root);
        const select = selector => [
            ...(root.matches?.(selector) ? [root] : []),
            ...root.querySelectorAll(selector)
        ];
        select('.phone-coming-soon:not([data-ppr-ready])').forEach(node => {
            const title = node.querySelector('.cs-title')?.textContent?.trim();
            if (title === 'Wave Tunes' || title === 'Paradise Tunes') tunes.mount(node);
            if (title === 'Waver Gram' || title === 'Paradise Gram') gram.mount(node);
        });
        select('.phone-gallery:not([data-ppr-ready])').forEach(mountGallery);
        select('.phone-bank-app').forEach(syncBank);
        select('.phone-bank-app').forEach(bank => {
            const summary = bank.querySelector('.bank-summary');
            if (summary && !summary.querySelector('[data-mobile-bank-policy]')) {
                const policy = document.createElement('div');
                policy.dataset.mobileBankPolicy = '1';
                policy.className = 'phone-bank-mobile-policy';
                policy.textContent = 'Dépôt mobile : une opération toutes les 30 minutes. Pour déposer plus souvent ou retirer des espèces, rendez-vous à Paradise Bank ou à un ATM.';
                summary.append(policy);
            }
        });
        select('.phone-camera-shell').forEach(enhanceCamera);
    }

    document.addEventListener('click', event => {
        const appBack = event.target.closest('.phone-app-home');
        if (appBack && gram.root?.isConnected && appBack.closest('.phone-active-app')?.contains(gram.root) && gram.back()) {
            event.preventDefault();
            event.stopImmediatePropagation();
            return;
        }
        const homeIndicator = event.target.closest('.phone-home-indicator');
        if (homeIndicator) {
            event.preventDefault();
            document.querySelector('.phone-app-home')?.click();
            return;
        }
        const wallpaper = event.target.closest('.phone-settings-app .wallpaper-swatch');
        if (wallpaper) {
            const background = wallpaper.style.background || getComputedStyle(wallpaper).background;
            visualSettings.write({ wallpaper: background });
            document.querySelectorAll('.wallpaper-swatch').forEach(node => node.classList.toggle('is-selected', node === wallpaper));
        }
        const reset = event.target.closest('.phone-settings-app button');
        if (reset && reset.textContent.trim() === 'Réinitialiser') visualSettings.write(visualSettings.defaults);
        const borderChoice = event.target.closest('[data-phone-border]');
        if (borderChoice) {
            event.preventDefault();
            const borderColor = borderChoice.dataset.phoneBorder;
            visualSettings.write({ borderColor });
            const input = borderChoice.closest('.phone-settings-app')?.querySelector('input[type="color"]');
            if (input) {
                input.value = borderColor;
                input.dispatchEvent(new Event('input', { bubbles: true }));
            }
            borderChoice.parentElement.querySelectorAll('button').forEach(button => button.classList.toggle('is-selected', button === borderChoice));
            return;
        }
        const removeFriend = event.target.closest('.phone-friends-app .icon-btn.remove');
        if (removeFriend && !confirm('Supprimer cet ami ?')) {
            event.preventDefault(); event.stopImmediatePropagation();
        }
        const bankAction = event.target.closest('.phone-bank-app .form-actions button');
        if (bankAction) {
            if (bankAction.dataset.pprBusy) {
                event.preventDefault();
                event.stopImmediatePropagation();
                return;
            }
            bankAction.dataset.pprBusy = '1';
            bankAction.disabled = true;
            setTimeout(() => {
                delete bankAction.dataset.pprBusy;
                bankAction.disabled = false;
                document.querySelectorAll('.phone-bank-app').forEach(syncBank);
            }, 1400);
        }
        const cameraButton = event.target.closest('.phone-camera-shell button');
        if (cameraButton && /photo|capture/i.test(cameraButton.title || cameraButton.getAttribute('aria-label') || '')) {
            if (cameraButton.dataset.pprBusy) { event.preventDefault(); event.stopImmediatePropagation(); return; }
            cameraButton.dataset.pprBusy = '1'; setTimeout(() => delete cameraButton.dataset.pprBusy, 1200);
        }
    }, true);

    document.addEventListener('input', event => {
        const field = event.target;
        if (!field.closest?.('.phone-settings-app')) return;
        if (field.type === 'color') visualSettings.write({ borderColor: field.value });
        if (field.tagName === 'SELECT') visualSettings.write({ theme: field.value });
    }, true);

    document.addEventListener('change', event => {
        const field = event.target;
        if (!field.closest?.('.phone-settings-app')) return;
        if (field.type === 'color') visualSettings.write({ borderColor: field.value });
        if (field.tagName === 'SELECT') visualSettings.write({ theme: field.value });
    }, true);

    document.addEventListener('wheel', event => {
        if (event.target.closest?.('.nitro-phone-frame .phone-app-body')) event.stopPropagation();
    }, { capture: true, passive: true });

    document.addEventListener('touchmove', event => {
        if (event.target.closest?.('.nitro-phone-frame .phone-app-body')) event.stopPropagation();
    }, { capture: true, passive: true });

    document.addEventListener('focusout', event => {
        const input = event.target;
        if (!(input instanceof HTMLInputElement) || !input.closest('.phone-settings-app') || !/https:\/\/\.\.\./i.test(input.placeholder)) return;
        const value = input.value.trim();
        if (!value) return;
        try {
            const url = new URL(value);
            if (url.protocol !== 'https:') throw new Error();
            input.setCustomValidity('');
            visualSettings.write({ wallpaper: `url("${url.href.replaceAll('"', '%22')}") center/cover no-repeat` });
        } catch {
            input.setCustomValidity('Utilisez une URL HTTPS valide.');
            input.reportValidity();
            event.preventDefault(); event.stopImmediatePropagation();
        }
    }, true);

    const observer = new MutationObserver(records => {
        for (const record of records) {
            record.addedNodes.forEach(node => { if (node.nodeType === Node.ELEMENT_NODE) enhance(node); });
        }
    });

    function boot() {
        observer.observe(document.getElementById('root') || document.body, { childList: true, subtree: true });
        enhance(document.body);
        visualSettings.apply();
    }
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true }); else boot();
})();
