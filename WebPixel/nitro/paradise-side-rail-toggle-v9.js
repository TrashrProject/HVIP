(() => {
    const BUILD = 'paradise-side-rail-toggle-v9';

    const rect = node => {
        try { return node.getBoundingClientRect(); } catch (_) { return null; }
    };

    function neutralizeNearbySurfaces(toggle) {
        if (!toggle?.isConnected) return;

        toggle.classList.add('paradise-side-rail-toggle-v9');

        const tr = rect(toggle);
        if (!tr) return;
        const tcx = tr.left + tr.width / 2;
        const tcy = tr.top + tr.height / 2;

        let node = toggle.parentElement;
        let depth = 0;
        while (node && node !== document.body && depth < 4) {
            const r = rect(node);
            if (!r) break;

            const cx = r.left + r.width / 2;
            const cy = r.top + r.height / 2;
            const closeCenter = Math.abs(cx - tcx) <= 10 && Math.abs(cy - tcy) <= 12;
            const toggleSized = r.width <= 42 && r.height <= 72;

            if (closeCenter && toggleSized) node.classList.add('paradise-side-rail-toggle-host-v9');
            else break;

            node = node.parentElement;
            depth++;
        }
    }

    function findToggle() {
        const tagged = document.querySelector('.paradise-side-rail-native-toggle');
        if (tagged) return tagged;

        // Fallback: the native tab lives immediately to the right of the styled rail.
        const shell = document.querySelector('.paradise-side-rail-shell');
        const sr = rect(shell);
        if (!shell || !sr) return null;

        const midY = sr.top + sr.height / 2;
        let best = null;

        for (const node of document.querySelectorAll('button, [role="button"], .cursor-pointer, div')) {
            if (!node.isConnected || shell.contains(node)) continue;
            const r = rect(node);
            if (!r) continue;
            if (r.width < 14 || r.width > 42 || r.height < 28 || r.height > 70) continue;
            if (r.left < sr.right - 6 || r.left > sr.right + 42) continue;

            const cy = r.top + r.height / 2;
            const score = Math.abs(r.left - sr.right) + Math.abs(cy - midY) * .4;
            if (!best || score < best.score) best = { node, score };
        }

        return best?.node || null;
    }

    function apply() {
        const toggle = findToggle();
        if (!toggle) return false;

        toggle.classList.add('paradise-side-rail-native-toggle');
        neutralizeNearbySurfaces(toggle);

        if (!toggle.dataset.paradiseV9Bound) {
            toggle.dataset.paradiseV9Bound = '1';
            toggle.addEventListener('click', () => {
                // Nitro may replace the visual wrapper when collapsing/expanding.
                // Re-check only a few times after the click, then stop.
                [30, 100, 240, 500, 850].forEach(delay => {
                    window.setTimeout(() => {
                        const next = findToggle();
                        if (next) {
                            next.classList.add('paradise-side-rail-native-toggle');
                            neutralizeNearbySurfaces(next);
                        }
                    }, delay);
                });
            }, { passive: true });
        }

        return true;
    }

    [1300, 2500, 4200, 6500].forEach(delay => window.setTimeout(apply, delay));
    console.info('[ParadiseRP] side rail toggle patch loaded', BUILD);
})();
