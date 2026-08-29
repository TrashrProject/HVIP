(() => {
    const BUILD = 'paradise-side-rail-native-v7';
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
    let shellRef = null;
    let toggleRef = null;
    let railItemsRef = [];
    let collapsed = false;
    let lastToggleRect = null;

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
        const raw = [...document.querySelectorAll('button, a, [role="button"], .cursor-pointer')]
            .filter(visible)
            .map(node => ({ node, r: rect(node) }))
            .filter(({ r }) => r)
            .filter(({ r }) =>
                r.left >= -5 && r.left <= 60 &&
                r.width >= 42 && r.width <= 70 &&
                r.height >= 42 && r.height <= 70 &&
                r.top >= 40 && r.bottom <= window.innerHeight - 30
            )
            .sort((a, b) => a.r.top - b.r.top || (b.r.width * b.r.height) - (a.r.width * a.r.height));

        const rows = [];
        for (const entry of raw) {
            const cy = entry.r.top + entry.r.height / 2;
            const cx = entry.r.left + entry.r.width / 2;
            const same = rows.find(row => Math.abs(row.cy - cy) < 8 && Math.abs(row.cx - cx) < 10);
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
                if (Math.abs(next.cx - baseX) > 14) continue;
                if (gap < 38) continue;
                if (gap > 76) break;
                run.push(next);
                if (run.length === 7) break;
            }

            if (run.length > best.length) best = run;
            if (best.length === 7) break;
        }

        return best.length >= 6 ? best.slice(0, 7).map(entry => entry.node) : [];
    }

    function findNativeShell(items) {
        if (!items.length) return null;
        const candidates = [];
        let node = items[0].parentElement;
        let depth = 0;

        while (node && node !== document.body && depth < 10) {
            if (items.every(item => node.contains(item))) {
                const r = rect(node);
                if (
                    r &&
                    r.left >= -12 && r.left <= 18 &&
                    r.width >= 55 && r.width <= 105 &&
                    r.height >= 260 && r.height <= 620
                ) {
                    candidates.push({ node, area: r.width * r.height, r });
                }
            }
            node = node.parentElement;
            depth++;
        }

        if (!candidates.length) return null;
        candidates.sort((a, b) => a.area - b.area);
        return candidates[0].node;
    }

    function findNativeToggle(shell, items) {
        const sr = rect(shell);
        if (!sr) return null;
        const railMid = sr.top + sr.height / 2;
        let best = null;

        for (const node of document.querySelectorAll('button, [role="button"], .cursor-pointer, div')) {
            if (!visible(node) || items.includes(node) || shell.contains(node)) continue;
            const r = rect(node);
            if (!r) continue;
            if (r.width < 16 || r.width > 38 || r.height < 30 || r.height > 64) continue;
            if (r.left < sr.right - 4 || r.left > sr.right + 40) continue;
            const mid = r.top + r.height / 2;
            if (Math.abs(mid - railMid) > 85) continue;

            const score = Math.abs(r.left - sr.right) + Math.abs(mid - railMid) * .35;
            if (!best || score < best.score) best = { node, score };
        }
        return best?.node || null;
    }

    function findToggleNearLastPosition() {
        if (!lastToggleRect) return null;
        const targetX = lastToggleRect.left + lastToggleRect.width / 2;
        const targetY = lastToggleRect.top + lastToggleRect.height / 2;
        let best = null;

        for (const node of document.querySelectorAll('button, [role="button"], .cursor-pointer, div')) {
            if (!visible(node)) continue;
            const r = rect(node);
            if (!r) continue;
            if (r.width < 14 || r.width > 42 || r.height < 28 || r.height > 68) continue;
            if (r.left < -4 || r.left > 120) continue;

            const cx = r.left + r.width / 2;
            const cy = r.top + r.height / 2;
            const distance = Math.hypot(cx - targetX, cy - targetY);
            if (distance > 90) continue;

            if (!best || distance < best.distance) best = { node, distance };
        }

        return best?.node || null;
    }

    function applyItemHooks(items) {
        items.forEach((item, index) => {
            item.classList.add('paradise-side-rail-item');
            item.dataset.paradiseIndex = String(index);
            item.dataset.paradiseLabel = LABELS[index] || `Menu ${index + 1}`;
            if (!item.getAttribute('aria-label')) item.setAttribute('aria-label', item.dataset.paradiseLabel);

            if (!item.dataset.paradiseActiveBound) {
                item.dataset.paradiseActiveBound = '1';
                item.addEventListener('click', () => {
                    items.forEach(other => other.classList.toggle('is-active', other === item));
                }, { passive: true });
            }
        });
    }

    function bindToggle(toggle) {
        if (!toggle) return;

        toggleRef = toggle;
        const tr = rect(toggle);
        if (tr) lastToggleRect = tr;

        toggle.classList.add('paradise-side-rail-native-toggle');
        toggle.classList.toggle('is-paradise-collapsed', collapsed);

        if (!toggle.dataset.paradiseChevronBound) {
            toggle.dataset.paradiseChevronBound = '1';
            toggle.addEventListener('click', () => {
                const currentRect = rect(toggle);
                if (currentRect) lastToggleRect = currentRect;

                collapsed = !collapsed;
                toggle.classList.toggle('is-paradise-collapsed', collapsed);

                // Nitro can repaint/replace its collapse control. Re-apply our classes
                // a few times after the native click, then stop. No observer or interval.
                [40, 140, 320, 650].forEach(delay => {
                    window.setTimeout(refreshAfterToggle, delay);
                });
            }, { passive: true });
        }
    }

    function refreshAfterToggle() {
        if (shellRef?.isConnected) shellRef.classList.add('paradise-side-rail-shell');

        let nextToggle = toggleRef?.isConnected && visible(toggleRef) ? toggleRef : null;
        if (!nextToggle) nextToggle = findToggleNearLastPosition();

        if (nextToggle) bindToggle(nextToggle);

        // On expand the item nodes can be recreated too, so retag them once they exist.
        if (!collapsed) {
            const items = findRailItems();
            if (items.length >= 6) {
                railItemsRef = items;
                applyItemHooks(items);
                const shell = findNativeShell(items);
                if (shell) {
                    shellRef = shell;
                    shellRef.classList.add('paradise-side-rail-shell');
                    const nativeToggle = findNativeToggle(shell, items);
                    if (nativeToggle) bindToggle(nativeToggle);
                }
            }
        }
    }

    function install() {
        const items = findRailItems();
        if (items.length < 6) return false;

        const shell = findNativeShell(items);
        if (!shell) return false;

        shellRef = shell;
        railItemsRef = items;
        shell.classList.add('paradise-side-rail-shell');
        shell.dataset.paradiseBuild = BUILD;

        applyItemHooks(items);

        const toggle = findNativeToggle(shell, items);
        if (toggle) bindToggle(toggle);

        installed = true;
        console.info('[ParadiseRP] side rail ready', BUILD, {
            items: items.length,
            shell: true,
            toggle: !!toggle
        });
        return true;
    }

    // Finite retries only. No MutationObserver, no interval, no permanent DOM scan.
    [1400, 2800, 4800, 7200].forEach(delay => {
        window.setTimeout(() => {
            if (!installed) install();
        }, delay);
    });

    console.info('[ParadiseRP] side rail enhancer loaded', BUILD);
})();
