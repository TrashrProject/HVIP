(() => {
  'use strict';

  const VERSION = '83.0.0-chat-line-cleanup';
  const HUD_ID = 'paradise-rp-hud';

  const isNativeChatField = element => {
    if (!element || (element.tagName !== 'INPUT' && element.tagName !== 'TEXTAREA')) return false;
    if (element.id === 'prhud-chat-input' || element.closest?.(`#${HUD_ID}`)) return false;
    const text = `${element.getAttribute('placeholder') || ''} ${element.className || ''} ${element.id || ''} ${element.getAttribute('aria-label') || ''}`;
    return /chat|chatter|chatear|haz|parler|message|say/i.test(text);
  };

  const findChat = () => {
    try {
      return [...document.querySelectorAll('#root [data-pr-native-chat-live="1"], #root input, #root textarea')]
        .find(element => isNativeChatField(element) && !element.disabled && !element.readOnly) || null;
    } catch (_) {
      return null;
    }
  };

  const hideChrome = element => {
    if (!element || element === document.body || element === document.documentElement) return;
    if (element.id === 'root' || element.id === HUD_ID) return;
    if (element.tagName === 'CANVAS' || element.tagName === 'IFRAME') return;
    try {
      element.setAttribute('data-pr-native-chat-chrome', '1');
      element.style.setProperty('display', 'none', 'important');
      element.style.setProperty('visibility', 'hidden', 'important');
      element.style.setProperty('opacity', '0', 'important');
      element.style.setProperty('pointer-events', 'none', 'important');
      element.style.setProperty('background', 'transparent', 'important');
      element.style.setProperty('border', '0', 'important');
      element.style.setProperty('box-shadow', 'none', 'important');
    } catch (_) {}
  };

  const neutralizeHost = (element, input) => {
    if (!element || element === document.body || element === document.documentElement) return;
    if (element.id === 'root' || element.id === HUD_ID) return;
    if (element.tagName === 'CANVAS' || element.tagName === 'IFRAME') return;

    try {
      element.setAttribute('data-pr-native-chat-host', '1');
      element.removeAttribute('data-pr-native-ui-killed');
      element.style.setProperty('display', 'block', 'important');
      element.style.setProperty('visibility', 'visible', 'important');
      element.style.setProperty('opacity', '1', 'important');
      element.style.setProperty('pointer-events', 'none', 'important');
      element.style.setProperty('background', 'transparent', 'important');
      element.style.setProperty('background-color', 'transparent', 'important');
      element.style.setProperty('border', '0', 'important');
      element.style.setProperty('outline', '0', 'important');
      element.style.setProperty('box-shadow', 'none', 'important');
      element.style.setProperty('filter', 'none', 'important');
      element.style.setProperty('backdrop-filter', 'none', 'important');
      element.style.setProperty('overflow', 'visible', 'important');

      [...element.children].forEach(child => {
        if (child === input || child.contains(input)) return;
        if (child.closest?.(`#${HUD_ID}`)) return;
        hideChrome(child);
      });
    } catch (_) {}
  };

  const cleanupChat = () => {
    const input = findChat();
    if (!input) return false;

    try {
      input.setAttribute('data-pr-native-chat-live', '1');
      input.setAttribute('data-pr-paradise-chat-input', '1');
      input.removeAttribute('data-pr-native-ui-killed');
      if (/haz|chatear/i.test(input.getAttribute('placeholder') || '')) {
        input.setAttribute('placeholder', 'Écrire un message...');
      }
      if (!input.getAttribute('placeholder')) input.setAttribute('placeholder', 'Écrire un message...');
      input.style.setProperty('display', 'block', 'important');
      input.style.setProperty('visibility', 'visible', 'important');
      input.style.setProperty('opacity', '1', 'important');
      input.style.setProperty('pointer-events', 'auto', 'important');
    } catch (_) {}

    let parent = input.parentElement;
    for (let depth = 0; parent && depth < 7; depth += 1, parent = parent.parentElement) {
      neutralizeHost(parent, input);
    }

    try {
      document.querySelectorAll('#root [data-pr-native-chat-host="1"]').forEach(host => {
        if (host === input || host.contains(input)) return;
        const r = host.getBoundingClientRect?.();
        if (r && r.width > window.innerWidth * 0.45 && r.height <= 12) {
          host.setAttribute('data-pr-native-chat-line', '1');
          hideChrome(host);
        }
      });
    } catch (_) {}

    return true;
  };

  const bindShellFocus = () => {
    try {
      const hud = document.getElementById(HUD_ID);
      const shell = hud?.querySelector?.('.pr4-chat-module, .pr4-polish-chat');
      if (!shell || shell.dataset.prChatFixBound === '1') return;
      shell.dataset.prChatFixBound = '1';
      shell.addEventListener('click', event => {
        event.preventDefault();
        const input = findChat();
        input?.focus?.({ preventScroll: true });
      }, true);
    } catch (_) {}
  };

  const scan = () => {
    cleanupChat();
    bindShellFocus();
  };

  const boot = () => {
    scan();
    window.setInterval(scan, 500);
    window.addEventListener('resize', scan, { passive: true });
    const observer = new MutationObserver(() => window.requestAnimationFrame(scan));
    observer.observe(document.body, { childList: true, subtree: true, attributes: true, attributeFilter: ['style', 'class', 'data-pr-native-chat-live', 'data-pr-native-chat-host'] });
    window.__ParadiseRPChatFix = { version: VERSION, scan, focus: () => findChat()?.focus?.({ preventScroll: true }) };
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
