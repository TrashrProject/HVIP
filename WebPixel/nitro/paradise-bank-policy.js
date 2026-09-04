(() => {
    'use strict';

    if (window.__PARADISE_BANK_POLICY__) return;
    window.__PARADISE_BANK_POLICY__ = '2.0.0';

    const BRAND = 'ParadiseBank';
    const ROOT_MARKER = 'data-paradise-bank-root';
    const TITLE_PATTERN = /^(?:Metro\s*Bank|Paradise\s*Bank)$/i;
    const TRANSFER_PATTERN = /(?:faire\s+un\s+virement|virement|transfer)/i;
    const DEPOSIT_PATTERN = /(?:déposer|deposer|deposit)/i;
    const FORBIDDEN_PATTERN = /(?:withdraw|retrait|retirer|open[\s_-]*account|ouvrir[\s_-]*un[\s_-]*compte)/i;
    const ACTION_SELECTOR = 'button,[role="button"],a,input[type="button"],input[type="submit"],form,[data-action],[data-bank-action],[data-type]';

    let refreshScheduled = false;

    function normalize(value) {
        return String(value || '').replace(/\s+/g, ' ').trim();
    }

    function descriptor(element) {
        if (!(element instanceof Element)) return '';
        const className = typeof element.className === 'string' ? element.className : '';
        return [
            normalize(element.textContent),
            element.id,
            className,
            element.getAttribute('name'),
            element.getAttribute('value'),
            element.getAttribute('data-action'),
            element.getAttribute('data-bank-action'),
            element.getAttribute('data-type'),
            element.getAttribute('aria-label'),
            element.getAttribute('title')
        ].filter(Boolean).join(' ');
    }

    function hasAction(root, pattern) {
        if (!(root instanceof Element)) return false;
        return [...root.querySelectorAll(ACTION_SELECTOR)].some(element => pattern.test(descriptor(element)));
    }

    function titleCandidates(root = document) {
        const selector = 'h1,h2,h3,h4,h5,h6,strong,b,span,p,div,[class*="title"],[class*="brand"]';
        return [...root.querySelectorAll(selector)].filter(element => TITLE_PATTERN.test(normalize(element.textContent)));
    }

    function findBankRootFromTitle(title) {
        let current = title;
        for (let depth = 0; current && depth < 12; depth++, current = current.parentElement) {
            if (hasAction(current, TRANSFER_PATTERN) && hasAction(current, DEPOSIT_PATTERN)) return current;
        }
        return title.closest('.phone-bank-app') || null;
    }

    function findBankRoots() {
        const roots = new Set();
        document.querySelectorAll(`.phone-bank-app,[${ROOT_MARKER}="1"]`).forEach(root => roots.add(root));
        titleCandidates().forEach(title => {
            const root = findBankRootFromTitle(title);
            if (root) roots.add(root);
        });
        return [...roots];
    }

    function installStyles() {
        if (document.getElementById('paradise-bank-policy-style')) return;
        const style = document.createElement('style');
        style.id = 'paradise-bank-policy-style';
        style.textContent = `
            [${ROOT_MARKER}="1"] [data-paradise-bank-logo="1"]{
                display:flex!important;align-items:center!important;justify-content:center!important;
                width:70px!important;height:58px!important;margin:0 auto 4px!important;
                color:#eef6ff!important;font:800 44px/1 Arial,sans-serif!important;
                letter-spacing:-4px!important;text-shadow:0 1px 0 #000,0 0 1px #fff!important;
                user-select:none!important;pointer-events:none!important;
            }
            [${ROOT_MARKER}="1"] [data-paradise-bank-title="1"]{font-weight:800!important;}
            [${ROOT_MARKER}="1"] [data-paradise-deposit="1"]{
                width:100%!important;max-width:none!important;flex:1 1 100%!important;
            }
            [${ROOT_MARKER}="1"] [data-paradise-deposit-row="1"]{
                display:flex!important;width:100%!important;grid-template-columns:1fr!important;
            }
        `;
        document.head.appendChild(style);
    }

    function replaceTextNodes(bank) {
        const walker = document.createTreeWalker(bank, NodeFilter.SHOW_TEXT);
        const nodes = [];
        while (walker.nextNode()) nodes.push(walker.currentNode);
        nodes.forEach(node => {
            const current = node.nodeValue || '';
            let next = current.replace(/Metro\s*Bank/gi, BRAND);
            if (/^\s*M\${1,3}\s*$/i.test(current)) next = current.replace(/M\${1,3}/i, 'P$');
            if (next !== current) node.nodeValue = next;
        });
    }

    function leafWithExactText(root, pattern) {
        return [...root.querySelectorAll('*')].find(element => {
            const own = normalize(element.textContent);
            if (!pattern.test(own)) return false;
            return ![...element.children].some(child => pattern.test(normalize(child.textContent)));
        }) || null;
    }

    function findOldLogo(title) {
        const parents = [title.parentElement, title.parentElement?.parentElement].filter(Boolean);
        for (const parent of parents) {
            const children = [...parent.children];
            const titleIndex = children.indexOf(title);
            const before = titleIndex > 0 ? children[titleIndex - 1] : null;
            if (before && (before.matches('svg,img') || before.querySelector('svg,img') || /^\s*M\${1,3}\s*$/i.test(normalize(before.textContent)) || /logo|brand/i.test(String(before.className || '')))) {
                return before;
            }
        }
        return null;
    }

    function brandBank(bank) {
        replaceTextNodes(bank);
        const title = leafWithExactText(bank, /^ParadiseBank$/i) || leafWithExactText(bank, /^Paradise\s*Bank$/i);
        if (!title) return;

        title.textContent = BRAND;
        title.setAttribute('data-paradise-bank-title', '1');

        let logo = bank.querySelector('[data-paradise-bank-logo="1"]');
        if (!logo) {
            const oldLogo = findOldLogo(title);
            if (oldLogo) oldLogo.style.setProperty('display', 'none', 'important');
            logo = document.createElement('div');
            logo.setAttribute('data-paradise-bank-logo', '1');
            logo.setAttribute('aria-label', 'ParadiseBank');
            logo.textContent = 'P$';
            title.parentElement?.insertBefore(logo, title);
        } else {
            logo.textContent = 'P$';
        }
    }

    function isForbidden(element) {
        return FORBIDDEN_PATTERN.test(descriptor(element));
    }

    function removeForbiddenActions(bank) {
        [...bank.querySelectorAll(ACTION_SELECTOR)].forEach(element => {
            if (!isForbidden(element)) return;
            const removable = element.closest('form,button,[role="button"],a') || element;
            removable.remove();
        });

        [...bank.querySelectorAll('*')].forEach(element => {
            if (element.children.length) return;
            if (!FORBIDDEN_PATTERN.test(normalize(element.textContent))) return;
            const control = element.closest('form,button,[role="button"],a,[data-action],[data-bank-action]');
            if (control && bank.contains(control)) control.remove();
        });
    }

    function normalizeDeposit(bank) {
        const deposit = [...bank.querySelectorAll(ACTION_SELECTOR)].find(element => DEPOSIT_PATTERN.test(descriptor(element)) && !FORBIDDEN_PATTERN.test(descriptor(element)));
        if (!deposit) return;
        deposit.setAttribute('data-paradise-deposit', '1');
        const row = deposit.parentElement;
        if (row) row.setAttribute('data-paradise-deposit-row', '1');
    }

    function cleanEmptyRows(bank) {
        [...bank.querySelectorAll('.bank-main-actions > div')].forEach(row => {
            if (!row.querySelector('button,[role="button"],a,input')) row.remove();
        });
    }

    function enforce(bank) {
        if (!(bank instanceof Element)) return;
        bank.setAttribute(ROOT_MARKER, '1');
        bank.classList.add('paradise-bank-app');
        installStyles();
        brandBank(bank);
        removeForbiddenActions(bank);
        normalizeDeposit(bank);
        cleanEmptyRows(bank);
    }

    function refresh() {
        refreshScheduled = false;
        findBankRoots().forEach(enforce);
    }

    function scheduleRefresh() {
        if (refreshScheduled) return;
        refreshScheduled = true;
        requestAnimationFrame(refresh);
    }

    function closestBankRoot(element) {
        if (!(element instanceof Element)) return null;
        const marked = element.closest(`[${ROOT_MARKER}="1"],.phone-bank-app`);
        if (marked) return marked;
        const roots = findBankRoots();
        return roots.find(root => root.contains(element)) || null;
    }

    function stopForbiddenPhoneAction(event) {
        const target = event.target instanceof Element ? event.target : null;
        if (!target) return;
        const action = target.closest(ACTION_SELECTOR);
        if (!action || !isForbidden(action)) return;
        const bank = closestBankRoot(action);
        if (!bank) return;
        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation?.();
        (action.closest('form,button,[role="button"],a') || action).remove();
        enforce(bank);
    }

    ['pointerdown', 'mousedown', 'touchstart', 'click', 'submit'].forEach(type => {
        window.addEventListener(type, stopForbiddenPhoneAction, true);
    });

    const observer = new MutationObserver(scheduleRefresh);

    function boot() {
        refresh();
        observer.observe(document.getElementById('root') || document.body, {
            childList: true,
            subtree: true,
            characterData: true
        });
        window.setInterval(scheduleRefresh, 1200);
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
    else boot();
})();
