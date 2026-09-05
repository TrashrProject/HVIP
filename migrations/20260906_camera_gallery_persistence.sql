CREATE TABLE IF NOT EXISTS `camera_photos` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `user_id` INT NOT NULL,
  `room_id` INT NOT NULL DEFAULT 0,
  `timestamp` INT NOT NULL,
  `url` VARCHAR(1000) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_camera_photos_user` (`user_id`,`timestamp`,`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
