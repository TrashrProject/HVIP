#!/usr/bin/env node
'use strict';

const fs = require('fs');
const path = require('path');
const root = path.resolve(__dirname, '..');
const furnitureDataPath = path.join(root, 'swf_pz', 'V5-0-2', 'gamedata', 'json', 'FurnitureData.json');
const migrations = [
  path.join(root, 'migrations', '20260903_paradise_catalogue_mass_habborpbr.sql'),
  path.join(root, 'migrations', '20260904_paradise_catalogue_mass_habborpbr_legacy.sql')
];

const pages = [
  [9967201, 'Construction - Blocs couleurs'],
  [9967202, 'Construction - Murs et sols'],
  [9967203, 'Maison - Salon et mobilier'],
  [9967204, 'Maison - Cuisine restaurant'],
  [9967205, 'Maison - Chambre et bain'],
  [9967206, 'Decoration et eclairage'],
  [9967207, 'Ville et services publics'],
  [9967208, 'Commerces et entreprises'],
  [9967209, 'Police justice et armee'],
  [9967210, 'Hopital secours et incendie'],
  [9967211, 'Transports et garages'],
  [9967212, 'Nature eau et exterieurs'],
  [9967213, 'Bureau et technologie'],
  [9967214, 'Jeux sport et musique'],
  [9967215, 'Animaux et compagnons'],
  [9967216, 'Noel et hiver'],
  [9967217, 'Halloween et fantastique'],
  [9967218, 'Paques amour et fetes'],
  [9967219, 'Rares statues et collections'],
  [9967220, 'Classiques et objets divers'],
  [9967221, 'Creations custom exclusives'],
  [9967222, 'Mode beaute et accessoires'],
  [9967223, 'Art cultures et histoire'],
  [9967224, 'Panneaux lettres et chiffres']
];

