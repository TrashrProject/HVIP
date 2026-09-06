-- ParadiseRP V5 - separation fine des familles de blocs (schema moderne)
SET NAMES utf8mb4;
START TRANSACTION;

UPDATE catalog_pages
SET caption='Construction - Blocs', visible='1', enabled='1'
WHERE id=9967201;

INSERT INTO catalog_pages
(id,parent_id,caption,icon_image,visible,enabled,min_rank,min_vip,order_num,page_link,page_layout,page_strings_1,page_strings_2)
VALUES
(9968140,9967201,'Blocs de couleur',1,'1','1',1,0,1,'','default_3x3','',''),
(9968141,9967201,'Large Blocks',1,'1','1',1,0,2,'','default_3x3','',''),
(9968142,9967201,'Cubes',1,'1','1',1,0,3,'','default_3x3','',''),
(9968143,9967201,'Wedges',1,'1','1',1,0,4,'','default_3x3','',''),
(9968144,9967201,'Pyramides',1,'1','1',1,0,5,'','default_3x3','',''),
(9968145,9967201,'Spheres',1,'1','1',1,0,6,'','default_3x3','',''),
(9968146,9967201,'Cylindres',1,'1','1',1,0,7,'','default_3x3','',''),
(9968147,9967201,'Quarter Rings',1,'1','1',1,0,8,'','default_3x3','',''),
(9968148,9967201,'Triangular Prisms',1,'1','1',1,0,9,'','default_3x3','',''),
(9968149,9967201,'Panels',1,'1','1',1,0,10,'','default_3x3','',''),
(9968150,9967201,'Small Blocks',1,'1','1',1,0,11,'','default_3x3','',''),
(9968151,9967201,'Rings arches et courbes',1,'1','1',1,0,12,'','default_3x3','',''),
(9968152,9967201,'Autres formes de construction',1,'1','1',1,0,13,'','default_3x3','','')
ON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption=VALUES(caption),order_num=VALUES(order_num),visible='1',enabled='1';

-- On ne traite que les offres deja identifiees comme blocs par la V4.
-- Les formes explicites passent avant le fallback "Blocs de couleur".
UPDATE catalog_items ci
LEFT JOIN items_base ib ON ib.id=CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(ci.item_id, ',', 1), ':', 1) AS UNSIGNED)
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
