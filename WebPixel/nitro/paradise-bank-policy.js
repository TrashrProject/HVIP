(() => {
    'use strict';

    window.__PARADISE_BANK_POLICY__ = '6.0.0';

    const FORBIDDEN = /(?:retirer|retrait|withdraw|ouvrir\s*un\s*compte|open\s*account)/i;
    const text = el => String(el?.textContent || '').replace(/\s+/g, ' ').trim();

    function isBankScreen(phone) {
        if (!phone) return false;
        const value = text(phone);
        return /ParadiseBank|Metro\s*Bank/i.test(value) && /solde\s+du\s+compte/i.test(value) && /argent\s+liquide/i.test(value);
    }

    function removeForbiddenControl(control) {
        if (!control?.isConnected) return;
        if ('disabled' in control) control.disabled = true;
        control.remove();
    }

    function applyBranding(phone) {
        const walker = document.createTreeWalker(phone, NodeFilter.SHOW_TEXT);
        const nodes = [];
        while (walker.nextNode()) nodes.push(walker.currentNode);
        for (const node of nodes) {
            if (/^\s*Metro\s*Bank\s*$/i.test(node.nodeValue || '')) {
                node.nodeValue = (node.nodeValue || '').replace(/Metro\s*Bank/i, 'ParadiseBank');
            }
        }
    }

    function cleanPhone(phone) {
        if (!phone || !isBankScreen(phone)) return;
        applyBranding(phone);

        const candidates = [...phone.querySelectorAll('button,a,[role="button"],input[type="button"],input[type="submit"]')];
        for (const control of candidates) {
            const descriptor = [
                text(control),
                control.getAttribute?.('aria-label') || '',
                control.getAttribute?.('title') || '',
                control.getAttribute?.('data-action') || '',
                control.getAttribute?.('data-bank-action') || '',
                control.getAttribute?.('name') || '',
                control.getAttribute?.('value') || ''
            ].join(' ');
            if (FORBIDDEN.test(descriptor)) removeForbiddenControl(control);
        }
    }

    function scan() {
        document.querySelectorAll('.nitro-phone-frame').forEach(cleanPhone);
    }

    function block(event) {
        const target = event.target instanceof Element ? event.target : null;
        if (!target) return;
        const phone = target.closest('.nitro-phone-frame');
        if (!phone || !isBankScreen(phone)) return;
        const control = target.closest('button,a,[role="button"],input[type="button"],input[type="submit"],form,[data-action],[data-bank-action]');
        if (!control) return;
        const descriptor = [
            text(control),
            control.getAttribute?.('aria-label') || '',
            control.getAttribute?.('title') || '',
            control.getAttribute?.('data-action') || '',
            control.getAttribute?.('data-bank-action') || ''
        ].join(' ');
        if (!FORBIDDEN.test(descriptor)) return;
        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation?.();
        removeForbiddenControl(control.closest('button,a,[role="button"]') || control);
    }

    ['pointerdown','mousedown','touchstart','click','submit'].forEach(type => {
        window.addEventListener(type, block, true);
    });

    let queued = false;
    const observer = new MutationObserver(() => {
        if (queued) return;
        queued = true;
        requestAnimationFrame(() => {
            queued = false;
            scan();
        });
    });

    function boot() {
        scan();
        observer.observe(document.getElementById('root') || document.body, {
            childList: true,
            subtree: true,
            characterData: true
        });
        setInterval(scan, 750);
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
    else boot();
})();
