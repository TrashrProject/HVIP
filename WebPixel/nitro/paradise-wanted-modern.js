(() => {
  'use strict';

  const ALERT_SELECTOR = '.nitro-alert.nitro-alert-wanted';
  let scheduled = false;

  const normalize = value => String(value ?? '').replace(/\s+/g, ' ').trim();

  function countStars(text) {
    const matches = normalize(text).match(/[⭐★✪]/g);
    return Math.max(0, Math.min(5, matches ? matches.length : 0));
  }

  function extractName(box) {
    const primary = box.querySelector('.text-primary, [class*="text-primary"], strong, b');
    if (primary) {
      const value = normalize(primary.textContent).replace(/[⭐★✪]/g, '').trim();
      if (value) return value;
    }

    const clone = box.cloneNode(true);
    clone.querySelectorAll('.wanted-avatar, .avatar-image').forEach(node => node.remove());
    return normalize(clone.textContent).replace(/[⭐★✪]/g, '').trim() || 'Inconnu';
  }

  function readSuspects(alert) {
    return [...alert.querySelectorAll('.wanted-box')].map((box, index) => ({
      id: index,
      name: extractName(box),
      stars: countStars(box.textContent),
      avatar: box.querySelector('.wanted-avatar')?.cloneNode(true) || null
    })).filter(user => user.name);
  }

  function starsMarkup(stars, showEmpty = false) {
    let html = '';
    for (let i = 1; i <= 5; i++) {
      if (i <= stars) html += '<span>★</span>';
      else if (showEmpty) html += '<span class="off">★</span>';
    }
    return html || '<span class="off">★</span>';
  }

  function findNativeClose(alert) {
    const candidates = [...alert.querySelectorAll('button, [class*="close"], [class*="Close"]')];
    return candidates.find(node => !node.closest('.paradise-wanted-shell')) || null;
  }

  function createShell(alert) {
    const shell = document.createElement('div');
    shell.className = 'paradise-wanted-shell';
    shell.innerHTML = `
      <div class="paradise-wanted-topbar">
        <div class="paradise-wanted-brand">
          <div class="paradise-wanted-logo">P</div>
          <div class="paradise-wanted-brand-copy"><strong>ParadiseRP</strong><span>LAKE PLACID</span></div>
        </div>
        <div class="paradise-wanted-heading"><strong>Personnes recherchées</strong><span>Pour une ville plus sûre</span></div>
        <button class="paradise-wanted-close" type="button" aria-label="Fermer">×</button>
      </div>
      <div class="paradise-wanted-body">
        <aside class="paradise-wanted-sidebar">
          <div class="paradise-wanted-tabs">
            <button class="paradise-wanted-tab is-active" type="button" data-pr-tab="list">LISTE</button>
            <button class="paradise-wanted-tab" type="button" data-pr-tab="info">INFOS</button>
          </div>
          <section class="paradise-wanted-panel" data-pr-panel="list">
            <div class="paradise-wanted-search-wrap"><input class="paradise-wanted-search" type="search" placeholder="Rechercher un suspect..." autocomplete="off"></div>
            <div class="paradise-wanted-list"></div>
            <div class="paradise-wanted-count"></div>
          </section>
          <section class="paradise-wanted-panel" data-pr-panel="info" hidden>
            <div class="paradise-wanted-info-panel"><strong>Registre des personnes recherchées</strong>Cette interface affiche les données réellement transmises par le serveur. Les informations non disponibles ne sont pas inventées.</div>
          </section>
        </aside>
        <main class="paradise-wanted-detail"></main>
      </div>`;
    alert.appendChild(shell);
    return shell;
  }

  function avatarWrap(user, className) {
    const wrap = document.createElement('div');
    wrap.className = className;
    if (user.avatar) wrap.appendChild(user.avatar.cloneNode(true));
    return wrap;
  }

  function renderDetail(shell, user) {
    const detail = shell.querySelector('.paradise-wanted-detail');
    detail.innerHTML = '';

    if (!user) {
      detail.innerHTML = '<div class="paradise-wanted-empty">Aucune personne recherchée actuellement.</div>';
      return;
    }

    const head = document.createElement('div');
    head.className = 'paradise-wanted-detail-head';
    head.appendChild(avatarWrap(user, 'paradise-wanted-profile-avatar'));

    const main = document.createElement('div');
    main.className = 'paradise-wanted-profile-main';
    main.innerHTML = `
      <div class="paradise-wanted-profile-name"></div>
      <div class="paradise-wanted-status">RECHERCHÉ</div>
      <div class="paradise-wanted-two-col">
        <div><div class="paradise-wanted-label">Alias</div><div class="paradise-wanted-value">Aucun alias communiqué</div></div>
        <div><div class="paradise-wanted-label">Niveau de danger</div><div class="paradise-wanted-danger-stars">${starsMarkup(user.stars, true)}</div></div>
      </div>`;
    main.querySelector('.paradise-wanted-profile-name').textContent = user.name;
    head.appendChild(main);
    detail.appendChild(head);

    const facts = document.createElement('div');
    facts.className = 'paradise-wanted-facts';
    facts.innerHTML = `
      <div class="paradise-wanted-fact"><div class="paradise-wanted-fact-icon">$</div><div><div class="paradise-wanted-label">Récompense</div><div class="paradise-wanted-value">Non communiquée</div></div></div>
      <div class="paradise-wanted-fact"><div class="paradise-wanted-fact-icon">⌖</div><div><div class="paradise-wanted-label">Dernier secteur connu</div><div class="paradise-wanted-value">Non transmis par le serveur</div></div></div>
      <div class="paradise-wanted-fact is-wide"><div class="paradise-wanted-fact-icon">≡</div><div><div class="paradise-wanted-label">Motif</div><div class="paradise-wanted-value">Personne activement recherchée par les autorités</div></div></div>
      <div class="paradise-wanted-fact is-wide"><div class="paradise-wanted-fact-icon">!</div><div><div class="paradise-wanted-label">Informations</div><div class="paradise-wanted-value">Le packet Wanted actuel transmet le pseudo, l'apparence et le niveau de recherche. Les informations complémentaires seront affichées ici dès qu'elles seront fournies par le serveur.</div></div></div>`;
    detail.appendChild(facts);

    const warning = document.createElement('div');
    warning.className = 'paradise-wanted-warning';
    warning.innerHTML = '<div class="paradise-wanted-warning-icon">!</div><div><strong>NE TENTEZ PAS D’INTERPELLER CET INDIVIDU VOUS-MÊME.</strong><span>Contactez les forces de l’ordre de ParadiseRP.</span></div>';
    detail.appendChild(warning);
  }

  function bindTabs(shell) {
    shell.querySelectorAll('[data-pr-tab]').forEach(button => {
      button.addEventListener('click', () => {
        const target = button.dataset.prTab;
        shell.querySelectorAll('[data-pr-tab]').forEach(tab => tab.classList.toggle('is-active', tab === button));
        shell.querySelectorAll('[data-pr-panel]').forEach(panel => {
          panel.hidden = panel.dataset.prPanel !== target;
        });
      });
    });
  }

  function renderList(alert, shell) {
    const users = readSuspects(alert);
    const list = shell.querySelector('.paradise-wanted-list');
    const count = shell.querySelector('.paradise-wanted-count');
    const search = shell.querySelector('.paradise-wanted-search');
    const previousSelection = shell.dataset.selectedName || '';
    const query = normalize(search.value).toLowerCase();
    const filtered = users.filter(user => !query || user.name.toLowerCase().includes(query));

    list.innerHTML = '';
    if (!filtered.length) {
      list.innerHTML = '<div class="paradise-wanted-empty">Aucun résultat.</div>';
    }

    let selected = users.find(user => user.name === previousSelection) || users[0] || null;

    filtered.forEach(user => {
      const card = document.createElement('button');
      card.type = 'button';
      card.className = 'pr-wanted-card';
      if (selected && selected.name === user.name) card.classList.add('is-selected');
      card.appendChild(avatarWrap(user, 'pr-wanted-avatar'));

      const meta = document.createElement('div');
      meta.className = 'pr-wanted-meta';
      const name = document.createElement('div');
      name.className = 'pr-wanted-name';
      name.textContent = user.name;
      const stars = document.createElement('div');
      stars.className = 'pr-wanted-stars';
      stars.innerHTML = starsMarkup(user.stars, false);
      meta.append(name, stars);
      card.appendChild(meta);

      const arrow = document.createElement('div');
      arrow.className = 'pr-wanted-arrow';
      arrow.textContent = '›';
      card.appendChild(arrow);

      card.addEventListener('click', () => {
        shell.dataset.selectedName = user.name;
        renderList(alert, shell);
        renderDetail(shell, user);
      });
      list.appendChild(card);
    });

    count.textContent = `${users.length} personne${users.length > 1 ? 's' : ''} dans la liste`;
    shell.dataset.selectedName = selected ? selected.name : '';
    renderDetail(shell, selected);
  }

  function enhance(alert) {
    if (!alert || alert.dataset.paradiseWantedEnhancing === '1') return;
    alert.dataset.paradiseWantedEnhancing = '1';

    let shell = alert.querySelector(':scope > .paradise-wanted-shell');
    if (!shell) {
      const nativeClose = findNativeClose(alert);
      shell = createShell(alert);
      alert.classList.add('paradise-wanted-enhanced');
      bindTabs(shell);

      shell.querySelector('.paradise-wanted-close').addEventListener('click', () => {
        if (nativeClose && typeof nativeClose.click === 'function') nativeClose.click();
        else alert.style.display = 'none';
      });

      shell.querySelector('.paradise-wanted-search').addEventListener('input', () => renderList(alert, shell));
    }

    renderList(alert, shell);
    alert.dataset.paradiseWantedEnhancing = '0';
  }

  function scan() {
    scheduled = false;
    document.querySelectorAll(ALERT_SELECTOR).forEach(enhance);
  }

  function scheduleScan() {
    if (scheduled) return;
    scheduled = true;
    requestAnimationFrame(scan);
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', scheduleScan, { once: true });
  else scheduleScan();

  new MutationObserver(scheduleScan).observe(document.documentElement, { childList: true, subtree: true });
})();
