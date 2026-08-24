START TRANSACTION;

UPDATE rooms room
LEFT JOIN users owner ON owner.id = room.owner_id
SET room.owner_id = 20000,
    room.owner_name = 'Nathan',
    room.owner = 'Nathan'
WHERE owner.id IS NULL;

UPDATE items item
LEFT JOIN users owner ON owner.id = item.user_id
LEFT JOIN rooms room ON room.id = item.room_id
SET item.user_id = COALESCE(room.owner_id, 20000)
WHERE owner.id IS NULL;

COMMIT;
