(() => {
  'use strict';

  if (window.ParadiseInventoryV2) return;

  const VERSION = '3.0.1-inventory-v2';
  const HUD_ID = 'paradise-rp-hud';
  const API_URL = '../rp-inventory-data.php';
  const CHARACTER_ACTION_URL = '../rp-character-action.php';
  const POLL_MS = 15000;
  const FILTERS = [
    ['all', 'Tous'], ['object', 'Objets'], ['food', 'Nourriture'], ['document', 'Documents'], ['key', 'Clés'], ['misc', 'Divers']
  ];

  let hud = null;
  let inventoryWindow = null;
  let body = null;
  let unsubscribe = () => {};
  let timer = 0;
  let request = null;
  let destroyed = false;
  let giveMode = null;
  let giveQuantity = 1;
  let lastUiEventId = null;

  const esc = value => String(value ?? '')
    .replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;').replaceAll("'", '&#039;');
  const text = value => value === null || value === undefined || String(value).trim() === '' ? null : String(value).trim();
  const number = value => Number.isFinite(Number(value)) ? Number(value) : null;
  const state = () => window.ParadiseStore?.getState?.() || null;

  function categoryKey(item) {
    const category = String(item?.category || '').toUpperCase();
    if (category === 'FOOD') return 'food';
    if (category === 'DOCUMENT') return 'document';
    if (category === 'KEY') return 'key';
    if (category === 'MISC' || category === 'RESOURCE') return 'misc';
    return 'object';
  }

  function formatWeight(value) {
    const n = Math.max(0, number(value) ?? 0);
    return `${new Intl.NumberFormat('fr-FR', { maximumFractionDigits: 2 }).format(n)} kg`;
  }

  function fallbackLabel(item) {
    const code = String(item?.code || '').toUpperCase();
    const effect = String(item?.effectType || '').toUpperCase();
    if (effect === 'PHONE' || code.includes('PHONE')) return ['TEL', 'phone'];
    if (effect === 'DOCUMENT' || categoryKey(item) === 'document') return ['ID', 'document'];
    if (effect === 'KEY' || categoryKey(item) === 'key') return ['KEY', 'key'];
    if (categoryKey(item) === 'food') return [code.includes('WATER') ? 'H2O' : 'FOOD', 'food'];
    if (categoryKey(item) === 'misc') return ['RP', 'misc'];
    return ['OBJ', 'object'];
  }

  function art(item) {
    const icon = text(item?.icon);
    const [label, tone] = fallbackLabel(item);
    if (icon) {
      return `<span class="pr3-item-art"><img src="${esc(icon)}" alt="" draggable="false" data-pr3-icon><span class="pr3-pixel-icon" data-tone="${tone}" hidden>${label}</span></span>`;
    }
    return `<span class="pr3-item-art"><span class="pr3-pixel-icon" data-tone="${tone}">${label}</span></span>`;
  }

  function shell() {
    return `<div class="pr3-inventory" data-pr3-inventory>
      <div class="pr3-inventory-top">
        <div><strong>Objets physiques du personnage</strong><small>Persistants · validés serveur · distincts des furnis Habbo</small></div>
        <div class="pr3-capacity" data-pr3-capacity>
          <div class="pr3-capacity-row"><span>CAPACITÉ</span><b data-pr3-weight>0 / 50 kg</b></div>
          <span class="pr3-capacity-track"><i data-pr3-weight-bar></i></span>
          <small data-pr3-slots>0 / 30 slots</small>
        </div>
      </div>
      <div class="pr3-tabs" role="tablist">${FILTERS.map(([key, label]) => `<button type="button" class="pr3-filter" data-pr3-filter="${key}">${label}</button>`).join('')}</div>
      <div class="pr3-workspace">
        <section class="pr3-grid-shell" aria-label="Objets de l’inventaire">
          <div class="pr3-grid" data-pr3-grid></div>
          <div class="pr3-empty" data-pr3-empty hidden><div><strong>Votre inventaire est vide.</strong><p>Explorez Placid Island pour obtenir vos premiers objets.</p></div></div>
        </section>
        <aside class="pr3-detail" data-pr3-detail></aside>
      </div>
    </div>`;
  }

  function filteredItems() {
    const store = state();
    const items = store?.inventory?.items || [];
    const filter = text(store?.ui?.inventoryFilter) || 'all';
    return filter === 'all' ? items : items.filter(item => categoryKey(item) === filter);
  }

  function selectedItem() {
    const store = state();
    const key = text(store?.ui?.inventorySelected);
    return key ? (store?.inventory?.items || []).find(item => item.key === key) || null : null;
  }

  function actionLabel(action) {
    if (action === 'use') return 'Utiliser';
    if (action === 'give') return 'Donner';
    if (action === 'view') return 'Consulter';
    if (action === 'present') return 'Présenter';
    if (action === 'inspect') return 'Inspecter';
    return action;
  }

  function detailMarkup(item) {
    if (!item) return `<div class="pr3-detail-empty"><div><strong>Sélectionnez un objet</strong><p>Ses informations et ses vraies actions apparaîtront ici.</p></div></div>`;
    const qty = Math.max(1, item.quantity || 1);
    const status = text(item.status);
    const facts = [
      ['Quantité', qty],
      ['Poids', item.source === 'document' ? 'Système' : formatWeight(item.weight)],
      ['Catégorie', categoryLabel(item)],
      ['État', status || (item.locked ? 'Lié' : 'Disponible')]
    ];
    // DROP remains deliberately absent until a stable ground-item representation
    // exists in the actual Nitro room. No fake action is ever rendered.
    const actions = (item.actions || []).filter(action => action !== 'drop');
    return `<div class="pr3-selected">
      <div class="pr3-selected-head"><div class="pr3-selected-art">${art(item)}</div><div class="pr3-selected-title"><h3>${esc(item.name)}</h3><small>${esc(item.code || categoryLabel(item))}</small></div></div>
      <div class="pr3-description">${esc(item.description || 'Aucune description.')}</div>
      <div class="pr3-facts">${facts.map(([label, value]) => `<div class="pr3-fact"><span>${esc(label)}</span><b>${esc(value)}</b></div>`).join('')}</div>
      <div class="pr3-actions">${actions.length ? actions.map(action => `<button type="button" class="pr3-button ${action === 'use' || action === 'view' ? 'is-primary' : action === 'present' ? 'is-gold' : ''}" data-pr3-action="${esc(action)}">${esc(actionLabel(action))}</button>`).join('') : '<small>Aucune action disponible.</small>'}</div>
      <div class="pr3-give-box" data-pr3-give-box hidden>
        <label><span data-pr3-give-label>Joueur destinataire</span><input type="text" maxlength="32" autocomplete="off" placeholder="Pseudo du joueur" data-pr3-target></label>
        <div class="pr3-give-controls"><div><small>Quantité</small><div class="pr3-quantity"><button type="button" class="pr3-button" data-pr3-qty="-1">−</button><output data-pr3-qty-value>1</output><button type="button" class="pr3-button" data-pr3-qty="1">+</button></div></div></div>
        <div class="pr3-give-foot"><button type="button" class="pr3-button" data-pr3-action="cancel-give">Annuler</button><button type="button" class="pr3-button is-primary" data-pr3-action="confirm-give">Confirmer</button></div>
      </div>
    </div>`;
  }

  function categoryLabel(item) {
    const key = categoryKey(item);
    return ({ food: 'Nourriture', document: 'Document', key: 'Clé', misc: 'Divers', object: 'Objet' })[key] || 'Objet';
  }

  function render() {
    if (!body || destroyed) return;
    const store = state();
    if (!store) return;
    const inv = store.inventory || {};
    const items = filteredItems();
    let selected = selectedItem();
    if (!selected && items.length) {
      selected = items[0];
      window.ParadiseStore?.setInventoryUi?.({ selected: selected.key });
    }

    body.querySelectorAll('[data-pr3-filter]').forEach(button => button.classList.toggle('is-active', button.dataset.pr3Filter === (store.ui.inventoryFilter || 'all')));
    const grid = body.querySelector('[data-pr3-grid]');
    const empty = body.querySelector('[data-pr3-empty]');
    if (grid) {
      grid.innerHTML = items.map(item => `<button type="button" class="pr3-item${item.key === selected?.key ? ' is-selected' : ''}${item.locked ? ' is-locked' : ''}" data-pr3-item="${esc(item.key)}" title="${esc(item.name)} · ${esc(formatWeight(item.weight))} · x${Math.max(1,item.quantity || 1)}">
        ${art(item)}${item.quantity > 1 ? `<span class="pr3-qty">${item.quantity}</span>` : '<span class="pr3-qty" hidden>1</span>'}${item.usable ? '<i class="pr3-item-usable" aria-label="Utilisable"></i>' : ''}
      </button>`).join('');
      grid.querySelectorAll('img[data-pr3-icon]').forEach(img => img.addEventListener('error', () => {
        img.hidden = true;
        const fallback = img.nextElementSibling;
        if (fallback) fallback.hidden = false;
      }, { once: true }));
    }
    if (empty) empty.hidden = items.length > 0;

    const detail = body.querySelector('[data-pr3-detail]');
    if (detail) detail.innerHTML = detailMarkup(selected);
    giveMode = null;
    giveQuantity = 1;

    const weight = Math.max(0, Number(inv.weight) || 0);
    const capacity = Math.max(0.001, Number(inv.capacity) || 50);
    const ratio = Math.max(0, Math.min(100, (weight / capacity) * 100));
    const capacityNode = body.querySelector('[data-pr3-capacity]');
    if (capacityNode) capacityNode.dataset.level = ratio >= 98 ? 'full' : ratio >= 80 ? 'warn' : 'ok';
    const weightNode = body.querySelector('[data-pr3-weight]');
    if (weightNode) weightNode.textContent = `${formatWeight(weight).replace(' kg','')} / ${formatWeight(capacity)}`;
    const bar = body.querySelector('[data-pr3-weight-bar]');
    if (bar) bar.style.width = `${ratio}%`;
    const slots = body.querySelector('[data-pr3-slots]');
    if (slots) slots.textContent = `${inv.slotsUsed || 0} / ${inv.maxSlots || 30} slots`;
  }

  async function refresh() {
    if (destroyed || request || !window.ParadiseStore) return request || false;
    request = (async () => {
      try {
        const response = await fetch(`${API_URL}?_=${Date.now()}`, { cache:'no-store', credentials:'same-origin', headers:{ Accept:'application/json' } });
        const payload = await response.json();
        if (!response.ok || payload?.ok === false) {
          window.ParadiseStore.applyInventorySnapshot(payload || { ok:false, reason:`HTTP ${response.status}` });
          return false;
        }
        return window.ParadiseStore.applyInventorySnapshot(payload);
      } catch (error) {
        console.warn('[ParadiseRP:inventory] refresh failed', error);
        window.ParadiseStore.applyInventorySnapshot({ ok:false, reason:error?.message || 'inventory_request_failed' });
        return false;
      } finally {
        request = null;
      }
    })();
    return request;
  }

  async function sendCommand(command, refreshAfter = true) {
    const adapter = window.__ParadiseNativeChatAdapter;
    if (!adapter?.send) {
      toast('Inventaire', 'Le chat Nitro n’est pas encore prêt.');
      return false;
    }
    const ok = await adapter.send(command);
    if (refreshAfter) {
      window.setTimeout(refresh, 250);
      window.setTimeout(refresh, 900);
    }
    return ok;
  }

  function toast(title, message, duration = 3200) {
    if (!hud) return;
    let host = hud.querySelector('.pr3-toast-host');
    if (!host) {
      host = document.createElement('div');
      host.className = 'pr3-toast-host';
      hud.appendChild(host);
    }
    const node = document.createElement('div');
    node.className = 'pr3-toast';
    node.innerHTML = `<strong>${esc(title || 'Inventaire')}</strong><span>${esc(message || '')}</span>`;
    host.appendChild(node);
    window.setTimeout(() => node.remove(), duration);
  }

  function openInventory() {
    window.ParadiseWindowManager?.openWindow?.('inventory');
    refresh();
  }

  function startGive(mode) {
    const item = selectedItem();
    if (!item) return;
    giveMode = mode;
    giveQuantity = 1;
    const box = body?.querySelector('[data-pr3-give-box]');
    if (!box) return;
    box.hidden = false;
    const label = box.querySelector('[data-pr3-give-label]');
    if (label) label.textContent = mode === 'present' ? 'Présenter à' : 'Donner à';
    const qtyRow = box.querySelector('.pr3-give-controls');
    if (qtyRow) qtyRow.hidden = mode === 'present';
    box.querySelector('[data-pr3-target]')?.focus({ preventScroll:true });
  }

  function updateGiveQuantity(delta) {
    const item = selectedItem();
    if (!item) return;
    giveQuantity = Math.max(1, Math.min(Math.max(1, item.quantity || 1), giveQuantity + delta));
    const output = body?.querySelector('[data-pr3-qty-value]');
    if (output) output.textContent = String(giveQuantity);
  }

  async function confirmGive() {
    const item = selectedItem();
    const target = text(body?.querySelector('[data-pr3-target]')?.value);
    if (!item || !target) {
      toast('Inventaire', 'Indiquez le joueur destinataire.');
      return;
    }

    if (giveMode === 'present' && item.source === 'document') {
      const command = item.documentType === 'DRIVER_LICENSE'
        ? `:inv presentlicense ${target}`
        : `:inv presentid ${target}`;
      await sendCommand(command, false);
      body.querySelector('[data-pr3-give-box]').hidden = true;
      return;
    }

    if (giveMode === 'give' && item.id) {
      await sendCommand(`:inv giveid ${target} ${item.id} ${giveQuantity}`);
      body.querySelector('[data-pr3-give-box]').hidden = true;
    }
  }

  function onClick(event) {
    const target = event.target;
    if (!(target instanceof Element) || !body?.contains(target)) return;

    const filter = target.closest('[data-pr3-filter]');
    if (filter) {
      window.ParadiseStore?.setInventoryUi?.({ filter: filter.dataset.pr3Filter, selected: null });
      render();
      return;
    }

    const itemButton = target.closest('[data-pr3-item]');
    if (itemButton) {
      window.ParadiseStore?.setInventoryUi?.({ selected: itemButton.dataset.pr3Item });
      render();
      return;
    }

    const qty = target.closest('[data-pr3-qty]');
    if (qty) {
      updateGiveQuantity(Number(qty.dataset.pr3Qty) || 0);
      return;
    }

    const action = target.closest('[data-pr3-action]');
    if (!action) return;
    const item = selectedItem();
    switch (action.dataset.pr3Action) {
      case 'use':
        if (!item?.id) return;
        // The server validates ownership/use first; PHONE_OPEN arrives only after that validation.
        sendCommand(`:inv useid ${item.id}`);
        break;
      case 'give': startGive('give'); break;
      case 'view':
        if (item?.source === 'document') window.ParadiseCharacterV2?.openDocuments?.(item.documentType || null);
        break;
      case 'present': startGive('present'); break;
      case 'inspect': toast(item?.name || 'Objet', item?.metadata || 'Aucune information supplémentaire.'); break;
      case 'cancel-give':
        giveMode = null;
        body.querySelector('[data-pr3-give-box]').hidden = true;
        break;
      case 'confirm-give': confirmGive(); break;
    }
  }

  function rewriteChatCommand(event) {
    if (event.key !== 'Enter' && event.keyCode !== 13) return;
    const input = event.target;
    if (!(input instanceof HTMLInputElement) || input.id !== 'pr4-chat-input') return;
    const raw = String(input.value || '').trim();
    if (!raw.startsWith(':')) return;
    const lower = raw.toLowerCase();

    // Compatibility aliases are translated to the already-existing :inv command,
    // avoiding a collision with the legacy staff :give command in CommandManager.
    if (lower === ':inventory') input.value = ':inv';
    else if (lower === ':weight') input.value = ':inv weight';
    else if (lower === ':poids') input.value = ':inv weight';
    else if (/^:use\s+/i.test(raw)) input.value = ':inv use ' + raw.replace(/^:use\s+/i, '');
    else if (/^:giveitem\s+/i.test(raw)) input.value = ':inv give ' + raw.replace(/^:giveitem\s+/i, '');

    if (lower === ':inventory' || lower === ':inv' || lower === ':inventario') window.setTimeout(openInventory, 0);
    if (/^:(inventory|inv|inventario|weight|poids|use|giveitem)\b/i.test(raw)) {
      window.setTimeout(refresh, 450);
      window.setTimeout(refresh, 1250);
    }
  }

  async function consumeUiEvent(id) {
    try {
      await fetch(CHARACTER_ACTION_URL, {
        method:'POST', credentials:'same-origin', cache:'no-store',
        headers:{ 'Content-Type':'application/json', Accept:'application/json', 'X-Paradise-Action':'phase3' },
        body:JSON.stringify({ action:'consume_ui_event', event_id:id })
      });
    } catch (_) {}
    window.ParadiseStore?.clearUiEvent?.(id);
  }

  function onUiEvent(eventData) {
    const id = number(eventData?.id);
    if (id === null || id === lastUiEventId) return;
    const type = text(eventData?.type);
    if (!['INVENTORY_OPEN','INVENTORY_TOAST','PHONE_OPEN'].includes(type)) return;
    lastUiEventId = id;
    if (type === 'INVENTORY_OPEN') openInventory();
    if (type === 'PHONE_OPEN') window.ParadiseWindowManager?.openWindow?.('phone');
    if (type === 'INVENTORY_TOAST') toast(eventData?.payload?.title || 'Inventaire', eventData?.payload?.message || 'Mise à jour.');
    consumeUiEvent(id);
  }

  function syncFromStore(store, eventName) {
    if (eventName === 'inventory:update' || eventName === 'ui:change') render();
    if (eventName === 'ui:event' && store?.meta?.pendingUiEvent) onUiEvent(store.meta.pendingUiEvent);
  }

  function mount() {
    hud = document.getElementById(HUD_ID);
    inventoryWindow = hud?.querySelector('.pr-window[data-window="inventory"]');
    body = inventoryWindow?.querySelector('.pr-window-body');
    if (!hud || !inventoryWindow || !body) return false;
    body.innerHTML = shell();
    const subtitle = inventoryWindow.querySelector('.pr-window-title small');
    if (subtitle) subtitle.textContent = 'Objets physiques · Placid Island';
    body.addEventListener('click', onClick, false);
    hud.addEventListener('keydown', rewriteChatCommand, true);
    unsubscribe = window.ParadiseStore?.subscribe?.(syncFromStore) || (() => {});
    render();
    return true;
  }

  function boot() {
    if (destroyed || !window.ParadiseStore || !mount()) return;
    refresh();
    timer = window.setInterval(refresh, POLL_MS);
    const pending = state()?.meta?.pendingUiEvent;
    if (pending) onUiEvent(pending);
    console.info('[ParadiseRP] Inventory V2 active', { version: VERSION, endpoint: API_URL });
  }

  function destroy() {
    if (destroyed) return;
    destroyed = true;
    window.clearInterval(timer);
    unsubscribe();
    body?.removeEventListener('click', onClick, false);
    hud?.removeEventListener('keydown', rewriteChatCommand, true);
  }

  window.ParadiseInventoryV2 = Object.freeze({
    version: VERSION,
    open: openInventory,
    refresh,
    getItems: () => (state()?.inventory?.items || []).slice(),
    getState: () => state()?.inventory || null,
    use: id => sendCommand(`:inv useid ${Number(id)}`),
    give: (username, id, quantity = 1) => sendCommand(`:inv giveid ${String(username || '').trim()} ${Number(id)} ${Math.max(1, Number(quantity) || 1)}`),
    destroy
  });

  window.addEventListener('beforeunload', destroy, { once:true });
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once:true });
  else boot();
})();
