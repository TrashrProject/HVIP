(() => {
  'use strict';

  const VERSION = '1.0.0-real-native-chat-focus';
  const HUD_ID = 'paradise-rp-hud';
  const PARADISE_INPUT_ID = 'pr4-chat-input';

  if (window.__ParadiseNativeChatAdapter?.version === VERSION) return;

  let paradiseInput = null;
  let nativeInput = null;
  let nativeAbort = null;
  let hudAbort = null;

  function findParadiseInput() {
    return document.getElementById(PARADISE_INPUT_ID);
  }

  function findNativeInput() {
    const explicit = document.querySelector('#root [data-pr-native-chat-bridge="1"]');
    if (explicit) return explicit;

    return [...document.querySelectorAll('#root input, #root textarea')].find(el => {
      if (!el || el.disabled || el.readOnly || el.closest(`#${HUD_ID}`)) return false;
      const text = `${el.placeholder || ''} ${el.className || ''} ${el.id || ''}`;
      return /haz|chatear|chat|chatter|parler|message|say/i.test(text);
    }) || null;
  }

  function syncFromNative() {
    if (!paradiseInput || !nativeInput) return;
    const value = String(nativeInput.value ?? '');
    if (paradiseInput.value !== value) paradiseInput.value = value;
  }

  function focusNative() {
    if (!nativeInput || !nativeInput.isConnected) bindNative();
    if (!nativeInput) return false;

    try {
      nativeInput.removeAttribute('aria-hidden');
      nativeInput.focus({ preventScroll: true });
      const length = String(nativeInput.value || '').length;
      nativeInput.setSelectionRange?.(length, length);
      syncFromNative();
      paradiseInput?.classList.add('is-native-focused');
      return document.activeElement === nativeInput;
    } catch (error) {
      console.warn('[ParadiseRP:chat] native focus failed', error);
      return false;
    }
  }

  function bindNative() {
    const next = findNativeInput();
    if (!next) return false;
    if (next === nativeInput && nativeAbort) return true;

    nativeAbort?.abort();
    nativeAbort = new AbortController();
    nativeInput = next;
    nativeInput.dataset.prNativeChatBridge = '1';

    nativeInput.addEventListener('input', syncFromNative, { signal: nativeAbort.signal });
    nativeInput.addEventListener('change', syncFromNative, { signal: nativeAbort.signal });
    nativeInput.addEventListener('focus', () => paradiseInput?.classList.add('is-native-focused'), { signal: nativeAbort.signal });
    nativeInput.addEventListener('blur', () => {
      paradiseInput?.classList.remove('is-native-focused');
      window.setTimeout(syncFromNative, 0);
    }, { signal: nativeAbort.signal });
    nativeInput.addEventListener('keydown', event => {
      // This is the REAL Nitro input receiving the REAL keyboard event.
      // Do not prevent Enter: Nitro must handle and send it itself.
      if (event.key === 'Enter') {
        window.setTimeout(syncFromNative, 0);
        window.setTimeout(syncFromNative, 30);
      } else if (event.key === 'Escape') {
        window.setTimeout(() => {
          try { nativeInput.blur(); } catch (_) {}
          syncFromNative();
        }, 0);
      }
    }, { signal: nativeAbort.signal });

    syncFromNative();
    return true;
  }

  function bindParadise() {
    const next = findParadiseInput();
    if (!next) return false;
    if (next === paradiseInput && hudAbort) return true;

    hudAbort?.abort();
    hudAbort = new AbortController();
    paradiseInput = next;

    // Paradise is the visible shell. Nitro remains the actual focused input.
    paradiseInput.readOnly = true;
    paradiseInput.dataset.prNativeChatVisual = '1';

    const redirect = event => {
      // Keep the click inside Paradise, but put the keyboard focus on Nitro.
      event?.preventDefault?.();
      event?.stopPropagation?.();
      bindNative();
      focusNative();
    };

    paradiseInput.addEventListener('pointerdown', redirect, { signal: hudAbort.signal });
    paradiseInput.addEventListener('click', redirect, { signal: hudAbort.signal });
    paradiseInput.addEventListener('focus', redirect, { signal: hudAbort.signal });

    const module = paradiseInput.closest('.pr4-chat-module');
    module?.addEventListener('pointerdown', event => {
      if (event.target === paradiseInput) return;
      redirect(event);
    }, { signal: hudAbort.signal });

    syncFromNative();
    return true;
  }

  function scan() {
    bindParadise();
    bindNative();
  }

  function boot() {
    scan();

    const observer = new MutationObserver(() => {
      // Nitro may recreate its chat input during room transitions.
      if (!nativeInput?.isConnected || !paradiseInput?.isConnected) scan();
    });
    observer.observe(document.body, { childList: true, subtree: true });

    window.__ParadiseNativeChatAdapter = {
      version: VERSION,
      scan,
      focus: focusNative,
      get nativeInput() { return nativeInput; },
      get paradiseInput() { return paradiseInput; }
    };

    console.info('[ParadiseRP:chat] real Nitro input adapter active', { version: VERSION });
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
