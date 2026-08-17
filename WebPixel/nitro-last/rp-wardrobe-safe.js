(() => {
  const STATE = { loaded: false, outfits: [], categories: [], active: 'all', panel: null, tab: null, accessLoaded: false, access: null, mode: 'outfits', avatar: null };

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

  async function loadAvatarEditor() {
    const response = await fetch('/rp-avatar-editor.php?v=' + Date.now(), { cache: 'no-store', credentials: 'same-origin' });
    const data = await response.json().catch(() => ({}));
    if (!response.ok || !data.ok) throw new Error(data.error || 'Éditeur avatar indisponible');
    STATE.avatar = data;
    return data;
  }

  function previewUrl(figure) {
    return 'https://www.habbo.com/habbo-imaging/avatarimage?figure=' + encodeURIComponent(figure || '') + '&size=l&direction=2&head_direction=2&gesture=sml&action=std';
  }

  function replacePart(look, type, setId, colorId) {
    const part = `${type}-${setId}-${colorId}`;
    const re = new RegExp(`(^|\\.)${type}-\\d+(?:-\\d+)*`, 'i');
    if (re.test(look || '')) return String(look || '').replace(re, (m, sep) => (sep === '.' ? '.' : '') + part);
    return look ? `${look}.${part}` : part;
  }

  function closePanel() {
    if (!STATE.panel) return;
    STATE.panel.remove();
    STATE.panel = null;
    STATE.mode = 'outfits';
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

  function setTopMode(mode) {
    STATE.mode = mode;
    if (!STATE.panel) return;
    STATE.panel.querySelectorAll('.pr-rp-mode-btn').forEach(b => b.classList.toggle('is-active', b.dataset.mode === mode));
    const outfitArea = STATE.panel.querySelector('.pr-rp-outfit-area');
    const editorArea = STATE.panel.querySelector('.pr-rp-editor-area');
    if (outfitArea) outfitArea.hidden = mode !== 'outfits';
    if (editorArea) editorArea.hidden = mode !== 'editor';
    if (mode === 'editor') renderAvatarEditor(editorArea);
  }

  async function renderAvatarEditor(container) {
    if (!container) return;
    container.hidden = false;
    container.innerHTML = '<div class="pr-rp-loading">Chargement de l’éditeur avatar...</div>';
    try {
      const data = await loadAvatarEditor();
      const cur = data.current || {};
      const hairSets = Array.isArray(data.hair_sets) ? data.hair_sets : [];
      const hairColors = Array.isArray(data.hair_colors) ? data.hair_colors : [];
      const skinColors = Array.isArray(data.skin_colors) ? data.skin_colors : [];
      container.innerHTML = `
        <div class="pr-rp-editor-layout">
          <div class="pr-rp-editor-preview">
            <img class="pr-rp-editor-img" src="${esc(previewUrl(data.look || ''))}" alt="Aperçu avatar">
            <strong>Personnalisation RP</strong>
            <small>La tenue métier reste intacte : seuls les cheveux et le teint sont modifiés.</small>
          </div>
          <div class="pr-rp-editor-controls">
            <label>Coupe de cheveux
              <select class="pr-rp-hair-select">
                ${hairSets.map(h => `<option value="${h.id}"${Number(h.id) === Number(cur.hair_set) ? ' selected' : ''}>Coupe #${h.id}</option>`).join('')}
              </select>
            </label>
            <div class="pr-rp-editor-group"><b>Couleur des cheveux</b><div class="pr-rp-swatches pr-rp-hair-colors"></div></div>
            <div class="pr-rp-editor-group"><b>Teint de peau</b><div class="pr-rp-swatches pr-rp-skin-colors"></div></div>
            <button type="button" class="pr-rp-avatar-save">Appliquer la personnalisation</button>
            <div class="pr-rp-editor-status"></div>
          </div>
        </div>`;

      const hairSelect = container.querySelector('.pr-rp-hair-select');
      const hairBox = container.querySelector('.pr-rp-hair-colors');
      const skinBox = container.querySelector('.pr-rp-skin-colors');
      const img = container.querySelector('.pr-rp-editor-img');
      let hairColor = Number(cur.hair_color || (hairColors[0] && hairColors[0].id) || 0);
      let skinColor = Number(cur.skin_color || (skinColors[0] && skinColors[0].id) || 0);

      const updatePreview = () => {
        let look = String(data.look || '');
        const hairSet = Number(hairSelect.value || cur.hair_set || 0);
        const headMatch = look.match(/(?:^|\.)hd-(\d+)(?:-\d+)?/i);
        const headSet = headMatch ? Number(headMatch[1]) : 180;
        if (hairSet && hairColor) look = replacePart(look, 'hr', hairSet, hairColor);
        if (skinColor) look = replacePart(look, 'hd', headSet, skinColor);
        img.src = previewUrl(look);
      };

      hairColors.forEach(c => {
        const b = document.createElement('button');
        b.type = 'button'; b.className = 'pr-rp-swatch'; b.title = `Couleur #${c.id}`; b.style.background = '#' + c.hex; b.dataset.id = c.id;
        b.classList.toggle('is-active', Number(c.id) === hairColor);
        b.addEventListener('click', () => { hairColor = Number(c.id); hairBox.querySelectorAll('.pr-rp-swatch').forEach(x => x.classList.toggle('is-active', x === b)); updatePreview(); });
        hairBox.appendChild(b);
      });
      skinColors.forEach(c => {
        const b = document.createElement('button');
        b.type = 'button'; b.className = 'pr-rp-swatch'; b.title = `Teint #${c.id}`; b.style.background = '#' + c.hex; b.dataset.id = c.id;
        b.classList.toggle('is-active', Number(c.id) === skinColor);
        b.addEventListener('click', () => { skinColor = Number(c.id); skinBox.querySelectorAll('.pr-rp-swatch').forEach(x => x.classList.toggle('is-active', x === b)); updatePreview(); });
        skinBox.appendChild(b);
      });
      hairSelect.addEventListener('change', updatePreview);

      container.querySelector('.pr-rp-avatar-save').addEventListener('click', async event => {
        const button = event.currentTarget;
        const status = container.querySelector('.pr-rp-editor-status');
        button.disabled = true; button.textContent = 'Application...'; status.textContent = '';
        try {
          const response = await fetch('/rp-avatar-editor.php', { method:'POST', credentials:'same-origin', headers:{'Content-Type':'application/json'}, body:JSON.stringify({ hair_set:Number(hairSelect.value), hair_color:hairColor, skin_color:skinColor }) });
          const saved = await response.json().catch(() => ({}));
          if (!response.ok || !saved.ok) throw new Error(saved.error || 'Impossible de sauvegarder');
          status.textContent = 'Personnalisation enregistrée ✓';
          button.textContent = 'Enregistré ✓';
          setTimeout(() => { try { window.top.location.reload(); } catch (_) { window.location.reload(); } }, 650);
        } catch (error) {
          button.disabled = false; button.textContent = 'Appliquer la personnalisation';
          status.textContent = 'Erreur : ' + (error && error.message ? error.message : 'échec');
        }
      });
    } catch (error) {
      container.innerHTML = '<div class="pr-rp-empty">Éditeur avatar indisponible : ' + esc(error && error.message ? error.message : 'erreur') + '</div>';
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
      <div class="pr-rp-modebar"><button type="button" class="pr-rp-mode-btn is-active" data-mode="outfits">👔 Tenues métier</button><button type="button" class="pr-rp-mode-btn" data-mode="editor">✂️ Cheveux & teint</button></div>
      <div class="pr-rp-outfit-area">
        <div class="pr-rp-filters-safe"><button type="button" data-cat="all" class="is-active">Toutes mes tenues</button></div>
        <div class="pr-rp-grid-safe"><div class="pr-rp-loading">Chargement des tenues autorisées...</div></div>
      </div>
      <div class="pr-rp-editor-area" hidden></div>
      <div class="pr-rp-note">Tenues filtrées selon le métier et le grade. L’éditeur cheveux/teint ne modifie pas les vêtements.</div>`;
    document.body.appendChild(panel);
    STATE.panel = panel;
    panel.querySelector('.pr-rp-close').addEventListener('click', closePanel);
    panel.querySelectorAll('.pr-rp-mode-btn').forEach(b => b.addEventListener('click', () => setTopMode(b.dataset.mode)));

    const grid = panel.querySelector('.pr-rp-grid-safe');
    const filters = panel.querySelector('.pr-rp-filters-safe');
    try {
      await loadCatalog();
      STATE.categories.forEach(category => {
        const b = document.createElement('button');
        b.type = 'button'; b.dataset.cat = category.id;
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
