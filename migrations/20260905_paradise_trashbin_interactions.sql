-- ParadiseRP - rendre les mobiliers de type poubelle fouillables par WaveRP.
-- Rejouable : l'UPDATE peut etre execute plusieurs fois sans creer de doublon.
-- Aucun ID n'est devine : on travaille sur les vrais item_name presents dans items_base.

UPDATE `items_base`
SET `interaction_type` = 'nahabbo_rp_trashbin'
WHERE LOWER(`item_name`) LIKE '%trash%'
   OR LOWER(`item_name`) LIKE '%garbage%'
   OR LOWER(`item_name`) LIKE '%dumpster%'
   OR LOWER(`item_name`) LIKE '%poubelle%';
