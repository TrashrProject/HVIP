CREATE TABLE IF NOT EXISTS cms_articles (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT,
    title VARCHAR(160) NOT NULL,
    slug VARCHAR(180) NOT NULL,
    summary VARCHAR(300) NOT NULL DEFAULT '',
    content MEDIUMTEXT NOT NULL,
    image_url VARCHAR(500) DEFAULT NULL,
    author_id INT NOT NULL,
    published TINYINT(1) NOT NULL DEFAULT 0,
    published_at INT DEFAULT NULL,
    created_at INT NOT NULL,
    updated_at INT NOT NULL,
    PRIMARY KEY (id),
    UNIQUE KEY cms_articles_slug_unique (slug),
    KEY cms_articles_published_date (published, published_at),
    KEY cms_articles_author (author_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS cms_sessions (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    user_id INT NOT NULL,
    token_hash CHAR(64) NOT NULL,
    ip_address VARCHAR(45) NOT NULL,
    user_agent VARCHAR(255) NOT NULL DEFAULT '',
    created_at INT NOT NULL,
    last_seen_at INT NOT NULL,
    expires_at INT NOT NULL,
    revoked_at INT DEFAULT NULL,
    PRIMARY KEY (id),
    UNIQUE KEY cms_sessions_token_unique (token_hash),
    KEY cms_sessions_user_active (user_id, revoked_at, expires_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS cms_audit_log (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    actor_id INT NOT NULL,
    action VARCHAR(80) NOT NULL,
    target_type VARCHAR(50) DEFAULT NULL,
    target_id VARCHAR(80) DEFAULT NULL,
    context_json TEXT DEFAULT NULL,
    ip_address VARCHAR(45) NOT NULL,
    created_at INT NOT NULL,
    PRIMARY KEY (id),
    KEY cms_audit_actor_date (actor_id, created_at),
    KEY cms_audit_action_date (action, created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Colonnes de compatibilité pour les fonctions gangs du CMS historique.
ALTER TABLE groups ADD COLUMN IF NOT EXISTS badge_changes INT NOT NULL DEFAULT 3;
ALTER TABLE groups ADD COLUMN IF NOT EXISTS gang_kills INT NOT NULL DEFAULT 0;
ALTER TABLE groups ADD COLUMN IF NOT EXISTS gang_deaths INT NOT NULL DEFAULT 0;
ALTER TABLE groups ADD COLUMN IF NOT EXISTS bank BIGINT NOT NULL DEFAULT 0;
ALTER TABLE groups ADD COLUMN IF NOT EXISTS gang_cop_kills INT NOT NULL DEFAULT 0;
ALTER TABLE groups ADD COLUMN IF NOT EXISTS gang_turfs_taken INT NOT NULL DEFAULT 0;
ALTER TABLE groups ADD COLUMN IF NOT EXISTS gang_turfs_defend INT NOT NULL DEFAULT 0;
ALTER TABLE groups ADD COLUMN IF NOT EXISTS gang_farm_cocaine BIGINT NOT NULL DEFAULT 0;
ALTER TABLE groups ADD COLUMN IF NOT EXISTS gang_farm_weed BIGINT NOT NULL DEFAULT 0;
ALTER TABLE groups ADD COLUMN IF NOT EXISTS gang_farm_medicines BIGINT NOT NULL DEFAULT 0;
ALTER TABLE groups ADD COLUMN IF NOT EXISTS gang_fab_guns BIGINT NOT NULL DEFAULT 0;
ALTER TABLE groups ADD COLUMN IF NOT EXISTS gang_heists INT NOT NULL DEFAULT 0;

-- Actualités utilisées par le navigateur RP historique du CMS.
CREATE TABLE IF NOT EXISTS rdp_news (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT,
    title VARCHAR(160) NOT NULL,
    type ENUM('news','events') NOT NULL DEFAULT 'news',
    image VARCHAR(500) NOT NULL DEFAULT '',
    description VARCHAR(300) NOT NULL DEFAULT '',
    content MEDIUMTEXT NOT NULL,
    author INT NOT NULL DEFAULT 0,
    date INT NOT NULL DEFAULT 0,
    PRIMARY KEY (id),
    KEY rdp_news_type_date (type, date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- La boutique CMS transactionnelle est créée par 002_cms_shop.sql.
