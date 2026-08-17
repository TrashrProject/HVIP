/* ParadiseRP — amélioration non destructive de la carte joueur */
(() => {
  'use strict';

  const REWARD_RE = /(putuacion|puntuaci[oó]n|puntuacion)?\s*recompensas?\s*:?\s*(\d+)?/i;
  const ROLE_HINT_RE = /(développeur|developpeur|officiel|staff|police|médecin|medecin|mécano|mecano|civil|citoyen|avocat|juge|agent|chef|directeur|gang|maf|job|emploi|métier|metier)/i;

  function compactCardCandidate(start) {
    let el = start;
    let best = null;
    for (let i = 0; el && i < 7; i++, el = el.parentElement) {
      if (!(el instanceof HTMLElement)) continue;
      const r = el.getBoundingClientRect();
      if (r.width >= 140 && r.width <= 360 && r.height >= 100 && r.height <= 460) best = el;
    }
    return best;
  }

  function visibleTextNodes(root) {
    return Array.from(root.querySelectorAll('*')).filter(el => {
      if (!(el instanceof HTMLElement)) return false;
      if (el.children.length) return false;
      const t = (el.textContent || '').trim();
      return !!t;
    });
  }

  function decorate(card, rewardNode) {
    if (!card || card.dataset.hvipRpDecorated === '1') return;
    card.dataset.hvipRpDecorated = '1';
    card.classList.add('hvip-rp-user-card');

    // Bouton fermer existant
    const close = Array.from(card.querySelectorAll('button, [role="button"], div, span')).find(el => {
      const t = (el.textContent || '').trim();
      const cls = String(el.className || '');
      return (t === '×' || t === '✕' || t === 'x' || /close/i.test(cls)) && el.getBoundingClientRect().width < 55;
    });
    if (close) close.classList.add('hvip-rp-close');

    const leaves = visibleTextNodes(card);

    // Nom: première valeur courte située dans la partie haute, hors récompenses.
    const cr = card.getBoundingClientRect();
    const username = leaves.find(el => {
      const t = (el.textContent || '').trim();
      if (!t || t.length > 28 || REWARD_RE.test(t) || ROLE_HINT_RE.test(t)) return false;
      if (/^(x|×|✕|\d+)$/i.test(t)) return false;
      const r = el.getBoundingClientRect();
      return r.top <= cr.top + Math.min(55, cr.height * .28);
    });
    if (username) username.classList.add('hvip-rp-username');

    // Ligne métier / rôle existante, sans changer sa valeur dynamique.
    const role = leaves.find(el => ROLE_HINT_RE.test((el.textContent || '').trim()) && !REWARD_RE.test((el.textContent || '').trim()));
    if (role) {
      const holder = role.parentElement && role.parentElement !== card ? role.parentElement : role;
      holder.classList.add('hvip-rp-role');
    }

    // Récompenses existantes.
    const rewardHolder = rewardNode.parentElement && rewardNode.parentElement !== card ? rewardNode.parentElement : rewardNode;
    rewardHolder.classList.add('hvip-rp-rewards');
    const match = (rewardNode.textContent || '').match(REWARD_RE);
    if (match && match[2]) {
      // On garde le texte existant et on met uniquement la valeur en évidence si possible.
      const raw = rewardNode.textContent;
      const idx = raw.lastIndexOf(match[2]);
      if (idx >= 0 && rewardNode.children.length === 0) {
        const before = raw.slice(0, idx).replace(/Putuacion/ig, 'Réputation').replace(/Puntuaci[oó]n/ig, 'Réputation');
        rewardNode.textContent = before;
        const value = document.createElement('strong');
        value.className = 'hvip-rp-reward-value';
        value.textContent = match[2];
        rewardNode.appendChild(value);
      }
    }

    // Images: le plus grand visuel = avatar, le plus petit = badge.
    const images = Array.from(card.querySelectorAll('img')).filter(img => {
      const r = img.getBoundingClientRect();
      return r.width > 10 && r.height > 10;
    });
    if (images.length) {
      images.sort((a, b) => {
        const ar = a.getBoundingClientRect();
        const br = b.getBoundingClientRect();
        return (br.width * br.height) - (ar.width * ar.height);
      });
      const avatar = images[0];
      avatar.classList.add('hvip-rp-avatar');
      if (avatar.parentElement && avatar.parentElement !== card) avatar.parentElement.classList.add('hvip-rp-avatar-wrap');

      if (images[1]) {
        const badge = images[images.length - 1];
        badge.classList.add('hvip-rp-badge');
        if (badge.parentElement && badge.parentElement !== card) badge.parentElement.classList.add('hvip-rp-badge-wrap');
      }
    }

    // Petit indicateur RP en ligne, purement visuel, à côté du nom.
    if (username && !username.querySelector('.hvip-rp-online-dot')) {
      const dot = document.createElement('span');
      dot.className = 'hvip-rp-online-dot';
      username.prepend(dot);
    }
  }

  function scan(root = document) {
    const all = root === document ? document.querySelectorAll('*') : [root, ...root.querySelectorAll('*')];
    for (const el of all) {
      if (!(el instanceof HTMLElement) || el.dataset.hvipRewardScanned === '1') continue;
      const text = (el.textContent || '').trim();
      if (!text || el.children.length > 2 || !REWARD_RE.test(text)) continue;
      el.dataset.hvipRewardScanned = '1';
      const card = compactCardCandidate(el);
      if (card) decorate(card, el);
    }
  }

  const start = () => {
    scan();
    const observer = new MutationObserver(mutations => {
      for (const mutation of mutations) {
        for (const node of mutation.addedNodes) {
          if (node instanceof HTMLElement) scan(node);
        }
      }
    });
    observer.observe(document.body, { childList: true, subtree: true });
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', start, { once: true });
  else start();
})();
