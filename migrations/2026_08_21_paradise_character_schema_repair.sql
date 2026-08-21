-- ParadiseRP Phase 2 — legacy rp_characters compatibility repair
-- MariaDB 10.4+
-- Non-destructive: preserves existing rows and the legacy citizen_number column.

-- If the table did not exist, create the current Phase 2 shape first.
CREATE TABLE IF NOT EXISTS `rp_characters` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT NOT NULL,
  `citizen_id` VARCHAR(32) NOT NULL,
  `first_name` VARCHAR(32) NOT NULL,
  `last_name` VARCHAR(32) NOT NULL,
  `birth_date` DATE NOT NULL,
  `gender` VARCHAR(24) NULL DEFAULT NULL,
  `nationality` VARCHAR(48) NOT NULL,
  `biography` VARCHAR(400) NULL DEFAULT NULL,
  `reputation` INT NOT NULL DEFAULT 0,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_rp_characters_user` (`user_id`),
  UNIQUE KEY `uq_rp_characters_citizen` (`citizen_id`),
  KEY `idx_rp_characters_name` (`last_name`, `first_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Legacy ParadiseRP installations may already have rp_characters with:
-- user_id, citizen_number, first_name, last_name, birth_date, gender,
-- nationality, created_at, updated_at.
-- Add only the Phase 2 fields that are missing.
ALTER TABLE `rp_characters`
  ADD COLUMN IF NOT EXISTS `id` INT UNSIGNED NULL FIRST,
  ADD COLUMN IF NOT EXISTS `citizen_id` VARCHAR(32) NULL AFTER `user_id`,
  ADD COLUMN IF NOT EXISTS `biography` VARCHAR(400) NULL DEFAULT NULL AFTER `nationality`,
  ADD COLUMN IF NOT EXISTS `reputation` INT NOT NULL DEFAULT 0 AFTER `biography`;

-- Preserve legacy identity numbers when citizen_number exists.
SET @pr_has_citizen_number := (
  SELECT COUNT(*)
  FROM `information_schema`.`COLUMNS`
  WHERE `TABLE_SCHEMA` = DATABASE()
    AND `TABLE_NAME` = 'rp_characters'
    AND `COLUMN_NAME` = 'citizen_number'
);
SET @pr_copy_citizen_sql := IF(
  @pr_has_citizen_number > 0,
  'UPDATE `rp_characters` SET `citizen_id` = CAST(`citizen_number` AS CHAR) WHERE (`citizen_id` IS NULL OR `citizen_id` = '''') AND `citizen_number` IS NOT NULL AND CAST(`citizen_number` AS CHAR) <> ''''',
  'SELECT 1'
);
PREPARE pr_stmt FROM @pr_copy_citizen_sql;
EXECUTE pr_stmt;
DEALLOCATE PREPARE pr_stmt;

-- Any row without a legacy citizen number receives a deterministic, unique
-- fallback based on its immutable user_id. Existing values are never replaced.
UPDATE `rp_characters`
SET `citizen_id` = CONCAT('PID-', LPAD(UPPER(HEX(`user_id`)), 8, '0'))
WHERE `citizen_id` IS NULL OR `citizen_id` = '';

-- Backfill the new surrogate id from user_id for all legacy rows.
UPDATE `rp_characters`
SET `id` = `user_id`
WHERE `id` IS NULL;

-- AUTO_INCREMENT requires an index whose first column is id. Add one only when
-- the legacy schema has no such index yet.
SET @pr_has_id_index := (
  SELECT COUNT(*)
  FROM `information_schema`.`STATISTICS`
  WHERE `TABLE_SCHEMA` = DATABASE()
    AND `TABLE_NAME` = 'rp_characters'
    AND `COLUMN_NAME` = 'id'
    AND `SEQ_IN_INDEX` = 1
);
SET @pr_add_id_index_sql := IF(
  @pr_has_id_index = 0,
  'ALTER TABLE `rp_characters` ADD INDEX `idx_rp_characters_id` (`id`)',
  'SELECT 1'
);
PREPARE pr_stmt FROM @pr_add_id_index_sql;
EXECUTE pr_stmt;
DEALLOCATE PREPARE pr_stmt;

-- New Phase 2 inserts omit id, so make it auto-generated after the backfill.
ALTER TABLE `rp_characters`
  MODIFY COLUMN `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  MODIFY COLUMN `citizen_id` VARCHAR(32) NOT NULL;

-- Ensure the lookup indexes expected by the Character System exist without
-- disturbing an existing legacy primary key on user_id.
SET @pr_has_user_index := (
  SELECT COUNT(*)
  FROM `information_schema`.`STATISTICS`
  WHERE `TABLE_SCHEMA` = DATABASE()
    AND `TABLE_NAME` = 'rp_characters'
    AND `COLUMN_NAME` = 'user_id'
    AND `SEQ_IN_INDEX` = 1
);
SET @pr_add_user_index_sql := IF(
  @pr_has_user_index = 0,
  'ALTER TABLE `rp_characters` ADD INDEX `idx_rp_characters_user` (`user_id`)',
  'SELECT 1'
);
PREPARE pr_stmt FROM @pr_add_user_index_sql;
EXECUTE pr_stmt;
DEALLOCATE PREPARE pr_stmt;

SET @pr_has_name_index := (
  SELECT COUNT(*)
  FROM `information_schema`.`STATISTICS`
  WHERE `TABLE_SCHEMA` = DATABASE()
    AND `TABLE_NAME` = 'rp_characters'
    AND `INDEX_NAME` = 'idx_rp_characters_name'
);
SET @pr_add_name_index_sql := IF(
  @pr_has_name_index = 0,
  'ALTER TABLE `rp_characters` ADD INDEX `idx_rp_characters_name` (`last_name`,`first_name`)',
  'SELECT 1'
);
PREPARE pr_stmt FROM @pr_add_name_index_sql;
EXECUTE pr_stmt;
DEALLOCATE PREPARE pr_stmt;

-- Verification output for VPS deployment.
SELECT `COLUMN_NAME`, `COLUMN_TYPE`, `IS_NULLABLE`, `COLUMN_KEY`, `EXTRA`
FROM `information_schema`.`COLUMNS`
WHERE `TABLE_SCHEMA` = DATABASE()
  AND `TABLE_NAME` = 'rp_characters'
ORDER BY `ORDINAL_POSITION`;
