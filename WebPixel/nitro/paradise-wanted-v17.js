(() => {
  'use strict';

  const NATIVE_SELECTOR = '.nitro-alert.nitro-alert-wanted';
  const ROOT_ID = 'paradise-wanted-v17';
  const STYLE_ID = 'paradise-wanted-v17-style';
  const INTERACTIVE_SELECTOR = 'button,input,textarea,select,a,[role="button"],[contenteditable="true"]';

  let scheduled = false;
  let selectedName = '';
  let manuallyClosed = false;
  let lastNativeCount = 0;

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
    const primary = box.querySelector('.text-primary,[class*="text-primary"],strong,b');
    if (primary) {
      const value = normalize(primary.textContent).replace(/[⭐★✪]/g, '').trim();
      if (value) return value;
    }

    const clone = box.cloneNode(true);
    clone.querySelectorAll('.wanted-avatar,.avatar-image').forEach(node => node.remove());
    return normalize(clone.textContent).replace(/[⭐★✪]/g, '').trim() || 'Inconnu';
  }

  function collectUsers(natives) {
    const users = [];
    const seen = new Set();

    natives.forEach(native => {
      native.querySelectorAll('.wanted-box').forEach((box, index) => {
        const name = extractName(box);
        const key = name.toLowerCase();
        if (!name || seen.has(key)) return;
        seen.add(key);

        users.push({
          id: `${key}-${index}`,
          name,
          stars: countStars(box.textContent),
          avatar: box.querySelector('.wanted-avatar')?.cloneNode(true) || null
        });
      });
    });

    return users;
  }

  function starsMarkup(stars) {
    let html = '';
    for (let i = 1; i <= 5; i++) html += i <= stars ? '<span>★</span>' : '<span class="off">★</span>';
    return html;
  }

  function ensureStyle() {
    if (document.getElementById(STYLE_ID)) return;

    const style = document.createElement('style');
    style.id = STYLE_ID;
    style.textContent = `
#${ROOT_ID},#${ROOT_ID} *{box-sizing:border-box}
#${ROOT_ID}{
  --bg:#0b1822;--panel:#142936;--panel2:#19313f;--line:#294858;--line2:#365b6e;
  --blue:#0d6f9e;--blue2:#1285b7;--cyan:#45bdea;--text:#f7fafc;--muted:#a9bbc5;
  --soft:#7f949f;--yellow:#ffc72c;--red:#8a3e45;--red2:#aa545c;
  position:fixed;left:50%;top:50%;transform:translate(-50%,-50%);z-index:9999;
  width:min(760px,calc(100vw - 18px));height:min(360px,calc(100vh - 18px));min-width:620px;min-height:320px;
  display:flex;flex-direction:column;overflow:hidden;border:1px solid #3e6477;border-radius:10px;
  background:var(--bg);color:var(--text);font-family:Inter,Arial,sans-serif;box-shadow:0 16px 38px rgba(0,0,0,.34);
}
#${ROOT_ID}.is-dragging{cursor:grabbing}
#${ROOT_ID} .prw17-topbar{height:48px;flex:0 0 48px;display:grid;grid-template-columns:165px 1fr auto 30px;align-items:center;gap:10px;padding:0 10px;background:linear-gradient(90deg,#0b6c98,#09587c 62%,#08465f);border-bottom:1px solid rgba(255,255,255,.1);cursor:grab;user-select:none}
#${ROOT_ID} .prw17-brand{display:flex;align-items:center;gap:8px;min-width:0}
#${ROOT_ID} .prw17-logo{width:28px;height:28px;display:grid;place-items:center;flex:0 0 28px;border-radius:6px;background:#e0f1f9;color:#0b5f86;font-size:15px;font-weight:900;box-shadow:0 1px 3px rgba(0,0,0,.2)}
#${ROOT_ID} .prw17-brand strong{display:block;font-size:15px;line-height:1;font-weight:900}
#${ROOT_ID} .prw17-brand span{display:block;margin-top:3px;color:#cde4ef;font-size:7.5px;letter-spacing:.8px;font-weight:700}
#${ROOT_ID} .prw17-heading{min-width:0;padding-left:11px;border-left:1px solid rgba(255,255,255,.18)}
#${ROOT_ID} .prw17-heading strong{display:block;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;font-size:19px;line-height:1;font-weight:900;letter-spacing:-.25px}
#${ROOT_ID} .prw17-heading span{display:block;margin-top:3px;color:#c8e0eb;font-size:9px}
#${ROOT_ID} .prw17-motto{color:#c4dbe5;font-size:7px;font-weight:850;letter-spacing:.45px;white-space:nowrap}
#${ROOT_ID} .prw17-motto b{color:#5fc5eb;margin:0 3px}
#${ROOT_ID} .prw17-close{width:29px;height:29px;padding:0;border:1px solid #7a3339;border-radius:7px;background:#d14b47;color:#fff;font-size:18px;font-weight:900;line-height:1;cursor:pointer;box-shadow:0 2px 4px rgba(0,0,0,.22)}
#${ROOT_ID} .prw17-close:hover{filter:brightness(1.08)}
#${ROOT_ID} .prw17-body{display:grid;grid-template-columns:205px minmax(0,1fr);gap:7px;flex:1;min-height:0;padding:7px;background:#0a1720}
#${ROOT_ID} .prw17-side,#${ROOT_ID} .prw17-detail{min-height:0;overflow:hidden;border:1px solid var(--line);border-radius:7px;background:linear-gradient(180deg,#162d3a,#132733)}
#${ROOT_ID} .prw17-side{display:flex;flex-direction:column}
#${ROOT_ID} .prw17-tabs{height:32px;flex:0 0 32px;display:flex;border-bottom:1px solid var(--line);background:#142a36}
#${ROOT_ID} .prw17-tab{flex:1;border:0;border-bottom:2px solid transparent;background:transparent;color:#9fb0ba;font-size:10px;font-weight:900;cursor:pointer}
#${ROOT_ID} .prw17-tab.is-active{background:#1b4356;border-bottom-color:var(--cyan);color:#fff}
#${ROOT_ID} .prw17-panel{display:flex;flex-direction:column;flex:1;min-height:0}
#${ROOT_ID} .prw17-panel[hidden]{display:none!important}
#${ROOT_ID} .prw17-search-wrap{position:relative;padding:6px}
#${ROOT_ID} .prw17-search-icon{position:absolute;left:15px;top:50%;transform:translateY(-50%);color:#86a0ad;font-size:12px;pointer-events:none}
#${ROOT_ID} .prw17-search{width:100%;height:29px;padding:0 9px 0 27px;border:1px solid #31505f;border-radius:6px;outline:0;background:#0c1c26;color:#fff;font-size:10.5px}
#${ROOT_ID} .prw17-search::placeholder{color:#8ba0aa}
#${ROOT_ID} .prw17-list{flex:1;min-height:0;overflow-y:auto;padding:0 6px 6px}
#${ROOT_ID} .prw17-list::-webkit-scrollbar{width:4px}#${ROOT_ID} .prw17-list::-webkit-scrollbar-thumb{background:#385767;border-radius:8px}
#${ROOT_ID} .prw17-card{width:100%;min-height:47px;display:grid;grid-template-columns:34px minmax(0,1fr) 10px;align-items:center;gap:7px;margin-bottom:5px;padding:5px;border:1px solid #315061;border-radius:7px;background:#18303d;color:#fff;text-align:left;cursor:pointer}
#${ROOT_ID} .prw17-card:hover{background:#1e3a49}#${ROOT_ID} .prw17-card.is-selected{background:linear-gradient(90deg,#164c65,#1d617d);border-color:#42bae8;box-shadow:inset 2px 0 0 #58c8ef}
#${ROOT_ID} .prw17-avatar{width:32px;height:32px;position:relative;overflow:hidden;border:1px solid rgba(255,255,255,.15);border-radius:5px;background:#0e668f}
#${ROOT_ID} .prw17-avatar .wanted-avatar{width:32px!important;height:32px!important;position:relative!important;overflow:visible!important;border:0!important;background:transparent!important}
#${ROOT_ID} .prw17-avatar .avatar-image{position:absolute!important;left:50%!important;top:50%!important;transform:translate(-50%,-53%) scale(.78)!important;transform-origin:center!important}
#${ROOT_ID} .prw17-card-name{overflow:hidden;text-overflow:ellipsis;white-space:nowrap;font-size:12px;font-weight:900;line-height:1}
#${ROOT_ID} .prw17-card-stars{margin-top:4px;color:var(--yellow);font-size:10px;line-height:1}.prw17-card-stars .off{color:#536773}
#${ROOT_ID} .prw17-arrow{justify-self:center;color:#a6bbc5;font-size:14px}
#${ROOT_ID} .prw17-count{padding:5px 7px;border-top:1px solid var(--line);background:#10232e;color:#94a8b3;font-size:8.5px}
#${ROOT_ID} .prw17-count:before{content:"";display:inline-block;width:5px;height:5px;margin-right:5px;border-radius:50%;background:#45bcea;vertical-align:1px}
#${ROOT_ID} .prw17-detail{display:flex;flex-direction:column;padding:8px;background:linear-gradient(180deg,#172f3d,#132936)}
#${ROOT_ID} .prw17-head{display:grid;grid-template-columns:82px minmax(0,1fr);gap:11px;align-items:center;padding-bottom:7px;border-bottom:1px solid var(--line)}
#${ROOT_ID} .prw17-profile-avatar{width:82px;height:86px;position:relative;overflow:hidden;border:1px solid #2c8ab4;border-radius:7px;background:radial-gradient(circle at 50% 38%,#1a7fa8,#10658d 70%,#0c5678)}
#${ROOT_ID} .prw17-profile-avatar .wanted-avatar{width:82px!important;height:86px!important;position:relative!important;overflow:visible!important;border:0!important;background:transparent!important}
#${ROOT_ID} .prw17-profile-avatar .avatar-image{position:absolute!important;left:50%!important;top:50%!important;transform:translate(-50%,-51%) scale(1.4)!important;transform-origin:center!important}
#${ROOT_ID} .prw17-name-row{display:flex;align-items:center;gap:7px;flex-wrap:wrap}
#${ROOT_ID} .prw17-name{font-size:23px;font-weight:900;line-height:1;letter-spacing:-.3px}
#${ROOT_ID} .prw17-status{height:20px;display:inline-flex;align-items:center;padding:0 8px;border:1px solid #ae5960;border-radius:5px;background:linear-gradient(180deg,#a34d54,#8c3e46);color:#ffecee;font-size:8.5px;font-weight:900}
#${ROOT_ID} .prw17-identity{display:grid;grid-template-columns:1fr 1fr;gap:16px;margin-top:8px}
#${ROOT_ID} .prw17-label{color:#aec0ca;font-size:9px;line-height:1;font-weight:900;letter-spacing:.42px;text-transform:uppercase}
#${ROOT_ID} .prw17-value{margin-top:3px;color:#f5f8fa;font-size:11px;line-height:1.15}
#${ROOT_ID} .prw17-danger-row{display:flex;align-items:center;gap:7px;margin-top:3px;flex-wrap:wrap}
#${ROOT_ID} .prw17-stars{color:var(--yellow);font-size:12px;line-height:1}.prw17-stars .off{color:#536773}
#${ROOT_ID} .prw17-level{padding:3px 7px;border:1px solid #365563;border-radius:999px;background:#213a47;color:#d1dbe0;font-size:8.5px;font-weight:850}
#${ROOT_ID} .prw17-facts{display:grid;grid-template-columns:1fr 1fr;margin-top:6px;border:1px solid #315061;border-radius:7px;overflow:hidden;background:#17303d}
#${ROOT_ID} .prw17-fact{min-height:41px;display:grid;grid-template-columns:28px minmax(0,1fr);gap:7px;align-items:center;padding:5px 8px}
#${ROOT_ID} .prw17-fact:nth-child(1){border-right:1px solid #294958}#${ROOT_ID} .prw17-fact.is-wide{grid-column:1/-1;border-top:1px solid #294958;min-height:38px}
#${ROOT_ID} .prw17-icon{width:27px;height:27px;display:grid;place-items:center;border:1px solid rgba(255,255,255,.09);border-radius:6px;background:linear-gradient(180deg,#1785b4,#106993);color:#fff;font-size:12px;font-weight:900}
#${ROOT_ID} .prw17-info{display:grid;grid-template-columns:28px 1fr;gap:7px;align-items:center;margin-top:6px;padding:6px 8px;border-left:2px solid #43b4df;border-radius:5px;background:#102631}
#${ROOT_ID} .prw17-warning{display:grid;grid-template-columns:24px 1fr;gap:7px;align-items:center;margin-top:6px;padding:5px 8px;border:1px solid #934c53;border-radius:6px;background:linear-gradient(90deg,#6d343a,#7d3d43)}
#${ROOT_ID} .prw17-warning-icon{width:22px;height:22px;display:grid;place-items:center;border-radius:50%;background:#efa1a5;color:#6e2e34;font-size:12px;font-weight:900}
#${ROOT_ID} .prw17-warning strong{display:block;color:#ffe7e8;font-size:8.5px;line-height:1.08}#${ROOT_ID} .prw17-warning span{display:block;margin-top:2px;color:#f0c4c6;font-size:8.3px;line-height:1.08}
#${ROOT_ID} .prw17-empty{padding:15px;color:#92a7b1;font-size:10px;text-align:center}
#${ROOT_ID} .prw17-info-panel{padding:12px;color:#b5c4cb;font-size:10.5px;line-height:1.5}#${ROOT_ID} .prw17-info-panel strong{display:block;margin-bottom:7px;color:#fff;font-size:12px}
@media(max-width:660px){#${ROOT_ID}{min-width:0;width:calc(100vw - 10px);height:calc(100vh - 10px);min-height:0}#${ROOT_ID} .prw17-motto{display:none}#${ROOT_ID} .prw17-body{grid-template-columns:190px 1fr;padding:5px;gap:5px}}
@media(max-height:370px) and (min-width:661px){#${ROOT_ID}{height:calc(100vh - 8px);min-height:0}#${ROOT_ID} .prw17-topbar{height:42px;flex-basis:42px}#${ROOT_ID} .prw17-body{padding:5px;gap:5px}#${ROOT_ID} .prw17-head{grid-template-columns:70px 1fr;gap:8px;padding-bottom:4px}#${ROOT_ID} .prw17-profile-avatar{width:70px;height:72px}#${ROOT_ID} .prw17-profile-avatar .wanted-avatar{width:70px!important;height:72px!important}#${ROOT_ID} .prw17-name{font-size:20px}#${ROOT_ID} .prw17-identity{margin-top:5px}#${ROOT_ID} .prw17-facts{margin-top:4px}#${ROOT_ID} .prw17-fact{min-height:34px;padding:3px 6px}#${ROOT_ID} .prw17-info,#${ROOT_ID} .prw17-warning{margin-top:4px;padding:4px 6px}}
`;
    document.head.appendChild(style);
  }

  function hideNatives(natives) {
    natives.forEach(native => {
      native.style.setProperty('display', 'none', 'important');
      native.setAttribute('aria-hidden', 'true');
    });
  }

  function closeNativeWindows(natives) {
    natives.forEach(native => {
      const candidates = [...native.querySelectorAll('button,[class*="close"],[class*="Close"]')];
      const close = candidates.find(node => !node.closest(`#${ROOT_ID}`));
      if (close && typeof close.click === 'function') {
        try { close.click(); } catch (_) {}
      }
    });
  }

  function avatarWrap(user, className) {
    const wrap = document.createElement('div');
    wrap.className = className;
    if (user.avatar) wrap.appendChild(user.avatar.cloneNode(true));
    return wrap;
  }

  function createRoot() {
    const root = document.createElement('div');
    root.id = ROOT_ID;
    root.innerHTML = `
      <div class="prw17-topbar">
        <div class="prw17-brand"><div class="prw17-logo">P</div><div><strong>ParadiseRP</strong><span>LAKE PLACID</span></div></div>
        <div class="prw17-heading"><strong>Personnes recherchées</strong><span>Pour une ville plus sûre</span></div>
        <div class="prw17-motto">SIGNALER <b>•</b> ENQUÊTER <b>•</b> PROTÉGER</div>
        <button class="prw17-close" type="button" aria-label="Fermer">×</button>
      </div>
      <div class="prw17-body">
        <aside class="prw17-side">
          <div class="prw17-tabs"><button class="prw17-tab is-active" data-tab="list" type="button">LISTE</button><button class="prw17-tab" data-tab="info" type="button">INFOS</button></div>
          <section class="prw17-panel" data-panel="list"><div class="prw17-search-wrap"><span class="prw17-search-icon">⌕</span><input class="prw17-search" type="search" placeholder="Rechercher un suspect..." autocomplete="off"></div><div class="prw17-list"></div><div class="prw17-count"></div></section>
          <section class="prw17-panel" data-panel="info" hidden><div class="prw17-info-panel"><strong>Registre Wanted</strong><p>Individus actuellement signalés par les autorités de Lake Placid.</p><p>Le niveau de danger reprend les étoiles reçues par le client.</p></div></section>
        </aside>
        <main class="prw17-detail"></main>
      </div>`;

    document.body.appendChild(root);
    bindRootEvents(root);
    return root;
  }

  function bindRootEvents(root) {
    root.querySelectorAll('[data-tab]').forEach(button => {
      button.addEventListener('click', () => {
        const tab = button.dataset.tab;
        root.querySelectorAll('[data-tab]').forEach(item => item.classList.toggle('is-active', item === button));
        root.querySelectorAll('[data-panel]').forEach(panel => panel.hidden = panel.dataset.panel !== tab);
      });
    });

    root.querySelector('.prw17-search').addEventListener('input', () => render());

    root.querySelector('.prw17-close').addEventListener('click', event => {
      event.preventDefault();
      event.stopPropagation();
      manuallyClosed = true;
      const natives = [...document.querySelectorAll(NATIVE_SELECTOR)];
      closeNativeWindows(natives);
      root.remove();
    });

    let drag = null;
    root.addEventListener('pointerdown', event => {
      if (event.button !== 0 || event.target.closest(INTERACTIVE_SELECTOR)) return;
      const rect = root.getBoundingClientRect();
      drag = { pointerId:event.pointerId,startX:event.clientX,startY:event.clientY,left:rect.left,top:rect.top,width:rect.width,height:rect.height };
      root.style.left = `${rect.left}px`;
      root.style.top = `${rect.top}px`;
      root.style.transform = 'none';
      root.classList.add('is-dragging');
      try { root.setPointerCapture(event.pointerId); } catch (_) {}
      event.preventDefault();
    });

    root.addEventListener('pointermove', event => {
      if (!drag || event.pointerId !== drag.pointerId) return;
      const dx = event.clientX - drag.startX;
      const dy = event.clientY - drag.startY;
      const margin = 4;
      const maxLeft = Math.max(margin, window.innerWidth - drag.width - margin);
      const maxTop = Math.max(margin, window.innerHeight - drag.height - margin);
      root.style.left = `${Math.min(maxLeft,Math.max(margin,drag.left + dx))}px`;
      root.style.top = `${Math.min(maxTop,Math.max(margin,drag.top + dy))}px`;
      event.preventDefault();
    });

    const finish = event => {
      if (!drag || event.pointerId !== drag.pointerId) return;
      try { root.releasePointerCapture(event.pointerId); } catch (_) {}
      drag = null;
      root.classList.remove('is-dragging');
    };
    root.addEventListener('pointerup', finish);
    root.addEventListener('pointercancel', finish);
  }

  function renderDetail(root, user) {
    const detail = root.querySelector('.prw17-detail');
    detail.innerHTML = '';
    if (!user) {
      detail.innerHTML = '<div class="prw17-empty">Aucune personne recherchée actuellement.</div>';
      return;
    }

    const head = document.createElement('div');
    head.className = 'prw17-head';
    head.appendChild(avatarWrap(user,'prw17-profile-avatar'));

    const profile = document.createElement('div');
    profile.innerHTML = `
      <div class="prw17-name-row"><div class="prw17-name"></div><div class="prw17-status">RECHERCHÉ</div></div>
      <div class="prw17-identity">
        <div><div class="prw17-label">Alias</div><div class="prw17-value">Aucun alias connu</div></div>
        <div><div class="prw17-label">Niveau de danger</div><div class="prw17-danger-row"><div class="prw17-stars">${starsMarkup(user.stars)}</div><div class="prw17-level">${dangerLabel(user.stars)}</div></div></div>
      </div>`;
    profile.querySelector('.prw17-name').textContent = user.name;
    head.appendChild(profile);
    detail.appendChild(head);

    const facts = document.createElement('div');
    facts.className = 'prw17-facts';
    facts.innerHTML = `
      <div class="prw17-fact"><div class="prw17-icon">$</div><div><div class="prw17-label">Récompense</div><div class="prw17-value">—</div></div></div>
      <div class="prw17-fact"><div class="prw17-icon">⌖</div><div><div class="prw17-label">Dernier secteur connu</div><div class="prw17-value">Inconnu</div></div></div>
      <div class="prw17-fact is-wide"><div class="prw17-icon">≡</div><div><div class="prw17-label">Motif</div><div class="prw17-value">Aucun motif communiqué.</div></div></div>`;
    detail.appendChild(facts);

    const info = document.createElement('div');
    info.className = 'prw17-info';
    info.innerHTML = '<div class="prw17-icon">i</div><div><div class="prw17-label">Informations</div><div class="prw17-value">Individu actuellement recherché par les autorités de Lake Placid.</div></div>';
    detail.appendChild(info);

    const warning = document.createElement('div');
    warning.className = 'prw17-warning';
    warning.innerHTML = '<div class="prw17-warning-icon">!</div><div><strong>NE TENTEZ PAS D’INTERPELLER CET INDIVIDU.</strong><span>Contactez les forces de l’ordre de ParadiseRP.</span></div>';
    detail.appendChild(warning);
  }

  function render() {
    const natives = [...document.querySelectorAll(NATIVE_SELECTOR)];
    if (!natives.length) return;

    ensureStyle();
    hideNatives(natives);

    let root = document.getElementById(ROOT_ID);
    if (!root) root = createRoot();

    const users = collectUsers(natives);
    const query = normalize(root.querySelector('.prw17-search').value).toLowerCase();
    const filtered = users.filter(user => !query || user.name.toLowerCase().includes(query));
    const selected = users.find(user => user.name === selectedName) || users[0] || null;
    selectedName = selected ? selected.name : '';

    const list = root.querySelector('.prw17-list');
    list.innerHTML = '';

    if (!filtered.length) list.innerHTML = '<div class="prw17-empty">Aucun résultat.</div>';

    filtered.forEach(user => {
      const card = document.createElement('button');
      card.type = 'button';
      card.className = 'prw17-card';
      if (selected && selected.name === user.name) card.classList.add('is-selected');
      card.appendChild(avatarWrap(user,'prw17-avatar'));

      const meta = document.createElement('div');
      meta.innerHTML = `<div class="prw17-card-name"></div><div class="prw17-card-stars">${starsMarkup(user.stars)}</div>`;
      meta.querySelector('.prw17-card-name').textContent = user.name;
      card.appendChild(meta);

      const arrow = document.createElement('div');
      arrow.className = 'prw17-arrow';
      arrow.textContent = '›';
      card.appendChild(arrow);

      card.addEventListener('click', () => {
        selectedName = user.name;
        render();
      });
      list.appendChild(card);
    });

    root.querySelector('.prw17-count').textContent = `${users.length} recherché${users.length > 1 ? 's' : ''}`;
    renderDetail(root, selected);
  }

  function scan() {
    scheduled = false;
    const natives = [...document.querySelectorAll(NATIVE_SELECTOR)];

    if (!natives.length) {
      lastNativeCount = 0;
      manuallyClosed = false;
      document.getElementById(ROOT_ID)?.remove();
      return;
    }

    if (manuallyClosed) {
      hideNatives(natives);
      return;
    }

    if (lastNativeCount === 0 || natives.length !== lastNativeCount || !document.getElementById(ROOT_ID)) render();
    else render();

    lastNativeCount = natives.length;
  }

  function scheduleScan() {
    if (scheduled) return;
    scheduled = true;
    requestAnimationFrame(scan);
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', scheduleScan, { once:true });
  else scheduleScan();

  new MutationObserver(scheduleScan).observe(document.documentElement,{childList:true,subtree:true,characterData:true});
})();
