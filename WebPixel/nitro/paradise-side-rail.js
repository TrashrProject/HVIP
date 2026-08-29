(() => {
    const BUILD = 'paradise-side-rail-safe-v3';
    const HANDLE_ID = 'paradise-side-rail-handle';
    const BACKDROP_ID = 'paradise-side-rail-backdrop';
    const LABELS = [
        'Inventaire',
        'Catalogue',
        'Appartements',
        'Profil',
        'Amis',
        'Mes appartements',
        'Réglages'
    ];

    let installed = false;
    let railItems = [];
    let nativeToggle = null;
    let backdrop = null;
    let handle = null;
    let collapsed = false;
    let lastBounds = null;

    const rect = node => {
        try { return node.getBoundingClientRect(); } catch (_) { return null; }
    };

    const visible = node => {
        if (!node || !node.isConnected) return false;
        const r = rect(node);
        if (!r || r.width <= 0 || r.height <= 0) return false;
        const s = getComputedStyle(node);
        return s.display !== 'none' && s.visibility !== 'hidden' && Number(s.opacity || 1) !== 0;
    };

    function collectButtonCandidates() {
        const selector = 'button, a, [role="button"], .cursor-pointer';
        const raw = [...document.querySelectorAll(selector)]
            .filter(visible)
            .map(node => ({ node, r: rect(node) }))
            .filter(({ r }) => r)
            .filter(({ r }) =>
                r.left >= -4 && r.left <= 58 &&
                r.width >= 42 && r.width <= 68 &&
                r.height >= 42 && r.height <= 68 &&
                r.top >= 45 && r.bottom <= window.innerHeight - 40
            )
            .sort((a, b) => a.r.top - b.r.top || (b.r.width * b.r.height) - (a.r.width * a.r.height));

        // Remove nested/duplicate clickable elements that occupy the same row.
        const rows = [];
        for (const entry of raw) {
            const cy = entry.r.top + entry.r.height / 2;
            const cx = entry.r.left + entry.r.width / 2;
            const same = rows.find(row => Math.abs(row.cy - cy) < 7 && Math.abs(row.cx - cx) < 10);
            if (!same) {
                rows.push({ ...entry, cy, cx });
                continue;
            }

            const currentArea = same.r.width * same.r.height;
            const nextArea = entry.r.width * entry.r.height;
            if (nextArea > currentArea) Object.assign(same, entry, { cy, cx });
        }

        return rows.sort((a, b) => a.r.top - b.r.top);
    }

    function findRailItems() {
        const candidates = collectButtonCandidates();
        if (candidates.length < 6) return [];

        let best = [];

        for (let start = 0; start < candidates.length; start++) {
            const run = [candidates[start]];
            const baseX = candidates[start].cx;

            for (let i = start + 1; i < candidates.length; i++) {
                const prev = run[run.length - 1];
                const next = candidates[i];
                const gap = next.cy - prev.cy;

                if (Math.abs(next.cx - baseX) > 13) continue;
                if (gap < 38) continue;
                if (gap > 74) break;

                run.push(next);
                if (run.length === 7) break;
            }

            if (run.length > best.length) best = run;
            if (best.length === 7) break;
        }

        if (best.length < 6) return [];
        return best.slice(0, 7).map(entry => entry.node);
    }

    function boundsFor(items) {
        const rs = items.map(rect).filter(Boolean);
        if (!rs.length) return null;
        return {
            left: Math.min(...rs.map(r => r.left)),
            top: Math.min(...rs.map(r => r.top)),
            right: Math.max(...rs.map(r => r.right)),
            bottom: Math.max(...rs.map(r => r.bottom))
        };
    }

    function findNativeToggle(bounds) {
        if (!bounds) return null;
        const railMid = (bounds.top + bounds.bottom) / 2;
        let best = null;

        for (const node of document.querySelectorAll('button, [role="button"], .cursor-pointer, div')) {
            if (!visible(node) || railItems.includes(node)) continue;
            const r = rect(node);
            if (!r) continue;

            if (r.width < 15 || r.width > 34 || r.height < 34 || r.height > 62) continue;
            if (r.left < bounds.right - 2 || r.left > bounds.right + 38) continue;

            const mid = r.top + r.height / 2;
            if (Math.abs(mid - railMid) > 90) continue;

            const score = Math.abs(r.left - bounds.right) + Math.abs(mid - railMid) * .35;
            if (!best || score < best.score) best = { node, score };
        }

        return best?.node || null;
    }

    function ensureBackdrop() {
        backdrop = document.getElementById(BACKDROP_ID);
        if (!backdrop) {
            backdrop = document.createElement('div');
            backdrop.id = BACKDROP_ID;
            backdrop.setAttribute('aria-hidden', 'true');
            document.body.appendChild(backdrop);
        }
        return backdrop;
    }

    function ensureHandle() {
        handle = document.getElementById(HANDLE_ID);
        if (handle) return handle;

        handle = document.createElement('button');
        handle.id = HANDLE_ID;
        handle.type = 'button';
        handle.setAttribute('aria-label', 'Réduire le menu latéral');
        handle.innerHTML = '<span aria-hidden="true"></span>';

        handle.addEventListener('click', () => {
            if (!nativeToggle) return;
            collapsed = !collapsed;
            try { nativeToggle.click(); } catch (_) {}
            updateOverlay();
            window.setTimeout(updateOverlay, 220);
            window.setTimeout(updateOverlay, 520);
        });

        document.body.appendChild(handle);
        return handle;
    }

    function updateOverlay() {
        if (!installed || !backdrop || !handle) return;

        const current = boundsFor(railItems.filter(visible));
        if (current) lastBounds = current;
        const b = current || lastBounds;
        if (!b) return;

        backdrop.style.left = Math.max(3, Math.round(b.left - 6)) + 'px';
        backdrop.style.top = Math.max(3, Math.round(b.top - 7)) + 'px';
        backdrop.style.width = Math.round((b.right - b.left) + 12) + 'px';
        backdrop.style.height = Math.round((b.bottom - b.top) + 14) + 'px';
        backdrop.classList.toggle('is-collapsed', collapsed);

        const handleHeight = handle.offsetHeight || 36;
        handle.style.left = (collapsed ? 4 : Math.round(b.right + 4)) + 'px';
        handle.style.top = Math.max(8, Math.min(window.innerHeight - handleHeight - 8, Math.round((b.top + b.bottom - handleHeight) / 2))) + 'px';
        handle.classList.toggle('is-collapsed', collapsed);
        handle.setAttribute('aria-label', collapsed ? 'Afficher le menu latéral' : 'Réduire le menu latéral');
    }

    function install() {
        if (installed) return true;

        const items = findRailItems();
        if (items.length < 6) return false;

        railItems = items;
        railItems.forEach((item, index) => {
            item.classList.add('paradise-side-rail-item');
            item.dataset.paradiseIndex = String(index);
            item.dataset.paradiseLabel = LABELS[index] || `Menu ${index + 1}`;
            if (!item.getAttribute('aria-label')) item.setAttribute('aria-label', item.dataset.paradiseLabel);
            item.addEventListener('click', () => {
                railItems.forEach(other => other.classList.toggle('is-active', other === item));
            }, { passive: true });
        });

        const b = boundsFor(railItems);
        if (!b) return false;
        lastBounds = b;

        nativeToggle = findNativeToggle(b);
        if (nativeToggle) nativeToggle.classList.add('paradise-side-rail-native-toggle');

        ensureBackdrop();
        ensureHandle();
        installed = true;
        updateOverlay();

        window.addEventListener('resize', updateOverlay, { passive: true });
        console.info('[ParadiseRP] side rail ready', BUILD, { items: railItems.length, toggle: !!nativeToggle });
        return true;
    }

    // Deliberately finite retries: no observer, no interval, no permanent DOM scan.
    const retryDelays = [1800, 3600, 6000];
    retryDelays.forEach(delay => window.setTimeout(() => {
        if (!installed) install();
    }, delay));

    console.info('[ParadiseRP] side rail enhancer loaded', BUILD);
})();
