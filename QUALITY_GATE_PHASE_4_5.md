# ParadiseRP — Phase 4.5 — Quality Gate Core V1

> **État du rapport : PRE-RUNTIME.**
> L'audit statique et les corrections identifiables sont effectués sur `quality/paradise-core-v1-fixes`.
> Le point de restauration est `quality/paradise-core-v1`.
> Aucun résultat VPS/2 joueurs/50 rooms n'est inventé : tant que ces tests ne sont pas exécutés, le Quality Gate global reste **FAIL / RETEST REQUIS**.

## Scope verrouillé

- Pas de Phase 5.
- Pas de Banque, Jobs, Entreprises, Immobilier, Police, EMS, Crime ou nouveau gros système.
- Pas de redesign de ParadisePhone.
- Pas de modification Nitro renderer / Pixi / canvas / WebSocket / framing / packet parser / nitro-proxy.
- Corrections limitées à audit, cohérence, bugfix, performance, sécurité et stabilité du Core existant.

## Corrections effectuées

### 1. Encodage UTF-8 Inventory / Documents

**Problème :** les libellés `Téléphone`, `Clé`, etc. apparaissaient corrompus dans l'inventaire (`T├...`).

**Cause :** les migrations sources sont UTF-8, mais l'import/connexion MariaDB Windows pouvait interpréter les littéraux avec un charset client incorrect.

**Fichiers :**
- `migrations/2026_08_21_quality_gate_utf8_repair.sql`
- `WebPixel/rp-inventory-data.php`
- `WebPixel/rp-phone-data.php`
- `WebPixel/rp-phone-action.php`
- `WebPixel/rp-character-action.php`

**Correction :**
- réparation idempotente avec littéraux hex UTF-8 + `CONVERT(... USING utf8mb4)` ;
- `SET NAMES utf8mb4` dans la migration ;
- `mysqli_set_charset(..., 'utf8mb4')` dans les endpoints concernés.

**Retest VPS :** migration à importer puis vérifier Inventory et documents.

### 2. Escape / focus global

**Problème :** Escape n'avait pas une politique uniforme. Dans plusieurs inputs il ne faisait rien ; dans le téléphone il pouvait fermer directement la fenêtre au lieu de revenir à l'écran précédent.

**Fichier :** `WebPixel/nitro-last/paradise-quality-gate-input.js`.

**Correction :**
- input Paradise actif → Escape retire le focus ;
- conversation téléphone → Escape revient aux Messages ;
- app téléphone → Escape revient au Home ;
- Home téléphone → Escape ferme le téléphone ;
- autre fenêtre active → Escape ferme la fenêtre ;
- menu Actions → Escape ferme le menu.

**Retest VPS :** Chat, bio, SMS, Contacts, Profile, Phone.

### 3. Double actions Inventory

**Problème :** `Use` avait une garde rapide mais `Give` n'avait pas de garde dédiée contre un double clic.

**Fichier :** `RDP EMU header change/HabboRoleplay/Paradise/Inventory/InventoryService.cs`.

**Correction :** garde serveur par utilisateur sur les transferts + transaction/`FOR UPDATE` déjà présente dans le Repository.

**Retest VPS :** double-clic sur donner avec deux comptes et vérification quantité source/cible.

### 4. Feedback Documents / Inventory

**Problème :** les conséquences gameplay n'avaient pas toujours un préfixe cohérent.

**Fichiers :**
- `InventoryService.cs`
- `DocumentService.cs`

**Correction :** retours serveur validés préfixés `[INVENTAIRE]` / `[DOCUMENT]` pour les conséquences concernées.

**Limite :** ParadisePhone produit aujourd'hui un feedback UI/événement après validation serveur, pas encore un canal système natif commun à tous les domaines. Ce point reste volontairement signalé au lieu d'ajouter une nouvelle architecture pendant le Quality Gate.

### 5. ParadisePhone — sécurité et cohérence

**Problèmes :**
- endpoint d'action moins strict que les autres endpoints ;
- double SMS rapide possible ;
- notifications non marquées lues côté serveur ;
- `Mode silencieux` / `Sons` persistés mais sans moteur audio réel ;
- réglage Notifications persisté mais pas appliqué à la création des notifications.

**Fichiers :**
- `WebPixel/rp-phone-action.php`
- `WebPixel/nitro-last/paradise-quality-gate-phone.js`

**Corrections :**
- POST obligatoire ;
- `X-Paradise-Action: phase4` obligatoire ;
- vérification same-origin ;
- payload JSON borné ;
- charset utf8mb4 ;
- garde SMS 1 seconde / 1 action + limite existante ;
- action propriétaire `read_notifications` ;
- lecture persistée en SQL ;
- `notifications_enabled` contrôle réellement la création des notifications ;
- switches sans fonction audio retirés de l'UI au lieu d'être décoratifs.

