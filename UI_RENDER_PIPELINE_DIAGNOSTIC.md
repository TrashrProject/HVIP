# ParadiseRP — diagnostic du pipeline UI réellement rendu

Branche de diagnostic: `diag/paradise-ui-render-pipeline`
Base: `quality/paradise-core-v1-fixes`

## Conclusion

Les écrans visibles ne proviennent pas d'un ancien composant React caché. Ils sont montés par les scripts statiques Paradise chargés par `WebPixel/nitro-last/index.html` au-dessus du Nitro React root.

La cause principale du faible impact visuel des passes précédentes est l'empilement de couches CSS/JS de polish sur les mêmes DOM, avec des feuilles plus tardives qui écrasent les feuilles antérieures. Le problème est particulièrement fort sur ParadisePhone, où plusieurs feuilles utilisent des sélecteurs très spécifiques et de nombreux `!important`.

La seconde cause est structurelle: plusieurs demandes précédentes ont été implémentées comme des couches de finition conservant le même markup de base. Ainsi, même lorsque les changements étaient réellement chargés, la composition générale restait volontairement proche de la version précédente.

Le cache / service worker n'est pas la cause principale observée: `/play` charge explicitement `/nitro-last/index.html?sso=...`, le loader Nitro-last désenregistre les service workers, et les versions runtime du Quality Gate visibles en console prouvent que les fichiers récents sont servis. Les query strings statiques `?v=1`, `?v=2`, etc. restent néanmoins une faiblesse de cache-busting à corriger plus tard de façon déterministe.

## Pipeline public réellement servi

`WebPixel/play.php`
→ iframe `/nitro-last/index.html?sso=...`
→ `WebPixel/nitro-last/index.html`
→ Nitro root `#root` + Paradise root `#paradise-ui-root`
→ `paradise-ui-foundation.js`
→ `#paradise-rp-hud`
→ WindowManager / fenêtres Profile, Inventory, Phone
→ modules métier/visuels dédiés.

Les bundles `assets/index-9f9954ad.js`, `vendor-0a5897c2.js`, `nitro-renderer-1dc7b8c5.js` servent Nitro. Les modules Paradise du HUD/Profile/Inventory/Phone sont des fichiers JS/CSS statiques séparés: ils ne nécessitent pas un rebuild du bundle React Nitro.

## Roots

1. `#root` — Nitro React / room / UI native Nitro.
2. `#paradise-ui-root` — overlay ParadiseRP.
3. `#paradise-rp-hud` — contenu généré par `paradise-ui-foundation.js` dans le Paradise root.
4. ParadisePhone n'est pas un root séparé: `.pp-device` vit dans la fenêtre `phone` du HUD Paradise.

Les dossiers `WebPixel/nitro`, `WebPixel/nitroo` et `WebPixel/nitro-last` coexistent, mais `/play` pointe explicitement sur `nitro-last`; les deux anciens dossiers ne sont donc pas la branche UI active pour la route publique actuelle.

## Main HUD / PlayerHUD / EconomyHUD

Markup et WindowManager:
- `WebPixel/nitro-last/paradise-ui-foundation.js`

Styles, dans l'ordre de cascade:
1. `paradise-ui-foundation.css`
2. `paradise-stable-ui-fixes.css`
3. `paradise-ui-phase11.css`
4. `paradise-quality-gate.css` pour certaines finitions globales
5. `paradise-clickthrough-guard.css` en dernier pour la sécurité pointer-events

`paradise-ui-phase11.css` réécrit déjà explicitement les tailles et espacements du PlayerHUD, EconomyHUD, navigation, chat et boutons de coin. Modifier uniquement `paradise-ui-foundation.css` ne garantit donc pas un changement visible.

## Profile

Fenêtre/shell:
- créée par `paradise-ui-foundation.js` avec `.pr-window[data-window="profile"]`.

Contenu actif réellement monté:
- `paradise-character-v2.js`
- root `.pr2-profile`
- tabs `.pr2-tabs`
- aperçu `.pr2-overview-grid`
- carte avatar `.pr2-avatar-card`
- cartes état/vie/bio `.pr2-card`

