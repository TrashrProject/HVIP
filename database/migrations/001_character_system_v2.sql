-- ParadiseRP migration 001
-- Character System V2
-- Target: MariaDB 10.4+
-- Non-destructive: this migration does not move health, armor, money, jobs or room state.
-- Those values remain authoritative in the existing users/play_stats/group tables.

START TRANSACTION;

CREATE TABLE IF NOT EXISTS `rp_characters` (
  `user_id` INT(11) UNSIGNED NOT NULL,
  `citizen_number` VARCHAR(24) NOT NULL,
  `first_name` VARCHAR(40) DEFAULT NULL,
  `last_name` VARCHAR(40) DEFAULT NULL,
  `birth_date` DATE DEFAULT NULL,
  `gender` VARCHAR(24) DEFAULT NULL,
  `nationality` VARCHAR(64) DEFAULT NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`user_id`),
  UNIQUE KEY `uq_rp_characters_citizen_number` (`citizen_number`),
  KEY `idx_rp_characters_name` (`last_name`, `first_name`),
  CONSTRAINT `fk_rp_characters_user`
    FOREIGN KEY (`user_id`) REFERENCES `users` (`id`)
    ON UPDATE CASCADE
    ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Existing accounts receive a stable Paradise citizen number without duplicating
-- account or gameplay data. RP names remain NULL until the identity is completed.
INSERT INTO `rp_characters` (`user_id`, `citizen_number`)
SELECT
  `u`.`id`,
  CONCAT('PR-', LPAD(`u`.`id`, 5, '0'))
FROM `users` AS `u`
LEFT JOIN `rp_characters` AS `c` ON `c`.`user_id` = `u`.`id`
WHERE `c`.`user_id` IS NULL;

COMMIT;
