(() => {
    'use strict';

    if (window.__PARADISE_PHONE_GALLERY_CONFIRM_V1__) return;
    window.__PARADISE_PHONE_GALLERY_CONFIRM_V1__ = '1.0.0';

    const DELETE_SELECTOR = '[data-ppr-gallery-delete]';

    function closeConfirm(root) {
        root?.querySelector('.pgal-delete-confirm-overlay')?.remove();
    }

    function showConfirm(root, deleteButton) {
        if (!root || !deleteButton) return;
        closeConfirm(root);

        const overlay = document.createElement('div');
        overlay.className = 'pgal-delete-confirm-overlay';
        overlay.innerHTML = `
            <div class="pgal-delete-confirm-card" role="alertdialog" aria-modal="true" aria-labelledby="pgal-delete-confirm-title" aria-describedby="pgal-delete-confirm-text">
                <div class="pgal-delete-confirm-icon" aria-hidden="true">×</div>
                <strong id="pgal-delete-confirm-title">Supprimer cette photo ?</strong>
                <p id="pgal-delete-confirm-text">Cette action est définitive.</p>
                <div class="pgal-delete-confirm-actions">
                    <button type="button" class="pgal-delete-confirm-cancel" data-pgal-delete-cancel>Annuler</button>
                    <button type="button" class="pgal-delete-confirm-delete" data-pgal-delete-confirm>Supprimer</button>
                </div>
            </div>`;

        root.append(overlay);

        const cancelButton = overlay.querySelector('[data-pgal-delete-cancel]');
        const confirmButton = overlay.querySelector('[data-pgal-delete-confirm]');

        const cancel = () => closeConfirm(root);
        cancelButton?.addEventListener('click', cancel);
        overlay.addEventListener('click', event => {
            if (event.target === overlay) cancel();
        });
        overlay.addEventListener('keydown', event => {
            if (event.key === 'Escape') {
                event.preventDefault();
                cancel();
            }
        });

        confirmButton?.addEventListener('click', event => {
            event.preventDefault();
            event.stopPropagation();
            if (!deleteButton.isConnected || confirmButton.disabled) return;

            confirmButton.disabled = true;
            if (cancelButton) cancelButton.disabled = true;
            confirmButton.textContent = 'Suppression…';

            // Le gestionnaire actuel de la galerie réalise déjà correctement la
            // suppression serveur, le nettoyage du runtime et le rafraîchissement.
            // On lui laisse faire son travail, mais on valide son ancien confirm()
            // uniquement pour ce clic synthétique afin de ne plus afficher la
            // boîte de dialogue native du navigateur.
            deleteButton.dataset.pprGalleryDeleteApproved = '1';
            const nativeConfirm = window.confirm;
            window.confirm = () => true;
            try {
                deleteButton.click();
            } finally {
                window.confirm = nativeConfirm;
                delete deleteButton.dataset.pprGalleryDeleteApproved;
                closeConfirm(root);
            }
        });

        requestAnimationFrame(() => cancelButton?.focus());
    }

    document.addEventListener('click', event => {
        const deleteButton = event.target.closest?.(DELETE_SELECTOR);
        if (!deleteButton) return;

        const root = deleteButton.closest('.phone-gallery[data-ppr-ready], .phone-gallery');
        if (!root) return;

        // Le clic de confirmation généré ci-dessus doit atteindre le gestionnaire
        // existant de paradise-phone-complete.js sans être intercepté une seconde fois.
        if (deleteButton.dataset.pprGalleryDeleteApproved === '1') return;

        event.preventDefault();
        event.stopImmediatePropagation();
        showConfirm(root, deleteButton);
    }, true);
})();
