# Catalogue ParadiseRP v3

## Resultat

Le catalogue `ParadiseRP - Construction` fournit une arborescence francaise consacree a la creation d'une ile RP : architecture, routes, transports, port, nature, logements, commerces, services publics, metiers, loisirs et outils Wired.

- 42 pages, dont une page racine servant de menu.
- 9 263 offres dans la base de validation du 3 septembre 2026.
- 282 meubles 2024 importes depuis les ressources HabboRPbr deja presentes localement.
- 8 699 meubles Paradise existants reutilises uniquement lorsqu'ils sont declares a la fois dans `FurnitureData.json`, `furniture` et `catalog_items`.
- Aucune reference de meuble manquante et aucun doublon par page lors des tests.
- La page `Wired et outils staff` exige le rang 7.

## Fichiers

- `migrations/20260903_paradise_catalogue_rp_v3.sql` : pages, meubles, prix et offres.
- `swf_pz/V5-0-2/gamedata/json/FurnitureData.json` : definitions client fusionnees.
- `swf_pz/V5-0-2/furniture/` : assets Nitro des collections 2024.
- `swf_pz/V5-0-2/dcr/hof_furni/icon/` : icones catalogue correspondantes.
- `tools/build-paradise-catalog-v3.ps1` : regeneration deterministe depuis le pack HabboRPbr local.

## Deploiement

La migration est incluse dans `mise-a-jour-wave-vps.ps1`. Le script effectue le `git pull`, recupere les objets Git LFS, applique la migration puis redemarre WaveRP. Comme `swf_pz` appartient au depot, les assets et le FurniData sont publies par la meme mise a jour.

Apres deploiement, vider le cache du navigateur avec `Ctrl+F5`. La version du FurniData dans `renderer-config.json` a ete changee pour invalider l'ancien cache.

## Validation effectuee

La migration a ete executee deux fois consecutivement sur une copie des trois tables de production (`catalog_pages`, `catalog_items`, `furniture`). Les deux executions ont reussi avec le meme total final, sans doublon et sans reference orpheline. Le JSON fusionne a ete parse et chaque nouvelle entree possede son fichier `.nitro` et son icone PNG.
