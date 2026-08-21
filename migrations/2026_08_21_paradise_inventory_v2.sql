-- ParadiseRP Phase 3 — Inventory V2
-- Additive production migration. No player/test seed is included here.
-- MariaDB 10.4 compatible.

CREATE TABLE IF NOT EXISTS `rp_item_definitions` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `code` VARCHAR(64) NOT NULL,
  `name` VARCHAR(96) NOT NULL,
  `description` VARCHAR(255) NOT NULL DEFAULT '',
  `category` VARCHAR(32) NOT NULL DEFAULT 'OBJECT',
  `weight` DECIMAL(8,3) NOT NULL DEFAULT 0.000,
  `max_stack` INT UNSIGNED NOT NULL DEFAULT 1,
  `icon` VARCHAR(255) NULL,
  `usable` TINYINT(1) NOT NULL DEFAULT 0,
  `tradeable` TINYINT(1) NOT NULL DEFAULT 1,
  `droppable` TINYINT(1) NOT NULL DEFAULT 0,
  `effect_type` VARCHAR(32) NOT NULL DEFAULT 'NONE',
  `effect_value` INT NOT NULL DEFAULT 0,
  `metadata_schema` LONGTEXT NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_rp_item_definitions_code` (`code`),
  KEY `idx_rp_item_definitions_category` (`category`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `rp_inventory_profiles` (
  `user_id` INT NOT NULL,
  `base_capacity` DECIMAL(8,3) NOT NULL DEFAULT 50.000,
  `capacity_bonus` DECIMAL(8,3) NOT NULL DEFAULT 0.000,
  `max_slots` INT UNSIGNED NOT NULL DEFAULT 30,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `rp_inventory_items` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `owner_user_id` INT NOT NULL,
  `item_definition_id` INT UNSIGNED NOT NULL,
  `quantity` INT UNSIGNED NOT NULL DEFAULT 1,
  `metadata` LONGTEXT NULL,
  `slot` INT UNSIGNED NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_rp_inventory_owner` (`owner_user_id`),
  KEY `idx_rp_inventory_definition` (`item_definition_id`),
  KEY `idx_rp_inventory_owner_definition` (`owner_user_id`,`item_definition_id`),
  CONSTRAINT `fk_rp_inventory_definition` FOREIGN KEY (`item_definition_id`) REFERENCES `rp_item_definitions` (`id`) ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `rp_inventory_transactions` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `transaction_type` VARCHAR(24) NOT NULL,
  `actor_user_id` INT NULL,
  `target_user_id` INT NULL,
  `inventory_item_id` BIGINT UNSIGNED NULL,
  `item_definition_id` INT UNSIGNED NULL,
  `quantity` INT UNSIGNED NOT NULL DEFAULT 1,
  `metadata` LONGTEXT NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_rp_inventory_tx_actor` (`actor_user_id`,`created_at`),
  KEY `idx_rp_inventory_tx_target` (`target_user_id`,`created_at`),
  KEY `idx_rp_inventory_tx_type` (`transaction_type`,`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Generic container foundation. Nothing is mapped to a live furni by this migration.
CREATE TABLE IF NOT EXISTS `rp_containers` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `container_type` VARCHAR(32) NOT NULL DEFAULT 'CHEST',
  `owner_type` VARCHAR(24) NOT NULL DEFAULT 'PLAYER',
  `owner_id` INT NOT NULL,
  `capacity` DECIMAL(8,3) NOT NULL DEFAULT 100.000,
  `max_slots` INT UNSIGNED NOT NULL DEFAULT 40,
  `metadata` LONGTEXT NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_rp_containers_owner` (`owner_type`,`owner_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `rp_container_items` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `container_id` BIGINT UNSIGNED NOT NULL,
  `item_definition_id` INT UNSIGNED NOT NULL,
  `quantity` INT UNSIGNED NOT NULL DEFAULT 1,
  `metadata` LONGTEXT NULL,
  `slot` INT UNSIGNED NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_rp_container_items_container` (`container_id`),
  CONSTRAINT `fk_rp_container_items_container` FOREIGN KEY (`container_id`) REFERENCES `rp_containers` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_rp_container_items_definition` FOREIGN KEY (`item_definition_id`) REFERENCES `rp_item_definitions` (`id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Furni interaction mapping foundation. Mapping is server-side and opt-in.
CREATE TABLE IF NOT EXISTS `rp_furni_interactions` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `base_item_id` INT UNSIGNED NOT NULL,
  `interaction_type` VARCHAR(32) NOT NULL,
  `configuration` LONGTEXT NULL,
  `enabled` TINYINT(1) NOT NULL DEFAULT 1,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_rp_furni_interaction_base` (`base_item_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Initial real definitions. These are catalogue/domain definitions, not player seed data.
-- Icon stays NULL until a verified local Habbo sprite path is mapped: UI uses a local fallback, never a 404 URL.
INSERT INTO `rp_item_definitions`
(`code`,`name`,`description`,`category`,`weight`,`max_stack`,`icon`,`usable`,`tradeable`,`droppable`,`effect_type`,`effect_value`)
VALUES
('WATER_BOTTLE','Bouteille d’eau','Une bouteille d’eau fraîche de Placid Island.','FOOD',0.500,10,NULL,1,1,0,'DRINK',0),
('SANDWICH','Sandwich','Un sandwich simple à emporter.','FOOD',0.350,10,NULL,1,1,0,'EAT',15),
('PHONE_BASIC','Téléphone','Votre téléphone personnel ParadiseRP.','OBJECT',0.220,1,NULL,1,0,0,'PHONE',0),
('GENERIC_KEY','Clé','Une clé physique liée à un accès RP.','KEY',0.080,1,NULL,0,1,0,'KEY',0),
('SCRAP_METAL','Ferraille','Petite ressource métier récupérable et transportable.','MISC',2.500,20,NULL,0,1,0,'RESOURCE',0)
ON DUPLICATE KEY UPDATE
  `name`=VALUES(`name`),
  `description`=VALUES(`description`),
  `category`=VALUES(`category`),
  `weight`=VALUES(`weight`),
  `max_stack`=VALUES(`max_stack`),
  `usable`=VALUES(`usable`),
  `tradeable`=VALUES(`tradeable`),
  `droppable`=VALUES(`droppable`),
  `effect_type`=VALUES(`effect_type`),
  `effect_value`=VALUES(`effect_value`);
