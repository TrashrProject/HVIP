(function () {
    'use strict';

    var windowEl = document.getElementById('WantedListV2');
    var legacyWindow = document.getElementById('WantedList');
    var legacyList = document.getElementById('WS_WList');
    var openButton = document.querySelector('[data-overlay="WantedList"]');
    var closeButton = document.getElementById('CloseWantedListV2');
    var searchInput = document.getElementById('WantedSearchV2');
    var cardsEl = document.getElementById('WantedCardsV2');
    var countEl = document.getElementById('WantedCountV2');
    var emptyEl = document.getElementById('WantedEmptyV2');
    var profileEl = document.getElementById('WantedProfileV2');
    var profileAvatar = document.getElementById('WantedProfileAvatarV2');
    var profileName = document.getElementById('WantedProfileNameV2');
    var profileAlias = document.getElementById('WantedProfileAliasV2');
    var profileStars = document.getElementById('WantedProfileStarsV2');
    var profileReward = document.getElementById('WantedProfileRewardV2');
    var profileRoom = document.getElementById('WantedProfileRoomV2');
    var profileReason = document.getElementById('WantedProfileReasonV2');
    var profileInfo = document.getElementById('WantedProfileInfoV2');
    var tabButtons = windowEl ? windowEl.querySelectorAll('[data-wanted-tab]') : [];
    var tabPanels = windowEl ? windowEl.querySelectorAll('[data-wanted-panel]') : [];
    var dragHandle = windowEl ? windowEl.querySelector('[data-wanted-drag-handle]') : null;

    if (!windowEl || !cardsEl || !profileEl) {
        return;
    }

    var state = {
        suspects: [],
        selectedName: null,
        query: '',
        hasBeenPositioned: false,
        syncScheduled: false
    };

    function clamp(value, min, max) {
        return Math.min(Math.max(value, min), max);
    }

    function safeText(value, fallback) {
        var result = String(value == null ? '' : value).replace(/\s+/g, ' ').trim();
        return result || fallback || '';
    }

    function extractBackgroundUrl(value) {
        var raw = safeText(value, '');
        var match = raw.match(/^url\(["']?(.*?)["']?\)$/i);
        return match ? match[1] : '';
    }

    function safeImageUrl(value) {
        var raw = safeText(value, '');
        if (!raw) return '';

        try {
            var parsed = new URL(raw, document.baseURI);
            if (parsed.protocol !== 'http:' && parsed.protocol !== 'https:') {
                return '';
            }
            return parsed.href;
        } catch (error) {
            return '';
        }
    }

    function setBackgroundImage(element, url) {
        if (!element) return;
        var safeUrl = safeImageUrl(url);
        element.style.backgroundImage = safeUrl ? 'url("' + safeUrl.replace(/["\\]/g, '\\$&') + '")' : 'none';
    }

    function readStars(node) {
        if (!node) return 0;
        var text = safeText(node.textContent, '');
        var symbols = text.match(/[★⭐✪]/g);
        if (symbols && symbols.length) {
            return clamp(symbols.length, 0, 5);
        }

        var numeric = parseInt(text, 10);
        return Number.isFinite(numeric) ? clamp(numeric, 0, 5) : 0;
    }

    function findRecordRoot(usernameNode) {
        var current = usernameNode;
        while (current && current !== legacyList) {
            if (current.querySelector && current.querySelector('.figure-H_RWF_0') && current.querySelector('.bsn_s_hours')) {
                return current;
            }
            current = current.parentElement;
        }
        return usernameNode.parentElement || legacyList;
    }

    function parseLegacySuspects() {
        if (!legacyList) return [];

        var usernameNodes = legacyList.querySelectorAll('.bsn_s_country');
        var parsed = [];
        var names = new Set();

        usernameNodes.forEach(function (usernameNode) {
            var username = safeText(usernameNode.textContent, '');
            if (!username || username.indexOf('{') !== -1 || names.has(username.toLowerCase())) {
                return;
            }

            var root = findRecordRoot(usernameNode);
            var avatarNode = root && root.querySelector ? root.querySelector('.figure-H_RWF_0') : null;
            var starNode = root && root.querySelector ? root.querySelector('.bsn_s_hours') : null;
            var infoNodes = root && root.querySelectorAll ? root.querySelectorAll('.bsn_s_info') : [];
            var avatarUrl = '';

            if (avatarNode) {
                avatarUrl = extractBackgroundUrl(avatarNode.style.backgroundImage || window.getComputedStyle(avatarNode).backgroundImage);
            }

            if (!avatarUrl && root && root.querySelector) {
                var avatarImage = root.querySelector('img[src*="habbo"], img[src*="imager"], img[src*="avatar"]');
                if (avatarImage) avatarUrl = avatarImage.src;
            }

            var reason = infoNodes.length > 0 ? safeText(infoNodes[0].textContent, '') : '';
            var room = infoNodes.length > 1 ? safeText(infoNodes[1].textContent, '') : '';

            parsed.push({
                username: username,
                avatarUrl: safeImageUrl(avatarUrl),
                stars: readStars(starNode),
                reason: reason,
                room: room,
                alias: '',
                reward: '',
                info: ''
            });

            names.add(username.toLowerCase());
        });

        return parsed;
    }

    function createStars(stars, target) {
        target.textContent = '';
        for (var i = 1; i <= 5; i += 1) {
            var star = document.createElement('span');
            star.className = 'paradise-wanted-star' + (i <= stars ? ' is-active' : '');
            star.textContent = '★';
            target.appendChild(star);
        }
    }

    function getFilteredSuspects() {
        if (!state.query) return state.suspects.slice();
        var query = state.query.toLocaleLowerCase('fr-FR');
        return state.suspects.filter(function (suspect) {
            return suspect.username.toLocaleLowerCase('fr-FR').indexOf(query) !== -1 ||
                safeText(suspect.alias, '').toLocaleLowerCase('fr-FR').indexOf(query) !== -1;
        });
    }

    function setSelected(username) {
        state.selectedName = username;
        render();
    }

    function renderCard(suspect) {
        var card = document.createElement('button');
        card.type = 'button';
        card.className = 'paradise-wanted-card' + (state.selectedName === suspect.username ? ' is-selected' : '');
        card.setAttribute('aria-pressed', state.selectedName === suspect.username ? 'true' : 'false');
        card.title = 'Afficher la fiche de ' + suspect.username;

        var avatar = document.createElement('span');
        avatar.className = 'paradise-wanted-card-avatar';
        setBackgroundImage(avatar, suspect.avatarUrl);

        var copy = document.createElement('span');
        copy.className = 'paradise-wanted-card-copy';

        var name = document.createElement('strong');
        name.className = 'paradise-wanted-card-name';
        name.textContent = suspect.username;

        var stars = document.createElement('span');
        stars.className = 'paradise-wanted-stars';
        stars.setAttribute('aria-label', suspect.stars + ' étoile' + (suspect.stars > 1 ? 's' : '') + ' sur 5');
        createStars(suspect.stars, stars);

        var chevron = document.createElement('span');
        chevron.className = 'paradise-wanted-chevron';
        chevron.setAttribute('aria-hidden', 'true');
        chevron.textContent = '›';

        copy.appendChild(name);
        copy.appendChild(stars);
        card.appendChild(avatar);
        card.appendChild(copy);
        card.appendChild(chevron);

        card.addEventListener('click', function () {
            setSelected(suspect.username);
        });

        return card;
    }

    function renderEmptyList() {
        var empty = document.createElement('div');
        empty.className = 'paradise-wanted-empty-list';

        var title = document.createElement('strong');
        title.textContent = state.query ? 'Aucun suspect trouvé' : 'Aucune personne recherchée';

        var text = document.createElement('span');
        text.textContent = state.query ? 'Essayez un autre pseudo.' : 'La liste des personnes recherchées est vide.';

        empty.appendChild(title);
        empty.appendChild(text);
        cardsEl.appendChild(empty);
    }

    function renderList() {
        var filtered = getFilteredSuspects();
        cardsEl.textContent = '';

        if (!filtered.length) {
            renderEmptyList();
        } else {
            filtered.forEach(function (suspect) {
                cardsEl.appendChild(renderCard(suspect));
            });
        }

        var total = state.suspects.length;
        countEl.textContent = total + ' personne' + (total > 1 ? 's' : '') + ' dans la liste';
    }

    function renderProfile() {
        var suspect = state.suspects.find(function (item) {
            return item.username === state.selectedName;
        });

        if (!suspect) {
            emptyEl.hidden = false;
            profileEl.hidden = true;
            return;
        }

        emptyEl.hidden = true;
        profileEl.hidden = false;

        setBackgroundImage(profileAvatar, suspect.avatarUrl);
        profileName.textContent = suspect.username;
        profileAlias.textContent = safeText(suspect.alias, 'Aucun alias connu');
        profileReward.textContent = safeText(suspect.reward, '—');
        profileRoom.textContent = safeText(suspect.room, 'Inconnu');
        profileReason.textContent = safeText(suspect.reason, 'Aucun motif communiqué.');
        profileInfo.textContent = safeText(
            suspect.info,
            'Individu actuellement recherché par les autorités de Lake Placid.'
        );
        createStars(suspect.stars, profileStars);
        profileStars.setAttribute('aria-label', suspect.stars + ' étoile' + (suspect.stars > 1 ? 's' : '') + ' sur 5');
    }

    function render() {
        if (state.selectedName && !state.suspects.some(function (suspect) { return suspect.username === state.selectedName; })) {
            state.selectedName = null;
        }

        if (!state.selectedName && state.suspects.length) {
            state.selectedName = state.suspects[0].username;
        }

        renderList();
        renderProfile();
    }

    function syncFromLegacy() {
        state.syncScheduled = false;
        var parsed = parseLegacySuspects();

        if (parsed.length || !legacyList || !legacyList.children.length) {
            state.suspects = parsed;
            render();
        }
    }

    function scheduleLegacySync() {
        if (state.syncScheduled) return;
        state.syncScheduled = true;
        window.requestAnimationFrame(syncFromLegacy);
    }

    function centerWindow() {
        windowEl.style.left = '0px';
        windowEl.style.top = '0px';
        windowEl.style.display = 'block';

        var rect = windowEl.getBoundingClientRect();
        var left = Math.max(12, Math.round((window.innerWidth - rect.width) / 2));
        var top = Math.max(12, Math.round((window.innerHeight - rect.height) / 2));
        windowEl.style.left = left + 'px';
        windowEl.style.top = top + 'px';
        state.hasBeenPositioned = true;
    }

    function clampWindowToViewport() {
        if (windowEl.style.display === 'none') return;
        var rect = windowEl.getBoundingClientRect();
        var maxLeft = Math.max(12, window.innerWidth - rect.width - 12);
        var maxTop = Math.max(12, window.innerHeight - rect.height - 12);
        var left = clamp(parseFloat(windowEl.style.left) || rect.left, 12, maxLeft);
        var top = clamp(parseFloat(windowEl.style.top) || rect.top, 12, maxTop);
        windowEl.style.left = left + 'px';
        windowEl.style.top = top + 'px';
    }

    function openWindow() {
        if (legacyWindow) legacyWindow.style.display = 'none';
        windowEl.style.display = 'block';
        windowEl.setAttribute('aria-hidden', 'false');

        if (!state.hasBeenPositioned) {
            centerWindow();
        } else {
            clampWindowToViewport();
        }

        scheduleLegacySync();
    }

    function closeWindow() {
        windowEl.style.display = 'none';
        windowEl.setAttribute('aria-hidden', 'true');
        if (legacyWindow) legacyWindow.style.display = 'none';
    }

    function selectTab(tabName) {
        tabButtons.forEach(function (button) {
            var active = button.getAttribute('data-wanted-tab') === tabName;
            button.classList.toggle('is-active', active);
            button.setAttribute('aria-selected', active ? 'true' : 'false');
        });

        tabPanels.forEach(function (panel) {
            panel.classList.toggle('is-active', panel.getAttribute('data-wanted-panel') === tabName);
        });
    }

    tabButtons.forEach(function (button) {
        button.addEventListener('click', function () {
            selectTab(button.getAttribute('data-wanted-tab'));
        });
    });

    if (searchInput) {
        searchInput.addEventListener('input', function () {
            state.query = safeText(searchInput.value, '').slice(0, 32);
            renderList();
        });
    }

    if (closeButton) {
        closeButton.addEventListener('click', closeWindow);
    }

    if (openButton) {
        openButton.addEventListener('click', function () {
            window.setTimeout(openWindow, 0);
        });
    }

    if (legacyList && window.MutationObserver) {
        new MutationObserver(scheduleLegacySync).observe(legacyList, {
            childList: true,
            subtree: true,
            characterData: true,
            attributes: true,
            attributeFilter: ['style', 'class']
        });
    }

    if (legacyWindow && window.MutationObserver) {
        new MutationObserver(function () {
            if (legacyWindow.style.display && legacyWindow.style.display !== 'none') {
                window.setTimeout(openWindow, 0);
            }
        }).observe(legacyWindow, {
            attributes: true,
            attributeFilter: ['style', 'class']
        });
    }

    if (dragHandle) {
        dragHandle.addEventListener('mousedown', function (event) {
            if (event.button !== 0 || event.target.closest('button, input, a')) return;

            var rect = windowEl.getBoundingClientRect();
            var offsetX = event.clientX - rect.left;
            var offsetY = event.clientY - rect.top;

            function move(moveEvent) {
                var maxLeft = Math.max(12, window.innerWidth - windowEl.offsetWidth - 12);
                var maxTop = Math.max(12, window.innerHeight - windowEl.offsetHeight - 12);
                windowEl.style.left = clamp(moveEvent.clientX - offsetX, 12, maxLeft) + 'px';
                windowEl.style.top = clamp(moveEvent.clientY - offsetY, 12, maxTop) + 'px';
            }

            function stop() {
                document.removeEventListener('mousemove', move);
                document.removeEventListener('mouseup', stop);
            }

            document.addEventListener('mousemove', move);
            document.addEventListener('mouseup', stop);
            event.preventDefault();
        });
    }

    window.addEventListener('resize', clampWindowToViewport);

    syncFromLegacy();

    if (legacyWindow && window.getComputedStyle(legacyWindow).display !== 'none') {
        openWindow();
    }
})();
