-- Copiando estrutura para tabela habbo.jobs_looks
DROP TABLE IF EXISTS `jobs_looks`;
CREATE TABLE IF NOT EXISTS `jobs_looks` (
  `job_rank` varchar(50) COLLATE armscii8_bin NOT NULL,
  `look_m` varchar(50) COLLATE armscii8_bin NOT NULL,
  `look_f` varchar(50) COLLATE armscii8_bin NOT NULL,
  PRIMARY KEY (`job_rank`),
  UNIQUE KEY `job_rank` (`job_rank`)
) ENGINE=InnoDB DEFAULT CHARSET=armscii8 COLLATE=armscii8_bin;

-- Copiando dados para a tabela habbo.jobs_looks: ~1 rows (aproximadamente)
INSERT INTO `jobs_looks` (`job_rank`, `look_m`, `look_f`) VALUES
	('hospital_manager', 'hr-3251-39-49.hd-3100-5.lg-3078-110', 'he-3884-92-93.ch-4004-92.hd-3100-5.lg-3078-110');

-- Copiando estrutura para tabela habbo.jobs_rooms
DROP TABLE IF EXISTS `jobs_rooms`;
CREATE TABLE IF NOT EXISTS `jobs_rooms` (
  `job` enum('police','hospital','starbucks','armory','casino','bank') COLLATE armscii8_bin NOT NULL,
  `rooms` varchar(50) COLLATE armscii8_bin NOT NULL DEFAULT '-1' COMMENT 'multiples rooms use , e.g: 1,2,3 and for any room use -1',
  PRIMARY KEY (`job`),
  UNIQUE KEY `job` (`job`)
) ENGINE=InnoDB DEFAULT CHARSET=armscii8 COLLATE=armscii8_bin;

-- Copiando dados para a tabela habbo.jobs_rooms: ~2 rows (aproximadamente)
INSERT INTO `jobs_rooms` (`job`, `rooms`) VALUES
	('police', '-1'),
	('hospital', '10,14,206');

