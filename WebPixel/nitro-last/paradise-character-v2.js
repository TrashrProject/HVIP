(() => {
  'use strict';

  if (window.ParadiseCharacterV2) return;

  const VERSION = '2.0.0-character-profile';
  const HUD_ID = 'paradise-rp-hud';
  const API_URL = '../rp-character-action.php';
  const VALID_TABS = new Set(['overview', 'identity', 'documents', 'statistics', 'reputation']);
  const ID_CODE = 'PLACID_ID';
  const LICENSE_CODE = 'DRIVER_LICENSE';

  let hud = null;
  let profileWindow = null;
  let profileBody = null;
  let unsubscribe = () => {};
  let onboardingTimer = 0;
  let onboardingShown = false;
  let bioEditing = false;
  let lastUiEventId = null;
  let lastOfferNotified = null;
  let miniToastTimer = 0;
  let destroyed = false;

  const COMMANDS = [
    [':profile', 'Ouvre votre Character Profile ParadiseRP.'],
    [':id', "Ouvre votre carte d’identité officielle de Placid Island."],
    [':documents', 'Ouvre directement vos documents officiels.'],
    [':showid <joueur>', "Présente votre carte d’identité à un joueur dans la même room."],
    [':license', 'Ouvre votre permis de conduire si vous en possédez un.'],
    [':showlicense <joueur>', 'Présente votre permis de conduire à un joueur dans la même room.']
  ];

  const text = value => value === null || value === undefined || String(value).trim() === '' ? null : String(value).trim();
  const number = value => Number.isFinite(Number(value)) ? Number(value) : null;
  const esc = value => String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;');

  const money = value => {
    const n = number(value);
    return n === null ? 'Indisponible' : `${new Intl.NumberFormat('fr-FR').format(Math.round(n))} $`;
  };

  function formatDate(value, fallback = 'Non renseigné') {
    if (!value) return fallback;
    try {
      let date;
      if (/^\d{9,13}$/.test(String(value))) {
        let stamp = Number(value);
        if (stamp < 100000000000) stamp *= 1000;
        date = new Date(stamp);
      } else {
        date = new Date(value);
      }
      if (Number.isNaN(date.getTime())) return fallback;
      return new Intl.DateTimeFormat('fr-FR', { day: '2-digit', month: '2-digit', year: 'numeric' }).format(date);
    } catch (_) {
      return fallback;
    }
  }

  function formatDuration(value) {
    const raw = number(value);
    if (raw === null || raw < 0) return 'Indisponible';
    const seconds = Math.round(raw);
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    if (hours > 0) return `${hours} h ${minutes} min`;
    return `${minutes} min`;
  }

  function statValue(stat) {
    const current = number(stat?.current);
    const max = number(stat?.max);
    return current !== null && max !== null ? `${Math.round(current)} / ${Math.round(max)}` : 'Indisponible';
  }

  function statRatio(stat) {
    const current = number(stat?.current);
    const max = number(stat?.max);
    if (current === null || max === null || max <= 0) return 0;
    return Math.max(0, Math.min(100, (current / max) * 100));
  }

  function healthTone(stat) {
    const ratio = statRatio(stat);
    if (ratio < 30) return 'danger';
    if (ratio <= 60) return 'warn';
    return 'health';
  }

  function avatarUrl(player, offeredLook = null) {
    const direct = text(player?.avatarUrl);
    if (direct && !offeredLook) return direct;
    const look = text(offeredLook || player?.look);
    if (!look || !/^[a-z0-9.\-]+$/i.test(look)) return null;
    return `../avatar-image.php?figure=${encodeURIComponent(look)}&direction=2&head_direction=3&gesture=sml&action=std&size=l&phase2=1`;
  }

  function avatarMarkup(player, className = '', offeredLook = null, alt = 'Avatar Habbo') {
    const url = avatarUrl(player, offeredLook);
    return url
      ? `<img class="${esc(className)}" src="${esc(url)}" alt="${esc(alt)}" draggable="false">`
      : '<span class="pr2-avatar-fallback">RP</span>';
  }

  function state() {
    return window.ParadiseStore?.getState?.() || null;
  }

  function currentTab() {
    const tab = text(state()?.ui?.profileTab) || 'overview';
    return VALID_TABS.has(tab) ? tab : 'overview';
  }

  function documentByCode(code) {
    return (state()?.documents || []).find(doc => text(doc.type) === code) || null;
  }

  function documentStatus(doc) {
    if (!doc) return { label: 'NON OBTENU', className: 'is-missing' };
    const status = (text(doc.status) || 'UNKNOWN').toUpperCase();
    if (status === 'VALID') {
      if (doc.expiresAt && new Date(doc.expiresAt).getTime() <= Date.now()) return { label: 'EXPIRÉ', className: 'is-bad' };
      return { label: 'VALIDE', className: '' };
    }
    if (status === 'SUSPENDED') return { label: 'SUSPENDU', className: 'is-bad' };
    if (status === 'REVOKED') return { label: 'RÉVOQUÉ', className: 'is-bad' };
    if (status === 'EXPIRED') return { label: 'EXPIRÉ', className: 'is-bad' };
    return { label: status, className: 'is-bad' };
  }

  function field(label, value, wide = false) {
    return `<div class="pr2-field${wide ? ' is-wide' : ''}"><span>${esc(label)}</span><strong>${esc(text(value) || 'Non renseigné')}</strong></div>`;
  }

  function info(label, value) {
    return `<div class="pr2-info"><span>${esc(label)}</span><strong>${esc(text(value) || 'Non renseigné')}</strong></div>`;
  }

  function profileShell() {
    return `
      <div class="pr2-profile" data-pr2-profile="1">
        <nav class="pr2-tabs" aria-label="Sections du profil">
          <button type="button" class="pr2-tab" data-pr2-tab="overview">Aperçu</button>
          <button type="button" class="pr2-tab" data-pr2-tab="identity">Identité</button>
          <button type="button" class="pr2-tab" data-pr2-tab="documents">Documents</button>
          <button type="button" class="pr2-tab" data-pr2-tab="statistics">Statistiques</button>
          <button type="button" class="pr2-tab" data-pr2-tab="reputation">Réputation</button>
        </nav>
        <div class="pr2-panels">
          <section class="pr2-panel" data-pr2-panel="overview"></section>
          <section class="pr2-panel" data-pr2-panel="identity"></section>
          <section class="pr2-panel" data-pr2-panel="documents"></section>
          <section class="pr2-panel" data-pr2-panel="statistics"></section>
          <section class="pr2-panel" data-pr2-panel="reputation"></section>
        </div>
      </div>`;
  }

  function overviewMarkup(store) {
    const player = store.gameplay.player;
    const character = store.character;
    const economy = store.gameplay.economy;
    const room = store.gameplay.room;
    const role = text(player.role) || 'Citoyen';
    const job = text(player.job) || 'Sans emploi';
    const displayName = character.exists ? (text(character.fullName) || `${text(character.firstName) || ''} ${text(character.lastName) || ''}`.trim()) : (text(player.username) || 'Joueur');
    const accountLine = character.exists && displayName !== text(player.username)
      ? `Compte Habbo : ${text(player.username) || '—'}`
      : 'Personnage ParadiseRP';
    const hpRatio = statRatio(player.health);
    const armorRatio = statRatio(player.armor);
    const bio = text(character.biography) || (text(player.motto) ? `Motto Habbo : ${text(player.motto)}` : 'Aucune biographie RP renseignée.');

    return `
      <div class="pr2-overview-grid">
        <aside class="pr2-avatar-card">
          <div class="pr2-avatar-stage">${avatarMarkup(player, '', null, displayName)}</div>
          <div class="pr2-identity-summary">
            <h2>${esc(displayName)}</h2>
            <div class="pr2-account-name">${esc(accountLine)}</div>
            <div class="pr2-summary-chips">
              <span class="pr2-chip">${esc(role)}</span>
              ${character.exists && character.age !== null ? `<span class="pr2-chip is-gold">${Math.round(character.age)} ans</span>` : ''}
              ${character.exists && character.nationality ? `<span class="pr2-chip is-neutral">${esc(character.nationality)}</span>` : ''}
            </div>
          </div>
        </aside>

        <div class="pr2-overview-main">
          <div class="pr2-card">
            <div class="pr2-card-title"><strong>État du personnage</strong><small>Données de session</small></div>
            <div class="pr2-stat-grid">
              <div class="pr2-stat-card" data-tone="${healthTone(player.health)}">
                <div class="pr2-stat-head"><span>Santé</span><b>${esc(statValue(player.health))}</b></div>
                <div class="pr2-meter"><i style="width:${hpRatio}%"></i></div>
              </div>
              <div class="pr2-stat-card" data-tone="armor">
                <div class="pr2-stat-head"><span>Armure</span><b>${esc(statValue(player.armor))}</b></div>
                <div class="pr2-meter"><i style="width:${armorRatio}%"></i></div>
              </div>
            </div>
          </div>

          <div class="pr2-card">
            <div class="pr2-card-title"><strong>Vie à Placid Island</strong><small>Résumé privé</small></div>
            <div class="pr2-info-grid">
              ${info('Métier', job)}
              ${info('Localisation', text(room.name) || 'Localisation inconnue')}
              ${info('Argent', money(economy.cash))}
              ${info('Banque', money(economy.bank))}
            </div>
          </div>

          <div class="pr2-card">
            <div class="pr2-card-title"><strong>Biographie</strong><small>${character.exists ? 'Profil RP' : 'Identité requise'}</small></div>
            ${bioEditing && character.exists ? `
              <form class="pr2-bio-editor" data-pr2-bio-form>
                <textarea name="biography" maxlength="400" placeholder="Une courte description de votre personnage...">${esc(character.biography || '')}</textarea>
                <div class="pr2-bio-editor-footer">
                  <span class="pr2-char-count" data-pr2-bio-count>${String(character.biography || '').length} / 400</span>
                  <div><button type="button" class="pr2-button is-ghost" data-pr2-action="cancel-bio">Annuler</button> <button type="submit" class="pr2-button is-primary">Enregistrer</button></div>
                </div>
              </form>` : `
              <div class="pr2-bio"><p>${esc(bio)}</p>${character.exists ? '<div class="pr2-bio-actions"><button type="button" class="pr2-button" data-pr2-action="edit-bio">Modifier</button></div>' : ''}</div>`}
          </div>
        </div>
      </div>`;
  }

  function identityCreateMarkup(store) {
    const player = store.gameplay.player;
    return `
      <div class="pr2-create-card">
        <div class="pr2-create-hero">
          <div class="pr2-seal">PI</div>
          <div><strong>Bienvenue sur Placid Island</strong><p>Votre compte Habbo reste votre référence technique. Créez maintenant votre identité citoyenne RP persistante.</p></div>
        </div>
        <form class="pr2-form" data-pr2-identity-form>
          <div class="pr2-form-grid">
            <label>Prénom RP<input name="first_name" maxlength="32" required autocomplete="off" value="${esc(text(player.username) || '')}"></label>
            <label>Nom RP<input name="last_name" maxlength="32" required autocomplete="off"></label>
            <label>Date de naissance<input name="birth_date" type="date" required></label>
            <label>Nationalité / origine<input name="nationality" maxlength="48" required autocomplete="off" placeholder="Ex. Placidienne"></label>
            <label>Genre RP (facultatif)<input name="gender" maxlength="24" autocomplete="off"></label>
            <label class="is-wide">Biographie (facultative)<textarea name="biography" maxlength="400" placeholder="Quelques lignes sur votre arrivée à Placid Island..."></textarea></label>
          </div>
          <div class="pr2-form-foot">
            <div class="pr2-form-status" data-pr2-form-status></div>
            <button type="submit" class="pr2-button is-primary">Créer mon identité</button>
          </div>
        </form>
      </div>`;
  }

  function identityMarkup(store) {
    const character = store.character;
    if (!character.exists) return identityCreateMarkup(store);

    return `
      <div class="pr2-dossier">
        <div class="pr2-dossier-head">
          <div class="pr2-seal">PI</div>
          <div><strong>Dossier citoyen Paradise</strong><small>République de Placid Island · informations administratives</small></div>
        </div>
        <div class="pr2-dossier-grid">
          ${field('Prénom', character.firstName)}
          ${field('Nom', character.lastName)}
          ${field('Date de naissance', formatDate(character.birthDate))}
          ${field('Âge calculé', character.age !== null ? `${Math.round(character.age)} ans` : null)}
          ${field('Nationalité / origine', character.nationality)}
          ${field('Genre RP', character.gender)}
          ${field('Paradise Citizen ID', character.citizenId)}
          ${field("Date d’arrivée citoyenne", formatDate(character.createdAt))}
          ${field('Compte Habbo technique', store.gameplay.player.username, true)}
        </div>
      </div>`;
  }

  function documentTile(doc, type, name, mark, missingText) {
    const selected = text(state()?.ui?.profileDocument) === type;
    const status = documentStatus(doc);
    return `
      <button type="button" class="pr2-doc-tile${selected ? ' is-selected' : ''}${doc ? '' : ' is-missing'}" ${doc ? `data-pr2-document="${esc(type)}"` : 'disabled'}>
        <span class="pr2-doc-mark${type === LICENSE_CODE ? ' is-license' : ''}">${esc(mark)}</span>
        <span><strong>${esc(name)}</strong><small>${doc ? esc(doc.number || 'Numéro indisponible') : esc(missingText)}</small></span>
        <span class="pr2-doc-status ${status.className}">${esc(status.label)}</span>
      </button>`;
  }

  function idCardMarkup(store, doc, offered = null) {
    const character = offered?.identity || store.character;
    const player = store.gameplay.player;
    const sender = offered?.sender || null;
    const firstName = text(character.first_name ?? character.firstName) || 'Non renseigné';
    const lastName = text(character.last_name ?? character.lastName) || 'Non renseigné';
    const birthDate = text(character.birth_date ?? character.birthDate);
    const citizenId = text(character.citizen_id ?? character.citizenId) || 'Indisponible';
    const nationality = text(character.nationality) || 'Non renseignée';
    const issuedAt = text(doc?.issued_at ?? doc?.issuedAt);
    const numberValue = text(doc?.number) || 'Indisponible';
    const look = offered ? text(sender?.look) : null;
    const alt = `${firstName} ${lastName}`;

    return `
      <div class="pr2-idcard">
        <div class="pr2-idcard-head">
          <div class="pr2-idcard-seal">PI</div>
          <div><strong>République de Placid Island</strong><small>Carte d’identité citoyenne</small></div>
        </div>
        <div class="pr2-idcard-body">
          <div class="pr2-id-photo">${avatarMarkup(player, '', look, alt)}</div>
          <div class="pr2-id-data">
            <p><span>Prénom</span><b>${esc(firstName)}</b></p>
            <p><span>Nom</span><b>${esc(lastName)}</b></p>
            <p><span>Naissance</span><b>${esc(formatDate(birthDate))}</b></p>
            <p><span>Nationalité</span><b>${esc(nationality)}</b></p>
            <p class="is-wide"><span>Paradise Citizen ID</span><b>${esc(citizenId)}</b></p>
            <p class="is-wide"><span>N° document</span><b>${esc(numberValue)}</b></p>
          </div>
        </div>
        <div class="pr2-idcard-foot"><span>Émise le ${esc(formatDate(issuedAt, 'Indisponible'))}</span><span class="pr2-signature">${esc(`${firstName} ${lastName}`)}</span></div>
      </div>`;
  }

  function licenseMarkup(store, doc, offered = null) {
    const character = offered?.identity || store.character;
    const sender = offered?.sender || null;
    const player = store.gameplay.player;
    const firstName = text(character.first_name ?? character.firstName) || 'Non renseigné';
    const lastName = text(character.last_name ?? character.lastName) || 'Non renseigné';
    const look = offered ? text(sender?.look) : null;
    const status = documentStatus(doc);
    return `
      <div class="pr2-license-card">
        <div class="pr2-license-head"><div><strong>Permis de conduire · Placid Island</strong><small>Document officiel ParadiseRP</small></div><span class="pr2-doc-status ${status.className}">${esc(status.label)}</span></div>
        <div class="pr2-license-grid">
          <div class="pr2-license-photo">${avatarMarkup(player, '', look, `${firstName} ${lastName}`)}</div>
          <div class="pr2-dossier-grid">
            ${field('Titulaire', `${firstName} ${lastName}`, true)}
            ${field('N° permis', doc?.number)}
            ${field('Délivré le', formatDate(doc?.issued_at ?? doc?.issuedAt))}
            ${field('Expiration', doc?.expires_at || doc?.expiresAt ? formatDate(doc.expires_at ?? doc.expiresAt) : 'Sans date renseignée')}
            ${field('Statut', status.label)}
          </div>
        </div>
      </div>`;
  }

  function genericDocumentMarkup(doc) {
    const status = documentStatus(doc);
    return `<div class="pr2-dossier"><div class="pr2-dossier-head"><div class="pr2-seal">DOC</div><div><strong>${esc(doc.name || 'Document ParadiseRP')}</strong><small>Document officiel enregistré</small></div></div><div class="pr2-dossier-grid">${field('Numéro', doc.number)}${field('Statut', status.label)}${field('Délivré le', formatDate(doc.issuedAt))}${field('Expiration', doc.expiresAt ? formatDate(doc.expiresAt) : 'Non applicable')}</div></div>`;
  }

  function presentedDocumentMarkup(store, offer) {
    const doc = offer?.document || {};
    const type = text(doc.type);
    const title = text(offer?.sender?.name) || text(offer?.sender?.username) || 'Un joueur';
    let body;
    if (type === ID_CODE) body = idCardMarkup(store, doc, offer);
    else if (type === LICENSE_CODE) body = licenseMarkup(store, doc, offer);
    else body = genericDocumentMarkup({ ...doc, issuedAt: doc.issued_at, expiresAt: doc.expires_at });
    return `<div class="pr2-card-title"><strong>Document présenté par ${esc(title)}</strong><button type="button" class="pr2-button is-ghost" data-pr2-action="close-presented">Revenir à mes documents</button></div>${body}`;
  }

  function documentsMarkup(store) {
    if (!store.character.exists) {
      return `<div class="pr2-doc-empty"><div><strong>Identité citoyenne requise</strong><p>Créez votre identité RP avant que l’administration de Placid Island puisse émettre votre carte officielle.</p><button type="button" class="pr2-button is-primary" data-pr2-tab="identity" style="margin-top:10px">Créer mon identité</button></div></div>`;
    }

    const offered = store.ui.presentedDocument;
    const identityDoc = store.documents.find(doc => doc.type === ID_CODE) || null;
    const licenseDoc = store.documents.find(doc => doc.type === LICENSE_CODE) || null;
    const extras = store.documents.filter(doc => doc.type !== ID_CODE && doc.type !== LICENSE_CODE);
    let selectedCode = text(store.ui.profileDocument);
    if (!selectedCode || !store.documents.some(doc => doc.type === selectedCode)) selectedCode = identityDoc?.type || licenseDoc?.type || null;
    const selected = store.documents.find(doc => doc.type === selectedCode) || null;

    let view = '<div class="pr2-doc-empty"><div><strong>Aucun document sélectionné</strong><p>Les documents réellement délivrés à votre personnage apparaissent ici.</p></div></div>';
    if (offered) view = presentedDocumentMarkup(store, offered);
    else if (selected?.type === ID_CODE) view = idCardMarkup(store, selected);
    else if (selected?.type === LICENSE_CODE) view = licenseMarkup(store, selected);
    else if (selected) view = genericDocumentMarkup(selected);

    return `
      <div class="pr2-doc-layout">
        <div class="pr2-doc-list">
          ${documentTile(identityDoc, ID_CODE, "Carte d’identité", 'ID', identityDoc ? '' : 'Non délivrée')}
          ${documentTile(licenseDoc, LICENSE_CODE, 'Permis de conduire', 'DL', 'Non obtenu')}
          ${extras.map(doc => documentTile(doc, doc.type, doc.name || 'Document', 'DOC', 'Non obtenu')).join('')}
        </div>
        <div class="pr2-doc-view">${view}</div>
      </div>`;
  }

  function statisticsMarkup(store) {
    const stats = store.statistics || {};
    const player = store.gameplay.player;
    const cards = [];
    if (stats.accountCreated) cards.push(['Compte créé', formatDate(stats.accountCreated), 'Donnée de compte fiable']);
    if (number(stats.onlineTime) !== null) cards.push(['Temps en ligne', formatDuration(stats.onlineTime), 'Temps cumulé enregistré']);
    if (number(stats.roomVisits) !== null) cards.push(['Rooms visitées', new Intl.NumberFormat('fr-FR').format(Math.round(stats.roomVisits)), 'Compteur Habbo existant']);
    if (number(player.level) !== null) cards.push(['Niveau RP existant', String(Math.round(player.level)), 'Valeur issue du système RP actuel']);
    if (text(player.job)) cards.push(['Métier actuel', player.job, 'Statut de session']);

    if (!cards.length) return `<div class="pr2-doc-empty"><div><strong>Aucune statistique fiable disponible</strong><p>ParadiseRP n’affiche pas de chiffres inventés. Les statistiques apparaîtront lorsque leur source réelle sera disponible.</p></div></div>`;
    return `<div class="pr2-dossier"><div class="pr2-dossier-head"><div class="pr2-seal">ST</div><div><strong>Statistiques du personnage</strong><small>Uniquement des données réellement disponibles</small></div></div><div class="pr2-statistics-list" style="margin-top:10px">${cards.map(([label, value, note]) => `<div class="pr2-statistic"><span>${esc(label)}</span><strong>${esc(value)}</strong><small>${esc(note)}</small></div>`).join('')}</div></div>`;
  }

  function reputationMarkup(store) {
    const points = number(store.reputation?.general) ?? 0;
    return `<div class="pr2-reputation-card"><div class="pr2-reputation-mark">★</div><h3>Réputation générale à Placid</h3><div class="pr2-reputation-points">${new Intl.NumberFormat('fr-FR').format(Math.round(points))}</div><p>Cette valeur persistante constitue le socle de réputation ParadiseRP. Aucun rang, seuil criminel, politique ou économique fictif n’est affiché tant que ces systèmes n’existent pas réellement.</p></div>`;
  }

  function renderPanel(name) {
    const store = state();
    if (!store || !profileBody) return;
    const panel = profileBody.querySelector(`[data-pr2-panel="${name}"]`);
    if (!panel) return;
    if (name === 'overview') panel.innerHTML = overviewMarkup(store);
    if (name === 'identity') panel.innerHTML = identityMarkup(store);
    if (name === 'documents') panel.innerHTML = documentsMarkup(store);
    if (name === 'statistics') panel.innerHTML = statisticsMarkup(store);
    if (name === 'reputation') panel.innerHTML = reputationMarkup(store);
  }

  function renderAll() {
    if (!profileBody) return;
    for (const tab of VALID_TABS) renderPanel(tab);
    syncTabs();
  }

  function syncTabs() {
    if (!profileBody) return;
    const tab = currentTab();
    profileBody.querySelectorAll('[data-pr2-tab]').forEach(button => {
      if (!(button instanceof HTMLButtonElement)) return;
      button.classList.toggle('is-active', button.dataset.pr2Tab === tab);
      button.setAttribute('aria-pressed', button.dataset.pr2Tab === tab ? 'true' : 'false');
    });
    profileBody.querySelectorAll('[data-pr2-panel]').forEach(panel => panel.classList.toggle('is-active', panel.dataset.pr2Panel === tab));
  }

  function setTab(tab, documentCode = null) {
    if (!VALID_TABS.has(tab)) tab = 'overview';
    const patch = { profileTab: tab };
    if (tab === 'documents') patch.profileDocument = documentCode || state()?.ui?.profileDocument || ID_CODE;
    else if (documentCode === null) patch.profileDocument = state()?.ui?.profileDocument || null;
    window.ParadiseStore?.setUi?.(patch);
    syncTabs();
    if (tab === 'documents') renderPanel('documents');
  }

  function openProfile(tab = null, documentCode = null, presentedDocument = undefined) {
    const store = state();
    if (!store) return false;
    const fallbackTab = store.character.exists ? 'overview' : 'identity';
    const requested = VALID_TABS.has(tab) ? tab : fallbackTab;
    const patch = {
      activeWindow: 'profile',
      actionsOpen: false,
      profileTab: requested,
      profileDocument: documentCode || (requested === 'documents' ? (store.ui.profileDocument || ID_CODE) : store.ui.profileDocument)
    };
    if (presentedDocument !== undefined) patch.presentedDocument = presentedDocument;
    window.ParadiseStore.setUi(patch);
    renderAll();
    return true;
  }

  function ensureAuxiliaryUi() {
    if (!hud) return;
    if (!hud.querySelector('.pr2-toast-host')) hud.insertAdjacentHTML('beforeend', '<div class="pr2-toast-host" aria-live="polite"></div>');
    if (!hud.querySelector('.pr2-command-layer')) {
      hud.insertAdjacentHTML('beforeend', `
        <div class="pr2-command-layer" data-pr2-command-layer hidden>
          <section class="pr2-command-guide" role="dialog" aria-label="Commandes ParadiseRP">
            <header class="pr2-command-head"><div><strong>Commandes ParadiseRP</strong><small>Character Profile · Identité · Documents</small></div><button type="button" class="pr2-close-mini" data-pr2-action="close-commands" aria-label="Fermer">×</button></header>
            <div class="pr2-command-body"><div class="pr2-command-category">PERSONNAGE &amp; DOCUMENTS</div>${COMMANDS.map(([cmd, desc]) => `<div class="pr2-command-row"><code>${esc(cmd)}</code><span>${esc(desc)}</span></div>`).join('')}</div>
          </section>
        </div>`);
    }
  }

  function showCommands() {
    ensureAuxiliaryUi();
    const layer = hud?.querySelector('[data-pr2-command-layer]');
    if (layer) layer.hidden = false;
  }

  function hideCommands() {
    const layer = hud?.querySelector('[data-pr2-command-layer]');
    if (layer) layer.hidden = true;
  }

  function showMiniToast(message, duration = 2800) {
    ensureAuxiliaryUi();
    const host = hud?.querySelector('.pr2-toast-host');
    if (!host) return;
    const existing = host.querySelector('.pr2-mini-toast');
    existing?.remove();
    const toast = document.createElement('div');
    toast.className = 'pr2-mini-toast';
    toast.textContent = message;
    host.appendChild(toast);
    window.clearTimeout(miniToastTimer);
    miniToastTimer = window.setTimeout(() => toast.remove(), duration);
  }

  function offerText(offer) {
    const type = text(offer?.document?.type);
    if (type === LICENSE_CODE) return 'vous présente son permis de conduire.';
    if (type === ID_CODE) return "vous présente sa carte d’identité.";
    return 'vous présente un document.';
  }

  function showDocumentOffer(offer) {
    const id = number(offer?.id);
    if (!offer || id === null || id === lastOfferNotified) return;
    lastOfferNotified = id;
    ensureAuxiliaryUi();
    const host = hud?.querySelector('.pr2-toast-host');
    if (!host) return;
    host.querySelectorAll('[data-pr2-offer]').forEach(node => node.remove());
    const senderName = text(offer?.sender?.name) || text(offer?.sender?.username) || 'Un joueur';
    const look = text(offer?.sender?.look);
    const avatar = avatarMarkup(state()?.gameplay?.player || {}, '', look, senderName);
    const toast = document.createElement('div');
    toast.className = 'pr2-offer-toast';
    toast.dataset.pr2Offer = String(id);
    toast.innerHTML = `<div class="pr2-offer-avatar">${avatar}</div><div><strong>${esc(senderName)}</strong><small>${esc(offerText(offer))}</small></div><button type="button" class="pr2-button is-primary" data-pr2-action="view-offer">Consulter</button>`;
    host.appendChild(toast);
  }

  async function apiAction(payload) {
    const response = await fetch(API_URL, {
      method: 'POST',
      credentials: 'same-origin',
      cache: 'no-store',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'X-Paradise-Action': 'phase2'
      },
      body: JSON.stringify(payload || {})
    });
    let data = null;
    try { data = await response.json(); } catch (_) {}
    if (!response.ok || !data?.ok) {
      const error = new Error(data?.errors?.join(' ') || data?.reason || `HTTP ${response.status}`);
      error.payload = data;
      throw error;
    }
    return data;
  }

  async function submitIdentity(form) {
    const status = form.querySelector('[data-pr2-form-status]');
    const submit = form.querySelector('button[type="submit"]');
    if (submit) submit.disabled = true;
    if (status) status.textContent = 'Création de votre identité...';
    const data = new FormData(form);
    const payload = {
      action: 'create_identity',
      first_name: data.get('first_name'),
      last_name: data.get('last_name'),
      birth_date: data.get('birth_date'),
      nationality: data.get('nationality'),
      gender: data.get('gender'),
      biography: data.get('biography')
    };
    try {
      await apiAction(payload);
      await window.ParadiseBridge?.refresh?.();
      bioEditing = false;
      openProfile('overview');
      showMiniToast('Identité citoyenne créée. Votre carte officielle a été délivrée.');
    } catch (error) {
      if (status) status.textContent = error.message || 'Impossible de créer votre identité.';
    } finally {
      if (submit) submit.disabled = false;
    }
  }

  async function submitBiography(form) {
    const textarea = form.querySelector('textarea[name="biography"]');
    const value = String(textarea?.value || '').trim();
    try {
      await apiAction({ action: 'update_biography', biography: value });
      await window.ParadiseBridge?.refresh?.();
      bioEditing = false;
      renderPanel('overview');
      showMiniToast('Biographie mise à jour.');
    } catch (error) {
      showMiniToast(error.message || 'Modification impossible.');
    }
  }

  async function viewOffer() {
    const offer = state()?.offers?.document;
    if (!offer) return;
    window.ParadiseStore?.setUi?.({ presentedDocument: offer, activeWindow: 'profile', profileTab: 'documents', profileDocument: text(offer?.document?.type) || ID_CODE });
    renderPanel('documents');
    syncTabs();
    try { await apiAction({ action: 'view_document_offer', offer_id: offer.id }); } catch (_) {}
    window.ParadiseStore?.clearDocumentOffer?.(offer.id);
    hud?.querySelector(`[data-pr2-offer="${offer.id}"]`)?.remove();
  }

  async function handleUiEvent(eventData) {
    const id = number(eventData?.id);
    if (id === null || id === lastUiEventId) return;
    lastUiEventId = id;
    const type = text(eventData?.type);
    if (type === 'PROFILE_OPEN') {
      const tab = text(eventData?.payload?.tab) || (state()?.character?.exists ? 'overview' : 'identity');
      const doc = text(eventData?.payload?.document);
      openProfile(tab, doc);
    }
    try { await apiAction({ action: 'consume_ui_event', event_id: id }); } catch (_) {}
    window.ParadiseStore?.clearUiEvent?.(id);
  }

  function scheduleCommandRefresh(message) {
    if (!/^:(profile|id|documents|license|showid|showlicense)\b/i.test(message)) return;
    window.setTimeout(() => window.ParadiseBridge?.refresh?.(), 450);
    window.setTimeout(() => window.ParadiseBridge?.refresh?.(), 1250);
  }

  function handleChatIntent(event) {
    if (event.key !== 'Enter' && event.keyCode !== 13) return;
    const input = event.target;
    if (!(input instanceof HTMLInputElement) || input.id !== 'pr4-chat-input') return;
    const message = String(input.value || '').trim();
    if (!message.startsWith(':')) return;

    const lower = message.toLowerCase();
    // Do not block Nitro/EMU: these are immediate UI equivalents while the real
    // command still travels through the existing native chat adapter.
    if (lower === ':profile') window.setTimeout(() => openProfile(state()?.character?.exists ? 'overview' : 'identity'), 0);
    else if (lower === ':id') window.setTimeout(() => openProfile(state()?.character?.exists ? 'documents' : 'identity', ID_CODE), 0);
    else if (lower === ':documents') window.setTimeout(() => openProfile(state()?.character?.exists ? 'documents' : 'identity'), 0);
    else if (lower === ':license') {
      window.setTimeout(() => {
        const license = documentByCode(LICENSE_CODE);
        if (license && documentStatus(license).label === 'VALIDE') openProfile('documents', LICENSE_CODE);
        else showMiniToast('Vous ne possédez pas de permis de conduire.');
      }, 0);
    } else if (lower === ':commands') window.setTimeout(showCommands, 0);

    scheduleCommandRefresh(message);
  }

  function handleHudClick(event) {
    const target = event.target;
    if (!(target instanceof Element)) return;

    const openButton = target.closest('[data-window-open]');
    if (openButton?.dataset.windowOpen === 'documents') {
      event.preventDefault();
      event.stopImmediatePropagation();
      openProfile(state()?.character?.exists ? 'documents' : 'identity');
      return;
    }
    if (openButton?.dataset.windowOpen === 'profile') {
      event.preventDefault();
      event.stopImmediatePropagation();
      openProfile(state()?.character?.exists ? 'overview' : 'identity');
      return;
    }

    const tabButton = target.closest('[data-pr2-tab]');
    if (tabButton) {
      event.preventDefault();
      event.stopPropagation();
      setTab(tabButton.dataset.pr2Tab);
      return;
    }

    const documentButton = target.closest('[data-pr2-document]');
    if (documentButton) {
      event.preventDefault();
      event.stopPropagation();
      window.ParadiseStore?.setUi?.({ profileDocument: documentButton.dataset.pr2Document, presentedDocument: null });
      renderPanel('documents');
      return;
    }

    const action = target.closest('[data-pr2-action]');
    if (!action) return;
    event.preventDefault();
    event.stopPropagation();
    const name = action.dataset.pr2Action;
    if (name === 'edit-bio') { bioEditing = true; renderPanel('overview'); }
    if (name === 'cancel-bio') { bioEditing = false; renderPanel('overview'); }
    if (name === 'close-presented') { window.ParadiseStore?.setUi?.({ presentedDocument: null }); renderPanel('documents'); }
    if (name === 'close-commands') hideCommands();
    if (name === 'view-offer') viewOffer();
  }

  function handleSubmit(event) {
    const form = event.target;
    if (!(form instanceof HTMLFormElement)) return;
    if (form.matches('[data-pr2-identity-form]')) {
      event.preventDefault();
      submitIdentity(form);
    }
    if (form.matches('[data-pr2-bio-form]')) {
      event.preventDefault();
      submitBiography(form);
    }
  }

  function handleInput(event) {
    const target = event.target;
    if (!(target instanceof HTMLTextAreaElement)) return;
    if (target.closest('[data-pr2-bio-form]')) {
      const count = target.closest('[data-pr2-bio-form]')?.querySelector('[data-pr2-bio-count]');
      if (count) count.textContent = `${target.value.length} / 400`;
    }
  }

  function scheduleOnboarding(store) {
    window.clearTimeout(onboardingTimer);
    if (onboardingShown || !store?.meta?.connected || store?.character?.exists || !store?.gameplay?.player?.username) return;
    const roomStable = Boolean(text(store?.gameplay?.room?.name));
    onboardingTimer = window.setTimeout(() => {
      if (destroyed || onboardingShown) return;
      const latest = state();
      if (!latest?.meta?.connected || latest?.character?.exists) return;
      onboardingShown = true;
      window.ParadiseStore?.setUi?.({ onboarding: true });
      openProfile('identity');
      showMiniToast('Votre identité citoyenne doit être créée.');
    }, roomStable ? 900 : 3500);
  }

  function syncFromStore(store, eventName) {
    if (!store || destroyed) return;
    if (['gameplay:snapshot','character:update','documents:update','reputation:update','statistics:update','room:change'].includes(eventName)) {
      renderAll();
      scheduleOnboarding(store);
    }
    if (eventName === 'ui:change') syncTabs();
    if (eventName === 'document:offer' && store.offers.document) showDocumentOffer(store.offers.document);
    if (eventName === 'ui:event' && store.meta.pendingUiEvent) handleUiEvent(store.meta.pendingUiEvent);
  }

  function mountProfile() {
    hud = document.getElementById(HUD_ID);
    if (!hud) return false;
    profileWindow = hud.querySelector('.pr-window[data-window="profile"]');
    profileBody = profileWindow?.querySelector('.pr-window-body');
    if (!profileWindow || !profileBody) return false;

    if (!profileBody.querySelector('[data-pr2-profile]')) profileBody.innerHTML = profileShell();
    const title = profileWindow.querySelector('.pr-window-title strong');
    const subtitle = profileWindow.querySelector('.pr-window-title small');
    if (title) title.textContent = 'Mon Profil';
    if (subtitle) subtitle.textContent = 'Identité citoyenne · Placid Island';

    ensureAuxiliaryUi();
    renderAll();
    return true;
  }

  function sendCommand(command) {
    const value = String(command || '').trim();
    if (!value) return Promise.resolve(false);
    const adapter = window.__ParadiseNativeChatAdapter;
    if (!adapter?.send) return Promise.resolve(false);
    const promise = adapter.send(value);
    scheduleCommandRefresh(value);
    return Promise.resolve(promise);
  }

  function boot() {
    if (destroyed || !window.ParadiseStore || !mountProfile()) return;
    hud.addEventListener('click', handleHudClick, true);
    hud.addEventListener('submit', handleSubmit, false);
    hud.addEventListener('input', handleInput, false);
    hud.addEventListener('keydown', handleChatIntent, true);

    unsubscribe = window.ParadiseStore.subscribe(syncFromStore);
    const store = state();
    scheduleOnboarding(store);
    if (store?.offers?.document) showDocumentOffer(store.offers.document);
    if (store?.meta?.pendingUiEvent) handleUiEvent(store.meta.pendingUiEvent);

    console.info('[ParadiseRP] Character Profile V2 active', { version: VERSION });
  }

  function destroy() {
    if (destroyed) return;
    destroyed = true;
    window.clearTimeout(onboardingTimer);
    window.clearTimeout(miniToastTimer);
    unsubscribe();
    if (hud) {
      hud.removeEventListener('click', handleHudClick, true);
      hud.removeEventListener('submit', handleSubmit, false);
      hud.removeEventListener('input', handleInput, false);
      hud.removeEventListener('keydown', handleChatIntent, true);
    }
  }

  window.ParadiseCharacterV2 = Object.freeze({
    version: VERSION,
    open: openProfile,
    openDocuments: documentCode => openProfile('documents', documentCode || null),
    showCommands,
    refresh: () => window.ParadiseBridge?.refresh?.(),
    presentIdentityTo: username => sendCommand(`:showid ${String(username || '').trim()}`),
    presentLicenseTo: username => sendCommand(`:showlicense ${String(username || '').trim()}`),
    openPublicProfile: username => sendCommand(`:profile ${String(username || '').trim()}`),
    getCharacter: () => state()?.character || null,
    getDocuments: () => (state()?.documents || []).slice(),
    destroy
  });

  window.addEventListener('beforeunload', destroy, { once: true });
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
