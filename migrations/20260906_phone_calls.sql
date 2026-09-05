-- ParadiseRP phone voice/video calls
-- WebRTC media stays peer-to-peer; this table only carries signaling/state.

CREATE TABLE IF NOT EXISTS `phone_calls` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `caller_id` INT NOT NULL,
  `callee_id` INT NOT NULL,
  `call_type` ENUM('audio','video') NOT NULL DEFAULT 'audio',
  `status` ENUM('ringing','accepted','declined','ended','missed') NOT NULL DEFAULT 'ringing',
  `offer_sdp` MEDIUMTEXT NULL,
  `answer_sdp` MEDIUMTEXT NULL,
  `created_at` INT UNSIGNED NOT NULL,
  `answered_at` INT UNSIGNED NULL,
  `ended_at` INT UNSIGNED NULL,
  `updated_at` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_phone_calls_callee_status` (`callee_id`,`status`,`id`),
  KEY `idx_phone_calls_caller_status` (`caller_id`,`status`,`id`),
  KEY `idx_phone_calls_created` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
