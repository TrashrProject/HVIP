(() => {
    'use strict';

    window.__PARADISE_BANK_POLICY__ = '4.0.0';

    const BRAND = 'ParadiseBank';
    const FORBIDDEN_TEXT = /^(?:retirer|retrait|withdraw|ouvrir\s*un\s*compte|open\s*account|open)$/i;
    const FORBIDDEN_ANY = /(?:retirer|retrait|withdraw|ouvrir\s*un\s*compte|open\s*account)/i;
    const text = el => String(el?.textContent || '').replace(/\s+/g, ' ').trim();

    function getBanks() {
        return [...document.querySelectorAll('.phone-bank-app')];
    }

    function ensureStyle() {
        if (document.getElementById('paradise-bank-v4-style')) return;
        const style = document.createElement('style');
        style.id = 'paradise-bank-v4-style';
        style.textContent = `
            .phone-bank-app [data-paradise-bank-logo="1"]{display:flex!important;align-items:center!important;justify-content:center!important;width:78px!important;height:62px!important;margin:2px auto 3px!important;color:#eef6ff!important;font:900 46px/1 Arial,sans-serif!important;letter-spacing:-5px!important;text-shadow:0 2px 0 rgba(0,0,0,.55)!important;user-select:none!important;pointer-events:none!important}
            .phone-bank-app [data-paradise-bank-deposit="1"]{width:100%!important;max-width:none!important;flex:1 1 100%!important}
            .phone-bank-app [data-paradise-bank-deposit-row="1"]{display:flex!important;grid-template-columns:1fr!important;width:100%!important;gap:0!important}
            .phone-bank-app [data-paradise-bank-forbidden="1"]{display:none!important;visibility:hidden!important;pointer-events:none!important}
        `;
        document.head.appendChild(style);
    }

    function brand(bank) {
        const walker = document.createTreeWalker(bank, NodeFilter.SHOW_TEXT);
        const nodes = [];
        while (walker.nextNode()) nodes.push(walker.currentNode);
        for (const node of nodes) {
            const before = node.nodeValue || '';
            let after = before.replace(/Metro\s*Bank/gi, BRAND);
            if (/^\s*M\${1,3}\s*$/i.test(before)) after = before.replace(/M\${1,3}/i, 'P$');
            if (after !== before) node.nodeValue = after;
        }

        const title = [...bank.querySelectorAll('h1,h2,h3,h4,h5,h6,strong,b,span,p,div')]
            .find(el => /^(?:Metro\s*Bank|ParadiseBank)$/i.test(text(el)));
        if (!title) return;
        title.textContent = BRAND;

        let logo = bank.querySelector('[data-paradise-bank-logo="1"]');
        if (!logo) {
            const oldLogo = title.previousElementSibling || title.parentElement?.querySelector('svg,img,[class*="logo" i]');
            if (oldLogo && !oldLogo.contains(title)) oldLogo.style.setProperty('display', 'none', 'important');
            logo = document.createElement('div');
            logo.dataset.paradiseBankLogo = '1';
            logo.textContent = 'P$';
            title.parentElement?.insertBefore(logo, title);
        } else {
            logo.textContent = 'P$';
        }
    }

    function killForbidden(bank) {
        const controls = [...bank.querySelectorAll('button,a,[role="button"],input[type="button"],input[type="submit"],[data-action],[data-bank-action],form')];
        for (const control of controls) {
            const descriptor = [text(control), control.id, control.className, control.getAttribute?.('name'), control.getAttribute?.('value'), control.getAttribute?.('data-action'), control.getAttribute?.('data-bank-action')].filter(Boolean).join(' ');
            if (!FORBIDDEN_ANY.test(descriptor) && !FORBIDDEN_TEXT.test(text(control))) continue;
            const target = control.closest('form,button,a,[role="button"]') || control;
            target.dataset.paradiseBankForbidden = '1';
            target.setAttribute('aria-hidden', 'true');
            if ('disabled' in target) target.disabled = true;
            target.style.setProperty('display', 'none', 'important');
            queueMicrotask(() => { if (target.isConnected) target.remove(); });
        }
    }

    function stretchDeposit(bank) {
        const deposit = [...bank.querySelectorAll('button,a,[role="button"],input[type="button"],input[type="submit"]')]
            .find(el => /^(?:déposer|deposer|deposit)$/i.test(text(el)));
        if (!deposit) return;
        deposit.dataset.paradiseBankDeposit = '1';
        if (deposit.parentElement) deposit.parentElement.dataset.paradiseBankDepositRow = '1';
    }

    function enforceBank(bank) {
        ensureStyle();
        brand(bank);
        killForbidden(bank);
        stretchDeposit(bank);
    }

    function enforceAll() {
        getBanks().forEach(enforceBank);
    }

    function blockForbidden(event) {
        const target = event.target instanceof Element ? event.target : null;
        if (!target) return;
        const bank = target.closest('.phone-bank-app');
        if (!bank) return;
        const control = target.closest('button,a,[role="button"],input[type="button"],input[type="submit"],form,[data-action],[data-bank-action]');
        if (!control) return;
        const descriptor = [text(control), control.id, control.className, control.getAttribute?.('data-action'), control.getAttribute?.('data-bank-action')].filter(Boolean).join(' ');
        if (!FORBIDDEN_ANY.test(descriptor) && !FORBIDDEN_TEXT.test(text(control))) return;
        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation?.();
        const remove = control.closest('form,button,a,[role="button"]') || control;
        remove.dataset.paradiseBankForbidden = '1';
        if ('disabled' in remove) remove.disabled = true;
        remove.style.setProperty('display', 'none', 'important');
        queueMicrotask(() => remove.remove());
    }

    ['pointerdown','mousedown','touchstart','click','submit'].forEach(type => window.addEventListener(type, blockForbidden, true));

    let raf = 0;
    const observer = new MutationObserver(() => {
        if (raf) return;
        raf = requestAnimationFrame(() => { raf = 0; enforceAll(); });
    });

    function boot() {
        enforceAll();
        observer.observe(document.body, { childList: true, subtree: true, characterData: true });
        setInterval(enforceAll, 150);
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
    else boot();
})();
