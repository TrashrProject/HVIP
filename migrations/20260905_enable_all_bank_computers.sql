-- Active tous les ordinateurs actuellement poses dans les salles du metier Banque.
-- Le plugin reconnait aussi automatiquement les futurs ordinateurs ajoutes dans ces salles.
INSERT INTO rp_bank_computer_items (item_id, active)
SELECT DISTINCT i.id, 1
FROM items i
JOIN items_base b ON b.id = i.item_id
JOIN jobs j ON j.name = 'bank' AND j.active = 1
JOIN jobs_rooms jr ON jr.job_id = j.id
WHERE FIND_IN_SET(CAST(i.room_id AS CHAR), REPLACE(jr.rooms, ' ', '')) > 0
  AND b.interaction_type = 'rp_bank_computer'
ON DUPLICATE KEY UPDATE active = VALUES(active);
