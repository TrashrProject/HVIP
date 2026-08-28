-- ParadiseRP Phase 3 — optional furni/container foundation.
-- No Habbo furni is activated by default. A base item must be explicitly mapped after validation.

CREATE TABLE IF NOT EXISTS `rp_furni_containers` (
  `room_item_id` BIGINT UNSIGNED NOT NULL,
  `room_id` INT UNSIGNED NOT NULL,
  `base_item_id` INT UNSIGNED NOT NULL,
  `container_id` BIGINT UNSIGNED NOT NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`room_item_id`),
  UNIQUE KEY `uq_rp_furni_container_container` (`container_id`),
  KEY `idx_rp_furni_container_room` (`room_id`),
  KEY `idx_rp_furni_container_base` (`base_item_id`),
  CONSTRAINT `fk_rp_furni_container_container` FOREIGN KEY (`container_id`) REFERENCES `rp_containers` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Intentional: no INSERT into rp_furni_interactions here.
-- We will only map verified chest/vending/ATM base_item_id values from the live hotel data.
