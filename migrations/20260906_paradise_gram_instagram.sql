CREATE TABLE IF NOT EXISTS `phone_gram_follows` (
  `follower_id` INT NOT NULL,
  `following_id` INT NOT NULL,
  `created_at` INT NOT NULL,
  PRIMARY KEY (`follower_id`,`following_id`),
  KEY `idx_phone_gram_follows_following` (`following_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `phone_gram_notifications` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `user_id` INT NOT NULL,
  `actor_id` INT NOT NULL,
  `type` VARCHAR(20) NOT NULL,
  `post_id` INT NULL,
  `created_at` INT NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_phone_gram_notifications_user` (`user_id`,`created_at`,`id`),
  KEY `idx_phone_gram_notifications_post` (`post_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
