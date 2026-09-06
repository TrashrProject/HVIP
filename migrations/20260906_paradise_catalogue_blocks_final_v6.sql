-- ParadiseRP V6 - passe finale des familles de blocs (schema moderne)
-- Cette migration doit etre executee APRES toutes les taxonomies V4/V5.
SET NAMES utf8mb4;
START TRANSACTION;

UPDATE catalog_pages
SET visible='1', enabled='1'
WHERE id IN (9967201,9968140,9968141,9968142,9968143,9968144,9968145,9968146,9968147,9968148,9968149,9968150,9968151,9968152);

UPDATE catalog_items ci
LEFT JOIN items_base ib ON ib.id=CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(ci.item_id, ',', 1), ':', 1) AS UNSIGNED)
SET ci.page_id=CASE
    WHEN CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(ci.item_id, ',', 1), ':', 1) AS UNSIGNED)=5480 THEN 9968141
    WHEN CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(ci.item_id, ',', 1), ':', 1) AS UNSIGNED) IN (5466,996661582) THEN 9968150
    WHEN CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(ci.item_id, ',', 1), ':', 1) AS UNSIGNED)=996700070 THEN 9968140
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'large[ _-]?block|grand bloc|big[ _-]?block' THEN 9968141
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'small[ _-]?block|petit bloc' THEN 9968150
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'wedge|coin triangulaire' THEN 9968143
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'pyramid|pyramide' THEN 9968144
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'sphere|spherical|boule' THEN 9968145
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'standing[ _-]?cylinder|cylinder|cylindre' THEN 9968146
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'quarter[ _-]?ring|quart[ _-]?de[ _-]?cercle' THEN 9968147
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'triangular[ _-]?prism|triangle[ _-]?prism|prisme triangulaire' THEN 9968148
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'glass[ _-]?panel|construction panel|building panel' THEN 9968149
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'ring|arch|arc|curve|curved|courbe' THEN 9968151
    WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'cube|cubo' THEN 9968142
    ELSE 9968140
END,
ci.offer_active='1'
WHERE
    CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(ci.item_id, ',', 1), ':', 1) AS UNSIGNED) IN (5480,5466,996661582,996700070)
    OR CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(ci.item_id, ',', 1), ':', 1) AS UNSIGNED) BETWEEN 996661787 AND 996661818
    OR ci.page_id BETWEEN 9968140 AND 9968152
    OR LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'bc_|building[ _-]?block|construction[ _-]?block|colour[ _-]?block|color[ _-]?block|bloc de construction|bloc de couleur|large[ _-]?block|small[ _-]?block|grand bloc|petit bloc|paradise_black_block|bloc noir pur|wedge|quarter[ _-]?ring|triangular[ _-]?prism|standing[ _-]?cylinder';

UPDATE catalog_items SET page_id=9968141,offer_active='1'
WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_id, ',', 1), ':', 1) AS UNSIGNED)=5480;
UPDATE catalog_items SET page_id=9968150,offer_active='1'
WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_id, ',', 1), ':', 1) AS UNSIGNED) IN (5466,996661582);
UPDATE catalog_items SET page_id=9968140,offer_active='1'
WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_id, ',', 1), ':', 1) AS UNSIGNED)=996700070;

COMMIT;
