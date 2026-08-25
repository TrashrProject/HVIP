-- Use the restored Bloomingdales interior as the public clothing store.
UPDATE rooms
SET name = 'Magasin de vêtements - Bloomingdales',
    description = 'Magasin de vêtements de ParadiseRP',
    state = 'open',
    category = 1
WHERE id = 98;

INSERT INTO navigator_publics (public_cat_id, room_id, visible)
SELECT 1, 98, '1'
WHERE EXISTS (SELECT 1 FROM rooms WHERE id = 98)
  AND NOT EXISTS (SELECT 1 FROM navigator_publics WHERE room_id = 98);

UPDATE navigator_publics
SET public_cat_id = 1, visible = '1'
WHERE room_id = 98;

INSERT INTO emulator_settings (`key`, value)
VALUES ('roleplay.rooms.change_clothing.enabled', '98')
ON DUPLICATE KEY UPDATE value = VALUES(value);
