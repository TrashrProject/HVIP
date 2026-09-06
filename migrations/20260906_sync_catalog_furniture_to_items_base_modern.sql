-- ParadiseRP - synchronise les offres modernes absentes vers la table réellement chargée par WaveRP.
-- Ne supprime et ne masque aucun mobilier.
SET NAMES utf8mb4;
START TRANSACTION;

INSERT INTO items_base (
    id, sprite_id, public_name, item_name, type, width, length, stack_height,
    allow_stack, allow_sit, allow_lay, allow_walk, allow_gift, allow_trade,
    allow_recycle, allow_marketplace_sell, allow_inventory_stack,
    interaction_type, interaction_modes_count, vending_ids, multiheight,
    customparams, effect_id_male, effect_id_female, clothing_on_walk
)
SELECT DISTINCT
       f.id,
       f.id,
       LEFT(COALESCE(NULLIF(f.public_name,''), f.item_name), 56),
       LEFT(f.item_name, 70),
       f.type,
       COALESCE(f.width, 1),
       COALESCE(f.length, 1),
       COALESCE(f.stack_height, 0),
       COALESCE(f.can_stack, 1),
       COALESCE(f.can_sit, 0),
       COALESCE(f.allow_lay, 0),
       COALESCE(f.is_walkable, 0),
       COALESCE(f.allow_gift, 1),
       COALESCE(f.allow_trade, 1),
       COALESCE(f.allow_recycle, 0),
       COALESCE(f.allow_marketplace_sell, 0),
       COALESCE(f.allow_inventory_stack, 1),
       LEFT(COALESCE(NULLIF(f.interaction_type,''), 'default'), 500),
       COALESCE(f.interaction_modes_count, 1),
       LEFT(COALESCE(f.vending_ids, '0'), 255),
       LEFT(COALESCE(f.height_adjustable, '0'), 50),
       '',
       COALESCE(f.effect_id, 0),
       COALESCE(f.effect_id, 0),
       IF(COALESCE(f.clothing_id,0) > 0, CAST(f.clothing_id AS CHAR), '')
FROM catalog_items ci
JOIN furniture f ON f.id = ci.item_id
LEFT JOIN items_base ib ON ib.id = f.id
WHERE f.type IN ('s','i')
  AND ib.id IS NULL;

COMMIT;
