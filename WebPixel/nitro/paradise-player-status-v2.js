(() => {
    const BUILD = 'paradise-player-status-v2';
    const root = () => document.getElementById('root') || document.body;

    const rect = node => {
        try { return node.getBoundingClientRect(); }
        catch (_) { return null; }
    };

    const visible = node => {
        if (!node || !node.isConnected) return false;
        const r = rect(node);
        if (!r || r.width < 1 || r.height < 1) return false;
        const s = getComputedStyle(node);
        return s.display !== 'none' && s.visibility !== 'hidden' && Number(s.opacity || 1) !== 0;
    };

    const normalizeText = node => String(node?.textContent || '').replace(/\s+/g, ' ').trim();

    const entries = () => [...root().querySelectorAll('*')]
        .filter(visible)
        .map(node => ({ node, r: rect(node), text: normalizeText(node) }))
        .filter(({ r }) => r && r.left >= -4 && r.top >= 8 && r.left < 330 && r.top < 135 && r.right > 0 && r.bottom > 0);

    const barCandidates = list => list.filter(({ r, text }) => {
        if (r.left < 58 || r.left > 112) return false;
        if (r.top < 30 || r.top > 100) return false;
        if (r.width < 145 || r.width > 225) return false;
        if (r.height < 15 || r.height > 30) return false;
        if (text.length > 40) return false;
        return true;
    });

    const hasNumber = text => /\d/.test(text || '');

    const pairScore = (a, b) => {
        if (b.r.top <= a.r.top) return Infinity;
        const deltaTop = b.r.top - a.r.top;
        if (deltaTop < 16 || deltaTop > 34) return Infinity;
        if (Math.abs(a.r.left - b.r.left) > 8) return Infinity;
        if (Math.abs(a.r.width - b.r.width) > 14) return Infinity;
        if (Math.abs(a.r.height - b.r.height) > 8) return Infinity;

        let score = 0;
        score += Math.abs(a.r.left - 76) * .8;
        score += Math.abs(a.r.top - 45) * 1.2;
        score += Math.abs(b.r.top - 67) * 1.2;
        score += Math.abs(a.r.width - 186) * .22;
        score += Math.abs(a.r.height - 22) * .5;
        if (hasNumber(a.text)) score -= 14;
        if (hasNumber(b.text)) score -= 14;
        if (a.text.length > 0 && a.text.length <= 12) score -= 4;
        if (b.text.length > 0 && b.text.length <= 12) score -= 4;
        return score;
    };

    const chooseBars = list => {
        const bars = barCandidates(list);
        let best = null;
        for (let i = 0; i < bars.length; i++) {
            for (let j = 0; j < bars.length; j++) {
                if (i === j) continue;
                const score = pairScore(bars[i], bars[j]);
                if (!Number.isFinite(score)) continue;
                if (!best || score < best.score) best = { health: bars[i], energy: bars[j], score };
            }
        }
        return best;
    };

    const commonAncestor = (a, b) => {
        if (!a || !b) return null;
        let current = a;
        let best = null;
        for (let depth = 0; current && current !== document.body && depth < 9; depth++, current = current.parentElement) {
            if (!current.contains(b)) continue;
            const r = rect(current);
            if (!r) continue;
            if (r.left < -4 || r.left > 90 || r.top < 10 || r.top > 80) continue;
            if (r.width < 175 || r.width > 315 || r.height < 40 || r.height > 115) continue;
            best = current;
            if (r.left <= 15 && r.width >= 250) break;
        }
        return best;
    };

    const pickAvatar = list => {
        const candidates = list.filter(({ node, r }) => {
            if (r.left < -2 || r.left > 28 || r.top < 22 || r.top > 52) return false;
            if (r.width < 55 || r.width > 82 || r.height < 55 || r.height > 82) return false;
            if (Math.abs(r.width - r.height) > 12) return false;
            return !!node.querySelector?.('img, canvas') || ['IMG', 'CANVAS'].includes(node.tagName) || getComputedStyle(node).backgroundImage !== 'none';
        });
        candidates.sort((a, b) => {
            const sa = Math.abs(a.r.left - 8) + Math.abs(a.r.top - 33) + Math.abs(a.r.width - 68);
            const sb = Math.abs(b.r.left - 8) + Math.abs(b.r.top - 33) + Math.abs(b.r.width - 68);
            return sa - sb;
        });
        return candidates[0]?.node || null;
    };

    const pickName = (list, healthRect) => {
        if (!healthRect) return null;
        const candidates = list.filter(({ node, r, text }) => {
            if (!text || text.length < 2 || text.length > 24 || /\n/.test(text)) return false;
            if (/^[+\-]?\d+(?:[.,]\d+)?$/.test(text)) return false;
            if (r.left < 68 || r.left > 205 || r.top < 14 || r.bottom > healthRect.top + 2) return false;
            if (r.width < 20 || r.width > 170 || r.height < 10 || r.height > 28) return false;
            return node.children.length === 0 || [...node.children].every(child => !visible(child));
        });
        candidates.sort((a, b) => {
            const sa = Math.abs(a.r.left - 83) + Math.abs(a.r.bottom - (healthRect.top - 3)) * 1.25;
            const sb = Math.abs(b.r.left - 83) + Math.abs(b.r.bottom - (healthRect.top - 3)) * 1.25;
            return sa - sb;
        });
        return candidates[0]?.node || null;
    };

    const pickGauge = (list, healthRect, energyRect) => {
        if (!healthRect || !energyRect) return null;
        const targetLeft = Math.max(healthRect.right, energyRect.right) + 5;
        const candidates = list.filter(({ r }) =>
            r.left >= targetLeft - 5 && r.left <= targetLeft + 18 &&
            r.top >= healthRect.top - 7 && r.top <= healthRect.top + 8 &&
            r.width >= 5 && r.width <= 18 &&
            r.height >= 35 && r.height <= 58
        );
        candidates.sort((a, b) => {
            const sa = Math.abs(a.r.left - targetLeft) + Math.abs(a.r.top - healthRect.top) + Math.abs(a.r.height - (energyRect.bottom - healthRect.top));
            const sb = Math.abs(b.r.left - targetLeft) + Math.abs(b.r.top - healthRect.top) + Math.abs(b.r.height - (energyRect.bottom - healthRect.top));
            return sa - sb;
        });
        return candidates[0]?.node || null;
    };

    const tagBarContents = (bar, type) => {
        if (!bar) return;
        const br = rect(bar);
        if (!br) return;

        [...bar.querySelectorAll('*')].filter(visible).forEach(child => {
            const cr = rect(child);
            if (!cr) return;
            const text = normalizeText(child);

            if (/^-?\d+(?:[.,]\d+)?$/.test(text) && cr.width <= br.width * .55 && cr.height <= br.height + 6) {
                child.classList.add('paradise-player-stat-value');
            }

            if (cr.left <= br.left + 34 && cr.width <= 30 && cr.height <= br.height + 8 && !/\d/.test(text)) {
                child.classList.add('paradise-player-stat-icon', `paradise-player-stat-icon-${type}`);
            }
        });
    };

    const decorate = () => {
        const list = entries();
        const pair = chooseBars(list);
        if (!pair) return false;

        const health = pair.health.node;
        const energy = pair.energy.node;
        const hr = pair.health.r;
        const er = pair.energy.r;

        health.classList.add('paradise-player-health');
        energy.classList.add('paradise-player-energy');
        tagBarContents(health, 'health');
        tagBarContents(energy, 'energy');

        const status = commonAncestor(health, energy);
        if (status) {
            status.classList.add('paradise-player-status');
            status.dataset.paradiseBuild = BUILD;
        }

        const avatar = pickAvatar(list);
        if (avatar) avatar.classList.add('paradise-player-avatar');

        const name = pickName(list, hr);
        if (name) name.classList.add('paradise-player-name');

        const gauge = pickGauge(list, hr, er);
        if (gauge) gauge.classList.add('paradise-player-gauge');

        console.info('[ParadiseRP] player status ready', BUILD, {
            score: Math.round(pair.score * 10) / 10,
            health: { x: Math.round(hr.left), y: Math.round(hr.top), w: Math.round(hr.width), h: Math.round(hr.height) },
            energy: { x: Math.round(er.left), y: Math.round(er.top), w: Math.round(er.width), h: Math.round(er.height) },
            status: !!status,
            avatar: !!avatar,
            name: !!name,
            gauge: !!gauge
        });
        return true;
    };

    // Nitro mounts asynchronously. Short finite retries only: no observer and no interval.
    [450, 900, 1600, 2800, 4500, 7000].forEach(delay => window.setTimeout(decorate, delay));

    // A focus refresh is cheap and helps after returning to the game tab without watching the DOM.
    window.addEventListener('focus', () => window.setTimeout(decorate, 80), { passive: true });

    console.info('[ParadiseRP] player status enhancer loaded', BUILD);
})();
