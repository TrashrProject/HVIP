#!/usr/bin/env node

const fs = require('fs');
const path = require('path');

const args = process.argv.slice(2);
const apply = args.includes('--apply');
const repository = path.resolve(args.includes('--repository') ? args[args.indexOf('--repository') + 1] : path.join(__dirname, '..'));
const furnitureDataPath = path.join(repository, 'swf_pz', 'V5-0-2', 'gamedata', 'json', 'FurnitureData.json');
const migrationOutputPath = path.join(repository, 'migrations', '20260906_paradise_catalogue_sprite_id_fix.sql');
const migrationPaths = [
    'migrations/20260903_paradise_catalogue_rp_v3.sql',
    'migrations/20260903_paradise_catalogue_extension_v4.sql',
    'migrations/20260903_paradise_catalogue_mass_habborpbr.sql',
    'migrations/20260904_paradise_island_builder_kit.sql'
].map(relative => path.join(repository, relative)).filter(file => fs.existsSync(file));

const parseTuple = line => {
    const values = [];
    let value = '';
    let quoted = false;
    for (let index = 1; index < line.length; index += 1) {
        const character = line[index];
        if (quoted) {
            if (character === "'" && line[index + 1] === "'") {
                value += "'";
                index += 1;
            } else if (character === "'") quoted = false;
            else value += character;
        } else if (character === "'") quoted = true;
        else if (character === ',') {
            values.push(value.trim());
            value = '';
        } else if (character === ')') {
            values.push(value.trim());
            break;
        } else value += character;
    }
    return values;
};

const mappings = new Map();
for (const migrationPath of migrationPaths) {
    for (const line of fs.readFileSync(migrationPath, 'utf8').split(/\r?\n/)) {
        if (!/^\(\d+,'/.test(line)) continue;
        const columns = parseTuple(line);
        // furniture rows have 27 values. Column 0 is the database base-item
        // id, column 1 the classname and column 10 the sprite id serialized to
        // Nitro by CatalogItem.serialize().
        if (columns.length !== 27 || !/^\d+$/.test(columns[0]) || !/^\d+$/.test(columns[10])) continue;
        const itemId = Number(columns[0]);
        const spriteId = Number(columns[10]);
        const className = columns[1];
        if (!className || !spriteId) continue;
        const previous = mappings.get(className.toLowerCase());
        if (!previous || previous.itemId === itemId) mappings.set(className.toLowerCase(), { itemId, spriteId, className });
    }
}

const furnitureData = JSON.parse(fs.readFileSync(furnitureDataPath, 'utf8'));
const roomEntries = furnitureData.roomitemtypes?.furnitype || [];
const wallEntries = furnitureData.wallitemtypes?.furnitype || [];
const entries = [...roomEntries, ...wallEntries];
const occupiedIds = new Map();
for (const entry of entries) occupiedIds.set(Number(entry.id), String(entry.classname || '').toLowerCase());

const repairs = [];
const changes = [];
const collisions = [];
for (const entry of entries) {
    const className = String(entry.classname || '');
    const mapping = mappings.get(className.toLowerCase());
    if (!mapping || Number(entry.id) === mapping.spriteId) continue;
    if (Number(entry.id) !== mapping.itemId) continue;
    repairs.push({ className, itemId: mapping.itemId, oldSpriteId: mapping.spriteId });
    const occupant = occupiedIds.get(mapping.spriteId);
    if (occupant && occupant !== className.toLowerCase()) {
        collisions.push({ className, itemId: mapping.itemId, spriteId: mapping.spriteId, occupiedBy: occupant });
        continue;
    }
    changes.push({ entry, className, from: Number(entry.id), to: mapping.spriteId });
}

if (apply) {
    const lines = [
        '-- ParadiseRP - aligne les sprite_id serveur sur les identifiants FurnitureData.',
        '-- Le catalogue sérialise sprite_id vers Nitro; FurnitureData utilise furniture.id pour ces imports.',
        '-- Idempotent : aucun mobilier et aucune offre ne sont supprimés.',
        'SET NAMES utf8mb4;',
        'START TRANSACTION;'
    ];
    for (let offset = 0; offset < repairs.length; offset += 250) {
        const ids = repairs.slice(offset, offset + 250).map(repair => repair.itemId).join(',');
        lines.push(`UPDATE furniture SET sprite_id=id WHERE id IN (${ids});`);
    }
    lines.push('COMMIT;', '');
    fs.writeFileSync(migrationOutputPath, lines.join('\n'), 'utf8');
}

console.log(JSON.stringify({
    mode: apply ? 'apply' : 'audit',
    migrationFiles: migrationPaths.length,
    furnitureMappings: mappings.size,
    furnitureDataEntries: entries.length,
    spriteIdsToRepair: repairs.length,
    collisions: collisions.length,
    sample: repairs.slice(0, 20),
    collisionSample: collisions.slice(0, 20),
    migrationOutputPath
}, null, 2));
