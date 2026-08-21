-- ParadiseRP Phase 2 — Character Profile V2 / Identity / Documents
-- Additive migration for MariaDB 10.4+. No destructive changes to users/play_stats.

CREATE TABLE IF NOT EXISTS `rp_characters` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT NOT NULL,
  `citizen_id` VARCHAR(16) NOT NULL,
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

CREATE TABLE IF NOT EXISTS `rp_document_types` (
  `id` SMALLINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `code` VARCHAR(40) NOT NULL,
  `name` VARCHAR(80) NOT NULL,
  `category` VARCHAR(32) NOT NULL,
  `expires` TINYINT(1) NOT NULL DEFAULT 0,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_rp_document_types_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- System definitions only; no player/test data.
INSERT IGNORE INTO `rp_document_types` (`code`, `name`, `category`, `expires`) VALUES
  ('PLACID_ID', 'Carte d’identité de Placid Island', 'identity', 0),
  ('DRIVER_LICENSE', 'Permis de conduire', 'license', 1);

CREATE TABLE IF NOT EXISTS `rp_player_documents` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT NOT NULL,
  `document_type_id` SMALLINT UNSIGNED NOT NULL,
  `document_number` VARCHAR(24) NOT NULL,
  `issued_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `expires_at` DATETIME NULL DEFAULT NULL,
  `status` VARCHAR(16) NOT NULL DEFAULT 'VALID',
  `metadata` LONGTEXT NULL DEFAULT NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_rp_player_document_number` (`document_number`),
  UNIQUE KEY `uq_rp_player_document_type` (`user_id`, `document_type_id`),
  KEY `idx_rp_player_documents_user` (`user_id`),
  KEY `idx_rp_player_documents_status` (`status`),
  CONSTRAINT `fk_rp_player_documents_type`
    FOREIGN KEY (`document_type_id`) REFERENCES `rp_document_types` (`id`)
    ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `rp_document_shares` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `sender_user_id` INT NOT NULL,
  `target_user_id` INT NOT NULL,
  `player_document_id` INT UNSIGNED NOT NULL,
  `status` VARCHAR(16) NOT NULL DEFAULT 'PENDING',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `expires_at` DATETIME NOT NULL,
  `viewed_at` DATETIME NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_rp_document_shares_target` (`target_user_id`, `status`, `expires_at`),
  KEY `idx_rp_document_shares_sender` (`sender_user_id`, `created_at`),
  CONSTRAINT `fk_rp_document_shares_document`
    FOREIGN KEY (`player_document_id`) REFERENCES `rp_player_documents` (`id`)
    ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Small one-shot queue used by EMU chat commands to ask the Paradise overlay to
-- open a specific profile tab. The existing read-only HUD bridge consumes it.
CREATE TABLE IF NOT EXISTS `rp_ui_events` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT NOT NULL,
  `event_type` VARCHAR(40) NOT NULL,
  `payload` VARCHAR(500) NULL DEFAULT NULL,
  `status` VARCHAR(16) NOT NULL DEFAULT 'PENDING',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `expires_at` DATETIME NOT NULL,
  `consumed_at` DATETIME NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_rp_ui_events_user` (`user_id`, `status`, `expires_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
