-- ParadiseRP - appels vocaux / vidéo du téléphone
-- Signalisation WebRTC uniquement : l'audio et la vidéo passent directement entre navigateurs.

CREATE TABLE IF NOT EXISTS `phone_calls` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `caller_id` INT NOT NULL,
  `callee_id` INT NOT NULL,
  `call_type` ENUM('voice','video') NOT NULL DEFAULT 'voice',
  `status` ENUM('ringing','accepted','rejected','ended','missed') NOT NULL DEFAULT 'ringing',
  `created_at` INT NOT NULL,
  `answered_at` INT DEFAULT NULL,
  `ended_at` INT DEFAULT NULL,
  `updated_at` INT NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_phone_calls_caller_status` (`caller_id`,`status`),
  KEY `idx_phone_calls_callee_status` (`callee_id`,`status`),
  KEY `idx_phone_calls_updated` (`updated_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `phone_call_signals` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `call_id` BIGINT UNSIGNED NOT NULL,
  `sender_id` INT NOT NULL,
  `recipient_id` INT NOT NULL,
  `signal_type` ENUM('offer','answer','ice') NOT NULL,
  `payload` MEDIUMTEXT NOT NULL,
  `created_at` INT NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_phone_signals_recipient` (`recipient_id`,`call_id`,`id`),
  KEY `idx_phone_signals_call` (`call_id`,`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
