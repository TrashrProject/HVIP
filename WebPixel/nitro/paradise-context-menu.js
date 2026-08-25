(function () {
    'use strict';

    if (window.__paradisePlayerContextMenu) return;

    var MENU_SELECTOR = '.nitro-context-menu';
    var ENHANCED_CLASS = 'paradise-player-context-menu';
    var MISSING_ACHIEVEMENTS_KEY = 'achievements.title';
    var ACHIEVEMENTS_FALLBACK = 'Succès';
    var COMPACT_LABELS = {
        "Décorer l'appartement": 'Décoration',
        'Décorer l’appartement': 'Décoration',
        'Mes vêtements': 'Tenues'
    };
    var scheduled = false;

    function normalize(value) {
        return (value || '')
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .replace(/[^a-z0-9]+/g, ' ')
            .replace(/\s+/g, ' ')
            .trim()
            .toLowerCase();
    }

    function isPlayerRootMenu(menu) {
        var rawText = menu.textContent || '';
        var text = normalize(rawText);

        if (rawText.indexOf(MISSING_ACHIEVEMENTS_KEY) !== -1) return true;

        var playerLabels = [
            'decorer l appartement',
            'mes vetements',
            'danser',
            'actions',
            'panneaux',
            'succes'
        ];
        var matches = playerLabels.filter(function (label) {
            return text.indexOf(label) !== -1;
        }).length;

        return matches >= 3;
    }

    function replaceExactText(menu, source, replacement) {
        var walker = document.createTreeWalker(menu, NodeFilter.SHOW_TEXT);
        var node;

        while ((node = walker.nextNode())) {
            if (node.nodeValue.trim() !== source) continue;

            node.nodeValue = node.nodeValue.replace(source, replacement);
        }
    }

    function updateVisibleLabels(menu) {
        replaceExactText(menu, MISSING_ACHIEVEMENTS_KEY, ACHIEVEMENTS_FALLBACK);

        Object.keys(COMPACT_LABELS).forEach(function (label) {
            replaceExactText(menu, label, COMPACT_LABELS[label]);
        });
    }

    function makeKeyboardClickable(element) {
        if (!element || element.dataset.paradiseKeyboard === 'true') return;

        element.dataset.paradiseKeyboard = 'true';
        element.tabIndex = 0;
    }

    function enhanceMenu(menu) {
        if (!menu.classList.contains(ENHANCED_CLASS)) {
            if (!isPlayerRootMenu(menu)) return;
            menu.classList.add(ENHANCED_CLASS);
        }

        updateVisibleLabels(menu);

        var rows = Array.prototype.slice.call(menu.querySelectorAll('.menu-item.list-item'));
        var backIcon = menu.querySelector('.menu-item.list-item .fa-icon.left');
        var header = menu.querySelector('.menu-header');
        var footer = menu.querySelector('.menu-footer');

        menu.setAttribute('role', 'menu');
        menu.dataset.paradiseView = backIcon ? 'submenu' : 'root';

        rows.forEach(function (row) {
            row.setAttribute('role', 'menuitem');
            makeKeyboardClickable(row);
            row.classList.toggle('paradise-back-item', Boolean(row.querySelector('.fa-icon.left')));

            if (row.querySelector('.fa-icon.right')) {
                row.setAttribute('aria-haspopup', 'menu');
                row.setAttribute('aria-expanded', 'false');
            }
        });

        if (header && header.classList.contains('cursor-pointer')) {
            header.setAttribute('role', 'button');
            makeKeyboardClickable(header);
        }

        if (footer) {
            footer.setAttribute('role', 'button');
            footer.setAttribute(
                'aria-label',
                menu.classList.contains('menu-hidden') ? 'Développer le menu joueur' : 'Réduire le menu joueur'
            );
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
        Promise.resolve().then(enhanceAll);
    }

    function mutationTouchesMenu(mutation) {
        var target = mutation.target.nodeType === Node.ELEMENT_NODE
            ? mutation.target
            : mutation.target.parentElement;

        if (target && (target.matches(MENU_SELECTOR) || target.closest(MENU_SELECTOR))) return true;

        return Array.prototype.some.call(mutation.addedNodes || [], function (node) {
            return node.nodeType === Node.ELEMENT_NODE &&
                (node.matches(MENU_SELECTOR) || node.querySelector(MENU_SELECTOR));
        });
    }

    function onMutations(mutations) {
        if (mutations.some(mutationTouchesMenu)) scheduleEnhancement();
    }

    function onPointerDown(event) {
        var row = event.target.closest('.' + ENHANCED_CLASS + ' .menu-item.list-item');
        if (!row || !row.querySelector('.fa-icon.right')) return;

        row.classList.add('paradise-item-opening');
        window.setTimeout(function () {
            row.classList.remove('paradise-item-opening');
        }, 180);
    }

    function onKeyDown(event) {
        if (event.key !== 'Enter' && event.key !== ' ') return;

        var target = event.target.closest('[data-paradise-keyboard="true"]');
        if (!target || !target.closest('.' + ENHANCED_CLASS)) return;

        event.preventDefault();
        target.click();
    }

    var observer = new MutationObserver(onMutations);
    observer.observe(document.documentElement, {
        attributes: true,
        attributeFilter: ['class'],
        characterData: true,
        childList: true,
        subtree: true
    });

    document.addEventListener('pointerdown', onPointerDown, true);
    document.addEventListener('keydown', onKeyDown, true);

    window.__paradisePlayerContextMenu = {
        observer: observer,
        version: '1.0.0'
    };

    scheduleEnhancement();
})();
