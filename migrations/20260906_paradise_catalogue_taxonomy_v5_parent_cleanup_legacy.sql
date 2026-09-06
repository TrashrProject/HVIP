-- ParadiseRP V5 legacy - aucun mobi ne doit rester directement dans les gros dossiers
SET NAMES utf8mb4;
START TRANSACTION;

INSERT INTO catalog_pages (id,parent_id,caption_save,caption,page_layout,icon_color,icon_image,min_rank,order_num,visible,enabled,club_only,vip_only,page_headline,page_teaser,page_special,page_text1,page_text2,page_text_details,page_text_teaser,room_id,includes) VALUES
(9968423,9967202,'Autres elements de construction','Autres elements de construction','default_3x3',1,1,1,99,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,''),
(9968424,9967212,'Autres nature et exterieur','Autres nature et exterieur','default_3x3',1,1,1,99,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')
ON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption_save=VALUES(caption_save),caption=VALUES(caption),order_num=VALUES(order_num),visible='1',enabled='1';

UPDATE catalog_items SET page_id=9968423 WHERE page_id=9967202;
UPDATE catalog_items SET page_id=9968424 WHERE page_id=9967212;

COMMIT;
