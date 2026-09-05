(() => {
    'use strict';
    if (window.__PARADISE_CAMERA_GRAM_FIX_V2__) return;
    window.__PARADISE_CAMERA_GRAM_FIX_V2__ = '2.0.0';

    const runtime = window.__PARADISE_PHONE_RUNTIME__ = window.__PARADISE_PHONE_RUNTIME__ || { photos: [] };
    const CAMERA_API = '/nitro/phone-camera-api.php';
    const PHONE_API = '/nitro/phone-api.php';
    const CAMERA_ROOT_SELECTOR = '.phone-camera-shell,.phone-camera-app,.pce-camera,.pcam-camera,[data-phone-camera]';
    const FINAL_PREVIEW_SELECTOR = '.pcam-preview-img,.pce-preview img,.phone-camera-preview img';

    const escapeHtml = value => String(value ?? '')
        .replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;').replaceAll("'", '&#039;');

    const formatDate = timestamp => {
        const date = new Date(Number(timestamp) * 1000);
        return Number.isNaN(date.getTime()) ? '' : date.toLocaleString('fr-FR', { dateStyle: 'short', timeStyle: 'short' });
    };

    function cameraRoot(node) {
        return node?.closest?.(CAMERA_ROOT_SELECTOR)
            || node?.closest?.('.phone-app-body')?.querySelector?.('.phone-camera-shell')
            || null;
    }

    function toast(root, message, error = false) {
        if (!root) return;
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
        node._timer = setTimeout(() => node.classList.remove('is-visible'), 2800);
    }

    function findSceneCanvas(root) {
        const preferred = root?.querySelector?.('.pcam-live-preview,canvas.pcam-preview,canvas.phone-camera-preview');
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

    async function imageSnapshot(image) {
        if (!(image instanceof HTMLImageElement) || !image.src) return null;
        if (image.src.startsWith('data:image/png;base64,')) return image.src;
        const canvas = document.createElement('canvas');
        canvas.width = 320;
        canvas.height = 320;
        const ctx = canvas.getContext('2d', { alpha: false });
        if (!ctx) return null;
        try {
            if (!image.complete) await image.decode();
            const w = image.naturalWidth || image.width;
            const h = image.naturalHeight || image.height;
            if (!w || !h) return null;
            const side = Math.min(w, h);
            ctx.drawImage(image, (w - side) / 2, (h - side) / 2, side, side, 0, 0, 320, 320);
            return canvas.toDataURL('image/png');
        } catch {
            return null;
        }
    }

    async function savePhonePhoto(root, trigger = null, previewImage = null) {
        if (!root || root.dataset.paradiseSaving === '1') return;
        root.dataset.paradiseSaving = '1';

        const button = trigger instanceof HTMLButtonElement ? trigger : null;
        const oldText = button?.textContent || '';
        if (button) {
            button.disabled = true;
            if (oldText.trim()) button.textContent = 'Sauvegarde…';
        }

        try {
            let imageData = null;
            const canvas = findSceneCanvas(root);
            if (canvas) {
                try { imageData = squareSnapshot(canvas); } catch {}
            }
            if (!imageData && previewImage) imageData = await imageSnapshot(previewImage);
            if (!imageData) throw new Error('Impossible de récupérer la vue de la caméra.');

            const response = await fetch(CAMERA_API, {
                method: 'POST',
                credentials: 'same-origin',
                cache: 'no-store',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ imageData, roomId: 0 })
            });
            const payload = await response.json().catch(() => null);
            if (!response.ok || !payload?.ok || !payload.photo?.id) throw new Error(payload?.error || 'Sauvegarde impossible.');

            runtime.photos = runtime.photos.filter(photo => Number(photo.id) !== Number(payload.photo.id) && photo.url !== payload.photo.url);
            runtime.photos.unshift(payload.photo);
            root.dataset.paradiseLastPhotoUrl = payload.photo.url;
            toast(root, 'Photo enregistrée dans votre galerie ✓');
            window.dispatchEvent(new CustomEvent('paradise:camera-photo-saved', { detail: payload.photo }));
        } catch (error) {
            toast(root, error?.message || 'Impossible de sauvegarder la photo.', true);
        } finally {
            delete root.dataset.paradiseSaving;
            if (button) {
                button.disabled = false;
                if (oldText.trim()) button.textContent = oldText;
            }
        }
    }

    function isCameraPurchaseButton(button) {
        if (!(button instanceof HTMLButtonElement)) return false;
        const label = `${button.textContent || ''} ${button.title || ''} ${button.getAttribute('aria-label') || ''} ${button.className || ''}`.toLowerCase();
        if (/(retour|back|fermer|close|annuler|cancel|effet|filter|zoom|flash)/i.test(label)) return false;
        return /(acheter|achat|purchase|buy|capture|photo|prendre|take|sauvegarder|save|shutter)/i.test(label);
    }

    function unlockCameraButtons(scope = document) {
        scope.querySelectorAll?.(`${CAMERA_ROOT_SELECTOR} button`).forEach(button => {
            if (!isCameraPurchaseButton(button)) return;
            if (button.disabled) button.disabled = false;
            button.removeAttribute('disabled');
            if (button.getAttribute('aria-disabled') === 'true') button.setAttribute('aria-disabled', 'false');
            button.classList.add('paradise-camera-enabled');
        });
    }

    // Le bouton natif peut être rendu disabled : pointerdown permet de prendre la main avant
    // que Nitro affiche son ancien message "caméra désactivée".
    const handleCameraAction = event => {
        const button = event.target.closest?.('button');
        if (!button || !isCameraPurchaseButton(button)) return;
        const root = cameraRoot(button);
        if (!root) return;
        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation();
        const preview = root.querySelector(FINAL_PREVIEW_SELECTOR);
        savePhonePhoto(root, button, preview);
    };
    document.addEventListener('pointerdown', handleCameraAction, true);
    document.addEventListener('click', handleCameraAction, true);

    // Certaines versions Nitro créent directement une prévisualisation finale sans bouton
    // exploitable. On la persiste une seule fois par source afin que la Galerie survive à la déco.
    function persistNewPreviews(scope = document) {
        scope.querySelectorAll?.(FINAL_PREVIEW_SELECTOR).forEach(image => {
            if (!(image instanceof HTMLImageElement)) return;
            const root = cameraRoot(image);
            if (!root) return;
            const source = image.currentSrc || image.src || '';
            if (!source || image.dataset.paradisePersistedSource === source) return;
            image.dataset.paradisePersistedSource = source;
            setTimeout(() => savePhonePhoto(root, null, image), 250);
        });
    }

    async function getFeed() {
        const response = await fetch(`${PHONE_API}?action=feed&_=${Date.now()}`, { credentials: 'same-origin', cache: 'no-store' });
        const data = await response.json().catch(() => null);
        if (!response.ok || !data?.ok) throw new Error(data?.error || 'Impossible de charger la publication.');
        return data;
    }

    function avatar(look, username) {
        return `<img src="/avatar.php?figure=${encodeURIComponent(look || '')}&size=m&direction=2&head_direction=2" alt="${escapeHtml(username)}">`;
    }

    function renderPostViewer(body, post, previous) {
        body.innerHTML = `<section class="pg-post-viewer">
            <button type="button" class="pg-viewer-back" data-pg-viewer-back><span class="pg-back-arrow">‹</span><span>Retour au profil</span></button>
            <article class="pg-viewer-card">
                <header><span class="pg-avatar">${avatar(post.look, post.username)}</span><div><strong>${escapeHtml(post.username)}</strong><small>${formatDate(post.createdAt)}</small></div></header>
                <img class="pg-viewer-photo" src="${escapeHtml(post.imageUrl || '')}" alt="Publication de ${escapeHtml(post.username)}">
                <div class="pg-viewer-actions"><span class="${post.liked ? 'is-liked' : ''}">${post.liked ? '♥' : '♡'}</span><strong>${Number(post.likes || 0)} J’aime</strong></div>
                ${post.body ? `<div class="pg-viewer-caption"><b>${escapeHtml(post.username)}</b> ${escapeHtml(post.body)}</div>` : ''}
                <div class="pg-viewer-comments">${(post.comments || []).length ? (post.comments || []).map(comment => `<p><b>${escapeHtml(comment.username)}</b> ${escapeHtml(comment.body)}</p>`).join('') : '<p class="pg-muted">Aucun commentaire.</p>'}</div>
            </article>
        </section>`;
        body.querySelector('[data-pg-viewer-back]')?.addEventListener('click', () => { body.innerHTML = previous; decorateProfileGrid(body); }, { once: true });
    }

    function openProfilePost(image) {
        const body = image.closest('.pg-body');
        if (!body || body.querySelector('.pg-post-viewer')) return;
        const url = image.currentSrc || image.src;
        if (!url) return;
        const previous = body.innerHTML;
        body.innerHTML = '<div class="pg-post-viewer pg-loading-view"><span class="ppr-loader"></span><strong>Chargement de la publication…</strong></div>';
        getFeed().then(data => {
            const post = (data.posts || []).find(item => {
                try { return new URL(item.imageUrl || '', location.href).href === new URL(url, location.href).href; }
                catch { return item.imageUrl === url; }
            });
            if (!post) throw new Error('Publication introuvable.');
            renderPostViewer(body, post, previous);
        }).catch(error => {
            body.innerHTML = `<div class="pg-post-viewer pg-error-view"><strong>${escapeHtml(error.message)}</strong><button type="button" data-pg-viewer-back>Retour</button></div>`;
            body.querySelector('[data-pg-viewer-back]')?.addEventListener('click', () => { body.innerHTML = previous; decorateProfileGrid(body); }, { once: true });
        });
    }

    function decorateProfileGrid(scope = document) {
        scope.querySelectorAll?.('.pg-profile .pg-grid > div').forEach(cell => {
            if (!cell.querySelector('img')) return;
            cell.classList.add('pg-clickable-post');
            cell.setAttribute('role', 'button');
            cell.setAttribute('tabindex', '0');
            cell.setAttribute('aria-label', 'Ouvrir la publication');
        });
    }

    document.addEventListener('click', event => {
        const cell = event.target.closest?.('.pg-profile .pg-grid > div,.pg-profile .pg-grid > button');
        const image = cell?.querySelector?.('img');
        if (!image) return;
        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation();
        openProfilePost(image);
    }, true);

    document.addEventListener('keydown', event => {
        if (event.key !== 'Enter' && event.key !== ' ') return;
        const cell = event.target.closest?.('.pg-clickable-post');
        const image = cell?.querySelector?.('img');
        if (!image) return;
        event.preventDefault();
        openProfilePost(image);
    }, true);

    // Quand une photo vient d'être sauvegardée, on force le prochain affichage de Galerie/Gram
    // à repartir du serveur au lieu de conserver l'ancien contenu mémoire.
    window.addEventListener('paradise:camera-photo-saved', () => {
        document.querySelectorAll('.phone-gallery[data-ppr-ready]').forEach(node => {
            delete node.dataset.pprReady;
        });
        document.querySelectorAll('.ppr-gram.pg-v2').forEach(node => {
            node.dataset.paradiseGalleryStale = '1';
        });
    });

    const observer = new MutationObserver(records => {
        for (const record of records) {
            for (const node of record.addedNodes) {
                if (!(node instanceof Element)) continue;
                unlockCameraButtons(node);
                persistNewPreviews(node);
                decorateProfileGrid(node);
            }
        }
        unlockCameraButtons(document);
    });
    observer.observe(document.getElementById('root') || document.body, { childList: true, subtree: true, attributes: true, attributeFilter: ['disabled', 'aria-disabled'] });

    unlockCameraButtons(document);
    persistNewPreviews(document);
    decorateProfileGrid(document);
})();
