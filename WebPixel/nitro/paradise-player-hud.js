(() => {
    'use strict';

    const BUILD = 'paradise-player-hud-v1';
    const HEADER = 'paradise_player_hud';
    const ROOT_ID = 'paradise-player-hud';

    let lastPayload = null;

    const clampNumber = (value, fallback = 0) => {
        const parsed = Number(value);
        return Number.isFinite(parsed) ? parsed : fallback;
    };

    const clampPercent = (current, maximum) => {
        const max = Math.max(0, clampNumber(maximum));
        const value = Math.max(0, clampNumber(current));
        if (max <= 0) return 0;
        return Math.max(0, Math.min(100, (value / max) * 100));
    };

    const escapeAttribute = value => String(value || '').replace(/["&<>]/g, character => ({
        '"': '&quot;',
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;'
    }[character]));

    const getAvatarUrl = look => {
        const figure = String(look || '').trim();
        if (!figure) return '';

        return 'https://www.habbo.com/habbo-imaging/avatarimage?figure=' +
            encodeURIComponent(figure) +
            '&size=l&direction=2&head_direction=2&gesture=sml&action=std';
    };

    const createHud = () => {
        let root = document.getElementById(ROOT_ID);
        if (root) return root;

        root = document.createElement('section');
        root.id = ROOT_ID;
        root.dataset.ready = '0';
        root.dataset.build = BUILD;
        root.setAttribute('aria-label', 'Statut du joueur');
        root.innerHTML = `
            <div class="paradise-player-hud__avatar">
                <img class="paradise-player-hud__avatar-image" alt="" draggable="false">
                <svg class="paradise-player-hud__avatar-fallback" viewBox="0 0 24 24" aria-hidden="true">
                    <path fill="currentColor" d="M12 12a4.25 4.25 0 1 0 0-8.5 4.25 4.25 0 0 0 0 8.5Zm0 2c-4.56 0-8.25 2.38-8.25 5.31 0 .66.54 1.19 1.2 1.19h14.1c.66 0 1.2-.53 1.2-1.19C20.25 16.38 16.56 14 12 14Z"/>
                </svg>
            </div>
            <div class="paradise-player-hud__content">
                <div class="paradise-player-hud__header">
                    <div class="paradise-player-hud__name">Joueur</div>
                    <div class="paradise-player-hud__role" data-visible="0">
                        <span class="paradise-player-hud__role-badge" aria-hidden="true">
                            <svg viewBox="0 0 24 24"><path d="m12 2 7 3v5c0 5.1-2.98 9.28-7 11-4.02-1.72-7-5.9-7-11V5l7-3Zm0 4.1L8.2 7.72v2.18c0 3.29 1.67 6.12 3.8 7.55 2.13-1.43 3.8-4.26 3.8-7.55V7.72L12 6.1Z"/></svg>
                        </span>
                        <span class="paradise-player-hud__role-text"></span>
                    </div>
                </div>
                <div class="paradise-player-hud__stats">
                    <div class="paradise-player-hud__row paradise-player-hud__row--health">
                        <span class="paradise-player-hud__stat-icon paradise-player-hud__stat-icon--health" aria-hidden="true">
                            <svg viewBox="0 0 24 24"><path d="M12 21s-8.5-4.9-8.5-11.2A4.8 4.8 0 0 1 12 6.76 4.8 4.8 0 0 1 20.5 9.8C20.5 16.1 12 21 12 21Z"/></svg>
                        </span>
                        <span class="paradise-player-hud__track"><span class="paradise-player-hud__fill paradise-player-hud__fill--health"></span></span>
                        <span class="paradise-player-hud__value"><span class="paradise-player-hud__health-current">0</span><span class="paradise-player-hud__value-max">/<span class="paradise-player-hud__health-max">0</span></span></span>
                    </div>
                    <div class="paradise-player-hud__row paradise-player-hud__row--shield">
                        <span class="paradise-player-hud__stat-icon paradise-player-hud__stat-icon--shield" aria-hidden="true">
                            <svg viewBox="0 0 24 24"><path d="M12 3 19 6v5.2c0 4.55-2.75 8.35-7 9.8-4.25-1.45-7-5.25-7-9.8V6l7-3Z"/></svg>
                        </span>
                        <span class="paradise-player-hud__track"><span class="paradise-player-hud__fill paradise-player-hud__fill--shield"></span></span>
                        <span class="paradise-player-hud__value"><span class="paradise-player-hud__shield-current">0</span><span class="paradise-player-hud__value-max">/<span class="paradise-player-hud__shield-max">0</span></span></span>
                    </div>
                </div>
            </div>`;

        document.body.appendChild(root);

        const avatar = root.querySelector('.paradise-player-hud__avatar');
        const avatarImage = root.querySelector('.paradise-player-hud__avatar-image');
        avatarImage.addEventListener('error', () => avatar.classList.add('is-fallback'));
        avatarImage.addEventListener('load', () => avatar.classList.remove('is-fallback'));

        return root;
    };

    const updateHud = payload => {
        if (!payload || typeof payload !== 'object') return;

        const root = createHud();
        const username = String(payload.username || '').trim() || 'Joueur';
        const health = Math.max(0, clampNumber(payload.health));
        const maxHealth = Math.max(0, clampNumber(payload.maxHealth));
        const shield = Math.max(0, clampNumber(payload.shield));
        const maxShield = Math.max(0, clampNumber(payload.maxShield));
        const role = String(payload.roleRank || payload.role || '').trim();
        const look = String(payload.look || '').trim();

        root.querySelector('.paradise-player-hud__name').textContent = username;
        root.querySelector('.paradise-player-hud__health-current').textContent = String(health);
        root.querySelector('.paradise-player-hud__health-max').textContent = String(maxHealth);
        root.querySelector('.paradise-player-hud__shield-current').textContent = String(shield);
        root.querySelector('.paradise-player-hud__shield-max').textContent = String(maxShield);
        root.querySelector('.paradise-player-hud__fill--health').style.width = clampPercent(health, maxHealth) + '%';
        root.querySelector('.paradise-player-hud__fill--shield').style.width = clampPercent(shield, maxShield) + '%';

        const roleNode = root.querySelector('.paradise-player-hud__role');
        const roleText = root.querySelector('.paradise-player-hud__role-text');
        roleText.textContent = role;
        roleNode.dataset.visible = role ? '1' : '0';
        roleNode.title = role || '';

        const avatar = root.querySelector('.paradise-player-hud__avatar');
        const avatarImage = root.querySelector('.paradise-player-hud__avatar-image');
        const avatarUrl = getAvatarUrl(look);
        if (avatarUrl) {
            if (avatarImage.dataset.look !== look) {
                avatarImage.dataset.look = escapeAttribute(look);
                avatarImage.src = avatarUrl;
            }
        } else {
            avatar.classList.add('is-fallback');
            avatarImage.removeAttribute('src');
        }

        root.dataset.ready = '1';
        lastPayload = payload;
    };

    const parseJsonCandidate = value => {
        if (!value) return null;
        if (typeof value === 'object') return value;
        if (typeof value !== 'string') return null;

        const text = value.trim();
        if (!text) return null;

        try {
            return JSON.parse(text);
        } catch (_) {
            const start = text.indexOf('{');
            const end = text.lastIndexOf('}');
            if (start !== -1 && end > start) {
                try {
                    return JSON.parse(text.slice(start, end + 1));
                } catch (_) {
                    return null;
                }
            }
        }

        return null;
    };

    const findHudPayload = value => {
        const parsed = parseJsonCandidate(value);
        if (!parsed) return null;

        if (Array.isArray(parsed)) {
            for (const entry of parsed) {
                const found = findHudPayload(entry);
                if (found) return found;
            }
            return null;
        }

        if (parsed.header === HEADER) return parsed;
        if (parsed.data && parsed.data.header === HEADER) return parsed.data;
        if (parsed.payload && parsed.payload.header === HEADER) return parsed.payload;

        return null;
    };

    const ingestIncomingPacket = (eventName, extraData) => {
        let payload = findHudPayload(extraData) || findHudPayload(eventName);

        if (!payload && typeof eventName === 'string' && eventName.includes(HEADER)) {
            const parsedExtra = parseJsonCandidate(extraData);
            if (parsedExtra && typeof parsedExtra === 'object') payload = parsedExtra;
        }

        if (!payload && typeof extraData === 'string' && extraData.includes(HEADER)) {
            payload = findHudPayload(extraData);
        }

        if (payload) updateHud(payload);
    };

    const installIncomingBridge = () => {
        if (!window.rdp || typeof window.rdp.IncomingPacket !== 'function') return false;

        const current = window.rdp.IncomingPacket;
        if (current.__paradisePlayerHudBridge === true) return true;

        function paradisePlayerHudIncomingBridge(eventName, extraData) {
            const result = current.apply(this, arguments);

            try {
                ingestIncomingPacket(eventName, extraData);
            } catch (error) {
                console.warn('[ParadiseRP] player HUD packet ignored', error);
            }

            return result;
        }

        paradisePlayerHudIncomingBridge.__paradisePlayerHudBridge = true;
        paradisePlayerHudIncomingBridge.__paradisePlayerHudOriginal = current;
        window.rdp.IncomingPacket = paradisePlayerHudIncomingBridge;
        return true;
    };

    window.addEventListener('paradise-player-hud', event => {
        if (event && event.detail) updateHud(event.detail);
    });

    createHud();

    if (!installIncomingBridge()) {
        let attempts = 0;
        const timer = window.setInterval(() => {
            attempts += 1;
            if (installIncomingBridge() || attempts >= 80) {
                window.clearInterval(timer);
            }
        }, 250);
    }

    // If another overlay wraps rdp.IncomingPacket after us, perform two finite rechecks.
    [5000, 12000].forEach(delay => window.setTimeout(installIncomingBridge, delay));

    if (lastPayload) updateHud(lastPayload);
    console.info('[ParadiseRP] player HUD loaded', BUILD);
})();
