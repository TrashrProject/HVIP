(() => {
    const BUILD = 'paradise-player-status-v3';
    const CARD_ID = 'paradise-player-card-v3';
    const root = () => document.getElementById('root') || document.body;

    let current = null;
    let lastScan = 0;

    const rect = node => {
        try { return node?.getBoundingClientRect?.() || null; }
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
        .filter(node => node.id !== CARD_ID && !node.closest?.(`#${CARD_ID}`))
        .filter(visible)
        .map(node => ({ node, r: rect(node), text: normalizeText(node) }))
        .filter(({ r }) => r && r.left >= -4 && r.top >= 8 && r.left < 330 && r.top < 135 && r.right > 0 && r.bottom > 0);

    const barCandidates = list => list.filter(({ r, text }) => {
        if (r.left < 58 || r.left > 112) return false;
        if (r.top < 30 || r.top > 100) return false;
        if (r.width < 145 || r.width > 225) return false;
        if (r.height < 15 || r.height > 30) return false;
        return text.length <= 40;
    });

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
        if (/\d/.test(a.text)) score -= 14;
        if (/\d/.test(b.text)) score -= 14;
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

    const pickAvatar = list => {
        const candidates = list.filter(({ node, r }) => {
            if (r.left < -2 || r.left > 30 || r.top < 18 || r.top > 56) return false;
            if (r.width < 52 || r.width > 86 || r.height < 52 || r.height > 86) return false;
            if (Math.abs(r.width - r.height) > 14) return false;
            return !!node.querySelector?.('img, canvas') || ['IMG', 'CANVAS'].includes(node.tagName) || getComputedStyle(node).backgroundImage !== 'none';
        });

        candidates.sort((a, b) => {
            const sa = Math.abs(a.r.left - 8) + Math.abs(a.r.top - 28) + Math.abs(a.r.width - 68);
            const sb = Math.abs(b.r.left - 8) + Math.abs(b.r.top - 28) + Math.abs(b.r.width - 68);
            return sa - sb;
        });
        return candidates[0]?.node || null;
    };

    const pickName = (list, healthRect) => {
        if (!healthRect) return null;
        const candidates = list.filter(({ node, r, text }) => {
            if (!text || text.length < 2 || text.length > 24) return false;
            if (/^[+\-]?\d+(?:[.,]\d+)?$/.test(text)) return false;
            if (r.left < 65 || r.left > 205 || r.top < 12 || r.bottom > healthRect.top + 3) return false;
            if (r.width < 20 || r.width > 170 || r.height < 10 || r.height > 30) return false;
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
            r.top >= healthRect.top - 8 && r.top <= healthRect.top + 9 &&
            r.width >= 5 && r.width <= 20 &&
            r.height >= 34 && r.height <= 62
        );
        candidates.sort((a, b) => {
            const sa = Math.abs(a.r.left - targetLeft) + Math.abs(a.r.top - healthRect.top) + Math.abs(a.r.height - (energyRect.bottom - healthRect.top));
            const sb = Math.abs(b.r.left - targetLeft) + Math.abs(b.r.top - healthRect.top) + Math.abs(b.r.height - (energyRect.bottom - healthRect.top));
            return sa - sb;
        });
        return candidates[0]?.node || null;
    };

    const readValue = node => {
        if (!node) return '—';
        const leafValues = [...node.querySelectorAll('*')]
            .filter(child => child.children.length === 0)
            .map(child => normalizeText(child))
            .filter(Boolean);

        for (const value of leafValues) {
            const match = value.match(/-?\d+(?:[.,]\d+)?/);
            if (match) return match[0];
        }

        const match = normalizeText(node).match(/-?\d+(?:[.,]\d+)?/);
        return match ? match[0] : '—';
    };

    const gaugeRatio = gauge => {
        const gr = rect(gauge);
        if (!gauge || !gr || gr.height <= 0) return 1;

        const candidates = [...gauge.querySelectorAll('*')]
            .filter(visible)
            .map(node => ({ node, r: rect(node) }))
            .filter(({ r }) => r && r.height >= 4 && r.height <= gr.height + 1 && r.width >= Math.max(2, gr.width * .38) && r.width <= gr.width + 2)
            .map(entry => ({
                ...entry,
                score: Math.abs(entry.r.bottom - gr.bottom) * 2 + Math.abs(entry.r.width - gr.width),
                ratio: Math.max(.12, Math.min(1, entry.r.height / gr.height))
            }))
            .sort((a, b) => a.score - b.score || a.r.height - b.r.height);

        return candidates[0]?.ratio || 1;
    };

    const stripIds = node => {
        if (!node?.querySelectorAll) return node;
        if (node.id) node.removeAttribute('id');
        node.querySelectorAll('[id]').forEach(child => child.removeAttribute('id'));
        return node;
    };

    const avatarSnapshot = source => {
        if (!source) return null;

        const sourceImg = source.tagName === 'IMG' ? source : source.querySelector?.('img');
        if (sourceImg) {
            const img = sourceImg.cloneNode(true);
            stripIds(img);
            img.className = 'paradise-player-card-avatar-art';
            img.removeAttribute('width');
            img.removeAttribute('height');
            return img;
        }

        const sourceCanvas = source.tagName === 'CANVAS' ? source : source.querySelector?.('canvas');
        if (sourceCanvas) {
            try {
                const img = document.createElement('img');
                img.src = sourceCanvas.toDataURL('image/png');
                img.alt = '';
                img.className = 'paradise-player-card-avatar-art';
                return img;
            } catch (_) {}
        }

        const bgCandidates = [source, ...source.querySelectorAll?.('*') || []];
        for (const candidate of bgCandidates) {
            const style = getComputedStyle(candidate);
            if (!style.backgroundImage || style.backgroundImage === 'none') continue;
            const art = document.createElement('div');
            art.className = 'paradise-player-card-avatar-art paradise-player-card-avatar-bg';
            art.style.backgroundImage = style.backgroundImage;
            art.style.backgroundPosition = style.backgroundPosition;
            art.style.backgroundSize = style.backgroundSize;
            art.style.backgroundRepeat = style.backgroundRepeat;
            return art;
        }

        return null;
    };

    const fallbackAvatar = () => {
        const wrap = document.createElement('div');
        wrap.className = 'paradise-player-card-fallback-avatar';
        wrap.innerHTML = '<svg viewBox="0 0 32 32" aria-hidden="true"><circle cx="16" cy="10" r="6" fill="#78929c"/><path d="M6 29c.6-7.1 4.4-11 10-11s9.4 3.9 10 11" fill="#78929c"/></svg>';
        return wrap;
    };

    const createCard = () => {
        let card = document.getElementById(CARD_ID);
        if (card) return card;

        card = document.createElement('section');
        card.id = CARD_ID;
        card.dataset.paradiseBuild = BUILD;
        card.setAttribute('aria-label', 'Statut du joueur');
        card.innerHTML = `
            <div class="paradise-player-card-avatar" data-slot="avatar"></div>
            <div class="paradise-player-card-main">
                <div class="paradise-player-card-head">
                    <div class="paradise-player-card-name" data-slot="name">Joueur</div>
                    <div class="paradise-player-card-state">RP</div>
                </div>
                <div class="paradise-player-card-stats">
                    <div class="paradise-player-card-stat paradise-player-card-stat-health">
                        <div class="paradise-player-card-stat-icon" aria-hidden="true">
                            <svg viewBox="0 0 20 20" fill="none"><path d="M10 3v14M3 10h14" stroke="currentColor" stroke-width="3" stroke-linecap="round"/></svg>
                        </div>
                        <div class="paradise-player-card-stat-copy">
                            <span class="paradise-player-card-stat-label">Vie</span>
                            <span class="paradise-player-card-stat-value" data-slot="health">—</span>
                        </div>
                    </div>
                    <div class="paradise-player-card-stat paradise-player-card-stat-energy">
                        <div class="paradise-player-card-stat-icon" aria-hidden="true">
                            <svg viewBox="0 0 20 20" fill="none"><path d="M11.6 1.8 4.8 11h5.1L8.4 18.2l6.8-9.3h-5.1l1.5-7.1Z" fill="currentColor"/></svg>
                        </div>
                        <div class="paradise-player-card-stat-copy">
                            <span class="paradise-player-card-stat-label">Énergie</span>
                            <span class="paradise-player-card-stat-value" data-slot="energy">—</span>
                        </div>
                    </div>
                    <div class="paradise-player-card-gauge" aria-hidden="true">
                        <div class="paradise-player-card-gauge-track">
                            <div class="paradise-player-card-gauge-fill" data-slot="gauge"></div>
                        </div>
                    </div>
                </div>
            </div>`;

        document.body.appendChild(card);
        return card;
    };

    const updateAvatar = (card, avatar) => {
        const slot = card.querySelector('[data-slot="avatar"]');
        if (!slot) return;
        slot.replaceChildren();
        slot.appendChild(avatarSnapshot(avatar) || fallbackAvatar());
    };

    const syncCard = () => {
        if (!current || !current.health?.isConnected || !current.energy?.isConnected) return false;
        const card = createCard();
        const name = normalizeText(current.name) || 'Joueur';
        card.querySelector('[data-slot="name"]').textContent = name;
        card.querySelector('[data-slot="health"]').textContent = readValue(current.health);
        card.querySelector('[data-slot="energy"]').textContent = readValue(current.energy);

        const gauge = card.querySelector('[data-slot="gauge"]');
        if (gauge) gauge.style.height = `${Math.round(gaugeRatio(current.gauge) * 100)}%`;
        return true;
    };

    const hideNative = refs => {
        [refs.avatar, refs.name, refs.health, refs.energy, refs.gauge]
            .filter(Boolean)
            .forEach(node => node.classList.add('paradise-player-native-hidden'));
    };

    const decorate = force => {
        const now = Date.now();
        if (!force && now - lastScan < 1200) return !!current;
        lastScan = now;

        const list = entries();
        const pair = chooseBars(list);
        if (!pair) return false;

        const refs = {
            health: pair.health.node,
            energy: pair.energy.node,
            avatar: pickAvatar(list),
            name: pickName(list, pair.health.r),
            gauge: pickGauge(list, pair.health.r, pair.energy.r)
        };

        current = refs;
        const card = createCard();
        updateAvatar(card, refs.avatar);
        hideNative(refs);
        syncCard();

        console.info('[ParadiseRP] player HUD redesigned', BUILD, {
            health: true,
            energy: true,
            avatar: !!refs.avatar,
            name: !!refs.name,
            gauge: !!refs.gauge,
            score: Math.round(pair.score * 10) / 10
        });
        return true;
    };

    [450, 900, 1600, 2800, 4500, 7000].forEach(delay => window.setTimeout(() => decorate(true), delay));

    // Lightweight sync only. DOM scanning happens again solely if Nitro replaces the native HUD nodes.
    window.setInterval(() => {
        if (!syncCard()) decorate(false);
    }, 850);

    window.addEventListener('focus', () => window.setTimeout(() => decorate(true), 100), { passive: true });

    console.info('[ParadiseRP] player HUD enhancer loaded', BUILD);
})();
