/* ParadiseRP — ParadisePhone Settings V3
 * Visual redesign only. Existing controls remain in place and keep their own handlers/state.
 */
(() => {
  'use strict';

  if (window.ParadisePhoneSettingsV3) return;

  const ROOT = '#paradise-rp-hud .pp-device';
  const SETTINGS = '.pp-settings';
  const VERSION = '3.0.0';
  let raf = 0;

  const normalize = value => String(value || '')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[’‘]/g, "'")
    .replace(/\s+/g, ' ')
    .trim()
    .toLowerCase();

  const ownText = node => normalize(node?.textContent);

  function findLabel(root, tests) {
    const nodes = root.querySelectorAll('label,legend,h1,h2,h3,h4,h5,h6,p,span,strong,small,div');
    let best = null;
    for (const node of nodes) {
      const text = ownText(node);
      if (!text || text.length > 90) continue;
      if (!tests.some(test => test(text))) continue;
      if (!best || node.children.length < best.children.length) best = node;
    }
    return best;
  }

  function interactiveCount(node) {
    return node?.querySelectorAll?.('button,input,select,textarea,[role="button"]').length || 0;
  }

  function findGroup(root, label, otherLabels) {
    if (!label) return null;
    let node = label.parentElement;
    let fallback = node;
    while (node && node !== root) {
      const containsOther = otherLabels.some(other => other && other !== label && node.contains(other));
      if (!containsOther && interactiveCount(node) > 0) return node;
      if (!containsOther) fallback = node;
      node = node.parentElement;
    }
    return fallback && fallback !== root ? fallback : null;
  }

  function addSectionBefore(group, title, key) {
    if (!group || group.parentElement?.querySelector(`:scope > [data-ppsv3-section="${key}"]`)) return;
    const heading = document.createElement('div');
    heading.className = 'ppsv3-section-title';
    heading.dataset.ppsv3Section = key;
    heading.textContent = title;
    group.parentElement?.insertBefore(heading, group);
  }

  function isSelected(node) {
    if (!node) return false;
    if (node.matches?.(':checked,[aria-pressed="true"],[aria-selected="true"],[data-selected="true"],[data-active="true"],.active,.selected,.is-active')) return true;
    const input = node.querySelector?.('input');
    return Boolean(input?.checked);
  }

  function decorateChoices(group, kind) {
    if (!group) return;
    const raw = Array.from(group.querySelectorAll('button,[role="button"],label,input[type="radio"],input[type="checkbox"],input[type="color"]'));
    const controls = raw.filter(node => !(node.matches('input') && node.closest('label') && raw.includes(node.closest('label'))));
    controls.forEach(control => {
      control.classList.add('ppsv3-choice', `ppsv3-${kind}-choice`);
      control.classList.toggle('ppsv3-active', isSelected(control));
    });
  }

  function refreshActive(root) {
    decorateChoices(root.querySelector('.ppsv3-theme-group'), 'theme');
    decorateChoices(root.querySelector('.ppsv3-border-group'), 'border');
    decorateChoices(root.querySelector('.ppsv3-wallpaper-group'), 'wallpaper');
  }

  function makeHeader(root, originalTitle) {
    if (root.querySelector(':scope > .ppsv3-header')) return;
    if (originalTitle) originalTitle.classList.add('ppsv3-original-title');

    const header = document.createElement('div');
    header.className = 'ppsv3-header';
    header.innerHTML = `
      <div class="ppsv3-header-copy">
        <strong>Paramètres</strong>
        <span>Personnalisez votre ParadisePhone</span>
      </div>`;
    root.prepend(header);
  }

  function decorate(root) {
    if (!(root instanceof HTMLElement)) return;

    const originalTitle = findLabel(root, [t => t === 'parametres']);
    const borderLabel = findLabel(root, [t => t.includes('couleur de la bordure'), t => t === 'bordure']);
    const themeLabel = findLabel(root, [t => t === 'theme']);
    const presetLabel = findLabel(root, [t => t.includes("fonds d'ecran predefinis")]);
    const customLabel = findLabel(root, [t => t.includes("fond d'ecran personnalise")]);
    const localInfo = findLabel(root, [t => t.includes('parametres sont enregistres localement')]);
    const reset = Array.from(root.querySelectorAll('button,[role="button"]')).find(node => ownText(node).startsWith('reinitialiser')) || null;

    const labels = [borderLabel, themeLabel, presetLabel, customLabel, localInfo];
    const themeGroup = findGroup(root, themeLabel, labels);
    const borderGroup = findGroup(root, borderLabel, labels);
    const presetGroup = findGroup(root, presetLabel, labels);
    const customGroup = findGroup(root, customLabel, labels);

    root.classList.add('pp-settings-v3');
    root.dataset.ppSettingsV3 = VERSION;
    makeHeader(root, originalTitle);

    [themeLabel, borderLabel, presetLabel, customLabel].filter(Boolean).forEach(label => label.classList.add('ppsv3-setting-label'));
    themeGroup?.classList.add('ppsv3-card', 'ppsv3-theme-group');
    borderGroup?.classList.add('ppsv3-card', 'ppsv3-border-group');
    presetGroup?.classList.add('ppsv3-card', 'ppsv3-wallpaper-group');
    customGroup?.classList.add('ppsv3-card', 'ppsv3-custom-group');

    if (themeGroup || borderGroup) addSectionBefore(themeGroup || borderGroup, 'Apparence', 'appearance');
    if (presetGroup || customGroup) addSectionBefore(presetGroup || customGroup, 'Personnalisation', 'personalization');

    if (localInfo) {
      localInfo.classList.add('ppsv3-local-info');
      if (!localInfo.querySelector('.ppsv3-info-dot')) {
        const dot = document.createElement('span');
        dot.className = 'ppsv3-info-dot';
        dot.setAttribute('aria-hidden', 'true');
        localInfo.prepend(dot);
      }
    }

    if (reset) {
      reset.classList.add('ppsv3-reset');
      const parent = reset.parentElement;
      if (parent && parent !== root && !parent.querySelector(':scope > [data-ppsv3-section="other"]')) {
        const heading = document.createElement('div');
        heading.className = 'ppsv3-section-title ppsv3-other-title';
        heading.dataset.ppsv3Section = 'other';
        heading.textContent = 'Autres';
        parent.insertBefore(heading, reset);
      }
    }

    const customInput = customGroup?.querySelector('input[type="url"],input[type="text"],input:not([type])');
    if (customInput) {
      customInput.classList.add('ppsv3-url');
      customInput.setAttribute('inputmode', 'url');
      customInput.setAttribute('autocomplete', 'off');
      customInput.setAttribute('spellcheck', 'false');
      customInput.setAttribute('pattern', 'https?://.*');
      customInput.setAttribute('title', 'Utilisez une URL http:// ou https://');
      const validate = () => {
        const value = String(customInput.value || '').trim();
        const safe = !value || /^https?:\/\//i.test(value);
        customInput.classList.toggle('ppsv3-url-invalid', !safe);
        customInput.setAttribute('aria-invalid', safe ? 'false' : 'true');
      };
      if (!customInput.dataset.ppsv3Bound) {
        customInput.dataset.ppsv3Bound = 'true';
        customInput.addEventListener('input', validate, { passive: true });
        customInput.addEventListener('change', validate, { passive: true });
        customInput.addEventListener('blur', validate, { passive: true });
      }
      validate();
    }

    if (!root.dataset.ppsv3Bound) {
      root.dataset.ppsv3Bound = 'true';
      root.addEventListener('click', () => setTimeout(() => refreshActive(root), 0), false);
      root.addEventListener('change', () => setTimeout(() => refreshActive(root), 0), false);
    }

    refreshActive(root);
  }

  function run() {
    raf = 0;
    document.querySelectorAll(`${ROOT} ${SETTINGS}`).forEach(decorate);
  }

  function schedule() {
    if (raf) return;
    raf = requestAnimationFrame(run);
  }

  const observer = new MutationObserver(schedule);
  observer.observe(document.documentElement, { childList: true, subtree: true });
  window.addEventListener('paradise:phone', schedule, false);
  schedule();

  window.ParadisePhoneSettingsV3 = Object.freeze({ version: VERSION, refresh: schedule });
})();