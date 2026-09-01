CREATE TABLE IF NOT EXISTS `phone_music_tracks` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `owner_id` INT NOT NULL,
  `title` VARCHAR(100) NOT NULL,
  `artist` VARCHAR(80) NOT NULL DEFAULT '',
  `audio_url` VARCHAR(500) NOT NULL,
  `cover_url` VARCHAR(500) NULL,
  `genre` VARCHAR(40) NOT NULL DEFAULT '',
  `description` VARCHAR(500) NOT NULL DEFAULT '',
  `created_at` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_phone_music_owner_url` (`owner_id`, `audio_url`(191)),
  KEY `idx_phone_music_owner_created` (`owner_id`, `created_at`, `id`),
  KEY `idx_phone_music_search` (`title`, `artist`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `phone_music_favorites` (
  `user_id` INT NOT NULL,
  `track_id` INT UNSIGNED NOT NULL,
  `created_at` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`user_id`, `track_id`),
  KEY `idx_phone_music_favorites_track` (`track_id`),
  CONSTRAINT `fk_phone_music_favorite_track` FOREIGN KEY (`track_id`) REFERENCES `phone_music_tracks` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `phone_music_playlists` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT NOT NULL,
  `name` VARCHAR(80) NOT NULL,
  `cover_url` VARCHAR(500) NULL,
  `created_at` INT UNSIGNED NOT NULL,
  `updated_at` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_phone_music_playlists_user` (`user_id`, `updated_at`, `id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `phone_music_playlist_tracks` (
  `playlist_id` INT UNSIGNED NOT NULL,
  `track_id` INT UNSIGNED NOT NULL,
  `position` INT UNSIGNED NOT NULL DEFAULT 0,
  `created_at` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`playlist_id`, `track_id`),
  KEY `idx_phone_music_playlist_position` (`playlist_id`, `position`, `track_id`),
  KEY `idx_phone_music_playlist_track` (`track_id`),
  CONSTRAINT `fk_phone_music_playlist` FOREIGN KEY (`playlist_id`) REFERENCES `phone_music_playlists` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_phone_music_playlist_track` FOREIGN KEY (`track_id`) REFERENCES `phone_music_tracks` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
