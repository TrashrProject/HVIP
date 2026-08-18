(() => {
  'use strict';

  const VERSION = '1.0.0';
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

  const nativeInputs = () => {
    const found = [];
    for (const doc of getDocs()) {
      try {
        doc.querySelectorAll('input[type="text"], input:not([type]), textarea, [contenteditable="true"]').forEach(el => {
          if (!el || isHudElement(el)) return;
          if (el.disabled || el.readOnly) return;
          const hint = `${el.id || ''} ${el.className || ''} ${el.getAttribute?.('placeholder') || ''} ${el.getAttribute?.('aria-label') || ''} ${el.getAttribute?.('data-pr-native-chat-parked') || ''}`;
          const rect = (() => { try { return el.getBoundingClientRect(); } catch (_) { return null; } })();
          let score = 0;
          if (/chat|chatear|haz|message|parler|say|input/i.test(hint)) score += 1000;
          if (el.getAttribute?.('data-pr-native-chat-parked') === '1') score += 900;
          if (rect) {
            if (rect.bottom > 0) score += Math.round(rect.bottom / 2);
            if (rect.width > 20 && rect.height > 10) score += 100;
          }
          found.push({ el, score });
        });
      } catch (_) {}
    }
    return found.sort((a, b) => b.score - a.score).map(x => x.el);
  };

  const reactProps = el => {
    try {
      const key = Object.keys(el).find(k => k.startsWith('__reactProps$'));
      return key ? el[key] : null;
    } catch (_) {
      return null;
    }
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

  const eventBase = (el, type, extra = {}) => ({
    type,
    target: el,
    currentTarget: el,
    bubbles: true,
    cancelable: true,
    defaultPrevented: false,
    isDefaultPrevented: () => false,
    isPropagationStopped: () => false,
    preventDefault() { this.defaultPrevented = true; },
    stopPropagation() {},
    persist() {},
    nativeEvent: extra,
    ...extra
  });

  const fireReact = (el, text) => {
    const props = reactProps(el);
    if (!props) return;
    try { props.onBeforeInput?.(eventBase(el, 'beforeinput', { data: text, inputType: 'insertText' })); } catch (_) {}
    try { props.onInput?.(eventBase(el, 'input', { data: text, inputType: 'insertText' })); } catch (_) {}
    try { props.onChange?.(eventBase(el, 'change', { data: text, inputType: 'insertText' })); } catch (_) {}
  };

  const dispatchInputEvents = (el, text) => {
    const doc = el.ownerDocument || document;
    const win = doc.defaultView || window;
    try {
      el.dispatchEvent(new win.InputEvent('beforeinput', { bubbles: true, cancelable: true, composed: true, inputType: 'insertText', data: text }));
    } catch (_) {}
    try {
      el.dispatchEvent(new win.InputEvent('input', { bubbles: true, cancelable: true, composed: true, inputType: 'insertText', data: text }));
    } catch (_) {
      try { el.dispatchEvent(new win.Event('input', { bubbles: true, composed: true })); } catch (_) {}
    }
    try { el.dispatchEvent(new win.Event('change', { bubbles: true, composed: true })); } catch (_) {}
    fireReact(el, text);
  };

  const fireEnter = el => {
    const doc = el.ownerDocument || document;
    const win = doc.defaultView || window;
    const props = reactProps(el);
    const opts = { key: 'Enter', code: 'Enter', keyCode: 13, which: 13, charCode: 13, bubbles: true, cancelable: true, composed: true, shiftKey: false, ctrlKey: false, altKey: false, metaKey: false, isComposing: false };
    const fake = type => eventBase(el, type, opts);

    try { props?.onKeyDown?.(fake('keydown')); } catch (_) {}
    try { props?.onKeyPress?.(fake('keypress')); } catch (_) {}
    try { props?.onKeyUp?.(fake('keyup')); } catch (_) {}

    for (const target of [el, doc, win]) {
      try { target.dispatchEvent(new win.KeyboardEvent('keydown', opts)); } catch (_) {}
      try { target.dispatchEvent(new win.KeyboardEvent('keypress', opts)); } catch (_) {}
      try { target.dispatchEvent(new win.KeyboardEvent('keyup', opts)); } catch (_) {}
    }
  };

  const temporarilyActivate = el => {
    let node = el;
    for (let i = 0; node && i < 4; i += 1, node = node.parentElement) {
      try {
        node.removeAttribute('data-pr-native-ui-killed');
        node.setAttribute('data-pr-chat-bridge-active', '1');
        node.style.setProperty('display', 'block', 'important');
        node.style.setProperty('visibility', 'visible', 'important');
        node.style.setProperty('opacity', node === el ? '0.01' : '0', 'important');
        node.style.setProperty('pointer-events', 'auto', 'important');
        node.style.setProperty('position', node === el ? 'fixed' : (node.style.position || 'fixed'), 'important');
        if (node === el) {
          node.style.setProperty('left', '8px', 'important');
          node.style.setProperty('bottom', '8px', 'important');
          node.style.setProperty('top', 'auto', 'important');
          node.style.setProperty('width', '280px', 'important');
          node.style.setProperty('height', '32px', 'important');
          node.style.setProperty('z-index', '-1', 'important');
        }
      } catch (_) {}
    }
  };

  const clickPossibleSendButton = el => {
    try {
      let parent = el.parentElement;
      for (let depth = 0; parent && depth < 5; depth += 1, parent = parent.parentElement) {
        const buttons = [...parent.querySelectorAll('button, [role="button"]')].filter(btn => !isHudElement(btn) && !btn.disabled);
        const chosen = buttons.find(btn => /send|envoyer|enviar|chat|arrow|submit/i.test(`${btn.className || ''} ${btn.id || ''} ${btn.getAttribute('aria-label') || ''} ${btn.title || ''} ${btn.textContent || ''}`)) || buttons[buttons.length - 1];
        if (chosen) {
          chosen.click();
          return true;
        }
      }
    } catch (_) {}
    return false;
  };

  const send = async text => {
    const clean = String(text || '').trim();
    if (!clean) return false;

    window.__paradiseChatBridgeActive = true;
    const inputs = nativeInputs();
    for (const input of inputs) {
      try {
        temporarilyActivate(input);
        input.focus?.({ preventScroll: true });
        setValue(input, clean);
        dispatchInputEvents(input, clean);
        try { input.setSelectionRange?.(clean.length, clean.length); } catch (_) {}

        await sleep(80);
        fireEnter(input);
        await sleep(90);
        fireEnter(input);
        clickPossibleSendButton(input);

        await sleep(160);
        try { input.blur?.(); } catch (_) {}
        window.__paradiseChatBridgeActive = false;
        try { window.__paradiseNativeUiOffScan?.(); } catch (_) {}
        return true;
      } catch (_) {}
    }
    window.__paradiseChatBridgeActive = false;
    return false;
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

  window.__paradiseChatBridge = { version: VERSION, send };
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', bind, { once: true });
  else bind();
})();
