-- ParadiseRP - catalogue V4 global
-- But: fusionner les pages catalogue joueur existantes avec les ajouts Paradise,
-- puis reclasser les offres par type de mobi dans des pages plus courtes.
-- Les definitions furniture/items_base ne sont jamais modifiees ici.
SET NAMES utf8mb4;
START TRANSACTION;

DROP TEMPORARY TABLE IF EXISTS paradise_catalog_source_pages;
CREATE TEMPORARY TABLE paradise_catalog_source_pages (page_id INT NOT NULL PRIMARY KEY);
INSERT IGNORE INTO paradise_catalog_source_pages (page_id)
SELECT DISTINCT ci.page_id
FROM catalog_items ci
JOIN catalog_pages cp ON cp.id=ci.page_id
WHERE cp.visible='1' AND cp.enabled='1' AND cp.min_rank<=1
  AND ci.page_id NOT BETWEEN 9967200 AND 9967224
  AND ci.page_id NOT BETWEEN 9968100 AND 9968199;

INSERT INTO catalog_pages (id,parent_id,caption,icon_image,visible,enabled,min_rank,min_vip,order_num,page_link,page_layout,page_strings_1,page_strings_2) VALUES
(9967200,-1,'Catalogue ParadiseRP complet',1,'1','1',1,0,90,'','default_3x3','',''),
(9967201,9967200,'Construction - Blocs et formes',1,'1','1',1,0,1,'','default_3x3','',''),
(9967202,9967200,'Construction - Batiment',1,'1','1',1,0,2,'','default_3x3','',''),
(9967203,9967200,'Maison - Salon et mobilier',1,'1','1',1,0,3,'','default_3x3','',''),
(9967204,9967200,'Cuisine - Nourriture et boissons',1,'1','1',1,0,4,'','default_3x3','',''),
(9967205,9967200,'Maison - Chambre et bain',1,'1','1',1,0,5,'','default_3x3','',''),
(9967206,9967200,'Decoration et eclairage',1,'1','1',1,0,6,'','default_3x3','',''),
(9967207,9967200,'Ville et services publics',1,'1','1',1,0,7,'','default_3x3','',''),
(9967208,9967200,'Commerces et entreprises',1,'1','1',1,0,8,'','default_3x3','',''),
(9967209,9967200,'RP - Police justice et armee',1,'1','1',1,0,9,'','default_3x3','',''),
(9967210,9967200,'RP - Hopital secours incendie',1,'1','1',1,0,10,'','default_3x3','',''),
(9967211,9967200,'Transports et garages',1,'1','1',1,0,11,'','default_3x3','',''),
(9967212,9967200,'Nature eau et exterieurs',1,'1','1',1,0,12,'','default_3x3','',''),
(9967213,9967200,'Bureau technologie et machines',1,'1','1',1,0,13,'','default_3x3','',''),
(9967214,9967200,'Loisirs jeux sport et musique',1,'1','1',1,0,14,'','default_3x3','',''),
(9967215,9967200,'Animaux et compagnons',1,'1','1',1,0,15,'','default_3x3','',''),
(9967216,9967200,'Noel hiver et neige',1,'1','1',1,0,16,'','default_3x3','',''),
(9967217,9967200,'Halloween fantastique aventure',1,'1','1',1,0,17,'','default_3x3','',''),
(9967218,9967200,'Paques amour et evenements',1,'1','1',1,0,18,'','default_3x3','',''),
(9967219,9967200,'Rares LTD et collections',1,'1','1',1,0,19,'','default_3x3','',''),
(9967220,9967200,'Classiques et objets divers',1,'1','1',1,0,20,'','default_3x3','',''),
(9967221,9967200,'Creations custom exclusives',1,'1','1',1,0,21,'','default_3x3','',''),
(9967222,9967200,'Mode beaute et accessoires',1,'1','1',1,0,22,'','default_3x3','',''),
(9967223,9967200,'Art cultures et histoire',1,'1','1',1,0,23,'','default_3x3','',''),
(9967224,9967200,'Panneaux lettres signaletique',1,'1','1',1,0,24,'','default_3x3','',''),
(9968101,9967202,'Murs et cloisons',1,'1','1',1,0,1,'','default_3x3','',''),
(9968102,9967202,'Sols et plateformes',1,'1','1',1,0,2,'','default_3x3','',''),
(9968103,9967202,'Portes fenetres portails',1,'1','1',1,0,3,'','default_3x3','',''),
(9968104,9967202,'Escaliers rampes barrieres',1,'1','1',1,0,4,'','default_3x3','',''),
(9968105,9967202,'Toits colonnes architecture',1,'1','1',1,0,5,'','default_3x3','',''),
(9968110,9967212,'Eau - Puits',1,'1','1',1,0,1,'','default_3x3','',''),
(9968111,9967212,'Eau - Fontaines cascades',1,'1','1',1,0,2,'','default_3x3','',''),
(9968112,9967212,'Eau - Piscines bassins',1,'1','1',1,0,3,'','default_3x3','',''),
(9968113,9967212,'Eau - Mer plage aquatique',1,'1','1',1,0,4,'','default_3x3','',''),
(9968114,9967212,'Nature - Arbres plantes',1,'1','1',1,0,5,'','default_3x3','',''),
(9968115,9967212,'Nature - Fleurs jardins',1,'1','1',1,0,6,'','default_3x3','',''),
(9968116,9967212,'Nature - Roches terrain montagne',1,'1','1',1,0,7,'','default_3x3','',''),
(9968117,9967212,'Exterieur - Camping loisirs',1,'1','1',1,0,8,'','default_3x3','',''),
(9968120,9967203,'Canapes fauteuils sieges',1,'1','1',1,0,1,'','default_3x3','',''),
(9968121,9967203,'Tables bureaux',1,'1','1',1,0,2,'','default_3x3','',''),
(9968122,9967203,'Rangements et etageres',1,'1','1',1,0,3,'','default_3x3','',''),
(9968123,9967205,'Lits et chambre',1,'1','1',1,0,1,'','default_3x3','',''),
(9968124,9967205,'Salle de bain',1,'1','1',1,0,2,'','default_3x3','',''),
(9968125,9967204,'Electromenager cuisine',1,'1','1',1,0,1,'','default_3x3','',''),
(9968126,9967204,'Nourriture',1,'1','1',1,0,2,'','default_3x3','',''),
(9968127,9967204,'Boissons et bar',1,'1','1',1,0,3,'','default_3x3','',''),
(9968128,9967206,'Lampes et eclairage',1,'1','1',1,0,1,'','default_3x3','',''),
(9968129,9967206,'Tapis rideaux miroirs',1,'1','1',1,0,2,'','default_3x3','',''),
(9968130,9967213,'Ordinateurs ecrans technologie',1,'1','1',1,0,1,'','default_3x3','',''),
(9968131,9967213,'Machines science',1,'1','1',1,0,2,'','default_3x3','',''),
(9968132,9967211,'Vehicules',1,'1','1',1,0,1,'','default_3x3','',''),
(9968133,9967211,'Routes parking signalisation',1,'1','1',1,0,2,'','default_3x3','',''),
(9968134,9967214,'Sport',1,'1','1',1,0,1,'','default_3x3','',''),
(9968135,9967214,'Jeux arcade',1,'1','1',1,0,2,'','default_3x3','',''),
(9968136,9967214,'Musique danse cinema',1,'1','1',1,0,3,'','default_3x3','',''),
(9968137,9967209,'Armee et armes',1,'1','1',1,0,1,'','default_3x3','',''),
(9968138,9967210,'Pompiers et secours',1,'1','1',1,0,1,'','default_3x3','','')
ON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption=VALUES(caption),order_num=VALUES(order_num),visible='1',enabled='1';

