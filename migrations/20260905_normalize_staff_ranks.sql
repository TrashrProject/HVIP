-- ParadiseRP - normalisation des rangs staff CMS / WavePlus
-- Owner et Fondateur representent le meme poste : Fondateur.
-- Migration rejouable : elle conserve les groupes historiques mais bascule les comptes Owner (rang 6)
-- vers le rang Fondateur (rang 7) apres avoir recopie les permissions manquantes.

START TRANSACTION;

-- Garantit que le rang 7 existe et porte le nom officiel ParadiseRP.
INSERT INTO `permissions_groups` (`id`, `level`, `name`, `description`, `badge_code`)
VALUES (7, 7, 'Fondateur', 'Acces complet du fondateur.', 'Owner_hpvp')
ON DUPLICATE KEY UPDATE
    `level` = 7,
    `name` = 'Fondateur',
    `description` = 'Acces complet du fondateur.',
    `badge_code` = 'Owner_hpvp';

-- Le rang historique 6 (Owner) et le rang 7 disposent des memes droits avant la fusion des comptes.
INSERT INTO `permissions_rights` (`group_id`, `permission_id`)
SELECT 7, pr.`permission_id`
FROM `permissions_rights` pr
WHERE pr.`group_id` = 6
  AND NOT EXISTS (
      SELECT 1
      FROM `permissions_rights` existing_right
      WHERE existing_right.`group_id` = 7
        AND existing_right.`permission_id` = pr.`permission_id`
  );

-- Owner et Fondateur sont un seul et meme poste : tous les comptes Owner deviennent rang 7.
UPDATE `users`
SET `rank` = 7
WHERE `rank` = 6;

-- On retire les libelles anglais/anciens de la base sans supprimer les groupes historiques,
-- afin de ne casser aucune ancienne reference de permission.
UPDATE `permissions_groups`
SET `name` = 'Fondateur'
WHERE `id` IN (6, 7)
   OR LOWER(TRIM(`name`)) IN ('owner', 'founder', 'fondateur');

UPDATE `permissions_groups`
SET `name` = 'Developpeur'
WHERE LOWER(TRIM(`name`)) IN ('developer', 'developper', 'developpeur', 'développeur');

UPDATE `permissions_groups`
SET `name` = 'Administrateur'
WHERE LOWER(TRIM(`name`)) IN ('administrator', 'administrateur', 'admin');

UPDATE `permissions_groups`
SET `name` = 'Moderateur'
WHERE LOWER(TRIM(`name`)) IN ('moderator', 'moderateur', 'modérateur');

COMMIT;
