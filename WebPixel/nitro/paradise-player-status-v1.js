(() => {
    const BUILD = 'paradise-player-status-v1';

    const rect = node => {
        try { return node.getBoundingClientRect(); }
        catch (_) { return null; }
    };

    const visible = node => {
        if (!node || !node.isConnected) return false;
        const r = rect(node);
        if (!r || r.width <= 0 || r.height <= 0) return false;
        const style = getComputedStyle(node);
        return style.display !== 'none' && style.visibility !== 'hidden' && Number(style.opacity || 1) !== 0;
    };

    const rgb = value => {
        const match = String(value || '').match(/rgba?\(\s*(\d+)\D+(\d+)\D+(\d+)/i);
        return match ? [Number(match[1]), Number(match[2]), Number(match[3])] : null;
    };

    const colorKind = node => {
        const style = getComputedStyle(node);
        const value = rgb(style.backgroundColor);
        if (!value) return null;

        const [r, g, b] = value;
        if (g >= 80 && g > r * 1.20 && g > b * 1.10) return 'health';
        if (b >= 90 && b > r * 1.28 && b > g * 1.08) return 'energy';
        if (r >= 125 && g >= 65 && b <= 105 && r > g * 1.08) return 'gauge';
        return null;
    };

    const topLeftElements = () => [...document.querySelectorAll('div, span')]
        .filter(visible)
        .map(node => ({ node, r: rect(node) }))
        .filter(({ r }) => r && r.left < 330 && r.top < 145 && r.right > 0 && r.bottom > 0);

    const horizontalCandidates = entries => entries.filter(({ r }) =>
        r.left >= 42 && r.left <= 120 &&
        r.top >= 28 && r.top <= 112 &&
        r.width >= 125 && r.width <= 245 &&
        r.height >= 14 && r.height <= 32
    );

    const chooseColored = (entries, kind, targetTop) => {
        const matches = entries
            .filter(({ node }) => colorKind(node) === kind)
            .sort((a, b) => {
                const sa = Math.abs(a.r.left - 75) * .7 + Math.abs(a.r.top - targetTop) + Math.abs(a.r.width - 187) * .18;
                const sb = Math.abs(b.r.left - 75) * .7 + Math.abs(b.r.top - targetTop) + Math.abs(b.r.width - 187) * .18;
                return sa - sb;
            });
        return matches[0] || null;
    };

    const choosePairFallback = entries => {
        let best = null;
        for (let i = 0; i < entries.length; i++) {
            for (let j = 0; j < entries.length; j++) {
                if (i === j) continue;
                const a = entries[i];
                const b = entries[j];
                if (b.r.top <= a.r.top) continue;
                if (Math.abs(a.r.left - b.r.left) > 9) continue;
                if (Math.abs(a.r.width - b.r.width) > 18) continue;
                const gap = b.r.top - a.r.bottom;
                if (gap < -4 || gap > 9) continue;
                if (Math.abs(a.r.height - b.r.height) > 8) continue;

                const score =
                    Math.abs(a.r.left - 75) * .6 +
                    Math.abs(a.r.top - 46) +
                    Math.abs(b.r.top - 69) +
                    Math.abs(a.r.width - 187) * .15;

                if (!best || score < best.score) best = { health: a, energy: b, score };
            }
        }
        return best;
    };

    const tagColoredDescendants = (root, kind, className) => {
        if (!root) return;
        const rr = rect(root);
        if (!rr) return;

        root.querySelectorAll('div, span').forEach(child => {
            if (!visible(child) || colorKind(child) !== kind) return;
            const cr = rect(child);
            if (!cr) return;
            if (cr.left < rr.left - 2 || cr.right > rr.right + 2 || cr.top < rr.top - 2 || cr.bottom > rr.bottom + 2) return;
            child.classList.add(className);
        });
    };

    const findCommonStatus = (health, energy) => {
        if (!health || !energy) return null;
        let node = health.parentElement;
        let depth = 0;
        while (node && node !== document.body && depth < 7) {
            if (node.contains(energy)) {
                const r = rect(node);
                if (r && r.left >= 0 && r.left < 100 && r.top >= 10 && r.top < 100 && r.width >= 160 && r.width <= 330 && r.height >= 45 && r.height <= 125) {
                    return node;
                }
            }
            node = node.parentElement;
            depth++;
        }
        return null;
    };

    const findName = entries => entries
        .filter(({ node, r }) => {
            if (r.left < 58 || r.left > 230 || r.top < 12 || r.top > 52 || r.height > 30 || r.width > 190) return false;
            const text = (node.textContent || '').trim();
            return text.length >= 2 && text.length <= 24 && !/^[-+]?\d+(?:[.,]\d+)?$/.test(text) && !text.includes('\n');
        })
        .sort((a, b) => {
            const sa = Math.abs(a.r.left - 83) + Math.abs(a.r.top - 25) * 1.4;
            const sb = Math.abs(b.r.left - 83) + Math.abs(b.r.top - 25) * 1.4;
            return sa - sb;
        })[0]?.node || null;

    const findAvatar = () => {
        const media = [...document.querySelectorAll('img, canvas')]
            .filter(visible)
            .map(node => ({ node, r: rect(node) }))
            .filter(({ r }) => r && r.left < 90 && r.top >= 18 && r.top < 115 && r.width >= 20 && r.width <= 85 && r.height >= 25 && r.height <= 90)
            .sort((a, b) => Math.abs(a.r.left - 28) + Math.abs(a.r.top - 40) - (Math.abs(b.r.left - 28) + Math.abs(b.r.top - 40)));

        for (const entry of media) {
            let node = entry.node;
            for (let depth = 0; depth < 5 && node && node !== document.body; depth++, node = node.parentElement) {
                const r = rect(node);
                if (!r) continue;
                if (r.left >= 0 && r.left <= 28 && r.top >= 22 && r.top <= 55 && r.width >= 55 && r.width <= 88 && r.height >= 55 && r.height <= 88) {
                    return node;
                }
            }
        }
        return null;
    };

    const findGauge = entries => entries
        .filter(({ node, r }) =>
            r.left >= 235 && r.left <= 300 &&
            r.top >= 30 && r.top <= 100 &&
            r.width >= 5 && r.width <= 22 &&
            r.height >= 28 && r.height <= 72 &&
            colorKind(node) === 'gauge'
        )
        .sort((a, b) => Math.abs(a.r.left - 267) + Math.abs(a.r.top - 46))[0]?.node || null;

    const decorate = () => {
        const entries = topLeftElements();
        const horizontal = horizontalCandidates(entries);

        let healthEntry = chooseColored(horizontal, 'health', 46);
        let energyEntry = chooseColored(horizontal, 'energy', 69);

        if (!healthEntry || !energyEntry) {
            const fallback = choosePairFallback(horizontal);
            if (fallback) {
                healthEntry ||= fallback.health;
                energyEntry ||= fallback.energy;
            }
        }

        const health = healthEntry?.node || null;
        const energy = energyEntry?.node || null;

        if (health) {
            health.classList.add('paradise-player-health');
            tagColoredDescendants(health, 'health', 'paradise-player-health-fill');
        }

        if (energy) {
            energy.classList.add('paradise-player-energy');
            tagColoredDescendants(energy, 'energy', 'paradise-player-energy-fill');
        }

        const status = findCommonStatus(health, energy);
        if (status) {
            status.classList.add('paradise-player-status');
            status.dataset.paradiseBuild = BUILD;
        }

        const name = findName(entries);
        if (name) name.classList.add('paradise-player-name');

        const avatar = findAvatar();
        if (avatar) avatar.classList.add('paradise-player-avatar');

        const gauge = findGauge(entries);
        if (gauge) {
            gauge.classList.add('paradise-player-gauge');
            tagColoredDescendants(gauge, 'gauge', 'paradise-player-gauge-fill');
        }

        const ready = !!(health && energy);
        if (ready) {
            console.info('[ParadiseRP] player status polished', BUILD, {
                health: true,
                energy: true,
                avatar: !!avatar,
                name: !!name,
                gauge: !!gauge
            });
        }
        return ready;
    };

    // Nitro mounts asynchronously. Finite retries only: no MutationObserver and no interval.
    [650, 1400, 2600, 4300, 7000].forEach(delay => window.setTimeout(decorate, delay));

    console.info('[ParadiseRP] player status enhancer loaded', BUILD);
})();
