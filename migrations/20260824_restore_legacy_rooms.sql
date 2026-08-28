SET @legacy_schema = 'waveplus_restore_rooms_tmp';
SET @owner_id = (SELECT id FROM waveplus.users WHERE username = 'Nathan' LIMIT 1);
SET @room_offset = (SELECT COALESCE(MAX(id), 0) FROM waveplus.rooms);
SET @item_offset = (SELECT COALESCE(MAX(id), 0) FROM waveplus.items);
SET @next_base_id = (SELECT COALESCE(MAX(id), 0) FROM waveplus.items_base);

START TRANSACTION;

CREATE TEMPORARY TABLE legacy_room_map (
    old_id INT NOT NULL PRIMARY KEY,
    new_id INT NOT NULL UNIQUE
) ENGINE=MEMORY;

INSERT INTO legacy_room_map (old_id, new_id)
SELECT id, @room_offset + id
FROM waveplus_restore_rooms_tmp.rooms;

CREATE TEMPORARY TABLE legacy_furniture_conflicts (
    sequence_id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    old_id INT NOT NULL UNIQUE
) ENGINE=MEMORY;

INSERT INTO legacy_furniture_conflicts (old_id)
SELECT DISTINCT furniture.id
FROM waveplus_restore_rooms_tmp.items legacy_item
JOIN waveplus_restore_rooms_tmp.furniture furniture ON furniture.id = legacy_item.base_item
JOIN waveplus.items_base current_base ON current_base.id = furniture.id
WHERE legacy_item.room_id > 0
  AND current_base.item_name <> furniture.item_name
ORDER BY furniture.id;

CREATE TEMPORARY TABLE legacy_furniture_map (
    old_id INT NOT NULL PRIMARY KEY,
    new_id INT NOT NULL UNIQUE
) ENGINE=MEMORY;

INSERT INTO legacy_furniture_map (old_id, new_id)
SELECT DISTINCT furniture.id,
       CASE
           WHEN conflict.old_id IS NULL THEN furniture.id
           ELSE @next_base_id + conflict.sequence_id
       END
FROM waveplus_restore_rooms_tmp.items legacy_item
JOIN waveplus_restore_rooms_tmp.furniture furniture ON furniture.id = legacy_item.base_item
LEFT JOIN legacy_furniture_conflicts conflict ON conflict.old_id = furniture.id
WHERE legacy_item.room_id > 0;

INSERT INTO waveplus.items_base (
    id, sprite_id, public_name, item_name, type, width, length, stack_height,
    allow_stack, allow_sit, allow_lay, allow_walk, allow_gift, allow_trade,
    allow_recycle, allow_marketplace_sell, allow_inventory_stack,
    interaction_type, interaction_modes_count, vending_ids, multiheight,
    customparams, effect_id_male, effect_id_female, clothing_on_walk
)
SELECT furniture_map.new_id,
       COALESCE(furniture.sprite_id, 0),
       LEFT(COALESCE(furniture.public_name, furniture.item_name), 56),
       furniture.item_name,
       furniture.type,
       furniture.width,
       furniture.length,
       furniture.stack_height,
       furniture.can_stack,
       furniture.can_sit,
       COALESCE(furniture.allow_lay, 0),
       COALESCE(furniture.is_walkable, 0),
       furniture.allow_gift,
       furniture.allow_trade,
       furniture.allow_recycle,
       furniture.allow_marketplace_sell,
       furniture.allow_inventory_stack,
       furniture.interaction_type,
       furniture.interaction_modes_count,
       furniture.vending_ids,
       furniture.height_adjustable,
       '',
       furniture.effect_id,
       furniture.effect_id,
       IF(furniture.clothing_id > 0, CAST(furniture.clothing_id AS CHAR), '')
FROM legacy_furniture_map furniture_map
JOIN waveplus_restore_rooms_tmp.furniture furniture ON furniture.id = furniture_map.old_id
LEFT JOIN waveplus.items_base existing_base ON existing_base.id = furniture_map.new_id
WHERE existing_base.id IS NULL;

INSERT INTO waveplus.room_models (
    name, door_x, door_y, door_dir, heightmap, public_items, club_only
)
SELECT legacy_model.id,
       legacy_model.door_x,
       legacy_model.door_y,
       legacy_model.door_dir,
       legacy_model.heightmap,
       COALESCE(legacy_model.public_items, ''),
       legacy_model.club_only
FROM waveplus_restore_rooms_tmp.room_models legacy_model
LEFT JOIN waveplus.room_models current_model ON current_model.name = legacy_model.id
WHERE current_model.name IS NULL;

INSERT INTO waveplus.rooms (
    id, owner_id, owner_name, name, description, model, password, state,
    users, users_max, guild_id, category, score, paper_floor, paper_wall,
    paper_landscape, thickness_wall, wall_height, thickness_floor, tags,
    is_public, allow_other_pets, allow_other_pets_eat, allow_walkthrough,
    allow_hidewall, chat_mode, chat_weight, chat_speed,
    chat_hearing_distance, chat_protection, trade_mode, roomtype, caption,
    owner, users_now, model_name, group_id
)
SELECT room_map.new_id,
       @owner_id,
       'Nathan',
       LEFT(legacy_room.caption, 50),
       legacy_room.description,
       CASE WHEN legacy_model.id IS NULL THEN 'model_a' ELSE legacy_room.model_name END,
       LEFT(legacy_room.password, 20),
       legacy_room.state,
       0,
       legacy_room.users_max,
       0,
       CASE WHEN category.id IS NULL THEN 1 ELSE legacy_room.category END,
       legacy_room.score,
       LEFT(legacy_room.floor, 5),
       LEFT(legacy_room.wallpaper, 5),
       LEFT(legacy_room.landscape, 5),
       legacy_room.wallthick,
       COALESCE(legacy_model.wall_height, -1),
       legacy_room.floorthick,
       legacy_room.tags,
       '0',
       IF(legacy_room.allow_pets = 1, '1', '0'),
       IF(legacy_room.allow_pets_eat = 1, '1', '0'),
       IF(legacy_room.room_blocking_disabled = 1, '1', '0'),
       IF(legacy_room.allow_hidewall = 1, '1', '0'),
       legacy_room.chat_mode,
       legacy_room.chat_size,
       legacy_room.chat_speed,
       legacy_room.chat_hearing_distance,
       legacy_room.chat_extra_flood,
       legacy_room.trade_settings,
       legacy_room.roomtype,
       LEFT(legacy_room.caption, 50),
       'Nathan',
       0,
       CASE WHEN legacy_model.id IS NULL THEN 'model_a' ELSE legacy_room.model_name END,
       0
