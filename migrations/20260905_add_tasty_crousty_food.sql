-- ParadiseRP - ajoute le Tasty Crousty au système de nourriture RP.
-- ID 6123, restaure 25 points de faim, pile maximale de 20, prix de base 30.

INSERT INTO rp_items
  (id, name, interaction_type, permission, enable_id, extra_data, max, price,
   offer_job_id, required_handitem, required_job_id, crafter_organizations)
VALUES
  (6123, 'Tasty Crousty', 'food', NULL, 0, '25', 20, 30,
   NULL, 0, NULL, '')
ON DUPLICATE KEY UPDATE
  name = VALUES(name),
  interaction_type = VALUES(interaction_type),
  permission = VALUES(permission),
  enable_id = VALUES(enable_id),
  extra_data = VALUES(extra_data),
  max = VALUES(max),
  price = VALUES(price),
  offer_job_id = VALUES(offer_job_id),
  required_handitem = VALUES(required_handitem),
  required_job_id = VALUES(required_job_id),
  crafter_organizations = VALUES(crafter_organizations);