-- Copiando estrutura para tabela habbo.rp_items
DROP TABLE IF EXISTS `rp_items`;
CREATE TABLE IF NOT EXISTS `rp_items` (
  `id` int(11) NOT NULL,
  `name` varchar(255) COLLATE armscii8_bin NOT NULL,
  `interaction_type` varchar(50) COLLATE armscii8_bin NOT NULL,
  `permission` varchar(50) COLLATE armscii8_bin DEFAULT NULL,
  `enable_id` int(11) NOT NULL DEFAULT -1,
  `extra_data` varchar(255) COLLATE armscii8_bin NOT NULL,
  `max` int(11) NOT NULL,
  `price` int(11) NOT NULL DEFAULT 0,
  `offer_job` varchar(50) COLLATE armscii8_bin DEFAULT NULL,
  `required_job` varchar(50) COLLATE armscii8_bin DEFAULT NULL,
  `crafter_organizations` varchar(50) COLLATE armscii8_bin DEFAULT '',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=armscii8 COLLATE=armscii8_bin;

-- Copiando dados para a tabela habbo.rp_items: ~9 rows (aproximadamente)
INSERT INTO `rp_items` (`id`, `name`, `interaction_type`, `permission`, `enable_id`, `extra_data`, `max`, `price`, `required_job`, `crafter_organizations`) VALUES
	(0, 'tazor', 'weapon', NULL, 182, '', 0, 1, 'police', ''),
	(1, 'Medkit', 'heal', NULL, 0, '100', 5, 1, NULL, ''),
	(2, 'Shield', 'shield', NULL, 0, '25', 5, 1, NULL, ''),
	(3, 'Snack', 'energy', NULL, 0, '15', 5, 1, NULL, ''),
	(4, 'Pistol', 'weapon', 'acc_rp_pistol', 164, '', 1, 1, NULL, ''),
	(5, 'Bat', 'weapon', NULL, 510, '', 1, 1, NULL, ''),
	(6, 'Sword', 'weapon', NULL, 162, '', 1, 1, NULL, ''),
	(7, 'Weed', 'drug', NULL, -1, 'fastwalk_60', 20, 1, NULL, 'gang,mafia,cartel'),
	(8, 'Cocaine', 'drug', NULL, -1, 'strength_100', 0, 1, NULL, 'cartel');

-- Add food items to the rp_items table
INSERT INTO `rp_items` (`id`, `name`, `interaction_type`, `permission`, `enable_id`, `extra_data`, `max`, `price`, `offer_job`, `required_job`, `crafter_organizations`) VALUES
(9, 'Apple', 'food', NULL, 0, '3', 10, 2, NULL, NULL, ''),
(10, 'Banana', 'food', NULL, 0, '4', 10, 2, NULL, NULL, ''),
(11, 'Sandwich', 'food', NULL, 0, '5', 10, 3, NULL, NULL, ''),
(12, 'Burger', 'food', NULL, 0, '7', 8, 5, NULL, NULL, ''),
(13, 'Pizza Slice', 'food', NULL, 0, '8', 8, 5, NULL, NULL, ''),
(14, 'Taco', 'food', NULL, 0, '9', 8, 6, NULL, NULL, ''),
(15, 'Sushi Roll', 'food', NULL, 0, '10', 8, 6, NULL, NULL, ''),
(16, 'Steak', 'food', NULL, 0, '12', 5, 8, NULL, NULL, ''),
(18, 'Pasta', 'food', NULL, 0, '14', 5, 8, NULL, NULL, ''),
(19, 'Lobster', 'food', NULL, 0, '16', 3, 12, NULL, NULL, ''),
(20, 'Beef', 'food', NULL, 0, '18', 3, 12, NULL, NULL, ''),
(21, 'Meal', 'food', NULL, 0, '20', 3, 15, NULL, NULL, '');

-- Copiando estrutura para tabela habbo.rp_organizations
DROP TABLE IF EXISTS `rp_organizations`;
CREATE TABLE IF NOT EXISTS `rp_organizations` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(50) COLLATE armscii8_bin NOT NULL DEFAULT '0',
  `type` enum('gang','mafia','cartel') COLLATE armscii8_bin NOT NULL,
  `admin_id` int(11) NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=armscii8 COLLATE=armscii8_bin;

-- Copiando dados para a tabela habbo.rp_organizations: ~2 rows (aproximadamente)
INSERT INTO `rp_organizations` (`id`, `name`, `type`, `admin_id`) VALUES
	(6, 'testing cartel ', 'cartel', 1),
	(7, 'cartel 2 ', 'cartel', 2);

-- Copiando estrutura para tabela habbo.rp_organization_members
DROP TABLE IF EXISTS `rp_organization_members`;
CREATE TABLE IF NOT EXISTS `rp_organization_members` (
  `user_id` int(11) NOT NULL,
  `organization_id` int(11) NOT NULL,
  `rank` int(11) NOT NULL DEFAULT 1,
  PRIMARY KEY (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=armscii8 COLLATE=armscii8_bin;

-- Copiando dados para a tabela habbo.rp_organization_members: ~2 rows (aproximadamente)
INSERT INTO `rp_organization_members` (`user_id`, `organization_id`, `rank`) VALUES
	(1, 6, 3),
	(2, 7, 3);

-- Copiando estrutura para tabela habbo.rp_territories
DROP TABLE IF EXISTS `rp_territories`;
CREATE TABLE IF NOT EXISTS `rp_territories` (
  `room_id` int(11) NOT NULL,
  `organization_id` int(11) DEFAULT NULL,
  `type` enum('gang','mafia','cartel','any') COLLATE armscii8_bin DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=armscii8 COLLATE=armscii8_bin;

-- Copiando dados para a tabela habbo.rp_territories: ~0 rows (aproximadamente)
INSERT INTO `rp_territories` (`room_id`, `organization_id`, `type`) VALUES
	(206, 6, 'cartel');

-- Copiando estrutura para tabela habbo.users_roleplay
DROP TABLE IF EXISTS `users_roleplay`;
CREATE TABLE IF NOT EXISTS `users_roleplay` (
  `user_id` int(11) NOT NULL,
  `health` int(11) DEFAULT 100,
  `shield` int(11) DEFAULT 0,
  `energy` int(11) DEFAULT 100,
  `passive` int(11) DEFAULT 1,
  `job` varchar(255) COLLATE armscii8_bin DEFAULT NULL,
  `jobrank` varchar(255) COLLATE armscii8_bin DEFAULT NULL,
  `last_pos` varchar(255) COLLATE armscii8_bin DEFAULT "",
  `jailtime` int(11) DEFAULT 0,
  `equippedweapon` int(11) DEFAULT 0,
  PRIMARY KEY (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=armscii8 COLLATE=armscii8_bin;

-- Modify the users_roleplay_inventory table with a foreign key constraint
DROP TABLE IF EXISTS `users_roleplay_inventory`;
CREATE TABLE IF NOT EXISTS `users_roleplay_inventory` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` int(11) NOT NULL,
  `item_id` int(11) NOT NULL,
  `quantity` int(11) DEFAULT 1,
  PRIMARY KEY (`id`),
  UNIQUE KEY `user_id_item_id` (`user_id`,`item_id`),
  KEY `item_id` (`item_id`),
  CONSTRAINT `fk_inventory_item` FOREIGN KEY (`item_id`) REFERENCES `rp_items` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_inventory_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=armscii8 COLLATE=armscii8_bin;

-- Crimes table for storing crime information
CREATE TABLE IF NOT EXISTS `crimes` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(64) NOT NULL,
  `stars` INT NOT NULL,
  `police_alert` BOOLEAN NOT NULL DEFAULT FALSE,
  `instant_alert` BOOLEAN NOT NULL DEFAULT FALSE,
  `is_auto_charge` BOOLEAN NOT NULL DEFAULT FALSE,
  `notes` VARCHAR(255) NULL,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `name_UNIQUE` (`name` ASC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Penalties table for storing star level penalties
CREATE TABLE IF NOT EXISTS `crime_penalties` (
  `star_level` INT NOT NULL,
  `jail_time` INT NOT NULL COMMENT 'Jail time in minutes',
  `fine_amount` INT NOT NULL COMMENT 'Fine amount in currency',
  PRIMARY KEY (`star_level`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Criminal records to track user crimes
CREATE TABLE IF NOT EXISTS `user_criminal_records` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `user_id` INT NOT NULL,
  `crime_id` INT NOT NULL,
  `charged_by` INT NULL COMMENT 'Officer user ID who charged the criminal',
  `charged_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `served_time` BOOLEAN NOT NULL DEFAULT FALSE,
  `paid_fine` BOOLEAN NOT NULL DEFAULT FALSE,
  PRIMARY KEY (`id`),
  INDEX `fk_user_criminal_records_1_idx` (`user_id` ASC),
  INDEX `fk_user_criminal_records_2_idx` (`crime_id` ASC),
  INDEX `fk_user_criminal_records_3_idx` (`charged_by` ASC),
  CONSTRAINT `fk_user_criminal_records_1`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_user_criminal_records_2`
    FOREIGN KEY (`crime_id`)
    REFERENCES `crimes` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_user_criminal_records_3`
    FOREIGN KEY (`charged_by`)
    REFERENCES `users` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data for crimes table
INSERT INTO `crimes` (`name`, `stars`, `police_alert`, `instant_alert`, `is_auto_charge`, `notes`) VALUES
-- Auto-charge crimes
('Murder', 2, TRUE, TRUE, TRUE, 'Killing a user that is not in an organization'),
('Gang Homicide', 3, TRUE, TRUE, TRUE, 'If the killer and victim are both in an organization'),
('Execution', 3, TRUE, TRUE, TRUE, 'Killing a user that is in cuffs'),
('Invasion', 4, TRUE, TRUE, TRUE, 'Attempting or successfully robbing a till, safe, or heists'),
('Mass Murder', 4, TRUE, TRUE, TRUE, 'Killing someone while already wanted'),
('Cop Murder', 5, TRUE, TRUE, TRUE, 'Killing an on-duty police officer'),
('Logout', 4, TRUE, TRUE, TRUE, 'Logging out while being escorted by police'),
-- Manual charge crimes
('Assault', 1, FALSE, FALSE, FALSE, 'Hitting another user'),
('911 Abuse', 1, FALSE, FALSE, FALSE, 'Abusing or falsely using the 911 call system'),
('Obstruction', 2, FALSE, FALSE, FALSE, 'Hitting a user that is being escorted by police'),
('Cop Assault', 2, FALSE, FALSE, FALSE, 'Hitting an on-duty officer');

-- Data for penalties table
INSERT INTO `crime_penalties` (`star_level`, `jail_time`, `fine_amount`) VALUES
(1, 3, 5),
(2, 6, 10),
(3, 9, 15),
(4, 12, 20),
(5, 15, 50);

INSERT INTO `emulator_settings` (`key`, `value`) VALUES
("roleplay.livefeed.enabled", "1"),
("roleplay.livefeed.webhook.url", "");

INSERT INTO `emulator_settings` (`key`, `value`) VALUES
("features.hunger.delay.minutes", "10");

INSERT INTO `emulator_texts` (`key`, `value`) VALUES
("roleplay.livefeed.webhook.title", "Live Feed");

ALTER TABLE `users_roleplay`
	CHANGE COLUMN `jailtime` `jailtime` BIGINT NULL DEFAULT NULL AFTER `last_pos`;

ALTER TABLE `users_roleplay`
  ADD COLUMN `wanted_stars` INT DEFAULT 0,
  ADD COLUMN `wanted_timer_end` BIGINT DEFAULT 0;

ALTER TABLE `user_criminal_records`
	ADD COLUMN `ends_at` TIMESTAMP NULL DEFAULT NULL;

ALTER TABLE users_roleplay
ADD COLUMN kills INT NOT NULL DEFAULT 0,
ADD COLUMN deaths INT NOT NULL DEFAULT 0,
ADD COLUMN arrests INT NOT NULL DEFAULT 0,
ADD COLUMN kdratio FLOAT NOT NULL DEFAULT 0.0,
ADD COLUMN punches_thrown INT NOT NULL DEFAULT 0,
ADD COLUMN punches_received INT NOT NULL DEFAULT 0,
ADD COLUMN damage_dealt INT NOT NULL DEFAULT 0,
ADD COLUMN damage_received INT NOT NULL DEFAULT 0,
ADD COLUMN hunger INT NOT NULL DEFAULT 25,
ADD COLUMN aggressionUntil TIMESTAMP NULL;

-- Drop the old table
DROP TABLE IF EXISTS `users_roleplay_inventory`;

-- Create new inventory table with slot support
CREATE TABLE IF NOT EXISTS `user_inventory` (
  `user_id` int NOT NULL,
  `slot_index` int NOT NULL,
  `item_id` int NOT NULL,
  `quantity` int NOT NULL DEFAULT '1',
  `durability` int NOT NULL DEFAULT '100',
  `is_deposit_box` boolean NOT NULL DEFAULT FALSE,
  PRIMARY KEY (`user_id`, `slot_index`, `is_deposit_box`),
  FOREIGN KEY (`user_id`) REFERENCES `users`(`id`) ON DELETE CASCADE,
  FOREIGN KEY (`item_id`) REFERENCES `rp_items`(`id`) ON DELETE CASCADE,
  INDEX `idx_user_inventory` (`user_id`, `is_deposit_box`),
  INDEX `idx_item_lookup` (`item_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Better primary key that prevents duplicates
ALTER TABLE `user_inventory` DROP PRIMARY KEY;
ALTER TABLE `user_inventory` ADD PRIMARY KEY (`user_id`, `slot_index`, `is_deposit_box`);

-- Even better: add a unique constraint to prevent any slot conflicts
ALTER TABLE `user_inventory` ADD CONSTRAINT `unique_user_slot`
UNIQUE (`user_id`, `slot_index`, `is_deposit_box`);


-- Disable foreign key checks temporarily to avoid constraint issues
SET FOREIGN_KEY_CHECKS = 0;

-- Drop existing tables if they exist (in reverse dependency order)
DROP TABLE IF EXISTS `jobs_looks`;
DROP TABLE IF EXISTS `jobs_rooms`;
DROP TABLE IF EXISTS `job_ranks`;
DROP TABLE IF EXISTS `jobs`;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

-- Jobs table
CREATE TABLE IF NOT EXISTS `jobs` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(50) NOT NULL UNIQUE,
  `display_name` varchar(100) NOT NULL,
  `description` text,
  `active` boolean DEFAULT TRUE,
  `created_at` timestamp DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_jobs_name` (`name`),
  KEY `idx_jobs_active` (`active`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Job ranks table
CREATE TABLE IF NOT EXISTS `job_ranks` (
  `id` int NOT NULL AUTO_INCREMENT,
  `job_id` int NOT NULL,
  `name` varchar(50) NOT NULL UNIQUE,
  `display_name` varchar(100) NOT NULL,
  `level` int NOT NULL DEFAULT 0,
  `is_manager` boolean DEFAULT FALSE,
  `salary` decimal(10,2) DEFAULT 0.00,
  `permissions` json,
  `active` boolean DEFAULT TRUE,
  `created_at` timestamp DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_job_ranks_job_id` (`job_id`),
  KEY `idx_job_ranks_name` (`name`),
  KEY `idx_job_ranks_level` (`level`),
  KEY `idx_job_ranks_active` (`active`),
  FOREIGN KEY (`job_id`) REFERENCES `jobs` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Jobs looks table
CREATE TABLE IF NOT EXISTS `jobs_looks` (
  `id` int NOT NULL AUTO_INCREMENT,
  `job_rank_id` int NOT NULL,
  `look_m` varchar(255),
  `look_f` varchar(255),
  `created_at` timestamp DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_jobs_looks_rank_id` (`job_rank_id`),
  FOREIGN KEY (`job_rank_id`) REFERENCES `job_ranks` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Jobs rooms table
CREATE TABLE IF NOT EXISTS `jobs_rooms` (
  `id` int NOT NULL AUTO_INCREMENT,
  `job_id` int NOT NULL,
  `rooms` text NOT NULL,
  `created_at` timestamp DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_jobs_rooms_job_id` (`job_id`),
  FOREIGN KEY (`job_id`) REFERENCES `jobs` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Insert default jobs
INSERT IGNORE INTO `jobs` (`name`, `display_name`, `description`) VALUES
('unemployed', 'Unemployed', 'No current employment'),
('police', 'Police Department', 'Law enforcement officers'),
('hospital', 'Hospital', 'Medical professionals'),
('starbucks', 'Starbucks', 'Coffee shop employees'),
('armory', 'Armory', 'Weapon and equipment suppliers'),
('casino', 'Casino', 'Gaming and entertainment staff'),
('bank', 'Bank', 'Financial services professionals');

-- Insert job ranks with permissions
-- Unemployed
INSERT IGNORE INTO `job_ranks` (`job_id`, `name`, `display_name`, `level`, `is_manager`, `salary`, `permissions`) VALUES
((SELECT id FROM jobs WHERE name = 'unemployed'), 'unemployed', 'Unemployed', 0, FALSE, 0.00, '[]');

-- Police ranks
INSERT IGNORE INTO `job_ranks` (`job_id`, `name`, `display_name`, `level`, `is_manager`, `salary`, `permissions`) VALUES
((SELECT id FROM jobs WHERE name = 'police'), 'police_cadet', 'Police Cadet', 0, FALSE, 50.00, '["police.patrol"]'),
((SELECT id FROM jobs WHERE name = 'police'), 'police_officer', 'Police Officer', 1, FALSE, 75.00, '["police.patrol", "police.arrest", "police.search", "police.cuff", "police.taze"]'),
((SELECT id FROM jobs WHERE name = 'police'), 'police_sergeant', 'Police Sergeant', 2, FALSE, 100.00, '["police.patrol", "police.arrest", "police.search", "police.cuff", "police.taze", "police.investigate"]'),
((SELECT id FROM jobs WHERE name = 'police'), 'police_lieutenant', 'Police Lieutenant', 3, TRUE, 125.00, '["police.patrol", "police.arrest", "police.search", "police.cuff", "police.taze", "police.investigate", "police.wanted.access", "job.hire"]'),
((SELECT id FROM jobs WHERE name = 'police'), 'police_captain', 'Police Captain', 4, TRUE, 150.00, '["police.patrol", "police.arrest", "police.search", "police.cuff", "police.taze", "police.investigate", "police.wanted.access", "job.hire", "job.fire", "job.promote", "job.demote"]'),
((SELECT id FROM jobs WHERE name = 'police'), 'police_chief', 'Police Chief', 5, TRUE, 200.00, '["police.patrol", "police.arrest", "police.search", "police.cuff", "police.taze", "police.investigate", "police.wanted.access", "job.hire", "job.fire", "job.promote", "job.demote", "job.manage_schedule"]');

-- Hospital ranks
INSERT IGNORE INTO `job_ranks` (`job_id`, `name`, `display_name`, `level`, `is_manager`, `salary`, `permissions`) VALUES
((SELECT id FROM jobs WHERE name = 'hospital'), 'hospital_nurse', 'Nurse', 0, FALSE, 40.00, '["medical.heal", "medical.bandage"]'),
((SELECT id FROM jobs WHERE name = 'hospital'), 'hospital_supervisor', 'Medical Supervisor', 1, FALSE, 60.00, '["medical.heal", "medical.bandage", "medical.revive"]'),
((SELECT id FROM jobs WHERE name = 'hospital'), 'hospital_asst_manager', 'Assistant Manager', 2, TRUE, 80.00, '["medical.heal", "medical.bandage", "medical.revive", "medical.ambulance", "job.hire"]'),
((SELECT id FROM jobs WHERE name = 'hospital'), 'hospital_manager', 'Hospital Manager', 3, TRUE, 120.00, '["medical.heal", "medical.bandage", "medical.revive", "medical.ambulance", "medical.surgery", "job.hire", "job.fire", "job.promote", "job.demote", "job.manage_schedule"]');

-- Starbucks ranks
INSERT IGNORE INTO `job_ranks` (`job_id`, `name`, `display_name`, `level`, `is_manager`, `salary`, `permissions`) VALUES
((SELECT id FROM jobs WHERE name = 'starbucks'), 'starbucks_barista', 'Barista', 0, FALSE, 30.00, '["business.sell", "business.cash_register"]'),
((SELECT id FROM jobs WHERE name = 'starbucks'), 'starbucks_supervisor', 'Supervisor', 1, FALSE, 45.00, '["business.sell", "business.cash_register", "business.manage_inventory"]'),
((SELECT id FROM jobs WHERE name = 'starbucks'), 'starbucks_asst_manager', 'Assistant Manager', 2, TRUE, 60.00, '["business.sell", "business.cash_register", "business.manage_inventory", "business.open_close", "job.hire"]'),
((SELECT id FROM jobs WHERE name = 'starbucks'), 'starbucks_manager', 'Store Manager', 3, TRUE, 90.00, '["business.sell", "business.cash_register", "business.manage_inventory", "business.open_close", "job.hire", "job.fire", "job.promote", "job.demote", "job.manage_schedule"]');

-- Armory ranks
INSERT IGNORE INTO `job_ranks` (`job_id`, `name`, `display_name`, `level`, `is_manager`, `salary`, `permissions`) VALUES
((SELECT id FROM jobs WHERE name = 'armory'), 'armory_sales', 'Sales Associate', 0, FALSE, 35.00, '["business.sell", "business.cash_register"]'),
((SELECT id FROM jobs WHERE name = 'armory'), 'armory_supervisor', 'Supervisor', 1, FALSE, 50.00, '["business.sell", "business.cash_register", "business.manage_inventory"]'),
((SELECT id FROM jobs WHERE name = 'armory'), 'armory_asst_manager', 'Assistant Manager', 2, TRUE, 70.00, '["business.sell", "business.cash_register", "business.manage_inventory", "business.open_close", "job.hire"]'),
((SELECT id FROM jobs WHERE name = 'armory'), 'armory_manager', 'Store Manager', 3, TRUE, 100.00, '["business.sell", "business.cash_register", "business.manage_inventory", "business.open_close", "job.hire", "job.fire", "job.promote", "job.demote", "job.manage_schedule"]');

-- Casino ranks
INSERT IGNORE INTO `job_ranks` (`job_id`, `name`, `display_name`, `level`, `is_manager`, `salary`, `permissions`) VALUES
((SELECT id FROM jobs WHERE name = 'casino'), 'casino_dealer', 'Dealer', 0, FALSE, 40.00, '["casino.deal", "security.escort"]'),
((SELECT id FROM jobs WHERE name = 'casino'), 'casino_supervisor', 'Supervisor', 1, FALSE, 60.00, '["casino.deal", "security.escort", "security.remove"]'),
((SELECT id FROM jobs WHERE name = 'casino'), 'casino_asst_manager', 'Assistant Manager', 2, TRUE, 80.00, '["casino.deal", "security.escort", "security.remove", "security.ban", "job.hire"]'),
((SELECT id FROM jobs WHERE name = 'casino'), 'casino_manager', 'Casino Manager', 3, TRUE, 120.00, '["casino.deal", "security.escort", "security.remove", "security.ban", "job.hire", "job.fire", "job.promote", "job.demote", "job.manage_schedule"]');

-- Bank ranks
INSERT IGNORE INTO `job_ranks` (`job_id`, `name`, `display_name`, `level`, `is_manager`, `salary`, `permissions`) VALUES
((SELECT id FROM jobs WHERE name = 'bank'), 'bank_teller', 'Bank Teller', 0, FALSE, 45.00, '["bank.account_access", "bank.transfer"]'),
((SELECT id FROM jobs WHERE name = 'bank'), 'bank_supervisor', 'Supervisor', 1, FALSE, 65.00, '["bank.account_access", "bank.transfer", "bank.loan"]'),
((SELECT id FROM jobs WHERE name = 'bank'), 'bank_asst_manager', 'Assistant Manager', 2, TRUE, 85.00, '["bank.account_access", "bank.transfer", "bank.loan", "job.hire"]'),
((SELECT id FROM jobs WHERE name = 'bank'), 'bank_manager', 'Bank Manager', 3, TRUE, 130.00, '["bank.account_access", "bank.transfer", "bank.loan", "job.hire", "job.fire", "job.promote", "job.demote", "job.manage_schedule"]');

-- Update users_roleplay table to use job and rank IDs instead of names
ALTER TABLE `users_roleplay`
ADD COLUMN `job_id` INT NULL AFTER `passive`,
ADD COLUMN `job_rank_id` INT NULL AFTER `job_id`,
ADD KEY `idx_users_roleplay_job_id` (`job_id`),
ADD KEY `idx_users_roleplay_job_rank_id` (`job_rank_id`),
ADD FOREIGN KEY (`job_id`) REFERENCES `jobs` (`id`) ON DELETE SET NULL,
ADD FOREIGN KEY (`job_rank_id`) REFERENCES `job_ranks` (`id`) ON DELETE SET NULL;

ALTER TABLE `users_roleplay` DROP COLUMN `job`, DROP COLUMN `jobrank`;

-- Update rp_items table to support new job system
-- Add new job ID columns
ALTER TABLE `rp_items` 
ADD COLUMN `required_job_id` INT NULL AFTER `required_job`,
ADD COLUMN `offer_job_id` INT NULL AFTER `offer_job`,
ADD COLUMN `required_handitem` INT NULL DEFAULT 0 AFTER `offer_job_id`;

-- Add indexes for the new columns
ALTER TABLE `rp_items` 
ADD KEY `idx_rp_items_required_job_id` (`required_job_id`),
ADD KEY `idx_rp_items_offer_job_id` (`offer_job_id`);

-- Add foreign key constraints (will fail silently if they already exist)
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
     WHERE TABLE_NAME = 'rp_items' 
     AND CONSTRAINT_NAME = 'rp_items_ibfk_required_job') = 0,
    'ALTER TABLE rp_items ADD FOREIGN KEY (required_job_id) REFERENCES jobs (id) ON DELETE SET NULL',
    'SELECT "Foreign key for required_job_id already exists"'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
     WHERE TABLE_NAME = 'rp_items' 
     AND CONSTRAINT_NAME = 'rp_items_ibfk_offer_job') = 0,
    'ALTER TABLE rp_items ADD FOREIGN KEY (offer_job_id) REFERENCES jobs (id) ON DELETE SET NULL',
    'SELECT "Foreign key for offer_job_id already exists"'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Migration script to convert existing job names to IDs
-- This maps the old enum values to the new database IDs

-- Update required_job_id based on required_job names
UPDATE rp_items SET required_job_id = (
    SELECT id FROM jobs WHERE name = 'police'
) WHERE required_job = 'POLICE';

UPDATE rp_items SET required_job_id = (
    SELECT id FROM jobs WHERE name = 'hospital'
) WHERE required_job = 'HOSPITAL';

UPDATE rp_items SET required_job_id = (
    SELECT id FROM jobs WHERE name = 'starbucks'
) WHERE required_job = 'STARBUCKS';

UPDATE rp_items SET required_job_id = (
    SELECT id FROM jobs WHERE name = 'armory'
) WHERE required_job = 'ARMORY';

UPDATE rp_items SET required_job_id = (
    SELECT id FROM jobs WHERE name = 'casino'
) WHERE required_job = 'CASINO';

UPDATE rp_items SET required_job_id = (
    SELECT id FROM jobs WHERE name = 'bank'
) WHERE required_job = 'BANK';

-- Update offer_job_id based on offer_job names
UPDATE rp_items SET offer_job_id = (
    SELECT id FROM jobs WHERE name = 'police'
) WHERE offer_job = 'POLICE';

UPDATE rp_items SET offer_job_id = (
    SELECT id FROM jobs WHERE name = 'hospital'
) WHERE offer_job = 'HOSPITAL';

UPDATE rp_items SET offer_job_id = (
    SELECT id FROM jobs WHERE name = 'starbucks'
) WHERE offer_job = 'STARBUCKS';

UPDATE rp_items SET offer_job_id = (
    SELECT id FROM jobs WHERE name = 'armory'
) WHERE offer_job = 'ARMORY';

UPDATE rp_items SET offer_job_id = (
    SELECT id FROM jobs WHERE name = 'casino'
) WHERE offer_job = 'CASINO';

UPDATE rp_items SET offer_job_id = (
    SELECT id FROM jobs WHERE name = 'bank'
) WHERE offer_job = 'BANK';

-- Optional: After migration is complete and tested, you can drop the old columns
ALTER TABLE rp_items DROP COLUMN required_job;
ALTER TABLE rp_items DROP COLUMN offer_job;

ALTER TABLE `users_roleplay`
	ADD COLUMN `dead` INT NOT NULL DEFAULT '0';

CREATE TABLE IF NOT EXISTS `users_macros` (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `name` tinytext NOT NULL,
  `configs` varchar(16000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `Index 2` (`user_id`,`name`(15))
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

ALTER TABLE `users_settings` ADD COLUMN `macro_id` INT(33) NOT NULL DEFAULT '0';

-- Banking System Tables

-- Bank accounts table - tracks which users have bank accounts
CREATE TABLE IF NOT EXISTS `bank_accounts` (
  `user_id` int NOT NULL,
  `account_number` varchar(20) NOT NULL UNIQUE,
  `bank_balance` decimal(15,2) NOT NULL DEFAULT 0.00,
  `created_at` timestamp DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`user_id`),
  KEY `idx_bank_accounts_account_number` (`account_number`),
  FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Bank transactions table - logs all banking transactions
CREATE TABLE IF NOT EXISTS `bank_transactions` (
  `id` int NOT NULL AUTO_INCREMENT,
  `from_user_id` int NULL,
  `to_user_id` int NULL,
  `transaction_type` enum('deposit','withdraw','transfer','atm_fee','robbery') NOT NULL,
  `amount` decimal(15,2) NOT NULL,
  `fee_amount` decimal(15,2) DEFAULT 0.00,
  `description` varchar(255),
  `room_id` int NULL,
  `created_at` timestamp DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_bank_transactions_from_user` (`from_user_id`),
  KEY `idx_bank_transactions_to_user` (`to_user_id`),
  KEY `idx_bank_transactions_type` (`transaction_type`),
  KEY `idx_bank_transactions_created` (`created_at`),
  FOREIGN KEY (`from_user_id`) REFERENCES `users` (`id`) ON DELETE SET NULL,
  FOREIGN KEY (`to_user_id`) REFERENCES `users` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ATM robberies table - tracks ATM robbery attempts and results
CREATE TABLE IF NOT EXISTS `atm_robberies` (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `room_id` int NOT NULL,
  `furni_id` int NOT NULL,
  `amount_stolen` decimal(15,2) DEFAULT 0.00,
  `success` boolean DEFAULT FALSE,
  `weapon_used` varchar(50),
  `police_alerted` boolean DEFAULT FALSE,
  `created_at` timestamp DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_atm_robberies_user` (`user_id`),
  KEY `idx_atm_robberies_room` (`room_id`),
  KEY `idx_atm_robberies_furni` (`furni_id`),
  KEY `idx_atm_robberies_created` (`created_at`),
  FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ATM locations table - defines which furni items are ATMs
CREATE TABLE IF NOT EXISTS `atm_locations` (
  `id` int NOT NULL AUTO_INCREMENT,
  `room_id` int NOT NULL,
  `furni_id` int NOT NULL,
  `furni_base_id` int NOT NULL,
  `cash_available` decimal(15,2) DEFAULT 10000.00,
  `max_cash_capacity` decimal(15,2) DEFAULT 50000.00,
  `last_restocked` timestamp DEFAULT CURRENT_TIMESTAMP,
  `active` boolean DEFAULT TRUE,
  `created_at` timestamp DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `unique_atm_location` (`room_id`, `furni_id`),
  KEY `idx_atm_locations_room` (`room_id`),
  KEY `idx_atm_locations_furni` (`furni_id`),
  KEY `idx_atm_locations_active` (`active`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
