-- ParadiseRP - garantir l'etat visuel ouvert/ferme des poubelles RP.
-- Rejouable : aucune table ni aucun ID de base item n'est invente.
-- Les poubelles deja marquees nahabbo_rp_trashbin doivent annoncer au moins deux etats
-- au client Nitro (0 = ferme, 1 = ouvert/utilise).

UPDATE items_base
SET interaction_modes_count = GREATEST(COALESCE(interaction_modes_count, 0), 2)
WHERE interaction_type = 'nahabbo_rp_trashbin';

-- Cas cible deja identifie dans ParadiseRP : 15562 est un ID d'instance `items`.
-- On remonte vers son vrai base item au lieu de deviner un ID de mobilier.
UPDATE items_base ib
INNER JOIN items i ON i.item_id = ib.id
SET ib.interaction_type = 'nahabbo_rp_trashbin',
    ib.interaction_modes_count = GREATEST(COALESCE(ib.interaction_modes_count, 0), 2)
WHERE i.id = 15562;

SELECT i.id AS instance_id,
       ib.id AS base_item_id,
       ib.item_name,
       ib.public_name,
       ib.interaction_type,
       ib.interaction_modes_count
FROM items i
INNER JOIN items_base ib ON ib.id = i.item_id
WHERE i.id = 15562;
