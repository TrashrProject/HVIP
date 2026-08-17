(() => {
  const STATE = { loaded: false, outfits: [], categories: [], active: 'all', panel: null, tab: null };

  const textOf = el => (el && (el.textContent || '') || '').trim();
  const esc = value => String(value || '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));

  async function loadCatalog() {
    if (STATE.loaded) return;
    const response = await fetch('./rp-outfits.json?v=' + Date.now(), { cache: 'no-store' });
    if (!response.ok) throw new Error('HTTP ' + response.status);
    const data = await response.json();
    STATE.outfits = Array.isArray(data.outfits) ? data.outfits : [];
    STATE.categories = Array.isArray(data.categories) ? data.categories : [];
    STATE.loaded = true;
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
      container.innerHTML = '<div class="pr-rp-empty">Aucune tenue détectée dans cette catégorie.</div>';
      return;
    }
    list.forEach(outfit => {
      const card = document.createElement('article');
      card.className = 'pr-rp-card-safe';
      card.innerHTML = `
        <div class="pr-rp-card-icon">${esc(outfit.icon || '★')}</div>
        <div class="pr-rp-card-copy">
          <strong>${esc(outfit.name || 'Tenue RP')}</strong>
          <span>${esc(outfit.categoryLabel || outfit.category || 'Roleplay')}</span>
          <small>${esc(outfit.source || (outfit.gender === 'F' ? 'Femme' : 'Homme'))}</small>
        </div>
        <button type="button" class="pr-rp-equip">Équiper</button>`;
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
        method: 'POST',
        credentials: 'same-origin',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id: outfit.id })
      });
      const data = await response.json().catch(() => ({}));
      if (!response.ok || !data.ok) throw new Error(data.error || 'Erreur serveur');
      button.textContent = 'Équipé ✓';
      card.classList.add('pr-rp-equipped');
      const note = STATE.panel && STATE.panel.querySelector('.pr-rp-note');
      if (note) note.textContent = `${data.name || 'Tenue RP'} équipée. Reconnexion au client...`;
      setTimeout(() => {
        try { window.top.location.reload(); }
        catch (_) { window.location.reload(); }
      }, 650);
    } catch (error) {
      button.disabled = false;
      button.textContent = old;
      const note = STATE.panel && STATE.panel.querySelector('.pr-rp-note');
      if (note) note.textContent = 'Erreur : ' + (error && error.message ? error.message : 'impossible d’équiper la tenue');
    }
  }

  async function openPanel() {
    if (STATE.panel) return closePanel();
    if (STATE.tab) STATE.tab.classList.add('pr-rp-active');

    const panel = document.createElement('section');
    panel.id = 'paradise-rp-wardrobe-panel';
    panel.innerHTML = `
      <div class="pr-rp-panel-head">
        <div><b>Tenues RP</b><span>Paradise Roleplay</span></div>
        <button type="button" class="pr-rp-close" aria-label="Fermer">×</button>
      </div>
      <div class="pr-rp-filters-safe"><button type="button" data-cat="all" class="is-active">Tous</button></div>
      <div class="pr-rp-grid-safe"><div class="pr-rp-loading">Chargement des tenues...</div></div>
      <div class="pr-rp-note">Sélectionne une tenue. Le client se reconnectera automatiquement après l’équipement.</div>`;
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
      grid.innerHTML = '<div class="pr-rp-empty">Les tenues RP ne sont pas encore générées sur le serveur.</div>';
      panel.querySelector('.pr-rp-note').textContent = 'Relance le générateur de tenues RP sur le VPS.';
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

  function installTab() {
    if (STATE.tab && document.contains(STATE.tab)) return;
    const wardrobe = findWardrobeTab();
    if (!wardrobe || !wardrobe.parentElement) return;

    const tab = document.createElement(wardrobe.tagName.toLowerCase() === 'button' ? 'button' : 'div');
    if (tab.tagName === 'BUTTON') tab.type = 'button';
    tab.className = `${wardrobe.className || ''} pr-rp-tab-safe`;
    tab.textContent = 'Tenues RP';
    tab.setAttribute('data-paradise-rp-tab', '1');
    tab.addEventListener('click', event => {
      event.preventDefault();
      event.stopPropagation();
      openPanel();
    });
    wardrobe.insertAdjacentElement('afterend', tab);
    STATE.tab = tab;
  }

  const observer = new MutationObserver(() => installTab());
  observer.observe(document.documentElement, { childList: true, subtree: true });
  setInterval(installTab, 900);
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', installTab);
  else installTab();
})();
