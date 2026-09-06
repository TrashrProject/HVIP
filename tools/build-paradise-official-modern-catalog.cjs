#!/usr/bin/env node
'use strict';

/**
 * Imports a prevalidated official furniture manifest into ParadiseRP.
 *
 * The source manifest is generated from higoka/habbo-downloader furnidata.
 * Every accepted entry must have both a converted .nitro bundle and its exact
 * icon variant. IDs live in a Paradise-only range so existing custom furniture
 * can never be overwritten.
 */

const fs = require('fs');
const path = require('path');

const repositoryRoot = path.resolve(__dirname, '..');
const manifestPath = process.argv[2];
const nitroSource = process.argv[3];
const iconSource = process.argv[4];

if(!manifestPath || !nitroSource || !iconSource) {
    throw new Error('Usage: node build-paradise-official-modern-catalog.cjs <manifest.json> <nitro-dir> <icon-dir>');
}

const furnitureDataPath = path.join(repositoryRoot, 'swf_pz', 'V5-0-2', 'gamedata', 'json', 'FurnitureData.json');
const furnitureTarget = path.join(repositoryRoot, 'swf_pz', 'V5-0-2', 'furniture');
const iconTarget = path.join(repositoryRoot, 'swf_pz', 'V5-0-2', 'dcr', 'hof_furni', 'icon');
const modernMigrationPath = path.join(repositoryRoot, 'migrations', '20260906_paradise_official_novelties_2023_2026.sql');
const legacyMigrationPath = path.join(repositoryRoot, 'migrations', '20260906_paradise_official_novelties_2023_2026_legacy.sql');
const reportPath = path.join(repositoryRoot, 'artifacts', 'paradise-official-novelties-2023-2026.json');
const itemIdStart = 997100000;
const offerIdStart = 1961000000;
const rootPageId = 9967800;

const pages = [
    { id: 9967801, caption: 'Construction 2023-2026', key: 'construction' },
    { id: 9967802, caption: 'Maison et design 2023-2026', key: 'home' },
    { id: 9967803, caption: 'Ville et RP 2023-2026', key: 'city' },
    { id: 9967804, caption: 'Nature et ferme 2023-2026', key: 'nature' },
    { id: 9967805, caption: 'Plage et mer 2023-2026', key: 'seaside' },
    { id: 9967806, caption: 'Loisirs et fete 2023-2026', key: 'leisure' },
    { id: 9967807, caption: 'Fantastique et aventure', key: 'fantasy' },
    { id: 9967808, caption: 'Halloween 2023-2026', key: 'halloween' },
    { id: 9967809, caption: 'Noel et Paques 2023-2026', key: 'seasonal' },
    { id: 9967810, caption: 'Rares et collections 2023-2026', key: 'rare' }
];

