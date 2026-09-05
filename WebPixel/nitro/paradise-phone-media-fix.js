(() => {
    'use strict';
    if (window.__PARADISE_PHONE_MEDIA_FIX__) return;
    window.__PARADISE_PHONE_MEDIA_FIX__ = '1.0.0';

    const API = '/nitro/phone-media-api.php';
    const runtime = window.__PARADISE_PHONE_RUNTIME__ = window.__PARADISE_PHONE_RUNTIME__ || { photos: [] };
    const saving = new WeakSet();
    let csrf = '';
    let serverPhotos = [];
    let lastFeed = null;

    const escapeHtml = value => String(value ?? '')
        .replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;').replaceAll("'", '&#039;');

    async function mediaRequest(action, options = {}) {
        const response = await fetch(`${API}?action=${encodeURIComponent(action)}`, {
            credentials: 'same-origin', cache: 'no-store', ...options
        });
        const payload = await response.json().catch(() => null);
        if (!response.ok || !payload?.ok) throw new Error(payload?.error || 'Galerie indisponible.');
        return payload;
    }

    async function loadServerPhotos() {
        const payload = await mediaRequest('list');
        csrf = payload.csrf || csrf;
        serverPhotos = Array.isArray(payload.photos) ? payload.photos : [];
        mergeRuntime(serverPhotos);
        return serverPhotos;
    }

    function mergeRuntime(photos) {
        for (const photo of photos) {
            const match = runtime.photos.find(item => Number(item.id || 0) === Number(photo.id) || (item.url && item.url === photo.url));
            if (match) Object.assign(match, photo);
            else runtime.photos.push({ ...photo });
        }
        runtime.photos.sort((a, b) => Number(b.timestamp || 0) - Number(a.timestamp || 0));
    }

    async function sourceToDataUrl(source) {
        if (!source) throw new Error('Photo vide.');
        if (source.startsWith('data:image/')) return source;
        if (source.startsWith('blob:')) {
            const blob = await fetch(source).then(response => response.blob());
            return new Promise((resolve, reject) => {
                const reader = new FileReader();
                reader.onload = () => resolve(String(reader.result || ''));
                reader.onerror = () => reject(new Error('Impossible de lire la photo.'));
                reader.readAsDataURL(blob);
            });
        }
        return source;
    }

    async function persistPhoto(photo) {
        if (!photo || Number(photo.id || 0) > 0 || saving.has(photo)) return photo;
        saving.add(photo);
        try {
            if (!csrf) await loadServerPhotos();
            const source = await sourceToDataUrl(String(photo.url || ''));
            const payload = await mediaRequest('save', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    csrf,
                    source,
                    roomId: Number(photo.roomId || 0),
                    timestamp: Number(photo.timestamp || Math.floor(Date.now() / 1000))
                })
            });
            Object.assign(photo, payload.photo || {});
            if (payload.photo) mergeRuntime([payload.photo]);
            await loadServerPhotos().catch(() => {});
            patchGramPicker();
            refreshGalleryIfOpen();
            return photo;
        } finally {
            saving.delete(photo);
        }
    }

    function showCameraToast(root, message, error = false) {
        if (!root) return;
        let node = root.querySelector('.pp-media-toast');
        if (!node) {
            node = document.createElement('div');
            node.className = 'pp-media-toast';
            root.append(node);
        }
        node.textContent = message;
        node.classList.toggle('error', error);
        clearTimeout(node._timer);
        node._timer = setTimeout(() => node.remove(), 2600);
    }

    function findCameraCanvas(root) {
        const candidates = [
            root?.querySelector('.pcam-live-preview'),
            root?.querySelector('canvas'),
            ...[...document.querySelectorAll('canvas')]
                .filter(canvas => !canvas.closest('.phone-camera-shell') && canvas.width > 160 && canvas.height > 160)
                .sort((a, b) => (b.width * b.height) - (a.width * a.height))
        ].filter(Boolean);
        return candidates[0] || null;
    }

    async function captureLocalCamera(root) {
        const source = findCameraCanvas(root);
        if (!source) throw new Error('Aperçu caméra introuvable.');
        const out = document.createElement('canvas');
        out.width = 320;
        out.height = 320;
        const ctx = out.getContext('2d', { alpha: false });
        const side = Math.min(source.width, source.height);
        const sx = Math.max(0, Math.floor((source.width - side) / 2));
        const sy = Math.max(0, Math.floor((source.height - side) / 2));
        ctx.drawImage(source, sx, sy, side, side, 0, 0, 320, 320);
        const photo = {
            url: out.toDataURL('image/png'),
            timestamp: Math.floor(Date.now() / 1000),
            roomId: 0
        };
        runtime.photos.unshift(photo);
        await persistPhoto(photo);
        return photo;
    }

    document.addEventListener('click', event => {
        const button = event.target.closest?.('.phone-camera-shell button');
        if (!button) return;
        const descriptor = `${button.textContent || ''} ${button.title || ''} ${button.getAttribute('aria-label') || ''}`.toLowerCase();
        if (!/(prendre|capture|photo|acheter|buy)/i.test(descriptor)) return;

        // Le serveur caméra natif du pack pointe vers un service indisponible. On utilise
        // l'aperçu Nitro déjà affiché et on sauvegarde la photo dans la vraie galerie.
        event.preventDefault();
        event.stopImmediatePropagation();
        const root = button.closest('.phone-camera-shell');
        if (button.dataset.ppMediaBusy) return;
        button.dataset.ppMediaBusy = '1';
        button.disabled = true;
        showCameraToast(root, 'Sauvegarde de la photo...');
        captureLocalCamera(root)
            .then(() => showCameraToast(root, 'Photo enregistrée dans votre galerie.'))
            .catch(error => showCameraToast(root, error.message || 'Impossible de prendre la photo.', true))
            .finally(() => {
                delete button.dataset.ppMediaBusy;
                button.disabled = false;
            });
    }, true);

    function uniquePhotos() {
        const all = [...serverPhotos, ...runtime.photos];
        return all.filter((photo, index) => photo?.url && all.findIndex(other =>
            (Number(photo.id || 0) > 0 && Number(other.id || 0) === Number(photo.id)) || other.url === photo.url
        ) === index);
    }

    function patchGramPicker() {
        const gram = document.querySelector('.ppr-gram.pg-v2');
        const picker = gram?.querySelector('.pg-picker');
        if (!picker) return;
        const photos = uniquePhotos();
        for (const photo of photos) {
            const id = Number(photo.id || 0);
            if (id <= 0 || picker.querySelector(`[data-pg-photo="${id}"]`)) continue;
            const button = document.createElement('button');
            button.type = 'button';
            button.dataset.pgPhoto = String(id);
            button.title = 'Choisir cette photo';
            button.innerHTML = `<img src="${escapeHtml(photo.url)}" alt="Photo de votre galerie" loading="lazy">`;
            picker.append(button);
        }

        const empty = gram.querySelector('.pg-publish .pg-empty');
        if (empty && photos.some(photo => Number(photo.id || 0) > 0)) empty.remove();
    }

    function refreshGalleryIfOpen() {
        const gallery = document.querySelector('.phone-gallery[data-ppr-ready]');
        if (!gallery) return;
        // L'UI existante lit runtime.photos : la rendre de nouveau éligible au montage
        // force la prochaine ouverture à repartir de la galerie persistante.
        gallery.dataset.ppHasPersistentPhotos = '1';
    }

    async function getFeed() {
        const response = await fetch('/nitro/phone-api.php?action=feed', { credentials: 'same-origin', cache: 'no-store' });
        const payload = await response.json().catch(() => null);
        if (!response.ok || !payload?.ok) throw new Error('Feed indisponible.');
        lastFeed = payload;
        return payload;
    }

    function findPostForGridButton(button, feed) {
        const image = button.querySelector('img');
        if (!image) return null;
        const src = image.getAttribute('src') || image.src || '';
        const candidates = (feed?.posts || []).filter(post => post.imageUrl && (post.imageUrl === src || image.src.endsWith(post.imageUrl)));
        return candidates[0] || null;
    }

    function renderPostOverlay(gram, post) {
        gram.querySelector('.pg-post-detail')?.remove();
        const overlay = document.createElement('div');
        overlay.className = 'pg-post-detail';
        overlay.innerHTML = `
            <div class="pg-detail-top"><button type="button" data-pg-detail-close aria-label="Retour">‹</button><strong>Publication</strong><span></span></div>
            <div class="pg-detail-scroll">
                <article class="pg-detail-post">
                    <div class="pg-detail-user"><strong>${escapeHtml(post.username || '')}</strong><small>${new Date(Number(post.createdAt || 0) * 1000).toLocaleString('fr-FR')}</small></div>
                    ${post.imageUrl ? `<img src="${escapeHtml(post.imageUrl)}" alt="Publication de ${escapeHtml(post.username || '')}">` : ''}
                    <div class="pg-detail-actions"><span>${post.liked ? '♥' : '♡'}</span><b>${Number(post.likes || 0)} J’aime</b></div>
                    ${post.body ? `<p><strong>${escapeHtml(post.username || '')}</strong> ${escapeHtml(post.body)}</p>` : ''}
                    <div class="pg-detail-comments">${(post.comments || []).map(comment => `<div><strong>${escapeHtml(comment.username || '')}</strong> ${escapeHtml(comment.body || '')}</div>`).join('')}</div>
                </article>
            </div>`;
        gram.append(overlay);
        overlay.querySelector('[data-pg-detail-close]')?.addEventListener('click', () => overlay.remove(), { once: true });
    }

    document.addEventListener('click', async event => {
        const button = event.target.closest?.('.ppr-gram.pg-v2 .pg-profile .pg-grid button');
        if (!button || button.hasAttribute('data-pg-photo')) return;
        event.preventDefault();
        event.stopPropagation();
        try {
            const feed = lastFeed || await getFeed();
            const post = findPostForGridButton(button, feed);
            if (post) renderPostOverlay(button.closest('.ppr-gram.pg-v2'), post);
        } catch {}
    }, true);

    async function syncRuntimePhotos() {
        for (const photo of [...runtime.photos]) {
            if (Number(photo?.id || 0) <= 0 && photo?.url) {
                await persistPhoto(photo).catch(() => {});
            }
        }
        patchGramPicker();
    }

    const observer = new MutationObserver(() => {
        patchGramPicker();
        syncRuntimePhotos().catch(() => {});
    });

    async function boot() {
        try { await loadServerPhotos(); } catch {}
        observer.observe(document.getElementById('root') || document.body, { childList: true, subtree: true });
        setInterval(() => syncRuntimePhotos().catch(() => {}), 1200);
        patchGramPicker();
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
    else boot();
})();
