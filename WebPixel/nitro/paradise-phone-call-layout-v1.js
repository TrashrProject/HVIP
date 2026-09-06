(() => {
  'use strict';

  if (window.__PARADISE_PHONE_CALL_LAYOUT_V1__) return;
  window.__PARADISE_PHONE_CALL_LAYOUT_V1__ = '1.0.0';

  const CHECK_MS = 350;
  const VIEWPORT_MARGIN = 8;

  let videoMounted = false;
  let expanded = true;
  let landscape = false;
  let drag = null;

  const getFrame = () => document.querySelector('.nitro-phone-frame');
  const getLayer = () => document.querySelector('.paradise-call-layer.paradise-call-stable-v2');
  const getVideoCard = () => getLayer()?.querySelector('.pcall-active.is-video') || null;

  function readTranslate(frame) {
    const raw = getComputedStyle(frame).translate;
    if (!raw || raw === 'none') return { x: 0, y: 0 };
    const parts = raw.split(/\s+/);
    const x = Number.parseFloat(parts[0]);
    const y = Number.parseFloat(parts[1] || '0');
    return {
      x: Number.isFinite(x) ? x : 0,
      y: Number.isFinite(y) ? y : 0
    };
  }

  function saveTranslate(frame, x, y) {
    frame.dataset.pcallTranslateX = String(x);
    frame.dataset.pcallTranslateY = String(y);
    frame.style.translate = `${x}px ${y}px`;
  }

  function currentTranslate(frame) {
    const x = Number.parseFloat(frame.dataset.pcallTranslateX || '');
    const y = Number.parseFloat(frame.dataset.pcallTranslateY || '');
    if (Number.isFinite(x) && Number.isFinite(y)) return { x, y };
    const computed = readTranslate(frame);
    frame.dataset.pcallTranslateX = String(computed.x);
    frame.dataset.pcallTranslateY = String(computed.y);
    return computed;
  }

  function nudgeIntoViewport() {
    const frame = getFrame();
    if (!frame) return;

    const rect = frame.getBoundingClientRect();
    const current = currentTranslate(frame);
    let dx = 0;
    let dy = 0;

    if (rect.left < VIEWPORT_MARGIN) dx += VIEWPORT_MARGIN - rect.left;
    if (rect.right > window.innerWidth - VIEWPORT_MARGIN) dx -= rect.right - (window.innerWidth - VIEWPORT_MARGIN);
    if (rect.top < VIEWPORT_MARGIN) dy += VIEWPORT_MARGIN - rect.top;
    if (rect.bottom > window.innerHeight - VIEWPORT_MARGIN) dy -= rect.bottom - (window.innerHeight - VIEWPORT_MARGIN);

    if (dx || dy) saveTranslate(frame, current.x + dx, current.y + dy);
  }

  function applyOrientation() {
    const frame = getFrame();
    const layer = getLayer();
    if (!frame || !layer) return;

    frame.classList.toggle('pcall-video-landscape', landscape);
    layer.classList.toggle('pcall-video-landscape-layer', landscape);

    if (landscape) {
      const width = Math.max(1, frame.offsetWidth);
      const height = Math.max(1, frame.offsetHeight);
      frame.style.setProperty('--pcall-phone-portrait-width', `${width}px`);
      frame.style.setProperty('--pcall-phone-portrait-height', `${height}px`);
    }

    requestAnimationFrame(nudgeIntoViewport);
  }

  function updateControlLabels() {
    const full = document.querySelector('[data-pcall-layout-full] span');
    const rotate = document.querySelector('[data-pcall-layout-rotate] span');
    if (full) full.textContent = expanded ? 'Réduire' : 'Plein écran';
    if (rotate) rotate.textContent = landscape ? 'Portrait' : 'Tourner';
  }

  function ensureExtraControls(card) {
    const controls = card.querySelector('.pcall-controls');
    if (!controls) return;

    if (!controls.querySelector('[data-pcall-layout-full]')) {
      const full = document.createElement('button');
      full.type = 'button';
      full.className = 'pcall-control pcall-layout-control';
      full.setAttribute('data-pcall-layout-full', '1');
      full.innerHTML = '⛶<span>Plein écran</span>';
      const hangup = controls.querySelector('.is-hangup');
      controls.insertBefore(full, hangup || null);
    }

    if (!controls.querySelector('[data-pcall-layout-rotate]')) {
      const rotate = document.createElement('button');
      rotate.type = 'button';
      rotate.className = 'pcall-control pcall-layout-control';
      rotate.setAttribute('data-pcall-layout-rotate', '1');
      rotate.innerHTML = '↻<span>Tourner</span>';
      const hangup = controls.querySelector('.is-hangup');
      controls.insertBefore(rotate, hangup || null);
    }

    updateControlLabels();
  }

  function ensureRoomWhisperBadge(card) {
    const head = card.querySelector('.pcall-video-head');
    if (!head || head.querySelector('.pcall-room-whisper-badge')) return;
    const badge = document.createElement('em');
    badge.className = 'pcall-room-whisper-badge';
    badge.textContent = 'Murmure salle actif';
    head.appendChild(badge);
  }

  function applyVideoLayout() {
    const layer = getLayer();
    const card = getVideoCard();
    const frame = getFrame();
    if (!layer || !card || !frame) return false;

    if (!videoMounted) {
      videoMounted = true;
      expanded = true;
      landscape = false;
    }

    layer.classList.toggle('pcall-video-expanded', expanded);
    ensureExtraControls(card);
    ensureRoomWhisperBadge(card);
    applyOrientation();
    return true;
  }

  function cleanupVideoLayout() {
    const frame = getFrame();
    const layer = getLayer();

    frame?.classList.remove('pcall-video-landscape');
    layer?.classList.remove('pcall-video-expanded', 'pcall-video-landscape-layer');
    frame?.style.removeProperty('--pcall-phone-portrait-width');
    frame?.style.removeProperty('--pcall-phone-portrait-height');

    videoMounted = false;
    expanded = true;
    landscape = false;
    requestAnimationFrame(nudgeIntoViewport);
  }

  function startDrag(event, handle) {
    if (event.button !== 0 && event.pointerType !== 'touch') return;
    if (event.target.closest('button,input,a,video')) return;

    const frame = getFrame();
    if (!frame) return;

    const base = currentTranslate(frame);
    const rect = frame.getBoundingClientRect();
    drag = {
      pointerId: event.pointerId,
      handle,
      frame,
      startX: event.clientX,
      startY: event.clientY,
      baseX: base.x,
      baseY: base.y,
      rect
    };

    frame.classList.add('pcall-phone-dragging');
    try { handle.setPointerCapture(event.pointerId); } catch {}
    event.preventDefault();
  }

  function moveDrag(event) {
    if (!drag || event.pointerId !== drag.pointerId) return;

    let dx = event.clientX - drag.startX;
    let dy = event.clientY - drag.startY;

    const minDx = VIEWPORT_MARGIN - drag.rect.left;
    const maxDx = window.innerWidth - VIEWPORT_MARGIN - drag.rect.right;
    const minDy = VIEWPORT_MARGIN - drag.rect.top;
    const maxDy = window.innerHeight - VIEWPORT_MARGIN - drag.rect.bottom;

    dx = Math.max(minDx, Math.min(maxDx, dx));
    dy = Math.max(minDy, Math.min(maxDy, dy));
    saveTranslate(drag.frame, drag.baseX + dx, drag.baseY + dy);
  }

  function endDrag(event) {
    if (!drag || (event && event.pointerId !== drag.pointerId)) return;
    drag.frame.classList.remove('pcall-phone-dragging');
    try { drag.handle.releasePointerCapture(drag.pointerId); } catch {}
    drag = null;
  }

  document.addEventListener('click', event => {
    const full = event.target.closest?.('[data-pcall-layout-full]');
    if (full) {
      event.preventDefault();
      event.stopPropagation();
      expanded = !expanded;
      getLayer()?.classList.toggle('pcall-video-expanded', expanded);
      updateControlLabels();
      requestAnimationFrame(nudgeIntoViewport);
      return;
    }

    const rotate = event.target.closest?.('[data-pcall-layout-rotate]');
    if (rotate) {
      event.preventDefault();
      event.stopPropagation();
      landscape = !landscape;
      applyOrientation();
      updateControlLabels();
    }
  }, true);

  document.addEventListener('pointerdown', event => {
    const handle = event.target.closest?.('.paradise-call-stable-v2 .pcall-video-head, .paradise-call-stable-v2 .pcall-topline');
    if (!handle) return;
    startDrag(event, handle);
  }, true);

  document.addEventListener('pointermove', moveDrag, true);
  document.addEventListener('pointerup', endDrag, true);
  document.addEventListener('pointercancel', endDrag, true);
  window.addEventListener('resize', () => requestAnimationFrame(nudgeIntoViewport), { passive: true });

  window.setInterval(() => {
    if (!applyVideoLayout() && videoMounted) cleanupVideoLayout();
  }, CHECK_MS);

  console.info('[ParadisePhone] layout appel vidéo V1 actif');
})();