ALTER TABLE bans
    ADD COLUMN IF NOT EXISTS bantype VARCHAR(16) NULL AFTER type,
    ADD COLUMN IF NOT EXISTS value VARCHAR(255) NULL AFTER bantype,
    ADD COLUMN IF NOT EXISTS expire INT NOT NULL DEFAULT 0 AFTER value,
    ADD COLUMN IF NOT EXISTS reason VARCHAR(200) NOT NULL DEFAULT '' AFTER expire,
    ADD COLUMN IF NOT EXISTS added_by VARCHAR(255) NOT NULL DEFAULT '' AFTER reason,
    ADD COLUMN IF NOT EXISTS added_date VARCHAR(32) NOT NULL DEFAULT '' AFTER added_by;

UPDATE bans b
LEFT JOIN users u ON u.id = b.user_id
LEFT JOIN users staff ON staff.id = b.user_staff_id
SET b.bantype = b.type,
    b.value = CASE WHEN b.type = 'ip' THEN b.ip ELSE COALESCE(u.username, '') END,
    b.expire = b.ban_expire,
    b.reason = b.ban_reason,
    b.added_by = COALESCE(staff.username, 'Systeme'),
    b.added_date = CAST(b.timestamp AS CHAR);

DROP TRIGGER IF EXISTS bans_cms_compatibility_insert;

DELIMITER $$
CREATE TRIGGER bans_cms_compatibility_insert
BEFORE INSERT ON bans
FOR EACH ROW
BEGIN
    IF NEW.bantype IS NOT NULL AND NEW.bantype <> '' THEN
        SET NEW.type = NEW.bantype;
        SET NEW.ban_expire = NEW.expire;
        SET NEW.ban_reason = NEW.reason;
        SET NEW.timestamp = COALESCE(NULLIF(CAST(NEW.added_date AS UNSIGNED), 0), UNIX_TIMESTAMP());
        SET NEW.user_staff_id = COALESCE(
            NULLIF(NEW.user_staff_id, 0),
            (SELECT id FROM users WHERE username = NEW.added_by LIMIT 1),
            0
        );
        IF NEW.type = 'ip' THEN
            SET NEW.ip = NEW.value;
        END IF;
    ELSE
        SET NEW.bantype = NEW.type;
        SET NEW.value = CASE
            WHEN NEW.type = 'ip' THEN NEW.ip
            ELSE COALESCE((SELECT username FROM users WHERE id = NEW.user_id LIMIT 1), '')
        END;
        SET NEW.expire = NEW.ban_expire;
        SET NEW.reason = NEW.ban_reason;
        SET NEW.added_by = COALESCE((SELECT username FROM users WHERE id = NEW.user_staff_id LIMIT 1), 'Systeme');
        SET NEW.added_date = CAST(NEW.timestamp AS CHAR);
    END IF;
END$$
DELIMITER ;
