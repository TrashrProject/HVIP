(() => {
    const BUILD = 'paradise-side-rail-v1';
    const HANDLE_ID = 'paradise-side-rail-handle';
    const STORAGE_KEY = 'paradise.sideRail.collapsed';

    const LABELS = [
        'Inventaire',
        'Catalogue',
        'Appartements',
        'Profil',
        'Amis',
        'Mes appartements',
        'Réglages'
    ];

    let rail = null;
    let handle = null;
    let nativeToggle = null;
    let raf = 0;

    const railImageSelector = [
        'img[src*="/side-rail/rail-icon-"]',
        'img[src*="/side-rail/inventory.png"]',
        'img[src*="/side-rail/catalog.png"]',
        'img[src*="/side-rail/rooms.png"]',
        'img[src*="/side-rail/me-profile.png"]',
        'img[src*="/side-rail/friendall.png"]',
        'img[src*="/side-rail/me-rooms.png"]',
        'img[src*="/side-rail/me-settings.png"]'
    ].join(',');

    function isVisible(node) {
        if (!node || !node.isConnected) return false;
        const style = getComputedStyle(node);
        if (style.display === 'none' || style.visibility === 'hidden' || Number(style.opacity) === 0) return false;
        const rect = node.getBoundingClientRect();
        return rect.width > 0 && rect.height > 0;
    }

    function railImagesWithin(node) {
        if (!node || !node.querySelectorAll) return [];
        return [...node.querySelectorAll(railImageSelector)].filter(isVisible);
    }

    function findRail() {
        const images = [...document.querySelectorAll(railImageSelector)].filter(isVisible);
        if (images.length < 5) return null;

        const candidates = new Map();

        for (const image of images) {
            let node = image.parentElement;
            let depth = 0;

            while (node && node !== document.body && depth < 8) {
                const rect = node.getBoundingClientRect();
                const count = railImagesWithin(node).length;

                if (
                    count >= 5 &&
                    rect.left < 130 &&
                    rect.width >= 40 && rect.width <= 180 &&
                    rect.height >= 180 && rect.height <= window.innerHeight
                ) {
                    const area = rect.width * rect.height;
                    const current = candidates.get(node);
                    if (!current || area < current.area) candidates.set(node, { node, area, count });
                }

                node = node.parentElement;
                depth++;
            }
        }

        if (!candidates.size) return null;

        return [...candidates.values()]
            .sort((a, b) => {
                if (b.count !== a.count) return b.count - a.count;
                return a.area - b.area;
            })[0].node;
    }

    function clickableForImage(image, root) {
        let node = image.parentElement;
        let fallback = image.parentElement;
        let depth = 0;

        while (node && node !== root && depth < 5) {
            if (
                node.matches('button, a, [role="button"], .cursor-pointer') ||
                typeof node.onclick === 'function'
            ) return node;

            const rect = node.getBoundingClientRect();
            if (rect.width >= 34 && rect.width <= 70 && rect.height >= 34 && rect.height <= 70) fallback = node;

            node = node.parentElement;
            depth++;
        }

        return fallback;
    }

    function sourceIndex(image, fallbackIndex) {
        const src = String(image.currentSrc || image.src || '');
        const match = src.match(/rail-icon-(\d+)\.png/i);
        if (match) return Math.max(0, Number(match[1]) - 1);

        if (/inventory\.png/i.test(src)) return 0;
        if (/catalog\.png/i.test(src)) return 1;
        if (/\/rooms\.png/i.test(src)) return 2;
        if (/me-profile\.png/i.test(src)) return 3;
        if (/friendall\.png/i.test(src)) return 4;
        if (/me-rooms\.png/i.test(src)) return 5;
        if (/me-settings\.png/i.test(src)) return 6;

        return fallbackIndex;
    }

    function decorateItems(root) {
        const images = railImagesWithin(root);
        const seen = new Set();

        images.forEach((image, fallbackIndex) => {
            const item = clickableForImage(image, root);
            if (!item || seen.has(item)) return;
            seen.add(item);

            const index = sourceIndex(image, fallbackIndex);
            item.classList.add('paradise-side-rail-item');
            item.dataset.paradiseIndex = String(index);
            item.dataset.paradiseLabel = LABELS[index] || `Menu ${index + 1}`;

            if (!item.getAttribute('aria-label')) item.setAttribute('aria-label', item.dataset.paradiseLabel);

            if (!item.dataset.paradiseBound) {
                item.dataset.paradiseBound = '1';
                item.addEventListener('click', () => {
                    root.querySelectorAll('.paradise-side-rail-item.is-active').forEach(node => {
                        if (node !== item) node.classList.remove('is-active');
                    });
                    item.classList.add('is-active');
                }, { passive: true });
            }
        });

        return [...seen];
    }

    function findNativeToggle(root, items) {
        const rootRect = root.getBoundingClientRect();
        const itemSet = new Set(items);
        let best = null;

        const candidates = [...document.querySelectorAll('button, [role="button"], .cursor-pointer')]
            .filter(node => !itemSet.has(node))
            .filter(node => !node.closest('#' + HANDLE_ID))
            .filter(isVisible);

        for (const node of candidates) {
            if (node.contains(root) || root.contains(node) && node.querySelector(railImageSelector)) continue;

            const rect = node.getBoundingClientRect();
            if (rect.width < 14 || rect.width > 46 || rect.height < 26 || rect.height > 72) continue;

            const nearRight = rect.left >= rootRect.right - 12 && rect.left <= rootRect.right + 42;
            const nearMiddle = Math.abs((rect.top + rect.height / 2) - (rootRect.top + rootRect.height / 2)) < 100;

            if (!nearRight || !nearMiddle) continue;

            const distance = Math.abs(rect.left - rootRect.right) + Math.abs((rect.top + rect.height / 2) - (rootRect.top + rootRect.height / 2));
            if (!best || distance < best.distance) best = { node, distance };
        }

        return best?.node || null;
    }

    function isCollapsed() {
        return !!rail?.classList.contains('is-collapsed');
    }

    function updateHandlePosition() {
        if (!rail || !handle || !rail.isConnected || !handle.isConnected) return;

        const rect = rail.getBoundingClientRect();
        const collapsed = isCollapsed();
        const handleHeight = handle.offsetHeight || 46;
        const left = collapsed ? 4 : Math.max(4, Math.round(rect.right + 3));
        const top = Math.max(8, Math.min(window.innerHeight - handleHeight - 8, Math.round(rect.top + rect.height / 2 - handleHeight / 2)));

        handle.style.left = left + 'px';
        handle.style.top = top + 'px';
        handle.classList.toggle('is-collapsed', collapsed);
        handle.setAttribute('aria-expanded', collapsed ? 'false' : 'true');
        handle.title = collapsed ? 'Afficher le menu' : 'Réduire le menu';
    }

    function scheduleHandlePosition() {
        cancelAnimationFrame(raf);
        raf = requestAnimationFrame(updateHandlePosition);
    }

    function createHandle() {
        if (handle?.isConnected) return handle;

        handle = document.getElementById(HANDLE_ID);
        if (handle) return handle;

        handle = document.createElement('button');
        handle.id = HANDLE_ID;
        handle.type = 'button';
        handle.setAttribute('aria-label', 'Afficher ou réduire le menu latéral');
        handle.innerHTML = '<span class="paradise-side-rail-chevron" aria-hidden="true"></span>';

        handle.addEventListener('click', () => {
            if (!rail) return;
            rail.classList.toggle('is-collapsed');
            try {
                localStorage.setItem(STORAGE_KEY, rail.classList.contains('is-collapsed') ? '1' : '0');
            } catch (_) {}
            scheduleHandlePosition();
            setTimeout(scheduleHandlePosition, 200);
        });

        document.body.appendChild(handle);
        return handle;
    }

    function applyCollapsedPreference(root) {
        let collapsed = false;
        try { collapsed = localStorage.getItem(STORAGE_KEY) === '1'; } catch (_) {}
        root.classList.toggle('is-collapsed', collapsed);
    }

    function decorate() {
        const found = findRail();
        if (!found) return;

        if (rail !== found) {
            if (rail) rail.classList.remove('paradise-side-rail', 'is-collapsed');
            rail = found;
            rail.classList.add('paradise-side-rail');
            rail.dataset.paradiseBuild = BUILD;
            applyCollapsedPreference(rail);
        }

        const items = decorateItems(rail);
        if (items.length < 5) return;

        createHandle();

        const foundToggle = findNativeToggle(rail, items);
        if (nativeToggle && nativeToggle !== foundToggle) nativeToggle.classList.remove('paradise-side-rail-native-toggle');
        nativeToggle = foundToggle;
        if (nativeToggle) nativeToggle.classList.add('paradise-side-rail-native-toggle');

        scheduleHandlePosition();
    }

    const observer = new MutationObserver(() => queueMicrotask(decorate));
    observer.observe(document.documentElement, { childList: true, subtree: true, attributes: true, attributeFilter: ['src', 'class', 'style'] });

    window.addEventListener('resize', scheduleHandlePosition, { passive: true });
    window.addEventListener('scroll', scheduleHandlePosition, { passive: true });

    setInterval(decorate, 800);
    setTimeout(decorate, 0);
})();
