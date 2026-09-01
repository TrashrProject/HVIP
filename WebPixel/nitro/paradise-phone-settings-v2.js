/* ParadiseRP — ParadisePhone Settings V3
 * Live DOM adapter: finds the real Settings screen by its visible labels instead of relying on a fixed phone root.
 * Existing controls, handlers, values and persistence remain authoritative.
 */
(() => {
  'use strict';

  const VERSION = '3.0.0';
  const ROOT_CLASS = 'pp-settings-v3';
  const resetBypass = new WeakSet();
  let scheduled = false;

  const normalize = value => String(value || '')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[’‘]/g, "'")
    .replace(/\s+/g, ' ')
    .trim()
    .toLowerCase();

  const txt = node => normalize(node?.textContent);
  const all = (root, selector) => Array.from(root?.querySelectorAll?.(selector) || []);

  function exactTextNode(root, tests) {
    return all(root, 'h1,h2,h3,h4,h5,h6,label,legend,p,small,strong,span,div')
      .filter(node => {
        const value = txt(node);
        return value && value.length < 100 && tests.some(test => test(value));
      })
      .sort((a, b) => a.children.length - b.children.length)[0] || null;
  }

  function scoreCandidate(node) {
    if (!(node instanceof HTMLElement)) return 0;
    const t = txt(node);
    let score = 0;
    if (t.includes('couleur de la bordure')) score += 3;
    if (t.includes('theme')) score += 2;
    if (t.includes("fonds d'ecran predefinis")) score += 3;
    if (t.includes("fond d'ecran personnalise")) score += 3;
    if (t.includes('reinitialiser')) score += 2;
    if (t.includes('parametres sont enregistres localement')) score += 1;
    if (node.querySelector('input,select,button,[role="button"]')) score += 1;
    return score;
  }

  function findSettingsRoot() {
    const obvious = all(document, '.pp-settings,[class*="phone"] [class*="settings"],[class*="Phone"] [class*="Settings"],[class*="phone"] form,[class*="phone"] main')
      .filter(node => scoreCandidate(node) >= 8)
      .sort((a, b) => {
        const scoreDiff = scoreCandidate(b) - scoreCandidate(a);
        if (scoreDiff) return scoreDiff;
        return a.querySelectorAll('*').length - b.querySelectorAll('*').length;
      });
    if (obvious[0]) return obvious[0];

    const label = all(document, 'label,legend,h1,h2,h3,h4,h5,h6,p,span,strong,div')
      .find(node => txt(node) === 'couleur de la bordure');
    if (!label) return null;

    let current = label.parentElement;
    let best = null;
    while (current && current !== document.body) {
      const score = scoreCandidate(current);
      if (score >= 10) best = current;
      if (score >= 13) return current;
      current = current.parentElement;
    }
    return best;
  }

  function interactiveCount(node) {
    return all(node, 'button,input,select,textarea,[role="button"],[tabindex]').length;
  }

  function resolveGroup(root, label, otherLabels) {
    if (!label) return null;
    let current = label;
    let fallback = label.parentElement;
    while (current && current !== root) {
      const parent = current.parentElement;
      if (!parent || parent === root) break;
      const containsOther = otherLabels.some(other => other && other !== label && parent.contains(other));
      if (!containsOther && interactiveCount(parent) > 0) return parent;
      if (!containsOther) fallback = parent;
      current = parent;
    }
    return fallback && fallback !== root ? fallback : null;
  }

  function selected(node) {
    if (!node) return false;
    if (node.matches?.(':checked,[aria-pressed="true"],[aria-selected="true"],[data-selected="true"],[data-active="true"],.active,.selected,.is-active')) return true;
    return Boolean(node.querySelector?.('input:checked'));
  }

  function decorateChoiceGroup(group, kind) {
    if (!group) return;
    let controls = all(group, 'button,[role="button"],label,input[type="radio"],input[type="checkbox"],input[type="color"]');
    controls = controls.filter(node => !(node.matches('input') && node.closest('label') && controls.includes(node.closest('label'))));
    controls.forEach(control => {
      control.classList.add('ppsv3-choice', `ppsv3-${kind}-choice`);
      control.classList.toggle('ppsv3-active', selected(control));
    });
  }

  function makeThemeSegments(group) {
    if (!group || group.querySelector('.ppsv3-theme-segments')) return;
    const select = group.querySelector('select');
    if (!select || select.options.length < 2 || select.options.length > 3) return;

    select.classList.add('ppsv3-native-theme-select');
    const wrap = document.createElement('div');
    wrap.className = 'ppsv3-theme-segments';

    Array.from(select.options).forEach(option => {
      const button = document.createElement('button');
      button.type = 'button';
      button.className = 'ppsv3-theme-segment';
      button.textContent = option.textContent?.trim() || option.value;
      button.dataset.value = option.value;
      button.addEventListener('click', () => {
        if (select.value === option.value) return;
        select.value = option.value;
        select.dispatchEvent(new Event('input', { bubbles: true }));
        select.dispatchEvent(new Event('change', { bubbles: true }));
        syncThemeSegments(group);
      });
      wrap.appendChild(button);
    });

    select.insertAdjacentElement('afterend', wrap);
    select.addEventListener('change', () => syncThemeSegments(group));
    syncThemeSegments(group);
  }

  function syncThemeSegments(group) {
    const select = group?.querySelector('select');
    if (!select) return;
    all(group, '.ppsv3-theme-segment').forEach(button => {
      const active = button.dataset.value === select.value;
      button.classList.toggle('ppsv3-active', active);
      button.setAttribute('aria-pressed', active ? 'true' : 'false');
    });
  }

  function isSafeUrl(value) {
    const raw = String(value || '').trim();
    if (!raw) return true;
    try {
      const url = new URL(raw);
      return url.protocol === 'http:' || url.protocol === 'https:';
    } catch (_) {
      return false;
    }
  }

  function validateUrl(input, showError) {
    if (!input) return true;
    const valid = isSafeUrl(input.value);
    input.setAttribute('aria-invalid', valid ? 'false' : 'true');
    const host = input.parentElement || input;
    let error = host.querySelector?.('.ppsv3-url-error');
    if (!valid && showError && !error) {
      error = document.createElement('small');
      error.className = 'ppsv3-url-error';
      error.textContent = 'Utilisez uniquement une URL http:// ou https://.';
      host.appendChild(error);
    }
    if (valid && error) error.remove();
    return valid;
  }

  function makeSection(title) {
    const section = document.createElement('section');
    section.className = 'ppsv3-section';
    const heading = document.createElement('div');
    heading.className = 'ppsv3-section-title';
    heading.textContent = title;
    const card = document.createElement('div');
    card.className = 'ppsv3-card';
    section.append(heading, card);
    return { section, card };
  }

  function showResetConfirm(root, resetButton) {
    if (root.querySelector('.ppsv3-confirm')) return;
    const overlay = document.createElement('div');
    overlay.className = 'ppsv3-confirm';
    overlay.innerHTML = `
      <div class="ppsv3-confirm-card" role="dialog" aria-modal="true" aria-label="Confirmer la réinitialisation">
        <strong>Réinitialiser ?</strong>
        <p>Le ParadisePhone retrouvera ses paramètres par défaut.</p>
        <div><button type="button" data-cancel>Annuler</button><button type="button" data-confirm>Réinitialiser</button></div>
      </div>`;
    const close = () => overlay.remove();
    overlay.addEventListener('click', event => { if (event.target === overlay) close(); });
    overlay.querySelector('[data-cancel]').addEventListener('click', close);
    overlay.querySelector('[data-confirm]').addEventListener('click', () => {
      close();
      resetBypass.add(resetButton);
      resetButton.click();
      setTimeout(() => resetBypass.delete(resetButton), 0);
    });
    root.appendChild(overlay);
  }

  function enhance(root) {
    if (!(root instanceof HTMLElement) || root.dataset.ppSettingsV3 === VERSION) return;

    const labelBorder = exactTextNode(root, [t => t === 'couleur de la bordure']);
    const labelTheme = exactTextNode(root, [t => t === 'theme']);
    const labelPresets = exactTextNode(root, [t => t.includes("fonds d'ecran predefinis")]);
    const labelCustom = exactTextNode(root, [t => t.includes("fond d'ecran personnalise")]);
    const localInfo = exactTextNode(root, [t => t.includes('parametres sont enregistres localement')]);
    const resetButton = all(root, 'button,[role="button"]').find(node => txt(node).startsWith('reinitialiser')) || null;
    const labels = [labelBorder, labelTheme, labelPresets, labelCustom, localInfo];

    const borderGroup = resolveGroup(root, labelBorder, labels);
    const themeGroup = resolveGroup(root, labelTheme, labels);
    const presetsGroup = resolveGroup(root, labelPresets, labels);
    const customGroup = resolveGroup(root, labelCustom, labels);
    const resetGroup = resetButton ? resolveGroup(root, resetButton, labels) : null;

    if (![borderGroup, themeGroup, presetsGroup, customGroup].every(Boolean)) return;
    const groups = [themeGroup, borderGroup, presetsGroup, customGroup, resetGroup].filter(Boolean);
    if (new Set(groups).size !== groups.length) return;

    root.dataset.ppSettingsV3 = VERSION;
    root.classList.add(ROOT_CLASS);

    [labelTheme, labelBorder, labelPresets, labelCustom].forEach(label => label?.classList.add('ppsv3-label'));
    themeGroup.classList.add('ppsv3-group', 'ppsv3-theme-group');
    borderGroup.classList.add('ppsv3-group', 'ppsv3-border-group');
    presetsGroup.classList.add('ppsv3-group', 'ppsv3-presets-group');
    customGroup.classList.add('ppsv3-group', 'ppsv3-custom-group');
    resetGroup?.classList.add('ppsv3-group', 'ppsv3-reset-group');
    resetButton?.classList.add('ppsv3-reset-button');

    const header = document.createElement('header');
    header.className = 'ppsv3-header';
    header.innerHTML = '<div class="ppsv3-title"><span aria-hidden="true">‹</span><div><strong>Paramètres</strong><small>Personnalisez votre ParadisePhone</small></div></div>';

    const layout = document.createElement('div');
    layout.className = 'ppsv3-layout';
    const appearance = makeSection('Apparence');
    const personalization = makeSection('Personnalisation');
    const other = makeSection('Autres');

    appearance.card.append(themeGroup, borderGroup);
    personalization.card.append(presetsGroup, customGroup);
    if (resetGroup) other.card.append(resetGroup);

    const info = document.createElement('p');
    info.className = 'ppsv3-info';
    info.innerHTML = '<span aria-hidden="true">ⓘ</span><span>Préférences enregistrées localement sur cet appareil.</span>';
    if (localInfo) localInfo.classList.add('ppsv3-original-info');

    layout.append(appearance.section, personalization.section);
    if (resetGroup) layout.append(other.section);
    layout.append(info);

    const childrenToHide = Array.from(root.children).filter(child => !groups.includes(child));
    childrenToHide.forEach(child => {
      if (child !== header && child !== layout && child.contains?.(labelBorder) === false && child.contains?.(themeGroup) === false) {
        const t = txt(child);
        if (t === 'parametres' || t === 'settings') child.classList.add('ppsv3-original-title');
      }
    });

    root.prepend(header);
    root.appendChild(layout);

    makeThemeSegments(themeGroup);
    decorateChoiceGroup(borderGroup, 'border');
    decorateChoiceGroup(presetsGroup, 'wallpaper');

    const urlInput = customGroup.querySelector('input[type="url"],input[type="text"],input:not([type])');
    if (urlInput) {
      urlInput.classList.add('ppsv3-url-input');
      urlInput.setAttribute('inputmode', 'url');
      urlInput.setAttribute('autocomplete', 'off');
      urlInput.addEventListener('input', () => validateUrl(urlInput, false));
      urlInput.addEventListener('blur', () => validateUrl(urlInput, true));
      urlInput.addEventListener('change', event => {
        if (!validateUrl(urlInput, true)) {
          event.preventDefault();
          event.stopPropagation();
        }
      }, true);
      urlInput.addEventListener('keydown', event => {
        if (event.key === 'Enter' && !validateUrl(urlInput, true)) {
          event.preventDefault();
          event.stopPropagation();
        }
      }, true);
    }

    root.addEventListener('click', event => {
      const target = event.target instanceof Element ? event.target : null;
      if (!target) return;
      const choice = target.closest('.ppsv3-choice');
      if (choice) {
        const group = choice.closest('.ppsv3-border-group,.ppsv3-presets-group');
        if (group) {
          all(group, '.ppsv3-choice').forEach(item => item.classList.remove('ppsv3-active'));
          choice.classList.add('ppsv3-active');
        }
      }
      if (resetButton && (target === resetButton || resetButton.contains(target)) && !resetBypass.has(resetButton)) {
        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation?.();
        showResetConfirm(root, resetButton);
      }
    }, true);
  }

  function refresh() {
    scheduled = false;
    const root = findSettingsRoot();
    if (root) enhance(root);
  }

  function schedule() {
    if (scheduled) return;
    scheduled = true;
    requestAnimationFrame(() => requestAnimationFrame(refresh));
  }

  new MutationObserver(schedule).observe(document.documentElement, { childList: true, subtree: true });
  window.addEventListener('paradise:phone', schedule);
  schedule();

  window.ParadisePhoneSettingsV3 = Object.freeze({ version: VERSION, refresh: schedule, isSafeUrl });
})();
