(() => {
  'use strict';

  if (window.__PARADISE_PHONE_VIDEO_V4__) return;
  window.__PARADISE_PHONE_VIDEO_V4__ = '4.0.0';

  const state = { landscape: false };

  function getFrame() {
    return document.querySelector('.nitro-phone-frame');
  }

  function getLayer() {
    return document.querySelector('.paradise-call-layer');
  }

  function getActiveCard() {
    return getLayer()?.querySelector('.pcall-active') || null;
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

  function moveWhispersToRoom(card) {
    const panel = card?.querySelector('.pcall-whisper-panel');
    if (!panel) return;
    const dock = ensureWhisperDock();
    const previous = dock.querySelector('.pcall-whisper-panel');
    if (previous && previous !== panel) previous.remove();
    panel.classList.add('pcall-whisper-on-room');
    dock.appendChild(panel);
  }

  function addVideoControls(card) {
    const controls = card?.querySelector('.pcall-controls');
    if (!controls || controls.querySelector('[data-pcall-v4-rotate]')) return;

    const rotate = document.createElement('button');
    rotate.type = 'button';
    rotate.className = 'pcall-control pcall-v4-rotate';
    rotate.dataset.pcallV4Rotate = '1';
    rotate.innerHTML = '↻<span>Tourner</span>';
    rotate.title = 'Passer l’appel vidéo en mode paysage';
    controls.prepend(rotate);
  }

  function applyLandscape(enabled) {
    state.landscape = Boolean(enabled);
    const layer = getLayer();
    const frame = getFrame();
    layer?.classList.toggle('pcall-v4-landscape', state.landscape);
    frame?.classList.toggle('pcall-v4-phone-landscape', state.landscape);
    document.body.classList.toggle('pcall-v4-landscape-open', state.landscape);
  }

  function enhance() {
    const layer = getLayer();
    const card = getActiveCard();
    const frame = getFrame();

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
      layer.classList.toggle('pcall-v4-landscape', state.landscape);
      frame?.classList.toggle('pcall-v4-phone-landscape', state.landscape);
    } else {
      applyLandscape(false);
      layer.classList.remove('pcall-v4-video-fullscreen');
    }
  }

  document.addEventListener('click', event => {
    const rotate = event.target.closest?.('[data-pcall-v4-rotate]');
    if (!rotate) return;
    event.preventDefault();
    event.stopPropagation();
    applyLandscape(!state.landscape);
    enhance();
  }, true);

  document.addEventListener('keydown', event => {
    if (event.key !== 'Escape' || !state.landscape) return;
    applyLandscape(false);
    enhance();
  }, true);

  const observer = new MutationObserver(() => enhance());

  function boot() {
    observer.observe(document.documentElement, { childList: true, subtree: true, attributes: true, attributeFilter: ['class'] });
    enhance();
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();