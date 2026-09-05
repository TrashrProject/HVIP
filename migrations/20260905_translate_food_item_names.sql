-- ParadiseRP - noms francais des nourritures, encodes en UTF-8.
-- Les IDs restent inchanges afin de conserver tous les inventaires existants.

ALTER TABLE rp_items
  MODIFY name VARCHAR(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL;

UPDATE rp_items SET name = 'Pomme' WHERE id = 9 AND interaction_type = 'food';
UPDATE rp_items SET name = 'Banane' WHERE id = 10 AND interaction_type = 'food';
UPDATE rp_items SET name = 'Sandwich' WHERE id = 11 AND interaction_type = 'food';
UPDATE rp_items SET name = 'Burger' WHERE id = 12 AND interaction_type = 'food';
UPDATE rp_items SET name = 'Part de pizza' WHERE id = 13 AND interaction_type = 'food';
UPDATE rp_items SET name = 'Taco' WHERE id = 14 AND interaction_type = 'food';
UPDATE rp_items SET name = 'Sushi' WHERE id = 15 AND interaction_type = 'food';
UPDATE rp_items SET name = 'Steak' WHERE id = 16 AND interaction_type = 'food';
UPDATE rp_items SET name = CONVERT(0x50C3A2746573 USING utf8mb4) WHERE id = 18 AND interaction_type = 'food';
UPDATE rp_items SET name = 'Homard' WHERE id = 19 AND interaction_type = 'food';
UPDATE rp_items SET name = CONVERT(0x42C5937566 USING utf8mb4) WHERE id = 20 AND interaction_type = 'food';
UPDATE rp_items SET name = 'Repas complet' WHERE id = 21 AND interaction_type = 'food';
