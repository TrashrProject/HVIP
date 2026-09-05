-- ParadiseRP - normalisation des rangs staff.
-- Objectifs :
-- 1) Owner / Founder / Fondateur = un seul rang : Fondateur (rang 7).
-- 2) Uniformiser les intitulés staff en français sans changer les IDs des autres groupes.
-- 3) Conserver les permissions existantes avant de rattacher les anciens fondateurs au rang 7.

-- Le rang 7 est la référence Fondateur déjà utilisée par ParadiseRP.
INSERT INTO `permissions_groups` (`id`, `level`, `name`, `description`, `badge_code`)
VALUES (7, 7, 'Fondateur', 'Accès complet du fondateur.', 'Owner_hpvp')
ON DUPLICATE KEY UPDATE
    `level` = 7,
    `name` = 'Fondateur',
    `description` = 'Accès complet du fondateur.',
    `badge_code` = 'Owner_hpvp';

-- Récupère sur le rang 7 les permissions de tous les anciens groupes
-- portant un intitulé Owner / Founder / Fondateur.
INSERT INTO `permissions_rights` (`group_id`, `permission_id`)
SELECT 7, pr.`permission_id`
FROM `permissions_rights` pr
INNER JOIN `permissions_groups` pg ON pg.`id` = pr.`group_id`
WHERE pg.`id` <> 7
  AND (
      LOWER(pg.`name`) LIKE '%owner%'
      OR LOWER(pg.`name`) LIKE '%founder%'
      OR LOWER(pg.`name`) LIKE '%fondateur%'
  )
  AND NOT EXISTS (
      SELECT 1
      FROM `permissions_rights` existing
      WHERE existing.`group_id` = 7
        AND existing.`permission_id` = pr.`permission_id`
  );

-- Tous les comptes qui étaient Owner/Founder/Fondateur utilisent maintenant
-- réellement le même rang en base : rank = 7.
UPDATE `users` u
INNER JOIN `permissions_groups` pg ON pg.`id` = u.`rank`
SET u.`rank` = 7
WHERE pg.`id` <> 7
  AND (
      LOWER(pg.`name`) LIKE '%owner%'
      OR LOWER(pg.`name`) LIKE '%founder%'
      OR LOWER(pg.`name`) LIKE '%fondateur%'
  );

-- Uniformisation des autres intitulés staff affichés par le CMS.
-- Les IDs et niveaux sont conservés afin de ne pas casser les permissions WavePlus.
UPDATE `permissions_groups`
SET `name` = 'Développeur'
WHERE LOWER(`name`) LIKE '%developer%'
   OR LOWER(`name`) LIKE '%developpeur%'
   OR LOWER(`name`) LIKE '%développeur%';

UPDATE `permissions_groups`
SET `name` = 'Administrateur'
WHERE LOWER(`name`) LIKE '%administrator%'
   OR LOWER(`name`) LIKE '%administrateur%'
   OR LOWER(`name`) LIKE '%manager%';

UPDATE `permissions_groups`
SET `name` = 'Modérateur'
WHERE LOWER(`name`) LIKE '%moderator%'
   OR LOWER(`name`) LIKE '%moderateur%'
   OR LOWER(`name`) LIKE '%modérateur%';

-- Les éventuels anciens groupes Owner/Founder restent présents pour compatibilité,
-- mais leur libellé est également normalisé et aucun utilisateur ne doit plus y être rattaché.
UPDATE `permissions_groups`
SET `name` = 'Fondateur'
WHERE `id` <> 7
  AND (
      LOWER(`name`) LIKE '%owner%'
      OR LOWER(`name`) LIKE '%founder%'
      OR LOWER(`name`) LIKE '%fondateur%'
  );
