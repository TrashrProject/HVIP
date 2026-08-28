-- ParadiseRP Phase 3 — DEVELOPMENT ONLY
-- This script grants a small test inventory only when PARADISE_DEV_USER_ID > 0.
-- Never run as part of the production migration.

SET @PARADISE_DEV_USER_ID := 0;

INSERT INTO `rp_inventory_profiles` (`user_id`,`base_capacity`,`capacity_bonus`,`max_slots`)
SELECT @PARADISE_DEV_USER_ID,50.000,0.000,30
WHERE @PARADISE_DEV_USER_ID > 0
ON DUPLICATE KEY UPDATE `user_id`=VALUES(`user_id`);

INSERT INTO `rp_inventory_items` (`owner_user_id`,`item_definition_id`,`quantity`,`metadata`)
SELECT @PARADISE_DEV_USER_ID,d.`id`,v.`qty`,NULL
FROM (
  SELECT 'WATER_BOTTLE' AS code,4 AS qty
  UNION ALL SELECT 'SANDWICH',2
  UNION ALL SELECT 'PHONE_BASIC',1
  UNION ALL SELECT 'GENERIC_KEY',1
  UNION ALL SELECT 'SCRAP_METAL',2
) v
INNER JOIN `rp_item_definitions` d ON d.`code`=v.`code`
WHERE @PARADISE_DEV_USER_ID > 0;
