-- ParadiseRP - V5 catalogue detaille complet (schema moderne)
SET NAMES utf8mb4;
START TRANSACTION;

INSERT INTO catalog_pages (id,parent_id,caption,icon_image,visible,enabled,min_rank,min_vip,order_num,page_link,page_layout,page_strings_1,page_strings_2) VALUES
-- Maison / mobilier
(9968301,9967203,'Canapes',1,'1','1',1,0,1,'','default_3x3','',''),
(9968302,9967203,'Fauteuils',1,'1','1',1,0,2,'','default_3x3','',''),
(9968303,9967203,'Chaises et tabourets',1,'1','1',1,0,3,'','default_3x3','',''),
(9968304,9967203,'Bancs',1,'1','1',1,0,4,'','default_3x3','',''),
(9968305,9967203,'Tables',1,'1','1',1,0,5,'','default_3x3','',''),
(9968306,9967203,'Bureaux',1,'1','1',1,0,6,'','default_3x3','',''),
(9968307,9967203,'Armoires et dressings',1,'1','1',1,0,7,'','default_3x3','',''),
(9968308,9967203,'Etageres bibliotheques',1,'1','1',1,0,8,'','default_3x3','',''),
(9968309,9967203,'Commodes tiroirs rangements',1,'1','1',1,0,9,'','default_3x3','',''),
-- Chambre / bain
(9968310,9967205,'Lits',1,'1','1',1,0,1,'','default_3x3','',''),
(9968311,9967205,'Oreillers couvertures matelas',1,'1','1',1,0,2,'','default_3x3','',''),
(9968312,9967205,'Chevets et meubles chambre',1,'1','1',1,0,3,'','default_3x3','',''),
(9968313,9967205,'Toilettes',1,'1','1',1,0,4,'','default_3x3','',''),
(9968314,9967205,'Douches et baignoires',1,'1','1',1,0,5,'','default_3x3','',''),
(9968315,9967205,'Lavabos serviettes bain',1,'1','1',1,0,6,'','default_3x3','',''),
-- Cuisine
(9968316,9967204,'Fours plaques micro-ondes',1,'1','1',1,0,1,'','default_3x3','',''),
(9968317,9967204,'Frigos congelateurs',1,'1','1',1,0,2,'','default_3x3','',''),
(9968318,9967204,'Eviers et plans de travail',1,'1','1',1,0,3,'','default_3x3','',''),
(9968319,9967204,'Nourriture salee',1,'1','1',1,0,4,'','default_3x3','',''),
(9968320,9967204,'Desserts bonbons glaces',1,'1','1',1,0,5,'','default_3x3','',''),
(9968321,9967204,'Fruits et legumes',1,'1','1',1,0,6,'','default_3x3','',''),
(9968322,9967204,'Boissons chaudes',1,'1','1',1,0,7,'','default_3x3','',''),
(9968323,9967204,'Boissons froides bouteilles',1,'1','1',1,0,8,'','default_3x3','',''),
(9968324,9967204,'Bars comptoirs barbecue',1,'1','1',1,0,9,'','default_3x3','',''),
-- Decoration
(9968325,9967206,'Lampes',1,'1','1',1,0,1,'','default_3x3','',''),
(9968326,9967206,'Neons enseignes lumineuses',1,'1','1',1,0,2,'','default_3x3','',''),
(9968327,9967206,'Bougies lanternes cheminees',1,'1','1',1,0,3,'','default_3x3','',''),
(9968328,9967206,'Tapis',1,'1','1',1,0,4,'','default_3x3','',''),
(9968329,9967206,'Rideaux',1,'1','1',1,0,5,'','default_3x3','',''),
(9968330,9967206,'Miroirs',1,'1','1',1,0,6,'','default_3x3','',''),
(9968331,9967206,'Horloges',1,'1','1',1,0,7,'','default_3x3','',''),
(9968332,9967206,'Vases ornements sculptures deco',1,'1','1',1,0,8,'','default_3x3','',''),
(9968333,9967206,'Peluches poupees jouets',1,'1','1',1,0,9,'','default_3x3','',''),
-- Technologie
(9968334,9967213,'Ordinateurs',1,'1','1',1,0,1,'','default_3x3','',''),
(9968335,9967213,'Ecrans televisions',1,'1','1',1,0,2,'','default_3x3','',''),
(9968336,9967213,'Telephones tablettes',1,'1','1',1,0,3,'','default_3x3','',''),
(9968337,9967213,'Imprimantes claviers accessoires',1,'1','1',1,0,4,'','default_3x3','',''),
(9968338,9967213,'Cameras radios haut-parleurs',1,'1','1',1,0,5,'','default_3x3','',''),
(9968339,9967213,'Robots et machines',1,'1','1',1,0,6,'','default_3x3','',''),
(9968340,9967213,'Science laboratoire',1,'1','1',1,0,7,'','default_3x3','',''),
(9968341,9967213,'Espace fusees satellites',1,'1','1',1,0,8,'','default_3x3','',''),
-- Transport
(9968342,9967211,'Voitures',1,'1','1',1,0,1,'','default_3x3','',''),
(9968343,9967211,'Motos scooters velos',1,'1','1',1,0,2,'','default_3x3','',''),
(9968344,9967211,'Bus taxis utilitaires',1,'1','1',1,0,3,'','default_3x3','',''),
(9968345,9967211,'Trains metros trams',1,'1','1',1,0,4,'','default_3x3','',''),
(9968346,9967211,'Avions aviation',1,'1','1',1,0,5,'','default_3x3','',''),
(9968347,9967211,'Bateaux yachts navires',1,'1','1',1,0,6,'','default_3x3','',''),
(9968348,9967211,'Routes',1,'1','1',1,0,7,'','default_3x3','',''),
(9968349,9967211,'Parkings garages',1,'1','1',1,0,8,'','default_3x3','',''),
(9968350,9967211,'Signalisation routiere',1,'1','1',1,0,9,'','default_3x3','',''),
-- Police / justice / armee
(9968351,9967209,'Police',1,'1','1',1,0,1,'','default_3x3','',''),
(9968352,9967209,'Prison cellules barreaux',1,'1','1',1,0,2,'','default_3x3','',''),
(9968353,9967209,'Justice tribunaux preuves',1,'1','1',1,0,3,'','default_3x3','',''),
(9968354,9967209,'Surveillance securite CCTV',1,'1','1',1,0,4,'','default_3x3','',''),
(9968355,9967209,'Armes a feu',1,'1','1',1,0,5,'','default_3x3','',''),
(9968356,9967209,'Armes blanches boucliers',1,'1','1',1,0,6,'','default_3x3','',''),
(9968357,9967209,'Armee militaire',1,'1','1',1,0,7,'','default_3x3','',''),
-- Hopital / secours
(9968358,9967210,'Hopital mobilier medical',1,'1','1',1,0,1,'','default_3x3','',''),
(9968359,9967210,'Docteurs infirmiers soins',1,'1','1',1,0,2,'','default_3x3','',''),
(9968360,9967210,'Ambulances brancards fauteuils',1,'1','1',1,0,3,'','default_3x3','',''),
(9968361,9967210,'Pharmacie medicaments',1,'1','1',1,0,4,'','default_3x3','',''),
(9968362,9967210,'Pompiers',1,'1','1',1,0,5,'','default_3x3','',''),
(9968363,9967210,'Incendie camions secours',1,'1','1',1,0,6,'','default_3x3','',''),
-- Commerces
(9968364,9967208,'Magasins boutiques',1,'1','1',1,0,1,'','default_3x3','',''),
(9968365,9967208,'Restaurants diners cafes',1,'1','1',1,0,2,'','default_3x3','',''),
(9968366,9967208,'Hotels receptions',1,'1','1',1,0,3,'','default_3x3','',''),
(9968367,9967208,'Banques caisses comptoirs',1,'1','1',1,0,4,'','default_3x3','',''),
(9968368,9967208,'Bureaux entreprises',1,'1','1',1,0,5,'','default_3x3','',''),
(9968369,9967208,'Usines entrepots',1,'1','1',1,0,6,'','default_3x3','',''),
-- Ville
(9968370,9967207,'Ecoles universites',1,'1','1',1,0,1,'','default_3x3','',''),
(9968371,9967207,'Bibliotheques musees',1,'1','1',1,0,2,'','default_3x3','',''),
(9968372,9967207,'Poste services publics',1,'1','1',1,0,3,'','default_3x3','',''),
(9968373,9967207,'Lampadaires arrets urbains',1,'1','1',1,0,4,'','default_3x3','',''),
(9968374,9967207,'Chantiers ville',1,'1','1',1,0,5,'','default_3x3','',''),
-- Loisirs
(9968375,9967214,'Football',1,'1','1',1,0,1,'','default_3x3','',''),
(9968376,9967214,'Basketball',1,'1','1',1,0,2,'','default_3x3','',''),
(9968377,9967214,'Tennis',1,'1','1',1,0,3,'','default_3x3','',''),
(9968378,9967214,'Fitness boxe autres sports',1,'1','1',1,0,4,'','default_3x3','',''),
(9968379,9967214,'Arcade',1,'1','1',1,0,5,'','default_3x3','',''),
(9968380,9967214,'Jeux de societe',1,'1','1',1,0,6,'','default_3x3','',''),
(9968381,9967214,'Musique instruments',1,'1','1',1,0,7,'','default_3x3','',''),
(9968382,9967214,'DJ danse disco',1,'1','1',1,0,8,'','default_3x3','',''),
(9968383,9967214,'Cinema theatre',1,'1','1',1,0,9,'','default_3x3','',''),
-- Animaux
(9968384,9967215,'Chiens',1,'1','1',1,0,1,'','default_3x3','',''),
(9968385,9967215,'Chats',1,'1','1',1,0,2,'','default_3x3','',''),
(9968386,9967215,'Chevaux poneys',1,'1','1',1,0,3,'','default_3x3','',''),
(9968387,9967215,'Oiseaux',1,'1','1',1,0,4,'','default_3x3','',''),
(9968388,9967215,'Poissons animaux marins',1,'1','1',1,0,5,'','default_3x3','',''),
(9968389,9967215,'Lapins et petits animaux',1,'1','1',1,0,6,'','default_3x3','',''),
(9968390,9967215,'Animaux sauvages',1,'1','1',1,0,7,'','default_3x3','',''),
(9968391,9967215,'Creatures fantastiques',1,'1','1',1,0,8,'','default_3x3','',''),
-- Saisons / fetes
(9968392,9967216,'Noel sapins cadeaux',1,'1','1',1,0,1,'','default_3x3','',''),
(9968393,9967216,'Neige glace hiver',1,'1','1',1,0,2,'','default_3x3','',''),
(9968394,9967216,'Pere Noel rennes casse-noisette',1,'1','1',1,0,3,'','default_3x3','',''),
(9968395,9967217,'Fantomes hantise',1,'1','1',1,0,1,'','default_3x3','',''),
(9968396,9967217,'Sorciers magie',1,'1','1',1,0,2,'','default_3x3','',''),
(9968397,9967217,'Vampires zombies horreur',1,'1','1',1,0,3,'','default_3x3','',''),
(9968398,9967217,'Donjons temples aventures',1,'1','1',1,0,4,'','default_3x3','',''),
(9968399,9967218,'Paques',1,'1','1',1,0,1,'','default_3x3','',''),
(9968400,9967218,'Saint Valentin amour',1,'1','1',1,0,2,'','default_3x3','',''),
(9968401,9967218,'Mariages',1,'1','1',1,0,3,'','default_3x3','',''),
(9968402,9967218,'Anniversaires ballons cadeaux',1,'1','1',1,0,4,'','default_3x3','',''),
(9968403,9967218,'Carnaval festivals',1,'1','1',1,0,5,'','default_3x3','',''),
-- Mode
(9968404,9967222,'Cheveux coiffures',1,'1','1',1,0,1,'','default_3x3','',''),
(9968405,9967222,'Chapeaux couvre-chefs',1,'1','1',1,0,2,'','default_3x3','',''),
(9968406,9967222,'Hauts vestes',1,'1','1',1,0,3,'','default_3x3','',''),
(9968407,9967222,'Pantalons robes',1,'1','1',1,0,4,'','default_3x3','',''),
(9968408,9967222,'Chaussures',1,'1','1',1,0,5,'','default_3x3','',''),
(9968409,9967222,'Bijoux accessoires',1,'1','1',1,0,6,'','default_3x3','',''),
(9968410,9967222,'Beaute maquillage parfum',1,'1','1',1,0,7,'','default_3x3','',''),
-- Signaletique / art / rares
(9968411,9967224,'Lettres alphabet',1,'1','1',1,0,1,'','default_3x3','',''),
(9968412,9967224,'Chiffres nombres',1,'1','1',1,0,2,'','default_3x3','',''),
(9968413,9967224,'Panneaux affiches',1,'1','1',1,0,3,'','default_3x3','',''),
(9968414,9967224,'Drapeaux logos bannieres',1,'1','1',1,0,4,'','default_3x3','',''),
(9968415,9967223,'Tableaux peintures',1,'1','1',1,0,1,'','default_3x3','',''),
(9968416,9967223,'Statues sculptures',1,'1','1',1,0,2,'','default_3x3','',''),
(9968417,9967223,'Antiquites histoire culture',1,'1','1',1,0,3,'','default_3x3','',''),
(9968418,9967219,'LTD limited',1,'1','1',1,0,1,'','default_3x3','',''),
(9968419,9967219,'Rares or argent bronze',1,'1','1',1,0,2,'','default_3x3','',''),
(9968420,9967219,'Trophees prix reliques',1,'1','1',1,0,3,'','default_3x3','',''),
(9968421,9967221,'Paradise custom',1,'1','1',1,0,1,'','default_3x3','',''),
(9968422,9967220,'Autres mobis non classes',1,'1','1',1,0,1,'','default_3x3','','')
ON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption=VALUES(caption),order_num=VALUES(order_num),visible='1',enabled='1';

