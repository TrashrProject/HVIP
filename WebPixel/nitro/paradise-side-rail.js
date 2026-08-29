(() => {
    const BUILD = 'paradise-side-rail-v2';
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
        'img[src*="/side-rail/"]',
        'img[src*="rail-icon-"]',
        'img[src*="inventory"]',
        'img[src*="catalog"]',
        'img[src*="rooms"]',
        'img[src*="profile"]',
        'img[src*="friend"]',
        'img[src*="settings"]'
    ].join(',');

    function isVisible(node) {
        if (!node || !node.isConnected) return false;
        const style = getComputedStyle(node);
        if (style.display === 'none' || style.visibility === 'hidden' || Number(style.opacity) === 0) return false;
        const rect = node.getBoundingClientRect();
        return rect.width > 0 && rect.height > 0;
    }

    function isClickable(node) {
        if (!node || !node.matches) return false;
        if (node.matches('button, a, [role="button"], .cursor-pointer')) return true;
        if (node.tabIndex >= 0) return true;
        if (typeof node.onclick === 'function') return true;
        try { return getComputedStyle(node).cursor === 'pointer'; } catch (_) { return false; }
    }

    function rectFor(node) {
        try { return node.getBoundingClientRect(); } catch (_) { return null; }
    }

    function squareClickableDescendants(root) {
        if (!root?.querySelectorAll) return [];
        const rootRect = rectFor(root);
        if (!rootRect) return [];

        const raw = [...root.querySelectorAll('*')]
            .filter(isVisible)
            .filter(isClickable)
            .map(node => ({ node, rect: rectFor(node) }))
            .filter(entry => entry.rect)
            .filter(({ rect }) =>
                rect.width >= 34 && rect.width <= 72 &&
                rect.height >= 34 && rect.height <= 72 &&
                rect.left >= rootRect.left - 4 &&
                rect.right <= rootRect.right + 8
            )
            .sort((a, b) => a.rect.top - b.rect.top || a.rect.left - b.rect.left || (a.rect.width * a.rect.height) - (b.rect.width * b.rect.height));

        const rows = [];
        for (const entry of raw) {
            const cy = entry.rect.top + entry.rect.height / 2;
            const existing = rows.find(row => Math.abs(row.cy - cy) < 12);
            if (!existing) {
                rows.push({ ...entry, cy });
                continue;
            }
            const currentArea = existing.rect.width * existing.rect.height;
            const nextArea = entry.rect.width * entry.rect.height;
            if (nextArea < currentArea) Object.assign(existing, entry, { cy });
        }

        return rows.sort((a, b) => a.rect.top - b.rect.top).map(row => row.node);
    }

    function imageDrivenRail() {
        const images = [...document.querySelectorAll(railImageSelector)].filter(isVisible);
        if (images.length < 5) return null;

        const candidates = [];
        for (const image of images) {
            let node = image.parentElement;
            let depth = 0;
            while (node && node !== document.body && depth < 9) {
                const rect = rectFor(node);
                if (rect && rect.left < 130 && rect.width >= 44 && rect.width <= 130 && rect.height >= 220 && rect.height <= Math.min(window.innerHeight, 720)) {
                    const imageCount = node.querySelectorAll(railImageSelector).length;
                    const clickCount = squareClickableDescendants(node).length;
                    if (imageCount >= 5 || clickCount >= 5) candidates.push({ node, rect, imageCount, clickCount });
                }
                node = node.parentElement;
                depth++;
            }
        }

        if (!candidates.length) return null;
        candidates.sort((a, b) =>
            (b.imageCount + b.clickCount) - (a.imageCount + a.clickCount) ||
            (a.rect.width * a.rect.height) - (b.rect.width * b.rect.height)
        );
        return candidates[0].node;
    }

    function geometryDrivenRail() {
        const roots = [...document.querySelectorAll('div, nav, aside, section')];
        let best = null;

        for (const node of roots) {
            if (!isVisible(node)) continue;
            const rect = rectFor(node);
            if (!rect) continue;

            if (rect.left > 28 || rect.right < 42) continue;
            if (rect.width < 48 || rect.width > 105) continue;
            if (rect.height < 250 || rect.height > Math.min(window.innerHeight - 20, 680)) continue;

            const items = squareClickableDescendants(node);
            if (items.length < 6 || items.length > 10) continue;

            const itemRects = items.map(rectFor).filter(Boolean);
            if (itemRects.length < 6) continue;
            const centers = itemRects.map(r => r.left + r.width / 2);
            const spread = Math.max(...centers) - Math.min(...centers);
            if (spread > 18) continue;

            const top = Math.min(...itemRects.map(r => r.top));
            const bottom = Math.max(...itemRects.map(r => r.bottom));
            const verticalCoverage = bottom - top;
            if (verticalCoverage < 240) continue;

            const score = items.length * 1000 - rect.width * rect.height * 0.01 - rect.left * 25 - spread * 20;
            if (!best || score > best.score) best = { node, score };
        }

        return best?.node || null;
    }

    function findRail() {
        return imageDrivenRail() || geometryDrivenRail();
    }

    function sourceIndexFromImage(item, fallbackIndex) {
        const image = item.querySelector?.('img');
        const src = String(image?.currentSrc || image?.src || '');
        const match = src.match(/rail-icon-(\d+)\.png/i);
        if (match) return Math.max(0, Number(match[1]) - 1);
        if (/inventory/i.test(src)) return 0;
        if (/catalog/i.test(src)) return 1;
        if (/\/rooms/i.test(src)) return 2;
        if (/profile/i.test(src)) return 3;
        if (/friend/i.test(src)) return 4;
        if (/me-rooms/i.test(src)) return 5;
        if (/settings/i.test(src)) return 6;
        return fallbackIndex;
    }

    function decorateItems(root) {
        let items = squareClickableDescendants(root);

        if (items.length < 5) {
            const images = [...root.querySelectorAll(railImageSelector)].filter(isVisible);
            items = images.map(image => {
                let node = image.parentElement;
                let fallback = node;
                let depth = 0;
                while (node && node !== root && depth < 5) {
                    if (isClickable(node)) return node;
                    const rect = rectFor(node);
                    if (rect && rect.width >= 34 && rect.width <= 72 && rect.height >= 34 && rect.height <= 72) fallback = node;
                    node = node.parentElement;
                    depth++;
                }
                return fallback;
            }).filter(Boolean);
        }

        items = [...new Set(items)]
            .sort((a, b) => rectFor(a).top - rectFor(b).top)
            .slice(0, 7);

        items.forEach((item, fallbackIndex) => {
            const index = sourceIndexFromImage(item, fallbackIndex);
            item.classList.add('paradise-side-rail-item');
            item.dataset.paradiseIndex = String(index);
            item.dataset.paradiseLabel = LABELS[index] || LABELS[fallbackIndex] || `Menu ${fallbackIndex + 1}`;
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

        return items;
    }

    function findNativeToggle(root, items) {
        const rootRect = rectFor(root);
        if (!rootRect) return null;
        const itemSet = new Set(items);
        let best = null;

        for (const node of document.querySelectorAll('button, [role="button"], .cursor-pointer, div')) {
            if (!isVisible(node) || itemSet.has(node) || root.contains(node) || node.id === HANDLE_ID) continue;
            const rect = rectFor(node);
            if (!rect) continue;
            if (rect.width < 16 || rect.width > 38 || rect.height < 34 || rect.height > 64) continue;
            if (rect.left < rootRect.right - 6 || rect.left > rootRect.right + 36) continue;

            const rootMid = rootRect.top + rootRect.height / 2;
            const itemMid = rect.top + rect.height / 2;
            if (Math.abs(itemMid - rootMid) > 85) continue;

            const style = getComputedStyle(node);
            const blueish = /rgb\((?:0|1?\d?\d),\s*(?:8\d|9\d|1\d\d),\s*(?:1\d\d|2\d\d)\)/.test(style.backgroundColor || '');
            const score = Math.abs(rect.left - rootRect.right) + Math.abs(itemMid - rootMid) - (blueish ? 25 : 0);
            if (!best || score < best.score) best = { node, score };
        }

        return best?.node || null;
    }

    function isCollapsed() {
        return !!rail?.classList.contains('is-collapsed');
    }

    function updateHandlePosition() {
        if (!rail || !handle || !rail.isConnected || !handle.isConnected) return;
        const rect = rectFor(rail);
        if (!rect) return;
        const collapsed = isCollapsed();
        const handleHeight = handle.offsetHeight || 38;
        const left = collapsed ? 5 : Math.max(5, Math.round(rect.right + 2));
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
            try { localStorage.setItem(STORAGE_KEY, rail.classList.contains('is-collapsed') ? '1' : '0'); } catch (_) {}
            scheduleHandlePosition();
            setTimeout(scheduleHandlePosition, 220);
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
    setInterval(decorate, 1000);
    setTimeout(decorate, 50);
    setTimeout(decorate, 600);

    console.info('[ParadiseRP] side rail enhancer loaded', BUILD);
})();
