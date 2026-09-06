(() => {
    'use strict';
    if (window.__PARADISE_CAMERA_GRAM_FIX_V4__) return;
    window.__PARADISE_CAMERA_GRAM_FIX_V4__ = '4.0.0';

    const runtime = window.__PARADISE_PHONE_RUNTIME__ = window.__PARADISE_PHONE_RUNTIME__ || { photos: [] };
    const CAMERA_API = '/nitro/phone-camera-api.php';
    const PHONE_API = '/nitro/phone-api.php';
    const CAMERA_ROOT_SELECTOR = '.phone-camera-shell,.phone-camera-app,.pce-camera,.pcam-camera,[data-phone-camera]';
    const CAMERA_ZOOM = 2.05;

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
        const candidates = [...document.querySelectorAll('canvas')].filter(canvas => {
            if (!(canvas instanceof HTMLCanvasElement)) return false;
            if (canvas.width < 300 || canvas.height < 200) return false;
            if (root?.contains(canvas)) return false;
            if (canvas.closest('.nitro-phone-frame,.phone-app-body,.pg-shell,#paradise-loading')) return false;
            const rect = canvas.getBoundingClientRect();
            if (rect.width < 200 || rect.height < 150) return false;
            const style = getComputedStyle(canvas);
            return style.display !== 'none' && style.visibility !== 'hidden' && Number(style.opacity || 1) > 0;
        });
        return candidates.sort((a, b) => (b.width * b.height) - (a.width * a.height))[0] || null;
    }

    function drawCameraView(source, output) {
        const ctx = output.getContext('2d', { alpha: false });
        if (!ctx) return false;

        const baseSide = Math.min(source.width, source.height);
        const cropSide = Math.max(1, Math.floor(baseSide / CAMERA_ZOOM));
        const sx = Math.max(0, Math.floor((source.width - cropSide) / 2));
        const sy = Math.max(0, Math.floor((source.height - cropSide) / 2));

        try {
            ctx.imageSmoothingEnabled = false;
            ctx.drawImage(source, sx, sy, cropSide, cropSide, 0, 0, output.width, output.height);
            return true;
        } catch {
            return false;
        }
    }

    function snapshot(root) {
        const source = findSceneCanvas(root);
        if (!source) throw new Error('La vue de la chambre est introuvable.');
        const output = document.createElement('canvas');
        output.width = 480;
        output.height = 480;
        if (!drawCameraView(source, output)) throw new Error('Impossible de capturer la chambre.');
        return output.toDataURL('image/png');
    }

    function installLivePreview(root) {
        if (!root || root.dataset.paradiseLivePreview === '1') return;
        root.dataset.paradiseLivePreview = '1';
        const lens = root.querySelector('.pcam-lens') || root.querySelector('.phone-camera-preview') || root.querySelector('.pce-preview');
        if (!lens) {
            delete root.dataset.paradiseLivePreview;
            return;
        }

        lens.style.position = 'relative';
        let preview = lens.querySelector('canvas.paradise-real-camera-preview');
        if (!preview) {
            preview = document.createElement('canvas');
            preview.className = 'paradise-real-camera-preview';
            preview.width = 480;
            preview.height = 480;
            Object.assign(preview.style, {
                position: 'absolute', inset: '0', width: '100%', height: '100%',
                zIndex: '5', pointerEvents: 'none', background: '#000'
            });
            lens.append(preview);
        }

        const tick = () => {
            if (!root.isConnected) return;
            const source = findSceneCanvas(root);
            if (source) drawCameraView(source, preview);
            requestAnimationFrame(tick);
        };
        requestAnimationFrame(tick);
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

            runtime.photos = runtime.photos.filter(photo => Number(photo.id) !== Number(payload.photo.id) && photo.url !== payload.photo.url);
            runtime.photos.unshift(payload.photo);
            window.dispatchEvent(new CustomEvent('paradise:camera-photo-saved', { detail: payload.photo }));
            closeConfirm(root);
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
        overlay.querySelector('[data-camera-retake]').addEventListener('click', () => closeConfirm(root));
        save.addEventListener('click', () => persistPhoto(root, imageData, save));
        root.append(overlay);
    }

    function isShutterButton(button) {
        if (!(button instanceof HTMLButtonElement)) return false;
        const label = `${button.textContent || ''} ${button.title || ''} ${button.getAttribute('aria-label') || ''} ${button.className || ''}`.toLowerCase();
        if (/(retour|back|fermer|close|annuler|cancel|effet|filter|zoom|flash)/i.test(label)) return false;
        return /(acheter|achat|purchase|buy|capture|photo|prendre|take|shutter)/i.test(label) || button.matches('.pcam-shutter,.camera-shutter');
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
        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation();
        try {
            const imageData = snapshot(root);
            showConfirm(root, imageData);
        } catch (error) {
            toast(root, error?.message || 'Impossible de prendre la photo.', true);
        }
    };
    document.addEventListener('pointerdown', handleCameraAction, true);
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
        scope.querySelectorAll?.('.pg-profile .pg-grid > div,.pg-profile .pg-grid > button').forEach(cell => {
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
        unlockCameraButtons(document);
        decorateProfileGrid(document);
    });
    observer.observe(document.getElementById('root') || document.body, { childList: true, subtree: true, attributes: true, attributeFilter: ['disabled', 'aria-disabled'] });
    unlockCameraButtons(document);
    decorateProfileGrid(document);
})();
