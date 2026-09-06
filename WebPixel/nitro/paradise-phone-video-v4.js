(() => {
  'use strict';

  if (window.__PARADISE_PHONE_VIDEO_V4__) return;
  window.__PARADISE_PHONE_VIDEO_V4__ = '4.0.2';

  const state = { landscape: false, queued: false };
  const RELEVANT_SELECTOR = '.nitro-phone-frame,.paradise-call-layer,.pcall-active,.pcall-whisper-panel,.pcall-controls';

  function getFrame() {
    return document.querySelector('.nitro-phone-frame');
  }

  function getLayer() {
    return document.querySelector('.paradise-call-layer');
  }

  function getActiveCard(layer = getLayer()) {
    return layer?.querySelector('.pcall-active') || null;
  }

  function ensureWhisperDock() {
    let dock = document.querySelector('.pcall-room-whisper-dock');
    if (!dock) {
      dock = document.createElement('div');
      dock.className = 'pcall-room-whisper-dock';
      dock.setAttribute('aria-label', 'Murmures privés de l’appel');
      document.body.appendChild(dock);
    }
    return dock;
  }

  function clearWhisperDock() {
    document.querySelector('.pcall-room-whisper-dock')?.remove();
  }

  function positionWhisperDock() {
    const dock = document.querySelector('.pcall-room-whisper-dock');
    const frame = getFrame();
    if (!dock) return;

    dock.style.cssText = '';

    if (state.landscape || !frame) {
      dock.style.left = '50%';
      dock.style.bottom = '22px';
      dock.style.transform = 'translateX(-50%)';
      dock.style.width = 'min(500px, calc(100vw - 28px))';
      return;
    }

    const rect = frame.getBoundingClientRect();
    const rightSpace = window.innerWidth - rect.right;
    const leftSpace = rect.left;

    if (rightSpace >= 330) {
      dock.style.left = `${Math.round(rect.right + 18)}px`;
      dock.style.bottom = '64px';
      dock.style.width = `${Math.min(390, Math.max(280, rightSpace - 34))}px`;
      return;
    }

    if (leftSpace >= 330) {
      dock.style.right = `${Math.round(window.innerWidth - rect.left + 18)}px`;
      dock.style.bottom = '64px';
      dock.style.width = `${Math.min(390, Math.max(280, leftSpace - 34))}px`;
      return;
    }

    dock.style.left = '50%';
    dock.style.bottom = '58px';
    dock.style.transform = 'translateX(-50%)';
    dock.style.width = 'min(410px, calc(100vw - 34px))';
  }

  function moveWhispersToRoom(card) {
    const panel = card?.querySelector('.pcall-whisper-panel');
    if (!panel) return;
    const dock = ensureWhisperDock();
    const previous = dock.querySelector('.pcall-whisper-panel');
    if (previous && previous !== panel) previous.remove();
    panel.classList.add('pcall-whisper-on-room');
    dock.appendChild(panel);
    positionWhisperDock();
  }

  function addVideoControls(card) {
    const controls = card?.querySelector('.pcall-controls');
    if (!controls || controls.querySelector('[data-pcall-v4-rotate]')) return;

    const rotate = document.createElement('button');
    rotate.type = 'button';
    rotate.className = 'pcall-control pcall-v4-rotate';
    rotate.dataset.pcallV4Rotate = '1';
    rotate.innerHTML = '↻<span>Tourner</span>';
    rotate.title = 'Tourner le téléphone en mode paysage';
    controls.prepend(rotate);
  }

  function applyLandscape(enabled) {
    state.landscape = Boolean(enabled);
    const layer = getLayer();
    const frame = getFrame();

    if (layer) {
      if (state.landscape) {
        if (layer.parentElement !== document.body) document.body.appendChild(layer);
      } else if (frame && layer.parentElement !== frame) {
        frame.appendChild(layer);
      }
      layer.classList.toggle('pcall-v4-landscape', state.landscape);
    }

    frame?.classList.toggle('pcall-v4-phone-landscape', state.landscape);
    document.body.classList.toggle('pcall-v4-landscape-open', state.landscape);
    positionWhisperDock();
  }

  function enhance() {
    const layer = getLayer();
    const frame = getFrame();
    const card = getActiveCard(layer);

    if (!layer || !card) {
      frame?.classList.remove('pcall-v4-video-active', 'pcall-v4-phone-landscape');
      document.body.classList.remove('pcall-v4-video-call', 'pcall-v4-landscape-open');
      clearWhisperDock();
      state.landscape = false;
      return;
    }

    moveWhispersToRoom(card);

    const video = card.classList.contains('is-video');
    layer.classList.toggle('pcall-v4-video-fullscreen', video);
    frame?.classList.toggle('pcall-v4-video-active', video);
    document.body.classList.toggle('pcall-v4-video-call', video);

    if (video) {
      addVideoControls(card);
      if (state.landscape && layer.parentElement !== document.body) document.body.appendChild(layer);
      layer.classList.toggle('pcall-v4-landscape', state.landscape);
      frame?.classList.toggle('pcall-v4-phone-landscape', state.landscape);
    } else {
      if (state.landscape) applyLandscape(false);
      layer.classList.remove('pcall-v4-video-fullscreen', 'pcall-v4-landscape');
    }

    positionWhisperDock();
  }

  function scheduleEnhance() {
    if (state.queued) return;
    state.queued = true;
    requestAnimationFrame(() => {
      state.queued = false;
      enhance();
    });
  }

  function mutationIsRelevant(record) {
    if (record.type !== 'childList') return false;
    const nodes = [...record.addedNodes, ...record.removedNodes];
    return nodes.some(node => {
      if (!(node instanceof Element)) return false;
      return node.matches(RELEVANT_SELECTOR) || Boolean(node.querySelector?.(RELEVANT_SELECTOR));
    });
  }

  document.addEventListener('click', event => {
    const rotate = event.target.closest?.('[data-pcall-v4-rotate]');
    if (!rotate) return;
    event.preventDefault();
    event.stopPropagation();
    applyLandscape(!state.landscape);
    scheduleEnhance();
  }, true);

  document.addEventListener('keydown', event => {
    if (event.key !== 'Escape' || !state.landscape) return;
    applyLandscape(false);
    scheduleEnhance();
  }, true);

  window.addEventListener('resize', () => {
    if (document.querySelector('.pcall-room-whisper-dock')) positionWhisperDock();
  }, { passive: true });

  const observer = new MutationObserver(records => {
    if (records.some(mutationIsRelevant)) scheduleEnhance();
  });

  function boot() {
    observer.observe(document.body, { childList: true, subtree: true });
    scheduleEnhance();
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();