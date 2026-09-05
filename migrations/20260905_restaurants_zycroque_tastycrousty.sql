-- ParadiseRP - Restaurants Zy'Croque et Tasty Crousty
-- Deux métiers indépendants partageant le même moteur Java Restaurant.

INSERT INTO `jobs` (`name`, `display_name`, `description`, `active`)
VALUES
('zycroque', 'Zy''Croque', 'Restaurant Zy''Croque', 1),
('tastycrousty', 'Tasty Crousty', 'Restaurant Tasty Crousty', 1)
ON DUPLICATE KEY UPDATE
  `display_name` = VALUES(`display_name`),
  `description` = VALUES(`description`),
  `active` = VALUES(`active`);

-- -1 = toutes les salles : aucun TP ajouté par le système Restaurant.
INSERT INTO `jobs_rooms` (`job_id`, `rooms`)
SELECT j.id, '-1'
FROM `jobs` j
WHERE j.name = 'zycroque'
  AND NOT EXISTS (SELECT 1 FROM `jobs_rooms` jr WHERE jr.job_id = j.id);

INSERT INTO `jobs_rooms` (`job_id`, `rooms`)
SELECT j.id, '-1'
FROM `jobs` j
WHERE j.name = 'tastycrousty'
  AND NOT EXISTS (SELECT 1 FROM `jobs_rooms` jr WHERE jr.job_id = j.id);

-- Grades Zy'Croque
INSERT INTO `job_ranks` (`job_id`,`name`,`display_name`,`level`,`is_manager`,`salary`,`permissions`,`active`)
SELECT j.id, 'zycroque_employee', 'Employé', 1, 0, 8.00,
       JSON_ARRAY('restaurant.menu','restaurant.order','restaurant.serve','restaurant.bill','restaurant.cash'), 1
FROM jobs j WHERE j.name='zycroque'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name), level=VALUES(level), is_manager=VALUES(is_manager), salary=VALUES(salary), permissions=VALUES(permissions), active=1;

INSERT INTO `job_ranks` (`job_id`,`name`,`display_name`,`level`,`is_manager`,`salary`,`permissions`,`active`)
SELECT j.id, 'zycroque_cook', 'Cuisinier', 2, 0, 10.00,
       JSON_ARRAY('restaurant.menu','restaurant.order','restaurant.prepare','restaurant.serve','restaurant.bill','restaurant.cash'), 1
FROM jobs j WHERE j.name='zycroque'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name), level=VALUES(level), is_manager=VALUES(is_manager), salary=VALUES(salary), permissions=VALUES(permissions), active=1;

INSERT INTO `job_ranks` (`job_id`,`name`,`display_name`,`level`,`is_manager`,`salary`,`permissions`,`active`)
SELECT j.id, 'zycroque_headchef', 'Chef de cuisine', 3, 0, 12.00,
       JSON_ARRAY('restaurant.menu','restaurant.order','restaurant.prepare','restaurant.serve','restaurant.bill','restaurant.cash','restaurant.kitchen'), 1
FROM jobs j WHERE j.name='zycroque'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name), level=VALUES(level), is_manager=VALUES(is_manager), salary=VALUES(salary), permissions=VALUES(permissions), active=1;

INSERT INTO `job_ranks` (`job_id`,`name`,`display_name`,`level`,`is_manager`,`salary`,`permissions`,`active`)
SELECT j.id, 'zycroque_manager', 'Manager', 4, 1, 15.00,
       JSON_ARRAY('restaurant.menu','restaurant.order','restaurant.prepare','restaurant.serve','restaurant.bill','restaurant.cash','restaurant.kitchen','job.hire','job.fire','job.promote','job.demote'), 1
FROM jobs j WHERE j.name='zycroque'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name), level=VALUES(level), is_manager=VALUES(is_manager), salary=VALUES(salary), permissions=VALUES(permissions), active=1;

INSERT INTO `job_ranks` (`job_id`,`name`,`display_name`,`level`,`is_manager`,`salary`,`permissions`,`active`)
SELECT j.id, 'zycroque_director', 'Directeur', 5, 1, 20.00,
       JSON_ARRAY('restaurant.menu','restaurant.order','restaurant.prepare','restaurant.serve','restaurant.bill','restaurant.cash','restaurant.kitchen','job.hire','job.fire','job.promote','job.demote','job.schedule'), 1
