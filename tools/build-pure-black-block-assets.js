#!/usr/bin/env node
'use strict';

const fs = require('fs');
const path = require('path');
const zlib = require('zlib');

const repositoryPath = path.resolve(process.argv[2] || path.join(__dirname, '..'));
const assetRoot = path.join(repositoryPath, 'swf_pz', 'V5-0-2');
const sourceName = 'bc_block_1';
const targetName = 'paradise_black_block';
const sourceNitro = path.join(assetRoot, 'furniture', `${sourceName}.nitro`);
const targetNitro = path.join(assetRoot, 'furniture', `${targetName}.nitro`);
const furnitureId = 996700070;
const blackIcon = Buffer.from(
    'iVBORw0KGgoAAAANSUhEUgAAABoAAAAbCAYAAABiFp9rAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABrSURBVEhLtY0xDsAwCMTy/0+3S9vhLCSKsCUvDoFz/nM9arwH0jVyceWYXNS1TX6cWpKDW+oHUgRLBEsESwRLBEsESwRLBEsESwRLBEsESwRLBMuPfNiyJAentsmPXcfkoso1cvH6gWR04AbdxRQJnYuK3AAAAABJRU5ErkJggg==',
    'base64'
);

function readBundle(filePath) {
    const input = fs.readFileSync(filePath);
    let offset = 0;
    const fileCount = input.readUInt16BE(offset);
    offset += 2;
    const files = [];

    for(let index = 0; index < fileCount; index++) {
        const nameLength = input.readUInt16BE(offset);
        offset += 2;
        const name = input.subarray(offset, offset + nameLength).toString('utf8');
        offset += nameLength;
        const compressedLength = input.readUInt32BE(offset);
        offset += 4;
        const data = zlib.inflateSync(input.subarray(offset, offset + compressedLength));
        offset += compressedLength;
        files.push({ name, data });
    }

    if(offset !== input.length) throw new Error(`Conteneur Nitro invalide: ${filePath}`);
    return files;
}

function writeBundle(filePath, files) {
    const chunks = [];
    const header = Buffer.alloc(2);
    header.writeUInt16BE(files.length);
    chunks.push(header);

    for(const file of files) {
        const name = Buffer.from(file.name, 'utf8');
        const compressed = zlib.deflateSync(file.data, { level: 9 });
        const nameLength = Buffer.alloc(2);
        nameLength.writeUInt16BE(name.length, 0);
        const dataLength = Buffer.alloc(4);
        dataLength.writeUInt32BE(compressed.length, 0);
        chunks.push(nameLength, name, dataLength, compressed);
    }

    fs.writeFileSync(filePath, Buffer.concat(chunks));
}

const crcTable = Array.from({ length: 256 }, (_, index) => {
    let value = index;
    for(let bit = 0; bit < 8; bit++) value = (value & 1) ? (0xedb88320 ^ (value >>> 1)) : (value >>> 1);
    return value >>> 0;
});

function crc32(buffer) {
    let value = 0xffffffff;
    for(const byte of buffer) value = crcTable[(value ^ byte) & 0xff] ^ (value >>> 8);
    return (value ^ 0xffffffff) >>> 0;
}

function pngChunk(type, data) {
    const name = Buffer.from(type, 'ascii');
    const output = Buffer.alloc(12 + data.length);
    output.writeUInt32BE(data.length, 0);
    name.copy(output, 4);
    data.copy(output, 8);
    output.writeUInt32BE(crc32(Buffer.concat([ name, data ])), 8 + data.length);
    return output;
}

function paeth(left, above, upperLeft) {
    const estimate = left + above - upperLeft;
    const leftDistance = Math.abs(estimate - left);
    const aboveDistance = Math.abs(estimate - above);
    const upperLeftDistance = Math.abs(estimate - upperLeft);
    return leftDistance <= aboveDistance && leftDistance <= upperLeftDistance ? left :
        (aboveDistance <= upperLeftDistance ? above : upperLeft);
}

