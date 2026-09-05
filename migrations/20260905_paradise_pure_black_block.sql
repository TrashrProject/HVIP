-- ParadiseRP - vrai cube noir pur, ressource autonome sans variante grise.
-- Schema moderne: furniture + catalog_items.item_id / offer_active.
SET NAMES utf8mb4;
START TRANSACTION;

UPDATE furniture SET public_name='Grand Bloc gris' WHERE id=5480 AND item_name='bc_block_1*13';
UPDATE furniture SET public_name='Petit Bloc gris' WHERE id=5466 AND item_name='bc_block_0*13';
UPDATE catalog_items SET catalog_name='Grand Bloc gris'
WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_id, ',', 1), ':', 1) AS UNSIGNED)=5480;
UPDATE catalog_items SET catalog_name='Petit Bloc gris'
WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_id, ',', 1), ':', 1) AS UNSIGNED)=5466;

INSERT INTO furniture
 (id,item_name,public_name,type,width,length,stack_height,can_stack,can_sit,is_walkable,
  sprite_id,allow_recycle,allow_trade,allow_marketplace_sell,allow_gift,allow_inventory_stack,
  interaction_type,behaviour_data,interaction_modes_count,vending_ids,height_adjustable,
  effect_id,wired_id,is_rare,clothing_id,extra_rot,allow_lay)
VALUES
 (996700070,'paradise_black_block*1','Bloc noir pur','s',1,1,1,1,0,1,
  996700070,'1','1','1','1','1','default',0,1,'0','0',
  0,0,'0',0,'0',0)
ON DUPLICATE KEY UPDATE
 item_name=VALUES(item_name),public_name=VALUES(public_name),sprite_id=VALUES(sprite_id),
 width=1,length=1,stack_height=1,can_stack=1,is_walkable=1;

UPDATE catalog_pages SET visible='1',enabled='1' WHERE id=9967201;
UPDATE catalog_items
SET page_id=9967201,catalog_name='Bloc noir pur',cost_credits=3,amount=1,offer_active='1'
WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_id, ',', 1), ':', 1) AS UNSIGNED)=996700070;
INSERT INTO catalog_items
 (page_id,item_id,catalog_name,cost_credits,cost_pixels,cost_diamonds,amount,
  limited_sells,limited_stack,offer_active,extradata,badge,offer_id,points_type)
SELECT 9967201,'996700070','Bloc noir pur',3,0,0,1,0,0,'1','','',1950499070,0
WHERE NOT EXISTS (
 SELECT 1 FROM catalog_items
 WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_id, ',', 1), ':', 1) AS UNSIGNED)=996700070
);

COMMIT;
