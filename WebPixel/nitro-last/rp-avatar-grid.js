(() => {
  const state = { installed: false, data: null, loading: false };
  const esc = value => String(value || '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
  const imageUrl = figure => 'https://www.habbo.com/habbo-imaging/avatarimage?figure=' + encodeURIComponent(figure || '') + '&size=l&direction=2&head_direction=2&gesture=sml&action=std';

  async function getData() {
    if (state.data) return state.data;
    if (state.loading) {
      while (!state.data && state.loading) await new Promise(r => setTimeout(r, 60));
      return state.data;
    }
    state.loading = true;
    try {
      const response = await fetch('/rp-avatar-editor.php?v=' + Date.now(), { cache:'no-store', credentials:'same-origin' });
      const data = await response.json().catch(() => ({}));
      if (!response.ok || !data.ok) throw new Error(data.error || 'Éditeur avatar indisponible');
      state.data = data;
      return data;
    } finally { state.loading = false; }
  }

  function syncActive(grid, selected) {
    grid.querySelectorAll('.pr-rp-hair-card').forEach(card => card.classList.toggle('is-active', Number(card.dataset.hairId) === Number(selected)));
  }

  async function install() {
    const area = document.querySelector('.pr-rp-editor-area:not([hidden])');
    if (!area) return;
    const select = area.querySelector('.pr-rp-hair-select');
    if (!select || area.querySelector('.pr-rp-hair-visual-block')) return;

    let data;
    try { data = await getData(); } catch (e) { return; }
    const hairs = Array.isArray(data.hair_sets) ? data.hair_sets : [];
    if (!hairs.length) return;

    const label = select.closest('label');
    if (label) label.style.display = 'none';

    const block = document.createElement('section');
    block.className = 'pr-rp-hair-visual-block';
    block.innerHTML = `
      <div class="pr-rp-hair-visual-head">
        <div><b>Coupes de cheveux</b><span>${hairs.length} coupes disponibles</span></div>
        <input type="search" class="pr-rp-hair-search" placeholder="Rechercher une coupe…" aria-label="Rechercher une coupe">
      </div>
      <div class="pr-rp-hair-grid"></div>`;

    const controls = area.querySelector('.pr-rp-editor-controls');
    if (controls) controls.insertBefore(block, controls.firstChild); else area.appendChild(block);
    const grid = block.querySelector('.pr-rp-hair-grid');
    const search = block.querySelector('.pr-rp-hair-search');

    const draw = filter => {
      const term = String(filter || '').trim().toLowerCase();
      grid.innerHTML = '';
      hairs.filter(h => !term || String(h.id).includes(term)).forEach(h => {
        const card = document.createElement('button');
        card.type = 'button';
        card.className = 'pr-rp-hair-card';
        card.dataset.hairId = h.id;
        card.innerHTML = `<img src="${esc(imageUrl(h.preview_figure || data.look || ''))}" alt="Coupe ${esc(h.id)}" loading="lazy"><span>Coupe #${esc(h.id)}</span>`;
        card.addEventListener('click', () => {
          select.value = String(h.id);
          select.dispatchEvent(new Event('change', { bubbles:true }));
          syncActive(grid, h.id);
        });
        grid.appendChild(card);
      });
      syncActive(grid, select.value || (data.current && data.current.hair_set));
    };

    search.addEventListener('input', () => draw(search.value));
    draw('');
  }

  new MutationObserver(install).observe(document.documentElement, { childList:true, subtree:true, attributes:true, attributeFilter:['hidden'] });
  setInterval(install, 700);
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', install); else install();
})();
