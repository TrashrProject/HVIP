(() => {
  'use strict';

  const OWN_SELECTORS = '#paradise-rp-hud,#paradise-loader,#paradise-rp-hard-ui-killer-style,#paradise-rp-hard-sidewall';
  const KILL_CLASS = 'prhud-native-hard-kill';

  const installStyle = () => {
    if (document.getElementById('paradise-rp-hard-ui-killer-style')) return;
    const style = document.createElement('style');
    style.id = 'paradise-rp-hard-ui-killer-style';
    style.textContent = `
      .${KILL_CLASS},[data-prhud-hard-kill="1"]{
        display:none!important;visibility:hidden!important;opacity:0!important;pointer-events:none!important;
      }
      #paradise-rp-hard-sidewall{
        position:fixed!important;left:0!important;top:0!important;bottom:0!important;width:112px!important;
        z-index:2147483646!important;pointer-events:none!important;
        background:linear-gradient(90deg,rgba(3,8,13,.98) 0,rgba(3,8,13,.98) 88px,rgba(3,8,13,.82) 96px,rgba(3,8,13,0) 112px)!important;
        box-shadow:18px 0 34px rgba(0,0,0,.22)!important;
      }
      #paradise-rp-hard-sidewall:before{
        content:"";position:absolute;left:0;top:0;bottom:0;width:4px;background:linear-gradient(180deg,#25d9ff,#ffc52f 45%,#25d9ff);
        opacity:.75;box-shadow:0 0 18px rgba(36,216,255,.28);
      }
    `;
    document.head.appendChild(style);
  };

  const installSidewall = () => {
    let wall = document.getElementById('paradise-rp-hard-sidewall');
    if (!wall) {
      wall = document.createElement('div');
      wall.id = 'paradise-rp-hard-sidewall';
      wall.setAttribute('aria-hidden', 'true');
      document.body.appendChild(wall);
    }
    document.body.appendChild(wall);
  };

  const isOwn = el => {
    if (!el || el === document.body || el === document.documentElement) return true;
    if (el.matches?.(OWN_SELECTORS) || el.closest?.(OWN_SELECTORS)) return true;
    return ['SCRIPT','STYLE','LINK','META','TITLE','BASE','NOSCRIPT'].includes(el.tagName);
  };

  const hasLargeCanvas = el => {
    if (!el || !el.querySelectorAll) return false;
    for (const canvas of el.querySelectorAll('canvas')) {
      const r = canvas.getBoundingClientRect();
      if (r.width > window.innerWidth * .42 && r.height > window.innerHeight * .42) return true;
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

  const inNativeUiZone = (el, r) => {
    const vw = window.innerWidth;
    const vh = window.innerHeight;
    const text = (el.innerText || el.textContent || '').trim();

    const leftNativeIcons = r.left < 112 && r.top > 42 && r.bottom < vh - 42 && r.width < 130 && r.height < vh - 120;
    const topLeftOldCard = r.left < 326 && r.top < 152 && r.width < 340 && r.height < 160;
    const bottomLeftOldBar = r.left < 520 && r.bottom > vh - 122 && r.width < 540 && r.height < 130;
    const bottomRightPhone = r.right > vw - 95 && r.bottom > vh - 95 && r.width < 110 && r.height < 110;
    const topRightOldWallet = r.right > vw - 355 && r.top < 90 && r.width < 360 && r.height < 96;
    const topHotelText = r.top < 82 && r.left > 245 && r.right < vw - 245 && r.height < 70 && r.width < 980;
    const hotelPromoText = /landing\.view|que hay|qué hay|promo|header|body/i.test(text);

    return leftNativeIcons || topLeftOldCard || bottomLeftOldBar || bottomRightPhone || topRightOldWallet || (topHotelText && (hotelPromoText || text.length > 0));
  };

  const pass = () => {
    installStyle();
    installSidewall();

    const all = Array.from(document.querySelectorAll('body *'));
    for (const el of all) {
      if (isOwn(el)) continue;
      if (el.id === 'root' || el.tagName === 'CANVAS') continue;
      const r = el.getBoundingClientRect();
      if (!r || r.width <= 2 || r.height <= 2) continue;
      if (r.width > window.innerWidth * .86 || r.height > window.innerHeight * .86) continue;
      if (!inNativeUiZone(el, r)) continue;
      if (hasLargeCanvas(el)) continue;
      hardKill(el);
    }
  };

  const start = () => {
    pass();
    let fastCount = 0;
    const fast = setInterval(() => {
      pass();
      if (++fastCount > 260) clearInterval(fast);
    }, 75);
    setInterval(pass, 500);
    new MutationObserver(pass).observe(document.body, { childList: true, subtree: true, attributes: true });
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', start);
  else start();
})();
