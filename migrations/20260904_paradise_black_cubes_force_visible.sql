-- ParadiseRP - force les trois cubes noirs dans la page visible des blocs.
-- Schema moderne: catalog_items.item_id / offer_active.
SET NAMES utf8mb4;
START TRANSACTION;

UPDATE catalog_pages
SET visible='1', enabled='1'
WHERE id=9967201;

UPDATE catalog_items
SET page_id=9967201, catalog_name='Grand Cube noir', offer_active='1'
WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_id, ',', 1), ':', 1) AS UNSIGNED)=5480;
INSERT INTO catalog_items
    (page_id,item_id,catalog_name,cost_credits,cost_pixels,cost_diamonds,amount,
     limited_sells,limited_stack,offer_active,extradata,badge,offer_id,points_type)
SELECT 9967201,'5480','Grand Cube noir',3,0,0,1,0,0,'1','','',1950499001,0
WHERE NOT EXISTS (
    SELECT 1 FROM catalog_items
    WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_id, ',', 1), ':', 1) AS UNSIGNED)=5480
);

UPDATE catalog_items
SET page_id=9967201, catalog_name='Petit Cube noir', offer_active='1'
WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_id, ',', 1), ':', 1) AS UNSIGNED)=5466;
INSERT INTO catalog_items
    (page_id,item_id,catalog_name,cost_credits,cost_pixels,cost_diamonds,amount,
     limited_sells,limited_stack,offer_active,extradata,badge,offer_id,points_type)
SELECT 9967201,'5466','Petit Cube noir',3,0,0,1,0,0,'1','','',1950499002,0
WHERE NOT EXISTS (
    SELECT 1 FROM catalog_items
    WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_id, ',', 1), ':', 1) AS UNSIGNED)=5466
);

UPDATE catalog_items
SET page_id=9967201, catalog_name='Cube noir compact', offer_active='1'
WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_id, ',', 1), ':', 1) AS UNSIGNED)=996661582;
INSERT INTO catalog_items
    (page_id,item_id,catalog_name,cost_credits,cost_pixels,cost_diamonds,amount,
     limited_sells,limited_stack,offer_active,extradata,badge,offer_id,points_type)
SELECT 9967201,'996661582','Cube noir compact',3,0,0,1,0,0,'1','','',1950499003,0
WHERE NOT EXISTS (
    SELECT 1 FROM catalog_items
    WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_id, ',', 1), ':', 1) AS UNSIGNED)=996661582
);

COMMIT;