function blackenPng(input) {
    const signature = Buffer.from('89504e470d0a1a0a', 'hex');
    if(!input.subarray(0, 8).equals(signature)) throw new Error('Spritesheet PNG invalide');
    let offset = 8;
    let ihdr;
    const idat = [];
    while(offset < input.length) {
        const length = input.readUInt32BE(offset);
        const type = input.subarray(offset + 4, offset + 8).toString('ascii');
        const data = input.subarray(offset + 8, offset + 8 + length);
        if(type === 'IHDR') ihdr = Buffer.from(data);
        if(type === 'IDAT') idat.push(data);
        offset += 12 + length;
        if(type === 'IEND') break;
    }
    if(!ihdr || !idat.length) throw new Error('Spritesheet PNG incomplete');
    const width = ihdr.readUInt32BE(0);
    const height = ihdr.readUInt32BE(4);
    if(ihdr[8] !== 8 || ihdr[9] !== 6 || ihdr[12] !== 0) {
        throw new Error('Le constructeur attend un PNG RGBA 8 bits non entrelace');
    }
    const bytesPerPixel = 4;
    const stride = width * bytesPerPixel;
    const filtered = zlib.inflateSync(Buffer.concat(idat));
    const pixels = Buffer.alloc(stride * height);
    let sourceOffset = 0;
    for(let y = 0; y < height; y++) {
        const filter = filtered[sourceOffset++];
        for(let x = 0; x < stride; x++) {
            const encoded = filtered[sourceOffset++];
            const index = y * stride + x;
            const left = x >= bytesPerPixel ? pixels[index - bytesPerPixel] : 0;
            const above = y > 0 ? pixels[index - stride] : 0;
            const upperLeft = y > 0 && x >= bytesPerPixel ? pixels[index - stride - bytesPerPixel] : 0;
            let predictor = 0;
            if(filter === 1) predictor = left;
            else if(filter === 2) predictor = above;
            else if(filter === 3) predictor = Math.floor((left + above) / 2);
            else if(filter === 4) predictor = paeth(left, above, upperLeft);
            else if(filter !== 0) throw new Error(`Filtre PNG inconnu: ${filter}`);
            pixels[index] = (encoded + predictor) & 0xff;
        }
    }
    for(let index = 0; index < pixels.length; index += 4) {
        pixels[index] = 0;
        pixels[index + 1] = 0;
        pixels[index + 2] = 0;
    }
    const unfiltered = Buffer.alloc((stride + 1) * height);
    for(let y = 0; y < height; y++) pixels.copy(unfiltered, y * (stride + 1) + 1, y * stride, (y + 1) * stride);
    return Buffer.concat([
        signature,
        pngChunk('IHDR', ihdr),
        pngChunk('IDAT', zlib.deflateSync(unfiltered, { level: 9 })),
        pngChunk('IEND', Buffer.alloc(0))
    ]);
}

function createBlackNitro() {
    const files = readBundle(sourceNitro).map(file => {
        let data = file.data;
        if(file.name.endsWith('.json')) {
            const original = JSON.parse(data.toString('utf8'));
            const renamed = JSON.parse(JSON.stringify(original).split(sourceName).join(targetName));
            for(const visualization of renamed.visualizations || []) {
                const black = { layers: {} };
                const sample = visualization.colors && (visualization.colors['13'] || visualization.colors['1']);
                for(const layerId of Object.keys((sample && sample.layers) || { 0: {} })) {
                    black.layers[layerId] = { color: 0 };
                }
                visualization.colors = { '1': black };
            }
            data = Buffer.from(JSON.stringify(renamed), 'utf8');
        }
        else if(file.name.endsWith('.png')) {
            data = blackenPng(data);
        }
        return {
            name: file.name.split(sourceName).join(targetName),
            data
        };
    });
    writeBundle(targetNitro, files);
}

function writeIcons() {
    const directories = [
        path.join(assetRoot, 'dcr', 'hof_furni'),
        path.join(assetRoot, 'dcr', 'hof_furni', 'icon')
    ];
    for(const directory of directories) {
        fs.mkdirSync(directory, { recursive: true });
        fs.writeFileSync(path.join(directory, `${targetName}_1_icon.png`), blackIcon);
        fs.writeFileSync(path.join(directory, `${targetName}_icon.png`), blackIcon);
    }
}

