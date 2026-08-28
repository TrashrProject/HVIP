-- Keep the existing fallback when valid; otherwise use the restored City Center.
INSERT INTO emulator_settings (`key`, value)
SELECT 'hotel.home.room', '63'
WHERE NOT EXISTS (
  SELECT 1 FROM emulator_settings WHERE `key` = 'hotel.home.room'
);

UPDATE emulator_settings AS setting
LEFT JOIN rooms AS configured_room
  ON configured_room.id = CAST(setting.value AS UNSIGNED)
SET setting.value = '63'
WHERE setting.`key` = 'hotel.home.room'
  AND (CAST(setting.value AS UNSIGNED) <= 0 OR configured_room.id IS NULL);
