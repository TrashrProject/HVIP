# Catalogue massif ParadiseRP / HabboRPbr

Cette extension fusionne le catalogue Paradise existant avec les meubles HabboRPbr réellement compatibles avec WavePlus et Nitro.

## Contenu livré

- 8 883 meubles HabboRPbr supplémentaires dans 17 catégories ;
- 20 146 offres au total dans les pages Paradise lors du test de migration ;
- 19 864 identifiants de meubles distincts dans ces pages ;
- 20 448 classes de meubles uniques dans `FurnitureData.json` ;
- 2 395 meubles de construction détectés (blocs, couleurs, murs, sols, dalles, briques, etc.) ;
- 660 meubles orientés RP détectés (services, police, santé, commerces, transports, etc.).

Chaque meuble importé possède les quatre éléments obligatoires : une définition `furniture`, une entrée FurnitureData utilisant le même identifiant, un asset `.nitro` et une icône PNG.

## Déploiement

Le script `mise-a-jour-wave-vps.ps1` détecte automatiquement le schéma du catalogue :

- schéma récent `item_id/min_vip` : catalogues v3, v4 puis import massif ;
- schéma historique `item_ids/vip_only` : import massif legacy autonome puis page des blocs colorés.

Les migrations sont idempotentes et leurs gros INSERT sont fractionnés pour fonctionner avec la valeur MySQL standard de `max_allowed_packet`.

Depuis `C:\HVIP` :

```powershell
powershell -ExecutionPolicy Bypass -File .\mise-a-jour-wave-vps.ps1
```

Après le déploiement, vider le cache du navigateur avec `Ctrl+F5`. La version de l'URL FurnitureData a été changée pour forcer le rechargement du catalogue corrigé.

## Vérifications effectuées

- chargement JSON du FurnitureData ;
- correspondance de 8 883 IDs client/serveur ;
- présence des 8 883 assets et icônes ;
- application de toutes les migrations sur une copie de `waveplus` ;
- seconde application de la migration massive ;
- absence de références catalogue vers un meuble inexistant.
