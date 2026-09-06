#!/usr/bin/env node
'use strict';

const fs = require('fs');
const fsp = fs.promises;
const path = require('path');

const repositoryRoot = path.resolve(__dirname, '..');
const furnidataPath = path.resolve(process.argv[2] || '');
const outputRoot = path.resolve(process.argv[3] || '');
if(!process.argv[2] || !process.argv[3]) {
    throw new Error('Usage: node download-paradise-official-missing-assets.cjs <furnidata.json> <output-dir>');
}

const localPath = path.join(repositoryRoot, 'swf_pz', 'V5-0-2', 'gamedata', 'json', 'FurnitureData.json');
const remote = JSON.parse(fs.readFileSync(furnidataPath, 'utf8'));
const local = JSON.parse(fs.readFileSync(localPath, 'utf8'));
const all = data => [
    ...(data.roomitemtypes?.furnitype || []).map(entry => ({ kind: 'room', entry })),
    ...(data.wallitemtypes?.furnitype || []).map(entry => ({ kind: 'wall', entry }))
];
const existing = new Set(all(local).map(item => String(item.entry.classname || '')));
const missing = all(remote)
    .filter(item => !existing.has(String(item.entry.classname || '')))
    .filter(item => Number(item.entry.specialtype || 1) !== 23);

const swfDirectory = path.join(outputRoot, 'swf');
const iconDirectory = path.join(outputRoot, 'icons');
fs.mkdirSync(swfDirectory, { recursive: true });
fs.mkdirSync(iconDirectory, { recursive: true });

const baseName = entry => String(entry.classname).split('*')[0];
const iconName = entry => String(entry.classname).replace('*', '_') + '_icon.png';
const swfs = new Map();
for(const item of missing) {
    const name = baseName(item.entry);
    const current = swfs.get(name);
    if(!current || Number(item.entry.revision) > Number(current.revision)) swfs.set(name, item.entry);
}
const jobs = [
    ...[...swfs.values()].map(entry => ({
        url: `https://images.habbo.com/dcr/hof_furni/${ entry.revision }/${ baseName(entry) }.swf`,
        target: path.join(swfDirectory, `${ baseName(entry) }.swf`)
    })),
    ...missing.map(item => ({
        url: `https://images.habbo.com/dcr/hof_furni/${ item.entry.revision }/${ iconName(item.entry) }`,
        target: path.join(iconDirectory, iconName(item.entry))
    }))
];

let completed = 0;
const failures = [];
async function download(job) {
    try {
        if((await fsp.stat(job.target)).size > 50) return;
    } catch {}
    let lastError;
    for(let attempt = 1; attempt <= 4; attempt++) {
        try {
            const response = await fetch(job.url, { signal: AbortSignal.timeout(30000) });
            if(!response.ok) throw new Error(`HTTP ${ response.status }`);
            const data = Buffer.from(await response.arrayBuffer());
            if(data.length < 50) throw new Error(`invalid payload (${ data.length } bytes)`);
            await fsp.writeFile(job.target, data);
            return;
        } catch(error) {
            lastError = error;
            await new Promise(resolve => setTimeout(resolve, attempt * 500));
        }
    }
    throw new Error(`${ job.url }: ${ lastError.message }`);
}

async function worker(queue) {
    while(queue.length) {
        const job = queue.shift();
        try {
            await download(job);
        } catch(error) {
            failures.push({ url: job.url, target: job.target, error: error.message });
        }
        completed++;
        if(completed % 250 === 0 || completed === jobs.length) {
            console.log(`Downloaded/verified ${ completed }/${ jobs.length }`);
        }
    }
}

const queue = [...jobs];
Promise.all(Array.from({ length: 24 }, () => worker(queue)))
    .then(() => console.log(JSON.stringify({ furniture: missing.length, swfs: swfs.size, files: jobs.length, failures }, null, 2)))
    .catch(error => { console.error(error.stack || error); process.exitCode = 1; });