**Retest VPS :** deux comptes, unread/read, réglage Notifications OFF/ON, SMS offline/reconnexion.

### 6. Character IDs

**Problème :** la BDD avait bien des contraintes UNIQUE, mais le générateur PHP ne retentait pas explicitement une collision avant l'INSERT.

**Fichier :** `WebPixel/rp-character-action.php`.

**Correction :** génération serveur allowlistée avec recherche de collision + retries ; contraintes UNIQUE SQL conservées comme autorité finale.

**Retest VPS :** création identité sur un compte de test sans identité.

### 7. Chat native adapter lifecycle

**Problème :** le `MutationObserver` utilisé pour retrouver le champ React/Nitro était attaché au root Nitro sans cleanup explicite et pouvait recevoir beaucoup de callbacks.

**Fichier :** `WebPixel/nitro-last/paradise-chat-native-adapter.js`.

**Correction :**
- observer conservé pour ne pas casser le bridge chat ;
- scan throttlé à une frame uniquement si un input est réellement perdu ;
- observer stocké puis `disconnect()` au teardown ;
- AbortControllers nettoyés ;
- diagnostics callbacks/scans disponibles.

**Retest VPS :** Enter/Escape + plusieurs changements de rooms + inspection `window.__ParadiseNativeChatAdapter.diag`.

### 8. UI Actions / Notifications

**Problème :** le menu Actions dupliquait `Véhicules`, alors que ce domaine n'est pas encore réellement implémenté ; la cloche HUD était un bouton sans action.

**Fichier :** `WebPixel/nitro-last/paradise-quality-gate-ui.js`.

**Correction :**
- `Véhicules` retiré uniquement du menu Actions ;
- `Véhicules` reste dans la navigation bas-gauche pour respecter la structure HUD verrouillée ;
- cloche HUD routée vers l'application Notifications ParadisePhone existante.

**Limite connue :** la fenêtre `Véhicules` bas-gauche reste un placeholder. Aucun système véhicule n'est créé pendant Phase 4.5.

---

## Rapport PASS / FAIL

| Domaine | Audit statique après corrections | Gate runtime | État actuel |
|---|---|---|---|
| HUD | PASS — structure verrouillée conservée, données Store, room live partagée | 50 rooms + pointer + reconnect requis | **FAIL — RETEST VPS** |
| CHAT | PASS — Enter/Escape/focus bridge, lifecycle observer corrigé | messages/commandes/rooms requis ; feedback système global encore partiel | **FAIL — feedback global + RETEST** |
| PROFILE | PASS — vraies données Store/Character, bio SQL/sanitization | modification/reconnexion/1366 requis | **FAIL — RETEST VPS** |
| IDENTITY | PASS — validation serveur, Citizen ID non dérivé du user id, collision retry | création compte B/reconnexion requise | **FAIL — RETEST VPS** |
| DOCUMENTS | PASS — même repository/document server, same-room Present | test 2 joueurs showid/license requis | **FAIL — RETEST VPS** |
| INVENTORY | PASS code — autorité serveur, transactions, poids/slots, double-give guard | migration UTF-8 + use/give/reconnect requis | **FAIL — RETEST VPS** |
| PARADISEPHONE | PASS visuel/statique — DA verrouillée conservée | 30 open/close + apps + 50 rooms requis | **FAIL — RETEST VPS** |
| CONTACTS | PASS code — owner phone, unique contact, persistence SQL | ajout/suppression avec 2 comptes requis | **FAIL — RETEST VPS** |
| MESSAGES | PASS code — sender serveur, owner-scoped, offline SQL, duplicate guard | online/offline/read/reconnect requis | **FAIL — RETEST VPS** |
| CALLS | PASS code — online/busy/self/active-call validations | 2 comptes : answer/decline/hangup/disconnect requis | **FAIL — RETEST VPS** |
| NOTIFICATIONS | PASS code après correction — read persistant + préférence effective | badge/read/OFF-ON requis | **FAIL — RETEST VPS** |
| COMMANDS | PASS statique — UI liste les commandes Paradise réellement branchées | exécution de chaque commande requise | **FAIL — RETEST VPS** |
| RESPONSIVE | CSS possède des contraintes responsive | 1920×1080, 2560×1440, surtout 1366×768 requis | **FAIL — NON TESTÉ** |
| NITRO ROOMS | Room Store/HUD/Profile cohérents sur captures fournies | 50/50 exigé | **FAIL — NON TESTÉ 50/50** |
| SQL | PASS statique — contraintes/indexes/transactions audités, petite réparation ciblée | import migration + requêtes de vérification requis | **FAIL — RETEST VPS** |
| EMU | PASS statique — domaines Character/Documents/Inventory séparés ; repository transactions | rebuild + démarrage + logs requis | **FAIL — REBUILD/RETEST** |
| SECURITY | PASS statique sur ownership, session, fake sender, qty, double SMS/give | tests adversariaux runtime requis | **FAIL — RETEST VPS** |
| PERFORMANCE | PASS statique partiel — listeners principaux cleanup ; chat observer throttlé | session longue CPU/mémoire + 50 rooms requis | **FAIL — RETEST VPS** |

