(function () {
    'use strict';

    if (window.__paradisePlayerContextMenu) return;

    var MENU_SELECTOR = '.nitro-context-menu.paradise-player-context-menu';
    var scheduled = false;
    var ROOT_ITEMS = [
        { match: ['changer de nom', 'change name'], kind: 'identity', icon: '✎', label: 'Changer de nom', hint: 'Modifier votre identité' },
        { match: ['mes vetements', 'tenues', 'my clothes'], kind: 'outfits', icon: 'T', label: 'Tenues', hint: 'Modifier votre apparence' },
        { match: ['danser', 'danses', 'dance'], kind: 'dance', icon: '♫', label: 'Danses', hint: 'Choisir un mouvement' },
        { match: ['actions', 'expressions'], kind: 'actions', icon: '✦', label: 'Actions', hint: 'Postures et expressions' },
        { match: ['panneaux', 'signs'], kind: 'signs', icon: '#', label: 'Panneaux', hint: 'Afficher un panneau' },
        { match: ['poser l objet', 'drop hand item', 'lacher'], kind: 'drop', icon: '↓', label: null, hint: 'Déposer l’objet tenu' }
    ];
    var ACTION_ICONS = [
        { match: ['asseoir', 'assoir', 'sit'], icon: '▰' },
        { match: ['lever', 'debout', 'stand'], icon: '↑' },
        { match: ['saluer', 'wave'], icon: '◒' },
        { match: ['rire', 'laugh'], icon: ':)' },
        { match: ['baiser', 'kiss', 'blow'], icon: '♥' },
        { match: ['idle', 'afk', 'reposer'], icon: '…' },
        { match: ['67'], icon: '67' }
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
        if (textNode && textNode.nodeValue.trim() !== replacement) textNode.nodeValue = ' ' + replacement + ' ';
    }

    function addIcon(row, value) {
        var icon = row.querySelector(':scope > .paradise-menu-icon');
        if (!icon) {
            icon = document.createElement('span');
            icon.className = 'paradise-menu-icon';
            icon.setAttribute('aria-hidden', 'true');
            row.insertBefore(icon, row.firstChild);
        }
        if (icon.textContent !== value) icon.textContent = value;
    }

    function classifyView(menu, rows) {
        if (menu.querySelectorAll('.menu-list-split-3').length >= 3) return 'signs';
        if (!rows.some(function (row) { return row.querySelector('.fa-icon.left'); })) return 'root';
        var labels = rows.map(function (row) { return normalize(directText(row)); }).join(' | ');
        return /danse|dance/.test(labels) ? 'dance' : 'actions';
    }

    function ensureViewHeading(menu, view) {
        var titles = {
            root: ['MENU PERSONNEL', 'Que voulez-vous faire ?'],
            dance: ['DANSES', 'Choisissez votre style'],
            actions: ['ACTIONS', 'Postures et expressions'],
            signs: ['PANNEAUX', 'Choisissez un panneau']
        };
        var heading = menu.querySelector(':scope > .paradise-menu-view-heading');
        if (!heading) {
            heading = document.createElement('div');
            heading.className = 'paradise-menu-view-heading';
            var header = menu.querySelector(':scope > .menu-header');
            if (header) header.insertAdjacentElement('afterend', heading);
        }
        if (heading.dataset.paradiseHeading !== view) {
            heading.innerHTML = '<span>' + titles[view][0] + '</span><small>' + titles[view][1] + '</small>';
            heading.dataset.paradiseHeading = view;
        }
    }

    function decorateRootRow(row) {
        var text = normalize(directText(row));
        var item = ROOT_ITEMS.find(function (candidate) { return matchesAny(text, candidate.match); });
        if (!item) return;
        row.dataset.paradiseKind = item.kind;
        row.dataset.paradiseHint = item.hint;
        replaceDirectText(row, item.label);
        addIcon(row, item.icon);
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
            addIcon(row, row.dataset.paradiseKind === 'stop' ? '■' : '♫');
            return;
        }
        if (view === 'actions') {
            var action = ACTION_ICONS.find(function (candidate) { return matchesAny(text, candidate.match); });
            row.dataset.paradiseKind = 'action-choice';
            addIcon(row, action ? action.icon : '✦');
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
        ensureViewHeading(menu, view);
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
