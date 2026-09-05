(() => {
    'use strict';

    const CHAT_INPUT_SELECTOR = '.nitro-chat-input-container .chat-input';
    let injectingFloorCommand = false;

    function setReactInputValue(input, value) {
        const descriptor = Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, 'value');

        if (!descriptor || typeof descriptor.set !== 'function') {
            return false;
        }

        descriptor.set.call(input, value);
        input.dispatchEvent(new Event('input', { bubbles: true }));
        return true;
    }

    function openNativeFloorEditor() {
        const input = document.querySelector(CHAT_INPUT_SELECTOR);

        if (!(input instanceof HTMLInputElement)) {
            return;
        }

        injectingFloorCommand = true;
        input.focus();

        // Nitro already implements :floor as a local command that triggers
        // CreateLinkEvent('floor-editor/show'). Reuse that native path instead of
        // duplicating or reverse-engineering the floor editor UI.
        if (!setReactInputValue(input, ':floor')) {
            injectingFloorCommand = false;
            return;
        }

        requestAnimationFrame(() => {
            input.dispatchEvent(new KeyboardEvent('keydown', {
                key: 'Enter',
                code: 'Enter',
                keyCode: 13,
                which: 13,
                bubbles: true,
                cancelable: true
            }));

            window.setTimeout(() => {
                injectingFloorCommand = false;
            }, 0);
        });
    }

    document.addEventListener('keydown', event => {
        if (injectingFloorCommand || event.key !== 'Enter') {
            return;
        }

        const input = event.target;
        if (!(input instanceof HTMLInputElement) || !input.matches(CHAT_INPUT_SELECTOR)) {
            return;
        }

        const command = input.value.trim().split(/\s+/)[0].toLowerCase();
        if (command !== ':noitemfloor') {
            return;
        }

        // Do not cancel the original Enter: Nitro must send :noitemfloor to WavePlus
        // first so server-side occupied-tile protection is disabled. Then execute the
        // built-in local :floor command to open the native editor automatically.
        window.setTimeout(openNativeFloorEditor, 250);
    }, true);

    // ParadiseRP: le client affiche parfois la clé brute "wantedTitle" au lieu
    // d'un vrai titre. Corrige-la directement dans l'UI active, y compris pour
    // les fenêtres créées après le chargement initial.
    function fixWantedTitle(root) {
        if (!root) return;
        const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
        let node;
        while ((node = walker.nextNode())) {
            const value = (node.nodeValue || '').trim();
            if (value === 'wantedTitle' || value === 'Wanted Title' || value === 'Wanted') {
                node.nodeValue = node.nodeValue.replace(value, 'Personnes recherchées');
            }
        }
    }

    const startWantedTitleFix = () => {
        fixWantedTitle(document.body);
        const observer = new MutationObserver(records => {
            for (const record of records) {
                for (const node of record.addedNodes) {
                    if (node.nodeType === Node.TEXT_NODE) {
                        const value = (node.nodeValue || '').trim();
                        if (value === 'wantedTitle' || value === 'Wanted Title' || value === 'Wanted') {
                            node.nodeValue = node.nodeValue.replace(value, 'Personnes recherchées');
                        }
                    } else if (node.nodeType === Node.ELEMENT_NODE) {
                        fixWantedTitle(node);
                    }
                }
            }
        });
        observer.observe(document.body, { childList: true, subtree: true });
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', startWantedTitleFix, { once: true });
    } else {
        startWantedTitleFix();
    }
})();
