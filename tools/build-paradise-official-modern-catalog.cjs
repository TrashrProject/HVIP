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
const profileName = process.argv[5] || 'recent';

if(!manifestPath || !nitroSource || !iconSource || !['recent', 'archive'].includes(profileName)) {
    throw new Error('Usage: node build-paradise-official-modern-catalog.cjs <manifest-or-furnidata.json> <nitro-dir> <icon-dir> [recent|archive]');
}

const furnitureDataPath = path.join(repositoryRoot, 'swf_pz', 'V5-0-2', 'gamedata', 'json', 'FurnitureData.json');
const furnitureTarget = path.join(repositoryRoot, 'swf_pz', 'V5-0-2', 'furniture');
const iconTarget = path.join(repositoryRoot, 'swf_pz', 'V5-0-2', 'dcr', 'hof_furni', 'icon');
const archiveProfile = profileName === 'archive';
const migrationStem = archiveProfile
    ? '20260906_paradise_official_archive_expansion'
    : '20260906_paradise_official_novelties_2023_2026';
const modernMigrationPath = path.join(repositoryRoot, 'migrations', `${ migrationStem }.sql`);
const legacyMigrationPath = path.join(repositoryRoot, 'migrations', `${ migrationStem }_legacy.sql`);
const reportPath = path.join(repositoryRoot, 'artifacts', archiveProfile
    ? 'paradise-official-archive-expansion.json'
    : 'paradise-official-novelties-2023-2026.json');
