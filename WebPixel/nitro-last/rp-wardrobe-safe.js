(() => {
  const S = { access:null, accessLoaded:false, outfits:[], categories:[], active:'all', panel:null, tab:null, mode:'outfits', avatar:null };
  const esc = v => String(v ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
  const txt = el => (el && el.textContent || '').trim();

  async function json(url, options={}) {
    const r = await fetch(url, { credentials:'same-origin', cache:'no-store', ...options });
    const d = await r.json().catch(() => ({}));
    if (!r.ok || !d || d.ok === false) throw new Error(d.error || `HTTP ${r.status}`);
    return d;
  }

  function imager(figure, headOnly=false, size='l') {
    const p = new URLSearchParams({ figure:String(figure||''), size, direction:'2', head_direction:'2', gesture:'std', action:'std' });
    if (headOnly) p.set('headonly','1');
    return '/avatar-image.php?' + p.toString();
  }

  async function loadAccess() {
    if (S.accessLoaded) return S.access;
    try { S.access = await json('/rp-outfit-access.php?v=' + Date.now()); }
    catch { S.access = { allowed:false }; }
    S.accessLoaded = true;
    return S.access;
  }

  async function loadCatalog() {
    const d = await json('/rp-authorized-outfits.php?v=' + Date.now());
    S.outfits = Array.isArray(d.outfits) ? d.outfits : [];
    S.categories = Array.isArray(d.categories) ? d.categories : [];
  }

  async function loadAvatar() {
    S.avatar = await json('/rp-avatar-editor.php?v=' + Date.now());
    return S.avatar;
  }

  function closePanel() {
    if (S.panel) S.panel.remove();
    S.panel = null;
    S.mode = 'outfits';
    if (S.tab) S.tab.classList.remove('pr-rp-active');
  }

  function setMode(mode) {
    S.mode = mode;
    if (!S.panel) return;
    S.panel.querySelectorAll('.pr-rp-mode-btn').forEach(b => b.classList.toggle('is-active', b.dataset.mode === mode));
    const outfit = S.panel.querySelector('.pr-rp-outfit-area');
    const editor = S.panel.querySelector('.pr-rp-editor-area');
    outfit.hidden = mode !== 'outfits';
    editor.hidden = mode !== 'editor';
    if (mode === 'editor') renderEditor(editor);
  }

  function renderOutfits(grid) {
    const list = S.active === 'all' ? S.outfits : S.outfits.filter(x => x.category === S.active);
    grid.innerHTML = '';
    if (!list.length) {
      grid.innerHTML = '<div class="pr-rp-empty">Aucune tenue disponible pour ce métier et ce grade.</div>';
      return;
    }
    list.forEach(outfit => {
      const card = document.createElement('article');
      card.className = 'pr-rp-card-safe';
      card.innerHTML = `
        <div class="pr-rp-preview">
          <div class="pr-rp-preview-loading">Chargement…</div>
          <img src="${esc(imager(outfit.figure,false,'l'))}" alt="${esc(outfit.name || 'Tenue RP')}" loading="lazy">
          <div class="pr-rp-preview-fallback">Aperçu indisponible</div>
        </div>
        <div class="pr-rp-card-copy">
          <strong>${esc(outfit.name || 'Tenue RP')}</strong>
          <span>${esc(outfit.categoryLabel || 'ParadiseRP')}</span>
          <small>${esc(outfit.source || '')}</small>
        </div>
        <button class="pr-rp-equip" type="button">Équiper</button>`;
      const img = card.querySelector('img');
      img.addEventListener('load', () => card.classList.add('is-preview-ready'));
      img.addEventListener('error', () => card.classList.add('is-preview-error'));
      card.querySelector('.pr-rp-equip').addEventListener('click', () => equip(outfit, card));
      grid.appendChild(card);
    });
  }

  async function equip(outfit, card) {
    const btn = card.querySelector('.pr-rp-equip');
    if (!btn || btn.disabled) return;
    const old = btn.textContent;
    btn.disabled = true; btn.textContent = 'Application…';
    try {
      const d = await json('/rp-outfit-apply.php', { method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify({id:outfit.id}) });
      btn.textContent = 'Équipé ✓'; card.classList.add('pr-rp-equipped');
      const note = S.panel?.querySelector('.pr-rp-note');
      if (note) note.textContent = `${d.name || outfit.name || 'Tenue'} équipée. Actualisation du personnage…`;
      setTimeout(() => { try { window.top.location.reload(); } catch { location.reload(); } }, 650);
    } catch (e) {
      btn.disabled = false; btn.textContent = old;
      const note = S.panel?.querySelector('.pr-rp-note');
      if (note) note.textContent = 'Erreur : ' + (e.message || 'impossible d’équiper');
    }
  }

  function swatch(c, active, onClick, title) {
    const b = document.createElement('button');
    b.type = 'button'; b.className = 'pr-rp-swatch'; b.style.background = '#' + c.hex; b.title = title || `#${c.id}`;
    b.classList.toggle('is-active', Number(c.id) === Number(active));
    b.addEventListener('click', () => onClick(Number(c.id), b));
    return b;
  }

  async function renderEditor(container) {
    container.hidden = false;
    container.innerHTML = '<div class="pr-rp-loading">Chargement du studio personnage…</div>';
    try {
      const d = await loadAvatar();
      const cur = d.current || {};
      const hairs = Array.isArray(d.hair_sets) ? d.hair_sets : [];
      const hairColors = Array.isArray(d.hair_colors) ? d.hair_colors : [];
      const skinColors = Array.isArray(d.skin_colors) ? d.skin_colors : [];
      let hairSet = Number(cur.hair_set || hairs[0]?.id || 0);
      let hairColor = Number(cur.hair_color || hairColors[0]?.id || 0);
      let skinColor = Number(cur.skin_color || skinColors[0]?.id || 0);
      let filtered = hairs;

      container.innerHTML = `
        <div class="pr-rp-studio">
          <aside class="pr-rp-studio-preview">
            <div class="pr-rp-live-badge"><i></i>Aperçu exact Nitro</div>
            <div class="pr-rp-live-stage"><img class="pr-rp-live-img" alt="Aperçu personnage"></div>
            <h3>Ton personnage</h3>
            <p>La tenue reste intacte. Seuls les cheveux et le teint sont modifiés.</p>
          </aside>
          <main class="pr-rp-studio-main">
            <div class="pr-rp-studio-title"><div><h2>Personnalisation</h2><p>Choisis visuellement ta coupe avant de l’appliquer.</p></div><span>${hairs.length} coupes</span></div>
            <div class="pr-rp-search-row"><label>Coiffure</label><input class="pr-rp-hair-search" type="search" placeholder="Rechercher une coupe…"></div>
            <div class="pr-rp-hair-grid"></div>
            <div class="pr-rp-editor-group"><b>Couleur des cheveux</b><div class="pr-rp-swatches pr-rp-hair-colors"></div></div>
            <div class="pr-rp-editor-group"><b>Teint de peau</b><div class="pr-rp-swatches pr-rp-skin-colors"></div></div>
            <div class="pr-rp-save-row"><button type="button" class="pr-rp-avatar-save">Appliquer la personnalisation</button><div class="pr-rp-editor-status"></div></div>
          </main>
        </div>`;

      const live = container.querySelector('.pr-rp-live-img');
      const grid = container.querySelector('.pr-rp-hair-grid');
      const hairBox = container.querySelector('.pr-rp-hair-colors');
      const skinBox = container.querySelector('.pr-rp-skin-colors');
      const search = container.querySelector('.pr-rp-hair-search');

      const buildLook = () => {
        let look = String(d.look || '');
        const replace = (type,set,color) => {
          const part = `${type}-${set}-${color}`;
          const re = new RegExp(`(^|\\.)${type}-\\d+(?:-\\d+)*`,'i');
          look = re.test(look) ? look.replace(re,(m,sep)=>(sep==='.'?'.':'')+part) : (look ? look+'.'+part : part);
        };
        const hm = look.match(/(?:^|\.)hd-(\d+)(?:-\d+)?/i);
        replace('hr', hairSet, hairColor);
        replace('hd', hm ? Number(hm[1]) : 180, skinColor);
        return look;
      };
      const refreshLive = () => { live.src = imager(buildLook(), false, 'l') + '&_=' + Date.now(); };

      const renderHairGrid = () => {
        grid.innerHTML = '';
        if (!filtered.length) { grid.innerHTML = '<div class="pr-rp-empty">Aucune coupe trouvée.</div>'; return; }
        filtered.forEach((h, i) => {
          const card = document.createElement('button');
          card.type = 'button'; card.className = 'pr-rp-hair-card'; card.dataset.id = h.id;
          card.classList.toggle('is-active', Number(h.id) === hairSet);
          card.innerHTML = `<div class="pr-rp-hair-thumb"><span>Chargement…</span><img src="${esc(imager(h.preview_figure || d.look,true,'n'))}" alt="Style ${i+1}" loading="lazy"></div><strong>Style ${i+1}</strong><small>Coupe ${h.id}</small><i class="pr-rp-selected-check">✓</i>`;
          const img = card.querySelector('img');
          img.addEventListener('load',()=>card.classList.add('is-ready'));
          img.addEventListener('error',()=>card.classList.add('is-error'));
          card.addEventListener('click',()=>{ hairSet=Number(h.id); renderHairGrid(); refreshLive(); });
          grid.appendChild(card);
        });
      };

      hairColors.forEach(c => hairBox.appendChild(swatch(c,hairColor,(id,b)=>{hairColor=id;hairBox.querySelectorAll('.pr-rp-swatch').forEach(x=>x.classList.toggle('is-active',x===b));renderHairGrid();refreshLive();},`Couleur cheveux ${c.id}`)));
      skinColors.forEach(c => skinBox.appendChild(swatch(c,skinColor,(id,b)=>{skinColor=id;skinBox.querySelectorAll('.pr-rp-swatch').forEach(x=>x.classList.toggle('is-active',x===b));refreshLive();},`Teint ${c.id}`)));
      search.addEventListener('input',()=>{
        const q = search.value.trim().toLowerCase();
        filtered = !q ? hairs : hairs.filter((h,i)=>String(h.id).includes(q) || `style ${i+1}`.includes(q));
        renderHairGrid();
      });
      renderHairGrid(); refreshLive();

      container.querySelector('.pr-rp-avatar-save').addEventListener('click', async e => {
        const btn=e.currentTarget, status=container.querySelector('.pr-rp-editor-status');
        btn.disabled=true; btn.textContent='Application…'; status.textContent='';
        try {
          await json('/rp-avatar-editor.php',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({hair_set:hairSet,hair_color:hairColor,skin_color:skinColor})});
          btn.textContent='Enregistré ✓'; status.textContent='Personnalisation enregistrée.';
          setTimeout(()=>{try{window.top.location.reload();}catch{location.reload();}},650);
        } catch(err) { btn.disabled=false; btn.textContent='Appliquer la personnalisation'; status.textContent='Erreur : '+(err.message||'échec'); }
      });
    } catch(e) {
      container.innerHTML = `<div class="pr-rp-empty">Éditeur indisponible : ${esc(e.message || 'erreur')}</div>`;
    }
  }

  async function openPanel() {
    if (S.panel) return closePanel();
    const access = await loadAccess();
    if (!access?.allowed) return;
    if (S.tab) S.tab.classList.add('pr-rp-active');
    const jobs = Array.isArray(access.jobs) ? access.jobs.map(x=>x.name).filter(Boolean) : [];
    const context = jobs.length ? jobs.join(' · ') : (access.staff?.name || 'ParadiseRP');
    const panel = document.createElement('section');
    panel.id='paradise-rp-wardrobe-panel';
    panel.innerHTML=`
      <header class="pr-rp-panel-head"><div><small>PARADISERP • PERSONNAGE</small><b>Vestiaire RP</b><span>${esc(context)}</span></div><button class="pr-rp-close" type="button">×</button></header>
      <nav class="pr-rp-modebar"><button class="pr-rp-mode-btn is-active" data-mode="outfits">👔 Tenues métier</button><button class="pr-rp-mode-btn" data-mode="editor">✂️ Cheveux & teint</button></nav>
      <section class="pr-rp-outfit-area"><div class="pr-rp-filters-safe"><button data-cat="all" class="is-active">Toutes mes tenues</button></div><div class="pr-rp-grid-safe"><div class="pr-rp-loading">Chargement…</div></div></section>
      <section class="pr-rp-editor-area" hidden></section>
      <footer class="pr-rp-note">Aperçus rendus avec les mêmes assets Nitro que le client.</footer>`;
    document.body.appendChild(panel); S.panel=panel;
    panel.querySelector('.pr-rp-close').addEventListener('click',closePanel);
    panel.querySelectorAll('.pr-rp-mode-btn').forEach(b=>b.addEventListener('click',()=>setMode(b.dataset.mode)));
    const grid=panel.querySelector('.pr-rp-grid-safe'), filters=panel.querySelector('.pr-rp-filters-safe');
    try {
      await loadCatalog();
      S.categories.forEach(c=>{const b=document.createElement('button');b.type='button';b.dataset.cat=c.id;b.textContent=`${c.icon||'💼'} ${c.label||c.id} (${c.count||0})`;filters.appendChild(b);});
      filters.addEventListener('click',e=>{const b=e.target.closest('button[data-cat]');if(!b)return;S.active=b.dataset.cat||'all';filters.querySelectorAll('button').forEach(x=>x.classList.toggle('is-active',x===b));renderOutfits(grid);});
      renderOutfits(grid);
    } catch(e) { grid.innerHTML='<div class="pr-rp-empty">Impossible de charger les tenues.</div>'; panel.querySelector('.pr-rp-note').textContent='Erreur : '+(e.message||'catalogue indisponible'); }
  }

  function findWardrobeTab() {
    return Array.from(document.querySelectorAll('button,[role="tab"],div,span')).find(el=>{
      const t=txt(el).toLowerCase(); if(t!=='armario'&&t!=='wardrobe')return false;
      const r=el.getBoundingClientRect(); return r.width>20&&r.height>15&&r.top>=0&&r.left>=0;
    }) || null;
  }

  async function installTab() {
    if (S.tab && document.contains(S.tab)) return;
    const access=await loadAccess();
    if(!access?.allowed){ if(S.tab?.isConnected)S.tab.remove(); S.tab=null; return; }
    const wardrobe=findWardrobeTab(); if(!wardrobe||!wardrobe.parentElement)return;
    const tab=document.createElement(wardrobe.tagName.toLowerCase()==='button'?'button':'div');
    tab.className=(wardrobe.className||'')+' pr-rp-tab-safe';
    tab.textContent='Tenues RP'; tab.type=tab.tagName==='BUTTON'?'button':undefined;
    tab.addEventListener('click',e=>{e.preventDefault();e.stopPropagation();openPanel();});
    wardrobe.parentElement.appendChild(tab); S.tab=tab;
  }

  const obs=new MutationObserver(()=>installTab());
  obs.observe(document.documentElement,{childList:true,subtree:true});
  setInterval(installTab,1000);
  if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',installTab);else installTab();
})();
