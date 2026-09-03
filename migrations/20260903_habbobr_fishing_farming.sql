-- ParadiseRP - HabboRPbr fishing/farming/combat support.
-- Requires the active rp_items/user_inventory schema and 20260903_habbobr_combat_items.sql.
-- Idempotent and safe to re-run.

INSERT INTO `rp_items`
(`id`,`name`,`interaction_type`,`permission`,`enable_id`,`extra_data`,`max`,`price`,`required_job_id`,`offer_job_id`,`required_handitem`,`crafter_organizations`) VALUES
(6119, 'Thon',       'food', NULL, 0, '10', 20, 25, NULL, NULL, 0, ''),
(6120, 'Saumon',    'food', NULL, 0, '15', 20, 40, NULL, NULL, 0, ''),
(6121, 'Carotte',   'food', NULL, 0, '8',  20, 12, NULL, NULL, 0, ''),
(6122, 'Munitions', 'ammo', NULL, 0, '',   100,  5, NULL, NULL, 0, '')
ON DUPLICATE KEY UPDATE
  `name`=VALUES(`name`),
  `interaction_type`=VALUES(`interaction_type`),
  `permission`=VALUES(`permission`),
  `enable_id`=VALUES(`enable_id`),
  `extra_data`=VALUES(`extra_data`),
  `max`=VALUES(`max`),
  `price`=VALUES(`price`),
  `required_job_id`=VALUES(`required_job_id`),
  `offer_job_id`=VALUES(`offer_job_id`),
  `required_handitem`=VALUES(`required_handitem`),
  `crafter_organizations`=VALUES(`crafter_organizations`);

CREATE TABLE IF NOT EXISTS `paradise_crops` (
  `user_id` INT NOT NULL,
  `crop_key` VARCHAR(32) NOT NULL DEFAULT 'carrot',
  `planted_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ready_at` TIMESTAMP NOT NULL,
  PRIMARY KEY (`user_id`),
  KEY `idx_paradise_crops_ready` (`ready_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `paradise_weapon_ammo` (
  `user_id` INT NOT NULL,
  `item_id` INT NOT NULL,
  `current_ammo` INT UNSIGNED NOT NULL DEFAULT 0,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`user_id`, `item_id`),
  KEY `idx_paradise_weapon_ammo_item` (`item_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
