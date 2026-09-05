-- ParadiseRP: corrige l'ancien déverrouillage global des skins d'armes.
-- Les skins non standards avaient été attribués automatiquement à tous les comptes.
-- On repart donc sur une base propre : seul le skin Standard est automatique,
-- et uniquement lorsque le joueur possède réellement l'arme dans user_inventory.

DELETE us
FROM paradise_user_weapon_skins us
INNER JOIN paradise_weapon_skins s ON s.id = us.skin_id
WHERE s.is_default = 0;

DELETE us
FROM paradise_user_weapon_skins us
INNER JOIN paradise_weapon_skins s ON s.id = us.skin_id
WHERE s.is_default = 1
  AND NOT EXISTS (
      SELECT 1
      FROM user_inventory ui
      INNER JOIN rp_items i ON i.id = ui.item_id
      WHERE ui.user_id = us.user_id
        AND ui.quantity > 0
        AND i.interaction_type = 'weapon'
        AND LOWER(i.name) = LOWER(s.weapon_key)
  );

INSERT IGNORE INTO paradise_user_weapon_skins (user_id, skin_id, equipped)
SELECT ui.user_id, s.id, 1
FROM user_inventory ui
INNER JOIN rp_items i ON i.id = ui.item_id
INNER JOIN paradise_weapon_skins s
    ON s.is_default = 1
   AND LOWER(s.weapon_key) = LOWER(i.name)
WHERE ui.quantity > 0
  AND i.interaction_type = 'weapon'
GROUP BY ui.user_id, s.id;
