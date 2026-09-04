(() => {
    'use strict';

    window.__PARADISE_BANK_POLICY__ = '3.0.0';

    const BRAND = 'ParadiseBank';
    const FORBIDDEN = /(?:retirer|retrait|withdraw|ouvrir\s*un\s*compte|open\s*account)/i;
    const DEPOSIT = /(?:déposer|deposer|deposit)/i;
    const TRANSFER = /(?:faire\s*un\s*virement|virement|transfer)/i;
    const text = el => String(el?.textContent || '').replace(/\s+/g, ' ').trim();

    function isBankRoot(root) {
        if (!(root instanceof Element)) return false;
        const value = text(root);
        return /Metro\s*Bank|ParadiseBank/i.test(value) && TRANSFER.test(value) && DEPOSIT.test(value);
    }

    function findBankRoots() {
        const roots = new Set();
        document.querySelectorAll('.phone-bank-app,[class*="bank"],[data-app*="bank" i]').forEach(el => {
            if (isBankRoot(el)) roots.add(el);
        });
        document.querySelectorAll('h1,h2,h3,h4,h5,h6,strong,b,span,p,div').forEach(el => {
            if (!/^(?:Metro\s*Bank|ParadiseBank)$/i.test(text(el))) return;
            let parent = el;
            for (let i = 0; parent && i < 14; i++, parent = parent.parentElement) {
                if (isBankRoot(parent)) { roots.add(parent); break; }
            }
        });
        return [...roots];
    }

    function installStyle() {
        if (document.getElementById('paradise-bank-v3-style')) return;
        const style = document.createElement('style');
        style.id = 'paradise-bank-v3-style';
        style.textContent = `
            [data-paradise-bank="1"] [data-paradise-bank-logo="1"]{display:flex!important;align-items:center!important;justify-content:center!important;width:78px!important;height:62px!important;margin:2px auto 3px!important;color:#eef6ff!important;font:900 46px/1 Arial,sans-serif!important;letter-spacing:-5px!important;text-shadow:0 2px 0 rgba(0,0,0,.55)!important;user-select:none!important;pointer-events:none!important}
            [data-paradise-bank="1"] [data-paradise-bank-deposit="1"]{width:100%!important;max-width:none!important;flex:1 1 100%!important}
            [data-paradise-bank="1"] [data-paradise-bank-deposit-row="1"]{display:flex!important;grid-template-columns:1fr!important;width:100%!important;gap:0!important}
        `;
        document.head.appendChild(style);
    }

    function replaceBrand(root) {
        const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
        const nodes = [];
        while (walker.nextNode()) nodes.push(walker.currentNode);
        nodes.forEach(node => {
            const before = node.nodeValue || '';
            const after = before.replace(/Metro\s*Bank/gi, BRAND).replace(/^\s*M\${1,3}\s*$/i, 'P$');
            if (after !== before) node.nodeValue = after;
        });
        const title = [...root.querySelectorAll('h1,h2,h3,h4,h5,h6,strong,b,span,p,div')].find(el => /^(?:Metro\s*Bank|ParadiseBank)$/i.test(text(el)));
        if (!title) return;
        title.textContent = BRAND;
        let logo = root.querySelector('[data-paradise-bank-logo="1"]');
        if (!logo) {
            let candidate = title.previousElementSibling;
            if (!candidate || candidate.matches('button,input')) candidate = title.parentElement?.querySelector('svg,img,[class*="logo" i]');
            if (candidate && !candidate.contains(title)) candidate.style.setProperty('display', 'none', 'important');
            logo = document.createElement('div');
            logo.dataset.paradiseBankLogo = '1';
            logo.textContent = 'P$';
            title.parentElement?.insertBefore(logo, title);
        }
    }

    function removeForbidden(root) {
        const selectors = 'button,a,[role="button"],input[type="button"],input[type="submit"],[data-action],[data-bank-action]';
        [...root.querySelectorAll(selectors)].forEach(el => {
            const descriptor = [text(el), el.id, el.className, el.getAttribute('name'), el.getAttribute('value'), el.getAttribute('data-action'), el.getAttribute('data-bank-action')].filter(Boolean).join(' ');
            if (FORBIDDEN.test(descriptor)) (el.closest('form') || el).remove();
        });
    }

    function stretchDeposit(root) {
        const deposit = [...root.querySelectorAll('button,a,[role="button"],input[type="button"],input[type="submit"],[data-action],[data-bank-action]')].find(el => DEPOSIT.test(text(el)) && !FORBIDDEN.test(text(el)));
        if (!deposit) return;
        deposit.dataset.paradiseBankDeposit = '1';
        if (deposit.parentElement) deposit.parentElement.dataset.paradiseBankDepositRow = '1';
    }

    function enforce(root) {
        root.dataset.paradiseBank = '1';
        installStyle();
        replaceBrand(root);
        removeForbidden(root);
        stretchDeposit(root);
    }

    function refresh() { findBankRoots().forEach(enforce); }

    function blockForbidden(event) {
        const target = event.target instanceof Element ? event.target : null;
        if (!target) return;
        const control = target.closest('button,a,[role="button"],input[type="button"],input[type="submit"],form,[data-action],[data-bank-action]');
        if (!control || !FORBIDDEN.test([text(control), control.id, control.className, control.getAttribute('data-action'), control.getAttribute('data-bank-action')].filter(Boolean).join(' '))) return;
        let root = control.closest('[data-paradise-bank="1"],.phone-bank-app');
        if (!root) root = findBankRoots().find(candidate => candidate.contains(control));
        if (!root) return;
        event.preventDefault(); event.stopPropagation(); event.stopImmediatePropagation?.();
        (control.closest('form') || control).remove();
        enforce(root);
    }

    ['pointerdown','mousedown','touchstart','click','submit'].forEach(type => window.addEventListener(type, blockForbidden, true));
    const observer = new MutationObserver(() => requestAnimationFrame(refresh));
    function boot() {
        refresh();
        observer.observe(document.body, { childList: true, subtree: true, characterData: true });
        setInterval(refresh, 700);
    }
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true }); else boot();
})();
