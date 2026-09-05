-- ParadiseRP - correction ciblee de la poubelle visible avec l'ID d'instance 15562.
-- L'ID affiche par Nitro est l'ID de la ligne `items`, pas l'ID de `items_base`.
-- Cette migration remonte donc automatiquement vers le vrai base item puis lui assigne
-- l'interaction RP deja enregistree par WaveRP-Plugin : nahabbo_rp_trashbin.

UPDATE items_base ib
INNER JOIN items i ON i.item_id = ib.id
SET ib.interaction_type = 'nahabbo_rp_trashbin'
WHERE i.id = 15562;

-- Garde aussi la detection globale pour les autres poubelles dont les noms DB sont explicites.
UPDATE items_base
SET interaction_type = 'nahabbo_rp_trashbin'
WHERE
    LOWER(COALESCE(item_name, '')) REGEXP '(^|[_ -])(trash|trashcan|garbage|garbagecan|dumpster|dustbin|rubbish|waste|litter|recycle|recycling|bin)([_ -]|[0-9]|$)'
    OR LOWER(COALESCE(public_name, '')) REGEXP '(^|[ -])(poubelle|poubelles|corbeille|corbeilles|ordure|ordures|dechet|dechets|trash|garbage|dumpster|dustbin|rubbish|waste|recycle|recycling|bin)([ -]|$)'
    OR LOWER(COALESCE(item_name, '')) REGEXP '(^bin[0-9]+$|^bin_|_bin$|_bin_|trash_can|garbage_can|waste_bin|recycle_bin|recycling_bin)';

-- Verification pratique : cette requete doit afficher nahabbo_rp_trashbin pour l'instance 15562.
SELECT i.id AS instance_id,
       ib.id AS base_item_id,
       ib.item_name,
       ib.public_name,
       ib.interaction_type
FROM items i
INNER JOIN items_base ib ON ib.id = i.item_id
WHERE i.id = 15562;
