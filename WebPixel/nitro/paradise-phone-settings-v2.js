/* ParadiseRP — ParadisePhone Settings V2
 * UI/UX enhancement layer for the existing Settings screen.
 * It preserves the current controls/handlers/state and only decorates the live DOM.
 */
(() => {
  'use strict';

  if (window.ParadisePhoneSettingsV2) return;

  const VERSION = '2.0.0';
  const ROOT_SELECTOR = '#paradise-rp-hud .pp-device';
  const SETTINGS_SELECTOR = '.pp-settings';
  const ENHANCED = 'ppSettingsV2';
  const resetBypass = new WeakSet();
  let scheduled = false;
  let observer = null;

  const normalize = value => String(value || '')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[’‘]/g, "'")
    .replace(/\s+/g, ' ')
    .trim()
    .toLowerCase();

  const textOf = node => normalize(node?.textContent);

  function findTextNode(root, matcher) {
    const nodes = root.querySelectorAll('h1,h2,h3,h4,h5,h6,p,span,strong,small,label,legend,div');
    let best = null;
    for (const node of nodes) {
      const text = textOf(node);
      if (!text || text.length > 90 || !matcher(text, node)) continue;
      if (!best || node.children.length < best.children.length) best = node;
    }
    return best;
  }

  function interactiveCount(node) {
    return node?.querySelectorAll?.('button,input,select,textarea,[role="button"],[tabindex]').length || 0;
  }

  function resolveGroup(root, label, otherLabels = []) {
    if (!label) return null;
    let current = label;
    let fallback = label.parentElement;
    while (current && current !== root) {
      const parent = current.parentElement;
      if (!parent || parent === root) {
        fallback = current;
        break;
      }
      const containsOtherLabel = otherLabels.some(other => other && other !== label && parent.contains(other));
      if (!containsOtherLabel && interactiveCount(parent) > 0) return parent;
      if (!containsOtherLabel) fallback = parent;
      current = parent;
    }
    return fallback && fallback !== root ? fallback : null;
  }

  function candidateControls(group) {
    if (!group) return [];
    const found = Array.from(group.querySelectorAll('button,[role="button"],label,input[type="radio"],input[type="checkbox"],input[type="color"]'));
    return found.filter((node, index) => {
      if (node.matches('input') && node.closest('label') && found.includes(node.closest('label'))) return false;
      return found.indexOf(node) === index;
    });
  }

  function looksSelected(node) {
    if (!node) return false;
    if (node.matches?.(':checked,[aria-pressed="true"],[aria-selected="true"],[data-selected="true"],[data-active="true"],.active,.selected,.is-active')) return true;
    const input = node.querySelector?.('input');
    return Boolean(input?.checked);
  }

  function refreshChoices(group, kind) {
    if (!group) return;
    const controls = candidateControls(group);
    controls.forEach(control => {
      control.classList.add('ppsv2-choice', `ppsv2-${kind}-choice`);
      control.classList.toggle('ppsv2-is-active', looksSelected(control));
    });
  }

  function markClickedChoice(group, target) {
    if (!group || !target) return;
    const choice = target.closest('.ppsv2-choice');
    if (!choice || !group.contains(choice)) return;
    group.querySelectorAll('.ppsv2-choice').forEach(item => item.classList.remove('ppsv2-is-active'));
    choice.classList.add('ppsv2-is-active');
  }

  function isSafeWallpaperUrl(value) {
    const raw = String(value || '').trim();
    if (!raw) return true;
    try {
      const parsed = new URL(raw);
      return parsed.protocol === 'https:' || parsed.protocol === 'http:';
    } catch (_) {
      return false;
    }
  }

  function setUrlState(input, valid, showMessage = false) {
    if (!input) return;
    input.setAttribute('aria-invalid', valid ? 'false' : 'true');
    const field = input.closest('.ppsv2-custom-field') || input.parentElement;
    if (!field) return;
    let error = field.querySelector('.ppsv2-url-error');
    if (!valid && showMessage) {
      if (!error) {
        error = document.createElement('small');
        error.className = 'ppsv2-url-error';
        error.textContent = 'Utilisez une URL http:// ou https:// valide.';
        field.appendChild(error);
      }
    } else if (valid && error) {
      error.remove();
    }
  }

  function createHeader(settings) {
    let header = settings.querySelector(':scope > .ppsv2-header');
    if (header) return header;

    header = document.createElement('header');
    header.className = 'ppsv2-header';
    header.innerHTML = `
      <button type="button" class="ppsv2-back" aria-label="Retour à l’accueil du téléphone">
        <span aria-hidden="true">‹</span>
      </button>
      <div class="ppsv2-heading">
        <strong>Paramètres</strong>
        <span>Personnalisez votre ParadisePhone</span>
      </div>`;

    header.querySelector('.ppsv2-back').addEventListener('click', () => {
      const device = settings.closest('.pp-device');
      const home = device?.querySelector('[data-pp-home]');
      if (home instanceof HTMLElement) home.click();
    });

    settings.prepend(header);
    return header;
  }

  function makeSection(title, className) {
    const section = document.createElement('section');
    section.className = `ppsv2-section ${className}`;
    const heading = document.createElement('div');
    heading.className = 'ppsv2-section-title';
    heading.textContent = title;
    const card = document.createElement('div');
    card.className = 'ppsv2-card';
    section.append(heading, card);
    return { section, card };
  }

  function moveUnique(card, groups) {
    const seen = new Set();
    groups.filter(Boolean).forEach(group => {
      if (seen.has(group)) return;
      seen.add(group);
      card.appendChild(group);
    });
  }

  function decorateSettings(settings) {
    if (!(settings instanceof HTMLElement)) return;
    if (settings.dataset[ENHANCED] === VERSION) {
      refreshExisting(settings);
      return;
    }

    settings.dataset[ENHANCED] = VERSION;
    settings.classList.add('pp-settings-v2');

    const labelBorder = findTextNode(settings, t => t === 'couleur de la bordure' || t === 'bordure');
    const labelTheme = findTextNode(settings, t => t === 'theme');
    const labelPresets = findTextNode(settings, t => t.includes("fonds d'ecran predefinis") || t.includes('fonds d’ecran predefinis'));
    const labelCustom = findTextNode(settings, t => t.includes("fond d'ecran personnalise") || t.includes('fond d’ecran personnalise'));
    const labelLocal = findTextNode(settings, t => t.includes('parametres sont enregistres localement'));
    const resetButton = Array.from(settings.querySelectorAll('button,[role="button"]')).find(node => textOf(node).startsWith('reinitialiser')) || null;
    const originalTitle = findTextNode(settings, t => t === 'parametres');

    const labels = [labelBorder, labelTheme, labelPresets, labelCustom, labelLocal];
    const groupTheme = resolveGroup(settings, labelTheme, labels);
    const groupBorder = resolveGroup(settings, labelBorder, labels);
    const groupPresets = resolveGroup(settings, labelPresets, labels);
    const groupCustom = resolveGroup(settings, labelCustom, labels);
    const groupReset = resetButton ? resolveGroup(settings, resetButton, labels) : null;

    if (originalTitle && !originalTitle.closest('.ppsv2-header')) originalTitle.classList.add('ppsv2-original-title');
    [labelBorder, labelTheme, labelPresets, labelCustom].filter(Boolean).forEach(label => label.classList.add('ppsv2-setting-label'));
    if (labelLocal) labelLocal.classList.add('ppsv2-original-local-info');

    createHeader(settings);

    const existingLayout = settings.querySelector(':scope > .ppsv2-layout');
    if (!existingLayout) {
      const layout = document.createElement('div');
      layout.className = 'ppsv2-layout';
      const appearance = makeSection('Apparence', 'ppsv2-appearance');
      const personalization = makeSection('Personnalisation', 'ppsv2-personalization');
      const other = makeSection('Autres', 'ppsv2-other');

      const groups = [groupTheme, groupBorder, groupPresets, groupCustom, groupReset].filter(Boolean);
      const uniqueGroups = new Set(groups);
      const safeToReparent = groups.length >= 4 && uniqueGroups.size === groups.length;

      if (safeToReparent) {
        moveUnique(appearance.card, [groupTheme, groupBorder]);
        moveUnique(personalization.card, [groupPresets, groupCustom]);
        moveUnique(other.card, [groupReset]);
      } else {
        // Conservative fallback: keep the live hierarchy untouched and style it in place.
        [groupTheme, groupBorder].filter(Boolean).forEach(group => group.classList.add('ppsv2-card-row', 'ppsv2-appearance-row'));
        [groupPresets, groupCustom].filter(Boolean).forEach(group => group.classList.add('ppsv2-card-row', 'ppsv2-personalization-row'));
        if (groupReset) groupReset.classList.add('ppsv2-card-row', 'ppsv2-reset-row');
        appearance.card.appendChild(document.createComment('Existing appearance controls remain in their original hierarchy.'));
        personalization.card.appendChild(document.createComment('Existing personalization controls remain in their original hierarchy.'));
        other.card.appendChild(document.createComment('Existing reset control remains in its original hierarchy.'));
        layout.classList.add('ppsv2-layout-fallback');
      }

      const info = document.createElement('p');
      info.className = 'ppsv2-local-info';
      info.innerHTML = '<span aria-hidden="true">ⓘ</span><span>Vos préférences sont enregistrées sur cet appareil.</span>';

      layout.append(appearance.section, personalization.section, other.section, info);
      settings.appendChild(layout);
    }

    if (groupTheme) {
      groupTheme.classList.add('ppsv2-setting-group', 'ppsv2-theme-group');
      refreshChoices(groupTheme, 'theme');
    }
    if (groupBorder) {
      groupBorder.classList.add('ppsv2-setting-group', 'ppsv2-border-group');
      refreshChoices(groupBorder, 'border');
    }
    if (groupPresets) {
      groupPresets.classList.add('ppsv2-setting-group', 'ppsv2-wallpaper-group');
      refreshChoices(groupPresets, 'wallpaper');
    }
    if (groupCustom) {
      groupCustom.classList.add('ppsv2-setting-group', 'ppsv2-custom-group');
      const input = groupCustom.querySelector('input[type="url"],input[type="text"],input:not([type])');
      if (input) {
        input.classList.add('ppsv2-url-input');
        const field = input.parentElement;
        if (field) field.classList.add('ppsv2-custom-field');
        input.setAttribute('inputmode', 'url');
        input.setAttribute('autocomplete', 'off');
        input.setAttribute('spellcheck', 'false');
      }
    }
    if (groupReset) groupReset.classList.add('ppsv2-setting-group', 'ppsv2-reset-group');
    if (resetButton) resetButton.classList.add('ppsv2-reset-button');

    bindInteractions(settings, { groupTheme, groupBorder, groupPresets, groupCustom, resetButton });
    refreshExisting(settings);
  }

  function showResetConfirm(settings, resetButton) {
    let dialog = settings.querySelector('.ppsv2-confirm');
    if (dialog) return;

    dialog = document.createElement('div');
    dialog.className = 'ppsv2-confirm';
    dialog.setAttribute('role', 'dialog');
    dialog.setAttribute('aria-modal', 'true');
    dialog.setAttribute('aria-label', 'Confirmer la réinitialisation');
    dialog.innerHTML = `
      <div class="ppsv2-confirm-card">
        <strong>Réinitialiser les paramètres ?</strong>
        <p>Les préférences du ParadisePhone seront remises par défaut.</p>
        <div class="ppsv2-confirm-actions">
          <button type="button" data-ppsv2-cancel>Annuler</button>
          <button type="button" class="ppsv2-confirm-reset" data-ppsv2-confirm>Réinitialiser</button>
        </div>
      </div>`;

    const close = () => dialog.remove();
    dialog.querySelector('[data-ppsv2-cancel]').addEventListener('click', close);
    dialog.addEventListener('click', event => {
      if (event.target === dialog) close();
    });
    dialog.querySelector('[data-ppsv2-confirm]').addEventListener('click', () => {
      close();
      resetBypass.add(resetButton);
      resetButton.click();
      window.setTimeout(() => resetBypass.delete(resetButton), 0);
    });
    settings.appendChild(dialog);
    dialog.querySelector('[data-ppsv2-cancel]').focus();
  }

  function bindInteractions(settings, refs) {
    if (settings.dataset.ppSettingsV2Bound === 'true') return;
    settings.dataset.ppSettingsV2Bound = 'true';

    settings.addEventListener('click', event => {
      const target = event.target instanceof Element ? event.target : null;
      if (!target) return;
      markClickedChoice(refs.groupTheme, target);
      markClickedChoice(refs.groupBorder, target);
      markClickedChoice(refs.groupPresets, target);

      if (refs.resetButton && (target === refs.resetButton || refs.resetButton.contains(target))) {
        if (resetBypass.has(refs.resetButton)) return;
        event.preventDefault();
        event.stopPropagation();
        if (typeof event.stopImmediatePropagation === 'function') event.stopImmediatePropagation();
        showResetConfirm(settings, refs.resetButton);
      }
      window.setTimeout(() => refreshExisting(settings), 0);
    }, true);

    const input = refs.groupCustom?.querySelector('input[type="url"],input[type="text"],input:not([type])');
    if (input) {
      const guard = (event, showMessage) => {
        const valid = isSafeWallpaperUrl(input.value);
        setUrlState(input, valid, showMessage);
        if (!valid) {
          event.preventDefault();
          event.stopPropagation();
          if (typeof event.stopImmediatePropagation === 'function') event.stopImmediatePropagation();
        }
      };
      input.addEventListener('input', event => guard(event, false), true);
      input.addEventListener('change', event => guard(event, true), true);
      input.addEventListener('blur', event => guard(event, true), true);
      input.addEventListener('keydown', event => {
        if (event.key === 'Enter') guard(event, true);
      }, true);
    }
  }

  function refreshExisting(settings) {
    if (!(settings instanceof HTMLElement)) return;
    const theme = settings.querySelector('.ppsv2-theme-group');
    const border = settings.querySelector('.ppsv2-border-group');
    const wallpaper = settings.querySelector('.ppsv2-wallpaper-group');
    refreshChoices(theme, 'theme');
    refreshChoices(border, 'border');
    refreshChoices(wallpaper, 'wallpaper');
  }

  function refresh() {
    scheduled = false;
    const device = document.querySelector(ROOT_SELECTOR);
    const settings = device?.querySelector(SETTINGS_SELECTOR);
    if (settings) decorateSettings(settings);
  }

  function scheduleRefresh() {
    if (scheduled) return;
    scheduled = true;
    requestAnimationFrame(() => requestAnimationFrame(refresh));
  }

  observer = new MutationObserver(scheduleRefresh);
  observer.observe(document.documentElement, { childList: true, subtree: true, attributes: true, attributeFilter: ['class', 'aria-pressed', 'aria-selected', 'data-selected', 'data-active', 'checked'] });
  window.addEventListener('paradise:phone', scheduleRefresh, false);
  window.addEventListener('beforeunload', () => observer?.disconnect(), { once: true });
  scheduleRefresh();

  window.ParadisePhoneSettingsV2 = Object.freeze({
    version: VERSION,
    refresh: scheduleRefresh,
    isSafeWallpaperUrl,
    getStatus: () => ({
      version: VERSION,
      enhanced: Boolean(document.querySelector(`${ROOT_SELECTOR} ${SETTINGS_SELECTOR}.pp-settings-v2`)),
      dialogOpen: Boolean(document.querySelector(`${ROOT_SELECTOR} .ppsv2-confirm`))
    })
  });
})();
