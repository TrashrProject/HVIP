(() => {
  'use strict';

  const STYLE_ID = 'paradise-rp-hard-ui-killer-style';
  const WALL_ID = 'paradise-rp-hard-sidewall';
  const KILL_CLASS = 'prhud-native-hard-kill';
  const OWN = '#paradise-rp-hud,#paradise-loader,#paradise-rp-hard-ui-killer-style,#paradise-rp-hard-sidewall';

  const css = `
    .${KILL_CLASS},[data-prhud-hard-kill="1"]{
      display:none!important;visibility:hidden!important;opacity:0!important;pointer-events:none!important;
    }
    #${WALL_ID}{
      position:fixed!important;left:0!important;top:130px!important;bottom:0!important;width:104px!important;
      z-index:2147483646!important;pointer-events:none!important;
      background:linear-gradient(90deg,rgba(4,8,13,.98),rgba(4,8,13,.94) 86px,rgba(4,8,13,0))!important;
      box-shadow:18px 0 34px rgba(0,0,0,.22)!important;
    }
    #${WALL_ID}:before{
      content:"";position:absolute;left:0;top:0;bottom:0;width:3px;
      background:linear-gradient(180deg,#25d9ff,#ffc52f 45%,#25d9ff);opacity:.75;
    }
  `;

  const installStyle = () => {
    let style = document.getElementById(STYLE_ID);
    if (!style) {
      style = document.createElement('style');
      style.id = STYLE_ID;
      document.head.appendChild(style);
    }
    style.textContent = css;
  };

  const installWall = () => {
    let wall = document.getElementById(WALL_ID);
    if (!wall) {
      wall = document.createElement('div');
      wall.id = WALL_ID;
      wall.setAttribute('aria-hidden', 'true');
    }
    document.body.appendChild(wall);
  };

  const hasRootCanvas = () => {
    const canvas = document.querySelector('#root canvas');
    if (!canvas) return false;
    const r = canvas.getBoundingClientRect();
    return r.width > window.innerWidth * .35 && r.height > window.innerHeight * .35;
  };

  const isOwn = el => {
    if (!el || el === document.body || el === document.documentElement) return true;
    if (el.matches?.(OWN) || el.closest?.(OWN)) return true;
    return ['SCRIPT','STYLE','LINK','META','TITLE','BASE','NOSCRIPT'].includes(el.tagName);
  };

  const hasLargeCanvas = el => {
    if (!el || !el.querySelectorAll) return false;
    for (const canvas of el.querySelectorAll('canvas')) {
      const r = canvas.getBoundingClientRect();
      if (r.width > window.innerWidth * .35 && r.height > window.innerHeight * .35) return true;
    }
    return false;
  };

  const hardKill = el => {
    try {
      el.classList.add(KILL_CLASS);
      el.setAttribute('data-prhud-hard-kill', '1');
      el.style.setProperty('display', 'none', 'important');
      el.style.setProperty('visibility', 'hidden', 'important');
      el.style.setProperty('opacity', '0', 'important');
      el.style.setProperty('pointer-events', 'none', 'important');
    } catch (_) {}
  };

  const isNativeUi = (el, r) => {
    const vw = window.innerWidth;
    const vh = window.innerHeight;
    const text = (el.innerText || el.textContent || '').trim();

    const nativeLeftRail = r.left < 104 && r.top > 40 && r.bottom < vh - 42 && r.width < 118 && r.height < vh - 100;
    const nativeTopLeft = r.left < 326 && r.top < 142 && r.width < 330 && r.height < 150;
    const nativeBottomLeft = r.left < 455 && r.bottom > vh - 116 && r.width < 485 && r.height < 125;
    const nativePhone = r.right > vw - 90 && r.bottom > vh - 92 && r.width < 105 && r.height < 105;
    const nativeTopRight = r.right > vw - 350 && r.top < 86 && r.width < 350 && r.height < 90;
    const hotelPromoText = r.top < 82 && r.left > 230 && r.right < vw - 230 && r.height < 70 && /landing\.view|que hay|qué hay|promo|header|body/i.test(text);

    return nativeLeftRail || nativeTopLeft || nativeBottomLeft || nativePhone || nativeTopRight || hotelPromoText;
  };

  const pass = () => {
    if (!hasRootCanvas() && performance.now() < 4500) return;

    installStyle();
    installWall();

    for (const el of Array.from(document.querySelectorAll('body *'))) {
      if (isOwn(el)) continue;
      if (el.id === 'root' || el.tagName === 'CANVAS') continue;

      const r = el.getBoundingClientRect();
      if (!r || r.width <= 2 || r.height <= 2) continue;
      if (r.width > window.innerWidth * .70 || r.height > window.innerHeight * .78) continue;
      if (!isNativeUi(el, r)) continue;
      if (hasLargeCanvas(el)) continue;

      hardKill(el);
    }
  };

  installStyle();
  window.setTimeout(pass, 600);
  window.setTimeout(pass, 1200);
  window.setTimeout(pass, 2200);

  if (window.__paradiseHardUiKiller) clearInterval(window.__paradiseHardUiKiller);
  window.__paradiseHardUiKiller = window.setInterval(pass, 260);
})();