UPDATE catalog_items ci
JOIN items_base ib ON ib.id=ci.item_id
SET ci.page_id=CASE
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'bc_|building block|construction block|colour block|color block|bloc de construction|cube|cubo|wedge|pyramid|sphere|quarter ring|triangular prism|standing cylinder' THEN 9967201
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP '(^|[^a-z])well([^a-z]|$)|puits|wishing well' THEN 9968110
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'fountain|waterfall|cascade|geyser' THEN 9968111
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'pool|pond|basin|jacuzzi|hot tub' THEN 9968112
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'river|ocean|sea_|beach|sand|coral|aquatic|underwater|(^|[^a-z])water([^a-z]|$)' THEN 9968113
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'tree|plant|bush|hedge|forest|bonsai|palm' THEN 9968114
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'flower|garden|grass|soil|crop|farm|seed|planter' THEN 9968115
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'rock|stone|mountain|cliff|terrain|cave' THEN 9968116
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'outdoor|camp|tent|campfire|hammock|picnic' THEN 9968117
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'wall|divider|partition|brick wall|glass panel|panel wall' THEN 9968101
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'floor|tile|platform|stage|decking|pavement|paving' THEN 9968102
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'door|window|gate|garage door|shutter' THEN 9968103
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'stair|step|ramp|rail|fence|barrier|banister|bridge' THEN 9968104
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'roof|pillar|column|architecture|facade|skyscraper' THEN 9968105
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'bath|toilet|shower|bathtub|washbasin|towel|bathroom' THEN 9968124
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'bedroom|bed_| bed |pillow|blanket|nightstand|sleep|mattress' THEN 9968123
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'kitchen|oven|fridge|refrigerator|freezer|microwave|stove|cooker|kitchen sink' THEN 9968125
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'drink|coffee|tea_|teapot|bottle|cup|glass|bar_|pub_|barbecue|bbq' THEN 9968127
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'food|meal|cake|pizza|burger|fruit|vegetable|snack|dinner|breakfast|icecream|ice cream|candy|bakery' THEN 9968126
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'sofa|couch|armchair|chair|seat|stool|bench|throne' THEN 9968120
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'table|desk|worktop' THEN 9968121
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'cabinet|shelf|bookcase|drawer|dresser|wardrobe|cupboard|storage' THEN 9968122
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'lamp|light|lantern|candle|torch|chandelier|neon|fireplace' THEN 9968128
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'rug|carpet|curtain|mirror|vase|ornament|decoration|clock|plush|doll|toy' THEN 9968129
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'computer|laptop|television|monitor|screen|console|printer|keyboard|camera|speaker|radio|phone' THEN 9968130
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'machine|robot|science|laboratory|tech|spaceship|rocket|satellite' THEN 9968131
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'army|military|soldier|weapon|gun|rifle|pistol|sword|shield' THEN 9968137
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'police|prison|jail|cell door|cell bars|security|detective|court|justice|swat|crime|criminal|handcuff|cctv|evidence|interrogation' THEN 9967209
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'firefighter|fire station|firetruck|rescue|first aid|emergency' THEN 9968138
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'hospital|medical|medic|doctor|nurse|ambulance|health|clinic|surgery|patient|pharmacy|medicine|defibrillator|stretcher|wheelchair|xray|x-ray|blood bag|syringe' THEN 9967210
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'garage|parking|traffic|road|street|airport|station|road sign|traffic sign' THEN 9968133
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'vehicle|car_| car |taxi|bus_| bus |train|subway|metro|tram|truck|motor|bike|bicycle|scooter|plane|aircraft|boat|ship|yacht' THEN 9968132
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'shop|store|market|restaurant|cafe|diner|office|business|commerce|bank|hotel|reception|cashier|counter|salon|boutique|supermarket|factory|warehouse' THEN 9967208
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'city|urban|public|school|university|library|museum|post office|town|streetlight|bus stop|construction site' THEN 9967207
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'football|fball|soccer|basketball|tennis|sport|skate|gym|boxing' THEN 9968134
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'game_|arcade|gaming|dice|chess|hobby|craft' THEN 9968135
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'dance|disco|music|guitar|piano|drum|turntable|microphone|cinema|theatre' THEN 9968136
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'animal|pet_|dog|cat_| cat |horse|pony|bird|parrot|penguin|duck|frog|fish|shark|whale|dolphin|turtle|rabbit|bunny|bear|monkey|gorilla|lion|tiger|elephant|deer|unicorn|dragon|dinosaur' THEN 9967215
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'xmas|christmas|santa|advent|winter|snow|ice_|frost|festive|reindeer|nutcracker|mistletoe' THEN 9967216
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'hween|halloween|horror|ghost|spooky|haunted|witch|vampire|zombie|skull|fantasy|magic|wizard|myth|olympus|wonderland|dream|quest|temple|ancient|dungeon' THEN 9967217
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'easter|valentine|love|wedding|party|birthday|celebration|festival|gift|present|balloon|heart|egg_|carnival' THEN 9967218
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'fashion|clothing|clothes|hair|beauty|makeup|jewelry|jewellery|dress|shirt|jacket|shoe|hat_| hat |accessory|perfume|mannequin' THEN 9967222
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'sign|poster|letter|number|alphabet|billboard|banner|placard|notice|logo|flag' THEN 9967224
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'painting|picture|art_| art |statue|sculpture|historic|history|culture|easel|portrait|canvas|gallery|antique' THEN 9967223
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'rare|diamond|ltd|limited|collectible|collection|golden|silver|bronze|trophy|prize|relic|nft_' THEN 9967219
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'custom|paradise|waveplus|waverp|habborpbr|exclusive' THEN 9967221
ELSE 9967220 END
WHERE ci.page_id IN (SELECT page_id FROM paradise_catalog_source_pages)
   OR ci.page_id BETWEEN 9967200 AND 9967224
   OR ci.page_id BETWEEN 9968100 AND 9968199;

-- Offres issues d'anciennes pages mais sans definition items_base : on les garde visibles.
UPDATE catalog_items SET page_id=9967220
WHERE page_id IN (SELECT page_id FROM paradise_catalog_source_pages);

-- Une fois les offres fusionnees, les anciennes pages source sont masquees.
UPDATE catalog_pages cp
JOIN paradise_catalog_source_pages src ON src.page_id=cp.id
SET cp.visible='0',cp.enabled='0';

-- Nettoyage des anciennes pages Paradise V2/V3 devenues inutiles.
UPDATE catalog_pages SET visible='0',enabled='0'
WHERE id BETWEEN 9967000 AND 9967839
  AND id NOT BETWEEN 9967200 AND 9967224;

COMMIT;
