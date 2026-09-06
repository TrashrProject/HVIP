-- ParadiseRP - validation catalogue V4 globale
-- Cette migration ne modifie aucun mobi. Elle materialise seulement des controles
-- simples utilisables aussi manuellement sur le VPS apres deploiement.
SET NAMES utf8mb4;

DROP TABLE IF EXISTS paradise_catalog_v4_validation;
CREATE TABLE paradise_catalog_v4_validation (
    metric VARCHAR(64) NOT NULL PRIMARY KEY,
    metric_value BIGINT NOT NULL,
    checked_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO paradise_catalog_v4_validation(metric, metric_value) VALUES
('root_categories', (SELECT COUNT(*) FROM catalog_pages WHERE parent_id=9967200 AND visible='1' AND enabled='1')),
('detail_categories', (SELECT COUNT(*) FROM catalog_pages WHERE id BETWEEN 9968100 AND 9968199 AND visible='1' AND enabled='1')),
('offers_in_paradise_catalog', (SELECT COUNT(*) FROM catalog_items WHERE page_id BETWEEN 9967201 AND 9968199)),
('largest_detail_page', (SELECT COALESCE(MAX(c),0) FROM (SELECT COUNT(*) c FROM catalog_items WHERE page_id BETWEEN 9968100 AND 9968199 GROUP BY page_id) q)),
('divers_page', (SELECT COUNT(*) FROM catalog_items WHERE page_id=9967220))
ON DUPLICATE KEY UPDATE metric_value=VALUES(metric_value), checked_at=CURRENT_TIMESTAMP;
