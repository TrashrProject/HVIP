(() => {
    'use strict';

    const CHAT_INPUT_SELECTOR = '.nitro-chat-input-container .chat-input';
    let injectingFloorCommand = false;

    function setReactInputValue(input, value) {
        const descriptor = Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, 'value');
        if (!descriptor || typeof descriptor.set !== 'function') return false;
        descriptor.set.call(input, value);
        input.dispatchEvent(new Event('input', { bubbles: true }));
        return true;
    }

    function openNativeFloorEditor() {
        const input = document.querySelector(CHAT_INPUT_SELECTOR);
        if (!(input instanceof HTMLInputElement)) return;

        injectingFloorCommand = true;
        input.focus();
        if (!setReactInputValue(input, ':floor')) {
            injectingFloorCommand = false;
            return;
        }

        requestAnimationFrame(() => {
            input.dispatchEvent(new KeyboardEvent('keydown', {
                key: 'Enter', code: 'Enter', keyCode: 13, which: 13,
                bubbles: true, cancelable: true
            }));
            window.setTimeout(() => { injectingFloorCommand = false; }, 0);
        });
    }

    document.addEventListener('keydown', event => {
        if (injectingFloorCommand || event.key !== 'Enter') return;
        const input = event.target;
        if (!(input instanceof HTMLInputElement) || !input.matches(CHAT_INPUT_SELECTOR)) return;
        const command = input.value.trim().split(/\s+/)[0].toLowerCase();
        if (command !== ':noitemfloor') return;
        window.setTimeout(openNativeFloorEditor, 250);
    }, true);

    const WANTED_KEYS = new Set([
        'wanted.title',
        'wantedTitle',
        'Wanted Title',
        'Wanted'
    ]);

    function replaceWantedTextNode(node) {
        if (!node || node.nodeType !== Node.TEXT_NODE) return;
        const raw = node.nodeValue || '';
        const value = raw.trim();
        if (!WANTED_KEYS.has(value)) return;
        node.nodeValue = raw.replace(value, 'Personnes recherchées');
    }

    function fixWantedTitle(root) {
        if (!root) return;
        if (root.nodeType === Node.TEXT_NODE) {
            replaceWantedTextNode(root);
            return;
        }
        const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
        let node;
        while ((node = walker.nextNode())) replaceWantedTextNode(node);
    }

    function startWantedTitleFix() {
        fixWantedTitle(document.body);
        const observer = new MutationObserver(records => {
            for (const record of records) {
                if (record.type === 'characterData') replaceWantedTextNode(record.target);
                for (const node of record.addedNodes) fixWantedTitle(node);
            }
        });
        observer.observe(document.body, {
            childList: true,
            subtree: true,
            characterData: true
        });

        // React peut réécrire le titre après le rendu du paquet wanted.
        // Quelques passages courts garantissent la correction sans modifier Nitro.
        [100, 300, 700, 1500].forEach(delay => {
            window.setTimeout(() => fixWantedTitle(document.body), delay);
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', startWantedTitleFix, { once: true });
    } else {
        startWantedTitleFix();
    }
})();
