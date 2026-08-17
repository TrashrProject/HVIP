(() => {
  const STATE = { loaded: false, outfits: [], categories: [], active: 'all', panel: null, tab: null, accessLoaded: false, access: null };

  const textOf = el => (el && (el.textContent || '') || '').trim();
  const esc = value => String(value || '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));

  async function loadAccess() {
    if (STATE.accessLoaded) return STATE.access;
    const response = await fetch('/rp-outfit-access.php?v=' + Date.now(), { cache: 'no-store', credentials: 'same-origin' });
    const data = await response.json().catch(() => ({}));
    STATE.access = response.ok && data && data.ok ? data : { allowed: false };
    STATE.accessLoaded = true;
    return STATE.access;
  }

  async function loadCatalog() {
    const response = await fetch('/rp-authorized-outfits.php?v=' + Date.now(), { cache: 'no-store', credentials: 'same-origin' });
    const data = await response.json().catch(() => ({}));
    if (!response.ok || !data.ok) throw new Error(data.error || ('HTTP ' + response.status));
    STATE.outfits = Array.isArray(data.outfits) ? data.outfits : [];
    STATE.categories = Array.isArray(data.categories) ? data.categories : [];
    STATE.loaded = true;
  }

  function previewUrl(figure) {
    return 'https://www.habbo.com/habbo-imaging/avatarimage?figure=' + encodeURIComponent(figure || '') + '&size=l&direction=2&head_direction=2&gesture=sml&action=std';
  }

  function closePanel() {
    if (!STATE.panel) return;
    STATE.panel.remove();
    STATE.panel = null;
    if (STATE.tab) STATE.tab.classList.remove('pr-rp-active');
  }

  function renderCards(container) {
    const list = STATE.active === 'all' ? STATE.outfits : STATE.outfits.filter(o => o.category === STATE.active);
    container.innerHTML = '';
    if (!list.length) {
      container.innerHTML = '<div class="pr-rp-empty">Aucune tenue disponible pour ton métier et ton grade.</div>';
      return;
    }

    list.forEach(outfit => {
      const card = document.createElement('article');
      card.className = 'pr-rp-card-safe';
      const image = previewUrl(outfit.figure);
      card.innerHTML = `
        <div class="pr-rp-preview">
          <img src="${esc(image)}" alt="${esc(outfit.name || 'Tenue RP')}" loading="lazy">
          <div class="pr-rp-preview-fallback">${esc(outfit.icon || '★')}</div>
        </div>
        <div class="pr-rp-card-copy">
          <strong>${esc(outfit.name || 'Tenue RP')}</strong>
          <span>${esc(outfit.categoryLabel || 'ParadiseRP')}</span>
          <small>${esc(outfit.source || '')}</small>
        </div>
        <button type="button" class="pr-rp-equip">Équiper</button>`;

      const img = card.querySelector('.pr-rp-preview img');
      if (img) img.addEventListener('error', () => card.classList.add('pr-rp-preview-error'));
      card.querySelector('.pr-rp-equip').addEventListener('click', () => equip(outfit, card));
      container.appendChild(card);
    });
  }

  async function equip(outfit, card) {
    const button = card.querySelector('.pr-rp-equip');
    if (!button || button.disabled) return;
    button.disabled = true;
    const old = button.textContent;
    button.textContent = 'Application...';
    try {
      const response = await fetch('/rp-outfit-apply.php', {
        method: 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ id: outfit.id })
      });
      const data = await response.json().catch(() => ({}));
      if (!response.ok || !data.ok) throw new Error(data.error || 'Erreur serveur');
      button.textContent = 'Équipé ✓';
      card.classList.add('pr-rp-equipped');
      const note = STATE.panel && STATE.panel.querySelector('.pr-rp-note');
      if (note) note.textContent = `${data.name || 'Tenue RP'} équipée. Reconnexion au client...`;
      setTimeout(() => { try { window.top.location.reload(); } catch (_) { window.location.reload(); } }, 650);
    } catch (error) {
      button.disabled = false;
      button.textContent = old;
      const note = STATE.panel && STATE.panel.querySelector('.pr-rp-note');
      if (note) note.textContent = 'Erreur : ' + (error && error.message ? error.message : 'impossible d’équiper la tenue');
    }
  }

  async function openPanel() {
    if (STATE.panel) return closePanel();
    const access = await loadAccess();
    if (!access || !access.allowed) return;
    if (STATE.tab) STATE.tab.classList.add('pr-rp-active');

    const panel = document.createElement('section');
    panel.id = 'paradise-rp-wardrobe-panel';
    const jobs = Array.isArray(access.jobs) ? access.jobs.map(j => j.name).filter(Boolean) : [];
    const staffName = access.staff && access.staff.name ? access.staff.name : '';
    const context = jobs.length ? jobs.join(' · ') : (staffName || 'ParadiseRP');
    panel.innerHTML = `
      <div class="pr-rp-panel-head">
        <div><b>Tenues RP</b><span>${esc(context)}</span></div>
        <button type="button" class="pr-rp-close" aria-label="Fermer">×</button>
      </div>
      <div class="pr-rp-filters-safe"><button type="button" data-cat="all" class="is-active">Toutes mes tenues</button></div>
      <div class="pr-rp-grid-safe"><div class="pr-rp-loading">Chargement des tenues autorisées...</div></div>
      <div class="pr-rp-note">Seules les tenues correspondant à tes métiers, ton grade ou ton rôle staff sont affichées.</div>`;
    document.body.appendChild(panel);
    STATE.panel = panel;
    panel.querySelector('.pr-rp-close').addEventListener('click', closePanel);

    const grid = panel.querySelector('.pr-rp-grid-safe');
    const filters = panel.querySelector('.pr-rp-filters-safe');
    try {
      await loadCatalog();
      STATE.categories.forEach(category => {
        const b = document.createElement('button');
        b.type = 'button';
        b.dataset.cat = category.id;
        b.textContent = `${category.icon || ''} ${category.label || category.id} (${category.count || 0})`.trim();
        filters.appendChild(b);
      });
      filters.addEventListener('click', event => {
        const b = event.target.closest('button[data-cat]');
        if (!b) return;
        STATE.active = b.dataset.cat || 'all';
        filters.querySelectorAll('button').forEach(x => x.classList.toggle('is-active', x === b));
        renderCards(grid);
      });
      renderCards(grid);
    } catch (error) {
      grid.innerHTML = '<div class="pr-rp-empty">Impossible de charger tes tenues autorisées.</div>';
      panel.querySelector('.pr-rp-note').textContent = 'Erreur : ' + (error && error.message ? error.message : 'catalogue indisponible');
    }
  }

  function findWardrobeTab() {
    const nodes = Array.from(document.querySelectorAll('button,[role="tab"],div,span'));
    return nodes.find(el => {
      const t = textOf(el).toLowerCase();
      if (t !== 'armario' && t !== 'wardrobe') return false;
      const rect = el.getBoundingClientRect();
      return rect.width > 20 && rect.height > 15 && rect.top >= 0 && rect.left >= 0;
    }) || null;
  }

  async function installTab() {
    if (STATE.tab && document.contains(STATE.tab)) return;
    const access = await loadAccess();
    if (!access || !access.allowed) {
      if (STATE.tab && document.contains(STATE.tab)) STATE.tab.remove();
      STATE.tab = null;
      return;
    }
    const wardrobe = findWardrobeTab();
    if (!wardrobe || !wardrobe.parentElement) return;
    const tab = document.createElement(wardrobe.tagName.toLowerCase() === 'button' ? 'button' : 'div');
    if (tab.tagName === 'BUTTON') tab.type = 'button';
    tab.className = `${wardrobe.className || ''} pr-rp-tab-safe`;
    tab.textContent = access.label || 'Tenues RP';
    tab.setAttribute('data-paradise-rp-tab', '1');
    tab.addEventListener('click', event => { event.preventDefault(); event.stopPropagation(); openPanel(); });
    wardrobe.insertAdjacentElement('afterend', tab);
    STATE.tab = tab;
  }

  const observer = new MutationObserver(() => installTab());
  observer.observe(document.documentElement, { childList: true, subtree: true });
  setInterval(installTab, 900);
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', installTab); else installTab();
})();
