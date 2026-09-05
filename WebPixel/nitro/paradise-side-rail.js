(() => {
    const BUILD = 'paradise-side-rail-native-v12';
    const LABELS = [
        'Inventaire',
        'Centre des métiers',
        'Gang',
        'Profil',
        'Téléphone',
        'Mes appartements',
        'Skin d’armes'
    ];

    const WEAPON_SKINS_ICON = '/nitro/weapon-skins/g36_584.png?v=20260905-1';

    const decorateItem = (item, index, items) => {
        item.classList.add('paradise-side-rail-item');
        item.dataset.paradiseIndex = String(index);
        item.dataset.paradiseLabel = LABELS[index] || `Menu ${index + 1}`;

        item.setAttribute('aria-label', item.dataset.paradiseLabel);
        item.setAttribute('title', item.dataset.paradiseLabel);

        if (index === 6) {
            const image = item.querySelector('img');
            if (image) {
                image.src = WEAPON_SKINS_ICON;
                image.alt = '';
                image.classList.add('paradise-weapon-skins-rail-icon');
            }
        }

        if (item.dataset.paradiseActiveBound === '1') return;

        item.dataset.paradiseActiveBound = '1';
        item.addEventListener('click', () => {
            items.forEach(other => {
                other.classList.toggle('is-active', other === item);
            });
        }, { passive: true });
    };

    const decorate = () => {
        const shell = document.querySelector('.roleplay-left-menu');
        if (!shell) return false;

        shell.classList.add('paradise-side-rail-shell');
        shell.dataset.paradiseBuild = BUILD;

        const items = [...shell.querySelectorAll('.roleplay-left-menu-buttons .left-menu-button')];
        items.forEach((item, index) => decorateItem(item, index, items));

        const toggle = shell.querySelector('.roleplay-left-menu-toggle');
        if (toggle) {
            toggle.classList.add('paradise-side-rail-native-toggle');

            if (toggle.dataset.paradiseRefreshBound !== '1') {
                toggle.dataset.paradiseRefreshBound = '1';
                toggle.addEventListener('click', () => {
                    // Nitro owns the collapse state and chevron. We only re-apply
                    // lightweight decoration after its native render cycle.
                    [0, 90, 220, 500].forEach(delay => {
                        window.setTimeout(decorate, delay);
                    });
                }, { passive: true });
            }
        }

        console.info('[ParadiseRP] native side rail ready', BUILD, {
            items: items.length,
            toggle: !!toggle
        });

        return true;
    };

    // Nitro is mounted asynchronously. Finite retries are sufficient and avoid
    // permanent DOM observers or viewport-wide element scans.
    [500, 1200, 2400, 4200, 7000].forEach(delay => {
        window.setTimeout(decorate, delay);
    });

    console.info('[ParadiseRP] side rail enhancer loaded', BUILD);
})();
