-- WaveRP - adaptation francaise du metier medical existant.
-- L'identifiant technique `hospital` et tous les IDs sont conserves.

UPDATE `jobs`
SET `display_name` = 'EMS - Services médicaux',
    `description` = 'Services médicaux et urgences de ParadiseRP',
    `active` = TRUE
WHERE `name` = 'hospital';

UPDATE `job_ranks`
SET `display_name` = CASE `name`
  WHEN 'hospital_nurse' THEN 'Infirmier'
  WHEN 'hospital_supervisor' THEN 'Médecin urgentiste'
  WHEN 'hospital_asst_manager' THEN 'Ambulancier superviseur'
  WHEN 'hospital_manager' THEN 'Directeur des services médicaux'
  ELSE `display_name`
END,
`active` = TRUE
WHERE `job_id` = (SELECT `id` FROM `jobs` WHERE `name` = 'hospital' LIMIT 1);
