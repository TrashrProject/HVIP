#!/usr/bin/env node

const fs = require('fs');
const path = require('path');
const zlib = require('zlib');

const args = process.argv.slice(2);
const apply = args.includes('--apply');
const valueAfter = (flag, fallback) => {
    const index = args.indexOf(flag);
    return index >= 0 && args[index + 1] ? path.resolve(args[index + 1]) : fallback;
};

const repository = valueAfter('--repository', path.resolve(__dirname, '..'));
const targetRoot = path.join(repository, 'swf_pz', 'V5-0-2');
const furnitureDataPath = path.join(targetRoot, 'gamedata', 'json', 'FurnitureData.json');
const targetFurniture = path.join(targetRoot, 'furniture');
const targetIcons = path.join(targetRoot, 'dcr', 'hof_furni', 'icon');
const reportPath = path.join(repository, 'runtime', 'reports', 'repair-paradise-assets-from-sources.csv');

const defaultSources = [
    path.resolve(repository, '..', 'HabboRPbr'),
    path.resolve(repository, '..', 'HVIP-paradise-skins'),
    path.resolve(repository, '..', 'HVIP-paradise-skins-clean')
];
const sourceArgs = [];
for (let index = 0; index < args.length; index += 1) {
    if (args[index] === '--source' && args[index + 1]) sourceArgs.push(path.resolve(args[++index]));
}
const sources = (sourceArgs.length ? sourceArgs : defaultSources).filter(source => fs.existsSync(source));

for (const required of [furnitureDataPath, targetFurniture, targetIcons]) {
    if (!fs.existsSync(required)) throw new Error(`Ressource requise absente : ${required}`);
}

const walk = (root, predicate, result = []) => {
    for (const entry of fs.readdirSync(root, { withFileTypes: true })) {
        const fullPath = path.join(root, entry.name);
        if (entry.isDirectory()) walk(fullPath, predicate, result);
        else if (predicate(entry.name)) result.push(fullPath);
    }
    return result;
};