FROM jobs j WHERE j.name='zycroque'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name), level=VALUES(level), is_manager=VALUES(is_manager), salary=VALUES(salary), permissions=VALUES(permissions), active=1;

-- Grades Tasty Crousty
INSERT INTO `job_ranks` (`job_id`,`name`,`display_name`,`level`,`is_manager`,`salary`,`permissions`,`active`)
SELECT j.id, 'tastycrousty_employee', 'Employé', 1, 0, 8.00,
       JSON_ARRAY('restaurant.menu','restaurant.order','restaurant.serve','restaurant.bill','restaurant.cash'), 1
FROM jobs j WHERE j.name='tastycrousty'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name), level=VALUES(level), is_manager=VALUES(is_manager), salary=VALUES(salary), permissions=VALUES(permissions), active=1;

INSERT INTO `job_ranks` (`job_id`,`name`,`display_name`,`level`,`is_manager`,`salary`,`permissions`,`active`)
SELECT j.id, 'tastycrousty_cook', 'Cuisinier', 2, 0, 10.00,
       JSON_ARRAY('restaurant.menu','restaurant.order','restaurant.prepare','restaurant.serve','restaurant.bill','restaurant.cash'), 1
FROM jobs j WHERE j.name='tastycrousty'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name), level=VALUES(level), is_manager=VALUES(is_manager), salary=VALUES(salary), permissions=VALUES(permissions), active=1;

INSERT INTO `job_ranks` (`job_id`,`name`,`display_name`,`level`,`is_manager`,`salary`,`permissions`,`active`)
SELECT j.id, 'tastycrousty_headchef', 'Chef de cuisine', 3, 0, 12.00,
       JSON_ARRAY('restaurant.menu','restaurant.order','restaurant.prepare','restaurant.serve','restaurant.bill','restaurant.cash','restaurant.kitchen'), 1
FROM jobs j WHERE j.name='tastycrousty'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name), level=VALUES(level), is_manager=VALUES(is_manager), salary=VALUES(salary), permissions=VALUES(permissions), active=1;

INSERT INTO `job_ranks` (`job_id`,`name`,`display_name`,`level`,`is_manager`,`salary`,`permissions`,`active`)
SELECT j.id, 'tastycrousty_manager', 'Manager', 4, 1, 15.00,
       JSON_ARRAY('restaurant.menu','restaurant.order','restaurant.prepare','restaurant.serve','restaurant.bill','restaurant.cash','restaurant.kitchen','job.hire','job.fire','job.promote','job.demote'), 1
FROM jobs j WHERE j.name='tastycrousty'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name), level=VALUES(level), is_manager=VALUES(is_manager), salary=VALUES(salary), permissions=VALUES(permissions), active=1;

INSERT INTO `job_ranks` (`job_id`,`name`,`display_name`,`level`,`is_manager`,`salary`,`permissions`,`active`)
SELECT j.id, 'tastycrousty_director', 'Directeur', 5, 1, 20.00,
       JSON_ARRAY('restaurant.menu','restaurant.order','restaurant.prepare','restaurant.serve','restaurant.bill','restaurant.cash','restaurant.kitchen','job.hire','job.fire','job.promote','job.demote','job.schedule'), 1
FROM jobs j WHERE j.name='tastycrousty'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name), level=VALUES(level), is_manager=VALUES(is_manager), salary=VALUES(salary), permissions=VALUES(permissions), active=1;

-- Nouveaux aliments intégrés au vrai système rp_items/food.
-- IDs réservés haut pour éviter les collisions avec les items RP existants.
INSERT INTO `rp_items` (`id`,`name`,`interaction_type`,`permission`,`enable_id`,`extra_data`,`max`,`price`,`offer_job`,`required_job`,`crafter_organizations`)
VALUES
(91001,'Croque-monsieur','food',NULL,0,'8',10,5,NULL,NULL,''),
(91002,'Frites','food',NULL,0,'6',10,4,NULL,NULL,''),
(91003,'Boisson','food',NULL,0,'4',10,3,NULL,NULL,''),
(91004,'Poulet croustillant','food',NULL,0,'10',8,7,NULL,NULL,'')
ON DUPLICATE KEY UPDATE name=VALUES(name), interaction_type='food', extra_data=VALUES(extra_data), max=VALUES(max), price=VALUES(price);

