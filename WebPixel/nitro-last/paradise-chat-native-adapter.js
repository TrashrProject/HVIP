(() => {
  'use strict';

  const VERSION = '2.0.0-direct-paradise-input';
  const HUD_ID = 'paradise-rp-hud';
  const PARADISE_INPUT_ID = 'pr4-chat-input';

  if (window.__ParadiseNativeChatAdapter?.version === VERSION) return;

  let paradiseInput = null;
  let nativeInput = null;
  let hudAbort = null;
  let nativeAbort = null;

  const diag = {
    version: VERSION,
    sends: 0,
    lastMessage: '',
    lastNativeFound: false,
    lastBeforeValue: '',
    lastAfterValue: '',
    lastError: null
  };

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

  function setNativeValue(input, value) {
    const win = input.ownerDocument?.defaultView || window;
    const proto = input instanceof win.HTMLTextAreaElement ? win.HTMLTextAreaElement.prototype : win.HTMLInputElement.prototype;
    const setter = Object.getOwnPropertyDescriptor(proto, 'value')?.set;
    if (setter) setter.call(input, String(value ?? ''));
    else input.value = String(value ?? '');

    input.dispatchEvent(new win.Event('input', { bubbles: true, composed: true }));
    input.dispatchEvent(new win.Event('change', { bubbles: true, composed: true }));
  }

  function makeEnterEvent(type) {
    const win = nativeInput?.ownerDocument?.defaultView || window;
    const event = new win.KeyboardEvent(type, {
      key: 'Enter',
      code: 'Enter',
      bubbles: true,
      cancelable: true,
      composed: true
    });

    // Older Nitro/Habbo handlers often test keyCode/which instead of event.key.
    for (const [name, value] of [['keyCode', 13], ['which', 13], ['charCode', type === 'keypress' ? 13 : 0]]) {
      try { Object.defineProperty(event, name, { configurable: true, get: () => value }); } catch (_) {}
    }

    return event;
  }

  function invokeReactEnterFallback(input) {
    try {
      let node = input;
      for (let depth = 0; node && depth < 5; depth += 1, node = node.parentElement) {
        const propsKey = Object.keys(node).find(key => key.startsWith('__reactProps$'));
        const props = propsKey ? node[propsKey] : null;
        if (!props) continue;

        const handler = props.onKeyDown || props.onKeyPress || props.onKeyUp;
        if (typeof handler !== 'function') continue;

        const fake = {
          key: 'Enter', code: 'Enter', keyCode: 13, which: 13, charCode: 13,
          target: input, currentTarget: node,
          nativeEvent: { key: 'Enter', code: 'Enter', keyCode: 13, which: 13 },
          defaultPrevented: false,
          preventDefault() { this.defaultPrevented = true; },
          stopPropagation() {},
          persist() {}
        };
        handler(fake);
        return true;
      }
    } catch (error) {
      diag.lastError = `react-fallback: ${error?.message || error}`;
    }
    return false;
  }

  function bindNative() {
    const next = findNativeInput();
    if (!next) {
      nativeInput = null;
      diag.lastNativeFound = false;
      return false;
    }

    diag.lastNativeFound = true;
    if (next === nativeInput && nativeAbort) return true;

    nativeAbort?.abort();
    nativeAbort = new AbortController();
    nativeInput = next;
    nativeInput.dataset.prNativeChatBridge = '1';

    // Keep Nitro's own field alive only as the network/controller bridge.
    nativeInput.addEventListener('input', () => {
      diag.lastAfterValue = String(nativeInput?.value ?? '');
    }, { signal: nativeAbort.signal });

    return true;
  }

  function sendThroughNitro(message) {
    const text = String(message || '').trim();
    if (!text) return true;
    if (!bindNative() || !nativeInput) return false;

    try {
      diag.sends += 1;
      diag.lastMessage = text;
      diag.lastError = null;
      diag.lastBeforeValue = String(nativeInput.value ?? '');

      setNativeValue(nativeInput, text);

      // Dispatch the complete legacy keyboard sequence with keyCode compatibility.
      nativeInput.dispatchEvent(makeEnterEvent('keydown'));
      nativeInput.dispatchEvent(makeEnterEvent('keypress'));
      nativeInput.dispatchEvent(makeEnterEvent('keyup'));

      // If Nitro did not consume/clear the field, try its React key handler directly.
      window.setTimeout(() => {
        try {
          diag.lastAfterValue = String(nativeInput?.value ?? '');
          if (nativeInput && nativeInput.value === text) invokeReactEnterFallback(nativeInput);
          window.setTimeout(() => {
            diag.lastAfterValue = String(nativeInput?.value ?? '');
          }, 30);
        } catch (_) {}
      }, 0);

      return true;
    } catch (error) {
      diag.lastError = error?.message || String(error);
      console.warn('[ParadiseRP:chat] Nitro send failed', error);
      return false;
    }
  }

  function bindParadise() {
    const next = findParadiseInput();
    if (!next) return false;
    if (next === paradiseInput && hudAbort) return true;

    hudAbort?.abort();
    hudAbort = new AbortController();
    paradiseInput = next;

    // Paradise is now a REAL writable input. No focus redirection, no readonly shell.
    paradiseInput.readOnly = false;
    paradiseInput.removeAttribute('readonly');
    paradiseInput.removeAttribute('data-pr-native-chat-visual');
    paradiseInput.dataset.prChatDirect = '1';
    paradiseInput.tabIndex = 0;

    paradiseInput.addEventListener('keydown', event => {
      if (event.key === 'Enter' || event.keyCode === 13) {
        event.preventDefault();
        event.stopImmediatePropagation();

        const text = paradiseInput.value;
        if (!String(text || '').trim()) {
          paradiseInput.blur();
          return;
        }

        if (sendThroughNitro(text)) {
          paradiseInput.value = '';
          paradiseInput.dispatchEvent(new Event('input', { bubbles: true }));
          paradiseInput.blur();
        } else {
          console.warn('[ParadiseRP:chat] Nitro native input not found');
        }
        return;
      }

      if (event.key === 'Escape') {
        event.preventDefault();
        event.stopImmediatePropagation();
        paradiseInput.blur();
      }
    }, { signal: hudAbort.signal });

    return true;
  }

  function scan() {
    bindParadise();
    bindNative();
  }

  function boot() {
    scan();

    const nitroRoot = document.getElementById('root');
    if (nitroRoot) {
      const observer = new MutationObserver(() => {
        if (!nativeInput?.isConnected) bindNative();
        if (!paradiseInput?.isConnected) bindParadise();
      });
      observer.observe(nitroRoot, { childList: true, subtree: true });
    }

    window.__ParadiseNativeChatAdapter = {
      version: VERSION,
      scan,
      send: sendThroughNitro,
      diag,
      get nativeInput() { return nativeInput; },
      get paradiseInput() { return paradiseInput; }
    };

    console.info('[ParadiseRP:chat] direct Paradise input adapter active', { version: VERSION });
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
