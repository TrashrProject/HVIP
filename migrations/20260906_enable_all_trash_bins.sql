-- ParadiseRP: rendre les poubelles existantes fouillables.
-- Migration rejouable : elle ne cree aucune table et ne modifie que interaction_type.
-- La detection couvre les noms internes Habbo (item_name) et les noms publics (public_name).

UPDATE items_base
SET interaction_type = 'nahabbo_rp_trashbin'
WHERE
    LOWER(COALESCE(item_name, '')) REGEXP '(^|[_ -])(trash|trashcan|garbage|garbagecan|dumpster|dustbin|rubbish|waste|litter|recycle|recycling|bin)([_ -]|[0-9]|$)'
    OR LOWER(COALESCE(public_name, '')) REGEXP '(^|[ -])(poubelle|poubelles|corbeille|corbeilles|ordure|ordures|dechet|dechets|trash|garbage|dumpster|dustbin|rubbish|waste|recycle|recycling|bin)([ -]|$)';

-- Cas compacts frequents dans les classnames Habbo : bin1, city_bin, waste_bin, etc.
UPDATE items_base
SET interaction_type = 'nahabbo_rp_trashbin'
WHERE
    LOWER(COALESCE(item_name, '')) REGEXP '(^bin[0-9]+$|^bin_|_bin$|_bin_|trash_can|garbage_can|waste_bin|recycle_bin|recycling_bin)';
