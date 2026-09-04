-- ParadiseRP - Machine à café RP
-- Rend la Machine à expresso HC (classname hc2_coffee) utilisable comme machine à café RP.
-- Rejouable sans effet de bord.

UPDATE `items_base`
SET `interaction_type` = 'rp_coffee_machine'
WHERE `item_name` = 'hc2_coffee';
