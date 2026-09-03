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
})();
