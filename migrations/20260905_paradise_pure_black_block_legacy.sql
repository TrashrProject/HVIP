-- ParadiseRP - vrai cube noir pur, ressource autonome sans variante grise.
-- Schema WavePlus: items_base + catalog_items.item_ids / have_offer.
SET NAMES utf8mb4;
START TRANSACTION;

UPDATE items_base SET public_name='Grand Bloc gris' WHERE id=5480 AND item_name='bc_block_1*13';
UPDATE items_base SET public_name='Petit Bloc gris' WHERE id=5466 AND item_name='bc_block_0*13';
UPDATE catalog_items SET catalog_name='Grand Bloc gris'
WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_ids, ',', 1), ':', 1) AS UNSIGNED)=5480;
UPDATE catalog_items SET catalog_name='Petit Bloc gris'
WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_ids, ',', 1), ':', 1) AS UNSIGNED)=5466;

INSERT INTO items_base
 (id,sprite_id,public_name,item_name,type,width,length,stack_height,
  allow_stack,allow_sit,allow_lay,allow_walk,allow_gift,allow_trade,
  allow_recycle,allow_marketplace_sell,allow_inventory_stack,
  interaction_type,interaction_modes_count,vending_ids,multiheight,
  customparams,effect_id_male,effect_id_female,clothing_on_walk)
VALUES
 (996700070,996700070,'Bloc noir pur','paradise_black_block*1','s',1,1,1,
  '1','0','0','1','1','1','0','1','1',
  'default',1,'0','0','',0,0,'')
ON DUPLICATE KEY UPDATE
 item_name=VALUES(item_name),public_name=VALUES(public_name),sprite_id=VALUES(sprite_id),
 width=1,length=1,stack_height=1,allow_stack='1',allow_walk='1';

UPDATE catalog_pages SET visible='1',enabled='1' WHERE id=9967201;
UPDATE catalog_items
SET page_id=9967201,catalog_name='Bloc noir pur',cost_credits=3,amount=1,
    order_number=0,club_only='0',have_offer='1'
WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_ids, ',', 1), ':', 1) AS UNSIGNED)=996700070;
INSERT INTO catalog_items
 (item_ids,page_id,catalog_name,cost_credits,cost_points,points_type,amount,
  limited_stack,limited_sells,order_number,offer_id,song_id,extradata,have_offer,club_only)
SELECT '996700070',9967201,'Bloc noir pur',3,0,0,1,0,0,0,1950499070,0,'','1','0'
WHERE NOT EXISTS (
 SELECT 1 FROM catalog_items
 WHERE CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(item_ids, ',', 1), ':', 1) AS UNSIGNED)=996700070
);

COMMIT;
