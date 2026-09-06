-- ParadiseRP - taxonomie globale et idempotente du catalogue
SET NAMES utf8mb4;
START TRANSACTION;

INSERT INTO catalog_pages (id,parent_id,caption,icon_image,visible,enabled,min_rank,min_vip,order_num,page_link,page_layout,page_strings_1,page_strings_2) VALUES
(9967200,-1,'Catalogue ParadiseRP complet',1,'1','1',1,0,90,'','default_3x3','',''),
(9967201,9967200,'Construction - Blocs et formes',1,'1','1',1,0,1,'','default_3x3','',''),
(9967209,9967200,'RP - Police justice et armee',1,'1','1',1,0,2,'','default_3x3','',''),
(9967210,9967200,'RP - Hopital secours et incendie',1,'1','1',1,0,3,'','default_3x3','',''),
(9967211,9967200,'RP - Transports et garages',1,'1','1',1,0,4,'','default_3x3','',''),
(9967208,9967200,'RP - Commerces restaurants et entreprises',1,'1','1',1,0,5,'','default_3x3','',''),
(9967207,9967200,'RP - Ville et services publics',1,'1','1',1,0,6,'','default_3x3','',''),
(9967204,9967200,'Maison - Cuisine nourriture et boissons',1,'1','1',1,0,7,'','default_3x3','',''),
(9967205,9967200,'Maison - Chambre salle de bain',1,'1','1',1,0,8,'','default_3x3','',''),
(9967215,9967200,'Nature - Animaux et compagnons',1,'1','1',1,0,9,'','default_3x3','',''),
(9967212,9967200,'Nature eau jardins et exterieurs',1,'1','1',1,0,10,'','default_3x3','',''),
(9967213,9967200,'Bureau technologie et machines',1,'1','1',1,0,11,'','default_3x3','',''),
(9967214,9967200,'Loisirs jeux sport et musique',1,'1','1',1,0,12,'','default_3x3','',''),
(9967216,9967200,'Saisons - Noel hiver et neige',1,'1','1',1,0,13,'','default_3x3','',''),
(9967217,9967200,'Halloween fantastique et aventure',1,'1','1',1,0,14,'','default_3x3','',''),
(9967218,9967200,'Fetes Paques amour et evenements',1,'1','1',1,0,15,'','default_3x3','',''),
(9967222,9967200,'Mode beaute et accessoires',1,'1','1',1,0,16,'','default_3x3','',''),
(9967224,9967200,'Panneaux lettres et signaletique',1,'1','1',1,0,17,'','default_3x3','',''),
(9967223,9967200,'Art cultures et histoire',1,'1','1',1,0,18,'','default_3x3','',''),
(9967206,9967200,'Decoration lampes et accessoires',1,'1','1',1,0,19,'','default_3x3','',''),
(9967203,9967200,'Maison - Salon sieges et mobilier',1,'1','1',1,0,20,'','default_3x3','',''),
(9967202,9967200,'Construction - Murs sols portes et toits',1,'1','1',1,0,21,'','default_3x3','',''),
(9967219,9967200,'Rares LTD et collections',1,'1','1',1,0,22,'','default_3x3','',''),
(9967221,9967200,'Creations custom exclusives',1,'1','1',1,0,23,'','default_3x3','',''),
(9967220,9967200,'Classiques et objets divers',1,'1','1',1,0,24,'','default_3x3','','')
ON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption=VALUES(caption),order_num=VALUES(order_num),visible='1',enabled='1';

