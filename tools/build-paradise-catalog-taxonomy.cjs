#!/usr/bin/env node
'use strict';

const fs = require('fs');
const path = require('path');

const root = path.resolve(__dirname, '..');
const pages = [
    [9967201, 'Construction - Blocs et formes', 'bc_|bloc de construction|building block|construction block|colour block|color block|cube|cubo|wedge|pyramid|sphere|quarter ring|triangular prism|standing cylinder|glass panel|flower hedge'],
    [9967209, 'RP - Police justice et armee', 'police|prison|jail|prison cell|jail cell|cell door|cell bars|army|military|soldier|weapon|gun|rifle|pistol|sword|shield|security|detective|court|justice|swat|crime|criminal|handcuff|cctv|evidence|interrogation'],
    [9967210, 'RP - Hopital secours et incendie', 'hospital|medical|medic|doctor|nurse|ambulance|health|clinic|surgery|patient|firefighter|fire station|firetruck|rescue|pharmacy|first aid|emergency|medicine|defibrillator|stretcher|wheelchair|xray|x-ray|blood bag|syringe'],
    [9967211, 'RP - Transports et garages', 'vehicle|garage|parking|traffic|road|street|car_| car |taxi|bus_| bus |train|subway|metro|tram|truck|motor|bike|bicycle|scooter|airport|plane|aircraft|boat|ship|yacht'],
    [9967208, 'RP - Commerces restaurants et entreprises', 'shop|store|market|restaurant|cafe|diner|bar_|pub_|office|business|commerce|bank|hotel|reception|cashier|counter|salon|boutique|bakery|supermarket|factory|warehouse'],
    [9967207, 'RP - Ville et services publics', 'city|urban|public|school|university|library|museum|station|post office|town|building|skyscraper|streetlight|bus stop|construction site'],
    [9967204, 'Maison - Cuisine nourriture et boissons', 'kitchen|food|drink|meal|cake|coffee|tea_|teapot|bottle|pizza|burger|fruit|vegetable|snack|dinner|breakfast|icecream|ice cream|candy|restaurant|oven|fridge|refrigerator|sink|plate|cup|glass|barbecue|bbq'],
    [9967205, 'Maison - Chambre salle de bain', 'bedroom|bed_| bed |pillow|blanket|wardrobe|dresser|nightstand|bath|toilet|shower|bathtub|washbasin|towel|sleep|mattress'],
    [9967215, 'Nature - Animaux et compagnons', 'animal|pet_|dog|cat_| cat |horse|pony|bird|parrot|penguin|duck|frog|fish|shark|whale|dolphin|turtle|rabbit|bunny|bear|monkey|gorilla|lion|tiger|elephant|deer|unicorn|dragon|dinosaur'],
    [9967212, 'Nature eau jardins et exterieurs', 'nature|garden|plant|tree|flower|grass|soil|crop|farm|forest|bush|rock|stone|water|waterfall|fountain|pond|river|ocean|sea_|beach|sand|coral|pool|outdoor|camp|mountain|cloud'],
    [9967213, 'Bureau technologie et machines', 'computer|laptop|phone|television|monitor|screen|console|machine|robot|camera|speaker|radio|science|laboratory|office|printer|keyboard|tech|spaceship|rocket|satellite'],
    [9967214, 'Loisirs jeux sport et musique', 'game_|arcade|gaming|football|fball|soccer|basketball|tennis|sport|trophy|dance|disco|music|guitar|piano|drum|turntable|microphone|cinema|theatre|hobby|craft|dice|chess|skate'],
    [9967216, 'Saisons - Noel hiver et neige', 'xmas|christmas|santa|advent|winter|snow|ice_|frost|festive|reindeer|nutcracker|mistletoe'],
    [9967217, 'Halloween fantastique et aventure', 'hween|halloween|horror|ghost|spooky|haunted|witch|vampire|zombie|skull|fantasy|magic|wizard|myth|olympus|wonderland|dream|quest|temple|ancient|dungeon'],
    [9967218, 'Fetes Paques amour et evenements', 'easter|valentine|love|wedding|party|birthday|celebration|festival|gift|present|balloon|heart|egg_|carnival'],
    [9967222, 'Mode beaute et accessoires', 'fashion|clothing|clothes|hair|beauty|makeup|jewelry|jewellery|dress|shirt|jacket|shoe|hat_| hat |accessory|perfume|mannequin'],
    [9967224, 'Panneaux lettres et signaletique', 'sign|poster|letter|number|alphabet|billboard|banner|road sign|traffic sign|placard|notice|logo|flag'],
    [9967223, 'Art cultures et histoire', 'painting|picture|art_| art |statue|sculpture|museum|historic|history|culture|easel|portrait|canvas|gallery|antique'],
    [9967206, 'Decoration lampes et accessoires', 'lamp|light|lantern|candle|torch|chandelier|neon|fireplace|rug|carpet|curtain|vase|ornament|decoration|mirror|clock|plush|doll|toy|plant pot'],
    [9967203, 'Maison - Salon sieges et mobilier', 'chair|sofa|couch|seat|stool|bench|armchair|table|desk|cabinet|shelf|bookcase|drawer|furniture|hammock|throne'],
    [9967202, 'Construction - Murs sols portes et toits', 'wall|floor|tile|roof|door|window|pillar|column|stair|ramp|bridge|divider|gate|fence|rail|platform|architecture|construction|brick|panel|background'],
    [9967219, 'Rares LTD et collections', 'rare|diamond|ltd|limited|collectible|collection|golden|silver|bronze|trophy|prize|relic|nft_'],
    [9967221, 'Creations custom exclusives', 'custom|paradise|waveplus|waverp|habborpbr|exclusive'],
    [9967220, 'Classiques et objets divers', '.*']
];

