-- ParadiseRP - force les trois cubes noirs dans la page visible des blocs.
-- Schema legacy WavePlus: catalog_items.item_ids / have_offer.
SET NAMES utf8mb4;
START TRANSACTION;

UPDATE catalog_pages
SET visible='1', enabled='1'
WHERE id=9967201;

UPDATE catalog_items
SET page_id=9967201, catalog_name='Grand Cube noir', order_number=1, have_offer='1'
WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_ids, ',', 1), ':', 1) AS UNSIGNED)=5480;
INSERT INTO catalog_items
    (item_ids,page_id,catalog_name,cost_credits,cost_points,points_type,amount,
     limited_stack,limited_sells,order_number,offer_id,song_id,extradata,have_offer,club_only)
SELECT '5480',9967201,'Grand Cube noir',3,0,0,1,0,0,1,1950499001,0,'','1','0'
WHERE NOT EXISTS (
    SELECT 1 FROM catalog_items
    WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_ids, ',', 1), ':', 1) AS UNSIGNED)=5480
);

UPDATE catalog_items
SET page_id=9967201, catalog_name='Petit Cube noir', order_number=2, have_offer='1'
WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_ids, ',', 1), ':', 1) AS UNSIGNED)=5466;
INSERT INTO catalog_items
    (item_ids,page_id,catalog_name,cost_credits,cost_points,points_type,amount,
     limited_stack,limited_sells,order_number,offer_id,song_id,extradata,have_offer,club_only)
SELECT '5466',9967201,'Petit Cube noir',3,0,0,1,0,0,2,1950499002,0,'','1','0'
WHERE NOT EXISTS (
    SELECT 1 FROM catalog_items
    WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_ids, ',', 1), ':', 1) AS UNSIGNED)=5466
);

UPDATE catalog_items
SET page_id=9967201, catalog_name='Cube noir compact', order_number=3, have_offer='1'
WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_ids, ',', 1), ':', 1) AS UNSIGNED)=996661582;
INSERT INTO catalog_items
    (item_ids,page_id,catalog_name,cost_credits,cost_points,points_type,amount,
     limited_stack,limited_sells,order_number,offer_id,song_id,extradata,have_offer,club_only)
SELECT '996661582',9967201,'Cube noir compact',3,0,0,1,0,0,3,1950499003,0,'','1','0'
WHERE NOT EXISTS (
    SELECT 1 FROM catalog_items
    WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_ids, ',', 1), ':', 1) AS UNSIGNED)=996661582
);

COMMIT;
