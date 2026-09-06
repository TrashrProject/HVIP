-- ParadiseRP V5 - separation fine des familles de blocs (schema legacy)
SET NAMES utf8mb4;
START TRANSACTION;

UPDATE catalog_pages
SET caption_save='Construction - Blocs', caption='Construction - Blocs', visible='1', enabled='1'
WHERE id=9967201;

INSERT INTO catalog_pages
(id,parent_id,caption_save,caption,page_layout,icon_color,icon_image,min_rank,order_num,visible,enabled,club_only,vip_only,page_headline,page_teaser,page_special,page_text1,page_text2,page_text_details,page_text_teaser,room_id,includes)
VALUES
(9968140,9967201,'Blocs de couleur','Blocs de couleur','default_3x3',1,1,1,1,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,''),
(9968141,9967201,'Large Blocks','Large Blocks','default_3x3',1,1,1,2,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,''),
(9968142,9967201,'Cubes','Cubes','default_3x3',1,1,1,3,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,''),
(9968143,9967201,'Wedges','Wedges','default_3x3',1,1,1,4,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,''),
(9968144,9967201,'Pyramides','Pyramides','default_3x3',1,1,1,5,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,''),
(9968145,9967201,'Spheres','Spheres','default_3x3',1,1,1,6,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,''),
(9968146,9967201,'Cylindres','Cylindres','default_3x3',1,1,1,7,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,''),
(9968147,9967201,'Quarter Rings','Quarter Rings','default_3x3',1,1,1,8,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,''),
(9968148,9967201,'Triangular Prisms','Triangular Prisms','default_3x3',1,1,1,9,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,''),
(9968149,9967201,'Panels','Panels','default_3x3',1,1,1,10,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,''),
(9968150,9967201,'Small Blocks','Small Blocks','default_3x3',1,1,1,11,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,''),
(9968151,9967201,'Rings arches et courbes','Rings arches et courbes','default_3x3',1,1,1,12,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,''),
(9968152,9967201,'Autres formes de construction','Autres formes de construction','default_3x3',1,1,1,13,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')
ON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption_save=VALUES(caption_save),caption=VALUES(caption),order_num=VALUES(order_num),visible='1',enabled='1';

UPDATE catalog_items ci
LEFT JOIN items_base ib ON ib.id=CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(ci.item_ids, ',', 1), ':', 1) AS UNSIGNED)
SET ci.page_id=CASE
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'large[ _-]?block|grand bloc|big[ _-]?block' THEN 9968141
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'small[ _-]?block|petit bloc' THEN 9968150
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'cube|cubo' THEN 9968142
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'wedge|coin triangulaire' THEN 9968143
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'pyramid|pyramide' THEN 9968144
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'sphere|spherical|boule' THEN 9968145
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'standing[ _-]?cylinder|cylinder|cylindre' THEN 9968146
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'quarter[ _-]?ring|quart[ _-]?de[ _-]?cercle' THEN 9968147
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'triangular[ _-]?prism|triangle[ _-]?prism|prisme triangulaire' THEN 9968148
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'glass[ _-]?panel|panel|panneau' THEN 9968149
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'ring|arch|arc|curve|curved|courbe' THEN 9968151
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'bc_|building[ _-]?block|construction[ _-]?block|colour[ _-]?block|color[ _-]?block|bloc de construction|bloc de couleur' THEN 9968140
    ELSE 9968152
END
WHERE ci.page_id=9967201;

COMMIT;
