-- ParadiseRP - Correction des permissions Zy'Croque / Tasty Crousty
-- Rend les commandes restaurant cohérentes avec le moteur Java et :commands.

-- Employé : carte, prise de commande, service, addition et encaissement.
UPDATE job_ranks jr
JOIN jobs j ON j.id = jr.job_id
SET jr.permissions = JSON_ARRAY(
  'restaurant.menu',
  'restaurant.order',
  'restaurant.serve',
  'restaurant.bill',
  'restaurant.cash'
)
WHERE j.name IN ('zycroque','tastycrousty')
  AND jr.level = 1;

-- Cuisinier : mêmes droits + préparation.
UPDATE job_ranks jr
JOIN jobs j ON j.id = jr.job_id
SET jr.permissions = JSON_ARRAY(
  'restaurant.menu',
  'restaurant.order',
  'restaurant.prepare',
  'restaurant.serve',
  'restaurant.bill',
  'restaurant.cash'
)
WHERE j.name IN ('zycroque','tastycrousty')
  AND jr.level = 2;

-- Chef de cuisine : accès cuisine complet.
UPDATE job_ranks jr
JOIN jobs j ON j.id = jr.job_id
SET jr.permissions = JSON_ARRAY(
  'restaurant.menu',
  'restaurant.order',
  'restaurant.prepare',
  'restaurant.serve',
  'restaurant.bill',
  'restaurant.cash',
  'restaurant.kitchen'
)
WHERE j.name IN ('zycroque','tastycrousty')
  AND jr.level = 3;

-- Manager : cuisine + gestion du personnel.
UPDATE job_ranks jr
JOIN jobs j ON j.id = jr.job_id
SET jr.permissions = JSON_ARRAY(
  'restaurant.menu',
  'restaurant.order',
  'restaurant.prepare',
  'restaurant.serve',
  'restaurant.bill',
  'restaurant.cash',
  'restaurant.kitchen',
  'job.hire',
  'job.fire',
  'job.promote',
  'job.demote'
)
WHERE j.name IN ('zycroque','tastycrousty')
  AND jr.level = 4;

-- Directeur : tous les droits restaurant + planning.
UPDATE job_ranks jr
JOIN jobs j ON j.id = jr.job_id
SET jr.permissions = JSON_ARRAY(
  'restaurant.menu',
  'restaurant.order',
  'restaurant.prepare',
  'restaurant.serve',
  'restaurant.bill',
  'restaurant.cash',
  'restaurant.kitchen',
  'job.hire',
  'job.fire',
  'job.promote',
  'job.demote',
  'job.schedule'
)
WHERE j.name IN ('zycroque','tastycrousty')
  AND jr.level >= 5;

SELECT j.name AS job, jr.level, jr.display_name, jr.permissions
FROM job_ranks jr
JOIN jobs j ON j.id = jr.job_id
WHERE j.name IN ('zycroque','tastycrousty')
ORDER BY j.id, jr.level;
