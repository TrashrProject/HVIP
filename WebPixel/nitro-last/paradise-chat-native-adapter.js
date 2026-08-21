(() => {
  'use strict';

  const VERSION = '3.1.0-quality-gate-chat-lifecycle';
  const HUD_ID = 'paradise-rp-hud';
  const PARADISE_INPUT_ID = 'pr4-chat-input';

  if (window.__ParadiseNativeChatAdapter?.version === VERSION) return;

  let paradiseInput = null;
  let nativeInput = null;
  let hudAbort = null;
  let nativeAbort = null;
  let nitroObserver = null;
  let observerScanScheduled = false;
  let sending = false;
  let destroyed = false;

  const diag = {
    version: VERSION,
    sends: 0,
    lastMessage: '',
    lastNativeFound: false,
    lastBeforeValue: '',
    lastValueBeforeEnter: '',
    lastAfterValue: '',
    lastReactProps: [],
    lastReactChangeCalled: false,
    lastReactEnterCalled: false,
    lastConsumed: false,
    lastError: null,
    observerActive: false,
    observerCallbacks: 0,
    observerScans: 0
  };

  const nextFrame = () => new Promise(resolve => requestAnimationFrame(() => resolve()));
  const delay = ms => new Promise(resolve => setTimeout(resolve, ms));

  function findParadiseInput() {
    return document.getElementById(PARADISE_INPUT_ID);
  }

  function findNativeInput() {
    const explicit = document.querySelector('#root [data-pr-native-chat-bridge="1"]');
    if (explicit && explicit.isConnected) return explicit;

    return [...document.querySelectorAll('#root input, #root textarea')].find(el => {
      if (!el || el.disabled || el.readOnly || el.closest(`#${HUD_ID}`)) return false;
      const value = `${el.placeholder || ''} ${el.className || ''} ${el.id || ''}`;
      return /haz|chatear|chat|chatter|parler|message|say/i.test(value);
    }) || null;
  }

  function getReactProps(node) {
    if (!node) return null;
    const key = Object.keys(node).find(name => name.startsWith('__reactProps$'));
    return key ? node[key] : null;
  }

  function collectReactProps(input) {
    const found = [];
    let node = input;
    for (let depth = 0; node && depth < 6; depth += 1, node = node.parentElement) {
      const props = getReactProps(node);
      if (!props) continue;
      found.push({ node, props, keys: Object.keys(props).filter(key => /^on[A-Z]/.test(key)) });
    }
    diag.lastReactProps = found.flatMap(item => item.keys);
    return found;
  }

  function setNativeValue(input, value) {
    const win = input.ownerDocument?.defaultView || window;
    const proto = input instanceof win.HTMLTextAreaElement ? win.HTMLTextAreaElement.prototype : win.HTMLInputElement.prototype;
    const setter = Object.getOwnPropertyDescriptor(proto, 'value')?.set;
    if (setter) setter.call(input, String(value ?? ''));
    else input.value = String(value ?? '');
  }

  function dispatchNativeInput(input) {
    const win = input.ownerDocument?.defaultView || window;
    try {
      input.dispatchEvent(new win.InputEvent('input', {
        bubbles: true,
        composed: true,
        inputType: 'insertText',
        data: String(input.value || '')
      }));
    } catch (_) {
      input.dispatchEvent(new win.Event('input', { bubbles: true, composed: true }));
    }
    input.dispatchEvent(new win.Event('change', { bubbles: true, composed: true }));
  }

  function invokeReactChange(input) {
    const chain = collectReactProps(input);
    for (const item of chain) {
      const handler = item.props.onChange || item.props.onInput;
      if (typeof handler !== 'function') continue;
      const event = {
        type: 'change',
        target: input,
        currentTarget: item.node,
        nativeEvent: { type: 'input', target: input },
        defaultPrevented: false,
        isPropagationStopped: () => false,
        isDefaultPrevented() { return this.defaultPrevented; },
        preventDefault() { this.defaultPrevented = true; },
        stopPropagation() {},
        persist() {}
      };
      try {
        handler(event);
        diag.lastReactChangeCalled = true;
        return true;
      } catch (error) {
        diag.lastError = `react-change: ${error?.message || error}`;
      }
    }
    return false;
  }

  function makeEnterEvent(type, input) {
    const win = input.ownerDocument?.defaultView || window;
    const event = new win.KeyboardEvent(type, {
      key: 'Enter', code: 'Enter', bubbles: true, cancelable: true, composed: true
    });
    for (const [name, value] of [['keyCode', 13], ['which', 13], ['charCode', type === 'keypress' ? 13 : 0]]) {
      try { Object.defineProperty(event, name, { configurable: true, get: () => value }); } catch (_) {}
    }
    return event;
  }

  function invokeReactEnter(input) {
    const chain = collectReactProps(input);
    for (const item of chain) {
      const handler = item.props.onKeyDown || item.props.onKeyPress || item.props.onKeyUp;
      if (typeof handler !== 'function') continue;
      const fake = {
        type: 'keydown',
        key: 'Enter', code: 'Enter', keyCode: 13, which: 13, charCode: 0,
        target: input, currentTarget: item.node,
        nativeEvent: { type: 'keydown', key: 'Enter', code: 'Enter', keyCode: 13, which: 13, target: input },
        defaultPrevented: false,
        isPropagationStopped: () => false,
        isDefaultPrevented() { return this.defaultPrevented; },
        preventDefault() { this.defaultPrevented = true; },
        stopPropagation() {},
        persist() {}
      };
      try {
        handler(fake);
        diag.lastReactEnterCalled = true;
        return true;
      } catch (error) {
        diag.lastError = `react-enter: ${error?.message || error}`;
      }
    }
    return false;
  }

  function bindNative() {
    if (destroyed) return false;
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

    nativeInput.addEventListener('input', () => {
      diag.lastAfterValue = String(nativeInput?.value ?? '');
    }, { signal: nativeAbort.signal });

    return true;
  }

  async function sendThroughNitro(message) {
    const messageText = String(message || '').trim();
    if (!messageText) return true;
    if (sending || destroyed) return false;
    if (!bindNative() || !nativeInput) return false;

    sending = true;
    diag.sends += 1;
    diag.lastMessage = messageText;
    diag.lastError = null;
    diag.lastConsumed = false;
    diag.lastReactChangeCalled = false;
    diag.lastReactEnterCalled = false;

    try {
      let input = nativeInput;
      diag.lastBeforeValue = String(input.value ?? '');

      setNativeValue(input, messageText);
      try { input.focus({ preventScroll: true }); } catch (_) {}
      dispatchNativeInput(input);
      invokeReactChange(input);

      await nextFrame();
      await delay(0);

      bindNative();
      input = nativeInput || input;
      if (String(input.value ?? '') !== messageText) {
        setNativeValue(input, messageText);
        dispatchNativeInput(input);
      }

      diag.lastValueBeforeEnter = String(input.value ?? '');
      try { input.focus({ preventScroll: true }); } catch (_) {}

      input.dispatchEvent(makeEnterEvent('keydown', input));
      input.dispatchEvent(makeEnterEvent('keypress', input));
      input.dispatchEvent(makeEnterEvent('keyup', input));

      await nextFrame();
      diag.lastAfterValue = String(input.value ?? '');

      if (diag.lastAfterValue === messageText) {
        invokeReactEnter(input);
        await nextFrame();
        await delay(10);
        diag.lastAfterValue = String((nativeInput && nativeInput.isConnected ? nativeInput : input).value ?? '');
      }

      diag.lastConsumed = diag.lastAfterValue !== messageText;
      return diag.lastConsumed;
    } catch (error) {
      diag.lastError = error?.message || String(error);
      console.warn('[ParadiseRP:chat] Nitro send failed', error);
      return false;
    } finally {
      sending = false;
    }
  }

  function bindParadise() {
    if (destroyed) return false;
    const next = findParadiseInput();
    if (!next) return false;
    if (next === paradiseInput && hudAbort) return true;

    hudAbort?.abort();
    hudAbort = new AbortController();
    paradiseInput = next;
    paradiseInput.readOnly = false;
    paradiseInput.removeAttribute('readonly');
    paradiseInput.dataset.prChatDirect = '1';
    paradiseInput.tabIndex = 0;

    paradiseInput.addEventListener('keydown', async event => {
      if (event.key === 'Enter' || event.keyCode === 13) {
        event.preventDefault();
        event.stopImmediatePropagation();
        const value = paradiseInput.value;
        if (!String(value || '').trim()) {
          paradiseInput.blur();
          return;
        }

        paradiseInput.disabled = true;
        const sent = await sendThroughNitro(value);
        paradiseInput.disabled = false;

        if (sent) {
          paradiseInput.value = '';
          paradiseInput.dispatchEvent(new Event('input', { bubbles: true }));
          paradiseInput.blur();
        } else {
          paradiseInput.focus({ preventScroll: true });
          console.warn('[ParadiseRP:chat] Nitro did not consume message', diag);
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
    if (destroyed) return;
    bindParadise();
    bindNative();
  }

  function scheduleObserverScan() {
    if (destroyed || observerScanScheduled) return;
    if (nativeInput?.isConnected && paradiseInput?.isConnected) return;
    observerScanScheduled = true;
    requestAnimationFrame(() => {
      observerScanScheduled = false;
      if (destroyed) return;
      diag.observerScans += 1;
      if (!nativeInput?.isConnected) bindNative();
      if (!paradiseInput?.isConnected) bindParadise();
    });
  }

  function destroy() {
    if (destroyed) return;
    destroyed = true;
    nitroObserver?.disconnect();
    nitroObserver = null;
    hudAbort?.abort();
    nativeAbort?.abort();
    hudAbort = null;
    nativeAbort = null;
    diag.observerActive = false;
  }

  function boot() {
    scan();
    const nitroRoot = document.getElementById('root');
    if (nitroRoot) {
      nitroObserver = new MutationObserver(() => {
        diag.observerCallbacks += 1;
        scheduleObserverScan();
      });
      nitroObserver.observe(nitroRoot, { childList: true, subtree: true });
      diag.observerActive = true;
    }

    window.__ParadiseNativeChatAdapter = {
      version: VERSION,
      scan,
      send: sendThroughNitro,
      diag,
      destroy,
      get nativeInput() { return nativeInput; },
      get paradiseInput() { return paradiseInput; }
    };

    window.addEventListener('beforeunload', destroy, { once: true });
    console.info('[ParadiseRP:chat] React-state synchronized adapter active', { version: VERSION });
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
