CREATE TABLE IF NOT EXISTS `phone_gram_posts` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `user_id` INT NOT NULL,
  `body` VARCHAR(500) NOT NULL DEFAULT '',
  `image_url` VARCHAR(500) NULL,
  `created_at` INT NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_phone_gram_posts_created` (`created_at`, `id`),
  KEY `idx_phone_gram_posts_user` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `phone_gram_likes` (
  `post_id` INT NOT NULL,
  `user_id` INT NOT NULL,
  `created_at` INT NOT NULL,
  PRIMARY KEY (`post_id`, `user_id`),
  KEY `idx_phone_gram_likes_user` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `phone_gram_comments` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `post_id` INT NOT NULL,
  `user_id` INT NOT NULL,
  `body` VARCHAR(240) NOT NULL,
  `created_at` INT NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_phone_gram_comments_post` (`post_id`, `id`),
  KEY `idx_phone_gram_comments_user` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
