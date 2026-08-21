-- ParadiseRP Phase 4 — ParadisePhone V1
-- Additive MariaDB 10.4 migration. No production player/test seed.

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

CREATE TABLE IF NOT EXISTS `rp_phone_messages` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `sender_phone_id` BIGINT UNSIGNED NOT NULL,
  `receiver_phone_id` BIGINT UNSIGNED NOT NULL,
  `body` VARCHAR(500) NOT NULL,
  `status` VARCHAR(16) NOT NULL DEFAULT 'SENT',
  `sent_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `read_at` TIMESTAMP NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_rp_phone_messages_sender` (`sender_phone_id`,`sent_at`),
  KEY `idx_rp_phone_messages_receiver` (`receiver_phone_id`,`sent_at`),
  KEY `idx_rp_phone_messages_unread` (`receiver_phone_id`,`read_at`,`sent_at`),
  CONSTRAINT `fk_rp_phone_messages_sender` FOREIGN KEY (`sender_phone_id`) REFERENCES `rp_phones` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_rp_phone_messages_receiver` FOREIGN KEY (`receiver_phone_id`) REFERENCES `rp_phones` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

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

-- Optional audit/rate-limit trail. No message body duplication beyond the authoritative message table.
CREATE TABLE IF NOT EXISTS `rp_phone_action_log` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `phone_id` BIGINT UNSIGNED NOT NULL,
  `action_type` VARCHAR(32) NOT NULL,
  `target_phone_id` BIGINT UNSIGNED NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_rp_phone_action_rate` (`phone_id`,`action_type`,`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
