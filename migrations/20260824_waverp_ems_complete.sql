-- WaveRP - EMS complet
-- Base cible : waveplus

CREATE TABLE IF NOT EXISTS `rp_ems_calls` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `caller_user_id` INT NOT NULL,
  `caller_name` VARCHAR(64) NOT NULL,
  `room_id` INT NOT NULL,
  `room_name` VARCHAR(128) NOT NULL,
  `reason` VARCHAR(160) NOT NULL,
  `status` VARCHAR(16) NOT NULL DEFAULT 'OPEN',
  `assigned_medic_user_id` INT NULL,
  `assigned_medic_name` VARCHAR(64) NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `assigned_at` TIMESTAMP NULL DEFAULT NULL,
  `closed_at` TIMESTAMP NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_rp_ems_calls_status_created` (`status`, `created_at`),
  KEY `idx_rp_ems_calls_caller_status` (`caller_user_id`, `status`),
  KEY `idx_rp_ems_calls_medic_status` (`assigned_medic_user_id`, `status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `rp_ems_treatments` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `patient_user_id` INT NOT NULL,
  `medic_user_id` INT NOT NULL,
  `room_id` INT NOT NULL,
  `treatment_type` VARCHAR(24) NOT NULL,
  `health_before` INT NOT NULL,
  `health_after` INT NOT NULL,
  `notes` VARCHAR(255) NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_rp_ems_treatments_patient` (`patient_user_id`, `created_at`),
  KEY `idx_rp_ems_treatments_medic` (`medic_user_id`, `created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `jobs` (`name`, `display_name`, `description`, `active`)
VALUES ('hospital', 'Hospital', 'Service medical et urgences de ParadiseRP', TRUE)
ON DUPLICATE KEY UPDATE
  `description` = VALUES(`description`),
  `active` = TRUE;

SET @hospital_job_id := (SELECT `id` FROM `jobs` WHERE `name` = 'hospital' LIMIT 1);

INSERT INTO `job_ranks`
(`job_id`, `name`, `display_name`, `level`, `is_manager`, `salary`, `permissions`, `active`) VALUES
(@hospital_job_id, 'hospital_nurse', 'Infirmier', 0, FALSE, 40.00,
 '["medical.heal","medical.bandage","medical.stabilize"]', TRUE),
(@hospital_job_id, 'hospital_supervisor', 'Medecin urgentiste', 1, FALSE, 60.00,
 '["medical.heal","medical.bandage","medical.stabilize","medical.revive"]', TRUE),
(@hospital_job_id, 'hospital_asst_manager', 'Ambulancier superviseur', 2, TRUE, 80.00,
 '["medical.heal","medical.bandage","medical.stabilize","medical.revive","medical.ambulance","medical.dispatch","job.hire"]', TRUE),
(@hospital_job_id, 'hospital_manager', 'Directeur de l''hopital', 3, TRUE, 120.00,
 '["medical.heal","medical.bandage","medical.stabilize","medical.revive","medical.ambulance","medical.dispatch","medical.surgery","job.hire","job.fire","job.promote","job.demote","job.schedule"]', TRUE)
ON DUPLICATE KEY UPDATE
  `display_name` = VALUES(`display_name`),
  `level` = VALUES(`level`),
  `is_manager` = VALUES(`is_manager`),
  `salary` = VALUES(`salary`),
  `permissions` = VALUES(`permissions`),
  `active` = TRUE;

INSERT INTO `emulator_settings` (`key`, `value`) VALUES
('features.ems.call.cooldown.seconds', '30'),
('features.ems.calls.list.limit', '15'),
('features.ems.bandage.health', '20'),
('features.ems.stabilize.seconds', '90'),
('features.ems.revive.health', '35'),
('features.ems.treatment.range', '1'),
('features.ems.treatment.cooldown.seconds', '3')
ON DUPLICATE KEY UPDATE `value` = VALUES(`value`);

