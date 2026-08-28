# Systeme EMS WaveRP

Le systeme EMS reutilise le metier `hospital`, les rangs et les permissions WaveRP. Il ne cree
pas de deuxieme systeme de metiers.

## Commandes joueurs

- `:ems [motif]` (`:medecin`, `:ambulance`) : ouvre un appel persistant depuis l'appartement courant.
- `:annulerems <numero>` : annule son propre appel encore actif.

## Commandes EMS en service

- `:appelsems` (`:emscalls`) : liste les appels ouverts et attribues.
- `:accepterems <numero>` : attribue un appel a l'ambulancier.
- `:fermerems <numero>` : cloture un appel attribue; un dispatcher peut cloturer tout appel.
- `:bandage <pseudo>` (`:panser`) : soigne partiellement un patient conscient.
- `:stabiliser <pseudo>` : retarde le transfert automatique d'un patient inconscient.
- `:reanimer <pseudo>` (`:revive`) : reanime un patient sur place.
- `:transporthopital <pseudo>` (`:evacuer`) : envoie le patient dans l'appartement hopital configure.
- `:heal <pseudo>` : soin complet existant, conserve.

Toutes les interventions exigent d'etre en service, dans le meme appartement et a portee du
patient. Les appels et soins sont journalises dans `rp_ems_calls` et `rp_ems_treatments`.

## Installation

1. Executer `migrations/20260824_waverp_ems_complete.sql` sur la base `waveplus`.
2. Configurer `nahabbo.features.hospital.roomid` avec l'identifiant de l'appartement hopital.
3. Compiler `WavePlus`, puis `WaveRP-Plugin`, et deployer le JAR du plugin dans le runtime.
4. Redemarrer l'emulateur et verifier que les commandes sont enregistrees dans `permissions`.