const rules = [
  [9967216, /xmas|christmas|noel|kerst|winter|snow|santa|rudolph|advent|gift|cadeau|natal|navidad|penguin|igloo|frost|ghiaccio/],
  [9967217, /hween|halloween|pumpkin|citrouille|abobora|spooky|haunted|ghost|fantom|zombie|vampir|witch|sorci|monster|demon|skull|squelette|magic|wizard|dragon/],
  [9967218, /easter|paques|paque|pasen|valentine|amour|love|heart|coeur|carnav|circus|festive|(^|[_ -])fest([_ -]|$)|newyear|party|birthday|anniv|wedding|mariage|pride|summer|ete|automn|autumn/],
  [9967209, /police|policia|prison|prisao|security|securite|court|justice|judge|swat|jail|guard|army|armee|military|weapon|gun|rifle|pistol|ammo|crime|detective/],
  [9967210, /hospital|hopital|clinic|clinique|medic|doctor|docteur|nurse|ambulance|pharmacy|pharmacie|surgery|dentist|firstaid|stretcher|wheelchair|xray|defib|firefighter|fireman|pompier|rescue|secours|hydrant|extinguisher/],
  [9967211, /(^|[_ -])(car|bus|bike|moto|taxi|tram|train|rail|plane|boat|ship)([_ -]|$)|vehicle|vehicule|carro|coche|garage|busstop|bicycle|scooter|metro|airport|aeroport|aviao|barco|parking|petrol|traffic|wheelstack|ontrackgp/],
  [9967215, /(^|[_ -])(pet|dog|cat|horse|fish|bird|animal)([_ -]|$)|chien|cachorro|gato|cheval|cavalo|poisson|peixe|oiseau|passaro|puppy|kitten|turtle|tortue|rabbit|bunny|lapin|duck|canard|frog|grenouille|cow|vache|pig|cochon|armadillo|beaver|eagle|opossum|raccoon|skunk|crow|squirrel|peacock|stag|bear|lion|tiger|monkey/],
  [9967204, /kitchen|cuisine|cozinha|cocina|fridge|refriger|geladeira|oven|four|forno|stove|cuisiniere|sink|lavabo|food|comida|restaurant|cafe|coffee|bar_|bakery|bakker|bakk_|bakkert|pizza|burger|diner|menu|counter|comptoir|balcao|cake|drink|tea.?set|toaster|barbecue/],
  [9967205, /(^|[_ -])(bed|bath|toilet|shower)([_ -]|$)|bedroom|chambre|quarto|cama|banho|douche|ducha|wardrobe|armoire|guarda.?roupa|dresser|nightstand|pillow|oreiller|blanket/],
  [9967208, /shop|store|market|marche|loja|tienda|mall|bank|banque|boutique|business|empresa|cashier|checkout|register|vending|kiosk|commerce|salon|hotel|reception|beautyspot/],
  [9967207, /city|ville|cidade|urban|school|ecole|escola|government|mairie|cityhall|library|bibli|museum|station|mail|poste|post_|elevator|ascenseur|locker|queue|barrier|streetlight|public|airport/],
  [9967213, /computer|ordinateur|computador|laptop|phone|telefone|screen|ecran|monitor|server|tech|robot|camera|(^|[_ -])tv([_ -]|$)|television|speaker|console|radio|tablet|keyboard|machine|device|printer|imprim|office|bureau|escritorio|desk/],
  [9967214, /game|jeu|jogo|gaming|music|musique|musica|disco|cinema|stage|gym|football|trophy|dance|theatre|basket|tennis|skate|(^|[_ -])ball([_ -]|$)|piano|guitar|drum|billiard|pooltable|foosball|arcade|chess|domino|mahjong|sport|dj_/],
  [9967212, /tree|arbre|arvore|plant|plante|planta|flower|fleur|flor|sakura|bush|grass|rock|roch|pedra|garden|jardin|jardim|forest|floresta|farm|fazenda|beach|plage|praia|water|(^|[_ -])eau([_ -]|$)|agua|pool|piscine|nature|outdoor|park|pond|river|mountain|sand|soil|sunflower|bamboo|bambou|wood|bois|legno|leaves|feuill|stump|logs/],
  [9967201, /block|bloc|bloco|bloque|cube|cubie|pixel|colour|color|(^|[_ -])cor([_ -]|$)|palette|gradient|builder|build|construction|beam|poutre|plank|planche|cement|concrete/],
  [9967202, /wall|mur|parede|pared|brick|tijolo|floor|(^|[_ -])sol([_ -]|$)|piso|chao|tile|dalle|tegel|roof|toit|telhado|door|porte|porta|window|fenetre|janela|fence|cloture|cerca|stair|escalier|escada|column|pillar|pilier|bridge|pont|road|route|rua|asphalt|gate|ladder/],
  [9967206, /lamp|light|lumiere|luz|candle|bougie|vela|neon|vase|frame|cadre|painting|tableau|paint|decor|clock|clck|horloge|mirror|miroir|poster|curtain|rideau|statue|fountain|fontaine|ornament|canvas|dreamcatcher|parasol|orb/],
  [9967203, /sofa|canape|couch|chair|chaise|cadeira|seat|fauteuil|stool|tabouret|banco|banc|bench|table|mesa|cabinet|armoire|shelf|regal|etagere|bookcase|bookshelf|rug|tapis|carpet|tapete|home|house|maison|living|salon|drawer|cushion|bean.?bag|lounger|divider|blinds|garderobe|crate|chest|baule/],
  [9967224, /(^|[_ -])(letter|lettre|chiffre|number|alphabet|sign|panneau|bouton)([_ -]|$)|letters|chiffres|signal|arrow|fleche|plaque/],
  [9967222, /fashion|mode|couture|clothing|clothes|cloth|rack|dress|shirt|shoe|hat|cap|bag|accessor|jewel|beauty|creme|spray|foehn|hair|haar|makeup|maquillage|parfum/],
  [9967223, /art_|artist|easel|paper|history|hhistory|grec|greek|roman|egypt|oriental|japo|japan|skorea|korea|china|asian|medieval|fantasy|eglise|church|temple|architetto|astronomy|globe|shield|bouclier|debris|scandi|japandi/],
  [9967219, /rare|ltd|limited|throne|trone|statue|trophy|collect|diamond|gold|silver|crystal|platinum|ruby|emerald|sapphire|ancient|relic|dragon|(^|[_ -])nft([_ -]|$)/],
  [9967221, /(^|[_ -])(habbox|habblet|cstm|wibbo|wibbocustom|yvess?5?|kasja|hellsinore|stark|atlanta|mut)([_ -]|$)|vwave|norja|pops_|fiore_/]
];

function normalize(value) {
  return String(value || '').toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, ' ');
}
function classify(value) {
  const text = normalize(value);
  for (const [page, pattern] of rules) if (pattern.test(text)) return page;
  return 9967220;
}
function splitFields(row) {
  const out = [];
  let value = '', quoted = false;
  for (let i = 1; i < row.length - 1; i++) {
    const c = row[i];
    if (c === "'" && quoted && row[i + 1] === "'") { value += "''"; i++; continue; }
    if (c === "'") { quoted = !quoted; value += c; continue; }
    if (c === ',' && !quoted) { out.push(value); value = ''; continue; }
    value += c;
  }
  out.push(value);
  return out;
}

const data = JSON.parse(fs.readFileSync(furnitureDataPath, 'utf8'));
const byId = new Map();
for (const entry of [...data.roomitemtypes.furnitype, ...data.wallitemtypes.furnitype]) {
  byId.set(String(entry.id), `${entry.classname || ''} ${entry.name || ''} ${entry.description || ''}`);
}

