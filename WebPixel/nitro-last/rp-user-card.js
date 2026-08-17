/* ParadiseRP — fiche joueur dynamique, compacte et fidèle à la carte RP */
(() => {
  'use strict';

  const REWARD_RE = /(putuacion|puntuaci[oó]n|puntuacion)?\s*recompensas?\s*:?\s*(\d+)?/i;
  const ROLE_HINT_RE = /(développeur|developpeur|officiel|staff|police|médecin|medecin|mécano|mecano|civil|citoyen|avocat|juge|agent|chef|directeur|gang|maf|job|emploi|métier|metier)/i;
  const OVERLAY_ID = 'hvip-rp-modern-card';
  let currentSource = null;

  function leaves(root) {
    return Array.from(root.querySelectorAll('*')).filter(el => el instanceof HTMLElement && !el.children.length && !!(el.textContent || '').trim());
  }

  function findCard(start) {
    let el = start, best = null;
    for (let i = 0; el && i < 9; i++, el = el.parentElement) {
      if (!(el instanceof HTMLElement)) continue;
      const r = el.getBoundingClientRect();
      if (r.width >= 140 && r.width <= 420 && r.height >= 100 && r.height <= 520) best = el;
    }
    return best;
  }

  function extractData(card, rewardNode) {
    const nodes = leaves(card);
    const cr = card.getBoundingClientRect();
    let username = nodes.find(el => {
      const t = (el.textContent || '').trim();
      if (!t || t.length > 28 || REWARD_RE.test(t) || ROLE_HINT_RE.test(t) || /^(x|×|✕|\d+)$/i.test(t)) return false;
      return el.getBoundingClientRect().top <= cr.top + Math.min(70, cr.height * .35);
    });
    if (!username) username = nodes.find(el => {
      const t = (el.textContent || '').trim();
      return t && t.length <= 28 && !REWARD_RE.test(t) && !ROLE_HINT_RE.test(t) && !/^(x|×|✕|\d+)$/i.test(t);
    });

    const role = nodes.find(el => ROLE_HINT_RE.test((el.textContent || '').trim()) && !REWARD_RE.test((el.textContent || '').trim()));
    const rewardMatch = ((rewardNode && rewardNode.textContent) || '').match(REWARD_RE);
    const images = Array.from(card.querySelectorAll('img')).filter(img => {
      const r = img.getBoundingClientRect();
      return r.width > 10 && r.height > 10 && img.src;
    }).sort((a,b) => {
      const ar = a.getBoundingClientRect(), br = b.getBoundingClientRect();
      return (br.width * br.height) - (ar.width * ar.height);
    });

    const avatar = images[0] || null;
    const badge = images.length > 1 ? images[images.length - 1] : null;
    const closeEl = Array.from(card.querySelectorAll('button,[role="button"],div,span')).find(el => {
      const t = (el.textContent || '').trim(), cls = String(el.className || ''), r = el.getBoundingClientRect();
      return (t === '×' || t === '✕' || t === 'x' || /close/i.test(cls)) && r.width < 60 && r.height < 60;
    }) || null;

    return {
      username: username ? (username.textContent || '').trim() : 'Joueur',
      role: role ? (role.textContent || '').trim() : 'Citoyen',
      reward: rewardMatch && rewardMatch[2] ? rewardMatch[2] : '0',
      avatar: avatar ? avatar.src : '',
      badge: badge && badge !== avatar ? badge.src : '',
      closeEl
    };
  }

  function ensureOverlay() {
    let o = document.getElementById(OVERLAY_ID);
    if (o) return o;

    o = document.createElement('aside');
    o.id = OVERLAY_ID;
    o.className = 'hvip-rp-dossier';
    o.innerHTML = `
      <div class="hvip-rp-dossier-accent"></div>
      <header class="hvip-rp-dossier-head">
        <span class="hvip-rp-eyebrow">Joueur</span>
        <button type="button" class="hvip-rp-dossier-close" aria-label="Fermer">×</button>
      </header>
      <section class="hvip-rp-identity">
        <div class="hvip-rp-portrait">
          <div class="hvip-rp-portrait-grid"></div>
          <img class="hvip-rp-dossier-avatar" alt="Avatar">
          <span class="hvip-rp-id-label">IDENTITÉ</span>
        </div>
        <div class="hvip-rp-person">
          <strong class="hvip-rp-dossier-name">Joueur</strong>
          <span class="hvip-rp-dossier-sub">Profil rôleplay</span>
          <div class="hvip-rp-badge-box">
            <div class="hvip-rp-badge-stage"><img class="hvip-rp-dossier-badge" alt="Badge"></div>
          </div>
        </div>
      </section>
      <section class="hvip-rp-info-card">
        <div class="hvip-rp-role-line"><span class="hvip-rp-role-mark">✎</span><strong class="hvip-rp-dossier-role">Citoyen</strong></div>
      </section>
      <section class="hvip-rp-rep-card">
        <div class="hvip-rp-rep-top"><div><strong class="hvip-rp-dossier-reward">0</strong></div><span class="hvip-rp-rep-grade">NEUTRE</span></div>
        <div class="hvip-rp-rep-track"><span></span></div>
      </section>
      <footer class="hvip-rp-dossier-foot"></footer>`;

    document.body.appendChild(o);
    o.querySelector('.hvip-rp-dossier-close').addEventListener('click', () => {
      o.classList.remove('is-visible');
      if (currentSource) currentSource.classList.remove('hvip-rp-source-card');
      const c = o.__sourceClose;
      if (c && document.documentElement.contains(c)) { try { c.click(); } catch (_) {} }
      currentSource = null;
    });
    return o;
  }

  function render(card, rewardNode) {
    if (!card || !rewardNode) return;
    const data = extractData(card, rewardNode), o = ensureOverlay();
    if (currentSource && currentSource !== card) currentSource.classList.remove('hvip-rp-source-card');
    currentSource = card;
    card.classList.add('hvip-rp-source-card');

    o.querySelector('.hvip-rp-eyebrow').textContent = data.username;
    o.querySelector('.hvip-rp-dossier-name').textContent = data.username;
    o.querySelector('.hvip-rp-dossier-role').textContent = data.role;
    o.querySelector('.hvip-rp-dossier-reward').textContent = data.reward;

    const av = o.querySelector('.hvip-rp-dossier-avatar');
    if (data.avatar) { av.src = data.avatar; av.style.display = ''; }
    else { av.removeAttribute('src'); av.style.display = 'none'; }

    const stage = o.querySelector('.hvip-rp-badge-stage');
    const bd = o.querySelector('.hvip-rp-dossier-badge');
    stage.classList.toggle('is-empty', !data.badge);
    if (data.badge) { bd.src = data.badge; bd.style.display = ''; }
    else { bd.removeAttribute('src'); bd.style.display = 'none'; }

    o.__sourceClose = data.closeEl;
    o.dataset.player = data.username;
    o.classList.add('is-visible');
  }

  function scan(root = document) {
    const list = root === document ? Array.from(document.querySelectorAll('*')) : [root, ...root.querySelectorAll('*')];
    for (const el of list) {
      if (!(el instanceof HTMLElement)) continue;
      const text = (el.textContent || '').trim();
      if (!text || el.children.length > 3 || !REWARD_RE.test(text)) continue;
      const card = findCard(el);
      if (card) render(card, el);
    }
  }

  function refresh() {
    scan();
    if (!currentSource || !document.documentElement.contains(currentSource)) return;
    const reward = leaves(currentSource).find(el => REWARD_RE.test((el.textContent || '').trim()));
    if (reward) render(currentSource, reward);
  }

  function start() {
    ensureOverlay();
    scan();
    const obs = new MutationObserver(() => {
      clearTimeout(window.__hvipRpRefreshTimer);
      window.__hvipRpRefreshTimer = setTimeout(refresh, 35);
    });
    obs.observe(document.body, { childList: true, subtree: true, characterData: true });
    document.addEventListener('click', () => {
      setTimeout(refresh, 50);
      setTimeout(refresh, 160);
    }, true);
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', start, { once: true });
  else start();
})();
