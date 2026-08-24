const { execFileSync } = require('child_process');
const fs = require('fs');
const path = require('path');

const root = path.resolve(__dirname, '..');
const furnitureDataPath = path.join(root, 'swf_pz', 'V5-0-2', 'gamedata', 'json', 'FurnitureData.json');
const mysqlPath = 'C:\\xampp\\mysql\\bin\\mysql.exe';
const furniturePath = path.join(root, 'swf_pz', 'V5-0-2', 'furniture');

const data = JSON.parse(fs.readFileSync(furnitureDataPath, 'utf8'));
const collections = {
    s: data.roomitemtypes.furnitype,
    i: data.wallitemtypes.furnitype
};
const ids = new Set();
const classes = { s: new Map(), i: new Map() };

for (const type of ['s', 'i']) {
    for (const item of collections[type]) {
        ids.add(`${type}:${item.id}`);
        const assetName = String(item.classname).split('*')[0];
        if (fs.existsSync(path.join(furniturePath, `${assetName}.nitro`))) {
            classes[type].set(String(item.classname).toLowerCase(), item);
        }
    }
}

const floorRules = [
    [/tp_arropoint|telearrow|tele.*arrow/, 'room_wl15_telearrow', 'teleport-arrow'],
    [/action.?point/, 'actionpoint01', 'action-point'],
    [/arrow|fleche/, 'wf_arrowplate', 'arrow'],
    [/clothing_/, 'clothing_apron', 'clothing'],
    [/road|route|street|asphalt/, 'holorp_black_road_borderless', 'road'],
    [/grass|herbe|gazon|lawn/, 'env_grass', 'grass'],
    [/tree|arbre|palm|sapin/, 'lt_c15_tree', 'tree'],
    [/plant|plante|flower|fleur|feuille|foliage|bush|buisson|bambou|cactus/, 'plant_pineapple', 'plant'],
    [/window|fenetre|raam/, 'elegant_c17_window', 'window'],
    [/door|porte|gate|barrier|barriere|fence|hek/, 'classic7_gate', 'door'],
    [/sofa|loveseat|canape/, 'classic3_sofa*0', 'sofa'],
    [/bench|banc/, 'classic3_bench*0', 'bench'],
    [/chair|chaise|stuhl|seat|fauteuil|stool|tabouret/, 'classic7_chair', 'chair'],
    [/computer|ordinateur|laptop|macbook/, 'computer_flatscreen', 'computer'],
    [/phone|telephone/, 'exe_c15_telephone', 'phone'],
    [/toilet|wc_/, 'toilet', 'toilet'],
    [/bed|\blit\b/, 'purablk_c16_bed*1', 'bed'],
    [/shelf|regal|cabinet|armoire|cupboard/, 'army_c15_bookshelf', 'shelf'],
    [/counter|comptoir/, 'classic2_counter', 'counter'],
    [/table|desk|bureau/, 'classic3_table', 'table'],
    [/lamp|light|lumi|lantern|laterne|neon|chandelier/, 'uni_c15_lamp', 'light'],
    [/water|eau|pond|fountain|fontaine/, 'jungle_c16_watertile', 'water'],
    [/vehicle|car|voiture|bike|scooter|moto/, 'anascar1', 'vehicle'],
    [/statue|bust|ange/, 'statue', 'statue'],
    [/\bbar\b|_bar_/, 'purablk_c16_bar', 'bar'],
    [/wall|mur|divider|pillar|pilier|column|roof|toit/, 'lt_stone2', 'structure'],
    [/brick|brique|block|bloc|cube|rock|stone|pierre|rocher|concrete|beton/, 'bc_block_redbrick*1', 'building-block'],
    [/floor|sol|tile|dalle|ground|dirt|terre|rug|tapis|carpet|gravier/, 'classic7_floor', 'floor']
];

const wallRules = [
    [/window|fenetre/, 'lodge_c15_window', 'wall-window'],
    [/wall|mur/, 'lodge_c15_wall', 'wall'],
    [/poster|sign|tableau|picture|painting|decor/, 'poster27', 'wall-decoration']
];

function selectFallback(type, name) {
    const rules = type === 'i' ? wallRules : floorRules;
    for (const [pattern, className, category] of rules) {
        if (pattern.test(name) && classes[type].has(className.toLowerCase())) {
            return { source: classes[type].get(className.toLowerCase()), category };
        }
    }

    const defaultClass = type === 'i' ? 'photo' : 'classic7_floor';
    return { source: classes[type].get(defaultClass), category: 'generic' };
}

const query = [
    'SELECT DISTINCT ib.sprite_id, ib.item_name, ib.type',
    'FROM items i JOIN items_base ib ON ib.id = i.item_id',
    "WHERE i.room_id > 0 AND ib.sprite_id > 0 AND ib.type IN ('s', 'i')",
    'ORDER BY ib.sprite_id'
].join(' ');
const output = execFileSync(mysqlPath, [
    '--host=127.0.0.1', '--user=root', '--database=waveplus',
    '--batch', '--raw', '--skip-column-names', `--execute=${query}`
], { encoding: 'utf8', maxBuffer: 16 * 1024 * 1024 });

const report = new Map();
let added = 0;
for (const row of output.trim().split(/\r?\n/)) {
    if (!row) continue;
    const [spriteText, itemName, type] = row.split('\t');
    const spriteId = Number(spriteText);
    const key = `${type}:${spriteId}`;
    if (ids.has(key)) continue;

    const fallback = selectFallback(type, itemName.toLowerCase());
    if (!fallback.source) throw new Error(`No fallback found for ${type}:${itemName}`);

    collections[type].push({ ...fallback.source, id: spriteId, offerid: spriteId });
    ids.add(key);
    report.set(fallback.category, (report.get(fallback.category) || 0) + 1);
    added++;
}

function replaceVisual(type, currentId, replacementClass) {
    const index = collections[type].findIndex(item => Number(item.id) === currentId);
    const replacement = classes[type].get(replacementClass.toLowerCase());
    if (index < 0 || !replacement) throw new Error(`Unable to replace ${type}:${currentId}`);
    collections[type][index] = { ...replacement, id: currentId, offerid: currentId };
}

replaceVisual('s', 5480, 'bc_block_1*14');
replaceVisual('s', 5593, 'bc_block_marble*14');

fs.writeFileSync(furnitureDataPath, JSON.stringify(data), 'utf8');
console.log(`Added ${added} placed-furniture fallbacks.`);
console.log([...report.entries()].sort((a, b) => a[0].localeCompare(b[0])).map(([key, value]) => `${key}=${value}`).join(' '));