CREATE TABLE IF NOT EXISTS `restaurant_menu` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `job_id` INT NOT NULL,
  `code` VARCHAR(64) NOT NULL,
  `display_name` VARCHAR(100) NOT NULL,
  `item_id` INT NOT NULL,
  `price` INT NOT NULL,
  `active` BOOLEAN NOT NULL DEFAULT TRUE,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_restaurant_menu_job_code` (`job_id`,`code`),
  CONSTRAINT `fk_restaurant_menu_job` FOREIGN KEY (`job_id`) REFERENCES `jobs` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_restaurant_menu_item` FOREIGN KEY (`item_id`) REFERENCES `rp_items` (`id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `restaurant_accounts` (
  `job_id` INT NOT NULL,
  `balance` BIGINT NOT NULL DEFAULT 0,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`job_id`),
  CONSTRAINT `fk_restaurant_accounts_job` FOREIGN KEY (`job_id`) REFERENCES `jobs` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO restaurant_accounts(job_id,balance)
SELECT id,0 FROM jobs WHERE name IN ('zycroque','tastycrousty')
ON DUPLICATE KEY UPDATE job_id=VALUES(job_id);

-- Zy'Croque
INSERT INTO restaurant_menu(job_id,code,display_name,item_id,price,active)
SELECT j.id,'croque','Croque-monsieur',91001,6,1 FROM jobs j WHERE j.name='zycroque'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name),item_id=VALUES(item_id),price=VALUES(price),active=1;
INSERT INTO restaurant_menu(job_id,code,display_name,item_id,price,active)
SELECT j.id,'sandwich','Sandwich',11,5,1 FROM jobs j WHERE j.name='zycroque'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name),item_id=VALUES(item_id),price=VALUES(price),active=1;
INSERT INTO restaurant_menu(job_id,code,display_name,item_id,price,active)
SELECT j.id,'frites','Frites',91002,4,1 FROM jobs j WHERE j.name='zycroque'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name),item_id=VALUES(item_id),price=VALUES(price),active=1;
INSERT INTO restaurant_menu(job_id,code,display_name,item_id,price,active)
SELECT j.id,'burger','Burger',12,7,1 FROM jobs j WHERE j.name='zycroque'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name),item_id=VALUES(item_id),price=VALUES(price),active=1;
INSERT INTO restaurant_menu(job_id,code,display_name,item_id,price,active)
SELECT j.id,'boisson','Boisson',91003,3,1 FROM jobs j WHERE j.name='zycroque'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name),item_id=VALUES(item_id),price=VALUES(price),active=1;

-- Tasty Crousty
INSERT INTO restaurant_menu(job_id,code,display_name,item_id,price,active)
SELECT j.id,'tacos','Tacos',14,7,1 FROM jobs j WHERE j.name='tastycrousty'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name),item_id=VALUES(item_id),price=VALUES(price),active=1;
INSERT INTO restaurant_menu(job_id,code,display_name,item_id,price,active)
SELECT j.id,'poulet','Poulet croustillant',91004,8,1 FROM jobs j WHERE j.name='tastycrousty'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name),item_id=VALUES(item_id),price=VALUES(price),active=1;
INSERT INTO restaurant_menu(job_id,code,display_name,item_id,price,active)
SELECT j.id,'frites','Frites',91002,4,1 FROM jobs j WHERE j.name='tastycrousty'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name),item_id=VALUES(item_id),price=VALUES(price),active=1;
INSERT INTO restaurant_menu(job_id,code,display_name,item_id,price,active)
SELECT j.id,'menu-poulet','Menu poulet',21,12,1 FROM jobs j WHERE j.name='tastycrousty'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name),item_id=VALUES(item_id),price=VALUES(price),active=1;
INSERT INTO restaurant_menu(job_id,code,display_name,item_id,price,active)
SELECT j.id,'boisson','Boisson',91003,3,1 FROM jobs j WHERE j.name='tastycrousty'
ON DUPLICATE KEY UPDATE display_name=VALUES(display_name),item_id=VALUES(item_id),price=VALUES(price),active=1;
