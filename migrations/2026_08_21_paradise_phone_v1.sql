-- ParadiseRP Phase 4 — ParadisePhone V1
-- Additive MariaDB 10.4 migration. No production player/test seed.
-- Existing legacy PhoneChat persistence is reused instead of creating a second message store.

CREATE TABLE IF NOT EXISTS `rp_phones` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT NOT NULL,
  `phone_number` VARCHAR(16) NOT NULL,
  `device_identifier` VARCHAR(64) NOT NULL,
  `status` VARCHAR(16) NOT NULL DEFAULT 'ACTIVE',
  `silent_mode` TINYINT(1) NOT NULL DEFAULT 0,
  `notifications_enabled` TINYINT(1) NOT NULL DEFAULT 1,
  `sounds_enabled` TINYINT(1) NOT NULL DEFAULT 1,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_rp_phones_user` (`user_id`),
  UNIQUE KEY `uq_rp_phones_number` (`phone_number`),
  KEY `idx_rp_phones_status` (`status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `rp_phone_contacts` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `phone_id` BIGINT UNSIGNED NOT NULL,
  `contact_phone_number` VARCHAR(16) NOT NULL,
  `display_name` VARCHAR(64) NOT NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_rp_phone_contact` (`phone_id`,`contact_phone_number`),
  KEY `idx_rp_phone_contacts_owner` (`phone_id`,`display_name`),
  CONSTRAINT `fk_rp_phone_contacts_phone` FOREIGN KEY (`phone_id`) REFERENCES `rp_phones` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Reuse the emulator's existing PhoneChat table.
CREATE TABLE IF NOT EXISTS `play_phone_chats` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `type` INT NOT NULL DEFAULT 1,
  `emisor_id` INT NOT NULL,
  `emisor_name` VARCHAR(64) NOT NULL,
  `receptor_id` INT NOT NULL,
  `receptor_name` VARCHAR(64) NOT NULL,
  `msg` TEXT NOT NULL,
  `timestamp` DATETIME NOT NULL,
  `status` VARCHAR(16) NOT NULL DEFAULT 'SENT',
  `read_at` DATETIME NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_phone_sender` (`emisor_id`),
  KEY `idx_phone_receiver` (`receptor_id`),
  KEY `idx_phone_type` (`type`),
  KEY `idx_phone_unread` (`receptor_id`,`type`,`read_at`,`timestamp`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

ALTER TABLE `play_phone_chats`
  ADD COLUMN IF NOT EXISTS `status` VARCHAR(16) NOT NULL DEFAULT 'SENT' AFTER `timestamp`,
  ADD COLUMN IF NOT EXISTS `read_at` DATETIME NULL DEFAULT NULL AFTER `status`;

CREATE TABLE IF NOT EXISTS `rp_phone_calls` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `caller_phone_id` BIGINT UNSIGNED NOT NULL,
  `receiver_phone_id` BIGINT UNSIGNED NOT NULL,
  `status` VARCHAR(16) NOT NULL DEFAULT 'RINGING',
  `started_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `answered_at` TIMESTAMP NULL DEFAULT NULL,
  `ended_at` TIMESTAMP NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_rp_phone_calls_caller` (`caller_phone_id`,`started_at`),
  KEY `idx_rp_phone_calls_receiver` (`receiver_phone_id`,`started_at`),
  KEY `idx_rp_phone_calls_active` (`status`,`caller_phone_id`,`receiver_phone_id`),
  CONSTRAINT `fk_rp_phone_calls_caller` FOREIGN KEY (`caller_phone_id`) REFERENCES `rp_phones` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_rp_phone_calls_receiver` FOREIGN KEY (`receiver_phone_id`) REFERENCES `rp_phones` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `rp_phone_notifications` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `phone_id` BIGINT UNSIGNED NOT NULL,
  `notification_type` VARCHAR(32) NOT NULL,
  `title` VARCHAR(96) NOT NULL,
  `body` VARCHAR(255) NOT NULL DEFAULT '',
  `metadata` LONGTEXT NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `read_at` TIMESTAMP NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_rp_phone_notifications_unread` (`phone_id`,`read_at`,`created_at`),
  CONSTRAINT `fk_rp_phone_notifications_phone` FOREIGN KEY (`phone_id`) REFERENCES `rp_phones` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `rp_phone_action_log` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `phone_id` BIGINT UNSIGNED NOT NULL,
  `action_type` VARCHAR(32) NOT NULL,
  `target_phone_id` BIGINT UNSIGNED NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_rp_phone_action_rate` (`phone_id`,`action_type`,`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
