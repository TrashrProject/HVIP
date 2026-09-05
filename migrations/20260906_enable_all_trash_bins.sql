-- ParadiseRP: rendre les poubelles existantes fouillables.
-- Migration rejouable : elle ne cree aucune table.
-- La detection couvre les noms internes Habbo (item_name) et les noms publics (public_name).
-- interaction_modes_count est force a au moins 2 : WavePlus utilise cette valeur pour annoncer
-- au client Nitro qu'un mobilier est utilisable. Sans cela, un mobi statique reste selectionnable
-- mais le client n'envoie pas l'action d'utilisation au serveur.

UPDATE items_base
SET interaction_type = 'nahabbo_rp_trashbin',
    interaction_modes_count = GREATEST(COALESCE(interaction_modes_count, 0), 2)
WHERE
    LOWER(COALESCE(item_name, '')) REGEXP '(^|[_ -])(trash|trashcan|garbage|garbagecan|dumpster|dustbin|rubbish|waste|litter|recycle|recycling|bin)([_ -]|[0-9]|$)'
    OR LOWER(COALESCE(public_name, '')) REGEXP '(^|[ -])(poubelle|poubelles|corbeille|corbeilles|ordure|ordures|dechet|dechets|trash|garbage|dumpster|dustbin|rubbish|waste|recycle|recycling|bin)([ -]|$)';

-- Cas compacts frequents dans les classnames Habbo : bin1, city_bin, waste_bin, etc.
UPDATE items_base
SET interaction_type = 'nahabbo_rp_trashbin',
    interaction_modes_count = GREATEST(COALESCE(interaction_modes_count, 0), 2)
WHERE
    LOWER(COALESCE(item_name, '')) REGEXP '(^bin[0-9]+$|^bin_|_bin$|_bin_|trash_can|garbage_can|waste_bin|recycle_bin|recycling_bin)';