function pageBlock(legacy) {
  const rootCaption = 'Catalogue ParadiseRP complet';
  if (legacy) {
    const rows = [[9967200, -1, rootCaption], ...pages.map(([id, caption]) => [id, 9967200, caption])]
      .map(([id, parent, caption], index) => `(${id},${parent},'${caption}','${caption}','default_3x3',1,1,1,${index === 0 ? 90 : index},'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')`);
    return `INSERT INTO catalog_pages (id,parent_id,caption_save,caption,page_layout,icon_color,icon_image,min_rank,order_num,visible,enabled,club_only,vip_only,page_headline,page_teaser,page_special,page_text1,page_text2,page_text_details,page_text_teaser,room_id,includes) VALUES\n${rows.join(',\n')}\nON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption_save=VALUES(caption_save),caption=VALUES(caption),order_num=VALUES(order_num),visible='1',enabled='1';`;
  }
  const rows = [[9967200, -1, rootCaption], ...pages.map(([id, caption]) => [id, 9967200, caption])]
    .map(([id, parent, caption], index) => `(${id},${parent},'${caption}',1,'1','1',1,0,${index === 0 ? 90 : index},'','default_3x3','','')`);
  return `INSERT INTO catalog_pages (id,parent_id,caption,icon_image,visible,enabled,min_rank,min_vip,order_num,page_link,page_layout,page_strings_1,page_strings_2) VALUES\n${rows.join(',\n')}\nON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption=VALUES(caption),order_num=VALUES(order_num),visible='1',enabled='1';`;
}

for (const file of migrations) {
  let sql = fs.readFileSync(file, 'utf8');
  const legacy = file.endsWith('_legacy.sql');
  sql = sql.replace(/INSERT INTO catalog_pages[\s\S]*?ON DUPLICATE KEY UPDATE[^;]+;/, pageBlock(legacy));
  const counts = new Map();
  const order = new Map();
  const unmatched = new Map();
  const assignments = [];
  sql = sql.split(/\r?\n/).map(line => {
    if (!line.startsWith('(') || !line.includes('19502')) return line;
    const suffix = line.endsWith(',') ? ',' : line.endsWith(';') ? ';' : '';
    const fields = splitFields(line.slice(0, suffix ? -1 : undefined));
    if (fields.length !== (legacy ? 15 : 14)) return line;
    const id = (legacy ? fields[0] : fields[1]).replaceAll("'", '');
    const name = (legacy ? fields[2] : fields[2]).replaceAll("''", "'");
    const page = classify(`${byId.get(id) || ''} ${name}`);
    if (page === 9967220) {
      const className = String(byId.get(id) || name).trim().split(/\s+/)[0];
      const prefix = className.split('_')[0] || '#';
      unmatched.set(prefix, (unmatched.get(prefix) || 0) + 1);
    }
    counts.set(page, (counts.get(page) || 0) + 1);
    const next = (order.get(page) || 0) + 1;
    order.set(page, next);
    const offerId = Number((legacy ? fields[10] : fields[12]).replaceAll("'", ''));
    assignments.push([offerId, page, next]);
    if (legacy) { fields[1] = String(page); fields[9] = String(next); }
    else fields[0] = String(page);
    return `(${fields.join(',')})${suffix}`;
  }).join('\n');
  fs.writeFileSync(file, sql.endsWith('\n') ? sql : sql + '\n', 'utf8');

  const reorg = [];
  reorg.push('-- ParadiseRP reclassement idempotent du catalogue massif HabboRPbr');
  reorg.push('SET NAMES utf8mb4;');
  reorg.push('START TRANSACTION;');
  reorg.push(pageBlock(legacy));
  reorg.push('DROP TEMPORARY TABLE IF EXISTS paradise_catalog_reorg;');
  reorg.push('CREATE TEMPORARY TABLE paradise_catalog_reorg (offer_id INT NOT NULL PRIMARY KEY,page_id INT NOT NULL,order_number INT NOT NULL);');
  for (let offset = 0; offset < assignments.length; offset += 500) {
    const values = assignments.slice(offset, offset + 500).map(row => `(${row.join(',')})`).join(',\n');
    reorg.push(`INSERT INTO paradise_catalog_reorg (offer_id,page_id,order_number) VALUES\n${values};`);
  }
  reorg.push(legacy
    ? 'UPDATE catalog_items c JOIN paradise_catalog_reorg r ON r.offer_id=c.offer_id SET c.page_id=r.page_id,c.order_number=r.order_number;'
    : 'UPDATE catalog_items c JOIN paradise_catalog_reorg r ON r.offer_id=c.offer_id SET c.page_id=r.page_id;');
  reorg.push('DROP TEMPORARY TABLE paradise_catalog_reorg;');
  reorg.push('COMMIT;');
  const reorgName = legacy
    ? '20260904_paradise_catalogue_reorganize_v2_legacy.sql'
    : '20260904_paradise_catalogue_reorganize_v2.sql';
  fs.writeFileSync(path.join(root, 'migrations', reorgName), reorg.join('\n') + '\n', 'utf8');
  console.log(path.basename(file));
  for (const [id, caption] of pages) console.log(`  ${String(counts.get(id) || 0).padStart(4)}  ${caption}`);
  console.log('  Principaux prefixes non classes:', [...unmatched].sort((a, b) => b[1] - a[1]).slice(0, 35).map(([k, v]) => `${k}:${v}`).join(', '));
}
