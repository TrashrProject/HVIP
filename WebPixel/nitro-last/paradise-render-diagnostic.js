(() => {
  'use strict';

  if (window.ParadiseRenderDiagnostic) return;

  const VERSION = '1.0.0-profile-render-proof';
  const PROPS = ['background', 'border', 'border-radius', 'padding', 'font-size', 'box-shadow', 'width', 'height'];
  const MARKER_ID = 'pr-render-diagnostic-style';
  const BADGE_ID = 'pr-render-diagnostic-badge';

  const q = selector => document.querySelector(selector);
  const cleanUrl = href => {
    if (!href) return '<inline>';
    try {
      const u = new URL(href, location.href);
      return u.pathname + u.search;
    } catch (_) {
      return String(href);
    }
  };

  function specificity(selector) {
    // Diagnostic approximation sufficient for the selectors used by ParadiseRP.
    const source = String(selector || '')
      .replace(/:where\([^)]*\)/g, '')
      .replace(/::[\w-]+/g, '');
    const ids = (source.match(/#[\w-]+/g) || []).length;
    const classes = (source.match(/\.[\w-]+|\[[^\]]+\]|:(?!:)[\w-]+(?:\([^)]*\))?/g) || []).length;
    const elements = (source
      .replace(/#[\w-]+|\.[\w-]+|\[[^\]]+\]|:(?!:)[\w-]+(?:\([^)]*\))?/g, ' ')
      .match(/(^|[\s>+~,(])([a-z][\w-]*)/gi) || []).length;
    return [ids, classes, elements];
  }

  function compareSpecificity(a, b) {
    for (let i = 0; i < 3; i += 1) {
      if (a[i] !== b[i]) return a[i] - b[i];
    }
    return 0;
  }

  function walkRules(ruleList, sheetIndex, sheetHref, out, element, orderRef) {
    if (!ruleList) return;
    for (const rule of Array.from(ruleList)) {
      if (rule.type === CSSRule.MEDIA_RULE) {
        if (window.matchMedia(rule.conditionText).matches) {
          walkRules(rule.cssRules, sheetIndex, sheetHref, out, element, orderRef);
        }
        continue;
      }
      if (rule.type === CSSRule.SUPPORTS_RULE || rule.type === CSSRule.LAYER_BLOCK_RULE) {
        walkRules(rule.cssRules, sheetIndex, sheetHref, out, element, orderRef);
        continue;
      }
      if (rule.type !== CSSRule.STYLE_RULE || !rule.selectorText) continue;
      orderRef.value += 1;
      let matches = false;
      try { matches = element.matches(rule.selectorText); } catch (_) { matches = false; }
      if (!matches) continue;

      const declarations = {};
      for (const prop of PROPS) {
        const value = rule.style.getPropertyValue(prop);
        if (value) declarations[prop] = {
          value: value.trim(),
          important: rule.style.getPropertyPriority(prop) === 'important'
        };
      }
      if (!Object.keys(declarations).length) continue;

      out.push({
        sheet: cleanUrl(sheetHref),
        sheetIndex,
        order: orderRef.value,
        selector: rule.selectorText,
        specificity: specificity(rule.selectorText),
        declarations
      });
    }
  }

  function matchingRules(element) {
    if (!element) return [];
    const out = [];
    const orderRef = { value: 0 };
    Array.from(document.styleSheets).forEach((sheet, sheetIndex) => {
      let rules = null;
      try { rules = sheet.cssRules; } catch (_) { return; }
      walkRules(rules, sheetIndex, sheet.href, out, element, orderRef);
    });
    return out;
  }

  function winnerForProperty(element, prop) {
    if (!element) return null;
    const candidates = matchingRules(element)
      .filter(rule => rule.declarations[prop])
      .map(rule => ({
        ...rule,
        value: rule.declarations[prop].value,
        important: rule.declarations[prop].important
      }));

    if (element.style?.getPropertyValue(prop)) {
      candidates.push({
        sheet: '<inline style>',
        sheetIndex: Number.MAX_SAFE_INTEGER,
        order: Number.MAX_SAFE_INTEGER,
        selector: 'style="..."',
        specificity: [1000000, 0, 0],
        value: element.style.getPropertyValue(prop).trim(),
        important: element.style.getPropertyPriority(prop) === 'important'
      });
    }

    candidates.sort((a, b) => {
      if (a.important !== b.important) return a.important ? 1 : -1;
      const spec = compareSpecificity(a.specificity, b.specificity);
      if (spec) return spec;
      if (a.sheetIndex !== b.sheetIndex) return a.sheetIndex - b.sheetIndex;
      return a.order - b.order;
    });

    return candidates.length ? candidates[candidates.length - 1] : null;
  }

  function computed(selector) {
    const element = q(selector);
    if (!element) return { selector, found: false };
    const cs = getComputedStyle(element);
    const values = {};
    const winners = {};
    for (const prop of PROPS) {
      values[prop] = cs.getPropertyValue(prop).trim();
      winners[prop] = winnerForProperty(element, prop);
    }
    return {
      selector,
      found: true,
      tag: element.tagName,
      className: element.className,
      values,
      winners,
      matchedRuleCount: matchingRules(element).length
    };
  }

  function roots() {
    const selectors = ['#root', '#paradise-ui-root', '#paradise-rp-hud', '#paradise-rp-hud .pr-window-layer', '#paradise-rp-hud .pp-device'];
    return selectors.map(selector => {
      const node = q(selector);
      return {
        selector,
        present: Boolean(node),
        parent: node?.parentElement ? `${node.parentElement.tagName.toLowerCase()}${node.parentElement.id ? '#' + node.parentElement.id : ''}` : null,
        children: node?.children?.length ?? 0
      };
    });
  }

  function assets() {
    return {
      stylesheets: Array.from(document.styleSheets).map((sheet, index) => ({ index, href: cleanUrl(sheet.href) })),
      scripts: Array.from(document.scripts).map((script, index) => ({ index, src: cleanUrl(script.src), type: script.type || 'classic' }))
    };
  }

  function modules() {
    return {
      baseline: window.__PARADISE_BASELINE__ || null,
      uiFoundation: window.__ParadiseRPUI?.version || null,
      characterV2: window.ParadiseCharacterV2?.version || window.__ParadiseCharacterV2 || null,
      inventoryV2: window.ParadiseInventoryV2?.version || window.__ParadiseInventoryV2 || null,
      phoneV1: window.ParadisePhoneV1?.version || null,
      phoneVisualParity: window.ParadisePhoneVisualParity?.version || null,
      phoneFinal: window.ParadisePhoneFinalPolish?.version || null,
      phoneStrictLayout: window.ParadisePhoneStrictLayout?.version || null,
      windowManager: Boolean(window.ParadiseWindowManager),
      activeWindow: window.ParadiseWindowManager?.getActiveWindow?.() || null
    };
  }

  function profile() {
    return {
      window: computed('#paradise-rp-hud .pr-window[data-window="profile"]'),
      header: computed('#paradise-rp-hud .pr-window[data-window="profile"] .pr-window-header'),
      profile: computed('#paradise-rp-hud .pr-window[data-window="profile"] .pr2-profile'),
      firstCard: computed('#paradise-rp-hud .pr-window[data-window="profile"] .pr2-card')
    };
  }

  function inventory() {
    return {
      window: computed('#paradise-rp-hud .pr-window[data-window="inventory"]'),
      root: computed('#paradise-rp-hud .pr-window[data-window="inventory"] .pr3-inventory'),
      gridPanel: computed('#paradise-rp-hud .pr-window[data-window="inventory"] .pr3-grid-panel'),
      detailPanel: computed('#paradise-rp-hud .pr-window[data-window="inventory"] .pr3-detail-panel')
    };
  }

  function phone() {
    return {
      window: computed('#paradise-rp-hud .pr-window[data-window="phone"]'),
      device: computed('#paradise-rp-hud .pp-device'),
      screen: computed('#paradise-rp-hud .pp-device .pp-screen'),
      home: computed('#paradise-rp-hud .pp-device .pp-home')
    };
  }

  function enableProfileMarker() {
    let style = document.getElementById(MARKER_ID);
    if (!style) {
      style = document.createElement('style');
      style.id = MARKER_ID;
      style.textContent = `
        #paradise-rp-hud .pr-window[data-window="profile"].is-open {
          outline: 4px solid #ff00ff !important;
          outline-offset: -4px !important;
        }
        #paradise-rp-hud .pr-window[data-window="profile"] #${BADGE_ID} {
          position:absolute; z-index:9999; top:7px; right:58px;
          padding:5px 8px; border-radius:6px;
          background:#ff00ff; color:#fff; border:2px solid #3b003b;
          box-shadow:0 3px 0 rgba(59,0,59,.35);
          font:900 10px/1 Arial,sans-serif; letter-spacing:.08em;
          pointer-events:none;
        }
      `;
      document.head.appendChild(style);
    }

    const header = q('#paradise-rp-hud .pr-window[data-window="profile"] .pr-window-header');
    if (header && !document.getElementById(BADGE_ID)) {
      if (getComputedStyle(header).position === 'static') header.style.position = 'relative';
      const badge = document.createElement('span');
      badge.id = BADGE_ID;
      badge.textContent = 'DEV-RENDER-CHECK';
      header.appendChild(badge);
    }
    return Boolean(document.getElementById(BADGE_ID));
  }

  function disableProfileMarker() {
    document.getElementById(MARKER_ID)?.remove();
    document.getElementById(BADGE_ID)?.remove();
    return true;
  }

  async function serviceWorkers() {
    if (!('serviceWorker' in navigator)) return [];
    const registrations = await navigator.serviceWorker.getRegistrations();
    return registrations.map(reg => ({
      scope: reg.scope,
      active: reg.active?.scriptURL || null,
      waiting: reg.waiting?.scriptURL || null,
      installing: reg.installing?.scriptURL || null
    }));
  }

  function report() {
    const data = {
      version: VERSION,
      href: location.href,
      roots: roots(),
      modules: modules(),
      assets: assets(),
      profile: profile(),
      inventory: inventory(),
      phone: phone()
    };
    console.group('[ParadiseRP] RENDER PIPELINE DIAGNOSTIC');
    console.log(data);
    console.groupEnd();
    return data;
  }

  function boot() {
    // Diagnostic branch only: deliberately impossible-to-miss proof that the
    // active Profile window is controlled by this served static pipeline.
    requestAnimationFrame(() => requestAnimationFrame(enableProfileMarker));
    console.info('[ParadiseRP] Render diagnostic active', { version: VERSION });
  }

  window.ParadiseRenderDiagnostic = Object.freeze({
    version: VERSION,
    report,
    roots,
    assets,
    modules,
    profile,
    inventory,
    phone,
    serviceWorkers,
    enableProfileMarker,
    disableProfileMarker
  });

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot, { once: true });
  else boot();
})();
