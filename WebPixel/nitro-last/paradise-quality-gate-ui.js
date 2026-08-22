(() => {
  'use strict';

  if (window.ParadiseQualityGateUi) return;

  const VERSION = '1.2.0-core-v1-render-diagnostic';
  const RENDER_CHECK_STYLE_ID = 'paradise-profile-render-check-style';
  const RENDER_CHECK_LABEL_ID = 'paradise-profile-render-check-label';
  const STYLE_PROPS = ['background', 'backgroundColor', 'border', 'borderRadius', 'padding', 'fontSize', 'boxShadow', 'width', 'height', 'maxHeight', 'outline', 'outlineOffset', 'display', 'visibility', 'opacity', 'position', 'zIndex'];
  let destroyed = false;
  let scheduled = false;
  let renderCheckTouched = [];

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
    return elementSnapshot(element, selector);
  }

  function elementSnapshot(element, selector = null) {
    const style = getComputedStyle(element);
    const values = {};
    STYLE_PROPS.forEach(prop => { values[prop] = style[prop]; });
    const rect = element.getBoundingClientRect();
    return {
      selector,
      exists: true,
      tag: element.tagName,
      className: element.className,
      id: element.id || null,
      ariaHidden: element.getAttribute('aria-hidden'),
      rect: {
        x: Math.round(rect.x),
        y: Math.round(rect.y),
        width: Math.round(rect.width),
        height: Math.round(rect.height)
      },
      visible: rect.width > 0 && rect.height > 0 && style.display !== 'none' && style.visibility !== 'hidden' && Number(style.opacity || 1) !== 0,
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

  function profileInstances() {
    return [...document.querySelectorAll('#paradise-rp-hud .pr-window[data-window="profile"]')]
      .map((element, index) => ({ index, ...elementSnapshot(element, `profile[${index}]`) }));
  }

  function componentMap() {
    return {
      roots: {
        nitroRoot: Boolean(document.getElementById('root')),
        paradiseRoot: Boolean(document.getElementById('paradise-ui-root')),
        paradiseHud: Boolean(document.getElementById('paradise-rp-hud')),
        paradiseHudCount: document.querySelectorAll('#paradise-rp-hud').length,
        outerLegacyAppInsideFrame: Boolean(document.getElementById('app'))
      },
      profile: {
        window: Boolean(document.querySelector('#paradise-rp-hud .pr-window[data-window="profile"]')),
        windowCount: document.querySelectorAll('#paradise-rp-hud .pr-window[data-window="profile"]').length,
        component: Boolean(document.querySelector('#paradise-rp-hud .pr2-profile')),
        componentCount: document.querySelectorAll('#paradise-rp-hud .pr2-profile').length,
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
      profileInstances: profileInstances(),
      profileWindow: computedSnapshot('#paradise-rp-hud .pr-window[data-window="profile"]'),
      profileHeader: computedSnapshot('#paradise-rp-hud .pr-window[data-window="profile"] .pr-window-header'),
      profileComponent: computedSnapshot('#paradise-rp-hud .pr2-profile'),
      profileAvatarCard: computedSnapshot('#paradise-rp-hud .pr2-avatar-card'),
      profileWindowRules: matchingRules('#paradise-rp-hud .pr-window[data-window="profile"]'),
      profileHeaderRules: matchingRules('#paradise-rp-hud .pr-window[data-window="profile"] .pr-window-header'),
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

  function rememberInline(element) {
    if (!element || renderCheckTouched.some(entry => entry.element === element)) return;
    renderCheckTouched.push({ element, cssText: element.style.cssText });
  }

  function forceStyle(element, properties) {
    if (!element) return;
    rememberInline(element);
    Object.entries(properties).forEach(([name, value]) => element.style.setProperty(name, value, 'important'));
  }

  function clearProfileVisualCheck() {
    document.getElementById(RENDER_CHECK_STYLE_ID)?.remove();
    document.querySelectorAll(`[id^="${RENDER_CHECK_LABEL_ID}"]`).forEach(node => node.remove());
    renderCheckTouched.forEach(({ element, cssText }) => {
      if (element?.isConnected) element.style.cssText = cssText;
    });
    renderCheckTouched = [];
    return true;
  }

  function runProfileVisualCheck() {
    window.ParadiseWindowManager?.openWindow?.('profile');
    clearProfileVisualCheck();

    const windows = [...document.querySelectorAll('#paradise-rp-hud .pr-window[data-window="profile"]')];
    const results = [];

    windows.forEach((win, index) => {
      const header = win.querySelector('.pr-window-header');
      const titleStrong = win.querySelector('.pr-window-title strong');
      const titleSmall = win.querySelector('.pr-window-title small');

      forceStyle(win, {
        outline: '6px solid #ff00ff',
        'outline-offset': '-6px',
        'box-shadow': '0 0 0 8px rgba(255,0,255,.35), 0 18px 50px rgba(255,0,255,.45)'
      });
      forceStyle(header, {
        background: '#ff00ff',
        'background-color': '#ff00ff',
        position: 'relative'
      });
      forceStyle(titleStrong, { color: '#ffffff' });
      forceStyle(titleSmall, { color: '#ffffff' });

      const label = document.createElement('div');
      label.id = `${RENDER_CHECK_LABEL_ID}-${index}`;
      label.textContent = `DEV-RENDER-CHECK · PROFILE ${index + 1}/${windows.length}`;
      Object.assign(label.style, {
        position: 'absolute',
        zIndex: '2147483647',
        left: '50%',
        top: '50%',
        transform: 'translate(-50%, -50%)',
        padding: '8px 12px',
        borderRadius: '6px',
        background: '#170017',
        border: '2px solid #ffffff',
        color: '#ffffff',
        fontSize: '11px',
        fontWeight: '900',
        letterSpacing: '.05em',
        whiteSpace: 'nowrap',
        boxShadow: '0 4px 14px rgba(0,0,0,.35)',
        pointerEvents: 'none'
      });
      (header || win).appendChild(label);

      results.push({
        index,
        window: elementSnapshot(win, `profile[${index}]`),
        header: header ? elementSnapshot(header, `profile[${index}] header`) : { exists: false },
        labelConnected: label.isConnected
      });
    });

    return {
      version: VERSION,
      count: windows.length,
      visibleCount: results.filter(item => item.window.visible).length,
      results
    };
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
    profileInstances,
    getStatus: () => ({
      version: VERSION,
      destroyed,
      actionsVehiclesRemoved: !document.querySelector('#paradise-rp-hud .pr-actions-menu [data-window-open="vehicles"]'),
      notificationBellRouted: Boolean(document.querySelector('#paradise-rp-hud [data-qg-notification-route="phone"]')),
      renderDiagnosticAvailable: true,
      profileRenderCheckActive: renderCheckTouched.length > 0,
      profileWindowCount: document.querySelectorAll('#paradise-rp-hud .pr-window[data-window="profile"]').length,
      profileComponentCount: document.querySelectorAll('#paradise-rp-hud .pr2-profile').length
    })
  });
})();