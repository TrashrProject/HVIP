-- ParadisePhone V1 DEV/TEST seed only.
-- NEVER sourced by production migration. Change the username explicitly before use.
SET @phone_test_username := 'CHANGE_ME';
SET @phone_test_user_id := (SELECT `id` FROM `users` WHERE `username`=@phone_test_username LIMIT 1);
SET @phone_definition_id := (SELECT `id` FROM `rp_item_definitions` WHERE `code`='PHONE_BASIC' LIMIT 1);

INSERT INTO `rp_inventory_items` (`owner_user_id`,`item_definition_id`,`quantity`,`metadata`)
SELECT @phone_test_user_id,@phone_definition_id,1,'{"source":"phase4_dev_seed"}'
WHERE @phone_test_user_id IS NOT NULL
  AND @phone_definition_id IS NOT NULL
  AND NOT EXISTS (
    SELECT 1 FROM `rp_inventory_items`
    WHERE `owner_user_id`=@phone_test_user_id AND `item_definition_id`=@phone_definition_id AND `quantity`>0
  );