UPDATE catalog_items ci
JOIN items_base ib ON ib.id=ci.item_id
SET ci.page_id=CASE
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'bc_|bloc de construction|building block|construction block|colour block|color block|cube|cubo|wedge|pyramid|sphere|quarter ring|triangular prism|standing cylinder|glass panel|flower hedge' THEN 9967201
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'police|prison|jail|prison cell|jail cell|cell door|cell bars|army|military|soldier|weapon|gun|rifle|pistol|sword|shield|security|detective|court|justice|swat|crime|criminal|handcuff|cctv|evidence|interrogation' THEN 9967209
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'hospital|medical|medic|doctor|nurse|ambulance|health|clinic|surgery|patient|firefighter|fire station|firetruck|rescue|pharmacy|first aid|emergency|medicine|defibrillator|stretcher|wheelchair|xray|x-ray|blood bag|syringe' THEN 9967210
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'vehicle|garage|parking|traffic|road|street|car_| car |taxi|bus_| bus |train|subway|metro|tram|truck|motor|bike|bicycle|scooter|airport|plane|aircraft|boat|ship|yacht' THEN 9967211
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'shop|store|market|restaurant|cafe|diner|bar_|pub_|office|business|commerce|bank|hotel|reception|cashier|counter|salon|boutique|bakery|supermarket|factory|warehouse' THEN 9967208
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'city|urban|public|school|university|library|museum|station|post office|town|building|skyscraper|streetlight|bus stop|construction site' THEN 9967207
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'kitchen|food|drink|meal|cake|coffee|tea_|teapot|bottle|pizza|burger|fruit|vegetable|snack|dinner|breakfast|icecream|ice cream|candy|restaurant|oven|fridge|refrigerator|sink|plate|cup|glass|barbecue|bbq' THEN 9967204
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'bedroom|bed_| bed |pillow|blanket|wardrobe|dresser|nightstand|bath|toilet|shower|bathtub|washbasin|towel|sleep|mattress' THEN 9967205
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'animal|pet_|dog|cat_| cat |horse|pony|bird|parrot|penguin|duck|frog|fish|shark|whale|dolphin|turtle|rabbit|bunny|bear|monkey|gorilla|lion|tiger|elephant|deer|unicorn|dragon|dinosaur' THEN 9967215
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'nature|garden|plant|tree|flower|grass|soil|crop|farm|forest|bush|rock|stone|water|waterfall|fountain|pond|river|ocean|sea_|beach|sand|coral|pool|outdoor|camp|mountain|cloud' THEN 9967212
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'computer|laptop|phone|television|monitor|screen|console|machine|robot|camera|speaker|radio|science|laboratory|office|printer|keyboard|tech|spaceship|rocket|satellite' THEN 9967213
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'game_|arcade|gaming|football|fball|soccer|basketball|tennis|sport|trophy|dance|disco|music|guitar|piano|drum|turntable|microphone|cinema|theatre|hobby|craft|dice|chess|skate' THEN 9967214
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'xmas|christmas|santa|advent|winter|snow|ice_|frost|festive|reindeer|nutcracker|mistletoe' THEN 9967216
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'hween|halloween|horror|ghost|spooky|haunted|witch|vampire|zombie|skull|fantasy|magic|wizard|myth|olympus|wonderland|dream|quest|temple|ancient|dungeon' THEN 9967217
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'easter|valentine|love|wedding|party|birthday|celebration|festival|gift|present|balloon|heart|egg_|carnival' THEN 9967218
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'fashion|clothing|clothes|hair|beauty|makeup|jewelry|jewellery|dress|shirt|jacket|shoe|hat_| hat |accessory|perfume|mannequin' THEN 9967222
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'sign|poster|letter|number|alphabet|billboard|banner|road sign|traffic sign|placard|notice|logo|flag' THEN 9967224
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'painting|picture|art_| art |statue|sculpture|museum|historic|history|culture|easel|portrait|canvas|gallery|antique' THEN 9967223
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'lamp|light|lantern|candle|torch|chandelier|neon|fireplace|rug|carpet|curtain|vase|ornament|decoration|mirror|clock|plush|doll|toy|plant pot' THEN 9967206
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'chair|sofa|couch|seat|stool|bench|armchair|table|desk|cabinet|shelf|bookcase|drawer|furniture|hammock|throne' THEN 9967203
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'wall|floor|tile|roof|door|window|pillar|column|stair|ramp|bridge|divider|gate|fence|rail|platform|architecture|construction|brick|panel|background' THEN 9967202
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'rare|diamond|ltd|limited|collectible|collection|golden|silver|bronze|trophy|prize|relic|nft_' THEN 9967219
        WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'custom|paradise|waveplus|waverp|habborpbr|exclusive' THEN 9967221
        ELSE 9967220
    END
WHERE ci.page_id BETWEEN 9967000 AND 9967839;

-- Les rares offres sans definition items_base restent accessibles dans Divers.
UPDATE catalog_items
SET page_id=9967220
WHERE page_id BETWEEN 9967000 AND 9967839
  AND page_id NOT BETWEEN 9967200 AND 9967224;

UPDATE catalog_pages
SET visible='0',enabled='0'
WHERE id BETWEEN 9967000 AND 9967839
  AND id NOT BETWEEN 9967200 AND 9967224;

COMMIT;
