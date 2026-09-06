(() => {
    'use strict';
    if (window.__PARADISE_CAMERA_GRAM_FIX_V8__) return;
    window.__PARADISE_CAMERA_GRAM_FIX_V8__ = '8.0.0';

    const runtime = window.__PARADISE_PHONE_RUNTIME__ = window.__PARADISE_PHONE_RUNTIME__ || { photos: [] };
    const CAMERA_API = '/nitro/phone-camera-api.php';
    const PHONE_API = '/nitro/phone-api.php';
    const CAMERA_ROOT_SELECTOR = '.phone-camera-shell,.phone-camera-app,.pce-camera,.pcam-camera,[data-phone-camera]';

    function installPhoneFixStyles() {
        if (document.getElementById('paradise-phone-camera-contact-fix-v8')) return;
        const style = document.createElement('style');
        style.id = 'paradise-phone-camera-contact-fix-v8';
        style.textContent = `
            .nitro-phone-frame .pcam-live-preview,
            .nitro-phone-frame .paradise-real-camera-preview{display:none!important}
            .nitro-phone-frame .phone-camera-app[data-mode="preview"] .pcam-topbar,
            .nitro-phone-frame .phone-camera-app[data-mode="preview"] .pcam-controls{visibility:hidden!important;pointer-events:none!important}
            .nitro-phone-frame.paradise-camera-active .phone-wallpaper{visibility:hidden!important}
            .nitro-phone-frame .phone-friends-app .friend-row.paradise-callable-friend{min-height:58px!important;padding:6px 108px 6px 7px!important;gap:7px!important;box-sizing:border-box!important}
            .nitro-phone-frame .phone-friends-app .friend-row.paradise-callable-friend .roleplay-avatar-list{flex:0 0 44px!important;width:44px!important;min-width:44px!important;max-width:44px!important;height:44px!important}
            .nitro-phone-frame .phone-friends-app .friend-row.paradise-callable-friend .friend-name{flex:1 1 auto!important;min-width:0!important;overflow:hidden!important;text-overflow:ellipsis!important;white-space:nowrap!important}
            .nitro-phone-frame .phone-friends-app .friend-row.paradise-callable-friend>.actions{position:absolute!important;right:6px!important;top:50%!important;transform:translateY(-50%)!important;display:flex!important;gap:3px!important}
            .nitro-phone-frame .phone-friends-app .friend-row.paradise-callable-friend>.actions .icon-btn,
            .nitro-phone-frame .phone-friends-app .friend-row.paradise-callable-friend .paradise-call-actions button{width:22px!important;min-width:22px!important;height:22px!important;padding:0!important}
            .nitro-phone-frame .phone-friends-app .friend-row.paradise-callable-friend .paradise-call-actions{right:56px!important;gap:3px!important}
        `;
        document.head.append(style);
    }
    installPhoneFixStyles();

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

    function installLivePreview(root) {
        if (!root) return;
        // Nitro capture directement la texture WebGL de la chambre. Les anciens
        // miroirs 2D créaient une image noire et masquaient le viseur natif.
        root.querySelectorAll('.paradise-real-camera-preview,.pcam-live-preview').forEach(node => node.remove());
        root.dataset.paradiseLivePreview = 'native';
        const frame = root.closest('.nitro-phone-frame');
        frame?.classList.add('paradise-camera-active');
        [root.closest('.phone-app-body'), root.closest('.phone-active-app'), root.closest('.phone-screen')]
            .filter(Boolean)
            .forEach(layer => {
                if (!layer._paradiseCameraBackground) {
                    layer._paradiseCameraBackground = {
                        value: layer.style.getPropertyValue('background-color'),
                        priority: layer.style.getPropertyPriority('background-color')
                    };
                }
                layer.dataset.paradiseCameraTransparent = '1';
                layer.style.setProperty('background-color', 'transparent', 'important');
            });
    }

    function restoreClosedCameraLayers() {
        document.querySelectorAll('.nitro-phone-frame.paradise-camera-active').forEach(frame => {
            if (frame.querySelector(CAMERA_ROOT_SELECTOR)) return;
            frame.classList.remove('paradise-camera-active');
        });
        document.querySelectorAll('[data-paradise-camera-transparent="1"]').forEach(layer => {
            if (layer.querySelector(CAMERA_ROOT_SELECTOR)) return;
            const previous = layer._paradiseCameraBackground || { value: '', priority: '' };
            if (previous.value) layer.style.setProperty('background-color', previous.value, previous.priority);
            else layer.style.removeProperty('background-color');
            delete layer._paradiseCameraBackground;
            delete layer.dataset.paradiseCameraTransparent;
        });
    }

    function waitForNativePhoto(root, previousUrl = '', timeout = 2500) {
        const started = performance.now();
        return new Promise((resolve, reject) => {
            const poll = () => {
                const image = root?.querySelector('.pcam-preview-img,.camera-area[src]');
                const url = image?.currentSrc || image?.src || '';
                if (url && url !== previousUrl) return resolve(url);
                if (!root?.isConnected || performance.now() - started >= timeout) {
                    reject(new Error("Nitro n'a pas pu générer la photo."));
                    return;
                }
                requestAnimationFrame(poll);
            };
            requestAnimationFrame(poll);
        });
    }

    async function sourceToPngDataUrl(source) {
        if (!source) throw new Error('La photo générée est vide.');
        const image = new Image();
        image.decoding = 'async';
        image.src = source;
        await image.decode();

        const width = Math.max(1, image.naturalWidth || image.width);
        const height = Math.max(1, image.naturalHeight || image.height);
        const output = document.createElement('canvas');
        output.width = Math.min(1024, width);
        output.height = Math.min(1024, height);
        const context = output.getContext('2d', { alpha: false, willReadFrequently: true });
        if (!context) throw new Error('Impossible de préparer la photo.');
        context.drawImage(image, 0, 0, output.width, output.height);

        const sample = context.getImageData(0, 0, output.width, output.height).data;
        const step = Math.max(4, Math.floor(sample.length / 4096 / 4) * 4);
        let brightest = 0;
        for (let index = 0; index < sample.length; index += step) {
            brightest = Math.max(brightest, sample[index], sample[index + 1], sample[index + 2]);
            if (brightest > 12) break;
        }
        if (brightest <= 12) throw new Error('La capture est noire. Réessayez après le chargement de la chambre.');
        return output.toDataURL('image/png');
    }

    async function persistPhoto(root, imageData, button) {
        if (root.dataset.paradiseSaving === '1') return;
        root.dataset.paradiseSaving = '1';
        const original = button?.textContent || '';
        if (button) { button.disabled = true; button.textContent = 'Enregistrement…'; }
        try {
            const response = await fetch(CAMERA_API, {
                method: 'POST', credentials: 'same-origin', cache: 'no-store',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ imageData, roomId: 0 })
            });
            const payload = await response.json().catch(() => null);
            if (!response.ok || !payload?.ok || !payload.photo?.id) throw new Error(payload?.error || 'Sauvegarde impossible.');

            // Retire aussi la copie temporaire créée par le camera-roll Nitro afin
            // que la galerie n'affiche pas deux fois la même prise de vue.
            runtime.photos = runtime.photos.filter(photo =>
                Number(photo.id || 0) > 0
                && Number(photo.id) !== Number(payload.photo.id)
                && photo.url !== payload.photo.url
            );
            runtime.photos.unshift(payload.photo);
            window.dispatchEvent(new CustomEvent('paradise:camera-photo-saved', { detail: payload.photo }));
            returnToCapture(root);
            requestAnimationFrame(() => closeConfirm(root));
            toast(root, 'Photo enregistrée dans votre galerie ✓');
        } catch (error) {
            toast(root, error?.message || 'Impossible de sauvegarder la photo.', true);
        } finally {
            delete root.dataset.paradiseSaving;
            if (button) { button.disabled = false; button.textContent = original || 'Enregistrer'; }
        }
    }

    function closeConfirm(root) {
        root?.querySelector('.paradise-camera-confirm')?.remove();
    }

    function returnToCapture(root) {
        const preview = root?.matches?.('.phone-camera-app[data-mode="preview"]') ? root : root?.querySelector('.phone-camera-app[data-mode="preview"]');
        const newPhoto = preview?.querySelector('.pcam-right-icons .pcam-icon');
        if (newPhoto instanceof HTMLButtonElement) newPhoto.click();
    }

    function showConfirm(root, imageData) {
        closeConfirm(root);
        const overlay = document.createElement('div');
        overlay.className = 'paradise-camera-confirm';
        overlay.innerHTML = `<div class="paradise-camera-confirm-card">
            <strong>Enregistrer cette photo ?</strong>
            <img src="${imageData}" alt="Aperçu de la photo">
            <div><button type="button" data-camera-retake>Reprendre</button><button type="button" data-camera-save>Enregistrer</button></div>
        </div>`;
        Object.assign(overlay.style, {
            position:'absolute', inset:'0', zIndex:'10000', background:'rgba(0,0,0,.92)',
            display:'flex', alignItems:'center', justifyContent:'center', padding:'18px', boxSizing:'border-box'
        });
        const card = overlay.querySelector('.paradise-camera-confirm-card');
        Object.assign(card.style, { width:'100%', display:'flex', flexDirection:'column', gap:'10px', color:'#fff', textAlign:'center' });
        const img = overlay.querySelector('img');
        Object.assign(img.style, { width:'100%', aspectRatio:'1/1', objectFit:'cover', borderRadius:'8px', background:'#000' });
        const actions = overlay.querySelector('.paradise-camera-confirm-card > div');
        Object.assign(actions.style, { display:'grid', gridTemplateColumns:'1fr 1fr', gap:'8px' });
        actions.querySelectorAll('button').forEach(button => Object.assign(button.style, {
            border:'0', borderRadius:'7px', padding:'9px 6px', fontWeight:'700', cursor:'pointer'
        }));
        const save = overlay.querySelector('[data-camera-save]');
        save.style.background = '#1689df'; save.style.color = '#fff';
        overlay.querySelector('[data-camera-retake]').addEventListener('click', () => {
            returnToCapture(root);
            requestAnimationFrame(() => closeConfirm(root));
        });
        save.addEventListener('click', () => persistPhoto(root, imageData, save));
        root.append(overlay);
    }

    function isShutterButton(button) {
        if (!(button instanceof HTMLButtonElement)) return false;
        if (button.matches('.pcam-shutter-btn,.pcam-shutter,.camera-shutter')) return true;
        if (button.matches('.pcam-icon,.pce-icon,[data-camera-retake],[data-camera-save]')) return false;
        const label = `${button.textContent || ''} ${button.title || ''} ${button.getAttribute('aria-label') || ''} ${button.className || ''}`.toLowerCase();
        if (/(retour|back|fermer|close|annuler|cancel|effet|filter|zoom|flash)/i.test(label)) return false;
        return /(capture|prendre|take|shutter)/i.test(label);
    }

    function unlockCameraButtons(scope = document) {
        scope.querySelectorAll?.(`${CAMERA_ROOT_SELECTOR} button`).forEach(button => {
            if (!isShutterButton(button)) return;
            button.disabled = false;
            button.removeAttribute('disabled');
            button.setAttribute('aria-disabled', 'false');
            button.classList.add('paradise-camera-enabled');
        });
        scope.querySelectorAll?.(CAMERA_ROOT_SELECTOR).forEach(installLivePreview);
    }

    const handleCameraAction = event => {
        const button = event.target.closest?.('button');
        if (!button || !isShutterButton(button)) return;
        const root = cameraRoot(button);
        if (!root) return;
        if (button.dataset.paradiseCapturePending === '1') return;
        button.dataset.paradiseCapturePending = '1';
        const previous = root.querySelector('.pcam-preview-img,.camera-area[src]');
        const previousUrl = previous?.currentSrc || previous?.src || '';
        // Le clic Nitro reste intact : React crée d'abord la texture native.
        waitForNativePhoto(root, previousUrl)
            .then(sourceToPngDataUrl)
            .then(imageData => showConfirm(root, imageData))
            .catch(error => { returnToCapture(root); toast(root, error?.message || 'Impossible de prendre la photo.', true); })
            .finally(() => delete button.dataset.paradiseCapturePending);
    };
    document.addEventListener('click', handleCameraAction, true);

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
        body.innerHTML = `<section class="pg-post-viewer"><button type="button" class="pg-viewer-back" data-pg-viewer-back><span class="pg-back-arrow">‹</span><span>Retour au profil</span></button><article class="pg-viewer-card"><header><span class="pg-avatar">${avatar(post.look, post.username)}</span><div><strong>${escapeHtml(post.username)}</strong><small>${formatDate(post.createdAt)}</small></div></header><img class="pg-viewer-photo" src="${escapeHtml(post.imageUrl || '')}" alt="Publication de ${escapeHtml(post.username)}"><div class="pg-viewer-actions"><span class="${post.liked ? 'is-liked' : ''}">${post.liked ? '♥' : '♡'}</span><strong>${Number(post.likes || 0)} J’aime</strong></div>${post.body ? `<div class="pg-viewer-caption"><b>${escapeHtml(post.username)}</b> ${escapeHtml(post.body)}</div>` : ''}<div class="pg-viewer-comments">${(post.comments || []).length ? (post.comments || []).map(comment => `<p><b>${escapeHtml(comment.username)}</b> ${escapeHtml(comment.body)}</p>`).join('') : '<p class="pg-muted">Aucun commentaire.</p>'}</div></article></section>`;
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
        scope.querySelectorAll?.('.pg-profile-post-open,.pg-profile .pg-grid > button').forEach(cell => {
            if (!cell.querySelector('img')) return;
            cell.classList.add('pg-clickable-post');
            cell.setAttribute('aria-label', 'Ouvrir la publication');
        });
    }

    document.addEventListener('click', event => {
        if (event.target.closest?.('[data-pg-delete]')) return;
        const cell = event.target.closest?.('.pg-profile-post-open,.pg-profile .pg-grid > button');
        const image = cell?.querySelector?.('img');
        if (!image) return;
        event.preventDefault(); event.stopPropagation(); event.stopImmediatePropagation();
        openProfilePost(image);
    }, true);

    document.addEventListener('keydown', event => {
        if (event.key !== 'Enter' && event.key !== ' ') return;
        const cell = event.target.closest?.('.pg-clickable-post');
        const image = cell?.querySelector?.('img');
        if (!image) return;
        event.preventDefault(); openProfilePost(image);
    }, true);

    window.addEventListener('paradise:camera-photo-saved', () => {
        document.querySelectorAll('.phone-gallery[data-ppr-ready]').forEach(node => delete node.dataset.pprReady);
        document.querySelectorAll('.ppr-gram.pg-v2').forEach(node => node.dataset.paradiseGalleryStale = '1');
    });

    const observer = new MutationObserver(() => {
        restoreClosedCameraLayers();
        unlockCameraButtons(document);
        decorateProfileGrid(document);
    });
    observer.observe(document.getElementById('root') || document.body, { childList: true, subtree: true });
    unlockCameraButtons(document);
    decorateProfileGrid(document);
})();
