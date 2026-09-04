(() => {
    'use strict';

    if (window.__PARADISE_BANK_POLICY__) return;
    window.__PARADISE_BANK_POLICY__ = '1.0.0';

    const BANK_SELECTOR = '.phone-bank-app';
    const FORBIDDEN_ACTION = /^(?:💰\s*)?(?:Open|Ouvrir un compte|Retirer|Withdraw)$/i;
    const FORBIDDEN_FORM = /(?:Ouvrir un compte|Open account|Retirer|Withdraw)/i;

    function installStyles() {
        if (document.getElementById('paradise-bank-policy-style')) return;
        const style = document.createElement('style');
        style.id = 'paradise-bank-policy-style';
        style.textContent = `
            ${BANK_SELECTOR} [data-paradise-bank-logo]{
                display:flex;align-items:center;justify-content:center;
                width:64px;height:58px;margin:0 auto 2px;
                color:#eef6ff;font:700 46px/1 Arial,sans-serif;
                letter-spacing:-5px;text-shadow:0 1px 0 #000,0 0 1px #fff;
                user-select:none;pointer-events:none;
            }
            ${BANK_SELECTOR} [data-paradise-bank-title]{font-weight:700!important;}
            ${BANK_SELECTOR} .bank-main-actions>div:empty{display:none!important;}
            ${BANK_SELECTOR} .bank-main-actions>div>button:only-child{width:100%!important;max-width:none!important;flex:1 1 100%!important;}
        `;
        document.head.appendChild(style);
    }

    function replaceBankText(bank) {
        const walker = document.createTreeWalker(bank, NodeFilter.SHOW_TEXT);
        const nodes = [];
        while (walker.nextNode()) nodes.push(walker.currentNode);
        for (const node of nodes) {
            const current = node.nodeValue || '';
            let next = current
                .replace(/Metro\s+Bank/gi, 'ParadiseBank')
                .replace(/MetroBank/gi, 'ParadiseBank');
            if (/^\s*M\${1,3}\s*$/.test(current)) next = current.replace(/M\${1,3}/, 'P$');
            if (next !== current) node.nodeValue = next;
        }
    }

    function leafWithText(root, pattern) {
        return [...root.querySelectorAll('*')].find(element => {
            if (!pattern.test(element.textContent.trim())) return false;
            return ![...element.children].some(child => pattern.test(child.textContent.trim()));
        }) || null;
    }

    function brandBank(bank) {
        replaceBankText(bank);

        const title = leafWithText(bank, /^ParadiseBank$/i);
        if (!title) return;
        title.dataset.paradiseBankTitle = '1';

        const brandArea = title.parentElement;
        if (!brandArea) return;

        let logo = brandArea.querySelector(':scope > [data-paradise-bank-logo]');
        if (!logo) {
            const existingTextLogo = [...brandArea.children].find(element => /^\s*(?:M\${1,3}|P\$)\s*$/.test(element.textContent || ''));
            if (existingTextLogo) {
                existingTextLogo.textContent = 'P$';
                existingTextLogo.dataset.paradiseBankLogo = '1';
                logo = existingTextLogo;
            } else {
                const visual = [...brandArea.children].find(element =>
                    element !== title && !element.matches('button') &&
                    (element.matches('svg,img') || element.querySelector?.('svg,img'))
                );
                if (visual) visual.style.setProperty('display', 'none', 'important');
                logo = document.createElement('div');
                logo.dataset.paradiseBankLogo = '1';
                logo.setAttribute('aria-label', 'ParadiseBank P dollar');
                logo.textContent = 'P$';
                brandArea.insertBefore(logo, title);
            }
        } else {
            logo.textContent = 'P$';
        }
    }

    function removeForbiddenActions(bank) {
        bank.querySelectorAll('button').forEach(button => {
            if (FORBIDDEN_ACTION.test(button.textContent.trim())) button.remove();
        });

        bank.querySelectorAll('.bank-action-form').forEach(form => {
            const title = form.querySelector('.title')?.textContent?.trim() || '';
            if (!FORBIDDEN_FORM.test(title)) return;
            const back = form.querySelector('.back-btn');
            if (back) back.click();
            else form.remove();
        });

        bank.querySelectorAll('.bank-main-actions > div').forEach(row => {
            if (!row.querySelector('button')) row.remove();
        });
    }

    function normalizeDeposit(bank) {
        const deposit = [...bank.querySelectorAll('button')].find(button => /^Déposer$/i.test(button.textContent.trim()));
        if (!deposit) return;
        const row = deposit.parentElement;
        if (!row) return;
        row.style.setProperty('display', 'flex', 'important');
        row.style.setProperty('width', '100%', 'important');
        deposit.style.setProperty('width', '100%', 'important');
        deposit.style.setProperty('flex', '1 1 100%', 'important');
    }

    function enforce(bank) {
        if (!(bank instanceof Element)) return;
        installStyles();
        brandBank(bank);
        removeForbiddenActions(bank);
        normalizeDeposit(bank);
    }

    function enforceAll(root = document) {
        if (root instanceof Element && root.matches(BANK_SELECTOR)) enforce(root);
        root.querySelectorAll?.(BANK_SELECTOR).forEach(enforce);
    }

    function stopForbiddenPhoneAction(event) {
        const button = event.target?.closest?.(`${BANK_SELECTOR} button`);
        if (!button || !FORBIDDEN_ACTION.test(button.textContent.trim())) return;
        event.preventDefault();
        event.stopImmediatePropagation();
        button.remove();
    }

    window.addEventListener('pointerdown', stopForbiddenPhoneAction, true);
    window.addEventListener('click', stopForbiddenPhoneAction, true);
    window.addEventListener('submit', event => {
        const form = event.target?.closest?.(`${BANK_SELECTOR} .bank-action-form`);
        if (!form) return;
        const title = form.querySelector('.title')?.textContent?.trim() || '';
        if (!FORBIDDEN_FORM.test(title)) return;
        event.preventDefault();
        event.stopImmediatePropagation();
        enforce(form.closest(BANK_SELECTOR));
    }, true);

    const observer = new MutationObserver(records => {
        for (const record of records) {
            if (record.target instanceof Element) {
                const bank = record.target.closest?.(BANK_SELECTOR);
                if (bank) enforce(bank);
            }
            record.addedNodes.forEach(node => {
                if (!(node instanceof Element)) return;
                if (node.matches(BANK_SELECTOR)) enforce(node);
                node.querySelectorAll?.(BANK_SELECTOR).forEach(enforce);
            });
        }
    });

    function boot() {
        enforceAll();
        observer.observe(document.getElementById('root') || document.body, { childList: true, subtree: true, characterData: true });
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
    else boot();
})();
