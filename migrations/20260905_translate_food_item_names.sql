-- ParadiseRP - Traduction réelle des noms de nourriture dans rp_items
-- Conserve les IDs existants pour ne casser aucun inventaire utilisateur.
-- Cette migration modifie directement le champ rp_items.name : les noms français deviennent les vrais noms des items.

UPDATE `rp_items` SET `name` = 'Pomme' WHERE `id` = 9 AND `interaction_type` = 'food';
UPDATE `rp_items` SET `name` = 'Banane' WHERE `id` = 10 AND `interaction_type` = 'food';
UPDATE `rp_items` SET `name` = 'Sandwich' WHERE `id` = 11 AND `interaction_type` = 'food';
UPDATE `rp_items` SET `name` = 'Burger' WHERE `id` = 12 AND `interaction_type` = 'food';
UPDATE `rp_items` SET `name` = 'Part de pizza' WHERE `id` = 13 AND `interaction_type` = 'food';
UPDATE `rp_items` SET `name` = 'Taco' WHERE `id` = 14 AND `interaction_type` = 'food';
UPDATE `rp_items` SET `name` = 'Sushi' WHERE `id` = 15 AND `interaction_type` = 'food';
UPDATE `rp_items` SET `name` = 'Steak' WHERE `id` = 16 AND `interaction_type` = 'food';
UPDATE `rp_items` SET `name` = 'Pâtes' WHERE `id` = 18 AND `interaction_type` = 'food';
UPDATE `rp_items` SET `name` = 'Homard' WHERE `id` = 19 AND `interaction_type` = 'food';
UPDATE `rp_items` SET `name` = 'Bœuf' WHERE `id` = 20 AND `interaction_type` = 'food';
UPDATE `rp_items` SET `name` = 'Repas complet' WHERE `id` = 21 AND `interaction_type` = 'food';
