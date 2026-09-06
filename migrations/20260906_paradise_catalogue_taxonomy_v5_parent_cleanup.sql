-- ParadiseRP V5 - aucun mobi ne doit rester directement dans les gros dossiers
SET NAMES utf8mb4;
START TRANSACTION;

INSERT INTO catalog_pages (id,parent_id,caption,icon_image,visible,enabled,min_rank,min_vip,order_num,page_link,page_layout,page_strings_1,page_strings_2) VALUES
(9968423,9967202,'Autres elements de construction',1,'1','1',1,0,99,'','default_3x3','',''),
(9968424,9967212,'Autres nature et exterieur',1,'1','1',1,0,99,'','default_3x3','','')
ON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption=VALUES(caption),order_num=VALUES(order_num),visible='1',enabled='1';

UPDATE catalog_items SET page_id=9968423 WHERE page_id=9967202;
UPDATE catalog_items SET page_id=9968424 WHERE page_id=9967212;

COMMIT;
