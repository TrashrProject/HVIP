(() => {
    'use strict';

    const API = '/inventory.php';
    const ROOT = '.roleplay-inventory';
    let selectedSlot = null;
    let loading = false;

    const escapeHtml = value => String(value ?? '').replace(/[&<>'"]/g, char => ({
        '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;'
    })[char]);

    function sendCommand(command) {
        const input = document.querySelector('.nitro-chat-input-container .chat-input, input.chat-input');
        if (!input) {
            showStatus('Entre dans un appartement avant d’utiliser un objet.', true);
            return false;
        }
        const setter = Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, 'value')?.set;
        setter ? setter.call(input, command) : (input.value = command);
        input.dispatchEvent(new Event('input', { bubbles: true }));
        input.focus();
        setTimeout(() => input.dispatchEvent(new KeyboardEvent('keydown', {
            key: 'Enter', code: 'Enter', keyCode: 13, which: 13, bubbles: true
        })), 40);
        setTimeout(refresh, 850);
        setTimeout(refresh, 1800);
        return true;
    }

    function showStatus(message, error = false) {
        const node = document.querySelector(`${ROOT} .p-inventory-status`);
        if (!node) return;
        node.textContent = message;
        node.classList.toggle('is-error', error);
        node.hidden = false;
        clearTimeout(showStatus.timer);
        showStatus.timer = setTimeout(() => { node.hidden = true; }, 2600);
    }

    function defaultAction(item) {
        if (!item || item.is_broken) return;
        if (item.slot_index === 0) return sendCommand(':desequiper arme');
        if (item.slot_index === 1) return sendCommand(':desequiper armure');
        if (item.interaction_type === 'weapon' || item.interaction_type === 'shield') {
            return sendCommand(`:equiper ${item.display_name}`);
        }
        return sendCommand(`:utiliser ${item.display_name}`);
    }

    function parseWeaponStats(extraData) {
        const result = {};
        String(extraData || '').split(';').forEach(part => {
            const [key, value] = part.split('=');
            if (key && value !== undefined) result[key.trim()] = value.trim();
        });
        return result;
    }

    function itemMarkup(item) {
        if (!item) return '<span class="p-inventory-empty" aria-hidden="true"></span>';
        const quantity = item.quantity > 1 ? `<b class="p-inventory-quantity">${item.quantity}</b>` : '';
        const durability = ['weapon', 'shield'].includes(item.interaction_type)
            ? `<i class="p-inventory-durability"><span style="width:${Math.max(0, Math.min(100, item.durability))}%"></span></i>` : '';
        const skin = item.skin ? `<span class="p-inventory-skin-dot" title="Skin : ${escapeHtml(item.skin.name)}"></span>` : '';
        return `<img src="${escapeHtml(item.image_url)}" alt="" draggable="false">${quantity}${durability}${skin}`;
    }

    function slotMarkup(index, item) {
        const classes = [
            'p-inventory-slot',
            index < 2 ? 'is-equipped-slot' : '',
            item?.is_broken ? 'is-broken' : '',
            item?.skin ? 'has-skin' : ''
        ].filter(Boolean).join(' ');
        const label = item ? `${item.display_name} x${item.quantity}` : (index === 0 ? 'Arme équipée' : index === 1 ? 'Protection équipée' : 'Case vide');
        return `<button type="button" class="${classes}" data-inventory-slot="${index}" title="${escapeHtml(label)}" aria-label="${escapeHtml(label)}">${itemMarkup(item)}</button>`;
    }

    function render(data) {
        const root = document.querySelector(ROOT);
        if (!root) return;

        const title = root.querySelector('.inventory-header-text');
        if (title) title.textContent = 'Inventaire';

        const bySlot = new Map((data.slots || []).map(item => [Number(item.slot_index), item]));
        root.dataset.paradiseInventory = 'habbo-fr-v2';
        const content = root.querySelector('.inventory-content');
        if (!content) return;

        content.innerHTML = `
            <section class="p-inventory-zone p-inventory-equipped-zone">
                <div class="p-inventory-zone-title"><span>Équipement</span></div>
                <div class="p-inventory-grid p-inventory-equipped-grid" aria-label="Équipement">
                    ${[0, 1].map(index => slotMarkup(index, bySlot.get(index))).join('')}
                </div>
            </section>
            <section class="p-inventory-zone p-inventory-bag-zone">
                <div class="p-inventory-zone-title"><span>Sac à dos</span></div>
                <div class="p-inventory-grid p-inventory-bag-grid" aria-label="Sac à dos">
                    ${Array.from({ length: 10 }, (_, offset) => slotMarkup(offset + 2, bySlot.get(offset + 2))).join('')}
                </div>
            </section>
            <div class="p-inventory-actions" hidden>
                <div class="p-inventory-card-head">
                    <img class="p-inventory-preview" alt="">
                    <div><strong class="p-inventory-name"></strong><small class="p-inventory-type"></small></div>
                </div>
                <div class="p-inventory-meta"></div>
                <div class="p-inventory-buttons">
                    <button type="button" data-inventory-action="default"></button>
                    <button type="button" data-inventory-action="use">Utiliser</button>
                    <button type="button" data-inventory-action="skin">Skins</button>
                </div>
            </div>
            <div class="p-inventory-status" hidden></div>`;
        root._paradiseItems = bySlot;
    }

    async function refresh() {
        const root = document.querySelector(ROOT);
        if (!root || loading) return;
        loading = true;
        root.classList.add('is-loading');
        try {
            const response = await fetch(`${API}?t=${Date.now()}`, { credentials: 'same-origin', cache: 'no-store' });
            const data = await response.json();
            if (!response.ok || !data.ok) throw new Error(data.error || 'Chargement impossible');
            render(data);
        } catch (error) {
            showStatus(error.message || 'Inventaire indisponible.', true);
        } finally {
            loading = false;
            document.querySelector(ROOT)?.classList.remove('is-loading');
        }
    }

    function selectSlot(button) {
        const root = button.closest(ROOT);
        const index = Number(button.dataset.inventorySlot);
        const item = root?._paradiseItems?.get(index);
        root?.querySelectorAll('.p-inventory-slot.is-selected').forEach(node => node.classList.remove('is-selected'));
        if (!item) {
            root?.querySelector('.p-inventory-actions')?.setAttribute('hidden', '');
            selectedSlot = null;
            return;
        }

        selectedSlot = index;
        button.classList.add('is-selected');
        const actions = root.querySelector('.p-inventory-actions');
        const main = actions.querySelector('[data-inventory-action="default"]');
        const use = actions.querySelector('[data-inventory-action="use"]');
        const skin = actions.querySelector('[data-inventory-action="skin"]');
        const stats = parseWeaponStats(item.extra_data);

        actions.querySelector('.p-inventory-name').textContent = item.display_name;
        actions.querySelector('.p-inventory-type').textContent = item.interaction_type === 'weapon' ? 'Arme' : item.interaction_type === 'shield' ? 'Protection' : 'Objet';
        actions.querySelector('.p-inventory-preview').src = item.image_url;
        actions.querySelector('.p-inventory-meta').innerHTML = [
            item.quantity > 1 ? `<span>Quantité <b>${item.quantity}</b></span>` : '',
            ['weapon', 'shield'].includes(item.interaction_type) ? `<span>État <b>${item.durability}%</b></span>` : '',
            stats.damage ? `<span>Dégâts <b>${escapeHtml(stats.damage)}</b></span>` : '',
            stats.range ? `<span>Portée <b>${escapeHtml(stats.range)}</b></span>` : '',
            stats.magazine ? `<span>Chargeur <b>${escapeHtml(stats.magazine)}</b></span>` : '',
            item.skin ? `<span class="p-inventory-current-skin">Skin <b>${escapeHtml(item.skin.name)}</b></span>` : (item.weapon_key ? '<span>Skin <b>Par défaut</b></span>' : '')
        ].filter(Boolean).join('');

        main.textContent = index < 2 ? 'Ranger' : (['weapon', 'shield'].includes(item.interaction_type) ? 'Équiper' : 'Utiliser');
        use.hidden = !['shield'].includes(item.interaction_type) || index < 2;
        skin.hidden = !item.weapon_key;
        actions.hidden = false;
    }

    document.addEventListener('click', event => {
        const slot = event.target.closest(`${ROOT} [data-inventory-slot]`);
        if (slot) return selectSlot(slot);
        const action = event.target.closest(`${ROOT} [data-inventory-action]`);
        if (!action) return;
        const root = action.closest(ROOT);
        const item = root?._paradiseItems?.get(selectedSlot);
        if (!item) return;

        if (action.dataset.inventoryAction === 'skin') {
            if (window.ParadiseWeaponSkins?.open) window.ParadiseWeaponSkins.open(item.weapon_key);
            else showStatus('Le gestionnaire de skins est indisponible.', true);
            return;
        }
        if (action.dataset.inventoryAction === 'use') sendCommand(`:utiliser ${item.display_name}`);
        else defaultAction(item);
        root.querySelector('.p-inventory-actions').hidden = true;
    });

    document.addEventListener('dblclick', event => {
        const slot = event.target.closest(`${ROOT} [data-inventory-slot]`);
        if (!slot) return;
        defaultAction(slot.closest(ROOT)?._paradiseItems?.get(Number(slot.dataset.inventorySlot)));
    });

    const observer = new MutationObserver(() => {
        const root = document.querySelector(ROOT);
        if (root && root.dataset.paradiseInventory !== 'habbo-fr-v2') refresh();
    });
    observer.observe(document.documentElement, { childList: true, subtree: true });
    setInterval(() => { if (document.querySelector(ROOT)) refresh(); }, 5000);
    window.ParadiseInventory = { refresh };
})();
