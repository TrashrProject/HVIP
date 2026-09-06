-- ParadiseRP - salaires par métier et par grade.
-- La paie est versée toutes les 10 minutes par PaydayTimer.
-- Les métiers connus ont leur propre grille. Les métiers supplémentaires utilisent une grille
-- de secours basée sur le niveau afin que TOUS les métiers actifs aient un salaire différent
-- selon le grade sans nécessiter une nouvelle migration.

START TRANSACTION;

-- Sans emploi : aucun salaire.
UPDATE job_ranks jr
JOIN jobs j ON j.id = jr.job_id
SET jr.salary = 0.00
WHERE LOWER(j.name) = 'unemployed';

-- Police : métier à responsabilité élevée.
UPDATE job_ranks jr
JOIN jobs j ON j.id = jr.job_id
SET jr.salary = CASE jr.level
  WHEN 0 THEN 50.00
  WHEN 1 THEN 65.00
  WHEN 2 THEN 80.00
  WHEN 3 THEN 100.00
  WHEN 4 THEN 125.00
  ELSE 150.00 + GREATEST(jr.level - 5, 0) * 20.00
END
WHERE LOWER(j.name) = 'police';

-- EMS / Hôpital.
UPDATE job_ranks jr
JOIN jobs j ON j.id = jr.job_id
SET jr.salary = CASE jr.level
  WHEN 0 THEN 45.00
  WHEN 1 THEN 60.00
  WHEN 2 THEN 80.00
  WHEN 3 THEN 105.00
  ELSE 125.00 + GREATEST(jr.level - 4, 0) * 20.00
END
WHERE LOWER(j.name) IN ('hospital', 'ems');

-- Banque.
UPDATE job_ranks jr
JOIN jobs j ON j.id = jr.job_id
SET jr.salary = CASE jr.level
  WHEN 0 THEN 45.00
  WHEN 1 THEN 60.00
  WHEN 2 THEN 80.00
  WHEN 3 THEN 110.00
  ELSE 130.00 + GREATEST(jr.level - 4, 0) * 20.00
END
WHERE LOWER(j.name) IN ('bank', 'banque');

-- Gouvernement.
UPDATE job_ranks jr
JOIN jobs j ON j.id = jr.job_id
SET jr.salary = CASE jr.level
  WHEN 0 THEN 55.00
  WHEN 1 THEN 75.00
  WHEN 2 THEN 100.00
  WHEN 3 THEN 130.00
  ELSE 150.00 + GREATEST(jr.level - 4, 0) * 25.00
END
WHERE LOWER(j.name) IN ('government', 'gouvernement');

-- Taxi.
UPDATE job_ranks jr
JOIN jobs j ON j.id = jr.job_id
SET jr.salary = CASE jr.level
  WHEN 0 THEN 30.00
  WHEN 1 THEN 40.00
  WHEN 2 THEN 50.00
  WHEN 3 THEN 65.00
  ELSE 75.00 + GREATEST(jr.level - 4, 0) * 10.00
END
WHERE LOWER(j.name) = 'taxi';

-- Restauration / cafés.
UPDATE job_ranks jr
JOIN jobs j ON j.id = jr.job_id
SET jr.salary = CASE jr.level
  WHEN 0 THEN 28.00
  WHEN 1 THEN 38.00
  WHEN 2 THEN 50.00
  WHEN 3 THEN 65.00
  ELSE 75.00 + GREATEST(jr.level - 4, 0) * 10.00
END
WHERE LOWER(j.name) IN ('starbucks', 'restaurant', 'restaurant_zycroque', 'zycroque', 'tasty_crousty');

-- Armurerie.
UPDATE job_ranks jr
JOIN jobs j ON j.id = jr.job_id
SET jr.salary = CASE jr.level
  WHEN 0 THEN 35.00
  WHEN 1 THEN 48.00
  WHEN 2 THEN 65.00
  WHEN 3 THEN 85.00
  ELSE 100.00 + GREATEST(jr.level - 4, 0) * 15.00
END
WHERE LOWER(j.name) = 'armory';

-- Casino.
UPDATE job_ranks jr
JOIN jobs j ON j.id = jr.job_id
SET jr.salary = CASE jr.level
  WHEN 0 THEN 38.00
  WHEN 1 THEN 52.00
  WHEN 2 THEN 70.00
  WHEN 3 THEN 95.00
  ELSE 110.00 + GREATEST(jr.level - 4, 0) * 15.00
END
WHERE LOWER(j.name) = 'casino';

-- Autres métiers existants ou futurs : grille générique par niveau.
-- On ne touche pas au chômage et on conserve les grilles spécifiques ci-dessus.
UPDATE job_ranks jr
JOIN jobs j ON j.id = jr.job_id
SET jr.salary = 30.00 + (GREATEST(jr.level, 0) * 15.00)
WHERE j.active = TRUE
  AND LOWER(j.name) NOT IN (
    'unemployed', 'police', 'hospital', 'ems', 'bank', 'banque',
    'government', 'gouvernement', 'taxi', 'starbucks', 'restaurant',
    'restaurant_zycroque', 'zycroque', 'tasty_crousty', 'armory', 'casino'
  );

-- Force l'intervalle de paie à 10 minutes si la clé existe, sinon la crée.
INSERT INTO emulator_settings (`key`, `value`)
VALUES ('features.payday.timer_minutes', '10')
ON DUPLICATE KEY UPDATE `value` = VALUES(`value`);

COMMIT;

-- Vérification visuelle.
SELECT
  j.name AS job_name,
  j.display_name AS job_display,
  jr.level,
  jr.display_name AS rank_display,
  jr.salary
FROM job_ranks jr
JOIN jobs j ON j.id = jr.job_id
WHERE j.active = TRUE
ORDER BY j.name, jr.level;