FROM waveplus_restore_rooms_tmp.rooms legacy_room
JOIN legacy_room_map room_map ON room_map.old_id = legacy_room.id
LEFT JOIN waveplus.navigator_flatcats category ON category.id = legacy_room.category
LEFT JOIN waveplus_restore_rooms_tmp.room_models legacy_model ON legacy_model.id = legacy_room.model_name;

INSERT INTO waveplus.items (
    id, user_id, room_id, item_id, wall_pos, x, y, z, rot,
    extra_data, wired_data, limited_data, guild_id
)
SELECT @item_offset + legacy_item.id,
       @owner_id,
       room_map.new_id,
       furniture_map.new_id,
       LEFT(COALESCE(legacy_item.wall_pos, ''), 20),
       legacy_item.x,
       legacy_item.y,
       legacy_item.z,
       legacy_item.rot,
       LEFT(COALESCE(legacy_item.extra_data, ''), 1024),
       '',
       CONCAT(COALESCE(legacy_item.limited_number, 0), ':', COALESCE(legacy_item.limited_stack, 0)),
       0
FROM waveplus_restore_rooms_tmp.items legacy_item
JOIN legacy_room_map room_map ON room_map.old_id = legacy_item.room_id
JOIN legacy_furniture_map furniture_map ON furniture_map.old_id = legacy_item.base_item
WHERE legacy_item.room_id > 0;

INSERT INTO waveplus.bots (
    id, user_id, room_id, name, motto, figure, gender, x, y, z, rot,
    chat_lines, chat_auto, chat_random, chat_delay, dance, freeroam,
    type, effect, bubble_id
)
SELECT legacy_bot.id,
       @owner_id,
       room_map.new_id,
       LEFT(legacy_bot.name, 25),
       LEFT(legacy_bot.motto, 100),
       COALESCE(legacy_bot.look, ''),
       IF(UPPER(legacy_bot.gender) = 'F', 'F', 'M'),
       legacy_bot.x,
       legacy_bot.y,
       legacy_bot.z,
       legacy_bot.rotation,
       LEFT(COALESCE(speech.chat_lines, ''), 5112),
       IF(legacy_bot.automatic_chat = 'true', '1', '0'),
       IF(legacy_bot.mix_sentences = '1', '1', '0'),
       legacy_bot.speaking_interval,
       legacy_bot.dance,
       IF(legacy_bot.walk_mode = 'freeroam', '1', '0'),
       IF(legacy_bot.ai_type = 'bartender', 'bartender', 'generic'),
       legacy_bot.effect_id,
       legacy_bot.chat_bubble
FROM waveplus_restore_rooms_tmp.bots legacy_bot
JOIN legacy_room_map room_map ON room_map.old_id = legacy_bot.room_id
LEFT JOIN (
    SELECT bot_id, GROUP_CONCAT(text ORDER BY id SEPARATOR '\r') AS chat_lines
    FROM waveplus_restore_rooms_tmp.bots_speech
    GROUP BY bot_id
) speech ON speech.bot_id = legacy_bot.id
LEFT JOIN waveplus.bots current_bot ON current_bot.id = legacy_bot.id
WHERE current_bot.id IS NULL;

INSERT INTO waveplus.items_teleports (teleport_one_id, teleport_two_id)
SELECT @item_offset + legacy_link.tele_one_id,
       @item_offset + legacy_link.tele_two_id
FROM waveplus_restore_rooms_tmp.room_items_tele_links legacy_link
JOIN waveplus_restore_rooms_tmp.items first_item
  ON first_item.id = legacy_link.tele_one_id AND first_item.room_id > 0
JOIN waveplus_restore_rooms_tmp.items second_item
  ON second_item.id = legacy_link.tele_two_id AND second_item.room_id > 0
LEFT JOIN waveplus.items_teleports current_link
  ON current_link.teleport_one_id = @item_offset + legacy_link.tele_one_id
 AND current_link.teleport_two_id = @item_offset + legacy_link.tele_two_id
WHERE current_link.teleport_one_id IS NULL;

UPDATE waveplus.room_items_tele_links current_link
JOIN waveplus_restore_rooms_tmp.room_items_tele_links legacy_link ON legacy_link.id = current_link.id
JOIN waveplus_restore_rooms_tmp.items first_item
  ON first_item.id = legacy_link.tele_one_id AND first_item.room_id > 0
JOIN waveplus_restore_rooms_tmp.items second_item
  ON second_item.id = legacy_link.tele_two_id AND second_item.room_id > 0
SET current_link.tele_one_id = @item_offset + legacy_link.tele_one_id,
    current_link.tele_two_id = @item_offset + legacy_link.tele_two_id;

UPDATE waveplus.room_taxi_points taxi_point
JOIN legacy_room_map room_map ON room_map.old_id = taxi_point.room_id
SET taxi_point.room_id = room_map.new_id;

COMMIT;
