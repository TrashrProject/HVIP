/* ParadiseRP — ParadisePhone Settings V4
 * Self-contained UI + interactions for theme, border color, wallpapers, custom URL and reset.
 */
(() => {
  'use strict';
  if (window.ParadisePhoneSettingsV4) return;

  const VERSION = '4.0.0';
  const STORAGE_KEY = 'paradise_phone_settings_v4';
  const DEFAULTS = { theme:'dark', border:'#168cff', wallpaper:'', wallpaperType:'preset', customUrl:'' };
  const PRESET_FALLBACKS = [
    'linear-gradient(135deg,#2f5f9c,#21476f)',
    'linear-gradient(135deg,#3b4145,#262d31)',
    'linear-gradient(135deg,#233b50,#172c3e)',
    'linear-gradient(135deg,#68446e,#46314e)',
    'linear-gradient(135deg,#342b69,#211c49)'
  ];
  const BORDER_FALLBACKS = ['#168cff','#21c7e8','#7b5cff','#e45e68','#f59a32','#20272c'];
  let observer;
  let scheduled = false;

  const norm = value => String(value ?? '').normalize('NFD').replace(/[\u0300-\u036f]/g,'').replace(/\s+/g,' ').trim().toLowerCase();
  const read = () => {
    try { return { ...DEFAULTS, ...(JSON.parse(localStorage.getItem(STORAGE_KEY) || '{}')) }; }
    catch (_) { return { ...DEFAULTS }; }
  };
  const write = state => {
    try { localStorage.setItem(STORAGE_KEY, JSON.stringify(state)); } catch (_) {}
  };

  function settingsRoots() {
    return [...document.querySelectorAll('.pp-settings')].filter(el => {
      const t = norm(el.textContent);
      return t.includes('theme') && t.includes('bordure') && t.includes('fond');
    });
  }

  function getDevice(root) { return root.closest('.pp-device') || document.querySelector('.pp-device'); }
  function getPage(root) { return root.closest('.pp-app-page') || root.parentElement; }

  function findByText(root, needle) {
    const nodes = root.querySelectorAll('label,span,strong,small,p,div,h1,h2,h3,h4,legend');
    let best = null;
    for (const node of nodes) {
      const text = norm(node.textContent);
      if (!text || text.length > 100 || !text.includes(needle)) continue;
      if (!best || node.children.length < best.children.length) best = node;
    }
    return best;
  }

  function groupFor(root, label, stopLabels=[]) {
    if (!label) return null;
    let node = label.parentElement;
    let fallback = node;
    while (node && node !== root) {
      const hasOther = stopLabels.some(other => other && other !== label && node.contains(other));
      if (!hasOther) fallback = node;
      const count = node.querySelectorAll('button,input,select,[role="button"]').length;
      if (!hasOther && count > 0) return node;
      node = node.parentElement;
    }
    return fallback;
  }

  function safeUrl(url) {
    const raw = String(url || '').trim();
    if (!raw) return true;
    try { const u = new URL(raw, location.href); return u.protocol === 'https:' || u.protocol === 'http:'; }
    catch (_) { return false; }
  }

  function applyState(root, state) {
    const device = getDevice(root);
    if (!device) return;
    device.dataset.ppTheme = state.theme;
    device.style.setProperty('--ppsv4-border-color', state.border || DEFAULTS.border);
    device.classList.toggle('ppsv4-light', state.theme === 'light');
    device.classList.toggle('ppsv4-dark', state.theme !== 'light');

    const content = device.querySelector('.pp-content') || root.closest('.pp-content');
    if (content) {
      const wallpaper = state.wallpaper || '';
      content.style.setProperty('--ppsv4-wallpaper', wallpaper || 'none');
      content.classList.toggle('ppsv4-has-wallpaper', Boolean(wallpaper));
    }
  }

  function decoratePage(root) {
    root.classList.add('pp-settings-v4');
    const page = getPage(root);
    page?.classList.add('pp-settings-page-v4');
    const header = page?.querySelector(':scope > header');
    if (header) {
      header.classList.add('ppsv4-page-header');
      const title = [...header.querySelectorAll('strong,span,h1,h2')].find(n => norm(n.textContent) === 'parametres');
      title?.classList.add('ppsv4-page-title');
      const back = header.querySelector('button,[data-pp-home]');
      back?.classList.add('ppsv4-page-back');
      if (!header.querySelector('.ppsv4-subtitle')) {
        const subtitle = document.createElement('small');
        subtitle.className = 'ppsv4-subtitle';
        subtitle.textContent = 'Personnalisez votre ParadisePhone';
        title?.insertAdjacentElement('afterend', subtitle);
      }
    }
  }

  function decorateAndBind(root) {
    if (!(root instanceof HTMLElement)) return;
    decoratePage(root);

    const borderLabel = findByText(root,'couleur de la bordure') || findByText(root,'bordure');
    const themeLabel = findByText(root,'theme');
    const presetLabel = findByText(root,"fonds d'ecran predefinis") || findByText(root,'fonds d’ecran predefinis');
    const customLabel = findByText(root,"fond d'ecran personnalise") || findByText(root,'fond d’ecran personnalisé');
    const infoLabel = findByText(root,'parametres sont enregistres localement');
    const labels = [borderLabel, themeLabel, presetLabel, customLabel, infoLabel];

    const borderGroup = groupFor(root,borderLabel,labels);
    const themeGroup = groupFor(root,themeLabel,labels);
    const presetGroup = groupFor(root,presetLabel,labels);
    const customGroup = groupFor(root,customLabel,labels);
    const reset = [...root.querySelectorAll('button,[role="button"]')].find(n => norm(n.textContent).startsWith('reinitialiser'));

    borderGroup?.classList.add('ppsv4-card','ppsv4-border-group');
    themeGroup?.classList.add('ppsv4-card','ppsv4-theme-group');
    presetGroup?.classList.add('ppsv4-card','ppsv4-wallpaper-group');
    customGroup?.classList.add('ppsv4-card','ppsv4-custom-group');
    [borderLabel,themeLabel,presetLabel,customLabel].filter(Boolean).forEach(n=>n.classList.add('ppsv4-label'));
    infoLabel?.classList.add('ppsv4-local-info');
    reset?.classList.add('ppsv4-reset');

    let state = read();
    applyState(root,state);

    // Theme control: supports select, buttons and radios.
    const themeSelect = themeGroup?.querySelector('select');
    if (themeSelect) {
      themeSelect.classList.add('ppsv4-theme-select');
      const applyThemeFromSelect = () => {
        const raw = norm(themeSelect.value || themeSelect.options?.[themeSelect.selectedIndex]?.text);
        state = { ...read(), theme: /clair|light|jour/.test(raw) ? 'light' : 'dark' };
        write(state); applyState(root,state); refresh(root);
      };
      if (!themeSelect.dataset.ppsv4Bound) {
        themeSelect.dataset.ppsv4Bound='1';
        themeSelect.addEventListener('change', applyThemeFromSelect);
        themeSelect.addEventListener('input', applyThemeFromSelect);
      }
      const target = state.theme === 'light' ? ['clair','light','jour'] : ['sombre','dark','nuit'];
      [...themeSelect.options].forEach((opt,i)=>{ if(target.some(x=>norm(opt.value+' '+opt.text).includes(x))) themeSelect.selectedIndex=i; });
    }

    const themeChoices = [...(themeGroup?.querySelectorAll('button,[role="button"],label,input[type="radio"]') || [])];
    themeChoices.forEach(choice => {
      if (choice.matches('input') && choice.closest('label')) return;
      choice.classList.add('ppsv4-theme-choice');
      const txt = norm(choice.textContent || choice.value || choice.getAttribute('aria-label'));
      const key = /clair|light|jour/.test(txt) ? 'light' : /sombre|dark|nuit/.test(txt) ? 'dark' : null;
      if (key) choice.classList.toggle('ppsv4-active', state.theme === key);
      if (key && !choice.dataset.ppsv4Bound) {
        choice.dataset.ppsv4Bound='1';
        choice.addEventListener('click', e => {
          e.preventDefault();
          state = { ...read(), theme:key }; write(state); applyState(root,state); refresh(root);
        });
      }
    });

    // Border colors.
    const borderChoices = [...(borderGroup?.querySelectorAll('button,[role="button"],label,input[type="radio"],input[type="color"]') || [])]
      .filter((el,i,a)=>!(el.matches('input')&&el.closest('label')&&a.includes(el.closest('label'))));
    borderChoices.forEach((choice,index) => {
      choice.classList.add('ppsv4-border-choice');
      const cs = getComputedStyle(choice);
      const attr = choice.dataset.color || choice.getAttribute('value') || choice.getAttribute('data-value');
      let color = attr || cs.backgroundColor;
      if (!color || color === 'rgba(0, 0, 0, 0)' || color === 'transparent') color = BORDER_FALLBACKS[index % BORDER_FALLBACKS.length];
      choice.dataset.ppsv4Color = color;
      choice.style.setProperty('--ppsv4-choice-color', color);
      choice.classList.toggle('ppsv4-active', String(state.border).toLowerCase() === String(color).toLowerCase());
      if (!choice.dataset.ppsv4Bound) {
        choice.dataset.ppsv4Bound='1';
        choice.addEventListener('click', e => {
          e.preventDefault();
          state = { ...read(), border:color }; write(state); applyState(root,state); refresh(root);
        });
      }
    });

    // Preset wallpapers.
    const wallpaperChoices = [...(presetGroup?.querySelectorAll('button,[role="button"],label,input[type="radio"]') || [])]
      .filter((el,i,a)=>!(el.matches('input')&&el.closest('label')&&a.includes(el.closest('label'))));
    wallpaperChoices.forEach((choice,index) => {
      choice.classList.add('ppsv4-wallpaper-choice');
      const cs = getComputedStyle(choice);
      let wallpaper = choice.dataset.wallpaper || choice.getAttribute('data-value') || choice.style.backgroundImage || cs.backgroundImage;
      if (!wallpaper || wallpaper === 'none') wallpaper = PRESET_FALLBACKS[index % PRESET_FALLBACKS.length];
      choice.dataset.ppsv4Wallpaper = wallpaper;
      if (!choice.style.backgroundImage && wallpaper.includes('gradient')) choice.style.backgroundImage = wallpaper;
      choice.classList.toggle('ppsv4-active', state.wallpaper === wallpaper && state.wallpaperType === 'preset');
      if (!choice.dataset.ppsv4Bound) {
        choice.dataset.ppsv4Bound='1';
        choice.addEventListener('click', e => {
          e.preventDefault();
          state = { ...read(), wallpaper, wallpaperType:'preset', customUrl:'' }; write(state); applyState(root,state); refresh(root);
        });
      }
    });

    // Custom wallpaper URL.
    const customInput = customGroup?.querySelector('input[type="url"],input[type="text"],input:not([type])');
    if (customInput) {
      customInput.classList.add('ppsv4-url');
      if (state.customUrl && customInput.value !== state.customUrl) customInput.value = state.customUrl;
      const commitUrl = () => {
        const value = String(customInput.value || '').trim();
        const valid = safeUrl(value);
        customInput.classList.toggle('ppsv4-invalid', !valid);
        customInput.setAttribute('aria-invalid', valid ? 'false' : 'true');
        if (!valid) return;
        const wallpaper = value ? `url("${value.replace(/"/g,'%22')}")` : '';
        state = { ...read(), customUrl:value, wallpaper, wallpaperType:value?'custom':'preset' };
        write(state); applyState(root,state); refresh(root);
      };
      if (!customInput.dataset.ppsv4Bound) {
        customInput.dataset.ppsv4Bound='1';
        customInput.addEventListener('change',commitUrl);
        customInput.addEventListener('blur',commitUrl);
        customInput.addEventListener('keydown',e=>{ if(e.key==='Enter'){ e.preventDefault(); commitUrl(); customInput.blur(); } });
        customInput.addEventListener('input',()=>{
          const ok=safeUrl(customInput.value); customInput.classList.toggle('ppsv4-invalid',!ok);
        });
      }
    }

    // Reset.
    if (reset && !reset.dataset.ppsv4Bound) {
      reset.dataset.ppsv4Bound='1';
      reset.addEventListener('click', e => {
        e.preventDefault();
        if (!window.confirm('Réinitialiser les paramètres du ParadisePhone ?')) return;
        state = { ...DEFAULTS }; write(state); applyState(root,state);
        if (customInput) customInput.value='';
        refresh(root);
      });
    }

    refresh(root);
  }

  function refresh(root) {
    const state = read();
    applyState(root,state);
    root.querySelectorAll('.ppsv4-theme-choice').forEach(el=>{
      const txt=norm(el.textContent||el.value||el.getAttribute('aria-label'));
      const key=/clair|light|jour/.test(txt)?'light':/sombre|dark|nuit/.test(txt)?'dark':null;
      if(key) el.classList.toggle('ppsv4-active',state.theme===key);
    });
    root.querySelectorAll('.ppsv4-border-choice').forEach(el=>el.classList.toggle('ppsv4-active',String(el.dataset.ppsv4Color).toLowerCase()===String(state.border).toLowerCase()));
    root.querySelectorAll('.ppsv4-wallpaper-choice').forEach(el=>el.classList.toggle('ppsv4-active',state.wallpaperType==='preset'&&el.dataset.ppsv4Wallpaper===state.wallpaper));
  }

  function run() {
    scheduled=false;
    settingsRoots().forEach(decorateAndBind);
  }
  function schedule() {
    if (scheduled) return;
    scheduled=true;
    requestAnimationFrame(run);
  }

  observer = new MutationObserver(schedule);
  observer.observe(document.documentElement,{childList:true,subtree:true});
  window.addEventListener('paradise:phone',schedule);
  window.addEventListener('storage',schedule);
  schedule();

  window.ParadisePhoneSettingsV4 = Object.freeze({ version:VERSION, refresh:schedule, reset(){ localStorage.removeItem(STORAGE_KEY); schedule(); } });
})();