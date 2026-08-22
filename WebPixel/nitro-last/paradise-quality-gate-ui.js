(() => {
  'use strict';

  if (window.ParadiseQualityGateUi) return;

  const VERSION = '1.1.0-core-v1-render-diagnostic';
  const RENDER_CHECK_STYLE_ID = 'paradise-profile-render-check-style';
  const RENDER_CHECK_LABEL_ID = 'paradise-profile-render-check-label';
  const STYLE_PROPS = ['background', 'backgroundColor', 'border', 'borderRadius', 'padding', 'fontSize', 'boxShadow', 'width', 'height', 'maxHeight'];
  let destroyed = false;
  let scheduled = false;

  function schedule() {
    if (destroyed || scheduled) return;
    scheduled = true;
    requestAnimationFrame(() => requestAnimationFrame(() => {
      scheduled = false;
      if (!destroyed) reconcile();
    }));
  }

  function reconcile() {
    const hud = document.getElementById('paradise-rp-hud');
    if (!hud) return;

    // Vehicles stays in the validated bottom-left HUD structure. Until the real
    // vehicle domain exists, do not duplicate its placeholder in Actions.
    hud.querySelector('.pr-actions-menu [data-window-open="vehicles"]')?.remove();

    const bell = hud.querySelector('[data-action="notifications"]');
    if (bell) {
      bell.title = 'Notifications ParadisePhone';
      bell.setAttribute('aria-label', 'Ouvrir les notifications ParadisePhone');
      bell.dataset.qgNotificationRoute = 'phone';
    }
  }

  function onClick(event) {
    const target = event.target;
    if (!(target instanceof Element)) return;
    const bell = target.closest('#paradise-rp-hud [data-action="notifications"][data-qg-notification-route="phone"]');
    if (!bell) return;

    event.preventDefault();
    event.stopPropagation();
    event.stopImmediatePropagation();
    window.ParadisePhoneV1?.open?.('notifications');
  }

  function assetLoaded(fragment, type = 'script') {
    const selector = type === 'style' ? 'link[rel="stylesheet"][href]' : 'script[src]';
    const attribute = type === 'style' ? 'href' : 'src';
    return [...document.querySelectorAll(selector)].some(node => String(node.getAttribute(attribute) || '').includes(fragment));
  }

  function computedSnapshot(selector) {
    const element = document.querySelector(selector);
    if (!element) return { selector, exists: false };
    const style = getComputedStyle(element);
    const values = {};
    STYLE_PROPS.forEach(prop => { values[prop] = style[prop]; });
    return {
      selector,
      exists: true,
      tag: element.tagName,
      className: element.className,
      rect: {
        x: Math.round(element.getBoundingClientRect().x),
        y: Math.round(element.getBoundingClientRect().y),
        width: Math.round(element.getBoundingClientRect().width),
        height: Math.round(element.getBoundingClientRect().height)
      },
      computed: values
    };
  }

  function collectRules(rules, element, href, output) {
    for (const rule of rules || []) {
      if (rule.cssRules) {
        try { collectRules(rule.cssRules, element, href, output); } catch (_) {}
        continue;
      }
      const selectorText = rule.selectorText;
      if (!selectorText || !rule.style) continue;
      let matches = false;
      try { matches = selectorText.split(',').some(selector => element.matches(selector.trim())); } catch (_) {}
      if (!matches) continue;

      const declarations = {};
      STYLE_PROPS.forEach(prop => {
        const cssName = prop.replace(/[A-Z]/g, letter => `-${letter.toLowerCase()}`);
        const value = rule.style.getPropertyValue(cssName);
        if (value) declarations[prop] = `${value}${rule.style.getPropertyPriority(cssName) ? ' !important' : ''}`;
      });
      if (Object.keys(declarations).length) output.push({ href: href || 'inline-style', selector: selectorText, declarations });
    }
  }

  function matchingRules(selector) {
    const element = document.querySelector(selector);
    if (!element) return [];
    const output = [];
    for (const sheet of document.styleSheets) {
      let rules;
      try { rules = sheet.cssRules; } catch (_) { continue; }
      collectRules(rules, element, sheet.href, output);
    }
    return output;
  }

  function componentMap() {
    return {
      roots: {
        nitroRoot: Boolean(document.getElementById('root')),
        paradiseRoot: Boolean(document.getElementById('paradise-ui-root')),
        paradiseHud: Boolean(document.getElementById('paradise-rp-hud')),
        outerLegacyAppInsideFrame: Boolean(document.getElementById('app'))
      },
      profile: {
        window: Boolean(document.querySelector('#paradise-rp-hud .pr-window[data-window="profile"]')),
        component: Boolean(document.querySelector('#paradise-rp-hud .pr2-profile')),
        generator: 'paradise-character-v2.js',
        generatorLoaded: assetLoaded('paradise-character-v2.js'),
        styles: ['paradise-character-v2.css', 'paradise-quality-gate.css'],
        characterCssLoaded: assetLoaded('paradise-character-v2.css', 'style'),
        qualityGateCssLoaded: assetLoaded('paradise-quality-gate.css', 'style')
      },
      inventory: {
        window: Boolean(document.querySelector('#paradise-rp-hud .pr-window[data-window="inventory"]')),
        component: Boolean(document.querySelector('#paradise-rp-hud [data-pr3-inventory]')),
        generator: 'paradise-inventory-v2.js',
        generatorLoaded: assetLoaded('paradise-inventory-v2.js'),
        styles: ['paradise-inventory-v2.css', 'paradise-quality-gate.css'],
        inventoryCssLoaded: assetLoaded('paradise-inventory-v2.css', 'style')
      },
      phone: {
        window: Boolean(document.querySelector('#paradise-rp-hud .pr-window[data-window="phone"]')),
        component: Boolean(document.querySelector('#paradise-rp-hud .pp-device')),
        generator: 'paradise-phone-v1.js',
        generatorLoaded: assetLoaded('paradise-phone-v1.js'),
        enhancementChain: ['paradise-phone-v1-ux.js', 'paradise-phone-final.js', 'paradise-phone-layout-final.js'],
        enhancementLoaded: ['paradise-phone-v1-ux.js', 'paradise-phone-final.js', 'paradise-phone-layout-final.js'].map(name => ({ name, loaded: assetLoaded(name) })),
        styleChain: ['paradise-phone-v1.css', 'paradise-quality-gate.css', 'paradise-phone-v1-ux.css', 'paradise-phone-final.css', 'paradise-phone-layout-final.css'],
        styleLoaded: ['paradise-phone-v1.css', 'paradise-quality-gate.css', 'paradise-phone-v1-ux.css', 'paradise-phone-final.css', 'paradise-phone-layout-final.css'].map(name => ({ name, loaded: assetLoaded(name, 'style') }))
      }
    };
  }

  function renderDiagnostic() {
    return {
      version: VERSION,
      href: location.href,
      baseline: window.__PARADISE_BASELINE__ || null,
      activeWindow: window.ParadiseWindowManager?.getActiveWindow?.() || null,
      components: componentMap(),
      profileWindow: computedSnapshot('#paradise-rp-hud .pr-window[data-window="profile"]'),
      profileComponent: computedSnapshot('#paradise-rp-hud .pr2-profile'),
      profileAvatarCard: computedSnapshot('#paradise-rp-hud .pr2-avatar-card'),
      profileWindowRules: matchingRules('#paradise-rp-hud .pr-window[data-window="profile"]'),
      profileAvatarRules: matchingRules('#paradise-rp-hud .pr2-avatar-card'),
      loadedStyles: [...document.querySelectorAll('link[rel="stylesheet"][href]')].map(node => node.href),
      loadedScripts: [...document.querySelectorAll('script[src]')].map(node => node.src)
    };
  }

  async function verifyStaticAssets() {
    const checks = [
      ['paradise-character-v2.js?v=2', '2.0.0-character-profile'],
      ['paradise-character-v2.css?v=2', 'Character Profile V2'],
      ['paradise-inventory-v2.js?v=1', '3.0.1-inventory-v2'],
      ['paradise-inventory-v2.css?v=1', 'Inventory V2'],
      ['paradise-phone-v1.js?v=1', '4.0.0-phone-v1'],
      ['paradise-phone-final.css?v=2', 'ParadisePhone final polish'],
      ['paradise-phone-layout-final.css?v=1', 'strict final layout correction']
    ];
    const results = [];
    for (const [url, marker] of checks) {
      try {
        const response = await fetch(`./${url}&renderdiag=${Date.now()}`, { cache: 'no-store', credentials: 'same-origin' });
        const body = await response.text();
        results.push({ url, status: response.status, ok: response.ok, marker, markerFound: body.includes(marker), bytes: body.length });
      } catch (error) {
        results.push({ url, ok: false, marker, markerFound: false, error: error?.message || String(error) });
      }
    }
    return results;
  }

  function runProfileVisualCheck() {
    window.ParadiseWindowManager?.openWindow?.('profile');
    document.getElementById(RENDER_CHECK_STYLE_ID)?.remove();
    document.getElementById(RENDER_CHECK_LABEL_ID)?.remove();

    const style = document.createElement('style');
    style.id = RENDER_CHECK_STYLE_ID;
    style.textContent = `
      #paradise-rp-hud .pr-window[data-window="profile"] { outline: 4px solid #ff00ff !important; outline-offset: -4px !important; }
      #paradise-rp-hud .pr-window[data-window="profile"] .pr-window-header { background: #ff00ff !important; }
      #paradise-rp-hud .pr-window[data-window="profile"] .pr-window-title strong,
      #paradise-rp-hud .pr-window[data-window="profile"] .pr-window-title small { color: #ffffff !important; }
    `;
    document.head.appendChild(style);

    const win = document.querySelector('#paradise-rp-hud .pr-window[data-window="profile"]');
    if (win) {
      const label = document.createElement('div');
      label.id = RENDER_CHECK_LABEL_ID;
      label.textContent = 'DEV-RENDER-CHECK · PROFILE ACTIF';
      Object.assign(label.style, {
        position: 'absolute',
        zIndex: '99999',
        top: '64px',
        right: '12px',
        padding: '7px 10px',
        borderRadius: '6px',
        background: '#ff00ff',
        color: '#ffffff',
        fontSize: '11px',
        fontWeight: '900',
        letterSpacing: '.04em',
        boxShadow: '0 4px 12px rgba(0,0,0,.25)',
        pointerEvents: 'none'
      });
      win.appendChild(label);
    }
    return computedSnapshot('#paradise-rp-hud .pr-window[data-window="profile"]');
  }

  function clearProfileVisualCheck() {
    document.getElementById(RENDER_CHECK_STYLE_ID)?.remove();
    document.getElementById(RENDER_CHECK_LABEL_ID)?.remove();
    return true;
  }

  function destroy() {
    if (destroyed) return;
    destroyed = true;
    clearProfileVisualCheck();
    document.removeEventListener('click', onClick, true);
    window.removeEventListener('paradise:store-change', schedule, false);
  }

  document.addEventListener('click', onClick, true);
  window.addEventListener('paradise:store-change', schedule, false);
  window.addEventListener('beforeunload', destroy, { once: true });
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', schedule, { once: true });
  else schedule();

  window.ParadiseQualityGateUi = Object.freeze({
    version: VERSION,
    refresh: schedule,
    renderDiagnostic,
    verifyStaticAssets,
    runProfileVisualCheck,
    clearProfileVisualCheck,
    inspect: computedSnapshot,
    matchingRules,
    getStatus: () => ({
      version: VERSION,
      destroyed,
      actionsVehiclesRemoved: !document.querySelector('#paradise-rp-hud .pr-actions-menu [data-window-open="vehicles"]'),
      notificationBellRouted: Boolean(document.querySelector('#paradise-rp-hud [data-qg-notification-route="phone"]')),
      renderDiagnosticAvailable: true,
      profileRenderCheckActive: Boolean(document.getElementById(RENDER_CHECK_STYLE_ID))
    })
  });
})();