Textes visibles comme `Vie à Placid Island`, `État du personnage`, `Biographie`, `Aperçu`, `Identité`, `Documents`, `Statistiques`, `Réputation` sont générés dans `paradise-character-v2.js`.

Styles:
1. base `paradise-character-v2.css`
2. override tardif `paradise-quality-gate.css`

Le Quality Gate redéfinit notamment la largeur de la fenêtre Profile, les tabs, `.pr2-overview-grid`, `.pr2-avatar-card`, `.pr2-card`, `.pr2-stat-card`, `.pr2-info`, `.pr2-bio`, etc. C'est une cause directe pour laquelle une modification faite seulement dans `paradise-character-v2.css` pouvait être peu visible ou annulée.

Le vieux `profileBody()` de `paradise-ui-foundation.js` existe encore comme placeholder/fallback de fondation, mais `paradise-character-v2.js` remplace le contenu de la fenêtre active. Ce n'est pas deux Profiles visibles superposés.

## Inventory

Fenêtre/shell:
- créée par `paradise-ui-foundation.js`.

Contenu actif réellement monté:
- `paradise-inventory-v2.js`
- root `.pr3-inventory`
- `.pr3-inventory-header`
- `.pr3-tabs`
- `.pr3-main`
- `.pr3-grid-panel`
- `.pr3-detail-panel`

Le texte `Objets physiques du personnage` est généré par ce composant. Le visuel `TEL` n'est pas un reste d'une ancienne UI: `paradise-inventory-v2.js` génère explicitement le placeholder téléphone via `iconMarkup()`.

Styles actifs:
- `paradise-inventory-v2.css`
- puis les règles globales chargées après lui.

La grande zone claire vide visible dans les captures est cohérente avec le markup et les styles actuels (`pr3-grid-panel` / `pr3-detail-panel`). Elle n'est donc pas la preuve d'un mauvais build: cette structure n'a tout simplement jamais été réellement recomposée.

## ParadisePhone / Contacts / Messages

Fenêtre/shell:
- créée par `paradise-ui-foundation.js`.

Logique/composant actif:
- `paradise-phone-v1.js`
- `.pp-device`, `.pp-screen`, `.pp-content`
- `homeMarkup()`
- `conversationsMarkup()` → Messages
- `conversationMarkup()` → conversation
- `contactsMarkup()` → Contacts
- `callsMarkup()` → appels
- `notificationsMarkup()` → notifications
- `settingsMarkup()` → paramètres

Textes `PLACID ISLAND`, `Messages`, `Aucune conversation` viennent de cette couche fonctionnelle ou des couches de layout qui réutilisent ses données.

Cascade visuelle Phone:
1. `paradise-phone-v1.css`
2. `paradise-quality-gate.css`
3. `paradise-phone-v1-ux.css`
4. `paradise-phone-final.css`
5. `paradise-phone-layout-final.css`

Puis les scripts de transformation visuelle:
- `paradise-phone-v1-ux.js`
- `paradise-phone-final.js`
- `paradise-phone-layout-final.js`

`paradise-phone-final.css` et `paradise-phone-layout-final.css` emploient des sélecteurs plus spécifiques sur `[data-pp-final="1"]` et beaucoup de `!important`. Les changements apportés dans `paradise-phone-v1.css` ou une couche antérieure peuvent donc être complètement masqués.

`paradise-phone-layout-final.js` injecte réellement la zone `Aperçu / Messages / Alertes` visible sur la capture. Cela prouve que la dernière couche est exécutée dans le client actuel.

## Documents

L'onglet Documents à l'intérieur de `Mon Profil` appartient à `paradise-character-v2.js` et utilise les styles `.pr2-doc-*` / `.pr2-idcard-*` de `paradise-character-v2.css`, avec les ajustements tardifs du Quality Gate.

