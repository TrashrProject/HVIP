-- ParadiseRP Phase 4.5 Quality Gate — UTF-8 repair
-- Small idempotent repair only. No player data is created or removed.
-- Hex literals make this safe even when mysql.exe starts with a non-UTF-8 client charset.

SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;

UPDATE `rp_item_definitions`
SET `name` = CONVERT(0x426f757465696c6c652064e28099656175 USING utf8mb4),
    `description` = CONVERT(0x556e6520626f757465696c6c652064e2809965617520667261c3ae63686520646520506c616369642049736c616e642e USING utf8mb4)
WHERE `code` = 'WATER_BOTTLE';

UPDATE `rp_item_definitions`
SET `name` = CONVERT(0x54c3a96cc3a970686f6e65 USING utf8mb4),
    `description` = CONVERT(0x566f7472652074c3a96cc3a970686f6e6520706572736f6e6e656c20506172616469736552502e USING utf8mb4)
WHERE `code` = 'PHONE_BASIC';

UPDATE `rp_item_definitions`
SET `name` = CONVERT(0x436cc3a9 USING utf8mb4),
    `description` = CONVERT(0x556e6520636cc3a9207068797369717565206c69c3a96520c3a020756e20616363c3a8732052502e USING utf8mb4)
WHERE `code` = 'GENERIC_KEY';

UPDATE `rp_document_types`
SET `name` = CONVERT(0x43617274652064e280996964656e746974c3a920646520506c616369642049736c616e64 USING utf8mb4)
WHERE `code` = 'PLACID_ID';

-- Read-only verification after import.
SELECT `code`,`name`,`description` FROM `rp_item_definitions`
WHERE `code` IN ('WATER_BOTTLE','PHONE_BASIC','GENERIC_KEY')
ORDER BY `id`;
SELECT `code`,`name` FROM `rp_document_types` WHERE `code`='PLACID_ID';
