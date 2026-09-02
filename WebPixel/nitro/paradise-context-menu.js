(function () {
    'use strict';

    if (window.__paradisePlayerContextMenu) return;

    var MENU_SELECTOR = '.nitro-context-menu.paradise-player-context-menu';
    var scheduled = false;
    var ROOT_ITEMS = [
        { match: ['changer de nom', 'change name'], kind: 'identity', label: 'Changer de nom', hint: 'Modifier votre identité' },
        { match: ['mes vetements', 'tenues', 'my clothes'], kind: 'outfits', label: 'Tenues', hint: 'Modifier votre apparence' },
        { match: ['danser', 'danses', 'dance'], kind: 'dance', label: 'Danses', hint: 'Choisir un mouvement' },
        { match: ['actions', 'expressions'], kind: 'actions', label: 'Actions', hint: 'Postures et expressions' },
        { match: ['panneaux', 'signs'], kind: 'signs', label: 'Panneaux', hint: 'Afficher un panneau' },
        { match: ['poser l objet', 'drop hand item', 'lacher'], kind: 'drop', label: null, hint: 'Déposer l’objet tenu' }
    ];

    function normalize(value) {
        return (value || '').normalize('NFD').replace(/[\u0300-\u036f]/g, '')
            .replace(/[^a-z0-9]+/g, ' ').replace(/\s+/g, ' ').trim().toLowerCase();
    }

    function directText(row) {
        return Array.prototype.reduce.call(row.childNodes, function (text, node) {
            return node.nodeType === Node.TEXT_NODE ? text + ' ' + node.nodeValue : text;
        }, '').trim() || row.textContent.trim();
    }

    function matchesAny(text, matches) {
        return matches.some(function (match) { return text.indexOf(match) !== -1; });
    }

    function replaceDirectText(row, replacement) {
        if (!replacement) return;
        var textNode = Array.prototype.find.call(row.childNodes, function (node) {
            return node.nodeType === Node.TEXT_NODE && node.nodeValue.trim();
        });
        if (!textNode) {
            var walker = document.createTreeWalker(row, NodeFilter.SHOW_TEXT);
            var candidate;
            while ((candidate = walker.nextNode())) {
                var parent = candidate.parentElement;
                if (!candidate.nodeValue.trim() || (parent && parent.closest('.fa-icon, .paradise-menu-icon'))) continue;
                textNode = candidate;
                break;
            }
        }
        if (textNode && textNode.nodeValue.trim() !== replacement) textNode.nodeValue = ' ' + replacement + ' ';
    }

    function classifyView(menu, rows) {
        if (menu.querySelectorAll('.menu-list-split-3').length >= 3) return 'signs';
        if (!rows.some(function (row) { return row.querySelector('.fa-icon.left'); })) return 'root';
        var labels = rows.map(function (row) { return normalize(directText(row)); }).join(' | ');
        return /danse|dance/.test(labels) ? 'dance' : 'actions';
    }

    function decorateRootRow(row) {
        var text = normalize(directText(row));
        var item = ROOT_ITEMS.find(function (candidate) { return matchesAny(text, candidate.match); });
        if (!item) return;
        row.dataset.paradiseKind = item.kind;
        row.dataset.paradiseHint = item.hint;
        replaceDirectText(row, item.label);
    }

    function decorateSubmenuRow(row, view) {
        var text = normalize(directText(row));
        if (row.querySelector('.fa-icon.left')) {
            row.classList.add('paradise-back-item');
            replaceDirectText(row, 'Retour au menu');
            return;
        }
        if (view === 'dance') {
            row.dataset.paradiseKind = text.indexOf('arreter') !== -1 || text.indexOf('stop') !== -1 ? 'stop' : 'dance-choice';
            return;
        }
        if (view === 'actions') {
            row.dataset.paradiseKind = 'action-choice';
        }
    }

    function makeKeyboardClickable(element) {
        if (!element || element.dataset.paradiseKeyboard === 'true') return;
        element.dataset.paradiseKeyboard = 'true';
        element.tabIndex = element.classList.contains('disabled') ? -1 : 0;
    }

    function enhanceMenu(menu) {
        var rows = Array.prototype.slice.call(menu.querySelectorAll('.menu-item.list-item'));
        if (!rows.length) return;
        var view = classifyView(menu, rows);
        menu.dataset.paradiseView = view;
        menu.setAttribute('role', 'menu');
        menu.setAttribute('aria-label', view === 'root' ? 'Actions de mon avatar' : 'Sous-menu des actions de mon avatar');
        rows.forEach(function (row) {
            row.setAttribute('role', 'menuitem');
            makeKeyboardClickable(row);
            row.classList.toggle('paradise-back-item', Boolean(row.querySelector('.fa-icon.left')));
            if (view === 'root') decorateRootRow(row);
            else if (view !== 'signs') decorateSubmenuRow(row, view);
            if (row.querySelector('.fa-icon.right')) {
                row.setAttribute('aria-haspopup', 'menu');
                row.setAttribute('aria-label', directText(row) + ', ouvrir le sous-menu');
            }
        });
        var header = menu.querySelector('.menu-header');
        var footer = menu.querySelector('.menu-footer');
        if (header && header.classList.contains('cursor-pointer')) {
            header.setAttribute('role', 'button');
            header.setAttribute('title', 'Ouvrir mon profil');
            makeKeyboardClickable(header);
        }
        if (footer) {
            footer.setAttribute('role', 'button');
            footer.setAttribute('aria-label', menu.classList.contains('menu-hidden') ? 'Développer le menu' : 'Réduire le menu');
            makeKeyboardClickable(footer);
        }
    }

    function enhanceAll() {
        scheduled = false;
        Array.prototype.forEach.call(document.querySelectorAll(MENU_SELECTOR), enhanceMenu);
    }

    function scheduleEnhancement() {
        if (scheduled) return;
        scheduled = true;
        window.requestAnimationFrame(enhanceAll);
    }

    function mutationTouchesMenu(mutation) {
        var target = mutation.target.nodeType === Node.ELEMENT_NODE ? mutation.target : mutation.target.parentElement;
        if (target && (target.matches(MENU_SELECTOR) || target.closest(MENU_SELECTOR))) return true;
        return Array.prototype.some.call(mutation.addedNodes || [], function (node) {
            return node.nodeType === Node.ELEMENT_NODE && (node.matches(MENU_SELECTOR) || node.querySelector(MENU_SELECTOR));
        });
    }

    function onPointerDown(event) {
        var row = event.target.closest(MENU_SELECTOR + ' .menu-item.list-item');
        if (!row || !row.querySelector('.fa-icon.right')) return;
        row.classList.add('paradise-item-opening');
        window.setTimeout(function () { row.classList.remove('paradise-item-opening'); }, 180);
    }

    function enabledRows(menu) {
        return Array.prototype.filter.call(menu.querySelectorAll('[data-paradise-keyboard="true"]'), function (item) {
            return !item.classList.contains('disabled') && item.offsetParent !== null;
        });
    }

    function onKeyDown(event) {
        var target = event.target.closest('[data-paradise-keyboard="true"]');
        var menu = target && target.closest(MENU_SELECTOR);
        if (!menu) return;
        if (event.key === 'Enter' || event.key === ' ') {
            event.preventDefault();
            target.click();
            return;
        }
        if (event.key === 'Escape') {
            var back = menu.querySelector('.paradise-back-item');
            if (back) {
                event.preventDefault();
                back.click();
            }
            return;
        }
        if (!['ArrowUp', 'ArrowDown', 'Home', 'End'].includes(event.key)) return;
        var items = enabledRows(menu);
        if (!items.length) return;
        event.preventDefault();
        var index = items.indexOf(target);
        if (event.key === 'Home') index = 0;
        else if (event.key === 'End') index = items.length - 1;
        else if (event.key === 'ArrowDown') index = (index + 1 + items.length) % items.length;
        else index = (index - 1 + items.length) % items.length;
        items[index].focus();
    }

    var observer = new MutationObserver(function (mutations) {
        if (mutations.some(mutationTouchesMenu)) scheduleEnhancement();
    });
    observer.observe(document.documentElement, { attributes: true, attributeFilter: ['class'], characterData: true, childList: true, subtree: true });
    document.addEventListener('pointerdown', onPointerDown, true);
    document.addEventListener('keydown', onKeyDown, true);
    window.__paradisePlayerContextMenu = { observer: observer, version: '2.0.0' };
    scheduleEnhancement();
})();