const esc = value => value.replace(/'/g, "''");
const expression = "LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name))";
const caseSql = pages.map(([id, , pattern], index) => index === pages.length - 1
    ? `        ELSE ${ id }`
    : `        WHEN ${ expression } REGEXP '${ esc(pattern) }' THEN ${ id }`).join('\n');

function pageRows(legacy) {
    const rootRow = legacy
        ? "(9967200,-1,'Catalogue ParadiseRP complet','Catalogue ParadiseRP complet','default_3x3',1,1,1,90,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')"
        : "(9967200,-1,'Catalogue ParadiseRP complet',1,'1','1',1,0,90,'','default_3x3','','')";
    const children = pages.map(([id, caption], index) => legacy
        ? `(${ id },9967200,'${ esc(caption) }','${ esc(caption) }','default_3x3',1,1,1,${ index + 1 },'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')`
        : `(${ id },9967200,'${ esc(caption) }',1,'1','1',1,0,${ index + 1 },'','default_3x3','','')`);
    return [rootRow, ...children];
}

function build(legacy) {
    const columns = legacy
        ? 'id,parent_id,caption_save,caption,page_layout,icon_color,icon_image,min_rank,order_num,visible,enabled,club_only,vip_only,page_headline,page_teaser,page_special,page_text1,page_text2,page_text_details,page_text_teaser,room_id,includes'
        : 'id,parent_id,caption,icon_image,visible,enabled,min_rank,min_vip,order_num,page_link,page_layout,page_strings_1,page_strings_2';
    const updateCaption = legacy ? 'caption_save=VALUES(caption_save),caption=VALUES(caption)' : 'caption=VALUES(caption)';
    const joinId = legacy
        ? "CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(ci.item_ids, ',', 1), ':', 1) AS UNSIGNED)"
        : 'ci.item_id';
    return `-- ParadiseRP - taxonomie globale et idempotente du catalogue\nSET NAMES utf8mb4;\nSTART TRANSACTION;\n\nINSERT INTO catalog_pages (${ columns }) VALUES\n${ pageRows(legacy).join(',\n') }\nON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),${ updateCaption },order_num=VALUES(order_num),visible='1',enabled='1';\n\nUPDATE catalog_items ci\nJOIN items_base ib ON ib.id=${ joinId }\nSET ci.page_id=CASE\n${ caseSql }\n    END\nWHERE ci.page_id BETWEEN 9967000 AND 9967839;\n\n-- Les rares offres sans definition items_base restent accessibles dans Divers.\nUPDATE catalog_items\nSET page_id=9967220\nWHERE page_id BETWEEN 9967000 AND 9967839\n  AND page_id NOT BETWEEN 9967200 AND 9967224;\n\nUPDATE catalog_pages\nSET visible='0',enabled='0'\nWHERE id BETWEEN 9967000 AND 9967839\n  AND id NOT BETWEEN 9967200 AND 9967224;\n\nCOMMIT;\n`;
}

const modern = path.join(root, 'migrations', '20260906_paradise_catalogue_taxonomy_v3.sql');
const legacy = path.join(root, 'migrations', '20260906_paradise_catalogue_taxonomy_v3_legacy.sql');
fs.writeFileSync(modern, build(false));
fs.writeFileSync(legacy, build(true));
fs.writeFileSync(path.join(root, 'artifacts', 'paradise-catalog-taxonomy-v3.json'), JSON.stringify({
    categories: pages.map(([id, caption, pattern]) => ({ id, caption, pattern })),
    scope: 'catalog pages 9967000-9967839',
    fallbackPage: 9967220
}, null, 2));
console.log(`Generated ${ pages.length } categories.`);