const normalizedName = value => value
    .normalize('NFKD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .replace(/[^a-z0-9]/g, '');

const furnitureData = JSON.parse(fs.readFileSync(furnitureDataPath, 'utf8'));
const entries = [
    ...(furnitureData.roomitemtypes?.furnitype || []),
    ...(furnitureData.wallitemtypes?.furnitype || [])
];
const classNames = [...new Set(entries
    .map(entry => String(entry.classname || '').split('*')[0])
    .filter(Boolean))];

const indexes = {
    nitro: { exact: new Map(), normalized: new Map() },
    icon: { exact: new Map(), normalized: new Map() }
};
const assetAliases = new Map([
    ['xs_c16_creature6', 'xmas_c16_creature6'],
    ['OnPixelsake', 'habbocake'],
    ['poster_habboli1', 'diamond_painting5']
]);

const crcTable = Array.from({ length: 256 }, (_, value) => {
    let crc = value;
    for (let bit = 0; bit < 8; bit += 1) crc = (crc & 1) ? (0xedb88320 ^ (crc >>> 1)) : (crc >>> 1);
    return crc >>> 0;
});
const crc32 = buffer => {
    let crc = 0xffffffff;
    for (const value of buffer) crc = crcTable[(crc ^ value) & 0xff] ^ (crc >>> 8);
    return (crc ^ 0xffffffff) >>> 0;
};
const pngChunk = (name, data) => {
    const type = Buffer.from(name, 'ascii');
    const length = Buffer.alloc(4);
    length.writeUInt32BE(data.length);
    const checksum = Buffer.alloc(4);
    checksum.writeUInt32BE(crc32(Buffer.concat([type, data])));
    return Buffer.concat([length, type, data, checksum]);
};
const cropRgbaPng = (png, frame) => {
    const signature = Buffer.from('89504e470d0a1a0a', 'hex');
    if (!png.subarray(0, 8).equals(signature)) throw new Error('Atlas PNG invalide');
    let offset = 8;
    let width = 0;
    let height = 0;
    let bitDepth = 0;
    let colorType = 0;
    let interlace = 0;
    const idat = [];
    while (offset < png.length) {
        const length = png.readUInt32BE(offset);
        const type = png.subarray(offset + 4, offset + 8).toString('ascii');
        const data = png.subarray(offset + 8, offset + 8 + length);
        offset += 12 + length;
        if (type === 'IHDR') {
            width = data.readUInt32BE(0);
            height = data.readUInt32BE(4);
            bitDepth = data[8];
            colorType = data[9];
            interlace = data[12];
        } else if (type === 'IDAT') idat.push(data);
        else if (type === 'IEND') break;
    }
    if (bitDepth !== 8 || colorType !== 6 || interlace !== 0) throw new Error('Atlas PNG non RGBA8/interlace');
    const bytesPerPixel = 4;
    const stride = width * bytesPerPixel;
    const compressed = zlib.inflateSync(Buffer.concat(idat));
    const pixels = Buffer.alloc(width * height * bytesPerPixel);
    let sourceOffset = 0;
    for (let y = 0; y < height; y += 1) {
        const filter = compressed[sourceOffset++];
        const rowOffset = y * stride;
        for (let x = 0; x < stride; x += 1) {
            const raw = compressed[sourceOffset++];
            const left = x >= bytesPerPixel ? pixels[rowOffset + x - bytesPerPixel] : 0;
            const up = y > 0 ? pixels[rowOffset - stride + x] : 0;
            const upLeft = y > 0 && x >= bytesPerPixel ? pixels[rowOffset - stride + x - bytesPerPixel] : 0;
            let predictor = 0;
            if (filter === 1) predictor = left;
            else if (filter === 2) predictor = up;
            else if (filter === 3) predictor = Math.floor((left + up) / 2);
            else if (filter === 4) {
                const estimate = left + up - upLeft;
                const pa = Math.abs(estimate - left), pb = Math.abs(estimate - up), pc = Math.abs(estimate - upLeft);
                predictor = pa <= pb && pa <= pc ? left : (pb <= pc ? up : upLeft);
            } else if (filter !== 0) throw new Error(`Filtre PNG inconnu : ${filter}`);
            pixels[rowOffset + x] = (raw + predictor) & 0xff;
        }
    }
    const cropped = Buffer.alloc((frame.w * bytesPerPixel + 1) * frame.h);
    for (let y = 0; y < frame.h; y += 1) {
        const outputOffset = y * (frame.w * bytesPerPixel + 1);
        cropped[outputOffset] = 0;
        pixels.copy(cropped, outputOffset + 1, ((frame.y + y) * width + frame.x) * bytesPerPixel, ((frame.y + y) * width + frame.x + frame.w) * bytesPerPixel);
    }
    const ihdr = Buffer.alloc(13);
    ihdr.writeUInt32BE(frame.w, 0);
    ihdr.writeUInt32BE(frame.h, 4);
    ihdr[8] = 8;
    ihdr[9] = 6;
    return Buffer.concat([signature, pngChunk('IHDR', ihdr), pngChunk('IDAT', zlib.deflateSync(cropped, { level: 9 })), pngChunk('IEND', Buffer.alloc(0))]);
};
const unpackNitro = filePath => {
    const archive = fs.readFileSync(filePath);
    let offset = 0;
    const count = archive.readUInt16BE(offset);
    offset += 2;
    const files = new Map();
    for (let index = 0; index < count; index += 1) {
        const nameLength = archive.readUInt16BE(offset);
        offset += 2;
        const name = archive.subarray(offset, offset + nameLength).toString('utf8');
        offset += nameLength;
        const compressedLength = archive.readUInt32BE(offset);
        offset += 4;
        files.set(name, zlib.unzipSync(archive.subarray(offset, offset + compressedLength)));
        offset += compressedLength;
    }
    return files;
};
const packNitro = (filePath, files) => {
    const chunks = [];
    const header = Buffer.alloc(2);
    header.writeUInt16BE(files.size);
    chunks.push(header);
    for (const [nameValue, data] of files) {
        const name = Buffer.from(nameValue, 'utf8');
        const compressed = zlib.deflateSync(data, { level: 9 });
        const nameLength = Buffer.alloc(2);
        nameLength.writeUInt16BE(name.length);
        const dataLength = Buffer.alloc(4);
        dataLength.writeUInt32BE(compressed.length);
        chunks.push(nameLength, name, dataLength, compressed);
    }
    fs.writeFileSync(filePath, Buffer.concat(chunks));
};
const recoverAliasedNitro = (className, destination) => {
    if (!apply || fs.existsSync(destination) || !assetAliases.has(className)) return '';
    const sourceName = assetAliases.get(className);
    const source = [
        path.join(targetFurniture, `${sourceName}.nitro`),
        indexes.nitro.exact.get(sourceName.toLowerCase())
    ].find(candidate => candidate && fs.existsSync(candidate));
    if (!source) return '';
    const renamed = new Map();
    for (const [fileName, originalData] of unpackNitro(source)) {
        const outputName = fileName.split(sourceName).join(className);
        const outputData = fileName.toLowerCase().endsWith('.json')
            ? Buffer.from(originalData.toString('utf8').split(sourceName).join(className), 'utf8')
            : originalData;
        renamed.set(outputName, outputData);
    }
    packNitro(destination, renamed);
    return source;
};
const extractIconFromNitro = (className, destination) => {
    const nitroPath = path.join(targetFurniture, `${className}.nitro`);
    if (!apply || !fs.existsSync(nitroPath) || fs.existsSync(destination)) return false;
    try {
        const files = unpackNitro(nitroPath);
        const jsonEntry = [...files.entries()].find(([name]) => name.toLowerCase().endsWith('.json'));
        const pngEntry = [...files.entries()].find(([name]) => name.toLowerCase().endsWith('.png'));
        if (!jsonEntry || !pngEntry) return false;
        const descriptor = JSON.parse(jsonEntry[1].toString('utf8'));
        const iconFrame = Object.entries(descriptor.spritesheet?.frames || {})
            .find(([name]) => /_icon_[a-z](?:\.png)?$/i.test(name));
        if (!iconFrame?.[1]?.frame) return false;
        fs.writeFileSync(destination, cropRgbaPng(pngEntry[1], iconFrame[1].frame));
        return true;
    } catch (error) {
        return false;
    }
};
const recoverParameterizedIcon = (className, destination) => {
    if (!apply || fs.existsSync(destination)) return '';
    const candidates = [
        path.join(targetIcons, `${className}_1_icon.png`),
        path.join(targetIcons, `${className}_0_icon.png`)
    ];
    const source = candidates.find(candidate => fs.existsSync(candidate));
    if (!source) return '';
    fs.copyFileSync(source, destination);
    return source;
};

for (const source of sources) {
    for (const filePath of walk(source, name => name.toLowerCase().endsWith('.nitro'))) {
        const baseName = path.basename(filePath, '.nitro');
        if (!indexes.nitro.exact.has(baseName.toLowerCase())) indexes.nitro.exact.set(baseName.toLowerCase(), filePath);
        if (!indexes.nitro.normalized.has(normalizedName(baseName))) indexes.nitro.normalized.set(normalizedName(baseName), filePath);
    }
    for (const filePath of walk(source, name => name.toLowerCase().endsWith('_icon.png'))) {
        const baseName = path.basename(filePath).replace(/_icon\.png$/i, '');
        if (!indexes.icon.exact.has(baseName.toLowerCase())) indexes.icon.exact.set(baseName.toLowerCase(), filePath);
        if (!indexes.icon.normalized.has(normalizedName(baseName))) indexes.icon.normalized.set(normalizedName(baseName), filePath);
    }
}

const rows = [];
const counters = { copiedNitro: 0, copiedIcons: 0, aliasedNitro: 0, extractedIcons: 0, aliasedIcons: 0, recoverableNitro: 0, recoverableIcons: 0 };

const recover = (kind, className, destination) => {
    if (fs.existsSync(destination)) return { state: 'present', source: '' };
    const index = indexes[kind];
    const source = index.exact.get(className.toLowerCase()) || index.normalized.get(normalizedName(className));
    if (!source) return { state: 'missing', source: '' };
    counters[kind === 'nitro' ? 'recoverableNitro' : 'recoverableIcons'] += 1;
    if (apply) {
        fs.copyFileSync(source, destination);
        counters[kind === 'nitro' ? 'copiedNitro' : 'copiedIcons'] += 1;
    }
    return { state: apply ? 'copied' : 'recoverable', source };
};

for (const className of classNames) {
    const nitroDestination = path.join(targetFurniture, `${className}.nitro`);
    let nitro = recover('nitro', className, nitroDestination);
    const aliasedNitro = nitro.state === 'missing' ? recoverAliasedNitro(className, nitroDestination) : '';
    if (aliasedNitro) {
        counters.aliasedNitro += 1;
        nitro = { state: 'aliased', source: aliasedNitro };
    }
    const iconDestination = path.join(targetIcons, `${className}_icon.png`);
    let icon = recover('icon', className, iconDestination);
    if (icon.state === 'missing' && assetAliases.has(className)) {
        const sourceIcon = path.join(targetIcons, `${assetAliases.get(className)}_icon.png`);
        if (apply && fs.existsSync(sourceIcon)) {
            fs.copyFileSync(sourceIcon, iconDestination);
            counters.aliasedIcons += 1;
            icon = { state: 'aliased', source: sourceIcon };
        }
    }
    const parameterizedIcon = icon.state === 'missing' ? recoverParameterizedIcon(className, iconDestination) : '';
    if (parameterizedIcon) {
        counters.aliasedIcons += 1;
        icon = { state: 'aliased', source: parameterizedIcon };
    }
    if (icon.state === 'missing' && extractIconFromNitro(className, iconDestination)) {
        counters.extractedIcons += 1;
        icon = { state: 'extracted', source: path.join(targetFurniture, `${className}.nitro`) };
    }
    if (nitro.state !== 'present' || icon.state !== 'present') {
        rows.push({ className, nitro: nitro.state, nitroSource: nitro.source, icon: icon.state, iconSource: icon.source });
    }
}

const csvEscape = value => `"${String(value).replace(/"/g, '""')}"`;
const csv = [
    ['classname', 'nitro', 'nitro_source', 'icon', 'icon_source'].map(csvEscape).join(','),
    ...rows.map(row => [row.className, row.nitro, row.nitroSource, row.icon, row.iconSource].map(csvEscape).join(','))
].join('\r\n') + '\r\n';
fs.mkdirSync(path.dirname(reportPath), { recursive: true });
fs.writeFileSync(reportPath, csv, 'utf8');

const remainingNitro = classNames.filter(className => !fs.existsSync(path.join(targetFurniture, `${className}.nitro`))).length;
const remainingIcons = classNames.filter(className => !fs.existsSync(path.join(targetIcons, `${className}_icon.png`))).length;
console.log(JSON.stringify({
    mode: apply ? 'apply' : 'audit',
    sources,
    furnitureDataClassNames: classNames.length,
    ...counters,
    remainingNitro,
    remainingIcons,
    reportPath
}, null, 2));
