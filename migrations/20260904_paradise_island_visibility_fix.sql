-- ParadiseRP - rend le kit ile visible sur les clients ne gerant que deux niveaux.
SET NAMES utf8mb4;
START TRANSACTION;

UPDATE catalog_pages
SET parent_id=9967200, visible='1', enabled='1', order_num=30+(id-9967300)
WHERE id BETWEEN 9967301 AND 9967313;
UPDATE catalog_pages SET visible='0', enabled='0' WHERE id=9967300;

UPDATE catalog_items SET page_id=9967201,catalog_name='Grand Cube noir'
WHERE item_id='5480' AND page_id BETWEEN 9967300 AND 9967399;
UPDATE catalog_items SET page_id=9967201,catalog_name='Petit Cube noir'
WHERE item_id='5466' AND page_id BETWEEN 9967300 AND 9967399;
UPDATE catalog_items SET page_id=9967201,catalog_name='Cube noir compact'
WHERE item_id='996661582' AND page_id BETWEEN 9967300 AND 9967399;

COMMIT;