function patchFurnitureData() {
    const jsonPath = path.join(assetRoot, 'gamedata', 'json', 'FurnitureData.json');
    let data = fs.readFileSync(jsonPath, 'utf8');
    if(!data.includes(`\"classname\":\"${targetName}*1\"`)) {
        const boundary = ']},\"wallitemtypes\"';
        const offset = data.indexOf(boundary);
        if(offset < 0) throw new Error(`Structure FurnitureData inconnue: ${jsonPath}`);
        const entry = JSON.stringify({
            id: furnitureId,
            classname: `${targetName}*1`,
            revision: 1,
            category: 'unknown',
            defaultdir: 0,
            xdim: 1,
            ydim: 1,
            partcolors: { color: [ '#ffffff', '#000000' ] },
            name: 'Bloc noir pur',
            description: 'Cube de construction noir pur',
            adurl: '',
            offerid: furnitureId,
            buyout: false,
            rentofferid: -1,
            rentbuyout: false,
            bc: false,
            excludeddynamic: false,
            customparams: '-0.25',
            specialtype: 1,
            canstandon: true,
            cansiton: false,
            canlayon: false,
            furniline: 'paradise_builders',
            environment: '',
            rare: false
        });
        data = `${data.slice(0, offset)},${entry}${data.slice(offset)}`;
        fs.writeFileSync(jsonPath, data, 'utf8');
    }

    const xmlPath = path.join(assetRoot, 'gamedata', 'furnidata.xml');
    let xml = fs.readFileSync(xmlPath, 'utf8');
    if(!xml.includes(`classname=\"${targetName}*1\"`)) {
        const boundary = '</roomitemtypes>';
        const offset = xml.indexOf(boundary);
        if(offset < 0) throw new Error(`Structure furnidata XML inconnue: ${xmlPath}`);
        const newline = xml.includes('\r\n') ? '\r\n' : '\n';
        const lines = [
            `\t\t<furnitype id=\"${furnitureId}\" classname=\"${targetName}*1\">`,
            '\t\t\t<revision>1</revision>',
            '\t\t\t<defaultdir>0</defaultdir>',
            '\t\t\t<xdim>1</xdim>',
            '\t\t\t<ydim>1</ydim>',
            '\t\t\t<partcolors>',
            '\t\t\t\t<color>#ffffff</color>',
            '\t\t\t\t<color>#000000</color>',
            '\t\t\t</partcolors>',
            '\t\t\t<name>Bloc noir pur</name>',
            '\t\t\t<description>Cube de construction noir pur</description>',
            '\t\t\t<adurl/>',
            `\t\t\t<offerid>${furnitureId}</offerid>`,
            '\t\t\t<buyout>0</buyout>',
            '\t\t\t<rentofferid>-1</rentofferid>',
            '\t\t\t<rentbuyout>0</rentbuyout>',
            '\t\t\t<bc>0</bc>',
            '\t\t\t<excludeddynamic>0</excludeddynamic>',
            '\t\t\t<customparams>-0.25</customparams>',
            '\t\t\t<specialtype>1</specialtype>',
            '\t\t\t<canstandon>1</canstandon>',
            '\t\t\t<cansiton>0</cansiton>',
            '\t\t\t<canlayon>0</canlayon>',
            '\t\t</furnitype>',
            ''
        ].join(newline);
        xml = `${xml.slice(0, offset)}${lines}${xml.slice(offset)}`;
        fs.writeFileSync(xmlPath, xml, 'utf8');
    }
}

function patchClientCache() {
    const rendererPath = path.join(repositoryPath, 'WebPixel', 'nitro', 'renderer-config.json');
    let renderer = fs.readFileSync(rendererPath, 'utf8');
    renderer = renderer.replace(
        /(\"furnidata\.url\"\s*:\s*\"[^\"]*FurnitureData\.json)\?v=[^\"]*(\")/,
        '$1?v=20260905-pure-black-v1$2'
    );
    fs.writeFileSync(rendererPath, renderer, 'utf8');

    const indexPath = path.join(repositoryPath, 'WebPixel', 'nitro', 'index.html');
    let index = fs.readFileSync(indexPath, 'utf8');
    index = index.replace(/renderer-config\.json\?v=\d+/, 'renderer-config.json?v=7');
    fs.writeFileSync(indexPath, index, 'utf8');
}

if(!fs.existsSync(sourceNitro)) throw new Error(`Ressource source absente: ${sourceNitro}`);
createBlackNitro();
writeIcons();
patchFurnitureData();
patchClientCache();
console.log(`OK: ${path.relative(repositoryPath, targetNitro)}`);
console.log('OK: icone noire pure ecrite dans les deux repertoires hof_furni');
console.log(`OK: FurnitureData JSON/XML contient ${targetName}*1 (${furnitureId})`);