La fenêtre globale `documents` existe aussi dans la fondation/WindowManager pour la navigation bas-gauche. Il faut distinguer cette fenêtre du tab Documents du Profile lors des prochains travaux.

## Build / output

Pour la Paradise UI actuelle:
- source modifiée: directement `WebPixel/nitro-last/paradise-*.js/.css`
- étape build supplémentaire: aucune
- output servi: le même dossier `WebPixel/nitro-last`
- entrée publique: `WebPixel/play.php`
- iframe réelle: `/nitro-last/index.html?sso=...`

Le Nitro React est déjà bundlé dans `nitro-last/assets/`. Les changements Paradise ne sont pas compilés dans `index-9f9954ad.js`.

## Cache / Service Worker

Un ancien `asset-sw.js` existe. Il ne met pas en cache les réponses réussies: il fait du network-first et ne fournit un fallback que pour les assets manquants. Surtout, `nitro-last/index.html` désenregistre toutes les registrations service worker au chargement.

Le cache-busting des fichiers Paradise est cependant manuel (`?v=1`, `?v=2`, `?v=3`). C'est une faiblesse réelle: si le contenu change sans changement de query string et qu'un proxy/CDN applique un cache, une ancienne ressource pourrait survivre. Ce n'est toutefois pas la cause principale des captures actuelles, puisque les versions runtime du Quality Gate et les éléments ajoutés par les dernières couches sont visibles.

## Feature flags / condition de rendu

Dans le loader `nitro-last/index.html`, les modules Paradise Profile/Inventory/Phone sont chargés sans feature flag conditionnel. Le `WindowManager` choisit uniquement quelle fenêtre est ouverte via `ParadiseStore.ui.activeWindow`. Le Phone choisit son app interne via son propre état.

Aucune branche `renderOldProfile()/renderNewProfile()` n'explique le rendu actuel. Les placeholders de la fondation sont remplacés par les modules dédiés lors de leur mount.

## Cause réelle des précédentes différences trop faibles

1. Les bons fichiers étaient globalement modifiés et servis.
2. Les écrans sont restés sur le même DOM fondamental.
3. Des couches CSS de plus en plus tardives ont été ajoutées au lieu de consolider les styles.
4. Certaines couches tardives gagnent par ordre, spécificité et `!important`.
5. Plusieurs prompts demandaient explicitement de préserver la structure existante, donc l'implémentation a surtout fait du polish plutôt qu'une recomposition.
6. Inventory en est la preuve la plus simple: la grande grille blanche et `TEL` sont codés dans le composant actif lui-même.
7. Le cache/build n'est pas la cause principale observée.

## Diagnostic runtime ajouté sur cette branche

`paradise-render-diagnostic.js` est chargé avec une query unique `?v=20260822-render-proof-1`.

Il expose:
- `ParadiseRenderDiagnostic.report()`
- `ParadiseRenderDiagnostic.profile()`
- `ParadiseRenderDiagnostic.inventory()`
- `ParadiseRenderDiagnostic.phone()`
- `ParadiseRenderDiagnostic.roots()`
- `ParadiseRenderDiagnostic.assets()`
- `ParadiseRenderDiagnostic.modules()`
- `ParadiseRenderDiagnostic.serviceWorkers()`

Pour les éléments visibles, il retourne les computed styles demandés et les règles CSS candidates gagnantes pour `background`, `border`, `border-radius`, `padding`, `font-size`, `box-shadow`, `width`, `height`.

## Test de preuve Profile uniquement

Cette branche ajoute volontairement, uniquement pour le diagnostic:
- outline magenta 4 px sur la fenêtre Profile ouverte;
- label `DEV-RENDER-CHECK` dans le header.

Si ces deux éléments apparaissent après déploiement de la branche, la chaîne suivante est prouvée dans le vrai client:

`/play` → `/nitro-last/index.html` → script diagnostic servi → vraie `.pr-window[data-window="profile"]` affichée.

Après capture/validation, ce marqueur doit être supprimé immédiatement. Aucun test visuel n'est encore appliqué à Inventory ou Phone.
