/* ParadiseRP — dossier joueur RP premium v4 */
(() => {
  'use strict';

  const REWARD_RE = /(putuacion|puntuaci[oó]n|puntuacion)?\s*recompensas?\s*:?\s*(\d+)?/i;
  const ROLE_HINT_RE = /(développeur|developpeur|officiel|staff|police|médecin|medecin|mécano|mecano|civil|citoyen|avocat|juge|agent|chef|directeur|gang|maf|job|emploi|métier|metier)/i;
  const OVERLAY_ID = 'hvip-rp-modern-card';
  let currentSource = null;

  const leaves = root => Array.from(root.querySelectorAll('*')).filter(el => el instanceof HTMLElement && !el.children.length && !!(el.textContent || '').trim());

  function findCard(start) {
    let el = start, best = null;
    for (let i = 0; el && i < 9; i++, el = el.parentElement) {
      if (!(el instanceof HTMLElement)) continue;
      const r = el.getBoundingClientRect();
      if (r.width >= 140 && r.width <= 420 && r.height >= 100 && r.height <= 520) best = el;
    }
    return best;
  }

  function backgroundUrl(el) {
    if (!(el instanceof HTMLElement)) return '';
    const bg = getComputedStyle(el).backgroundImage || '';
    const m = bg.match(/url\(["']?(.*?)["']?\)/i);
    return m ? m[1] : '';
  }

  function collectVisuals(card) {
    const visuals = [];
    card.querySelectorAll('img').forEach(img => {
      const r = img.getBoundingClientRect();
      if (img.src && r.width > 8 && r.height > 8) visuals.push({ url: img.src, area: r.width * r.height });
    });
    card.querySelectorAll('*').forEach(el => {
      const url = backgroundUrl(el), r = el.getBoundingClientRect();
      if (url && url !== 'none' && r.width >= 8 && r.height >= 8) visuals.push({ url, area: r.width * r.height });
    });
    return visuals.filter((v, i, arr) => arr.findIndex(x => x.url === v.url) === i);
  }

  function scoreAvatar(v) {
    const u = v.url.toLowerCase();
    let s = v.area;
    if (/avatar|figure|habbo-imaging|avatarimage/.test(u)) s += 100000;
    if (/badge|album|icon|logo/.test(u)) s -= 60000;
    return s;
  }

  function scoreBadge(v) {
    const u = v.url.toLowerCase();
    let s = Math.max(0, 50000 - v.area);
    if (/badge|album|staff|adm|rank|icon/.test(u)) s += 100000;
    if (/avatar|figure|avatarimage/.test(u)) s -= 100000;
    return s;
  }

  function dossierId(username) {
    let h = 0;
    for (let i = 0; i < username.length; i++) h = ((h << 5) - h + username.charCodeAt(i)) | 0;
    return 'RP-' + String(Math.abs(h) % 99999).padStart(5, '0');
  }

  function extractData(card, rewardNode) {
    const nodes = leaves(card), cr = card.getBoundingClientRect();
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
    const visuals = collectVisuals(card);
    const avatarVisual = visuals.slice().sort((a,b) => scoreAvatar(b) - scoreAvatar(a))[0] || null;
    const badgeVisual = visuals.filter(v => !avatarVisual || v.url !== avatarVisual.url).sort((a,b) => scoreBadge(b) - scoreBadge(a))[0] || null;
    const closeEl = Array.from(card.querySelectorAll('button,[role="button"],div,span')).find(el => {
      const t=(el.textContent||'').trim(), cls=String(el.className||''), r=el.getBoundingClientRect();
      return (t==='×'||t==='✕'||t==='x'||/close/i.test(cls)) && r.width<60 && r.height<60;
    }) || null;
    const name = username ? (username.textContent || '').trim() : 'Joueur';

    return {
      username: name,
      role: role ? (role.textContent || '').trim() : 'Citoyen',
      reward: rewardMatch && rewardMatch[2] ? rewardMatch[2] : '0',
      avatar: avatarVisual ? avatarVisual.url : '',
      badge: badgeVisual ? badgeVisual.url : '',
      dossierId: dossierId(name),
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
        <div class="hvip-rp-head-copy"><span class="hvip-rp-head-kicker">FICHE CITOYENNE · PARADISE RP</span><span class="hvip-rp-eyebrow">Joueur</span></div>
        <button type="button" class="hvip-rp-dossier-close" aria-label="Fermer">×</button>
      </header>
      <section class="hvip-rp-hero">
        <div class="hvip-rp-portrait">
          <div class="hvip-rp-portrait-grid"></div>
          <img class="hvip-rp-dossier-avatar" alt="Avatar">
          <span class="hvip-rp-id-label">DOSSIER RP</span>
          <span class="hvip-rp-presence">PRÉSENT</span>
        </div>
        <div class="hvip-rp-person">
          <span class="hvip-rp-mini-label">IDENTITÉ</span>
          <strong class="hvip-rp-dossier-name">Joueur</strong>
          <span class="hvip-rp-dossier-sub">Identité enregistrée dans la ville</span>
          <div class="hvip-rp-meta">
            <div class="hvip-rp-meta-row"><span>Dossier</span><strong class="hvip-rp-meta-id">RP-00000</strong></div>
            <div class="hvip-rp-meta-row"><span>Statut</span><strong>Actif</strong></div>
          </div>
          <div class="hvip-rp-badge-box"><div class="hvip-rp-badge-stage"><img class="hvip-rp-dossier-badge" alt="Badge"></div></div>
        </div>
      </section>
      <div class="hvip-rp-divider"></div>
      <section class="hvip-rp-info-grid">
        <div class="hvip-rp-info-card">
          <span class="hvip-rp-mini-label">FONCTION / MÉTIER</span>
          <div class="hvip-rp-role-line"><span class="hvip-rp-role-mark">◆</span><strong class="hvip-rp-dossier-role">Citoyen</strong></div>
        </div>
        <div class="hvip-rp-rep-card">
          <div class="hvip-rp-rep-top"><div><strong class="hvip-rp-dossier-reward">0</strong></div><span class="hvip-rp-rep-grade">NEUTRE</span></div>
          <div class="hvip-rp-rep-track"><span></span></div>
        </div>
      </section>
      <footer class="hvip-rp-dossier-foot"><span>Informations RP du joueur sélectionné</span><span class="hvip-rp-foot-seal">DOSSIER ACTIF</span></footer>`;

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
    o.querySelector('.hvip-rp-meta-id').textContent = data.dossierId;

    const av = o.querySelector('.hvip-rp-dossier-avatar');
    if (data.avatar) { av.src = data.avatar; av.style.display = ''; } else { av.removeAttribute('src'); av.style.display = 'none'; }

    const stage = o.querySelector('.hvip-rp-badge-stage'), bd = o.querySelector('.hvip-rp-dossier-badge');
    stage.classList.toggle('is-empty', !data.badge);
    if (data.badge) { bd.src = data.badge; bd.style.display = ''; } else { bd.removeAttribute('src'); bd.style.display = 'none'; }

    const score = Math.max(0, Number(data.reward) || 0);
    o.querySelector('.hvip-rp-rep-grade').textContent = score >= 75 ? 'RECONNU' : score >= 35 ? 'CONNU' : 'NEUTRE';
    o.querySelector('.hvip-rp-rep-track span').style.width = Math.min(100, score) + '%';

    o.__sourceClose = data.closeEl;
    o.dataset.player = data.username;
    o.classList.add('is-visible');
  }

  function scan(root=document) {
    const list = root===document ? Array.from(document.querySelectorAll('*')) : [root,...root.querySelectorAll('*')];
    for (const el of list) {
      if (!(el instanceof HTMLElement)) continue;
      const text=(el.textContent||'').trim();
      if (!text || el.children.length>3 || !REWARD_RE.test(text)) continue;
      const card=findCard(el); if(card) render(card,el);
    }
  }

  function refresh(){
    scan();
    if(!currentSource || !document.documentElement.contains(currentSource)) return;
    const reward=leaves(currentSource).find(el=>REWARD_RE.test((el.textContent||'').trim()));
    if(reward) render(currentSource,reward);
  }

  function start(){
    ensureOverlay(); scan();
    const obs=new MutationObserver(()=>{clearTimeout(window.__hvipRpRefreshTimer);window.__hvipRpRefreshTimer=setTimeout(refresh,35);});
    obs.observe(document.body,{childList:true,subtree:true,characterData:true,attributes:true,attributeFilter:['src','style','class']});
    document.addEventListener('click',()=>{setTimeout(refresh,40);setTimeout(refresh,140);},true);
  }

  if(document.readyState==='loading') document.addEventListener('DOMContentLoaded',start,{once:true}); else start();
})();
