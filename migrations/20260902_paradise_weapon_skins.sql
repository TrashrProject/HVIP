-- ParadiseRP weapon skins (idempotent production migration)
CREATE TABLE IF NOT EXISTS `paradise_weapon_skins` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `weapon_key` VARCHAR(32) NOT NULL,
  `name` VARCHAR(64) NOT NULL,
  `effect_id` INT NOT NULL,
  `image` VARCHAR(160) NOT NULL,
  `avatar_image` VARCHAR(160) NOT NULL,
  `is_default` TINYINT(1) NOT NULL DEFAULT 0,
  `sort_order` SMALLINT UNSIGNED NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_rp_weapon_skin` (`weapon_key`, `effect_id`),
  KEY `idx_rp_weapon_skin_weapon` (`weapon_key`, `sort_order`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `paradise_user_weapon_skins` (
  `user_id` INT NOT NULL,
  `skin_id` INT UNSIGNED NOT NULL,
  `equipped` TINYINT(1) NOT NULL DEFAULT 0,
  `unlocked_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`user_id`, `skin_id`),
  KEY `idx_rp_user_skin_equipped` (`user_id`, `equipped`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `paradise_weapon_skins`
(`weapon_key`,`name`,`effect_id`,`image`,`avatar_image`,`is_default`,`sort_order`) VALUES
('tazor','Standard',592,'stun_592.png','avatar_stun_592.png',1,10),
('tazor','Cyber',101,'stun_101.png','avatar_stun_101.png',0,20),
('tazor','Doré',677,'stun_677.png','avatar_stun_677.png',0,30),
('tazor','Rose',716,'stun_716.png','avatar_stun_716.png',0,40),
('tazor','Camouflage',718,'stun_718.png','avatar_stun_718.png',0,50),
('tazor','Police',717,'stun_717.png','avatar_stun_717.png',0,60),
('ak47','Standard',583,'ak47_583.png','avatar_ak47_583.png',1,10),
('ak47','Rose',587,'ak47_587.png','avatar_ak47_587.png',0,20),
('ak47','Camouflage',588,'ak47_588.png','avatar_ak47_588.png',0,30),
('ak47','Doré',589,'ak47_589.png','avatar_ak47_589.png',0,40),
('ak47','Feu',603,'ak47_603.png','avatar_ak47_603.png',0,50),
('akm','Standard',575,'akm_575.png','avatar_akm_575.png',1,10),
('akm','AKM lunaire',576,'akm_576.png','avatar_akm_576.png',0,20),
('g36','Standard',584,'g36_584.png','avatar_g36_584.png',1,10)
ON DUPLICATE KEY UPDATE
  `name`=VALUES(`name`), `image`=VALUES(`image`),
  `avatar_image`=VALUES(`avatar_image`), `is_default`=VALUES(`is_default`),
  `sort_order`=VALUES(`sort_order`);

-- The requested complete pack ships every current skin unlocked. The ownership
-- table remains separate so future shop/event skins can still be restricted.
INSERT IGNORE INTO `paradise_user_weapon_skins` (`user_id`,`skin_id`,`equipped`)
SELECT u.`id`, s.`id`, s.`is_default` FROM `users` u CROSS JOIN `paradise_weapon_skins` s;