const itemIdStart = archiveProfile ? 997200000 : 997100000;
const offerIdStart = archiveProfile ? 1962000000 : 1961000000;
const rootPageId = archiveProfile ? 9967820 : 9967800;
const rootCaption = archiveProfile ? 'Collections officielles et classiques' : 'Nouveautes officielles 2023-2026';
const itemManagerPath = path.join(repositoryRoot, 'WavePlus', 'src', 'main', 'java', 'com', 'eu', 'habbo', 'habbohotel', 'items', 'ItemManager.java');
const itemManagerSource = fs.existsSync(itemManagerPath) ? fs.readFileSync(itemManagerPath, 'utf8') : '';
const supportedInteractions = new Set([...itemManagerSource.matchAll(/new ItemInteraction\("([^"]+)"/g)].map(match => match[1]));

const pages = [
    { id: rootPageId + 1, caption: archiveProfile ? 'Construction et architecture' : 'Construction 2023-2026', key: 'construction' },
    { id: rootPageId + 2, caption: archiveProfile ? 'Maison et decoration' : 'Maison et design 2023-2026', key: 'home' },
    { id: rootPageId + 3, caption: archiveProfile ? 'Ville commerces et RP' : 'Ville et RP 2023-2026', key: 'city' },
    { id: rootPageId + 4, caption: archiveProfile ? 'Nature jardins et ferme' : 'Nature et ferme 2023-2026', key: 'nature' },
    { id: rootPageId + 5, caption: archiveProfile ? 'Plage mer et piscines' : 'Plage et mer 2023-2026', key: 'seaside' },
    { id: rootPageId + 6, caption: archiveProfile ? 'Loisirs jeux et musique' : 'Loisirs et fete 2023-2026', key: 'leisure' },
    { id: rootPageId + 7, caption: 'Fantastique et aventure', key: 'fantasy' },
    { id: rootPageId + 8, caption: archiveProfile ? 'Halloween et horreur' : 'Halloween 2023-2026', key: 'halloween' },
    { id: rootPageId + 9, caption: archiveProfile ? 'Noel Paques et saisons' : 'Noel et Paques 2023-2026', key: 'seasonal' },
    { id: rootPageId + 10, caption: archiveProfile ? 'Rares et collections' : 'Rares et collections 2023-2026', key: 'rare' }
];
if(archiveProfile) pages.push(
    { id: rootPageId + 11, caption: 'Sieges lits et confort', key: 'seating' },
    { id: rootPageId + 12, caption: 'Lampes et eclairage', key: 'lighting' },
    { id: rootPageId + 13, caption: 'Cuisine nourriture et boissons', key: 'food' },
    { id: rootPageId + 14, caption: 'Technologie et machines', key: 'technology' },
    { id: rootPageId + 15, caption: 'Decoration art et accessoires', key: 'decoration' },
    { id: rootPageId + 16, caption: 'Collections NFT', key: 'nft' },
    { id: rootPageId + 17, caption: 'Classiques et objets divers', key: 'classic' }
);

const sql = value => String(value ?? '').replace(/\\/g, '\\\\').replace(/'/g, "''").replace(/[\r\n]+/g, ' ').trim();
const bool = value => value ? 1 : 0;
const fullName = entry => String(entry.classname || '');
const baseName = entry => fullName(entry).split('*')[0];
const iconName = entry => fullName(entry).replace('*', '_') + '_icon.png';
const interactionType = entry => supportedInteractions.has(baseName(entry)) ? baseName(entry) : 'default';

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
    if(/mafia|nyc|street|road|traffic|urban|public|shop|store|restaurant|cafe|office|city|garage|vehicle|car_|ambulance|hospital|school|bank|police|prison|army|airport|bus_|train|subway/.test(text)) return 'city';
    if(archiveProfile && /chair|sofa|couch|seat|stool|bench|bed|hammock|pillow|beanbag|armchair|throne/.test(text)) return 'seating';
    if(archiveProfile && /lamp|light|lantern|candle|torch|chandelier|neon|fireplace/.test(text)) return 'lighting';
    if(archiveProfile && /food|drink|kitchen|cake|coffee|tea_|bottle|pizza|burger|fruit|snack|dinner|breakfast|icecream|candy/.test(text)) return 'food';
    if(archiveProfile && /computer|laptop|phone|television|\btv\b|console|machine|robot|camera|speaker|radio|screen|monitor|science|laboratory/.test(text)) return 'technology';
    if(archiveProfile && /painting|poster|picture|art_|statue|vase|rug|carpet|ornament|trophy|banner|flag|plush|doll|toy/.test(text)) return 'decoration';
    if(/wall|floor|tile|roof|door|window|pillar|column|stair|block|bridge|divider|gate|building|construction/.test(text)) return 'construction';
    if(/\brare\b|diamond|sculpture|regal|giftbox|ducket|sid_|\bltd\b|_ltd/.test(text)) return 'rare';
    if(archiveProfile && /nft_/.test(text)) return 'nft';
    if(archiveProfile && /ads_|bonusbag|collectible|classic|habbo25|hc2[5-6]_/.test(text)) return 'classic';
    return 'home';
}

const manifestInput = JSON.parse(fs.readFileSync(path.resolve(manifestPath), 'utf8'));
const manifest = Array.isArray(manifestInput)
    ? manifestInput
    : [
        ...(manifestInput.roomitemtypes?.furnitype || []).map(entry => ({ kind: 'room', entry })),
        ...(manifestInput.wallitemtypes?.furnitype || []).map(entry => ({ kind: 'wall', entry }))
    ];
const current = JSON.parse(fs.readFileSync(furnitureDataPath, 'utf8'));
const profileItemIdEnd = itemIdStart + 99999;
current.roomitemtypes.furnitype = (current.roomitemtypes?.furnitype || [])
    .filter(entry => Number(entry.id) < itemIdStart || Number(entry.id) > profileItemIdEnd);
current.wallitemtypes.furnitype = (current.wallitemtypes?.furnitype || [])
    .filter(entry => Number(entry.id) < itemIdStart || Number(entry.id) > profileItemIdEnd);
const existing = new Set([
    ...(current.roomitemtypes?.furnitype || []),
    ...(current.wallitemtypes?.furnitype || [])
].map(fullName));

const candidates = manifest
    .filter(item => item && item.entry && Number(item.entry.specialtype || 1) !== 23)
    .filter(item => !existing.has(fullName(item.entry)))
    .sort((a, b) => fullName(a.entry).localeCompare(fullName(b.entry)));
const skippedAssets = [];
const chosen = candidates
    .filter(item => {
        const nitro = path.join(path.resolve(nitroSource), baseName(item.entry) + '.nitro');
        const icon = path.join(path.resolve(iconSource), iconName(item.entry));
        const valid = fs.existsSync(nitro) && fs.statSync(nitro).size >= 100 && fs.existsSync(icon) && fs.statSync(icon).size >= 50;
        if(!valid) skippedAssets.push(fullName(item.entry));
        return valid;
    })
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
    const destination = path.join(furnitureTarget, name + '.nitro');
    if(!fs.existsSync(destination)) fs.copyFileSync(path.join(path.resolve(nitroSource), name + '.nitro'), destination);
}
for(const item of chosen) {
    const destination = path.join(iconTarget, iconName(item.entry));
    if(!fs.existsSync(destination)) fs.copyFileSync(path.join(path.resolve(iconSource), iconName(item.entry)), destination);
}

current.roomitemtypes.furnitype.push(...chosen.filter(item => item.kind === 'room').map(item => item.entry));
current.wallitemtypes.furnitype.push(...chosen.filter(item => item.kind === 'wall').map(item => item.entry));
fs.writeFileSync(furnitureDataPath, JSON.stringify(current));

function furnitureRows() {
    return chosen.map(({ entry }) => {
        const type = entry.type === 'i' ? 'i' : (entry.type === 's' ? 's' : null);
        const actualType = type || (chosen.find(item => item.entry === entry)?.kind === 'wall' ? 'i' : 's');
        const height = /rug|floor|tile/i.test(`${ entry.category || '' } ${ fullName(entry) }`) ? 0.1 : 1;
        return `(${ entry.id },'${ sql(fullName(entry)) }','${ sql(friendlyName(entry)) }','${ actualType }',${ Number(entry.xdim) || 1 },${ Number(entry.ydim) || 1 },${ height },1,${ bool(entry.cansiton) },${ bool(entry.canstandon) },${ entry.id },'1','1','1','1','1','${ sql(interactionType(entry)) }',0,1,'','0',0,0,'0',0,'0',${ bool(entry.canlayon) })`;
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
        `-- ParadiseRP - ${ chosen.length } mobiliers officiels (${ profileName }), assets valides`,
        '-- Plage d IDs isolee; aucun mobilier existant n est supprime ou remplace.',
        'SET NAMES utf8mb4;',
        'START TRANSACTION;'
    ];
    if(legacy) {
        const pageRows = [
            `(${ rootPageId },9967200,'${ rootCaption }','${ rootCaption }','default_3x3',1,1,1,${ archiveProfile ? 96 : 95 },'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')`,
            ...pages.map((page, index) => `(${ page.id },${ rootPageId },'${ page.caption }','${ page.caption }','default_3x3',1,1,1,${ index + 1 },'1','1','0','0','','',NULL,NULL,NULL,NULL,NULL,0,'')`)
        ];
        lines.push('INSERT INTO catalog_pages (id,parent_id,caption_save,caption,page_layout,icon_color,icon_image,min_rank,order_num,visible,enabled,club_only,vip_only,page_headline,page_teaser,page_special,page_text1,page_text2,page_text_details,page_text_teaser,room_id,includes) VALUES');
        lines.push(pageRows.join(',\n') + "\nON DUPLICATE KEY UPDATE parent_id=VALUES(parent_id),caption_save=VALUES(caption_save),caption=VALUES(caption),order_num=VALUES(order_num),visible='1',enabled='1';");
    } else {
        const pageRows = [
            `(${ rootPageId },9967200,'${ rootCaption }',1,'1','1',1,0,${ archiveProfile ? 96 : 95 },'','default_3x3','','')`,
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
fs.writeFileSync(reportPath, JSON.stringify({ source: 'higoka/habbo-downloader', imported: chosen.length, baseAssets: new Set(chosen.map(item => baseName(item.entry))).size, skippedAssets, itemIdStart, itemIdEnd: itemIdStart + chosen.length - 1, counts }, null, 2));
console.log(JSON.stringify({ imported: chosen.length, baseAssets: new Set(chosen.map(item => baseName(item.entry))).size, skippedAssets, counts }, null, 2));
