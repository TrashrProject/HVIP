-- ParadiseRP complete inventory, consumables and equipment.
-- Idempotent: safe to run on every deployment.

CREATE TABLE IF NOT EXISTS `rp_items` (
  `id` INT NOT NULL,
  `name` VARCHAR(255) NOT NULL,
  `interaction_type` VARCHAR(50) NOT NULL,
  `permission` VARCHAR(255) NULL,
  `enable_id` INT NOT NULL DEFAULT 0,
  `extra_data` VARCHAR(255) NOT NULL DEFAULT '',
  `max` INT NOT NULL DEFAULT 1,
  `price` INT NOT NULL DEFAULT 0,
  `required_job_id` INT NULL,
  `offer_job_id` INT NULL,
  `required_handitem` INT NULL DEFAULT 0,
  `crafter_organizations` VARCHAR(255) NOT NULL DEFAULT '',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

ALTER TABLE `rp_items` ADD COLUMN IF NOT EXISTS `required_job_id` INT NULL;
ALTER TABLE `rp_items` ADD COLUMN IF NOT EXISTS `offer_job_id` INT NULL;
ALTER TABLE `rp_items` ADD COLUMN IF NOT EXISTS `required_handitem` INT NULL DEFAULT 0;
ALTER TABLE `rp_items` ADD COLUMN IF NOT EXISTS `crafter_organizations` VARCHAR(255) NOT NULL DEFAULT '';

CREATE TABLE IF NOT EXISTS `user_inventory` (
  `user_id` INT NOT NULL,
  `slot_index` INT NOT NULL,
  `item_id` INT NOT NULL,
  `quantity` INT NOT NULL DEFAULT 1,
  `durability` INT NOT NULL DEFAULT 100,
  `is_deposit_box` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`user_id`, `slot_index`, `is_deposit_box`),
  KEY `idx_paradise_inventory_item` (`item_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `rp_items`
(`id`,`name`,`interaction_type`,`permission`,`enable_id`,`extra_data`,`max`,`price`,`required_job_id`,`offer_job_id`,`required_handitem`,`crafter_organizations`) VALUES
(1, 'Medkit', 'heal', NULL, 0, '100', 5, 75, NULL, NULL, 0, ''),
(2, 'Shield', 'shield', NULL, 0, '25', 1, 100, NULL, NULL, 0, ''),
(3, 'Snack', 'energy', NULL, 0, '15', 10, 15, NULL, NULL, 0, ''),
(7, 'Weed', 'drug', NULL, -1, 'fastwalk_60', 20, 40, NULL, NULL, 0, 'cartel'),
(8, 'Cocaine', 'drug', NULL, -1, 'strength_100', 10, 90, NULL, NULL, 0, 'cartel'),
(9, 'Apple', 'food', NULL, 0, '3', 10, 5, NULL, NULL, 0, ''),
(10, 'Banana', 'food', NULL, 0, '4', 10, 5, NULL, NULL, 0, ''),
(11, 'Sandwich', 'food', NULL, 0, '5', 10, 10, NULL, NULL, 0, ''),
(12, 'Burger', 'food', NULL, 0, '7', 10, 15, NULL, NULL, 0, ''),
(13, 'Pizza Slice', 'food', NULL, 0, '8', 10, 15, NULL, NULL, 0, ''),
(14, 'Taco', 'food', NULL, 0, '9', 10, 18, NULL, NULL, 0, ''),
(15, 'Sushi Roll', 'food', NULL, 0, '10', 10, 20, NULL, NULL, 0, ''),
(16, 'Steak', 'food', NULL, 0, '12', 10, 25, NULL, NULL, 0, ''),
(18, 'Pasta', 'food', NULL, 0, '14', 10, 28, NULL, NULL, 0, ''),
(19, 'Lobster', 'food', NULL, 0, '16', 10, 35, NULL, NULL, 0, ''),
(20, 'Beef', 'food', NULL, 0, '18', 10, 40, NULL, NULL, 0, ''),
(21, 'Meal', 'food', NULL, 0, '20', 10, 45, NULL, NULL, 0, ''),
(1001, 'Bandage', 'heal', NULL, 0, '25', 10, 25, NULL, NULL, 0, ''),
(1002, 'Energy Drink', 'energy', NULL, 0, '30', 10, 30, NULL, NULL, 0, ''),
(1003, 'Armor Kit', 'shield', NULL, 0, '40', 1, 125, NULL, NULL, 0, ''),
(1004, 'Deluxe Medkit', 'heal', NULL, 0, '100', 5, 150, NULL, NULL, 0, '')
ON DUPLICATE KEY UPDATE
  `name`=VALUES(`name`),
  `interaction_type`=VALUES(`interaction_type`),
  `permission`=VALUES(`permission`),
  `enable_id`=VALUES(`enable_id`),
  `extra_data`=VALUES(`extra_data`),
  `max`=VALUES(`max`),
  `price`=VALUES(`price`),
  `required_handitem`=VALUES(`required_handitem`),
  `crafter_organizations`=VALUES(`crafter_organizations`);
