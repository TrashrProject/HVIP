CREATE TABLE IF NOT EXISTS `rp_clothing_categories` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `tab_name` varchar(80) NOT NULL,
  `room_id` int(11) NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  KEY `idx_rp_clothing_categories_room_id` (`room_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `rp_clothing_sets` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `tab_id` smallint(6) DEFAULT NULL,
  `set_type` varchar(32) NOT NULL,
  `part_types` varchar(32) NOT NULL,
  `color` varchar(32) NOT NULL,
  `name` varchar(80) NOT NULL,
  `stock` int(11) NOT NULL DEFAULT 0,
  `base_price` int(11) NOT NULL DEFAULT 0,
  `discount_price` int(11) NOT NULL DEFAULT 0,
  `visible` tinyint(1) NOT NULL DEFAULT 1,
  PRIMARY KEY (`id`),
  KEY `idx_rp_clothing_sets_tab_id` (`tab_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `user_xmas15_calendar` (
  `user_id` int(11) NOT NULL,
  `status` int(11) NOT NULL DEFAULT 0,
  `day` int(11) NOT NULL DEFAULT 0,
  KEY `idx_user_xmas15_calendar_user_id` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

UPDATE `website_settings`
SET `value` = '0'
WHERE `key` IN ('maintenance_enabled', 'requires_beta_code', 'disable_registration', 'disable_register');

UPDATE `website_settings`
SET `value` = 'https://paradiserp.fr'
WHERE `key` = 'avatar_imager';

UPDATE `rdp_config`
SET `site_name` = 'ParadiseRP',
    `site_url` = 'paradiserp.fr',
    `site_ssl` = 'https',
    `beta_mode` = 'off';