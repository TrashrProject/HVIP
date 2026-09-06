(() => {
  'use strict';

  const ALERT_SELECTOR = '.nitro-alert.nitro-alert-wanted';
  let scheduled = false;

  const normalize = value => String(value ?? '').replace(/\s+/g, ' ').trim();

  function countStars(text) {
    const matches = normalize(text).match(/[⭐★✪]/g);
    return Math.max(0, Math.min(5, matches ? matches.length : 0));
  }

  function dangerLabel(stars) {
    if (stars <= 0) return 'Non évalué';
    if (stars === 1) return 'Faible';
    if (stars === 2) return 'Modéré';
    if (stars === 3) return 'Élevé';
    if (stars === 4) return 'Très élevé';
    return 'Critique';
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

  function starsMarkup(stars) {
    let html = '';
    for (let i = 1; i <= 5; i++) {
      html += i <= stars ? '<span>★</span>' : '<span class="off">★</span>';
    }
    return html;
  }

  function findNativeClose(alert) {
    const candidates = [...alert.querySelectorAll('button, [class*="close"], [class*="Close"]')];
    return candidates.find(node => !node.closest('.paradise-wanted-shell')) || null;
  }

  function closeWanted(alert, nativeClose) {
    if (nativeClose && nativeClose.isConnected && typeof nativeClose.click === 'function') {
      nativeClose.click();
    }

    window.setTimeout(() => {
      if (!alert.isConnected) return;
      alert.style.setProperty('display', 'none', 'important');
      alert.setAttribute('aria-hidden', 'true');
    }, 0);
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
        <div class="paradise-wanted-motto">SIGNALER <span>•</span> ENQUÊTER <span>•</span> PROTÉGER</div>
        <button class="paradise-wanted-close" type="button" aria-label="Fermer">×</button>
      </div>
      <div class="paradise-wanted-body">
        <aside class="paradise-wanted-sidebar">
          <div class="paradise-wanted-tabs">
            <button class="paradise-wanted-tab is-active" type="button" data-pr-tab="list">LISTE</button>
            <button class="paradise-wanted-tab" type="button" data-pr-tab="info">INFOS</button>
          </div>
          <section class="paradise-wanted-panel" data-pr-panel="list">
            <div class="paradise-wanted-search-wrap"><span class="paradise-wanted-search-icon" aria-hidden="true">⌕</span><input class="paradise-wanted-search" type="search" placeholder="Rechercher un suspect..." autocomplete="off"></div>
            <div class="paradise-wanted-list"></div>
            <div class="paradise-wanted-count"></div>
          </section>
          <section class="paradise-wanted-panel" data-pr-panel="info" hidden>
            <div class="paradise-wanted-info-panel"><strong>Registre Wanted</strong><p>Individus actuellement signalés par les autorités de Lake Placid.</p><p>Le niveau de danger reflète les étoiles reçues par le client.</p></div>
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
      detail.innerHTML = '<div class="paradise-wanted-empty paradise-wanted-empty-detail">Aucune personne recherchée actuellement.</div>';
      return;
    }

    detail.dataset.danger = String(user.stars);

    const head = document.createElement('div');
    head.className = 'paradise-wanted-detail-head';
    head.appendChild(avatarWrap(user, 'paradise-wanted-profile-avatar'));

    const main = document.createElement('div');
    main.className = 'paradise-wanted-profile-main';
    main.innerHTML = `
      <div class="paradise-wanted-name-row">
        <div class="paradise-wanted-profile-name"></div>
        <div class="paradise-wanted-status">RECHERCHÉ</div>
      </div>
      <div class="paradise-wanted-two-col">
        <div class="paradise-wanted-identity-line">
          <div class="paradise-wanted-label">Alias</div>
          <div class="paradise-wanted-value">Aucun alias connu</div>
        </div>
        <div class="paradise-wanted-identity-line">
          <div class="paradise-wanted-label">Niveau de danger</div>
          <div class="paradise-wanted-danger-row">
            <div class="paradise-wanted-danger-stars">${starsMarkup(user.stars)}</div>
            <div class="paradise-wanted-danger-level">${dangerLabel(user.stars)}</div>
          </div>
        </div>
      </div>`;
    main.querySelector('.paradise-wanted-profile-name').textContent = user.name;
    head.appendChild(main);
    detail.appendChild(head);

    const facts = document.createElement('div');
    facts.className = 'paradise-wanted-facts';
    facts.innerHTML = `
      <div class="paradise-wanted-fact">
        <div class="paradise-wanted-fact-icon">$</div>
        <div><div class="paradise-wanted-label">Récompense</div><div class="paradise-wanted-value paradise-wanted-fact-value-strong">—</div></div>
      </div>
      <div class="paradise-wanted-fact">
        <div class="paradise-wanted-fact-icon">⌖</div>
        <div><div class="paradise-wanted-label">Dernier secteur connu</div><div class="paradise-wanted-value">Inconnu</div></div>
      </div>
      <div class="paradise-wanted-fact is-wide">
        <div class="paradise-wanted-fact-icon">≡</div>
        <div><div class="paradise-wanted-label">Motif</div><div class="paradise-wanted-value">Aucun motif communiqué.</div></div>
      </div>`;
    detail.appendChild(facts);

    const information = document.createElement('div');
    information.className = 'paradise-wanted-information';
    information.innerHTML = '<div class="paradise-wanted-information-icon">i</div><div><div class="paradise-wanted-label">Informations</div><div class="paradise-wanted-value">Individu actuellement recherché par les autorités de Lake Placid.</div></div>';
    detail.appendChild(information);

    const warning = document.createElement('div');
    warning.className = 'paradise-wanted-warning';
    warning.innerHTML = '<div class="paradise-wanted-warning-icon">!</div><div><strong>NE TENTEZ PAS D’INTERPELLER CET INDIVIDU.</strong><span>Contactez les forces de l’ordre de ParadiseRP.</span></div>';
    detail.appendChild(warning);
  }

  function bindTabs(shell) {
    shell.querySelectorAll('[data-pr-tab]').forEach(button => {
      button.addEventListener('click', event => {
        event.stopPropagation();
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
    if (!filtered.length) list.innerHTML = '<div class="paradise-wanted-empty">Aucun résultat.</div>';

    const selected = users.find(user => user.name === previousSelection) || users[0] || null;

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
      stars.innerHTML = starsMarkup(user.stars);

      meta.append(name, stars);
      card.appendChild(meta);

      const arrow = document.createElement('div');
      arrow.className = 'pr-wanted-arrow';
      arrow.textContent = '›';
      card.appendChild(arrow);

      card.addEventListener('click', event => {
        event.stopPropagation();
        shell.dataset.selectedName = user.name;
        renderList(alert, shell);
        renderDetail(shell, user);
      });

      list.appendChild(card);
    });

    count.textContent = `${users.length} recherché${users.length > 1 ? 's' : ''}`;
    shell.dataset.selectedName = selected ? selected.name : '';
    renderDetail(shell, selected);
  }

  function enhance(alert) {
    if (!alert || alert.dataset.paradiseWantedEnhancing === '1') return;
    alert.dataset.paradiseWantedEnhancing = '1';
    alert.removeAttribute('aria-hidden');

    let shell = alert.querySelector(':scope > .paradise-wanted-shell');
    if (!shell) {
      const nativeClose = findNativeClose(alert);
      shell = createShell(alert);
      alert.classList.add('paradise-wanted-enhanced');
      bindTabs(shell);

      shell.querySelector('.paradise-wanted-close').addEventListener('pointerdown', event => {
        event.stopPropagation();
      });
      shell.querySelector('.paradise-wanted-close').addEventListener('click', event => {
        event.preventDefault();
        event.stopPropagation();
        closeWanted(alert, nativeClose);
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
