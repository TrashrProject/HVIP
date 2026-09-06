-- ParadiseRP V4 - passe totale moderne
-- Tous les catalog_items, y compris staff/caches/desactives, sont absorbes dans Catalogue ParadiseRP complet.
-- Aucun furniture/items_base n'est supprime ou modifie.
SET NAMES utf8mb4;
START TRANSACTION;

DROP TEMPORARY TABLE IF EXISTS paradise_catalog_all_source_pages;
CREATE TEMPORARY TABLE paradise_catalog_all_source_pages (page_id INT NOT NULL PRIMARY KEY);
INSERT IGNORE INTO paradise_catalog_all_source_pages (page_id)
SELECT DISTINCT page_id
FROM catalog_items
WHERE page_id NOT BETWEEN 9967200 AND 9967224
  AND page_id NOT BETWEEN 9968100 AND 9968199;

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
ELSE 9967220 END;

-- Toute offre sans correspondance items_base est quand meme rendue accessible dans Divers.
UPDATE catalog_items ci
LEFT JOIN items_base ib ON ib.id=ci.item_id
SET ci.page_id=9967220
WHERE ib.id IS NULL;

-- Masque toutes les anciennes pages qui contenaient des offres, y compris staff/cachees.
UPDATE catalog_pages cp
JOIN paradise_catalog_all_source_pages src ON src.page_id=cp.id
SET cp.visible='0', cp.enabled='0';

COMMIT;
