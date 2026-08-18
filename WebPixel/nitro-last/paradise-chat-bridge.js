(() => {
  'use strict';

  const VERSION = '2.0.0-force-enter-chain';
  const HUD_ID = 'paradise-rp-hud';
  const HUD_INPUT_ID = 'prhud-chat-input';
  const HUD_FORM_ID = 'prhud-chat-form';

  const sleep = ms => new Promise(resolve => setTimeout(resolve, ms));

  const getDocs = () => {
    const docs = [document];
    try {
      document.querySelectorAll('iframe').forEach(frame => {
        try {
          const doc = frame.contentDocument || frame.contentWindow?.document;
          if (doc && !docs.includes(doc)) docs.push(doc);
        } catch (_) {}
      });
    } catch (_) {}
    return docs;
  };

  const isHudElement = el => !!(el && (el.id === HUD_INPUT_ID || el.closest?.(`#${HUD_ID}`)));

  const reactProps = el => {
    try {
      const key = Object.keys(el).find(k => k.startsWith('__reactProps$'));
      return key ? el[key] : null;
    } catch (_) {
      return null;
    }
  };

  const reactFiber = el => {
    try {
      const key = Object.keys(el).find(k => k.startsWith('__reactFiber$'));
      return key ? el[key] : null;
    } catch (_) {
      return null;
    }
  };

  const hintFor = el => {
    try {
      return `${el.id || ''} ${el.className || ''} ${el.getAttribute?.('placeholder') || ''} ${el.getAttribute?.('aria-label') || ''} ${el.getAttribute?.('title') || ''} ${el.getAttribute?.('data-pr-native-chat-parked') || ''} ${el.textContent || ''}`;
    } catch (_) {
      return '';
    }
  };

  const rectOf = el => {
    try { return el.getBoundingClientRect(); } catch (_) { return null; }
  };

  const scoreTarget = el => {
    const hint = hintFor(el);
    const rect = rectOf(el);
    let score = 0;
    if (/chat|chatear|haz|message|parler|say|input/i.test(hint)) score += 1200;
    if (el.getAttribute?.('data-pr-native-chat-parked') === '1') score += 1000;
    if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA' || el.isContentEditable) score += 300;
    if (reactProps(el)) score += 120;
    if (rect) {
      if (rect.bottom > 0) score += Math.round(rect.bottom / 3);
      if (rect.width > 20 && rect.height > 10) score += 80;
      if (rect.top > window.innerHeight * 0.55) score += 80;
    }
    return score;
  };

  const collectNativeInputs = () => {
    const found = new Set();
    for (const doc of getDocs()) {
      try {
        doc.querySelectorAll('input[type="text"], input:not([type]), textarea, [contenteditable="true"]').forEach(el => {
          if (!el || isHudElement(el)) return;
          if (el.disabled || el.readOnly) return;
          found.add(el);
        });
      } catch (_) {}
    }
    return [...found].sort((a, b) => scoreTarget(b) - scoreTarget(a));
  };

  const collectReactChatNodes = input => {
    const nodes = [];
    const push = el => {
      if (!el || nodes.includes(el) || isHudElement(el)) return;
      nodes.push(el);
    };

    let node = input;
    for (let i = 0; node && i < 10; i += 1, node = node.parentElement) push(node);

    for (const doc of getDocs()) {
      try {
        doc.querySelectorAll('input, textarea, [contenteditable="true"], div, form, section').forEach(el => {
          const props = reactProps(el);
          if (!props) return;
          const hasChatHandler = props.onKeyDown || props.onKeyPress || props.onKeyUp || props.onSubmit || props.onChange || props.onInput || props.onPaste || props.onBeforeInput;
          if (!hasChatHandler) return;
          if (/chat|message|say|input|chatear|parler/i.test(hintFor(el)) || el.contains(input) || input.contains?.(el)) push(el);
        });
      } catch (_) {}
    }

    return nodes;
  };

  const setValue = (el, value) => {
    const doc = el.ownerDocument || document;
    const win = doc.defaultView || window;
    if (el.isContentEditable) {
      el.textContent = value;
      return;
    }
    const proto = el instanceof win.HTMLTextAreaElement ? win.HTMLTextAreaElement.prototype : win.HTMLInputElement.prototype;
    const setter = Object.getOwnPropertyDescriptor(proto, 'value')?.set;
    if (setter) setter.call(el, value);
    else el.value = value;
    try { el._valueTracker?.setValue(''); } catch (_) {}
  };

  const patchKeyEvent = event => {
    for (const [key, value] of Object.entries({ keyCode: 13, which: 13, charCode: 13 })) {
      try { Object.defineProperty(event, key, { get: () => value }); } catch (_) {}
    }
    return event;
  };

  const domKeyEvent = (win, type) => patchKeyEvent(new win.KeyboardEvent(type, {
    key: 'Enter',
    code: 'Enter',
    keyCode: 13,
    which: 13,
    charCode: 13,
    bubbles: true,
    cancelable: true,
    composed: true,
    shiftKey: false,
    ctrlKey: false,
    altKey: false,
    metaKey: false,
    isComposing: false
  }));

  const fakeReactEvent = (type, target, currentTarget, extra = {}) => {
    let prevented = false;
    return {
      type,
      target,
      currentTarget,
      bubbles: true,
      cancelable: true,
      defaultPrevented: false,
      isDefaultPrevented: () => prevented,
      isPropagationStopped: () => false,
      preventDefault() { prevented = true; this.defaultPrevented = true; },
      stopPropagation() {},
      persist() {},
      nativeEvent: extra,
      key: extra.key,
      code: extra.code,
      keyCode: extra.keyCode,
      which: extra.which,
      charCode: extra.charCode,
      shiftKey: false,
      ctrlKey: false,
      altKey: false,
      metaKey: false,
      isComposing: false,
      data: extra.data,
      inputType: extra.inputType,
      clipboardData: extra.clipboardData,
      ...extra
    };
  };

  const callReactTextHandlers = (input, text) => {
    const nodes = collectReactChatNodes(input);
    for (const node of nodes) {
      const props = reactProps(node);
      if (!props) continue;
      const base = { data: text, inputType: 'insertText' };
      try { props.onFocus?.(fakeReactEvent('focus', input, node)); } catch (_) {}
      try { props.onBeforeInput?.(fakeReactEvent('beforeinput', input, node, base)); } catch (_) {}
      try { props.onInput?.(fakeReactEvent('input', input, node, base)); } catch (_) {}
      try { props.onChange?.(fakeReactEvent('change', input, node, base)); } catch (_) {}
    }
  };

  const callReactEnterHandlers = input => {
    const nodes = collectReactChatNodes(input);
    const opts = { key: 'Enter', code: 'Enter', keyCode: 13, which: 13, charCode: 13 };
    for (const node of nodes) {
      const props = reactProps(node);
      if (!props) continue;
      try { props.onKeyDown?.(fakeReactEvent('keydown', input, node, opts)); } catch (_) {}
      try { props.onKeyPress?.(fakeReactEvent('keypress', input, node, opts)); } catch (_) {}
      try { props.onKeyUp?.(fakeReactEvent('keyup', input, node, opts)); } catch (_) {}
      try { props.onSubmit?.(fakeReactEvent('submit', input, node, opts)); } catch (_) {}
    }
  };

  const dispatchTextEvents = (input, text) => {
    const doc = input.ownerDocument || document;
    const win = doc.defaultView || window;
    try {
      input.dispatchEvent(new win.InputEvent('beforeinput', { bubbles: true, cancelable: true, composed: true, inputType: 'insertText', data: text }));
    } catch (_) {}
    try {
      input.dispatchEvent(new win.InputEvent('input', { bubbles: true, cancelable: true, composed: true, inputType: 'insertText', data: text }));
    } catch (_) {
      try { input.dispatchEvent(new win.Event('input', { bubbles: true, composed: true })); } catch (_) {}
    }
    try { input.dispatchEvent(new win.Event('change', { bubbles: true, composed: true })); } catch (_) {}
    callReactTextHandlers(input, text);
  };

  const dispatchEnterEverywhere = input => {
    const doc = input.ownerDocument || document;
    const win = doc.defaultView || window;
    const targets = [input];
    let node = input.parentElement;
    for (let i = 0; node && i < 8; i += 1, node = node.parentElement) targets.push(node);
    targets.push(doc, win);

    callReactEnterHandlers(input);
    for (const target of targets) {
      for (const type of ['keydown', 'keypress', 'keyup']) {
        try { target.dispatchEvent(domKeyEvent(win, type)); } catch (_) {}
      }
    }
  };

  const activateForSend = input => {
    let node = input;
    for (let i = 0; node && i < 6; i += 1, node = node.parentElement) {
      try {
        node.removeAttribute('data-pr-native-ui-killed');
        node.removeAttribute('data-pr-native-chat-parked');
        node.setAttribute('data-pr-chat-bridge-active', '1');
        node.style.setProperty('display', 'block', 'important');
        node.style.setProperty('visibility', 'visible', 'important');
        node.style.setProperty('opacity', node === input ? '0.02' : '0', 'important');
        node.style.setProperty('pointer-events', 'auto', 'important');
      } catch (_) {}
    }
    try {
      input.style.setProperty('position', 'fixed', 'important');
      input.style.setProperty('left', '12px', 'important');
      input.style.setProperty('bottom', '12px', 'important');
      input.style.setProperty('top', 'auto', 'important');
      input.style.setProperty('width', '340px', 'important');
      input.style.setProperty('height', '34px', 'important');
      input.style.setProperty('z-index', '1', 'important');
    } catch (_) {}
  };

  const clickSendAround = input => {
    let parent = input.parentElement;
    for (let depth = 0; parent && depth < 8; depth += 1, parent = parent.parentElement) {
      try {
        const buttons = [...parent.querySelectorAll('button, [role="button"], [type="submit"]')]
          .filter(btn => !isHudElement(btn) && !btn.disabled);
        const chosen = buttons.find(btn => /send|envoyer|enviar|arrow|submit|chat/i.test(hintFor(btn))) || buttons[buttons.length - 1];
        if (chosen) {
          chosen.click();
          return true;
        }
      } catch (_) {}
    }
    try {
      const form = input.closest('form');
      if (form) {
        form.requestSubmit ? form.requestSubmit() : form.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
        return true;
      }
    } catch (_) {}
    return false;
  };

  const sendUsingInput = async (input, clean) => {
    activateForSend(input);
    try { input.focus({ preventScroll: true }); } catch (_) { try { input.focus(); } catch (__) {} }
    await sleep(25);

    setValue(input, '');
    dispatchTextEvents(input, '');
    await sleep(25);

    setValue(input, clean);
    try { input.setSelectionRange?.(clean.length, clean.length); } catch (_) {}
    dispatchTextEvents(input, clean);
    await sleep(130);

    // Some Nitro builds keep the chat state in React and only send on the
    // focused input/parent key handlers. We therefore hit the full chain twice.
    dispatchEnterEverywhere(input);
    await sleep(110);
    dispatchEnterEverywhere(input);
    await sleep(60);
    clickSendAround(input);
    await sleep(120);

    try { input.blur?.(); } catch (_) {}
    return true;
  };

  const send = async text => {
    const clean = String(text || '').trim();
    if (!clean) return false;

    window.__paradiseChatBridgeActive = true;
    let ok = false;
    let error = null;
    const inputs = collectNativeInputs();

    for (const input of inputs) {
      try {
        ok = await sendUsingInput(input, clean);
        if (ok) break;
      } catch (e) {
        error = e;
      }
    }

    window.__paradiseChatBridgeActive = false;
    setTimeout(() => { try { window.__paradiseNativeUiOffScan?.(); } catch (_) {} }, 250);
    window.__paradiseChatBridgeLast = {
      ok,
      error: error ? String(error && (error.message || error)) : null,
      candidates: inputs.length,
      at: new Date().toISOString()
    };
    return ok;
  };

  const bind = () => {
    document.addEventListener('submit', event => {
      const form = event.target;
      if (!form || form.id !== HUD_FORM_ID) return;
      event.preventDefault();
      event.stopImmediatePropagation();
      const input = document.getElementById(HUD_INPUT_ID);
      const value = input?.value || '';
      if (!value.trim()) return;
      input?.classList?.add('is-sending');
      send(value).then(ok => {
        if (ok) input.value = '';
        else input?.classList?.add('is-error');
        input?.classList?.remove('is-sending');
        setTimeout(() => input?.classList?.remove('is-error'), 700);
        input?.focus?.();
      });
    }, true);

    document.addEventListener('keydown', event => {
      const input = event.target;
      if (!input || input.id !== HUD_INPUT_ID) return;
      if (event.key !== 'Enter' || event.shiftKey) return;
      event.preventDefault();
      event.stopImmediatePropagation();
      const form = document.getElementById(HUD_FORM_ID);
      if (form) form.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    }, true);
  };

  const debug = () => collectNativeInputs().map(el => {
    const r = rectOf(el) || {};
    return {
      tag: el.tagName,
      id: el.id || '',
      className: String(el.className || '').slice(0, 120),
      placeholder: el.getAttribute?.('placeholder') || '',
      parked: el.getAttribute?.('data-pr-native-chat-parked') || '',
      react: !!reactProps(el),
      fiber: !!reactFiber(el),
      value: el.value || el.textContent || '',
      rect: [Math.round(r.left || 0), Math.round(r.top || 0), Math.round(r.width || 0), Math.round(r.height || 0)],
      score: scoreTarget(el)
    };
  });

  window.__paradiseChatBridge = { version: VERSION, send, debug };
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', bind, { once: true });
  else bind();
})();