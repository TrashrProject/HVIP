-- Aligne le rang 7 de la base active sur le rang Fondateur du CMS.

INSERT INTO `permissions_groups` (`id`, `level`, `name`, `description`, `badge_code`)
VALUES (7, 7, 'Fondateur', 'Acces complet du fondateur.', 'Owner_hpvp')
ON DUPLICATE KEY UPDATE
    `level` = VALUES(`level`),
    `name` = VALUES(`name`),
    `description` = VALUES(`description`),
    `badge_code` = VALUES(`badge_code`);

-- Le chargeur de l'emulateur attribue les droits par groupe exact.
INSERT INTO `permissions_rights` (`group_id`, `permission_id`)
SELECT 7, source_rights.`permission_id`
FROM `permissions_rights` AS source_rights
WHERE source_rights.`group_id` = 6
  AND NOT EXISTS (
      SELECT 1
      FROM `permissions_rights` AS founder_rights
      WHERE founder_rights.`group_id` = 7
        AND founder_rights.`permission_id` = source_rights.`permission_id`
  );

