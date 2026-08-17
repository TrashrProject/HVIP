/* ParadiseRP — carte joueur dynamique selon le joueur sélectionné */
(() => {
  'use strict';

  const REWARD_RE = /(putuacion|puntuaci[oó]n|puntuacion)?\s*recompensas?\s*:?\s*(\d+)?/i;
  const ROLE_HINT_RE = /(développeur|developpeur|officiel|staff|police|médecin|medecin|mécano|mecano|civil|citoyen|avocat|juge|agent|chef|directeur|gang|maf|job|emploi|métier|metier)/i;

  function compactCardCandidate(start) {
    let el = start;
    let best = null;
    for (let i = 0; el && i < 8; i++, el = el.parentElement) {
      if (!(el instanceof HTMLElement)) continue;
      const r = el.getBoundingClientRect();
      if (r.width >= 140 && r.width <= 380 && r.height >= 100 && r.height <= 500) best = el;
    }
    return best;
  }

  function leaves(root) {
    return Array.from(root.querySelectorAll('*')).filter(el => {
      if (!(el instanceof HTMLElement) || el.children.length) return false;
      return !!(el.textContent || '').trim();
    });
  }

  function clearDecorations(card) {
    card.querySelectorAll('.hvip-rp-username,.hvip-rp-role,.hvip-rp-rewards,.hvip-rp-avatar,.hvip-rp-avatar-wrap,.hvip-rp-badge,.hvip-rp-badge-wrap,.hvip-rp-close')
      .forEach(el => el.classList.remove('hvip-rp-username','hvip-rp-role','hvip-rp-rewards','hvip-rp-avatar','hvip-rp-avatar-wrap','hvip-rp-badge','hvip-rp-badge-wrap','hvip-rp-close'));
    card.querySelectorAll('.hvip-rp-online-dot').forEach(el => el.remove());
  }

  function decorate(card, rewardNode) {
    if (!card || !rewardNode) return;
    clearDecorations(card);
    card.classList.add('hvip-rp-user-card');

    const close = Array.from(card.querySelectorAll('button,[role="button"],div,span')).find(el => {
      const t = (el.textContent || '').trim();
      const cls = String(el.className || '');
      const r = el.getBoundingClientRect();
      return (t === '×' || t === '✕' || t === 'x' || /close/i.test(cls)) && r.width < 55 && r.height < 55;
    });
    if (close) close.classList.add('hvip-rp-close');

    const textNodes = leaves(card);
    const cr = card.getBoundingClientRect();
    const username = textNodes.find(el => {
      const t = (el.textContent || '').trim();
      if (!t || t.length > 28 || REWARD_RE.test(t) || ROLE_HINT_RE.test(t) || /^(x|×|✕|\d+)$/i.test(t)) return false;
      const r = el.getBoundingClientRect();
      return r.top <= cr.top + Math.min(65, cr.height * .32);
    });

    if (username) {
      username.classList.add('hvip-rp-username');
      const dot = document.createElement('span');
      dot.className = 'hvip-rp-online-dot';
      username.prepend(dot);
    }

    const role = textNodes.find(el => ROLE_HINT_RE.test((el.textContent || '').trim()) && !REWARD_RE.test((el.textContent || '').trim()));
    if (role) {
      const holder = role.parentElement && role.parentElement !== card ? role.parentElement : role;
      holder.classList.add('hvip-rp-role');
    }

    const rewardHolder = rewardNode.parentElement && rewardNode.parentElement !== card ? rewardNode.parentElement : rewardNode;
    rewardHolder.classList.add('hvip-rp-rewards');

    const raw = (rewardNode.textContent || '').trim();
    const match = raw.match(REWARD_RE);
    if (match && match[2] && rewardNode.children.length === 0) {
      const idx = raw.lastIndexOf(match[2]);
      if (idx >= 0) {
        rewardNode.textContent = raw.slice(0, idx).replace(/Putuacion/ig, 'Réputation').replace(/Puntuaci[oó]n/ig, 'Réputation');
        const value = document.createElement('strong');
        value.className = 'hvip-rp-reward-value';
        value.textContent = match[2];
        rewardNode.appendChild(value);
      }
    }

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
      if (images.length > 1) {
        const badge = images[images.length - 1];
        if (badge !== avatar) {
          badge.classList.add('hvip-rp-badge');
          if (badge.parentElement && badge.parentElement !== card) badge.parentElement.classList.add('hvip-rp-badge-wrap');
        }
      }
    }

    card.dataset.hvipRpFingerprint = (card.textContent || '').trim().slice(0, 220);
  }

  function scan(root = document) {
    const list = root === document ? Array.from(document.querySelectorAll('*')) : [root, ...root.querySelectorAll('*')];
    for (const el of list) {
      if (!(el instanceof HTMLElement)) continue;
      const text = (el.textContent || '').trim();
      if (!text || el.children.length > 3 || !REWARD_RE.test(text)) continue;
      const card = compactCardCandidate(el);
      if (card) decorate(card, el);
    }
  }

  function refreshVisibleCards() {
    document.querySelectorAll('.hvip-rp-user-card').forEach(card => {
      const rewardNode = leaves(card).find(el => REWARD_RE.test((el.textContent || '').trim()));
      if (!rewardNode) return;
      const fingerprint = (card.textContent || '').trim().slice(0, 220);
      if (fingerprint !== card.dataset.hvipRpFingerprint) decorate(card, rewardNode);
    });
    scan();
  }

  const start = () => {
    scan();
    const observer = new MutationObserver(() => {
      clearTimeout(window.__hvipRpRefreshTimer);
      window.__hvipRpRefreshTimer = setTimeout(refreshVisibleCards, 30);
    });
    observer.observe(document.body, { childList: true, subtree: true, characterData: true });

    document.addEventListener('click', () => {
      setTimeout(refreshVisibleCards, 40);
      setTimeout(refreshVisibleCards, 140);
    }, true);
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', start, { once: true });
  else start();
})();