UPDATE catalog_items ci
LEFT JOIN items_base ib ON ib.id=ci.item_id
SET ci.page_id=CASE
-- Maison
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'sofa|couch' THEN 9968301
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'armchair|fauteuil' THEN 9968302
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'chair|stool|seat|tabouret' THEN 9968303
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'bench|banc' THEN 9968304
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'table|coffee table|dining table' THEN 9968305
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'desk|bureau|worktop' THEN 9968306
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'wardrobe|closet|armoire|dresser' THEN 9968307
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'shelf|bookcase|bibliotheque|etagere' THEN 9968308
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'cabinet|drawer|storage|cupboard|commode' THEN 9968309
-- Chambre bain
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP '(^|[^a-z])bed([^a-z]|$)|bed_|lit ' THEN 9968310
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'pillow|blanket|mattress|oreiller|couverture|matelas' THEN 9968311
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'nightstand|bedside|chevet' THEN 9968312
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'toilet|wc_' THEN 9968313
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'shower|bathtub|bath tub|douche|baignoire' THEN 9968314
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'washbasin|sink bathroom|towel|lavabo|serviette' THEN 9968315
-- Cuisine
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'oven|microwave|stove|cooker|hob|four|plaque' THEN 9968316
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'fridge|refrigerator|freezer|frigo|congel' THEN 9968317
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'kitchen sink|countertop|worktop|evier|plan de travail' THEN 9968318
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'pizza|burger|meal|snack|bread|meat|fish dish|food' THEN 9968319
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'cake|candy|icecream|ice cream|dessert|chocolate|bonbon|glace' THEN 9968320
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'fruit|vegetable|apple|banana|orange|carrot|tomato' THEN 9968321
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'coffee|tea_|teapot|hot chocolate|cafe|theiere' THEN 9968322
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'bottle|juice|soda|drink|glass|cup' THEN 9968323
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'bar_|pub_|counter|barbecue|bbq' THEN 9968324
-- Decoration
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'lamp|lampe' THEN 9968325
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'neon|light sign|enseigne' THEN 9968326
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'candle|lantern|fireplace|torch|bougie|lanterne|cheminee' THEN 9968327
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'rug|carpet|tapis' THEN 9968328
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'curtain|rideau' THEN 9968329
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'mirror|miroir' THEN 9968330
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'clock|horloge' THEN 9968331
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'vase|ornament|decor|sculpture|statue deco' THEN 9968332
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'plush|doll|toy|peluche|poupee|jouet' THEN 9968333
-- Tech
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'computer|pc_|desktop|ordinateur' THEN 9968334
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'television|tv_|monitor|screen|ecran' THEN 9968335
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'phone|tablet|telephone|smartphone' THEN 9968336
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'printer|keyboard|mouse|imprimante|clavier' THEN 9968337
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'camera|radio|speaker|haut-parleur' THEN 9968338
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'robot|machine' THEN 9968339
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'laboratory|science|lab_|microscope|chimie' THEN 9968340
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'rocket|spaceship|satellite|space|fusee' THEN 9968341
-- Transport
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'car_| car |automobile|voiture' THEN 9968342
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'motorbike|motorcycle|scooter|bike|bicycle|moto|velo' THEN 9968343
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'bus_| bus |taxi|van|truck|camion' THEN 9968344
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'train|metro|subway|tram' THEN 9968345
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'plane|aircraft|airport|avion' THEN 9968346
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'boat|ship|yacht|navire|bateau' THEN 9968347
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'road|street|route|pavement' THEN 9968348
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'parking|garage' THEN 9968349
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'traffic sign|road sign|signalisation' THEN 9968350
-- RP police
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'police|swat|detective|officer' THEN 9968351
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'prison|jail|cell bars|cell door|barreau' THEN 9968352
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'court|justice|evidence|interrogation|tribunal|preuve' THEN 9968353
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'security|cctv|surveillance|camera security' THEN 9968354
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'gun|rifle|pistol|firearm|arme a feu' THEN 9968355
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'sword|knife|shield|epee|couteau|bouclier' THEN 9968356
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'army|military|soldier|armee|militaire' THEN 9968357
-- Medical
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'hospital|medical bed|xray|x-ray|clinic' THEN 9968358
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'doctor|nurse|medic|syringe|blood bag|defibrillator' THEN 9968359
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'ambulance|stretcher|wheelchair|brancard|fauteuil roulant' THEN 9968360
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'pharmacy|medicine|pill|medicament' THEN 9968361
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'firefighter|pompier' THEN 9968362
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'firetruck|fire station|rescue|emergency|incendie|secours' THEN 9968363
-- Commerce / ville
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'shop|store|boutique|supermarket|market' THEN 9968364
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'restaurant|diner|cafe|bakery' THEN 9968365
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'hotel|reception' THEN 9968366
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'bank|cashier|till|caisse' THEN 9968367
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'office|business|entreprise' THEN 9968368
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'factory|warehouse|usine|entrepot' THEN 9968369
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'school|university|ecole|universite' THEN 9968370
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'library|museum|bibliotheque|musee' THEN 9968371
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'post office|public service|poste|service public' THEN 9968372
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'streetlight|bus stop|lampadaire|arret' THEN 9968373
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'construction site|chantier' THEN 9968374
-- Loisirs
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'football|soccer|fball' THEN 9968375
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'basketball' THEN 9968376
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'tennis' THEN 9968377
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'gym|fitness|boxing|sport|skate' THEN 9968378
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'arcade' THEN 9968379
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'chess|dice|board game|game_|gaming' THEN 9968380
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'guitar|piano|drum|microphone|instrument|music' THEN 9968381
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'dj|turntable|dance|disco' THEN 9968382
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'cinema|theatre|movie' THEN 9968383
-- Animaux
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'dog|chien' THEN 9968384
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'cat_| cat |chat ' THEN 9968385
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'horse|pony|cheval|poney' THEN 9968386
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'bird|parrot|duck|penguin|oiseau' THEN 9968387
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'fish|shark|whale|dolphin|poisson|requin|baleine|dauphin' THEN 9968388
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'rabbit|bunny|hamster|lapin' THEN 9968389
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'lion|tiger|elephant|bear|monkey|gorilla|deer' THEN 9968390
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'unicorn|dragon|dinosaur' THEN 9968391
-- Saisons
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'christmas|xmas|tree xmas|cadeau noel|gift xmas' THEN 9968392
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'snow|ice_|winter|frost|neige|hiver' THEN 9968393
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'santa|reindeer|nutcracker|pere noel|renne' THEN 9968394
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'ghost|haunted|fantome' THEN 9968395
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'witch|wizard|magic|sorcier|magie' THEN 9968396
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'vampire|zombie|horror|skull' THEN 9968397
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'dungeon|temple|quest|adventure|donjon' THEN 9968398
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'easter|paques|easter egg' THEN 9968399
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'valentine|heart|love|amour' THEN 9968400
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'wedding|mariage' THEN 9968401
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'birthday|balloon|present|anniversaire|ballon' THEN 9968402
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'carnival|festival|carnaval' THEN 9968403
-- Mode
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'hair|coiffure' THEN 9968404
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'hat_| hat |cap|helmet|chapeau|casquette' THEN 9968405
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'shirt|jacket|top_|coat|chemise|veste' THEN 9968406
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'pants|trousers|dress|skirt|pantalon|robe|jupe' THEN 9968407
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'shoe|boot|sneaker|chaussure' THEN 9968408
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'jewelry|jewellery|necklace|ring|accessory|bijou|collier' THEN 9968409
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'makeup|beauty|perfume|maquillage|parfum' THEN 9968410
-- Signal / art / rares
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'alphabet|letter|lettre' THEN 9968411
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'number|digit|chiffre' THEN 9968412
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'sign|poster|placard|notice|panneau|affiche' THEN 9968413
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'flag|logo|banner|drapeau|banniere' THEN 9968414
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'painting|picture|portrait|canvas|tableau|peinture' THEN 9968415
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'statue|sculpture' THEN 9968416
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'antique|historic|history|culture|antiquite' THEN 9968417
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'ltd|limited' THEN 9968418
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'rare|golden|silver|bronze|diamond|or_|argent' THEN 9968419
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'trophy|prize|relic|trophee|relique' THEN 9968420
WHEN LOWER(CONCAT_WS(' ',ci.catalog_name,ib.item_name,ib.public_name)) REGEXP 'custom|paradise|waverp|waveplus|habborpbr|exclusive' THEN 9968421
ELSE ci.page_id END
WHERE ci.page_id BETWEEN 9967203 AND 9967224 OR ci.page_id BETWEEN 9968120 AND 9968138 OR ci.page_id BETWEEN 9968301 AND 9968422;

-- Les grosses pages traitees deviennent des dossiers : tout reste accessible via une sous-page.
UPDATE catalog_items SET page_id=9968422
WHERE page_id IN (9967203,9967204,9967205,9967206,9967207,9967208,9967209,9967210,9967211,9967213,9967214,9967215,9967216,9967217,9967218,9967219,9967220,9967221,9967222,9967223,9967224);

COMMIT;
