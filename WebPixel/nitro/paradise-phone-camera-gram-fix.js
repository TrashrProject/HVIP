(() => {
    'use strict';
    if (window.__PARADISE_CAMERA_GRAM_FIX__) return;
    window.__PARADISE_CAMERA_GRAM_FIX__ = '1.0.0';

    const runtime = window.__PARADISE_PHONE_RUNTIME__ = window.__PARADISE_PHONE_RUNTIME__ || { photos: [] };
    const CAMERA_API = '/nitro/phone-camera-api.php';
    const PHONE_API = '/nitro/phone-api.php';

    const escapeHtml = value => String(value ?? '')
        .replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;').replaceAll("'", '&#039;');

    const formatDate = timestamp => {
        const date = new Date(Number(timestamp) * 1000);
        return Number.isNaN(date.getTime()) ? '' : date.toLocaleString('fr-FR', { dateStyle: 'short', timeStyle: 'short' });
    };

    function toast(root, message, error = false) {
        let node = root.querySelector('.pcam-paradise-status');
        if (!node) {
            node = document.createElement('div');
            node.className = 'pcam-paradise-status';
            root.append(node);
        }
        node.textContent = message;
        node.classList.toggle('is-error', error);
        node.classList.add('is-visible');
        clearTimeout(node._timer);
        node._timer = setTimeout(() => node.classList.remove('is-visible'), 2600);
    }

    function findSceneCanvas(root) {
        const preferred = root.querySelector('.pcam-live-preview');
        if (preferred instanceof HTMLCanvasElement && preferred.width > 0 && preferred.height > 0) return preferred;
        return [...document.querySelectorAll('canvas')]
            .filter(canvas => canvas.width >= 160 && canvas.height >= 160 && !canvas.closest('.pg-shell'))
            .sort((a, b) => (b.width * b.height) - (a.width * a.height))[0] || null;
    }

    function squareSnapshot(source) {
        const output = document.createElement('canvas');
        output.width = 320;
        output.height = 320;
        const context = output.getContext('2d', { alpha: false });
        if (!context) throw new Error('Capture impossible.');
        const side = Math.min(source.width, source.height);
        const sx = Math.max(0, Math.floor((source.width - side) / 2));
        const sy = Math.max(0, Math.floor((source.height - side) / 2));
        context.drawImage(source, sx, sy, side, side, 0, 0, 320, 320);
        return output.toDataURL('image/png');
    }

    async function savePhonePhoto(root, button) {
        if (button.dataset.paradiseSaving) return;
        const canvas = findSceneCanvas(root);
        if (!canvas) {
            toast(root, 'Impossible de récupérer la vue de la caméra.', true);
            return;
        }

        let imageData;
        try { imageData = squareSnapshot(canvas); }
        catch (error) { toast(root, 'Capture impossible. Réessayez.', true); return; }

        button.dataset.paradiseSaving = '1';
        const oldText = button.textContent;
        button.disabled = true;
        if (oldText?.trim()) button.textContent = 'Sauvegarde…';
        try {
            const response = await fetch(CAMERA_API, {
                method: 'POST',
                credentials: 'same-origin',
                cache: 'no-store',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ imageData, roomId: 0 })
            });
            const payload = await response.json().catch(() => null);
            if (!response.ok || !payload?.ok || !payload.photo?.id) throw new Error(payload?.error || 'Sauvegarde impossible.');
            runtime.photos = runtime.photos.filter(photo => Number(photo.id) !== Number(payload.photo.id));
            runtime.photos.unshift(payload.photo);
            toast(root, 'Photo enregistrée dans votre galerie ✓');

            const preview = root.querySelector('.pcam-preview-img, .pce-preview img, .phone-camera-preview img');
            if (preview instanceof HTMLImageElement) preview.src = payload.photo.url;
        } catch (error) {
            toast(root, error.message || 'Impossible de sauvegarder la photo.', true);
        } finally {
            delete button.dataset.paradiseSaving;
            button.disabled = false;
            if (oldText?.trim()) button.textContent = oldText;
        }
    }

    document.addEventListener('click', event => {
        const button = event.target.closest('.phone-camera-shell button');
        if (!button) return;
        const label = `${button.textContent || ''} ${button.title || ''} ${button.getAttribute('aria-label') || ''}`.toLowerCase();
        if (!/(acheter|capture|photo|prendre|sauvegarder|save)/i.test(label)) return;

        // Le vieux système Arcturus essaie de joindre un serveur caméra externe mort et affiche
        // "caméra désactivée". Pour le téléphone ParadiseRP on sauvegarde directement la vue Nitro.
        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation();
        savePhonePhoto(button.closest('.phone-camera-shell'), button);
    }, true);

    async function getFeed() {
        const response = await fetch(`${PHONE_API}?action=feed`, { credentials: 'same-origin', cache: 'no-store' });
        const data = await response.json().catch(() => null);
        if (!response.ok || !data?.ok) throw new Error(data?.error || 'Impossible de charger la publication.');
        return data;
    }

    function avatar(look, username) {
        return `<img src="/avatar.php?figure=${encodeURIComponent(look || '')}&size=m&direction=2&head_direction=2" alt="${escapeHtml(username)}">`;
    }

    function openProfilePost(image) {
        const body = image.closest('.pg-body');
        if (!body || body.querySelector('.pg-post-viewer')) return;
        const url = image.currentSrc || image.src;
        if (!url) return;
        const previous = body.innerHTML;
        body.innerHTML = '<div class="pg-post-viewer pg-loading-view"><span></span><strong>Chargement…</strong></div>';
        getFeed().then(data => {
            const post = (data.posts || []).find(item => {
                try { return new URL(item.imageUrl || '', location.href).href === new URL(url, location.href).href; }
                catch { return item.imageUrl === url; }
            });
            if (!post) throw new Error('Publication introuvable.');
            body.innerHTML = `<section class="pg-post-viewer">
                <button type="button" class="pg-viewer-back" data-pg-viewer-back>‹ <span>Profil</span></button>
                <article class="pg-viewer-card">
                    <header><span class="pg-avatar">${avatar(post.look, post.username)}</span><div><strong>${escapeHtml(post.username)}</strong><small>${formatDate(post.createdAt)}</small></div></header>
                    <img class="pg-viewer-photo" src="${escapeHtml(post.imageUrl || '')}" alt="Publication de ${escapeHtml(post.username)}">
                    <div class="pg-viewer-meta"><strong>${Number(post.likes || 0)} J’aime</strong>${post.body ? `<p><b>${escapeHtml(post.username)}</b> ${escapeHtml(post.body)}</p>` : ''}</div>
                    <div class="pg-viewer-comments">${(post.comments || []).map(comment => `<p><b>${escapeHtml(comment.username)}</b> ${escapeHtml(comment.body)}</p>`).join('')}</div>
                </article>
            </section>`;
            body.querySelector('[data-pg-viewer-back]')?.addEventListener('click', () => { body.innerHTML = previous; }, { once: true });
        }).catch(error => {
            body.innerHTML = `<div class="pg-post-viewer pg-error-view"><strong>${escapeHtml(error.message)}</strong><button type="button" data-pg-viewer-back>Retour</button></div>`;
            body.querySelector('[data-pg-viewer-back]')?.addEventListener('click', () => { body.innerHTML = previous; }, { once: true });
        });
    }

    document.addEventListener('click', event => {
        const image = event.target.closest('.pg-profile .pg-grid img');
        if (!image) return;
        event.preventDefault();
        event.stopPropagation();
        openProfilePost(image);
    }, true);

    // Rend les cases du profil explicitement cliquables même si l'ancien rendu utilise des <div>.
    const observer = new MutationObserver(records => {
        for (const record of records) {
            for (const node of record.addedNodes) {
                if (!(node instanceof Element)) continue;
                const grids = [...(node.matches?.('.pg-profile .pg-grid') ? [node] : []), ...node.querySelectorAll?.('.pg-profile .pg-grid') || []];
                grids.forEach(grid => grid.querySelectorAll(':scope > div').forEach(cell => {
                    if (!cell.querySelector('img')) return;
                    cell.classList.add('pg-clickable-post');
                    cell.setAttribute('role', 'button');
                    cell.setAttribute('tabindex', '0');
                }));
            }
        }
    });
    observer.observe(document.getElementById('root') || document.body, { childList: true, subtree: true });
})();
