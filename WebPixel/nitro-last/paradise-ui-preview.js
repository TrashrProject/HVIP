(() => {
  'use strict';

  const $ = (selector, root = document) => root.querySelector(selector);
  const $$ = (selector, root = document) => [...root.querySelectorAll(selector)];
  const panels = $$('.screen-panel');
  const navButtons = $$('[data-screen]');

  const commands = [
    { name: ':me', desc: 'Afficher une action RP au-dessus du personnage.', syntax: ':me <action>', example: ':me sort sa carte citoyenne' },
    { name: ':pay', desc: 'Donner de l’argent liquide à un citoyen proche.', syntax: ':pay <joueur> <montant>', example: ':pay Alex 250' },
    { name: ':give', desc: 'Donner un objet de votre inventaire.', syntax: ':give <joueur> <objet>', example: ':give Lina cle_voiture' },
    { name: ':phone', desc: 'Ouvrir le ParadisePhone.', syntax: ':phone', example: ':phone' },
    { name: ':id', desc: 'Présenter sa carte citoyenne.', syntax: ':id', example: ':id' },
    { name: ':job', desc: 'Consulter son métier et son service.', syntax: ':job', example: ':job' },
    { name: ':radio', desc: 'Parler sur une fréquence professionnelle.', syntax: ':radio <message>', example: ':radio Besoin unité au port' }
  ];

  const items = [
    ['🪪', 1, 'Carte citoyenne'], ['🥤', 9, 'Boisson fraîche'], ['🥪', 4, 'Sandwich marina'], ['📷', 1, 'Appareil photo'], ['🔑', 2, 'Clés'],
    ['🎒', 1, 'Sac urbain'], ['🕶️', 1, 'Lunettes'], ['⌚', 1, 'Montre'], ['💎', 12, 'Diamants'], ['📦', 2, 'Colis'],
    ['💊', 3, 'Kit soins'], ['🧾', 5, 'Contrats'], ['📱', 1, 'ParadisePhone'], ['🔦', 1, 'Lampe'], ['🎟️', 7, 'Tickets']
  ];

  const openScreen = screen => {
    panels.forEach(panel => panel.classList.toggle('active', panel.dataset.panel === screen));
    navButtons.forEach(button => button.classList.toggle('active', button.dataset.screen === screen));
    if (screen === 'home') panels.forEach(panel => panel.classList.remove('active'));
    if (screen !== 'home') toast(`Ouverture`, `${labelFor(screen)} est maintenant affiché.`);
  };

  const labelFor = screen => ({
    phone: 'ParadisePhone', inventory: 'Inventaire', bank: 'Banque', jobs: 'Métiers', commands: 'Commandes', identity: 'Carte citoyenne', messages: 'Messages'
  }[screen] || 'Interface');

  const toast = (title, text) => {
    const stack = $('#toastStack');
    if (!stack) return;
    const el = document.createElement('div');
    el.className = 'toast';
    el.innerHTML = `<b>${title}</b><small>${text}</small>`;
    stack.prepend(el);
    setTimeout(() => el.remove(), 4300);
  };

  const renderInventory = () => {
    const grid = $('#inventoryGrid');
    if (!grid) return;
    grid.innerHTML = items.map(([icon, qty, label], index) => `<button class="slot ${index === 0 ? 'active' : ''}" title="${label}"><span>${icon}</span><b>${qty}</b></button>`).join('');
    $$('.slot', grid).forEach((slot, index) => {
      slot.addEventListener('click', () => {
        $$('.slot', grid).forEach(x => x.classList.remove('active'));
        slot.classList.add('active');
        const [icon, qty, label] = items[index];
        $('.item-large').textContent = icon;
        $('.item-detail strong').textContent = label;
        $('.item-detail p').textContent = `${qty} objet(s) disponible(s). Objet RP compatible avec utiliser, donner, jeter ou ajouter aux favoris.`;
      });
    });
  };

  const renderCommands = (query = '') => {
    const list = $('#commandList');
    if (!list) return;
    const q = query.trim().toLowerCase();
    const rows = commands.filter(c => !q || `${c.name} ${c.desc} ${c.syntax}`.toLowerCase().includes(q));
    list.innerHTML = rows.map(c => `<article class="command"><code>${c.name}</code><span>${c.desc}<br><small>${c.syntax}</small></span><small>${c.example}</small></article>`).join('') || '<article class="command"><code>—</code><span>Aucune commande trouvée.</span><small>Essaie un autre mot</small></article>';
  };

  const tick = () => {
    const el = $('#clock');
    if (!el) return;
    el.textContent = new Date().toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' });
  };

  navButtons.forEach(button => button.addEventListener('click', () => openScreen(button.dataset.screen)));
  $$('.close-screen').forEach(button => button.addEventListener('click', () => openScreen('home')));
  $('.chat-console')?.addEventListener('submit', event => {
    event.preventDefault();
    const input = $('.chat-console input');
    const value = input.value.trim();
    if (!value) return;
    toast('Message RP', value);
    input.value = '';
  });
  $('#commandSearch')?.addEventListener('input', event => renderCommands(event.target.value));

  renderInventory();
  renderCommands();
  tick();
  setInterval(tick, 15000);
  setTimeout(() => toast('Bienvenue', 'Maquette UI ParadiseRP chargée en mode preview.'), 650);
})();