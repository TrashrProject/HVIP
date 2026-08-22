-- ParadiseRP — Paradise Control Center V3
-- Additive / non-destructive migration for MariaDB 10.4+.
-- The Control Center deliberately refuses sensitive writes until this table exists.

CREATE TABLE IF NOT EXISTS `cms_admin_audit_log` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `staff_id` INT NOT NULL,
  `staff_username` VARCHAR(64) NOT NULL,
  `action` VARCHAR(64) NOT NULL,
  `module` VARCHAR(40) NOT NULL,
  `target_type` VARCHAR(40) NOT NULL,
  `target_id` VARCHAR(96) NULL DEFAULT NULL,
  `before_data` LONGTEXT NULL DEFAULT NULL,
  `after_data` LONGTEXT NULL DEFAULT NULL,
  `reason` VARCHAR(500) NOT NULL,
  `ip_address` VARCHAR(45) NULL DEFAULT NULL,
  `created_at` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_pcc_audit_created` (`created_at`),
  KEY `idx_pcc_audit_staff` (`staff_id`,`created_at`),
  KEY `idx_pcc_audit_action` (`action`,`created_at`),
  KEY `idx_pcc_audit_module` (`module`,`created_at`),
  KEY `idx_pcc_audit_target` (`target_type`,`target_id`,`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
