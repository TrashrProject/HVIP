CREATE TABLE IF NOT EXISTS cms_shop_categories (
 id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY, name VARCHAR(80) NOT NULL, slug VARCHAR(90) NOT NULL UNIQUE,
 display_order INT NOT NULL DEFAULT 0, active TINYINT(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
CREATE TABLE IF NOT EXISTS cms_shop_products (
 id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY, category_id INT UNSIGNED NOT NULL, name VARCHAR(120) NOT NULL,
 description VARCHAR(500) NOT NULL DEFAULT '', image_url VARCHAR(500) DEFAULT NULL,
 price INT UNSIGNED NOT NULL, currency ENUM('credits','duckets','diamonds') NOT NULL,
 reward_type ENUM('credits','duckets','diamonds','vip') NOT NULL, reward_amount INT UNSIGNED NOT NULL,
 duration_days INT UNSIGNED NOT NULL DEFAULT 0, badge VARCHAR(30) DEFAULT NULL, featured TINYINT(1) NOT NULL DEFAULT 0,
 active TINYINT(1) NOT NULL DEFAULT 1, stock INT DEFAULT NULL, per_user_limit INT DEFAULT NULL,
 sale_price INT UNSIGNED DEFAULT NULL, sale_start INT DEFAULT NULL, sale_end INT DEFAULT NULL,
 created_at INT NOT NULL, updated_at INT NOT NULL,
 KEY shop_product_category(category_id,active), KEY shop_product_featured(featured,active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
CREATE TABLE IF NOT EXISTS cms_shop_orders (
 id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY, request_token CHAR(32) NOT NULL UNIQUE, user_id INT NOT NULL,
 product_id INT UNSIGNED NOT NULL, product_name VARCHAR(120) NOT NULL, price INT UNSIGNED NOT NULL,
 currency ENUM('credits','duckets','diamonds') NOT NULL, reward_type VARCHAR(30) NOT NULL, reward_amount INT UNSIGNED NOT NULL,
 status ENUM('completed','failed') NOT NULL, failure_reason VARCHAR(160) DEFAULT NULL, created_at INT NOT NULL,
 KEY shop_order_user(user_id,created_at), KEY shop_order_product(product_id,created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
INSERT IGNORE INTO cms_shop_categories(name,slug,display_order,active) VALUES
('A la une','a-la-une',0,1),('VIP','vip',10,1),('Monnaies','monnaies',20,1),('Roleplay','roleplay',30,1);
INSERT INTO cms_shop_products(category_id,name,description,image_url,price,currency,reward_type,reward_amount,duration_days,badge,featured,active,created_at,updated_at)
SELECT id,'VIP 30 jours','Acces au rang VIP WavePlus pendant 30 jours','Dynamics/img/store/vip.png',5,'diamonds','vip',1,30,'VIP',1,1,UNIX_TIMESTAMP(),UNIX_TIMESTAMP() FROM cms_shop_categories
WHERE slug='vip' AND NOT EXISTS(SELECT 1 FROM cms_shop_products WHERE name='VIP 30 jours');
INSERT INTO cms_shop_products(category_id,name,description,image_url,price,currency,reward_type,reward_amount,duration_days,badge,featured,active,created_at,updated_at)
SELECT id,'VIP Plus 30 jours','Acces au second niveau VIP WavePlus pendant 30 jours','Dynamics/img/store/vip.png',10,'diamonds','vip',2,30,'VIP+',0,1,UNIX_TIMESTAMP(),UNIX_TIMESTAMP() FROM cms_shop_categories
WHERE slug='vip' AND NOT EXISTS(SELECT 1 FROM cms_shop_products WHERE name='VIP Plus 30 jours');
