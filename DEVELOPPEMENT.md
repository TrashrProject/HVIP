# Guide de developpement WaveRP

## Branche de travail

- Branche active : `quality/paradise-core-v1-fixes`
- Ne pas developper directement sur `main`.

Avant chaque modification :

```powershell
git switch quality/paradise-core-v1-fixes
git pull
```

## Quel pack est utilise ?

Le serveur utilise actuellement :

- CMS : `WebPixel`
- Client actif : `WebPixel/nitro`
- Emulateur : WaveRP base Arcturus, sources dans `WavePlus/src/main/java`
- Plugin RP : sources dans `_waverp_arcturus_stage_20260824/waverp_plugin/roleplay-plugin-main/packages/plugin`
- Base de donnees : `waveplus`
- Runtime deploye : `runtime/WavePlus`

Les dossiers `backups`, les archives ZIP/RAR et les bases de sauvegarde ne sont pas des versions de production a modifier. Le chemin `_waverp_arcturus_stage_20260824` contient actuellement les sources actives du plugin WaveRP malgre son nom technique.

## Commandes en jeu

### Commande RP du plugin

Les commandes de metiers, Police, Taxi, banque et RP sont dans :

```text
_waverp_arcturus_stage_20260824/
  waverp_plugin/roleplay-plugin-main/packages/plugin/src/main/java/
    io/github/brenoepics/roleplay/commands/
```

Repertoires principaux :

- `commands/generic` : Taxi et commandes generales.
- `commands/jobs` : travail, recrutement, promotion et licenciement.
- `commands/jobs/police` : Taser, prison, liberation et Police.
- `commands/escort` : escorte et fin d'escorte.
- `commands/staff` : commandes reservees au staff, dont `:superhire`.
- `commands/banking` : banque.
- `commands/combat` : combat.

Chaque nouvelle commande doit aussi etre enregistree dans :

```text
.../roleplay/utilities/LoadPlayerCommands.java
```

Exemple : creer `MaCommande.java`, l'importer dans `LoadPlayerCommands.java`, puis appeler `addCommand(...)` avec son nom, ses alias et sa permission.

Les permissions de metier existantes sont dans :

```text
.../roleplay/features/job/JobPermissions.java
```

Ne pas creer un deuxieme systeme de metiers. Utiliser `RpAvatar`, `JobService`, `JobsManager`, les ranks et permissions existants.

### Commande native Arcturus

Les commandes internes de l'emulateur sont dans :

```text
WavePlus/src/main/java/com/eu/habbo/habbohotel/commands
```

Fichiers importants :

- `Command.java` : classe de base et refus de permission.
- `CommandHandler.java` : chargement et execution des commandes.
- `CommandsCommand.java` : commande `:commands`.
- `WavePlus/src/main/java/com/eu/habbo/messages/outgoing/generic/CommandsWindowComposer.java` : fenetre UI de `:commands`.

## UI du jeu Nitro

### UI active

Le client charge par `WebPixel/play.php` cette URL :

```text
/nitro/index.html
```

Le dossier actif est donc :

```text
WebPixel/nitro
```

Fichiers utiles :

- `index.html` : point d'entree et chargement des CSS/JS.
- `local-config.json` : connexion, URLs et configuration generale.
- `renderer-config.json` : ressources du renderer et mobilier.
- `ui-config.json` : configuration de l'interface.
- `profile-fix.css` : correctifs du profil/HUD actuellement charges.
- `assets/` : bundles JavaScript, CSS et images du client.

Pour ajouter une petite interface sans reconstruire Nitro :

1. Ajouter un fichier `paradise-nom-feature.js` dans `WebPixel/nitro`.
2. Ajouter un fichier `paradise-nom-feature.css` si necessaire.
3. Les charger explicitement dans `WebPixel/nitro/index.html`.
4. Ajouter un parametre de version `?v=...` pour forcer le rafraichissement du cache navigateur.
5. Tester l'interface sur ordinateur et mobile.

## CMS

Le CMS actif est :

```text
WebPixel
```

Organisation :

- Pages publiques : `WebPixel/*.php` (`me.php`, `staff.php`, `corporations.php`, etc.).
- Controleurs : `WebPixel/app/Controller`.
- Modeles/acces SQL : `WebPixel/app/Modal`.
- Vues : `WebPixel/app/View/Directory/Body`.
- Navigation : `WebPixel/app/View/Directory/Navigation/navbar.php`.
- CSS et images : `WebPixel/Dynamics`.
- Entree du jeu : `WebPixel/play.php`.

Ne pas modifier les dossiers `WebPixel_backup_*` ou `WebPixel_previous_*` pour une correction active.

## Base de donnees et SQL

La base utilisee par le CMS et l'emulateur est :

```text
waveplus
```

Toute modification de structure ou de configuration doit avoir un fichier versionne dans :

```text
migrations/
```

Regles :

- Utiliser une migration rejouable (`IF NOT EXISTS` ou `ON DUPLICATE KEY UPDATE`).
- Ne jamais pousser un dump complet de la base.
- Ne jamais pousser `config.ini`, mots de passe, tokens ou cles API.
- Ne pas recreer une table de metiers si `jobs`, `job_ranks` ou les tables RP existantes couvrent le besoin.

## Compilation

### Plugin WaveRP

Depuis le dossier du plugin :

```powershell
$env:JAVA_HOME = 'C:\Program Files\Android\openjdk\jdk-21.0.8'
cd _waverp_arcturus_stage_20260824\waverp_plugin\roleplay-plugin-main\packages\plugin
_tools\apache-maven-3.9.11\bin\mvn.cmd -DskipTests clean package
```

Artefact produit :

```text
target/Roleplay-1.0.45-jar-with-dependencies.jar
```

Deploiement serveur :

```text
runtime/WavePlus/plugins/WaveRP-Roleplay.jar
```

### Coeur Arcturus

Le projet Maven du coeur est `WavePlus/pom.xml`. Le JAR utilise en production est :

```text
runtime/WavePlus/WaveRP-Arcturus.jar
```

Apres une modification Java, toujours compiler avant de remplacer un JAR. Sauvegarder le JAR actif, deployer le nouveau, redemarrer, puis verifier les logs et les ports `30000`, `30001` et `2096`.

## Procedure pour demander une modification a ChatGPT/Codex

Commande en jeu :

```text
Travaille sur la branche quality/paradise-core-v1-fixes. Analyse le systeme existant dans le plugin WaveRP, ajoute la commande sans creer un second systeme, compile, puis commit et push.
```

UI du jeu :

```text
Modifie uniquement l'UI Wave active dans WebPixel/nitro. Verifie index.html, les fichiers charges et teste le resultat sur ordinateur et mobile.
```

CMS :

```text
Modifie uniquement le CMS actif WebPixel. Conserve ses configurations de connexion et ne touche pas au client Nitro Wave.
```

## Git et deploiement entre le PC et le serveur

Sur le PC, apres verification :

```powershell
git add <fichiers precis>
git commit -m "description claire"
git push
```

Sur le serveur, ne pas lancer automatiquement `git pull` si le depot contient des modifications locales. Examiner d'abord `git status`, sauvegarder les fichiers concernes, puis recuperer et deployer uniquement les changements valides.

Important : le depot GitHub contient actuellement les fichiers Java modifies, mais pas encore l'integralite du pack Wave Java importe. La compilation complete reste donc fiable dans l'environnement serveur. Avant de compiler tout le pack sur un autre PC, il faudra versionner proprement l'ensemble des sources Wave Java et leurs dependances, sans les runtimes ni les sauvegardes.
