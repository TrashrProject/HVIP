(() => {
    'use strict';

    window.__PARADISE_BANK_POLICY__ = '5.0.0';

    const FORBIDDEN = /(?:retirer|retrait|withdraw|ouvrir\s*un\s*compte|open\s*account)/i;
    const text = el => String(el?.textContent || '').replace(/\s+/g, ' ').trim();

    function ensureStyle() {
        if (document.getElementById('paradise-bank-v5-style')) return;
        const style = document.createElement('style');
        style.id = 'paradise-bank-v5-style';
        style.textContent = `
            .nitro-phone-frame [data-paradise-bank-hidden="1"]{display:none!important;visibility:hidden!important;opacity:0!important;pointer-events:none!important;width:0!important;height:0!important;min-width:0!important;min-height:0!important;margin:0!important;padding:0!important;border:0!important;overflow:hidden!important}
            .nitro-phone-frame [data-paradise-bank-deposit="1"]{width:100%!important;max-width:none!important;flex:1 1 100%!important}
            .nitro-phone-frame [data-paradise-bank-deposit-row="1"]{display:flex!important;grid-template-columns:1fr!important;width:100%!important;gap:0!important}
        `;
        document.head.appendChild(style);
    }

    function isBankScreen(phone) {
        if (!phone) return false;
        const value = text(phone);
        return /ParadiseBank|Metro\s*Bank/i.test(value) && /solde\s+du\s+compte/i.test(value) && /argent\s+liquide/i.test(value);
    }

    function hideForbiddenControl(control) {
        if (!control || control.dataset?.paradiseBankHidden === '1') return;
        control.dataset.paradiseBankHidden = '1';
        control.setAttribute('aria-hidden', 'true');
        if ('disabled' in control) control.disabled = true;
        control.style.setProperty('display', 'none', 'important');
        control.style.setProperty('visibility', 'hidden', 'important');
        control.style.setProperty('pointer-events', 'none', 'important');
        setTimeout(() => {
            if (control.isConnected) control.remove();
        }, 0);
    }

    function cleanPhone(phone) {
        if (!phone || !isBankScreen(phone)) return;

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
            if (FORBIDDEN.test(descriptor)) hideForbiddenControl(control);
        }

        const deposit = candidates.find(control => /(?:^|\s)(?:déposer|deposer|deposit)(?:\s|$)/i.test(text(control)));
        if (deposit && deposit.isConnected) {
            deposit.dataset.paradiseBankDeposit = '1';
            if (deposit.parentElement) deposit.parentElement.dataset.paradiseBankDepositRow = '1';
        }
    }

    function scan() {
        ensureStyle();
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
        hideForbiddenControl(control.closest('button,a,[role="button"]') || control);
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
        setInterval(scan, 100);
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
    else boot();
})();
