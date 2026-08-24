const { execFileSync } = require('child_process');
const fs = require('fs');
const path = require('path');

const projectRoot = path.resolve(__dirname, '..');
const furnitureDataPath = path.join(
    projectRoot,
    'swf_pz',
    'V5-0-2',
    'gamedata',
    'json',
    'FurnitureData.json'
);
const mysqlPath = 'C:\\xampp\\mysql\\bin\\mysql.exe';

const query = [
    'SELECT sprite_id, item_name, type',
    'FROM items_base',
    "WHERE sprite_id > 0 AND type IN ('s', 'i')",
    'ORDER BY id'
].join(' ');

const rawRows = execFileSync(mysqlPath, [
    '--host=127.0.0.1',
    '--user=root',
    '--database=waveplus',
    '--batch',
    '--raw',
    '--skip-column-names',
    `--execute=${query}`
], { encoding: 'utf8', maxBuffer: 16 * 1024 * 1024 });

const data = JSON.parse(fs.readFileSync(furnitureDataPath, 'utf8'));
const floorItems = data.roomitemtypes.furnitype;
const wallItems = data.wallitemtypes.furnitype;
const idIndex = new Set();
const classIndex = new Map();

for (const [type, items] of [['s', floorItems], ['i', wallItems]]) {
    for (const item of items) {
        idIndex.add(`${type}:${item.id}`);
        classIndex.set(`${type}:${String(item.classname).toLowerCase()}`, item);
    }
}

let added = 0;
for (const row of rawRows.trim().split(/\r?\n/)) {
    if (!row) continue;

    const [spriteIdText, itemName, type] = row.split('\t');
    const spriteId = Number(spriteIdText);
    const idKey = `${type}:${spriteId}`;
    if (idIndex.has(idKey)) continue;

    const source = classIndex.get(`${type}:${itemName.toLowerCase()}`);
    if (!source) continue;

    const alias = { ...source, id: spriteId, offerid: spriteId };
    (type === 's' ? floorItems : wallItems).push(alias);
    idIndex.add(idKey);
    added++;
}

fs.writeFileSync(furnitureDataPath, JSON.stringify(data), 'utf8');
console.log(`Added ${added} FurnitureData aliases.`);