const sql = value => String(value ?? '').replace(/\\/g, '\\\\').replace(/'/g, "''").replace(/[\r\n]+/g, ' ').trim();
const bool = value => value ? 1 : 0;
const fullName = entry => String(entry.classname || '');
const baseName = entry => fullName(entry).split('*')[0];
const iconName = entry => fullName(entry).replace('*', '_') + '_icon.png';

function friendlyName(entry) {
    const current = String(entry.name || '').trim();
    if(current && !/\bname$/i.test(current)) return current.slice(0, 56);
    return fullName(entry)
        .replace(/\*\d+$/, '')
        .replace(/_(c2[3-6])_/i, ' ')
        .replace(/[_-]+/g, ' ')
        .replace(/\b\w/g, letter => letter.toUpperCase())
        .trim()
        .slice(0, 56);
}

function categoryFor(entry) {
    const text = `${ fullName(entry) } ${ entry.name || '' } ${ entry.description || '' }`.toLowerCase();
    if(/hween|halloween/.test(text)) return 'halloween';
    if(/xmas|christmas|easter|paques|advent|snow|winter/.test(text)) return 'seasonal';
    if(/seaside|beach|ocean|sea_|water|pool|dock|boat|yacht|surf|sand|lighthouse|jellyfish|coral/.test(text)) return 'seaside';
    if(/ranch|farm|garden|plant|tree|flower|grass|soil|crop|corn|pumpkin|tomato|grape|nature|forest|bush|rock/.test(text)) return 'nature';
    if(/neopets|wonderland|olympus|fantasy|dream|quest|dragon|magic|myth|jungle|temple|spirit|ancient/.test(text)) return 'fantasy';
    if(/disco|hobbies|gacha|game_|arcade|football|fball|trophy|party|dance|music|sport|dice|craft/.test(text)) return 'leisure';
    if(/nft_|rare|diamond|sculpture|regal|giftbox|ducket|sid_/.test(text)) return 'rare';
    if(/wall|floor|tile|roof|door|window|pillar|column|stair|block|bridge|divider|gate|building|construction/.test(text)) return 'construction';
    if(/mafia|nyc|street|shop|store|restaurant|cafe|office|city|garage|vehicle|car_|ambulance|hospital|school|bank/.test(text)) return 'city';
    return 'home';
}

const manifest = JSON.parse(fs.readFileSync(path.resolve(manifestPath), 'utf8'));
const current = JSON.parse(fs.readFileSync(furnitureDataPath, 'utf8'));
const existing = new Set([
    ...(current.roomitemtypes?.furnitype || []),
    ...(current.wallitemtypes?.furnitype || [])
].map(fullName));

const chosen = manifest
    .filter(item => item && item.entry && Number(item.entry.specialtype || 1) !== 23)
    .filter(item => !existing.has(fullName(item.entry)))
    .sort((a, b) => fullName(a.entry).localeCompare(fullName(b.entry)))
    .map((item, index) => ({
        kind: item.kind,
        entry: { ...item.entry, id: itemIdStart + index, offerid: itemIdStart + index },
        category: categoryFor(item.entry),
        offerId: offerIdStart + index
    }));

if(!chosen.length) throw new Error('No new furniture remains to import.');

for(const item of chosen) {
    const nitro = path.join(path.resolve(nitroSource), baseName(item.entry) + '.nitro');
    const icon = path.join(path.resolve(iconSource), iconName(item.entry));
    if(!fs.existsSync(nitro) || fs.statSync(nitro).size < 100) throw new Error(`Invalid Nitro bundle: ${ nitro }`);
    if(!fs.existsSync(icon) || fs.statSync(icon).size < 50) throw new Error(`Invalid icon: ${ icon }`);
}

fs.mkdirSync(furnitureTarget, { recursive: true });
fs.mkdirSync(iconTarget, { recursive: true });
for(const name of new Set(chosen.map(item => baseName(item.entry)))) {
    fs.copyFileSync(path.join(path.resolve(nitroSource), name + '.nitro'), path.join(furnitureTarget, name + '.nitro'));
}
for(const item of chosen) {
    fs.copyFileSync(path.join(path.resolve(iconSource), iconName(item.entry)), path.join(iconTarget, iconName(item.entry)));
}

current.roomitemtypes.furnitype.push(...chosen.filter(item => item.kind === 'room').map(item => item.entry));
current.wallitemtypes.furnitype.push(...chosen.filter(item => item.kind === 'wall').map(item => item.entry));
fs.writeFileSync(furnitureDataPath, JSON.stringify(current));

function furnitureRows() {
    return chosen.map(({ entry }) => {
        const type = entry.type === 'i' ? 'i' : (entry.type === 's' ? 's' : null);
        const actualType = type || (chosen.find(item => item.entry === entry)?.kind === 'wall' ? 'i' : 's');
        const height = /rug|floor|tile/i.test(`${ entry.category || '' } ${ fullName(entry) }`) ? 0.1 : 1;
        return `(${ entry.id },'${ sql(fullName(entry)) }','${ sql(friendlyName(entry)) }','${ actualType }',${ Number(entry.xdim) || 1 },${ Number(entry.ydim) || 1 },${ height },1,${ bool(entry.cansiton) },${ bool(entry.canstandon) },${ entry.id },'1','1','1','1','1','default',0,1,'','0',0,0,'0',0,'0',${ bool(entry.canlayon) })`;
    });
}

function appendBatches(lines, header, rows, ending) {
    for(let offset = 0; offset < rows.length; offset += 200) {
        lines.push(header);
        lines.push(rows.slice(offset, offset + 200).join(',\n') + `\n${ ending }`);
    }
}

function buildMigration(legacy) {
    const lines = [
        '-- ParadiseRP - 1057 nouveautes officielles 2023-2026, assets valides',
        '-- Plage d IDs isolee; aucun mobilier existant n est supprime ou remplace.',
        'SET NAMES utf8mb4;',
        'START TRANSACTION;'
    ];
    if(legacy) {
        const pageRows = [
            `(${ rootPageId },9967200,'Nouveautes officielles 2023-2026','Nouveautes officielles 2023-2026','default_3x3',1,1,1,95,'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')`,
            ...pages.map((page, index) => `(${ page.id },${ rootPageId },'${ page.caption }','${ page.caption }','default_3x3',1,1,1,${ index + 1 },'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')`)
        ];
        lines.push('INSERT INTO catalog_pages (id,parent_id,caption_save,caption,page_layout,icon_color,icon_image,min_rank,order_num,visible,enabled,club_only,vip_only,page_headline,page_teaser,page_special,page_text1,page_text2,page_text_details,page_text_teaser,room_id,includes) VALUES');
        lines.push(pageRows.join(',\n') + "\nON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption_save=VALUES(caption_save),caption=VALUES(caption),order_num=VALUES(order_num),visible='1',enabled='1';");
    } else {
        const pageRows = [
            `(${ rootPageId },9967200,'Nouveautes officielles 2023-2026',1,'1','1',1,0,95,'','default_3x3','','')`,
            ...pages.map((page, index) => `(${ page.id },${ rootPageId },'${ page.caption }',1,'1','1',1,0,${ index + 1 },'','default_3x3','','')`)
        ];
        lines.push('INSERT INTO catalog_pages (id,parent_id,caption,icon_image,visible,enabled,min_rank,min_vip,order_num,page_link,page_layout,page_strings_1,page_strings_2) VALUES');
        lines.push(pageRows.join(',\n') + "\nON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption=VALUES(caption),order_num=VALUES(order_num),visible='1',enabled='1';");
    }
    lines.push(`DELETE FROM catalog_items WHERE page_id BETWEEN ${ rootPageId } AND ${ rootPageId + 19 };`);
    appendBatches(
        lines,
        'INSERT INTO furniture (id,item_name,public_name,type,width,length,stack_height,can_stack,can_sit,is_walkable,sprite_id,allow_recycle,allow_trade,allow_marketplace_sell,allow_gift,allow_inventory_stack,interaction_type,behaviour_data,interaction_modes_count,vending_ids,height_adjustable,effect_id,wired_id,is_rare,clothing_id,extra_rot,allow_lay) VALUES',
        furnitureRows(),
        'ON DUPLICATE KEY UPDATE item_name=VALUES(item_name),public_name=VALUES(public_name),type=VALUES(type),width=VALUES(width),length=VALUES(length),stack_height=VALUES(stack_height),can_stack=VALUES(can_stack),can_sit=VALUES(can_sit),is_walkable=VALUES(is_walkable),sprite_id=VALUES(sprite_id),allow_lay=VALUES(allow_lay);'
    );
    const catalogRows = chosen.map(item => {
        const page = pages.find(page => page.key === item.category);
        const name = sql(friendlyName(item.entry));
        return legacy
            ? `('${ item.entry.id }',${ page.id },'${ name }',3,0,0,1,0,0,${ item.entry.id - itemIdStart + 1 },${ item.offerId },0,'','1','0')`
            : `(${ page.id },'${ item.entry.id }','${ name }',3,0,0,1,0,0,'1','','',${ item.offerId },0)`;
    });
    appendBatches(
        lines,
        legacy
            ? 'INSERT INTO catalog_items (item_ids,page_id,catalog_name,cost_credits,cost_points,points_type,amount,limited_stack,limited_sells,order_number,offer_id,song_id,extradata,have_offer,club_only) VALUES'
            : 'INSERT INTO catalog_items (page_id,item_id,catalog_name,cost_credits,cost_pixels,cost_diamonds,amount,limited_sells,limited_stack,offer_active,extradata,badge,offer_id,points_type) VALUES',
        catalogRows,
        ';'
    );
    lines.push('COMMIT;', '');
    return lines.join('\n');
}

fs.writeFileSync(modernMigrationPath, buildMigration(false));
fs.writeFileSync(legacyMigrationPath, buildMigration(true));
fs.mkdirSync(path.dirname(reportPath), { recursive: true });
const counts = Object.fromEntries(pages.map(page => [page.caption, chosen.filter(item => item.category === page.key).length]));
fs.writeFileSync(reportPath, JSON.stringify({ source: 'higoka/habbo-downloader', imported: chosen.length, baseAssets: new Set(chosen.map(item => baseName(item.entry))).size, itemIdStart, itemIdEnd: itemIdStart + chosen.length - 1, counts }, null, 2));
console.log(JSON.stringify({ imported: chosen.length, baseAssets: new Set(chosen.map(item => baseName(item.entry))).size, counts }, null, 2));
