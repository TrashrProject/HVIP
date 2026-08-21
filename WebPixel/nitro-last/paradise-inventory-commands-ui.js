(() => {
  'use strict';
  const ID = 'pr3-command-inventory';
  const COMMANDS = [
    [':inventory / :inv', 'Ouvre votre inventaire physique ParadiseRP.'],
    [':use <objet>', 'Utilise un objet utilisable. La fenêtre Inventaire reste la méthode recommandée.'],
    [':giveitem <joueur> <objet> [quantité]', 'Donne un objet à un joueur dans la même room. :give reste réservé à la commande staff historique.'],
    [':weight', 'Affiche le poids actuel et la capacité serveur de votre inventaire.']
  ];
  const esc = value => String(value ?? '').replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('>','&gt;').replaceAll('"','&quot;').replaceAll("'",'&#039;');

  function inject() {
    if (document.getElementById(ID)) return true;
    const body = document.querySelector('#paradise-rp-hud .pr2-command-body');
    if (!body) return false;
    const section = document.createElement('div');
    section.id = ID;
    section.innerHTML = `<div class="pr2-command-category" style="margin-top:12px">INVENTAIRE</div>${COMMANDS.map(([cmd,desc]) => `<div class="pr2-command-row"><code>${esc(cmd)}</code><span>${esc(desc)}</span></div>`).join('')}`;
    body.appendChild(section);
    return true;
  }

  function boot() {
    if (inject()) return;
    const observer = new MutationObserver(() => { if (inject()) observer.disconnect(); });
    observer.observe(document.body, { childList:true, subtree:true });
    window.setTimeout(() => observer.disconnect(), 30000);
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once:true });
  else boot();
})();
