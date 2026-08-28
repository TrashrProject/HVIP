SET @police_group_id = (
    SELECT CAST(`value` AS UNSIGNED)
    FROM `server_settings`
    WHERE `key` = 'rp.police.corporation.id'
    LIMIT 1
);

UPDATE `groups`
SET `name` = 'Police',
    `desc` = 'Service de Police de Paradise',
    `group_type` = 1
WHERE `id` = @police_group_id;

UPDATE `groups` g
LEFT JOIN `users` current_owner ON current_owner.`id` = g.`owner_id`
SET g.`owner_id` = (SELECT u.`id` FROM `users` u WHERE u.`username` = 'Unio' LIMIT 1)
WHERE g.`id` = @police_group_id
  AND current_owner.`id` IS NULL
  AND EXISTS (SELECT 1 FROM `users` u WHERE u.`username` = 'Unio');

DELETE gm
FROM `group_memberships` gm
LEFT JOIN `users` u ON u.`id` = gm.`user_id`
WHERE gm.`group_id` = @police_group_id AND u.`id` IS NULL;

UPDATE `group_roles`
SET `name` = 'Agent de Police',
    `shift_pay` = 10,
    `shift_duration` = 10,
    `shift_motto` = '[EN SERVICE] Police'
WHERE `group_id` = @police_group_id AND `level` = 1;

INSERT INTO `group_memberships` (`group_id`, `user_id`, `level`)
SELECT g.`id`, g.`owner_id`, 1
FROM `groups` g
WHERE g.`id` = @police_group_id
  AND NOT EXISTS (
      SELECT 1
      FROM `group_memberships` gm
      WHERE gm.`group_id` = g.`id` AND gm.`user_id` = g.`owner_id`
  );