### Conclusion actuelle

**PARADISERP CORE V1 — PAS ENCORE VALIDÉ STABLE.**

La base statique est nettement plus cohérente après corrections, mais le Quality Gate ne doit passer à PASS qu'après les tests VPS obligatoires.

---

## Déploiement du Quality Gate sur le VPS

```powershell
cd C:\HVIP
git fetch origin
git switch quality/paradise-core-v1-fixes
git pull origin quality/paradise-core-v1-fixes
```

### Migration UTF-8 unique

```powershell
& "C:\xampp\mysql\bin\mysql.exe" -u root -p
```

Puis :

```sql
USE hv_rp;
SOURCE C:/HVIP/migrations/2026_08_21_quality_gate_utf8_repair.sql;
```

Ne pas réimporter les anciennes migrations.

### Rebuild ÉMU requis

Les guards Inventory/Documents ont changé côté C#.

```powershell
Get-CimInstance Win32_Process |
  Where-Object { $_.Name -eq "Plus Emulator.exe" } |
  ForEach-Object { Stop-Process -Id $_.ProcessId -Force }

cd "C:\HVIP\RDP EMU header change"
$msbuild = "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
& $msbuild ".\Plus Emulator.sln" /t:Rebuild /p:Configuration=Debug /p:Platform=x86 /m
```

Attendu : `0 Error(s)`.

Puis redémarrer l'ÉMU depuis `bin\Debug\Plus Emulator.exe`, recharger le client avec `Ctrl+F5` et reconnecter les comptes.

---

## Retest obligatoire

### Test 1 — smoke UI

- HUD sans fenêtre.
- Profile Aperçu / Identité / Documents / Statistiques.
- Inventory : `Téléphone` doit être correctement encodé.
- ParadisePhone Home / Contacts / Messages / Conversation / Notifications / Settings.
- Actions : pas de raccourci `Véhicules` dans le menu.
- Cloche HUD → Notifications ParadisePhone.
- Escape selon la règle globale.

Diagnostics :

```javascript
window.__PARADISE_BASELINE__
ParadiseStore.getState()
ParadiseRoomAdapter.getStatus()
ParadiseRoomUiSync.getStatus()
ParadiseQualityGateInput.getStatus()
ParadiseQualityGatePhone.getStatus()
ParadiseQualityGateUi.getStatus()
window.__ParadiseNativeChatAdapter.diag
```

### Test 2 — deux joueurs

Compte A + compte B :

- `:showid` ;
- give item ;
- double give ;
- ajout/suppression contact ;
- SMS A→B online ;
- SMS B offline puis reconnexion ;
- double SMS ;
- appel entrant ;
- décrocher ;
- refuser ;
- raccrocher ;
- déconnexion en appel ;
- Notifications OFF puis action génératrice ;
- Notifications ON puis action génératrice.

### Test 3 — 50 rooms

Pendant 50 changements :

- envoyer du chat ;
- ouvrir/fermer Profile ;
- ouvrir/fermer Phone ;
- ouvrir Inventory/Documents ;
- cliquer le sol/furnis/portes ;
- vérifier HUD et Profile : même room ;
- vérifier marche sans zone morte.

Critères obligatoires :

- 50/50 rooms ;
- 0 écran noir ;
- 0 `DataView RangeError` ;
- 0 canvas perdu ;
- 0 WebSocket close anormal ;
- 1 seul `#paradise-ui-root` ;
- 1 seul `#paradise-rp-hud` ;
- 0 reload nécessaire.

### Test 4 — responsive

Tester au minimum :

- 1366×768 ;
- 1920×1080 ;
- 2560×1440.

Aucune fenêtre ne doit sortir de l'écran et les zones libres doivent rester click-through vers Nitro.

---

## STOP

Aucune Phase 5 ne doit être commencée après ce rapport.
Le Quality Gate reste ouvert jusqu'aux retests VPS et au rapport final PASS/FAIL mis à jour avec les résultats réels.
